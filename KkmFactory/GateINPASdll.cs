using System.Threading.Tasks;
using DualConnector;
using Newtonsoft.Json;

namespace KkmFactory;

internal class GateINPASdll : Unit
{
	private DCLink dclink;

	private bool DllIsLosd;

	private readonly GateINPAS GateINPAS;

	public GateINPASdll(Global.DeviceSettings SettDr, int NumUnit, GateINPAS GateINPAS)
		: base(SettDr, NumUnit)
	{
		this.GateINPAS = GateINPAS;
	}

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		try
		{
			dclink = new DCLink();
		}
		catch
		{
			GateINPAS.Error = "Не удалось инициализировать компоненту.";
			return;
		}
		DllIsLosd = true;
		ISAPacket val = (ISAPacket)new SAPacket();
		ISAPacket val2 = (ISAPacket)new SAPacket();
		val.Amount = ((ulong)(DataCommand.Amount * 100m)).ToString();
		val.CurrencyCode = "643";
		switch (Command)
		{
		case 0:
			val.OperationCode = 1;
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			val.OperationCode = 29;
			val.ReferenceNumber = DataCommand.RRNCode;
			val.AuthorizationCode = DataCommand.AuthorizationCode;
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			val.OperationCode = 4;
			val.ReferenceNumber = DataCommand.RRNCode;
			val.AuthorizationCode = DataCommand.AuthorizationCode;
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		val.TerminalID = GateINPAS.TerminalID;
		int errorByte = dclink.InitResources();
		if (IsCommandBad(RezultCommand, errorByte, 1, "Ошибка инициализации устройства"))
		{
			return;
		}
		errorByte = dclink.Exchange(ref val, ref val2, 180000);
		AddLogs(errorByte, val2);
		RezultCommand.Slip = GetSlip(val2);
		if (IsCommandBad(RezultCommand, errorByte, val2.Status, dclink.ErrorDescription))
		{
			if (val2 != null && val2.TextResponse != null && val2.TextResponse != "")
			{
				GateINPAS.Error = val2.TextResponse + ": " + GateINPAS.Error;
			}
			return;
		}
		DllIsLosd = false;
		dclink.Dispose();
		RezultCommand.CardNumber = val2.PAN;
		RezultCommand.ReceiptNumber = val2.CommodityCode;
		RezultCommand.RRNCode = val2.ReferenceNumber;
		RezultCommand.AuthorizationCode = val2.AuthorizationCode;
		RezultCommand.Amount = decimal.Parse(val2.Amount) / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		try
		{
			dclink = new DCLink();
		}
		catch
		{
			GateINPAS.Error = "Не удалось инициализировать компоненту.";
			return;
		}
		DllIsLosd = true;
		ISAPacket val = (ISAPacket)new SAPacket();
		ISAPacket val2 = (ISAPacket)new SAPacket();
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		val.Amount = ((ulong)(DataCommand.Amount * 100m)).ToString();
		val.CurrencyCode = "643";
		val.OperationCode = 53;
		val.TerminalID = GateINPAS.TerminalID;
		int errorByte = dclink.InitResources();
		if (IsCommandBad(RezultCommand, errorByte, 1, "Ошибка инициализации устройства"))
		{
			return;
		}
		errorByte = dclink.Exchange(ref val, ref val2, 180000);
		AddLogs(errorByte, val2);
		RezultCommand.Slip = GetSlip(val2);
		if (IsCommandBad(RezultCommand, errorByte, val2.Status, dclink.ErrorDescription))
		{
			if (val2 != null && val2.TextResponse != null && val2.TextResponse != "")
			{
				GateINPAS.Error = val2.TextResponse + ": " + GateINPAS.Error;
			}
			return;
		}
		DllIsLosd = false;
		dclink.Dispose();
		RezultCommand.CardNumber = val2.PAN;
		RezultCommand.ReceiptNumber = val2.CommodityCode;
		RezultCommand.RRNCode = val2.ReferenceNumber;
		RezultCommand.AuthorizationCode = val2.AuthorizationCode;
		RezultCommand.Amount = decimal.Parse(val2.Amount) / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		try
		{
			dclink = new DCLink();
		}
		catch
		{
			GateINPAS.Error = "Не удалось инициализировать компоненту.";
			return;
		}
		DllIsLosd = true;
		int errorByte = dclink.InitResources();
		if (IsCommandBad(RezultCommand, errorByte, 1, "Ошибка инициализации устройства"))
		{
			return;
		}
		ISAPacket val = (ISAPacket)new SAPacket();
		ISAPacket val2 = (ISAPacket)new SAPacket();
		val.OperationCode = 59;
		val.TerminalID = GateINPAS.TerminalID;
		errorByte = dclink.Exchange(ref val, ref val2, 180000);
		AddLogs(errorByte, val2);
		try
		{
			RezultCommand.Slip = GetSlip(val2, Del2Slip: false);
		}
		catch
		{
		}
		if (IsCommandBad(RezultCommand, errorByte, val2.Status, dclink.ErrorDescription))
		{
			if (val2 != null && val2.TextResponse != null && val2.TextResponse != "")
			{
				GateINPAS.Error = val2.TextResponse + ": " + GateINPAS.Error;
			}
			return;
		}
		DllIsLosd = false;
		dclink.Dispose();
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = val2.CommodityCode;
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.Amount = default(decimal);
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		try
		{
			dclink = new DCLink();
		}
		catch
		{
			GateINPAS.Error = "Не удалось инициализировать компоненту.";
			return;
		}
		DllIsLosd = true;
		int errorByte = dclink.InitResources();
		if (IsCommandBad(RezultCommand, errorByte, 1, "Ошибка инициализации устройства"))
		{
			return;
		}
		ISAPacket val = (ISAPacket)new SAPacket();
		ISAPacket val2 = (ISAPacket)new SAPacket();
		val.OperationCode = 63;
		val.TerminalID = GateINPAS.TerminalID;
		if (!DataCommand.Detailed)
		{
			val.CommandMode2 = 20;
		}
		else if (DataCommand.Detailed)
		{
			val.CommandMode2 = 21;
		}
		errorByte = dclink.Exchange(ref val, ref val2, 180000);
		AddLogs(errorByte, val2);
		try
		{
			RezultCommand.Slip = GetSlip(val2, Del2Slip: false);
		}
		catch
		{
		}
		if (IsCommandBad(RezultCommand, errorByte, val2.Status, dclink.ErrorDescription))
		{
			if (val2 != null && val2.TextResponse != null && val2.TextResponse != "")
			{
				GateINPAS.Error = val2.TextResponse + ": " + GateINPAS.Error;
			}
			return;
		}
		DllIsLosd = false;
		dclink.Dispose();
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = val2.CommodityCode;
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.Amount = default(decimal);
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
	}

