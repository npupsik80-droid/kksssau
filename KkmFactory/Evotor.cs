using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace KkmFactory;

public class Evotor : UnitPort
{
	private string KkmServerURI = "https://kkmserver.ru/Data/";

	public string TokenAppEvotor = "";

	public string EvotorDeviceId = "";

	public string EvotorDeviceName = "";

	public static string PublisherToken = "";

	public Evotor(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		Kkm.IsKKT = true;
		IsNotSetOrStatus = true;
		UseBuiltTerminal = true;
		LicenseFlags = ComDevice.PaymentOption.None;
		Task.Run(delegate
		{
			GetPublisherToken();
		});
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "Драйвер ККМ для моделей: " + SettDr.TypeDevice.SupportModels;
		UnitEquipmentType = "ФискальныйРегистратор";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kkmserver.ru";
		NameDevice = "ККТ Эвотор";
		UnitAdditionallinks = "<a href='https://market.evotor.ru/store/auth/login'>Личный кабинет на сайте Эвотора</a><br/><a href='https://market.evotor.ru/store/apps/5ada1aac-dd2d-4df9-a09f-df1fb17eea0f'>Приложение kkmserver для Эвотора</a><br/><a href='https://kkmserver.ru/WiKi/SettingEvotor'>Инструкция по настройке</a><br/>";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TokenAppEvotor\" Caption=\"ID пользователя Эвотор-a\" TypeValue=\"String\" DefaultValue=\"\" SaveOnChange=\"true\"\r\n                Help=\"Инструкция по настройке: &lt;a id=&quot;help&quot; href=&quot;https://kkmserver.ru/WiKi/SettingAtol&quot;&gt;https://kkmserver.ru/WiKi/SettingAtol&lt;/a&gt;\"\r\n                Description=\"\">\r\n            </Parameter>\r\n            <Parameter Name=\"EvotorDevice\" Caption=\"Терминал в л.кабинете эвотора\" TypeValue=\"String\" DefaultValue=\"\">\r\n                <ChoiceList>\r\n                        #ChoiceListrDevice#\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n        <Group Caption=\"Параметры Организации\">\r\n            <Parameter Name=\"NameOrganization\" Caption=\"Наименование организации\" TypeValue=\"String\"  />\r\n            <Parameter Name=\"InnOrganization\" Caption=\"ИНН организации\" TypeValue=\"String\"\r\n                Help=\"Можно посмотреть в Этовторе->Настройки->Обслуживание ККТ->Изменения реквизитов ЕНВД\"/>\r\n        </Group>\r\n        <Group Caption=\"Зарегистрированные системы налогообложения в ККТ\">\r\n            <Parameter Name=\"SnoOsn\" Caption=\"Общая(ОСН)\" TypeValue=\"Boolean\" DefaultValue=\"false\"/>\r\n            <Parameter Name=\"SnoDoh\" Caption=\"УСН (Доход)\" TypeValue=\"Boolean\" DefaultValue=\"false\"/>\r\n            <Parameter Name=\"SnoDohRas\" Caption=\"УСН (Доход-Расход)\" TypeValue=\"Boolean\" DefaultValue=\"false\"/>\r\n            <Parameter Name=\"SnoEnvd\" Caption=\"ЕНВД\" TypeValue=\"Boolean\" DefaultValue=\"false\"/>\r\n            <Parameter Name=\"SnoEsn\" Caption=\"ЕСН\" TypeValue=\"Boolean\" DefaultValue=\"false\"/>\r\n            <Parameter Name=\"SnoPat\" Caption=\"Патент\" TypeValue=\"Boolean\" DefaultValue=\"false\"\r\n                Help=\"Можно посмотреть в Этовторе->Настройки->Обслуживание ККТ->Изменения реквизитов ЕНВД->Далее\"/>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		StringBuilder stringBuilder = new StringBuilder("<Item Value=\"\">Не указанно</Item>");
		try
		{
			if (TokenAppEvotor != "")
			{
				EvotorStoresDevices evotorStoresDevices = (EvotorStoresDevices)HTTP_Command("GET", "stores", new EvotorStoresDevices(), null, "application/vnd.evotor.v2+json", "application/vnd.evotor.v2+json");
				EvotorStoresDevices obj = (EvotorStoresDevices)HTTP_Command("GET", "devices", new EvotorStoresDevices(), null, "application/vnd.evotor.v2+json", "application/vnd.evotor.v2+json");
				string text2 = "";
				foreach (EvotorStoresDevices.EvotorStoreDevice item in obj.items)
				{
					foreach (EvotorStoresDevices.EvotorStoreDevice item2 in evotorStoresDevices.items)
					{
						if (item.store_id == item2.id)
						{
							text2 = item2.name;
							break;
						}
					}
					stringBuilder.Append("<Item Value=\"" + item.id + "|" + item.name.Replace("\"", "_") + "\">" + item.name + " - " + text2 + "</Item>");
				}
				text = text.Replace("#ChoiceListrDevice#", stringBuilder.ToString());
			}
		}
		catch (Exception)
		{
		}
		text = text.Replace("#ChoiceListrDevice#", "<Item Value=\"\">Не указанно</Item>");
		LoadParametsFromXML(text);
		string paramsXML = "";
		LoadAdditionalActionsFromXML(paramsXML);
	}

	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
		Kkm.TaxVariant = "";
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			switch (unitParamet.Key)
			{
			case "TokenAppEvotor":
				TokenAppEvotor = unitParamet.Value.Trim();
				break;
			case "EvotorDevice":
			{
				string[] array = unitParamet.Value.Trim().Split('|');
				if (array.Length >= 1)
				{
					EvotorDeviceId = array[0];
				}
				else
				{
					EvotorDeviceId = "";
				}
				if (array.Length >= 2)
				{
					EvotorDeviceName = array[1];
				}
				else
				{
					EvotorDeviceName = "<Не определено>";
				}
				break;
			}
			case "NameOrganization":
				Kkm.Organization = unitParamet.Value.Trim();
				break;
			case "InnOrganization":
				Kkm.INN = unitParamet.Value.Trim();
				SettDr.INN = Kkm.INN;
				break;
			case "SnoOsn":
				Kkm.TaxVariant = (unitParamet.Value.AsBool() ? (Kkm.TaxVariant + ((Kkm.TaxVariant == "") ? "" : ",") + "0") : Kkm.TaxVariant);
				break;
			case "SnoDoh":
				Kkm.TaxVariant = (unitParamet.Value.AsBool() ? (Kkm.TaxVariant + ((Kkm.TaxVariant == "") ? "" : ",") + "1") : Kkm.TaxVariant);
				break;
			case "SnoDohRas":
				Kkm.TaxVariant = (unitParamet.Value.AsBool() ? (Kkm.TaxVariant + ((Kkm.TaxVariant == "") ? "" : ",") + "2") : Kkm.TaxVariant);
				break;
			case "SnoEnvd":
				Kkm.TaxVariant = (unitParamet.Value.AsBool() ? (Kkm.TaxVariant + ((Kkm.TaxVariant == "") ? "" : ",") + "3") : Kkm.TaxVariant);
				break;
			case "SnoEsn":
				Kkm.TaxVariant = (unitParamet.Value.AsBool() ? (Kkm.TaxVariant + ((Kkm.TaxVariant == "") ? "" : ",") + "4") : Kkm.TaxVariant);
				break;
			case "SnoPat":
				Kkm.TaxVariant = (unitParamet.Value.AsBool() ? (Kkm.TaxVariant + ((Kkm.TaxVariant == "") ? "" : ",") + "5") : Kkm.TaxVariant);
				break;
			}
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		NameDevice = "Эвотор: " + EvotorDeviceName;
		Kkm.IsKKT = true;
		Kkm.PrintingWidth = 32;
		Error = "";
		IsInit = true;
		return true;
	}

	public override async Task RegisterCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		await MarkingCodeValidationFromCheck(DataCommand, RezultCommand, InCheck: true);
		if (Error != "")
		{
			return;
		}
		if (!UnitParamets["UseBuiltTerminal"].AsBool())
		{
			DataCommand.PayByProcessing = false;
		}
		else if (UnitParamets["UseBuiltTerminal"].AsBool() && !DataCommand.PayByProcessing.HasValue)
		{
			DataCommand.PayByProcessing = true;
		}
		bool flag = DataCommand.TypeCheck == 2 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 12 || DataCommand.TypeCheck == 13;
		if (DataCommand.SenderEmail != null && DataCommand.SenderEmail != "")
		{
			Warning += "Не поддерживается передача поля SenderEmail; ";
		}
		if (DataCommand.AdditionalAttribute != null && DataCommand.AdditionalAttribute != "")
		{
			Warning += "Не поддерживается передача поля AdditionalAttribute; ";
		}
		if (DataCommand.UserAttribute != null)
		{
			Warning += "Не поддерживается передача поля UserAttribute; ";
		}
		if (DataCommand.PlaceMarket != null && DataCommand.PlaceMarket != "")
		{
			Warning += "Не поддерживается передача поля PlaceMarket; ";
		}
		if (DataCommand.AgentSign.HasValue || DataCommand.AgentData != null || DataCommand.PurveyorData != null)
		{
			Warning += "Не поддерживается передача полей агентской схемы в шапке документа ";
		}
		DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
		List<DataCommand.CheckString> list = new List<DataCommand.CheckString>();
		DataCommand.CheckString[] array = checkStrings;
		foreach (DataCommand.CheckString checkString in array)
		{
			if (checkString.PrintImage != null)
			{
				DataCommand.CheckString checkString2 = new DataCommand.CheckString();
				checkString2.PrintImage = checkString.PrintImage;
				list.Add(checkString2);
			}
			if (checkString.PrintText != null)
			{
				DataCommand.CheckString checkString3 = new DataCommand.CheckString();
				checkString3.PrintText = checkString.PrintText;
				checkString3.PrintText.Text = Unit.GetPringString(checkString3.PrintText.Text, Kkm.PrintingWidth);
				list.Add(checkString3);
			}
			if (checkString.Register != null)
			{
				if (flag)
				{
					continue;
				}
				if (checkString.Register.CountryOfOrigin != null && checkString.Register.CountryOfOrigin != "")
				{
					Warning += "Не поддерживается передача поля CountryOfOrigin для предмета расчета; ";
				}
				if (checkString.Register.CustomsDeclaration != null && checkString.Register.CustomsDeclaration != "")
				{
					Warning += "Не поддерживается передача поля CustomsDeclaration для предмета расчета; ";
				}
				if (checkString.Register.ExciseAmount.HasValue)
				{
					Warning += "Не поддерживается передача поля ExciseAmount для предмета расчета; ";
				}
				if (checkString.Register.AdditionalAttribute != null && checkString.Register.AdditionalAttribute != "")
				{
					Warning += "Не поддерживается передача поля AdditionalAttribute для предмета расчета; ";
				}
				foreach (DataCommand.Register item in SplitRegisterString(checkString))
				{
					DataCommand.CheckString checkString4 = new DataCommand.CheckString();
					checkString4.Register = new DataCommand.Register();
					checkString4.Register.Name = checkString.Register.Name;
					checkString4.Register.Quantity = item.Quantity;
					checkString4.Register.Price = item.Price;
					checkString4.Register.Amount = item.Amount;
					checkString4.Register.Department = checkString.Register.Department;
					checkString4.Register.Tax = checkString.Register.Tax;
					checkString4.Register.EAN13 = checkString.Register.EAN13;
					checkString4.Register.SignMethodCalculation = checkString.Register.SignMethodCalculation;
					checkString4.Register.SignCalculationObject = checkString.Register.SignCalculationObject;
					checkString4.Register.MeasurementUnit = checkString.Register.MeasurementUnit;
					checkString4.Register.GoodCodeData = checkString.Register.GoodCodeData;
					checkString4.Register.AgentSign = checkString.Register.AgentSign;
					checkString4.Register.AgentData = checkString.Register.AgentData;
					checkString4.Register.PurveyorData = checkString.Register.PurveyorData;
					checkString4.Register.AdditionalAttribute = checkString.Register.AdditionalAttribute;
					checkString4.Register.CountryOfOrigin = checkString.Register.CountryOfOrigin;
					checkString4.Register.CustomsDeclaration = checkString.Register.CustomsDeclaration;
					checkString4.Register.ExciseAmount = checkString.Register.ExciseAmount;
					list.Add(checkString4);
					if (item.StSkidka != "")
					{
						checkString4 = new DataCommand.CheckString();
						checkString4.PrintText = new DataCommand.PrintString();
						checkString4.PrintText.Text = item.StSkidka;
						checkString4.PrintText.Text = Unit.GetPringString(checkString4.PrintText.Text, Kkm.PrintingWidth);
						list.Add(checkString4);
					}
				}
			}
			if (checkString.BarCode != null)
			{
				DataCommand.CheckString checkString5 = new DataCommand.CheckString();
				checkString5.BarCode = checkString.BarCode;
				list.Add(checkString5);
			}
		}
		DataCommand.CheckStrings = list.ToArray();
		if (DataCommand.IsFiscalCheck && (DataCommand.TaxVariant == "" || DataCommand.TaxVariant == null))
		{
			string[] array2 = Kkm.TaxVariant.Split(',');
			DataCommand.TaxVariant = array2[0];
		}
		RunCommand(DataCommand, RezultCommand);
	}

	public override async Task OpenShift(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		new RezultCommandKKm();
		DataCommand dataCommand = new DataCommand();
		dataCommand.Command = "GetDataKKT";
		dataCommand.NumDevice = DataCommand.NumDevice;
		dataCommand.KktNumber = DataCommand.KktNumber;
		dataCommand.InnKkm = DataCommand.InnKkm;
		dataCommand.TaxVariant = DataCommand.TaxVariant;
		dataCommand.IdCommand = Guid.NewGuid().ToString();
		dataCommand.RunComPort = true;
		await GetDataKKT(dataCommand, RezultCommand);
		if (SessionOpen == 2 || SessionOpen == 3)
		{
			Error = "Смена открыта. Повторное открытие невозможно";
		}
		else
		{
			SessionOpen = 2;
		}
	}

	public override async Task CloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		Error = "";
		RunCommand(DataCommand, RezultCommand);
	}

	public override async Task XReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		RunCommand(DataCommand, RezultCommand);
	}

	public override async Task PaymentCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		RunCommand(DataCommand, RezultCommand);
	}

	public override async Task DepositingCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		RunCommand(DataCommand, RezultCommand);
	}

	public override async Task GetLineLength(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		RunCommand(DataCommand, RezultCommand);
	}

	public override async Task OpenCashDrawer(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "Команда не поддерживается Эвотором";
	}

	public override async Task GetDataKKT(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		RunCommand(DataCommand, RezultCommand);
		if (Error == "" && RezultCommand.Status == ExecuteStatus.Ok)
		{
			Kkm.PrintingWidth = RezultCommand.LineLength;
			byte b = 0;
			if (RezultCommand.Info.FFDVersion == "1.0")
			{
				b = 1;
			}
			else if (RezultCommand.Info.FFDVersion == "1.05")
			{
				b = 2;
			}
			else if (RezultCommand.Info.FFDVersion == "1.1")
			{
				b = 3;
			}
			Kkm.FfdVersion = b;
			Kkm.FfdSupportVersion = b;
			Kkm.FfdSupportVersion = b;
			Kkm.FN_Status = 3;
			Kkm.RegNumber = RezultCommand.Info.RegNumber;
			Kkm.NumberKkm = RezultCommand.Info.KktNumber;
			Kkm.FN_IsFiscal = true;
			Kkm.AddressSettle = RezultCommand.Info.AddressSettle;
			Kkm.PlaceSettle = RezultCommand.Info.PlaceSettle;
			Kkm.SignOfAgent = RezultCommand.Info.SignOfAgent;
			SessionOpen = RezultCommand.Info.SessionState;
			Kkm.DateTimeKKT = RezultCommand.Info.DateTimeKKT;
			await base.GetDataKKT(DataCommand, RezultCommand);
		}
	}

	public override async Task<uint> GetLastFiscalNumber()
	{
		Error = "Команда не поддерживается Эвотором";
		return 0u;
	}

	public override async Task<Dictionary<int, string>> GetRegisterCheck(uint FiscalNumber, Dictionary<int, Type> Types)
	{
		Error = "Команда не поддерживается Эвотором";
		return null;
	}

	public override void GetRezult(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		Error = "";
		if (RezultCommand.Status != ExecuteStatus.Ok && RezultCommand.Status != ExecuteStatus.Error)
		{
			RunCommand(DataCommand, (RezultCommandKKm)RezultCommand, OnlyReturn: true);
		}
	}

	public void RunCommand(DataCommand DataCommandIn, RezultCommandKKm RezultCommand, bool OnlyReturn = false)
	{
		if (DataCommandIn.IdCommand == "" || DataCommandIn.IdCommand == null)
		{
			DataCommandIn.IdCommand = Guid.NewGuid().ToString();
		}
		if (DataCommandIn.IdCommandInternal == "" || DataCommandIn.IdCommandInternal == null)
		{
			DataCommandIn.IdCommandInternal = Guid.NewGuid().ToString();
		}
		DataCommand dataCommand = new DataCommand();
		Unit.CopyObject(DataCommandIn, dataCommand);
		dataCommand.IdCommand = DataCommandIn.IdCommandInternal;
		GetPublisherToken();
		if (Error != "")
		{
			string error = Error;
			Error = "";
			throw new Exception(error);
		}
		byte[] buf = null;
		HttpStatusCode httpStatusCode = (HttpStatusCode)0;
		GZipStream gZipStream = null;
		if (!OnlyReturn)
		{
			string text = "";
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
			text = JsonConvert.SerializeObject(dataCommand, jsonSerializerSettings);
			byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(text);
			MemoryStream memoryStream = new MemoryStream();
			gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
			gZipStream.Write(bytes, 0, bytes.Length);
			gZipStream.Close();
			memoryStream.Close();
			buf = memoryStream.ToArray();
			httpStatusCode = HttpStatusCode.OK;
			try
			{
				HTTP_KkmServer("POST", "EvotorSave/" + EvotorDeviceId + "/" + dataCommand.IdCommand, buf, 1, out httpStatusCode);
				RezultCommand.Status = ExecuteStatus.Run;
			}
			catch (Exception ex)
			{
				Error = "Ошибка передачи команды на kkmserver.ru: " + ex.Message;
			}
			if (httpStatusCode != HttpStatusCode.OK)
			{
				Error = "Ошибка передачи команды на kkmserver.ru: " + httpStatusCode;
			}
			EvotorSendPUSH evotorSendPUSH = new EvotorSendPUSH();
			evotorSendPUSH.payload.IdCommand = dataCommand.IdCommand;
			evotorSendPUSH.payload.Command = "EvotorLoad";
			evotorSendPUSH.active_till = DateTime.Now.AddSeconds(10.0).ToString();
			string text2 = "api/apps/{application_id}/devices/{device_uuid}/push-notifications";
			text2 = text2.Replace("{application_id}", HttpUtility.UrlEncode("5ada1aac-dd2d-4df9-a09f-df1fb17eea0f"));
			text2 = text2.Replace("{device_uuid}", HttpUtility.UrlEncode(EvotorDeviceId));
			EvotorRecivePUSH evotorRecivePUSH = (EvotorRecivePUSH)HTTP_Command("POST", text2, new EvotorRecivePUSH(), evotorSendPUSH, "application/json", "", PublisherToken);
			if (evotorRecivePUSH.details.Count <= 0 || (!(evotorRecivePUSH.details[0].status == "DELIVERED") && !(evotorRecivePUSH.details[0].status == "ACCEPTED")))
			{
				_ = evotorRecivePUSH.status != "FAILED";
				throw new Exception("Ошибка передачи PUSH сообщения в облако Эвотор-а");
			}
			Thread.Sleep(1000);
		}
		httpStatusCode = HttpStatusCode.OK;
		int timeOut = 1;
		if (!OnlyReturn)
		{
			timeOut = dataCommand.Timeout;
		}
		try
		{
			buf = HTTP_KkmServer("GET", "EvotorRead/" + EvotorDeviceId + "/" + dataCommand.IdCommand, buf, timeOut, out httpStatusCode);
		}
		catch (Exception ex2)
		{
			Error = "Ошибка передачи команды на kkmserver.ru: " + ex2.Message;
			return;
		}
		switch (httpStatusCode)
		{
		case HttpStatusCode.OK:
		{
			MemoryStream memoryStream2 = new MemoryStream();
			memoryStream2.Write(buf, 0, buf.Length);
			memoryStream2.Position = 0L;
			gZipStream = new GZipStream(memoryStream2, CompressionMode.Decompress);
			int num = 1024;
			byte[] array = new byte[num];
			int num2 = 0;
			MemoryStream memoryStream3 = new MemoryStream();
			do
			{
				num2 = gZipStream.Read(array, 0, num);
				if (num2 > 0)
				{
					memoryStream3.Write(array, 0, num2);
				}
			}
			while (num2 > 0);
			buf = memoryStream3.ToArray();
			RezultCommandKKm rezultCommandKKm = (RezultCommandKKm)JsonConvert.DeserializeObject(Encoding.GetEncoding("UTF-8").GetString(buf), typeof(RezultCommandKKm));
			Error = rezultCommandKKm.Error;
			RezultCommand.Error = "";
			Warning = rezultCommandKKm.Warning;
			RezultCommand.Warning = "";
			RezultCommand.Message = rezultCommandKKm.Message;
			RezultCommand.SubRezultCommand = null;
			RezultCommand.CheckNumber = rezultCommandKKm.CheckNumber;
			RezultCommand.SessionNumber = rezultCommandKKm.SessionNumber;
			RezultCommand.SessionCheckNumber = rezultCommandKKm.SessionCheckNumber;
			RezultCommand.LineLength = rezultCommandKKm.LineLength;
			RezultCommand.QRCode = rezultCommandKKm.QRCode;
			RezultCommand.Info = rezultCommandKKm.Info;
			RezultCommand.RezultProcessing = rezultCommandKKm.RezultProcessing;
			RezultCommand.URL = Unit.GetUrlFromQRCode(rezultCommandKKm.QRCode, Kkm.InnOfd, Kkm.INN, Kkm.RegNumber);
			if (Error == "")
			{
				RezultCommand.Status = ExecuteStatus.Ok;
			}
			else
			{
				RezultCommand.Status = ExecuteStatus.Error;
				if (OnlyReturn)
				{
					RezultCommand.Error = Error;
				}
			}
			RezultCommand.Cash = rezultCommandKKm.Cash;
			RezultCommand.ElectronicPayment = rezultCommandKKm.ElectronicPayment;
			RezultCommand.Credit = rezultCommandKKm.Credit;
			RezultCommand.AdvancePayment = rezultCommandKKm.AdvancePayment;
			RezultCommand.CashProvision = rezultCommandKKm.CashProvision;
			IsNotErrorStatus = true;
			break;
		}
		case HttpStatusCode.MethodNotAllowed:
			if (RezultCommand.GetType() == typeof(RezultCommandKKm))
			{
				RezultCommand.SubRezultCommand = new RezultCommandKKm();
			}
			else if (RezultCommand.GetType() == typeof(RezultCommandProcessing))
			{
				RezultCommand.SubRezultCommand = new RezultCommandProcessing();
			}
			Unit.CopyObject(RezultCommand, RezultCommand.SubRezultCommand);
			RezultCommand.SubRezultCommand.Status = ExecuteStatus.NotRun;
			RezultCommand.Status = ExecuteStatus.Run;
			Error = RezultCommand.Error;
			RezultCommand.Error = "";
			IsNotErrorStatus = false;
			break;
		case HttpStatusCode.NotAcceptable:
			if (RezultCommand.GetType() == typeof(RezultCommandKKm))
			{
				RezultCommand.SubRezultCommand = new RezultCommandKKm();
			}
			else if (RezultCommand.GetType() == typeof(RezultCommandProcessing))
			{
				RezultCommand.SubRezultCommand = new RezultCommandProcessing();
			}
			Unit.CopyObject(RezultCommand, RezultCommand.SubRezultCommand);
			RezultCommand.SubRezultCommand.Status = ExecuteStatus.Run;
			RezultCommand.Status = ExecuteStatus.Run;
			Error = RezultCommand.Error;
			RezultCommand.Error = "";
			IsNotErrorStatus = false;
			break;
		default:
			Error = "Ошибка получения результата на kkmserver.ru: " + httpStatusCode;
			break;
		}
	}

	public void GetPublisherToken()
	{
		if (PublisherToken == "")
		{
			try
			{
				PublisherToken = (string)HTTP_Command("GET", "EvotorPublisherToken", "", null, "", "", "", KkmServerURI);
				PublisherToken = PublisherToken.Replace("\0", "");
			}
			catch (Exception ex)
			{
				Error = "Ошибка получения PublisherToken: " + ex.Message;
			}
		}
	}

	public object HTTP_Command(string Method = "GET", string Command = "", object tObject = null, object Object = null, string ContentType = "", string Accept = "", string Authorization = "", string URI = "https://api.evotor.ru/")
	{
		if (Authorization == "")
		{
			Authorization = TokenAppEvotor;
		}
		string text = "";
		if (Object != null)
		{
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
			text = JsonConvert.SerializeObject(Object, jsonSerializerSettings);
		}
		HttpClient httpClient = new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false,
			ServerCertificateCustomValidationCallback = (HttpRequestMessage Request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
		});
		if (Accept != "")
		{
			httpClient.DefaultRequestHeaders.Accept.TryParseAdd(ContentType);
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", ContentType);
		}
		httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Authorization);
		Task<HttpResponseMessage> task = null;
		try
		{
			if (Method == "POST")
			{
				ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
				if (ContentType != "")
				{
					byteArrayContent.Headers.TryAddWithoutValidation("Content-Type", ContentType);
				}
				task = httpClient.PostAsync(URI + Command, byteArrayContent);
			}
			else
			{
				task = httpClient.GetAsync(URI + Command);
			}
			task.Wait();
		}
		catch (Exception ex)
		{
			throw new Exception("Ошибка вызова облака эвотор: " + Global.GetInnerErrorMessagee(ex.InnerException));
		}
		Task<string> task2 = task.Result.Content.ReadAsStringAsync();
		task2.Wait();
		text = task2.Result;
		if (task.Result.StatusCode != HttpStatusCode.OK && task.Result.StatusCode != HttpStatusCode.Accepted)
		{
			string text2 = "";
			switch (task.Result.StatusCode)
			{
			case HttpStatusCode.Forbidden:
				text2 = " Нет доступа. Ошибка возникает когда у приложения нет разрешения на запрашиваемое действие или пользователь не установил приложение в Личном кабинете.";
				break;
			case HttpStatusCode.PaymentRequired:
				text2 = " Приложение не оплачено и/или не установленно на всех смарт-терминалах, привязанных к данному магазину. Сообщение содержит идентификатор магазина, в котором возникла ошибка.";
				break;
			case HttpStatusCode.Accepted:
				text2 = " Асинхронное выполнение операции. В ответе должен присутствовать заголовок Location со значением /resource-tasks/:task-id.";
				break;
			case HttpStatusCode.Created:
				text2 = " Если выполнение синхронной операции удачно и конечно, а реализация не хочет ничего возвращать в ответе, - в теле ответа должен присутствовать заголовок Location. Если делался запрос на операцию с одним элементом - то в ответе должен присутствовать заголовок Location со ссылкой на этот элемент: /resources/:resource-id. Если запрос выполнялся на коллекцию, то Location: /resources.";
				break;
			}
			try
			{
				List<EvotorError> list = JsonConvert.DeserializeObject<List<EvotorError>>(text);
				if (list != null && list.Count > 0)
				{
					throw new Exception(task.Result.StatusCode.ToString() + ": " + list[0].code + "-" + list[0].message + text2);
				}
				throw new Exception("");
			}
			catch
			{
				if (text != null)
				{
					throw new Exception(task.Result.StatusCode.ToString() + ": " + text + text2);
				}
				throw new Exception(task.Result.StatusCode.ToString() + ": Ошибка вызова облака эвотор" + text2);
			}
		}
		object obj2 = null;
		if (tObject.GetType() == typeof(string))
		{
			return text;
		}
		return JsonConvert.DeserializeObject(text, tObject.GetType());
	}

	public byte[] HTTP_KkmServer(string Method, string Command, byte[] Buf, int TimeOut, out HttpStatusCode ErrorCode)
	{
		string text = "";
		string text2 = "";
		for (int i = 0; i < 10; i++)
		{
			try
			{
				text = ComDevice.QueryToServ("base/hs/lic/GetIdSession", 5000).Result;
				if (text != null && text != "")
				{
					text2 = ComDevice.DeShifrovka(text);
					break;
				}
			}
			catch
			{
			}
			Thread.Sleep(100);
		}
		if (text2.Length != 16)
		{
			throw new Exception("Ошибка вызова сервиса на kkmserver.ru");
		}
		string text3 = ComDevice.Shifrovka("KkmServerAuth", text2);
		HttpClient httpClient = new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false,
			ServerCertificateCustomValidationCallback = (HttpRequestMessage Request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
		});
		httpClient.DefaultRequestHeaders.TryAddWithoutValidation("TypeAlgr", "Ver1");
		if (TimeOut == 0)
		{
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("TimeOut", "60");
		}
		else
		{
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("TimeOut", TimeOut.ToString());
			httpClient.Timeout = TimeSpan.FromMilliseconds((Math.Max(10, TimeOut) + 10) * 1000);
		}
		Task<HttpResponseMessage> task = null;
		try
		{
			if (Method == "POST")
			{
				ByteArrayContent byteArrayContent = new ByteArrayContent(Buf);
				byteArrayContent.Headers.TryAddWithoutValidation("Content-Type", text3);
				task = httpClient.PostAsync(KkmServerURI + Command, byteArrayContent);
			}
			else
			{
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", text3);
				task = httpClient.GetAsync(KkmServerURI + Command);
			}
			task.Wait();
		}
		catch (Exception ex)
		{
			throw new Exception("Ошибка вызова облака эвотор: " + Global.GetInnerErrorMessagee(ex.InnerException));
		}
		Task<byte[]> task2 = task.Result.Content.ReadAsByteArrayAsync();
		task2.Wait();
		byte[] result = task2.Result;
		ErrorCode = task.Result.StatusCode;
		return result;
	}
}
