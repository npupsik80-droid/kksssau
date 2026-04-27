using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class LibraMassaK : UnitPort
{
	public LibraMassaK(Global.DeviceSettings SettDr, int NumUnit)
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
		UnitDownloadURL = "http://www.massa.ru";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP: адрес\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"5001\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"4800\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\"\r\n                Description = \"Для 'Протокол 1c'-57600, для 'Протокол STANDART'-19200, остальное 4800\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"Protocol\" Caption=\"Протокол\" TypeValue=\"Number\" DefaultValue=\"0\">\r\n                <ChoiceList>\r\n                    <Item Value=\"0\">Протокол №2</Item>\r\n                    <Item Value=\"1\">Протокол STANDART</Item>\r\n                    <!--<Item Value=\"2\">Протокол №3</Item>\r\n                    <Item Value=\"3\">Протокол №9</Item>-->\r\n                    <Item Value=\"4\">Протокол 1c</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
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
		NameDevice = "Масса-К";
		return true;
	}

	public override async Task Calibrate(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		string val = UnitParamets["Protocol"];
		if (val.AsInt() == 0)
		{
			await CalibrateP2(DataCommand, RezultCommand, Parity.Even);
		}
		else if (val.AsInt() == 1)
		{
			await CalibrateP2(DataCommand, RezultCommand);
		}
		else if (val.AsInt() == 4)
		{
			await CalibrateP3(DataCommand, RezultCommand);
		}
	}

	public override async Task GetWeight(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		string val = UnitParamets["Protocol"];
		if (val.AsInt() == 0)
		{
			await GetWeightP2(DataCommand, RezultCommand, Parity.Even);
		}
		else if (val.AsInt() == 1)
		{
			await GetWeightP2(DataCommand, RezultCommand);
		}
		else if (val.AsInt() == 4)
		{
			await GetWeightP3(DataCommand, RezultCommand);
		}
		await ComDevice.PostCheck(RezultCommand, this);
	}

	public async Task CalibrateP2(DataCommand DataCommand, RezultCommandLibra RezultCommand, Parity Parity = Parity.None)
	{
		bool OpenSerial = await PortOpenAsync(Parity);
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		base.PortReadTimeout = 1000;
		base.PortWriteTimeout = 1000;
		try
		{
			await PortWriteAsync(new byte[1] { 13 }, 0, 1);
		}
		catch (Exception)
		{
			Error = "Ошибка передачи команды";
			RezultCommand.Status = ExecuteStatus.Error;
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return;
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public async Task GetWeightP2(DataCommand DataCommand, RezultCommandLibra RezultCommand, Parity Parity = Parity.None)
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
			base.PortReadTimeout = 10;
			try
			{
				await PortReadByteAsync();
				await PortReadByteAsync();
			}
			catch (Exception)
			{
			}
			base.PortReadTimeout = 2000;
			Error = "";
			try
			{
				await PortWriteAsync(new byte[1] { 72 }, 0, 1);
			}
			catch (Exception)
			{
				Error = "Ошибка передачи команды";
				PortLogs.Append(Error);
				continue;
			}
			byte Response1;
			try
			{
				Response1 = await PortReadByteAsync();
			}
			catch (Exception)
			{
				Error = "Ошибка приема кадра сообщения (1)";
				PortLogs.Append(Error);
				continue;
			}
			byte b;
			try
			{
				b = await PortReadByteAsync();
			}
			catch (Exception)
			{
				Error = "Ошибка приема кадра сообщения (2)";
				PortLogs.Append(Error);
				continue;
			}
			if ((Response1 & 0x80) <= 0)
			{
				Thread.Sleep(1000);
				continue;
			}
			bool Gramm = b == 0;
			try
			{
				await PortWriteAsync(new byte[1] { 69 }, 0, 1);
			}
			catch (Exception)
			{
				Error = "Ошибка передачи команды";
				break;
			}
			try
			{
				Response1 = await PortReadByteAsync();
			}
			catch (Exception)
			{
				Error = "Ошибка приема кадра сообщения (1)";
				PortLogs.Append(Error);
				break;
			}
			try
			{
				b = await PortReadByteAsync();
			}
			catch (Exception)
			{
				Error = "Ошибка приема кадра сообщения (2)";
				PortLogs.Append(Error);
				break;
			}
			uint num = (uint)(Response1 + (b << 8));
			int num2 = (((num & 0x8000) == 0) ? 1 : (-1));
			RezultCommand.Weight = num & (32767 * num2);
			if (!Gramm)
			{
				RezultCommand.Weight /= 10m;
			}
			RezultCommand.Weight /= 1000m;
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

	public async Task CalibrateP3(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)0);
		binaryWriter.Write((byte)0);
		binaryWriter.Write((byte)0);
		binaryWriter.Write((byte)0);
		await RunCommand(163u, memoryStream);
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
		}
		else if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public async Task GetWeightP3(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			byte[] array = await RunCommand(160u, new MemoryStream());
			if (Error != "")
			{
				RezultCommand.Status = ExecuteStatus.Error;
				return;
			}
			if (array[6] == 0)
			{
				Thread.Sleep(1000);
				continue;
			}
			ulong num = 0uL;
			for (int j = 0; j < 4; j++)
			{
				num += (uint)(array[j + 1] << 8 * j);
			}
			decimal weight = num;
			if (array[5] == 0)
			{
				weight /= 10000m;
			}
			else if (array[5] == 1)
			{
				weight /= 1000m;
			}
			else if (array[5] == 2)
			{
				weight /= 100m;
			}
			else if (array[5] == 3)
			{
				weight /= 10m;
			}
			else if (array[5] == 4)
			{
				weight /= 1m;
			}
			RezultCommand.Weight = weight;
			break;
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public async Task<byte[]> RunCommand(uint Command, MemoryStream Msg, int TimeOut = 5000)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			return new byte[0];
		}
		base.PortReadTimeout = TimeOut;
		await SendFrame(Command, Msg);
		if (Error != "")
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return new byte[0];
		}
		byte[] bData = await GetFrame(TimeOut);
		if (Error != "")
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return new byte[0];
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		return bData;
	}

	public async Task<bool> SendFrame(uint Command, MemoryStream Msg)
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			Error = "";
			binaryWriter.Write((byte)248);
			binaryWriter.Write((byte)85);
			binaryWriter.Write((byte)206);
			int num = (int)(1 + Msg.Length);
			binaryWriter.Write((byte)(num & 0xFF));
			binaryWriter.Write((byte)(num >> 8));
			binaryWriter.Write((byte)Command);
			binaryWriter.Write(Msg.ToArray());
			byte[] array = memoryStream.ToArray();
			uint num2 = 0u;
			for (int i = 5; i < array.Length; i++)
			{
				uint num3 = 0u;
				uint num4 = num2 >> 8 << 8;
				for (uint num5 = 0u; num5 < 8; num5++)
				{
					num3 = ((((num4 ^ num3) & 0x8000) == 0) ? (num3 << 1) : ((num3 << 1) ^ 0x1021));
					num4 <<= 1;
				}
				num2 = num3 ^ (num2 << 8) ^ (uint)(array[i] & 0xFF);
			}
			num2 &= 0xFFFF;
			binaryWriter.Write((byte)(num2 & 0xFF));
			binaryWriter.Write((byte)(num2 >> 8));
			base.PortReadTimeout = 500;
			base.PortWriteTimeout = 500;
			await PortWriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length);
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

	public async Task<byte[]> GetFrame(int TimeOut)
	{
		Error = "";
		MemoryStream Data = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(Data);
		base.PortReadTimeout = TimeOut;
		base.PortWriteTimeout = 500;
		while (true)
		{
			byte b;
			try
			{
				b = await PortReadByteAsync();
			}
			catch (Exception)
			{
				Error = "Ошибка приема кадра сообщения (1)";
				PortLogs.Append(Error);
				if (Global.Settings.SetNotActiveOnError)
				{
					IsInit = false;
				}
				return Data.ToArray();
			}
			if (b != 248)
			{
				continue;
			}
			int i = 0;
			while (true)
			{
				if (i < 2)
				{
					byte Char = 0;
					switch (i)
					{
					case 0:
						Char = 85;
						break;
					case 1:
						Char = 206;
						break;
					}
					try
					{
						b = await PortReadByteAsync();
					}
					catch (Exception)
					{
						Error = "Ошибка приема кадра сообщения (1)";
						PortLogs.Append(Error);
						if (Global.Settings.SetNotActiveOnError)
						{
							IsInit = false;
						}
						return Data.ToArray();
					}
					if (b != Char)
					{
						break;
					}
					i++;
					continue;
				}
				byte SizeFrame1;
				try
				{
					SizeFrame1 = await PortReadByteAsync();
				}
				catch (Exception)
				{
					Error = "Ошибка приема кадра сообщения (3)";
					PortLogs.Append(Error);
					if (Global.Settings.SetNotActiveOnError)
					{
						IsInit = false;
					}
					return Data.ToArray();
				}
				byte b2;
				try
				{
					b2 = await PortReadByteAsync();
				}
				catch (Exception)
				{
					Error = "Ошибка приема кадра сообщения (3)";
					PortLogs.Append(Error);
					if (Global.Settings.SetNotActiveOnError)
					{
						IsInit = false;
					}
					return Data.ToArray();
				}
				uint SizeFrame2 = (uint)(SizeFrame1 + (b2 << 8));
				base.PortReadTimeout = 500;
				for (i = 0; i < SizeFrame2; i++)
				{
					try
					{
						b = await PortReadByteAsync();
					}
					catch (Exception)
					{
						Error = "Ошибка приема кадра сообщения (4)";
						PortLogs.Append(Error);
						if (Global.Settings.SetNotActiveOnError)
						{
							IsInit = false;
						}
						return Data.ToArray();
					}
					bw.Write(b);
				}
				try
				{
					b = await PortReadByteAsync();
				}
				catch (Exception)
				{
					Error = "Ошибка приема кадра сообщения (5)";
					PortLogs.Append(Error);
					if (Global.Settings.SetNotActiveOnError)
					{
						IsInit = false;
					}
					return Data.ToArray();
				}
				uint CRC = b;
				try
				{
					b = await PortReadByteAsync();
				}
				catch (Exception)
				{
					Error = "Ошибка приема кадра сообщения (5)";
					PortLogs.Append(Error);
					if (Global.Settings.SetNotActiveOnError)
					{
						IsInit = false;
					}
					return Data.ToArray();
				}
				CRC += (uint)(b << 8);
				byte[] array = Data.ToArray();
				uint num = 0u;
				for (int j = 0; j < array.Length; j++)
				{
					uint num2 = 0u;
					uint num3 = num >> 8 << 8;
					for (uint num4 = 0u; num4 < 8; num4++)
					{
						num2 = ((((num3 ^ num2) & 0x8000) == 0) ? (num2 << 1) : ((num2 << 1) ^ 0x1021));
						num3 <<= 1;
					}
					num = num2 ^ (num << 8) ^ (uint)(array[j] & 0xFF);
				}
				num &= 0xFFFF;
				if (CRC != num)
				{
					Error = "Не правильная контрольная сумма ответа";
					PortLogs.Append(Error);
				}
				return Data.ToArray();
			}
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
