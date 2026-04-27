using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateSbRf : UnitPort
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int DllTestPinpad();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int card_authorize(nint track2, nint auth_ans);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int card_authorize12(string track2, nint auth_ans);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int card_authorize15(nint track2, nint auth_ans, nint payment_info_item, nint dataIn, nint dataOut);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate nint ctxAlloc();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int ctxGetString(nint ctx, int EParameterName, nint val, int sz);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate void ctxFree(nint ctx);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int close_day(nint auth_ans);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int RollBackTrx(uint Amount, [MarshalAs(UnmanagedType.LPStr)] string pAuthCode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int dGetTerminalID(nint TerminalID);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private delegate int ShowQrCodeDisplay(string QR);

	[StructLayout((LayoutKind)0, Pack = 1)]
	private struct TAuthAnswer
	{
		public int TType;

		public uint Amount;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
		public string Rcode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		public string AMessage;

		public int CType;

		public nint pCheck;
	}

	[StructLayout((LayoutKind)0, Pack = 1)]
	private struct TAuthAnswer8
	{
		public TAuthAnswer AuthAnswer;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
		public string AuthCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
		public string CardID;

		public int ErrorCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
		public string TransDate;

		public int TransNumber;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
		public string RRN;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string EncryptedData;
	}

	[StructLayout((LayoutKind)0, Pack = 1)]
	private struct TAuthAnswer12
	{
		public TAuthAnswer AuthAnswer;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
		public string AuthCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
		public string CardID;

		public int ErrorCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
		public string TransDate;

		public int TransNumber;

		public int SberOwnCard;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
		public string Hash;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 104)]
		public string Track3;

		public uint RequestID;

		public uint Department;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
		public string RRN;
	}

	[StructLayout((LayoutKind)0, Pack = 1)]
	private struct TAuthAnswer14
	{
		public TAuthAnswer AuthAnswer;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
		public string AuthCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
		public string CardID;

		public int ErrorCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
		public string TransDate;

		public int TransNumber;

		public int SberOwnCard;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
		public string Hash;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 104)]
		public string Track3;

		public uint RequestID;

		public uint Department;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
		public string RRN;

		public uint CurrencyCode;

		public char CardEntryMode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
		public string CardName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
		public string AID;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string FullErrorText;

		public uint GoodsPrice;

		public uint GoodsVolume;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
		public string GoodsCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
		public string GoodsName;
	}

	private static object Lock_PilotNT = new object();

	private string DirectoryDll = "";

	private nint Dllptr;

	private bool DllIsLosd;

	private decimal OldAmmount;

	private string OldAuthCode = "";

	private string OldRRNCode = "";

	private string OldCardID = "";

	public string Slip = "";

	private uint Department;

	private int VerAPI = 12;

	private Encoding Encoding;

	private string FileNameExe = "";

	private string FileNameSettings = "";

	private string FileNameSlip = "";

	private string FileNameOut = "";

	public GateSbRf(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		IsCommandCancelled = true;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n        <Group Caption='Протокол обмена'>\r\n            <Parameter Name=\"VerAPI\" Caption=\"Версия API\" TypeValue=\"Number\" DefaultValue=\"12\">\r\n                <ChoiceList>\r\n                    <Item Value=\"15\">API Версии 15 (ver.15)</Item>\r\n                    <Item Value=\"12\">API Версии 12 (ver.12)</Item>\r\n                    <Item Value=\"8\">API Версии 8 (ver.8)</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n        <Group Caption='Настройки порта'>\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\" DefaultValue=\"2\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"14400\">14400</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"IP\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\" DefaultValue=\"\"/>\r\n        <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n        <Parameter Name=\"DirectoryDll\" Caption=\"Путь к дистрибутиву\" TypeValue=\"String\" DefaultValue=\"C:\\sc552\" Description=\"Путь к папке, содержащей библиотеку pilot_nt.dll\" MasterParameterName=\"VerAPI\" MasterParameterOperation=\"NotEqual\" MasterParameterValue=\"0\"/>\r\n        </Group>\r\n        <Group Caption='Прочие настройки'>\r\n        <Parameter Name='Department' Caption='Отдел по умолчанию' TypeValue='Number' DefaultValue='0'/>\r\n        <Parameter Name='КодыСимволовОтреза' Caption='Коды символов отреза' TypeValue='String' DefaultValue='01' Description=\"Бывают коды: 1, 16, 010D0A, 050D0D0A\"/>\r\n        <Parameter Name='ErrorDoubleCheck' Caption='Устранить ошибку дублирования слип-чеков' TypeValue='Boolean' DefaultValue='false'\r\n                Description='Если есть задвоение слип-чеков сначала попробуйте в настройках терминала указать 1 копию слип-чека.\r\n                Если не поможет взведите этот параметр.'/>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "СБРФ:Эквайринговые терминалы";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		UnitVersion = Global.Verson;
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "Установка программного обеспечения 'UPOS' производится специалистом Сбербанка.";
		NameDevice = "СБРФ: Платежный терминал";
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
			case "DirectoryDll":
				DirectoryDll = unitParamet.Value.Trim();
				break;
			case "Department":
				Department = unitParamet.Value.AsUInt();
				break;
			case "VerAPI":
				VerAPI = unitParamet.Value.AsInt();
				break;
			}
		}
		try
		{
			if (UnitParamets["КодыСимволовОтреза"] == "" && SettDr.Paramets.ContainsKey("КодСимволаЧастичногоОтреза") && SettDr.Paramets["КодСимволаЧастичногоОтреза"].AsInt() != 0)
			{
				UnitParamets["КодыСимволовОтреза"] = SettDr.Paramets["КодСимволаЧастичногоОтреза"].AsInt().ToString("X2");
				SettDr.Paramets["КодыСимволовОтреза"] = UnitParamets["КодыСимволовОтреза"];
			}
		}
		catch
		{
		}
		if (UnitParamets["КодыСимволовОтреза"] == "")
		{
			SettDr.Paramets["КодыСимволовОтреза"] = UnitParamets["КодыСимволовОтреза"];
		}
	}

	public override void SaveParametrs(Dictionary<string, string> NewParamets)
	{
		Dictionary<string, string> dictionary = null;
		try
		{
			dictionary = ReadIniFile(FileNameSettings, Encoding);
			SetValueForKeyUpp(dictionary, "ComPort", NewParamets["ComId"].ToUpper().Replace("COM".ToUpper(), ""), NewParamets["TypeConnect"] != "2");
			SetValueForKeyUpp(dictionary, "Speed", NewParamets["ComSpeed"], NewParamets["TypeConnect"] != "2");
			SetValueForKeyUpp(dictionary, "PinpadIPAddr", NewParamets["IP"], NewParamets["TypeConnect"] != "1");
			SetValueForKeyUpp(dictionary, "PinpadIPPort", NewParamets["Port"], NewParamets["TypeConnect"] != "1");
			SetValueForKeyUpp(dictionary, "PrinterEnd", NewParamets["КодыСимволовОтреза"]);
			SetValueForKeyUpp(dictionary, "PrintEnd", NewParamets["КодыСимволовОтреза"]);
			WriteIniFile(FileNameSettings, dictionary, Encoding);
		}
		catch (Exception)
		{
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		Encoding = Encoding.GetEncoding(1251);
		FileNameExe = Path.Combine(DirectoryDll, "sb_pilot");
		FileNameSettings = Path.Combine(DirectoryDll, "pinpad.ini");
		FileNameSlip = Path.Combine(DirectoryDll, "p");
		FileNameOut = Path.Combine(DirectoryDll, "e");
		Dictionary<string, string> data;
		try
		{
			data = ReadIniFile(FileNameSettings, Encoding);
		}
		catch (Exception)
		{
			Error = "Ошибка чтения файла настройки. Или указан не правильно каталог ПО Сбербанка или не хватает прав на чтение файла настроек ПО Сбербанка";
			return false;
		}
		if (GetValueForKeyUpp(data, "comport") != null)
		{
			UnitParamets["ComId"] = "COM" + GetValueForKeyUpp(data, "comport");
			SettDr.Paramets["ComId"] = UnitParamets["ComId"];
			UnitParamets["TypeConnect"] = "2";
			SettDr.Paramets["TypeConnect"] = "2";
		}
		if (GetValueForKeyUpp(data, "Speed") != null)
		{
			UnitParamets["ComSpeed"] = GetValueForKeyUpp(data, "Speed");
			SettDr.Paramets["ComSpeed"] = UnitParamets["ComSpeed"];
		}
		if (GetValueForKeyUpp(data, "PinpadIPAddr") != null)
		{
			UnitParamets["IP"] = GetValueForKeyUpp(data, "PinpadIPAddr");
			SettDr.Paramets["IP"] = UnitParamets["IP"];
			UnitParamets["TypeConnect"] = "1";
			SettDr.Paramets["TypeConnect"] = "1";
		}
		if (GetValueForKeyUpp(data, "PinpadIPPort") != null)
		{
			UnitParamets["Port"] = GetValueForKeyUpp(data, "PinpadIPPort");
			SettDr.Paramets["Port"] = UnitParamets["Port"];
		}
		if (GetValueForKeyUpp(data, "PrinterEnd") != null)
		{
			UnitParamets["КодыСимволовОтреза"] = GetValueForKeyUpp(data, "PrinterEnd");
			SettDr.Paramets["КодыСимволовОтреза"] = UnitParamets["КодыСимволовОтреза"];
			UnitParamets["ErrorDoubleCheck"] = UnitParamets["ErrorDoubleCheck"];
			SettDr.Paramets["ErrorDoubleCheck"] = UnitParamets["ErrorDoubleCheck"];
		}
		if (GetValueForKeyUpp(data, "PrinterFile") != null)
		{
			FileNameSlip = Path.Combine(DirectoryDll, GetValueForKeyUpp(data, "PrinterFile"));
		}
		try
		{
			if (!TestPinpad())
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
		try
		{
			File.Delete(FileNameSlip);
		}
		catch
		{
		}
		SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		if (VerAPI == 15)
		{
			CommandPayTerminal15(DataCommand, RezultCommand, Command);
		}
		else if (VerAPI == 12)
		{
			CommandPayTerminal12(DataCommand, RezultCommand, Command);
		}
		else if (VerAPI == 8)
		{
			CommandPayTerminal8(DataCommand, RezultCommand, Command);
		}
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
		SettlementWindows(DataCommand, RezultCommand);
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		TerminalReportWindows(DataCommand, RezultCommand);
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
			ServiceMenuWindows(DataCommand, RezultCommand);
		}
		base.DoAdditionalAction(DataCommand, ref RezultCommand);
	}

	public void CommandPayTerminal15(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		string text = null;
		TAuthAnswer14 tAuthAnswer = default(TAuthAnswer14);
		if (DataCommand.Department.HasValue)
		{
			tAuthAnswer.Department = DataCommand.Department.Value;
		}
		else
		{
			tAuthAnswer.Department = Department;
		}
		tAuthAnswer.AuthAnswer = default(TAuthAnswer);
		tAuthAnswer.AuthAnswer.Amount = (uint)(DataCommand.Amount * 100m);
		tAuthAnswer.AuthAnswer.CType = 0;
		switch (Command)
		{
		case 0:
			tAuthAnswer.AuthAnswer.TType = 1;
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 3;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 3;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			text = "QSELECT";
			break;
		case 3:
			tAuthAnswer.AuthAnswer.TType = 51;
			break;
		case 4:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 52;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			text = "QSELECT";
			break;
		case 5:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 43;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			text = "QSELECT";
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		int errorByte = 0;
		string text2 = "";
		string terminalID = "";
		string text3 = "";
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				terminalID = GetTerminalID();
				ctxAlloc ctxAlloc2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(ctxAlloc), "_ctxAlloc") as ctxAlloc;
				ctxFree ctxFree2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(ctxFree), "_ctxFree") as ctxFree;
				ctxGetString ctxGetString2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(ctxGetString), "_ctxGetString") as ctxGetString;
				card_authorize15 card_authorize16 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(card_authorize15), "_card_authorize15") as card_authorize15;
				nint track = ((text != null) ? Marshal.StringToBSTR(text) : 0);
				nint num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TAuthAnswer14)));
				Marshal.StructureToPtr(tAuthAnswer, num, false);
				try
				{
					nint num2 = ctxAlloc2();
					errorByte = card_authorize16(track, num, 0, new IntPtr(0), num2);
					string s = "".PadRight(25, '\0');
					nint num3 = Marshal.StringToBSTR(s);
					int num4 = ctxGetString2(num2, 11, num3, 25);
					s = Marshal.PtrToStringAnsi(num3);
					ctxFree2(num2);
					text3 = ((num4 != 0) ? tAuthAnswer.CardID : s);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_card_authorize15", ex.Message));
				}
				tAuthAnswer = (TAuthAnswer14)Marshal.PtrToStructure(num, typeof(TAuthAnswer14));
				if (((IntPtr)tAuthAnswer.AuthAnswer.pCheck).ToInt64() != 0L)
				{
					text2 = Marshal.PtrToStringAnsi(tAuthAnswer.AuthAnswer.pCheck);
					if (text2 == null)
					{
						text2 = "";
					}
					if (UnitParamets["ErrorDoubleCheck"] == ExtensionMethods.AsString(Val: true))
					{
						text2 = text2.Substring(0, text2.Length / 2);
					}
					Marshal.FreeHGlobal(tAuthAnswer.AuthAnswer.pCheck);
				}
				Marshal.FreeHGlobal(num);
			}
			finally
			{
				StopDll();
			}
		}
		if (!IsCommandBad(RezultCommand, errorByte, tAuthAnswer.AuthAnswer))
		{
			RezultCommand.CardNumber = tAuthAnswer.CardID;
			RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
			RezultCommand.RRNCode = tAuthAnswer.RRN;
			RezultCommand.AuthorizationCode = tAuthAnswer.AuthCode;
			RezultCommand.Slip = GetSlipSBRF(text2);
			RezultCommand.Amount = (decimal)tAuthAnswer.AuthAnswer.Amount / 100m;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
			RezultCommand.CardHash = tAuthAnswer.Hash;
			if (text3 != null)
			{
				RezultCommand.CardDPAN = text3;
			}
			try
			{
				RezultCommand.TransDate = DateTime.Parse(tAuthAnswer.TransDate);
			}
			catch
			{
			}
			RezultCommand.TerminalID = terminalID;
			RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
			OldAmmount = (decimal)tAuthAnswer.AuthAnswer.Amount / 100m;
			OldAuthCode = tAuthAnswer.AuthCode;
			OldRRNCode = tAuthAnswer.RRN;
			OldCardID = tAuthAnswer.CardID;
		}
	}

	public void CommandPayTerminal12(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		string track = null;
		TAuthAnswer12 tAuthAnswer = default(TAuthAnswer12);
		if (DataCommand.Department.HasValue)
		{
			tAuthAnswer.Department = DataCommand.Department.Value;
		}
		else
		{
			tAuthAnswer.Department = Department;
		}
		tAuthAnswer.AuthAnswer = default(TAuthAnswer);
		tAuthAnswer.AuthAnswer.Amount = (uint)(DataCommand.Amount * 100m);
		tAuthAnswer.AuthAnswer.CType = 0;
		switch (Command)
		{
		case 0:
			tAuthAnswer.AuthAnswer.TType = 1;
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 3;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 3;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			track = "QSELECT";
			break;
		case 3:
			tAuthAnswer.AuthAnswer.TType = 51;
			break;
		case 4:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 52;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			track = "QSELECT";
			break;
		case 5:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 43;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			track = "QSELECT";
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		int errorByte = 0;
		string text = "";
		string terminalID = "";
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				terminalID = GetTerminalID();
				card_authorize12 card_authorize16 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(card_authorize12), "_card_authorize12") as card_authorize12;
				nint num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TAuthAnswer12)));
				Marshal.StructureToPtr(tAuthAnswer, num, false);
				try
				{
					errorByte = card_authorize16(track, num);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_card_authorize12", ex.Message));
				}
				tAuthAnswer = (TAuthAnswer12)Marshal.PtrToStructure(num, typeof(TAuthAnswer12));
				if (((IntPtr)tAuthAnswer.AuthAnswer.pCheck).ToInt64() != 0L)
				{
					text = Marshal.PtrToStringAnsi(tAuthAnswer.AuthAnswer.pCheck);
					if (text == null)
					{
						text = "";
					}
					if (UnitParamets["ErrorDoubleCheck"] == ExtensionMethods.AsString(Val: true))
					{
						text = text.Substring(0, text.Length / 2);
					}
					Marshal.FreeHGlobal(tAuthAnswer.AuthAnswer.pCheck);
				}
				Marshal.FreeHGlobal(num);
			}
			finally
			{
				StopDll();
			}
		}
		if (!IsCommandBad(RezultCommand, errorByte, tAuthAnswer.AuthAnswer))
		{
			RezultCommand.CardNumber = tAuthAnswer.CardID;
			RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
			RezultCommand.RRNCode = tAuthAnswer.RRN;
			RezultCommand.AuthorizationCode = tAuthAnswer.AuthCode;
			RezultCommand.Slip = GetSlipSBRF(text);
			RezultCommand.Amount = (decimal)tAuthAnswer.AuthAnswer.Amount / 100m;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
			RezultCommand.CardHash = tAuthAnswer.Hash;
			try
			{
				RezultCommand.TransDate = DateTime.Parse(tAuthAnswer.TransDate);
			}
			catch
			{
			}
			RezultCommand.TerminalID = terminalID;
			RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
			OldAmmount = (decimal)tAuthAnswer.AuthAnswer.Amount / 100m;
			OldAuthCode = tAuthAnswer.AuthCode;
			OldRRNCode = tAuthAnswer.RRN;
			OldCardID = tAuthAnswer.CardID;
		}
	}

	public void CommandPayTerminal8(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		string text = null;
		TAuthAnswer8 tAuthAnswer = default(TAuthAnswer8);
		tAuthAnswer.AuthAnswer = default(TAuthAnswer);
		tAuthAnswer.AuthAnswer.Amount = (uint)(DataCommand.Amount * 100m);
		tAuthAnswer.AuthAnswer.CType = 0;
		switch (Command)
		{
		case 0:
			tAuthAnswer.AuthAnswer.TType = 1;
			break;
		case 1:
		{
			Dictionary<string, object> dictionary3 = SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 3;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			if (dictionary3.TryGetValue("ED", out var value3))
			{
				text = "E" + ((string)value3).ToUpper();
			}
			break;
		}
		case 2:
		{
			Dictionary<string, object> dictionary4 = SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 3;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			text = "QSELECT";
			if (dictionary4.TryGetValue("ED", out var value4))
			{
				text = "E" + ((string)value4).ToUpper();
			}
			break;
		}
		case 3:
			tAuthAnswer.AuthAnswer.TType = 51;
			break;
		case 4:
		{
			Dictionary<string, object> dictionary2 = SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 52;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			text = "QSELECT";
			if (dictionary2.TryGetValue("ED", out var value2))
			{
				text = "E" + ((string)value2).ToUpper();
			}
			break;
		}
		case 5:
		{
			Dictionary<string, object> dictionary = SetDictFromString(DataCommand.UniversalID, DataCommand);
			tAuthAnswer.AuthAnswer.TType = 43;
			tAuthAnswer.RRN = DataCommand.RRNCode;
			tAuthAnswer.AuthCode = DataCommand.AuthorizationCode;
			tAuthAnswer.CardID = DataCommand.CardNumber;
			text = "QSELECT";
			if (dictionary.TryGetValue("ED", out var value))
			{
				text = "E" + ((string)value).ToUpper();
			}
			break;
		}
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		int errorByte = 0;
		string text2 = "";
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				card_authorize card_authorize16 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(card_authorize), "_card_authorize8") as card_authorize;
				nint num = ((text != null) ? Marshal.StringToHGlobalAnsi(text) : 0);
				nint num2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TAuthAnswer8)));
				Marshal.StructureToPtr(tAuthAnswer, num2, false);
				try
				{
					errorByte = card_authorize16(num, num2);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_card_authorize8", ex.Message));
				}
				tAuthAnswer = (TAuthAnswer8)Marshal.PtrToStructure(num2, typeof(TAuthAnswer8));
				if (((IntPtr)tAuthAnswer.AuthAnswer.pCheck).ToInt64() != 0L)
				{
					text2 = Marshal.PtrToStringAnsi(tAuthAnswer.AuthAnswer.pCheck);
					if (text2 == null)
					{
						text2 = "";
					}
					if (UnitParamets["ErrorDoubleCheck"] == ExtensionMethods.AsString(Val: true))
					{
						text2 = text2.Substring(0, text2.Length / 2);
					}
					Marshal.FreeHGlobal(tAuthAnswer.AuthAnswer.pCheck);
				}
				Marshal.FreeHGlobal(num2);
				if (text != null)
				{
					Marshal.FreeHGlobal(num);
				}
			}
			finally
			{
				StopDll();
			}
		}
		if (!IsCommandBad(RezultCommand, errorByte, tAuthAnswer.AuthAnswer))
		{
			RezultCommand.CardNumber = tAuthAnswer.CardID;
			RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
			RezultCommand.RRNCode = tAuthAnswer.RRN;
			RezultCommand.CardEncryptedData = tAuthAnswer.EncryptedData;
			RezultCommand.AuthorizationCode = tAuthAnswer.AuthCode;
			RezultCommand.Slip = GetSlipSBRF(text2);
			RezultCommand.Amount = (decimal)tAuthAnswer.AuthAnswer.Amount / 100m;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
			RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
			OldAmmount = (decimal)tAuthAnswer.AuthAnswer.Amount / 100m;
			OldAuthCode = tAuthAnswer.AuthCode;
			OldRRNCode = tAuthAnswer.RRN;
			OldCardID = tAuthAnswer.CardID;
		}
	}

	public void SettlementWindows(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		try
		{
			File.Delete(FileNameSlip);
		}
		catch
		{
		}
		SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		TAuthAnswer tAuthAnswer = new TAuthAnswer
		{
			Amount = 0u,
			CType = 0,
			TType = 7
		};
		string text = "";
		int errorByte = 0;
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				close_day close_day2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(close_day), "_close_day") as close_day;
				nint num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TAuthAnswer)));
				Marshal.StructureToPtr(tAuthAnswer, num, false);
				try
				{
					errorByte = close_day2(num);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_close_day", ex.Message));
				}
				tAuthAnswer = (TAuthAnswer)Marshal.PtrToStructure(num, typeof(TAuthAnswer));
				if (((IntPtr)tAuthAnswer.pCheck).ToInt64() != 0L)
				{
					text = Marshal.PtrToStringAnsi(tAuthAnswer.pCheck);
					if (text == null)
					{
						text = "";
					}
					Marshal.FreeHGlobal(tAuthAnswer.pCheck);
				}
				Marshal.FreeHGlobal(num);
			}
			finally
			{
				StopDll();
			}
		}
		if (!IsCommandBad(RezultCommand, errorByte, tAuthAnswer))
		{
			RezultCommand.CardNumber = "";
			RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
			RezultCommand.RRNCode = "";
			RezultCommand.AuthorizationCode = "";
			RezultCommand.Slip = GetSlipSBRF(text);
			RezultCommand.Amount = tAuthAnswer.Amount * 100;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
		}
	}

	public void TerminalReportWindows(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		try
		{
			File.Delete(FileNameSlip);
		}
		catch
		{
		}
		SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		if (!DataCommand.Detailed)
		{
			SberShift(DataCommand, RezultCommand, IsDetailed: false);
		}
		else if (DataCommand.Detailed)
		{
			SberShift(DataCommand, RezultCommand, IsDetailed: true);
		}
	}

	public void ServiceMenuWindows(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		TAuthAnswer tAuthAnswer = new TAuthAnswer
		{
			Amount = 0u,
			CType = 0,
			TType = 7
		};
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				close_day close_day2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(close_day), "_ServiceMenu") as close_day;
				nint num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TAuthAnswer)));
				Marshal.StructureToPtr(tAuthAnswer, num, false);
				try
				{
					close_day2(num);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_ServiceMenu", ex.Message));
				}
			}
			finally
			{
				StopDll();
			}
		}
	}

	public void SberShift(DataCommand DataCommand, RezultCommandProcessing RezultCommand, bool IsDetailed)
	{
		TAuthAnswer tAuthAnswer = new TAuthAnswer
		{
			Amount = 0u,
			CType = 0
		};
		if (IsDetailed)
		{
			tAuthAnswer.TType = 0;
		}
		else
		{
			tAuthAnswer.TType = 1;
		}
		int errorByte = 0;
		string text = "";
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				close_day close_day2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(close_day), "_get_statistics") as close_day;
				nint num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TAuthAnswer)));
				Marshal.StructureToPtr(tAuthAnswer, num, false);
				try
				{
					errorByte = close_day2(num);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_get_statistics", ex.Message));
				}
				tAuthAnswer = (TAuthAnswer)Marshal.PtrToStructure(num, typeof(TAuthAnswer));
				if (((IntPtr)tAuthAnswer.pCheck).ToInt64() != 0L)
				{
					text = Marshal.PtrToStringAnsi(tAuthAnswer.pCheck);
					if (text == null)
					{
						text = "";
					}
					Marshal.FreeHGlobal(tAuthAnswer.pCheck);
				}
				Marshal.FreeHGlobal(num);
			}
			finally
			{
				StopDll();
			}
		}
		if (!IsCommandBad(RezultCommand, errorByte, tAuthAnswer))
		{
			RezultCommand.CardNumber = "";
			RezultCommand.ReceiptNumber = "";
			RezultCommand.RRNCode = "";
			RezultCommand.AuthorizationCode = "";
			RezultCommand.Slip = GetSlipSBRF(text);
			RezultCommand.Amount = default(decimal);
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
		}
	}

	public string GetTerminalID()
	{
		string text = null;
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			return text;
		}
		try
		{
			dGetTerminalID dGetTerminalID2 = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(dGetTerminalID), "_GetTerminalID") as dGetTerminalID;
			nint num = 0;
			num = Marshal.StringToBSTR("                          ");
			try
			{
				dGetTerminalID2(num);
			}
			catch (Exception ex)
			{
				throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_GetTerminalID", ex.Message));
			}
			if (((IntPtr)num).ToInt64() != 0L)
			{
				text = Marshal.PtrToStringAnsi(num);
				if (text == null)
				{
					text = "";
				}
				Marshal.FreeHGlobal(num);
			}
		}
		catch
		{
		}
		Error = "";
		return text;
	}

	private void SetPOLLING(bool OnOff, bool RunOff, DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		string text = Path.Combine(DirectoryDll, "POLLING.off");
		if (!OnOff)
		{
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
		}
		else
		{
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
			File.WriteAllText(text, "abort");
		}
		if (!RunOff)
		{
			return;
		}
		new Task(delegate
		{
			int num = 180;
			if (DataCommand.Timeout >= num)
			{
				num = DataCommand.Timeout - 3;
			}
			for (int i = 0; i < num; i++)
			{
				Thread.Sleep(1000);
				if (CancellationCommand)
				{
					SetPOLLING(OnOff: true, RunOff: false, null, null);
					return;
				}
			}
			if (RezultCommand.Status != ExecuteStatus.Ok && RezultCommand.Status != ExecuteStatus.Error)
			{
				SetPOLLING(OnOff: true, RunOff: false, null, null);
			}
		}).Start();
	}

	private void RunDLL()
	{
		if (DllIsLosd)
		{
			try
			{
				StopDll();
			}
			catch
			{
			}
		}
		DllIsLosd = false;
		string text = Path.Combine(DirectoryDll, "pilot_nt.dll");
		ExecuteDll.RunDLL(ref Dllptr, text, ref DllIsLosd);
		if (Dllptr == IntPtr.Zero)
		{
			throw new ArgumentException($"Ошибка загрузки dll '{text}'.");
		}
	}

	private void StopDll()
	{
		if (DllIsLosd)
		{
			ExecuteDll.StopDll(ref Dllptr, ref DllIsLosd);
			Dllptr = IntPtr.Zero;
		}
	}

	private bool TestPinpad()
	{
		int errorByte = 0;
		lock (Lock_PilotNT)
		{
			RunDLL();
			try
			{
				DllTestPinpad dllTestPinpad = ExecuteDll.CreareFunctionDelegae(Dllptr, typeof(DllTestPinpad), "_TestPinpad") as DllTestPinpad;
				try
				{
					errorByte = dllTestPinpad();
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Ошибка вызова функции '{0}' в dll, Error # {1}.", "_TestPinpad", ex.Message));
				}
			}
			finally
			{
				StopDll();
			}
		}
		if (CreateTextError(errorByte))
		{
			return false;
		}
		return true;
	}

	private string GetSlipSBRF(string Text)
	{
		if (Text == null || Text.Trim() == "")
		{
			try
			{
				Text = File.ReadAllText(FileNameSlip, Encoding.GetEncoding(866));
			}
			catch
			{
			}
		}
		string text = UnitParamets["КодыСимволовОтреза"];
		byte[] array = null;
		string text2;
		if (text.Length == 0)
		{
			text2 = "";
		}
		else
		{
			if (text.Length <= 2)
			{
				array = new byte[1] { Convert.ToByte(text, 16) };
			}
			else
			{
				array = new byte[text.Length / 2];
				for (int i = 0; i < text.Length; i += 2)
				{
					array[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
				}
			}
			text2 = Encoding.UTF8.GetString(array);
		}
		string text3 = Text;
		if (UnitParamets["ErrorDoubleCheck"] == ExtensionMethods.AsString(Val: false) && text2.Length != 0 && text3.IndexOf(text2) > 10)
		{
			text3 = text3.Substring(0, text3.IndexOf(text2));
		}
		return text3.ToString();
	}

	private bool IsCommandBad(RezultCommand RezultCommand, int ErrorByte, TAuthAnswer AuthAnswer, bool IsStopDll = true, string ErrorText = "")
	{
		if (Error != "")
		{
			if (IsStopDll && DllIsLosd)
			{
				StopDll();
			}
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
		if (ErrorByte != 0)
		{
			if (AuthAnswer.AMessage != null)
			{
				CreateTextError(ErrorByte, AuthAnswer.AMessage, ErrorText);
			}
			else
			{
				CreateTextError(ErrorByte, "", ErrorText);
			}
			if (IsStopDll && DllIsLosd)
			{
				StopDll();
			}
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
		string text = "";
		if (AuthAnswer.Rcode != null)
		{
			text = AuthAnswer.Rcode.Trim();
		}
		if (text != "0" && text != "00")
		{
			if (IsStopDll && DllIsLosd)
			{
				StopDll();
			}
			if (RezultCommand != null)
			{
				RezultCommand.Status = ExecuteStatus.Error;
				if (ErrorText == "")
				{
					ErrorText = text + ": Не правильный результат авторизации";
				}
				if (ErrorText != "")
				{
					Error = ErrorText + " (" + Error + ")";
				}
			}
			return true;
		}
		return false;
	}

	private bool CreateTextError(int ErrorByte, string DllError = "", string TextError = "")
	{
		if (ErrorByte == 0)
		{
			return false;
		}
		string text = "Неизвестный код ошибки";
		switch (ErrorByte)
		{
		case 12:
			text = "Ошибка возникает обычно в ДОС-версиях. Возможных причин две: 1. В настройках указан неверный тип пинпада. Должно быть РС-2, а указано РС-3. 2. Если ошибка возникает неустойчиво, то скорее всего виноват СОМ-порт. Он или нестандартный, или неисправный. Попробовать перенести пинпад на другой порт, а лучше – на USB.";
			break;
		case 99:
			text = "Нарушился контакт с пинпадом, либо невозможно открыть указанный СОМ-порт (он или отсутствует в системе, или захвачен другой программой).";
			break;
		case 361:
		case 362:
		case 363:
		case 364:
			text = "Нарушился контакт с чипом карты. Чип не читается. Попробовать вставить другую карту. Если ошибка возникает на всех картах – неисправен чиповый ридер пинпада.";
			break;
		case 403:
			text = "Клиент ошибся при вводе ПИНа";
			break;
		case 405:
			text = "ПИН клиента заблокирован";
			break;
		case 444:
			text = "Истек срок действия карты";
			break;
		case 507:
			text = "Истек срок действия карты";
			break;
		case 518:
			text = "На терминале установлена неверная дата";
			break;
		case 521:
			text = "На карте недостаточно средств";
			break;
		case 572:
			text = "Истек срок действия карты";
			break;
		case 574:
			text = "Карта заблокирована";
			break;
		case 579:
			text = "Карта заблокирована";
			break;
		case 584:
		case 585:
			text = "Истек период обслуживания карты (СБЕРКАРТ)";
			break;
		case 705:
		case 707:
			text = "Карта заблокирована (СБЕРКАРТ)";
			break;
		case 708:
		case 709:
			text = "ПИН клиента заблокирован (СБЕРКАРТ)";
			break;
		case 2000:
			text = "Операция прервана нажатием клавиши ОТМЕНА. Другая возможная причина – не проведена предварительная сверка итогов, и на терминале еще нет сеансовых ключей.";
			break;
		case 2002:
			text = "Клиент слишком долго вводит ПИН. Истек таймаут.";
			break;
		case 2004:
		case 2005:
		case 2006:
		case 2007:
		case 2405:
		case 2406:
		case 2407:
			text = "Карта заблокирована (СБЕРКАРТ)";
			break;
		case 3001:
			text = "Недостаточно средств для загрузки на карту (СБЕРКАРТ)";
			break;
		case 3019:
		case 3020:
		case 3021:
			text = "На сервере проводятся регламентные работы (СБЕРКАРТ)";
			break;
		case 4100:
			text = "Нет связи с банком при удаленной загрузке. Возможно, на терминале неверно задан параметр «Код региона и участника для удаленной загрузки».";
			break;
		case 4101:
		case 4102:
			text = "Карта терминала не проинкассирована";
			break;
		case 4103:
		case 4104:
			text = "Ошибка обмена с чипом карты";
			break;
		case 4108:
			text = "Неправильно введен или прочитан номер карты (ошибка контрольного разряда)";
			break;
		case 4110:
		case 4111:
		case 4112:
			text = "Требуется проинкассировать карту терминала (СБЕРКАРТ)";
			break;
		case 4113:
		case 4114:
			text = "Превышен лимит, допустимый без связи с банком (СБЕРКАРТ)";
			break;
		case 4115:
			text = "Ручной ввод для таких карт запрещен";
			break;
		case 4117:
			text = "Клиент отказался от ввода ПИНа";
			break;
		case 4119:
			text = "Нет связи с банком. Другая возможная причина – неверный ключ KLK для пинпада Verifone pp1000se или встроенного пинпада Verifone. Если терминал Verifone работает по Ethernet, то иногда избавиться от ошибки можно, понизив скорость порта с 115200 до 57600 бод.";
			break;
		case 4120:
			text = "В пинпаде нет ключа KLK.";
			break;
		case 4121:
			text = "Ошибка файловой структуры терминала. Невозможно записать файл BTCH.D.";
			break;
		case 4122:
			text = "Ошибка смены ключей: либо на хосте нет нужного KLK, либо в настройках терминала указан неверный мерчант.";
			break;
		case 4123:
			text = "На терминале нет сеансовых ключей";
			break;
		case 4124:
			text = "На терминале нет мастер-ключей";
			break;
		case 4125:
			text = "На карте есть чип, а прочитана была магнитная полоса";
			break;
		case 4128:
			text = "Неверный МАС — код при сверке итогов. Вероятно, неверный ключ KLK.";
			break;
		case 4130:
			text = "Память терминала заполнена. Пора делать сверку итогов (лучше несколько раз подряд, чтобы почистить старые отчеты).";
			break;
		case 4131:
			text = "Установлен тип пинпада РС-2, но с момента последней прогрузки параметров пинпад был заменен (изменился его серийный номер). Необходимо повторно прогрузить TLV-файл или выполнить удаленную загрузку.";
			break;
		case 4132:
			text = "Операция отклонена картой. Возможно, карту вытащили из чипового ридера до завершения печати чека. Повторить операцию заново. Если ошибка возникает постоянно, возможно, карта неисправна.";
			break;
		case 4134:
			text = "Слишком долго не выполнялась сверка итогов на терминале (прошло более 5 дней с момента последней операции).";
			break;
		case 4135:
			text = "Нет SAM-карты для выбранного отдела (СБЕРКАРТ)";
			break;
		case 4136:
			text = "Требуется более свежая версия прошивки в пинпаде.";
			break;
		case 4137:
			text = "Ошибка при повторном вводе нового ПИНа.";
			break;
		case 4138:
			text = "Номер карты получателя не может совпадать с номером карты отправителя.";
			break;
		case 4139:
			text = "В настройках терминала нет ни одного варианта связи, пригодного для требуемой операции.";
			break;
		case 4140:
			text = "Неверно указаны сумма или код авторизации в команде SUSPEND из кассовой программы.";
			break;
		case 4141:
			text = "Невозможно выполнить команду SUSPEND: не найден файл SHCN.D.";
			break;
		case 4142:
			text = "Не удалось выполнить команду ROLLBACK из кассовой прграммы.";
			break;
		case 4143:
			text = "На терминале слишком старый стоп-лист.";
			break;
		case 4144:
		case 4145:
		case 4146:
		case 4147:
			text = "Неверный формат стоп-листа на терминале (для торговли в самолете без авторизации).";
			break;
		case 4148:
			text = "Карта в стоп-листе.";
			break;
		case 4149:
			text = "На карте нет фамилии держателя.";
			break;
		case 4150:
			text = "Превышен лимит, допустимый без связи с банком (для торговли на борту самолета без авторизации).";
			break;
		case 4151:
			text = "Истек срок действия карты (для торговли на борту самолета без авторизации).";
			break;
		case 4152:
			text = "На карте нет списка транзакций (ПРО100).";
			break;
		case 4153:
			text = "Список транзакций на карте имеет неизвестный формат (ПРО100).";
			break;
		case 4154:
			text = "Невозможно распечатать список транзакций карты, потому что его можно считать только с чипа, а прочитана магнитная полоса (ПРО100).";
			break;
		case 4155:
			text = "Список транзакций пуст (ПРО100).";
			break;
		case 4160:
			text = "Неверный ответ от карты при считывании биометрических данных";
			break;
		case 4161:
			text = "На терминале нет файла с биометрическим сертификатом BSCP.CR";
			break;
		case 4162:
		case 4163:
		case 4164:
			text = "Ошибка расшифровки биометрического сертификата карты. Возможно, неверный файл BSCP.CR";
			break;
		case 4165:
		case 4166:
		case 4167:
			text = "Ошибка взаимной аутентификации биосканера и карты. Возможно, неверный файл BSCP.CR";
			break;
		case 4168:
		case 4169:
			text = "Ошибка расшифровки шаблонов пальцев, считанных с карты.";
			break;
		case 4171:
			text = "В ответе хоста на запрос enrollment’a нет биометрической криптограммы.";
			break;
		case 4202:
			text = "Сбой при удаленной загрузке: неверное смещение в данных.";
			break;
		case 4203:
			text = "Не указанный или неверный код активации при удаленной загрузке.";
			break;
		case 4208:
			text = "Ошибка удаленной загрузки: на сервере не активирован какой-либо шаблон для данного терминала.";
			break;
		case 4209:
			text = "Ошибка удаленной загрузки: на сервере проблемы с доступом к БД.";
			break;
		case 4211:
			text = "На терминале нет EMV-ключа с номером 62 (он нужен для удаленной загрузки).";
			break;
		case 4300:
			text = "Недостаточно параметров при запуске модуля sb_pilot. В командной строке указаны не все требуемые параметры.";
			break;
		case 4301:
			text = "Кассовая программа передала в UPOS недопустимый тип операции";
			break;
		case 4302:
			text = "Кассовая программа передала в UPOS недопустимый тип карты";
			break;
		case 4303:
			text = "Тип карты, переданный из кассовой программы, не значится в настройках UPOS. Возможно, на диске кассы имеется несколько каталогов с библиотекой UPOS. Банковский инженер настраивал один экземпляр, а кассовая программа обращается к другому, где никаких настроек (а значит, и типов карт) нет.";
			break;
		case 4305:
			text = "Ошибка инициализации библиотеки sb_kernel.dll. Кассовая программа ожидает библиотеку с более свежей версией.";
			break;
		case 4306:
			text = "Библиотека sb_kernel.dll не была инициализирована. Эта ошибка может разово возникать после обновления библиотеки через удаленную загрузку. Нужно просто повторить операцию.";
			break;
		case 4308:
			text = "В старых версиях этим кодом обозначалась любая из проблем, которые сейчас обозначаются кодами 4331-4342";
			break;
		case 4309:
			text = "Печатать нечего. Эта ошибка возникает в интегрированных решениях, которые выполнены не вполне корректно: в случае любой ошибки (нет связи, ПИН неверен, неверный ключ KLK и т.д.) кассовая программа все равно запрашивает у библиотеки sb_kernel.dll образ чека для печати. Поскольку по умолчанию библиотека при отказах чек не формирует, то на запрос чека она возвращает кассовой программе код 4309 – печатать нечего, нет документа для печати. Исходный код ошибки (тот, который обозначает причину отказа) кассовая программа при этом забывает.";
			break;
		case 4310:
			text = "Кассовая программа передала в UPOS недопустимый трек2.";
			break;
		case 4314:
			text = "Кассовая программа передала код операции «Оплата по международной карте», а вставлена была карта СБЕРКАРТ.";
			break;
		case 4332:
			text = "Сверка итогов не выполнена (причина неизвестна, но печатать в итоге нечего).";
			break;
		case 4333:
			text = "Распечатать контрольную ленту невозможно (причина неизвестна, но печатать в итоге нечего).";
			break;
		case 4334:
			text = "Карта не считана. Либо цикл ожидания карты прерван нажатием клавиши ESC, либо просто истек таймаут.";
			break;
		case 4335:
			text = "Сумма не введена при операции ввода слипа.";
			break;
		case 4336:
			text = "Из кассовой программы передан неверный код валюты.";
			break;
		case 4337:
			text = "Из кассовой программы передан неверный тип карты.";
			break;
		case 4338:
			text = "Вызвана операция по карте СБЕРКАРТ, но прочитать карту СБЕРКАРТ не удалось.";
			break;
		case 4339:
			text = "Вызвана недопустимая операция по карте СБЕРКАРТ.";
			break;
		case 4340:
			text = "Ошибка повторного считывания карты СБЕРКАРТ.";
			break;
		case 4341:
			text = "Вызвана операция по карте СБЕРКАРТ, но вставлена карта другого типа, либо не вставлена никакая.";
			break;
		case 4342:
			text = "Ошибка: невозможно запустить диалоговое окно UPOS (тред почему-то не создается).";
			break;
		case 5002:
			text = "Карта криво выпущена и поэтому дает сбой на терминалах, поддерживающих режим Offline Enciphered PIN.";
			break;
		case 5026:
			text = "Ошибка проверки RSA-подписи. На терминале отсутствует (или некорректный) один из ключей из раздела «Ключи EMV».";
			break;
		case 5063:
			text = "На карте ПРО100 нет списка транзакций.";
			break;
		case 5100:
		case 5101:
		case 5102:
		case 5103:
		case 5104:
		case 5105:
		case 5106:
		case 5107:
		case 5108:
			text = "Нарушены данные на чипе карты";
			break;
		case 5109:
			text = "Срок действия карты истек";
			break;
		case 5110:
			text = "Срок действия карты еще не начался";
			break;
		case 5111:
			text = "Для этой карты такая операция не разрешена";
			break;
		case 5116:
		case 5120:
			text = "Клиент отказался от ввода ПИНа";
			break;
		case 5133:
			text = "Операция отклонена картой";
			break;
		}
		if (ErrorByte >= 4400 && ErrorByte < 4499)
		{
			text = "От фронтальной системы получен код ответа:";
		}
		Error = ErrorByte + ((DllError == "") ? "" : (" (" + DllError + ")")) + " : " + text;
		if (TextError != "")
		{
			Error = TextError + " ( " + Error + " )";
		}
		return ErrorByte != 0;
	}

	public void DeleteAllFile()
	{
		try
		{
			File.Delete(FileNameSlip);
		}
		catch
		{
		}
		try
		{
			File.Delete(FileNameOut);
		}
		catch
		{
		}
	}

	public static void CopyFiles(string DirectoryDll, string FileNameExe)
	{
		FileInfo[] files = new DirectoryInfo(Path.Combine(Global.GetPaht(), "sb_pilot")).GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			try
			{
				string text = Path.Combine(DirectoryDll, fileInfo.Name);
				if (File.Exists(FileNameExe))
				{
					File.Delete(text);
				}
				fileInfo.CopyTo(text, true);
			}
			catch
			{
			}
		}
	}

	private void Test(nint Ptr)
	{
		byte[] array = new byte[50];
		for (int i = 0; i < 50; i++)
		{
			array[i] = Marshal.ReadByte(Ptr, i);
		}
		BitConverter.ToString(array);
	}
}
