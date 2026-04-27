using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateConsole : Unit
{
	private static object Lock_Gate = new object();

	private Encoding Encoding;

	private string FileNameExe = "";

	private string FileNameSlip = "";

	private string FileNameOut = "";

	private string Slip = "";

	private bool ProcessRun;

	private CancellationTokenSource CancellToken;

	private readonly GateINPAS GateINPAS;

	public GateConsole(Global.DeviceSettings SettDr, int NumUnit, GateINPAS GateINPAS)
		: base(SettDr, NumUnit)
	{
		this.GateINPAS = GateINPAS;
		Encoding = Encoding.GetEncoding(1251);
		FileNameExe = Path.Combine(GateINPAS.DirectoryDll, "DCConsole.bat");
		FileNameSlip = Path.Combine(GateINPAS.DirectoryDll, "receipt.txt");
		FileNameOut = Path.Combine(GateINPAS.DirectoryDll, "result.txt");
		if (!File.Exists(FileNameExe))
		{
			FileNameExe = Path.Combine(GateINPAS.DirectoryDll, "DC Console.bat");
		}
		if (!File.Exists(FileNameExe))
		{
			FileNameExe = Path.Combine(GateINPAS.DirectoryDll, "DCConsole.exe");
		}
		if (!File.Exists(FileNameExe))
		{
			FileNameExe = Path.Combine(GateINPAS.DirectoryDll, "DC Console.exe");
		}
		if (!File.Exists(FileNameExe))
		{
			FileNameExe = "";
		}
	}

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		await SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		int num = 180;
		if (DataCommand.Timeout >= num)
		{
			num = DataCommand.Timeout - 3;
		}
		string text = (num * 1000).ToString();
		string text2;
		switch (Command)
		{
		case 0:
			text2 = $"-o1 -a{((int)(DataCommand.Amount * 100m)).ToString()} -c643 -z{GateINPAS.TerminalID} -s{text}";
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			text2 = $"-o29 -a{((int)(DataCommand.Amount * 100m)).ToString()} -c643 -r{DataCommand.RRNCode.Trim()} -u{DataCommand.AuthorizationCode.Trim()} -z{GateINPAS.TerminalID} -s{text}";
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			text2 = $"-o4 -a{((int)(DataCommand.Amount * 100m)).ToString()} -c643 -r{DataCommand.RRNCode.Trim()} -u{DataCommand.AuthorizationCode.Trim()} -z{GateINPAS.TerminalID} -s{text}";
			break;
		case 100:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			text2 = string.Format("-o53 -z{0} -s{1}", ((int)(DataCommand.Amount * 100m)).ToString(), GateINPAS.TerminalID, text);
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		text2 = text2.Replace("  ", " ");
		Dictionary<string, string> dictionary = await RunCommand(text2);
		RezultCommand.CardNumber = dictionary["10"];
		string.IsNullOrEmpty(dictionary["108"]);
		RezultCommand.ReceiptNumber = dictionary["108"];
		RezultCommand.RRNCode = dictionary["14"];
		RezultCommand.AuthorizationCode = dictionary["13"];
		try
		{
			RezultCommand.TransDate = DateTime.ParseExact(dictionary["06"], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
		}
		catch
		{
		}
		RezultCommand.TerminalID = GateINPAS.TerminalID;
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = DataCommand.Amount;
		if (string.IsNullOrEmpty(Error))
		{
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		else
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		RezultCommand.Error = Error;
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		GateINPAS.Error = Error;
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await ProcessCommandPayTerminal(DataCommand, RezultCommand, 100);
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		await SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		int num = 180;
		if (DataCommand.Timeout >= num)
		{
			num = DataCommand.Timeout - 3;
		}
		string arg = (num * 1000).ToString();
		string text = $"-o59 -z{GateINPAS.TerminalID} -s{arg}";
		text = text.Replace("  ", " ");
		Dictionary<string, string> dictionary = await RunCommand(text, Del2Slip: false);
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = dictionary["80"];
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.TerminalID = GateINPAS.TerminalID;
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = default(decimal);
		if (string.IsNullOrEmpty(Error))
		{
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		else
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		RezultCommand.Error = Error;
		GateINPAS.Error = Error;
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		await SetPOLLING(OnOff: false, RunOff: true, DataCommand, RezultCommand);
		string arg = "";
		if (!DataCommand.Detailed)
		{
			arg = "20";
		}
		else if (DataCommand.Detailed)
		{
			arg = "21";
		}
		int num = 180;
		if (DataCommand.Timeout >= num)
		{
			num = DataCommand.Timeout - 3;
		}
		string arg2 = (num * 1000).ToString();
		string text = $"-o63 -m{arg} -z{GateINPAS.TerminalID} -s{arg2}";
		text = text.Replace("  ", " ");
		Dictionary<string, string> dictionary = await RunCommand(text, Del2Slip: false);
		RezultCommand.CardNumber = "";
		RezultCommand.ReceiptNumber = dictionary["80"];
		RezultCommand.RRNCode = "";
		RezultCommand.AuthorizationCode = "";
		RezultCommand.TerminalID = GateINPAS.TerminalID;
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = default(decimal);
		if (string.IsNullOrEmpty(Error))
		{
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		else
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		RezultCommand.Error = Error;
		GateINPAS.Error = Error;
	}

	public override async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		int num = 180;
		if (DataCommand.Timeout >= num)
		{
			num = DataCommand.Timeout - 3;
		}
		string arg = (num * 1000).ToString();
		string text = $"-o63 -m22 -z{GateINPAS.TerminalID} -s{arg}";
		text = text.Replace("  ", " ");
		Dictionary<string, string> dictionary = await RunCommand(text);
		RezultCommand.CardNumber = dictionary["10"];
		RezultCommand.ReceiptNumber = dictionary["80"];
		RezultCommand.RRNCode = dictionary["14"];
		RezultCommand.AuthorizationCode = dictionary["13"];
		RezultCommand.TerminalID = GateINPAS.TerminalID;
		RezultCommand.Slip = Slip;
		RezultCommand.Amount = default(decimal);
		if (string.IsNullOrEmpty(Error))
		{
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		else
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		RezultCommand.Error = Error;
		GateINPAS.Error = Error;
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "Драйвер не поддерживает эту команду";
	}

	public override void Test()
	{
	}

	public async Task<Dictionary<string, string>> RunCommand(string Arg, bool Del2Slip = true)
	{
		DeleteAllFile();
		Error = "";
		Slip = "";
		string RezCode = "";
		Dictionary<string, string> Rez = new Dictionary<string, string>();
		PortLogs.Append(FileNameExe, "exe:");
		PortLogs.Append(Arg, "arg:");
		ProcessRun = true;
		CancellToken = new CancellationTokenSource();
		if (FileNameExe == "")
		{
			throw new Exception("Не найден файл 'DCConsole.bat/exe .. \"DC Console.bat/exe\"' ");
		}
		try
		{
			if (!Global.isRunFromModuleTest)
			{
				await Global.ExecuteCommandAsync(FileNameExe, GateINPAS.DirectoryDll, Arg, RunAsAdmin: false, RegisterError: false, "", CancellToken.Token);
			}
		}
		catch
		{
		}
		ProcessRun = false;
		GateINPAS.OnUnitEvents("AfterRunCommand", GateINPAS.DirectoryDll);
		int CoolRead = 0;
		while (true)
		{
			try
			{
				Slip = await File.ReadAllTextAsync(FileNameSlip, Encoding);
				if (Slip.IndexOf("0xDF") == 0 && Slip.Length >= 5)
				{
					Slip = Slip.Substring(4);
				}
				if (Slip.IndexOf("^") == 0 && Slip.Length >= 2)
				{
					Slip = Slip.Substring(1);
				}
				if (Slip.IndexOf("^") == 0 && Slip.Length >= 2)
				{
					Slip = Slip.Substring(1);
				}
				if (Slip.IndexOf("^") == 0 && Slip.Length >= 2)
				{
					Slip = Slip.Substring(1);
				}
				if (!Del2Slip)
				{
					Slip = Slip.Replace("~0xDA^^", "\r\n");
				}
				if (Slip.IndexOf("~") != -1)
				{
					Slip = Slip.Substring(0, Slip.IndexOf("~") - 1);
				}
			}
			catch (Exception ex)
			{
				if (CoolRead < 4)
				{
					CoolRead++;
					await Task.Delay(500);
					continue;
				}
				Warning = Warning + ((Warning == "") ? ", " : "") + "Не удается прочитать файл Слип-чека: " + ex.Message;
				PortLogs.Append(Error, "-");
				Slip = "";
			}
			break;
		}
		CoolRead = 0;
		while (true)
		{
			try
			{
				PortLogs.Append("Ответ банка:", "<");
				string text = await File.ReadAllTextAsync(FileNameOut, Encoding);
				PortLogs.Append("result.txt: " + text, "<");
				string[] array = text.Replace("\r", "").Split("\n");
				foreach (string text2 in array)
				{
					if (!(text2 != "") || text2.IndexOf('[') == -1 || text2.IndexOf(']') == -1)
					{
						continue;
					}
					string[] array2 = text2.Split("]");
					if (array2[0].IndexOf('[') != -1)
					{
						array2[0] = array2[0].Substring(array2[0].IndexOf('[') + 1);
						if (array2[1].IndexOf('=') != -1)
						{
							array2[1] = array2[1].Substring(array2[1].IndexOf('=') + 1);
						}
						if (!Rez.ContainsKey(array2[0].Trim()))
						{
							Rez.Add(array2[0].Trim(), array2[1].Replace("'", "").Trim());
							PortLogs.AppendText(text2.Trim());
						}
					}
				}
			}
			catch (Exception ex2)
			{
				if (CoolRead < 4)
				{
					CoolRead++;
					await Task.Delay(500);
					continue;
				}
				Rez.Add("39", "13");
				Error = Error + ((Error == "") ? ", " : "") + "Ошибка операции: (" + ex2.Message + "): ";
				PortLogs.Append(Error, "-");
				Slip = "";
			}
			break;
		}
		try
		{
			if (int.Parse(Rez["39"]) != 1)
			{
				if (Rez.ContainsKey("19") && !string.IsNullOrEmpty(Rez["19"]))
				{
					Error = "Ошибка проведения транзакции: " + Rez["39"] + " - " + Rez["19"];
				}
				else if (Rez["39"] == "4")
				{
					Error = "Ошибка проведения транзакции: 4 - запрос не содержит полей";
				}
				else if (Rez["39"] == "11")
				{
					Error = "Ошибка проведения транзакции: 11 - ответ не содержит полей";
				}
				else if (Rez["39"] == "13")
				{
					Error = "Oбщая ошибка";
				}
				else
				{
					Error = "Oбщая ошибка";
				}
			}
		}
		catch
		{
		}
		if (!Rez.ContainsKey("39"))
		{
			Rez.Add("39", "Oбщая ошибка");
		}
		if (!Rez.ContainsKey("10"))
		{
			Rez.Add("10", "");
		}
		if (!Rez.ContainsKey("108"))
		{
			Rez.Add("108", "");
		}
		if (!Rez.ContainsKey("52"))
		{
			Rez.Add("52", "");
		}
		if (!Rez.ContainsKey("06"))
		{
			Rez.Add("06", "");
		}
		if (!Rez.ContainsKey("13"))
		{
			Rez.Add("13", "");
		}
		if (!Rez.ContainsKey("14"))
		{
			Rez.Add("14", "");
		}
		if (!Rez.ContainsKey("80"))
		{
			Rez.Add("80", "");
		}
		PortLogs.Append("Код результата:" + RezCode, "<");
		return Rez;
	}

	private async Task SetPOLLING(bool OnOff, bool RunOff, DataCommand DataCommand, RezultCommand RezultCommand)
	{
		if (!RunOff)
		{
			return;
		}
		new Task(async delegate
		{
			int Timeout = 210;
			if (DataCommand.Timeout >= Timeout)
			{
				Timeout = DataCommand.Timeout - 3;
			}
			for (int ct = 0; ct < Timeout + 10; ct++)
			{
				await Task.Delay(1000);
				if (!ProcessRun)
				{
					return;
				}
			}
			if (ProcessRun && RezultCommand.Status != ExecuteStatus.Ok && RezultCommand.Status != ExecuteStatus.Error)
			{
				Error = "Операция отменена по таймауту.";
				PortLogs.Append("Операция отменена по таймауту.", "-");
				if (ProcessRun)
				{
					PortLogs.Append("Процесс не останавливается, прерываем принудительно", "-");
					await CancellToken.CancelAsync();
				}
			}
		}).Start();
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
}
