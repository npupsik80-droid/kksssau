using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace KkmFactory;

internal class KktEmulator : Unit
{
	public string AccessPassword = "30";

	public string OperatorPasswor = "31";

	private Encoding Win1251 = Encoding.GetEncoding(1251);

	public SortedList<int, byte> Nalogs = new SortedList<int, byte>();

	public Dictionary<int, int> NumberTypeTable = new Dictionary<int, int>();

	private int NumLineCashier;

	private decimal BalanceCash;

	private string CashierName = "";

	private string CashierVATIN = "";

	private string SenderEmail = "";

	private int NumDoc;

	private int NumSes = 1;

	private int NumDocSes = 1;

	private int NumFPD;

	private decimal oDepositingCash;

	private decimal oPaymentCash;

	private decimal oProdCash;

	private decimal oVozvCash;

	private decimal oKorProdCash;

	private decimal oKorVozvCash;

	private decimal oProd;

	private decimal oVozvProd;

	private decimal oPok;

	private decimal oVozvPok;

	private List<Dictionary<int, string>> ListRegisterCheck = new List<Dictionary<int, string>>();

	public KktEmulator(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		Kkm.IsKKT = true;
		LicenseFlags = ComDevice.PaymentOption.None;
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		NameDevice = "Эмулятор ККТ 54-фз";
		UnitName = "Эмулятор ККТ 54-фз";
		UnitDescription = "Эмулятор предназначен для отладки регистрации чеков на ККТ по 54-фз\r\n                                Внимание: нет вывода на экран при запуске сервера как \"Windows-служба (Сервисом)\"";
		UnitEquipmentType = "ФискальныйРегистратор";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kkmserver.ru";
		string paramsXML = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Информация\">\r\n            <Parameter Name=\"Name\" Caption=\"Имя устройства\" TypeValue=\"String\" DefaultValue=\"Эмулятор ККТ №1\" />\r\n        </Group>\r\n        <Group Caption=\"Tипы оплат\">\r\n            <Parameter Name=\"LessType1\" Caption=\"Тип безнал. оплаты 1\" TypeValue=\"String\" DefaultValue=\"ПЛАТ.КАРТОЙ\" /> \r\n            <Parameter Name=\"LessType2\" Caption=\"Тип безнал. оплаты 2\" TypeValue=\"String\" DefaultValue=\"БАНК.КРЕДИТОМ\" />\r\n            <Parameter Name=\"LessType3\" Caption=\"Тип безнал. оплаты 3\" TypeValue=\"String\" DefaultValue=\"ПРОЧЕЕ\" />  \r\n        </Group>\r\n        <Group Caption=\"Параметры Организации\">\r\n            <Parameter Name=\"NameOrganization\" Caption=\"Организация\" TypeValue=\"String\" DefaultValue=\"ООО &quot;Рога и Копыта&quot;\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"InnOrganization\" Caption=\"ИНН\" TypeValue=\"String\" DefaultValue=\"7701237658\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"AddressSettle\" Caption=\"Адрес установки\" TypeValue=\"String\" DefaultValue=\"г. Сочи, переулок &quot;Два Карла&quot;\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"PlaceSettle\" Caption=\"Место установки\" TypeValue=\"String\" DefaultValue=\"Подвал &quot;Контрабандный&quot;\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"SenderEmail\" Caption=\"Email магазина\" TypeValue=\"String\" DefaultValue=\"odessa@Mother.ru\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"TaxVariant\" Caption=\"Система налогообложения\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"SignOfAgent\" Caption=\"Применяемые коды агента\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n        </Group>\r\n        <Group Caption=\"Параметры ККТ\">\r\n            <Parameter Name=\"FfdVersion\" Caption=\"FfdVersion\" TypeValue=\"String\" DefaultValue=\"1.05\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"RegNumber\" Caption=\"Регистрационный №\" TypeValue=\"String\" DefaultValue=\"0000000003056478\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"FacNumber\" Caption=\"Заводской №\" TypeValue=\"String\" DefaultValue=\"0149060506089651\"/>\r\n            <Parameter Name=\"FnNumber\" Caption=\"Номер ФН\" TypeValue=\"String\" DefaultValue=\"0149060506089651\"/>\r\n            <Parameter Name=\"StatusFN\" Caption=\"Статус ФН\" TypeValue=\"String\" DefaultValue=\"Настройка ФН\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"StatusККТ\" Caption=\"Статусы ККТ\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"Session\" Caption=\"Дата окончания смены\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n        </Group>\r\n        <Group Caption=\"Параметры ОФД\">\r\n            <Parameter Name=\"UrlServerOfd\" Caption=\"URL или IP сервера ОФД\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"PortServerOfd\" Caption=\"IP-порт сервера ОФД\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"NameOFD\" Caption=\"Наименование ОФД\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"UrlOfd\" Caption=\"URL ОФД для поиска чека\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n            <Parameter Name=\"InnOfd\" Caption=\"ИНН ОФД\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		LoadParametsFromXML(paramsXML);
		UnitName = "KKT Эмулятор";
		NameDevice = "KKT Эмулятор";
		string paramsXML2 = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                <Actions>\r\n                    <Action Name=\"ClearFN\" Caption=\"Тестовый сброс ФН\"/> \r\n                </Actions>";
		LoadAdditionalActionsFromXML(paramsXML2);
		Kkm.PrintingWidth = 40;
	}

	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		Kkm.INN = UnitParamets["InnOrganization"];
		Kkm.Organization = UnitParamets["NameOrganization"];
		Kkm.NumberKkm = UnitParamets["FacNumber"];
		Kkm.PaperOver = false;
		Kkm.PrintingWidth = 48;
		switch (UnitParamets["FfdVersion"])
		{
		case "1.0":
			Kkm.FfdVersion = 1;
			break;
		case "1.05":
			Kkm.FfdVersion = 2;
			break;
		case "1.1":
			Kkm.FfdVersion = 3;
			break;
		}
		Kkm.FfdSupportVersion = 2;
		Kkm.FfdMinimumVersion = 1;
		Kkm.Fn_Number = UnitParamets["FnNumber"];
		if (UnitParamets["Session"] == "")
		{
			SessionOpen = 1;
		}
		else if (DateTime.Parse(UnitParamets["Session"]) > DateTime.Now)
		{
			SessionOpen = 2;
		}
		else
		{
			SessionOpen = 3;
		}
		await ReadStatusOFD(FullInit);
		IsInit = true;
		return true;
	}

	public override void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		if (DataCommand.AdditionalActions == "ClearFN")
		{
			UnitParamets["TaxVariant"] = "";
			UnitParamets["Session"] = "";
			UnitParamets["StatusFN"] = "Настройка ФН";
			UnitParamets["StatusККТ"] = "";
			Global.SaveSettingsAsync().Wait();
			SessionOpen = 1;
		}
		base.DoAdditionalAction(DataCommand, ref RezultCommand);
		SaveParametrs();
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task RegisterCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		bool PrintCopy = false;
		decimal AllSumm;
		DateTime now;
		while (true)
		{
			bool ClearText = true;
			if (PrintCopy)
			{
				ClearText = false;
			}
			if (DataCommand.NotClearText)
			{
				ClearText = false;
			}
			await ProcessInitDevice();
			if (DataCommand.IsFiscalCheck && Kkm.FN_Status != 3)
			{
				CreateTextError(2, "Ошибка", RezultCommand);
				return;
			}
			if (DataCommand.IsFiscalCheck && SessionOpen == 1)
			{
				await OpenShift(DataCommand, RezultCommand);
				RezultCommand.Status = ExecuteStatus.Run;
				await ProcessInitDevice();
				ClearText = false;
			}
			if (DataCommand.IsFiscalCheck && SessionOpen != 2)
			{
				CreateTextError(61, "Ошибка регистрации", RezultCommand);
				return;
			}
			CashierName = "";
			CashierVATIN = "";
			if (DataCommand.CashierName != null && DataCommand.CashierName != "")
			{
				CashierName = DataCommand.CashierName;
				CashierVATIN = DataCommand.CashierVATIN;
			}
			SenderEmail = UnitParamets["SenderEmail"];
			if (DataCommand.SenderEmail != null && DataCommand.SenderEmail != "")
			{
				SenderEmail = DataCommand.SenderEmail;
			}
			PrintHead(ClearText);
			bool IsReturn = false;
			if (DataCommand.IsFiscalCheck)
			{
				if (DataCommand.TypeCheck == 0)
				{
					WriteLine(">#4#<Чек прихода", 1, Clear: false, null, AsCheck: true);
				}
				else if (DataCommand.TypeCheck == 1)
				{
					WriteLine(">#4#<Чек возврата прихода", 1, Clear: false, null, AsCheck: true);
					IsReturn = true;
				}
				else if (DataCommand.TypeCheck == 2)
				{
					WriteLine(">#4#<Чек корректировки прихода", 1, Clear: false, null, AsCheck: true);
				}
				else if (DataCommand.TypeCheck == 3)
				{
					WriteLine(">#4#<Чек корректировки возврата прихода", 2, Clear: false, null, AsCheck: true);
				}
				else if (DataCommand.TypeCheck == 10)
				{
					WriteLine(">#4#<Чек расхода", 2, Clear: false, null, AsCheck: true);
					IsReturn = true;
				}
				else if (DataCommand.TypeCheck == 11)
				{
					WriteLine(">#4#<Чек возврата расхода", 1, Clear: false, null, AsCheck: true);
				}
				else if (DataCommand.TypeCheck == 12)
				{
					WriteLine(">#4#<Чек корректировки расхода", 1, Clear: false, null, AsCheck: true);
				}
				else
				{
					if (DataCommand.TypeCheck != 13)
					{
						Error = "Команда не поддерживается оборудованием";
						return;
					}
					WriteLine(">#4#<Чек корректировки возврата расхода", 2, Clear: false, null, AsCheck: true);
				}
			}
			else
			{
				WriteLine("", 1, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.IsFiscalCheck)
			{
				PrintHead1();
				if (Error != "")
				{
					return;
				}
			}
			if (DataCommand.IsFiscalCheck)
			{
				if (DataCommand.PlaceMarket != null && DataCommand.PlaceMarket != "")
				{
					WriteLine("Место расчетов:<#0#>" + DataCommand.PlaceMarket, 4, Clear: false, null, AsCheck: true);
				}
				else
				{
					WriteLine("Место расчетов:<#0#>" + UnitParamets["PlaceSettle"], 4, Clear: false, null, AsCheck: true);
				}
				if (DataCommand.SenderEmail != null && DataCommand.SenderEmail != "")
				{
					WriteLine("Эл.адрес отправителя:<#0#>" + DataCommand.SenderEmail, 4, Clear: false, null, AsCheck: true);
				}
				else if (UnitParamets["SenderEmail"] != null && UnitParamets["SenderEmail"] != "")
				{
					WriteLine("Эл.адрес отправителя:<#0#>" + UnitParamets["SenderEmail"], 4, Clear: false, null, AsCheck: true);
				}
				if (DataCommand.ClientAddress != null && DataCommand.ClientAddress != "")
				{
					WriteLine("Тел/емайл покупателя:<#0#>" + DataCommand.ClientAddress, 4, Clear: false, null, AsCheck: true);
				}
			}
			if (DataCommand.IsFiscalCheck && DataCommand.CheckProps != null)
			{
				DataCommand.CheckProp[] checkProps = DataCommand.CheckProps;
				foreach (DataCommand.CheckProp checkProp in checkProps)
				{
					if (checkProp.PrintInHeader)
					{
						WriteLine(checkProp.Teg + ":<#0#>" + checkProp.Prop.ToString(), 0, Clear: false, null, AsCheck: true);
					}
				}
			}
			if (DataCommand.IsFiscalCheck && DataCommand.AdditionalProps != null)
			{
				DataCommand.AdditionalProp[] additionalProps = DataCommand.AdditionalProps;
				foreach (DataCommand.AdditionalProp additionalProp in additionalProps)
				{
					if (additionalProp.PrintInHeader && additionalProp.Print && !DataCommand.NotPrint.Value)
					{
						WriteLine(additionalProp.NameProp.ToString() + ":<#4#>" + additionalProp.Prop.ToString(), 0, Clear: false, null, AsCheck: true);
					}
				}
			}
			if (DataCommand.AgentSign.HasValue)
			{
				PrintAgentData(DataCommand.AgentSign, DataCommand.AgentData, DataCommand.PurveyorData);
				if (Error != "")
				{
					return;
				}
			}
			if (DataCommand.ClientInfo != null && DataCommand.ClientInfo != "")
			{
				WriteLine("Покупатель:<#0#>" + DataCommand.ClientInfo, 3, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.ClientINN != null && DataCommand.ClientINN != "")
			{
				WriteLine("ИНН покупателя:<#0#>" + DataCommand.ClientINN, 3, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.IsFiscalCheck)
			{
				WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.CorrectionType == 0)
			{
				WriteLine("Тип чека:<#0#>Самостоятельно", 0, Clear: false, null, AsCheck: true);
			}
			else if (DataCommand.CorrectionType == 0)
			{
				WriteLine("Тип чека:<#0#>По предписанию", 0, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.CorrectionBaseDate.HasValue && DataCommand.CorrectionBaseDate != default(DateTime))
			{
				WriteLine("Дата документа:<#0#>" + DataCommand.CorrectionBaseDate.ToString(), 0, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.CorrectionBaseNumber != null && DataCommand.CorrectionBaseNumber != "")
			{
				WriteLine("Номер документа:<#0#>" + DataCommand.CorrectionBaseNumber, 0, Clear: false, null, AsCheck: true);
			}
			bool flag = false;
			if (DataCommand.AdditionalAttribute != null && DataCommand.AdditionalAttribute != "")
			{
				WriteLine("Доп.реквизит:<#0#>" + DataCommand.AdditionalAttribute, 3, Clear: false, null, AsCheck: true);
				flag = true;
			}
			if (DataCommand.UserAttribute != null)
			{
				string text = "Реквизит пользователя:";
				if (DataCommand.UserAttribute.Name != null && DataCommand.UserAttribute.Name != "")
				{
					text = DataCommand.UserAttribute.Name;
				}
				string text2 = "<Не задано>:";
				if (DataCommand.UserAttribute.Value != null && DataCommand.UserAttribute.Value != "")
				{
					text2 = DataCommand.UserAttribute.Value;
				}
				WriteLine(text + ":<#0#>" + text2, 3, Clear: false, null, AsCheck: true);
				flag = true;
			}
			if (DataCommand.IsFiscalCheck && flag)
			{
				WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
			}
			AllSumm = default(decimal);
			if (DataCommand.KeySubLicensing != null && DataCommand.KeySubLicensing != "")
			{
				await ComDevice.PostCheck(DataCommand, this);
			}
			DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
			foreach (DataCommand.CheckString checkString in checkStrings)
			{
				if (DataCommand.NotPrint == false && checkString != null && checkString.PrintImage != null)
				{
					PrintImage(checkString.PrintImage);
				}
				if (DataCommand.NotPrint == false && checkString != null && checkString.PrintText != null)
				{
					WriteLine(checkString.PrintText.Text, checkString.PrintText.Font, Clear: false, null, AsCheck: true);
				}
				if (DataCommand.IsFiscalCheck && checkString != null && checkString.Register != null && checkString.Register.Quantity != 0m)
				{
					if (checkString.Register.Amount < 0m || checkString.Register.Price < 0m || checkString.Register.Quantity < 0m)
					{
						Error = "Некоректная сумма или количество в фискальной строке";
						RezultCommand.Status = ExecuteStatus.Error;
						return;
					}
					checkString.Register.Amount = Math.Abs(checkString.Register.Amount);
					checkString.Register.Price = Math.Abs(checkString.Register.Price);
					AllSumm += checkString.Register.Amount;
					foreach (DataCommand.Register item in SplitRegisterString(checkString))
					{
						string text3 = "";
						if (checkString.Register.GoodCodeData != null)
						{
							text3 = " [M]";
						}
						WriteLine(checkString.Register.Name + text3 + ":<#0#>" + item.Quantity + " X " + item.Price, 0, Clear: false, null, AsCheck: true);
						if (item.StSkidka != "")
						{
							WriteLine(item.StSkidka, 0, Clear: false, null, AsCheck: true);
						}
						WriteLine("Итого:<#0#>= " + item.Amount, 0, Clear: false, null, AsCheck: true);
						string text4 = "";
						decimal num = default(decimal);
						decimal tax = checkString.Register.Tax;
						if (tax <= 20m)
						{
							if (tax <= 5m)
							{
								if (!(tax == -1m))
								{
									if (!(tax == 0m))
									{
										if (tax == 5m)
										{
											text4 = "НДС 5%";
											num = Math.Round(item.Amount / 105m * 5m, 2, MidpointRounding.AwayFromZero);
										}
									}
									else
									{
										text4 = "НДС 0%";
									}
								}
								else
								{
									text4 = "Без НДС";
								}
							}
							else if (tax <= 10m)
							{
								if (!(tax == 7m))
								{
									if (tax == 10m)
									{
										text4 = "НДС 10%";
										num = Math.Round(item.Amount / 110m * 10m, 2, MidpointRounding.AwayFromZero);
									}
								}
								else
								{
									text4 = "НДС 7%";
									num = Math.Round(item.Amount / 107m * 7m, 2, MidpointRounding.AwayFromZero);
								}
							}
							else if (!(tax == 18m))
							{
								if (tax == 20m)
								{
									text4 = "НДС 20%";
									num = Math.Round(item.Amount / 120m * 20m, 2, MidpointRounding.AwayFromZero);
								}
							}
							else
							{
								text4 = "НДС 18%";
								num = Math.Round(item.Amount / 118m * 18m, 2, MidpointRounding.AwayFromZero);
							}
						}
						else if (tax <= 107m)
						{
							if (!(tax == 22m))
							{
								if (!(tax == 105m))
								{
									if (tax == 107m)
									{
										text4 = "НДС 7/107%";
										num = Math.Round(item.Amount / 107m * 7m, 2, MidpointRounding.AwayFromZero);
									}
								}
								else
								{
									text4 = "НДС 5/105%";
									num = Math.Round(item.Amount / 105m * 5m, 2, MidpointRounding.AwayFromZero);
								}
							}
							else
							{
								text4 = "НДС 22%";
								num = Math.Round(item.Amount / 122m * 22m, 2, MidpointRounding.AwayFromZero);
							}
						}
						else if (tax <= 118m)
						{
							if (!(tax == 110m))
							{
								if (tax == 118m)
								{
									text4 = "НДС 18/118%";
									num = Math.Round(item.Amount / 118m * 18m, 2, MidpointRounding.AwayFromZero);
								}
							}
							else
							{
								text4 = "НДС 10/110%";
								num = Math.Round(item.Amount / 110m * 10m, 2, MidpointRounding.AwayFromZero);
							}
						}
						else if (!(tax == 120m))
						{
							if (tax == 122m)
							{
								text4 = "НДС 22/122%";
								num = Math.Round(item.Amount / 122m * 22m, 2, MidpointRounding.AwayFromZero);
							}
						}
						else
						{
							text4 = "НДС 20/120%";
							num = Math.Round(item.Amount / 120m * 20m, 2, MidpointRounding.AwayFromZero);
						}
						WriteLine(text4 + "<#0#> " + num, 0, Clear: false, null, AsCheck: true);
						if (Kkm.FfdVersion >= 2)
						{
							if (!checkString.Register.SignCalculationObject.HasValue || checkString.Register.SignCalculationObject == 0)
							{
								CreateTextError(105, "Ошибка регистрации фискальной строки", RezultCommand);
								return;
							}
							string text5 = "";
							switch (checkString.Register.SignCalculationObject)
							{
							case 1:
								text5 = "ТОВАР";
								break;
							case 2:
								text5 = "ПОДАКЦИЗНЫЙ ТОВАР";
								break;
							case 3:
								text5 = "РАБОТА";
								break;
							case 4:
								text5 = "УСЛУГА";
								break;
							case 5:
								text5 = "СТАВКА АЗАРТНОЙ ИГРЫ";
								break;
							case 6:
								text5 = "ВЫИГРЫШ АЗАРТНОЙ ИГРЫ";
								break;
							case 7:
								text5 = "ЛОТЕРЕЙНЫЙ БИЛЕТ";
								break;
							case 8:
								text5 = "ВЫИГРЫШ ЛОТЕРЕИ";
								break;
							case 9:
								text5 = "ПРЕДОСТАВЛЕНИЕ РИД";
								break;
							case 10:
								text5 = "ПЛАТЕЖ";
								break;
							case 11:
								text5 = "АГЕНТСКОЕ ВОЗНАГРАЖДЕНИЕ";
								break;
							case 12:
								text5 = "СОСТАВНОЙ ПРЕДМЕТ РАСЧЕТА";
								break;
							case 13:
								text5 = "ИНОЙ ПРЕДМЕТ РАСЧЕТА";
								break;
							case 14:
								text5 = "ИМУЩЕСТВЕННОЕ ПРАВО";
								break;
							case 15:
								text5 = "ВНЕРЕАЛИЗАЦИОННЫЙ ДОХОД";
								break;
							case 16:
								text5 = "СТРАХОВЫЕ ВЗНОСЫ";
								break;
							case 17:
								text5 = "ТОРГОВЫЙ СБОР";
								break;
							case 18:
								text5 = "КУРОРТНЫЙ СБОР";
								break;
							case 19:
								text5 = "ЗАЛОГ";
								break;
							}
							WriteLine("Предмет расчета:<#0#>" + text5, 0, Clear: false, null, AsCheck: true);
							if (!checkString.Register.SignMethodCalculation.HasValue || checkString.Register.SignMethodCalculation == 0)
							{
								CreateTextError(106, "Ошибка регистрации фискальной строки", RezultCommand);
								return;
							}
							string text6 = "";
							switch (checkString.Register.SignMethodCalculation)
							{
							case 1:
								text6 = "ПРЕДОПЛАТА 100 %";
								break;
							case 2:
								text6 = "ПРЕДОПЛАТА";
								break;
							case 3:
								text6 = "АВАНС";
								break;
							case 4:
								text6 = "ПОЛНЫЙ РАСЧЕТ";
								break;
							case 5:
								text6 = "ЧАСТИЧНЫЙ РАСЧЕТ И КРЕДИТ";
								break;
							case 6:
								text6 = "ПЕРЕДАЧА В КРЕДИТ";
								break;
							case 7:
								text6 = "ОПЛАТА КРЕДИТА";
								break;
							}
							WriteLine("Cпособ расчета:<#0#>" + text6, 0, Clear: false, null, AsCheck: true);
						}
						if (checkString.Register.AgentSign.HasValue && Kkm.FfdVersion >= 2)
						{
							PrintAgentData(checkString.Register.AgentSign, checkString.Register.AgentData, checkString.Register.PurveyorData);
							if (Error != "")
							{
								return;
							}
						}
						if (checkString.Register.CountryOfOrigin != null && checkString.Register.CountryOfOrigin != "")
						{
							WriteLine("Код страны производителя:<#0#>" + checkString.Register.CountryOfOrigin, 3, Clear: false, null, AsCheck: true);
						}
						if (checkString.Register.CustomsDeclaration != null && checkString.Register.CustomsDeclaration != "")
						{
							WriteLine("ГТД:<#0#>" + checkString.Register.CustomsDeclaration, 3, Clear: false, null, AsCheck: true);
						}
						if (checkString.Register.ExciseAmount.HasValue)
						{
							WriteLine("Сумма акциза:<#0#>" + checkString.Register.ExciseAmount, 3, Clear: false, null, AsCheck: true);
						}
						if (checkString.Register.AdditionalAttribute != null && checkString.Register.AdditionalAttribute != "")
						{
							WriteLine("Доп.реквизит:<#0#>" + checkString.Register.AdditionalAttribute, 3, Clear: false, null, AsCheck: true);
						}
					}
				}
				if (DataCommand.NotPrint == false && checkString != null && checkString.BarCode != null && checkString.BarCode.BarcodeType != "" && !PrintBarCode(checkString.BarCode))
				{
					return;
				}
			}
			if (DataCommand.IsFiscalCheck && DataCommand.CheckProps != null)
			{
				DataCommand.CheckProp[] checkProps = DataCommand.CheckProps;
				foreach (DataCommand.CheckProp checkProp2 in checkProps)
				{
					if (!checkProp2.PrintInHeader)
					{
						WriteLine(checkProp2.Teg + ":<#0#>" + checkProp2.Prop.ToString(), 0, Clear: false, null, AsCheck: true);
					}
				}
			}
			if (DataCommand.IsFiscalCheck && DataCommand.AdditionalProps != null)
			{
				DataCommand.AdditionalProp[] additionalProps = DataCommand.AdditionalProps;
				foreach (DataCommand.AdditionalProp additionalProp2 in additionalProps)
				{
					if (!additionalProp2.PrintInHeader && additionalProp2.Print && !DataCommand.NotPrint.Value)
					{
						WriteLine(additionalProp2.NameProp.ToString() + ":<#0#>" + additionalProp2.Prop.ToString(), 0, Clear: false, null, AsCheck: true);
					}
				}
			}
			if (DataCommand.TaxVariant != "" && DataCommand.TaxVariant != null)
			{
				string text7 = "";
				string text8 = DataCommand.TaxVariant.Trim();
				if (text8 == "")
				{
					string[] array = UnitParamets["TaxVariant"].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					if (array.Length != 0)
					{
						text8 = array[0].Trim();
					}
				}
				switch (text8)
				{
				case "0":
					text7 = "Основная (ОСН)";
					break;
				case "1":
					text7 = "УСН (Доход)";
					break;
				case "2":
					text7 = "УСН (Доход-Расход)";
					break;
				case "3":
					text7 = "ЕНВД";
					break;
				case "4":
					text7 = "Сельскохозяйственный налог";
					break;
				case "5":
					text7 = "Патент";
					break;
				}
				WriteLine("Применяемая СН:<#0#> " + text7, 0, Clear: false, null, AsCheck: true);
			}
			if (DataCommand.IsFiscalCheck)
			{
				WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
			}
			DataCommand.Amount = Math.Abs(DataCommand.Amount);
			DataCommand.Cash = Math.Abs(DataCommand.Cash);
			DataCommand.ElectronicPayment = Math.Abs(DataCommand.ElectronicPayment);
			DataCommand.AdvancePayment = Math.Abs(DataCommand.AdvancePayment);
			DataCommand.Credit = Math.Abs(DataCommand.Credit);
			DataCommand.CashProvision = Math.Abs(DataCommand.CashProvision);
			DataCommand.CashLessType1 = Math.Abs(DataCommand.CashLessType1);
			DataCommand.CashLessType2 = Math.Abs(DataCommand.CashLessType2);
			DataCommand.CashLessType3 = Math.Abs(DataCommand.CashLessType3);
			decimal num2 = default(decimal);
			decimal num3 = default(decimal);
			if (DataCommand.IsFiscalCheck && Kkm.FfdVersion >= 2)
			{
				WriteLine("ИТОГ:<#0#>" + AllSumm, 2, Clear: false, null, AsCheck: true);
				if (DataCommand.Cash != 0m)
				{
					WriteLine("Наличными:<#16#>>" + DataCommand.Cash, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.Cash;
				}
				if (DataCommand.ElectronicPayment != 0m)
				{
					WriteLine("Безналичными:<#16#>>" + DataCommand.ElectronicPayment, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.ElectronicPayment;
				}
				if (DataCommand.AdvancePayment != 0m)
				{
					WriteLine("Зачет аванса:<#16#>>" + DataCommand.AdvancePayment, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.AdvancePayment;
				}
				if (DataCommand.Credit != 0m)
				{
					WriteLine("Кредит:<#16#>>" + DataCommand.Credit, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.Credit;
				}
				if (DataCommand.CashProvision != 0m)
				{
					WriteLine("Встречное представление:<#16#>>" + DataCommand.CashProvision, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashProvision;
				}
				if (DataCommand.CashLessType1 != 0m)
				{
					WriteLine(UnitParamets["LessType1"] + ":<#16#>>" + DataCommand.CashLessType1, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashLessType1;
				}
				if (DataCommand.CashLessType2 != 0m)
				{
					WriteLine(UnitParamets["LessType2"] + ":<#16#>>" + DataCommand.CashLessType2, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashLessType2;
				}
				if (DataCommand.CashLessType3 != 0m)
				{
					WriteLine(UnitParamets["LessType3"] + ":<#16#>>" + DataCommand.CashLessType3, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashLessType3;
				}
			}
			if (DataCommand.IsFiscalCheck && Kkm.FfdVersion == 1)
			{
				WriteLine("ИТОГ:<#0#>" + AllSumm, 2, Clear: false, null, AsCheck: true);
				if (DataCommand.Cash != 0m)
				{
					WriteLine("Наличными:<#16#>>" + DataCommand.Cash, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.Cash;
				}
				if (DataCommand.ElectronicPayment != 0m)
				{
					WriteLine("Безналичными:<#16#>>" + DataCommand.ElectronicPayment, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.ElectronicPayment;
				}
				if (DataCommand.AdvancePayment != 0m)
				{
					WriteLine("Зачет аванса:<#16#>>" + DataCommand.AdvancePayment, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.AdvancePayment;
				}
				if (DataCommand.Credit != 0m)
				{
					WriteLine("Кредит:<#16#>>" + DataCommand.Credit, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.Credit;
				}
				if (DataCommand.CashProvision != 0m)
				{
					WriteLine("Встречное представление:<#16#>>" + DataCommand.CashProvision, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashProvision;
				}
				if (DataCommand.CashLessType1 != 0m)
				{
					WriteLine(UnitParamets["LessType1"] + ":<#16#>>" + DataCommand.CashLessType1, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashLessType1;
				}
				if (DataCommand.CashLessType2 != 0m)
				{
					WriteLine(UnitParamets["LessType2"] + ":<#16#>>" + DataCommand.CashLessType2, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashLessType2;
				}
				if (DataCommand.CashLessType3 != 0m)
				{
					WriteLine(UnitParamets["LessType3"] + ":<#16#>>" + DataCommand.CashLessType3, 0, Clear: false, null, AsCheck: true);
					num2 += DataCommand.CashLessType3;
				}
			}
			if (DataCommand.IsFiscalCheck)
			{
				if (num2 < AllSumm)
				{
					CreateTextError(69, "Ошибка регистрации чека", RezultCommand);
					WriteLine(">#0#<Чек аннулирован", 1, Clear: false, null, AsCheck: true);
					return;
				}
				if (num2 - DataCommand.Cash > AllSumm)
				{
					CreateTextError(77, "Ошибка регистрации чека", RezultCommand);
					WriteLine(">#0#<Чек аннулирован", 1, Clear: false, null, AsCheck: true);
					return;
				}
				num3 = num2 - AllSumm;
				if (IsReturn && DataCommand.Cash - num3 > BalanceCash)
				{
					CreateTextError(70, "Ошибка регистрации чека", RezultCommand);
					WriteLine(">#0#<Чек аннулирован", 1, Clear: false, null, AsCheck: true);
					return;
				}
				if (num3 != 0m)
				{
					WriteLine("Сдача:<#16#>>" + num3, 0, Clear: false, null, AsCheck: true);
				}
			}
			now = DateTime.Now;
			if (DataCommand.IsFiscalCheck)
			{
				NumDoc++;
				NumDocSes++;
				Random random = new Random();
				NumFPD = random.Next(0, int.MaxValue);
				RezultCommand.QRCode = "t=" + DateTime.Now.ToString("yyyyMMddTHHmm") + "&s=" + AllSumm.ToString("0.00").Replace(',', '.') + "&fn=" + Kkm.Fn_Number + "&i=" + NumDoc.ToString("D0") + "&fp=" + NumFPD.ToString("D0") + "&n=1";
				RezultCommand.CheckNumber = NumDoc;
				RezultCommand.SessionNumber = NumSes;
				RezultCommand.SessionCheckNumber = NumDocSes;
			}
			else
			{
				RezultCommand.QRCode = "";
			}
			if (DataCommand.IsFiscalCheck)
			{
				PrintFN(RezultCommand.QRCode);
			}
			PrintFooter();
			if (DataCommand.IsFiscalCheck && !PrintCopy)
			{
				if (DataCommand.TypeCheck == 0)
				{
					oProdCash = oProdCash + DataCommand.Cash - num3;
					oProd += AllSumm;
					BalanceCash = BalanceCash + DataCommand.Cash - num3;
				}
				else if (DataCommand.TypeCheck == 1)
				{
					oVozvCash = oVozvCash + DataCommand.Cash - num3;
					oVozvProd += AllSumm;
					BalanceCash = BalanceCash - DataCommand.Cash + num3;
				}
				else if (DataCommand.TypeCheck == 10)
				{
					oProdCash = oProdCash - DataCommand.Cash + num3;
					oPok += AllSumm;
					BalanceCash = BalanceCash - DataCommand.Cash + num3;
				}
				else if (DataCommand.TypeCheck == 11)
				{
					oVozvCash = oVozvCash - DataCommand.Cash + num3;
					oVozvPok += AllSumm;
					BalanceCash = BalanceCash + DataCommand.Cash - num3;
				}
				else if (DataCommand.TypeCheck == 2)
				{
					oKorProdCash += num2;
				}
				else if (DataCommand.TypeCheck == 3)
				{
					oKorProdCash -= num2;
				}
			}
			int numberCopies = DataCommand.NumberCopies;
			if (numberCopies <= 0)
			{
				break;
			}
			DataCommand.NumberCopies--;
			PrintCopy = true;
		}
		if (DataCommand.IsFiscalCheck && DataCommand.TypeCheck != 8 && DataCommand.TypeCheck != 9)
		{
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			if (DataCommand.TypeCheck == 0)
			{
				dictionary.Add(1054, 1.AsString());
				dictionary.Add(0, 3.AsString());
			}
			else if (DataCommand.TypeCheck == 1)
			{
				dictionary.Add(1054, 2.AsString());
				dictionary.Add(0, 3.AsString());
			}
			else if (DataCommand.TypeCheck == 10)
			{
				dictionary.Add(1054, 3.AsString());
				dictionary.Add(0, 3.AsString());
			}
			else if (DataCommand.TypeCheck == 11)
			{
				dictionary.Add(1054, 4.AsString());
				dictionary.Add(0, 3.AsString());
			}
			else if (DataCommand.TypeCheck == 2)
			{
				dictionary.Add(1054, 1.AsString());
				dictionary.Add(0, 31.AsString());
			}
			else if (DataCommand.TypeCheck == 12)
			{
				dictionary.Add(1054, 3.AsString());
				dictionary.Add(0, 31.AsString());
			}
			dictionary.Add(1020, AllSumm.AsString());
			dictionary.Add(1031, DataCommand.Cash.AsString());
			dictionary.Add(1081, DataCommand.ElectronicPayment.AsString());
			dictionary.Add(1215, DataCommand.AdvancePayment.AsString());
			dictionary.Add(1216, DataCommand.Credit.AsString());
			dictionary.Add(1217, DataCommand.CashProvision.AsString());
			dictionary.Add(1040, NumDoc.AsString());
			dictionary.Add(1012, now.AsString());
			dictionary.Add(1077, NumFPD.ToString());
			dictionary.Add(1021, DataCommand.CashierName);
			dictionary.Add(1203, DataCommand.CashierVATIN);
			if (DataCommand.PlaceMarket != null && DataCommand.PlaceMarket != "")
			{
				dictionary.Add(1187, DataCommand.PlaceMarket);
			}
			else
			{
				dictionary.Add(1187, UnitParamets["PlaceSettle"]);
			}
			if (DataCommand.SenderEmail != null && DataCommand.SenderEmail != "")
			{
				dictionary.Add(1117, DataCommand.SenderEmail);
			}
			else if (UnitParamets["SenderEmail"] != null && UnitParamets["SenderEmail"] != "")
			{
				dictionary.Add(1117, UnitParamets["SenderEmail"]);
			}
			dictionary.Add(1038, NumSes.AsString());
			dictionary.Add(1042, NumDocSes.AsString());
			dictionary.Add(1008, DataCommand.ClientAddress);
			DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
			foreach (DataCommand.CheckString checkString2 in checkStrings)
			{
				if (!DataCommand.IsFiscalCheck || checkString2 == null || checkString2.Register == null || !(checkString2.Register.Quantity != 0m))
				{
					continue;
				}
				Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
				dictionary2.Add(1023, checkString2.Register.Quantity.AsString());
				dictionary2.Add(1043, checkString2.Register.Amount.AsString());
				dictionary2.Add(1030, checkString2.Register.Name);
				dictionary2.Add(1055, DataCommand.TaxVariant);
				decimal tax = checkString2.Register.Tax;
				if (tax <= 20m)
				{
					if (tax <= 5m)
					{
						if (!(tax == -1m))
						{
							if (!(tax == 0m))
							{
								if (tax == 5m)
								{
									dictionary2.Add(1199, 2.AsString());
								}
							}
							else
							{
								dictionary2.Add(1199, 5.AsString());
							}
						}
						else
						{
							dictionary2.Add(1199, 6.AsString());
						}
					}
					else if (tax <= 10m)
					{
						if (!(tax == 7m))
						{
							if (tax == 10m)
							{
								dictionary2.Add(1199, 2.AsString());
							}
						}
						else
						{
							dictionary2.Add(1199, 2.AsString());
						}
					}
					else if (!(tax == 18m))
					{
						if (tax == 20m)
						{
							dictionary2.Add(1199, 7.AsString());
						}
					}
					else
					{
						dictionary2.Add(1199, 1.AsString());
					}
				}
				else if (tax <= 107m)
				{
					if (!(tax == 22m))
					{
						if (!(tax == 105m))
						{
							if (tax == 107m)
							{
								dictionary2.Add(1199, 4.AsString());
							}
						}
						else
						{
							dictionary2.Add(1199, 4.AsString());
						}
					}
					else
					{
						dictionary2.Add(1199, 7.AsString());
					}
				}
				else if (tax <= 118m)
				{
					if (!(tax == 110m))
					{
						if (tax == 118m)
						{
							dictionary2.Add(1199, 3.AsString());
						}
					}
					else
					{
						dictionary2.Add(1199, 4.AsString());
					}
				}
				else if (!(tax == 120m))
				{
					if (tax == 122m)
					{
						dictionary2.Add(1199, 8.AsString());
					}
				}
				else
				{
					dictionary2.Add(1199, 8.AsString());
				}
				if (checkString2.Register.GoodCodeData != null && !string.IsNullOrEmpty(checkString2.Register.GoodCodeData.MarkingCodeBase64))
				{
					dictionary2.Add(1162, checkString2.Register.GoodCodeData.MarkingCodeBase64);
				}
				List<Dictionary<int, string>> list = null;
				if (!dictionary.ContainsKey(1059))
				{
					list = new List<Dictionary<int, string>>();
					dictionary.Add(1059, list.AsString());
				}
				else if (dictionary.ContainsKey(1059))
				{
					list = dictionary[1059].AsListDictionaryIntString();
				}
				list.Add(dictionary2);
				dictionary[1059] = list.AsString();
			}
			ListRegisterCheck.Add(dictionary);
		}
		Error = "";
		RezultCommand.Status = ExecuteStatus.Ok;
		PrintSlip();
	}

	public override async Task OpenShift(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		Error = "";
		await ProcessInitDevice();
		if (Kkm.FN_Status != 3)
		{
			CreateTextError(2, "Ошибка", RezultCommand);
			return;
		}
		if (SessionOpen == 2 || SessionOpen == 3)
		{
			CreateTextError(60, "Ошибка регистрации", RezultCommand);
			return;
		}
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
			CashierVATIN = DataCommand.CashierVATIN;
		}
		UnitParamets["Session"] = DateTime.Now.AddDays(1.0).ToString();
		NumSes++;
		NumDocSes = 1;
		PrintHead();
		WriteLine(">#4#<Открытие смены", 1, Clear: false, null, AsCheck: true);
		PrintHead1();
		if (!(Error != ""))
		{
			WriteLine("Смена открыта, #:<#0#>" + NumSes, 0, Clear: false, null, AsCheck: true);
			PrintFN();
			PrintFooter();
			oProdCash = default(decimal);
			oProd = default(decimal);
			oVozvCash = default(decimal);
			oVozvProd = default(decimal);
			oPok = default(decimal);
			oVozvPok = default(decimal);
			oKorProdCash = default(decimal);
			SaveParametrs();
			await ProcessInitDevice();
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.CheckNumber = NumDoc;
			RezultCommand.SessionNumber = NumSes;
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			dictionary.Add(1054, 0.AsString());
			dictionary.Add(0, 2.AsString());
			dictionary.Add(1020, 0.AsString());
			dictionary.Add(1040, NumDoc.AsString());
			dictionary.Add(1012, DateTime.Now.AsString());
			dictionary.Add(1077, NumFPD.ToString());
			dictionary.Add(1021, DataCommand.CashierName);
			dictionary.Add(1203, DataCommand.CashierVATIN);
			dictionary.Add(1038, NumSes.AsString());
			dictionary.Add(1042, NumDocSes.AsString());
			ListRegisterCheck.Add(dictionary);
			PrintSlip();
		}
	}

	public override async Task CloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		Error = "";
		await ProcessInitDevice();
		if (Kkm.FN_Status != 3)
		{
			CreateTextError(2, "Ошибка", RezultCommand);
			return;
		}
		if (SessionOpen != 2 && SessionOpen != 3)
		{
			CreateTextError(61, "Ошибка регистрации", RezultCommand);
			return;
		}
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
			CashierVATIN = DataCommand.CashierVATIN;
		}
		NumDocSes++;
		UnitParamets["Session"] = "";
		PrintHead(ClearText);
		WriteLine(">#4#<Закрытие смены", 1, Clear: false, null, AsCheck: true);
		PrintHead1();
		if (!(Error != ""))
		{
			WriteLine("Смена закрыта, #:<#0#>" + NumSes, 0, Clear: false, null, AsCheck: true);
			PrintFN();
			PrintFooter();
			SaveParametrs();
			await ProcessInitDevice();
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.CheckNumber = NumDoc;
			RezultCommand.SessionNumber = NumSes;
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			dictionary.Add(1054, 0.AsString());
			dictionary.Add(0, 5.AsString());
			dictionary.Add(1020, 0.AsString());
			dictionary.Add(1040, NumDoc.AsString());
			dictionary.Add(1012, DateTime.Now.AsString());
			dictionary.Add(1077, NumFPD.ToString());
			dictionary.Add(1021, DataCommand.CashierName);
			dictionary.Add(1203, DataCommand.CashierVATIN);
			dictionary.Add(1038, NumSes.AsString());
			dictionary.Add(1042, NumDocSes.AsString());
			ListRegisterCheck.Add(dictionary);
			PrintSlip();
		}
	}

	public override async Task XReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		await ProcessInitDevice();
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
			CashierVATIN = DataCommand.CashierVATIN;
		}
		PrintHead();
		WriteLine(">#4#<X отчет", 1, Clear: false, null, AsCheck: true);
		PrintHead1();
		Error = "";
		WriteLine("Остаток наличных:<#16#>>" + BalanceCash, 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Внесено наличных:<#16#>>" + oDepositingCash, 0, Clear: false, null, AsCheck: true);
		WriteLine("Выемка наличных:<#16#>>" + oPaymentCash, 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Сумма продаж:<#16#>>" + oProd, 0, Clear: false, null, AsCheck: true);
		WriteLine("Сумма возвратов продаж:<#16#>>" + oVozvProd, 0, Clear: false, null, AsCheck: true);
		WriteLine("Итого продаж:<#16#>>" + (oProd - oVozvProd), 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Сумма покупок:<#16#>>" + oPok, 0, Clear: false, null, AsCheck: true);
		WriteLine("Сумма возвратов покупок:<#16#>>" + oVozvPok, 0, Clear: false, null, AsCheck: true);
		WriteLine("Итого покупок:<#16#>>" + (oPok - oVozvPok), 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Сумма продаж/покупок за наличные:<#16#>>" + oProdCash, 0, Clear: false, null, AsCheck: true);
		WriteLine("Сумма возвратов за наличные:<#16#>>" + oVozvCash, 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Корректировка продаж:<#16#>>" + oKorProdCash, 0, Clear: false, null, AsCheck: true);
		WriteLine("Корректировка возвратов:<#16#>>" + oKorVozvCash, 0, Clear: false, null, AsCheck: true);
		PrintFooter();
		RezultCommand.Status = ExecuteStatus.Ok;
		PrintSlip();
	}

	public override async Task OfdReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		await ProcessInitDevice();
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
			CashierVATIN = DataCommand.CashierVATIN;
		}
		PrintHead();
		WriteLine(">#4#<Состояние расчетов", 1, Clear: false, null, AsCheck: true);
		PrintHead1();
		if (!(Error != ""))
		{
			WriteLine("Количество непереданных документов:<#6#>>0", 0, Clear: false, null, AsCheck: true);
			WriteLine("Дата первого непереданного:<#6#>>нет", 0, Clear: false, null, AsCheck: true);
			WriteLine("Номер первого непереданного:<#6#>>нет", 0, Clear: false, null, AsCheck: true);
			PrintFN();
			PrintFooter();
			await GetCheckAndSession(RezultCommand);
			RezultCommand.Status = ExecuteStatus.Ok;
			PrintSlip();
		}
	}

	public override async Task OpenCashDrawer(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		PrintHead();
		WriteLine(">#4#<Открыли денежный ящик", 1, Clear: false, null, AsCheck: true);
		PrintFooter();
		RezultCommand.Status = ExecuteStatus.Ok;
		PrintSlip();
	}

	public override async Task DepositingCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		await ProcessInitDevice();
		if (Kkm.FN_Status != 3)
		{
			CreateTextError(2, "Ошибка", RezultCommand);
			return;
		}
		if (SessionOpen != 2)
		{
			CreateTextError(61, "Ошибка регистрации", RezultCommand);
			return;
		}
		NumDoc++;
		NumDocSes++;
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
			CashierVATIN = DataCommand.CashierVATIN;
		}
		BalanceCash += DataCommand.Amount;
		oDepositingCash += DataCommand.Amount;
		PrintHead();
		WriteLine(">#4#<Чек внесения", 1, Clear: false, null, AsCheck: true);
		PrintHead1();
		if (!(Error != ""))
		{
			WriteLine("Сумма внесения:<#16#>>" + DataCommand.Amount, 0, Clear: false, null, AsCheck: true);
			WriteLine("Остаток наличных:<#16#>>" + BalanceCash, 0, Clear: false, null, AsCheck: true);
			PrintFN();
			PrintFooter();
			SaveParametrs();
			await GetCheckAndSession(RezultCommand);
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.CheckNumber = NumDoc;
			RezultCommand.SessionNumber = NumSes;
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			dictionary.Add(1054, 1.AsString());
			dictionary.Add(0, 31.AsString());
			dictionary.Add(1040, NumDoc.AsString());
			dictionary.Add(1012, DateTime.Now.AsString());
			dictionary.Add(1077, NumFPD.ToString());
			dictionary.Add(1021, DataCommand.CashierName);
			dictionary.Add(1203, DataCommand.CashierVATIN);
			dictionary.Add(1038, NumSes.AsString());
			dictionary.Add(1042, NumDocSes.AsString());
			dictionary.Add(1020, DataCommand.Amount.AsString());
			dictionary.Add(1031, DataCommand.Amount.AsString());
			ListRegisterCheck.Add(dictionary);
			PrintSlip();
		}
	}

	public override async Task PaymentCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (Kkm.FN_Status != 3)
		{
			CreateTextError(2, "Ошибка", RezultCommand);
			return;
		}
		if (SessionOpen != 2)
		{
			CreateTextError(61, "Ошибка регистрации", RezultCommand);
			return;
		}
		if (BalanceCash - DataCommand.Amount < 0m)
		{
			CreateTextError(70, "Ошибка выемки", RezultCommand);
			return;
		}
		NumDoc++;
		NumDocSes++;
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
		}
		BalanceCash -= DataCommand.Amount;
		oPaymentCash = oDepositingCash + DataCommand.Amount;
		PrintHead();
		WriteLine(">#4#<Чек выемки", 1, Clear: false, null, AsCheck: true);
		PrintHead1();
		if (!(Error != ""))
		{
			WriteLine("Сумма выемки:<#16#>>" + DataCommand.Amount, 0, Clear: false, null, AsCheck: true);
			WriteLine("Остаток наличных:<#16#>>" + BalanceCash, 0, Clear: false, null, AsCheck: true);
			PrintFN();
			PrintFooter();
			SaveParametrs();
			await GetCheckAndSession(RezultCommand);
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.CheckNumber = NumDoc;
			RezultCommand.SessionNumber = NumSes;
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			dictionary.Add(1054, 3.AsString());
			dictionary.Add(0, 31.AsString());
			dictionary.Add(1040, NumDoc.AsString());
			dictionary.Add(1012, DateTime.Now.AsString());
			dictionary.Add(1077, NumFPD.ToString());
			dictionary.Add(1021, DataCommand.CashierName);
			dictionary.Add(1203, DataCommand.CashierVATIN);
			dictionary.Add(1038, NumSes.AsString());
			dictionary.Add(1042, NumDocSes.AsString());
			dictionary.Add(1020, DataCommand.Amount.AsString());
			dictionary.Add(1031, DataCommand.Amount.AsString());
			ListRegisterCheck.Add(dictionary);
			PrintSlip();
		}
	}

	public override async Task GetLineLength(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		RezultCommand.LineLength = Kkm.PrintingWidth;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task KkmRegOfd(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		await ProcessInitDevice(FullInit: true);
		await ReadStatusOFD(Full: true);
		if (DataCommand.RegKkmOfd.Command == "Open")
		{
			if (Kkm.FN_Status != 0 && Kkm.FN_Status != 1)
			{
				CreateTextError(2, "Ошибка регистрации", RezultCommand);
				return;
			}
		}
		else if (Kkm.FN_Status != 3)
		{
			CreateTextError(2, "Ошибка регистрации", RezultCommand);
			return;
		}
		if (SessionOpen == 2 || SessionOpen == 3)
		{
			CreateTextError(60, "Ошибка регистрации", RezultCommand);
			return;
		}
		CashierName = "";
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			CashierName = DataCommand.CashierName;
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "Open")
		{
			if (DataCommand.RegKkmOfd.UrlServerOfd != "")
			{
				UnitParamets["UrlServerOfd"] = DataCommand.RegKkmOfd.UrlServerOfd.Trim();
			}
			if (DataCommand.RegKkmOfd.PortServerOfd != "")
			{
				UnitParamets["PortServerOfd"] = DataCommand.RegKkmOfd.PortServerOfd.Trim();
			}
			if (DataCommand.RegKkmOfd.UrlOfd != "")
			{
				UnitParamets["UrlOfd"] = DataCommand.RegKkmOfd.UrlOfd.Trim();
			}
			if (DataCommand.RegKkmOfd.InnOfd != "")
			{
				if (DataCommand.RegKkmOfd.InnOfd.Length != 10 && DataCommand.RegKkmOfd.InnOfd.Length != 12)
				{
					CreateTextError(51, "Ошибка установки ИНН ОФД", RezultCommand);
					return;
				}
				UnitParamets["InnOfd"] = DataCommand.RegKkmOfd.InnOfd.Trim();
			}
			if (DataCommand.RegKkmOfd.NameOFD != "")
			{
				UnitParamets["NameOFD"] = DataCommand.RegKkmOfd.NameOFD.Trim();
			}
		}
		UnitParamets["StatusККТ"] = "";
		if (DataCommand.RegKkmOfd.Command == "Open")
		{
			if (DataCommand.RegKkmOfd.InnOrganization.Length != 10 && DataCommand.RegKkmOfd.InnOrganization.Length != 12)
			{
				CreateTextError(51, "Ошибка установки ИНН организации", RezultCommand);
				return;
			}
			UnitParamets["InnOrganization"] = DataCommand.RegKkmOfd.InnOrganization.Trim();
			UnitParamets["RegNumber"] = DataCommand.RegKkmOfd.RegNumber;
		}
		if (!(DataCommand.RegKkmOfd.Command == "ChangeFN"))
		{
			_ = DataCommand.RegKkmOfd.Command == "Open";
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "Open")
		{
			UnitParamets["InnOfd"] = DataCommand.RegKkmOfd.InnOfd.Trim();
		}
		string[] array;
		if (DataCommand.RegKkmOfd.Command == "ChangeOrganization" || DataCommand.RegKkmOfd.Command == "Open")
		{
			if (DataCommand.RegKkmOfd.NameOrganization != "")
			{
				UnitParamets["NameOrganization"] = DataCommand.RegKkmOfd.NameOrganization.Trim();
			}
			if (DataCommand.RegKkmOfd.AddressSettle != "")
			{
				UnitParamets["AddressSettle"] = DataCommand.RegKkmOfd.AddressSettle.Trim();
			}
			if (DataCommand.RegKkmOfd.PlaceSettle != "")
			{
				UnitParamets["PlaceSettle"] = DataCommand.RegKkmOfd.PlaceSettle.Trim();
			}
			if (DataCommand.RegKkmOfd.SenderEmail != "")
			{
				UnitParamets["SenderEmail"] = DataCommand.RegKkmOfd.SenderEmail.Trim();
			}
			string text = "";
			array = DataCommand.RegKkmOfd.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i])
				{
				case "0":
					text = text + ((text != "") ? ", " : "") + "Основная (ОСН)";
					break;
				case "1":
					text = text + ((text != "") ? ", " : "") + "УСН (Доход)";
					break;
				case "2":
					text = text + ((text != "") ? ", " : "") + "УСН (Доход-Расход)";
					break;
				case "3":
					text = text + ((text != "") ? ", " : "") + "ЕНВД";
					break;
				case "4":
					text = text + ((text != "") ? ", " : "") + "Сельскохозяйственный налог";
					break;
				case "5":
					text = text + ((text != "") ? ", " : "") + "Патент";
					break;
				}
			}
			UnitParamets["TaxVariant"] = text;
			UnitParamets["SignOfAgent"] = DataCommand.RegKkmOfd.SignOfAgent;
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "Open")
		{
			string text2 = "";
			switch (DataCommand.RegKkmOfd.SetFfdVersion)
			{
			case 1:
				text2 = "1.0";
				break;
			case 2:
				text2 = "1.05";
				break;
			case 3:
				text2 = "1.1";
				break;
			}
			UnitParamets["FfdVersion"] = text2;
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.ServiceMode ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Расчеты за услуги") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.BSOMode ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Только БСО") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.AutomaticMode ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Автоматический режим") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.InternetMode ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Расчеты только в Интернете") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.OfflineMode ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Автономный режим") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.EncryptionMode ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Шифрование") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.PrinterAutomatic ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Установка принтера в автомате") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.SignOfGambling ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Проведения азартных игр") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.SignOfLottery ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Проведения лотереи") : "");
			UnitParamets["StatusККТ"] = UnitParamets["StatusККТ"] + (DataCommand.RegKkmOfd.SaleExcisableGoods ? (((UnitParamets["StatusККТ"] != "") ? ", " : "") + "Продажа подакцизного товара") : "");
		}
		switch (DataCommand.RegKkmOfd.Command)
		{
		case "Open":
			UnitParamets["StatusFN"] = "Фискальный режим";
			break;
		case "ChangeFN":
			UnitParamets["StatusFN"] = "Фискальный режим";
			break;
		case "ChangeOFD":
			UnitParamets["StatusFN"] = "Фискальный режим";
			break;
		case "ChangeOrganization":
			UnitParamets["StatusFN"] = "Фискальный режим";
			break;
		case "ChangeKkm":
			UnitParamets["StatusFN"] = "Фискальный режим";
			break;
		case "Close":
			UnitParamets["StatusFN"] = "Чтение из архива ФН";
			break;
		}
		SaveParametrs();
		await ProcessInitDevice(FullInit: true);
		Error = "";
		Kkm.InfoRegKkt = "Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + ", ФД: 123456789, ФПД: 123456789544";
		RezultCommand.QRCode = Kkm.InfoRegKkt;
		PrintHead();
		switch (DataCommand.RegKkmOfd.Command)
		{
		case "Open":
			WriteLine(">#4#<Первичная регистрация ККТ", 2, Clear: false, null, AsCheck: true);
			break;
		case "ChangeFN":
			WriteLine(">#0#<Замена ФН", 2, Clear: false, null, AsCheck: true);
			break;
		case "ChangeOFD":
			WriteLine(">#0#<Изменение параметров ОФД", 2, Clear: false, null, AsCheck: true);
			break;
		case "ChangeOrganization":
			WriteLine(">#0#<Именение параметров Организации", 2, Clear: false, null, AsCheck: true);
			break;
		case "ChangeKkm":
			WriteLine(">#0#<Изменение параметров ККТ", 2, Clear: false, null, AsCheck: true);
			break;
		case "Close":
			WriteLine(">#0#<Закрытие архива ФН", 2, Clear: false, null, AsCheck: true);
			break;
		}
		WriteLine("Заводской #:<#0#>" + UnitParamets["FacNumber"], 0, Clear: false, null, AsCheck: true);
		WriteLine("Номер ФН:<#0#>" + UnitParamets["FnNumber"], 0, Clear: false, null, AsCheck: true);
		WriteLine("Регистрационный #:<#0#>" + UnitParamets["RegNumber"], 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Наименование:<#0#>" + UnitParamets["NameOrganization"], 0, Clear: false, null, AsCheck: true);
		WriteLine("ИНН организации:<#0#>" + UnitParamets["InnOrganization"], 0, Clear: false, null, AsCheck: true);
		WriteLine("Адрес установки:<#0#>" + UnitParamets["AddressSettle"], 3, Clear: false, null, AsCheck: true);
		if (Kkm.FfdVersion >= 2)
		{
			WriteLine("Место установки:<#0#>" + UnitParamets["PlaceSettle"], 3, Clear: false, null, AsCheck: true);
			WriteLine("Email магазина:<#0#>" + UnitParamets["SenderEmail"], 3, Clear: false, null, AsCheck: true);
		}
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		array = UnitParamets["TaxVariant"].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text3 in array)
		{
			WriteLine(text3 + ":<#10#>>да", 0, Clear: false, null, AsCheck: true);
		}
		if (Kkm.FfdVersion >= 2)
		{
			WriteLine("Коды агента:<#10#>>" + UnitParamets["SignOfAgent"], 0, Clear: false, null, AsCheck: true);
		}
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Версия ФФД:<#10#>>" + UnitParamets["FfdVersion"], 0, Clear: false, null, AsCheck: true);
		array = UnitParamets["StatusККТ"].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text4 in array)
		{
			WriteLine(text4 + ":<#10#>>да", 0, Clear: false, null, AsCheck: true);
		}
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Наименование:<#0#>" + UnitParamets["NameOFD"], 0, Clear: false, null, AsCheck: true);
		WriteLine("ИНН:<#0#>" + UnitParamets["InnOfd"], 0, Clear: false, null, AsCheck: true);
		WriteLine("URL сервис:<#0#>" + UnitParamets["UrlServerOfd"], 0, Clear: false, null, AsCheck: true);
		WriteLine("PORT сервиса:<#0#>" + UnitParamets["PortServerOfd"], 0, Clear: false, null, AsCheck: true);
		WriteLine("URL проверки чека:<#0#>" + UnitParamets["UrlOfd"], 0, Clear: false, null, AsCheck: true);
		PrintFooter();
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task GetDataKKT(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (!IsInitOfd)
		{
			await ReadStatusOFD(Full: true);
		}
		Kkm.DateTimeKKT = DateTime.Now;
		await base.GetDataKKT(DataCommand, RezultCommand);
		RezultCommand.Info.SessionState = SessionOpen;
		RezultCommand.Info.BalanceCash = BalanceCash;
		RezultCommand.CheckNumber = NumDoc;
		RezultCommand.SessionNumber = NumSes;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task<uint> GetLastFiscalNumber()
	{
		if (ListRegisterCheck.Count != 0 && ListRegisterCheck.Last().ContainsKey(1040))
		{
			return ListRegisterCheck.Last()[1040].AsUInt();
		}
		return 0u;
	}

	public override async Task<Dictionary<int, string>> GetRegisterCheck(uint FiscalNumber, Dictionary<int, Type> Types)
	{
		Dictionary<int, string> result = null;
		foreach (Dictionary<int, string> item in ListRegisterCheck)
		{
			if (item.ContainsKey(1040) && item[1040].AsUInt() == FiscalNumber)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	public override async Task ReadStatusOFD(bool Full = false, bool ReadInfoGer = false, bool NoInit = false)
	{
		Kkm.INN = UnitParamets["InnOrganization"];
		Kkm.Organization = UnitParamets["NameOrganization"];
		Kkm.AddressSettle = UnitParamets["AddressSettle"];
		Kkm.PlaceSettle = UnitParamets["PlaceSettle"];
		Kkm.SenderEmail = UnitParamets["SenderEmail"];
		Kkm.SignOfAgent = UnitParamets["SignOfAgent"];
		Kkm.NumberKkm = UnitParamets["FacNumber"];
		Kkm.RegNumber = UnitParamets["RegNumber"];
		switch (UnitParamets["FfdVersion"])
		{
		case "1.0":
			Kkm.FfdVersion = 1;
			break;
		case "1.05":
			Kkm.FfdVersion = 2;
			break;
		case "1.1":
			Kkm.FfdVersion = 3;
			break;
		}
		Kkm.Fn_Number = UnitParamets["FnNumber"];
		Kkm.UrlOfd = UnitParamets["UrlOfd"];
		Kkm.UrlServerOfd = UnitParamets["UrlServerOfd"];
		Kkm.PortServerOfd = UnitParamets["PortServerOfd"];
		Kkm.InnOfd = UnitParamets["InnOfd"];
		Kkm.NameOFD = UnitParamets["NameOFD"];
		if (UnitParamets["StatusFN"] == "Настройка ФН")
		{
			Kkm.FN_Status = 0;
		}
		else if (UnitParamets["StatusFN"] == "Готовность к фискализации")
		{
			Kkm.FN_Status = 1;
		}
		else if (UnitParamets["StatusFN"] == "Фискальный режим")
		{
			Kkm.FN_Status = 3;
		}
		else if (UnitParamets["StatusFN"] == "Закрытия архива ФН")
		{
			Kkm.FN_Status = 7;
		}
		else if (UnitParamets["StatusFN"] == "Чтение из архива ФН")
		{
			Kkm.FN_Status = 15;
		}
		Kkm.TaxVariant = "";
		Kkm.TaxVariant += ((UnitParamets["TaxVariant"].IndexOf("Основная (ОСН)") != -1) ? (((Kkm.TaxVariant == "") ? "" : ",") + "0") : "");
		Kkm.TaxVariant += ((UnitParamets["TaxVariant"].IndexOf("УСН (Доход)") != -1) ? (((Kkm.TaxVariant == "") ? "" : ",") + "1") : "");
		Kkm.TaxVariant += ((UnitParamets["TaxVariant"].IndexOf("УСН (Доход-Расход)") != -1) ? (((Kkm.TaxVariant == "") ? "" : ",") + "2") : "");
		Kkm.TaxVariant += ((UnitParamets["TaxVariant"].IndexOf("ЕНВД") != -1) ? (((Kkm.TaxVariant == "") ? "" : ",") + "3") : "");
		Kkm.TaxVariant += ((UnitParamets["TaxVariant"].IndexOf("Сельскохозяйственный налог") != -1) ? (((Kkm.TaxVariant == "") ? "" : ",") + "4") : "");
		Kkm.TaxVariant += ((UnitParamets["TaxVariant"].IndexOf("Патент") != -1) ? (((Kkm.TaxVariant == "") ? "" : ",") + "5") : "");
		Kkm.EncryptionMode = UnitParamets["StatusККТ"].IndexOf("Шифрование") != -1;
		Kkm.OfflineMode = UnitParamets["StatusККТ"].IndexOf("Автономный режим") != -1;
		Kkm.AutomaticMode = UnitParamets["StatusККТ"].IndexOf("Автоматический режим") != -1;
		Kkm.InternetMode = UnitParamets["StatusККТ"].IndexOf("Расчеты только в Интернете") != -1;
		Kkm.BSOMode = UnitParamets["StatusККТ"].IndexOf("Только БСО") != -1;
		Kkm.ServiceMode = UnitParamets["StatusККТ"].IndexOf("Расчеты за услуги") != -1;
		Kkm.PrinterAutomatic = UnitParamets["StatusККТ"].IndexOf("Установка принтера в автомате") != -1;
		Kkm.SignOfGambling = UnitParamets["StatusККТ"].IndexOf("Проведения азартных игр") != -1;
		Kkm.SignOfLottery = UnitParamets["StatusККТ"].IndexOf("Проведения лотереи") != -1;
		Kkm.SaleExcisableGoods = UnitParamets["StatusККТ"].IndexOf("Продажа подакцизного товара") != -1;
		Kkm.FN_IsFiscal = Kkm.FN_Status == 3;
		Kkm.FN_MemOverflowl = false;
		Kkm.FN_DateEnd = DateTime.Today.AddYears(1);
		Kkm.OFD_NumErrorDoc = 0;
		Kkm.OFD_DateErrorDoc = default(DateTime);
		Kkm.OFD_Error = "";
		Kkm.InfoRegKkt = "Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + ", ФД: 123456789, ФПД: 123456789544";
		Kkm.FN_DateStart = DateTime.Now.AddYears(0).Date;
	}

	public bool PrintBarCode(DataCommand.PrintBarcode PrintBarCode)
	{
		if (PrintBarCode.BarcodeType != null && PrintBarCode.BarcodeType != "")
		{
			ImageBarCode imageBarCode = null;
			switch (PrintBarCode.BarcodeType.ToUpper())
			{
			case "EAN13":
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 240, 160);
				break;
			case "CODE39":
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 80);
				break;
			case "CODE128":
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 80);
				break;
			case "PDF417":
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 140);
				break;
			case "QR":
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 200, 200);
				break;
			}
			if (imageBarCode != null)
			{
				WriteLine(imageBarCode, 0, Clear: false, null, AsCheck: true);
			}
		}
		return true;
	}

	public bool PrintImage(DataCommand.PrintImage PrintImage)
	{
		Image<Rgba32> prnObject = BarCode.ImageFromBase64(PrintImage.Image);
		WriteLine(prnObject, 0, Clear: false, null, AsCheck: true);
		return true;
	}

	public void SaveParametrs()
	{
		SettDr.Paramets = new Dictionary<string, string>(UnitParamets);
		Global.SaveSettingsAsync().Wait();
	}

	public void PrintHead(bool ClearText = true)
	{
		WriteLine("", 0, ClearText, false, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
	}

	public void PrintHead1()
	{
		WriteLine("ИНН организации:<#0#>" + UnitParamets["InnOrganization"], 0, Clear: false, null, AsCheck: true);
		WriteLine(UnitParamets["NameOrganization"] + ", " + UnitParamets["AddressSettle"], 0, Clear: false, null, AsCheck: true);
		WriteLine("Заводской номер ККТ:<#0#>" + UnitParamets["FacNumber"], 0, Clear: false, null, AsCheck: true);
		WriteLine("Email:<#0#>" + SenderEmail, 0, Clear: false, null, AsCheck: true);
		WriteLine("Кассир:<#0#>" + CashierName, 0, Clear: false, null, AsCheck: true);
		if (Kkm.FfdVersion >= 2)
		{
			WriteLine("ИНН кассира:<#0#>" + CashierVATIN, 3, Clear: false, null, AsCheck: true);
		}
		WriteLine("Дата:<#0#>" + DateTime.Now, 0, Clear: false, null, AsCheck: true);
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
	}

	public void PrintFN(string URL = "")
	{
		if (URL == "")
		{
			NumDoc++;
			Random random = new Random();
			NumFPD = random.Next(0, int.MaxValue);
		}
		WriteLine("".PadRight(Kkm.PrintingWidth, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("Номер ФН:<#22#>>" + UnitParamets["FnNumber"], 0, Clear: false, null, AsCheck: true);
		WriteLine("Рег. #:<#22#>>" + UnitParamets["RegNumber"], 0, Clear: false, null, AsCheck: true);
		WriteLine("ФД #:<#22#>>" + NumDoc, 0, Clear: false, null, AsCheck: true);
		WriteLine("ФПД #:<#22#>>" + NumFPD, 0, Clear: false, null, AsCheck: true);
		if (URL != "")
		{
			DataCommand.PrintBarcode printBarcode = new DataCommand.PrintBarcode();
			printBarcode.BarcodeType = "QR";
			printBarcode.Barcode = URL;
			PrintBarCode(printBarcode);
		}
	}

	public void PrintAgentData(int? iAgentSign, DataCommand.TypeAgentData AgentData, DataCommand.TypePurveyorData PurveyorData)
	{
		if (Kkm.FfdVersion >= 2)
		{
			byte b = (byte)iAgentSign.Value;
			if (!((ICollection<string>)Kkm.SignOfAgent.Split(',')).Contains(b.ToString()))
			{
				CreateTextError(102, "Ошибка записи параметров агента");
				return;
			}
			string text = "";
			switch (b)
			{
			case 0:
				text = "«БАНК.ПЛ.АГЕНТ»";
				break;
			case 1:
				text = "«БАНК.ПЛ.СУБАГЕНТ»";
				break;
			case 2:
				text = "«ПЛ.АГЕНТ»";
				break;
			case 3:
				text = "«ПЛ.СУБАГЕНТ»";
				break;
			case 4:
				text = "«ПОВЕРЕННЫЙ»";
				break;
			case 5:
				text = "«КОМИССИОНЕР»";
				break;
			case 6:
				text = "«АГЕНТ»";
				break;
			}
			WriteLine("Данные агента:<#0#>" + text, 3, Clear: false, null, AsCheck: true);
		}
		else
		{
			WriteLine("Данные агента:<#0#>:", 3, Clear: false, null, AsCheck: true);
		}
		if (AgentData == null && PurveyorData == null)
		{
			CreateTextError(103, "Ошибка записи параметров агента");
			return;
		}
		if (AgentData != null)
		{
			if (AgentData.PayingAgentOperation != null && AgentData.PayingAgentOperation != "")
			{
				WriteLine("Операция платежного агента:<#0#>" + AgentData.PayingAgentOperation, 3, Clear: false, null, AsCheck: true);
			}
			if (AgentData.PayingAgentPhone != null && AgentData.PayingAgentPhone != "")
			{
				WriteLine("Тел. платежного агента:<#0#>" + AgentData.PayingAgentPhone, 3, Clear: false, null, AsCheck: true);
			}
			if (AgentData.ReceivePaymentsOperatorPhone != null && AgentData.ReceivePaymentsOperatorPhone != "")
			{
				WriteLine("Тел. оператора по приему платежей:<#0#>" + AgentData.ReceivePaymentsOperatorPhone, 3, Clear: false, null, AsCheck: true);
			}
			if (AgentData.MoneyTransferOperatorName != null && AgentData.MoneyTransferOperatorName != "")
			{
				WriteLine("Оператора перевода:<#0#>" + AgentData.MoneyTransferOperatorName, 3, Clear: false, null, AsCheck: true);
			}
			if (AgentData.MoneyTransferOperatorPhone != null && AgentData.MoneyTransferOperatorPhone != "")
			{
				WriteLine("Тел. оператора перевода:<#0#>" + AgentData.MoneyTransferOperatorPhone, 3, Clear: false, null, AsCheck: true);
			}
			if (AgentData.MoneyTransferOperatorAddress != null && AgentData.MoneyTransferOperatorAddress != "")
			{
				WriteLine("Адрес оператора перевода:<#0#>" + AgentData.MoneyTransferOperatorAddress, 3, Clear: false, null, AsCheck: true);
			}
			if (AgentData.MoneyTransferOperatorVATIN != null && AgentData.MoneyTransferOperatorVATIN != "")
			{
				WriteLine("ИНН оператора перевода:<#0#>" + AgentData.MoneyTransferOperatorVATIN, 3, Clear: false, null, AsCheck: true);
			}
		}
		if (PurveyorData != null)
		{
			if (PurveyorData.PurveyorName != null && PurveyorData.PurveyorName != "")
			{
				WriteLine("Поставщик:<#0#>" + PurveyorData.PurveyorName, 3, Clear: false, null, AsCheck: true);
			}
			if (PurveyorData.PurveyorPhone != null && PurveyorData.PurveyorPhone != "")
			{
				WriteLine("Телефон поставщика:<#0#>" + PurveyorData.PurveyorPhone, 3, Clear: false, null, AsCheck: true);
			}
			if (PurveyorData.PurveyorVATIN != null && PurveyorData.PurveyorVATIN != "")
			{
				WriteLine("ИНН поставщика:<#0#>" + PurveyorData.PurveyorVATIN, 3, Clear: false, null, AsCheck: true);
			}
		}
	}

	public void PrintFooter()
	{
		WriteLine("".PadRight((Kkm.PrintingWidth - 12) / 2, '-') + "линия отреза" + "".PadRight((Kkm.PrintingWidth - 12) / 2, '-'), 0, Clear: false, null, AsCheck: true);
		WriteLine("", 0, Clear: false, true, AsCheck: true);
	}

	public void PrintSlip()
	{
		Global.TextLines = TextLines;
		Global.WriteLine("", 0, Clear: false, true, AsCheck: true);
	}

	public bool CreateTextError(byte ErrorByte, string TextError, RezultCommandKKm RezultCommand = null)
	{
		string text = "Неизвестный код ошибки";
		switch (ErrorByte)
		{
		case 0:
			text = "ФH: Успешное выполнение команды";
			break;
		case 1:
			text = "ФH: Неизвестная команда, неверный формат посылки или неизвестные параметры";
			break;
		case 2:
			text = "ФH: Неверное состояние ФН. Данная команда требует другого состояния ФН.\r\nВот такое непонятное сообщение может выдать Вам реальный аппарат.\r\nДанное сообщение означает что ККТ или не прошел регистрацию или наооборот закончился срок действия ФН или ФН был закрыт.\r\nВ первом случае надо провести регистрацию ККТ/эмулятора (данные в налоговую естественно не будут уходить)\r\nВо втором случае надо сбросить ФН эмулятора в начальное состояние в его настройках.\r\nВнимание! Реальный ФН сбросить нельзя!";
			break;
		case 3:
			text = "ФH: Ошибка ФН";
			break;
		case 4:
			text = "ФH: Ошибка контрольной суммы ФН";
			break;
		case 5:
			text = "ФH: Закончен срок эксплуатации ФН";
			break;
		case 6:
			text = "ФH: Архив ФН переполнен";
			break;
		case 7:
			text = "ФH: Неверные дата и / или время";
			break;
		case 8:
			text = "ФH: Нет запрошенных данных / Запрошенные данные отсутствуют в Архиве ФН";
			break;
		case 9:
			text = "ФH: Некорректное значение параметров команды / Параметры команды имеют правильный формат, но их значение не верно";
			break;
		case 16:
			text = "ФH: Превышение размеров TLV данных";
			break;
		case 17:
			text = "ФH: Нет транспортного соединения";
			break;
		case 18:
			text = "ФH: Исчерпан ресурс КС(криптографического сопроцессора). Требуется закрытие фискального режима";
			break;
		case 20:
			text = "ФH: Ресурс для хранения документов для ОФД исчерпан";
			break;
		case 21:
			text = "ФH: Исчерпан ресурс Ожидания передачи сообщения. Время нахождения в очереди самого старого сообщения на выдачу более 30 календарных дней.";
			break;
		case 22:
			text = "ФH: Продолжительность смены более 24 часов";
			break;
		case 23:
			text = "ФH: Разница более чем на 5 минут отличается от разницы определенному по внутреннему таймеру ФН.";
			break;
		case 32:
			text = "ФH: Сообщение от ОФД не может быть принято";
			break;
		case 38:
			text = "ККТ: Вносимая клиентом сумма меньше суммы чека";
			break;
		case 40:
			text = "ККТ: Ничего важного не изменилось. Перерегистрация не нужна";
			break;
		case 41:
			text = "ККТ: ИНН и Регистрационный номер не должны меняться";
			break;
		case 51:
			text = "ККТ: Некорректные параметры в команде";
			break;
		case 52:
			text = "ККТ: Нет данных";
			break;
		case 55:
			text = "ККТ: Команда не поддерживается в данной реализации ККТ";
			break;
		case 57:
			text = "ККТ: Внутренняя ошибка ПО ККТ";
			break;
		case 60:
			text = "ККТ: Смена открыта – операция невозможна";
			break;
		case 61:
			text = "ККТ: Смена не открыта или смена превысила 24 часа – операция невозможна";
			break;
		case 69:
			text = "ККТ: Сумма всех типов оплаты меньше итога чека";
			break;
		case 70:
			text = "ККТ: Не хватает наличности в кассе";
			break;
		case 73:
			text = "ККТ: Операция невозможна в открытом чеке данного типа";
			break;
		case 74:
			text = "ККТ: Открыт чек – операция невозможна";
			break;
		case 77:
			text = "ККТ: Вносимая безналичной оплатой сумма больше суммы чека";
			break;
		case 79:
			text = "ККТ: Неверный пароль";
			break;
		case 80:
			text = "ККТ: Идет печать результатов выполнения предыдущей команды";
			break;
		case 85:
			text = "ККТ: Чек закрыт – операция невозможна";
			break;
		case 90:
			text = "ККТ: Скидка больше итога по строке";
			break;
		case 94:
			text = "ККТ: Неверная команда";
			break;
		case 95:
			text = "ККТ: Сторно больше итогов чека";
			break;
		case 100:
			text = "ККТ: Не задан кассир";
			break;
		case 101:
			text = "ККТ: Не задан ИНН кассира";
			break;
		case 102:
			text = "ККТ: Код агента не соотвествует зарегистрированному в ККТ";
			break;
		case 103:
			text = "ККТ: Нет данных агента";
			break;
		case 104:
			text = "ККТ: Нет данных поставщика";
			break;
		case 105:
			text = "ККТ: Не указан признак предмета расчета! ККТ работает по ФФД 1.05/1.1 Признак обязателен";
			break;
		case 106:
			text = "ККТ: Не указан признак способа расчета! ККТ работает по ФФД 1.05/1.1 Признак обязателен";
			break;
		case 109:
			text = "ККТ: Не хватает денег по налогу";
			break;
		case 114:
			text = "ККТ: Команда не поддерживается в данном подрежиме";
			break;
		case 115:
			text = "ККТ: Команда не поддерживается в данном режиме";
			break;
		case 124:
			text = "ККТ: Не совпадает дата";
			break;
		case 125:
			text = "ККТ: Неверный формат даты";
			break;
		case 142:
			text = "ККТ: Нулевой итог чека";
			break;
		case 192:
			text = "ККТ: Контроль даты и времени(подтвердите дату и время)";
			break;
		case 196:
			text = "ККТ: Несовпадение номеров смен";
			break;
		case 200:
			text = "ККТ: Нет связи с принтером";
			break;
		case 207:
			text = "ККТ: Неверная дата(Часы сброшены ? Установите дату!)";
			break;
		case 249:
			text = "ККТ: Ошибка транспортного уровня при получении данных из архива ФН";
			break;
		case 250:
			text = "ККТ: Основная плата устройства не отвечает";
			break;
		case 252:
			text = "ККТ: Неверная контрольная сумма файла";
			break;
		case 253:
			text = "ККТ: Прочие ошибки принтера";
			break;
		case 254:
			text = "ККТ: Принтер в оффлайне";
			break;
		case byte.MaxValue:
			text = "ККТ: Фатальная ошибка устройства";
			break;
		}
		Error = TextError + " ( " + ErrorByte + " : " + text + " )";
		if (RezultCommand != null)
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		return true;
	}
}
