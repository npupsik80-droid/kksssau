using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public static class HttpService
{
	private const string FormDescription = "<div class=\"&divNameDescription\" style=\"visibility:hidden; position: fixed; left: 300px; top: 150px; width:600px; opacity:1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\"\r\n              onclick=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" onkeypress=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" >\r\n                <h2 class=\"help\" style=\"margin-left: 8px;\">&NameDescription</h2>\r\n                <table border = \"0\" >\r\n                    <tr>\r\n                        <td class=\"Caption\" align=\"left\" style=\"width:100px; vertical-align:top; padding-left:9px;\">Описание:</td>\r\n                        <td>\r\n                            <h6 class=\"help\" style=\"padding-top: 4px;\">\r\n                                &Description\r\n                            </h6>\r\n                        </td>\r\n                    </tr>\r\n                 </table>\r\n                <br/>\r\n                <br/>\r\n            </div>";

	public static HttpResponse RedirectHTTP(HttpRequest Request, string RedirectUrl)
	{
		HttpResponse httpResponse = new HttpResponse(Request, HttpStatusCode.Found, "");
		string text = Request.Url;
		if (text.IndexOf("?") != -1)
		{
			text = text.Substring(0, text.IndexOf("?"));
		}
		httpResponse.HeadersAdd("Location", new HttpResponse.ItemBase(text + "/" + RedirectUrl));
		return httpResponse;
	}

	public static string RedirectHTML(string RedirectUrl, int Time)
	{
		StringBuilder stringBuilder = new StringBuilder("\r\n                <!DOCTYPE html>\r\n                <html>\r\n                    <head>\r\n                        <meta http-equiv='refresh' content='&Time;URL=&Url'/>\r\n                    </head>\r\n                    <body>Ожидание перезагрузки сервера...</body>\r\n                </html>\r\n                ");
		stringBuilder.Replace("&Time", Time.ToString());
		stringBuilder.Replace("&Url", RedirectUrl);
		return stringBuilder.ToString();
	}

	public static string GetUrl(HttpRequest Request, string Url1 = "", string Url2 = "", string Url3 = "", bool GetHostFromSertificate = false, bool NotServerURL = true)
	{
		if (NotServerURL)
		{
			return ((Url1 != "") ? ("/" + Url1) : "") + ((Url2 != "") ? ("/" + Url2) : "") + ((Url3 != "") ? ("/" + Url3) : "");
		}
		string text = "";
		text = ((!GetHostFromSertificate && Global.RunIsSllMode) ? "https://" : ((!GetHostFromSertificate || !(Global.Settings.ServerSertificate != "")) ? "http://" : "https://"));
		string text2 = "";
		if (Request != null)
		{
			text = text + Request.Host + ":" + Request.Port;
			text2 = Request.Host;
		}
		else
		{
			text = text + "localhost:" + Global.Settings.ipPort;
			text2 = "localhost";
		}
		if (GetHostFromSertificate && Global.Settings.ServerSertificate != "" && (text2.ToLower() == "localhost" || text2.ToLower() == "127.0.0.1"))
		{
			X509Certificate2 x509Certificate = null;
			if (Global.Settings.ServerSertificate != "")
			{
				x509Certificate = UtilSertificate.GetCertFromStoreS(Global.Settings.ServerSertificate);
				if (x509Certificate == null)
				{
					Global.WriteError("Not found server certificate: " + Global.Settings.ServerSertificate);
				}
			}
			if (x509Certificate != null)
			{
				string nameCert = Global.GetNameCert(x509Certificate.Subject, "CN=");
				if (nameCert != "" && nameCert[0] != '*')
				{
					text = "https://" + nameCert + ":" + Global.Settings.ipPort;
				}
				else if (nameCert != "" && nameCert[0] == '*')
				{
					IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
					for (int i = 0; i < addressList.Length; i++)
					{
						IPHostEntry hostEntry = Dns.GetHostEntry(addressList[i]);
						if (hostEntry.HostName.ToLower().IndexOf(nameCert.Substring(1)) != -1)
						{
							text = "https://" + hostEntry.HostName + ":" + Global.Settings.ipPort;
							break;
						}
					}
				}
				x509Certificate.Dispose();
			}
		}
		return text + ((Url1 != "") ? ("/" + Url1) : "") + ((Url2 != "") ? ("/" + Url2) : "") + ((Url3 != "") ? ("/" + Url3) : "");
	}

	public static StringBuilder RootHtml(string OnLoad = "", bool AddUnitTestJS = false, bool NotClient = false)
	{
		string text = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "html", "index.html");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(File.ReadAllText(text));
		stringBuilder.Replace("\r\n", "");
		stringBuilder.Replace("&OnLoad", OnLoad);
		stringBuilder.Replace("&UseGainUp", "");
		if (Global.Settings.Marke == "GainUp")
		{
			stringBuilder.Replace("https://kkmserver.ru", "http://GainUp.ru");
		}
		else if (Global.Settings.Marke == "YClients")
		{
			stringBuilder.Replace("https://kkmserver.ru", "https://yclients.com");
		}
		if (Global.Settings.Marke == "GainUp")
		{
			stringBuilder.Replace("&Logo.png", "logo-GainUp.png");
			stringBuilder.Replace("https://kkmserver.ru", "http://GainUp.ru");
		}
		else if (Global.Settings.Marke == "YClients")
		{
			stringBuilder.Replace("&Logo.png", "Logo-YClients.png");
			stringBuilder.Replace("https://kkmserver.ru", "https://yclients.com");
			stringBuilder.Replace("/html/favicon.ico", "/html/FaviconYClients.png");
		}
		else
		{
			stringBuilder.Replace("&Logo.png", "Logo-KkmServer.png");
		}
		stringBuilder.Replace("&Logo.png", "Logo-AddIn.png");
		string text2 = "";
		foreach (Attribute customAttribute in Assembly.GetExecutingAssembly().GetCustomAttributes())
		{
			if (customAttribute.GetType().Name == "AssemblyFileVersionAttribute")
			{
				text2 = ((AssemblyFileVersionAttribute)customAttribute).Version;
			}
		}
		stringBuilder.Replace("&0.0.0.00", text2);
		if (AddUnitTestJS)
		{
			stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" src=\"/html/unittest.js\"></script>\r\n                <script type=\"text/javascript\" >\r\n                    window.addEventListener('DOMContentLoaded', function () {\r\n                        User = '&User';\r\n                        Password = '&Password';\r\n                    });\r\n                </script>\r\n                &ДополнительныеСкрипты");
			stringBuilder.Replace("&User", Global.Settings.LoginAdmin);
			stringBuilder.Replace("&Password", Global.Settings.PassAdmin);
		}
		return stringBuilder;
	}

	public static void CommandMenu(StringBuilder html, string Razdel, string Par1)
	{
		string dopCommand = "/";
		StringBuilder stringBuilder = new StringBuilder();
		AddCommandMenu(stringBuilder, "About", "Информация о сервере", "fa fa-info-circle", dopCommand);
		AddCommandMenu(stringBuilder, "Settings", "Настройки сервера", "fa fa-cogs", dopCommand);
		AddCommandMenu(stringBuilder, "UnitTest", "Тест оборудования", "fa fa-play", dopCommand);
		AddCommandMenu(stringBuilder, "UnitSettings", "Настройка оборудования", "fa fa-wrench", dopCommand);
		AddCommandMenu(stringBuilder, "KkmRegOfd", "Регистрация ККТ", "fa fa-wpforms", dopCommand);
		AddCommandMenu(stringBuilder, "PermitRegim", "Разрешительный режим", "fa fa-map-signs", dopCommand);
		if (Global.Settings.StatisticsСollection)
		{
			AddCommandMenu(stringBuilder, "Statistics", "Статистика сервера", "fa fa-bar-chart", dopCommand);
		}
		AddCommandMenu(stringBuilder, "Logs", "Логи команд", "fa fa-heartbeat", dopCommand);
		AddCommandMenu(stringBuilder, "License", "Лицензия", "fa fa-certificate", dopCommand);
		AddCommandMenu(stringBuilder, "OperationsHistory", "История операций", "fa fa-calendar", dopCommand);
		AddCommandMenu(stringBuilder, "PayByCard", "Оплата картой", "fa fa-cc-visa", dopCommand);
		AddCommandMenu(stringBuilder, "PrintCheck", "Регистрация чека", "fa fa-rub", dopCommand);
		AddCommandMenu(stringBuilder, "GetDataCheck", "Получить данные чека", "fa fa-repeat", dopCommand);
		AddCommandMenu(stringBuilder, "GetGoodCodeData", "Проверка ШК", "fa fa-barcode", dopCommand);
		html.Replace("&МенюКоманд", stringBuilder.ToString());
		html.Replace("&CurCommand", Razdel);
	}

	public static void AddCommandMenu(StringBuilder Html, string IdMenu, string MenuName, string Icone, string DopCommand)
	{
		StringBuilder stringBuilder = new StringBuilder("<div id = 'MenuCom&IdMenu' class='MenuDiv'>\r\n                    <a class='MenuText' href='/&IdMenu&DopCommand'>\r\n                    <i class='&Icone Menu'></i>\r\n                    &MenuName</a>\r\n                    <span class='Pointer'></span>\r\n                </div>\r\n                ");
		stringBuilder.Replace("&IdMenu", IdMenu);
		stringBuilder.Replace("&DopCommand", DopCommand);
		stringBuilder.Replace("&MenuName", MenuName);
		stringBuilder.Replace("&Icone", Icone);
		Html.Append(stringBuilder.ToString());
	}

	public static void UnitsMenu(StringBuilder html, string Razdel, string Par1, bool AddUnit = false, string UnitPassword = "")
	{
		if (AddUnit)
		{
			html.Replace("&СписокУстройств", "<div id='MenuAddUnit' class='MenuDiv'>\r\n                        <a class='MenuText' href='/AddUnit' style='color:rgba(99, 185, 49, 1)'>\r\n                        <i class='fa fa-plus-circle' aria-hidden='true'></i>\r\n                        Добавить устройство</a>\r\n                    </div>\r\n                    &СписокУстройств");
		}
		foreach (KeyValuePair<int, Unit> unit in Global.UnitManager.Units)
		{
			if ((unit.Value.UnitParamets.ContainsKey("UnitPassword") && unit.Value.UnitParamets["UnitPassword"] != "" && unit.Value.UnitParamets["UnitPassword"] != UnitPassword && UnitPassword != "AllUnitsPasswordKkmServer") || (Razdel == "KkmRegOfd" && (unit.Value.SettDr.TypeDevice.Type != TypeDevice.enType.ФискальныйРегистратор || !unit.Value.Kkm.IsKKT)) || (Razdel == "PrintCheck" && (unit.Value.SettDr.TypeDevice.Type != TypeDevice.enType.ФискальныйРегистратор || !unit.Value.Kkm.IsKKT)) || (Razdel == "PayByCard" && unit.Value.SettDr.TypeDevice.Type != TypeDevice.enType.ЭквайринговыйТерминал))
			{
				continue;
			}
			string text = "";
			text = unit.Value.SettDr.TypeDevice.Type switch
			{
				TypeDevice.enType.ФискальныйРегистратор => "fa fa-print", 
				TypeDevice.enType.СканерШтрихкода => "fa fa-barcode", 
				TypeDevice.enType.ПринтерЧеков => "fa fa-file-text-o", 
				TypeDevice.enType.ЭлектронныеВесы => "fa fa-balance-scale", 
				TypeDevice.enType.ЭквайринговыйТерминал => "fa fa-cc-visa", 
				TypeDevice.enType.ЭлектронныеЗамки => "fa fa-unlock-alt", 
				TypeDevice.enType.ДисплеиПокупателя => "fa fa-desktop", 
				_ => "xx", 
			};
			StringBuilder stringBuilder = new StringBuilder("<div id='MenuDev&NumDev' class='MenuDiv'>\r\n                        <a id='MenuDevA&NumDev' class='MenuText' href='/&Razdel/&NumDev'>\r\n                        <i class='&Icone  Menu'></i>\r\n                        &NumDev: &NameDev</a>\r\n                        <a id='MenuMiniText' class='MenuText' href='/&Razdel/&NumDev'>\r\n                        &OrganizationТекст</a>\r\n                        <span class='Pointer'></span>\r\n                    </div>\r\n                    &СписокУстройств");
			stringBuilder.Replace("&NumDev", unit.Key.ToString());
			stringBuilder.Replace("&Razdel", Razdel);
			string text2 = "";
			if (unit.Value.UnitParamets.ContainsKey("NameDevice") && unit.Value.UnitParamets["NameDevice"] != "")
			{
				text2 = unit.Value.UnitParamets["NameDevice"];
				stringBuilder.Replace("&NameDev", text2);
			}
			else
			{
				text2 = ((unit.Value != null && !(unit.Value.NameDevice == "") && !(unit.Value.NameDevice == "<Не определено>")) ? unit.Value.NameDevice : unit.Value.SettDr.TypeDevice.Protocol);
				stringBuilder.Replace("&NameDev", text2);
				if (unit.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && unit.Value.Kkm.Organization != "")
				{
					string text3 = unit.Value.Kkm.Organization + "</br>";
					string text4 = "";
					if (unit.Value.Kkm.TaxVariant.IndexOf('0') != -1)
					{
						text4 = text4 + ((text4 == "") ? "" : ", ") + "Основная СН";
					}
					if (unit.Value.Kkm.TaxVariant.IndexOf('1') != -1)
					{
						text4 = text4 + ((text4 == "") ? "" : ", ") + "УСН доход";
					}
					if (unit.Value.Kkm.TaxVariant.IndexOf('2') != -1)
					{
						text4 = text4 + ((text4 == "") ? "" : ", ") + "УСН дох.-расх.";
					}
					if (unit.Value.Kkm.TaxVariant.IndexOf('3') != -1)
					{
						text4 = text4 + ((text4 == "") ? "" : ", ") + "УНВД!????";
					}
					if (unit.Value.Kkm.TaxVariant.IndexOf('4') != -1)
					{
						text4 = text4 + ((text4 == "") ? "" : ", ") + "ЕСН";
					}
					if (unit.Value.Kkm.TaxVariant.IndexOf('5') != -1)
					{
						text4 = text4 + ((text4 == "") ? "" : ", ") + "Патент";
					}
					stringBuilder.Replace("&OrganizationТекст", text3 + text4);
				}
				else if (unit.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && unit.Value.Kkm.Organization == "")
				{
					stringBuilder.Replace("&OrganizationТекст", "<не настроено>");
				}
				else
				{
					stringBuilder.Replace("&OrganizationТекст", "");
				}
			}
			stringBuilder.Replace("&Icone", text);
			html.Replace("&СписокУстройств", stringBuilder.ToString());
		}
		html.Replace("&СписокУстройств", "");
		html.Replace("&CurNumDevice", Par1);
	}

	public static StringBuilder GetBodyRazdel(string RazdelName, bool Line = true)
	{
		StringBuilder stringBuilder = new StringBuilder("<div id='DeskGeneralSettings' class='Desk'>\r\n                    <h2 style='margin: 0 0 0 20px'>\r\n                        <span>&RazdelName:</span>\r\n                    </h2>\r\n                    <hr />\r\n                    &ТелоСтраницы\r\n                <div>");
		stringBuilder.Replace("&RazdelName", RazdelName);
		if (!Line)
		{
			stringBuilder.Replace("<hr />", "");
		}
		return stringBuilder;
	}

	public static void GetHeadetBox(StringBuilder html, string Caption, string Header, bool Any = false)
	{
		if (!Any)
		{
			StringBuilder stringBuilder = new StringBuilder("<div>\r\n                        <div class='Caption' align='right'>\r\n                            &Caption\r\n                        </div>\r\n                        <div class='input'>\r\n                            <h4 class='help'>&Header</h4>\r\n                        </div>\r\n                    </div>\r\n                    &ТелоСтраницы");
			stringBuilder.Replace("&Caption", Caption);
			stringBuilder.Replace("&Header", Header);
			html.Replace("&ТелоСтраницы", stringBuilder.ToString());
		}
		else
		{
			StringBuilder stringBuilder2 = new StringBuilder("<div>\r\n                        <div class='Caption' align='right'>\r\n                            &Caption\r\n                        </div>\r\n                        <div class='input' style='width: 600px;'>\r\n                            &Header\r\n                        </div>\r\n                    </div>\r\n                    &ТелоСтраницы");
			stringBuilder2.Replace("&Caption", Caption);
			stringBuilder2.Replace("&Header", Header);
			html.Replace("&ТелоСтраницы", stringBuilder2.ToString());
		}
	}

	public static void GetInputBox(StringBuilder html, string Caption, string Name, string Value, string Type = "", string Event = "", bool Disabled = false, string Help = "", string Description = "", string pattern = "", bool Visibility = true, string Param = "")
	{
		StringBuilder stringBuilder = new StringBuilder("<div class='InputValue' id='StrInput&Name' &DivStile>\r\n                    <div class='Caption' align='right'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='input'>\r\n                        <input class='input' name='&Name' id='&Name' &Type &autocomplete &pattern &Param value='&Value' &Event &Disabled/>\r\n                        &Helpinput\r\n                    </div>\r\n                    &ButtonDescription\r\n                    &Help\r\n                </div>\r\n                &ТелоDescription\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", Caption);
		stringBuilder.Replace("&Name", Name);
		if (Type == "")
		{
			stringBuilder.Replace("&Type", "");
		}
		else if (Type == "checkbox")
		{
			stringBuilder.Replace("&Type", "type='checkbox' &checked");
			stringBuilder.Replace("&checked", "style='width: 15px;' &checked");
			stringBuilder.Replace("&checked", (Value == "True" || Value == "true") ? "checked" : "");
			stringBuilder.Replace("&Value", "true");
		}
		else
		{
			stringBuilder.Replace("&Type", "type='&Type'");
			stringBuilder.Replace("&Type", Type);
		}
		if (Type == "password")
		{
			stringBuilder.Replace("&autocomplete", "autocomplete='new-password'");
		}
		else
		{
			stringBuilder.Replace("&autocomplete", "");
		}
		if (pattern == "")
		{
			stringBuilder.Replace("&pattern", "");
		}
		else
		{
			stringBuilder.Replace("&pattern", "pattern=\"" + pattern + "\"");
		}
		stringBuilder.Replace("&Param", Param);
		stringBuilder.Replace("&Value", HttpUtility.HtmlEncode(Value));
		stringBuilder.Replace("&Event", Event);
		stringBuilder.Replace("&Disabled", Disabled ? "disabled" : "");
		if (Description == "" || Description == null)
		{
			stringBuilder.Replace("&ButtonDescription", "");
			stringBuilder.Replace("&ТелоDescription", "");
		}
		else
		{
			string newValue = "div" + Guid.NewGuid();
			string text = "<input class=\"ButtonDescription\" type=\"button\" onclick=\"VisibleDescriptionWindows('&divNameDescription')\" value=\"?\">";
			text = text.Replace("&divNameDescription", newValue);
			string text2 = "<div class=\"&divNameDescription\" style=\"visibility:hidden; position: fixed; left: 300px; top: 150px; width:600px; opacity:1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\"\r\n              onclick=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" onkeypress=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" >\r\n                <h2 class=\"help\" style=\"margin-left: 8px;\">&NameDescription</h2>\r\n                <table border = \"0\" >\r\n                    <tr>\r\n                        <td class=\"Caption\" align=\"left\" style=\"width:100px; vertical-align:top; padding-left:9px;\">Описание:</td>\r\n                        <td>\r\n                            <h6 class=\"help\" style=\"padding-top: 4px;\">\r\n                                &Description\r\n                            </h6>\r\n                        </td>\r\n                    </tr>\r\n                 </table>\r\n                <br/>\r\n                <br/>\r\n            </div>".Replace("&divNameDescription", newValue);
			text2 = text2.Replace("&NameDescription", Caption);
			text2 = text2.Replace("&Description", Description);
			stringBuilder.Replace("&ButtonDescription", text);
			stringBuilder.Replace("&ТелоDescription", text2);
		}
		if (Type == "checkbox")
		{
			stringBuilder.Replace("&Helpinput", (Help == "") ? "" : ("<div class='InputHelinput'>" + Help + "</div>"));
			stringBuilder.Replace("&Help", "");
		}
		else
		{
			stringBuilder.Replace("&Helpinput", "");
			stringBuilder.Replace("&Help", (Help == "") ? "" : ("<div class='InputHelp'>" + Help + "</div>"));
		}
		if (Visibility)
		{
			stringBuilder.Replace("&DivStile", "");
		}
		else
		{
			stringBuilder.Replace("&DivStile", "style = 'display: none;'");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetSelectBox(StringBuilder html, string Caption, string Name, object Value, Dictionary<string, string> List, string Event = "", bool Disabled = false, string Help = "", string Description = "", bool Visibility = true)
	{
		if (Value == null)
		{
			Value = "";
		}
		StringBuilder stringBuilder = new StringBuilder("<div class='InputValue' id='StrSelect&Name'&DivStile>\r\n                    <div class='Caption' align='right'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='input'>\r\n                        <select class='input' name='&Name' id='&Name' &Event &Disabled>\r\n                            &List\r\n                        <select>\r\n                    </div>\r\n                    &ButtonDescription\r\n                    &Help\r\n                </div>\r\n                &ТелоDescription\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", Caption);
		stringBuilder.Replace("&Name", Name);
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (KeyValuePair<string, string> item in List)
		{
			bool flag = false;
			if (Value is string)
			{
				flag = item.Key.ToLower() == (Value as string).ToLower();
			}
			else if (Value is string[])
			{
				string[] array = Value as string[];
				foreach (string text in array)
				{
					flag = item.Key.ToLower() == text.ToLower();
				}
			}
			if (item.Key.Contains("optgroup"))
			{
				stringBuilder2.Append("<optgroup label='" + item.Value + "'</optgroup>");
				continue;
			}
			stringBuilder2.Append("<option " + (flag ? "selected" : "") + " value='" + item.Key + "'>" + item.Value + "</option>");
		}
		stringBuilder.Replace("&List", stringBuilder2.ToString());
		stringBuilder.Replace("&Event", Event);
		stringBuilder.Replace("&Disabled", Disabled ? "disabled" : "");
		if (Description == "" || Description == null)
		{
			stringBuilder.Replace("&ButtonDescription", "");
			stringBuilder.Replace("&ТелоDescription", "");
		}
		else
		{
			string newValue = "div" + Guid.NewGuid();
			string text2 = "<input class=\"ButtonDescription\" type=\"button\" onclick=\"VisibleDescriptionWindows('&divNameDescription')\" value=\"?\">";
			text2 = text2.Replace("&divNameDescription", newValue);
			string text3 = "<div class=\"&divNameDescription\" style=\"visibility:hidden; position: fixed; left: 300px; top: 150px; width:600px; opacity:1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\"\r\n              onclick=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" onkeypress=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" >\r\n                <h2 class=\"help\" style=\"margin-left: 8px;\">&NameDescription</h2>\r\n                <table border = \"0\" >\r\n                    <tr>\r\n                        <td class=\"Caption\" align=\"left\" style=\"width:100px; vertical-align:top; padding-left:9px;\">Описание:</td>\r\n                        <td>\r\n                            <h6 class=\"help\" style=\"padding-top: 4px;\">\r\n                                &Description\r\n                            </h6>\r\n                        </td>\r\n                    </tr>\r\n                 </table>\r\n                <br/>\r\n                <br/>\r\n            </div>".Replace("&divNameDescription", newValue);
			text3 = text3.Replace("&NameDescription", Caption);
			text3 = text3.Replace("&Description", Description);
			stringBuilder.Replace("&ButtonDescription", text2);
			stringBuilder.Replace("&ТелоDescription", text3);
		}
		stringBuilder.Replace("&Help", (Help == "") ? "" : ("<div class='InputHelp'>" + Help + "</div>"));
		if (Visibility)
		{
			stringBuilder.Replace("&DivStile", "");
		}
		else
		{
			stringBuilder.Replace("&DivStile", "style = 'display: none;'");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetHelpBox(StringBuilder html, string Caption, string Help)
	{
		StringBuilder stringBuilder = new StringBuilder("<div class=\"InputValue\">\r\n                    <div class='Caption' align='right'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='inputhelp'>\r\n                        <h6 class='help'>&Help</h6>\r\n                    </div>\r\n                </div>\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", Caption);
		stringBuilder.Replace("&Help", Help);
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetLine(StringBuilder html)
	{
		html.Replace("&ТелоСтраницы", "<hr/>\r\n                   &ТелоСтраницы");
	}

	public static void GetButton(StringBuilder html, string Caption = "", string NameButton = "", string Color = "", string Url = "", string Script = "", bool small = false)
	{
		StringBuilder stringBuilder = new StringBuilder("<div &stylediv>\r\n                    <div class='Caption' align='right'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='input'>\r\n                        <h5  &styleh5>\r\n                            <input class='Button' value='&NameButton' type='&type' &styleinput &formaction &ТелоСкрипта/>\r\n                        </h5>\r\n                    </div>\r\n                </div>\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", Caption);
		string text = "";
		if (small)
		{
			stringBuilder.Replace("&stylediv", "style=\"font-size: 70%\"");
			stringBuilder.Replace("&styleh5", "style=\"margin: 5px 0px;\"");
			text = "font-size:120%;";
		}
		else
		{
			stringBuilder.Replace("&stylediv", "");
			stringBuilder.Replace("&styleh5", "");
		}
		stringBuilder.Replace("&NameButton", NameButton);
		if (Color != "")
		{
			switch (Color)
			{
			case "Grin":
				text += "background:#85AB70;";
				break;
			case "Red":
				text += "background:rgba(145, 50, 51, 1); color:#FFF;";
				break;
			case "Blue":
				text += "background:rgba(154, 154, 254, 1);";
				break;
			}
		}
		stringBuilder.Replace("&styleinput", "style=\"" + text + "\"");
		if (Url != "")
		{
			stringBuilder.Replace("&type", "submit");
			stringBuilder.Replace("&formaction", "formaction='" + Url + "'");
		}
		else if (Script != "")
		{
			stringBuilder.Replace("&type", "button");
			stringBuilder.Replace("&formaction", "");
		}
		else
		{
			stringBuilder.Replace("&type", "submit");
			stringBuilder.Replace("&formaction", "");
		}
		if (Script != "")
		{
			stringBuilder.Replace("&ТелоСкрипта", "onclick=\"" + Script + "\"");
		}
		else
		{
			stringBuilder.Replace("&ТелоСкрипта", "");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetButtonTest(StringBuilder html, string Caption = "", string NameButton = "", string Color = "", string onclick = "")
	{
		StringBuilder stringBuilder = new StringBuilder("<div>\r\n                    <div class='Par'></div>\r\n                    <div class='Caption' align='right'>\r\n                        <button class='button' onclick='&onclick' &Color>&NameButton</button>\r\n                    </div>\r\n                    <div class='input'>\r\n                        &Caption\r\n                    </div>\r\n                </div>\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", Caption);
		stringBuilder.Replace("&NameButton", NameButton);
		stringBuilder.Replace("&onclick", onclick);
		if (Color != "")
		{
			switch (Color)
			{
			case "Grin":
				stringBuilder.Replace("&Color", "style='background:#85AB70'");
				break;
			case "Red":
				stringBuilder.Replace("&Color", "style='background:rgba(145, 50, 51, 1); color:#FFF'");
				break;
			case "Blue":
				stringBuilder.Replace("&Color", "style='background:rgba(154, 154, 254, 1)'");
				break;
			}
		}
		else
		{
			stringBuilder.Replace("&Color", "");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetHeadetTest(StringBuilder html, string Caption, string Header, bool Any = false)
	{
		if (!Any)
		{
			StringBuilder stringBuilder = new StringBuilder("<div>\r\n                        <div class='Par' align='right' style='width:70px'>\r\n                            &Caption\r\n                        </div>\r\n                        <div class='input'>\r\n                            <h4 class='help'>&Header</h4>\r\n                        </div>\r\n                    </div>\r\n                    &ТелоСтраницы");
			stringBuilder.Replace("&Caption", Caption);
			stringBuilder.Replace("&Header", Header);
			html.Replace("&ТелоСтраницы", stringBuilder.ToString());
		}
		else
		{
			StringBuilder stringBuilder2 = new StringBuilder("<div>\r\n                        <div class='Par' align='right' style='width:70px'>\r\n                            &Caption\r\n                        </div>\r\n                        <div class='input' style='width: 600px;'>\r\n                            &Header\r\n                        </div>\r\n                    </div>\r\n                    &ТелоСтраницы");
			stringBuilder2.Replace("&Caption", Caption);
			stringBuilder2.Replace("&Header", Header);
			html.Replace("&ТелоСтраницы", stringBuilder2.ToString());
		}
	}

	public static void GetText(StringBuilder html, string Caption = "", string Text = "", string Color = "", string Url = "", string id = "help", string DivStile = "", string aStile = "")
	{
		StringBuilder stringBuilder = new StringBuilder("<div class='Text' id='&idDiv' &DivStile>\r\n                    <div class='Caption' align='right' style='vertical-align: top;'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='inputhelp'>\r\n                        <h4 class='help' &Color><a id='&id' &aStile &href>&Text</a> </h4>\r\n                     </div>\r\n                </div>\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", HttpUtility.HtmlEncode(Caption));
		stringBuilder.Replace("&Text", Text);
		stringBuilder.Replace("&idDiv", "Div" + id);
		stringBuilder.Replace("&id", id);
		stringBuilder.Replace("&DivStile", DivStile);
		stringBuilder.Replace("&aStile", aStile);
		if (Color != "")
		{
			switch (Color)
			{
			case "Grin":
				stringBuilder.Replace("&Color", "style='color:#85AB70'");
				break;
			case "Red":
				stringBuilder.Replace("&Color", "style='color:rgba(145, 50, 51, 1)'");
				break;
			case "Blue":
				stringBuilder.Replace("&Color", "style='color:rgba(29, 29, 254, 1)'");
				break;
			default:
				stringBuilder.Replace("&Color", "style='color:color: rgb(104, 92, 83)'");
				break;
			}
		}
		else
		{
			stringBuilder.Replace("&Color", "");
		}
		if (Url != "")
		{
			stringBuilder.Replace("&href", "href='" + Url + "'");
		}
		else
		{
			stringBuilder.Replace("&href", "");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetTextArea(StringBuilder html, string Caption, string Name, string Value, string rows = "", string Event = "", bool Disabled = false, string Help = "", string Description = "", string pattern = "", bool Visibility = true)
	{
		StringBuilder stringBuilder = new StringBuilder("<div class='InputValue' id='Div&Name' &DivStile>\r\n                    <div class='Caption' align='right'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='input'>\r\n                        <textarea class='input' rows='&rows' wrap='off' name ='&Name' id='&Name' &pattern &Event &Disabled>&Value</textarea>\r\n                        &Helpinput\r\n                    </div>\r\n                    &ButtonDescription\r\n                    &Help\r\n                </div>\r\n                &ТелоDescription\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", Caption);
		stringBuilder.Replace("&Name", Name);
		if (rows == "")
		{
			stringBuilder.Replace("&rows", "1");
		}
		else
		{
			stringBuilder.Replace("&rows", rows);
		}
		if (pattern == "")
		{
			stringBuilder.Replace("&pattern", "");
		}
		else
		{
			stringBuilder.Replace("&pattern", "pattern=\"" + pattern + "\"");
		}
		stringBuilder.Replace("&Value", Value);
		stringBuilder.Replace("&Event", Event);
		stringBuilder.Replace("&Disabled", Disabled ? "disabled" : "");
		if (Description == "" || Description == null)
		{
			stringBuilder.Replace("&ButtonDescription", "");
			stringBuilder.Replace("&ТелоDescription", "");
		}
		else
		{
			string newValue = "div" + Guid.NewGuid();
			string text = "<input class=\"ButtonDescription\" type=\"button\" onclick=\"VisibleDescriptionWindows('&divNameDescription')\" value=\"?\">";
			text = text.Replace("&divNameDescription", newValue);
			string text2 = "<div class=\"&divNameDescription\" style=\"visibility:hidden; position: fixed; left: 300px; top: 150px; width:600px; opacity:1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\"\r\n              onclick=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" onkeypress=\"document.getElementsByClassName('&divNameDescription')[0].style.visibility = 'hidden';\" >\r\n                <h2 class=\"help\" style=\"margin-left: 8px;\">&NameDescription</h2>\r\n                <table border = \"0\" >\r\n                    <tr>\r\n                        <td class=\"Caption\" align=\"left\" style=\"width:100px; vertical-align:top; padding-left:9px;\">Описание:</td>\r\n                        <td>\r\n                            <h6 class=\"help\" style=\"padding-top: 4px;\">\r\n                                &Description\r\n                            </h6>\r\n                        </td>\r\n                    </tr>\r\n                 </table>\r\n                <br/>\r\n                <br/>\r\n            </div>".Replace("&divNameDescription", newValue);
			text2 = text2.Replace("&NameDescription", Caption);
			text2 = text2.Replace("&Description", Description);
			stringBuilder.Replace("&ButtonDescription", text);
			stringBuilder.Replace("&ТелоDescription", text2);
		}
		stringBuilder.Replace("&Helpinput", "");
		stringBuilder.Replace("&Help", (Help == "") ? "" : ("<div class='InputHelp'>" + Help + "</div>"));
		if (Visibility)
		{
			stringBuilder.Replace("&DivStile", "");
		}
		else
		{
			stringBuilder.Replace("&DivStile", "style = 'display: none;'");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static StringBuilder GetStartCollapsPanel(string Caption, string Name, bool Collaps = true, bool LineBottom = false, bool LineTop = false)
	{
		StringBuilder stringBuilder = new StringBuilder("<hr  id='&NameLine'style='display:&DisplayLine;' />\r\n            <div class='CollapsPanelHead'>\r\n                <div class='ButtonFill' align='right'>\r\n                </div>\r\n                <button class='ButtonOn' type='button' id='&NameOn' style='display:&DisplayButtonOn;' onclick='CollapsPanel(true, \"&Name\")'>\r\n                    +\r\n                </button>\r\n                <button class='ButtonOff' type='button' id='&NameOff' style='display:&DisplayButtonOff;' onclick='CollapsPanel(false, \"&Name\")'>\r\n                    -\r\n                </button>\r\n                <div class='PanelCaption'>\r\n                    &Caption\r\n                </div>\r\n            </div>\r\n            <div class='CollapsPanelBody' id='&Name' style='display:&DisplayCollapsPanel;'>\r\n                &ТелоСтраницы\r\n            </div>\r\n            <hr style='display:&LineBottom;' />\r\n            &ПродолжениеТелаСтраницы");
		if (Collaps)
		{
			stringBuilder.Replace("&DisplayLine", "none");
			stringBuilder.Replace("&DisplayButtonOn", "");
			stringBuilder.Replace("&DisplayButtonOff", "none");
			stringBuilder.Replace("&DisplayCollapsPanel", "none");
		}
		else
		{
			stringBuilder.Replace("&DisplayLine", LineBottom ? "" : "none");
			stringBuilder.Replace("&DisplayButtonOn", "none");
			stringBuilder.Replace("&DisplayButtonOff", "");
			stringBuilder.Replace("&DisplayCollapsPanel", "");
		}
		stringBuilder.Replace("&LineBottom", LineBottom ? "" : "none");
		if (!LineTop)
		{
			stringBuilder.Replace("&NameLine", "Non&NameLine");
		}
		stringBuilder.Replace("&Caption", Caption);
		stringBuilder.Replace("&Name", Name);
		return stringBuilder;
	}

	public static void GetEndCollapsPanel(StringBuilder html, StringBuilder CollapsPanel)
	{
		CollapsPanel.Replace("&ТелоСтраницы", "");
		CollapsPanel.Replace("&ПродолжениеТелаСтраницы", "&ТелоСтраницы");
		html.Replace("&ТелоСтраницы", CollapsPanel.ToString());
	}

	public static void GetPre(StringBuilder html, string Caption = "", string Text = "", string id = "help", bool Visibility = true)
	{
		StringBuilder stringBuilder = new StringBuilder("<div id='Div&id' style='display: block;'>\r\n                    <div class='Caption' align='right' style='vertical-align: top;'>\r\n                        &Caption\r\n                    </div>\r\n                    <div class='inputhelp'>\r\n                        <pre id='&id' style='margin: 0;'>&Text</pre>\r\n                     </div>\r\n                </div>\r\n                &ТелоСтраницы");
		stringBuilder.Replace("&Caption", HttpUtility.HtmlEncode(Caption));
		stringBuilder.Replace("&Text", Text);
		stringBuilder.Replace("&id", id);
		if (!Visibility)
		{
			stringBuilder.Replace("display: block", "display: none");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetChangeForDevice(StringBuilder html, string Caption = "")
	{
		html.Replace("&ТелоСтраницы", "<div &stylediv>\r\n                    <div class='Caption' align='right'>\r\n                        &CaptionСanvas\r\n                    </div>\r\n                    <div class='input'>\r\n                    </div>\r\n                </div>\r\n                <canvas id=\"CanvasLine\" width=\"500\" height=\"500\"></canvas> \r\n                <script type='text/javascript'>\r\n                var canvas = document.getElementById(\"CanvasLine\");\r\n                var context = canvas.getContext(\"2d\");\r\n                context.beginPath();\r\n                context.lineWidth = 3;\r\n                context.moveTo(350, 10);\r\n                context.lineTo(350, 370);\r\n                context.lineTo(39, 370);\r\n                context.moveTo(40, 370);\r\n                context.lineTo(60, 355);\r\n                context.moveTo(40, 370);\r\n                context.lineTo(60, 385);\r\n                context.strokeStyle = \"#607455\";\r\n                context.stroke();\r\n                </script>\r\n                &ТелоСтраницы");
		html.Replace("&CaptionСanvas", Caption);
	}

	public static void GetForm(StringBuilder html, string name, string action, string method = "get", string style = "")
	{
		StringBuilder stringBuilder = new StringBuilder("<form name='&name' action='&action' method='&method' &style>\r\n                   &ТелоСтраницы");
		stringBuilder.Replace("&name", name);
		stringBuilder.Replace("&action", action);
		stringBuilder.Replace("&method", method);
		if (style != "")
		{
			stringBuilder.Replace("&style", "style ='" + style + "'");
		}
		else
		{
			stringBuilder.Replace("&style", "");
		}
		html.Replace("&ТелоСтраницы", stringBuilder.ToString());
	}

	public static void GetFormEnd(StringBuilder html, string Caption = "", string NameButton = "")
	{
		if (NameButton != "" || Caption != "")
		{
			StringBuilder stringBuilder = new StringBuilder("<div>\r\n                        <div class='Caption' align='right'>\r\n                            &Caption\r\n                        </div>\r\n                        <div class='input'>\r\n                            <h5>\r\n                                <input class='Button' value='&NameButton' type='submit'/>\r\n                            </h5>\r\n                        </div>\r\n                    </div>\r\n                    </form>\r\n                    &ТелоСтраницы");
			stringBuilder.Replace("&Caption", Caption);
			stringBuilder.Replace("&NameButton", NameButton);
			html.Replace("&ТелоСтраницы", stringBuilder.ToString());
		}
		else
		{
			html.Replace("&ТелоСтраницы", "</form>\r\n                    &ТелоСтраницы");
		}
	}
}
