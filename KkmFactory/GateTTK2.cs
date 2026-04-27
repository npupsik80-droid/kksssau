using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateTTK2 : UnitPort
{
	private class SbrTlv : Tlv
	{
		public static InfoTeg TAG_TTK_CLIENT_MID = new InfoTeg(1, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_CLIENT_ECR = new InfoTeg(2, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_CLIENT_ERN = new InfoTeg(3, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_CLIENT_AMT = new InfoTeg(4, TagDataType.Int, TagDataFormat.BCD);

		public static InfoTeg TAG_TTK_TRX_MODE = new InfoTeg(8, TagDataType.Int, TagDataFormat.Bin_LitEndian);

		public static InfoTeg TAG_TTK_CLIENT_AUT_CODE = new InfoTeg(12, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_TRACK2 = new InfoTeg(23, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_CLIENT_RRN = new InfoTeg(24, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_OPER_TYPE = new InfoTeg(26, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_DEPARTMENT_INDEX = new InfoTeg(24322, TagDataType.Int, TagDataFormat.Bin_LitEndian, 1);

		public static InfoTeg TAG_TTK_LLT_ID = new InfoTeg(24323, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_UPOS_MESSAGE = new InfoTeg(24325, TagDataType.String, (TagDataFormat)866);

		public static InfoTeg TAG_TTK_CARD_EXPDATE = new InfoTeg(24326, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_PILOT_OPER_TYPE = new InfoTeg(24327, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_CARD_TYPE = new InfoTeg(24333, TagDataType.RawData);

		public static InfoTeg TAG_TTK_REQUEST_ID = new InfoTeg(24330, TagDataType.Int, TagDataFormat.Bin_LitEndian, 4);

		public static InfoTeg TAG_TTK_TRX_FLAGS = new InfoTeg(24332, TagDataType.Int, TagDataFormat.Bin_BigEndian, 4);

		public static InfoTeg TAG_TTK_SERVER_AMT_C = new InfoTeg(24371, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_ENCDATA = new InfoTeg(24377, TagDataType.RawData);

		public static InfoTeg TAG_TTK_CURRENCY = new InfoTeg(27, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_BUFFER_SIZE = new InfoTeg(62, TagDataType.Int, TagDataFormat.BCD);

		public static InfoTeg TAG_TTK_COMMAND = new InfoTeg(64, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_DATA = new InfoTeg(65, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_MID = new InfoTeg(129, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_ECR = new InfoTeg(130, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_ERN = new InfoTeg(131, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_AMT = new InfoTeg(132, TagDataType.Int, TagDataFormat.BCD);

		public static InfoTeg TAG_TTK_PAN = new InfoTeg(137, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_TSN = new InfoTeg(139, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_AUT_CODE = new InfoTeg(140, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_DATE = new InfoTeg(141, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_TIME = new InfoTeg(142, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_ISSUER_NAME = new InfoTeg(143, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_MERCHANT_ID = new InfoTeg(144, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_RRN = new InfoTeg(152, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_TERMINAL_ID = new InfoTeg(157, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_RESPONSE_CODE = new InfoTeg(155, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_CHEQUE_IN_ASCII = new InfoTeg(156, TagDataType.String, (TagDataFormat)866);

		public static InfoTeg TAG_TTK_CHEQUE_IN_PDS = new InfoTeg(158, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_ERROR_TEXT = new InfoTeg(160, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_APPROVE = new InfoTeg(161, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_HASH = new InfoTeg(163, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_IS_OWN = new InfoTeg(164, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_ERROR_CODE = new InfoTeg(165, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_STATE_CODE = new InfoTeg(7936, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_STATE_DATA = new InfoTeg(7937, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_STATE = new InfoTeg(8048, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_SERVER_MSG = new InfoTeg(8049, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_TIMEOUT = new InfoTeg(8060, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAG_TTK_EXTRA_DATA = new InfoTeg(25, TagDataType.RawData);

		public static InfoTeg TAG_CASHOUT = new InfoTeg(57168, TagDataType.Int, TagDataFormat.Bin_LitEndian, 4);

		public static InfoTeg TAG_CUR_ID = new InfoTeg(24362, TagDataType.Int, TagDataFormat.Bin_BigEndian, 4);

		public static InfoTeg TAG_TTK_CARD_DATA = new InfoTeg(24398, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAGM_IMEI = new InfoTeg(209, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAGM_ICCID = new InfoTeg(210, TagDataType.String, (TagDataFormat)1251);

		public static InfoTeg TAGM_TCPDEVICE = new InfoTeg(211, TagDataType.RawData);

		public override void SetSettings()
		{
			TlvSettings = ETlvSettings.Standart;
		}
	}

	private int VerAPI;

	private decimal OldAmmount;

	private string OldAuthCode = "";

	private string OldRRNCode = "";

	private string OldCardID = "";

	public string Slip = "";

	private string KodCut = "~S";

	private uint Department;

	private bool WriteTTK2Sign;

	public GateTTK2(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		IsCommandCancelled = true;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n        <Group Caption='Протокол обмена'>\r\n            <Parameter Name=\"VerAPI\" Caption=\"Версия API\" TypeValue=\"Number\" DefaultValue=\"12\">\r\n                <ChoiceList>\r\n                    <Item Value=\"0\">ТТК-2 протокол</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n        <Group Caption='Настройки порта'>\r\n        <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\" DefaultValue=\"1\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"IP\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\" DefaultValue=\"\"/>\r\n        <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"888\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name='WriteTTK2Sign' Caption='Передавать признак ТТК2' TypeValue='Boolean' DefaultValue='false' />\r\n        </Group>\r\n        <Group Caption='Прочие настройки'>\r\n        <Parameter Name='СимволыОтреза' Caption='Символы отреза' TypeValue='String' DefaultValue='~S' />\r\n        <Parameter Name='Department' Caption='Отдел по умолчанию' TypeValue='Number' DefaultValue='0'/>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "TTK2: Эквайринговые терминалы";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		UnitVersion = Global.Verson;
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		NameDevice = "TTK2: Платежный терминал";
		text = text.Replace("'", "\"");
		LoadParametsFromXML(text);
		string paramsXML = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                <Actions>\r\n                    <Action Name=\"ServiceMenu\" Caption=\"Войти в техническое меню\"/> \r\n                </Actions>";
		LoadAdditionalActionsFromXML(paramsXML);
	}

	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			switch (unitParamet.Key)
			{
			case "VerAPI":
				VerAPI = unitParamet.Value.AsInt();
				break;
			case "TypeConnect":
				if (unitParamet.Value == "1")
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.IP;
				}
				else if (unitParamet.Value == "2")
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.Com;
				}
				else
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.None;
				}
				break;
			case "IP":
				SetPort.IP = unitParamet.Value.Trim();
				break;
			case "Port":
				SetPort.Port = unitParamet.Value.Trim();
				break;
			case "ComId":
				SetPort.ComId = unitParamet.Value.Trim();
				break;
			case "ComSpeed":
				SetPort.ComSpeed = unitParamet.Value.AsInt();
				break;
			case "WriteTTK2Sign":
				WriteTTK2Sign = unitParamet.Value.AsBool();
				break;
			case "СимволыОтреза":
				KodCut = unitParamet.Value;
				break;
			case "Department":
				Department = unitParamet.Value.AsUInt();
				break;
			}
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		try
		{
			if (!(await TestPinpad()))
			{
				Error = "Пинпад не подключен: " + Error;
				IsInit = false;
				return false;
			}
		}
		catch
		{
		}
		IsFullInitDate = DateTime.Now;
		IsInit = true;
		return true;
	}

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		SbrTlv sbrTlv = new SbrTlv();
		switch (Command)
		{
		case 0:
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "PUR");
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "REF");
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_RRN, DataCommand.RRNCode, AddEmpty: true);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_AUT_CODE, DataCommand.AuthorizationCode, AddEmpty: true);
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "VOI");
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_RRN, DataCommand.RRNCode, AddEmpty: true);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_AUT_CODE, DataCommand.AuthorizationCode, AddEmpty: true);
			break;
		case 3:
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "AUT");
			break;
		case 4:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "AUH");
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_RRN, DataCommand.RRNCode, AddEmpty: true);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_AUT_CODE, DataCommand.AuthorizationCode, AddEmpty: true);
			break;
		case 5:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "CMP");
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_RRN, DataCommand.RRNCode, AddEmpty: true);
			sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_AUT_CODE, DataCommand.AuthorizationCode, AddEmpty: true);
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		DateTime now = DateTime.Now;
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ECR, NumUnit.ToString());
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ERN, DateTime.Now.ToString("ddHHmmss"));
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_AMT, (ulong)(DataCommand.Amount * 100m));
		sbrTlv.Add(SbrTlv.TAG_TTK_DEPARTMENT_INDEX, DataCommand.Department.HasValue ? DataCommand.Department : new uint?(0u));
		sbrTlv.Add(SbrTlv.TAG_TTK_BUFFER_SIZE, 65000);
		sbrTlv.Add(SbrTlv.TAG_TTK_REQUEST_ID, (ulong)(((long)now.Day << 8 << 8 << 8) | ((long)now.Hour << 8 << 8) | ((long)now.Hour << 8 << 8) | ((long)now.Minute << 8) | now.Second));
		SbrTlv sbrTlv2 = await RunCommandTTK2(sbrTlv, DataCommand, (DataCommand.Timeout >= 30) ? DataCommand.Timeout : 110);
		string obj = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_APPROVE);
		string text = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_RESPONSE_CODE);
		if (!(obj == "Y"))
		{
			switch (text)
			{
			default:
			{
				string visualResponse = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_ERROR_TEXT);
				if (IsCommandBad(null, text, visualResponse, "Ошибка операции: "))
				{
					return;
				}
				break;
			}
			case "":
			case "0":
			case "00":
				break;
			}
		}
		RezultCommand.CardNumber = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_PAN);
		RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
		RezultCommand.RRNCode = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_SERVER_RRN);
		RezultCommand.CardDPAN = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_CARD_DATA);
		RezultCommand.AuthorizationCode = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_SERVER_AUT_CODE);
		RezultCommand.Slip = GetSlipSBRF((string)sbrTlv2.Find(SbrTlv.TAG_TTK_CHEQUE_IN_ASCII));
		try
		{
			RezultCommand.Amount = (decimal)sbrTlv2.Find(SbrTlv.TAG_TTK_SERVER_AMT) / 100m;
		}
		catch
		{
			RezultCommand.Amount = decimal.Parse((string)sbrTlv2.Find(SbrTlv.TAG_TTK_SERVER_AMT_C)) / 100m;
		}
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		RezultCommand.CardHash = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_HASH);
		RezultCommand.TerminalID = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_TERMINAL_ID);
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		OldAmmount = RezultCommand.Amount;
		OldAuthCode = RezultCommand.AuthorizationCode;
		OldRRNCode = RezultCommand.RRNCode;
		OldCardID = RezultCommand.CardNumber;
		IsInit = true;
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		DataCommand.RRNCode = OldRRNCode;
		DataCommand.AuthorizationCode = OldAuthCode;
		DataCommand.CardNumber = OldCardID;
		DataCommand.Amount = OldAmmount;
		await ProcessCommandPayTerminal(DataCommand, RezultCommand, 2);
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		SbrTlv sbrTlv = new SbrTlv();
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "SRV");
		sbrTlv.Add(SbrTlv.TAG_TTK_OPER_TYPE, "2");
		_ = DateTime.Now;
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ECR, NumUnit.ToString());
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ERN, DateTime.Now.ToString("ddHHmmss"));
		sbrTlv.Add(SbrTlv.TAG_TTK_DEPARTMENT_INDEX, DataCommand.Department.HasValue ? DataCommand.Department : new uint?(0u));
		SbrTlv sbrTlv2 = await RunCommandTTK2(sbrTlv, DataCommand, (DataCommand.Timeout >= 30) ? DataCommand.Timeout : 110);
		string obj = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_APPROVE);
		string text = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_RESPONSE_CODE);
		if (!(obj == "Y"))
		{
			switch (text)
			{
			default:
			{
				string visualResponse = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_ERROR_TEXT);
				if (IsCommandBad(null, text, visualResponse, "Ошибка операции: "))
				{
					return;
				}
				break;
			}
			case "":
			case "0":
			case "00":
				break;
			}
		}
		RezultCommand.Slip = GetSlipSBRF((string)sbrTlv2.Find(SbrTlv.TAG_TTK_CHEQUE_IN_ASCII));
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		RezultCommand.TerminalID = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_TERMINAL_ID);
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		IsInit = true;
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		SbrTlv sbrTlv = new SbrTlv();
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "SRV");
		if (!DataCommand.Detailed)
		{
			sbrTlv.Add(SbrTlv.TAG_TTK_OPER_TYPE, "6");
		}
		else if (DataCommand.Detailed)
		{
			sbrTlv.Add(SbrTlv.TAG_TTK_OPER_TYPE, "6");
		}
		_ = DateTime.Now;
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ECR, NumUnit.ToString());
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ERN, DateTime.Now.ToString("ddHHmmss"));
		sbrTlv.Add(SbrTlv.TAG_TTK_DEPARTMENT_INDEX, DataCommand.Department.HasValue ? DataCommand.Department : new uint?(0u));
		SbrTlv sbrTlv2 = await RunCommandTTK2(sbrTlv, DataCommand, (DataCommand.Timeout >= 30) ? DataCommand.Timeout : 110);
		string obj = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_APPROVE);
		string text = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_RESPONSE_CODE);
		if (!(obj == "Y"))
		{
			switch (text)
			{
			default:
			{
				string visualResponse = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_ERROR_TEXT);
				if (IsCommandBad(null, text, visualResponse, "Ошибка операции: "))
				{
					return;
				}
				break;
			}
			case "":
			case "0":
			case "00":
				break;
			}
		}
		RezultCommand.Slip = GetSlipSBRF((string)sbrTlv2.Find(SbrTlv.TAG_TTK_CHEQUE_IN_ASCII));
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		RezultCommand.TerminalID = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_TERMINAL_ID);
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		IsInit = true;
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		if (DataCommand.AdditionalActions == "ServiceMenu")
		{
			ServiceMenu(DataCommand, RezultCommand).Wait();
		}
		else
		{
			base.DoAdditionalAction(DataCommand, ref RezultCommand);
		}
	}

	private async Task ServiceMenu(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		SbrTlv sbrTlv = new SbrTlv();
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "SRV");
		sbrTlv.Add(SbrTlv.TAG_TTK_OPER_TYPE, "C");
		_ = DateTime.Now;
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ECR, NumUnit.ToString());
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ERN, DateTime.Now.ToString("ddHHmmss"));
		SbrTlv sbrTlv2 = await RunCommandTTK2(sbrTlv, DataCommand, (DataCommand.Timeout >= 30) ? DataCommand.Timeout : 110);
		string text = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_APPROVE);
		string text2 = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_RESPONSE_CODE);
		if (!(text == "Y"))
		{
			switch (text2)
			{
			default:
			{
				string visualResponse = (string)sbrTlv2.Find(SbrTlv.TAG_TTK_ERROR_TEXT);
				if (IsCommandBad(null, text2, visualResponse, "Ошибка операции: "))
				{
					return;
				}
				break;
			}
			case "":
			case "0":
			case "00":
				break;
			}
		}
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "";
		IsInit = true;
	}

	private async Task<bool> TestPinpad()
	{
		SbrTlv sbrTlv = new SbrTlv();
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_MID, "SRV");
		sbrTlv.Add(SbrTlv.TAG_TTK_OPER_TYPE, "B");
		_ = DateTime.Now;
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ECR, NumUnit.ToString());
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_ERN, DateTime.Now.ToString("ddHHmmss"));
		sbrTlv.Add(SbrTlv.TAG_TTK_CLIENT_AMT, 100uL);
		sbrTlv.Add(SbrTlv.TAG_TTK_TRACK2, "00000000");
		sbrTlv.Add(SbrTlv.TAG_TTK_BUFFER_SIZE, 65000);
		try
		{
			await RunCommandTTK2(sbrTlv, null, 5);
		}
		catch (Exception ex)
		{
			Error = ex.Message;
			return false;
		}
		return true;
	}

	private string GetSlipSBRF(string Text)
	{
		string text = Text;
		if (KodCut.Length != 0 && text.IndexOf(KodCut) > 10)
		{
			text = text.Substring(0, text.IndexOf(KodCut));
		}
		return text.ToString();
	}

	private async Task<SbrTlv> RunCommandTTK2(SbrTlv Msg, DataCommand DataCommand, int TimeOut = 30)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			return null;
		}
		CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
		System.Threading.CancellationToken token = cancelTokenSource.Token;
		SbrTlv result;
		try
		{
			base.PortReadTimeout = TimeOut * 1000;
			base.PortWriteTimeout = 15000;
			int ColInf = 0;
			await SendFrameTTK2(Msg);
			DateTime start = DateTime.Now;
			new Task(async delegate
			{
				do
				{
					await Task.Delay(500);
					if ((DateTime.Now - start).TotalSeconds >= (double)(TimeOut - 3))
					{
						cancelTokenSource.Cancel();
						CancellationCommand = true;
					}
					else if (token.IsCancellationRequested)
					{
						return;
					}
				}
				while (!CancellationCommand);
				SbrTlv sbrTlv2 = new SbrTlv();
				sbrTlv2.Add(SbrTlv.TAG_TTK_CLIENT_MID, "ABR");
				_ = DateTime.Now;
				sbrTlv2.Add(SbrTlv.TAG_TTK_CLIENT_ECR, NumUnit.ToString());
				sbrTlv2.Add(SbrTlv.TAG_TTK_CLIENT_ERN, DateTime.Now.ToString("ddHHmmss"));
				sbrTlv2.Add(SbrTlv.TAG_TTK_BUFFER_SIZE, 65000);
				await SendFrameTTK2(sbrTlv2);
			}, token).Start();
			SbrTlv sbrTlv;
			while (true)
			{
				sbrTlv = await GetFrameTTK2(TimeOut * 1000);
				if (sbrTlv == null)
				{
					if ((DateTime.Now - start).TotalSeconds > (double)TimeOut)
					{
						string text = "Ошибка приема кадра сообщения";
						PortLogs.Append(text);
						throw new Exception(text);
					}
				}
				else if ((string)sbrTlv.Find(SbrTlv.TAG_TTK_SERVER_MID) == "INF")
				{
					string text2 = (string)sbrTlv.Find(SbrTlv.TAG_TTK_SERVER_MSG);
					GateTTK2 unit = this;
					int num = ColInf + 1;
					ColInf = num;
					Unit.WindowTrackingStatus(DataCommand, unit, num + ": " + text2 + "...");
				}
				else if ((string)sbrTlv.Find(SbrTlv.TAG_TTK_SERVER_MID) == (string)Msg.Find(SbrTlv.TAG_TTK_CLIENT_MID))
				{
					break;
				}
			}
			result = sbrTlv;
		}
		finally
		{
			cancelTokenSource.Cancel();
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
		}
		return result;
	}

	private async Task<bool> SendFrameTTK2(SbrTlv Msg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			byte[] array = Msg.ToArray(stringBuilder);
			int num;
			byte[] PrtMsg;
			if (WriteTTK2Sign)
			{
				num = array.Length + 2;
				PrtMsg = new byte[num];
				PrtMsg[0] = 150;
				PrtMsg[1] = 242;
				array.CopyTo(PrtMsg, 2);
			}
			else
			{
				num = array.Length;
				PrtMsg = array;
			}
			PortLogs.Append(stringBuilder.ToString(), "<");
			byte[] array2 = new byte[2]
			{
				(byte)((num >> 8) & 0xFF),
				(byte)(num & 0xFF)
			};
			await PortWriteAsync(array2, 0, array2.Length);
			await PortWriteAsync(PrtMsg, 0, PrtMsg.Length);
		}
		catch
		{
			Error = "Ошибка передачи данных";
			PortLogs.Append(Error);
			if (Global.Settings.SetNotActiveOnError)
			{
				IsInit = false;
			}
			return false;
		}
		return true;
	}

	private async Task<SbrTlv> GetFrameTTK2(int TimeOut = 30000)
	{
		Error = "";
		StringBuilder ParcerLog = new StringBuilder();
		MemoryStream Data = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(Data);
		base.PortReadTimeout = TimeOut;
		uint SizeFrame;
		try
		{
			SizeFrame = (uint)((await PortReadByteAsync() << 8) | await PortReadByteAsync());
		}
		catch (Exception)
		{
			return null;
		}
		for (int i = 0; i < SizeFrame; i++)
		{
			byte Response;
			try
			{
				Response = await PortReadByteAsync();
			}
			catch (Exception ex2)
			{
				Error = "Ошибка приема кадра сообщения (2)";
				PortLogs.Append(Error);
				throw new Exception(ex2.Message);
			}
			bw.Write(Response);
		}
		SbrTlv sbrTlv;
		try
		{
			sbrTlv = new SbrTlv();
			byte[] array = Data.ToArray();
			if (!WriteTTK2Sign)
			{
				sbrTlv.ParceArray(array, ParcerLog);
			}
			else if (WriteTTK2Sign && array[0] == 151 && array[1] == 242)
			{
				sbrTlv.ParceArray(new Span<byte>(array).Slice(2), ParcerLog);
			}
			else if (WriteTTK2Sign)
			{
				throw new Exception("Сбойный пакет от терминала");
			}
			PortLogs.Append(ParcerLog.ToString());
		}
		catch (Exception ex3)
		{
			Error = "Ошибка парсинга сообщения (3)";
			PortLogs.Append(Error);
			throw new Exception(ex3.Message);
		}
		return sbrTlv;
	}

	private bool IsCommandBad(RezultCommand RezultCommand, string ErrorString, string VisualResponse, string ErrorText = "")
	{
		if (Error != "")
		{
			if (RezultCommand != null)
			{
				RezultCommand.Status = ExecuteStatus.Error;
				if (ErrorText != "")
				{
					Error = ErrorText + " (" + Error + ")";
				}
			}
			return true;
		}
		switch (ErrorString)
		{
		case "":
		case "0":
		case "00":
			return false;
		default:
			if (VisualResponse != null && VisualResponse != "")
			{
				Error = ErrorString + " : " + VisualResponse;
				if (ErrorText != "")
				{
					Error = ErrorText + " ( " + Error + " )";
				}
				return true;
			}
			if (ErrorString != null)
			{
				CreateTextError(ErrorString, ErrorText);
				return true;
			}
			return false;
		}
	}

	private bool CreateTextError(string ErrorString, string TextError = "")
	{
		uint num = 0u;
		string text = "Неизвестный код ошибки";
		try
		{
			num = uint.Parse(ErrorString);
			switch (num)
			{
			case 0u:
				return false;
			case 1u:
				text = "";
				break;
			case 99u:
				text = "Нарушился контакт с пинпадом, либо невозможно открыть указанный СОМ-порт (он или отсутствует в системе, или захвачен другой программой).";
				break;
			case 361u:
			case 362u:
			case 363u:
			case 364u:
				text = "Нарушился контакт с чипом карты. Чип не читается. Попробовать вставить другую карту. Если ошибка возникает на всех картах – неисправен чиповый ридер пинпада.";
				break;
			case 403u:
				text = "Клиент ошибся при вводе ПИНа";
				break;
			case 405u:
				text = "ПИН клиента заблокирован";
				break;
			case 444u:
				text = "Истек срок действия карты";
				break;
			case 507u:
				text = "Истек срок действия карты";
				break;
			case 518u:
				text = "На терминале установлена неверная дата";
				break;
			case 521u:
				text = "На карте недостаточно средств";
				break;
			case 572u:
				text = "Истек срок действия карты";
				break;
			case 574u:
				text = "Карта заблокирована";
				break;
			case 579u:
				text = "Карта заблокирована";
				break;
			case 584u:
			case 585u:
				text = "Истек период обслуживания карты (СБЕРКАРТ)";
				break;
			case 705u:
			case 707u:
				text = "Карта заблокирована (СБЕРКАРТ)";
				break;
			case 708u:
			case 709u:
				text = "ПИН клиента заблокирован (СБЕРКАРТ)";
				break;
			case 2000u:
				text = "Операция прервана нажатием клавиши ОТМЕНА. Другая возможная причина – не проведена предварительная сверка итогов, и на терминале еще нет сеансовых ключей.";
				break;
			case 2002u:
				text = "Клиент слишком долго вводит ПИН. Истек таймаут.";
				break;
			case 2004u:
			case 2005u:
			case 2006u:
			case 2007u:
			case 2405u:
			case 2406u:
			case 2407u:
				text = "Карта заблокирована (СБЕРКАРТ)";
				break;
			case 3001u:
				text = "Недостаточно средств для загрузки на карту (СБЕРКАРТ)";
				break;
			case 3019u:
			case 3020u:
			case 3021u:
				text = "На сервере проводятся регламентные работы (СБЕРКАРТ)";
				break;
			case 4100u:
				text = "Нет связи с банком при удаленной загрузке. Возможно, на терминале неверно задан параметр «Код региона и участника для удаленной загрузки».";
				break;
			case 4101u:
			case 4102u:
				text = "Карта терминала не проинкассирована";
				break;
			case 4103u:
			case 4104u:
				text = "Ошибка обмена с чипом карты";
				break;
			case 4108u:
				text = "Неправильно введен или прочитан номер карты (ошибка контрольного разряда)";
				break;
			case 4110u:
			case 4111u:
			case 4112u:
				text = "Требуется проинкассировать карту терминала (СБЕРКАРТ)";
				break;
			case 4113u:
			case 4114u:
				text = "Превышен лимит, допустимый без связи с банком (СБЕРКАРТ)";
				break;
			case 4115u:
				text = "Ручной ввод для таких карт запрещен";
				break;
			case 4117u:
				text = "Клиент отказался от ввода ПИНа";
				break;
			case 4119u:
				text = "Нет связи с банком. Другая возможная причина – неверный ключ KLK для пинпада Verifone pp1000se или встроенного пинпада Verifone. Если терминал Verifone работает по Ethernet, то иногда избавиться от ошибки можно, понизив скорость порта с 115200 до 57600 бод.";
				break;
			case 4120u:
				text = "В пинпаде нет ключа KLK.";
				break;
			case 4121u:
				text = "Ошибка файловой структуры терминала. Невозможно записать файл BTCH.D.";
				break;
			case 4122u:
				text = "Ошибка смены ключей: либо на хосте нет нужного KLK, либо в настройках терминала указан неверный мерчант.";
				break;
			case 4123u:
				text = "На терминале нет сеансовых ключей";
				break;
			case 4124u:
				text = "На терминале нет мастер-ключей";
				break;
			case 4125u:
				text = "На карте есть чип, а прочитана была магнитная полоса";
				break;
			case 4128u:
				text = "Неверный МАС — код при сверке итогов. Вероятно, неверный ключ KLK.";
				break;
			case 4130u:
				text = "Память терминала заполнена. Пора делать сверку итогов (лучше несколько раз подряд, чтобы почистить старые отчеты).";
				break;
			case 4131u:
				text = "Установлен тип пинпада РС-2, но с момента последней прогрузки параметров пинпад был заменен (изменился его серийный номер). Необходимо повторно прогрузить TLV-файл или выполнить удаленную загрузку.";
				break;
			case 4132u:
				text = "Операция отклонена картой. Возможно, карту вытащили из чипового ридера до завершения печати чека. Повторить операцию заново. Если ошибка возникает постоянно, возможно, карта неисправна.";
				break;
			case 4134u:
				text = "Слишком долго не выполнялась сверка итогов на терминале (прошло более 5 дней с момента последней операции).";
				break;
			case 4135u:
				text = "Нет SAM-карты для выбранного отдела (СБЕРКАРТ)";
				break;
			case 4136u:
				text = "Требуется более свежая версия прошивки в пинпаде.";
				break;
			case 4137u:
				text = "Ошибка при повторном вводе нового ПИНа.";
				break;
			case 4138u:
				text = "Номер карты получателя не может совпадать с номером карты отправителя.";
				break;
			case 4139u:
				text = "В настройках терминала нет ни одного варианта связи, пригодного для требуемой операции.";
				break;
			case 4140u:
				text = "Неверно указаны сумма или код авторизации в команде SUSPEND из кассовой программы.";
				break;
			case 4141u:
				text = "Невозможно выполнить команду SUSPEND: не найден файл SHCN.D.";
				break;
			case 4142u:
				text = "Не удалось выполнить команду ROLLBACK из кассовой прграммы.";
				break;
			case 4143u:
				text = "На терминале слишком старый стоп-лист.";
				break;
			case 4144u:
			case 4145u:
			case 4146u:
			case 4147u:
				text = "Неверный формат стоп-листа на терминале (для торговли в самолете без авторизации).";
				break;
			case 4148u:
				text = "Карта в стоп-листе.";
				break;
			case 4149u:
				text = "На карте нет фамилии держателя.";
				break;
			case 4150u:
				text = "Превышен лимит, допустимый без связи с банком (для торговли на борту самолета без авторизации).";
				break;
			case 4151u:
				text = "Истек срок действия карты (для торговли на борту самолета без авторизации).";
				break;
			case 4152u:
				text = "На карте нет списка транзакций (ПРО100).";
				break;
			case 4153u:
				text = "Список транзакций на карте имеет неизвестный формат (ПРО100).";
				break;
			case 4154u:
				text = "Невозможно распечатать список транзакций карты, потому что его можно считать только с чипа, а прочитана магнитная полоса (ПРО100).";
				break;
			case 4155u:
				text = "Список транзакций пуст (ПРО100).";
				break;
			case 4160u:
				text = "Неверный ответ от карты при считывании биометрических данных";
				break;
			case 4161u:
				text = "На терминале нет файла с биометрическим сертификатом BSCP.CR";
				break;
			case 4162u:
			case 4163u:
			case 4164u:
				text = "Ошибка расшифровки биометрического сертификата карты. Возможно, неверный файл BSCP.CR";
				break;
			case 4165u:
			case 4166u:
			case 4167u:
				text = "Ошибка взаимной аутентификации биосканера и карты. Возможно, неверный файл BSCP.CR";
				break;
			case 4168u:
			case 4169u:
				text = "Ошибка расшифровки шаблонов пальцев, считанных с карты.";
				break;
			case 4171u:
				text = "В ответе хоста на запрос enrollment’a нет биометрической криптограммы.";
				break;
			case 4202u:
				text = "Сбой при удаленной загрузке: неверное смещение в данных.";
				break;
			case 4203u:
				text = "Не указанный или неверный код активации при удаленной загрузке.";
				break;
			case 4208u:
				text = "Ошибка удаленной загрузки: на сервере не активирован какой-либо шаблон для данного терминала.";
				break;
			case 4209u:
				text = "Ошибка удаленной загрузки: на сервере проблемы с доступом к БД.";
				break;
			case 4211u:
				text = "На терминале нет EMV-ключа с номером 62 (он нужен для удаленной загрузки).";
				break;
			case 4300u:
				text = "Недостаточно параметров при запуске модуля sb_pilot. В командной строке указаны не все требуемые параметры.";
				break;
			case 4301u:
				text = "Кассовая программа передала в UPOS недопустимый тип операции";
				break;
			case 4302u:
				text = "Кассовая программа передала в UPOS недопустимый тип карты";
				break;
			case 4303u:
				text = "Тип карты, переданный из кассовой программы, не значится в настройках UPOS. Возможно, на диске кассы имеется несколько каталогов с библиотекой UPOS. Банковский инженер настраивал один экземпляр, а кассовая программа обращается к другому, где никаких настроек (а значит, и типов карт) нет.";
				break;
			case 4305u:
				text = "Ошибка инициализации библиотеки sb_kernel.dll. Кассовая программа ожидает библиотеку с более свежей версией.";
				break;
			case 4306u:
				text = "Библиотека sb_kernel.dll не была инициализирована. Эта ошибка может разово возникать после обновления библиотеки через удаленную загрузку. Нужно просто повторить операцию.";
				break;
			case 4308u:
				text = "В старых версиях этим кодом обозначалась любая из проблем, которые сейчас обозначаются кодами 4331-4342";
				break;
			case 4309u:
				text = "Печатать нечего. Эта ошибка возникает в интегрированных решениях, которые выполнены не вполне корректно: в случае любой ошибки (нет связи, ПИН неверен, неверный ключ KLK и т.д.) кассовая программа все равно запрашивает у библиотеки sb_kernel.dll образ чека для печати. Поскольку по умолчанию библиотека при отказах чек не формирует, то на запрос чека она возвращает кассовой программе код 4309 – печатать нечего, нет документа для печати. Исходный код ошибки (тот, который обозначает причину отказа) кассовая программа при этом забывает.";
				break;
			case 4310u:
				text = "Кассовая программа передала в UPOS недопустимый трек2.";
				break;
			case 4314u:
				text = "Кассовая программа передала код операции «Оплата по международной карте», а вставлена была карта СБЕРКАРТ.";
				break;
			case 4332u:
				text = "Сверка итогов не выполнена (причина неизвестна, но печатать в итоге нечего).";
				break;
			case 4333u:
				text = "Распечатать контрольную ленту невозможно (причина неизвестна, но печатать в итоге нечего).";
				break;
			case 4334u:
				text = "Карта не считана. Либо цикл ожидания карты прерван нажатием клавиши ESC, либо просто истек таймаут.";
				break;
			case 4335u:
				text = "Сумма не введена при операции ввода слипа.";
				break;
			case 4336u:
				text = "Из кассовой программы передан неверный код валюты.";
				break;
			case 4337u:
				text = "Из кассовой программы передан неверный тип карты.";
				break;
			case 4338u:
				text = "Вызвана операция по карте СБЕРКАРТ, но прочитать карту СБЕРКАРТ не удалось.";
				break;
			case 4339u:
				text = "Вызвана недопустимая операция по карте СБЕРКАРТ.";
				break;
			case 4340u:
				text = "Ошибка повторного считывания карты СБЕРКАРТ.";
				break;
			case 4341u:
				text = "Вызвана операция по карте СБЕРКАРТ, но вставлена карта другого типа, либо не вставлена никакая.";
				break;
			case 4342u:
				text = "Ошибка: невозможно запустить диалоговое окно UPOS (тред почему-то не создается).";
				break;
			case 5002u:
				text = "Карта криво выпущена и поэтому дает сбой на терминалах, поддерживающих режим Offline Enciphered PIN.";
				break;
			case 5026u:
				text = "Ошибка проверки RSA-подписи. На терминале отсутствует (или некорректный) один из ключей из раздела «Ключи EMV».";
				break;
			case 5063u:
				text = "На карте ПРО100 нет списка транзакций.";
				break;
			case 5100u:
			case 5101u:
			case 5102u:
			case 5103u:
			case 5104u:
			case 5105u:
			case 5106u:
			case 5107u:
			case 5108u:
				text = "Нарушены данные на чипе карты";
				break;
			case 5109u:
				text = "Срок действия карты истек";
				break;
			case 5110u:
				text = "Срок действия карты еще не начался";
				break;
			case 5111u:
				text = "Для этой карты такая операция не разрешена";
				break;
			case 5116u:
			case 5120u:
				text = "Клиент отказался от ввода ПИНа";
				break;
			case 5133u:
				text = "Операция отклонена картой";
				break;
			}
		}
		catch
		{
			if (ErrorString == null || ErrorString == "")
			{
				return false;
			}
			if (ErrorString != null)
			{
				int length = ErrorString.Length;
				if (length == 2)
				{
					switch (ErrorString[1])
					{
					case '4':
						if (ErrorString == "B4")
						{
							text = "Неверный номер ERN или отменяемая операция не найдена";
						}
						break;
					case 'B':
						if (ErrorString == "BB")
						{
							text = "Требуется синхронизация журнала";
						}
						break;
					case 'E':
						if (ErrorString == "FE")
						{
							text = "Неверный формат сообщения, отсутствуют обязательные поля.";
						}
						break;
					case 'F':
						if (!(ErrorString == "JF"))
						{
							if (ErrorString == "NF")
							{
								text = "Оригинальная транзакция не найдена по номеру банковского чека";
							}
						}
						else
						{
							text = "Требуется сверка итогов";
						}
						break;
					case 'N':
						if (ErrorString == "UN")
						{
							text = "Выполнение операции невозможно из-за ограничений функциональности";
						}
						break;
					case 'P':
						if (ErrorString == "UP")
						{
							text = "Требуется обновление ПО";
						}
						break;
					}
				}
			}
		}
		Error = num + " : " + text;
		if (TextError != "")
		{
			Error = TextError + " ( " + Error + " )";
		}
		return num != 0;
	}
}
