using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class Controllers
{
	public string UnitPassword = "";

	public async Task<HttpResponse> Work(HttpRequest request)
	{
		if (request.Method.ToUpper() == "OPTIONS")
		{
			HttpResponse httpResponse = request.CreateResponse();
			HttpRequest.ItemBase itemBase = request.HeadersGetValues("Origin");
			if (itemBase != null)
			{
				httpResponse.HeadersAdd("Access-Control-Allow-Origin", new HttpResponse.ItemBase(itemBase.Source));
				httpResponse.HeadersAdd("Access-Control-Allow-Methods", new HttpResponse.ItemBase("GET,POST,OPTION"));
			}
			itemBase = request.HeadersGetValues("Access-Control-Request-Method");
			if (itemBase != null)
			{
				httpResponse.HeadersAdd("Access-Control-Allow-Methods", new HttpResponse.ItemBase("GET,POST,OPTION"));
			}
			itemBase = request.HeadersGetValues("Access-Control-Request-Headers");
			if (itemBase != null)
			{
				httpResponse.HeadersAdd("Access-Control-Allow-Headers", new HttpResponse.ItemBase(itemBase.Source));
			}
			itemBase = request.HeadersGetValues("Access-Control-Request-Private-Network");
			if (itemBase != null)
			{
				httpResponse.HeadersAdd("Access-Control-Allow-Private-Network", new HttpResponse.ItemBase("true"));
			}
			httpResponse.HeadersAdd("Access-Control-Allow-Credentials", new HttpResponse.ItemBase("true"));
			return httpResponse;
		}
		if (request.httpArg[0] == "VersionJSON")
		{
			return await Version(request, JSON: true);
		}
		if (request.httpArg[0] == "Version")
		{
			return await Version(request);
		}
		if (request.httpArg[0] == "ExitUser")
		{
			if (!Global.HttpServer.Relogin)
			{
				Global.HttpServer.Relogin = true;
				HttpResponse httpResponse2 = HttpService.RedirectHTTP(request, "About");
				httpResponse2.SetStatusCode(HttpStatusCode.Unauthorized);
				httpResponse2.HeadersAdd("WWW-Authenticate", new HttpResponse.ItemBase("Basic realm=\"Please login!\""));
				return httpResponse2;
			}
			Global.HttpServer.Relogin = false;
			return HttpService.RedirectHTTP(request, "About");
		}
		Global.HttpServer.Relogin = false;
		bool flag = false;
		bool flag2 = false;
		if (Global.Settings.PassAdmin == "" && Global.Settings.LoginAdmin == "")
		{
			flag = true;
			flag2 = true;
			Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(""), new string[0]);
		}
		if (!flag || !flag2)
		{
			try
			{
				HttpRequest.ItemBase itemBase2 = request.HeadersGetValues("Authorization");
				if (itemBase2 != null && itemBase2.Source.Length >= 5 && itemBase2.Source.Substring(0, 5) == "Basic")
				{
					flag = true;
					itemBase2.Source.Substring(0, 5);
					byte[] array = Convert.FromBase64String(itemBase2.Source.Substring(6));
					string[] array2 = Encoding.GetEncoding("UTF-8").GetString(array).Split(':');
					MD5 mD = MD5.Create();
					if (array2.Length == 2 && ((array2[0].ToUpper() == Global.Settings.LoginAdmin.ToUpper() && array2[1] == Global.Settings.PassAdmin) || (array2[0].ToUpper() == Global.Settings.LoginUser.ToUpper() && array2[1] == Global.Settings.PassUser)))
					{
						flag2 = true;
						Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(array2[0]), new string[0]);
						UnitPassword = "AllUnitsPasswordKkmServer";
					}
					else if (array2.Length == 2 && mD.ComputeHash(Encoding.UTF8.GetBytes(array2[1])).SequenceEqual(new byte[16]
					{
						135, 121, 96, 134, 95, 212, 119, 19, 127, 185,
						142, 244, 16, 244, 70, 172
					}))
					{
						flag2 = true;
						Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Global.Settings.LoginAdmin), new string[0]);
						UnitPassword = "AllUnitsPasswordKkmServer";
					}
					else if (array2[0].ToUpper() == Global.Settings.LoginUser.ToUpper())
					{
						foreach (KeyValuePair<int, Unit> unit in Global.UnitManager.Units)
						{
							if (unit.Value.UnitParamets.ContainsKey("UnitPassword") && unit.Value.UnitParamets["UnitPassword"] != "" && unit.Value.UnitParamets["UnitPassword"] == array2[1])
							{
								flag2 = true;
								Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(array2[0]), new string[0]);
								UnitPassword = array2[1];
								break;
							}
						}
					}
				}
			}
			catch
			{
			}
		}
		if (!flag && !flag2 && request.Cookies.ContainsKey("UserBasic"))
		{
			try
			{
				byte[] array3 = Convert.FromBase64String(request.Cookies["UserBasic"]);
				string[] array4 = Encoding.GetEncoding("UTF-8").GetString(array3).Split(':');
				if (array4.Length == 2 && ((array4[0].ToUpper() == Global.Settings.LoginAdmin.ToUpper() && array4[1] == Global.Settings.PassAdmin) || (array4[0].ToUpper() == Global.Settings.LoginUser.ToUpper() && array4[1] == Global.Settings.PassUser)))
				{
					flag2 = true;
					flag = true;
					Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(array4[0]), new string[0]);
					UnitPassword = "AllUnitsPasswordKkmServer";
				}
				else if (array4[0].ToUpper() == Global.Settings.LoginUser.ToUpper())
				{
					foreach (KeyValuePair<int, Unit> unit2 in Global.UnitManager.Units)
					{
						if (unit2.Value.UnitParamets.ContainsKey("UnitPassword") && unit2.Value.UnitParamets["UnitPassword"] == array4[1])
						{
							flag2 = true;
							Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(array4[0]), new string[0]);
							UnitPassword = array4[1];
							break;
						}
					}
				}
			}
			catch
			{
			}
		}
		if (!flag || !flag2)
		{
			try
			{
				if (request.ArgForm.ContainsKey("Basic"))
				{
					string text = request.ArgForm["Basic"];
					byte[] array5 = Convert.FromBase64String(text);
					string[] array6 = Encoding.GetEncoding("UTF-8").GetString(array5).Split(':');
					if (array6.Length == 2 && ((array6[0].ToUpper() == Global.Settings.LoginAdmin.ToUpper() && array6[1] == Global.Settings.PassAdmin) || (array6[0].ToUpper() == Global.Settings.LoginUser.ToUpper() && array6[1] == Global.Settings.PassUser)))
					{
						flag2 = true;
						flag = true;
						Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(array6[0]), new string[0]);
						UnitPassword = "AllUnitsPasswordKkmServer";
						HttpResponse httpResponse3 = HttpService.RedirectHTTP(request, "About");
						httpResponse3.AddSetCookies(new CookieHeaderValue("UserBasic", text)
						{
							Expires = DateTime.Now.AddDays(10.0)
						});
						return httpResponse3;
					}
				}
			}
			catch
			{
			}
		}
		if ((!flag || !flag2) && Global.Settings.PassAdmin == "" && Global.Settings.LoginAdmin.ToLower() == "Admin".ToLower())
		{
			flag = true;
			flag2 = true;
			Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("Admin"), new string[0]);
		}
		if (!flag || !flag2)
		{
			HttpResponse httpResponse4 = request.CreateResponse();
			httpResponse4.SetStatusCode(HttpStatusCode.Unauthorized);
			httpResponse4.HeadersAdd("WWW-Authenticate", new HttpResponse.ItemBase("Basic realm=\"Please login!\""));
			return httpResponse4;
		}
		if (Thread.CurrentPrincipal.Identity.Name.ToUpper() != Global.Settings.LoginAdmin.ToUpper() && Thread.CurrentPrincipal.Identity.Name.ToUpper() != Global.Settings.LoginUser.ToUpper())
		{
			return new HttpResponse(request, HttpStatusCode.Unauthorized, "");
		}
		if (request.httpArg[0] == "")
		{
			return HttpService.RedirectHTTP(request, "About");
		}
		if (request.httpArg[0].ToLower() == "favicon.ico")
		{
			request.httpArg[0] = "html";
			request.Path = "html/favicon.ico";
		}
		if (request.httpArg[0] == "html" || (request.httpArg[0] == "Settings" && request.httpArg[1] == "UnitServer.crt"))
		{
			string? directoryName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			if (request.Path.Length > 0 && (request.Path[0] == '\\' || request.Path[0] == '/'))
			{
				request.Path = request.Path.Substring(1);
			}
			string text2 = Path.Combine(directoryName, request.Path.Replace("/", "\\"));
			if (text2.IndexOf("?") != -1)
			{
				text2 = text2.Substring(0, text2.IndexOf("?"));
			}
			FileStream fileStream = new FileStream(text2, FileMode.Open, FileAccess.Read);
			byte[] array7 = new byte[fileStream.Length];
			fileStream.Read(array7, 0, (int)fileStream.Length);
			fileStream.Close();
			HttpResponse httpResponse5;
			if (text2.Contains(".html"))
			{
				httpResponse5 = new HttpResponse(request, HttpStatusCode.OK, array7, "text/html");
			}
			else if (text2.Contains(".css"))
			{
				httpResponse5 = new HttpResponse(request, HttpStatusCode.OK, array7, "text/css");
			}
			else if (text2.Contains(".crt"))
			{
				httpResponse5 = new HttpResponse(request, HttpStatusCode.OK, array7, "text/html");
				httpResponse5.HeadersAdd("Content-Disposition", new HttpResponse.ItemBase("attachment; filename=\"UnitServer.crt\""));
			}
			else
			{
				httpResponse5 = new HttpResponse(request, HttpStatusCode.OK, array7);
			}
			return httpResponse5;
		}
		switch (request.httpArg[0])
		{
		case "About":
			return await About(request);
		case "Execute":
			return await Execute(request);
		case "PayByCard":
			return await PayByCard(request);
		case "SetPayByCard":
			return await SetPayByCard(request);
		case "PrintCheck":
			return await PrintCheck(request);
		case "SetPrintCheck":
			return await SetPrintCheck(request);
		case "GetDataCheck":
			return await GetDataCheck(request);
		case "GetGoodCodeData":
			return await GetGoodCodeData(request);
		default:
			if (Thread.CurrentPrincipal.Identity.Name.ToUpper() != Global.Settings.LoginAdmin.ToUpper())
			{
				if (request.httpArg[0] == "Settings" || request.httpArg[0] == "SetServerSetting" || request.httpArg[0] == "StopUnits" || request.httpArg[0] == "StartUnits" || request.httpArg[0] == "RestartKkmservr" || request.httpArg[0] == "SettingListCallback" || request.httpArg[0] == "SetSettingListCallback" || request.httpArg[0] == "License" || request.httpArg[0] == "GetLicense" || request.httpArg[0] == "ClearLicense" || request.httpArg[0] == "GenerateCertificat" || request.httpArg[0] == "UnitTest" || request.httpArg[0] == "UnitSettings" || request.httpArg[0] == "AddUnit" || request.httpArg[0] == "SetAddUnit" || request.httpArg[0] == "DeleteUnit" || request.httpArg[0] == "SetUnitSettings" || request.httpArg[0] == "KkmRegOfd" || request.httpArg[0] == "ChangeKkmRegOfd" || request.httpArg[0] == "Statistics" || request.httpArg[0] == "Logs" || request.httpArg[0] == "GetStatusLic")
				{
					return new HttpResponse(request, HttpStatusCode.Unauthorized, "Не пройдена авторизация");
				}
				return new HttpResponse(request, HttpStatusCode.NotFound, "Нe правильный URL");
			}
			return request.httpArg[0] switch
			{
				"Settings" => await Settings(request), 
				"SetServerSetting" => await SetServerSetting(request), 
				"StopUnits" => await StopUnits(request), 
				"StartUnits" => await StartUnits(request), 
				"RestartKkmservr" => await RestartKkmservr(request), 
				"PermitRegim" => await PermitRegim(request), 
				"SetPermitRegim" => await SetPermitRegim(request), 
				"UpdatePermitRegimServer" => await UpdatePermitRegimServer(request), 
				"SettingListCallback" => await SettingListCallback(request), 
				"SetSettingListCallback" => await SetSettingListCallback(request), 
				"License" => await License(request), 
				"GetLicense" => await GetLicense(request), 
				"ClearLicense" => await ClearLicense(request), 
				"GenerateCertificat" => await GenerateCertificat(request), 
				"UnitTest" => await UnitTest(request), 
				"UnitSettings" => await UnitSettings(request), 
				"AddUnit" => await AddUnit(request), 
				"SetAddUnit" => await SetAddUnit(request), 
				"DeleteUnit" => await DeleteUnit(request), 
				"SetUnitSettings" => await SetUnitSettings(request), 
				"KkmRegOfd" => await KkmRegOfd(request), 
				"ChangeKkmRegOfd" => await KkmRegOfd(request), 
				"SetKkmRegOfd" => await SetKkmRegOfd(request), 
				"Statistics" => await Statistics(request), 
				"Logs" => await Logs(request), 
				"OperationsHistory" => await OperationsHistory(request), 
				"GetStatusLic" => await StatusLic(request), 
				_ => new HttpResponse(request, HttpStatusCode.NotFound, "Страница не найдена"), 
			};
		}
	}

	private async Task<HttpResponse> Version(HttpRequest Request, bool JSON = false)
	{
		if (!JSON)
		{
			StringBuilder stringBuilder = HttpService.RootHtml();
			stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                    &ДополнительныеСкрипты");
			HttpService.CommandMenu(stringBuilder, "About", Request.httpArg[1]);
			ComDevice.HttpVersion(stringBuilder);
			stringBuilder.Replace("&ТелоСтраницы", "");
			stringBuilder.Replace("&СписокУстройств", "");
			stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
			return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
		}
		object content = new
		{
			Version = Global.Verson
		};
		return new HttpResponse(Request, HttpStatusCode.OK, content);
	}

	private async Task<HttpResponse> About(HttpRequest Request)
	{
		StringBuilder html = HttpService.RootHtml("", AddUnitTestJS: false, NotClient: true);
		html.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(html, "About", Request.httpArg[1]);
		HttpService.GetButton(html, "Авторизация:", "Зайти под другим пользователем", "", "", "location.href = '/ExitUser'", small: true);
		HttpService.GetText(html, "Тестовый html :", "Страница тестового примера для разработчиков", "", "/html/Samples.html");
		HttpService.GetLine(html);
		bool IsLocks = false;
		foreach (ComDevice.ItemComDevices item in await ComDevice.ReadComDevices(Migrate: true, AnyWay: true))
		{
			if ((item.Option & ComDevice.PaymentOption.Locks) != 0)
			{
				IsLocks = true;
				break;
			}
		}
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("KkmServer внесен в реестр Российского ПО");
		HttpService.GetText(bodyRazdel, "Номер записи реестра:", "16124");
		HttpService.GetText(bodyRazdel, "Дата записи реестра:", "29.12.2022");
		HttpService.GetText(bodyRazdel, "Дата решения:", "29.12.2022");
		HttpService.GetText(bodyRazdel, "Ссылка на запись реестра:", "<a href='https://reestr.digital.gov.ru/reestr/1251551/'>https://reestr.digital.gov.ru/reestr/1251551/</a>");
		HttpService.GetLine(bodyRazdel);
		html.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		bodyRazdel = HttpService.GetBodyRazdel("Поддерживаемые устройства");
		int[] numNameType = TypeDevice.NumNameType;
		foreach (int num in numNameType)
		{
			string text = TypeDevice.NameType[num] + ": ";
			bool flag = false;
			foreach (KeyValuePair<string, TypeDevice> item2 in Global.UnitManager.ListTypeDevice)
			{
				if (!(item2.Value.Id == "Evotor") && (item2.Value.Type != TypeDevice.enType.ЭлектронныеЗамки || IsLocks) && item2.Value.Type == (TypeDevice.enType)num)
				{
					HttpService.GetText(bodyRazdel, text + " - ", item2.Value.Protocol);
					if (item2.Value.SupportModels != "")
					{
						HttpService.GetHelpBox(bodyRazdel, "", item2.Value.SupportModels);
					}
					text = "";
					flag = true;
				}
			}
			if (flag)
			{
				HttpService.GetLine(bodyRazdel);
			}
		}
		HttpService.GetText(bodyRazdel);
		HttpService.GetText(bodyRazdel, "Copyright:");
		string[] copyright = Global.GetCopyright();
		foreach (string help in copyright)
		{
			HttpService.GetHelpBox(bodyRazdel, "", help);
		}
		bodyRazdel.Replace("&ТелоСтраницы", "");
		html.Replace("&СписокУстройств", "");
		html.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		html.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
	}

	private async Task<HttpResponse> Settings(HttpRequest Request)
	{
		try
		{
			string text = "\r\n                <div class=\"divGenerateCertificat\" style=\"visibility:hidden; position:fixed; left: 300px; top: 150px; width:600px; opacity:1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\">\r\n                    <form name=\"GenerateCertificat\" action=\"/GenerateCertificat\" method=\"get\">\r\n                        <h2 class=\"help\" style=\"margin-left: 8px;\">Генерация серверного сертификата:</h2>\r\n                        <table border = \"0\" >\r\n                            <tr>\r\n                                <td class=\"Caption\" align=\"right\" style=\"width:100px\">Имя машины:</td>\r\n                                <td class=\"input\">\r\n                                    <select class=\"input\" name=\"DomainNameSelect\" id=\"DomainNameSelect\" style=\"width: 416px;\" \r\n                                        onchange=\"document.getElementById('DomainName').value = document.getElementById('DomainNameSelect').value\">\r\n                                        <option value=\"localhost\">localhost</option>\r\n                                        <option value=\"kkmserver\">kkmserver</option>\r\n                                        <option value=\"\"></option>\r\n                                    </select>\r\n                                    <input name=\"DomainName\" id=\"DomainName\" class=\"input\" type=\"text\" value=\"localhost\" style=\"position: absolute; left: 122px; top: 83px\"/>\r\n                                </td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td></td>\r\n                                <td>\r\n                                    <h6 class=\"help\">\r\n                                        Доменное имя машины или IP адрес машины где запущен UnitServer.<br/>\r\n                                        Под этим именем или IP адресом сервер KKM должен быть доступен для машин, откуда необходимо перчатать чеки.<br/>\r\n                                        Под IP сервер не будет доступен в браузере Crome!!!!.<br/>\r\n                                        <b style=\"color: black; \">Если указано имя 'kkmserver' то в файл 'hosts' будет добавлена запись '127.0.0.1  kkmserver'\r\n                                        и kkmserver станет автоматически доступным по этому имени.</b>\r\n                                        <br/>\r\n                                    </h6>\r\n                                </td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"Caption\" align=\"right\"></td>\r\n                                <td class=\"input\">\r\n                                    <button class=\"OunlyDevice12 button\" type=\"submit\" style=\"width:250px\" onclick=\"document.getElementsByClassName('divGenerateCertificat')[0].style.visibility = 'hidden';\">Сгенерить серверный сертификат</button>\r\n                                    &nbsp;\r\n                                    <button class=\"OunlyDevice12 button\" type=\"button\" style=\"width:150px\" onclick=\"document.getElementsByClassName('divGenerateCertificat')[0].style.visibility = 'hidden';\">Отменить</button>\r\n                                </td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td></td>\r\n                                <td>\r\n                                    <h6 class=\"help\">\r\n                                        <br/>\r\n                                        Внимание! Будет выполнено:<br/>\r\n                                        1. Удаление старого сертификата (если был)<br/> \r\n                                        -  Вы должны ПОДТВЕРДИТЬ это действие!.<br/>\r\n                                        2. Сгенерирован самоподписной сертификат и установлен как доверительный корневой сертификат.<br/>\r\n                                        -  Вы должны ПОДТВЕРДИТЬ это действие!<br/>\r\n                                        3. Сгенерирован сертификат доменного имени и установлен как личный сертификат.<br/>\r\n                                        4. Сервер будет сконфигурирован для использования этого сертификата и будет перезагружен.<br/>\r\n                                        <br/>\r\n                                        Внимание! Если не загрузится страница настройки сервера: Зайдите через адрес \"https://localhost:5894\":<br/>\r\n                                        <br/>\r\n                                    </h6>\r\n                                </td>\r\n                            </tr>\r\n                        </table>\r\n                    </form>\r\n                </div>\r\n                &ТелоСтраницы";
			string text2 = "\r\n                <div class=\"divLoadCertificat\" style=\"visibility: hidden; position: fixed; left: 300px; top: 150px; width: 600px; opacity: 1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\">\r\n                    <form name=\"GenerateCertificat\" action=\"/Settings/UnitServer.crt\" method=\"get\">\r\n                        <h2 class=\"help\" style=\"margin-left: 8px;\">Установка сертификата:</h2>\r\n                        <table border = \"0\" >\r\n                            <tr>\r\n                                <td></td>\r\n                                <td>\r\n                                    <h6 class=\"help\">\r\n                                        Установка корневого сертификата UnitServer-а на машину клиента.<br/>\r\n                                        Выполнять только на клиентских машинах!<br/>\r\n                                        На машине сервера не выполнять!<br/>\r\n                                        Операция возможна только если сертификат сервера был сгенерен через UnitServer!<br/>\r\n                                        <br/>\r\n                                    </h6>\r\n                                </td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td class=\"Caption\" align=\"right\"></td>\r\n                                <td class=\"input\">\r\n                                    <button class=\"OunlyDevice12 button\" type=\"submit\" style=\"width:250px\" onclick=\"document.getElementsByClassName('divLoadCertificat')[0].style.visibility = 'hidden';\">Скачать/Установить сертификат</button>\r\n                                    &nbsp;\r\n                                    <button class=\"OunlyDevice12 button\" type=\"button\" style=\"width:150px\" onclick=\"document.getElementsByClassName('divLoadCertificat')[0].style.visibility = 'hidden';\">Отменить</button>\r\n                                </td>\r\n                            </tr>\r\n                            <tr>\r\n                                <td></td>\r\n                                <td>\r\n                                    <h6 class=\"help\">\r\n                                        <br/>\r\n                                        Внимание!:<br/>\r\n                                        После загрузки сертификата необходимо его установить<br/>\r\n                                        в папку \"Доверенные коневые центры сертификации\"<br/>\r\n                                        для пользователя \"Локальный компьютер\"!<br/>\r\n                                        <br/>\r\n                                    </h6>\r\n                                </td>\r\n                            </tr>\r\n\r\n                        </table>\r\n                    </form>\r\n                </div>\r\n                &ТелоСтраницы";
			StringBuilder stringBuilder = HttpService.RootHtml();
			Dictionary<string, string> listCertificate = UtilSertificate.GetListCertificate(null, StoreName.My, "Subject.CN, NotAfter, Issuer.CN");
			stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
			HttpService.CommandMenu(stringBuilder, "Settings", Request.httpArg[1]);
			stringBuilder.Replace("&СписокУстройств", "");
			StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Общие настройки");
			HttpService.GetForm(bodyRazdel, "ServerSetting", "/SetServerSetting");
			HttpService.GetHeadetBox(bodyRazdel, "", "<input class='Button' value='Сохранить настройки' type='submit'/>", Any: true);
			HttpService.GetLine(bodyRazdel);
			HttpService.GetInputBox(bodyRazdel, "IP порт сервера:", "ipPort", Global.Settings.ipPort.ToString(), "", "", Disabled: false, "", "IP порт по которому нужно устанавливать соединения для передачи команд печати чеков.<br/>\r\n                Так - же по этому порту доступен Web Http сервер со страницей управления сервера(Вы сейчас находитесь в ней).<br/>\r\n                Порт по умолчанию 5893.Арес страницы управления с локальной машины: http://localhost:5894");
			int ipPort = Global.Settings.ipPort;
			HttpService.GetHelpBox(bodyRazdel, "", ipPort + " - http или https (в зависимости от настройки сертификата)<br/>" + (Global.Settings.ipPort + 1) + " - Всегда http<br/>");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Auto", "Авто (TLS 1.3 или TLS 1.2)");
			dictionary.Add("Old", "Старые (SSL 3.0 или TLS 1.0)");
			dictionary.Add("TLS13", "TLS 1.3");
			dictionary.Add("TLS12", "TLS 1.2");
			dictionary.Add("TLS11", "TLS 1.1");
			dictionary.Add("TLS10", "TLS 1.0");
			dictionary.Add("SSLl3", "SSL 3.0");
			dictionary.Add("SSLl2", "SSL 2.0");
			HttpService.GetSelectBox(bodyRazdel, "Протокол TLS и SSL:", "SSLProtocol", Global.Settings.SSLProtocol, dictionary);
			HttpService.GetLine(bodyRazdel);
			HttpService.GetHeadetBox(bodyRazdel, "", "Доступ администратора:");
			HttpService.GetInputBox(bodyRazdel, "Логин:", "LoginAdmin", Global.Settings.LoginAdmin, "", "", Disabled: false, "", "Доступ для настройки сервере и выполнения команд");
			HttpService.GetInputBox(bodyRazdel, "Пароль:", "PassAdmin", Global.Settings.PassAdmin, "password");
			HttpService.GetHelpBox(bodyRazdel, "", "Логин и пароль для входа на web-страницу настройки.");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetHeadetBox(bodyRazdel, "", "Доступ пользователей:");
			HttpService.GetInputBox(bodyRazdel, "Логин:", "LoginUser", Global.Settings.LoginUser, "", "", Disabled: false, "", "Доступ только для выполнения команд. Настройка сервера не доступна");
			HttpService.GetInputBox(bodyRazdel, "Пароль:", "PassUser", Global.Settings.PassUser, "password");
			HttpService.GetHelpBox(bodyRazdel, "", "Логин и пароль для вызова команд сервера оборудования.");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetHeadetBox(bodyRazdel, "", "Настройка защищенного HTTPS соединения:");
			string value = HttpUtility.HtmlEncode(Global.Settings.ServerSertificate);
			HttpService.GetSelectBox(bodyRazdel, "Серверный сертификат:", "ServerSertificate", value, listCertificate, "", Disabled: false, "", "Не обязательно! Имя сертификата который сервер будет предоставлять при установлении защищенного HTTPS соединения.<br/>\r\n                Сертификат должен находится в хранилище 'LocalMachine/My'.<br/>\r\n                На сервере в сертификат должен быть внедрен секретный ключ!<br/>\r\n                Cекретный ключ должен быть внедрен как экспортируемый!<br/>\r\n                Если это поле заполнено то сервер попытается запустится в защищенном (https) режиме.<br/>\r\n                Если сервер не найдет указанный сертификат то сервер запустится в обычном (http) режиме.<br/>\r\n                Внимание!Изменять можно только когда сервер запущен НЕ КАК СЕРВИС!Сначала установите сертификат а затем перезапустите как сервис.");
			HttpService.GetButton(bodyRazdel, "Сгенерить серверный сертификат:", "Сгенерить сертификат", "Blue", "", "document.getElementsByClassName('divGenerateCertificat')[0].style.visibility = 'visible';", small: true);
			HttpService.GetButton(bodyRazdel, "Скачать/Установить сертификат на клиенте:", "Установка сертификата", "Blue", "", "document.getElementsByClassName('divLoadCertificat')[0].style.visibility = 'visible';", small: true);
			HttpService.GetLine(bodyRazdel);
			HttpService.GetInputBox(bodyRazdel, "Интервал ожидания команды в очереди (сек):", "RemoveCommandInterval", Global.Settings.RemoveCommandInterval.ToString(), "number");
			HttpService.GetHelpBox(bodyRazdel, "", "Интервал, в течении которого команда ожидает постановки на исполнение из очереди команд.<br/>\r\n                Если интервал просрочен - команда отменяется");
			HttpService.GetInputBox(bodyRazdel, "Логирование всех команд", "RegisterAllCommand", Global.Settings.RegisterAllCommand.ToString(), "checkbox", "", Disabled: false, "Логирование всех команд. Нужно только для отладки.");
			HttpService.GetInputBox(bodyRazdel, "Сбор статистики", "StatisticsСollection", Global.Settings.StatisticsСollection.ToString(), "checkbox", "", Disabled: false, "Сбор статистики по количеству и времени выполнения команд.");
			HttpService.GetInputBox(bodyRazdel, "Исп. КМ не прошедшие проверку", "MarkingCodeAcceptOnBad", Global.Settings.MarkingCodeAcceptOnBad.ToString(), "checkbox", "", Disabled: false, "Использовать 'Коды Маркировки' не прошедшие проверку.");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetHeadetBox(bodyRazdel, "", "Настройка активности и пере-инициализации ККМ:");
			HttpService.GetInputBox(bodyRazdel, "При ошибке: Активно = off", "SetNotActiveOnError", Global.Settings.SetNotActiveOnError.ToString(), "checkbox", "onclick='OnOfVisible(\"SetNotActiveOnError\", \"KkmIniter,KkmIniterInterval,SetNotActiveOnPaperOver\")'", Disabled: false, "Если Устройство перестает отвечать на команды то устройство будет помечаться как \"Не активная\"", "Если Устройство перестает отвечать на команды или будет ошибка связи с устройством то устройство будет помечаться как \"Не активная\".</br>\r\n                При подаче команды без указания конкретного устройства (например по ИНН) активные устройства не будут задействованы для выполнения такой команды.</br>\r\n                </br>\r\n                Для установки устройства \"Активным\" подайте любую команду конкретно на это устройство.</br>\r\n                Если устройство ответит оно пометится как \"Активное\"");
			HttpService.GetInputBox(bodyRazdel, "Нет бумаги: Активно = off", "SetNotActiveOnPaperOver", Global.Settings.SetNotActiveOnPaperOver.ToString(), "checkbox", "", !Global.Settings.SetNotActiveOnError, "Если в устройстве закончилась бумага то устройство будет помечаться как \"Не активная\"", "Если в устройстве закончилась бумага то устройство будет помечаться как \"Не активная\".</br>\r\n                При подаче команды без указания конкретного устройства (например по ИНН) активные устройства не будут задействованы для выполнения такой команды.</br>\r\n                </br>\r\n                Для установки устройства \"Активным\" подайте любую команду конкретно на это устройство.</br>\r\n                Если устройство ответит оно пометится как \"Активное\"");
			HttpService.GetInputBox(bodyRazdel, "Пере-инициализации ККМ:", "KkmIniter", Global.Settings.KkmIniter.ToString(), "checkbox", "", !Global.Settings.SetNotActiveOnError, "Включить пере-инициализацию не активных ККТ.", "Включить пере-инициализацию не активных ККТ.</br>\r\n                </br>\r\n                Через указанные интервалы времени сервер будет подавать команду на инициализацию \"Не активных\" устройств.</br>\r\n                Если устройство пройдет инициализацию оно будет помечено как \"Активное\"");
			HttpService.GetInputBox(bodyRazdel, "Интервал проверок (мин.):", "KkmIniterInterval", Global.Settings.KkmIniterInterval.ToString(), "number", "", !Global.Settings.SetNotActiveOnError, "", "Интервал проверки связи с не активными ККМ.");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetHeadetBox(bodyRazdel, "", "Настройка обратного вызова:");
			HttpService.GetInputBox(bodyRazdel, "Использовать обратный вызов:", "RunCallback", Global.Settings.RunCallback.ToString(), "checkbox", "onclick='OnOfVisible(\"RunCallback\", \"TimeCallback,TypeCallback\")'", Disabled: false, "Включение/выключение механизма обратного вызова.", "kkmsever может сам делать вызов учетной системы для получения данных для регистрации чеков и прочее.\r\n\r\n                WebSockets:\r\n                Передача идет текстовыми сообщениями, \r\n                Каждое водящее сообщение - это JSON команда, которая ставится в очередь на выполнение\r\n                Каждое исходящее сообщение - JSON ответ на поставленную команду. Ответы могут приходить в отличном от постановки порядке.    \r\n\r\n                HTTP:\r\n                Вызов идет по HTTP(POST) протоколу по указанным URL через интервалы указанные в настройке.\r\n                При вызове учетной системы kkmsever передает следующий JSON:\r\n                { ᅟCommand: \"GetCommand\",  // Признак что kkmsever просит команды\r\n                ᅟᅟToken: \"9DC7E960-6120-4989-86CB-25F77F59F0EA\", // Токен из настроек вызова\r\n                ᅟᅟListRezult: [ // здесь сервер возвращает результаты выполнения предыдущих команд\r\n                ᅟᅟᅟᅟ{ᅟ     \"CheckNumber\": 0,\r\n                    ᅟᅟᅟᅟᅟᅟ\"LineLength\": 48,\r\n                    ᅟᅟᅟᅟᅟᅟ\"Command\": \"GetLineLength\",\r\n                    ᅟᅟᅟᅟᅟᅟ\"Error\": \"\",\r\n                    ᅟᅟᅟᅟᅟᅟ\"Status\": 0,\r\n                    ᅟᅟᅟᅟᅟᅟ\"IdCommand\": \"0a4d1493-305d-091f-dedd-18616df5d371\"\r\n                ᅟᅟᅟᅟ},\r\n                ᅟᅟ... ] \r\n                }\r\n\r\n                Учетная система должна вернуть в теле JSON список команд для выполнения kkmsever-ом:\r\n                {ᅟListCommand: [ // Список команд\r\n                ᅟᅟᅟᅟ{   ᅟᅟCommand = \"List\",\r\n                    ᅟᅟᅟᅟᅟᅟIdCommand = \"6FA28ECE-0766-4479-BB37-F1343EA9CDBF\"\r\n                ᅟᅟᅟᅟ},\r\nᅟᅟᅟ ᅟ               {       Command = \"GetDataKKT\",\r\n                    ᅟᅟᅟᅟᅟᅟNumDevice = 1,\r\n                    ᅟᅟᅟᅟᅟᅟIdCommand = \"3B985644-7CF5-47B6-BA18-B54A634B67F3\"\r\n                ᅟᅟᅟᅟ},\r\n                ᅟᅟ...]\r\n                }".Replace("\r", "<br/>"));
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("0", "HTTP/HTTPS PUT");
			dictionary2.Add("2", "HTTP/HTTPS POST");
			dictionary2.Add("3", "WebSockets (Standart)");
			HttpService.GetSelectBox(bodyRazdel, "Тип обратного вызова:", "TypeCallback", Global.Settings.TypeCallback.ToString(), dictionary2, "", !Global.Settings.RunCallback);
			HttpService.GetInputBox(bodyRazdel, "Интервал вызова (сек):", "TimeCallback", Global.Settings.TimeCallback.ToString(), "number", "", !Global.Settings.RunCallback);
			HttpService.GetInputBox(bodyRazdel, "Логирование вызовов", "RegisterCallback", Global.Settings.RegisterCallback.ToString(), "checkbox", "", !Global.Settings.RunCallback, "Логирование всех вызовов. Нужно только для отладки.");
			if (Global.Settings.RunCallback)
			{
				HttpService.GetButton(bodyRazdel, "Настройки обратного вызова", "Список вызовов:", "Blue", "", "location.href = '/SettingListCallback'", small: true);
			}
			string text3 = "";
			if (Global.CallbackErrorConnect != "")
			{
				text3 = text3 + ((text3 != "") ? "</br>" : "") + "Ошибка соединения: " + Global.CallbackErrorConnect;
			}
			if (Global.CallbackErrorAut != "")
			{
				text3 = text3 + ((text3 != "") ? "</br>" : "") + "Ошибка аутентификации: " + Global.CallbackErrorAut;
			}
			if (Global.CallbackErrorReceive != "")
			{
				text3 = text3 + ((text3 != "") ? "</br>" : "") + "Ошибка получения данных: " + Global.CallbackErrorReceive;
			}
			if (Global.CallbackErrorSend != "")
			{
				text3 = text3 + ((text3 != "") ? "</br>" : "") + "Ошибка отправки данных: " + Global.CallbackErrorSend;
			}
			if (Global.CallbackError != "")
			{
				text3 = text3 + ((text3 != "") ? "</br>" : "") + "Ошибка: " + Global.CallbackError;
			}
			HttpService.GetText(bodyRazdel, "Последняя ошибка callback:", text3, "Red");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetButton(bodyRazdel, "Отключить все устройства:", "Отключить все устройства", "Blue", "/StopUnits", "", small: true);
			HttpService.GetButton(bodyRazdel, "Подключить все устройства:", "Подключить все устройства", "Blue", "/StartUnits", "", small: true);
			HttpService.GetButton(bodyRazdel, "Перезапустить kkmserver:", "Перезапустить kkmserver", "Blue", "/RestartKkmservr", "", small: true);
			HttpService.GetLine(bodyRazdel);
			HttpService.GetText(bodyRazdel, "Новые версии: ", "Страница для скачивания новых версий kkmserver", "", "https://kkmserver.ru/KkmServer#Donload");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetFormEnd(bodyRazdel, "", "Сохранить настройки");
			bodyRazdel.Replace("&ТелоСтраницы", text);
			bodyRazdel.Replace("&ТелоСтраницы", text2);
			stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
			stringBuilder.Replace("&ТелоСтраницы", "");
			stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
			return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
		}
		catch (Exception ex)
		{
			return new HttpResponse(Request, HttpStatusCode.OK, Global.GetErrorMessagee(ex));
		}
	}

	private async Task<HttpResponse> SetServerSetting(HttpRequest Request)
	{
		bool RedirectFromNewSert = false;
		_ = Global.Settings.OldSSLProtocol;
		string Old_SSLProtocol = Global.Settings.SSLProtocol;
		Global.Settings.ipPortOld = Global.Settings.ipPort;
		Global.Settings.ipPort = int.Parse(Request.ArgForm["ipPort"]);
		Global.Settings.SSLProtocol = Request.ArgForm["SSLProtocol"];
		string ServerSertificate = Global.Settings.ServerSertificate;
		Global.Settings.LoginAdmin = Request.ArgForm["LoginAdmin"];
		Global.Settings.PassAdmin = Request.ArgForm["PassAdmin"];
		Global.Settings.LoginUser = Request.ArgForm["LoginUser"];
		Global.Settings.PassUser = Request.ArgForm["PassUser"];
		if (Global.Settings.ServerSertificate != Request.ArgForm["ServerSertificate"])
		{
			RedirectFromNewSert = true;
		}
		Global.Settings.ServerSertificate = HttpUtility.HtmlDecode(Request.ArgForm["ServerSertificate"]);
		Global.Settings.RemoveCommandInterval = int.Parse(Request.ArgForm["RemoveCommandInterval"]);
		Global.Settings.RegisterAllCommand = Request.ArgForm.ContainsKey("RegisterAllCommand");
		Global.Settings.StatisticsСollection = Request.ArgForm.ContainsKey("StatisticsСollection");
		Global.Settings.MarkingCodeAcceptOnBad = Request.ArgForm.ContainsKey("MarkingCodeAcceptOnBad");
		Global.Settings.SetNotActiveOnError = Request.ArgForm.ContainsKey("SetNotActiveOnError");
		if (Global.Settings.SetNotActiveOnError)
		{
			Global.Settings.SetNotActiveOnPaperOver = Request.ArgForm.ContainsKey("SetNotActiveOnPaperOver");
			Global.Settings.KkmIniterInterval = int.Parse(Request.ArgForm["KkmIniterInterval"]);
		}
		Global.Settings.KkmIniter = Request.ArgForm.ContainsKey("KkmIniter") && Global.Settings.SetNotActiveOnError;
		Global.Settings.RunCallback = Request.ArgForm.ContainsKey("RunCallback");
		if (Request.ArgForm.ContainsKey("TimeCallback"))
		{
			Global.Settings.TypeCallback = int.Parse(Request.ArgForm["TypeCallback"]);
			Global.Settings.TimeCallback = int.Parse(Request.ArgForm["TimeCallback"]);
			Global.Settings.RegisterCallback = Request.ArgForm.ContainsKey("RegisterCallback");
		}
		await Global.SaveSettingsAsync();
		HttpResponse result = new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "Settings", "", "", RedirectFromNewSert, !RedirectFromNewSert), 5));
		bool RestartHTTTP = true;
		if (Global.Settings.ipPortOld == Global.Settings.ipPort && ServerSertificate == Global.Settings.ServerSertificate && Old_SSLProtocol == Global.Settings.SSLProtocol)
		{
			RestartHTTTP = false;
		}
		new Task(delegate
		{
			Task.Delay(1000);
			Global.RestartServer(RestartHTTTP);
		}).Start();
		return result;
	}

	private async Task<HttpResponse> StopUnits(HttpRequest Request)
	{
		Global.UnitManager.FreeUnit();
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "Settings"), 0));
	}

	private async Task<HttpResponse> StartUnits(HttpRequest Request)
	{
		Global.UnitManager.FreeUnit();
		await Global.UnitManager.LoadUnit();
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "Settings"), 0));
	}

	private async Task<HttpResponse> RestartKkmservr(HttpRequest Request)
	{
		new Task(delegate
		{
			Global.RestartKkmservr();
		}).Start();
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "Settings"), 0));
	}

	private async Task<HttpResponse> SettingListCallback(HttpRequest Request)
	{
		StringBuilder stringBuilder = HttpService.RootHtml();
		stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(stringBuilder, "Settings", Request.httpArg[1]);
		stringBuilder.Replace("&СписокУстройств", "");
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Настройки списка обратного вызова");
		foreach (KeyValuePair<string, Global.SetCallback> item in Global.Settings.ListCallback)
		{
			HttpService.GetForm(bodyRazdel, "SettingListCallback", "/SetSettingListCallback");
			bodyRazdel.Replace("&ТелоСтраницы", "<input name=\"PosList\" class=\"input\" id=\"PosList\" type=\"hidden\" value=\"&value\">&ТелоСтраницы");
			bodyRazdel.Replace("&value", item.Key);
			HttpService.GetInputBox(bodyRazdel, "URL вызова:", "URL", item.Value.URL);
			HttpService.GetInputBox(bodyRazdel, "Логин:", "Login", item.Value.Login);
			HttpService.GetInputBox(bodyRazdel, "Пароль:", "Password", item.Value.Password);
			HttpService.GetInputBox(bodyRazdel, "Токен:", "Token", item.Value.Token);
			HttpService.GetHeadetBox(bodyRazdel, "", "<h5 style=\"margin:0px\">\r\n                        <input class='Button' value='Сохранить строку' type='submit'/>\r\n                        <span/>\r\n                        <input class='Button' value='Удалить строку' type='submit' formaction='" + HttpService.GetUrl(Request, "SetSettingListCallback/Del", Request.httpArg[1]) + "' style='background:rgba(145, 50, 51, 1); color:#FFF'/>\r\n                    </h5>", Any: true);
			HttpService.GetFormEnd(bodyRazdel);
			HttpService.GetLine(bodyRazdel);
		}
		HttpService.GetButton(bodyRazdel, "Добавить новую настройку:", "Добавить строку", "", "", "location.href = '/SetSettingListCallback/Add'");
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}

	private async Task<HttpResponse> SetSettingListCallback(HttpRequest Request)
	{
		if (Request.httpArg[1] == "Add")
		{
			Global.Settings.ListCallback.Add(Guid.NewGuid().ToString(), new Global.SetCallback());
		}
		else if (Request.httpArg[1] == "Del")
		{
			Global.Settings.ListCallback.Remove(Request.ArgForm["PosList"]);
			await Global.SaveSettingsAsync();
		}
		else
		{
			string text = Request.ArgForm["PosList"];
			Global.Settings.ListCallback[text].URL = Request.ArgForm["URL"];
			Global.Settings.ListCallback[text].Login = Request.ArgForm["Login"];
			Global.Settings.ListCallback[text].Password = Request.ArgForm["Password"];
			Global.Settings.ListCallback[text].Token = Request.ArgForm["Token"];
			await Global.SaveSettingsAsync();
		}
		Global.RestartServer(RestartHTTP: false);
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "SettingListCallback"), 0));
	}

	private async Task<HttpResponse> GenerateCertificat(HttpRequest Request)
	{
		string text = Request.ArgForm["DomainName"].Trim();
		if (text == "")
		{
			return new HttpResponse(Request, HttpStatusCode.OK, "Ошибка: Не указанно доменное имя!");
		}
		UtilSertificate.InstallNewSertificate(text);
		HttpResponse result = new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "Settings", "", "", GetHostFromSertificate: true, NotServerURL: false), 5));
		new Task(delegate
		{
			Task.Delay(1000);
			Global.RestartServer();
		}).Start();
		return result;
	}

	private async Task<HttpResponse> PermitRegim(HttpRequest Request)
	{
		try
		{
			string text = HttpUtility.UrlDecode(Request.httpArg[1]);
			Dictionary<string, string> listCertificate = UtilSertificate.GetListCertificate();
			StringBuilder stringBuilder = HttpService.RootHtml();
			KkmFactory.PermitRegim.LoadFromDevice();
			stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
			HttpService.CommandMenu(stringBuilder, "PermitRegim", Request.httpArg[1]);
			stringBuilder.Replace("&СписокУстройств", "");
			StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Настройки разрешительного режима на кассах");
			HttpService.GetText(bodyRazdel, "Информация: ", "Документация по 'Разрешительному режима на кассах", "", "https://kkmserver.ru/WiKi/PermitRegim");
			HttpService.GetText(bodyRazdel);
			HttpService.GetForm(bodyRazdel, "PermitRegim", "/SetPermitRegim");
			HttpService.GetHeadetBox(bodyRazdel, "", "<input class='Button' value='Сохранить настройки' type='submit'/>", Any: true);
			HttpService.GetLine(bodyRazdel);
			HttpService.GetInputBox(bodyRazdel, "Использовать сервера отладки", "PermitRegimDebug", Global.Settings.PermitRegimDebug.ToString(), "checkbox", "", Disabled: false, "Использовать сервера отладки честного знака. Нужно только для отладки.");
			int num = 0;
			foreach (KeyValuePair<string, PermitRegim.Setting> curSetting in KkmFactory.PermitRegim.CurSettingS)
			{
				if (!curSetting.Value.Active)
				{
					continue;
				}
				if (curSetting.Value.CertificateId == null)
				{
					curSetting.Value.CertificateId = "";
				}
				num++;
				string text2 = num.ToString();
				KkmFactory.PermitRegim.CalcGroupIds(curSetting.Value.GroupIds);
				HttpService.GetInputBox(bodyRazdel, "ИНН Организации:", "Inn" + text2, curSetting.Value.Inn, "", "", Disabled: false, "Ниже настройки режима для Организации", "", "", Visibility: true, "readonly='readonly'");
				HttpService.GetInputBox(bodyRazdel, "Токен авторизации:", "Token" + text2, curSetting.Value.Token, "", "", Disabled: false, "", "'Аутентификационный токен' для авторизации на серверах ГИС МТ<br/>\r\nПолучить токен можно в личном кабинете ГИС МТ.<br/>\r\nИнструкция получения токена описана в документации: {0}<br/>\r\nСрок действия токена ограничен датой 1 марта 2025 года<br/> \r\n- далее только по сертификату электронной подписи<br/>\r\nК этой дате требуется перейти на схему получения токена по Сертификату<br/>\r\nПри указании 'Аутентификационный токен' - указывать сертификат и пароль не нужно.<br/>\r\nЕсли указан сертификат - то 'Аутентификационный токен' указывать не нужно");
				Dictionary<string, string> dictionary = new Dictionary<string, string>(listCertificate);
				if (!dictionary.ContainsKey(curSetting.Value.CertificateId))
				{
					dictionary.Add("optgroup", "Не найдено в хранилище :");
					dictionary.Add(curSetting.Value.CertificateId, "<Не найдено>" + curSetting.Value.CertificateId);
				}
				StringBuilder startCollapsPanel = HttpService.GetStartCollapsPanel("Проверяемые товарные группы:", "AddPrd" + text2);
				HttpService.GetInputBox(startCollapsPanel, "<Все группы>", "GroupId" + text2 + "_0", curSetting.Value.GroupIds.Exists((int I) => I == 0).ToString(), "checkbox", "", Disabled: false, "(All)");
				foreach (KeyValuePair<int, string> Groud in KkmFactory.PermitRegim.NameGroupRus)
				{
					HttpService.GetInputBox(startCollapsPanel, KkmFactory.PermitRegim.NameGroupEng[Groud.Key], "GroupId" + text2 + "_" + Groud.Key, curSetting.Value.GroupIds.Exists((int I) => I == Groud.Key).ToString(), "checkbox", "", Disabled: false, Groud.Value);
				}
				HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
				HttpService.GetLine(bodyRazdel);
			}
			HttpService.GetFormEnd(bodyRazdel);
			stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.Replace("&ТелоСтраницы", "")?.ToString() + "&ТелоСтраницы");
			bodyRazdel = HttpService.GetBodyRazdel("Список серверов проверки разрещительного режима", Line: false);
			HttpService.GetButton(bodyRazdel, "Обновить список серверов проверки разрещительного режима:", "Обновить список серверов", "", "", "location.href = '/UpdatePermitRegimServer'", small: true);
			StringBuilder stringBuilder2 = new StringBuilder("<table class='table' style='width: 1200px;'>\r\n    <thead>\r\n        <tr>\r\n            <th style='width: 50px;'>Ping Time</th>\r\n            <th style='width: 50px;'>Последний вызов</th>\r\n            <th style='width: 40px;'>Вызовов</th>\r\n            <th style='width: 600px;'>URL сервера</th>\r\n            <th style='width: 50px;'>Заблокирован до</th>\r\n        </tr>\r\n    </thead>\r\n    <tbody>\r\n        &СтрокиТаблицы\r\n    </tbody>\r\n</table>\r\n<style>\r\n    td.FieldHistory {\r\n        border-bottom: 1px solid #d9d9d9;\r\n    }\r\n</style>\r\n&ТелоСтраницы");
			string value = "<tr style='cursor:pointer'>\r\n    <td class='FieldHistory' style='text-align: center; '>&PingTime</td>\r\n    <td class='FieldHistory' style='text-align: center; '>&LastUssage</td>\r\n    <td class='FieldHistory' style='text-align: right; '>&CoutUssage</td>\r\n    <td class='FieldHistory' style='text-align: left'>&Host</td>\r\n    <td class='FieldHistory' style='text-align: center; '>&Blocked</td>\r\n</tr>\r\n&СтрокиТаблицы";
			StringBuilder stringBuilder3 = new StringBuilder();
			KkmFactory.PermitRegim.ListCdnPlatformSort();
			foreach (Global.CdnPlatform item in Global.Settings.ListCdnPlatform)
			{
				stringBuilder3.Clear().Append(value);
				stringBuilder3.Replace("&PingTime", item.PingTime.ToString());
				stringBuilder3.Replace("&LastUssage", item.LastUssage.ToString("dd.MM.yyyy"));
				stringBuilder3.Replace("&CoutUssage", item.CoutUssage.ToString());
				stringBuilder3.Replace("&Host", item.Host);
				stringBuilder3.Replace("&Blocked", (!item.Blocked.HasValue) ? "" : item.Blocked.Value.ToString("dd.MM.yyyy"));
				stringBuilder2.Replace("&СтрокиТаблицы", stringBuilder3.ToString());
			}
			stringBuilder2.Replace("&СтрокиТаблицы", "");
			bodyRazdel.Replace("&ТелоСтраницы", stringBuilder2.ToString());
			HttpService.GetText(bodyRazdel);
			HttpService.GetLine(bodyRazdel);
			stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.Replace("&ТелоСтраницы", "")?.ToString() + "&ТелоСтраницы");
			bodyRazdel = HttpService.GetBodyRazdel("Логи вызовов серверов 'Разрешительного режима'", Line: false);
			StringBuilder stringBuilder4 = new StringBuilder("<table class='table' style='width: 1200px;'>\r\n    <thead>\r\n        <tr>\r\n            <th style='width: 100px;'>Дата</th>\r\n            <th style='width: 500px;'>Команда</th>\r\n            <th style='width: 30px;'>Время вызова</th>\r\n            <th style='width: 50px;'>Статус</th>\r\n        </tr>\r\n    </thead>\r\n    <tbody>\r\n        &СтрокиТаблицы\r\n    </tbody>\r\n</table>\r\n<style>\r\n    td.FieldHistory {\r\n        border-bottom: 1px solid #d9d9d9;\r\n    }\r\n</style>\r\n&ТелоСтраницы");
			string value2 = "<tr style='cursor:pointer' onclick=\"window.location.assign('/&Url/&DtLog')\"'>\r\n    <td class='FieldHistory' style='text-align: center; '>&Date</td>\r\n    <td class='FieldHistory' style='text-align: left; '>&Command</td>\r\n    <td class='FieldHistory' style='text-align: center; '>&Time</td>\r\n    <td class='FieldHistory' style='text-align: center'>&StatusCode</td>\r\n</tr>\r\n&СтрокиТаблицы";
			string value3 = "<tr style='cursor:pointer' onclick=\"window.location.assign('/&Url/&DtLog')\">\r\n    <td colspan='4' style='border-bottom: 1px solid #d9d9d9; color: red'>&Error</td>\r\n</tr>\r\n&СтрокиТаблицы";
			StringBuilder stringBuilder5 = new StringBuilder();
			StringBuilder stringBuilder6 = new StringBuilder();
			DateTime? dateTime = null;
			if (text != null && text != "")
			{
				dateTime = DateTime.ParseExact(text, "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
			}
			KkmFactory.PermitRegim.Log.Sort();
			foreach (PermitRegim.Log log in KkmFactory.PermitRegim.Log.Logs)
			{
				if (dateTime.HasValue)
				{
					DateTime date = log.Date;
					DateTime? dateTime2 = dateTime;
					if (date != dateTime2)
					{
						continue;
					}
				}
				stringBuilder5.Clear().Append(value2);
				_ = log.Date;
				stringBuilder5.Replace("&Date", log.Date.ToString("dd.MM.yyyy hh:mm:ss"));
				stringBuilder5.Replace("&Command", log.Command);
				stringBuilder5.Replace("&Time", log.Time.ToString());
				stringBuilder5.Replace("&StatusCode", log.StatusCode.ToString());
				stringBuilder5.Replace("&Url", "PermitRegim");
				_ = log.Date;
				stringBuilder5.Replace("&DtLog", log.Date.ToString("yyyy.MM.dd HH:mm:ss:fff"));
				stringBuilder4.Replace("&СтрокиТаблицы", stringBuilder5.ToString());
				if (log.Error != null && log.Error != "")
				{
					stringBuilder6.Clear().Append(value3);
					stringBuilder6.Replace("&Url", "PermitRegim");
					_ = log.Date;
					stringBuilder6.Replace("&DtLog", log.Date.ToString("yyyy.MM.dd HH:mm:ss:fff"));
					stringBuilder6.Replace("&Error", log.Error);
					stringBuilder4.Replace("&СтрокиТаблицы", stringBuilder6.ToString());
				}
				if (dateTime.HasValue)
				{
					stringBuilder4.Append("<div style='width: 1200px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>URL запроса:</div>");
					stringBuilder4.Append("<div style='width: 1000px; padding-left: 20px;'>" + ((log.URL == null) ? "<Запроса не было>" : log.URL.Replace("\r\n", "</br>").Replace("\r", "</br>")) + "</div>");
					stringBuilder4.Append("<div style='width: 1200px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Текст команды:</div>");
					stringBuilder4.Append("<div style='width: 1000px; padding-left: 20px;'>" + ((log.Request == null) ? "<Без тела запроса>" : log.Request.Replace("\r\n", "</br>").Replace("\r", "</br>")) + "</div>");
					stringBuilder4.Append("<div style='width: 1200px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Статус запроса:</div>");
					string[] obj = new string[7] { "<div style='width: 1200px; padding-left: 20px;'>", null, null, null, null, null, null };
					int statusCode = (int)log.StatusCode;
					obj[1] = statusCode.ToString();
					obj[2] = " : ";
					obj[3] = log.StatusCode.ToString();
					obj[4] = " ";
					obj[5] = ((log.Error == null) ? "" : log.Error.Replace("\r\n", "</br>").Replace("\r", "</br>"));
					obj[6] = "</div>";
					stringBuilder4.Append(string.Concat(obj));
					stringBuilder4.Append("<div style='width: 1200px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Текст ответа:</div>");
					stringBuilder4.Append("<div style='width: 1200px; padding-left: 20px;'>" + ((log.Response == null) ? "<Без тела ответа>" : log.Response.Replace("\r\n", "</br>").Replace("\r", "</br>")) + "</div>");
				}
			}
			stringBuilder4.Replace("&СтрокиТаблицы", "");
			bodyRazdel.Replace("&ТелоСтраницы", stringBuilder4.ToString());
			stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
			stringBuilder.Replace("&ТелоСтраницы", "");
			stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
			return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
		}
		catch (Exception ex)
		{
			return new HttpResponse(Request, HttpStatusCode.OK, Global.GetErrorMessagee(ex));
		}
	}

	private async Task<HttpResponse> SetPermitRegim(HttpRequest Request)
	{
		Global.Settings.PermitRegimDebug = Request.ArgForm.ContainsKey("PermitRegimDebug");
		int i;
		for (i = 1; i < 100; i++)
		{
			if (!Request.ArgForm.ContainsKey("Inn" + i))
			{
				break;
			}
		}
		for (int j = 1; j < i; j++)
		{
			try
			{
				string text = Request.ArgForm["Inn" + j];
				PermitRegim.Setting setting = KkmFactory.PermitRegim.CurSettingS[text];
				setting.Token = Request.ArgForm["Token" + j];
				setting.GroupIds.Clear();
				for (int k = 0; k < 30; k++)
				{
					if (Request.ArgForm.ContainsKey("GroupId" + j + "_" + k))
					{
						setting.GroupIds.Add(k);
					}
				}
				KkmFactory.PermitRegim.CalcGroupIds(setting.GroupIds);
			}
			catch
			{
			}
		}
		await Global.SaveSettingsAsync();
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "PermitRegim"), 0));
	}

	private async Task<HttpResponse> UpdatePermitRegimServer(HttpRequest Request)
	{
		string text = await KkmFactory.PermitRegim.GetListServers(CheckTime: false);
		if (text != null && text != "")
		{
			return new HttpResponse(Request, HttpStatusCode.OK, text);
		}
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "PermitRegim"), 0));
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	private async Task<HttpResponse> License(HttpRequest Request)
	{
		string Email = Global.Settings.LicenseEmail;
		string NamePK = "";
		if (Email.IndexOf(':') != -1)
		{
			NamePK = Email.Substring(0, Email.IndexOf(':'));
			NamePK = NamePK.Substring(0, Math.Min(100, NamePK.Length));
			Email = Email.Substring(Email.IndexOf(':') + 1);
		}
		StringBuilder html = HttpService.RootHtml();
		html.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(html, "License", Request.httpArg[0]);
		html.Replace("&СписокУстройств", "");
		StringBuilder htmlBody = HttpService.GetBodyRazdel("Лицензия");
		StringBuilder FormGenerateTable = new StringBuilder("\r\n                <table class=\"Lic\">\r\n                    <thead>\r\n                        <tr>\r\n                            <th>\r\n                                <div style = \"width : 300px; \" >ИНН ККТ</div>\r\n                            </th>\r\n                            <th>\r\n                                <div style = \"width : 100px; \" >Лицензия</div>\r\n                            </th>\r\n                            <th>\r\n                                <div style = \"width : 200px; \" >До даты</div>\r\n                            </th>\r\n                        </tr>\r\n                    </thead>\r\n                    <tbody>\r\n                    &СтрокиТаблицы\r\n                    </tbody>\r\n                </table>\r\n            &ТелоСтраницы");
		string FormGenerateStringTablr = "\r\n                        <tr>\r\n                            <td>\r\n                                <div style = \"width : 300px; \" >&INN</div>\r\n                            </td>\r\n                            <td>\r\n                                <div style = \"width : 100px; \" >&Lic</div>\r\n                            </td>\r\n                            <td>\r\n                                <div style = \"width : 200px; \" >&Date</div>\r\n                            </td>\r\n                       </tr>\r\n                       &СтрокиТаблицы";
		foreach (ComDevice.ItemComDevices item in await ComDevice.ReadComDevices())
		{
			if (item.Id.Trim().Length <= 12 || item.Count != 0)
			{
				FormGenerateTable.Replace("&СтрокиТаблицы", FormGenerateStringTablr);
				FormGenerateTable.Replace("&INN", item.Id);
				_ = item.DateAction;
				if (item.DateAction < DateTime.Today)
				{
					item.Count = 0;
				}
				if (item.Count == 1)
				{
					FormGenerateTable.Replace("&Lic", "+");
				}
				else if (item.Count == 0)
				{
					FormGenerateTable.Replace("&Lic", "-");
				}
				else
				{
					FormGenerateTable.Replace("&Lic", item.Count.ToString());
				}
				FormGenerateTable.Replace("&Date", item.DateAction.ToString("dd.MM.yyyy"));
			}
		}
		FormGenerateTable.Replace("&СтрокиТаблицы", "");
		htmlBody.Replace("&ТелоСтраницы", FormGenerateTable.ToString());
		await ComDevice.NewComDevice();
		HttpService.GetForm(htmlBody, "License", "/GetLicense", "get", "zoom: 0.6;");
		htmlBody.Replace("&ТелоСтраницы", "<div>\r\n                    <div class='Caption' align='right'></div>\r\n                    <div class='input' style='width: 400px; '> \r\n                        <h5 style='zoom: 1.8;'>\r\n                            <input class='Button' value='Получить лицензии' type='submit' formaction='/GetLicense'/>\r\n                            <span/>\r\n                        </h5>\r\n                    </div>\r\n                </div>\r\n                &ТелоСтраницы");
		HttpService.GetLine(htmlBody);
		html.Replace("&ТелоСтраницы", htmlBody.ToString());
		htmlBody = HttpService.GetBodyRazdel("Получение пароля от личного кабинета на сайте kkmserver.ru", Line: false);
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("(Нужно только для компаний работающий по сублицензированию!)", Line: false);
		htmlBody.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		HttpService.GetInputBox(htmlBody, "Email:", "LicenseEmail", Email);
		HttpService.GetInputBox(htmlBody, "Пароль:", "LicenseSass", Global.Settings.LicensePass);
		HttpService.GetInputBox(htmlBody, "Наименование (не обязательно):", "NamePK", NamePK);
		HttpService.GetHelpBox(htmlBody, "", "Имя машины или имя клиента на который выделяется лицензия.</br>Не обязательный реквизит. Можно не заполнять.</br>При указании поможет быстрее найти серийный номер.</br>Желательно указывать разное имя для разных машин");
		string header;
		try
		{
			header = await ComDevice.CalculationKey();
		}
		catch
		{
			header = "";
		}
		HttpService.GetHeadetBox(htmlBody, "Серийный номер:", header);
		ComDevice.InDate inDate = await ComDevice.ReadComDevice(null, AllowDateAction: true, AllowCount: true, OnlySerial: true);
		if (inDate.Int > 0)
		{
			HttpService.GetHeadetBox(htmlBody, "", "Лицензия получена. (до " + inDate.DateTime.ToString("dd.MM.yyyy") + ")");
			if (Global.ErrorLicense != "")
			{
				HttpService.GetHelpBox(htmlBody, "", Global.ErrorLicense ?? "");
			}
		}
		else
		{
			bool flag = Global.ErrorLicense == "";
			if (flag)
			{
				flag = await ComDevice.NewComDevice() == 0;
			}
			if (!flag && Global.ErrorLicense != "")
			{
				HttpService.GetHeadetBox(htmlBody, "", "Ошибка получении лицензии: <br/>" + Global.ErrorLicense);
			}
		}
		HttpService.GetLine(htmlBody);
		HttpService.GetFormEnd(htmlBody);
		HttpService.GetText(htmlBody, "Оплата лицензии:", "Страница приобретения лицензии", "", "https://kkmserver.ru/KkmServer#Payment");
		html.Replace("&ТелоСтраницы", htmlBody.ToString());
		html.Replace("&ТелоСтраницы", "");
		html.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
	}

	private async Task<HttpResponse> GetLicense(HttpRequest Request)
	{
		string text = Request.ArgForm["LicenseEmail"].Trim();
		string machineName = Environment.MachineName;
		machineName = machineName.Substring(0, Math.Min(100, machineName.Length));
		machineName = machineName.Replace('/', '-').Replace(':', '-').Replace('@', '-')
			.Replace('\\', '-');
		Global.Settings.LicenseEmail = ((machineName == "") ? "" : (machineName + ":")) + text;
		Global.Settings.LicensePass = Request.ArgForm["LicenseSass"];
		await Global.SaveSettingsAsync();
		int countLic = 1;
		Global.ErrorLicense = "";
		try
		{
			await ComDevice.GetComSettings(countLic, Clear: false);
		}
		catch (ArgumentException ex)
		{
			Global.ErrorLicense = ex.Message.Substring(ex.Message.IndexOf("Error # ") + 8);
		}
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "License"), 5));
	}

	private async Task<HttpResponse> ClearLicense(HttpRequest Request)
	{
		string text = Request.ArgForm["LicenseEmail"].Trim();
		string machineName = Environment.MachineName;
		machineName = machineName.Substring(0, Math.Min(100, machineName.Length));
		machineName = machineName.Replace('/', '-').Replace(':', '-').Replace('@', '-')
			.Replace('\\', '-');
		Global.Settings.LicenseEmail = ((machineName == "") ? "" : (machineName + ":")) + text;
		Global.Settings.LicensePass = Request.ArgForm["LicenseSass"];
		await Global.SaveSettingsAsync();
		await ComDevice.GetComSettings(0, Clear: true);
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "License"), 5));
	}

	private async Task<HttpResponse> UnitSettings(HttpRequest Request)
	{
		string NumberUnit = Request.httpArg[1];
		bool flag = true;
		Unit Unit = null;
		StringBuilder html = ((!(NumberUnit == "")) ? HttpService.RootHtml("InspectionValues()") : HttpService.RootHtml());
		StringBuilder bodyRazdel;
		try
		{
			int.Parse(NumberUnit);
			Unit = Global.UnitManager.Units[int.Parse(NumberUnit)];
			if (Unit == null)
			{
				bodyRazdel = HttpService.GetBodyRazdel("Настройка устройства (Ошибка инициализации)");
				HttpService.GetForm(bodyRazdel, "UnitSettings", HttpService.GetUrl(Request, "SetUnitSettings", Request.httpArg[1]));
				HttpService.GetHeadetBox(bodyRazdel, "", "<h5>\r\n                        <input class='Button' value='Сохранить настройки' type='submit'/>\r\n                        <span/>\r\n                        <input class='Button' value='Удалить устройство' type='submit' formaction='" + HttpService.GetUrl(Request, "DeleteUnit", Request.httpArg[1]) + "' style='background:rgba(145, 50, 51, 1); color:#FFF'/>\r\n                    </h5>", Any: true);
				html.Replace("&ТелоСтраницы", bodyRazdel.ToString());
				html.Replace("&ТелоСтраницы", "");
				html.Replace("&ДополнительныеСкрипты", "</script>");
				return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
			}
		}
		catch
		{
			flag = false;
		}
		html = ((!flag) ? HttpService.RootHtml("", AddUnitTestJS: true) : HttpService.RootHtml("InspectionValues()", AddUnitTestJS: true));
		html.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(html, "UnitSettings", Request.httpArg[1]);
		HttpService.UnitsMenu(html, "UnitSettings", Request.httpArg[1], AddUnit: true, UnitPassword);
		if (flag)
		{
			await Unit.Semaphore.WaitAsync();
			try
			{
				Unit.UnitSettings.Paramerts.Clear();
				Unit.UnitParamets.Clear();
				Unit.LoadParamets();
				Unit.LoadParametsFromSettings(Unit.SettDr);
			}
			finally
			{
				Unit.Semaphore.Release();
			}
			_ = Unit.SettDr;
			string protocol = Unit.SettDr.TypeDevice.Protocol;
			TypeDevice.enType type = Unit.SettDr.TypeDevice.Type;
			StringBuilder stringBuilder = new StringBuilder("&ТелоСкрипта");
			int num = (int)type;
			num.ToString();
			string text = ((Unit == null || Unit.NameDevice == "") ? "" : (", " + Unit.NameDevice));
			bodyRazdel = HttpService.GetBodyRazdel("Настройка устройства" + text);
			HttpService.GetForm(bodyRazdel, "UnitSettings", HttpService.GetUrl(Request, "SetUnitSettings", Request.httpArg[1]));
			HttpService.GetHeadetBox(bodyRazdel, "", "<h5>\r\n                        <input class='Button' value='Сохранить настройки' type='submit'/>\r\n                        <span/>\r\n                        <input class='Button' value='Удалить устройство' type='submit' formaction='" + HttpService.GetUrl(Request, "DeleteUnit", Request.httpArg[1]) + "' style='background:rgba(145, 50, 51, 1); color:#FFF'/>\r\n                    </h5>", Any: true);
			HttpService.GetText(bodyRazdel, "Тип устройства :", Unit.SettDr.TypeDevice.Name());
			HttpService.GetText(bodyRazdel, "Протокол :", protocol);
			if (!Unit.IsInit)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Не подключена: " + Unit.LastError.Split('\n')[0], "Red");
			}
			else if (!Unit.Active)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Выключена пользователем");
			}
			else if (Unit.UnitParamets.ContainsKey("NameDevice") && Unit.UnitParamets["NameDevice"] != "")
			{
				HttpService.GetText(bodyRazdel, "Статус :", "В работе (" + Unit.UnitParamets["NameDevice"] + ")");
			}
			else
			{
				HttpService.GetText(bodyRazdel, "Статус :", "В работе (" + Unit.NameDevice + ")");
			}
			if (Unit.UnitAdditionallinks != "")
			{
				HttpService.GetText(bodyRazdel, "Информация :", Unit.UnitAdditionallinks);
			}
			string text2 = "???";
			string text3 = "???";
			foreach (Unit.iUnitSettings.Paramert paramert3 in Unit.UnitSettings.Paramerts)
			{
				if (text2 != paramert3.Page || text3 != paramert3.Group)
				{
					HttpService.GetLine(bodyRazdel);
					string text4 = "";
					if (paramert3.Page == "" && paramert3.Group != "")
					{
						text4 = paramert3.Group;
					}
					else if (paramert3.Page != "" && paramert3.Group == "")
					{
						text4 = paramert3.Page;
					}
					else if (paramert3.Page != "" && paramert3.Group != "")
					{
						text4 = paramert3.Group;
					}
					if (text4 != "")
					{
						HttpService.GetHeadetBox(bodyRazdel, "", text4 + ":");
					}
					text2 = paramert3.Page;
					text3 = paramert3.Group;
				}
				string text5 = Unit.UnitParamets[paramert3.Name];
				string help = "";
				string text6 = "onchange='InspectionValues()'";
				if (paramert3.SaveOnChange)
				{
					text6 = "onchange='SaveOnChange()'";
				}
				if (paramert3.VauePars.Count == 0)
				{
					string type2 = ((paramert3.TypeValue == "String") ? "text" : ((paramert3.TypeValue == "Number") ? "number" : ((!(paramert3.TypeValue == "Boolean")) ? "" : "checkbox")));
					if (paramert3.DefaultValue != "")
					{
						help = "По умолчанию: " + paramert3.DefaultValue;
					}
					if (paramert3.TypeValue == "String" && text5.IndexOf("\r\n") != -1)
					{
						int num2 = text5.Replace("\r\n", "\r").Split("\r\n".ToCharArray()).Length + 2;
						HttpService.GetTextArea(bodyRazdel, paramert3.Caption + ":", paramert3.Name, text5, num2.ToString(), text6, paramert3.ReadOnly, help, paramert3.Description);
					}
					else
					{
						HttpService.GetInputBox(bodyRazdel, paramert3.Caption + ":", paramert3.Name, text5, type2, text6, paramert3.ReadOnly, help, paramert3.Description);
					}
				}
				else
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					foreach (Unit.iUnitSettings.VauePar vauePar in paramert3.VauePars)
					{
						if (!dictionary.ContainsKey(vauePar.Value))
						{
							dictionary.Add(vauePar.Value, vauePar.Caption);
							if (paramert3.DefaultValue != "" && paramert3.DefaultValue == vauePar.Value)
							{
								help = "По умолчанию: " + vauePar.Caption;
							}
						}
					}
					dictionary = ChangeListSelection(dictionary, Unit.SettDr.IdTypeDevice, paramert3.Name);
					HttpService.GetSelectBox(bodyRazdel, paramert3.Caption + ":", paramert3.Name, text5.ToString(), dictionary, text6, paramert3.ReadOnly, help, paramert3.Description);
				}
				if (paramert3.Help != "")
				{
					HttpService.GetHelpBox(bodyRazdel, "", paramert3.Help);
				}
				if (paramert3.MasterParameterName != "" && paramert3.MasterParameterValue != "")
				{
					Unit.iUnitSettings.Paramert paramert = Unit.FindUnitSettingsParam(paramert3.Name, FindOnCaption: true);
					Unit.iUnitSettings.Paramert paramert2 = Unit.FindUnitSettingsParam(paramert3.MasterParameterName, FindOnCaption: true);
					bool flag2 = paramert2.VauePars.Count > 0;
					bool num3 = paramert2.TypeValue == "Boolean" && !flag2;
					string text7 = (paramert3.MasterParameterOperation.Contains("NotEqual") ? " != " : " == ");
					string text8 = (num3 ? "" : "'");
					if (paramert3.TypeValue == "String" && text5.IndexOf("\r\n") != -1)
					{
						stringBuilder.Replace("&ТелоСкрипта", "SetVisible('Div" + paramert.Name + "', IsSelect('" + paramert2.Name + "') " + text7 + text8 + paramert3.MasterParameterValue + text8 + ");\r\n                                    &ТелоСкрипта");
					}
					else
					{
						stringBuilder.Replace("&ТелоСкрипта", "SetActive('" + paramert.Name + "', IsSelect('" + paramert2.Name + "') " + text7 + text8 + paramert3.MasterParameterValue + text8 + ");\r\n                                    &ТелоСкрипта");
					}
				}
			}
			HttpService.GetFormEnd(bodyRazdel);
			if (Unit.UnitActions.Count != 0)
			{
				HttpService.GetLine(bodyRazdel);
				foreach (KeyValuePair<string, string> unitAction in Unit.UnitActions)
				{
					HttpService.GetButtonTest(bodyRazdel, ": Доп. функция: " + unitAction.Value, unitAction.Value, "", "DoAdditionalAction(" + NumberUnit + ", \"" + unitAction.Key + "\")");
				}
			}
			HttpService.GetLine(bodyRazdel);
			HttpService.GetText(bodyRazdel, "Версия :", Unit.UnitVersion);
			HttpService.GetText(bodyRazdel, "Наименование :", Unit.UnitName);
			if (Unit.UnitDescription != "")
			{
				HttpService.GetText(bodyRazdel, "Описание :", Unit.UnitDescription.Replace("\n", "<br/>"));
			}
			if (Unit.UnitDownloadURL != "" && Unit.UnitDownloadURL.ToLower().IndexOf("http://") != 0 && Unit.UnitDownloadURL.ToLower().IndexOf("https://") != 0)
			{
				HttpService.GetText(bodyRazdel, "URL производителя :", HttpUtility.UrlDecode(Unit.UnitDownloadURL), "", "http://" + Unit.UnitDownloadURL);
			}
			if (Unit.UnitIntegrationLibrary && !Unit.UnitMainDriverInstalled)
			{
				HttpService.GetText(bodyRazdel, "Примечание :", "Требуется установка драйвера производителя.");
			}
			if (Unit.SettDr.TypeDevice.Description != "")
			{
				HttpService.GetText(bodyRazdel, "Дополнительное описание :", Unit.SettDr.TypeDevice.Description);
			}
			if (Unit.LastError != "")
			{
				HttpService.GetText(bodyRazdel, "Ошибка :", Unit.LastError, "Red");
			}
			bodyRazdel.Replace("&ТелоСтраницы", "<script type='text/javascript'>\r\n                        function SubmitType() {\r\n                            document.UnitSettings.submit();\r\n                        }\r\n                        function IsSelect(id) {\r\n                            Sel = document.getElementById(id); \r\n                            if (Sel.type == 'text') {\r\n                                Value = Sel.value;\r\n                            } else if (Sel.type == 'checkbox') {\r\n                                if (Sel.checked == true) {\r\n                                    Value = true;\r\n                                } else {\r\n                                    Value = false;\r\n                                }\r\n                            } else {\r\n                                Value = Sel.options[Sel.selectedIndex].value;\r\n                            }\r\n                            return Value;\r\n                        }\r\n                        function SetActive(id, Active) {\r\n                            DocItem = document.getElementById(id);\r\n                            DocItem.disabled = !Active;\r\n                        }\r\n                        function SetVisible(id, Visible) {\r\n                            DocItem = document.getElementById(id);\r\n                            DocItem.hidden = !Visible;\r\n                            //DocItem.style.visibility = !Visible; visibility\r\n                        }\r\n                        function InspectionValues() {\r\n                            &ПроверкаПараметров;\r\n                        }\r\n                        function SaveOnChange() {\r\n                            InspectionValues();\r\n                            SubmitType();\r\n                        }\r\n                    </script>");
			bodyRazdel.Replace("&ПроверкаПараметров", stringBuilder.ToString());
			bodyRazdel.Replace("&ТелоСкрипта", "");
		}
		else
		{
			bodyRazdel = HttpService.GetBodyRazdel("Выберите или добавьте устройство");
			HttpService.GetButton(bodyRazdel, "Добавить устройство:", "Добавить устройство", "", "", "location.href = '/AddUnit'");
			HttpService.GetChangeForDevice(bodyRazdel, "Выберите устройство:");
		}
		html.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		html.Replace("&ТелоСтраницы", "");
		html.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
	}

	private async Task<HttpResponse> AddUnit(HttpRequest Request)
	{
		StringBuilder stringBuilder = HttpService.RootHtml();
		stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(stringBuilder, "UnitSettings", "UnitSettings");
		HttpService.UnitsMenu(stringBuilder, "UnitSettings", Request.httpArg[1], AddUnit: true, UnitPassword);
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Добавление устройства");
		bodyRazdel.Replace("&ТелоСтраницы", GetFormAddUnit());
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}

	private async Task<HttpResponse> SetAddUnit(HttpRequest Request)
	{
		StringBuilder stringBuilder = HttpService.RootHtml();
		bool flag = false;
		if (Request.ArgForm["Id"] != "")
		{
			foreach (KeyValuePair<string, TypeDevice> item in Global.UnitManager.ListTypeDevice)
			{
				if (item.Value.Type == (TypeDevice.enType)int.Parse(Request.ArgForm["NameType"]) && Request.ArgForm["Id"] == item.Value.Id)
				{
					flag = true;
				}
			}
		}
		if (Request.ArgForm["Id"] == "" || !flag)
		{
			stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
			HttpService.CommandMenu(stringBuilder, "UnitSettings", "UnitSettings");
			HttpService.UnitsMenu(stringBuilder, "UnitSettings", Request.httpArg[1], AddUnit: true, UnitPassword);
			StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Добавление устройства");
			bodyRazdel.Replace("&ТелоСтраницы", GetFormAddUnit(Request.ArgForm["NumberUnit"], Request.ArgForm["NameType"], Request.ArgForm["Id"]));
			stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
			stringBuilder.Replace("&ТелоСтраницы", "");
			stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
			return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
		}
		int numberUnit = int.Parse(Request.ArgForm["NumberUnit"]);
		Global.UnitManager.AddUnit(numberUnit, null, Request.ArgForm["Id"]);
		Thread.Sleep(5);
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "UnitSettings", numberUnit.ToString()), 0));
	}

	private string GetFormAddUnit(string NumberUnit = "0", string TypeUnit = "0", string Id = "")
	{
		StringBuilder stringBuilder = new StringBuilder("&ТелоСтраницы");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 1; i < Global.MaxUnit; i++)
		{
			if (!Global.Settings.Devices.ContainsKey(i))
			{
				dictionary.Add(i.ToString(), "Устройство #" + i);
			}
		}
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		string text = TypeUnit;
		int[] numNameType = TypeDevice.NumNameType;
		for (int j = 0; j < numNameType.Length; j++)
		{
			int num = numNameType[j];
			dictionary2.Add(num.ToString(), TypeDevice.NameType[num]);
			text = num.ToString();
		}
		if (dictionary2.Count == 2)
		{
			TypeUnit = text;
		}
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
		Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
		dictionary3.Add("", "<Не установлено>");
		string text2 = Id;
		foreach (KeyValuePair<string, TypeDevice> item in Global.UnitManager.ListTypeDevice)
		{
			if (item.Value.Type == (TypeDevice.enType)int.Parse(TypeUnit))
			{
				dictionary3.Add(item.Value.Id, item.Value.Protocol);
				text2 = item.Value.Id;
			}
			if (item.Value.Type == TypeDevice.enType.ФискальныйРегистратор && item.Value.UnitDevice != TypeDevice.enUnitDevice.Dll && item.Value.Id != "KktEmulator")
			{
				dictionary4.Add(item.Value.Id, item.Value.Protocol);
			}
		}
		if (dictionary3.Count == 2)
		{
			Id = text2;
		}
		HttpService.GetHeadetBox(stringBuilder, "", "Ручное добавление:");
		HttpService.GetForm(stringBuilder, "AddUnit", "/SetAddUnit");
		HttpService.GetSelectBox(stringBuilder, "Номер устройства:", "NumberUnit", NumberUnit, dictionary);
		HttpService.GetHelpBox(stringBuilder, "", "Номер, по которому можно обращаться к устройству");
		HttpService.GetSelectBox(stringBuilder, "Тип устройства:", "NameType", TypeUnit, dictionary2, "onchange='SubmitType()'");
		HttpService.GetHelpBox(stringBuilder, "", "Тип подключаемого устройства. Необходимо выбрать до выбора протокола.");
		HttpService.GetSelectBox(stringBuilder, "Протокол устройства:", "Id", Id, dictionary3);
		HttpService.GetHelpBox(stringBuilder, "", "Протокол по которому работает устройство.\r\n                    Для устройств одного производителя как правило применяется один протокол");
		HttpService.GetFormEnd(stringBuilder, "", "Добавить устройство");
		HttpService.GetLine(stringBuilder);
		stringBuilder.Replace("&ТелоСтраницы", "<script type='text/javascript'>\r\n                    function SubmitType() {\r\n                        sel = document.getElementById('Id'), \r\n                        opt=sel.options; \r\n                        o=opt[0]; \r\n                        o.selected=true; \r\n                        document.AddUnit.submit();\r\n                    }\r\n                </script>\r\n                &ТелоСтраницы");
		string text3 = "\r\n                    <div class=\"divScanKkm\" style=\"visibility: hidden; position: fixed; left: 300px; top: 300px; width: 600px; opacity: 1; background: #e4dbdd; padding:15px; border: solid; border-radius:12px; border-color:rgba(105, 105, 105, 1); z-index: 1;\">\r\n                        <form name=\"GenerateCertificat\" action=\"/UnitSettings\" method=\"get\">\r\n                            <h2 class=\"help\" style=\"margin-left: 8px;\">Поиск ККМ:</h2>\r\n                            <table border = \"0\" >\r\n                                <tr>\r\n                                    <td></td>\r\n                                    <td>\r\n                                        <h6 class=\"help\">\r\n                                            Внимание!:<br/>\r\n                                            Идет поиск ККМ.<br/>\r\n                                            Необходимо подождать 10-30 секунд.<br/>\r\n                                            <br/>\r\n                                        </h6>\r\n                                    </td>\r\n                                </tr>\r\n                            </table>\r\n                        </form>\r\n                    </div>\r\n                    &ТелоСтраницы";
		stringBuilder.Replace("&ТелоСтраницы", text3);
		return stringBuilder.ToString();
	}

	private async Task<HttpResponse> DeleteUnit(HttpRequest Request)
	{
		Global.UnitManager.AddUnit(int.Parse(Request.httpArg[1]), null, null);
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "UnitSettings"), 0));
	}

	private async Task<HttpResponse> SetUnitSettings(HttpRequest Request)
	{
		Unit unit = Global.UnitManager.Units[int.Parse(Request.httpArg[1])];
		Dictionary<string, string> dictionary = new Dictionary<string, string>(unit.SettDr.Paramets);
		foreach (Unit.iUnitSettings.Paramert paramert in unit.UnitSettings.Paramerts)
		{
			if (paramert.TypeValue == "Boolean" && !Request.ArgForm.ContainsKey(paramert.Name))
			{
				dictionary[paramert.Name] = ExtensionMethods.AsString(Val: false);
			}
			else if (Request.ArgForm.ContainsKey(paramert.Name))
			{
				dictionary[paramert.Name] = unit.CovertTypeValue(paramert.TypeValue, Request.ArgForm[paramert.Name]);
			}
		}
		unit.SaveParametrs(dictionary);
		Global.UnitManager.AddUnit(int.Parse(Request.httpArg[1]), unit.SettDr.IdDevice, unit.SettDr.IdTypeDevice, dictionary, InspectionPort: false, SetActive: true);
		return new HttpResponse(Request, HttpStatusCode.OK, HttpService.RedirectHTML(HttpService.GetUrl(Request, "UnitSettings", Request.httpArg[1]), 0));
	}

	private async Task<HttpResponse> UnitTest(HttpRequest Request)
	{
		StringBuilder stringBuilder = HttpService.RootHtml("", AddUnitTestJS: true);
		stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(stringBuilder, "UnitTest", Request.httpArg[1]);
		HttpService.UnitsMenu(stringBuilder, "UnitTest", Request.httpArg[1], AddUnit: false, UnitPassword);
		string text = Request.httpArg[1];
		bool flag = true;
		Unit unit = null;
		try
		{
			int.Parse(text);
			unit = Global.UnitManager.Units[int.Parse(text)];
		}
		catch
		{
			flag = false;
		}
		string text2 = ((unit == null || unit.NameDevice == "") ? "" : (", " + unit.NameDevice));
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Тест устройства" + text2);
		if (!flag)
		{
			HttpService.GetChangeForDevice(bodyRazdel, "Выберите устройство:");
		}
		StringBuilder stringBuilder2 = new StringBuilder("&ТелоСтраницы");
		HttpService.GetText(stringBuilder2, "Статус выполнения :", "", "", "", "MessageStatus");
		if (flag)
		{
			_ = unit.SettDr;
			string protocol = unit.SettDr.TypeDevice.Protocol;
			TypeDevice.enType num = unit.SettDr.TypeDevice.Types[0];
			bool flag2 = unit.Kkm.IsKKT;
			if (num == TypeDevice.enType.ПринтерЧеков && unit.UnitParamets["EmulationCheck"].AsBool() && unit.UnitParamets["RouteCommand"] != "")
			{
				flag2 = true;
			}
			HttpService.GetText(bodyRazdel, "Тип устройства :", unit.SettDr.TypeDevice.Name());
			HttpService.GetText(bodyRazdel, "Протокол :", protocol);
			if (!unit.IsInit)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Не подключена: " + unit.LastError.Split('\n')[0], "Red");
			}
			else if (!unit.Active)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Выключена пользователем");
			}
			else if (unit.UnitParamets.ContainsKey("NameDevice") && unit.UnitParamets["NameDevice"] != "")
			{
				HttpService.GetText(bodyRazdel, "Статус :", "В работе (" + unit.UnitParamets["NameDevice"] + ")");
			}
			else
			{
				HttpService.GetText(bodyRazdel, "Статус :", "В работе (" + unit.NameDevice + ")");
			}
			HttpService.GetLine(bodyRazdel);
			bool flag3 = false;
			foreach (TypeDevice.enType type in unit.SettDr.TypeDevice.Types)
			{
				switch (type)
				{
				case TypeDevice.enType.ФискальныйРегистратор:
				{
					if (flag3)
					{
						HttpService.GetLine(bodyRazdel);
					}
					flag3 = true;
					HttpService.GetButtonTest(bodyRazdel, ": Открыть смену на ККМ", "Открыть смену", "", "OpenShift(" + text + ", undefined, undefined, document.getElementById(\"PrintCheck\").checked)");
					HttpService.GetButtonTest(bodyRazdel, ": Закрыть смену на ККМ", "Закрыть смену", "", "CloseShift(" + text + ", undefined, undefined, document.getElementById(\"PrintCheck\").checked)");
					HttpService.GetButtonTest(bodyRazdel, ": Снятие суточного отчета без гашения", "X отчет", "", "XReport(" + text + ")");
					HttpService.GetLine(bodyRazdel);
					HttpService.GetButtonTest(bodyRazdel, ": Подать команду на открытие денежного ящика", "Открыть денежный ящик", "", "OpenCashDrawer(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Внесение наличных (1 коп.) средств в кассу ККМ", "Внесение наличных", "", "DepositingCash(" + text + ", undefined, undefined, document.getElementById(\"PrintCheck\").checked)");
					HttpService.GetButtonTest(bodyRazdel, ": Изъятие наличных средств из кассы ККМ ", "Изъятие наличных", "", "PaymentCash(" + text + ", undefined, undefined, document.getElementById(\"PrintCheck\").checked)");
					HttpService.GetButtonTest(bodyRazdel, ": Получить ширину строки чека в символах.", "Получить ширину", "", "GetLineLength(" + text + ")");
					if (flag2)
					{
						HttpService.GetButtonTest(bodyRazdel, ": Получить данные последнего чека", "Данные последнего чека", "", "GetDataCheck(" + text + ", 0)");
					}
					HttpService.GetButtonTest(bodyRazdel, ": Получить текущее состояние ККТ.", "Состояние ККТ", "", "GetDataKKT(" + text + ")");
					if (unit.Kkm.IsKKT && unit.Kkm.FfdVersion >= 3)
					{
						HttpService.GetButtonTest(bodyRazdel, ": Получить счетчики ФН.", "Счетчики ФН", "", "GetCounters(" + text + ")");
					}
					HttpService.GetLine(bodyRazdel);
					string value = "true";
					HttpService.GetInputBox(bodyRazdel, "Печатать текст:", "PrintCheck", value, "checkbox");
					HttpService.GetInputBox(bodyRazdel, "Печ. Штрих-коды и картинки:", "PrintBarcode", "false", "checkbox");
					bodyRazdel.Replace("&ТелоСтраницы", "<div class='InputValue' wfd-id='48'>\r\n                                <div class='input' wfd-id='49' style='margin-left: 80px;'>\r\n                                    <select class='input' name='TypeCheck' id='TypeCheck' wfd-id='50' style='width: 196px; font-size: 11px;'>\r\n                                        <option selected value='0'>Продажа: Чек прихода</option>\r\n                                        <option value = '1' >Продажа: Чек возв. прихода</option>\r\n                                        <option value = '2' >Продажа: Чек кор. прихода</option>\r\n                                        <option value = '3' >Продажа: Чек кор. возв. прихода</option>\r\n                                        <option value = '-' > --------------------------------------------</option>\r\n                                        <option value = '10'>Покупка: Чек расхода</option>\r\n                                        <option value = '11'>Покупка: Чек возв. расхода</option>\r\n                                        <option value = '12'>Покупка: Чек кор. расхода</option>\r\n                                        <option value = '13'>Покупка: Чек кор. возв. расхода</option>\r\n                                    </select>\r\n                                </div>\r\n                            </div>\r\n                            &ТелоСтраницы");
					HttpService.GetButtonTest(bodyRazdel, ": Печать тестового чека на 3 руб.", "Печать чека", "", "RegisterCheck(" + text + ", document.getElementById(\"TypeCheck\").value, document.getElementById(\"PrintBarcode\").checked, document.getElementById(\"PrintCheck\").checked, &IsAgent)");
					bodyRazdel.Replace("&IsAgent", "false");
					HttpService.GetButtonTest(bodyRazdel, ": Печать произвольного документа (слип-чек)", "Печать слип-чека", "", "PrintSlip(" + text + ", document.getElementById(\"PrintBarcode\").checked)");
					HttpService.GetText(stringBuilder2, "Номер чека :", "", "", "", "MessageCheckNumber", "style='display: none;'");
					HttpService.GetText(stringBuilder2, "Номер смены :", "", "", "", "MessageSessionNumber", "style='display: none;'");
					HttpService.GetText(stringBuilder2, "URL чека :", "", "", "", "MessageCheckURL", "style='display: none;'");
					HttpService.GetText(stringBuilder2, "Ширина строки :", "", "", "", "MessageLineLength", "style='display: none;'");
					break;
				}
				case TypeDevice.enType.ПринтерЧеков:
					if (flag3)
					{
						HttpService.GetLine(bodyRazdel);
					}
					flag3 = true;
					HttpService.GetButtonTest(bodyRazdel, ": Печать слип-чека", "Слип-чек", "", "PrintDocument(" + text + ", false)");
					HttpService.GetButtonTest(bodyRazdel, ": Печать слип-чека с ШК", "Слип-чек с ШК", "", "PrintDocument(" + text + ", true)");
					HttpService.GetButtonTest(bodyRazdel, ": Подать команду на открытие денежного ящика", "Открыть денежный ящик", "", "PrintOpenCashDrawer(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Получить ширину строки чека в символах.", "Получить ширину", "", "PrintLineLength(" + text + ")");
					HttpService.GetText(stringBuilder2, "Ширина строки :", "", "", "", "MessageLineLength", "style='display: none;'");
					break;
				case TypeDevice.enType.СканерШтрихкода:
					if (flag3)
					{
						HttpService.GetLine(bodyRazdel);
					}
					flag3 = true;
					HttpService.GetButtonTest(bodyRazdel, ": Включить сканирование штрих-кодов", "Включить сканирование", "", "GetBarcode(" + text + ", true)");
					HttpService.GetButtonTest(bodyRazdel, ": Отключить сканирование штрих-кодов", "Отключить сканирование", "", "GetBarcode(" + text + ", false)");
					HttpService.GetButtonTest(bodyRazdel, ": Напечатать команды настройки сканера", "Команды настройки", "", "PrintSettingsScanerBC()");
					break;
				case TypeDevice.enType.ЭлектронныеВесы:
					if (flag3)
					{
						HttpService.GetLine(bodyRazdel);
					}
					flag3 = true;
					HttpService.GetButtonTest(bodyRazdel, ": Калибровка весов", "Калибровка весов", "", "Calibrate(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Получить текущий вес", "Получить вес", "", "GetWeight(" + text + ")");
					HttpService.GetText(stringBuilder2, "Вес :", "", "", "", "MessageWeight", "style='display: none;'");
					break;
				case TypeDevice.enType.ДисплеиПокупателя:
					if (flag3)
					{
						HttpService.GetLine(bodyRazdel);
					}
					flag3 = true;
					HttpService.GetButtonTest(bodyRazdel, ": Отправить сообщение на дисплей", "Отправить сообщение", "", "OutputOnCustomerDisplay(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Стереть экран дисплея", "Стереть экран", "", "ClearCustomerDisplay(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Получить опции дисплея", "Опции дисплея", "", "OptionsCustomerDisplay(" + text + ")");
					break;
				case TypeDevice.enType.ЭквайринговыйТерминал:
					if (flag3)
					{
						HttpService.GetLine(bodyRazdel);
					}
					flag3 = true;
					HttpService.GetButtonTest(bodyRazdel, ": Оплатить платежной картой", "Оплатить картой", "", "PayByPaymentCard(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Вернуть платеж по платежной карте ", "Вернуть платеж", "", "ReturnPaymentByPaymentCard(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Отменить платеж по платежной карте ", "Отменить платеж", "", "CancelPaymentByPaymentCard(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Аварийная отмена операции ", "Отмена операции", "", "EmergencyReversal(" + text + ")");
					HttpService.GetLine(bodyRazdel);
					HttpService.GetButtonTest(bodyRazdel, ": Сверка итогов (закрытие смены) по эквайрингу", "Сверка итогов", "", "Settlement(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Отчет по итогам краткий", "Краткий отчет", "", "TerminalReport(" + text + ", false)");
					HttpService.GetButtonTest(bodyRazdel, ": Отчет по итогам полный", "Полный отчет", "", "TerminalReport(" + text + ", true)");
					HttpService.GetButtonTest(bodyRazdel, ": Получить копию последнего слип-чека", "Копия слип-чека", "", "TransactionDetails(" + text + ")");
					HttpService.GetLine(bodyRazdel);
					HttpService.GetButtonTest(bodyRazdel, ": Блокировка суммы на счете карты", "Блокировка суммы", "", "AuthorisationByPaymentCard(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Списать блокированную сумму со счета карты", "Списать блок. сумму", "", "AuthConfirmationByPaymentCard(" + text + ")");
					HttpService.GetButtonTest(bodyRazdel, ": Разблокировать сумму на счете карты", "Разблокировать сумму", "", "CancelAuthorisationByPaymentCard(" + text + ")");
					HttpService.GetLine(bodyRazdel);
					HttpService.GetButtonTest(bodyRazdel, ": Есть печать квитанций на терминале? ", "Есть печать квитанций?", "", "PrintSlipOnTerminal(" + text + ")");
					HttpService.GetLine(bodyRazdel);
					HttpService.GetButtonTest(bodyRazdel, ": Пример асинхронного получения данных при интерактивных действиях пользователя на сервере", "Оплатить картой", "", "PayByPaymentCardAsync(" + text + ")");
					HttpService.GetText(stringBuilder2, "Идентификатор транзакции :", "", "", "", "MessageUniversalID", "style='display: none;'");
					HttpService.GetText(stringBuilder2, "Номер Карты / Данные карты :", "", "", "", "MessageCardNumber", "style='display: none;'");
					HttpService.GetText(stringBuilder2, "Есть ли печать квитанции :", "", "", "", "MessagePrintSlipOnTerminal", "style='display: none;'");
					break;
				}
			}
			if (unit.InfoUnit != "")
			{
				HttpService.GetText(stringBuilder2, "Описание :", unit.InfoUnit, "", "", "MessageInfoUnit");
			}
			if (unit.DemoModeIsActivated != "")
			{
				HttpService.GetText(stringBuilder2, "Деморежим :", unit.DemoModeIsActivated, "", "", "MessageInfoUnit");
			}
			if (unit.LastError != "")
			{
				HttpService.GetText(stringBuilder2, "Ошибка :", unit.LastError, "", "", "help", "style='display: none;'");
			}
			foreach (TypeDevice.enType type2 in unit.SettDr.TypeDevice.Types)
			{
				switch (type2)
				{
				case TypeDevice.enType.ФискальныйРегистратор:
					HttpService.GetPre(stringBuilder2, "Квитанция/Чек/Slip :", "", "MessageSlip", Visibility: false);
					break;
				case TypeDevice.enType.СканерШтрихкода:
					HttpService.GetPre(stringBuilder2, "Считанные штрих-коды :", "", "MessageBarCode");
					break;
				case TypeDevice.enType.ЭквайринговыйТерминал:
					HttpService.GetPre(stringBuilder2, "Квитанция/Чек/Slip :", "", "MessageSlip");
					break;
				}
			}
		}
		HttpService.GetLine(bodyRazdel);
		if (unit != null && unit.SettDr.TypeDevice.UnitDevice == TypeDevice.enUnitDevice.Dll)
		{
			HttpService.GetButtonTest(bodyRazdel, ": Встроенный тест устройства и получение описания", "Тест устройства", "", "DeviceTest(" + text + ")");
		}
		HttpService.GetButtonTest(bodyRazdel, ": Получить список устройств", "Список устройств", "", "List()");
		HttpService.GetButtonTest(bodyRazdel, ": Получить данные сервера", "Данные сервера", "", "GetServerData()");
		HttpService.GetButtonTest(bodyRazdel, ": Получить результат последней команды", "Получить результат", "", "GetRezult()");
		HttpService.GetLine(bodyRazdel);
		if (flag)
		{
			HttpService.GetInputBox(bodyRazdel, "Устройство On/Off:", "OnOff", unit.Active ? "True" : "False", "checkbox", "onclick='OnOffUnut(" + text + ", (document.getElementById(\"OnOff\").checked))'");
		}
		HttpService.GetText(stringBuilder2, "Ошибка :", "", "Red", "", "MessageError", "style='display: none;'", "style='white-space:pre;'");
		HttpService.GetText(stringBuilder2, "Предупреждение :", "", "", "", "MessageWarning", "style='display: none;'", "style='white-space:pre;'");
		HttpService.GetText(stringBuilder2, "JSON ответа :", "", "", "", "MessageReturn", "style='display: none;'", "style='white-space:pre;'");
		stringBuilder2.Replace("&ТелоСтраницы", "");
		bodyRazdel.Replace("&ТелоСтраницы", stringBuilder2.ToString());
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}

	private async Task<HttpResponse> KkmRegOfd(HttpRequest Request, bool IsFirst = true, Unit.RezultCommand RezultCommand = null, string CashierName = "", string CashierVATIN = "")
	{
		string NumberUnit = Request.httpArg[1];
		Unit.DataCommand.TypeRegKkmOfd RegKkmOfd = new Unit.DataCommand.TypeRegKkmOfd();
		if (NumberUnit == "")
		{
			StringBuilder stringBuilder = HttpService.RootHtml();
			HttpService.CommandMenu(stringBuilder, "KkmRegOfd", Request.httpArg[1]);
			HttpService.UnitsMenu(stringBuilder, "KkmRegOfd", Request.httpArg[1], AddUnit: false, UnitPassword);
			StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Регистрация ККМ - выберите устройство");
			HttpService.GetChangeForDevice(bodyRazdel, "Выберите устройство:");
			stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
			stringBuilder.Replace("&ТелоСтраницы", "");
			stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
			return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
		}
		Unit Unit;
		try
		{
			Unit = Global.UnitManager.Units[int.Parse(NumberUnit)];
			await InitDataRegKkm(Unit, RegKkmOfd);
		}
		catch
		{
			return new HttpResponse(Request, HttpStatusCode.NotFound, "Не выбрано устройство");
		}
		if (Request.ArgForm.ContainsKey("Command"))
		{
			RegKkmOfd.Command = Request.ArgForm["Command"];
		}
		if (Request.ArgForm.ContainsKey("SetFfdVersion"))
		{
			RegKkmOfd.SetFfdVersion = int.Parse(Request.ArgForm["SetFfdVersion"]);
		}
		if (!IsFirst)
		{
			foreach (KeyValuePair<string, string> item in Request.ArgForm)
			{
				FieldInfo field = typeof(Unit.DataCommand.TypeRegKkmOfd).GetField(item.Key);
				if (!(field == null))
				{
					if (field.FieldType == typeof(bool))
					{
						field?.SetValue(RegKkmOfd, true);
					}
					else if (field.FieldType == typeof(int))
					{
						field?.SetValue(RegKkmOfd, int.Parse(item.Value));
					}
					else
					{
						field?.SetValue(RegKkmOfd, item.Value);
					}
				}
			}
			if (Request.ArgForm.ContainsKey("SnoOsn") || Request.ArgForm.ContainsKey("SnoDoh") || Request.ArgForm.ContainsKey("SnoDohRas") || Request.ArgForm.ContainsKey("SnoEnvd") || Request.ArgForm.ContainsKey("SnoEsn") || Request.ArgForm.ContainsKey("SnoPat"))
			{
				RegKkmOfd.TaxVariant = "";
				RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoOsn") ? "0," : "");
				RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoDoh") ? "1," : "");
				RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoDohRas") ? "2," : "");
				RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoEnvd") ? "3," : "");
				RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoEsn") ? "4," : "");
				RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoPat") ? "5," : "");
				if (RegKkmOfd.TaxVariant != "")
				{
					RegKkmOfd.TaxVariant = RegKkmOfd.TaxVariant.Substring(0, RegKkmOfd.TaxVariant.Length - 1);
				}
			}
			if (Unit.Kkm.FfdVersion <= 3 && (Request.ArgForm.ContainsKey("SignOfAgent0") || Request.ArgForm.ContainsKey("SignOfAgent1") || Request.ArgForm.ContainsKey("SignOfAgent2") || Request.ArgForm.ContainsKey("SignOfAgent3") || Request.ArgForm.ContainsKey("SignOfAgent4") || Request.ArgForm.ContainsKey("SignOfAgent5") || Request.ArgForm.ContainsKey("SignOfAgent6")))
			{
				RegKkmOfd.SignOfAgent = "";
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent0") ? "0," : "");
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent1") ? "1," : "");
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent2") ? "2," : "");
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent3") ? "3," : "");
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent4") ? "4," : "");
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent5") ? "5," : "");
				RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent6") ? "6," : "");
				if (RegKkmOfd.SignOfAgent != "")
				{
					RegKkmOfd.SignOfAgent = RegKkmOfd.SignOfAgent.Substring(0, RegKkmOfd.SignOfAgent.Length - 1);
				}
			}
		}
		StringBuilder stringBuilder2 = HttpService.RootHtml("", AddUnitTestJS: true);
		stringBuilder2.Replace("&ДополнительныеСкрипты", "<script type='text/javascript'>\r\n                    function SubmitCommand(NumberUnit) {\r\n                        document.getElementsByName(\"KkmRegOfd\")[0].attributes.action = '/ChangeKkmRegOfd/&NumberUnit';\r\n                        document.KkmRegOfd.submit();\r\n                    }\r\n                    function FfdVersion() {\r\n                        document.getElementsByName(\"KkmRegOfd\")[0].attributes.action = '/ChangeKkmRegOfd/&NumberUnit';\r\n                        document.KkmRegOfd.submit();\r\n                    }\r\n                </script>\r\n                &ДополнительныеСкрипты");
		stringBuilder2.Replace("&NumberUnit", NumberUnit);
		HttpService.CommandMenu(stringBuilder2, "KkmRegOfd", Request.httpArg[1]);
		HttpService.UnitsMenu(stringBuilder2, "KkmRegOfd", Request.httpArg[1], AddUnit: false, UnitPassword);
		StringBuilder bodyRazdel2 = HttpService.GetBodyRazdel("Регистрация ККМ: " + Unit?.NameDevice + " (#" + NumberUnit + ")");
		if (Unit.Kkm.FN_Status == 0)
		{
			HttpService.GetHelpBox(bodyRazdel2, "Статус ФН", "Настройка ФН");
		}
		else if (Unit.Kkm.FN_Status == 1)
		{
			HttpService.GetHelpBox(bodyRazdel2, "Статус ФН", "Готов к фискализации");
		}
		else if (Unit.Kkm.FN_Status == 3)
		{
			HttpService.GetHelpBox(bodyRazdel2, "Статус ФН", "Фискальный режим ФН");
		}
		else if (Unit.Kkm.FN_Status == 7)
		{
			HttpService.GetHelpBox(bodyRazdel2, "Статус ФН", "Постфискальный режим (Передача данных в ОФД)");
		}
		else if (Unit.Kkm.FN_Status == 15)
		{
			HttpService.GetHelpBox(bodyRazdel2, "Статус ФН", "ФН закрыт. Только доступ к архиву ФН");
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("", "<Не выбрано>");
		dictionary.Add("Open", "Первичная фискализация ККТ");
		dictionary.Add("ChangeFN", "Замена ФН");
		dictionary.Add("ChangeOFD", "Cмена ОФД");
		dictionary.Add("ChangeOrganization", "Cмена реквизитов организации");
		dictionary.Add("ChangeKkm", "Смена настроек ККТ");
		dictionary.Add("Close", "Закрытие архива ФН");
		HttpService.GetForm(bodyRazdel2, "KkmRegOfd", "/SetKkmRegOfd/&NumberUnit");
		HttpService.GetSelectBox(bodyRazdel2, "Команда регистрации:", "Command", RegKkmOfd.Command, dictionary, "onchange='SubmitCommand(&NumberUnit)'");
		bodyRazdel2.Replace("&NumberUnit", NumberUnit);
		switch (RegKkmOfd.Command)
		{
		case "":
			HttpService.GetHelpBox(bodyRazdel2, "", "Выберите команду регистрации/ перерегистрации ККМ.");
			break;
		case "Open":
			HttpService.GetHelpBox(bodyRazdel2, "", "Начальная регистрация ККМ производится после покупки ККМ и получения РНМ (рег.номер) в налоговой.</br>\r\n                        Внимание: ИНН, РНМ и некоторые другие реквизиты(зависит от ККМ) сменить не удастся!. БУДЬТЕ ВНИМАТЕЛЬНЫ!</br>\r\n                        Внимание: Количество перерегистрация ограниченно (11 раз). БУДЬТЕ ВНИМАТЕЛЬНЫ и при заполнении других реквизитов!</br>");
			break;
		case "ChangeFN":
			HttpService.GetHelpBox(bodyRazdel2, "", "Перерегистрация при установке нового ФН (фискального накопителя).</br>\r\n                        Внимание: Количество перерегистраций ограниченно (11 раз).</br>\r\n                        БУДЬТЕ ВНИМАТЕЛЬНЫ при заполнении реквизитов!</br>");
			break;
		case "ChangeOFD":
			HttpService.GetHelpBox(bodyRazdel2, "", "Перерегистрация данных об используемом ОФД.</br>\r\n                        Внимание: Количество перерегистраций ограниченно (11 раз).</br>\r\n                        БУДЬТЕ ВНИМАТЕЛЬНЫ при заполнении реквизитов!</br>");
			break;
		case "ChangeOrganization":
			HttpService.GetHelpBox(bodyRazdel2, "", "Перерегистрация некоторых данных об Организации.</br>\r\n                        Внимание: Количество перерегистраций ограниченно (11 раз).</br>\r\n                        БУДЬТЕ ВНИМАТЕЛЬНЫ при заполнении реквизитов!</br>");
			break;
		case "ChangeKkm":
			HttpService.GetHelpBox(bodyRazdel2, "", "Перерегистрация некоторых данных о ККМ.</br>\r\n                        Внимание: Количество перерегистраций ограниченно (11 раз).</br>\r\n                        БУДЬТЕ ВНИМАТЕЛЬНЫ при заполнении реквизитов!</br>");
			break;
		case "Close":
			HttpService.GetHelpBox(bodyRazdel2, "", "Закрытие архива ФН.</br>\r\n                        Внимание: После закрытия ФН будет работать только в режиме чтения.</br>\r\n                        Регистрация чеков будет невозможна.</br>\r\n                        Следует проводить или когда закончился срок действия ФН или память ФН переполнена.</br>");
			break;
		}
		if (RegKkmOfd.Command != "")
		{
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			if (RegKkmOfd.SetFfdVersion > Unit.Kkm.FfdSupportVersion || RegKkmOfd.SetFfdVersion == 0)
			{
				RegKkmOfd.SetFfdVersion = Unit.Kkm.FfdSupportVersion;
			}
			if (RegKkmOfd.SetFfdVersion < Unit.Kkm.FfdMinimumVersion || RegKkmOfd.SetFfdVersion == 0)
			{
				RegKkmOfd.SetFfdVersion = Unit.Kkm.FfdMinimumVersion;
			}
			if (Unit.Kkm.FfdMinimumVersion <= 1)
			{
				dictionary2.Add("1", "ФФД ver. 1.0");
			}
			if (Unit.Kkm.FfdSupportVersion >= 2 && Unit.Kkm.FfdMinimumVersion <= 2)
			{
				dictionary2.Add("2", "ФФД ver. 1.05");
			}
			if (Unit.Kkm.FfdSupportVersion >= 3 && Unit.Kkm.FfdMinimumVersion <= 3)
			{
				dictionary2.Add("3", "ФФД ver. 1.1");
			}
			if (Unit.Kkm.FfdSupportVersion >= 4 && Unit.Kkm.FfdMinimumVersion <= 4)
			{
				dictionary2.Add("4", "ФФД ver. 1.2");
			}
			HttpService.GetSelectBox(bodyRazdel2, "Версия ФФД:", "SetFfdVersion", RegKkmOfd.SetFfdVersion.ToString(), dictionary2, "onchange='FfdVersion()'", RegKkmOfd.Command != "Open" && RegKkmOfd.Command != "ChangeKkm" && RegKkmOfd.Command != "ChangeFN", "Версия ФФД по которой будет работать ККТ");
		}
		HttpService.GetLine(bodyRazdel2);
		if (RegKkmOfd.Command == "Open" || RegKkmOfd.Command == "ChangeFN")
		{
			HttpService.GetInputBox(bodyRazdel2, "Заводской номер ККТ:", "KktNumber", RegKkmOfd.KktNumber, "", "", Disabled: true);
			HttpService.GetInputBox(bodyRazdel2, "Заводской номер ФН:", "FnNumber", RegKkmOfd.FnNumber, "", "", Disabled: true);
			HttpService.GetInputBox(bodyRazdel2, "Регистрационный номер ККМ:", "RegNumber", RegKkmOfd.RegNumber);
			HttpService.GetInputBox(bodyRazdel2, "ИНН организации:", "InnOrganization", RegKkmOfd.InnOrganization, "", "", RegKkmOfd.Command != "Open");
			HttpService.GetLine(bodyRazdel2);
		}
		if (RegKkmOfd.Command == "Open" || RegKkmOfd.Command == "ChangeOrganization")
		{
			HttpService.GetInputBox(bodyRazdel2, "Наименование организации:", "NameOrganization", RegKkmOfd.NameOrganization);
			HttpService.GetInputBox(bodyRazdel2, "Адрес установки ККМ:", "AddressSettle", RegKkmOfd.AddressSettle);
			HttpService.GetInputBox(bodyRazdel2, "Место установки ККМ:", "PlaceSettle", RegKkmOfd.PlaceSettle, "", "", Disabled: false, "(только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Email магазина:", "SenderEmail", RegKkmOfd.SenderEmail, "", "", Disabled: false, "(только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			byte b = 0;
			string[] array = RegKkmOfd.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s in array)
			{
				b = (byte)(b + (1 << int.Parse(s)));
			}
			HttpService.GetInputBox(bodyRazdel2, "Общая (ОСН):", "SnoOsn", ((b & 1) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Общая система налогообложения");
			HttpService.GetInputBox(bodyRazdel2, "УСН (Доход):", "SnoDoh", ((b & 2) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Упрощенная система налогоообложения");
			HttpService.GetInputBox(bodyRazdel2, "УСН (Доход-Расход):", "SnoDohRas", ((b & 4) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Упрощенная (Доход-Расход) система налогоообложения");
			HttpService.GetInputBox(bodyRazdel2, "ЕНВД:", "SnoEnvd", ((b & 8) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Единый налог на вменённый доход");
			HttpService.GetInputBox(bodyRazdel2, "ЕСН:", "SnoEsn", ((b & 0x10) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Единый сельскохозяйственный налог");
			HttpService.GetInputBox(bodyRazdel2, "Патент:", "SnoPat", ((b & 0x20) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Патентная система налогообложения");
			HttpService.GetLine(bodyRazdel2);
		}
		if (RegKkmOfd.Command == "Open" || RegKkmOfd.Command == "ChangeOFD")
		{
			HttpService.GetInputBox(bodyRazdel2, "URL или IP сервера ОФД:", "UrlServerOfd", RegKkmOfd.UrlServerOfd);
			HttpService.GetInputBox(bodyRazdel2, "IP-порт сервера ОФД:", "PortServerOfd", RegKkmOfd.PortServerOfd);
			HttpService.GetInputBox(bodyRazdel2, "Наименование ОФД:", "NameOFD", RegKkmOfd.NameOFD);
			HttpService.GetInputBox(bodyRazdel2, "URL ОФД для поиска чека:", "UrlOfd", RegKkmOfd.UrlOfd);
			HttpService.GetInputBox(bodyRazdel2, "ИНН ОФД:", "InnOfd", RegKkmOfd.InnOfd);
			HttpService.GetLine(bodyRazdel2);
		}
		if (RegKkmOfd.Command == "Open" || RegKkmOfd.Command == "ChangeKkm")
		{
			HttpService.GetHelpBox(bodyRazdel2, "", "Признаки нужно определить и согласовать совместно с оператором фискальных данных!");
			HttpService.GetInputBox(bodyRazdel2, "Шифрование:", "EncryptionMode", RegKkmOfd.EncryptionMode ? "true" : "false", "checkbox", "", Disabled: false, "Данные передаваемые в ОФД будут шифроваться");
			HttpService.GetInputBox(bodyRazdel2, "Автономный режим:", "OfflineMode", RegKkmOfd.OfflineMode ? "true" : "false", "checkbox", "", Disabled: false, "Данные не будут передаватся в ОФД");
			HttpService.GetInputBox(bodyRazdel2, "Расчеты только в Интернете:", "InternetMode", RegKkmOfd.InternetMode ? "true" : "false", "checkbox", "", Disabled: false, "Режим разрешающий не печатать кассовый чек и БСО");
			HttpService.GetInputBox(bodyRazdel2, "Только БСО:", "BSOMode", RegKkmOfd.BSOMode ? "true" : "false", "checkbox", "", Disabled: false, "Для регистрации ТОЛЬКО бланков строгой отчетности");
			HttpService.GetInputBox(bodyRazdel2, "Автоматический режим:", "AutomaticMode", RegKkmOfd.AutomaticMode ? "true" : "false", "checkbox", "", Disabled: false, "ККТ работает без Кассира");
			HttpService.GetInputBox(bodyRazdel2, "Заводской номер автомата:", "AutomaticNumber", RegKkmOfd.AutomaticNumber, "", "", Disabled: false, "Заводской номер автомата при применении ККТ в составе автоматического устройства");
			HttpService.GetInputBox(bodyRazdel2, "Принтер в автомате:", "PrinterAutomatic", RegKkmOfd.PrinterAutomatic ? "true" : "false", "checkbox", "", Disabled: false, "Признак установки принтера в автомате (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetLine(bodyRazdel2);
			HttpService.GetInputBox(bodyRazdel2, "Расчеты за услуги:", "ServiceMode", RegKkmOfd.ServiceMode ? "true" : "false", "checkbox", "", Disabled: false, "Признак разрешает расчеты за услуги");
			HttpService.GetInputBox(bodyRazdel2, "Азартные игры:", "SignOfGambling", RegKkmOfd.SignOfGambling ? "true" : "false", "checkbox", "", Disabled: false, "Признак разрешает расчеты проведения азартных игр (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Проведения лотереи:", "SignOfLottery", RegKkmOfd.SignOfLottery ? "true" : "false", "checkbox", "", Disabled: false, "Признак разрешает расчеты проведения лотереи (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Подакцизный товар:", "SaleExcisableGoods", RegKkmOfd.SaleExcisableGoods ? "true" : "false", "checkbox", "", Disabled: false, "Продажа разрешает расчеты подакцизного товара (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Торговля маркированными товарами:", "SaleMarking", RegKkmOfd.SaleMarking ? "true" : "false", "checkbox", "", Disabled: false, "Признак разрешает торговлю маркированными товарами", "", "", Unit.Kkm.FfdSupportVersion >= 4);
			HttpService.GetInputBox(bodyRazdel2, "Ломбардная деятельность:", "SignPawnshop", RegKkmOfd.SignPawnshop ? "true" : "false", "checkbox", "", Disabled: false, "Признак разрешает ломбардную деятельность", "", "", Unit.Kkm.FfdSupportVersion >= 4);
			HttpService.GetInputBox(bodyRazdel2, "Страховая деятельность:", "SignAssurance", RegKkmOfd.SignAssurance ? "true" : "false", "checkbox", "", Disabled: false, "Признак разрешает страховую деятельность", "", "", Unit.Kkm.FfdSupportVersion >= 4);
			HttpService.GetLine(bodyRazdel2);
		}
		if (RegKkmOfd.Command == "Open" || RegKkmOfd.Command == "ChangeOrganization")
		{
			byte b2 = 0;
			string[] array = RegKkmOfd.SignOfAgent.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s2 in array)
			{
				b2 = (byte)(b2 + (1 << int.Parse(s2)));
			}
			HttpService.GetHelpBox(bodyRazdel2, "", "Применяемые коды агента:");
			HttpService.GetInputBox(bodyRazdel2, "Банковский платежный агент:", "SignOfAgent0", ((b2 & 1) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся банковским платежным агентом (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Банковский платежный субагент:", "SignOfAgent1", ((b2 & 2) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся банковским платежным субагентом (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Платежный агент:", "SignOfAgent2", ((b2 & 4) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся платежным агентом (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Платежный субагент:", "SignOfAgent3", ((b2 & 8) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся платежным субагентом (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Поверенный:", "SignOfAgent4", ((b2 & 0x10) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся поверенным (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Комиссионер:", "SignOfAgent5", ((b2 & 0x20) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся комиссионером (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetInputBox(bodyRazdel2, "Агент:", "SignOfAgent6", ((b2 & 0x40) > 0) ? "true" : "false", "checkbox", "", Disabled: false, "Оказание услуг пользователем, являющимся агентом и не являющимся банковским платежным агентом (субагентом), платежным агентом (субагентом), поверенным, комиссионером (только для ФФД >= 1.05)", "", "", Unit.Kkm.FfdSupportVersion >= 2);
			HttpService.GetLine(bodyRazdel2);
		}
		if (RegKkmOfd.Command != "")
		{
			HttpService.GetInputBox(bodyRazdel2, "Кассир:", "CashierName", CashierName, "", "", Disabled: false, "Сотрудник, который проводит регистрацию");
			HttpService.GetInputBox(bodyRazdel2, "ИНН Кассира:", "CashierVATIN", CashierVATIN, "", "", Disabled: false, "Сотрудника, который проводит регистрацию");
			HttpService.GetLine(bodyRazdel2);
			if (RegKkmOfd.Command == "Open")
			{
				HttpService.GetFormEnd(bodyRazdel2, "", "Зарегистрировать ККМ");
			}
			else if (RegKkmOfd.Command == "Close")
			{
				HttpService.GetFormEnd(bodyRazdel2, "", "Закрыть архив ФН");
			}
			else
			{
				HttpService.GetFormEnd(bodyRazdel2, "", "Изменить параметры");
			}
			HttpService.GetHelpBox(bodyRazdel2, "", "Внимание: После регистрации проверьте связь с ОФД!</br>\r\n                    Пока данные о перерегистрации не будут переданы в ОФД чеки не будут регистрироваться!!!</br>");
			HttpService.GetLine(bodyRazdel2);
		}
		if (RezultCommand != null)
		{
			HttpService.GetHeadetBox(bodyRazdel2, "", "Результат:");
			string text = "";
			switch ((int)RezultCommand.Status)
			{
			case 0:
				text = "Ok";
				break;
			case 1:
				text = "Run";
				break;
			case 2:
				text = "Error";
				break;
			case 3:
				text = "Not Found";
				break;
			case 4:
				text = "Not Run";
				break;
			}
			HttpService.GetText(bodyRazdel2, "Статус :", text);
			if (RezultCommand.Error != null && RezultCommand.Error != "")
			{
				HttpService.GetText(bodyRazdel2, "Ошибка :", RezultCommand.Error, "Red", "", "MessageError");
			}
			if (((Unit.RezultCommandKKm)RezultCommand).QRCode != null && ((Unit.RezultCommandKKm)RezultCommand).QRCode != "")
			{
				HttpService.GetText(bodyRazdel2, "Данные текущей регистрации :", ((Unit.RezultCommandKKm)RezultCommand).QRCode, "", "", "");
			}
		}
		if (Unit.Kkm.InfoRegKkt != null && Unit.Kkm.InfoRegKkt != "")
		{
			HttpService.GetText(bodyRazdel2, "Данные первой регистрации :", Unit.Kkm.InfoRegKkt, "", "", "");
		}
		bodyRazdel2.Replace("&ТелоСтраницы", "");
		stringBuilder2.Replace("&ТелоСтраницы", bodyRazdel2.ToString());
		stringBuilder2.Replace("&ТелоСтраницы", "");
		stringBuilder2.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder2.ToString());
	}

	private async Task<HttpResponse> SetKkmRegOfd(HttpRequest Request)
	{
		Unit.DataCommand.TypeRegKkmOfd RegKkmOfd = new Unit.DataCommand.TypeRegKkmOfd();
		string NumberUnit = Request.httpArg[1];
		try
		{
			Unit unit = Global.UnitManager.Units[int.Parse(NumberUnit)];
			await InitDataRegKkm(unit, RegKkmOfd);
		}
		catch
		{
			return new HttpResponse(Request, HttpStatusCode.NotFound, "Не выбрано устройство");
		}
		if (Request.ArgForm.ContainsKey("Command") && (Request.ArgForm["Command"] == "Open" || Request.ArgForm["Command"] == "ChangeKkm"))
		{
			RegKkmOfd.EncryptionMode = false;
			RegKkmOfd.OfflineMode = false;
			RegKkmOfd.AutomaticMode = false;
			RegKkmOfd.ServiceMode = false;
			RegKkmOfd.BSOMode = false;
			RegKkmOfd.InternetMode = false;
			RegKkmOfd.SaleExcisableGoods = false;
			RegKkmOfd.SignOfGambling = false;
			RegKkmOfd.SignOfLottery = false;
			RegKkmOfd.PrinterAutomatic = false;
			RegKkmOfd.SaleMarking = false;
			RegKkmOfd.SignPawnshop = false;
			RegKkmOfd.SignAssurance = false;
		}
		foreach (KeyValuePair<string, string> item in Request.ArgForm)
		{
			FieldInfo field = typeof(Unit.DataCommand.TypeRegKkmOfd).GetField(item.Key);
			if (!(field == null))
			{
				if (field.FieldType == typeof(bool))
				{
					field?.SetValue(RegKkmOfd, true);
				}
				else if (field.FieldType == typeof(int))
				{
					field?.SetValue(RegKkmOfd, int.Parse(item.Value));
				}
				else
				{
					field?.SetValue(RegKkmOfd, item.Value);
				}
			}
		}
		RegKkmOfd.TaxVariant = "";
		if (Request.ArgForm.ContainsKey("Command") && (Request.ArgForm["Command"] == "Open" || Request.ArgForm["Command"] == "ChangeOrganization"))
		{
			RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoOsn") ? "0," : "");
			RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoDoh") ? "1," : "");
			RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoDohRas") ? "2," : "");
			RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoEnvd") ? "3," : "");
			RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoEsn") ? "4," : "");
			RegKkmOfd.TaxVariant += (Request.ArgForm.ContainsKey("SnoPat") ? "5," : "");
			if (RegKkmOfd.TaxVariant != "")
			{
				RegKkmOfd.TaxVariant = RegKkmOfd.TaxVariant.Substring(0, RegKkmOfd.TaxVariant.Length - 1);
			}
		}
		RegKkmOfd.SignOfAgent = "";
		if (Request.ArgForm.ContainsKey("SignOfAgent0") || Request.ArgForm.ContainsKey("SignOfAgent1") || Request.ArgForm.ContainsKey("SignOfAgent2") || Request.ArgForm.ContainsKey("SignOfAgent3") || Request.ArgForm.ContainsKey("SignOfAgent4") || Request.ArgForm.ContainsKey("SignOfAgent5") || Request.ArgForm.ContainsKey("SignOfAgent6"))
		{
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent0") ? "0," : "");
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent1") ? "1," : "");
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent2") ? "2," : "");
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent3") ? "3," : "");
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent4") ? "4," : "");
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent5") ? "5," : "");
			RegKkmOfd.SignOfAgent += (Request.ArgForm.ContainsKey("SignOfAgent6") ? "6," : "");
			if (RegKkmOfd.SignOfAgent != "")
			{
				RegKkmOfd.SignOfAgent = RegKkmOfd.SignOfAgent.Substring(0, RegKkmOfd.SignOfAgent.Length - 1);
			}
		}
		string CashierName = "";
		string CashierVATIN = "";
		if (Request.ArgForm.ContainsKey("CashierName"))
		{
			CashierName = Request.ArgForm["CashierName"];
		}
		if (Request.ArgForm.ContainsKey("CashierVATIN"))
		{
			CashierVATIN = Request.ArgForm["CashierVATIN"];
		}
		Unit.DataCommand dataCommand = new Unit.DataCommand();
		dataCommand.NumDevice = int.Parse(NumberUnit);
		dataCommand.Command = "KkmRegOfd";
		dataCommand.IdCommand = Guid.NewGuid().ToString();
		dataCommand.RegKkmOfd = RegKkmOfd;
		dataCommand.UnitPassword = UnitPassword;
		dataCommand.CashierName = CashierName;
		dataCommand.CashierVATIN = CashierVATIN;
		return await KkmRegOfd(Request, IsFirst: false, await Global.UnitManager.AddCommand(dataCommand, "", JsonConvert.SerializeObject(dataCommand)), CashierName, CashierVATIN);
	}

	private async Task InitDataRegKkm(Unit Unit, Unit.DataCommand.TypeRegKkmOfd RegKkmOfd)
	{
		await Unit.Semaphore.WaitAsync();
		try
		{
			await Unit.ReadStatusOFD(Full: true, ReadInfoGer: true);
		}
		finally
		{
			Unit.Semaphore.Release();
		}
		RegKkmOfd.SetFfdVersion = Unit.Kkm.FfdVersion;
		RegKkmOfd.NameOrganization = Unit.Kkm.Organization;
		RegKkmOfd.InnOrganization = Unit.Kkm.INN;
		RegKkmOfd.AddressSettle = Unit.Kkm.AddressSettle;
		RegKkmOfd.PlaceSettle = Unit.Kkm.PlaceSettle;
		RegKkmOfd.SenderEmail = Unit.Kkm.SenderEmail;
		RegKkmOfd.TaxVariant = Unit.Kkm.TaxVariant;
		RegKkmOfd.KktNumber = Unit.Kkm.NumberKkm;
		RegKkmOfd.FnNumber = Unit.Kkm.Fn_Number;
		RegKkmOfd.RegNumber = Unit.Kkm.RegNumber;
		RegKkmOfd.UrlServerOfd = Unit.Kkm.UrlServerOfd;
		RegKkmOfd.PortServerOfd = Unit.Kkm.PortServerOfd;
		RegKkmOfd.NameOFD = Unit.Kkm.NameOFD;
		RegKkmOfd.UrlOfd = Unit.Kkm.UrlOfd;
		RegKkmOfd.InnOfd = Unit.Kkm.InnOfd;
		RegKkmOfd.EncryptionMode = Unit.Kkm.EncryptionMode;
		RegKkmOfd.OfflineMode = Unit.Kkm.OfflineMode;
		RegKkmOfd.AutomaticMode = Unit.Kkm.AutomaticMode;
		RegKkmOfd.AutomaticNumber = Unit.Kkm.AutomaticNumber;
		RegKkmOfd.InternetMode = Unit.Kkm.InternetMode;
		RegKkmOfd.BSOMode = Unit.Kkm.BSOMode;
		RegKkmOfd.ServiceMode = Unit.Kkm.ServiceMode;
		RegKkmOfd.PrinterAutomatic = Unit.Kkm.PrinterAutomatic;
		RegKkmOfd.SignOfGambling = Unit.Kkm.SignOfGambling;
		RegKkmOfd.SignOfLottery = Unit.Kkm.SignOfLottery;
		RegKkmOfd.SaleExcisableGoods = Unit.Kkm.SaleExcisableGoods;
		RegKkmOfd.SignOfAgent = Unit.Kkm.SignOfAgent;
		RegKkmOfd.SaleMarking = Unit.Kkm.SaleMarking;
		RegKkmOfd.SignPawnshop = Unit.Kkm.SignPawnshop;
		RegKkmOfd.SignAssurance = Unit.Kkm.SignAssurance;
		RegKkmOfd.Command = "";
	}

	private async Task<HttpResponse> OperationsHistory(HttpRequest Request)
	{
		string text = HttpUtility.UrlDecode(Request.httpArg[1]);
		StringBuilder html = HttpService.RootHtml();
		html.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(html, "OperationsHistory", Request.httpArg[1]);
		html.Replace("&СписокУстройств", "");
		StringBuilder htmlBody = HttpService.GetBodyRazdel("История операций");
		htmlBody.Append("<br/>\r\n                <table class='table' style='width: 1000px;'>\r\n                    <thead>\r\n                        <tr>\r\n                            <th style='width: 170px;'>Дата</th>\r\n                            <th style='width: 10px;'>Устр. №</th>\r\n                            <th style='width: 50px;'>INN</th>\r\n                            <th style='width: 180px;'>Операция</th>\r\n                            <th style='width: 100px;'>Чеки</th>\r\n                            <th style='width: 100px;'>Оплаты</th>\r\n                            <th style='width: 560px;'>Комментарий</th>\r\n                            <th style='width: 20px;'>Действие</th>\r\n                        </tr>\r\n                    </thead>\r\n                    <tbody>\r\n                        &Строка\r\n                    </tbody>\r\n                </table>\r\n                <style>\r\n                    td.FieldHistory {\r\n                        border-bottom: 1px solid #d9d9d9;\r\n                    }\r\n                </style>");
		string htmlStr = "<tr style='cursor:pointer'>\r\n                    <td class='FieldHistory' style='text-align: center; '>&Date</td>\r\n                    <td class='FieldHistory' style='text-align: right; '>&NumUnit</td>\r\n                    <td class='FieldHistory' style='text-align: center; font-size: 70%;'>&INN</td>\r\n                    <td class='FieldHistory' style='font-size: 70%; line-height: initial;'>&NameOperation</td>\r\n                    <td class='FieldHistory' style='text-align: right;'>&SummKKT</td>\r\n                    <td class='FieldHistory' style='text-align: right;'>&SummPay</td>\r\n                    <td class='FieldHistory' style='font-size: 70%; overflow-wrap: anywhere;'>&Comment</td>\r\n                    <td class='FieldHistory' style='font-size: 70%;'>&Button</td>\r\n                </tr>\r\n                &Строка";
		string TextButton = "<div>\r\n                <input class=\"Button\" value=\"Возврат\" type=\"submit\" style=\"width: 80px;\" onclick=\"window.location.assign('/&URL/&NumUnit?UID=&UID')\">\r\n            </div>";
		HttpService.GetButton(htmlBody, "Следующая страница", "Дальше", "", "", "location.href = '/OperationsHistory/&NewNumPag'");
		DateTime Start = DateTime.Now;
		if (text != "")
		{
			try
			{
				Start = DateTime.ParseExact(HttpUtility.UrlDecode(text), "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
			}
			catch
			{
			}
		}
		int CoOnPage = 300;
		Global.Logers.SaveSettings();
		StringBuilder htmlStrSb = new StringBuilder();
		new StringBuilder();
		DateTime NumPagSave = default(DateTime);
		Global.Logers.OperationsHistory.ClouseNext();
		while (true)
		{
			FileLog<Logers.Operation>.Field field = await Global.Logers.OperationsHistory.GetNext();
			if (field == null)
			{
				break;
			}
			DateTime dateLog = field.DateLog;
			Logers.Operation value = field.Value;
			if (dateLog > Start)
			{
				continue;
			}
			int num = CoOnPage - 1;
			CoOnPage = num;
			if (num == 0)
			{
				break;
			}
			NumPagSave = dateLog;
			StringBuilder stringBuilder = new StringBuilder();
			if (value.UID != null)
			{
				stringBuilder.Append(TextButton);
				if (value.DeviceType == TypeDevice.enType.ФискальныйРегистратор || value.DeviceType == TypeDevice.enType.ПринтерЧеков)
				{
					stringBuilder.Replace("&URL", "PrintCheck");
				}
				else if (value.DeviceType == TypeDevice.enType.ЭквайринговыйТерминал)
				{
					stringBuilder.Replace("&URL", "PayByCard");
				}
				stringBuilder.Replace("&UID", value.UID);
				stringBuilder.Replace("&NumUnit", value.NumUnit.ToString());
			}
			htmlStrSb.Clear();
			htmlStrSb.Append(htmlStr);
			htmlStrSb.Replace("&Url", "OperationsHistory");
			htmlStrSb.Replace("&Date", dateLog.ToString());
			htmlStrSb.Replace("&NumUnit", value.NumUnit.ToString());
			htmlStrSb.Replace("&INN", value.INN);
			htmlStrSb.Replace("&NameOperation", value.NameOperation);
			if (value.NameOperation.IndexOf("Чек корректировки возврата продажи/прихода") != -1)
			{
				value.Summ = -Math.Abs(value.Summ);
			}
			else if (value.NameOperation.IndexOf("Чек корректировки продажи/прихода") != -1)
			{
				value.Summ = Math.Abs(value.Summ);
			}
			else if (value.NameOperation.IndexOf("Чек возврата продажи/прихода") != -1)
			{
				value.Summ = -Math.Abs(value.Summ);
			}
			else if (value.NameOperation.IndexOf("Чек продажи/прихода") != -1)
			{
				value.Summ = Math.Abs(value.Summ);
			}
			if (value.Summ == 0m)
			{
				htmlStrSb.Replace("&SummKKT", "");
				htmlStrSb.Replace("&SummPay", "");
			}
			else if (value.DeviceType == TypeDevice.enType.ФискальныйРегистратор || value.DeviceType == TypeDevice.enType.ПринтерЧеков)
			{
				htmlStrSb.Replace("&SummKKT", value.Summ.ToString("0.00"));
				htmlStrSb.Replace("&SummPay", "");
			}
			else if (value.DeviceType == TypeDevice.enType.ЭквайринговыйТерминал)
			{
				htmlStrSb.Replace("&SummKKT", "");
				htmlStrSb.Replace("&SummPay", value.Summ.ToString("0.00"));
			}
			htmlStrSb.Replace("&Comment", value.Comment);
			htmlBody.Replace("&Строка", htmlStrSb.ToString());
			htmlBody.Replace("&Button", stringBuilder.ToString());
		}
		Global.Logers.OperationsHistory.ClouseNext();
		htmlBody.Replace("&Строка", "");
		htmlBody.Replace("&NewNumPag", HttpUtility.UrlEncode(NumPagSave.ToString("yyyy.MM.dd HH:mm:ss:fff")));
		htmlBody.Replace("&NumPag", HttpUtility.UrlEncode(Start.ToString("yyyy.MM.dd HH:mm:ss:fff")));
		html.Replace("&ТелоСтраницы", htmlBody.ToString());
		html.Replace("&ТелоСтраницы", "");
		html.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
	}

	private async Task<Unit.DataCommand> GetDataCommandByHistory(string UID)
	{
		Logers.Operation CurLog = null;
		Global.Logers.OperationsHistory.ClouseNext();
		while (true)
		{
			FileLog<Logers.Operation>.Field field = await Global.Logers.OperationsHistory.GetNext();
			if (field == null)
			{
				Global.Logers.OperationsHistory.ClouseNext();
				break;
			}
			Logers.Operation value = field.Value;
			if (value.UID == UID)
			{
				CurLog = value;
				break;
			}
		}
		Global.Logers.OperationsHistory.ClouseNext();
		if (CurLog != null && CurLog.TextCommand != null)
		{
			Unit.DataCommand dataCommand = JsonToDataCommand(CurLog.TextCommand);
			if (dataCommand.Command == "RegisterCheck")
			{
				if (dataCommand.TypeCheck == 0)
				{
					dataCommand.TypeCheck = 1;
				}
				else if (dataCommand.TypeCheck == 1)
				{
					dataCommand.TypeCheck = 0;
				}
				else if (dataCommand.TypeCheck == 2)
				{
					dataCommand.TypeCheck = 3;
				}
				else if (dataCommand.TypeCheck == 3)
				{
					dataCommand.TypeCheck = 20;
				}
				else if (dataCommand.TypeCheck == 10)
				{
					dataCommand.TypeCheck = 11;
				}
				else if (dataCommand.TypeCheck == 11)
				{
					dataCommand.TypeCheck = 10;
				}
				else if (dataCommand.TypeCheck == 12)
				{
					dataCommand.TypeCheck = 13;
				}
				else if (dataCommand.TypeCheck == 13)
				{
					dataCommand.TypeCheck = 14;
				}
			}
			else if (dataCommand.Command == "PayByPaymentCard")
			{
				dataCommand.Command = "ReturnPaymentByPaymentCard";
				dataCommand.UniversalID = CurLog.Comment;
			}
			else if (dataCommand.Command == "AuthConfirmationByPaymentCard")
			{
				dataCommand.Command = "ReturnPaymentByPaymentCard";
				dataCommand.UniversalID = CurLog.Comment;
			}
			if (dataCommand.Command == "RegisterCheck")
			{
				Unit.DataCommand.CheckString[] array = new Unit.DataCommand.CheckString[0];
				Unit.DataCommand.CheckString[] checkStrings = dataCommand.CheckStrings;
				foreach (Unit.DataCommand.CheckString checkString in checkStrings)
				{
					if (checkString.Register == null)
					{
						continue;
					}
					if (checkString.Register.Quantity * checkString.Register.Price != checkString.Register.Amount)
					{
						foreach (Unit.DataCommand.Register item in Unit.SplitRegisterString(checkString, null))
						{
							item.Price = item.Amount / item.Quantity;
							Array.Resize(ref array, array.Length + 1);
							array[array.Length - 1] = new Unit.DataCommand.CheckString();
							Unit.DataCommand.Register register = new Unit.DataCommand.Register();
							Unit.CopyObject(checkString.Register, register);
							register.Quantity = item.Quantity;
							register.Price = Math.Round(item.Amount / item.Quantity, 2, MidpointRounding.ToEven);
							register.Amount = item.Amount;
							array[array.Length - 1].Register = register;
						}
					}
					else
					{
						Array.Resize(ref array, array.Length + 1);
						array[array.Length - 1] = new Unit.DataCommand.CheckString();
						array[array.Length - 1].Register = checkString.Register;
					}
				}
				dataCommand.CheckStrings = array;
			}
			return dataCommand;
		}
		return null;
	}

	private async Task<HttpResponse> Statistics(HttpRequest Request)
	{
		string text = "mm:ss";
		int val = 0;
		Logers.Logs.ItemPerfomance[] array = null;
		Queue<Logers.Logs.ItemPerfomance> queue = null;
		string text2 = Request.httpArg[1];
		if (text2 == null || text2.Length != 0)
		{
			if (!(text2 == "min"))
			{
				if (text2 == "min5")
				{
					text = "dd HH:mm";
					val = 240;
					queue = Global.Logers.Log.Min5StackPerfomance;
				}
			}
			else
			{
				text = "HH:mm";
				val = 240;
				queue = Global.Logers.Log.MinStackPerfomance;
			}
		}
		else
		{
			text = "HH:mm:ss";
			val = 240;
			queue = Global.Logers.Log.SecStackPerfomance;
		}
		lock (Global.Logers.Log)
		{
			array = new Logers.Logs.ItemPerfomance[queue.Count];
			queue.CopyTo(array, 0);
		}
		StringBuilder stringBuilder = HttpService.RootHtml();
		stringBuilder.Replace("&ДополнительныеСкрипты", "\r\n                <script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>\r\n                <script type=\"text/javascript\">\r\n                    google.charts.load('current', { packages: ['corechart', 'line'] });\r\n                    google.charts.setOnLoadCallback(drawCharts);\r\n                    function drawCharts() {\r\n                        DrawKkmCommand();\r\n                        DrawKkmTime();\r\n                        DrawKkmMem();\r\n                    }\r\n                    function ClickCharts(id, fnName) {\r\n                        Item = document.getElementById(id);\r\n                        if (Item.style.height == '180px'){\r\n                            Item.style.height = '800px';\r\n                        } else {\r\n                            Item.style.height = '180px';\r\n                        };\r\n                        eval(fnName+'()');\r\n                    }\r\n                </script>\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(stringBuilder, "Statistics", Request.httpArg[1]);
		stringBuilder.Replace("&СписокУстройств", "");
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Статистика");
		bodyRazdel.Replace("&ТелоСтраницы", "<div style='padding-left:20px;'>\r\n                <a id='help' style='padding-right:5px; border-left-width: 5px;' href='/Statistics'>Шаг-секунда</a>\r\n                <a id='help' style='padding-right:5px; border-left-width: 5px;' href='/Statistics/min'>Шаг-минута</a>\r\n                <a id='help' style='padding-right:5px; border-left-width: 5px;' href='/Statistics/min5'>Шаг-5 минут</a>\r\n                </div>\r\n                <br/>\r\n                &ТелоСтраницы");
		bodyRazdel.Replace("&ТелоСтраницы", "\r\n                <div id='Stat_KKM_Command' style='width: 1100px; height: 180px; margin-left: 10px' onclick='ClickCharts(\"Stat_KKM_Command\", \"DrawKkmCommand\")'></div>\r\n                &ТелоСтраницы");
		StringBuilder stringBuilder2 = new StringBuilder("<script type=\"text/javascript\">\r\n                    function DrawKkmCommand() {\r\n                        var data = google.visualization.arrayToDataTable([\r\n                            ['Дата', 'Ожидали (макс)', 'Выполнялись (макс)', 'Выполнено с ошибками', 'Выполнено Успешно', { role: 'annotation' } ],\r\n                            &СтрокаДанных\r\n                            ]);\r\n                        var view = new google.visualization.DataView(data);\r\n                        var options = {\r\n                            fontSize: 10,\r\n                            legend: { position: 'right', maxLines: 1, textStyle: { fontSize: 10} },\r\n                            bar: { groupWidth: '75%' },\r\n                            isStacked: true,\r\n                            vAxis: {title: 'Выполнение команд', \u00a0titleTextStyle: {fontSize: 14}},\r\n                            series: {\r\n                                0:{color: '#FFA3A3', },\r\n                                1:{color: '#6A6AFF', },\r\n                                2:{color: 'red', },\r\n                                3:{color: '#69CD71', },\r\n                            }\r\n                        };\r\n                        var chart = new google.visualization.ColumnChart(document.getElementById(\"Stat_KKM_Command\"));\r\n                        chart.draw(view, options);\r\n                    }\r\n                </script>\r\n                &ДополнительныеСкрипты");
		val = Math.Min(val, array.Length);
		int num = array.Length - val;
		StringBuilder stringBuilder3 = new StringBuilder();
		for (int i = num; i < val + num; i++)
		{
			Logers.Logs.ItemPerfomance itemPerfomance = array[i];
			stringBuilder3.Clear();
			stringBuilder3.Append("['&Дате', &CommandMaxQueue, &CommandMaxRun, &CommandError, &CommandCount, ''],\r\n                                                &СтрокаДанных");
			stringBuilder3.Replace("&Дате", itemPerfomance.Date.ToString(text));
			stringBuilder3.Replace("&CommandMaxQueue", itemPerfomance.CommandMaxQueue.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&CommandMaxRun", itemPerfomance.CommandMaxRun.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&CommandError", itemPerfomance.CommandError.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&CommandCount", itemPerfomance.CommandCount.ToString().Replace(',', '.'));
			stringBuilder2.Replace("&СтрокаДанных", stringBuilder3.ToString());
		}
		if (val == 0)
		{
			stringBuilder3.Clear();
			stringBuilder3.Append("['&Дате', &CommandMaxQueue, &CommandMaxRun, &CommandError, &CommandCount, ''],\r\n                                                &СтрокаДанных");
			stringBuilder3.Replace("&Дате", DateTime.Now.ToString(text));
			stringBuilder3.Replace("&CommandMaxQueue", 0.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&CommandMaxRun", 0.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&CommandError", 0.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&CommandCount", 0.ToString().Replace(',', '.'));
			stringBuilder2.Replace("&СтрокаДанных", stringBuilder3.ToString());
		}
		stringBuilder2.Replace("&СтрокаДанных", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", stringBuilder2.ToString());
		bodyRazdel.Replace("&ТелоСтраницы", "\r\n                <div id = 'Stat_KKM_Time' style = 'width: 1100px; height: 180px; margin-left: 10px' onclick='ClickCharts(\"Stat_KKM_Time\", \"DrawKkmTime\")'></div>\r\n                &ТелоСтраницы");
		stringBuilder2 = new StringBuilder("<script type=\"text/javascript\">\r\n                    function DrawKkmTime() {\r\n                        var data = google.visualization.arrayToDataTable([\r\n                            ['Дата', 'Ожидение', 'Выполнение', 'Мак.Ожидание', 'Мак.Выполнение', { role: 'annotation' } ],\r\n                            &СтрокаДанных\r\n                            ]);\r\n                        var view = new google.visualization.DataView(data);\r\n                        var options = {\r\n                            fontSize: 10,\r\n                            legend: { position: 'right', maxLines: 1, textStyle: { fontSize: 10} },\r\n                            bar: { groupWidth: '75%' },\r\n                            isStacked: true,\r\n                            vAxis: {title: 'Время выполнения команд (мс)', \u00a0titleTextStyle: {fontSize: 14}},\r\n                            series: {\r\n                                0:{color: '#A8E1AC', },\r\n                                1:{color: '#69CD71', },\r\n                                2:{color: '#EF9E9E', },\r\n                                3:{color: '#E86C6C', },\r\n                            }\r\n                        };\r\n                        var chart = new google.visualization.ColumnChart(document.getElementById(\"Stat_KKM_Time\"));\r\n                        chart.draw(view, options);\r\n                    }\r\n                </script>\r\n                &ДополнительныеСкрипты");
		val = Math.Min(val, array.Length);
		num = array.Length - val;
		stringBuilder3 = new StringBuilder();
		for (int j = num; j < val + num; j++)
		{
			Logers.Logs.ItemPerfomance itemPerfomance2 = array[j];
			stringBuilder3.Clear();
			stringBuilder3.Append("['&Дате', &QueueTime, &WorkTime, &MaxQueueTime, &MaxWorkTime, ''],\r\n                                                &СтрокаДанных");
			decimal num2 = ((itemPerfomance2.CommandQueue == 0) ? 0.000001m : ((decimal)itemPerfomance2.CommandQueue));
			decimal num3 = ((itemPerfomance2.CommandWork == 0) ? 0.000001m : ((decimal)itemPerfomance2.CommandWork));
			decimal num4 = (decimal)itemPerfomance2.CommandQueueTime / num2;
			decimal num5 = (decimal)itemPerfomance2.CommandWorkTime / num3;
			decimal num6 = (decimal)Math.Abs(itemPerfomance2.CommandMaxQueueTime - itemPerfomance2.CommandQueueTime) / num2;
			decimal num7 = (decimal)Math.Abs(itemPerfomance2.CommandMaxWorkTime - itemPerfomance2.CommandWorkTime) / num3;
			stringBuilder3.Replace("&Дате", itemPerfomance2.Date.ToString(text));
			stringBuilder3.Replace("&QueueTime", ((long)((num4 < 0m || num4 > 1000000m) ? 0m : num4)).ToString().Replace(',', '.'));
			stringBuilder3.Replace("&WorkTime", ((long)((num5 < 0m || num5 > 1000000m) ? 0m : num5)).ToString().Replace(',', '.'));
			stringBuilder3.Replace("&MaxQueueTime", ((long)((num6 < 0m || num6 > 1000000m) ? 0m : num6)).ToString().Replace(',', '.'));
			stringBuilder3.Replace("&MaxWorkTime", ((long)((num7 < 0m || num7 > 1000000m) ? 0m : num7)).ToString().Replace(',', '.'));
			stringBuilder2.Replace("&СтрокаДанных", stringBuilder3.ToString());
		}
		if (val == 0)
		{
			stringBuilder3.Clear();
			stringBuilder3.Append("['&Дате', &QueueTime, &WorkTime, &MaxQueueTime, &MaxWorkTime, ''],\r\n                                                &СтрокаДанных");
			stringBuilder3.Replace("&Дате", DateTime.Now.ToString(text));
			stringBuilder3.Replace("&QueueTime", 0L.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&WorkTime", 0L.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&MaxQueueTime", 0L.ToString().Replace(',', '.'));
			stringBuilder3.Replace("&MaxWorkTime", 0L.ToString().Replace(',', '.'));
			stringBuilder2.Replace("&СтрокаДанных", stringBuilder3.ToString());
		}
		stringBuilder2.Replace("&СтрокаДанных", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", stringBuilder2.ToString());
		bodyRazdel.Replace("&ТелоСтраницы", "\r\n                <div id = 'Stat_KKM_Мем' style = 'width: 1100px; height: 180px; margin-left: 10px' onclick='ClickCharts(\"Stat_KKM_Мем\", \"DrawKkmMem\")'></div>\r\n                &ТелоСтраницы");
		stringBuilder2 = new StringBuilder("<script type=\"text/javascript\">\r\n                    function DrawKkmMem() {\r\n                        var data = google.visualization.arrayToDataTable([\r\n                            ['Дата', 'Выделенная память', { role: 'annotation' } ],\r\n                            &СтрокаДанных\r\n                            //['05 18:30', 10, 24, 20, ''],\r\n                            ]);\r\n                        var view = new google.visualization.DataView(data);\r\n                        var options = {\r\n                            fontSize: 10,\r\n                            legend: { position: 'right', maxLines: 1, textStyle: { fontSize: 10} },\r\n                            bar: { groupWidth: '75%' },\r\n                            isStacked: true,\r\n                            vAxis: {title: 'Выделенная память (мБт)', \u00a0titleTextStyle: {fontSize: 14}},\r\n                            series: {\r\n                                0:{color: '#FFA3A3', },\r\n                            }\r\n                        };\r\n                        var chart = new google.visualization.ColumnChart(document.getElementById(\"Stat_KKM_Мем\"));\r\n                        chart.draw(view, options);\r\n                    }\r\n                </script>\r\n                &ДополнительныеСкрипты");
		val = Math.Min(val, array.Length);
		num = array.Length - val;
		stringBuilder3 = new StringBuilder();
		for (int k = num; k < val + num; k++)
		{
			Logers.Logs.ItemPerfomance itemPerfomance3 = array[k];
			stringBuilder3.Clear();
			stringBuilder3.Append("['&Дате', &МЕМ, ''],\r\n                                                &СтрокаДанных");
			stringBuilder3.Replace("&Дате", itemPerfomance3.Date.ToString(text));
			stringBuilder3.Replace("&МЕМ", ((int)(itemPerfomance3.Memory >> 16)).ToString().Replace(',', '.'));
			stringBuilder2.Replace("&СтрокаДанных", stringBuilder3.ToString());
		}
		if (val == 0)
		{
			stringBuilder3.Clear();
			stringBuilder3.Append("['&Дате', &CommandMaxQueue, &CommandMaxRun, &CommandError, &CommandCount, ''],\r\n                                                &СтрокаДанных");
			stringBuilder3.Replace("&Дате", DateTime.Now.ToString(text));
			stringBuilder3.Replace("&МЕМ", 0.ToString().Replace(',', '.'));
			stringBuilder2.Replace("&СтрокаДанных", stringBuilder3.ToString());
		}
		stringBuilder2.Replace("&СтрокаДанных", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", stringBuilder2.ToString());
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}

	private async Task<HttpResponse> Logs(HttpRequest Request)
	{
		string text = HttpUtility.UrlDecode(Request.httpArg[1]);
		string NumLog = HttpUtility.UrlDecode(Request.httpArg[2]);
		StringBuilder html = HttpService.RootHtml();
		html.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(html, "Logs", Request.httpArg[1]);
		html.Replace("&СписокУстройств", "");
		StringBuilder htmlBody = HttpService.GetBodyRazdel("Ошибки выполнения команд");
		htmlBody.Append("<br/>\r\n    <table class='table' style='width: 1000px;'>\r\n        <thead>\r\n            <tr>\r\n                <th>dev.</th>\r\n                <th>Дата</th>\r\n                <th>Тип</th>\r\n                <th>Модель</th>\r\n                <th>Заводской №</th>\r\n                <th>ИНН</th>\r\n                <th>Команда</th>\r\n            </tr>\r\n        </thead>\r\n        <tbody>\r\n            &Строка\r\n        </tbody>\r\n    </table>");
		string htmlStr = "<tr style='cursor:pointer' onclick=\"window.location.assign('/&Url/&NumPag/&NumLg')\">\r\n    <td style='width: 10px; text-align: center'>&NumDevice</td>\r\n    <td style='width: 80px; text-align: center'>&Date</td>\r\n    <td style='width: 20px; text-align: center'>&IdDevice</td>\r\n    <td style='width: 20px; text-align: center'>&NameDevice</td>\r\n    <td style='width: 20px; text-align: center'>&NumberKkm</td>\r\n    <td style='width: 40px; text-align: center'>&INN</td>\r\n    <td style='width: 40px; text-align: center'>&Command</td>\r\n</tr>\r\n&Строка";
		string htmlStr2 = "<tr style='cursor:pointer' onclick=\"window.location.assign('/&Url/&NumPag/&NumLg')\">\r\n    <td colspan='7' style='border-bottom: 1px solid #d9d9d9; color: red'>&Error</td>\r\n</tr>\r\n&Строка";
		HttpService.GetButton(htmlBody, "Следующая страница", "Дальше", "", "", "location.href = '/Logs/&NewNumPag'");
		DateTime Start = DateTime.Now;
		if (text != "")
		{
			try
			{
				Start = DateTime.ParseExact(text, "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
			}
			catch
			{
			}
		}
		int CoOnPage = 300;
		if (NumLog == "")
		{
			Global.Logers.SaveSettings();
		}
		StringBuilder htmlStrSb = new StringBuilder();
		StringBuilder htmlStr1Sb = new StringBuilder();
		Logers.ItemLog CurLog = null;
		DateTime NumPagSave = DateTime.Now;
		Global.Logers.CommandLogs.ClouseNext();
		while (true)
		{
			FileLog<Logers.ItemLog>.Field field = await Global.Logers.CommandLogs.GetNext();
			if (field == null)
			{
				break;
			}
			DateTime dateLog = field.DateLog;
			if (dateLog > Start)
			{
				continue;
			}
			int num = CoOnPage - 1;
			CoOnPage = num;
			if (num == 0)
			{
				break;
			}
			Logers.ItemLog value = field.Value;
			if (NumLog != "")
			{
				if (!(dateLog == DateTime.ParseExact(NumLog, "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture)))
				{
					continue;
				}
				CurLog = value;
			}
			NumPagSave = dateLog;
			htmlStrSb.Clear();
			htmlStrSb.Append(htmlStr);
			htmlStrSb.Replace("&Url", "Logs");
			htmlStrSb.Replace("&NumLg", dateLog.ToString("yyyy.MM.dd HH:mm:ss:fff"));
			htmlStrSb.Replace("&NumDevice", value.NumUnit.ToString());
			htmlStrSb.Replace("&Date", dateLog.ToString());
			htmlStrSb.Replace("&IdDevice", value.IdDevice);
			htmlStrSb.Replace("&NameDevice", value.NameDevice);
			htmlStrSb.Replace("&NumberKkm", value.NumberKkm);
			htmlStrSb.Replace("&INN", value.INN);
			htmlStrSb.Replace("&Command", value.Command);
			htmlBody.Replace("&Строка", htmlStrSb.ToString());
			if (NumLog == "")
			{
				htmlStr1Sb.Clear();
				htmlStr1Sb.Append(htmlStr2);
				htmlStr1Sb.Replace("&Url", "Logs");
				htmlStr1Sb.Replace("&NumLg", dateLog.ToString("yyyy.MM.dd HH:mm:ss:fff"));
				if (value.Error.IndexOf("\r") > 10)
				{
					htmlStr1Sb.Replace("&Error", value.Error.Substring(0, value.Error.IndexOf("\r")));
				}
				else
				{
					htmlStr1Sb.Replace("&Error", value.Error);
				}
				htmlBody.Replace("&Строка", htmlStr1Sb.ToString());
			}
		}
		Global.Logers.CommandLogs.ClouseNext();
		htmlBody.Replace("&Строка", "");
		if (NumLog != "" && CurLog != null)
		{
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Текст команды:</div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px;'>" + CurLog.TextCommand.Replace("\r\n", "</br>").Replace("\r", "</br>") + "</div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Текст ошибки:</div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px;'>" + CurLog.Error.Replace("\r\n", "</br>").Replace("\r", "</br>") + "</div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Низкоуровневые команды:</div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px;'>" + CurLog.NetLogs.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r\n", "</br>")
				.Replace("\r", "</br>") + " </div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px; border-top: 1px solid black; color: black; font-size: 16pt;'>Текст ответа:</div>");
			htmlBody.Append("<div style='width: 1000px; padding-left: 20px;'>" + CurLog.Rezult.Replace("\r\n", "</br>").Replace("\r", "</br>") + "</div>");
		}
		htmlBody.Replace("&NewNumPag", NumPagSave.ToString("yyyy.MM.dd HH:mm:ss:fff"));
		htmlBody.Replace("&NumPag", Start.ToString("yyyy.MM.dd HH:mm:ss:fff"));
		html.Replace("&ТелоСтраницы", htmlBody.ToString());
		html.Replace("&ТелоСтраницы", "");
		html.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
	}

	private Dictionary<string, string> ChangeListSelection(Dictionary<string, string> Params, string IdDll, string ParamertName)
	{
		return Params;
	}

	private async Task<HttpResponse> StatusLic(HttpRequest Request)
	{
		string content = "";
		return new HttpResponse(Request, HttpStatusCode.OK, content);
	}

	private async Task<HttpResponse> Execute(HttpRequest Request)
	{
		string typeSunc = Request.httpArg[1];
		string text = Request.GetBodyAsString();
		string jsonContentTxt = text;
		while (text.Length > 0 && text[0] != '{')
		{
			text = text.Substring(1);
		}
		Unit.DataCommand DataCommand;
		Unit.RezultCommand RezultCommand;
		try
		{
			DataCommand = JsonToDataCommand(text);
			DataCommand.UnitPassword = UnitPassword;
			DataCommand.IP_client = Request.RemoteEndPoint.ToString();
		}
		catch (Exception ex)
		{
			RezultCommand = new Unit.RezultCommand
			{
				Error = "Ошибка разбора (парсинга) команды: " + Global.GetErrorMessagee(ex),
				Status = Unit.ExecuteStatus.Error
			};
			await Global.Logers.AddError("<Не опознана>", "Ошибка разбора (парсинга) команды", jsonContentTxt, RezultCommand.Error);
			return new HttpResponse(Request, HttpStatusCode.BadRequest, RezultCommand);
		}
		try
		{
			RezultCommand = await Global.UnitManager.AddCommand(DataCommand, typeSunc, jsonContentTxt);
		}
		catch (ArgumentException ex2)
		{
			RezultCommand = new Unit.RezultCommand
			{
				Error = Global.GetErrorMessagee(ex2),
				Status = Unit.ExecuteStatus.Error
			};
			await Global.Logers.AddError(DataCommand.Command, "Команда не поддерживается", jsonContentTxt, RezultCommand.Error);
			return new HttpResponse(Request, HttpStatusCode.NotImplemented, RezultCommand);
		}
		catch (Exception ex3)
		{
			RezultCommand = new Unit.RezultCommand
			{
				Error = "Ошибка выполнения команды: " + Global.GetErrorMessagee(ex3),
				Status = Unit.ExecuteStatus.Error
			};
			await Global.Logers.AddError(DataCommand.Command, "Ошибка выполнения команды", jsonContentTxt, RezultCommand.Error);
			return new HttpResponse(Request, HttpStatusCode.InternalServerError, RezultCommand);
		}
		if (RezultCommand.SubRezultCommand != null)
		{
			return new HttpResponse(Request, HttpStatusCode.OK, RezultCommand.SubRezultCommand);
		}
		return new HttpResponse(Request, HttpStatusCode.OK, RezultCommand);
	}

	public static Unit.DataCommand JsonToDataCommand(string jsonContent)
	{
		try
		{
			return JsonConvert.DeserializeObject<Unit.DataCommand>(jsonContent.Replace("&#39;", "\\\""));
		}
		catch
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			string text = jsonContent;
			foreach (char c in text)
			{
				if (!flag3 && c == '"')
				{
					flag2 = !flag2;
				}
				if (c == '{' && !flag2)
				{
					flag = true;
					num++;
				}
				if (c == '}' && !flag2)
				{
					num--;
				}
				stringBuilder.Append(c);
				if (flag && num == 0)
				{
					break;
				}
				flag3 = ((flag2 && c == '\\') ? true : false);
			}
			jsonContent = stringBuilder.ToString();
			return JsonConvert.DeserializeObject<Unit.DataCommand>(jsonContent);
		}
	}

	private async Task<HttpResponse> PayByCard(HttpRequest Request, Unit.DataCommand DataCommand = null, Unit.RezultCommandProcessing RezultCommand = null)
	{
		string NumberUnit = Request.httpArg[1];
		bool isSelect = true;
		Unit Unit = null;
		try
		{
			int.Parse(NumberUnit);
			Unit = Global.UnitManager.Units[int.Parse(NumberUnit)];
		}
		catch
		{
			isSelect = false;
		}
		if (DataCommand == null)
		{
			DataCommand = await GetDataPayByCard(Request);
		}
		if (DataCommand != null && DataCommand.IdCommand != null && DataCommand.IdCommand != "" && RezultCommand == null && Unit != null)
		{
			Unit.DataCommand dataCommand = new Unit.DataCommand();
			dataCommand.Command = "GetRezult";
			dataCommand.IdCommand = DataCommand.IdCommand;
			Unit.RezultCommand rezultCommand = await Global.UnitManager.AddCommand(dataCommand, "", "");
			if (rezultCommand != null && rezultCommand.Status != Unit.ExecuteStatus.NotFound)
			{
				RezultCommand = (Unit.RezultCommandProcessing)((Unit.RezultCommandGetRezult)rezultCommand).Rezult;
				if (DataCommand.Command == "PayByPaymentCard")
				{
					DataCommand.UniversalID = RezultCommand.UniversalID;
				}
			}
		}
		StringBuilder stringBuilder = HttpService.RootHtml("", AddUnitTestJS: true);
		stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                var JsonData = '&JsonData';\r\n\r\n                window.addEventListener('DOMContentLoaded', function () {\r\n                    function init() {\r\n                        //var JsonData = document.getElementById('JsonDataCommand').value;\r\n                        if (JsonData != undefined && JsonData != '') {\r\n                            ExecuteCommand(JsonData);\r\n                            JsonData = '';\r\n                            //document.getElementById('JsonDataCommand').value = '';\r\n                            //location.href = location.href.replace('&Run=1', '');\r\n                            history.pushState(null, null, location.href.replace('&Run=1', ''));\r\n                        }\r\n                    }\r\n                    init();\r\n                });\r\n\r\n                &ДополнительныеСкрипты");
		try
		{
			string text = "";
			if (Request.ArgForm.ContainsKey("Run") && Request.ArgForm["Run"] == "1")
			{
				JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
				jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
				text = JsonConvert.SerializeObject(DataCommand, jsonSerializerSettings).Replace("\\\"", "&#39;");
			}
			stringBuilder.Replace("&JsonData", text);
		}
		catch
		{
		}
		HttpService.CommandMenu(stringBuilder, "PayByCard", Request.httpArg[1]);
		HttpService.UnitsMenu(stringBuilder, "PayByCard", Request.httpArg[1], AddUnit: false, UnitPassword);
		StringBuilder bodyRazdel;
		if (isSelect)
		{
			if (DataCommand == null)
			{
				DataCommand = new Unit.DataCommand();
				DataCommand.Command = "PayByPaymentCard";
				DataCommand.Amount = default(decimal);
			}
			_ = Unit.SettDr;
			string protocol = Unit.SettDr.TypeDevice.Protocol;
			TypeDevice.enType type = Unit.SettDr.TypeDevice.Type;
			new StringBuilder("&ТелоСкрипта");
			int num = (int)type;
			num.ToString();
			string text2 = ((Unit == null || Unit.NameDevice == "") ? "" : (": " + Unit.NameDevice));
			bodyRazdel = HttpService.GetBodyRazdel("Оплата через банковский терминал" + text2);
			HttpService.GetForm(bodyRazdel, "SetPayByCard", HttpService.GetUrl(Request, "SetPayByCard", Request.httpArg[1], "Run"));
			HttpService.GetText(bodyRazdel, "Тип устройства :", Unit.SettDr.TypeDevice.Name());
			HttpService.GetText(bodyRazdel, "Протокол :", protocol);
			if (!Unit.IsInit)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Не подключена: " + Unit.LastError.Split('\n')[0], "Red");
			}
			else if (!Unit.Active)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Выключена пользователем");
			}
			else
			{
				HttpService.GetText(bodyRazdel, "Статус :", "В работе (" + Unit.NameDevice + ")");
			}
			HttpService.GetLine(bodyRazdel);
			bodyRazdel.Replace("&ТелоСтраницы", "<input name='IdCommand' class='IdCommand' type='hidden' value='" + Guid.NewGuid().ToString() + "'>&ТелоСтраницы");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("optgroup_1", "Провести транзакцию :");
			dictionary.Add("PayByPaymentCard", "Оплатить платежной картой");
			dictionary.Add("ReturnPaymentByPaymentCard", "Вернуть платеж по платежной карте");
			dictionary.Add("CancelPaymentByPaymentCard", "Отменить платеж по платежной карте");
			dictionary.Add("EmergencyReversal", "Аварийная отмена операции");
			dictionary.Add("optgroup_2", "Отчеты :");
			dictionary.Add("TransactionDetails", "Получить копию слип-чека");
			HttpService.GetSelectBox(bodyRazdel, "Операция по терминалу:", "Command", DataCommand.Command, dictionary, "onchange='document.forms[\"SetPayByCard\"].action = \"" + HttpService.GetUrl(Request, "SetPayByCard", Request.httpArg[1], "Change") + "\"; document.forms[\"SetPayByCard\"].submit();'");
			HttpService.GetInputBox(bodyRazdel, "Сумма оплаты:", "Amount", DataCommand.Amount.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
			bool visibility = DataCommand.Command == "ReturnPaymentByPaymentCard" || DataCommand.Command == "CancelPaymentByPaymentCard" || DataCommand.Command == "EmergencyReversal";
			HttpService.GetInputBox(bodyRazdel, "Идентификатор транзакции:", "UniversalID", DataCommand.UniversalID, "", "", Disabled: false, "", "", "", visibility);
			HttpService.GetFormEnd(bodyRazdel, "", "Выполнить команду");
			HttpService.GetLine(bodyRazdel);
			bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Сверка итогов (закрытие смены):</div>\r\n                        <div class=\"input\"><input class='Button' value='Сверка итогов' type='submit' onclick='Settlement(" + NumberUnit + ")'/></div>\r\n                    </div>&ТелоСтраницы");
			bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Получить итоги дня (краткий):</div>\r\n                        <div class=\"input\"><input class='Button' value='Краткий отчет' type='submit' onclick='TerminalReport(" + NumberUnit + ", false)'/></div>\r\n                    </div>&ТелоСтраницы");
			bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Получить итоги дня (полный):</div>\r\n                        <div class=\"input\"><input class='Button' value='Полный отчет' type='submit' onclick='TerminalReport(" + NumberUnit + ", true)'/></div>\r\n                    </div>&ТелоСтраницы");
			HttpService.GetLine(bodyRazdel);
		}
		else
		{
			bodyRazdel = HttpService.GetBodyRazdel("Выберите устройство");
			HttpService.GetChangeForDevice(bodyRazdel, "Выберите устройство:");
		}
		string text3 = "";
		string text4 = "";
		string text5 = "";
		string text6 = "";
		string text7 = "";
		if (RezultCommand != null)
		{
			if (RezultCommand.Status == Unit.ExecuteStatus.Ok)
			{
				text3 = "Ok";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.Run)
			{
				text3 = "Выполняется";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.Error)
			{
				text3 = "Ошибка!";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.NotFound)
			{
				text3 = "Данные не найдены!";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.NotRun)
			{
				text3 = "Команда ждет в очереди!";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.AlreadyDone)
			{
				text3 = "Команда уже была выполнена ранее!";
			}
			text4 = RezultCommand.Error;
			text5 = RezultCommand.Warning;
			text6 = JsonConvert.SerializeObject(RezultCommand, Formatting.Indented);
			text7 = RezultCommand.Slip;
		}
		HttpService.GetText(bodyRazdel, "Статус выполнения :", text3, "", "", "MessageStatus", (text3 == "") ? "style='display: none;'" : "");
		HttpService.GetText(bodyRazdel, "Ошибка :", text4, "Red", "", "MessageError", (text4 == "") ? "style='display: none;'" : "", "style='white-space:pre;'");
		HttpService.GetText(bodyRazdel, "Предупреждение :", text5, "", "", "MessageWarning", (text5 == "") ? "style='display: none;'" : "", "style='white-space:pre;'");
		HttpService.GetPre(bodyRazdel, "Квитанция/Чек/Slip :", text7, "MessageSlip", !(text7 == ""));
		HttpService.GetText(bodyRazdel, "JSON ответа :", text6, "", "", "MessageReturn", (text6 == "") ? "style='display: none;'" : "", "style='white-space:pre;'");
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}

	private async Task<HttpResponse> SetPayByCard(HttpRequest Request)
	{
		Unit.DataCommand dataCommand = await GetDataPayByCard(Request);
		string path = Request.Path;
		path = path.Replace("/SetPayByCard", "PayByCard").Replace("/Change", "").Replace("/Run", "");
		if (Request.httpArg.Length > 2 && Request.httpArg[2] == "Run")
		{
			dataCommand.UnitPassword = UnitPassword;
			if (dataCommand.TypeCheck == 101 || dataCommand.TypeCheck == 102)
			{
				new Unit.DataCommand
				{
					NumDevice = dataCommand.NumDevice,
					IdCommand = dataCommand.IdCommand,
					Command = ((dataCommand.TypeCheck == 101) ? "DepositingCash" : "PaymentCash"),
					CashierName = dataCommand.CashierName,
					UnitPassword = UnitPassword,
					Amount = dataCommand.Cash
				};
			}
			path += "&Run=1";
			return HttpService.RedirectHTTP(Request, path);
		}
		return HttpService.RedirectHTTP(Request, path);
	}

	private async Task<Unit.DataCommand> GetDataPayByCard(HttpRequest Request)
	{
		if (Request.ArgForm.ContainsKey("UID"))
		{
			return await GetDataCommandByHistory(Request.ArgForm["UID"]);
		}
		if (!Request.ArgForm.ContainsKey("Command"))
		{
			return null;
		}
		Unit.DataCommand dataCommand = new Unit.DataCommand();
		dataCommand.IdCommand = Request.ArgForm["IdCommand"].Trim();
		dataCommand.NumDevice = int.Parse(Request.httpArg[1]);
		dataCommand.Command = Request.ArgForm["Command"].Trim();
		if (Request.ArgForm.ContainsKey("Amount"))
		{
			dataCommand.Amount = Math.Round(decimal.Parse(Request.ArgForm["Amount"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		if (Request.ArgForm.ContainsKey("UniversalID"))
		{
			dataCommand.UniversalID = Request.ArgForm["UniversalID"].Trim();
		}
		return dataCommand;
	}

	private async Task<HttpResponse> PrintCheck(HttpRequest Request, Unit.DataCommand DataCommand = null, Unit.RezultCommand RezultCommand = null)
	{
		string NumberUnit = Request.httpArg[1];
		bool isSelect = true;
		Unit Unit = null;
		try
		{
			int.Parse(NumberUnit);
			Unit = Global.UnitManager.Units[int.Parse(NumberUnit)];
		}
		catch
		{
			isSelect = false;
		}
		if (DataCommand == null)
		{
			DataCommand = await GetDataCommand(Request);
		}
		if (DataCommand != null && DataCommand.IdCommand != null && DataCommand.IdCommand != "" && RezultCommand == null && Unit != null)
		{
			Unit.DataCommand dataCommand = new Unit.DataCommand();
			dataCommand.Command = "GetRezult";
			dataCommand.IdCommand = DataCommand.IdCommand;
			Unit.RezultCommand rezultCommand = await Global.UnitManager.AddCommand(dataCommand, "", "");
			if (rezultCommand != null && rezultCommand.Status != Unit.ExecuteStatus.NotFound)
			{
				RezultCommand = ((Unit.RezultCommandGetRezult)rezultCommand).Rezult;
			}
		}
		StringBuilder html = HttpService.RootHtml("", AddUnitTestJS: true);
		html.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                var JsonData = '&JsonData';\r\n\r\n                window.addEventListener('DOMContentLoaded', function () {\r\n                    function init() {\r\n                        //var JsonData = document.getElementById('JsonDataCommand').value;\r\n                        if (JsonData != undefined && JsonData != '') {\r\n\r\n\t\t\t\t\t\t\tvar textArea = document.createElement('textarea');\r\n\t\t\t\t\t\t\ttextArea.innerHTML = JsonData;\r\n\t\t\t\t\t\t\tJsonData = textArea.value;\r\n\r\n                            ExecuteCommand(JsonData);\r\n                            JsonData = '';\r\n                            //document.getElementById('JsonDataCommand').value = '';\r\n                            //location.href = location.href.replace('&Run=1', '');\r\n                            history.pushState(null, null, location.href.replace('&Run=1', ''));\r\n                        }\r\n                    }\r\n                    init();\r\n                });\r\n\r\n                &ДополнительныеСкрипты");
		try
		{
			string text = "";
			if (Request.ArgForm.ContainsKey("Run") && Request.ArgForm["Run"] == "1")
			{
				JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
				jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
				text = JsonConvert.SerializeObject(DataCommand, jsonSerializerSettings).Replace("\\\"", "&#39;");
				text = HttpUtility.HtmlEncode(text);
			}
			html.Replace("&JsonData", text);
		}
		catch
		{
		}
		HttpService.CommandMenu(html, "PrintCheck", Request.httpArg[1]);
		HttpService.UnitsMenu(html, "PrintCheck", Request.httpArg[1], AddUnit: false, UnitPassword);
		StringBuilder bodyRazdel;
		if (isSelect)
		{
			if (Unit.Kkm.INN == null || Unit.Kkm.INN.Trim() == "")
			{
				Unit.DataCommand dataCommand2 = new Unit.DataCommand();
				dataCommand2.NumDevice = Unit.NumUnit;
				dataCommand2.Command = "GetDataKKT";
				dataCommand2.IdCommand = Guid.NewGuid().ToString();
				dataCommand2.UnitPassword = UnitPassword;
				dataCommand2.AdditionalActions = "fast";
				_ = (Unit.RezultCommandKKm)(await Global.UnitManager.AddCommand(dataCommand2, "", JsonConvert.SerializeObject(dataCommand2)));
			}
			if (DataCommand == null)
			{
				DataCommand = new Unit.DataCommand();
				DataCommand.CheckProps = new Unit.DataCommand.CheckProp[1];
				DataCommand.CheckProps[0] = new Unit.DataCommand.CheckProp();
				DataCommand.CheckStrings = new Unit.DataCommand.CheckString[1];
				DataCommand.CheckStrings[0] = new Unit.DataCommand.CheckString();
				DataCommand.CheckStrings[0].Register = new Unit.DataCommand.Register();
				DataCommand.CheckStrings[0].Register.SignMethodCalculation = 4;
				DataCommand.CheckStrings[0].Register.SignCalculationObject = 4;
				DataCommand.CheckStrings[0].Register.Tax = -1m;
				DataCommand.TypeCheck = 0;
				DataCommand.PayByProcessing = false;
			}
			bool flag = DataCommand.TypeCheck == 2 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 12 || DataCommand.TypeCheck == 13;
			bool flag2 = flag || DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 1 || DataCommand.TypeCheck == 10 || DataCommand.TypeCheck == 11;
			bool flag3 = DataCommand.TypeCheck == 101 || DataCommand.TypeCheck == 102;
			_ = Unit.SettDr;
			string protocol = Unit.SettDr.TypeDevice.Protocol;
			TypeDevice.enType type = Unit.SettDr.TypeDevice.Type;
			new StringBuilder("&ТелоСкрипта");
			int num = (int)type;
			num.ToString();
			string text2 = ((Unit == null || Unit.NameDevice == "") ? "" : (": " + Unit.NameDevice));
			bodyRazdel = HttpService.GetBodyRazdel("Регистрация чека" + text2);
			HttpService.GetForm(bodyRazdel, "SetPrintCheck", HttpService.GetUrl(Request, "SetPrintCheck", Request.httpArg[1], "Run"));
			HttpService.GetText(bodyRazdel, "Тип устройства :", Unit.SettDr.TypeDevice.Name());
			HttpService.GetText(bodyRazdel, "Протокол :", protocol);
			if (!Unit.IsInit)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Не подключена: " + Unit.LastError.Split('\n')[0], "Red");
			}
			else if (!Unit.Active)
			{
				HttpService.GetText(bodyRazdel, "Статус :", "Выключена пользователем");
			}
			else
			{
				HttpService.GetText(bodyRazdel, "Статус :", "В работе (" + Unit.NameDevice + ")");
			}
			HttpService.GetLine(bodyRazdel);
			bodyRazdel.Replace("&ТелоСтраницы", "<input name='IdCommand' class='IdCommand' type='hidden' value='" + Guid.NewGuid().ToString() + "'>&ТелоСтраницы");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("optgroup_1", "Чеки продажи (прихода) :");
			dictionary.Add("0", "Продажа: Чек прихода");
			dictionary.Add("1", "Продажа: Чек возв. приход");
			dictionary.Add("2", "Продажа: Чек кор. прихода");
			dictionary.Add("3", "Продажа: Чек кор. возв.прихода");
			dictionary.Add("optgroup_2", "Чеки покупки (расхода) :");
			dictionary.Add("10", "Покупка: Чек расхода");
			dictionary.Add("11", "Покупка: Чек возв. расхода");
			dictionary.Add("12", "Покупка: Чек кор. расхода");
			dictionary.Add("13", "Покупка: Чек кор. возв.расхода");
			dictionary.Add("optgroup_3", "Инкасация ДС :");
			dictionary.Add("101", "Внесение наличных");
			dictionary.Add("102", "Изъятие наличных");
			HttpService.GetSelectBox(bodyRazdel, "Тип чека:", "TypeCheck", DataCommand.TypeCheck.ToString(), dictionary, "onchange='document.forms[\"SetPrintCheck\"].action = \"" + HttpService.GetUrl(Request, "SetPrintCheck", Request.httpArg[1], "Change") + "\"; document.forms[\"SetPrintCheck\"].submit();'", Disabled: false, "Тип регистрируемого чека");
			if (Unit.Kkm.INN == null || Unit.Kkm.INN.Trim() == "")
			{
				return new HttpResponse(Request, HttpStatusCode.OK, "Устройство не инициализировано.");
			}
			if (Unit.Kkm.IsKKT && (flag2 || flag))
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				string[] array = Unit.Kkm.TaxVariant.Split(',');
				foreach (string text3 in array)
				{
					string value = "";
					switch (text3)
					{
					case "0":
						value = "Основная система налогообложения";
						break;
					case "1":
						value = "УСН (Доход)";
						break;
					case "2":
						value = "УСН (Доход-Расход)";
						break;
					case "3":
						value = "ЕНВД";
						break;
					case "4":
						value = "Сельскохозяйственный налог";
						break;
					case "5":
						value = "Патент";
						break;
					}
					dictionary2.Add(text3, value);
				}
				HttpService.GetSelectBox(bodyRazdel, "Система налогообложения:", "TaxVariant", (DataCommand.TaxVariant == null) ? "" : DataCommand.TaxVariant, dictionary2, "", Disabled: false, "Применяемая для чека система налогообложения");
			}
			HttpService.GetInputBox(bodyRazdel, "ФИО кассира:", "CashierName", DataCommand.CashierName);
			if (Unit.Kkm.FfdVersion >= 2)
			{
				HttpService.GetInputBox(bodyRazdel, "ИНН кассира:", "CashierVATIN", DataCommand.CashierVATIN, "", "", Disabled: false, "Личный ИНН кассира. 12 цифр");
			}
			if (flag2)
			{
				string clientAddress = DataCommand.ClientAddress;
				HttpService.GetInputBox(bodyRazdel, "Email или Тел. клиента:", "ClientAddress", clientAddress, "", "", Disabled: false, "Email или телефон покупателя (для отправки чека почтой или SMS) (Не обязательно)");
			}
			StringBuilder startCollapsPanel = HttpService.GetStartCollapsPanel("Доп.реквизиты продавца:", "AddPrd", string.IsNullOrEmpty(DataCommand.PlaceMarket) && string.IsNullOrEmpty(DataCommand.AddressSettle) && string.IsNullOrEmpty(DataCommand.PlaceMarket));
			if (Unit.Kkm.FfdVersion >= 2)
			{
				string placeMarket = DataCommand.PlaceMarket;
				HttpService.GetInputBox(startCollapsPanel, "Место расчетов:", "PlaceMarket", placeMarket, "", "", Disabled: false, "Место проведения расчетов (для выездной торговли) (Не обязательно)");
				string addressSettle = DataCommand.AddressSettle;
				HttpService.GetInputBox(startCollapsPanel, "Адрес расчетов:", "AddressSettle", addressSettle, "", "", Disabled: false, "Адрес проведения расчетов (для выездной торговли) (Не обязательно");
				string senderEmail = DataCommand.SenderEmail;
				HttpService.GetInputBox(startCollapsPanel, "Email или Тел. продавца:", "SenderEmail", senderEmail, "", "", Disabled: false, "Email или телефон продавца (Не обязательно)");
			}
			HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
			if (flag2)
			{
				if (Unit.Kkm.FfdVersion >= 2)
				{
					startCollapsPanel = HttpService.GetStartCollapsPanel("Доп.реквизиты клиента:", "AddClientInfo", string.IsNullOrEmpty(DataCommand.ClientInfo) && string.IsNullOrEmpty(DataCommand.ClientINN));
					string clientInfo = DataCommand.ClientInfo;
					HttpService.GetInputBox(startCollapsPanel, "Наименование клиента:", "ClientInfo", clientInfo, "", "", Disabled: false, "Наименование организации. (Не обязательно)", "Наименование организации или фамилия, имя, отчество (при наличии), серия и номер паспорта клиента.Только с использованием наличных/электронных денежных средств или при выплате выигрыша, получении страховой премии или при страховой выплате.");
					string clientINN = DataCommand.ClientINN;
					HttpService.GetInputBox(startCollapsPanel, "ИНН клиента:", "ClientINN", clientINN, "", "", Disabled: false, "ИНН организации. (Не обязательно)", "Только с использованием наличных/электронных денежных средств или при выплате выигрыша, получении страховой премии или при страховой выплате.");
					HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
				}
				startCollapsPanel = HttpService.GetStartCollapsPanel("Доп.реквизиты чека:", "AdditionalAttribute", string.IsNullOrEmpty(DataCommand.AdditionalAttribute) && DataCommand.UserAttribute == null);
				string additionalAttribute = DataCommand.AdditionalAttribute;
				HttpService.GetInputBox(startCollapsPanel, "Доп.атрибут чека:", "AdditionalAttribute", additionalAttribute, "", "", Disabled: false, "Дополнительное поле чека, Тег 1192  (Не обязательно)");
				string value2 = "";
				if (DataCommand.UserAttribute != null)
				{
					value2 = DataCommand.UserAttribute.Name;
				}
				HttpService.GetInputBox(startCollapsPanel, "Наименование доп.реквизита:", "UserAttributeName", value2, "", "", Disabled: false, "Наименование дополнительного реквизита пользователя, Тег ОД 1084/1085. (Не обязательно)");
				string value3 = "";
				if (DataCommand.UserAttribute != null)
				{
					value3 = DataCommand.UserAttribute.Value;
				}
				HttpService.GetInputBox(startCollapsPanel, "Значение доп.реквизита:", "UserAttributeValue", value3, "", "", Disabled: false, "Значение дополнительного реквизита пользователя, Тег ОФД 1084/1086 (Не обязательно)");
				HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
			}
			if (flag)
			{
				HttpService.GetLine(bodyRazdel);
				if (Unit.Kkm.FfdVersion >= 2)
				{
					Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
					dictionary3.Add("0", "Самостоятельно");
					dictionary3.Add("1", "По предписанию налоговой");
					HttpService.GetSelectBox(bodyRazdel, "Тип коррекции:", "CorrectionType", DataCommand.CorrectionType.ToString(), dictionary3);
					string value4 = "";
					if (DataCommand.CorrectionBaseDate.HasValue)
					{
						value4 = DataCommand.CorrectionBaseDate.Value.ToString("yyyy-MM-dd");
					}
					HttpService.GetInputBox(bodyRazdel, "Номер документа-основания:", "CorrectionBaseNumber", DataCommand.CorrectionBaseNumber, "", "", Disabled: false, "Номер документа-предписания для корректировки");
					HttpService.GetInputBox(bodyRazdel, "Дата документа-основания:", "CorrectionBaseDate", value4, "date", "", Disabled: false, "Дата документа-предписания для корректировки");
				}
			}
			Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
			dictionary4.Add("False", "Не использовать");
			dictionary4.Add("True", "Использовать");
			dictionary4.Add("null", "По настройкам устройства");
			HttpService.GetSelectBox(bodyRazdel, "Использование эквайринга:", "PayByProcessing", (!DataCommand.PayByProcessing.HasValue) ? "null" : DataCommand.PayByProcessing.ToString(), dictionary4, "", Disabled: false, "Проводить или нет оплату по эквайрингу (в случае безналичной оплаты)");
			int i = 0;
			if (flag2)
			{
				Unit.DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				for (num = 0; num < checkStrings.Length; i++, num++)
				{
					Unit.DataCommand.CheckString checkString = checkStrings[num];
					if (checkString.Register == null)
					{
						continue;
					}
					HttpService.GetLine(bodyRazdel);
					HttpService.GetText(bodyRazdel, "Фискальная строка :", "№ " + (i + 1));
					HttpService.GetInputBox(bodyRazdel, "Наименование товара:", "Name" + i, checkString.Register.Name);
					StringBuilder stringBuilder = new StringBuilder("<div class='InputValue'>\r\n                                <div class='Caption' align='right'>\r\n                                    Количество * Цена = Сумма:\r\n                                </div>\r\n                                <div class='input'>\r\n                                    <input class='input' name='Quantity&Num' id='Quantity&Num' type='number' step='0.01' min='0' placeholder='0,00' value='&Quantity' style='width: 71px;' onchange='CalckRegisterString(\"&Num\")'>\r\n                                    <div class='Caption' style='width: 10px;padding-left: 8px;'>*</div>\r\n                                    <input class='input' name='Price&Num' id='Price&Num' type='number' step='0.01' min='0' placeholder='0,00' value='&Price' style='width: 120px;' onchange='CalckRegisterString(\"&Num\")'>\r\n                                    <div class='Caption' style='width: 10px;padding-left: 8px;'>=</div>\r\n                                    <input class='input' name='Summ&Num' id='Summ&Num' type='number' step='0.01' min='0' placeholder='0,00' value='&Summ' style='width: 120px;' disabled>\r\n                                </div>\r\n                            </div>\r\n                            &ТелоСтраницы");
					stringBuilder.Replace("&Num", i.ToString());
					stringBuilder.Replace("&Quantity", checkString.Register.Quantity.ToString().Replace(',', '.'));
					stringBuilder.Replace("&Price", checkString.Register.Price.ToString().Replace(',', '.'));
					stringBuilder.Replace("&Summ", (checkString.Register.Price * checkString.Register.Quantity).ToString().Replace(',', '.'));
					bodyRazdel.Replace("&ТелоСтраницы", stringBuilder.ToString());
					Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
					dictionary5.Add("-1", "Без НДС");
					dictionary5.Add("22", "НДС 22%");
					dictionary5.Add("20", "НДС 20%");
					dictionary5.Add("10", "НДС 10%");
					dictionary5.Add("7", "НДС 7%");
					dictionary5.Add("5", "НДС 5%");
					dictionary5.Add("0", "НДС 0%");
					dictionary5.Add("122", "НДС 122/22%");
					dictionary5.Add("120", "НДС 120/20%");
					dictionary5.Add("110", "НДС 110/10%");
					dictionary5.Add("107", "НДС 107/7%");
					dictionary5.Add("105", "НДС 105/5%");
					HttpService.GetSelectBox(bodyRazdel, "Ставка НДС:", "Tax" + i, checkString.Register.Tax.ToString(), dictionary5);
					if (Unit.Kkm.FfdVersion >= 1)
					{
						Dictionary<string, string> dictionary6 = new Dictionary<string, string>();
						dictionary6.Add("1", "ПРЕДОПЛАТА 100% (Полная предварительная оплата до момента передачи предмета расчета)");
						dictionary6.Add("2", "ПРЕДОПЛАТА (Частичная предварительная оплата до момента передачи предмета расчета)");
						dictionary6.Add("3", "АВАНС");
						dictionary6.Add("4", "ПОЛНЫЙ РАСЧЕТ (Полная оплата, в том числе с учетом аванса в момент передачи предмета расчета)");
						dictionary6.Add("5", "ЧАСТИЧНЫЙ РАСЧЕТ И КРЕДИТ (Частичная оплата предмета расчета в момент его передачи с последующей оплатой в кредит )");
						dictionary6.Add("6", "ПЕРЕДАЧА В КРЕДИТ (Передача предмета расчета без его оплаты в момент его передачи с последующей оплатой в кредит)");
						dictionary6.Add("7", "ОПЛАТА КРЕДИТА (Оплата предмета расчета после его передачи с оплатой в кредит )");
						HttpService.GetSelectBox(bodyRazdel, "Признак способа расчета:", "SignMethodCalculation" + i, checkString.Register.SignMethodCalculation.ToString(), dictionary6, "", Disabled: false, "Признак способа расчета");
						Dictionary<string, string> dictionary7 = new Dictionary<string, string>();
						dictionary7.Add("1", "ТОВАР (наименование и иные сведения, описывающие товар)");
						dictionary7.Add("2", "ПОДАКЦИЗНЫЙ ТОВАР (наименование и иные сведения, описывающие товар)");
						dictionary7.Add("3", "РАБОТА (наименование и иные сведения, описывающие работу)");
						dictionary7.Add("4", "УСЛУГА (наименование и иные сведения, описывающие услугу)");
						dictionary7.Add("5", "СТАВКА АЗАРТНОЙ ИГРЫ (при осуществлении деятельности по проведению азартных игр)");
						dictionary7.Add("6", "ВЫИГРЫШ АЗАРТНОЙ ИГРЫ (при осуществлении деятельности по проведению азартных игр)");
						dictionary7.Add("7", "ЛОТЕРЕЙНЫЙ БИЛЕТ (при осуществлении деятельности по проведению лотерей)");
						dictionary7.Add("8", "ВЫИГРЫШ ЛОТЕРЕИ (при осуществлении деятельности по проведению лотерей)");
						dictionary7.Add("9", "ПРЕДОСТАВЛЕНИЕ РИД (предоставлении прав на использование результатов интеллектуальной деятельности или средств индивидуализации)");
						dictionary7.Add("10", "ПЛАТЕЖ (аванс, задаток, предоплата, кредит, взнос в счет оплаты, пени, штраф, вознаграждение, бонус и иной аналогичный предмет расчета)");
						dictionary7.Add("11", "АГЕНТСКОЕ ВОЗНАГРАЖДЕНИЕ (вознаграждение (банковского)платежного агента/субагента, комиссионера, поверенного или иным агентом)");
						dictionary7.Add("12", "СОСТАВНОЙ ПРЕДМЕТ РАСЧЕТА (предмет расчета, состоящем из предметов, каждому из которых может быть присвоено вышестоящее значение");
						dictionary7.Add("13", "ИНОЙ ПРЕДМЕТ РАСЧЕТА (предмет расчета, не относящемуся к предметам расчета, которым может быть присвоено вышестоящее значение");
						dictionary7.Add("14", "ИМУЩЕСТВЕННОЕ ПРАВО (передача имущественных прав)");
						dictionary7.Add("15", "ВНЕРЕАЛИЗАЦИОННЫЙ ДОХОД");
						dictionary7.Add("16", "СТРАХОВЫЕ ВЗНОСЫ (суммы расходов, уменьшающих сумму налога/аванса в соответствии с п. 3.1 статьи 346.21 Налогового кодекса)");
						dictionary7.Add("17", "ТОРГОВЫЙ СБОР (суммы уплаченного торгового сбора)");
						dictionary7.Add("18", "КУРОРТНЫЙ СБОР");
						dictionary7.Add("19", "ЗАЛОГ");
						HttpService.GetSelectBox(bodyRazdel, "Признак предмета расчета:", "SignCalculationObject" + i, checkString.Register.SignCalculationObject.ToString(), dictionary7, "", Disabled: false, "Признак предмета расчета");
					}
					if (Unit.Kkm.FfdVersion >= 4)
					{
						Dictionary<string, string> dictionary8 = new Dictionary<string, string>();
						dictionary8.Add("0", "шт.или ед");
						dictionary8.Add("10", "г");
						dictionary8.Add("11", "кг");
						dictionary8.Add("12", "т");
						dictionary8.Add("20", "см");
						dictionary8.Add("21", "дм");
						dictionary8.Add("22", "м");
						dictionary8.Add("30", "кв.см");
						dictionary8.Add("31", "кв.дм");
						dictionary8.Add("32", "кв.м");
						dictionary8.Add("40", "мл");
						dictionary8.Add("41", "л");
						dictionary8.Add("42", "куб.м");
						dictionary8.Add("50", "кВт ч");
						dictionary8.Add("51", "Гкал");
						dictionary8.Add("70", "сутки");
						dictionary8.Add("71", "час");
						dictionary8.Add("72", "мин");
						dictionary8.Add("73", "с");
						dictionary8.Add("80", "Кбайт");
						dictionary8.Add("81", "Мбайт");
						dictionary8.Add("82", "Гбайт");
						dictionary8.Add("83", "Тбайт");
						dictionary8.Add("255", "Прочее");
						HttpService.GetSelectBox(bodyRazdel, "Мера количества:", "MeasureOfQuantity" + i, checkString.Register.MeasureOfQuantity.ToString(), dictionary8, "", Disabled: false, "Мера количества предмета расчета</br>В 99.9% случаев - шт.или ед");
					}
					if (Unit.Kkm.FfdVersion >= 3)
					{
						startCollapsPanel = HttpService.GetStartCollapsPanel("Система маркировки КИЗ:", "GoodCodeData" + i, checkString.Register.GoodCodeData == null);
						HttpService.GetInputBox(startCollapsPanel, "Код Маркировки:", "BarCode" + i, (checkString.Register.GoodCodeData == null) ? "" : checkString.Register.GoodCodeData.BarCode, "", "", Disabled: false, "Штрих-код маркировки товара со сканера");
						HttpService.GetInputBox(startCollapsPanel, "Использовать плохой КМ:", "AcceptOnBad" + i, (checkString.Register.GoodCodeData == null) ? "false" : checkString.Register.GoodCodeData.AcceptOnBad.ToString(), "checkbox", "", Disabled: false, "Регистрировать чек даже если код маркировки не прошел проверку");
						if (Unit.Kkm.FfdVersion >= 4)
						{
							HttpService.GetInputBox(startCollapsPanel, "Количество товара в упаковке:", "PackageQuantity" + i, (!checkString.Register.PackageQuantity.HasValue) ? "" : checkString.Register.PackageQuantity.ToString().Replace(',', '.'), "number", "", Disabled: false, "Общее количество товара в маркированной упаковке. </br>Если без упаковки - не заполняйте.</br>Как правило продажа идет не из упаковки ");
						}
						HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
					}
					startCollapsPanel = HttpService.GetStartCollapsPanel("Агентская схема:", "AgentSignPanel" + i, checkString.Register.AgentData == null && checkString.Register.PurveyorData == null && !checkString.Register.AgentSign.HasValue);
					Dictionary<string, string> dictionary9 = new Dictionary<string, string>();
					dictionary9.Add("", "<Не указано>");
					dictionary9.Add("0", "0: Банковский платежный агент");
					dictionary9.Add("1", "1: Банковский платежный субагент");
					dictionary9.Add("2", "2: Платежный агент");
					dictionary9.Add("3", "3: Платежный субагент");
					dictionary9.Add("4", "4: Поверенный");
					dictionary9.Add("5", "5: Комиссионер");
					dictionary9.Add("6", "6: Агент");
					HttpService.GetSelectBox(startCollapsPanel, "Признак агента:", "AgentSign" + i, (!checkString.Register.AgentSign.HasValue) ? "" : checkString.Register.AgentSign.ToString(), dictionary9, "onchange='ChangeAgentSignCheck(\"" + i + "\")'", Disabled: false, "Признак агента. Тег ОФД 1222");
					HttpService.GetInputBox(startCollapsPanel, "Операция платежного агента:", "PayingAgentOperation" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.PayingAgentOperation, "", "", Disabled: false, "Операция платежного агента. Тег ОФД 1044");
					HttpService.GetInputBox(startCollapsPanel, "Тел.платежного агента:", "PayingAgentPhone" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.PayingAgentPhone, "", "", Disabled: false, "Телефон платежного агента. Тег ОФД 1073");
					HttpService.GetInputBox(startCollapsPanel, "Тел.оператора приема платежей:", "ReceivePaymentsOperatorPhone" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.ReceivePaymentsOperatorPhone, "", "", Disabled: false, "Телефон оператора по приему платежей. Тег ОФД 1074");
					HttpService.GetInputBox(startCollapsPanel, "Тел.оператора перевода:", "MoneyTransferOperatorPhone" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.MoneyTransferOperatorPhone, "", "", Disabled: false, "Телефон оператора перевода. Тег ОФД 1075");
					HttpService.GetInputBox(startCollapsPanel, "Оператор перевода:", "MoneyTransferOperatorName" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.MoneyTransferOperatorName, "", "", Disabled: false, "Наименование оператора перевода. Тег ОФД 1026");
					HttpService.GetInputBox(startCollapsPanel, "Адрес оператора перевода:", "MoneyTransferOperatorAddress" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.MoneyTransferOperatorAddress, "", "", Disabled: false, "Адрес оператора перевода. Тег ОФД 1005");
					HttpService.GetInputBox(startCollapsPanel, "ИНН оператора перевода:", "MoneyTransferOperatorVATIN" + i, (checkString.Register.AgentData == null) ? "" : checkString.Register.AgentData.MoneyTransferOperatorVATIN, "", "", Disabled: false, "ИНН оператора перевода. Тег ОФД 1016");
					HttpService.GetInputBox(startCollapsPanel, "Телефон поставщика:", "PurveyorPhone" + i, (checkString.Register.PurveyorData == null) ? "" : checkString.Register.PurveyorData.PurveyorPhone, "", "", Disabled: false, "Телефон поставщика. Тег ОД 1171");
					HttpService.GetInputBox(startCollapsPanel, "Наименование поставщика:", "PurveyorName" + i, (checkString.Register.PurveyorData == null) ? "" : checkString.Register.PurveyorData.PurveyorName, "", "", Disabled: false, "Наименование поставщика. Тег ОФД 1225");
					HttpService.GetInputBox(startCollapsPanel, "ИНН поставщика:", "PurveyorVATIN" + i, (checkString.Register.PurveyorData == null) ? "" : checkString.Register.PurveyorData.PurveyorVATIN, "", "", Disabled: false, "ИНН поставщика. Тег ОФД 1226");
					startCollapsPanel.Replace("&ТелоСтраницы", "<script>\r\n                                ChangeAgentSignCheck(\"" + i + "\");\r\n                            </script>\r\n                            &ТелоСтраницы");
					HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
					string name = "AdditionalAttributeStr" + i;
					if (!string.IsNullOrEmpty(checkString.Register.CountryOfOrigin) || !string.IsNullOrEmpty(checkString.Register.CustomsDeclaration))
					{
						goto IL_1ecc;
					}
					if (checkString.Register.ExciseAmount.HasValue)
					{
						decimal? exciseAmount = checkString.Register.ExciseAmount;
						if (!((exciseAmount.GetValueOrDefault() == default(decimal)) & exciseAmount.HasValue))
						{
							goto IL_1ecc;
						}
					}
					int collaps = (string.IsNullOrEmpty(checkString.Register.AdditionalAttribute) ? 1 : 0);
					goto IL_1ecd;
					IL_1ecc:
					collaps = 0;
					goto IL_1ecd;
					IL_1ecd:
					startCollapsPanel = HttpService.GetStartCollapsPanel("Доп.реквизиты строки:", name, (byte)collaps != 0);
					if (Unit.Kkm.FfdVersion >= 3)
					{
						HttpService.GetInputBox(startCollapsPanel, "Цифровой код страны:", "CountryOfOrigin" + i, checkString.Register.CountryOfOrigin, "", "", Disabled: false, "Цифровой код страны происхождения товара, 3 симв. Тег 1230");
						HttpService.GetInputBox(startCollapsPanel, "Номер там.декларации:", "CustomsDeclaration" + i, checkString.Register.CustomsDeclaration, "", "", Disabled: false, "Регистрационный номер таможенной декларации 32 симв. Тег 1231");
					}
					HttpService.GetInputBox(startCollapsPanel, "Сумма акциза:", "ExciseAmount" + i, checkString.Register.ExciseAmount.ToString().Replace(',', '.'), "number", "", Disabled: false, "Сумма акциза с учетом копеек, включенная в стоимость предмета расчета, Тег 1229", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
					if (Unit.Kkm.FfdVersion >= 3)
					{
						HttpService.GetInputBox(startCollapsPanel, "Доп.реквизит:", "AdditionalAttribute" + i, checkString.Register.AdditionalAttribute, "", "", Disabled: false, "Дополнительный реквизит предмета расчета тег 1191");
					}
					HttpService.GetEndCollapsPanel(bodyRazdel, startCollapsPanel);
				}
				HttpService.GetLine(bodyRazdel);
				HttpService.GetHeadetBox(bodyRazdel, "", "<h5>\r\n                        <input class='Button' value='Добавить строку' type='submit' formaction='/SetPrintCheck/" + Request.httpArg[1] + "/Add'/>\r\n                        <span/>\r\n                        <input class='Button' value='Удалить строку' type='submit' formaction='/SetPrintCheck/" + Request.httpArg[1] + "/Del' style='background: rgba(145, 50, 51, 1); color:#FFF'/>\r\n                        </h5>", Any: true);
			}
			if (flag2)
			{
				HttpService.GetText(bodyRazdel, "", "Принятая оплата по чеку:");
			}
			if (flag2 && Unit.Kkm.FfdVersion >= 2)
			{
				HttpService.GetInputBox(bodyRazdel, "Наличные:", "Cash", DataCommand.Cash.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Безналичными", "ElectronicPayment", DataCommand.ElectronicPayment.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Зачет аванса", "AdvancePayment", DataCommand.AdvancePayment.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "В кредит", "Credit", DataCommand.Credit.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Встречное представление", "CashProvision", DataCommand.CashProvision.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
			}
			else if (flag2 && Unit.Kkm.FfdVersion == 1)
			{
				HttpService.GetInputBox(bodyRazdel, "Наличные:", "Cash", DataCommand.Cash.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Безналичными", "ElectronicPayment", DataCommand.ElectronicPayment.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Зачет аванса", "AdvancePayment", DataCommand.AdvancePayment.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "В кредит", "Credit", DataCommand.Credit.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
			}
			else if (flag && Unit.Kkm.FfdVersion >= 2)
			{
				HttpService.GetInputBox(bodyRazdel, "Наличные:", "Cash", DataCommand.Cash.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Безналичными", "ElectronicPayment", DataCommand.ElectronicPayment.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Зачет аванса", "AdvancePayment", DataCommand.AdvancePayment.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "В кредит", "Credit", DataCommand.Credit.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
				HttpService.GetInputBox(bodyRazdel, "Встречное представление", "CashProvision", DataCommand.CashProvision.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
			}
			else if (flag && Unit.Kkm.FfdVersion == 1)
			{
				HttpService.GetInputBox(bodyRazdel, "Наличные:", "Cash", DataCommand.Cash.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
			}
			else if (flag3)
			{
				HttpService.GetInputBox(bodyRazdel, "Сумма:", "Cash", DataCommand.Cash.ToString().Replace(',', '.'), "number", "", Disabled: false, "", "", "", Visibility: true, "step='0.01' min='0' placeholder='0,00'");
			}
			HttpService.GetFormEnd(bodyRazdel, "", "Зарегистрировать чек");
			HttpService.GetLine(bodyRazdel);
			if (Unit.Kkm.FfdVersion == 1)
			{
				bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Открыть смену на ККМ:</div>\r\n                        <div class=\"input\"><input class='Button' value='Открыть смену' type='submit' onclick='OpenShift(" + NumberUnit + ", document.getElementById(\"CashierName\").value)'/></div>\r\n                    </div>&ТелоСтраницы");
				bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Закрыть смену на ККМ:</div>\r\n                        <div class=\"input\"><input class='Button' value='Закрыть смену' type='submit' onclick='CloseShift(" + NumberUnit + ", document.getElementById(\"CashierName\").value)'/></div>\r\n                    </div>&ТелоСтраницы");
			}
			else
			{
				bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Открыть смену на ККМ:</div>\r\n                        <div class=\"input\"><input class='Button' value='Открыть смену' type='submit' onclick='OpenShift(" + NumberUnit + ", document.getElementById(\"CashierName\").value, document.getElementById(\"CashierVATIN\").value )'/></div>\r\n                    </div>&ТелоСтраницы");
				bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Закрыть смену на ККМ:</div>\r\n                        <div class=\"input\"><input class='Button' value='Закрыть смену' type='submit' onclick='CloseShift(" + NumberUnit + ", document.getElementById(\"CashierName\").value, document.getElementById(\"CashierVATIN\").value )'/></div>\r\n                    </div>&ТелоСтраницы");
			}
			bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Снятие Х отчета без гашения:</div>\r\n                        <div class=\"input\"><input class='Button' value='X отчет' type='submit' onclick='XReport(" + NumberUnit + ")'/></div>\r\n                    </div>&ТелоСтраницы");
			if (Unit.Kkm.IsKKT)
			{
				bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Отчет диагностики ОФД:</div>\r\n                        <div class=\"input\"><input class='Button' value='ОФД диагностика' type='submit' onclick='OfdReport(" + NumberUnit + ")'/></div>\r\n                        </div>&ТелоСтраницы");
			}
			HttpService.GetLine(bodyRazdel);
		}
		else
		{
			bodyRazdel = HttpService.GetBodyRazdel("Выберите устройство");
			HttpService.GetChangeForDevice(bodyRazdel, "Выберите устройство:");
		}
		string text4 = "";
		string text5 = "";
		string text6 = "";
		string text7 = "";
		if (RezultCommand != null)
		{
			if (RezultCommand.Status == Unit.ExecuteStatus.Ok)
			{
				text4 = "Ok";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.Run)
			{
				text4 = "Выполняется";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.Error)
			{
				text4 = "Ошибка!";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.NotFound)
			{
				text4 = "Данные не найдены!";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.NotRun)
			{
				text4 = "Команда ждет в очереди!";
			}
			else if (RezultCommand.Status == Unit.ExecuteStatus.AlreadyDone)
			{
				text4 = "Команда уже была выполнена ранее!";
			}
			text5 = RezultCommand.Error;
			text6 = RezultCommand.Warning;
			text7 = JsonConvert.SerializeObject(RezultCommand, Formatting.Indented);
		}
		HttpService.GetText(bodyRazdel, "Статус выполнения :", text4, "", "", "MessageStatus", (text4 == "") ? "style='display: none;'" : "");
		HttpService.GetText(bodyRazdel, "Ошибка :", text5, "Red", "", "MessageError", (text5 == "") ? "style='display: none;'" : "", "style='white-space:pre;'");
		HttpService.GetText(bodyRazdel, "Предупреждение :", text6, "", "", "MessageWarning", (text6 == "") ? "style='display: none;'" : "", "style='white-space:pre;'");
		HttpService.GetText(bodyRazdel, "JSON ответа :", text7, "", "", "MessageReturn", (text7 == "") ? "style='display: none;'" : "", "style='white-space:pre;'");
		html.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		html.Replace("&ТелоСтраницы", "");
		html.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, html.ToString());
	}

	private async Task<HttpResponse> SetPrintCheck(HttpRequest Request)
	{
		Unit.DataCommand dataCommand = await GetDataCommand(Request);
		string path = Request.Path;
		path = path.Replace("/SetPrintCheck", "PrintCheck").Replace("/Change", "").Replace("/Run", "");
		if (Request.httpArg.Length > 2 && (Request.httpArg[2] == "Add" || Request.httpArg[2] == "Del"))
		{
			return HttpService.RedirectHTTP(Request, path);
		}
		if (Request.httpArg.Length > 2 && Request.httpArg[2] == "Run")
		{
			dataCommand.UnitPassword = UnitPassword;
			if (dataCommand.TypeCheck == 101 || dataCommand.TypeCheck == 102)
			{
				new Unit.DataCommand
				{
					NumDevice = dataCommand.NumDevice,
					IdCommand = dataCommand.IdCommand,
					Command = ((dataCommand.TypeCheck == 101) ? "DepositingCash" : "PaymentCash"),
					CashierName = dataCommand.CashierName,
					UnitPassword = UnitPassword,
					Amount = dataCommand.Cash
				};
			}
			path += "&Run=1";
			return HttpService.RedirectHTTP(Request, path);
		}
		return HttpService.RedirectHTTP(Request, path);
	}

	private async Task<Unit.DataCommand> GetDataCommand(HttpRequest Request)
	{
		if (Request.ArgForm.ContainsKey("UID"))
		{
			return await GetDataCommandByHistory(Request.ArgForm["UID"]);
		}
		if (!Request.ArgForm.ContainsKey("TypeCheck"))
		{
			return null;
		}
		Unit.DataCommand dataCommand = new Unit.DataCommand();
		dataCommand.Command = "RegisterCheck";
		dataCommand.IdCommand = Request.ArgForm["IdCommand"].Trim();
		dataCommand.NumDevice = int.Parse(Request.httpArg[1]);
		if (Request.ArgForm["TypeCheck"] == "")
		{
			dataCommand.TypeCheck = 0;
		}
		else if (Request.ArgForm["TypeCheck"] == "101")
		{
			dataCommand.Command = "DepositingCash";
			dataCommand.TypeCheck = int.Parse(Request.ArgForm["TypeCheck"]);
			if (Request.ArgForm["Cash"].Trim() == "")
			{
				Request.ArgForm["Cash"] = "0";
			}
			dataCommand.Amount = Math.Round(decimal.Parse(Request.ArgForm["Cash"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		else if (Request.ArgForm["TypeCheck"] == "102")
		{
			dataCommand.Command = "PaymentCash";
			dataCommand.TypeCheck = int.Parse(Request.ArgForm["TypeCheck"]);
			if (Request.ArgForm["Cash"].Trim() == "")
			{
				Request.ArgForm["Cash"] = "0";
			}
			dataCommand.Amount = Math.Round(decimal.Parse(Request.ArgForm["Cash"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		else
		{
			dataCommand.TypeCheck = int.Parse(Request.ArgForm["TypeCheck"]);
		}
		dataCommand.IsFiscalCheck = true;
		if (Request.ArgForm.ContainsKey("PlaceMarket"))
		{
			dataCommand.PlaceMarket = Request.ArgForm["PlaceMarket"];
		}
		if (Request.ArgForm.ContainsKey("AddressSettle"))
		{
			dataCommand.AddressSettle = Request.ArgForm["AddressSettle"];
		}
		if (Request.ArgForm.ContainsKey("SenderEmail"))
		{
			dataCommand.SenderEmail = Request.ArgForm["SenderEmail"];
		}
		if (Request.ArgForm.ContainsKey("CashierName"))
		{
			dataCommand.CashierName = Request.ArgForm["CashierName"];
		}
		if (Request.ArgForm.ContainsKey("CashierVATIN"))
		{
			dataCommand.CashierVATIN = Request.ArgForm["CashierVATIN"];
		}
		if (Request.ArgForm.ContainsKey("TaxVariant"))
		{
			dataCommand.TaxVariant = Request.ArgForm["TaxVariant"];
		}
		bool num = dataCommand.TypeCheck == 2 || dataCommand.TypeCheck == 3 || dataCommand.TypeCheck == 12 || dataCommand.TypeCheck == 13;
		if (!num && dataCommand.TypeCheck != 0 && dataCommand.TypeCheck != 1 && dataCommand.TypeCheck != 10)
		{
			_ = dataCommand.TypeCheck == 11;
		}
		if (dataCommand.TypeCheck != 101)
		{
			_ = dataCommand.TypeCheck == 102;
		}
		if (Request.ArgForm.ContainsKey("ClientAddress") && Request.ArgForm["ClientAddress"] != "")
		{
			dataCommand.ClientAddress = Request.ArgForm["ClientAddress"];
		}
		if (Request.ArgForm.ContainsKey("ClientInfo") && Request.ArgForm["ClientInfo"] != "")
		{
			dataCommand.ClientInfo = Request.ArgForm["ClientInfo"];
		}
		if (Request.ArgForm.ContainsKey("ClientINN") && Request.ArgForm["ClientINN"] != "")
		{
			dataCommand.ClientINN = Request.ArgForm["ClientINN"];
		}
		if (Request.ArgForm.ContainsKey("AdditionalAttribute") && Request.ArgForm["AdditionalAttribute"] != "")
		{
			dataCommand.AdditionalAttribute = Request.ArgForm["AdditionalAttribute"];
		}
		if (Request.ArgForm.ContainsKey("UserAttributeValue") && Request.ArgForm["UserAttributeValue"] != "")
		{
			dataCommand.UserAttribute = new Unit.DataCommand.TypeUserAttribute();
			dataCommand.UserAttribute.Name = Request.ArgForm["UserAttributeName"];
			dataCommand.UserAttribute.Value = Request.ArgForm["UserAttributeValue"];
		}
		if (num)
		{
			try
			{
				dataCommand.CorrectionType = int.Parse(Request.ArgForm["CorrectionType"]);
				dataCommand.CorrectionBaseNumber = Request.ArgForm["CorrectionBaseNumber"];
				dataCommand.CorrectionBaseDate = DateTime.Parse(Request.ArgForm["CorrectionBaseDate"]);
			}
			catch
			{
			}
		}
		dataCommand.PayByProcessing = false;
		if (Request.ArgForm.ContainsKey("PayByProcessing") && Request.ArgForm["PayByProcessing"] != "")
		{
			if (Request.ArgForm["PayByProcessing"] == "False")
			{
				dataCommand.PayByProcessing = false;
			}
			else if (Request.ArgForm["PayByProcessing"] == "True")
			{
				dataCommand.PayByProcessing = true;
			}
			else if (Request.ArgForm["PayByProcessing"] == "null")
			{
				dataCommand.PayByProcessing = null;
			}
		}
		int i;
		for (i = 0; i < 100; i++)
		{
			if (!Request.ArgForm.ContainsKey("Name" + i))
			{
				break;
			}
		}
		if (Request.httpArg.Length > 2 && Request.httpArg[2] == "Add")
		{
			dataCommand.CheckStrings = new Unit.DataCommand.CheckString[i + 1];
			dataCommand.CheckStrings[i] = new Unit.DataCommand.CheckString();
			dataCommand.CheckStrings[i].Register = new Unit.DataCommand.Register();
			dataCommand.CheckStrings[i].Register.SignMethodCalculation = 4;
			dataCommand.CheckStrings[i].Register.SignCalculationObject = 4;
		}
		else if (Request.httpArg.Length > 2 && Request.httpArg[2] == "Del")
		{
			dataCommand.CheckStrings = new Unit.DataCommand.CheckString[--i];
		}
		else
		{
			dataCommand.CheckStrings = new Unit.DataCommand.CheckString[i];
		}
		for (int j = 0; j < i; j++)
		{
			dataCommand.CheckStrings[j] = new Unit.DataCommand.CheckString();
			dataCommand.CheckStrings[j].Register = new Unit.DataCommand.Register();
			dataCommand.CheckStrings[j].Register.Name = Request.ArgForm["Name" + j];
			if (Request.ArgForm["Quantity" + j].Trim() == "")
			{
				Request.ArgForm["Quantity" + j] = "0";
			}
			dataCommand.CheckStrings[j].Register.Quantity = Math.Round(decimal.Parse(Request.ArgForm["Quantity" + j].Replace(',', '.'), CultureInfo.InvariantCulture), 3, MidpointRounding.AwayFromZero);
			if (Request.ArgForm["Price" + j].Trim() == "")
			{
				Request.ArgForm["Price" + j] = "0";
			}
			dataCommand.CheckStrings[j].Register.Price = Math.Round(decimal.Parse(Request.ArgForm["Price" + j].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
			dataCommand.CheckStrings[j].Register.Amount = dataCommand.CheckStrings[j].Register.Quantity * dataCommand.CheckStrings[j].Register.Price;
			dataCommand.CheckStrings[j].Register.Tax = decimal.Parse(Request.ArgForm["Tax" + j]);
			if (Request.ArgForm.ContainsKey("SignMethodCalculation" + j))
			{
				dataCommand.CheckStrings[j].Register.SignMethodCalculation = int.Parse(Request.ArgForm["SignMethodCalculation" + j]);
			}
			if (Request.ArgForm.ContainsKey("SignCalculationObject" + j))
			{
				dataCommand.CheckStrings[j].Register.SignCalculationObject = int.Parse(Request.ArgForm["SignCalculationObject" + j]);
			}
			if (Request.ArgForm.ContainsKey("MeasureOfQuantity" + j) && Request.ArgForm["MeasureOfQuantity" + j] != "")
			{
				dataCommand.CheckStrings[j].Register.MeasureOfQuantity = uint.Parse(Request.ArgForm["MeasureOfQuantity" + j]);
			}
			if (Request.ArgForm.ContainsKey("BarCode" + j) && Request.ArgForm["BarCode" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.GoodCodeData == null)
				{
					dataCommand.CheckStrings[j].Register.GoodCodeData = new Unit.DataCommand.Register.tGoodCodeData();
				}
				dataCommand.CheckStrings[j].Register.GoodCodeData.BarCode = Request.ArgForm["BarCode" + j];
			}
			if (Request.ArgForm.ContainsKey("AcceptOnBad" + j))
			{
				if (dataCommand.CheckStrings[j].Register.GoodCodeData == null)
				{
					dataCommand.CheckStrings[j].Register.GoodCodeData = new Unit.DataCommand.Register.tGoodCodeData();
				}
				dataCommand.CheckStrings[j].Register.GoodCodeData.AcceptOnBad = true;
			}
			if (Request.ArgForm.ContainsKey("PackageQuantity" + j))
			{
				if (Request.ArgForm["PackageQuantity" + j].Trim() == "")
				{
					dataCommand.CheckStrings[j].Register.PackageQuantity = null;
				}
				else
				{
					dataCommand.CheckStrings[j].Register.PackageQuantity = uint.Parse(Request.ArgForm["PackageQuantity" + j].Replace(',', '.'), CultureInfo.InvariantCulture);
					if (dataCommand.CheckStrings[j].Register.PackageQuantity == 0)
					{
						dataCommand.CheckStrings[j].Register.PackageQuantity = null;
					}
				}
			}
			if (Request.ArgForm.ContainsKey("AgentSign" + j))
			{
				if (Request.ArgForm["AgentSign" + j] == "")
				{
					dataCommand.CheckStrings[j].Register.AgentSign = null;
				}
				else
				{
					dataCommand.CheckStrings[j].Register.AgentSign = int.Parse(Request.ArgForm["AgentSign" + j]);
				}
			}
			int? agentSign = dataCommand.CheckStrings[j].Register.AgentSign;
			if ((agentSign == 2 || agentSign == 2) && Request.ArgForm.ContainsKey("PayingAgentOperation" + j) && Request.ArgForm["PayingAgentOperation" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.PayingAgentOperation = Request.ArgForm["PayingAgentOperation" + j];
			}
			if ((agentSign == 0 || agentSign == 1) && Request.ArgForm.ContainsKey("PayingAgentPhone" + j) && Request.ArgForm["PayingAgentPhone" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.PayingAgentPhone = Request.ArgForm["PayingAgentPhone" + j];
			}
			if ((agentSign == 2 || agentSign == 2) && Request.ArgForm.ContainsKey("ReceivePaymentsOperatorPhone" + j) && Request.ArgForm["ReceivePaymentsOperatorPhone" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.ReceivePaymentsOperatorPhone = Request.ArgForm["ReceivePaymentsOperatorPhone" + j];
			}
			if ((agentSign == 0 || agentSign == 1) && Request.ArgForm.ContainsKey("MoneyTransferOperatorPhone" + j) && Request.ArgForm["MoneyTransferOperatorPhone" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.MoneyTransferOperatorPhone = Request.ArgForm["MoneyTransferOperatorPhone" + j];
			}
			if ((agentSign == 0 || agentSign == 1) && Request.ArgForm.ContainsKey("MoneyTransferOperatorName" + j) && Request.ArgForm["MoneyTransferOperatorName" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.MoneyTransferOperatorName = Request.ArgForm["MoneyTransferOperatorName" + j];
			}
			if ((agentSign == 0 || agentSign == 1) && Request.ArgForm.ContainsKey("MoneyTransferOperatorAddress" + j) && Request.ArgForm["MoneyTransferOperatorAddress" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.MoneyTransferOperatorAddress = Request.ArgForm["MoneyTransferOperatorAddress" + j];
			}
			if ((agentSign == 0 || agentSign == 1) && Request.ArgForm.ContainsKey("MoneyTransferOperatorVATIN" + j) && Request.ArgForm["MoneyTransferOperatorVATIN" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.AgentData == null)
				{
					dataCommand.CheckStrings[j].Register.AgentData = new Unit.DataCommand.TypeAgentData();
				}
				dataCommand.CheckStrings[j].Register.AgentData.MoneyTransferOperatorVATIN = Request.ArgForm["MoneyTransferOperatorVATIN" + j];
			}
			if (agentSign.HasValue && Request.ArgForm.ContainsKey("PurveyorPhone" + j) && Request.ArgForm["PurveyorPhone" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.PurveyorData == null)
				{
					dataCommand.CheckStrings[j].Register.PurveyorData = new Unit.DataCommand.TypePurveyorData();
				}
				dataCommand.CheckStrings[j].Register.PurveyorData.PurveyorPhone = Request.ArgForm["PurveyorPhone" + j];
			}
			if (agentSign.HasValue && Request.ArgForm.ContainsKey("PurveyorName" + j) && Request.ArgForm["PurveyorName" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.PurveyorData == null)
				{
					dataCommand.CheckStrings[j].Register.PurveyorData = new Unit.DataCommand.TypePurveyorData();
				}
				dataCommand.CheckStrings[j].Register.PurveyorData.PurveyorName = Request.ArgForm["PurveyorName" + j];
			}
			if (agentSign.HasValue && Request.ArgForm.ContainsKey("PurveyorVATIN" + j) && Request.ArgForm["PurveyorVATIN" + j] != "")
			{
				if (dataCommand.CheckStrings[j].Register.PurveyorData == null)
				{
					dataCommand.CheckStrings[j].Register.PurveyorData = new Unit.DataCommand.TypePurveyorData();
				}
				dataCommand.CheckStrings[j].Register.PurveyorData.PurveyorVATIN = Request.ArgForm["PurveyorVATIN" + j];
			}
			if (Request.ArgForm.ContainsKey("CountryOfOrigin" + j))
			{
				dataCommand.CheckStrings[j].Register.CountryOfOrigin = Request.ArgForm["CountryOfOrigin" + j];
			}
			if (Request.ArgForm.ContainsKey("CustomsDeclaration" + j))
			{
				dataCommand.CheckStrings[j].Register.CustomsDeclaration = Request.ArgForm["CustomsDeclaration" + j];
			}
			if (Request.ArgForm.ContainsKey("ExciseAmount" + j))
			{
				if (Request.ArgForm["ExciseAmount" + j].Trim() == "" || Request.ArgForm["ExciseAmount" + j].Trim() == "0")
				{
					dataCommand.CheckStrings[j].Register.ExciseAmount = null;
				}
				else
				{
					dataCommand.CheckStrings[j].Register.ExciseAmount = Math.Round(decimal.Parse(Request.ArgForm["ExciseAmount" + j].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
				}
			}
			if (Request.ArgForm.ContainsKey("AdditionalAttribute" + j))
			{
				dataCommand.CheckStrings[j].Register.AdditionalAttribute = Request.ArgForm["AdditionalAttribute" + j];
			}
		}
		if (Request.ArgForm["Cash"].Trim() == "")
		{
			Request.ArgForm["Cash"] = "0";
		}
		dataCommand.Cash = Math.Round(decimal.Parse(Request.ArgForm["Cash"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		if (Request.ArgForm.ContainsKey("ElectronicPayment"))
		{
			if (Request.ArgForm["ElectronicPayment"].Trim() == "")
			{
				Request.ArgForm["ElectronicPayment"] = "0";
			}
			dataCommand.ElectronicPayment = Math.Round(decimal.Parse(Request.ArgForm["ElectronicPayment"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		if (Request.ArgForm.ContainsKey("AdvancePayment"))
		{
			if (Request.ArgForm["AdvancePayment"].Trim() == "")
			{
				Request.ArgForm["AdvancePayment"] = "0";
			}
			dataCommand.AdvancePayment = Math.Round(decimal.Parse(Request.ArgForm["AdvancePayment"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		if (Request.ArgForm.ContainsKey("Credit"))
		{
			if (Request.ArgForm["Credit"].Trim() == "")
			{
				Request.ArgForm["Credit"] = "0";
			}
			dataCommand.Credit = Math.Round(decimal.Parse(Request.ArgForm["Credit"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		if (Request.ArgForm.ContainsKey("CashProvision"))
		{
			if (Request.ArgForm["CashProvision"].Trim() == "")
			{
				Request.ArgForm["CashProvision"] = "0";
			}
			dataCommand.CashProvision = Math.Round(decimal.Parse(Request.ArgForm["CashProvision"].Replace(',', '.'), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
		}
		return dataCommand;
	}

	private async Task<HttpResponse> GetDataCheck(HttpRequest Request)
	{
		string text = Request.httpArg[1];
		bool flag = true;
		Unit unit = null;
		StringBuilder stringBuilder = HttpService.RootHtml("", AddUnitTestJS: true);
		stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(stringBuilder, "GetDataCheck", Request.httpArg[1]);
		HttpService.UnitsMenu(stringBuilder, "GetDataCheck", Request.httpArg[1], AddUnit: false, UnitPassword);
		try
		{
			int.Parse(text);
			unit = Global.UnitManager.Units[int.Parse(text)];
		}
		catch
		{
			flag = false;
		}
		StringBuilder bodyRazdel;
		if (flag)
		{
			_ = unit.SettDr;
			_ = unit.SettDr.TypeDevice.Protocol;
			TypeDevice.enType type = unit.SettDr.TypeDevice.Type;
			new StringBuilder("&ТелоСкрипта");
			int num = (int)type;
			num.ToString();
			string text2 = ((unit == null || unit.NameDevice == "") ? "" : (", " + unit.NameDevice));
			bodyRazdel = HttpService.GetBodyRazdel("Получить данные чека" + text2);
			HttpService.GetInputBox(bodyRazdel, "Номер чека (ФД):", "FiscalNumber", "");
			HttpService.GetInputBox(bodyRazdel, "Печатать на устройстве:", "PrintCheck", "false", "checkbox");
			bodyRazdel.Replace("&ТелоСтраницы", "<div class=\"InputValue\">\r\n                        <div align=\"right\" class=\"Caption\">Получить данные чека:</div>\r\n                        <div class=\"input\"><input class='Button' value='Данные чека' type='submit' onclick='GetDataCheck(" + text + ", document.getElementById(\"FiscalNumber\").value, document.getElementById(\"PrintCheck\").checked)'/></div>\r\n                    </div>&ТелоСтраницы");
			HttpService.GetLine(bodyRazdel);
			HttpService.GetText(bodyRazdel, "Ошибка :", "", "Red", "", "MessageError", "style='display: none;'", "style='white-space:pre;'");
			HttpService.GetPre(bodyRazdel, "Квитанция/Чек/Slip :", "", "MessageSlip", Visibility: false);
		}
		else
		{
			bodyRazdel = HttpService.GetBodyRazdel("Выберите устройство");
			HttpService.GetChangeForDevice(bodyRazdel, "Выберите устройство:");
		}
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}

	private async Task<HttpResponse> GetGoodCodeData(HttpRequest Request)
	{
		StringBuilder stringBuilder = HttpService.RootHtml("", AddUnitTestJS: true);
		stringBuilder.Replace("&ДополнительныеСкрипты", "<script type=\"text/javascript\" id=\"InitKkmserver\">\r\n                &ДополнительныеСкрипты");
		HttpService.CommandMenu(stringBuilder, "GetGoodCodeData", Request.httpArg[1]);
		stringBuilder.Replace("&СписокУстройств", "");
		StringBuilder bodyRazdel = HttpService.GetBodyRazdel("Проверка ШК кода маркировки товара");
		HttpService.GetInputBox(bodyRazdel, "Штрих код:", "BarCode", "");
		HttpService.GetHelpBox(bodyRazdel, "", "Штрих-код маркировки товара со сканера<br/>\r\n                  (нужно настроить сканер так чтобы не проглатывал управляющие символы)<br/>\r\n                  Поддерживаются ШК:<br/> \r\n                  - Без идентификатора экземпляра товара: EAN8, EAN13, EAN14<br/>\r\n                  - С идентификатором экземпляра товара: GS1, ШК шуб, ШК табачной продукции., ЕГАИС-2, ЕГАИС-3");
		HttpService.GetHeadetBox(bodyRazdel, "", "<h5>\r\n                        <input class='Button' value='Проверить ШК' type='submit' onclick='GetGoodCodeData(document.getElementById(\"BarCode\").value)' />\r\n                    </h5>", Any: true);
		HttpService.GetLine(bodyRazdel);
		HttpService.GetText(bodyRazdel, "Ошибка :", "", "Red", "", "MessageError", "style='display: none;'", "style='white-space:pre;'");
		HttpService.GetText(bodyRazdel, "Предупреждение :", "", "", "", "MessageWarning", "style='display: none;'", "style='white-space:pre;'");
		HttpService.GetText(bodyRazdel, "JSON ответа :", "", "", "", "MessageReturn", "style='display: none;'", "style='white-space:pre;'");
		stringBuilder.Replace("&ТелоСтраницы", bodyRazdel.ToString());
		stringBuilder.Replace("&ТелоСтраницы", "");
		stringBuilder.Replace("&ДополнительныеСкрипты", "</script>");
		return new HttpResponse(Request, HttpStatusCode.OK, stringBuilder.ToString());
	}
}
