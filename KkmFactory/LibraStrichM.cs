using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class LibraStrichM(Global.DeviceSettings SettDr, int NumUnit) : UnitPort(SettDr, NumUnit)
{
	public string OperatorPassword = "30";

	private Encoding Win1251 = Encoding.GetEncoding(1251);

	private Encoding e886 = Encoding.GetEncoding(866);

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
			case "OperatorPassword":
				OperatorPassword = unitParamet.Value.Trim();
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
		UnitDownloadURL = "https://www.shtrih-m.ru/";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP: адрес\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"5001\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"4800\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\"\r\n                Description = \"Для 'Протокол 1c'-57600, для 'Протокол STANDART'-19200, остальное 4800\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n        <Group Caption=\"Общие параметры\">\r\n            <Parameter Name=\"OperatorPassword\" Caption=\"Пароль администратора\" TypeValue=\"String\" DefaultValue=\"30\" /> \r\n            <Parameter Name=\"Protocol\" Caption=\"Протокол\" TypeValue=\"Number\" DefaultValue=\"0\">\r\n                <ChoiceList>\r\n                    <Item Value=\"0\">Штрих</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
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
		Error = "";
		NameDevice = "Штрих-М";
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			return false;
		}
		byte[] array = await RunCommand(252u, null, new MemoryStream(), 2000);
		if (!IsCommandBad(null, array, OpenSerial: false, "Весы не подключены!"))
		{
			NameDevice = StringFromStream(array, 8, array.Length - 8);
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		IsInit = true;
		return true;
	}

	public override async Task Calibrate(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		if (UnitParamets["Protocol"].AsInt() == 0)
		{
			await Calibrate1(DataCommand, RezultCommand, Parity.Even);
		}
	}

	public override async Task GetWeight(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		if (UnitParamets["Protocol"].AsInt() == 0)
		{
			await GetWeight1(DataCommand, RezultCommand, Parity.Even);
		}
		await ComDevice.PostCheck(RezultCommand, this);
	}

	public async Task Calibrate1(DataCommand DataCommand, RezultCommandLibra RezultCommand, Parity Parity = Parity.None)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync(Parity);
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		if (IsCommandBad(RezultCommand, await RunCommand(7u, OperatorPassword, new MemoryStream(new byte[1]), 2000), OpenSerial: false, "Не удалось перейти в режим 00!"))
		{
			Error = "";
		}
		if (!IsCommandBad(RezultCommand, await RunCommand(48u, OperatorPassword, new MemoryStream(new byte[0]), 2000), OpenSerial, "Не удалось установить 0") && OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public async Task GetWeight1(DataCommand DataCommand, RezultCommandLibra RezultCommand, Parity Parity = Parity.None)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync(Parity);
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		byte[] array = await RunCommand(7u, OperatorPassword, new MemoryStream(new byte[1]), 2000);
		if (IsCommandBad(RezultCommand, array, OpenSerial: false, "Не удалось перейти в режим 00!"))
		{
			Error = "";
		}
		for (int x = 0; x < 20; x++)
		{
			array = await RunCommand(58u, OperatorPassword, new MemoryStream(new byte[0]), 2000);
			if (IsCommandBad(RezultCommand, array, OpenSerial, "Не удалось получить вес"))
			{
				return;
			}
			if (((int)NumberFromStream(array, 2, 2) & 0x10) > 0)
			{
				break;
			}
			Thread.Sleep(500);
		}
		RezultCommand.Weight = NumberFromStream(array, 4, 4) / 1000m;
		if (RezultCommand.Weight > 4000000m)
		{
			Error = "Не правильная калибровка весов";
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public decimal NumberFromStream(byte[] Data, byte Pos, byte CountByte)
	{
		decimal result = default(decimal);
		decimal num = 1m;
		for (int i = Pos; i < Pos + CountByte; i++)
		{
			result += (decimal)Data[i] * num;
			num *= 256m;
		}
		return result;
	}

	public string StringNumberFromStreamg(byte[] Data, byte Pos, byte CountByte, byte SizeString)
	{
		string text = NumberFromStream(Data, Pos, CountByte).ToString("F0");
		while (text.Length < SizeString)
		{
			text = "0" + text;
		}
		return text;
	}

	public string StringFromStream(byte[] Data, byte Pos, int CountByte = 0, bool NulTerminate = false, bool use866 = false)
	{
		if (CountByte == 0)
		{
			CountByte = Data.Length - Pos;
		}
		string text = null;
		text = ((!use866) ? Win1251.GetString(Data, Pos, CountByte).TrimEnd('\0') : e886.GetString(Data, Pos, CountByte).TrimEnd('\0'));
		if (NulTerminate && text.IndexOf('\0') != -1)
		{
			text = text.Substring(0, text.IndexOf('\0'));
		}
		return text;
	}

	public void NumberToStream(BinaryWriter bw, decimal Number, byte CountByte)
	{
		MemoryStream memoryStream = new MemoryStream();
		new BinaryWriter(memoryStream).Write((long)Number);
		byte[] array = memoryStream.ToArray();
		for (int i = 0; i < CountByte; i++)
		{
			bw.Write(array[i]);
		}
	}

	public void StringToStream(BinaryWriter bw, string Value, int CountByte, byte FillChar = 32, bool use866 = false)
	{
		if (CountByte == 0)
		{
			CountByte = Value.Length;
		}
		if (Value.Length > CountByte)
		{
			Value = Value.Substring(0, CountByte);
		}
		byte[] array = null;
		array = ((!use866) ? Win1251.GetBytes(Value) : e886.GetBytes(Value));
		for (int i = 0; i < Value.Length; i++)
		{
			bw.Write(array[i]);
		}
		for (int j = array.Length; j < CountByte; j++)
		{
			bw.Write(FillChar);
		}
	}

	public async Task<byte[]> RunCommand(uint Command, string Pass, MemoryStream Msg, int TimeOut = 15000)
	{
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			return new byte[0];
		}
		base.PortReadTimeout = TimeOut;
		int iError80 = 0;
		byte[] bData;
		while (true)
		{
			await SendFrame(Command, Pass, Msg);
			if (Error != "")
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				return new byte[0];
			}
			bData = await GetFrame(TimeOut);
			if (Error != "")
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				return new byte[0];
			}
			byte b = 0;
			if (bData.Length >= 2 && bData[0] != byte.MaxValue)
			{
				b = bData[1];
			}
			if (bData.Length >= 3 && bData[0] == byte.MaxValue)
			{
				b = bData[2];
			}
			if (b != 80 || iError80 >= 50)
			{
				break;
			}
			iError80++;
			Thread.Sleep(100);
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		return bData;
	}

	public async Task<bool> SendFrame(uint Command, string Pass, MemoryStream Msg)
	{
		_ = 6;
		try
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(ms);
			int num = 0;
			if (Pass != "" && Pass != null)
			{
				try
				{
					num = int.Parse(Pass);
				}
				catch
				{
					Error = "Не правильно указан пароль!";
					return false;
				}
			}
			Error = "";
			binaryWriter.Write((byte)2);
			if (Command <= 255)
			{
				binaryWriter.Write((byte)(1 + ((Pass != null) ? 4 : 0) + Msg.Length));
				binaryWriter.Write((byte)Command);
			}
			else
			{
				binaryWriter.Write((byte)(2 + ((Pass != null) ? 4 : 0) + Msg.Length));
				binaryWriter.Write((byte)(Command >> 8));
				binaryWriter.Write((byte)(Command & 0xFF));
			}
			if (Pass != null)
			{
				binaryWriter.Write(num);
			}
			binaryWriter.Write(Msg.ToArray());
			byte[] array = ms.ToArray();
			int num2 = 0;
			for (int i = 1; i < array.Length; i++)
			{
				int num3 = array[i];
				num2 ^= num3;
			}
			binaryWriter.Write((byte)num2);
			base.PortWriteTimeout = 500;
			byte[] PrtMsg = new byte[1] { 5 };
			await PortWriteAsync(PrtMsg, 0, 1);
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
				await GetFrame(5000);
				await PortWriteAsync(PrtMsg, 0, 1);
				try
				{
					b = await PortReadByteAsync();
				}
				catch (Exception)
				{
					b = 0;
				}
			}
			if (b != 21)
			{
				Error = "Ошибка команды на открытие сеанса";
				PortLogs.Append(Error);
				return false;
			}
			await PortWriteAsync(ms.ToArray(), 0, (int)ms.Length);
			try
			{
				b = await PortReadByteAsync();
			}
			catch (Exception)
			{
				b = 0;
			}
			if (b != 6)
			{
				Error = "Ошибка передачи кадра сообщения";
				PortLogs.Append(Error);
				return false;
			}
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
		if (b != 2)
		{
			Error = "Ошибка приема кадра сообщения (2)";
			PortLogs.Append(Error);
			if (Global.Settings.SetNotActiveOnError)
			{
				IsInit = false;
			}
			return Data.ToArray();
		}
		byte SizeFrame;
		try
		{
			SizeFrame = await PortReadByteAsync();
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
		for (int i = 0; i < SizeFrame; i++)
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
		byte[] array = Data.ToArray();
		int num = 0 ^ SizeFrame;
		foreach (int num2 in array)
		{
			num ^= num2;
		}
		if ((byte)num == b)
		{
			byte[] buffer = new byte[1] { 6 };
			await PortWriteAsync(buffer, 0, 1);
		}
		else
		{
			Error = "Не правильная контрольная сумма ответа";
			PortLogs.Append(Error);
		}
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			try
			{
				await PortReadByteAsync();
			}
			catch (Exception)
			{
			}
		}
		return Data.ToArray();
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
		_ = SetPort.PortOpen;
		return await base.PortCloseAsync();
	}

	public bool IsCommandBad(RezultCommandLibra RezultCommand, byte[] Buffer, bool OpenSerial, string ErrorText, bool Ignore55 = false)
	{
		if (Error != "")
		{
			if (OpenSerial)
			{
				PortCloseAsync().Wait();
			}
			if (RezultCommand != null)
			{
				if (!IsNotErrorStatus)
				{
					RezultCommand.Status = ExecuteStatus.Error;
				}
				if (ErrorText != "" && ErrorText != null)
				{
					Error = ErrorText + " (" + Error + ")";
				}
			}
		}
		else
		{
			if (Buffer == null)
			{
				return false;
			}
			int num = 1;
			if (Buffer != null && Buffer[0] == byte.MaxValue)
			{
				num = 2;
			}
			if (Buffer == null || Buffer[num] == 0 || (Ignore55 && (Buffer[num] == 55 || Buffer[num] == 115)))
			{
				return false;
			}
			CreateTextError(Buffer[num], ErrorText);
			if (OpenSerial)
			{
				PortCloseAsync().Wait();
			}
			if (RezultCommand != null && !IsNotErrorStatus)
			{
				RezultCommand.Status = ExecuteStatus.Error;
			}
		}
		if (IsNotErrorStatus)
		{
			Warning += Error;
			Error = "";
		}
		return true;
	}

	public bool CreateTextError(byte ErrorByte, string TextError)
	{
		string text = "Неизвестный код ошибки";
		switch (ErrorByte)
		{
		case 0:
			text = "Ошибок нет";
			break;
		case 17:
			text = "Ошибка в значении тары";
			break;
		case 120:
			text = "Неизвестная команда";
			break;
		case 121:
			text = "Неверная длина данных команды";
			break;
		case 122:
			text = "Неверный пароль";
			break;
		case 123:
			text = "Команда не реализуется в данном режиме";
			break;
		case 124:
			text = "Неверное значение параметра";
			break;
		case 150:
			text = "Ошибка при попытке установки нуля";
			break;
		case 151:
			text = "Ошибка при установке тары";
			break;
		case 152:
			text = "Вес не фиксирован";
			break;
		case 166:
			text = "Сбой энергонезависимой памяти";
			break;
		case 167:
			text = "Команда не реализуется интерфейсом";
			break;
		case 170:
			text = "Исчерпан лимит попыток обращения с неверным паролем";
			break;
		case 180:
			text = "Режим градуировки блокирован градуировочным переключателем";
			break;
		case 181:
			text = "Клавиатура заблокирована";
			break;
		case 182:
			text = "Нельзя поменять тип текущего канала";
			break;
		case 183:
			text = "Нельзя выключить текущий канал";
			break;
		case 184:
			text = "С данным каналом ничего нельзя делать";
			break;
		case 185:
			text = "Неверный номер канала";
			break;
		case 186:
			text = "Нет ответа от АЦП";
			break;
		}
		Error = TextError + " ( " + ErrorByte + "-" + text + " )";
		return true;
	}
}
