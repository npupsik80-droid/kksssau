using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KkmFactory;

internal class GateSbpTinkoff : UnitPort
{
	private class IntRequest
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string OrderId = "";

		[DataMember]
		public decimal Amount;

		[DataMember]
		public string Token = "";

		[DataMember]
		public DateTime RedirectDueDate;

		[DataMember]
		public Dictionary<string, string> DATA = new Dictionary<string, string>();
	}

	private class IntAnswer
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string OrderId = "";

		[DataMember]
		public string PaymentId = "";

		[DataMember]
		public decimal Amount;

		[DataMember]
		public bool Success;

		[DataMember]
		public string Status = "";

		[DataMember]
		public string ErrorCode = "";

		[DataMember]
		public string Message = "";

		[DataMember]
		public string Details = "";
	}

	private class GetQrRequest
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string PaymentId = "";

		[DataMember]
		public string DataType = "";

		[DataMember]
		public string Token = "";
	}

	private class GetQrAnswer
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string OrderId = "";

		[DataMember]
		public bool Success;

		[DataMember]
		public string Data = "";

		[DataMember]
		public string PaymentId = "";

		[DataMember]
		public string ErrorCode = "";

		[DataMember]
		public string Message = "";

		[DataMember]
		public string Details = "";
	}

	private class CancelRequest
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string PaymentId = "";

		[DataMember]
		public string Token = "";
	}

	private class CancelAnswer
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string OrderId = "";

		[DataMember]
		public bool Success;

		[DataMember]
		public string PaymentId = "";

		[DataMember]
		public string Status = "";

		[DataMember]
		public string ErrorCode = "";

		[DataMember]
		public string Message = "";

		[DataMember]
		public string Details = "";

		[DataMember]
		public decimal NewAmount;
	}

	private class GetQrStateRequest
	{
		[DataMember]
		public string TerminalKey = "";

		[DataMember]
		public string PaymentId = "";

		[DataMember]
		public string Token = "";
	}

	private class GetQrStateAnswer
	{
		[DataMember]
		public string OrderId = "";

		[DataMember]
		public bool Success;

		[DataMember]
		public string Status = "";

		[DataMember]
		public string ErrorCode = "";

		[DataMember]
		public string Message = "";

		[DataMember]
		public string Details = "";
	}

	private decimal OldAmount;

	private string OldUniversalID = "";

	private int Width = 36;

	private string TerminalKey = "";

	private string Password = "";

	private DateTime DateChange = DateTime.Now;

	private bool PrintOnKkt = true;

	private bool PrintOnWindow = true;

	private int TimeOut = 60;

	private const string UrlService = "https://securepay.tinkoff.ru/v2/";

	private List<Unit> ListUnits;

	private X509Certificate2 Cert;

	public GateSbpTinkoff(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		IsCommandCancelled = true;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n        <Group Caption='Настройки доступа к системе СБП'>\r\n        <Parameter Name=\"Width\" Caption=\"Ширина чека\" TypeValue=\"String\" DefaultValue=\"36\">\r\n                <ChoiceList>\r\n                    <Item Value=\"48\">48</Item>\r\n                    <Item Value=\"42\">42</Item>\r\n                    <Item Value=\"36\">36</Item>\r\n                    <Item Value=\"32\">32</Item>\r\n                </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"TerminalKey\" Caption=\"Идентификатор терминала (TerminalKey)\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Будет доступен на сайте после подключения к системе СБП.\"/>\r\n        <Parameter Name=\"Password\" Caption=\"Пароль на терминал\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Будет доступен на сайте после подключения к системе СБП.\"/>\r\n\r\n        </Group>\r\n    </Page>\r\n    <Page Caption='Параметры'> \r\n        <Parameter Name=\"TimeOut\" Caption=\"Вемя ожидания оплаты (секунд)\" TypeValue=\"Number\" DefaultValue=\"70\"/>\r\n        <Parameter Name=\"DateChange\" Caption=\"Дата открытия смены\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n    </Page>\r\n    <Page Caption='Вывод QR кода для сканирование клиента'> \r\n        <Parameter Name=\"PrintOnKkt\" Caption=\"Печать на ККТ\" TypeValue=\"Boolean\" DefaultValue=\"true\"/>\r\n        <Parameter Name=\"PrintOnWindow\" Caption=\"Печать на экране\" TypeValue=\"Boolean\" DefaultValue=\"true\"/>\r\n    </Page>\r\n</Settings>";
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "Система быстрых платежей (СБП) по QR коду из приложения на телефоне";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		NameDevice = "СБП Тинькофф";
		UnitAdditionallinks = "<a href='https://sbp.nspk.ru/business/'>О системе быстрых платежей</a> - Общая информация<br/>\r\n                                <a href='https://www.tinkoff.ru/kassa/solution/qr/'>Регистрация СБП в банке</a><br/>";
		text = text.Replace("'", "\"");
		LoadParametsFromXML(text);
	}

	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			switch (unitParamet.Key)
			{
			case "Width":
				Width = unitParamet.Value.AsInt();
				break;
			case "TerminalKey":
				TerminalKey = unitParamet.Value.Trim();
				break;
			case "Password":
				Password = unitParamet.Value.Trim();
				break;
			case "PrintOnKkt":
				PrintOnKkt = unitParamet.Value.AsBool();
				break;
			case "PrintOnWindow":
				PrintOnWindow = unitParamet.Value.AsBool();
				break;
			case "TimeOut":
				TimeOut = unitParamet.Value.AsInt();
				break;
			case "DateChange":
				try
				{
					DateChange = unitParamet.Value.AsDateTime();
				}
				catch
				{
				}
				break;
			}
		}
	}

	public override void SaveParametrs(Dictionary<string, string> NewParamets)
	{
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		IsInit = true;
		return true;
	}

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		ListUnits = null;
		DateTime StartTime = DateTime.Now;
		StringBuilder Slip = new StringBuilder();
		string OrderStateName = "";
		string orderId = Guid.NewGuid().ToString().Replace("-", "");
		Slip.Append(Unit.GetPringString("<<=>>", Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("ИНН:<#0#>" + Kkm.INN, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("Терминал:<#0#>" + TerminalKey, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
		PortLogs.Append("Start");
		string PngBase64 = null;
		string TextQR = null;
		string TextRez = "";
		switch (Command)
		{
		case 0:
		{
			if (DataCommand.Amount < 100m)
			{
				Error = "Сумма оплаты должна быть не менее 100 руб";
				return;
			}
			Unit.WindowTrackingStatus(DataCommand, this, "Создание id оплаты... ", TextQR, PngBase64);
			OutputOnCustomerDisplay("Получение QR оплаты... ", DataCommand.Amount + " руб.");
			PortLogs.Append("Запрос на создание платежа оплаты");
			DateTime redirectDueDate = StartTime.AddSeconds(120.0);
			if (TimeOut < 130)
			{
				TimeOut = 130;
			}
			_ = TimeOut;
			_ = 30;
			IntRequest intRequest = new IntRequest
			{
				TerminalKey = TerminalKey,
				OrderId = orderId,
				Amount = (ulong)DataCommand.Amount * 100,
				RedirectDueDate = redirectDueDate
			};
			intRequest.DATA.Add("QR", "true");
			SetToken(intRequest);
			IntAnswer intAnswer = (IntAnswer)QueryToServ("Init", intRequest, typeof(IntAnswer), " Создание id оплаты");
			if (CreateTextError(intAnswer.ErrorCode, intAnswer.Message, intAnswer.Details))
			{
				return;
			}
			string status = intAnswer.Status;
			string PaymentId = intAnswer.PaymentId;
			PortLogs.Append("Статус на создание id оплаты = " + status, "<");
			if (status != "NEW")
			{
				throw new Exception("Ошибка вызова сервиса создания платежа СБП: " + status);
			}
			Unit.WindowTrackingStatus(DataCommand, this, "Получение QR оплаты... ", TextQR, PngBase64);
			PortLogs.Append("Запрос на создание QR");
			GetQrRequest getQrRequest = new GetQrRequest
			{
				TerminalKey = TerminalKey,
				PaymentId = PaymentId,
				DataType = "PAYLOAD"
			};
			SetToken(getQrRequest);
			GetQrAnswer GetQrAnswer = (GetQrAnswer)QueryToServ("GetQr", getQrRequest, typeof(GetQrAnswer), "Получение QR оплаты");
			if (CreateTextError(GetQrAnswer.ErrorCode, GetQrAnswer.Message, GetQrAnswer.Details))
			{
				return;
			}
			RezultCommand.ReceiptNumber = PaymentId;
			RezultCommand.TerminalID = TerminalKey;
			RezultCommand.Url = GetQrAnswer.Data;
			RezultCommand.Amount = DataCommand.Amount;
			status = GetQrAnswer.ErrorCode;
			PortLogs.Append("Статус на создание QR = " + status, "<");
			if (!GetQrAnswer.Success)
			{
				throw new Exception("Ошибка вызова сервиса создания QR платежа СБП: " + status);
			}
			Slip.Append(Unit.GetPringString(">#2#<ОПЛАТА", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Система:<#0#>Тинькофф банк СБП", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Номер:<#0#>" + PaymentId, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("ID QR:<#0#>" + PaymentId, Width, "\r\n") + "\r\n");
			if (PrintOnWindow)
			{
				PortLogs.Append("Выводим QR код для СБП на экран");
				if (DataCommand.RunAsAddIn)
				{
					TextQR = "ОТСКАНИРУЙТЕ КОД ИЗ МОБИЛЬНОГО ПРИЛОЖЕНИЯ";
					PngBase64 = BarCode.GetImageBarCode("QR", GetQrAnswer.Data, 400, 400).PngBase64;
				}
				else
				{
					WriteLine(Unit.GetPringString("<<=>>", Width));
					WriteLine(Unit.GetPringString(">#2#<QR КОД ОПЛАТЫ", Width));
					ImageBarCode imageBarCode = BarCode.GetImageBarCode("QR", GetQrAnswer.Data, 200, 200);
					WriteLine(imageBarCode, 0, Clear: false, null, AsCheck: true);
					WriteLine(Unit.GetPringString(">#2#<ОТСКАНИРУЙТЕ КОД ИЗ МОБИЛЬНОГО ПРИЛОЖЕНИЯ", Width));
					WriteLine(Unit.GetPringString("Система:<#0#>Тинькофф банк СБП", Width));
					WriteLine(Unit.GetPringString("Номер:<#0#>" + PaymentId, Width));
					WriteLine(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width));
					WriteLine(Unit.GetPringString("<<=>>", Width));
					WriteLine("", 0, Clear: false, true, AsCheck: true);
					RezultCommand.TypeMessageHTM = "TrackingMessage";
				}
			}
			if (PrintOnKkt)
			{
				PortLogs.Append("Печатаем QR код для СБП на ККТ");
				int num = int.Parse(SettDr.Paramets["NumDeviceByPrintSlip"]);
				if (num == 0)
				{
					foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
					{
						if ((device.Value.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && Kkm.INN == "") || device.Value.INN == Kkm.INN)
						{
							num = device.Value.NumDevice;
							break;
						}
					}
				}
				DataCommand dataCommand = new DataCommand();
				dataCommand.Command = "RegisterCheck";
				dataCommand.IsFiscalCheck = false;
				dataCommand.NumDevice = num;
				dataCommand.RunComPort = true;
				List<DataCommand.CheckString> list = new List<DataCommand.CheckString>();
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("<<=>>", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(">#2#<QR КОД ОПЛАТЫ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(" ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					BarCode = new DataCommand.PrintBarcode
					{
						BarcodeType = "QR",
						Barcode = GetQrAnswer.Data
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(" ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(">#2#<ОТСКАНИРУЙТЕ КОД ИЗ МОБИЛЬНОГО ПРИЛОЖЕНИЯ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("Система:<#0#>Тинькофф банк СБП", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("Номер:<#0#>" + PaymentId, Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("<<=>>", Width)
					}
				});
				dataCommand.CheckStrings = list.ToArray();
				try
				{
					Unit unit = Global.UnitManager.Units[num];
					RezultCommand rezultCommand = new RezultCommandKKm();
					await unit.ExecuteCommand(dataCommand, rezultCommand);
				}
				catch
				{
				}
			}
			await Task.Delay(3000);
			Unit.WindowTrackingStatus(DataCommand, this, "Ожидение оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
			OutputOnCustomerDisplay("Отсканируйте код для оплаты...", DataCommand.Amount + " руб.", GetQrAnswer.Data);
			await Task.Delay(7000);
			Unit.WindowTrackingStatus(DataCommand, this, "Проверка оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
			while (true)
			{
				try
				{
					Error = "";
					GetQrStateRequest getQrStateRequest2 = new GetQrStateRequest
					{
						TerminalKey = TerminalKey,
						PaymentId = PaymentId
					};
					SetToken(getQrStateRequest2);
					GetQrStateAnswer getQrStateAnswer2 = (GetQrStateAnswer)QueryToServ("GetQrState", getQrStateRequest2, typeof(GetQrStateAnswer), "проверку оплаты... ");
					if (CreateTextError(getQrStateAnswer2.ErrorCode, getQrStateAnswer2.Message, getQrStateAnswer2.Details))
					{
						throw new Exception(Error);
					}
					status = getQrStateAnswer2.Status;
					if (status == "CONFIRMED" || status == "REJECTED")
					{
						break;
					}
					PortLogs.Append("Статус запроса = " + status, "<");
					Unit.WindowTrackingStatus(DataCommand, this, "Проверка статуса оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
					goto IL_0d98;
				}
				catch
				{
					Unit.WindowTrackingStatus(DataCommand, this, "Ошибка проверка статуса... " + GetTimeEnd(StartTime), TextQR, PngBase64);
					goto IL_0d98;
				}
				IL_0d98:
				if (StartTime.AddSeconds(TimeOut + 5) < DateTime.Now || CancellationCommand)
				{
					status = "EXPIRED";
					break;
				}
				await Task.Delay(7000);
			}
			PortLogs.Append("Запрос статуса QR = '" + TextRez + "'");
			switch (status)
			{
			case "CONFIRMED":
				OrderStateName = "Оплачено";
				break;
			case "REJECTED":
				Error = "Счет отменен";
				break;
			case "EXPIRED":
				Error = "Счет просрочен";
				break;
			case "REFUNDED":
				Error = "Счет возвращен";
				break;
			case "AUTHORIZED":
				Error = "Зарезервировано (1)";
				break;
			default:
				Error = status;
				break;
			}
			break;
		}
		case 1:
		case 2:
		{
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			string PaymentId = DataCommand.ReceiptNumber;
			if (PaymentId == null || PaymentId == "")
			{
				throw new Exception("Не указан RN отменяемой транзакции");
			}
			PortLogs.Append("Запрос на возврат оплаты QR");
			string text = "";
			switch (Command)
			{
			case 1:
				text = "ВОЗВРАТ ОПЛАТЫ";
				break;
			case 2:
				text = "ОТМЕНА ОПЛАТЫ";
				break;
			}
			CancelRequest cancelRequest = new CancelRequest
			{
				TerminalKey = TerminalKey,
				PaymentId = PaymentId
			};
			SetToken(cancelRequest);
			CancelAnswer cancelAnswer = (CancelAnswer)QueryToServ("Cancel", cancelRequest, typeof(CancelAnswer), "Запрос на возврат оплаты QR");
			if (CreateTextError(cancelAnswer.ErrorCode, cancelAnswer.Message, cancelAnswer.Details))
			{
				throw new Exception(Error);
			}
			string status = cancelAnswer.Status;
			RezultCommand.ReceiptNumber = PaymentId;
			RezultCommand.TerminalID = TerminalKey;
			RezultCommand.Amount = DataCommand.Amount;
			PortLogs.Append("Статус запроса на возврат оплаты QR = " + status, "<");
			Slip.Append(Unit.GetPringString(">#2#<" + text, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Система:<#0#>Тинькофф банк СБП", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("ID QR:<#0#>" + DataCommand.ReceiptNumber, Width, "\r\n") + "\r\n");
			PortLogs.Append("Запрос статуса QR = '" + TextRez + "'");
			while (true)
			{
				try
				{
					Error = "";
					GetQrStateRequest getQrStateRequest = new GetQrStateRequest
					{
						TerminalKey = TerminalKey,
						PaymentId = PaymentId
					};
					SetToken(getQrStateRequest);
					GetQrStateAnswer getQrStateAnswer = (GetQrStateAnswer)QueryToServ("GetQrState", getQrStateRequest, typeof(GetQrStateAnswer), "Запрос статуса QR'");
					if (CreateTextError(getQrStateAnswer.ErrorCode, getQrStateAnswer.Message, getQrStateAnswer.Details))
					{
						throw new Exception(Error);
					}
					status = getQrStateAnswer.Status;
					if (status == "REFUNDED" || status == "REJECTED")
					{
						break;
					}
					PortLogs.Append("Статус запроса = " + status, "<");
					Unit.WindowTrackingStatus(DataCommand, this, "Проверка статуса оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
					goto IL_12b6;
				}
				catch
				{
					Unit.WindowTrackingStatus(DataCommand, this, "Ошибка проверка статуса... " + GetTimeEnd(StartTime), TextQR, PngBase64);
					goto IL_12b6;
				}
				IL_12b6:
				if (StartTime.AddSeconds(TimeOut + 60) < DateTime.Now || CancellationCommand)
				{
					status = "EXPIRED";
					break;
				}
				await Task.Delay(5000);
			}
			switch (status)
			{
			case "REFUNDED":
				OrderStateName = "Возвращено";
				break;
			case "REJECTED":
				Error = "Операция отменена";
				break;
			case "EXPIRED":
				Error = "Операция просрочена";
				break;
			case "AUTHORIZED":
				Error = "Зарезервировано (1)";
				break;
			default:
				Error = status;
				break;
			}
			break;
		}
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			new StringBuilder();
			return;
		}
		PortLogs.Append("End");
		if (Error != "")
		{
			Slip.Append(Unit.GetPringString("Статус:<#0#>" + Error, Width, "\r\n") + "\r\n");
			RezultCommand.Status = ExecuteStatus.Error;
		}
		else
		{
			Slip.Append(Unit.GetPringString("Статус:<#0#>" + OrderStateName, Width, "\r\n") + "\r\n");
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		Slip.Append(Unit.GetPringString("<<=>>", Width, "\r\n") + "\r\n");
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		RezultCommand.Slip = Slip.ToString();
		TextLines.Clear();
		if (Command == 0)
		{
			OldAmount = RezultCommand.Amount;
			OldUniversalID = RezultCommand.UniversalID;
		}
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.UniversalID == "")
		{
			DataCommand.UniversalID = OldUniversalID;
		}
		await CommandPayTerminal(DataCommand, RezultCommand, 2);
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await TerminalReport(DataCommand, RezultCommand);
		DateChange = DateTime.Now;
		SettDr.Paramets["DateChange"] = DateChange.ToString("yyyy.MM.dd HH:mm:ss");
		await Global.SaveSettingsAsync();
	}

	private void OutputOnCustomerDisplay(string TopString, string BottomString, string CodeQR = null)
	{
		DataCommand DataCommandCustomerDisplay = new DataCommand();
		DataCommandCustomerDisplay.NumDevice = int.Parse(UnitParamets["NumDeviceCustomerDisplay"]);
		DataCommandCustomerDisplay.NoError = DataCommandCustomerDisplay.NumDevice == 0;
		DataCommandCustomerDisplay.Command = "OutputOnCustomerDisplay";
		DataCommandCustomerDisplay.TopString = TopString;
		DataCommandCustomerDisplay.BottomString = BottomString;
		DataCommandCustomerDisplay.CodeQR = CodeQR;
		if (ListUnits == null)
		{
			List<Unit> ListSortUnits = Global.UnitManager.Units.Select(delegate(KeyValuePair<int, Unit> u)
			{
				KeyValuePair<int, Unit> keyValuePair = u;
				return keyValuePair.Value;
			}).ToList();
			ListUnits = UnitManager.GetListUnitsForCommand(DataCommandCustomerDisplay, TypeDevice.enType.ДисплеиПокупателя, ref ListSortUnits);
		}
		if (ListUnits.Count <= 0)
		{
			return;
		}
		Task.Run(async delegate
		{
			string textCommand = JsonConvert.SerializeObject(DataCommandCustomerDisplay, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			});
			try
			{
				await Global.UnitManager.AddCommand(DataCommandCustomerDisplay, "", textCommand);
			}
			catch (Exception)
			{
			}
		});
	}

	public void PrintBarCode(string Barcode)
	{
		ImageBarCode imageBarCode = null;
		imageBarCode = BarCode.GetImageBarCode("QR", Barcode, 200, 200);
		if (imageBarCode != null)
		{
			WriteLine(imageBarCode, 0, Clear: false, null, AsCheck: true);
		}
	}

	private string GetTimeEnd(DateTime StartTime)
	{
		return "отмена через " + (StartTime.AddSeconds(TimeOut) - DateTime.Now).ToString("m\\:ss") + " с";
	}

	private void SetToken(object IntRequest)
	{
		SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
		FieldInfo[] fields = IntRequest.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.Name != "Shops" && fieldInfo.Name != "Receipt" && fieldInfo.Name != "DATA" && fieldInfo.Name != "Token")
			{
				sortedDictionary.Add(fieldInfo.Name, fieldInfo.GetValue(IntRequest).ToString());
			}
		}
		sortedDictionary.Add("Password", Password);
		string text = "";
		foreach (KeyValuePair<string, string> item in sortedDictionary)
		{
			text += item.Value;
		}
		text = Tlv.GetHexString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text))).ToLower();
		IntRequest.GetType().GetField("Token").SetValue(IntRequest, text);
	}

	private object QueryToServ(string UrlQuery, object body, Type TypeRez, string NmeCommandForLog)
	{
		string text = "https://securepay.tinkoff.ru/v2/" + UrlQuery;
		HttpClient httpClient = new HttpClient();
		httpClient.Timeout = TimeSpan.FromMilliseconds(20000.0);
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";
		string text2 = JsonConvert.SerializeObject(body, jsonSerializerSettings);
		PortLogs.Append("Запрос на " + NmeCommandForLog + " = '" + text2 + "'");
		ByteArrayContent byteArrayContent = new ByteArrayContent(new UTF8Encoding().GetBytes(text2));
		byteArrayContent.Headers.TryAddWithoutValidation("Content-Type", "application/json");
		string text3 = null;
		Task<HttpResponseMessage> task = null;
		Task<string> task2 = null;
		try
		{
			task = httpClient.PostAsync(text, byteArrayContent);
			task.Wait();
			task2 = task.Result.Content.ReadAsStringAsync();
			task2.Wait();
		}
		catch (AggregateException)
		{
			text3 = "Сервер не отвечает (таймаут)";
			PortLogs.Append("Ошибка вызова сервиса СБП, UrlQuery = " + UrlQuery + ": " + text3, "<");
			throw new Exception("Ошибка вызова сервиса аутенфикации СБП: " + text3);
		}
		catch (Exception ex2)
		{
			text3 = Global.GetInnerErrorMessagee(ex2.InnerException);
			try
			{
				task2 = task.Result.Content.ReadAsStringAsync();
				task2.Wait();
				text3 = text3 + "<br/>" + task2.Result;
			}
			catch
			{
			}
			PortLogs.Append("Ошибка вызова сервиса СБП, UrlQuery = " + UrlQuery + ": " + text3, "<");
			throw new Exception("Ошибка вызова сервиса СБП: " + text3);
		}
		if (task.Result.StatusCode != HttpStatusCode.OK)
		{
			string text4 = task.Result.StatusCode.ToString();
			PortLogs.Append("Ошибка вызова сервиса СБП, UrlQuery = " + UrlQuery + ": " + text4, "<");
			throw new Exception("Ошибка вызова сервиса СБП: " + text4);
		}
		if (task.Result.StatusCode == HttpStatusCode.OK)
		{
			text3 = task2.Result;
			PortLogs.Append("Ответ на " + NmeCommandForLog + " = '" + text3 + "'");
			return JsonConvert.DeserializeObject(text3, TypeRez, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			});
		}
		return null;
	}

	private bool CreateTextError(string CodeError, string error_message, string error_description)
	{
		if (CodeError != "0")
		{
			if (error_message != "" && error_message != null)
			{
				Error += error_message;
			}
			if (error_description != "" && error_description != null)
			{
				if (error_message != "" && error_message != null)
				{
					Error += " ";
				}
				Error += error_description;
			}
			return true;
		}
		return false;
	}
}
