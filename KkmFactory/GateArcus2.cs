using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateArcus2 : UnitPort
{
	private Encoding Win1251 = Encoding.GetEncoding(1251);

	public string TerminalID = "";

	public string Slip = "";

	private Encoding Encoding;

	private string DirectoryArcus2 = "";

	private string FileNameExe = "";

	private string FileNameSettings = "";

	private string FileNameOps = "";

	private string FileNameSlip = "";

	private string FileNameResult = "";

	private string FileNameOut = "";

	private string FileNameErrorHelp = "";

	private string ComPayByPaymentCard = "";

	private string ComReturnPaymentByPaymentCard = "";

	private string ComCancelPaymentByPaymentCard = "";

	private string ComAuthorisationByPaymentCard = "";

	private string ComAuthConfirmationByPaymentCard = "";

	private string ComEmergencyReversal = "";

	private string ComSettlement = "";

	private string ComTerminalReport = "";

	private string ComTerminalReportDetailed = "";

	private string ComTransactionDetails = "";

	private string ComPayByPaymentCardQR = "";

	private string ComReturnPaymentByPaymentCardQR = "";

	private Dictionary<string, string> KeyErrorHelp = new Dictionary<string, string>();

	public GateArcus2(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "Эквайринговые терминалы ARCUS 2 (ПО Аркус-2 должно быть настроено на протокол TPTP)";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		UnitVersion = Global.Verson;
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "";
		NameDevice = "ARCUS 2: Платежный терминал";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"DirectoryArcus2\" Caption=\"Путь к дистрибутиву\" TypeValue=\"String\" DefaultValue=\"C:\\Arcus2\" Description=\"Путь к папке, содержащей ПО ARCUS 2\" />\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"14400\">14400</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
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
			if (unitParamet.Key == "DirectoryArcus2")
			{
				DirectoryArcus2 = unitParamet.Value.Trim();
			}
		}
	}

	public override void SaveParametrs(Dictionary<string, string> NewParamets)
	{
		Dictionary<string, string> dictionary = null;
		try
		{
			dictionary = ReadIniFile(FileNameSettings, Encoding);
			string text = NewParamets["ComId"];
			if (text.Length > 4)
			{
				text = "\\\\.\\" + text;
			}
			SetValueForKeyUpp(dictionary, "PORT", text);
			SetValueForKeyUpp(dictionary, "SPEED", NewParamets["ComSpeed"]);
			WriteIniFile(FileNameSettings, dictionary, Encoding);
		}
		catch (Exception)
		{
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		Error = "";
		await base.InitDevice(FullInit, Program);
		Encoding = Encoding.GetEncoding(1251);
		string text = "CommandLineTool\\bin\\CommandLineTool.exe";
		string text2 = "INI\\cashreg.ini";
		string text3 = "INI\\ops.ini";
		string text4 = "cheq.out";
		string text5 = "rc.out";
		string text6 = "output.dat";
		string text7 = "INI\\rc_res.ini";
		FileNameExe = Path.Combine(DirectoryArcus2, text);
		FileNameSettings = Path.Combine(DirectoryArcus2, text2);
		FileNameOps = Path.Combine(DirectoryArcus2, text3);
		FileNameSlip = Path.Combine(DirectoryArcus2, text4);
		FileNameResult = Path.Combine(DirectoryArcus2, text5);
		FileNameOut = Path.Combine(DirectoryArcus2, text6);
		FileNameErrorHelp = Path.Combine(DirectoryArcus2, text7);
		Dictionary<string, string> data;
		try
		{
			data = ReadIniFile(FileNameSettings, Encoding);
		}
		catch (Exception)
		{
			Error = "Ошибка чтения файла настройки. Или указан не правильно каталог ПО ARCUS 2 или не хватает прав на чтение файла настроек ARCUS 2";
			return false;
		}
		if (GetValueForKeyUpp(data, "PORT") != null && GetValueForKeyUpp(data, "SPEED") != null)
		{
			string text8 = GetValueForKeyUpp(data, "PORT").Trim();
			if (text8.IndexOf("\\\\.\\") == 0)
			{
				text8 = text8.Substring(4);
			}
			UnitParamets["ComId"] = text8;
			UnitParamets["ComSpeed"] = GetValueForKeyUpp(data, "SPEED");
			SettDr.Paramets["ComId"] = UnitParamets["ComId"];
			SettDr.Paramets["ComSpeed"] = UnitParamets["ComSpeed"];
		}
		if (GetValueForKeyUpp(data, "CHEQ_FILE") != null)
		{
			FileNameSlip = Path.Combine(DirectoryArcus2, GetValueForKeyUpp(data, "CHEQ_FILE"));
		}
		if (GetValueForKeyUpp(data, "RESULT_FILE") != null)
		{
			FileNameResult = Path.Combine(DirectoryArcus2, GetValueForKeyUpp(data, "RESULT_FILE"));
		}
		if (GetValueForKeyUpp(data, "RC_RESOLVE_FILE") != null)
		{
			FileNameErrorHelp = Path.Combine(DirectoryArcus2, "INI", GetValueForKeyUpp(data, "RC_RESOLVE_FILE"));
		}
		try
		{
			KeyErrorHelp = ReadIniFile(FileNameErrorHelp, Encoding);
		}
		catch (Exception)
		{
			Error = "Ошибка чтения файла списка ошибок";
			return false;
		}
		try
		{
			string[] array = File.ReadAllText(FileNameOps, Encoding).Replace("\r\n", "\r").Replace("\n", "\r")
				.Split('\r');
			for (int i = 0; i < array.Length; i++)
			{
				string text9 = array[i].Trim();
				try
				{
					if (text9 == "" || text9[0] == '#')
					{
						continue;
					}
					string[] array2 = text9.Split('=');
					if (array2.Length < 2)
					{
						continue;
					}
					string[] array3 = array2[1].Split(',');
					if (array3.Length >= 2 && array2[0] != "")
					{
						int num = int.Parse(array2[0]);
						int num2 = int.Parse(array3[0]);
						int num3 = int.Parse(array3[1]);
						if (num2 == 1 && num3 == 1 && ComPayByPaymentCard == "")
						{
							ComPayByPaymentCard = num.ToString();
						}
						else if (num2 == 1 && num3 == 11 && ComReturnPaymentByPaymentCard == "")
						{
							ComReturnPaymentByPaymentCard = num.ToString();
						}
						else if (num2 == 1 && num3 == 5 && ComCancelPaymentByPaymentCard == "")
						{
							ComCancelPaymentByPaymentCard = num.ToString();
						}
						else if (num2 == 1 && num3 == 3 && ComAuthorisationByPaymentCard == "")
						{
							ComAuthorisationByPaymentCard = num.ToString();
						}
						else if (num2 == 1 && num3 == 4 && ComAuthConfirmationByPaymentCard == "")
						{
							ComAuthConfirmationByPaymentCard = num.ToString();
						}
						else if (num2 == 2 && num3 == 3 && ComEmergencyReversal == "")
						{
							ComEmergencyReversal = num.ToString();
						}
						else if (num2 == 2 && num3 == 1 && ComSettlement == "")
						{
							ComSettlement = num.ToString();
						}
						else if (num2 == 2 && num3 == 10 && ComTerminalReport == "")
						{
							ComTerminalReport = num.ToString();
						}
						else if (num2 == 2 && num3 == 0 && ComTerminalReportDetailed == "")
						{
							ComTerminalReportDetailed = num.ToString();
						}
						else if (num2 == 2 && num3 == 27 && ComTransactionDetails == "")
						{
							ComTransactionDetails = num.ToString();
						}
						else if (num2 == 2 && num3 == 22 && array3.Length >= 3 && array3[2].ToUpper().IndexOf("СВЕРКА ИТОГОВ") != -1 && ComSettlement == "")
						{
							ComSettlement = num.ToString();
						}
						else if (num2 == 2 && num3 == 41 && ComTerminalReport == "")
						{
							ComTerminalReport = num.ToString();
						}
						else if (num2 == 2 && num3 == 22 && ComTerminalReportDetailed == "")
						{
							ComTerminalReportDetailed = num.ToString();
						}
						else if (num2 == 0 && num3 == 128 && ComPayByPaymentCard == "")
						{
							ComPayByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 130 && ComReturnPaymentByPaymentCard == "")
						{
							ComReturnPaymentByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 225 && ComCancelPaymentByPaymentCard == "")
						{
							ComCancelPaymentByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 133 && ComAuthorisationByPaymentCard == "")
						{
							ComAuthorisationByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 134 && ComAuthConfirmationByPaymentCard == "")
						{
							ComAuthConfirmationByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 224 && ComEmergencyReversal == "")
						{
							ComEmergencyReversal = num.ToString();
						}
						else if (num2 == 0 && num3 == 161 && ComSettlement == "")
						{
							ComSettlement = num.ToString();
						}
						else if (num2 == 0 && num3 == 180 && ComTerminalReport == "")
						{
							ComTerminalReport = num.ToString();
						}
						else if (num2 == 0 && num3 == 187 && ComTerminalReportDetailed == "")
						{
							ComTerminalReportDetailed = num.ToString();
						}
						else if (num2 == 0 && num3 == 177 && ComTransactionDetails == "")
						{
							ComTransactionDetails = num.ToString();
						}
						else if (num2 == 0 && num3 == 0 && ComPayByPaymentCard == "")
						{
							ComPayByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 2 && ComCancelPaymentByPaymentCard == "")
						{
							ComCancelPaymentByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 1 && ComReturnPaymentByPaymentCard == "")
						{
							ComReturnPaymentByPaymentCard = num.ToString();
						}
						else if (num2 == 0 && num3 == 5 && ComSettlement == "")
						{
							ComSettlement = num.ToString();
						}
						else if (num2 == 1 && num3 == 109 && ComPayByPaymentCardQR == "")
						{
							ComPayByPaymentCardQR = num.ToString();
							SupportsSBP = true;
						}
						else if (num2 == 1 && num3 == 110 && ComReturnPaymentByPaymentCardQR == "")
						{
							ComReturnPaymentByPaymentCardQR = num.ToString();
						}
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception)
		{
			Error = "Ошибка чтения файла списка ошибок";
			return false;
		}
		DeleteAllFile();
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
		string arg;
		if (!DataCommand.OnSBP)
		{
			switch (Command)
			{
			case 0:
				if (ComPayByPaymentCard == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComPayByPaymentCard} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /v{DataCommand.AuthorizationCode} /r{DataCommand.RRNCode}";
				break;
			case 1:
				SetDictFromString(DataCommand.UniversalID, DataCommand);
				if (ComReturnPaymentByPaymentCard == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComReturnPaymentByPaymentCard} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /v{DataCommand.AuthorizationCode} /r{DataCommand.RRNCode} /x{DataCommand.IdProcessing}";
				break;
			case 2:
				SetDictFromString(DataCommand.UniversalID, DataCommand);
				if (ComCancelPaymentByPaymentCard == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = string.Format("/o{0} /a{1} /c643 /v{2} /r{3} /x{4}", ComCancelPaymentByPaymentCard, ((int)(DataCommand.Amount * 100m)).ToString(), DataCommand.AuthorizationCode, DataCommand.RRNCode);
				break;
			case 3:
				if (ComAuthorisationByPaymentCard == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComAuthorisationByPaymentCard} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /v{DataCommand.AuthorizationCode} /r{DataCommand.RRNCode}";
				break;
			case 4:
				SetDictFromString(DataCommand.UniversalID, DataCommand);
				if (ComAuthConfirmationByPaymentCard == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComAuthConfirmationByPaymentCard} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /v{DataCommand.AuthorizationCode} /r{DataCommand.RRNCode}";
				break;
			case 5:
				SetDictFromString(DataCommand.UniversalID, DataCommand);
				if (ComCancelPaymentByPaymentCard == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComCancelPaymentByPaymentCard} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /v{DataCommand.AuthorizationCode} /r{DataCommand.RRNCode} /x{DataCommand.IdProcessing}";
				break;
			default:
				RezultCommand.Status = ExecuteStatus.Error;
				RezultCommand.Error = "Драйвер не поддерживает эту команду";
				return;
			}
		}
		else
		{
			switch (Command)
			{
			case 0:
				if (ComPayByPaymentCardQR == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComPayByPaymentCardQR} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643";
				break;
			case 1:
				SetDictFromString(DataCommand.UniversalID, DataCommand);
				if (ComReturnPaymentByPaymentCardQR == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComReturnPaymentByPaymentCardQR} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /qx{DataCommand.ReceiptNumber}";
				break;
			case 2:
				SetDictFromString(DataCommand.UniversalID, DataCommand);
				if (ComReturnPaymentByPaymentCardQR == "")
				{
					throw new Exception("Команда не поддерживается Вашим банком");
				}
				arg = $"/o{ComReturnPaymentByPaymentCardQR} /a{((int)(DataCommand.Amount * 100m)).ToString()} /c643 /qx{DataCommand.ReceiptNumber}";
				break;
			case 3:
				throw new Exception("Команда не поддерживается Вашим банком");
			case 4:
				throw new Exception("Команда не поддерживается Вашим банком");
			case 5:
				throw new Exception("Команда не поддерживается Вашим банком");
			default:
				RezultCommand.Status = ExecuteStatus.Error;
				RezultCommand.Error = "Драйвер не поддерживает эту команду";
				return;
			}
		}
		Dictionary<string, string> dictionary = await RunCommand(arg);
		RezultCommand.CardNumber = dictionary["CardNumber"];
		RezultCommand.ReceiptNumber = dictionary["ReceiptNumber"];
		RezultCommand.RRNCode = dictionary["RRNCode"];
		RezultCommand.AuthorizationCode = dictionary["AuthorizationCode"];
		RezultCommand.IdProcessing = dictionary["IdProcessing"];
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = DataCommand.Amount;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		if (int.Parse(dictionary["RezultCode"]) != 0)
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.OnSBP)
		{
			await CommandPayTerminal(DataCommand, RezultCommand, 1);
			return;
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		if (ComEmergencyReversal == "")
		{
			throw new Exception("Команда не поддерживается Вашим банком");
		}
		string arg = $"/o{ComEmergencyReversal}";
		Dictionary<string, string> dictionary = await RunCommand(arg);
		RezultCommand.CardNumber = dictionary["CardNumber"];
		RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
		RezultCommand.RRNCode = dictionary["RRNCode"];
		RezultCommand.AuthorizationCode = dictionary["AuthorizationCode"];
		RezultCommand.IdProcessing = dictionary["IdProcessing"];
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

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.OnSBP)
		{
			throw new Exception("Команда не поддерживается Вашим банком");
		}
		if (ComSettlement == "")
		{
			throw new Exception("Команда не поддерживается Вашим банком");
		}
		string arg = $"/o{ComSettlement}";
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
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
		if (DataCommand.OnSBP)
		{
			throw new Exception("Команда не поддерживается Вашим банком");
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		string arg = "";
		if (!DataCommand.Detailed)
		{
			if (ComTerminalReport == "")
			{
				throw new Exception("Команда не поддерживается Вашим банком");
			}
			arg = $"/o{ComTerminalReport}";
		}
		else if (DataCommand.Detailed)
		{
			if (ComTerminalReportDetailed == "")
			{
				throw new Exception("Команда не поддерживается Вашим банком");
			}
			arg = $"/o{ComTerminalReportDetailed}";
		}
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

	public override async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.OnSBP)
		{
			throw new Exception("Команда не поддерживается Вашим банком");
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		if (ComTransactionDetails == "")
		{
			throw new Exception("Команда не поддерживается Вашим банком");
		}
		string arg = string.Format("/o{0} /a{0} /c643 /v{1} /r{2}", ComTransactionDetails, ((int)(DataCommand.Amount * 100m)).ToString(), DataCommand.AuthorizationCode, DataCommand.RRNCode);
		Dictionary<string, string> dictionary = await RunCommand(arg);
		RezultCommand.CardNumber = dictionary["CardNumber"];
		RezultCommand.ReceiptNumber = DataCommand.ReceiptNumber;
		RezultCommand.RRNCode = dictionary["RRNCode"];
		RezultCommand.AuthorizationCode = dictionary["AuthorizationCode"];
		RezultCommand.IdProcessing = dictionary["IdProcessing"];
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = DataCommand.Amount;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = Error;
		if (int.Parse(dictionary["RezultCode"]) != 0)
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
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
		string RezCode = "";
		string OutPar = "";
		Dictionary<string, string> Rez = new Dictionary<string, string>
		{
			{ "AuthorizationCode", "" },
			{ "RRNCode", "" },
			{ "CardNumber", "" },
			{ "ReceiptNumber", "" },
			{ "RezultCode", "" },
			{ "IdProcessing", "" }
		};
		PortLogs.Append(FileNameExe, "exe:");
		PortLogs.Append(Arg, "arg:");
		string workingDirectory = Path.Combine(DirectoryArcus2, "CommandLineTool\\bin");
		await Global.ExecuteCommandAsync(FileNameExe, workingDirectory, Arg, RunAsAdmin: false, RegisterError: false);
		try
		{
			Slip = await File.ReadAllTextAsync(FileNameSlip, Encoding);
		}
		catch
		{
		}
		try
		{
			RezCode = await File.ReadAllTextAsync(FileNameResult, Encoding);
			RezCode = RezCode.Replace("\r\n", "");
		}
		catch
		{
		}
		try
		{
			OutPar = await File.ReadAllTextAsync(FileNameOut, Encoding);
		}
		catch
		{
		}
		Rez["RezultCode"] = RezCode;
		if (int.Parse(RezCode) != 0)
		{
			Error = "Ошибка операции: (" + RezCode + "): ";
			if (KeyErrorHelp.ContainsKey(RezCode))
			{
				Error += KeyErrorHelp[RezCode];
			}
			PortLogs.Append(Error, "-");
			Slip = "";
			return Rez;
		}
		OutPar = OutPar.Replace("\r\n", "\r");
		string[] array = OutPar.Split('\r');
		if (array.Length >= 2)
		{
			Rez["CardNumber"] = array[1];
		}
		if (array.Length >= 4)
		{
			Rez["AuthorizationCode"] = array[3];
		}
		if (Rez["CardNumber"] == "")
		{
			Rez["CardNumber"] = GetStrInText(Slip, "Карта", "1234567890*");
		}
		if (Rez["AuthorizationCode"] == "")
		{
			Rez["AuthorizationCode"] = GetStrInText(Slip, "авторизац");
		}
		if (Rez["RRNCode"] == "")
		{
			Rez["RRNCode"] = GetStrInText(Slip, "RRN");
			if (Rez["RRNCode"] == "" || Rez["RRNCode"] == null)
			{
				Rez["RRNCode"] = GetStrInText(Slip, "Ссылка");
			}
			if (Rez["RRNCode"] == "" || Rez["RRNCode"] == null)
			{
				Rez["RRNCode"] = GetStrInText(Slip, "Ссылки");
			}
		}
		if (Rez["ReceiptNumber"] == "")
		{
			Rez["ReceiptNumber"] = GetStrInText(Slip, "SourceID");
		}
		if (Rez["IdProcessing"] == "")
		{
			Rez["IdProcessing"] = GetStrInText(Slip, "ID транзакции");
		}
		PortLogs.Append(RezCode, "rez:");
		return Rez;
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
			File.Delete(FileNameResult);
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
}
