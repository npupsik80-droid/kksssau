using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace KkmFactory;

internal class QrScreen : UnitPort
{
	public QrScreen(Global.DeviceSettings SettDr, int NumUnit)
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
				if (unitParamet.Value == "2")
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.Com;
				}
				else
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.None;
				}
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
		UnitDescription = "Дисплей QR кодов QR-Screen";
		UnitEquipmentType = "ДисплеиПокупателя";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://qr-screen.ru/product/";
		UnitAdditionallinks = "<a href='https://qr-screen.ru/product/'>Информация по дисплеям</a><br/>";
		NameDevice = "QR-Screen: Дисплей покупателя";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\"\r\n                Description = \"Для 'Пассивный' протокол-9600\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
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
		NameDevice = "QR-Screen";
		return true;
	}

	public override async Task OutputOnCustomerDisplay(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		base.PortReadTimeout = 10;
		base.PortWriteTimeout = 100;
		Error = "";
		await ClearCustomerDisplay(null, RezultCommand);
		if (Error != "")
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return;
		}
		RezultCommand.Status = ExecuteStatus.Run;
		try
		{
			Encoding e886 = Encoding.UTF8;
			if (DataCommand.TopString != null && DataCommand.TopString != "")
			{
				byte[] bytes = e886.GetBytes("[TXT1]" + DataCommand.TopString + "\r\n");
				SetPort.SerialPort.DiscardInBuffer();
				await PortWriteAsync(bytes, 0, bytes.Length);
			}
			if (DataCommand.CodeQR != null && DataCommand.CodeQR != "")
			{
				byte[] bytes2 = e886.GetBytes("[QRL]" + DataCommand.CodeQR + "\r\n");
				SetPort.SerialPort.DiscardInBuffer();
				await PortWriteAsync(bytes2, 0, bytes2.Length);
			}
			if (DataCommand.BottomString != null && DataCommand.BottomString != "")
			{
				byte[] bytes3 = e886.GetBytes("[TXT2]" + DataCommand.BottomString + "\r\n");
				SetPort.SerialPort.DiscardInBuffer();
				await PortWriteAsync(bytes3, 0, bytes3.Length);
			}
		}
		catch (Exception)
		{
			Error = "Ошибка передачи команды";
			PortLogs.Append(Error);
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task ClearCustomerDisplay(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		base.PortReadTimeout = 100;
		base.PortWriteTimeout = 100;
		Error = "";
		try
		{
			Encoding e886 = Encoding.UTF8;
			byte[] bytes = e886.GetBytes("[TXT1]\r\n");
			SetPort.SerialPort.DiscardInBuffer();
			await PortWriteAsync(bytes, 0, bytes.Length);
			bytes = e886.GetBytes("[CQL]\r\n");
			SetPort.SerialPort.DiscardInBuffer();
			await PortWriteAsync(bytes, 0, bytes.Length);
			bytes = e886.GetBytes("[TXT2]\r\n");
			SetPort.SerialPort.DiscardInBuffer();
			await PortWriteAsync(bytes, 0, bytes.Length);
		}
		catch (Exception)
		{
			Error = "Ошибка передачи команды";
			PortLogs.Append(Error);
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task OptionsCustomerDisplay(DataCommand DataCommand, RezultCommandCD RezultCommand)
	{
		RezultCommand.IsTopString = true;
		RezultCommand.IsCodeQR = true;
		RezultCommand.IsBottomString = true;
		RezultCommand.Status = ExecuteStatus.Ok;
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
