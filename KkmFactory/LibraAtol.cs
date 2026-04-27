using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class LibraAtol : UnitPort
{
	public LibraAtol(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
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
			}
		}
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = Global.Description;
		UnitEquipmentType = "ЭлектронныеВесы";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "http://www.Atol.ru";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP: адрес\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"5001\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"9600\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\"\r\n                Description = \"Для 'Пассивный' протокол-9600\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"Protocol\" Caption=\"Протокол\" TypeValue=\"Number\" DefaultValue=\"0\">\r\n                <ChoiceList>\r\n                    <Item Value=\"0\">'Пассивный' протокол</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
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

	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		IsInit = true;
		NameDevice = "Атол";
		return true;
	}

	public override async Task Calibrate(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		if (UnitParamets["Protocol"].AsInt() == 0)
		{
			await CalibrateP1(DataCommand, RezultCommand);
		}
	}

	public override async Task GetWeight(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		if (UnitParamets["Protocol"].AsInt() == 0)
		{
			await GetWeightP1(DataCommand, RezultCommand);
		}
		await ComDevice.PostCheck(RezultCommand, this);
	}

	public async Task CalibrateP1(DataCommand DataCommand, RezultCommandLibra RezultCommand, Parity Parity = Parity.None)
	{
		bool OpenSerial = await PortOpenAsync(Parity);
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		base.PortReadTimeout = 2000;
		base.PortWriteTimeout = 10;
		Error = "";
		base.PortReadTimeout = 1;
		try
		{
			for (int i = 0; i < 10; i++)
			{
				await PortReadByteAsync();
			}
		}
		catch (Exception)
		{
		}
		base.PortReadTimeout = 2000;
		Error = "";
		try
		{
			await PortWriteAsync(new byte[1] { 5 }, 0, 1);
			if (await PortReadByteAsync() != 6)
			{
				throw new Exception("Ошибка приема кадра сообщения (1)");
			}
			await PortWriteAsync(new byte[5] { 60, 84, 75, 62, 9 }, 0, 5);
			byte b = await PortReadByteAsync();
			if (b != 6 && b != 21)
			{
				throw new Exception("Ошибка приема кадра сообщения (3)");
			}
			try
			{
				base.PortReadTimeout = 1;
				await PortReadByteAsync();
				base.PortReadTimeout = 2000;
			}
			catch (Exception)
			{
			}
		}
		catch (Exception)
		{
			Error = "Ошибка передачи команды";
			PortLogs.Append(Error);
		}
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public async Task GetWeightP1(DataCommand DataCommand, RezultCommandLibra RezultCommand, Parity Parity = Parity.None)
	{
		bool OpenSerial = await PortOpenAsync(Parity);
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		base.PortReadTimeout = 2000;
		base.PortWriteTimeout = 10;
		for (int x = 0; x < 5; x++)
		{
			Error = "";
			base.PortReadTimeout = 1;
			try
			{
				for (int i = 0; i < 10; i++)
				{
					await PortReadByteAsync();
				}
			}
			catch (Exception)
			{
			}
			base.PortReadTimeout = 2000;
			Error = "";
			byte[] WeightB = new byte[6];
			try
			{
				await PortWriteAsync(new byte[1] { 5 }, 0, 1);
				if (await PortReadByteAsync() != 6)
				{
					throw new Exception("Ошибка приема кадра сообщения (1)");
				}
				await PortWriteAsync(new byte[1] { 17 }, 0, 1);
				if (await PortReadByteAsync() != 1)
				{
					throw new Exception("Ошибка приема кадра сообщения (2)");
				}
				if (await PortReadByteAsync() != 2)
				{
					throw new Exception("Ошибка приема кадра сообщения (3)");
				}
				byte STA = await PortReadByteAsync();
				byte Sign = await PortReadByteAsync();
				for (int i = 0; i < 6; i++)
				{
					byte[] array = WeightB;
					int num = i;
					array[num] = await PortReadByteAsync();
				}
				byte[] Unit = new byte[3];
				for (int i = 0; i < 3; i++)
				{
					byte[] array = Unit;
					int num = i;
					array[num] = await PortReadByteAsync();
				}
				if (await PortReadByteAsync() != 3 && await PortReadByteAsync() != 3)
				{
					throw new Exception("Ошибка приема кадра сообщения (3)");
				}
				if (await PortReadByteAsync() != 4)
				{
					throw new Exception("Ошибка приема кадра сообщения (3)");
				}
				try
				{
					base.PortReadTimeout = 1;
					await PortReadByteAsync();
					base.PortReadTimeout = 2000;
				}
				catch (Exception)
				{
				}
				decimal num2 = decimal.Parse(Encoding.ASCII.GetString(WeightB), CultureInfo.InvariantCulture);
				num2 = ((Sign == 45) ? (num2 * -1m) : num2);
				RezultCommand.Weight = num2;
				if (STA == 85)
				{
					Thread.Sleep(1000);
					continue;
				}
			}
			catch (Exception)
			{
				Error = "Ошибка передачи команды";
				PortLogs.Append(Error);
				continue;
			}
			break;
		}
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public override async Task<bool> PortOpenAsync(Parity Parity = Parity.None)
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			Error = "Устройство не настроено";
			PortLogs.Append(Error);
			return false;
		}
		_ = SetPort.PortOpen;
		bool result = await base.PortOpenAsync(Parity);
		if (SetPort.SerialPort != null)
		{
			SetPort.SerialPort.DtrEnable = false;
			SetPort.SerialPort.RtsEnable = false;
		}
		return result;
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
