using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateUCS_New : UnitPort
{
	public class UCSResponce
	{
		public string Command = "";

		public int Length;

		public List<string> Data = new List<string>();

		public string Error = "";

		public UCSStryctResponce Stryct;

		public bool? LastLineFlag;

		public int Rez;
	}

	public class UCSStryctResponce
	{
		public bool IsPrintLine;

		public bool LastLineFlag;

		public int[] Params;

		public string Operation;

		public UCSStryctResponce(bool IsPrintLine, bool LastLineFlag, string Operation, params int[] Params)
		{
			this.IsPrintLine = IsPrintLine;
			this.LastLineFlag = LastLineFlag;
			this.Operation = Operation;
			this.Params = Params;
		}
	}

	private Encoding Win1251 = Encoding.GetEncoding(1251);

	public string TerminalID = "";

	public string Slip = "";

	private decimal OldAmmount;

	private string OldRRNCode = "";

	private string OldAuthCode = "";

	private nint Dllptr;

	private bool DllIsLosd;

	private static object Lock_Dll = new object();

	private nint pvSelf;

	private DateTime TimeWaitCommand;

	private Dictionary<string, UCSStryctResponce> ParTypes = new Dictionary<string, UCSStryctResponce>();

	public GateUCS_New(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		ParTypes.Add("1-0", new UCSStryctResponce(true, false, "Sale", 12, -1));
		ParTypes.Add("1-1", new UCSStryctResponce(true, false, "Pre-Auth", 12, -1));
		ParTypes.Add("1-4", new UCSStryctResponce(true, false, "Credit", 12, -1));
		ParTypes.Add("1-9", new UCSStryctResponce(true, false, "Void", 12));
		ParTypes.Add("1-A", new UCSStryctResponce(true, false, "Reversal of Sale", 12, 12));
		ParTypes.Add("1-K", new UCSStryctResponce(true, false, "Balance inquiry", 12, -1));
		ParTypes.Add("2-0", new UCSStryctResponce(true, false, "Transaction result", 12));
		ParTypes.Add("2-1", new UCSStryctResponce(true, false, "Finalize day totals"));
		ParTypes.Add("2-2", new UCSStryctResponce(false, false, "Finalize day totals", 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12, 1, 5, 12));
		ParTypes.Add("2-5", new UCSStryctResponce(true, false, "Get Report", 1));
		ParTypes.Add("3-0", new UCSStryctResponce(false, false, "Login", -1));
		ParTypes.Add("3-1", new UCSStryctResponce(false, false, "Login response", 1, 1));
		ParTypes.Add("3-2", new UCSStryctResponce(false, true, "Print Line", 1, -1));
		ParTypes.Add("3-6", new UCSStryctResponce(false, false, "Print Line", 2, 2, -1));
		ParTypes.Add("5-0", new UCSStryctResponce(false, false, "Initial response Ok"));
		ParTypes.Add("5-1", new UCSStryctResponce(false, false, "Initial response Error"));
		ParTypes.Add("5-2", new UCSStryctResponce(false, false, "PIN Entry required response"));
		ParTypes.Add("5-3", new UCSStryctResponce(false, false, "On-line authorisation required response"));
		ParTypes.Add("5-4", new UCSStryctResponce(false, false, "Initial response Error"));
		ParTypes.Add("5-5", new UCSStryctResponce(false, false, "Hold response"));
		ParTypes.Add("5-X", new UCSStryctResponce(false, false, "Error response", 2, -1));
		ParTypes.Add("5-M", new UCSStryctResponce(false, false, "Console message", -1));
		ParTypes.Add("6-0", new UCSStryctResponce(false, false, "Authorization Response", 1, 12, 3, 8, 6, 15, 12, 2, 7, 25, 16, 60));
		ParTypes.Add("8-6", new UCSStryctResponce(true, false, "Sale", 12, 6, 12, -1));
		ParTypes.Add("8-A", new UCSStryctResponce(true, false, "Reversal of Sale", 12, 12, 6, 0));
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "UCS:Эквайринговые терминалы UCS ";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		UnitVersion = Global.Verson;
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "";
		NameDevice = "UCS: Платежный терминал";
		UnitAdditionallinks = "<a href='https://posconfig.ucscards.ru/downloads/MicroModule_32'>Дистрибутив 'USC' для Windows x32</a><br/>Только для типа соединения 'Дистрибутив UCS'<br/>Логин 'ucs', Пароль 'ucsucs'<br/>";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TerminalID\" Caption=\"Идентификатор терминала\" TypeValue=\"String\" \r\n                Description=\"Узнать номер можно на экране терминала: \r\n                Выберите в меню 'СИСТЕМА',\r\n                Смотрите поле 'TID' \"/>\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" DefaultValue=\"192.168.1.10\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"\r\n                Description=\"Узнать IP адресс можно на экране терминала: \r\n                Выберите в меню 'СИСТЕМА',\r\n                Смотрите поле 'IP(S)' \"/>\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"4001\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"9600\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"14400\">14400</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name='PrintSlipOnTerminal' Caption='Есть печать чеков на терминале' TypeValue='Boolean' DefaultValue='false'\r\n                Description='Если в терминале настроена печать слип-чеков через встроенный принтер установите этот флажок'/>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		LoadParametsFromXML(text);
		string paramsXML = "";
		LoadAdditionalActionsFromXML(paramsXML);
	}

	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			switch (unitParamet.Key)
			{
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
			case "TerminalID":
				TerminalID = unitParamet.Value.Trim();
				break;
			}
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		Error = "";
		await base.InitDevice(FullInit, Program);
		try
		{
			if (Error != "")
			{
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
		new UCSResponce();
		await RunCommand("3-0", "3-1", 3000);
		UCSResponce uCSResponce;
		switch (Command)
		{
		case 0:
			uCSResponce = await RunCommand("1-0", "6-0", 30000, ((uint)(DataCommand.Amount * 100m)).ToString("D12"));
			break;
		case 1:
			uCSResponce = await RunCommand("1-4", "6-0", 30000, ((uint)(DataCommand.Amount * 100m)).ToString("D12"));
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			uCSResponce = await RunCommand("1-A", "6-0", 30000, DataCommand.RRNCode, ((uint)(DataCommand.Amount * 100m)).ToString("D12"));
			if (Error != "")
			{
				Error = "";
				uCSResponce = await RunCommand("1-9", "6-0", 30000, DataCommand.RRNCode);
			}
			break;
		case 3:
			uCSResponce = await RunCommand("1-1", "6-0", 30000, ((uint)(DataCommand.Amount * 100m)).ToString("D12"));
			break;
		case 4:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			uCSResponce = await RunCommand("8-6", "6-0", 30000, ((uint)(DataCommand.Amount * 100m)).ToString("D12"), DataCommand.AuthorizationCode.PadRight(6, ' '), DataCommand.RRNCode, DataCommand.CardNumber.Split('|')[1]);
			break;
		case 5:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			uCSResponce = await RunCommand("1-A", "6-0", 30000, DataCommand.RRNCode, ((uint)(DataCommand.Amount * 100m)).ToString("D12"));
			if (Error != "")
			{
				Error = "";
				uCSResponce = await RunCommand("8-A", "6-0", 30000, DataCommand.RRNCode, ((uint)(DataCommand.Amount * 100m)).ToString("D12"), DataCommand.AuthorizationCode.PadRight(6, ' '), DataCommand.CardNumber.Split('|')[1]);
			}
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		if (uCSResponce != null)
		{
			RezultCommand.CardNumber = uCSResponce.Data[9] + "|" + uCSResponce.Data[10];
			RezultCommand.RRNCode = uCSResponce.Data[6];
			RezultCommand.AuthorizationCode = uCSResponce.Data[8];
			RezultCommand.Amount = decimal.Parse(uCSResponce.Data[1]) / 100m;
		}
		RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
		RezultCommand.Slip = Slip;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		if (uCSResponce != null && uCSResponce.Data[7] != "00")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			Error = Error + ((Error == "") ? "" : "; ") + uCSResponce.Data[11];
		}
		if (uCSResponce != null)
		{
			OldAmmount = decimal.Parse(uCSResponce.Data[1]) / 100m;
			OldAuthCode = uCSResponce.Data[8];
			OldRRNCode = uCSResponce.Data[6];
		}
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		new UCSResponce();
		await RunCommand("3-0", "3-1", 3000);
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		UCSResponce uCSResponce = await RunCommand("1-A", "6-0", 30000, OldRRNCode, ((uint)(OldAmmount * 100m)).ToString("D12"));
		if (Error != "")
		{
			Error = "";
			uCSResponce = await RunCommand("1-9", "6-0", 30000, OldRRNCode);
		}
		RezultCommand.CardNumber = uCSResponce.Data[10];
		RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
		RezultCommand.RRNCode = uCSResponce.Data[6];
		RezultCommand.AuthorizationCode = uCSResponce.Data[8];
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = decimal.Parse(uCSResponce.Data[1]) / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		if (uCSResponce.Data[7] != "00")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = Error + ((Error == "") ? "" : "; ") + uCSResponce.Data[11];
		}
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		await RunCommand("3-0", "3-1", 3000);
		UCSResponce uCSResponce = await RunCommand("2-1", "2-2", 30000);
		decimal num = default(decimal);
		string text = "02345678DEFIKNQ";
		if (Error == "")
		{
			for (int i = 0; i < Math.Min(12, uCSResponce.Data.Count / 3); i++)
			{
				if (text.IndexOf(uCSResponce.Data[i]) != -1 && uCSResponce.Data[i + 2] != "")
				{
					num += decimal.Parse(uCSResponce.Data[i + 2]);
				}
			}
		}
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = "";
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = num / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		if (!DataCommand.Detailed)
		{
			await RunCommand("3-0", "3-1", 3000);
			await RunCommand("2-5", "", 30000, "2");
		}
		else if (DataCommand.Detailed)
		{
			await RunCommand("3-0", "3-1", 3000);
			await RunCommand("2-5", "", 30000, "3");
		}
		decimal num = default(decimal);
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = "";
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = num / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		await RunCommand("3-0", "3-1", 3000);
		UCSResponce uCSResponce = await RunCommand("2-0", "6-0", 30000, DataCommand.RRNCode);
		RezultCommand.CardNumber = uCSResponce.Data[9] + "|" + uCSResponce.Data[10];
		RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
		RezultCommand.RRNCode = uCSResponce.Data[6];
		RezultCommand.AuthorizationCode = uCSResponce.Data[8];
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = decimal.Parse(uCSResponce.Data[1]) / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task<UCSResponce> RunCommand(string In, string Out, int TimeOut = 30000, params string[] Pars)
	{
		bool IsStartAgain = false;
		UCSResponce Rez;
		bool OpenSerial;
		while (true)
		{
			if (TimeWaitCommand != default(DateTime))
			{
				DateTime now = DateTime.Now;
				if (TimeWaitCommand > now)
				{
					Thread.Sleep((int)(TimeWaitCommand - now).TotalMilliseconds);
				}
				TimeWaitCommand = default(DateTime);
			}
			int IsTimeWaitCommand = 0;
			Error = "";
			Rez = null;
			Slip = "";
			bool WriteError = true;
			UCSStryctResponce ParType = ParTypes[In];
			OpenSerial = await PortOpenAsync();
			if (Error != "")
			{
				return Rez;
			}
			base.PortWriteTimeout = 3000;
			base.PortReadTimeout = TimeOut;
			await SendFrame(In, Pars, TimeOut);
			string text = "";
			foreach (string text2 in Pars)
			{
				text = text + " " + text2;
			}
			PortLogs.Append(ParType.Operation + ": " + In + " " + text, "<");
			if (Error != "")
			{
				break;
			}
			List<UCSResponce> ReadBuf = new List<UCSResponce>();
			bool PrintLine = false;
			DateTime CurTime = DateTime.Now;
			while (true)
			{
				UCSResponce uCSResponce = await GetFrame(TimeOut, WriteError);
				if (uCSResponce == null)
				{
					if (DateTime.Now.Subtract(CurTime).TotalMilliseconds < (double)TimeOut)
					{
						continue;
					}
					PortLogs.Append(Error);
					if (Rez == null && Error == "")
					{
						Error = "Ошибка приема кадра сообщения";
					}
				}
				else
				{
					text = "";
					foreach (string datum in uCSResponce.Data)
					{
						text = text + " " + datum;
					}
					PortLogs.Append(uCSResponce.Stryct.Operation + ": " + uCSResponce.Command + " " + text);
					ReadBuf.Add(uCSResponce);
					if (uCSResponce.Command == "3-1")
					{
						IsTimeWaitCommand = 3000;
					}
					if (uCSResponce.Command == "5-1")
					{
						IsTimeWaitCommand = 3000;
					}
					if (uCSResponce.Command == "5-4")
					{
						IsTimeWaitCommand = 3000;
					}
					if (uCSResponce.Command == "6-0")
					{
						IsTimeWaitCommand = 10000;
					}
					if (uCSResponce.Command == "5-X")
					{
						IsTimeWaitCommand = 15000;
					}
					if (uCSResponce.Command == "3-2")
					{
						IsTimeWaitCommand = 10000;
					}
					if (uCSResponce.Command == "3-2")
					{
						TimeOut = 10000;
						WriteError = false;
						if (!PrintLine && uCSResponce.Data[1].Length > 0 && uCSResponce.Data[1].IndexOf('\r') != -1)
						{
							Slip = Slip + uCSResponce.Data[1].Substring(0, uCSResponce.Data[1].IndexOf('\r')).Replace("\n", "\r\n") + "\r\n";
							PrintLine = true;
						}
						if (!PrintLine && uCSResponce.Data[1].Length > 0 && uCSResponce.Data[1][0] == '\n')
						{
							PrintLine = true;
						}
						if (!PrintLine)
						{
							Slip = Slip + uCSResponce.Data[1].Replace("\n", "\r\n") + "\r\n";
						}
						if (uCSResponce.LastLineFlag == true)
						{
							PrintLine = true;
						}
					}
					if (uCSResponce.Command == "3-1")
					{
						TimeOut = 3000;
						WriteError = false;
						if (uCSResponce.Data.Count >= 2 && uCSResponce.Data[1] == "1")
						{
							Error = Error + ((Error == "") ? "" : "; ") + "Устройству требуется инкассация";
						}
						if (uCSResponce.Data.Count >= 2 && uCSResponce.Data[1] == "2")
						{
							Error = Error + ((Error == "") ? "" : "; ") + "В устройстве закончилась бумага для печати";
						}
					}
					if (uCSResponce.Command == Out)
					{
						Rez = uCSResponce;
						TimeOut = 3000;
						WriteError = false;
					}
					if (uCSResponce.Command == "6-0")
					{
						TimeOut = 30000;
					}
					if (uCSResponce.Command == "5-1")
					{
						if (!IsStartAgain)
						{
							break;
						}
						TimeOut = 3000;
						WriteError = false;
						Error = Error + ((Error == "") ? "" : "; ") + "Устройство требует ввода пароля сначала";
					}
					if (uCSResponce.Command == "5-4")
					{
						TimeOut = 3000;
						Error = Error + ((Error == "") ? "" : "; ") + "Не обнаружена транзакция с таким ссылочным номером";
					}
					else if (uCSResponce.Command == "5-X")
					{
						TimeOut = 3000;
						Error = Error + ((Error == "") ? "" : "; ") + "Ошибка " + uCSResponce.Data[0];
						if (uCSResponce.Data.Count >= 2)
						{
							Error = Error + ": " + uCSResponce.Data[1];
						}
					}
					else if ((Rez == null && !(Out == "")) || (ParType.IsPrintLine && !PrintLine) || (uCSResponce != null && uCSResponce.Stryct.LastLineFlag && uCSResponce.LastLineFlag != true))
					{
						continue;
					}
				}
				if (IsTimeWaitCommand != 0)
				{
					TimeWaitCommand = DateTime.Now.AddMilliseconds(IsTimeWaitCommand);
				}
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				return Rez;
			}
			await RunCommand("3-0", "3-1", 3000);
			IsStartAgain = true;
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		return Rez;
	}

	public async Task<bool> SendFrame(string In, string[] Pars, int TimeOut)
	{
		UCSStryctResponce uCSStryctResponce = ParTypes[In];
		int num = 0;
		int num2 = 0;
		string[] array = Pars;
		foreach (string text in array)
		{
			num = ((uCSStryctResponce.Params[num2] == -1) ? (num + text.Length) : (num + uCSStryctResponce.Params[num2]));
			num2++;
		}
		string[] array2 = In.Split('-');
		char c = array2[0].ToCharArray()[0];
		char c2 = array2[1].ToCharArray()[0];
		int num3 = 0;
		num2 = 0;
		int[] array3 = uCSStryctResponce.Params;
		for (int i = 0; i < array3.Length; i++)
		{
			if (array3[i] == 0)
			{
				num3 = num3 + Pars[num2].Length + 1;
			}
			num2++;
		}
		byte[] Buf = new byte[num + 14 + num3];
		Buf[0] = (byte)c;
		Buf[1] = (byte)c2;
		Win1251.GetBytes(TerminalID.PadLeft(10, '0'), 0, 10, Buf, 2);
		Win1251.GetBytes(num.ToString("X2"), 0, 2, Buf, 12);
		int num4 = 14;
		int num5 = 0;
		array = Pars;
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i];
			int num6 = uCSStryctResponce.Params[num5];
			if (num6 != -1 && num6 != 0 && num6 > text2.Length)
			{
				text2 = text2.PadLeft(num6, '0');
			}
			else if (num6 != -1 && num6 != 0 && num6 < text2.Length)
			{
				text2 = text2.Substring(0, num6);
			}
			Win1251.GetBytes(text2, 0, text2.Length, Buf, num4);
			num4 += text2.Length;
			if (num6 == 0)
			{
				Win1251.GetBytes("\u001b", 0, 1, Buf, num4);
				num4++;
			}
			num5++;
		}
		Error = "";
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			for (int x = 0; x < 3; x++)
			{
				base.PortWriteTimeout = 3000;
				await PortWriteByteAsync(5);
				base.PortReadTimeout = 3000;
				byte b;
				try
				{
					b = await PortReadByteAsync();
				}
				catch
				{
					b = 0;
				}
				if (b == 6)
				{
					break;
				}
				if (x == 2)
				{
					Error = "Ошибка приема кадра сообщения - нет Prt_ACK";
					PortLogs.Append(Error);
					return false;
				}
			}
		}
		int CountStartBufAgain = 0;
		while (true)
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
			{
				base.PortWriteTimeout = 3000;
				await PortWriteByteAsync(16);
				await PortWriteByteAsync(2);
			}
			base.PortWriteTimeout = 5000;
			await PortWriteAsync(Buf, 0, Buf.Length);
			if (SetPort.TypeConnect != SetPorts.enTypeConnect.Com)
			{
				break;
			}
			base.PortWriteTimeout = 3000;
			await PortWriteByteAsync(16);
			await PortWriteByteAsync(3);
			byte b2 = 0;
			for (int j = 0; j < Buf.Length; j++)
			{
				b2 ^= Buf[j];
			}
			await PortWriteByteAsync(b2);
			base.PortReadTimeout = 5000;
			byte b;
			try
			{
				b = await PortReadByteAsync();
			}
			catch
			{
				b = 0;
			}
			if (b == 21 && CountStartBufAgain < 3)
			{
				CountStartBufAgain++;
				continue;
			}
			switch (b)
			{
			case 21:
				Error = "Ошибка приема кадра сообщения - нет подтверждения приема";
				PortLogs.Append(Error);
				return false;
			default:
				if (CountStartBufAgain < 3)
				{
					CountStartBufAgain++;
					continue;
				}
				break;
			case 6:
				break;
			}
			if (b != 6)
			{
				Error = "Ошибка приема кадра сообщения - нет подтверждения приема";
				PortLogs.Append(Error);
				return false;
			}
			base.PortWriteTimeout = 3000;
			await PortWriteByteAsync(4);
			break;
		}
		return true;
	}

	public async Task<UCSResponce> GetFrame(int TimeOut, bool WriteError = true)
	{
		UCSResponce Rez = new UCSResponce();
		base.PortReadTimeout = TimeOut;
		base.PortWriteTimeout = 3000;
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			byte b;
			try
			{
				b = await PortReadByteAsync();
			}
			catch
			{
				b = 0;
			}
			if (b != 5)
			{
				if (WriteError)
				{
					Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Prt_ENQ";
					PortLogs.Append(Error);
				}
				return null;
			}
			await PortWriteByteAsync(6);
		}
		int CountRead = 0;
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			byte b;
			try
			{
				b = await PortReadByteAsync();
			}
			catch
			{
				b = 0;
			}
			if (b != 16)
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Prt_DLE";
				PortLogs.Append(Error);
				return null;
			}
			try
			{
				b = await PortReadByteAsync();
			}
			catch
			{
				b = 0;
			}
			if (b != 2)
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Prt_ETX";
				PortLogs.Append(Error);
				return null;
			}
		}
		MemoryStream Data = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(Data);
		char Class;
		try
		{
			byte b2 = await PortReadByteAsync();
			Class = (char)b2;
			bw.Write(b2);
		}
		catch
		{
			if (WriteError)
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Class";
				PortLogs.Append(Error);
			}
			return null;
		}
		char c;
		try
		{
			byte b3 = await PortReadByteAsync();
			c = (char)b3;
			bw.Write(b3);
		}
		catch
		{
			Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Code";
			PortLogs.Append(Error);
			return null;
		}
		Rez.Command = Class + "-" + c;
		byte[] Buf = new byte[10];
		try
		{
			for (int x = 0; x < 10; x++)
			{
				byte[] array = Buf;
				int num = x;
				array[num] = await PortReadByteAsync();
			}
		}
		catch
		{
			Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Data";
			PortLogs.Append(Error);
			return null;
		}
		bw.Write(Buf);
		string TerID = "";
		try
		{
			TerID = int.Parse(Win1251.GetString(Buf)).ToString("D");
		}
		catch
		{
			Error = Error + ((Error == "") ? "" : "; ") + "Не правильный TerID";
		}
		Buf = new byte[2];
		try
		{
			for (int x = 0; x < 2; x++)
			{
				byte[] array = Buf;
				int num = x;
				array[num] = await PortReadByteAsync();
			}
		}
		catch
		{
			Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет длины кадра";
			PortLogs.Append(Error);
			return null;
		}
		bw.Write(Buf);
		Rez.Length = int.Parse(Win1251.GetString(Buf), NumberStyles.HexNumber);
		Buf = new byte[Rez.Length];
		try
		{
			for (int x = 0; x < Rez.Length; x++)
			{
				byte[] array = Buf;
				int num = x;
				array[num] = await PortReadByteAsync();
			}
		}
		catch
		{
			Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет данных";
			PortLogs.Append(Error);
			return null;
		}
		bw.Write(Buf);
		string sData = Win1251.GetString(Buf);
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			byte[] array2 = Data.ToArray();
			byte lrc = 0;
			for (int i = 0; i < array2.Length; i++)
			{
				lrc ^= array2[i];
			}
			byte b;
			try
			{
				b = await PortReadByteAsync();
			}
			catch
			{
				b = 0;
			}
			if (b != 16)
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Prt_DLE";
				PortLogs.Append(Error);
				return null;
			}
			try
			{
				b = await PortReadByteAsync();
			}
			catch
			{
				b = 0;
			}
			if (b != 3)
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет Prt_ETX";
				PortLogs.Append(Error);
				return null;
			}
			byte b4;
			try
			{
				b4 = await PortReadByteAsync();
			}
			catch
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - нет lrcR";
				PortLogs.Append(Error);
				return null;
			}
			if (b4 != lrc && CountRead < 3)
			{
				await PortWriteByteAsync(21);
			}
			else if (b4 != lrc)
			{
				Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - не правильная контрольная сумма";
				PortLogs.Append(Error);
				base.PortReadTimeout = 1000;
				await PortReadByteAsync();
				return null;
			}
			await PortWriteByteAsync(6);
			base.PortReadTimeout = 3000;
			try
			{
				await PortReadByteAsync();
			}
			catch
			{
			}
		}
		if (TerID != TerminalID)
		{
			Error = Error + ((Error == "") ? "" : "; ") + "Ошибка приема кадра сообщения - не правильный номер терминала";
			PortLogs.Append(Error);
			base.PortReadTimeout = 1000;
			await PortReadByteAsync();
			return null;
		}
		string command = Rez.Command;
		if (!ParTypes.ContainsKey(command))
		{
			Rez.Data.Add(sData);
		}
		else
		{
			UCSStryctResponce uCSStryctResponce = (Rez.Stryct = ParTypes[command]);
			int num2 = 0;
			int[] array3 = uCSStryctResponce.Params;
			foreach (int num3 in array3)
			{
				string text = "";
				int num4 = ((num3 == -1) ? 10000 : num3);
				for (int k = 0; k < num4; k++)
				{
					if (sData.Length > num2)
					{
						if (sData.Substring(num2, 1) == "\u001b")
						{
							num2++;
							break;
						}
						text += sData.Substring(num2, 1);
						num2++;
					}
				}
				Rez.Data.Add(text);
			}
			if (uCSStryctResponce.LastLineFlag)
			{
				Rez.LastLineFlag = Rez.Data[0] == "1";
			}
		}
		if (Rez.Command == "0-0")
		{
			return null;
		}
		return Rez;
	}

	public override async Task<bool> PortOpenAsync()
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			Error = "Устройство не настроено";
			PortLogs.Append(Error);
			return false;
		}
		_ = SetPort.PortOpen;
		return await base.PortOpenAsync();
	}

	public override async Task<bool> PortCloseAsync()
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			Error = "Устройство не настроено";
			PortLogs.Append(Error);
			return false;
		}
		return await base.PortCloseAsync();
	}
}
