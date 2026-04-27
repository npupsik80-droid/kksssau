using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateINPAS : UnitPort
{
	private Unit GateINPASdll;

	public string TerminalID = "";

	public string DirectoryDll = "";

	public GateINPAS(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
	}

	public Assembly AssemblyResolve(object sender, ResolveEventArgs args)
	{
		if (args.Name.Contains("DualConnector"))
		{
			return Assembly.LoadFrom(Path.Combine(DirectoryDll, "DualConnector.dll"));
		}
		return null;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n    <Group Caption='Настройки'>\r\n            <Parameter Name=\"TerminalID\" Caption=\"Идентификатор терминала\" TypeValue=\"String\" \r\n                Help=\"Узнать номер можно на экране терминала или в слип-чеке\" />\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\" Description=\"\" >\r\n                <ChoiceList>\r\n                    <Item Value=\"2\">COM порт (SmartSale)</Item>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi (SmartSale)</Item>\r\n                    <Item Value=\"0\">Через DualConnectorl</Item><Item Value=\"3\">Через DLL DualConnectorl</Item>                </ChoiceList>\r\n            </Parameter>\r\n        <Parameter Name=\"DirectoryDll\" Caption=\"Путь к дистрибутиву\" TypeValue=\"String\" DefaultValue=\"C:\\Program Files (x86)\\INPAS\\DualConnector\\\" Description=\"Путь к папке, содержащей библиотеку DualConnector.dll\" \r\n            MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"0\"\r\n            Help=\"После изменения необходимо перезапустить kkmserver\"/>\r\n        <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\" Description=\"\" >\r\n            <ChoiceList>\r\n                #ChoiceListCOM#\r\n            </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n            <ChoiceList>\r\n                <Item Value=\"2400\">2400</Item>\r\n                <Item Value=\"4800\">4800</Item>\r\n                <Item Value=\"9600\">9600</Item>\r\n                <Item Value=\"14400\">14400</Item>\r\n                <Item Value=\"19200\">19200</Item>\r\n                <Item Value=\"38400\">38400</Item>\r\n                <Item Value=\"57600\">57600</Item>\r\n                <Item Value=\"115200\">115200</Item>\r\n            </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"IP\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" DefaultValue=\"192.168.1.100\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\" Description=\"\" />\r\n        <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"6060\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n    </Group>\r\n    </Page>\r\n</Settings>";
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "INPAS: Эквайринговые терминалы.\n\rВ настройках терминала необходимо установить признак работы терминала с кассой.";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		UnitVersion = Global.Verson;
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "";
		NameDevice = "INPAS: Платежный терминал";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			if (item.Key.IndexOf("COM") == 0)
			{
				text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
			}
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		text = text.Replace("'", "\"");
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
				else if (unitParamet.Value == "3")
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.Software;
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
			{
				if (unitParamet.Value.GetType() == typeof(string))
				{
					SetPort.ComId = unitParamet.Value.Trim();
					break;
				}
				string text = unitParamet.Value.Trim();
				if (text.Length < 3)
				{
					text = "COM" + text;
				}
				SetPort.ComId = text;
				break;
			}
			case "ComSpeed":
				SetPort.ComSpeed = unitParamet.Value.AsInt();
				break;
			case "TerminalID":
				TerminalID = unitParamet.Value.Trim();
				break;
			case "DirectoryDll":
				DirectoryDll = unitParamet.Value.Trim();
				break;
			}
		}
		try
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
			{
				GateINPASdll = new GateConsole(null, 0, this);
			}
			else if (SetPort.TypeConnect == SetPorts.enTypeConnect.Software)
			{
				GateINPASdll = new GateINPASdll(null, 0, this);
			}
			else
			{
				GateINPASdll = new GateSmartSale(null, 0, this);
			}
		}
		catch (Exception ex)
		{
			Error = "Ошибка инициализации: " + ex.Message;
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		Error = "";
		await base.InitDevice(FullInit, Program);
		try
		{
			Test();
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
		await GateINPASdll.CommandPayTerminal(DataCommand, RezultCommand, Command);
		NetLogs = GateINPASdll.NetLogs;
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await GateINPASdll.EmergencyReversal(DataCommand, RezultCommand);
		NetLogs = GateINPASdll.NetLogs;
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await GateINPASdll.Settlement(DataCommand, RezultCommand);
		NetLogs = GateINPASdll.NetLogs;
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await GateINPASdll.TerminalReport(DataCommand, RezultCommand);
		NetLogs = GateINPASdll.NetLogs;
	}

	public override async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await GateINPASdll.TransactionDetails(DataCommand, RezultCommand);
		NetLogs = GateINPASdll.NetLogs;
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await GateINPASdll.PrintSlipOnTerminal(DataCommand, RezultCommand);
		NetLogs = GateINPASdll.NetLogs;
	}

	public override void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		base.DoAdditionalAction(DataCommand, ref RezultCommand);
	}

	public override void Test()
	{
		GateINPASdll.Test();
		NetLogs = GateINPASdll.NetLogs;
	}

	public override async Task<bool> PortOpenAsync()
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			return false;
		}
		return await base.PortOpenAsync();
	}

	public override async Task<bool> PortCloseAsync()
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			return false;
		}
		return await base.PortCloseAsync();
	}
}
