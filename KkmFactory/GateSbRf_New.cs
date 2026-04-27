using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateSbRf_New : UnitPort
{
	private static object Lock_PilotNT = new object();

	private string DirectoryDll = "";

	private bool DllIsLosd;

	private decimal OldAmmount;

	private string OldAuthCode = "";

	private string OldRRNCode = "";

	private string OldCardID = "";

	public string Slip = "";

	private uint Department;

	private int VerAPI = 12;

	private string CurrencyСode = "643";

	private string NumDeviceByProcessing = "0";

	private bool ProcessRun;

	private CancellationTokenSource CancellToken;

	private Encoding Encoding;

	private string FileNameExe = "";

	private string FileNameSettings = "";

	private string FileNameSlip = "";

	private string FileNameOut = "";

	public GateSbRf_New(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		IsCommandCancelled = true;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n        <Group Caption='Настройки порта'>\r\n            <Parameter Name=\"NumDeviceByProcessing\" Caption=\"Физический терминал сбербанка\" TypeValue=\"String\" DefaultValue=\"0\">\r\n                <ChoiceList>\r\n                    <Item Value=\"0\">Это физическое устройство</Item>\r\n                    {СписокУстройствЭквайринга}\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"DirectoryDll\" Caption=\"Путь к дистрибутиву\" TypeValue=\"String\" DefaultValue=\"C:\\sc552\" Description=\"Путь к папке, содержащей библиотеку pilot_nt.dll\" MasterParameterName=\"NumDeviceByProcessing\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"0\" />\r\n        </Group>\r\n        <Group Caption='Прочие настройки'>\r\n            <Parameter Name='Department' Caption='Отдел по умолчанию' TypeValue='Number' DefaultValue='0'/>\r\n            <Parameter Name='КодыСимволовОтреза' Caption='Коды символов отреза' TypeValue='String' DefaultValue='01' Description=\"Бывают коды: 1, 16, 010D0A, 050D0D0A\" MasterParameterName=\"NumDeviceByProcessing\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"0\" />\r\n            <Parameter Name=\"CurrencyСode\" Caption=\"Код валюты\" TypeValue=\"String\" DefaultValue=\"643\" MasterParameterName=\"NumDeviceByProcessing\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"0\" >\r\n                <ChoiceList>\r\n                    <Item Value=\"643\">643</Item>\r\n                    <Item Value=\"810\">810</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name='ErrorDoubleCheck' Caption='Устранить ошибку дублирования слип-чеков' TypeValue='Boolean' DefaultValue='false'\r\n                    Description='Если есть задвоение слип-чеков сначала попробуйте в настройках терминала указать 1 копию слип-чека.\r\n                    Если не поможет взведите этот параметр.' \r\n                    MasterParameterName=\"NumDeviceByProcessing\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"0\" />\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		text2 = "";
		foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
		{
			if (device.Value.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал && device.Value.TypeDevice.UnitDevice == TypeDevice.enUnitDevice.GateSbRf && NumUnit != device.Value.NumDevice && (!device.Value.Paramets.ContainsKey("NumDeviceByProcessing") || device.Value.Paramets["NumDeviceByProcessing"] == "0"))
			{
				text2 = text2 + "<Item Value=\"" + device.Value.NumDevice + "\">" + device.Value.NumDevice + " - " + device.Value.TypeDevice.Protocol + "</Item>";
			}
		}
		text = text.Replace("{СписокУстройствЭквайринга}", text2);
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
			case "CurrencyСode":
				CurrencyСode = unitParamet.Value.Trim();
				break;
			case "NumDeviceByProcessing":
				NumDeviceByProcessing = unitParamet.Value.Trim();
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
		Encoding = Encoding.GetEncoding(866);
		FileNameExe = Path.Combine(DirectoryDll, "sb_pilot.exe");
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
			UnitParamets["ErrorDoubleCheck"] = ExtensionMethods.AsString(Val: false);
			SettDr.Paramets["ErrorDoubleCheck"] = ExtensionMethods.AsString(Val: false);
		}
		else if (GetValueForKeyUpp(data, "PrintEnd") != null)
		{
			UnitParamets["КодыСимволовОтреза"] = GetValueForKeyUpp(data, "PrintEnd");
			SettDr.Paramets["КодыСимволовОтреза"] = UnitParamets["КодыСимволовОтреза"];
			UnitParamets["ErrorDoubleCheck"] = ExtensionMethods.AsString(Val: false);
			SettDr.Paramets["ErrorDoubleCheck"] = ExtensionMethods.AsString(Val: false);
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
		if (!File.Exists(FileNameExe))
		{
			CopyFiles(DirectoryDll, FileNameExe);
			if (!Global.IsAdmin())
			{
				Global.RightsUp("SbRfCopyFile " + DirectoryDll + " " + FileNameExe);
			}
		}
		return true;
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

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		if (NumDeviceByProcessing != "0")
		{
			if (!DataCommand.Department.HasValue || DataCommand.Department == 0)
			{
				DataCommand.Department = Department;
			}
			Unit ExecuteUnit = Global.UnitManager.Units[int.Parse(NumDeviceByProcessing)];
			try
			{
				await ExecuteUnit.Semaphore.WaitAsync();
				ClPortLogs portLogs = ExecuteUnit.PortLogs;
				try
				{
					ExecuteUnit.PortLogs = PortLogs;
					ExecuteUnit.CommandPayTerminal(DataCommand, RezultCommand, Command).Wait();
					return;
				}
				finally
				{
					ExecuteUnit.PortLogs = portLogs;
				}
			}
			finally
			{
				ExecuteUnit.Semaphore.Release();
			}
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		await SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		string text = "";
		if (DataCommand.Department.HasValue && DataCommand.Department != 0)
		{
			text = "/d=" + DataCommand.Department.Value;
		}
		else
		{
			_ = Department;
			if (Department != 0)
			{
				text = "/d=" + Department;
			}
		}
		string text2;
		switch (Command)
		{
		case 0:
			text2 = $"1 {((int)(DataCommand.Amount * 100m)).ToString()} {text} /r={CurrencyСode}";
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			text2 = string.Format("3 {0} {1} {2} /r={3}", ((int)(DataCommand.Amount * 100m)).ToString(), (DataCommand.RRNCode.Trim() != "") ? ("0 0 " + DataCommand.RRNCode.Trim()) : "", text, CurrencyСode);
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			text2 = string.Format("8 {0} {1} {2} /r={3}", ((int)(DataCommand.Amount * 100m)).ToString(), (DataCommand.RRNCode.Trim() != "") ? ("0 0 " + DataCommand.RRNCode.Trim()) : "", text, CurrencyСode);
			break;
		case 100:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			text2 = string.Format("13 {0} {1} {2} /r={3}", ((int)(DataCommand.Amount * 100m)).ToString(), (DataCommand.AuthorizationCode.Trim() != "") ? DataCommand.AuthorizationCode.Trim() : "", text, CurrencyСode);
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		text2 = text2.Replace("  ", " ");
		text2 = text2.Replace("  ", " ");
		text2 = text2.Replace("  ", " ");
		Dictionary<string, string> dictionary = await RunCommand(text2);
		RezultCommand.CardNumber = dictionary["CardNumber"];
		RezultCommand.ReceiptNumber = dictionary["ReceiptNumber"];
		RezultCommand.RRNCode = dictionary["RRNCode"];
		RezultCommand.AuthorizationCode = dictionary["AuthorizationCode"];
		RezultCommand.CardHash = dictionary["CardHash"];
		try
		{
			RezultCommand.TransDate = DateTime.ParseExact(dictionary["TransDate"], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
		}
		catch
		{
		}
		RezultCommand.TerminalID = dictionary["TerminalID"];
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = DataCommand.Amount;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = Error;
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		if (int.Parse(dictionary["RezultCode"]) != 0)
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await ProcessCommandPayTerminal(DataCommand, RezultCommand, 100);
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (NumDeviceByProcessing != "0")
		{
			if (!DataCommand.Department.HasValue || DataCommand.Department == 0)
			{
				DataCommand.Department = Department;
			}
			Unit ExecuteUnit = Global.UnitManager.Units[int.Parse(NumDeviceByProcessing)];
			try
			{
				await ExecuteUnit.Semaphore.WaitAsync();
				ClPortLogs portLogs = ExecuteUnit.PortLogs;
				try
				{
					ExecuteUnit.PortLogs = PortLogs;
					ExecuteUnit.Settlement(DataCommand, RezultCommand).Wait();
					return;
				}
				finally
				{
					ExecuteUnit.PortLogs = portLogs;
				}
			}
			finally
			{
				ExecuteUnit.Semaphore.Release();
			}
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		await SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		string arg = $"7";
		decimal Amount = default(decimal);
		Dictionary<string, string> obj = await RunCommand(arg);
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = "";
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = Amount / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = Error;
		if (int.Parse(obj["RezultCode"]) != 0)
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (NumDeviceByProcessing != "0")
		{
			if (!DataCommand.Department.HasValue || DataCommand.Department == 0)
			{
				DataCommand.Department = Department;
			}
			Unit ExecuteUnit = Global.UnitManager.Units[int.Parse(NumDeviceByProcessing)];
			try
			{
				await ExecuteUnit.Semaphore.WaitAsync();
				ClPortLogs portLogs = ExecuteUnit.PortLogs;
				try
				{
					ExecuteUnit.PortLogs = PortLogs;
					ExecuteUnit.TerminalReport(DataCommand, RezultCommand).Wait();
					return;
				}
				finally
				{
					ExecuteUnit.PortLogs = portLogs;
				}
			}
			finally
			{
				ExecuteUnit.Semaphore.Release();
			}
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		await SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		string text = "";
		if (DataCommand.Department.HasValue && DataCommand.Department != 0)
		{
			text = "/d=" + DataCommand.Department.Value;
		}
		else
		{
			_ = Department;
			if (Department != 0)
			{
				text = "/d=" + Department;
			}
		}
		string text2 = "";
		if (!DataCommand.Detailed)
		{
			text2 = $"9 0";
		}
		else if (DataCommand.Detailed)
		{
			text2 = $"9 1";
		}
		text2 = text2 + " " + text;
		decimal Amount = default(decimal);
		Dictionary<string, string> obj = await RunCommand(text2);
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = "";
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = Amount / 100m;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = Error;
		if (int.Parse(obj["RezultCode"]) != 0)
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (NumDeviceByProcessing != "0")
		{
			if (!DataCommand.Department.HasValue || DataCommand.Department == 0)
			{
				DataCommand.Department = Department;
			}
			Unit ExecuteUnit = Global.UnitManager.Units[int.Parse(NumDeviceByProcessing)];
			try
			{
				await ExecuteUnit.Semaphore.WaitAsync();
				ExecuteUnit.PrintSlipOnTerminal(DataCommand, RezultCommand).Wait();
				return;
			}
			finally
			{
				ExecuteUnit.Semaphore.Release();
			}
		}
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		if (NumDeviceByProcessing != "0")
		{
			if (!DataCommand.Department.HasValue || DataCommand.Department == 0)
			{
				DataCommand.Department = Department;
			}
			Unit unit = Global.UnitManager.Units[int.Parse(NumDeviceByProcessing)];
			try
			{
				unit.Semaphore.Wait();
				unit.DoAdditionalAction(DataCommand, ref RezultCommand);
				return;
			}
			finally
			{
				unit.Semaphore.Release();
			}
		}
		_ = DataCommand.AdditionalActions == "ServiceMenu";
		base.DoAdditionalAction(DataCommand, ref RezultCommand);
	}

	private bool TestPinpad()
	{
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
		if (!UnitParamets["ErrorDoubleCheck"].AsBool() && text2.Length != 0 && text3.IndexOf(text2) != -1)
		{
			text3 = text3.Substring(0, text3.IndexOf(text2));
		}
		return text3.ToString();
	}

	public string GetStrInText(string Text, string Key, string GoodChar = "1234567890")
	{
		StringBuilder stringBuilder = new StringBuilder();
		int i = Text.ToUpper().IndexOf(Key.ToUpper());
		if (i == -1)
		{
			return stringBuilder.ToString();
		}
		int length = Text.Length;
		bool flag = false;
		for (; i < length - 1; i++)
		{
			char value = Text[i];
			bool flag2 = GoodChar.IndexOf(value) != -1;
			if (flag && !flag2)
			{
				break;
			}
			if (!flag && flag2)
			{
				flag = true;
			}
			if (flag2)
			{
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	public async Task<Dictionary<string, string>> RunCommand(string Arg)
	{
		DeleteAllFile();
		Error = "";
		Slip = "";
		Dictionary<string, string> Rez = new Dictionary<string, string>
		{
			{ "AuthorizationCode", "" },
			{ "RRNCode", "" },
			{ "CardNumber", "" },
			{ "RezultCode", "" },
			{ "ReceiptNumber", "" },
			{ "CardHash", "" },
			{ "TransDate", "" },
			{ "TerminalID", "" }
		};
		PortLogs.Append(FileNameExe, "exe:");
		PortLogs.Append(Arg, "arg:");
		ProcessRun = true;
		CancellToken = new CancellationTokenSource();
		try
		{
			if (!Global.isRunFromModuleTest)
			{
				await Global.ExecuteCommandAsync(FileNameExe, DirectoryDll, Arg, RunAsAdmin: false, RegisterError: false, "", CancellToken.Token);
			}
		}
		catch
		{
		}
		ProcessRun = false;
		OnUnitEvents("AfterRunCommand", DirectoryDll);
		try
		{
			Slip = await File.ReadAllTextAsync(FileNameSlip, Encoding);
		}
		catch
		{
		}
		string text;
		string text2;
		try
		{
			text = await File.ReadAllTextAsync(FileNameOut, Encoding);
			text2 = text.Substring(0, text.IndexOf(','));
			if (int.Parse(text2) != 0)
			{
				Rez["RezultCode"] = text2;
				Error = text.Substring(0, text.IndexOf('\n'));
				PortLogs.Append(Error, "-");
				Slip = "";
				return Rez;
			}
		}
		catch (Exception ex)
		{
			Rez["RezultCode"] = "999";
			Error = Error + ((Error == "") ? ", " : "") + "Ошибка операции: (" + ex.Message + "): ";
			PortLogs.Append(Error, "-");
			Slip = "";
			return Rez;
		}
		Rez["RezultCode"] = text2;
		string[] array = text.Split('\n');
		if (array.Length >= 2)
		{
			Rez["CardNumber"] = array[1].Replace("\r", "");
		}
		if (array.Length >= 4)
		{
			Rez["AuthorizationCode"] = array[3].Replace("\r", "");
		}
		if (array.Length >= 5)
		{
			Rez["ReceiptNumber"] = array[4].Replace("\r", "");
		}
		if (array.Length >= 8)
		{
			Rez["TerminalID"] = array[7].Replace("\r", "");
		}
		if (array.Length >= 9)
		{
			Rez["TransDate"] = array[8].Replace("\r", "");
		}
		if (array.Length >= 10)
		{
			Rez["RRNCode"] = array[9].Replace("\r", "");
		}
		if (array.Length >= 11)
		{
			Rez["CardHash"] = array[10].Replace("\r", "");
		}
		PortLogs.Append("Ответ банка:", "<");
		int num = 1;
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			PortLogs.AppendText($"[{num++}]" + text3.Trim());
		}
		PortLogs.Append("Код результата:" + text2, "<");
		return Rez;
	}

	private async Task SetPOLLING(bool OnOff, bool RunOff, DataCommand DataCommand, RezultCommand RezultCommand)
	{
		string text = Path.Combine(DirectoryDll, "POLLING.OFF");
		string text2 = Path.Combine(DirectoryDll, "POLLING.OK");
		if (!OnOff)
		{
			try
			{
				File.Delete(text);
				File.Delete(text2);
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
				File.Delete(text2);
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
		new Task(async delegate
		{
			int Timeout = 110;
			if (DataCommand.Timeout >= 30)
			{
				Timeout = DataCommand.Timeout - 3;
			}
			for (int ct = 0; ct < Timeout - 60; ct++)
			{
				await Task.Delay(1000);
				if (!ProcessRun)
				{
					return;
				}
				if (CancellationCommand)
				{
					break;
				}
			}
			if (ProcessRun && RezultCommand.Status != ExecuteStatus.Ok && RezultCommand.Status != ExecuteStatus.Error)
			{
				if (CancellationCommand)
				{
					Error = "Операция отменена вручную.";
					PortLogs.Append("Операция отменена вручную. Выставлен сигнал 'POLLING.OFF'", "-");
				}
				else
				{
					Error = "Операция отменена по таймауту.";
					PortLogs.Append("Операция отменена по таймауту. Выставлен сигнал 'POLLING.OFF'", "-");
				}
				await SetPOLLING(OnOff: true, RunOff: false, null, null);
				for (int ct = 0; ct <= 10; ct++)
				{
					await Task.Delay(1000);
					if (!ProcessRun)
					{
						break;
					}
				}
				if (ProcessRun)
				{
					PortLogs.Append("Процесс не останавливается, прерываем принудительно", "-");
					await CancellToken.CancelAsync();
				}
			}
		}).Start();
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