	public override async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		try
		{
			dclink = new DCLink();
		}
		catch
		{
			GateINPAS.Error = "Не удалось инициализировать компоненту.";
			return;
		}
		DllIsLosd = true;
		ISAPacket val = (ISAPacket)new SAPacket();
		ISAPacket val2 = (ISAPacket)new SAPacket();
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		val.OperationCode = 63;
		val.CommandMode2 = 22;
		val.ReferenceNumber = DataCommand.RRNCode;
		val.AuthorizationCode = DataCommand.AuthorizationCode;
		val.TerminalID = GateINPAS.TerminalID;
		int errorByte = dclink.InitResources();
		if (IsCommandBad(RezultCommand, errorByte, 1, "Ошибка инициализации устройства"))
		{
			return;
		}
		errorByte = dclink.Exchange(ref val, ref val2, 180000);
		AddLogs(errorByte, val2);
		RezultCommand.Slip = GetSlip(val2);
		if (IsCommandBad(RezultCommand, errorByte, val2.Status, dclink.ErrorDescription))
		{
			if (val2 != null && val2.TextResponse != null && val2.TextResponse != "")
			{
				GateINPAS.Error = val2.TextResponse + ": " + GateINPAS.Error;
			}
			return;
		}
		DllIsLosd = false;
		dclink.Dispose();
		RezultCommand.CardNumber = val2.PAN;
		RezultCommand.ReceiptNumber = val2.CommodityCode;
		RezultCommand.RRNCode = val2.ReferenceNumber;
		RezultCommand.AuthorizationCode = val2.AuthorizationCode;
		RezultCommand.Amount = decimal.Parse(val2.Amount) / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "Драйвер не поддерживает эту команду";
	}

	private string GetSlip(ISAPacket response, bool Del2Slip = true)
	{
		if (response != null && response.ReceiptData != null)
		{
			string text = response.ReceiptData;
			if (text.IndexOf("0xDF") == 0 && text.Length >= 5)
			{
				text = text.Substring(4);
			}
			if (text.IndexOf("^") == 0 && text.Length >= 2)
			{
				text = text.Substring(1);
			}
			if (text.IndexOf("^") == 0 && text.Length >= 2)
			{
				text = text.Substring(1);
			}
			if (text.IndexOf("^") == 0 && text.Length >= 2)
			{
				text = text.Substring(1);
			}
			if (!Del2Slip)
			{
				text = text.Replace("~0xDA^^", "\r\n");
			}
			if (text.IndexOf("~") != -1)
			{
				text = text.Substring(0, text.IndexOf("~") - 1);
			}
			return text;
		}
		return "";
	}

	public override void Test()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		try
		{
			dclink = new DCLink();
		}
		catch
		{
			GateINPAS.Error = "Не удалось инициализировать устройство";
			return;
		}
		DllIsLosd = true;
		int errorByte = dclink.InitResources();
		if (!IsCommandBad(null, errorByte, 1, "Ошибка инициализации устройства"))
		{
			DllIsLosd = false;
			dclink.Dispose();
		}
	}

	private bool IsCommandBad(RezultCommand RezultCommand, int ErrorByte, int StatusByte = 1, string ErrorText = "")
	{
		if (GateINPAS.Error != "")
		{
			if (DllIsLosd)
			{
				dclink.Dispose();
				DllIsLosd = false;
			}
			if (RezultCommand != null)
			{
				RezultCommand.Status = ExecuteStatus.Error;
				if (ErrorText != "")
				{
					GateINPAS.Error = ErrorText + " (" + GateINPAS.Error + ")";
				}
			}
			return true;
		}
		if (CreateTextError(ErrorByte, StatusByte, ErrorText))
		{
			if (DllIsLosd)
			{
				dclink.Dispose();
				dclink = null;
				DllIsLosd = false;
			}
			if (RezultCommand != null)
			{
				RezultCommand.Status = ExecuteStatus.Error;
			}
			return true;
		}
		return false;
	}

	private bool CreateTextError(int ErrorByte, int StatusByte = 1, string ErrorText = "")
	{
		if (ErrorByte == 0 && (StatusByte == 1 || StatusByte == 17))
		{
			return false;
		}
		string text = "";
		if (ErrorByte != 0)
		{
			text = "Неизвестный код ошибки";
		}
		switch (ErrorByte)
		{
		case 1:
			text = "Истёк таймаут операции";
			break;
		case 2:
			text = "Ошибка создания LOG файла";
			break;
		case 3:
			text = "Общая ошибка";
			break;
		case 4:
			text = "Ошибка данных запроса";
			break;
		case 6:
			text = "Не найден файл конфигурации";
			break;
		case 7:
			text = "Ошибка формата файла конфигурации";
			break;
		case 8:
			text = "Ошибка параметров логирования";
			break;
		case 9:
			text = "Ошибка в параметрах терминала";
			break;
		case 10:
			text = "Ошибка настройки устройства на COM порт";
			break;
		case 11:
			text = "Ошибка в выходных параметрах";
			break;
		case 12:
			text = "Ошибка при передаче образа чека";
			break;
		case 13:
			text = "Ошибка установки связи с устройством";
			break;
		case 14:
			text = "Ошибка в параметрах настройки интерфейса взаимодействия с пользователем.";
			break;
		}
		string text2 = "";
		switch (StatusByte)
		{
		case 16:
			text2 = "Отказано";
			break;
		case 34:
			text2 = "Нет соединения";
			break;
		case 53:
			text2 = "Операция прервана";
			break;
		}
		GateINPAS.Error = "";
		if (ErrorByte != 0 || StatusByte != 1)
		{
			if (ErrorText != "")
			{
				text2 = text2 + ((text2 == "") ? "" : ": ") + ErrorText;
			}
			if (text != "")
			{
				GateINPAS.Error = text;
			}
			if (text2 != "")
			{
				GateINPAS.Error = GateINPAS.Error + ((GateINPAS.Error == "") ? "" : " ( ") + text2 + ((GateINPAS.Error == "") ? "" : ")");
			}
		}
		if (ErrorByte == 0)
		{
			return StatusByte != 1;
		}
		return true;
	}

	private void AddLogs(int res, ISAPacket response)
	{
		try
		{
			string data = JsonConvert.SerializeObject(new
			{
				Rezult = res,
				Response = response
			}, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			});
			GateINPAS.PortLogs.Append(data);
		}
		catch
		{
		}
	}
}
