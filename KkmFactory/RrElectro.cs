using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace KkmFactory;

public class RrElectro : UnitPort
{
	public enum enProtocol
	{
		Null,
		v10,
		v20
	}

	public string AccessPassword = "30";

	public string OperatorPasswor = "30";

	public byte TypeDevice;

	public enProtocol Protocol;

	private bool NewModel;

	private bool OfdStatusFullRead;

	private int NumBlockBarCode;

	private int NumSrtGr;

	private int NumLineCashier;

	private int OldSessionCheckNumber;

	private bool NotCloseCom;

	private string TypeProtocol = "1";

	private bool NotChangeTypeProtocol;

	private bool Send05;

	private bool Get02;

	private Encoding Win1251 = Encoding.GetEncoding(1251);

	private Encoding e886 = Encoding.GetEncoding(866);

	public SortedList<int, byte> Nalogs = new SortedList<int, byte>();

	private string URL = "";

	private bool ClearCheck;

	public RrElectro(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		Kkm.IsKKT = true;
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
	}

	public override void Destroy()
	{
		try
		{
			base.PortCloseAsync().Wait();
		}
		catch
		{
		}
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = (UnitDescription = "Драйвер ККМ для моделей: " + SettDr.TypeDevice.SupportModels);
		UnitEquipmentType = "ФискальныйРегистратор";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kkmserver.ru";
		UnitAdditionallinks = "<a href='https://kkmserver.ru/WiKi/SettingStrihM'>Инструкция по настройке</a><br/>\r\n                            <a href='https://kkmserver.ru/Donload/Shtrih-M_DTO_x32_setup.exe'>Дистрибутив 'Штрих ДТО' для Windows x32</a><br/>\r\n\t\t\t\t\t\t\t<a href='https://knowledge-base.rr-electro.com/pages/viewpage.action?pageId=20938788'>Дистрибутивы 'РР-Электро ДТО'</a><br/>";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\"\r\n                Help=\"Инструкция по настройке: &lt;a id=&quot;help&quot; href=&quot;https://kkmserver.ru/WiKi/SettingStrihM&quot;&gt;https://kkmserver.ru/WiKi/SettingStrihM&lt;/a&gt;\"\r\n                Description=\"ККТ 'Штрих-М' может быть подключен через следующие интерфейсы (типы соединений):\r\n\r\n                            Ehternet/WiFi: сеть (Предпочтительный способ подключения)\r\n                            USB(протокол RNDIS): эмуляция Ehternet интерфейса через USB\r\n                            USB-to-COM: эмуляция СОМ порта через USB\r\n                            COM порт: через СОМ порт ПК\r\n\r\n                            'Ehternet' и 'COM порт' работают всегда \r\n                            Из 'USB(протокол RNDIS)' и 'USB-to-COM' работает кто-то один - возможно программное переключение.\r\n\r\n                            При включении интерфейса 'USB(протокол RNDIS)' необходимо убедится что в ККТ задан постоянный IP, Шлюз, Маска сети и отлючен dhcp.\r\n                            По умолчанию IP = 192.168.137.111, Маска сети = 255.255.255.0, Шлюз = 192.168.137.1\r\n                                            \r\n                            При работе через 'Ehternet/WiFi' рекомендуется включить dhcp.\r\n\r\n                            Инструкция по програмированию различных интерфейсов:\r\n                            &lt;a id=&quot;help&quot; href=&quot;https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf&quot;&gt;https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf&lt;/a&gt; \r\n                            \" >\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" DefaultValue=\"192.168.137.111\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"\r\n                Description=\"В режиме интерфейса 'Ehternet/WiFi' KKТ 'Штрих-М' в отличее от ККТ 'Атол' при включении НЕ! напечатает свой IP адрес.\r\n\r\n                            По умолчанию IP = 192.168.137.111, Маска сети = 255.255.255.0, Шлюз = 192.168.137.1\r\n                            При работе через 'Ehternet/WiFi' рекомендуется включить dhcp.\r\n\r\n                            Инструкция по програмированию различных интерфейсов:\r\n                            &lt;a id=&quot;help&quot; href=&quot;https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf&quot;&gt;https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf&lt;/a&gt; \r\n                            \" />\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"7778\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\"\r\n                Description=\"Если ККТ 'Штрих-М' не работает по интерфейсу 'USB-to-COM':\r\n\r\n                            Из 'USB(протокол RNDIS)' и 'USB-to-COM' работает кто-то один - возможно программное переключение.\r\n\r\n                            Инструкция по програмированию различных интерфейсов:\r\n                            &lt;a id=&quot;help&quot; href=&quot;https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf&quot;&gt;https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf&lt;/a&gt; \r\n                            \" >\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"14400\">14400</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"NotCloseCom\" Caption=\"Не закрывать COM порт\" TypeValue=\"Boolean\" DefaultValue=\"true\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\" />\r\n            <Parameter Name=\"NotChangeTypeProtocol\" Caption=\"Не менять тип протокола\" TypeValue=\"Boolean\" DefaultValue=\"false\" />\r\n        </Group>\r\n        <Group Caption=\"Общие параметры\">\r\n            <Parameter Name=\"AccessPassword\" Caption=\"Пароль доступа\" TypeValue=\"String\" DefaultValue=\"30\" /> \r\n            <Parameter Name=\"OperatorPasswor\" Caption=\"Пароль администратора\" TypeValue=\"String\" DefaultValue=\"30\" /> \r\n            <Parameter Name=\"PrintFiscalQRBarCodr\" Caption=\"Печатать QR код с данными чека\" TypeValue=\"Boolean\" DefaultValue=\"true\" />  \r\n        </Group>\r\n        <Group Caption=\"Настройка связи с ОФД\">\r\n            <Parameter Name=\"ChannelExchangeOFD\" Caption=\"Канал обмена с ОФД\" TypeValue=\"String\" DefaultValue=\"\" \r\n                Help=\"Осторожно!!! Обязательно прочтите справку-'?'\"\r\n                Description=\"ВНИМАНИЕ!\r\n                            В некоторых случаях может сменится порт подключения (COM, USB, Ethernet) ККТ к ПК!!!\r\n\r\n                            Если выбран канал 'Ethernet' или 'WiFi' (Рекомендуется) то нужно убедиться что: \r\n                            1. ККТ подсоединен к ПК через 'Ethernet' или по WiFi\r\n                            2. Убедитесь что в сети к которой подключен ККТ есть доступ в интернет\r\n                            3. В настройке 'COM порт PPP Ehernet Over Usb' укажите 'Ethernet/WiFi'\r\n                            !В ККТ будут запрограммированы следующие настройки:\r\n                            1. Автоматическое получение IP по dhcp будет включено\r\n                            2. Режим 'USB to Ethernet (протокол RNDIS)' будет выключен\r\n\r\n                            Если выбран канал 'USB to Ethernet (протокол RNDIS)' (То-же рекомендуется) то нужно убедиться что:\r\n                            1. ККТ подсоединен к ПК напрямую в 'Ethernet' порт\r\n                            2. Порт 'Ethernet' ПК (не ККТ!) к которому подключен ККТ должен быть запрограмирован в IP = 192.168.137.1, Маска сети = 255.255.255.0\r\n                            3. Разрешить этому порту 'Ethernet' ПК доступ в интернет\r\n                            4. В настройке 'COM порт PPP Ehernet Over Usb' укажите 'Ethernet/WiFi'\r\n                            !В ККТ будут запрограммированы следующие настройки:\r\n                            1. Автоматическое получение IP по dhcp будет выключено\r\n                            2. Режим 'USB to Ethernet (протокол RNDIS)' будет включен\r\n                            3. В ККТ будет установлен статический IP 192.168.137.111, маска сети 255.255.255.0, шлюз 192.168.137.1\r\n\r\n                            Если выбран канал 'WiFi' то нужно убедиться что:\r\n                            1. В ККТ предварительно было настроено WiFi соединение\r\n                            -В ККТ будут запрограммированы следующие настройки:\r\n                            1. Автоматическое получение IP по dhcp будет включено\r\n                            2. Режим 'USB to Ethernet (протокол RNDIS)' будет выключен\r\n\r\n                            Если выбран канал 'PPP Ethernet Over USB.':\r\n                            1. ККТ подсоединен к ПК через USB (для передачи команд ККТ)\r\n                            2. ККТ подсоединен к ПК так же через физический СОМ порт (для протокола PPP Ehernet Over Usb)\r\n                            3. В настройке 'COM порт PPP Ehernet Over Usb' надо указать физический COM порт для запуска службы 'PPP Ethernet Over Usb'\r\n                            !В ККТ будут запрограммированы следующие настройки:\r\n                            1. Режим 'USB to Ethernet (протокол RNDIS)' будет выключен\r\n\r\n                            После записи настроек необходимо черз 30 сек. выключить а затем включить ККТ\r\n                            Осторожно! После смены канала ККТ может стать не доступен по некоторым портам (исходя из логики настройки)\r\n                            \">\r\n                <ChoiceList>\r\n                    <Item Value=\"\">Настройка из ККТ</Item>\r\n                    <Item Value=\"2\">Ethernet</Item>\r\n                    <Item Value=\"1\">USB to Ethernet (протокол RNDIS)</Item>\r\n                    <Item Value=\"3\">PPP Ethernet Over USB.</Item>\r\n                    <Item Value=\"4\">WiFi</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		LoadParametsFromXML(text);
		string paramsXML = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                <Actions>\r\n                    <Action Name=\"RestartKKT\" Caption=\"Перезагрузить ККТ\"/> \r\n                </Actions>";
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
			case "AccessPassword":
				AccessPassword = unitParamet.Value.Trim();
				break;
			case "OperatorPasswor":
				OperatorPasswor = unitParamet.Value.Trim();
				break;
			case "NotCloseCom":
				NotCloseCom = unitParamet.Value.AsBool();
				break;
			case "NotChangeTypeProtocol":
				NotChangeTypeProtocol = unitParamet.Value.AsBool();
				break;
			}
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		TypeProtocol = "1";
		await base.InitDevice(FullInit, Program);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			return false;
		}
		if (FullInit)
		{
			Protocol = enProtocol.Null;
		}
		if (Protocol == enProtocol.Null)
		{
			await RunCommand(16u, OperatorPasswor, new MemoryStream(), 200);
			if (!IsCommandBad(null, null, OpenSerial: false, ClearCheck: false, "ККМ не подключена!"))
			{
				Protocol = enProtocol.v10;
				Error = "";
			}
			else
			{
				Protocol = enProtocol.v20;
			}
		}
		await TerminateStausOutDate();
		byte[] array = await RunCommand(16u, OperatorPasswor, new MemoryStream(), 5000);
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			LastError = Error;
			return false;
		}
		Kkm.PaperOver = (array[3] & 0x80) == 0;
		switch (array[5] & 0xF)
		{
		case 2:
			SessionOpen = 2;
			break;
		case 3:
			SessionOpen = 3;
			break;
		default:
			SessionOpen = 1;
			break;
		}
		await ClearOldCheck();
		array = await RunCommand(252u, null, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "ККМ не подключена!"))
		{
			if (IsInit)
			{
				IsInitDate = DateTime.Now;
				IsInit = false;
			}
			Error = "ККМ не подключена! (" + Error + ")";
			return false;
		}
		TypeDevice = array[3];
		IdModel = array[6];
		NameDevice = Win1251.GetString(array, 8, array.Length - 8);
		_ = (byte[])(await GetValueInTable(18, 1, 2, typeof(byte[])));
		if (!IsCommandBad(null, null, OpenSerial: false, ClearCheck: false, ""))
		{
			NewModel = true;
		}
		else
		{
			NewModel = false;
		}
		Error = "";
		Kkm.IsKKT = NewModel;
		MemoryStream memoryStream = new MemoryStream();
		new BinaryWriter(memoryStream).Write((byte)1);
		array = await RunCommand(38u, OperatorPasswor, memoryStream);
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить данные шрифтов"))
		{
			LastError = Error;
			return false;
		}
		Kkm.PrintingWidth = ((array[3] << 8) + array[2]) / array[4];
		for (int ii = 30; ii > 0; ii--)
		{
			if (((int)(await GetValueInTable(2, ii, 1, typeof(int)))).ToString() == AccessPassword)
			{
				NumLineCashier = ii;
				break;
			}
		}
		if (!FullInit && IsInit)
		{
			if (IdModel != 0 && IdModel != 255)
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				return true;
			}
			if (IdModel == 255)
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				return false;
			}
		}
		Kkm.INN = "";
		if (NewModel)
		{
			SetKkm kkm = Kkm;
			kkm.INN = (string)(await GetValueInTable(18, 1, 2, typeof(string)));
			if (IsCommandBad(null, null, OpenSerial, ClearCheck: false, "Не удалось получить данные ККМ"))
			{
				LastError = Error;
				return false;
			}
		}
		if (Kkm.INN == null || Kkm.INN.Length < 5 || Kkm.INN == "000000000000")
		{
			array = await RunCommand(17u, OperatorPasswor, new MemoryStream(), 500);
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить данные ККМ"))
			{
				LastError = Error;
				return false;
			}
			Kkm.INN = StringNumberFromStreamg(array, 42, 6, 12);
		}
		if (Kkm.INN == null || Kkm.INN == (281474976710655L.ToString() ?? ""))
		{
			Kkm.INN = "";
		}
		if (Kkm.INN != null)
		{
			Kkm.INN = Kkm.INN.Trim();
		}
		if (Kkm.INN.Length == 12 && Kkm.INN.Substring(0, 2) == "00")
		{
			Kkm.INN = Kkm.INN.Substring(2);
		}
		Kkm.Organization = "<Не определено>";
		Kkm.NumberKkm = "";
		if (NewModel)
		{
			SetKkm kkm = Kkm;
			kkm.Organization = (string)(await GetValueInTable(18, 1, 7, typeof(string)));
			if (IsCommandBad(null, null, OpenSerial, ClearCheck: false, "Не удалось получить наименование организации"))
			{
				LastError = Error;
				return false;
			}
			kkm = Kkm;
			kkm.NumberKkm = (string)(await GetValueInTable(18, 1, 1, typeof(string)));
			if (IsCommandBad(null, null, OpenSerial, ClearCheck: false, "Не удалось получить номер ККМ"))
			{
				LastError = Error;
				return false;
			}
		}
		if (Kkm.Organization != null)
		{
			Kkm.Organization = Kkm.Organization.Trim();
		}
		Nalogs.Clear();
		Nalogs.Add(22, 1);
		Nalogs.Add(20, 1);
		Nalogs.Add(10, 2);
		Nalogs.Add(0, 4);
		Nalogs.Add(-1, 8);
		Nalogs.Add(122, 16);
		Nalogs.Add(120, 16);
		Nalogs.Add(110, 32);
		Nalogs.Add(5, 129);
		Nalogs.Add(7, 130);
		Nalogs.Add(105, 132);
		Nalogs.Add(107, 136);
		if (NewModel)
		{
			await ReadStatusOFD(FullInit);
		}
		if (Global.Settings.SetNotActiveOnPaperOver)
		{
			IsInit = !Kkm.PaperOver;
		}
		else
		{
			IsInit = true;
		}
		if (Program)
		{
			if (NewModel)
			{
				await SetValueInTable(17, 1, 3, (byte)2);
				await SetValueInTable(17, 1, 14, (byte)0);
				if (UnitParamets["ChannelExchangeOFD"] != null && UnitParamets["ChannelExchangeOFD"] != "")
				{
					await SetValueInTable(21, 1, 2, (byte)1);
					switch (UnitParamets["ChannelExchangeOFD"])
					{
					case "1":
					{
						byte StaticIp = (byte)(int)(await GetValueInTable(16, 1, 1, typeof(byte)));
						await SetValueInTable(21, 1, 1, (byte)0);
						await SetValueInTable(16, 1, 1, (byte)1);
						if (StaticIp == 0)
						{
							await SetValueInTable(16, 1, 3, (byte)192);
							await SetValueInTable(16, 1, 4, (byte)168);
							await SetValueInTable(16, 1, 5, (byte)137);
							await SetValueInTable(16, 1, 6, (byte)111);
							await SetValueInTable(16, 1, 7, (byte)192);
							await SetValueInTable(16, 1, 8, (byte)168);
							await SetValueInTable(16, 1, 9, (byte)137);
							await SetValueInTable(16, 1, 10, (byte)1);
							await SetValueInTable(16, 1, 11, byte.MaxValue);
							await SetValueInTable(16, 1, 12, byte.MaxValue);
							await SetValueInTable(16, 1, 13, byte.MaxValue);
							await SetValueInTable(16, 1, 14, (byte)0);
							await SetValueInTable(16, 1, 15, (byte)192);
							await SetValueInTable(16, 1, 16, (byte)168);
							await SetValueInTable(16, 1, 17, (byte)137);
							await SetValueInTable(16, 1, 18, (byte)1);
						}
						await SetValueInTable(21, 1, 9, (byte)1);
						break;
					}
					case "2":
						await SetValueInTable(21, 1, 1, (byte)0);
						await SetValueInTable(16, 1, 1, (byte)0);
						await SetValueInTable(21, 1, 9, (byte)0);
						break;
					case "3":
						await SetValueInTable(21, 1, 1, (byte)1);
						await SetValueInTable(16, 1, 1, (byte)0);
						await SetValueInTable(21, 1, 9, (byte)0);
						break;
					case "4":
						await SetValueInTable(21, 1, 1, (byte)0);
						await SetValueInTable(16, 1, 1, (byte)0);
						await SetValueInTable(21, 1, 9, (byte)0);
						break;
					}
				}
				await SetValueInTable(1, 1, 41, UnitParamets["PrintFiscalQRBarCodr"].AsBool() ? ((byte)1) : ((byte)0));
			}
			byte b = 0;
			if (UnitParamets.ContainsKey("PaymentCashOnClouseShift") && UnitParamets["PaymentCashOnClouseShift"] == "Zreport")
			{
				b = 1;
			}
			await SetValueInTable(1, 1, 2, b);
			Error = "";
			await SetValueInTable(17, 1, 36, (byte)1);
			Error = "";
			await SetValueInTable(1, 1, 14, (byte)0);
		}
		IsFullInitDate = DateTime.Now;
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		return true;
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task RegisterCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		byte[] bData = null;
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await CloseDocumentAndOpenShift(DataCommand, RezultCommand);
		await SetNotPrint(DataCommand);
		if (DataCommand.IsFiscalCheck)
		{
			await SerCashier(DataCommand, InTable: true);
		}
		string NumberCheck = "0";
		if (DataCommand.IsFiscalCheck)
		{
			await GetCheckAndSession(RezultCommand);
			NumberCheck = "0";
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			if (NewModel)
			{
				binaryWriter.Write((byte)152);
			}
			else if (DataCommand.TypeCheck == 0)
			{
				binaryWriter.Write((byte)148);
			}
			else if (DataCommand.TypeCheck == 1)
			{
				binaryWriter.Write((byte)150);
			}
			else if (DataCommand.TypeCheck == 10)
			{
				binaryWriter.Write((byte)149);
			}
			else if (DataCommand.TypeCheck == 11)
			{
				binaryWriter.Write((byte)151);
			}
			if (memoryStream.Length > 0)
			{
				bData = await RunCommand(27u, OperatorPasswor, memoryStream);
				if (!IsCommandBad(RezultCommand, bData, OpenSerial, ClearCheck: false, "Не удалось получить данные чека"))
				{
					NumberCheck = StringNumberFromStreamg(bData, 3, 2, 4);
					int.Parse(NumberCheck);
					NumberCheck = NumberCheck.ToString();
				}
				if (!(await WaitPrint(OpenSerial, RezultCommand)))
				{
					return;
				}
			}
		}
		if (DataCommand.IsFiscalCheck && (DataCommand.TaxVariant == "" || DataCommand.TaxVariant == null))
		{
			string[] array = Kkm.TaxVariant.Split(',');
			DataCommand.TaxVariant = array[0];
		}
		byte TaxVariant = 0;
		if (DataCommand.TaxVariant != "" && DataCommand.TaxVariant != null)
		{
			TaxVariant = (byte)(1 << (int)byte.Parse(DataCommand.TaxVariant));
		}
		PortLogs.Append("Открытие чека", "-");
		int NumOpen = 0;
		bool IsCheckCorrection;
		int CheckType;
		while (true)
		{
			IsCheckCorrection = false;
			await SetNotPrint(DataCommand);
			CheckType = 0;
			if (!DataCommand.IsFiscalCheck)
			{
				break;
			}
			for (int i = 0; i < 20; i++)
			{
				MemoryStream memoryStream = new MemoryStream();
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				if (DataCommand.TypeCheck == 0)
				{
					binaryWriter.Write((byte)0);
					CheckType = 1;
				}
				else if (DataCommand.TypeCheck == 1)
				{
					binaryWriter.Write((byte)2);
					CheckType = 2;
				}
				else if (DataCommand.TypeCheck == 2)
				{
					if (!NewModel)
					{
						Error = "Команда не поддерживается оборудованием";
						return;
					}
					IsCheckCorrection = true;
					if (Kkm.FfdVersion >= 3)
					{
						binaryWriter.Write((byte)128);
					}
					CheckType = 1;
				}
				else if (DataCommand.TypeCheck == 3)
				{
					if (!NewModel)
					{
						Error = "Команда не поддерживается оборудованием";
						return;
					}
					IsCheckCorrection = true;
					if (Kkm.FfdVersion >= 3)
					{
						binaryWriter.Write((byte)130);
					}
					CheckType = 2;
				}
				else if (DataCommand.TypeCheck == 10)
				{
					binaryWriter.Write((byte)1);
					CheckType = 3;
				}
				else if (DataCommand.TypeCheck == 11)
				{
					binaryWriter.Write((byte)3);
					CheckType = 4;
				}
				else if (DataCommand.TypeCheck == 12)
				{
					if (!NewModel)
					{
						Error = "Команда не поддерживается оборудованием";
						return;
					}
					IsCheckCorrection = true;
					if (Kkm.FfdVersion >= 3)
					{
						binaryWriter.Write((byte)129);
					}
					CheckType = 3;
				}
				else
				{
					if (DataCommand.TypeCheck != 13)
					{
						Error = "Команда не поддерживается оборудованием";
						if (OpenSerial)
						{
							await PortCloseAsync();
						}
						return;
					}
					if (!NewModel)
					{
						Error = "Команда не поддерживается оборудованием";
						return;
					}
					IsCheckCorrection = true;
					if (Kkm.FfdVersion >= 3)
					{
						binaryWriter.Write((byte)131);
					}
					CheckType = 4;
				}
				if (!IsCheckCorrection || Kkm.FfdVersion >= 3)
				{
					bData = await RunCommand(141u, OperatorPasswor, memoryStream);
					if (bData[1] != 80)
					{
						break;
					}
				}
				else
				{
					bData = await RunCommand(65333u, AccessPassword, memoryStream);
					if (bData[1] != 80)
					{
						break;
					}
				}
				await Task.Delay(200);
			}
			if (!IsCommandBad(RezultCommand, bData, OpenSerial, ClearCheck: false, "Не удалось открыть регистрацию чека"))
			{
				break;
			}
			if (NumOpen == 1)
			{
				return;
			}
			Error = "";
			NumOpen++;
			bData = await RunCommand(136u, OperatorPasswor, new MemoryStream());
			Error = "";
		}
		if (DataCommand.IsFiscalCheck)
		{
			await SerCashier(DataCommand);
		}
		if (DataCommand.IsFiscalCheck)
		{
			if (NewModel && DataCommand.ClientAddress != null && DataCommand.ClientAddress != "")
			{
				await WriteProp(DataCommand.NotPrint == false, 1008, DataCommand.ClientAddress, 64);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, ""))
				{
					return;
				}
			}
			if (NewModel && !IsCheckCorrection && DataCommand.SenderEmail != null && DataCommand.SenderEmail != "")
			{
				await WriteProp(DataCommand.NotPrint == false, 1117, DataCommand.SenderEmail);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, ""))
				{
					return;
				}
			}
			if (DataCommand.AddressSettle != null && DataCommand.AddressSettle != "")
			{
				await WriteProp(DataCommand.NotPrint == false, 1009, DataCommand.AddressSettle);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
				{
					Error = "";
				}
			}
			if (DataCommand.PlaceMarket != null && DataCommand.PlaceMarket != "")
			{
				await WriteProp(DataCommand.NotPrint == false, 1187, DataCommand.PlaceMarket);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
				{
					Error = "";
				}
			}
		}
		if (DataCommand.AgentSign.HasValue)
		{
			byte b = (byte)(1 << DataCommand.AgentSign).Value;
			await WriteProp(DataCommand.NotPrint == false, 1057, b);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле AgentSign тег 1057"))
			{
				return;
			}
		}
		if (DataCommand.AgentData != null)
		{
			if (!string.IsNullOrEmpty(DataCommand.AgentData.PayingAgentOperation))
			{
				await WriteProp(DataCommand.NotPrint == false, 1044, DataCommand.AgentData.PayingAgentOperation);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле PayingAgentOperation тег 1224"))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.PayingAgentPhone))
			{
				await WriteProp(DataCommand.NotPrint == false, 1073, DataCommand.AgentData.PayingAgentPhone);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле PayingAgentPhone тег 1073"))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.ReceivePaymentsOperatorPhone))
			{
				await WriteProp(DataCommand.NotPrint == false, 1074, DataCommand.AgentData.ReceivePaymentsOperatorPhone);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле ReceivePaymentsOperatorPhone тег 1074"))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorPhone))
			{
				await WriteProp(DataCommand.NotPrint == false, 1075, DataCommand.AgentData.MoneyTransferOperatorPhone);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле MoneyTransferOperatorPhone тег 1075"))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorName))
			{
				await WriteProp(DataCommand.NotPrint == false, 1026, DataCommand.AgentData.MoneyTransferOperatorName);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле MoneyTransferOperatorName тег 1026"))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorAddress))
			{
				await WriteProp(DataCommand.NotPrint == false, 1005, DataCommand.AgentData.MoneyTransferOperatorAddress);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле MoneyTransferOperatorAddress тег 1005"))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorVATIN))
			{
				await WriteProp(DataCommand.NotPrint == false, 1016, DataCommand.AgentData.MoneyTransferOperatorVATIN.PadRight(12, ' '));
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле MoneyTransferOperatorVATIN тег 1016"))
				{
					return;
				}
			}
		}
		if (DataCommand.PurveyorData != null && !string.IsNullOrEmpty(DataCommand.PurveyorData.PurveyorPhone))
		{
			await WriteProp(DataCommand.NotPrint == false, 1171, DataCommand.PurveyorData.PurveyorPhone);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить поле PurveyorPhone тег 1171"))
			{
				return;
			}
		}
		if (!IsCheckCorrection && Kkm.FfdVersion <= 1)
		{
			DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
			foreach (DataCommand.CheckString PrintString in checkStrings)
			{
				if (DataCommand.IsFiscalCheck && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m && PrintString.Register.AgentSign.HasValue && !DataCommand.AgentSign.HasValue)
				{
					byte b2 = (byte)(1 << PrintString.Register.AgentSign).Value;
					await WriteProp(DataCommand.NotPrint == false, 1057, b2);
					if (!IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "") && await SetAgentData(PrintString.Register.AgentData, OpenSerial) && await SetPurveyorData(PrintString.Register.PurveyorData, OpenSerial, Head: true))
					{
						break;
					}
					return;
				}
			}
		}
		if (DataCommand.AdditionalAttribute != null && DataCommand.AdditionalAttribute != "")
		{
			await WriteProp(DataCommand.NotPrint == false, 1192, DataCommand.AdditionalAttribute);
			if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
			{
				Warning = Warning + "Не поддерживается передача поля AdditionalAttribute; " + Error;
				Error = "";
			}
		}
		if (DataCommand.UserAttribute != null && !string.IsNullOrEmpty(DataCommand.UserAttribute.Name) && !string.IsNullOrEmpty(DataCommand.UserAttribute.Value))
		{
			Dictionary<int, object> dictionary = new Dictionary<int, object>();
			if (!string.IsNullOrEmpty(DataCommand.UserAttribute.Name))
			{
				dictionary.Add(1085, DataCommand.UserAttribute.Name);
			}
			if (!string.IsNullOrEmpty(DataCommand.UserAttribute.Value))
			{
				dictionary.Add(1086, DataCommand.UserAttribute.Value);
			}
			await WriteArrProp(DataCommand.NotPrint == false, 1084, dictionary);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось записать дополнительный реквизит пользователя"))
			{
				return;
			}
		}
		if (Kkm.FfdVersion <= 3)
		{
			if (DataCommand.ClientInfo != null && DataCommand.ClientInfo != "")
			{
				await WriteProp(DataCommand.NotPrint == false, 1227, DataCommand.ClientInfo);
				if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
				{
					Warning = Warning + "Не поддерживается передача поля ClientInfo; " + Error;
					Error = "";
				}
			}
			if (DataCommand.ClientINN != null && DataCommand.ClientINN != "")
			{
				await WriteProp(DataCommand.NotPrint == false, 1228, DataCommand.ClientINN.PadRight(12, ' '));
				if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
				{
					Warning = Warning + "Не поддерживается передача поля ClientINN; " + Error;
					Error = "";
				}
			}
		}
		if (Kkm.FfdVersion >= 4 && ((DataCommand.ClientInfo != null && DataCommand.ClientInfo != "") || (DataCommand.ClientINN != null && DataCommand.ClientINN != "")))
		{
			Dictionary<int, object> dictionary2 = new Dictionary<int, object>();
			if (DataCommand.ClientInfo != null && DataCommand.ClientInfo != "")
			{
				dictionary2.Add(1227, DataCommand.ClientInfo);
			}
			if (DataCommand.ClientINN != null && DataCommand.ClientINN != "")
			{
				dictionary2.Add(1228, DataCommand.ClientINN.PadRight(12, ' '));
			}
			await WriteArrProp(DataCommand.NotPrint == false, 1256, dictionary2);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Не удалось записать дополнительный реквизит пользователя"))
			{
				Warning = Warning + "Не поддерживается передача поля ClientInfo и ClientINN; " + Error;
				Error = "";
			}
		}
		if (NewModel && !IsCheckCorrection && DataCommand.IsFiscalCheck && DataCommand.CheckProps != null)
		{
			DataCommand.CheckProp[] checkProps = DataCommand.CheckProps;
			foreach (DataCommand.CheckProp Prop in checkProps)
			{
				if (Prop.PrintInHeader)
				{
					await WriteProp(Prop.Print && !DataCommand.NotPrint.Value, Prop.Teg, (string)Prop.Prop);
					if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Ошибка передачи тега " + Prop.Teg + ": "))
					{
						return;
					}
				}
			}
		}
		if (NewModel && !IsCheckCorrection && DataCommand.IsFiscalCheck && DataCommand.AdditionalProps != null)
		{
			DataCommand.AdditionalProp[] additionalProps = DataCommand.AdditionalProps;
			foreach (DataCommand.AdditionalProp additionalProp in additionalProps)
			{
				if (additionalProp.PrintInHeader)
				{
					await WriteAdditionaProp(additionalProp.Print && !DataCommand.NotPrint.Value, additionalProp.NameProp, additionalProp.Prop);
					if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Ошибка передачи произвольных полей "))
					{
						return;
					}
				}
			}
		}
		if (Kkm.FfdVersion >= 4)
		{
			bool flag = (DataCommand.InternetMode.HasValue ? DataCommand.InternetMode.Value : Kkm.InternetMode);
			await WriteProp(DataCommand.NotPrint == false, 1125, flag ? ((byte)1) : ((byte)0));
			if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
			{
				Warning = Warning + "Не поддерживается передача поля InternetMode (1125); " + Error;
				Error = "";
			}
		}
		await ComDevice.PostCheck(DataCommand, this);
		decimal AllSumm = default(decimal);
		DataCommand.CheckString[] checkStrings2 = DataCommand.CheckStrings;
		foreach (DataCommand.CheckString PrintString in checkStrings2)
		{
			if (DataCommand.NotPrint == false && PrintString != null && PrintString.PrintImage != null)
			{
				PortLogs.Append("Печать картинки", "-");
				await PrintImage(PrintString.PrintImage);
			}
			if (DataCommand.NotPrint == false && PrintString != null && PrintString.PrintText != null)
			{
				int CurWidth = Kkm.PrintingWidth;
				if (PrintString.PrintText.Font == 1 || PrintString.PrintText.Font == 2)
				{
					CurWidth /= 2;
				}
				int Font = PrintString.PrintText.Font switch
				{
					1 => 1, 
					2 => 3, 
					3 => 2, 
					4 => 4, 
					_ => PrintString.PrintText.Font, 
				};
				string text = PrintString.PrintText.Text;
				text = Unit.GetPringString(text, CurWidth);
				string OstText;
				do
				{
					if (text.Length > CurWidth)
					{
						OstText = text.Substring(CurWidth);
						text = text.Substring(0, CurWidth);
					}
					else
					{
						OstText = "";
					}
					MemoryStream memoryStream = new MemoryStream();
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write((byte)2);
					binaryWriter.Write((byte)(Font + 1));
					StringToStream(binaryWriter, text, (Kkm.PrintingWidth <= 40) ? 40 : Kkm.PrintingWidth, 32);
					if (IsCommandBad(RezultCommand, await RunCommand(47u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать не фискальную строку"))
					{
						return;
					}
					text = OstText;
				}
				while (OstText != "");
			}
			if (DataCommand.IsFiscalCheck && NewModel && Kkm.FfdVersion >= 2 && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m)
			{
				if (IsCheckCorrection && Kkm.FfdVersion < 3)
				{
					continue;
				}
				PortLogs.Append("Регистрация фискальной строки", "-");
				List<DataCommand.Register> list = SplitRegisterString(PrintString);
				foreach (DataCommand.Register SplitStr in list)
				{
					AllSumm += SplitStr.Amount;
					MemoryStream memoryStream = new MemoryStream();
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					if (DataCommand.TypeCheck == 0)
					{
						binaryWriter.Write((byte)1);
					}
					else if (DataCommand.TypeCheck == 1)
					{
						binaryWriter.Write((byte)2);
					}
					else if (DataCommand.TypeCheck == 10)
					{
						binaryWriter.Write((byte)3);
					}
					else if (DataCommand.TypeCheck == 11)
					{
						binaryWriter.Write((byte)4);
					}
					else if (DataCommand.TypeCheck == 2)
					{
						binaryWriter.Write((byte)1);
					}
					else if (DataCommand.TypeCheck == 3)
					{
						binaryWriter.Write((byte)2);
					}
					else if (DataCommand.TypeCheck == 12)
					{
						binaryWriter.Write((byte)3);
					}
					else if (DataCommand.TypeCheck == 13)
					{
						binaryWriter.Write((byte)4);
					}
					NumberToStream(binaryWriter, (ulong)(SplitStr.Quantity * 1000000m), 6);
					NumberToStream(binaryWriter, (ulong)(SplitStr.Price * 100m), 5);
					NumberToStream(binaryWriter, (ulong)(SplitStr.Amount * 100m), 5);
					NumberToStream(binaryWriter, 0m, 5);
					if (Nalogs.ContainsKey((int)PrintString.Register.Tax))
					{
						binaryWriter.Write(Nalogs[(int)PrintString.Register.Tax]);
					}
					else
					{
						Error = $"Ставка налога \"{PrintString.Register.Tax}\" не запрограммирована в ККМ";
					}
					binaryWriter.Write((byte)PrintString.Register.Department);
					binaryWriter.Write((byte)PrintString.Register.SignMethodCalculation.Value);
					binaryWriter.Write((byte)PrintString.Register.SignCalculationObject.Value);
					StringToStream(binaryWriter, PrintString.Register.Name, Math.Min(128, PrintString.Register.Name.Length), 0);
					if (IsCommandBad(RezultCommand, await RunCommand(65350u, AccessPassword, memoryStream), OpenSerial, ClearCheck: true, "Не удалось зарегистрировать фискальную строку"))
					{
						return;
					}
					if (Kkm.FfdVersion >= 2)
					{
						await WriteProp(DataCommand.NotPrint == false, 2108, (byte)PrintString.Register.MeasureOfQuantity.Value, 0, -1, InFicalString: true);
						if (Error != "")
						{
							Error = "";
							Warning += "Не поддерживается передача поля MeasureOfQuantity; ";
						}
					}
					if (Kkm.FfdVersion >= 4 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.TryBarCode))
					{
						if (PrintString.Register.PackageQuantity.HasValue)
						{
							await WriteProp(DataCommand.NotPrint == false, 1293, (uint)Math.Truncate(PrintString.Register.Quantity), 0, -1, InFicalString: true);
							if (Error != "")
							{
								Error = "";
							}
							await WriteProp(DataCommand.NotPrint == false, 1294, PrintString.Register.PackageQuantity.Value, 0, -1, InFicalString: true);
							if (Error != "")
							{
								Error = "";
							}
						}
						string text2 = PrintString.Register.GoodCodeData.TryBarCode;
						if (text2.Substring(text2.Length - 1, 1) == "\u001d")
						{
							text2 = text2.Substring(0, text2.Length - 1);
						}
						memoryStream = new MemoryStream();
						binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write((byte)text2.Length);
						StringToStream(binaryWriter, text2, text2.Length, 32);
						if (IsCommandBad(RezultCommand, await RunCommand(65383u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: false, "Ошибка записи тега 1163 (код маркировки)"))
						{
							return;
						}
					}
					else if (Kkm.FfdVersion <= 3 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.MarkingCodeBase64))
					{
						await WriteProp(DataCommand.NotPrint == false, 1162, Convert.FromBase64String(PrintString.Register.GoodCodeData.MarkingCodeBase64), 0, -1, InFicalString: true);
						if (Error != "")
						{
							Error = "Ошибка записи тега 1162 (код маркировки): " + Error;
							if (OpenSerial)
							{
								await PortCloseAsync();
							}
							return;
						}
					}
					if (Kkm.FfdVersion >= 4 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.IndustryProps))
					{
						await WriteProp(DataCommand.NotPrint == false, 1262, PrintString.Register.GoodCodeData.Props1262, 0, -1, InFicalString: true);
						await WriteProp(DataCommand.NotPrint == false, 1263, PrintString.Register.GoodCodeData.Props1263, 0, -1, InFicalString: true);
						await WriteProp(DataCommand.NotPrint == false, 1264, PrintString.Register.GoodCodeData.Props1264, 0, -1, InFicalString: true);
						await WriteProp(DataCommand.NotPrint == false, 1265, PrintString.Register.GoodCodeData.IndustryProps, 0, -1, InFicalString: true);
						if (Error != "")
						{
							Warning += "Не поддерживается передача отраслевого реквизита - тег 1260 ";
							Error = "";
						}
					}
					if (PrintString.Register.CountryOfOrigin != null && PrintString.Register.CountryOfOrigin != "")
					{
						await WriteProp(DataCommand.NotPrint == false, 1230, PrintString.Register.CountryOfOrigin, 0, -1, InFicalString: true);
						if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
						{
							Warning += "Не поддерживается передача поля CountryOfOrigin; ";
							Error = "";
						}
					}
					if (PrintString.Register.CustomsDeclaration != null && PrintString.Register.CustomsDeclaration != "")
					{
						await WriteProp(DataCommand.NotPrint == false, 1231, PrintString.Register.CustomsDeclaration, 0, -1, InFicalString: true);
						if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
						{
							Warning += "Не поддерживается передача поля CustomsDeclaration; ";
							Error = "";
						}
					}
					if (PrintString.Register.ExciseAmount.HasValue)
					{
						await WriteProp(DataCommand.NotPrint == false, 1229, (int)PrintString.Register.ExciseAmount.Value * 100, 6, -1, InFicalString: true);
						if (IsCommandBad(RezultCommand, null, OpenSerial: false, ClearCheck: false, ""))
						{
							Warning += "Не поддерживается передача поля ExciseAmount; ";
							Error = "";
						}
					}
					if (!string.IsNullOrEmpty(PrintString.Register.AdditionalAttribute))
					{
						await WriteProp(DataCommand.NotPrint == false, 1191, PrintString.Register.AdditionalAttribute, 0, -1, InFicalString: true);
						if (Error != "")
						{
							Warning += "Не поддерживается передача поля AdditionalAttribute для предмета расчета; ";
							Error = "";
						}
					}
					if (PrintString.Register.AgentSign.HasValue)
					{
						byte b3 = (byte)(1 << PrintString.Register.AgentSign).Value;
						await WriteProp(DataCommand.NotPrint == false, 1222, b3, 0, -1, InFicalString: true);
						if (Error != "")
						{
							Warning += "Не поддерживается передача поля AgentSign для предмета расчета; ";
							Error = "";
						}
					}
					if (PrintString.Register.AgentData != null)
					{
						Dictionary<int, object> dictionary3 = new Dictionary<int, object>();
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.PayingAgentOperation))
						{
							dictionary3.Add(1044, PrintString.Register.AgentData.PayingAgentOperation);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.PayingAgentPhone))
						{
							dictionary3.Add(1073, PrintString.Register.AgentData.PayingAgentPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.ReceivePaymentsOperatorPhone))
						{
							dictionary3.Add(1074, PrintString.Register.AgentData.ReceivePaymentsOperatorPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorPhone))
						{
							dictionary3.Add(1075, PrintString.Register.AgentData.MoneyTransferOperatorPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorName))
						{
							dictionary3.Add(1026, PrintString.Register.AgentData.MoneyTransferOperatorName);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorAddress))
						{
							dictionary3.Add(1005, PrintString.Register.AgentData.MoneyTransferOperatorAddress);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorVATIN))
						{
							dictionary3.Add(1016, PrintString.Register.AgentData.MoneyTransferOperatorVATIN.PadRight(12, ' '));
						}
						await WriteArrProp(DataCommand.NotPrint == false, 1223, dictionary3, 0, InFicalString: true);
						if (Error != "")
						{
							Warning += "Не поддерживается передача поля AgentData для предмета расчета; ";
							Error = "";
						}
					}
					if (PrintString.Register.PurveyorData != null)
					{
						Dictionary<int, object> dictionary4 = new Dictionary<int, object>();
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorPhone))
						{
							dictionary4.Add(1171, PrintString.Register.PurveyorData.PurveyorPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorName))
						{
							dictionary4.Add(1225, PrintString.Register.PurveyorData.PurveyorName);
						}
						await WriteArrProp(DataCommand.NotPrint == false, 1224, dictionary4, 0, InFicalString: true);
						if (Error != "")
						{
							Warning += "Не поддерживается передача поля PurveyorData для предмета расчета; ";
							Error = "";
						}
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorVATIN))
						{
							await WriteProp(DataCommand.NotPrint == false, 1226, PrintString.Register.PurveyorData.PurveyorVATIN.PadRight(12, ' '), 0, -1, InFicalString: true);
							if (Error != "")
							{
								Warning += "Не поддерживается передача поля PurveyorVATIN для предмета расчета; ";
								Error = "";
							}
						}
					}
					if (SplitStr.StSkidka != "")
					{
						memoryStream = new MemoryStream();
						binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write((byte)2);
						binaryWriter.Write((byte)1);
						StringToStream(binaryWriter, SplitStr.StSkidka, (Kkm.PrintingWidth <= 40) ? 40 : Kkm.PrintingWidth, 32);
						if (IsCommandBad(RezultCommand, await RunCommand(47u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать не фискальную строку"))
						{
							return;
						}
					}
				}
			}
			if (DataCommand.IsFiscalCheck && NewModel && Kkm.FfdVersion == 1 && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m)
			{
				if (IsCheckCorrection)
				{
					Error = "Недопустимо в чеке коррекции фискальные строки!";
					RezultCommand.Status = ExecuteStatus.Error;
					return;
				}
				decimal Skidka = Math.Round(PrintString.Register.Quantity * PrintString.Register.Price, 2, MidpointRounding.AwayFromZero) - PrintString.Register.Amount;
				Skidka = Math.Round(Skidka, 2, MidpointRounding.AwayFromZero);
				decimal Price = Math.Round(PrintString.Register.Price, 2, MidpointRounding.AwayFromZero);
				decimal Price2 = Math.Round(PrintString.Register.Amount / PrintString.Register.Quantity, 2, MidpointRounding.AwayFromZero);
				decimal Amount1 = Math.Round(Price2 * PrintString.Register.Quantity, 2, MidpointRounding.AwayFromZero);
				decimal Quantity1 = PrintString.Register.Quantity;
				decimal Price3 = default(decimal);
				decimal Amount2 = default(decimal);
				decimal Quantity2 = default(decimal);
				if (Amount1 - PrintString.Register.Amount != 0m && PrintString.Register.Quantity > 1m)
				{
					Quantity1 -= 1m;
					Amount1 = Math.Round(Price2 * Quantity1, 2, MidpointRounding.AwayFromZero);
					Quantity2 = 1m;
					Price3 = PrintString.Register.Amount - Amount1;
					Amount2 = Math.Round(Price3 * Quantity2, 2, MidpointRounding.AwayFromZero);
				}
				for (int Font = 0; Font < 2; Font++)
				{
					if (Font == 1)
					{
						Quantity1 = Quantity2;
						Amount1 = Amount2;
						Price2 = Price3;
					}
					if (Price2 == 0m && Quantity1 == 0m)
					{
						continue;
					}
					if (Font == 1)
					{
						Price2 = Math.Round(Amount1 / Quantity1, 2, MidpointRounding.AwayFromZero);
					}
					MemoryStream memoryStream = new MemoryStream();
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					if (DataCommand.TypeCheck == 0)
					{
						binaryWriter.Write((byte)1);
					}
					else if (DataCommand.TypeCheck == 1)
					{
						binaryWriter.Write((byte)2);
					}
					else if (DataCommand.TypeCheck == 10)
					{
						binaryWriter.Write((byte)3);
					}
					else if (DataCommand.TypeCheck == 11)
					{
						binaryWriter.Write((byte)4);
					}
					NumberToStream(binaryWriter, (int)(Quantity1 * 1000m), 5);
					NumberToStream(binaryWriter, (int)(Price2 * 100m), 5);
					NumberToStream(binaryWriter, 0m, 5);
					NumberToStream(binaryWriter, 0m, 5);
					binaryWriter.Write((byte)PrintString.Register.Department);
					if (Nalogs.ContainsKey((int)PrintString.Register.Tax))
					{
						binaryWriter.Write((byte)(1 << Nalogs[(int)PrintString.Register.Tax] - 1));
					}
					else
					{
						Error = $"Ставка налога \"{PrintString.Register.Tax}\" не запрограммирована в ККМ";
					}
					if (PrintString.Register.EAN13 != null && PrintString.Register.EAN13 != "")
					{
						NumberToStream(binaryWriter, long.Parse(PrintString.Register.EAN13), 5);
					}
					else
					{
						NumberToStream(binaryWriter, 0m, 5);
					}
					StringToStream(binaryWriter, PrintString.Register.Name, 220, 0);
					if (IsCommandBad(RezultCommand, await RunCommand(65293u, AccessPassword, memoryStream), OpenSerial, ClearCheck: true, "Не удалось зарегистрировать фискальную строку"))
					{
						return;
					}
					string text3 = "";
					if (Skidka > 0m)
					{
						text3 = Unit.GetPringString("Скидка:<#0#>" + (-Amount1 + Price * Quantity1), Kkm.PrintingWidth);
					}
					else if (Skidka < 0m)
					{
						text3 = Unit.GetPringString("Наценка:<#0#>" + (Amount1 - Price * Quantity1), Kkm.PrintingWidth);
					}
					if (text3 != "")
					{
						memoryStream = new MemoryStream();
						binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write((byte)2);
						binaryWriter.Write((byte)1);
						StringToStream(binaryWriter, text3, (Kkm.PrintingWidth <= 40) ? 40 : Kkm.PrintingWidth, 32);
						if (IsCommandBad(RezultCommand, await RunCommand(47u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать не фискальную строку"))
						{
							return;
						}
					}
				}
			}
			if (!NewModel)
			{
				if (DataCommand.IsFiscalCheck && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m)
				{
					MemoryStream memoryStream = new MemoryStream();
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					byte command = 0;
					if (DataCommand.TypeCheck == 0)
					{
						command = 128;
					}
					else if (DataCommand.TypeCheck == 1)
					{
						command = 130;
					}
					else if (DataCommand.TypeCheck == 10)
					{
						command = 129;
					}
					else if (DataCommand.TypeCheck == 11)
					{
						command = 131;
					}
					decimal price = PrintString.Register.Price;
					NumberToStream(binaryWriter, PrintString.Register.Quantity * 1000m, 5);
					NumberToStream(binaryWriter, price * 100m, 5);
					binaryWriter.Write((byte)PrintString.Register.Department);
					byte b4 = 0;
					if (PrintString.Register.Tax != -50m)
					{
						if (Nalogs.ContainsKey((int)PrintString.Register.Tax))
						{
							b4 = Nalogs[(int)PrintString.Register.Tax];
						}
						else
						{
							Error = $"Ставка налога \"{PrintString.Register.Tax}\" не запрограммирована в ККМ";
						}
					}
					binaryWriter.Write(b4);
					binaryWriter.Write((byte)0);
					binaryWriter.Write((byte)0);
					binaryWriter.Write((byte)0);
					StringToStream(binaryWriter, PrintString.Register.Name, (Kkm.PrintingWidth <= 40) ? 40 : Kkm.PrintingWidth, 32);
					if (IsCommandBad(RezultCommand, await RunCommand(command, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать фискальную строку"))
					{
						return;
					}
				}
				if (DataCommand.IsFiscalCheck && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m)
				{
					decimal num = Math.Round(PrintString.Register.Quantity * PrintString.Register.Price, 2, MidpointRounding.AwayFromZero) - PrintString.Register.Amount;
					num = Math.Round(num, 2, MidpointRounding.AwayFromZero);
					if (PrintString.Register.Price != 0m && num != 0m)
					{
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
						byte command2;
						if (num > 0m)
						{
							command2 = 134;
						}
						else
						{
							command2 = 135;
							num = -num;
						}
						NumberToStream(binaryWriter, num * 100m, 5);
						byte b5 = 0;
						if (PrintString.Register.Tax != -50m)
						{
							if (Nalogs.ContainsKey((int)PrintString.Register.Tax))
							{
								b5 = Nalogs[(int)PrintString.Register.Tax];
							}
							else
							{
								Error = $"Ставка налога \"{PrintString.Register.Tax}\" не запрограммирована в ККМ";
							}
						}
						binaryWriter.Write(b5);
						binaryWriter.Write((byte)0);
						binaryWriter.Write((byte)0);
						binaryWriter.Write((byte)0);
						if (NewModel)
						{
							StringToStream(binaryWriter, "", 40, 0);
						}
						else
						{
							StringToStream(binaryWriter, "", 40, 0);
						}
						if (IsCommandBad(RezultCommand, await RunCommand(command2, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать фискальную скидку"))
						{
							return;
						}
					}
				}
			}
			if (DataCommand.NotPrint == false && PrintString != null && PrintString.BarCode != null && PrintString.BarCode.BarcodeType != "" && !(await PrintBarCode(PrintString.BarCode)))
			{
				return;
			}
		}
		PortLogs.Append("Закрытие чека", "-");
		if (NewModel && !IsCheckCorrection && DataCommand.IsFiscalCheck && DataCommand.CheckProps != null)
		{
			DataCommand.CheckProp[] checkProps = DataCommand.CheckProps;
			foreach (DataCommand.CheckProp Prop in checkProps)
			{
				if (!Prop.PrintInHeader)
				{
					await WriteProp(Prop.Print && !DataCommand.NotPrint.Value, Prop.Teg, Prop.Prop);
					if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Ошибка передачи тега " + Prop.Teg + ": "))
					{
						return;
					}
				}
			}
		}
		if (NewModel && !IsCheckCorrection && DataCommand.IsFiscalCheck && DataCommand.AdditionalProps != null)
		{
			DataCommand.AdditionalProp[] additionalProps = DataCommand.AdditionalProps;
			foreach (DataCommand.AdditionalProp additionalProp2 in additionalProps)
			{
				if (!additionalProp2.PrintInHeader)
				{
					await WriteAdditionaProp(additionalProp2.Print && !DataCommand.NotPrint.Value, additionalProp2.NameProp, additionalProp2.Prop);
					if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Ошибка передачи произвольных полей"))
					{
						return;
					}
				}
			}
		}
		bool Error55 = false;
		if (NewModel && DataCommand.IsFiscalCheck && (!IsCheckCorrection || Kkm.FfdVersion >= 3))
		{
			if (IsCheckCorrection)
			{
				string prop = ((DataCommand.CorrectionBaseNumber != null && DataCommand.CorrectionBaseNumber != "") ? DataCommand.CorrectionBaseNumber : " ");
				await WriteProp(!DataCommand.NotPrint.Value, 1179, prop);
				int num2 = (int)((DataCommand.CorrectionBaseDate.HasValue ? DataCommand.CorrectionBaseDate : new DateTime?(new DateTime(2001, 1, 1))).Value.Date - new DateTime(1970, 1, 1)).TotalSeconds;
				await WriteProp(!DataCommand.NotPrint.Value, 1178, num2);
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			NumberToStream(binaryWriter, DataCommand.Cash * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.CashLessType1 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.CashLessType2 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.CashLessType3 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.ElectronicPayment * 100m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, DataCommand.AdvancePayment * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.Credit * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.CashProvision * 100m, 5);
			NumberToStream(binaryWriter, 0m, 1);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, TaxVariant, 1);
			StringToStream(binaryWriter, "", 40, 32);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			NumberToStream(binaryWriter, 0m, 5);
			bData = await RunCommand(65349u, OperatorPasswor, memoryStream);
			if (IsCommandBad(RezultCommand, bData, OpenSerial: false, ClearCheck: false, "Не удалось закрыть чек"))
			{
				int num3 = 1;
				if (bData != null && bData[0] == byte.MaxValue)
				{
					num3 = 2;
				}
				if (bData == null || bData[num3] != 55)
				{
					await ClearOldCheck();
					if (OpenSerial)
					{
						await PortCloseAsync();
					}
					return;
				}
				Error55 = true;
				Error = "";
			}
			byte[] array2 = await RunCommand(65344u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(RezultCommand, array2, OpenSerial: false, ClearCheck: false, "Не удалось получить данные чека"))
			{
				RezultCommand.SessionCheckNumber = (int)NumberFromStream(array2, 6, 2);
			}
			if (bData.Length >= 21)
			{
				uint num4 = (uint)NumberFromStream(bData, 8, 4);
				ulong num5 = (uint)((bData[15] << 24) + (bData[14] << 16) + (bData[13] << 8) + bData[12]);
				DateTime dateTime = new DateTime(bData[16] + 2000, bData[17], bData[18], bData[19], bData[20], 0, 0);
				RezultCommand.QRCode = "t=" + dateTime.ToString("yyyyMMddTHHmm") + "&s=" + AllSumm.ToString("0.00").Replace(',', '.') + "&fn=" + Kkm.Fn_Number + "&i=" + num4.ToString("D0") + "&fp=" + num5.ToString("D0") + "&n=" + CheckType;
			}
			await GetCheckAndSession(RezultCommand);
		}
		if (NewModel && DataCommand.IsFiscalCheck && IsCheckCorrection && Kkm.FfdVersion <= 2)
		{
			string prop2 = ((DataCommand.CorrectionBaseNumber != null && DataCommand.CorrectionBaseNumber != "") ? DataCommand.CorrectionBaseNumber : " ");
			await WriteProp(!DataCommand.NotPrint.Value, 1179, prop2);
			int num6 = (int)((DataCommand.CorrectionBaseDate.HasValue ? DataCommand.CorrectionBaseDate : new DateTime?(new DateTime(2001, 1, 1))).Value.Date - new DateTime(1970, 1, 1)).TotalSeconds;
			await WriteProp(!DataCommand.NotPrint.Value, 1178, num6);
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write((byte)DataCommand.CorrectionType);
			if (DataCommand.TypeCheck == 2)
			{
				binaryWriter.Write((byte)1);
			}
			else if (DataCommand.TypeCheck == 3)
			{
				binaryWriter.Write((byte)2);
			}
			else if (DataCommand.TypeCheck == 12)
			{
				binaryWriter.Write((byte)2);
			}
			else if (DataCommand.TypeCheck == 13)
			{
				binaryWriter.Write((byte)1);
			}
			NumberToStream(binaryWriter, (int)(DataCommand.Amount * 100m), 5);
			NumberToStream(binaryWriter, DataCommand.Cash * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.ElectronicPayment * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.AdvancePayment * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.Credit * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.CashProvision * 100m, 5);
			NumberToStream(binaryWriter, (DataCommand.SumTax18 + DataCommand.SumTax20 + DataCommand.SumTax22) * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTax10 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTax0 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTaxNone * 100m, 5);
			NumberToStream(binaryWriter, (DataCommand.SumTax118 + DataCommand.SumTax120 + DataCommand.SumTax122) * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTax110 * 100m, 5);
			NumberToStream(binaryWriter, TaxVariant, 1);
			NumberToStream(binaryWriter, DataCommand.SumTax5 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTax7 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTax105 * 100m, 5);
			NumberToStream(binaryWriter, DataCommand.SumTax107 * 100m, 5);
			bData = await RunCommand(65354u, AccessPassword, memoryStream);
			if (IsCommandBad(RezultCommand, bData, OpenSerial: false, ClearCheck: false, "Не удалось закрыть чек"))
			{
				int num7 = 1;
				if (bData != null && bData[0] == byte.MaxValue)
				{
					num7 = 2;
				}
				if (bData == null || bData[num7] != 55)
				{
					await ClearOldCheck();
					if (OpenSerial)
					{
						await PortCloseAsync();
					}
					return;
				}
				Error55 = true;
				Error = "";
			}
			NumberCheck = StringNumberFromStreamg(bData, 3, 2, 4);
			int num8 = int.Parse(NumberCheck);
			RezultCommand.CheckNumber = num8;
		}
		if ((Error55 || !NewModel) && DataCommand.IsFiscalCheck && !IsCheckCorrection)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			NumberToStream(binaryWriter, DataCommand.Cash * 100m, 5);
			NumberToStream(binaryWriter, (DataCommand.CashLessType1 + DataCommand.ElectronicPayment + DataCommand.AdvancePayment) * 100m, 5);
			NumberToStream(binaryWriter, (DataCommand.CashLessType2 + DataCommand.Credit) * 100m, 5);
			NumberToStream(binaryWriter, (DataCommand.CashLessType3 + DataCommand.CashProvision) * 100m, 5);
			NumberToStream(binaryWriter, 0m, 2);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)0);
			StringToStream(binaryWriter, "", 40, 32);
			if (IsCommandBad(RezultCommand, await RunCommand(133u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			RezultCommand.CheckNumber = int.Parse(NumberCheck) + 1;
		}
		else if (Error55 && Kkm.FfdVersion == 1 && DataCommand.IsFiscalCheck && IsCheckCorrection)
		{
			await SetValueInTable(18, 1, 5, TaxVariant);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Ошибка установки СНО"))
			{
				return;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			NumberToStream(binaryWriter, (int)((DataCommand.Cash + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3) * 100m), 5);
			if (DataCommand.TypeCheck == 2)
			{
				binaryWriter.Write((byte)1);
			}
			else if (DataCommand.TypeCheck == 3)
			{
				binaryWriter.Write((byte)2);
			}
			else if (DataCommand.TypeCheck == 12)
			{
				binaryWriter.Write((byte)3);
			}
			else if (DataCommand.TypeCheck == 13)
			{
				binaryWriter.Write((byte)4);
			}
			bData = await RunCommand(65334u, AccessPassword, memoryStream);
			if (IsCommandBad(RezultCommand, bData, OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			NumberCheck = StringNumberFromStreamg(bData, 3, 2, 4);
			int num9 = int.Parse(NumberCheck);
			RezultCommand.CheckNumber = num9;
		}
		if (DataCommand.IsFiscalCheck)
		{
			IsNotErrorStatus = true;
		}
		RezultCommand.Error = Error;
		IsNotErrorStatus = true;
		PortLogs.Append("Конец регистрации чека", "-");
		if (!DataCommand.IsFiscalCheck)
		{
			await RunCommand(83u, OperatorPasswor, new MemoryStream(new byte[1] { 1 }), 5000);
			Error = "";
		}
		if (RezultCommand.QRCode == "" && DataCommand.IsFiscalCheck && NewModel)
		{
			RezultCommand.QRCode = await GetUrlDoc(ShekOrDoc: true, RezultCommand);
			RezultCommand.SessionCheckNumber = OldSessionCheckNumber;
		}
		for (int i = DataCommand.NumberCopies; i > 0; i--)
		{
			await WaitPrint(OpenSerial, RezultCommand);
			RezultCommand.Error = "";
			RezultCommand.Status = ExecuteStatus.Ok;
			await RunCommand(140u, AccessPassword, new MemoryStream());
		}
		if (DataCommand.Sound)
		{
			await RunCommand(19u, OperatorPasswor, new MemoryStream());
			Error = "";
		}
		Error = "";
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task ValidationMarkingCode(DataCommand DataCommand, RezultMarkingCodeValidation RezultCommand, bool InCheck = false)
	{
		await base.ValidationMarkingCode(DataCommand, RezultCommand);
		if (Kkm.FfdVersion < 4)
		{
			Error = "ККТ не поддерживает формат ФФД 1.2 и выше";
			return;
		}
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await CloseDocumentAndOpenShift(DataCommand, RezultCommand);
		foreach (DataCommand.GoodCodeData GoodCodeData in DataCommand.GoodCodeDatas)
		{
			RezultMarkingCodeValidation.tMarkingCodeValidation ItemValidation = RezultCommand.MarkingCodeValidation.Find((RezultMarkingCodeValidation.tMarkingCodeValidation i) => i.TryBarCode == GoodCodeData.TryBarCode);
			ItemValidation.ValidationKKT.ValidationResult = 1u;
			ItemValidation.ValidationKKT.DecryptionResult = GetMarkingCodeDecryptionResult(ItemValidation.ValidationKKT.ValidationResult);
			if (string.IsNullOrEmpty(GoodCodeData.TryBarCode))
			{
				continue;
			}
			string text = GoodCodeData.TryBarCode;
			if (text.Substring(text.Length - 1, 1) == "\u001d")
			{
				text = text.Substring(0, text.Length - 1);
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			int statusMarkingCode = GetStatusMarkingCode(DataCommand.TypeCheck, GoodCodeData.MeasureOfQuantity);
			binaryWriter.Write((byte)statusMarkingCode);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)text.Length);
			if (GoodCodeData.PackageQuantity.HasValue)
			{
				binaryWriter.Write((byte)17);
			}
			else if (GoodCodeData.MeasureOfQuantity != 0)
			{
				binaryWriter.Write((byte)17);
			}
			else
			{
				binaryWriter.Write((byte)0);
			}
			StringToStream(binaryWriter, text, text.Length, 32);
			if (GoodCodeData.PackageQuantity.HasValue)
			{
				binaryWriter.Write((ushort)2108);
				binaryWriter.Write((ushort)1);
				binaryWriter.Write((byte)GoodCodeData.MeasureOfQuantity.Value);
				binaryWriter.Write((ushort)1023);
				binaryWriter.Write((ushort)8);
				FVLNWrite(binaryWriter, 1m);
				binaryWriter.Write((ushort)1293);
				binaryWriter.Write((ushort)8);
				VLNWrite(binaryWriter, (uint)Math.Truncate(GoodCodeData.Quantity));
				binaryWriter.Write((ushort)1294);
				binaryWriter.Write((ushort)8);
				VLNWrite(binaryWriter, GoodCodeData.PackageQuantity.Value);
			}
			else if (GoodCodeData.MeasureOfQuantity != 0)
			{
				binaryWriter.Write((ushort)2108);
				binaryWriter.Write((ushort)1);
				binaryWriter.Write((byte)GoodCodeData.MeasureOfQuantity.Value);
				binaryWriter.Write((ushort)1023);
				binaryWriter.Write((ushort)8);
				FVLNWrite(binaryWriter, GoodCodeData.Quantity);
			}
			byte[] array = await RunCommand(65377u, OperatorPasswor, memoryStream);
			if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось начать проверку кода маркировки"))
			{
				return;
			}
			uint num = array[3];
			uint CodeRezultFN = array[4];
			_ = array[5];
			byte num2 = array[6];
			uint CodeRezultFN_StrihM = 0u;
			if ((uint)num2 >= 1u)
			{
				CodeRezultFN_StrihM = array[7];
			}
			uint ValidationResult;
			if ((uint)num2 >= 2u)
			{
				ValidationResult = array[8];
			}
			else
			{
				ValidationResult = num switch
				{
					0u => 3u, 
					1u => 3u, 
					2u => 1u, 
					3u => 1u, 
					4u => 3u, 
					_ => 0u, 
				};
				if (Kkm.OfflineMode)
				{
					ValidationResult += 16;
				}
			}
			bool num3 = MarkingCodeIsBad(ValidationResult);
			bool acceptMarking = true;
			if (num3 && !GoodCodeData.AcceptOnBad)
			{
				acceptMarking = false;
			}
			memoryStream = new MemoryStream();
			binaryWriter = new BinaryWriter(memoryStream);
			if (acceptMarking)
			{
				binaryWriter.Write((byte)1);
			}
			else
			{
				binaryWriter.Write((byte)0);
			}
			if (IsCommandBad(RezultCommand, await RunCommand(65385u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: false, "Не удалось подтвердить код маркировки"))
			{
				return;
			}
			ItemValidation.ValidationKKT.ValidationResult = ValidationResult;
			ItemValidation.ValidationKKT.DecryptionResult = GetMarkingCodeDecryptionResult(ItemValidation.ValidationKKT.ValidationResult, 0u, 1u, CodeRezultFN);
			if (ValidationResult != 0)
			{
				switch (CodeRezultFN_StrihM)
				{
				case 1u:
					ItemValidation.ValidationKKT.DecryptionResult += "; Ошибка: Неверный фискальный признак отве";
					break;
				case 2u:
					ItemValidation.ValidationKKT.DecryptionResult += "; Ошибка: Неверный формат реквизиов ответа";
					break;
				case 3u:
					ItemValidation.ValidationKKT.DecryptionResult += "; Ошибка: Неверный номер запроса в ответе";
					break;
				case 4u:
					ItemValidation.ValidationKKT.DecryptionResult += "; Ошибка: Неверный номер ФН";
					break;
				case 5u:
					ItemValidation.ValidationKKT.DecryptionResult += "; Неверный CRC блока данных";
					break;
				case 7u:
					ItemValidation.ValidationKKT.DecryptionResult += "; Неверная длина ответа.";
					break;
				default:
				{
					RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationKKT validationKKT = ItemValidation.ValidationKKT;
					validationKKT.DecryptionResult = validationKKT.DecryptionResult + "; Ошибка: " + CodeRezultFN_StrihM;
					break;
				}
				case 0u:
				case 32u:
					break;
				}
			}
			if (!acceptMarking && InCheck)
			{
				throw new Exception("Код маркировки не прошел проверку: " + ItemValidation.ValidationKKT.DecryptionResult);
			}
		}
		Error = "";
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task OpenShift(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		byte[] array = await RunCommand(17u, OperatorPasswor, new MemoryStream());
		if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			return;
		}
		byte b = (byte)NumberFromStream(array, 15, 1);
		if ((b & 0xF) == 8)
		{
			if (IsCommandBad(RezultCommand, await RunCommand(136u, OperatorPasswor, new MemoryStream()), OpenSerial, ClearCheck: false, "Не удалось отменить предыдущий чек"))
			{
				return;
			}
			if (NewModel)
			{
				await SerCashier(DataCommand);
			}
			array = await RunCommand(17u, OperatorPasswor, new MemoryStream());
			if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
			{
				return;
			}
			b = (byte)NumberFromStream(array, 15, 1);
		}
		if ((b & 0xF) == 3)
		{
			Error = "Не удалось открыть смену";
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return;
		}
		await SetNotPrint(DataCommand);
		await SetNotPrint(DataCommand);
		array = await RunCommand(65345u, AccessPassword, new MemoryStream());
		if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось открыть смену"))
		{
			if (array != null && array[1] == 115 && SessionOpen == 1)
			{
				Error = "Не удалось открыть смену (Смена уже открыта))";
			}
			return;
		}
		await SerCashier(DataCommand);
		array = await RunCommand(224u, AccessPassword, new MemoryStream());
		if ((array == null || array[1] != 55) && IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось открыть смену"))
		{
			if (array != null && array[1] == 115 && SessionOpen != 1)
			{
				Error = "Не удалось открыть смену (Смена уже открыта)";
			}
			return;
		}
		await GetCheckAndSession(RezultCommand);
		RezultCommand.QRCode = await GetUrlDoc();
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task CloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		await GetCheckAndSession(RezultCommand, IsSessionNumber: true, IsCheckNumber: false);
		await SetNotPrint(DataCommand);
		await SetNotPrint(DataCommand);
		byte[] array = await RunCommand(65346u, AccessPassword, new MemoryStream());
		if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось закрыть смену"))
		{
			if (array != null && array[1] == 115 && SessionOpen == 1)
			{
				Error = "Не удалось закрыть смену (Смена не открыта)";
			}
			return;
		}
		await SerCashier(DataCommand);
		array = await RunCommand(65u, AccessPassword, new MemoryStream());
		if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось закрыть смену"))
		{
			if (array != null && array[1] == 115 && SessionOpen == 1)
			{
				Error = "Не удалось закрыть смену (Смена не открыта)";
			}
			return;
		}
		await GetCheckAndSession(RezultCommand, IsSessionNumber: false);
		RezultCommand.QRCode = await GetUrlDoc();
		if (SettDr.Paramets["SetDateTime"].AsBool())
		{
			try
			{
				DateTime Now = DateTime.Now;
				if (!IsCommandBad(null, await RunCommand(34u, AccessPassword, new MemoryStream(new byte[3]
				{
					(byte)Now.Day,
					(byte)Now.Month,
					(byte)(Now.Year - 2000)
				})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени"))
				{
					Error = "";
					await RunCommand(35u, AccessPassword, new MemoryStream(new byte[3]
					{
						(byte)Now.Day,
						(byte)Now.Month,
						(byte)(Now.Year - 2000)
					}));
					Error = "";
					await RunCommand(33u, AccessPassword, new MemoryStream(new byte[3]
					{
						(byte)Now.Hour,
						(byte)Now.Minute,
						(byte)Now.Second
					}));
				}
				Error = "";
			}
			catch
			{
				Error = "";
			}
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task XReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		if (!IsCommandBad(RezultCommand, await RunCommand(64u, AccessPassword, new MemoryStream()), OpenSerial, ClearCheck: false, "Не удалось Напечатать X отчет"))
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task OfdReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		if (!NewModel)
		{
			Error = "Команда не поддерживается оборудованием";
			return;
		}
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		if (NewModel && DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			await SetValueInTable(2, NumLineCashier, 2, DataCommand.CashierName, 21);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить кассира"))
			{
				return;
			}
		}
		byte[] array = await RunCommand(65337u, AccessPassword, new MemoryStream());
		if (!IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось Напечатать отчет диагностики соединения с ОФД"))
		{
			DataCommand dataCommand = new DataCommand();
			dataCommand.Command = "RegisterCheck";
			dataCommand.IsFiscalCheck = false;
			dataCommand.NotPrint = false;
			dataCommand.CheckStrings = new DataCommand.CheckString[15];
			dataCommand.CheckStrings[0] = new DataCommand.CheckString();
			dataCommand.CheckStrings[0].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[0].PrintText.Font = 1;
			dataCommand.CheckStrings[0].PrintText.Intensity = 15;
			dataCommand.CheckStrings[0].PrintText.Text = "Диагностика соединения с ОФД";
			dataCommand.CheckStrings[1] = new DataCommand.CheckString();
			dataCommand.CheckStrings[1].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[1].PrintText.Text = "".PadRight(Kkm.PrintingWidth, '-');
			dataCommand.CheckStrings[2] = new DataCommand.CheckString();
			dataCommand.CheckStrings[2].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[2].PrintText.Text = "Транспортное соединение установлено : " + (((array[3] & 1) > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[3] = new DataCommand.CheckString();
			dataCommand.CheckStrings[3].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[3].PrintText.Text = "Есть сообщение для передачи в ОФД   : " + (((array[3] & 2) > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[4] = new DataCommand.CheckString();
			dataCommand.CheckStrings[4].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[4].PrintText.Text = "Ожидание ответного сообщения от ОФД : " + (((array[3] & 4) > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[5] = new DataCommand.CheckString();
			dataCommand.CheckStrings[5].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[5].PrintText.Text = "Есть команда от ОФД                 : " + (((array[3] & 8) > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[6] = new DataCommand.CheckString();
			dataCommand.CheckStrings[6].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[6].PrintText.Text = "Изменились настройки соединения     : " + (((array[3] & 0x10) > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[7] = new DataCommand.CheckString();
			dataCommand.CheckStrings[7].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[7].PrintText.Text = "Ожидание ответа на команду от ОФД   : " + (((array[3] & 0x20) > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[8] = new DataCommand.CheckString();
			dataCommand.CheckStrings[8].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[8].PrintText.Text = "".PadRight(Kkm.PrintingWidth, '-');
			dataCommand.CheckStrings[9] = new DataCommand.CheckString();
			dataCommand.CheckStrings[9].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[9].PrintText.Text = "Состояние чтения сообщения   : " + ((array[4] > 0) ? "Да" : "Нет");
			dataCommand.CheckStrings[10] = new DataCommand.CheckString();
			dataCommand.CheckStrings[10].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[10].PrintText.Text = "Количество сообщений для ОФД : " + (long)(array[5] + (array[6] << 8));
			dataCommand.CheckStrings[11] = new DataCommand.CheckString();
			dataCommand.CheckStrings[11].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[11].PrintText.Text = "Номер первого не переданного : " + (long)(array[7] + (array[8] << 8) + (array[9] << 16) + array[10]);
			dataCommand.CheckStrings[12] = new DataCommand.CheckString();
			dataCommand.CheckStrings[12].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[12].PrintText.Text = "Дата первого не переданного  : ";
			dataCommand.CheckStrings[13] = new DataCommand.CheckString();
			dataCommand.CheckStrings[13].PrintText = new DataCommand.PrintString();
			try
			{
				dataCommand.CheckStrings[13].PrintText.Text = ": " + new DateTime(2000 + array[11], array[12], array[13], array[14], array[15], 0);
			}
			catch
			{
				dataCommand.CheckStrings[13].PrintText.Text = ": " + default(DateTime);
			}
			dataCommand.CheckStrings[14] = new DataCommand.CheckString();
			dataCommand.CheckStrings[14].PrintText = new DataCommand.PrintString();
			dataCommand.CheckStrings[14].PrintText.Text = "".PadRight(Kkm.PrintingWidth, '-');
			await RegisterCheck(dataCommand, RezultCommand);
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task OpenCashDrawer(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		MemoryStream memoryStream = new MemoryStream();
		new BinaryWriter(memoryStream).Write((byte)0);
		if (!IsCommandBad(RezultCommand, await RunCommand(40u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: false, "Не удалось Открыть денежный ящик"))
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task DepositingCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		await SetNotPrint(DataCommand);
		if (NewModel && DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			await SetValueInTable(2, NumLineCashier, 2, DataCommand.CashierName, 21);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить кассира"))
			{
				return;
			}
		}
		await SetNotPrint(DataCommand);
		decimal amount = DataCommand.Amount;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(memoryStream);
		NumberToStream(bw, amount * 100m, 5);
		if (!IsCommandBad(RezultCommand, await RunCommand(80u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: false, "Не удалась операция Внесение денег"))
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task PaymentCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		await SetNotPrint(DataCommand);
		if (NewModel && DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			await SetValueInTable(2, NumLineCashier, 2, DataCommand.CashierName, 21);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, "Не удалось установить кассира"))
			{
				return;
			}
		}
		await SetNotPrint(DataCommand);
		decimal amount = DataCommand.Amount;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(memoryStream);
		NumberToStream(bw, amount * 100m, 5);
		if (!IsCommandBad(RezultCommand, await RunCommand(81u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: false, "Не удалась операция Изъятие денег"))
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task GetLineLength(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		await ClearOldCheck();
		RezultCommand.LineLength = Kkm.PrintingWidth;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task KkmRegOfd(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		bool IsReboot = false;
		if (!NewModel)
		{
			Error = "Команда не поддерживается оборудованием";
			return;
		}
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ProcessInitDevice(FullInit: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (SessionOpen == 2 || SessionOpen == 3)
		{
			RezultCommand.Status = ExecuteStatus.Error;
			Error = "Нельзя выполнить команду при открытой смене.";
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return;
		}
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			await SetValueInTable(2, NumLineCashier, 2, DataCommand.CashierName, 21);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: true, ""))
			{
				return;
			}
		}
		DateTime Now = DateTime.Now;
		IsCommandBad(null, await RunCommand(34u, AccessPassword, new MemoryStream(new byte[3]
		{
			(byte)Now.Day,
			(byte)Now.Month,
			(byte)(Now.Year - 2000)
		})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
		Error = "";
		IsCommandBad(null, await RunCommand(35u, AccessPassword, new MemoryStream(new byte[3]
		{
			(byte)Now.Day,
			(byte)Now.Month,
			(byte)(Now.Year - 2000)
		})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
		Error = "";
		IsCommandBad(null, await RunCommand(33u, AccessPassword, new MemoryStream(new byte[3]
		{
			(byte)Now.Hour,
			(byte)Now.Minute,
			(byte)Now.Second
		})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
		Error = "";
		if (DataCommand.RegKkmOfd.Command == "Open" || DataCommand.RegKkmOfd.Command == "ChangeFN" || DataCommand.RegKkmOfd.Command == "ChangeOrganization" || DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "ChangeOFD")
		{
			byte b = 0;
			if (DataCommand.RegKkmOfd.Command == "Open")
			{
				b = 0;
			}
			else if (DataCommand.RegKkmOfd.Command == "ChangeFN")
			{
				b = 1;
			}
			else if (DataCommand.RegKkmOfd.Command == "ChangeOrganization" || DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "ChangeOFD")
			{
				b = 2;
			}
			if (IsCommandBad(RezultCommand, await RunCommand(65285u, AccessPassword, new MemoryStream(new byte[1] { b })), OpenSerial, ClearCheck: false, "Ошибка открытия отчета о перерегистрации"))
			{
				return;
			}
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "Open")
		{
			if (DataCommand.RegKkmOfd.UrlServerOfd != "")
			{
				await SetValueInTable(19, 1, 1, DataCommand.RegKkmOfd.UrlServerOfd.PadRight(64, '\0'), 64);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки URL сервера ОФД: "))
				{
					return;
				}
			}
			if (DataCommand.RegKkmOfd.PortServerOfd != "")
			{
				await SetValueInTable(19, 1, 2, new byte[2]
				{
					(byte)(int.Parse(DataCommand.RegKkmOfd.PortServerOfd) & 0xFF),
					(byte)(int.Parse(DataCommand.RegKkmOfd.PortServerOfd) >> 8)
				});
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки порта сервера ОФД: "))
				{
					return;
				}
			}
			if (DataCommand.RegKkmOfd.UrlOfd != "")
			{
				await SetValueInTable(18, 1, 11, DataCommand.RegKkmOfd.UrlOfd.PadRight(64, '\0'), 64);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки префикса URL чека: "))
				{
					return;
				}
			}
			if (DataCommand.RegKkmOfd.NameOFD != "")
			{
				await SetValueInTable(18, 1, 10, DataCommand.RegKkmOfd.NameOFD.PadRight(64, '\0'), 64);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки имени ОФД: "))
				{
					return;
				}
			}
			if (DataCommand.RegKkmOfd.InnOfd != "")
			{
				await SetValueInTable(18, 1, 12, DataCommand.RegKkmOfd.InnOfd.PadRight(17, '\0'), 17);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки ИНН ОФД: ") && Kkm.FfdVersion >= 2)
				{
					return;
				}
				Error = "";
			}
		}
		int KktMode = 0;
		KktMode += (DataCommand.RegKkmOfd.EncryptionMode ? 1 : 0);
		KktMode += (DataCommand.RegKkmOfd.OfflineMode ? 2 : 0);
		KktMode += (DataCommand.RegKkmOfd.AutomaticMode ? 4 : 0);
		KktMode += (DataCommand.RegKkmOfd.ServiceMode ? 8 : 0);
		KktMode += (DataCommand.RegKkmOfd.BSOMode ? 16 : 0);
		KktMode += (DataCommand.RegKkmOfd.InternetMode ? 32 : 0);
		int KktMode2 = 0;
		if (Kkm.FfdSupportVersion >= 2)
		{
			KktMode2 += (DataCommand.RegKkmOfd.SaleExcisableGoods ? 1 : 0);
			KktMode2 += (DataCommand.RegKkmOfd.SignOfGambling ? 2 : 0);
			KktMode2 += (DataCommand.RegKkmOfd.SignOfLottery ? 4 : 0);
			KktMode += (DataCommand.RegKkmOfd.PrinterAutomatic ? 8 : 0);
		}
		if (Kkm.FfdSupportVersion >= 4)
		{
			KktMode2 += (DataCommand.RegKkmOfd.SaleMarking ? 16 : 0);
			KktMode2 += (DataCommand.RegKkmOfd.SignPawnshop ? 32 : 0);
			KktMode2 += (DataCommand.RegKkmOfd.SignAssurance ? 64 : 0);
		}
		byte SignOfAgent = 0;
		string[] array = DataCommand.RegKkmOfd.SignOfAgent.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string s in array)
		{
			SignOfAgent = (byte)(SignOfAgent + (1 << int.Parse(s)));
		}
		string[] array2 = DataCommand.RegKkmOfd.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		byte TaxVariant = 0;
		if (array2.Length == 0)
		{
			array2 = Kkm.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length == 0)
		{
			array2 = Kkm.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		}
		array = array2;
		foreach (string s2 in array)
		{
			TaxVariant = (byte)(TaxVariant + (1 << int.Parse(s2)));
		}
		if (DataCommand.RegKkmOfd.Command == "Open")
		{
			await SetValueInTable(18, 1, 6, (byte)KktMode);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки Услуги: "))
			{
				return;
			}
			if (Kkm.FfdSupportVersion >= 2)
			{
				await SetValueInTable(18, 1, 21, (byte)KktMode2);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки Услуги: "))
				{
					return;
				}
			}
		}
		if (!(DataCommand.RegKkmOfd.Command == "ChangeFN"))
		{
			_ = DataCommand.RegKkmOfd.Command == "Open";
		}
		if ((DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "Open") && DataCommand.RegKkmOfd.InnOfd != "")
		{
			await SetValueInTable(18, 1, 12, DataCommand.RegKkmOfd.InnOfd.PadRight(64, '\0'), 64);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки ИНН ОФД: "))
			{
				return;
			}
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeOrganization" || DataCommand.RegKkmOfd.Command == "Open")
		{
			if (DataCommand.RegKkmOfd.NameOrganization != "")
			{
				await SetValueInTable(18, 1, 7, DataCommand.RegKkmOfd.NameOrganization);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки имени организации: "))
				{
					return;
				}
			}
			if (DataCommand.RegKkmOfd.AddressSettle != "")
			{
				await SetValueInTable(18, 1, 9, DataCommand.RegKkmOfd.AddressSettle);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки адреса установки ККМ: "))
				{
					return;
				}
			}
			if (DataCommand.RegKkmOfd.PlaceSettle != "")
			{
				await SetValueInTable(18, 1, 14, DataCommand.RegKkmOfd.PlaceSettle);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки места установки ККМ: ") && Kkm.FfdVersion >= 2)
				{
					return;
				}
				Error = "";
			}
			await SetValueInTable(18, 1, 5, TaxVariant);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки СНО: "))
			{
				return;
			}
			if (Kkm.FfdSupportVersion <= 3)
			{
				await SetValueInTable(18, 1, 16, SignOfAgent);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки признака агента: ") && Kkm.FfdVersion >= 2)
				{
					return;
				}
			}
			Error = "";
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "Open")
		{
			if (Kkm.FfdSupportVersion >= 2)
			{
				byte OldFfdVersion = (byte)(int)(await GetValueInTable(17, 1, 17, typeof(byte)));
				Error = "";
				byte SetFfdVersion = 0;
				if (DataCommand.RegKkmOfd.SetFfdVersion == 0)
				{
					DataCommand.RegKkmOfd.SetFfdVersion = Kkm.FfdMinimumVersion;
				}
				switch (DataCommand.RegKkmOfd.SetFfdVersion)
				{
				case 1:
					SetFfdVersion = 1;
					break;
				case 2:
					SetFfdVersion = 2;
					break;
				case 3:
					SetFfdVersion = 3;
					break;
				case 4:
					SetFfdVersion = 4;
					break;
				}
				await SetValueInTable(17, 1, 17, SetFfdVersion);
				if (OldFfdVersion != SetFfdVersion)
				{
					IsReboot = true;
				}
			}
			await SetValueInTable(18, 1, 6, (byte)KktMode);
			if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки Услуги: "))
			{
				return;
			}
			if (Kkm.FfdSupportVersion >= 2)
			{
				await SetValueInTable(18, 1, 21, (byte)KktMode2);
				if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Ошибка установки Услуги: "))
				{
					return;
				}
			}
		}
		byte b2 = 0;
		switch (DataCommand.RegKkmOfd.Command)
		{
		case "ChangeFN":
			b2 = 1;
			break;
		case "ChangeOFD":
			b2 = 2;
			break;
		case "ChangeOrganization":
			b2 = 3;
			break;
		case "ChangeKkm":
			b2 = 4;
			break;
		}
		byte[] buffer;
		if (DataCommand.RegKkmOfd.Command == "Open")
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			StringToStream(binaryWriter, DataCommand.RegKkmOfd.InnOrganization.PadRight(12, ' '), 12, 32);
			StringToStream(binaryWriter, DataCommand.RegKkmOfd.RegNumber.PadRight(20, ' '), 20, 32);
			binaryWriter.Write(TaxVariant);
			binaryWriter.Write((byte)KktMode);
			buffer = await RunCommand(65286u, AccessPassword, memoryStream);
		}
		else if (DataCommand.RegKkmOfd.Command == "Close")
		{
			if (IsCommandBad(RezultCommand, await RunCommand(65341u, AccessPassword, new MemoryStream()), OpenSerial, ClearCheck: false, "Не удалось выполнить команду регистрации: "))
			{
				return;
			}
			buffer = await RunCommand(65342u, AccessPassword, new MemoryStream());
			if (IsCommandBad(RezultCommand, buffer, OpenSerial, ClearCheck: false, "Не удалось выполнить команду регистрации: "))
			{
				return;
			}
		}
		else
		{
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
			StringToStream(binaryWriter2, DataCommand.RegKkmOfd.InnOrganization.PadRight(12, ' '), 12, 32);
			StringToStream(binaryWriter2, DataCommand.RegKkmOfd.RegNumber.PadRight(20, ' '), 20, 32);
			binaryWriter2.Write(TaxVariant);
			binaryWriter2.Write((byte)KktMode);
			binaryWriter2.Write(b2);
			buffer = await RunCommand(65332u, AccessPassword, memoryStream2);
		}
		if (IsCommandBad(RezultCommand, buffer, OpenSerial, ClearCheck: false, "Не удалось выполнить команду регистрации: "))
		{
			string LastError = Error;
			await ProcessInitDevice(FullInit: true);
			Error = LastError;
			return;
		}
		try
		{
			uint DocNumber = 0u;
			byte[] array3 = await RunCommand(65281u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(RezultCommand, array3, OpenSerial, ClearCheck: false, "Не удалось получить данные чека"))
			{
				string s3 = StringNumberFromStreamg(array3, 29, 4, 0);
				DocNumber = uint.Parse(s3);
			}
			MemoryStream memoryStream3 = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(memoryStream3);
			NumberToStream(bw, DocNumber, 4);
			array3 = await RunCommand(65338u, AccessPassword, memoryStream3);
			if (IsCommandBad(RezultCommand, array3, OpenSerial, ClearCheck: true, "Ошибка запроса документа"))
			{
				return;
			}
			int.Parse(StringNumberFromStreamg(array3, 5, 2, 0));
			DateTime Date = default(DateTime);
			long FiscalSign = 0L;
			int i2 = 0;
			do
			{
				array3 = await RunCommand(65339u, AccessPassword, new MemoryStream());
				if (IsCommandBad(RezultCommand, array3, OpenSerial, ClearCheck: true, "Ошибка запроса документа"))
				{
					break;
				}
				string text = StringNumberFromStreamg(array3, 3, 2, 0);
				int.Parse(StringNumberFromStreamg(array3, 5, 2, 0));
				if (!(text == "1012"))
				{
					if (text == "1077")
					{
						FiscalSign = (uint)((array3[9] << 24) + (array3[10] << 16) + (array3[11] << 8) + array3[12]);
						i2++;
					}
				}
				else
				{
					Date = new DateTime(1970, 1, 1).AddSeconds(uint.Parse(StringNumberFromStreamg(array3, 7, 4, 0)));
					i2++;
				}
			}
			while (i2 < 2);
			Error = "";
			RezultCommand.QRCode = "Дата: " + Date.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + DocNumber.ToString("D0") + ", ФПД: " + FiscalSign.ToString("D0");
			await TerminateStausOutDate();
		}
		catch (Exception)
		{
			RezultCommand.QRCode = "Ошибка чтения реквизитов документа ";
		}
		IsInit = false;
		OfdStatusFullRead = false;
		if (IsReboot)
		{
			await RunCommand(65267u, AccessPassword, new MemoryStream());
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			await Task.Delay(10000);
			for (int i2 = 0; i2 < 20; i2++)
			{
				await Task.Delay(1000);
				OpenSerial = await PortOpenAsync();
				if (OpenSerial)
				{
					break;
				}
				if (i2 == 19)
				{
					Error = "Не удалось подключится к ККТ после регистрации.";
					return;
				}
			}
		}
		await ProcessInitDevice(FullInit: true);
		await ReadStatusOFD(Full: true);
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task GetDataKKT(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await ClearOldCheck();
		await ReadStatusOFD(Full: true);
		byte[] array = await RunCommand(16u, OperatorPasswor, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			LastError = Error;
			return;
		}
		Kkm.PaperOver = (array[3] & 0x80) == 0;
		switch (array[5] & 0xF)
		{
		case 2:
			SessionOpen = 2;
			break;
		case 3:
			SessionOpen = 3;
			break;
		default:
			SessionOpen = 1;
			break;
		}
		if (Global.Settings.SetNotActiveOnPaperOver)
		{
			IsInit = !Kkm.PaperOver;
		}
		array = await RunCommand(17u, OperatorPasswor, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			LastError = Error;
			return;
		}
		Kkm.DateTimeKKT = new DateTime(2000 + array[27], array[26], array[25], array[28], array[29], 0);
		if (NewModel)
		{
			Kkm.FN_DateEnd = default(DateTime);
			byte[] array2 = await RunCommand(65283u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(null, array2, OpenSerial: false, ClearCheck: false, ""))
			{
				try
				{
					Kkm.FN_DateEnd = new DateTime(2000 + array2[3], array2[4], array2[5]);
				}
				catch
				{
				}
			}
			else
			{
				Error = "";
			}
		}
		if (NewModel)
		{
			Kkm.OFD_NumErrorDoc = 0;
			Kkm.OFD_DateErrorDoc = default(DateTime);
			byte[] array3 = await RunCommand(65337u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(null, array3, OpenSerial: false, ClearCheck: false, ""))
			{
				Kkm.OFD_NumErrorDoc = array3[5] + (array3[6] << 8);
				try
				{
					Kkm.OFD_DateErrorDoc = new DateTime(2000 + array3[11], array3[12], array3[13], array3[14], array3[15], 0);
				}
				catch
				{
				}
			}
			else
			{
				Error = "";
			}
		}
		await base.GetDataKKT(DataCommand, RezultCommand);
		await GetCheckAndSession(RezultCommand);
		RezultCommand.Info.SessionState = SessionOpen;
		byte[] array4 = await RunCommand(26u, AccessPassword, new MemoryStream(new byte[1] { 241 }));
		if (!IsCommandBad(null, array4, OpenSerial: false, ClearCheck: false, ""))
		{
			decimal num = NumberFromStream(array4, 3, 6);
			RezultCommand.Info.BalanceCash = num / 100m;
		}
		Error = "";
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task GetCounters(DataCommand DataCommand, RezultCounters RezultCommand)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		string[] array = "Total,Shift".Split(',');
		string[] ReceiptTypes = "Shell,ShellReturn,Buy,BuyReturn".Split(',');
		int[] NumReg = null;
		string[] array2 = array;
		foreach (string CountersType in array2)
		{
			string[] array3 = ReceiptTypes;
			foreach (string ReceiptType in array3)
			{
				new List<string>().Add("23");
				RezultCounters.tСounter Сounter = new RezultCounters.tСounter
				{
					CountersType = CountersType,
					ReceiptType = ReceiptType
				};
				switch (CountersType + ReceiptType)
				{
				case "TotalShell":
					NumReg = new int[26]
					{
						148, -1, -1, -1, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, 200, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, -1, -1, -1
					};
					break;
				case "TotalShellReturn":
					NumReg = new int[26]
					{
						149, -1, -1, -1, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, 204, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, -1, -1, -1
					};
					break;
				case "TotalBuy":
					NumReg = new int[26]
					{
						150, -1, -1, -1, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, 201, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, -1, -1, -1
					};
					break;
				case "TotalBuyReturn":
					NumReg = new int[26]
					{
						151, -1, -1, -1, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, 205, -1, -1, -1, -1, -1, -1,
						-1, -1, -1, -1, -1, -1
					};
					break;
				case "ShiftShell":
					NumReg = new int[26]
					{
						144, -1, 193, 197, 4180, 4184, 4188, 225, 229, 217,
						221, 4216, 4220, 202, 4224, 201, 205, 4144, 4148, 4152,
						4156, 4160, 4164, 4168, 4172, 4176
					};
					break;
				case "ShiftShellReturn":
					NumReg = new int[26]
					{
						145, -1, 195, 199, 4182, 4186, 4190, 227, 231, 219,
						223, 4218, 4222, 206, 4226, 203, 207, 4146, 4150, 4154,
						4158, 4162, 4166, 4170, 4174, 4178
					};
					break;
				case "ShiftBuy":
					NumReg = new int[26]
					{
						146, -1, 194, 198, 4181, 4185, 4189, 226, 230, 218,
						222, 4217, 4221, 202, 4225, 202, 206, 4145, 4149, 4153,
						4157, 4161, 4165, 4169, 4173, 4177
					};
					break;
				case "ShiftBuyReturn":
					NumReg = new int[26]
					{
						147, -1, 196, 200, 4183, 4187, 4191, 228, 232, 220,
						224, 4219, 4223, 207, 4227, 204, 208, 4147, 4151, 4155,
						4159, 4163, 4167, 4171, 4175, 4179
					};
					break;
				}
				for (int x = 0; x <= 25; x++)
				{
					if (NumReg[x] != -1)
					{
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter bw = new BinaryWriter(memoryStream);
						if (NumReg[x] <= 255)
						{
							NumberToStream(bw, NumReg[x], 1);
						}
						else
						{
							NumberToStream(bw, NumReg[x], 2);
						}
						byte[] array4 = ((x != 0 && x != 13) ? (await RunCommand(26u, OperatorPasswor, memoryStream)) : (await RunCommand(27u, OperatorPasswor, memoryStream)));
						if (IsCommandBad(RezultCommand, array4, OpenSerial, ClearCheck: false, "Не удалось получить счетчик"))
						{
							return;
						}
						switch (x)
						{
						case 0:
							Сounter.Count += (uint)NumberFromStream(array4, 3, 2);
							break;
						case 1:
							Сounter.Sum += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 2:
							Сounter.Cash += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 3:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 4:
							Сounter.AdvancePayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 5:
							Сounter.Credit += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 6:
							Сounter.CashProvision += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 7:
							Сounter.Tax22 += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 8:
							Сounter.Tax10 += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 9:
							Сounter.Tax0 += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 10:
							Сounter.TaxNo += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 11:
							Сounter.Tax122 += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 12:
							Сounter.Tax110 += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 13:
							Сounter.CorrectionsCount += (uint)NumberFromStream(array4, 3, 2);
							break;
						case 14:
							Сounter.CorrectionsSum = NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 15:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 16:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 17:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 18:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 19:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 20:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 21:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 22:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 23:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 24:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						case 25:
							Сounter.ElectronicPayment += NumberFromStream(array4, 3, 6) / 100m;
							break;
						}
					}
				}
				if (CountersType == "Total")
				{
					MemoryStream memoryStream2 = new MemoryStream();
					BinaryWriter bw2 = new BinaryWriter(memoryStream2);
					switch (ReceiptType)
					{
					case "Shell":
						NumberToStream(bw2, 1m, 1);
						break;
					case "ShellReturn":
						NumberToStream(bw2, 2m, 1);
						break;
					case "Buy":
						NumberToStream(bw2, 3m, 1);
						break;
					case "BuyReturn":
						NumberToStream(bw2, 4m, 1);
						break;
					}
					NumberToStream(bw2, 0m, 3);
					byte[] array4 = await RunCommand(65268u, null, memoryStream2);
					if (IsCommandBad(RezultCommand, array4, OpenSerial, ClearCheck: false, "Не удалось получить счетчик"))
					{
						return;
					}
					Сounter.Cash = NumberFromStream(array4, 2, 8) / 100m;
					Сounter.ElectronicPayment = NumberFromStream(array4, 10, 8) / 100m + NumberFromStream(array4, 18, 8) / 100m + NumberFromStream(array4, 26, 8) / 100m + NumberFromStream(array4, 34, 8) / 100m + NumberFromStream(array4, 42, 8) / 100m + NumberFromStream(array4, 50, 8) / 100m + NumberFromStream(array4, 58, 8) / 100m + NumberFromStream(array4, 66, 8) / 100m + NumberFromStream(array4, 74, 8) / 100m + NumberFromStream(array4, 82, 8) / 100m + NumberFromStream(array4, 90, 8) / 100m + NumberFromStream(array4, 98, 8) / 100m;
					Сounter.AdvancePayment = NumberFromStream(array4, 106, 8) / 100m;
					Сounter.Credit = NumberFromStream(array4, 114, 8) / 100m;
					Сounter.CashProvision = NumberFromStream(array4, 122, 8) / 100m;
					memoryStream2 = new MemoryStream();
					bw2 = new BinaryWriter(memoryStream2);
					NumberToStream(bw2, 5m, 1);
					NumberToStream(bw2, 0m, 3);
					array4 = await RunCommand(65268u, null, memoryStream2);
					if (IsCommandBad(RezultCommand, array4, OpenSerial, ClearCheck: false, "Не удалось получить счетчик"))
					{
						return;
					}
					int num = 0;
					switch (ReceiptType)
					{
					case "Shell":
						num = 0;
						break;
					case "ShellReturn":
						num = 1;
						break;
					case "Buy":
						num = 2;
						break;
					case "BuyReturn":
						num = 3;
						break;
					}
					Сounter.CorrectionsSum = NumberFromStream(array4, (byte)(2 + 8 * num), 8) / 100m;
				}
				Сounter.Sum = Сounter.Cash + Сounter.ElectronicPayment + Сounter.AdvancePayment + Сounter.Credit + Сounter.CashProvision;
				RezultCommand.Counters.Add(Сounter);
			}
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public static bool TestСommunication(string IP, string Port, string Com)
	{
		StrihM strihM = new StrihM(null, 0);
		strihM.AccessPassword = "30";
		strihM.OperatorPasswor = "30";
		if (IP != "")
		{
			strihM.SetPort.TypeConnect = SetPorts.enTypeConnect.IP;
			strihM.SetPort.IP = IP;
			strihM.SetPort.Port = Port;
		}
		else
		{
			strihM.SetPort.TypeConnect = SetPorts.enTypeConnect.Com;
			strihM.SetPort.ComId = Com;
		}
		bool result = false;
		bool result2 = strihM.PortOpenAsync().Result;
		if (strihM.Error != "")
		{
			return false;
		}
		byte[] result3 = strihM.RunCommand(16u, strihM.OperatorPasswor, new MemoryStream(), 200).Result;
		if (!strihM.IsCommandBad(null, null, OpenSerial: false, ClearCheck: false, "ККМ не подключена!"))
		{
			result = true;
		}
		else
		{
			strihM.Protocol = StrihM.enProtocol.v20;
			strihM.Error = "";
			_ = strihM.RunCommand(16u, strihM.OperatorPasswor, new MemoryStream()).Result;
			if (!strihM.IsCommandBad(null, result3, OpenSerial: false, ClearCheck: false, "ККМ не подключена!", Ignore55: true))
			{
				result = true;
			}
		}
		if (result2)
		{
			strihM.PortCloseAsync().Wait();
		}
		return result;
	}

	public override void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		if (DataCommand.AdditionalActions == "RestartKKT")
		{
			RestartKKT(DataCommand, RezultCommand).Wait();
		}
		else
		{
			base.DoAdditionalAction(DataCommand, ref RezultCommand);
		}
	}

	private async Task RestartKKT(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream();
		new BinaryWriter(memoryStream);
		if (!IsCommandBad(RezultCommand, await RunCommand(65267u, OperatorPasswor, memoryStream), OpenSerial, ClearCheck: false, "Не удалось перезагрузить ККТ"))
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task<bool> CloseDocumentAndOpenShift(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return false;
		}
		if (!(await WaitPrint(OpenSerial, RezultCommand)))
		{
			return false;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return false;
		}
		byte StatusDevice = await ClearOldCheck();
		await SetNotPrint(DataCommand);
		if (DataCommand.IsFiscalCheck)
		{
			await SerCashier(DataCommand, InTable: true);
		}
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.IsFiscalCheck && (StatusDevice & 0xF) == 3)
			{
				CreateTextError(22, Error);
				IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Не удалось зарегистрировать документ");
				return false;
			}
			if (DataCommand.IsFiscalCheck && (StatusDevice & 0xF) == 4 && IsCommandBad(RezultCommand, await RunCommand(224u, AccessPassword, new MemoryStream(), 60000), OpenSerial, ClearCheck: false, "Не удалось открыть смену", Ignore55: true))
			{
				return false;
			}
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Error = "";
		return true;
	}

	public override async Task ReadStatusOFD(bool Full = false, bool ReadInfoGer = false, bool NoInit = false)
	{
		if (!NewModel)
		{
			return;
		}
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(null, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		try
		{
			byte[] data = await RunCommand(65282u, AccessPassword, new MemoryStream());
			Kkm.Fn_Number = StringFromStream(data, 3);
			if (Full)
			{
				data = await RunCommand(17u, OperatorPasswor, new MemoryStream(), 500);
				if (!IsCommandBad(null, data, OpenSerial, ClearCheck: false, "Не удалось получить данные ККМ"))
				{
					DateTime dateTime = new DateTime(2000 + data[9], data[8], data[7]);
					if (dateTime >= new DateTime(2021, 7, 1))
					{
						Kkm.FfdSupportVersion = 4;
					}
					else if (dateTime >= new DateTime(2019, 7, 10))
					{
						Kkm.FfdSupportVersion = 3;
					}
					else if (dateTime >= new DateTime(2017, 2, 20))
					{
						Kkm.FfdSupportVersion = 2;
					}
					else
					{
						Kkm.FfdSupportVersion = 1;
					}
					Kkm.Firmware_Version = NumberFromStream(data, 5, 2) + " от " + dateTime.ToString("dd.MM.yyyy");
				}
				else
				{
					Error = "";
					Kkm.Firmware_Version = "<Не определено>";
				}
				byte b = (byte)(int)(await GetValueInTable(17, 1, 17, typeof(byte)));
				if (Error == "")
				{
					switch (b)
					{
					case 0:
						Kkm.FfdVersion = 1;
						break;
					case 1:
						Kkm.FfdVersion = 1;
						break;
					case 2:
						Kkm.FfdVersion = 2;
						break;
					case 3:
						Kkm.FfdVersion = 3;
						break;
					case 4:
						Kkm.FfdVersion = 4;
						break;
					}
				}
				else
				{
					Kkm.FfdVersion = 2;
					Error = "";
				}
				Kkm.TaxVariant = "";
				data = await RunCommand(65289u, AccessPassword, new MemoryStream());
				if (data.Length >= 40)
				{
					byte b2 = data[40];
					for (int i = 0; i <= 5; i++)
					{
						if (((b2 >> i) & 1) == 1)
						{
							if (Kkm.TaxVariant != "")
							{
								Kkm.TaxVariant += ",";
							}
							Kkm.TaxVariant += i;
						}
					}
				}
				else
				{
					Kkm.TaxVariant = "";
				}
				SetKkm kkm = Kkm;
				kkm.UrlServerOfd = ((string)(await GetValueInTable(19, 1, 1, typeof(string)))).Trim();
				byte[] array = (byte[])(await GetValueInTable(19, 1, 2, typeof(byte[]), 2));
				if (array.Length >= 3)
				{
					Kkm.PortServerOfd = ((array[3] << 8) + array[2]).ToString();
				}
				else
				{
					Kkm.PortServerOfd = "";
				}
				kkm = Kkm;
				kkm.RegNumber = ((string)(await GetValueInTable(18, 1, 3, typeof(string)))).Trim();
				kkm = Kkm;
				kkm.InnOfd = ((string)(await GetValueInTable(18, 1, 12, typeof(string)))).Trim();
				kkm = Kkm;
				kkm.NameOFD = ((string)(await GetValueInTable(18, 1, 10, typeof(string)))).Trim();
				kkm = Kkm;
				kkm.UrlOfd = ((string)(await GetValueInTable(18, 1, 11, typeof(string)))).Trim();
				Error = "";
				kkm = Kkm;
				kkm.AddressSettle = ((string)(await GetValueInTable(18, 1, 9, typeof(string)))).Trim();
				if (Kkm.FfdVersion >= 2)
				{
					kkm = Kkm;
					kkm.PlaceSettle = ((string)(await GetValueInTable(18, 1, 14, typeof(string)))).Trim();
					Error = "";
					kkm = Kkm;
					kkm.SenderEmail = ((string)(await GetValueInTable(18, 1, 15, typeof(string)))).Trim();
					Error = "";
					byte b3 = (byte)(int)(await GetValueInTable(18, 1, 16, typeof(byte)));
					Kkm.SignOfAgent = "";
					for (int j = 0; j <= 6; j++)
					{
						if (((b3 >> j) & 1) == 1)
						{
							if (Kkm.SignOfAgent != "")
							{
								Kkm.SignOfAgent += ",";
							}
							Kkm.SignOfAgent += j;
						}
					}
					Error = "";
				}
				else
				{
					Kkm.PlaceSettle = "";
					Kkm.SenderEmail = "";
					Kkm.SignOfAgent = "";
				}
				byte b4 = (byte)(int)(await GetValueInTable(18, 1, 6, typeof(byte), 1));
				Kkm.EncryptionMode = (b4 & 1) == 1;
				Kkm.OfflineMode = (b4 & 2) == 2;
				Kkm.AutomaticMode = (b4 & 4) == 4;
				Kkm.InternetMode = (b4 & 0x20) == 32;
				Kkm.BSOMode = (b4 & 0x10) == 16;
				Kkm.ServiceMode = (b4 & 8) == 8;
				if (Kkm.FfdVersion >= 2)
				{
					b4 = (byte)(int)(await GetValueInTable(18, 1, 21, typeof(byte), 1));
					Kkm.SaleExcisableGoods = (b4 & 1) == 1;
					Kkm.SignOfGambling = (b4 & 2) == 2;
					Kkm.SignOfLottery = (b4 & 4) == 4;
					Kkm.PrinterAutomatic = (b4 & 8) == 8;
					Kkm.SaleMarking = (b4 & 0x10) == 16;
					Kkm.SignPawnshop = (b4 & 0x20) == 32;
					Kkm.SignAssurance = (b4 & 0x40) == 64;
					Error = "";
				}
				kkm = Kkm;
				kkm.AutomaticNumber = ((string)(await GetValueInTable(24, 1, 1, typeof(string)))).Trim();
				Error = "";
				kkm = Kkm;
				kkm.AutomaticNumber = ((string)(await GetValueInTable(24, 1, 1, typeof(string)))).Trim();
				Error = "";
				byte[] array2 = await RunCommand(65281u, AccessPassword, new MemoryStream());
				if (!IsCommandBad(null, array2, OpenSerial: false, ClearCheck: false, ""))
				{
					Kkm.FN_Status = (byte)(array2[3] & 0xF);
					Kkm.FN_MemOverflowl = (array2[7] & 4) != 0;
				}
				else
				{
					Kkm.FN_Status = 0;
					Error = "";
				}
				IsInitOfd = true;
			}
			Kkm.FN_IsFiscal = false;
			byte[] array3 = await RunCommand(65281u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(null, array3, OpenSerial: false, ClearCheck: false, ""))
			{
				if (array3[3] == 3)
				{
					Kkm.FN_IsFiscal = true;
				}
			}
			else
			{
				Error = "";
			}
			Kkm.FN_DateEnd = default(DateTime);
			array3 = await RunCommand(65283u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(null, array3, OpenSerial: false, ClearCheck: false, ""))
			{
				try
				{
					Kkm.FN_DateEnd = new DateTime(2000 + array3[3], array3[4], array3[5]);
				}
				catch
				{
				}
			}
			else
			{
				Error = "";
			}
			Kkm.OFD_NumErrorDoc = 0;
			Kkm.OFD_DateErrorDoc = default(DateTime);
			array3 = await RunCommand(65337u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(null, array3, OpenSerial: false, ClearCheck: false, ""))
			{
				Kkm.OFD_NumErrorDoc = array3[5] + (array3[6] << 8);
				try
				{
					Kkm.OFD_DateErrorDoc = new DateTime(2000 + array3[11], array3[12], array3[13], array3[14], array3[15], 0);
				}
				catch
				{
				}
			}
			else
			{
				Error = "";
			}
			if (ReadInfoGer)
			{
				try
				{
					array3 = await RunCommand(65289u, AccessPassword, new MemoryStream());
					DateTime dateTime2 = new DateTime(2000 + array3[3], array3[4], array3[5], array3[6], array3[7], 0);
					uint num = (uint)(array3[43] + (array3[44] << 8) + (array3[45] << 16) + (array3[46] << 24));
					uint num2 = (uint)(array3[47] + (array3[48] << 8) + (array3[49] << 16) + (array3[50] << 24));
					Error = "";
					Kkm.InfoRegKkt = "Дата: " + dateTime2.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + num.ToString("D0") + ", ФПД: " + num2.ToString("D0");
					Kkm.FN_DateStart = dateTime2.Date;
				}
				catch (Exception)
				{
					Kkm.InfoRegKkt = "Ошибка чтения параметров регистрации";
				}
			}
			OfdStatusFullRead = Full;
		}
		catch
		{
			OfdStatusFullRead = false;
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
	}

	public override async Task GetCheckAndSession(RezultCommandKKm RezultCommand, bool IsSessionNumber = true, bool IsCheckNumber = true)
	{
		string NumberCheck = "0";
		int SessionNumber = 0;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		if (NewModel)
		{
			byte[] array = await RunCommand(65281u, OperatorPasswor, memoryStream);
			if (!IsCommandBad(RezultCommand, array, OpenSerial: false, ClearCheck: false, "Не удалось получить данные чека"))
			{
				NumberCheck = StringNumberFromStreamg(array, 29, 4, 4);
			}
		}
		else
		{
			binaryWriter.Write((byte)148);
			byte[] array2 = await RunCommand(27u, OperatorPasswor, memoryStream);
			if (!IsCommandBad(RezultCommand, array2, OpenSerial: false, ClearCheck: false, "Не удалось получить данные чека"))
			{
				NumberCheck = StringNumberFromStreamg(array2, 3, 2, 4);
			}
		}
		if (NewModel)
		{
			byte[] array3 = await RunCommand(65344u, AccessPassword, new MemoryStream());
			if (!IsCommandBad(RezultCommand, array3, OpenSerial: false, ClearCheck: false, "Не удалось получить данные чека"))
			{
				SessionNumber = array3[4] + (array3[5] << 8);
			}
		}
		if (IsSessionNumber)
		{
			RezultCommand.SessionNumber = SessionNumber;
		}
		if (IsCheckNumber)
		{
			RezultCommand.CheckNumber = int.Parse(NumberCheck);
		}
	}

	public async Task SerCashier(DataCommand DataCommand, bool InTable = false)
	{
		if (InTable && NewModel && DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			await SetValueInTable(2, NumLineCashier, 2, DataCommand.CashierName, 21);
			Error = "";
		}
		if (!InTable && NewModel && DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			await WriteProp(Print: true, 1021, DataCommand.CashierName);
			Error = "";
		}
		if (!InTable && NewModel && Kkm.FfdVersion >= 2 && DataCommand.CashierVATIN != null && DataCommand.CashierVATIN != "")
		{
			await WriteProp(Print: true, 1203, DataCommand.CashierVATIN);
			Error = "";
		}
	}

	public async Task<bool> SetAgentData(DataCommand.TypeAgentData AgentData, bool OpenSerial)
	{
		if (AgentData == null)
		{
			return true;
		}
		for (int i = 1; i <= 7; i++)
		{
			string text = "";
			int num = 0;
			switch (i)
			{
			case 1:
				try
				{
					text = AgentData.PayingAgentOperation;
				}
				catch
				{
				}
				num = 1044;
				break;
			case 2:
				try
				{
					text = AgentData.PayingAgentPhone;
				}
				catch
				{
				}
				num = 1073;
				break;
			case 3:
				try
				{
					text = AgentData.ReceivePaymentsOperatorPhone;
				}
				catch
				{
				}
				num = 1074;
				break;
			case 4:
				try
				{
					text = AgentData.MoneyTransferOperatorPhone;
				}
				catch
				{
				}
				num = 1075;
				break;
			case 5:
				try
				{
					text = AgentData.MoneyTransferOperatorName;
				}
				catch
				{
				}
				num = 1026;
				break;
			case 6:
				try
				{
					text = AgentData.MoneyTransferOperatorAddress;
				}
				catch
				{
				}
				num = 1005;
				break;
			case 7:
				try
				{
					text = AgentData.MoneyTransferOperatorVATIN.PadRight(12, ' ');
				}
				catch
				{
				}
				num = 1016;
				break;
			}
			if (num != 0 && text != "")
			{
				await WriteProp(Print: false, num, text);
				if (IsCommandBad(null, null, OpenSerial, ClearCheck: true, "Не удалось установить Данные агента чека"))
				{
					return false;
				}
			}
		}
		return true;
	}

	public async Task<bool> SetPurveyorData(DataCommand.TypePurveyorData PurveyorData, bool OpenSerial, bool Head)
	{
		for (int i = 1; i <= 3; i++)
		{
			string text = "";
			int num = 0;
			if (i == 1)
			{
				try
				{
					text = PurveyorData.PurveyorPhone;
				}
				catch
				{
				}
				num = (Head ? 1171 : 0);
			}
			if (num != 0 && text != null && text != "")
			{
				await WriteProp(Print: false, num, text);
				if (IsCommandBad(null, null, OpenSerial, ClearCheck: true, "Не удалось установить Данные поставщика чека"))
				{
					return false;
				}
			}
		}
		return true;
	}

	public async Task<object> GetValueInTable(int Table, int Row, int Col, Type Type, int Lenght = 0)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)Table);
		binaryWriter.Write((short)Row);
		binaryWriter.Write((byte)Col);
		byte[] array = await RunCommand(31u, AccessPassword, memoryStream);
		if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Не удалось прочитать таблицу "))
		{
			if (Type == typeof(byte))
			{
				return (int)NumberFromStream(array, 2, 1);
			}
			if (Type == typeof(int))
			{
				return (int)NumberFromStream(array, 2, 2);
			}
			if (Type == typeof(string))
			{
				return StringFromStream(array, 2, 0, NulTerminate: true);
			}
			if (Type == typeof(byte[]))
			{
				return array;
			}
		}
		if (Type == typeof(byte))
		{
			return 0;
		}
		if (Type == typeof(int))
		{
			return 0;
		}
		if (Type == typeof(string))
		{
			return "";
		}
		if (Type == typeof(byte[]))
		{
			return new byte[0];
		}
		return null;
	}

	public async Task<bool> SetValueInTable(int Table, int Row, int Col, object Value, int Lenght = 0)
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(ms);
		binaryWriter.Write((byte)Table);
		binaryWriter.Write((byte)Col);
		byte[] array = await RunCommand(46u, AccessPassword, ms);
		if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка программирования ККМ"))
		{
			Lenght = array[43];
		}
		Error = "";
		ms = new MemoryStream();
		binaryWriter = new BinaryWriter(ms);
		binaryWriter.Write((byte)Table);
		binaryWriter.Write((short)Row);
		binaryWriter.Write((byte)Col);
		if (Value.GetType() == typeof(byte))
		{
			binaryWriter.Write((byte)Value);
		}
		else if (Value.GetType() == typeof(int))
		{
			binaryWriter.Write((int)Value);
		}
		else if (Value.GetType() == typeof(string))
		{
			StringToStream(binaryWriter, (string)Value, Lenght, 0);
		}
		else if (Value.GetType() == typeof(byte[]))
		{
			binaryWriter.Write((byte[])Value);
		}
		for (int i = 0; i < 10; i++)
		{
			Error = "";
			array = await RunCommand(30u, AccessPassword, ms);
			if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка записи таблицы"))
			{
				break;
			}
			if (array.Length > 2 && array[1] != 80)
			{
				return false;
			}
			await Task.Delay(100);
		}
		return true;
	}

	public int MaxLenghtProp(int Teg, ref object Prop)
	{
		int result = -1;
		if (Prop.GetType() == typeof(string))
		{
			switch (Teg)
			{
			case 1005:
				result = 100;
				break;
			case 1008:
				result = 64;
				break;
			case 1009:
				result = 255;
				break;
			case 1016:
				result = 12;
				break;
			case 1021:
				result = 64;
				break;
			case 1026:
				result = 64;
				break;
			case 1036:
				result = 12;
				break;
			case 1044:
				result = 24;
				break;
			case 1045:
				result = 24;
				break;
			case 1046:
				result = 255;
				break;
			case 1060:
				result = 255;
				break;
			case 1073:
				result = 19;
				break;
			case 1074:
				result = 19;
				break;
			case 1075:
				result = 19;
				break;
			case 1082:
				result = 19;
				break;
			case 1083:
				result = 19;
				break;
			case 1085:
				result = 64;
				break;
			case 1086:
				result = 256;
				break;
			case 1117:
				result = 64;
				break;
			case 1119:
				result = 19;
				break;
			case 1162:
				result = 32;
				break;
			case 1171:
				result = 19;
				break;
			case 1179:
				result = 32;
				break;
			case 1187:
				result = 200;
				break;
			case 1191:
				result = 65;
				break;
			case 1192:
				result = 16;
				break;
			case 1197:
				result = 16;
				break;
			case 1203:
				result = 12;
				break;
			case 1225:
				result = 255;
				break;
			case 1226:
				result = 12;
				break;
			case 1227:
				result = 255;
				break;
			case 1228:
				result = 12;
				break;
			case 1229:
				result = 6;
				break;
			case 1230:
				result = 3;
				break;
			case 1231:
				result = 32;
				break;
			case 1262:
				result = 3;
				break;
			case 1263:
				result = 10;
				break;
			case 1264:
				result = 32;
				break;
			case 1265:
				result = 256;
				break;
			}
		}
		else if (Prop.GetType() == typeof(int))
		{
			result = 4;
		}
		else if (Prop.GetType() == typeof(int))
		{
			result = 4;
		}
		else if (Prop.GetType() == typeof(short))
		{
			result = 2;
		}
		else if (Prop.GetType() == typeof(double))
		{
			result = 8;
			Prop = (long)((double)Prop * 100.0);
		}
		else if (Prop.GetType() == typeof(decimal))
		{
			result = 8;
			Prop = (long)((decimal)Prop * 100m);
		}
		else if (Prop.GetType() == typeof(byte))
		{
			result = 1;
		}
		else if (Prop.GetType() == typeof(byte[]) && Teg == 1162)
		{
			result = 32;
		}
		return result;
	}

	public async Task WriteProp(bool Print, int Teg, object Prop, int MaxLenght = 0, int Fill = -1, bool InFicalString = false)
	{
		if (Prop == null)
		{
			return;
		}
		if (MaxLenght == 0)
		{
			MaxLenght = MaxLenghtProp(Teg, ref Prop);
		}
		if ((MaxLenght == -1 && IsCommandBad(null, null, OpenSerial: false, ClearCheck: false, "Попытка записать не разрешенный тег")) || !NewModel)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)(Teg & 0xFF));
		binaryWriter.Write((byte)(Teg >> 8));
		if (Prop.GetType() == typeof(string))
		{
			if (MaxLenght == 0)
			{
				MaxLenght = ((string)Prop).Length;
			}
			if (((string)Prop).Length < MaxLenght)
			{
				MaxLenght = ((string)Prop).Length;
			}
			if (((string)Prop).Length > MaxLenght)
			{
				Prop = ((string)Prop).Substring(0, MaxLenght);
			}
			binaryWriter.Write((byte)(((string)Prop).Length & 0xFF));
			binaryWriter.Write((byte)(((string)Prop).Length >> 8));
			StringToStream(binaryWriter, (string)Prop, MaxLenght, (byte)Fill, use866: true);
		}
		else if (Prop.GetType() == typeof(int))
		{
			binaryWriter.Write((byte)(MaxLenght & 0xFF));
			binaryWriter.Write((byte)(MaxLenght >> 8));
			int num = (int)Prop;
			for (int i = 0; i < MaxLenght; i++)
			{
				binaryWriter.Write((byte)(num & 0xFF));
				num >>= 8;
			}
		}
		else if (Prop.GetType() == typeof(byte))
		{
			binaryWriter.Write((byte)1);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)Prop);
		}
		else if (Prop.GetType() == typeof(byte[]))
		{
			if (MaxLenght == 0)
			{
				MaxLenght = ((byte[])Prop).Length;
			}
			if (((byte[])Prop).Length < MaxLenght)
			{
				MaxLenght = ((byte[])Prop).Length;
			}
			if (((byte[])Prop).Length > MaxLenght)
			{
				byte[] array = (byte[])Prop;
				Array.Resize(ref array, MaxLenght);
				Prop = array;
			}
			binaryWriter.Write((byte)(MaxLenght & 0xFF));
			binaryWriter.Write((byte)(MaxLenght >> 8));
			binaryWriter.Write((byte[])Prop);
		}
		memoryStream.ToArray();
		byte[] buffer = (InFicalString ? (await RunCommand(65357u, AccessPassword, memoryStream)) : (await RunCommand(65292u, AccessPassword, memoryStream)));
		IsCommandBad(null, buffer, OpenSerial: false, ClearCheck: false, "Ошибка записи реквизита ОФД: ");
	}

	public async Task WriteArrProp(bool Print, int Teg, Dictionary<int, object> ArrProp, int Fill = 0, bool InFicalString = false)
	{
		if (!NewModel)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (KeyValuePair<int, object> item in ArrProp)
		{
			object Prop = item.Value;
			int num = MaxLenghtProp(item.Key, ref Prop);
			if (num == -1 && IsCommandBad(null, null, OpenSerial: false, ClearCheck: false, "Попытка записать не разрешенный тег"))
			{
				return;
			}
			binaryWriter.Write((byte)(item.Key & 0xFF));
			binaryWriter.Write((byte)(item.Key >> 8));
			if (item.Value.GetType() == typeof(string))
			{
				if (num == 0)
				{
					num = ((string)Prop).Length;
				}
				if (((string)Prop).Length < num)
				{
					num = ((string)Prop).Length;
				}
				if (((string)Prop).Length > num)
				{
					Prop = ((string)Prop).Substring(0, num);
				}
				binaryWriter.Write((byte)(num & 0xFF));
				binaryWriter.Write((byte)(num >> 8));
				StringToStream(binaryWriter, (string)Prop, num, (byte)Fill, use866: true);
			}
			else if (Prop.GetType() == typeof(int))
			{
				binaryWriter.Write((byte)(num & 0xFF));
				binaryWriter.Write((byte)(num >> 8));
				int num2 = (int)Prop;
				for (int i = 0; i < num; i++)
				{
					binaryWriter.Write((byte)(num2 & 0xFF));
					num2 >>= 8;
				}
			}
			else if (Prop.GetType() == typeof(int))
			{
				binaryWriter.Write((byte)(num & 0xFF));
				binaryWriter.Write((byte)(num >> 8));
				int num3 = (int)Prop;
				for (int j = 0; j < num; j++)
				{
					binaryWriter.Write((byte)(num3 & 0xFF));
					num3 >>= 8;
				}
			}
			else if (Prop.GetType() == typeof(byte))
			{
				binaryWriter.Write((byte)1);
				binaryWriter.Write((byte)0);
				binaryWriter.Write((byte)Prop);
			}
		}
		int num4 = (int)memoryStream.Length;
		MemoryStream memoryStream2 = new MemoryStream();
		BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2);
		binaryWriter2.Write((byte)(Teg & 0xFF));
		binaryWriter2.Write((byte)(Teg >> 8));
		binaryWriter2.Write((byte)(num4 & 0xFF));
		binaryWriter2.Write((byte)(num4 >> 8));
		binaryWriter2.Write(memoryStream.ToArray());
		memoryStream2.ToArray();
		byte[] buffer = (InFicalString ? (await RunCommand(65357u, AccessPassword, memoryStream2)) : (await RunCommand(65292u, AccessPassword, memoryStream2)));
		IsCommandBad(null, buffer, OpenSerial: false, ClearCheck: false, "Ошибка записи реквизита ФН: ");
	}

	public async Task WriteAdditionaProp(bool Print, string NameProp, string Prop)
	{
		if (Prop != null && NewModel)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			_ = Print;
			int num = ((NameProp.Length > 64) ? 64 : NameProp.Length);
			int num2 = ((Prop.Length > 256) ? 256 : Prop.Length);
			int num3 = num + num2 + 8;
			binaryWriter.Write((byte)60);
			binaryWriter.Write((byte)4);
			binaryWriter.Write((byte)(num3 & 0xFF));
			binaryWriter.Write((byte)(num3 >> 8));
			binaryWriter.Write((byte)61);
			binaryWriter.Write((byte)4);
			binaryWriter.Write((byte)(num & 0xFF));
			binaryWriter.Write((byte)(num >> 8));
			StringToStream(binaryWriter, NameProp, num, 32, use866: true);
			binaryWriter.Write((byte)62);
			binaryWriter.Write((byte)4);
			binaryWriter.Write((byte)(num2 & 0xFF));
			binaryWriter.Write((byte)(num2 >> 8));
			StringToStream(binaryWriter, Prop, num2, 32, use866: true);
			memoryStream.ToArray();
			IsCommandBad(null, await RunCommand(65292u, AccessPassword, memoryStream), OpenSerial: false, ClearCheck: false, "Ошибка записи реквизита ОФД: ");
		}
	}

	public async Task<bool> WaitPrint(bool OpenSerial, RezultCommand Rezult)
	{
		await ClearOldCheck();
		Error = "";
		int RejimFR = 0;
		int PodRejimFR = 0;
		for (int i = 0; i < 50; i++)
		{
			byte[] array = await RunCommand(16u, OperatorPasswor, new MemoryStream());
			if (IsCommandBad(Rezult, array, OpenSerial, ClearCheck: false, "Не удалось получить данные ККМ"))
			{
				Rezult.Error = Error;
				return false;
			}
			RejimFR = array[5] & 0xF;
			PodRejimFR = array[6] & 0xF;
			switch (PodRejimFR)
			{
			case 3:
				await RunCommand(176u, OperatorPasswor, new MemoryStream());
				Error = "";
				continue;
			case 4:
			case 5:
				await Task.Delay(100);
				continue;
			}
			break;
		}
		switch (PodRejimFR)
		{
		case 1:
			Error = "Отсуствует бумага";
			break;
		case 2:
			Error = "Отсуствует бумага. Не закончена печать предыдущего чека. Вставьте бумагу и нажмите на кнопку.";
			break;
		case 3:
			Error = "Не закончена выдача предыдущих данных. (Нажмите кнопку на ККТ для печати предыдущего чека)";
			break;
		default:
			if (RejimFR == 8)
			{
				Error = "Не закончено формирование предыдущего чека.";
				break;
			}
			return true;
		}
		IsCommandBad(null, null, OpenSerial, ClearCheck: false, "");
		return false;
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

	public void NumberToStream(BinaryWriter bw, decimal Number, byte CountByte, bool InventOrderByte = false)
	{
		MemoryStream memoryStream = new MemoryStream();
		new BinaryWriter(memoryStream).Write((long)Number);
		byte[] array = memoryStream.ToArray();
		for (int i = 0; i < CountByte; i++)
		{
			if (!InventOrderByte)
			{
				bw.Write(array[i]);
			}
			else
			{
				bw.Write(array[CountByte - 1 - i]);
			}
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

	public void FVLNWrite(BinaryWriter bw, decimal Value)
	{
		uint num = (uint)Math.Truncate(Value);
		uint num2 = (uint)(Value - (decimal)num);
		int num3 = 0;
		if (num2 != 0)
		{
			bw.Write((byte)1);
			num3++;
			bw.Write((ushort)num2);
			num3 += 2;
		}
		else
		{
			bw.Write((byte)0);
			num3++;
		}
		while (num != 0 && num3 < 8)
		{
			bw.Write((byte)(num & 0xFF));
			num >>= 8;
			num3++;
		}
		for (; num3 < 8; num3++)
		{
			bw.Write((byte)0);
		}
	}

	public void VLNWrite(BinaryWriter bw, uint Value)
	{
		while (Value != 0)
		{
			bw.Write((byte)(Value & 0xFF));
			Value >>= 8;
		}
	}

	public async Task<bool> PrintBarCode(DataCommand.PrintBarcode PrintBarCode)
	{
		if (PrintBarCode.BarcodeType != "EAN13" && PrintBarCode.BarcodeType != "QR" && PrintBarCode.BarcodeType != "PDF417")
		{
			return await PrintBarCodeGR(PrintBarCode);
		}
		byte[] bData = null;
		try
		{
			if (PrintBarCode.BarcodeType == "EAN13")
			{
				MemoryStream memoryStream = new MemoryStream();
				BinaryWriter bw = new BinaryWriter(memoryStream);
				NumberToStream(bw, long.Parse(PrintBarCode.Barcode.Substring(0, 12)), 5);
				bData = await RunCommand(194u, OperatorPasswor, memoryStream);
				if (bData != null && bData.Length > 1 && bData[1] == 55)
				{
					Error = "";
					return await PrintBarCodeGR(PrintBarCode);
				}
				if (IsCommandBad(null, bData, OpenSerial: false, ClearCheck: false, "Ошибка печати ШК EAN13: "))
				{
					return false;
				}
			}
			else
			{
				if (PrintBarCode.BarcodeType == "PDF417")
				{
					return await PrintBarCodeGR(PrintBarCode);
				}
				if (PrintBarCode.BarcodeType == "QR")
				{
					string OstSt = PrintBarCode.Barcode;
					int NumBlock = NumBlockBarCode;
					while (OstSt.Length > 0)
					{
						string value;
						if (OstSt.Length > 64)
						{
							value = OstSt.Substring(0, 64);
							OstSt = OstSt.Substring(64);
						}
						else
						{
							value = OstSt;
							OstSt = "";
						}
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter bw = new BinaryWriter(memoryStream);
						bw.Write((byte)0);
						bw.Write((byte)NumBlockBarCode);
						StringToStream(bw, value, 64, 0);
						bData = await RunCommand(221u, OperatorPasswor, memoryStream);
						if (bData != null && bData.Length > 1 && bData[1] == 55)
						{
							Error = "";
							return await PrintBarCodeGR(PrintBarCode);
						}
						if (IsCommandBad(null, bData, OpenSerial: false, ClearCheck: false, "Ошибка печати ШК QR: "))
						{
							return false;
						}
						NumBlockBarCode++;
					}
					if (PrintBarCode.BarcodeType == "QR")
					{
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter bw = new BinaryWriter(memoryStream);
						bw.Write((byte)3);
						bw.Write((short)PrintBarCode.Barcode.Length);
						bw.Write((byte)NumBlock);
						bw.Write((byte)0);
						bw.Write((byte)0);
						bw.Write((byte)5);
						bw.Write((byte)0);
						bw.Write((byte)1);
						bw.Write((byte)1);
						bData = await RunCommand(222u, OperatorPasswor, memoryStream);
						if (bData != null && bData.Length > 1 && bData[1] == 55)
						{
							Error = "";
							return await PrintBarCodeGR(PrintBarCode);
						}
						if (IsCommandBad(null, bData, OpenSerial: false, ClearCheck: false, "Ошибка печати ШК QR: "))
						{
							return false;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (bData == null || bData.Length <= 1 || bData[1] != 90)
			{
				Error = "Ошибка печати ШК: " + ex.Message;
				return false;
			}
			await PrintBarCodeGR(PrintBarCode);
		}
		return true;
	}

	public async Task<bool> PrintBarCodeGR(DataCommand.PrintBarcode PrintBarCode)
	{
		if (PrintBarCode.BarcodeType != null && PrintBarCode.BarcodeType != "")
		{
			ImageBarCode ImageBarCode = null;
			switch (PrintBarCode.BarcodeType.ToUpper())
			{
			case "EAN13":
				ImageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 80);
				break;
			case "CODE39":
				ImageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 80);
				break;
			case "CODE128":
				ImageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 80);
				break;
			case "PDF417":
				ImageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 320, 140);
				break;
			case "QR":
				ImageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, 200, 200);
				break;
			}
			if (ImageBarCode != null)
			{
				int WidthByte = ImageBarCode.Width / 8;
				MemoryStream memoryStream;
				BinaryWriter binaryWriter;
				for (int y = 0; y < ImageBarCode.Height; y++)
				{
					memoryStream = new MemoryStream();
					binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write((byte)((y + NumSrtGr) & 0xFF));
					binaryWriter.Write((byte)(y + NumSrtGr >> 8));
					int num = 40 - WidthByte;
					for (int i = 0; i < num / 2; i++)
					{
						binaryWriter.Write((byte)0);
					}
					for (int j = 0; j < WidthByte; j++)
					{
						binaryWriter.Write(ImageBarCode.Pixels[y * WidthByte + j]);
					}
					for (int k = 0; k < num - num / 2; k++)
					{
						binaryWriter.Write((byte)0);
					}
					if (IsCommandBad(null, await RunCommand(196u, OperatorPasswor, memoryStream), OpenSerial: false, ClearCheck: true, "Не удалось загрузить графику"))
					{
						return false;
					}
				}
				memoryStream = new MemoryStream();
				binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write((byte)3);
				byte[] array = new byte[40];
				for (int l = 0; l < 40; l++)
				{
					array[l] = 32;
				}
				binaryWriter.Write(array);
				if (IsCommandBad(null, await RunCommand(23u, OperatorPasswor, memoryStream), OpenSerial: false, ClearCheck: true, "Не удалось напечатать не фискальную строку"))
				{
					return false;
				}
				memoryStream = new MemoryStream();
				binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write((byte)((NumSrtGr + 1) & 0xFF));
				binaryWriter.Write((byte)(NumSrtGr + 1 >> 8));
				binaryWriter.Write((byte)((ImageBarCode.Height + NumSrtGr + 1) & 0xFF));
				binaryWriter.Write((byte)(ImageBarCode.Height + NumSrtGr + 1 >> 8));
				binaryWriter.Write((byte)0);
				if (IsCommandBad(null, await RunCommand(195u, OperatorPasswor, memoryStream), OpenSerial: false, ClearCheck: true, "Не удалось напечатать графику"))
				{
					PortLogs.Append(Error);
					Error = "";
				}
				NumSrtGr += ImageBarCode.Height;
			}
		}
		return true;
	}

	public async Task<bool> PrintImage(DataCommand.PrintImage PrintImage)
	{
		Image<Rgba32> Image = BarCode.ImageFromBase64(PrintImage.Image);
		int WidthByte = Image.Width / 8;
		MemoryStream memoryStream;
		BinaryWriter binaryWriter;
		for (int y = 0; y < Image.Height; y++)
		{
			memoryStream = new MemoryStream();
			binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write((byte)((y + NumSrtGr) & 0xFF));
			binaryWriter.Write((byte)(y + NumSrtGr >> 8));
			int num = 40 - WidthByte;
			for (int i = 0; i < num / 2; i++)
			{
				binaryWriter.Write((byte)0);
			}
			for (int j = 0; j < WidthByte; j++)
			{
				int num2 = 0;
				for (int k = 0; k < 8; k++)
				{
					int num3 = ((!((double)BarCode.GetBrightness(Image[j * 8 + k, y]) > 0.5)) ? 1 : 0);
					num2 += (byte)(num3 << k);
				}
				binaryWriter.Write((byte)num2);
			}
			for (int l = 0; l < num - num / 2; l++)
			{
				binaryWriter.Write((byte)0);
			}
			if (IsCommandBad(null, await RunCommand(196u, OperatorPasswor, memoryStream), OpenSerial: false, ClearCheck: true, "Не удалось загрузить графику"))
			{
				PortLogs.Append(Error);
				Error = "";
			}
		}
		memoryStream = new MemoryStream();
		binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)2);
		binaryWriter.Write((byte)1);
		StringToStream(binaryWriter, "", (Kkm.PrintingWidth <= 40) ? 40 : Kkm.PrintingWidth, 32);
		if (IsCommandBad(null, await RunCommand(47u, OperatorPasswor, memoryStream), OpenSerial: false, ClearCheck: true, "Не удалось сделать протяжку"))
		{
			return false;
		}
		memoryStream = new MemoryStream();
		binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)((NumSrtGr + 1) & 0xFF));
		binaryWriter.Write((byte)(NumSrtGr + 1 >> 8));
		binaryWriter.Write((byte)((Image.Height + NumSrtGr + 1) & 0xFF));
		binaryWriter.Write((byte)(Image.Height + NumSrtGr + 1 >> 8));
		binaryWriter.Write((byte)0);
		if (IsCommandBad(null, await RunCommand(195u, OperatorPasswor, memoryStream), OpenSerial: false, ClearCheck: true, "Не удалось напечатать графику"))
		{
			PortLogs.Append(Error);
			Error = "";
		}
		NumSrtGr += Image.Height;
		return true;
	}

	public async Task<byte> ClearOldCheck()
	{
		byte StatusDevice = 0;
		bool IsErr11 = false;
		while (true)
		{
			byte[] array = await RunCommand(17u, OperatorPasswor, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Не удалось получить статус ККМ"))
			{
				return StatusDevice;
			}
			StatusDevice = (byte)NumberFromStream(array, 15, 1);
			if (array[16] != 3 || IsErr11)
			{
				break;
			}
			await RunCommand(176u, OperatorPasswor, new MemoryStream());
			Error = "";
			IsErr11 = true;
		}
		if ((StatusDevice & 0xF) == 8)
		{
			if (IsCommandBad(null, await RunCommand(136u, OperatorPasswor, new MemoryStream()), OpenSerial: false, ClearCheck: false, "Не удалось отменить предыдущий чек", Ignore55: true))
			{
				return StatusDevice;
			}
			byte[] array = await RunCommand(17u, OperatorPasswor, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Не удалось получить статус ККМ"))
			{
				return StatusDevice;
			}
			StatusDevice = (byte)NumberFromStream(array, 15, 1);
		}
		if ((StatusDevice & 0xF) == 6)
		{
			DateTime Now = DateTime.Now;
			if (!IsCommandBad(null, await RunCommand(34u, AccessPassword, new MemoryStream(new byte[3]
			{
				(byte)Now.Day,
				(byte)Now.Month,
				(byte)(Now.Year - 2000)
			})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени"))
			{
				Error = "";
				IsCommandBad(null, await RunCommand(35u, AccessPassword, new MemoryStream(new byte[3]
				{
					(byte)Now.Day,
					(byte)Now.Month,
					(byte)(Now.Year - 2000)
				})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
				Error = "";
				IsCommandBad(null, await RunCommand(33u, AccessPassword, new MemoryStream(new byte[3]
				{
					(byte)Now.Hour,
					(byte)Now.Minute,
					(byte)Now.Second
				})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
			}
			Error = "";
		}
		return StatusDevice;
	}

	public async Task SetNotPrint(DataCommand DataCommand)
	{
		if (DataCommand.NotPrint == true && (byte)(int)(await GetValueInTable(17, 1, 7, typeof(byte))) == 0)
		{
			await SetValueInTable(17, 1, 7, (byte)1);
		}
	}

	public async Task TerminateStausOutDate()
	{
		byte[] array = await RunCommand(17u, OperatorPasswor, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			return;
		}
		byte StatusDevice = (byte)NumberFromStream(array, 15, 1);
		if ((StatusDevice & 0xF) == 1)
		{
			do
			{
				array = await RunCommand(65339u, AccessPassword, new MemoryStream());
			}
			while (array.Length < 3 || array[2] == 0);
		}
		if ((StatusDevice & 0xF) == 6)
		{
			DateTime Now = DateTime.Now;
			if (!IsCommandBad(null, await RunCommand(34u, AccessPassword, new MemoryStream(new byte[3]
			{
				(byte)Now.Day,
				(byte)Now.Month,
				(byte)(Now.Year - 2000)
			})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени"))
			{
				Error = "";
				IsCommandBad(null, await RunCommand(35u, AccessPassword, new MemoryStream(new byte[3]
				{
					(byte)Now.Day,
					(byte)Now.Month,
					(byte)(Now.Year - 2000)
				})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
				Error = "";
				IsCommandBad(null, await RunCommand(33u, AccessPassword, new MemoryStream(new byte[3]
				{
					(byte)Now.Hour,
					(byte)Now.Minute,
					(byte)Now.Second
				})), OpenSerial: false, ClearCheck: false, "Ошибка установки даты-времени");
			}
			Error = "";
		}
	}

	public async Task<string> GetUrlDoc(bool ShekOrDoc = false, RezultCommandKKm RezultCommand = null)
	{
		if (UnitParamets["NoReadQrCode"].AsBool())
		{
			return "";
		}
		string URL = "";
		if (NewModel)
		{
			try
			{
				uint DocNumber = 0u;
				byte[] array = await RunCommand(65281u, AccessPassword, new MemoryStream());
				if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, ""))
				{
					string s = StringNumberFromStreamg(array, 29, 4, 0);
					DocNumber = uint.Parse(s);
					if (RezultCommand != null)
					{
						RezultCommand.CheckNumber = DocNumber;
					}
				}
				string Warning1 = Warning;
				for (int t = 0; t < 30; t++)
				{
					Error = "";
					MemoryStream memoryStream = new MemoryStream();
					BinaryWriter bw = new BinaryWriter(memoryStream);
					Warning = "";
					NumberToStream(bw, DocNumber, 4);
					array = await RunCommand(65338u, AccessPassword, memoryStream);
					if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса документа"))
					{
						int.Parse(StringNumberFromStreamg(array, 5, 2, 0));
						break;
					}
					await Task.Delay(100);
				}
				Warning = Warning1 + Warning;
				if (IsCommandBad(null, null, OpenSerial: false, ClearCheck: false, ""))
				{
					return "Ошибка получения реквизитов документа: " + Error;
				}
				uint LastCheckType = 0u;
				decimal Summ = default(decimal);
				DateTime Date = default(DateTime);
				long FiscalSign = 0L;
				int i = 0;
				do
				{
					array = await RunCommand(65339u, AccessPassword, new MemoryStream());
					if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "") || array.Length <= 6)
					{
						break;
					}
					string text = StringNumberFromStreamg(array, 3, 2, 0);
					int num = int.Parse(StringNumberFromStreamg(array, 5, 2, 0));
					if (ShekOrDoc)
					{
						switch (text)
						{
						case "1054":
							LastCheckType = uint.Parse(StringNumberFromStreamg(array, 7, 1, 0));
							i++;
							break;
						case "1020":
							Summ = decimal.Parse(StringNumberFromStreamg(array, 7, (byte)num, 0)) / 100m;
							i++;
							break;
						case "1042":
							OldSessionCheckNumber = int.Parse(StringNumberFromStreamg(array, 7, (byte)num, 0));
							i++;
							break;
						}
					}
					if (!(text == "1012"))
					{
						if (text == "1077")
						{
							FiscalSign = (long)(((ulong)array[9] << 24) + ((ulong)array[10] << 16) + ((ulong)array[11] << 8) + array[12]);
							i++;
						}
					}
					else
					{
						Date = new DateTime(1970, 1, 1).AddSeconds(uint.Parse(StringNumberFromStreamg(array, 7, 4, 0)));
						i++;
					}
				}
				while ((!ShekOrDoc || i < 5) && (ShekOrDoc || i < 2));
				Error = "";
				URL = "t=" + Date.ToString("yyyyMMddTHHmm") + (ShekOrDoc ? ("&s=" + Summ.ToString("0.00").Replace(',', '.')) : "") + "&fn=" + Kkm.Fn_Number + "&i=" + DocNumber.ToString("D0") + "&fp=" + FiscalSign.ToString("D0") + (ShekOrDoc ? ("&n=" + LastCheckType) : "");
				await TerminateStausOutDate();
			}
			catch (Exception)
			{
				URL = "Ошибка чтения реквизитов документа ";
			}
		}
		return URL;
	}

	public override async Task<uint> GetLastFiscalNumber()
	{
		byte[] array = await RunCommand(65281u, OperatorPasswor, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка получения номера документа"))
		{
			return 0u;
		}
		return (uint)NumberFromStream(array, 29, 4);
	}

	public override async Task<Dictionary<int, string>> GetRegisterCheck(uint FiscalNumber, Dictionary<int, Type> Types)
	{
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(null, null, OpenSerial, ClearCheck: false, ""))
		{
			return null;
		}
		Dictionary<int, string> Rez = new Dictionary<int, string>();
		List<Dictionary<int, string>> Rez1059 = new List<Dictionary<int, string>>();
		Rez.Add(1059, Rez1059.AsString());
		MemoryStream memoryStream = new MemoryStream();
		new BinaryWriter(memoryStream).Write((int)FiscalNumber);
		byte[] array = await RunCommand(65338u, OperatorPasswor, memoryStream);
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
		{
			return null;
		}
		Rez.Add(0, Unit.NumberFromArray(array, 3, 2).AsString());
		while (true)
		{
			array = await RunCommand(65339u, OperatorPasswor, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "") || array.Length <= 3)
			{
				Error = "";
				return Rez;
			}
			byte[] Data2 = new byte[array.Length - 3];
			Array.Copy(array, 3, Data2, 0, Data2.Length);
			if ((Data2[3] << 8) + Data2[2] + 4 > Data2.Length)
			{
				array = await RunCommand(65339u, OperatorPasswor, new MemoryStream());
				if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					break;
				}
				byte[] array2 = new byte[array.Length - 3];
				Array.Copy(array, 3, array2, 0, array2.Length);
				Data2 = Data2.Concat(array2).ToArray();
			}
			int num = (Data2[1] << 8) + Data2[0];
			List<byte[]> list = new List<byte[]>();
			Dictionary<int, string> dictionary;
			if (num == 1059)
			{
				int num2 = 4;
				while (num2 < Data2.Length)
				{
					byte[] array3 = new byte[(Data2[num2 + 3] << 8) + Data2[num2 + 2] + 4];
					Array.Copy(Data2, num2, array3, 0, array3.Length);
					num2 += array3.Length;
					list.Add(array3);
				}
				dictionary = new Dictionary<int, string>();
				Rez1059.Add(dictionary);
			}
			else
			{
				list.Add(Data2);
				dictionary = Rez;
			}
			foreach (byte[] item in list)
			{
				num = (item[1] << 8) + item[0];
				if (!Types.ContainsKey(num))
				{
					continue;
				}
				if (Types[num] == typeof(int))
				{
					dictionary.Add(num, Unit.NumberFromArray(item, 4, item.Length - 4).AsString());
				}
				else if (Types[num] == typeof(uint))
				{
					dictionary.Add(num, Unit.NumberFromArray(item, 4, item.Length - 4).AsString());
					if (num == 1077)
					{
						dictionary[1077] = ((item[6] << 24) + (item[7] << 16) + (item[8] << 8) + item[9]).AsString();
					}
				}
				else if (Types[num] == typeof(byte))
				{
					dictionary.Add(num, item[4].AsString());
				}
				else if (Types[num] == typeof(decimal))
				{
					decimal val;
					if (num == 1023)
					{
						val = Unit.NumberFromArray(item, 5, item.Length - 5);
						ulong num3 = Unit.NumberFromArray(item, 4, 1);
						val /= (decimal)Math.Pow(10.0, num3);
					}
					else
					{
						val = Unit.NumberFromArray(item, 4, item.Length - 4);
						val /= 100m;
					}
					dictionary.Add(num, val.AsString());
				}
				else if (Types[num] == typeof(string))
				{
					dictionary.Add(num, StringFromStream(item, 4, item.Length - 4, NulTerminate: false, use866: true));
				}
				else if (Types[num] == typeof(DateTime))
				{
					int num4 = (item[7] << 24) + (item[6] << 16) + (item[5] << 8) + item[4];
					dictionary.Add(num, new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(num4).AsString());
				}
				else if (Types[num] == typeof(byte[]))
				{
					byte[] array4 = new byte[item.Length - 4];
					Array.Copy(item, 4, array4, 0, item.Length - 4);
					dictionary.Add(num, array4.AsString());
				}
			}
		}
		Error = "";
		return Rez;
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
		int CountStartAgain = 50;
		byte[] bData;
		do
		{
			if (!(TypeProtocol == "0"))
			{
				await SendFrame1(Command, Pass, Msg, TimeOut);
			}
			else
			{
				await SendFrame(Command, Pass, Msg, TimeOut);
			}
			if (Error != "")
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				return new byte[0];
			}
			bData = ((!(TypeProtocol == "0")) ? (await GetFrame1(TimeOut)) : (await GetFrame(TimeOut)));
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
			await Task.Delay(100);
		}
		while (CountStartAgain-- > 0);
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		return bData;
	}

	public async Task<bool> SendFrame(uint Command, string Pass, MemoryStream Msg, int TimeOut = 15000)
	{
		_ = 6;
		try
		{
			int ColRepeat = 5;
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
			base.PortReadTimeout = TimeOut;
			base.PortWriteTimeout = 500;
			byte[] PrtMsg = new byte[1] { 5 };
			await PortWriteAsync(PrtMsg, 0, 1);
			PortLogs.Status = 0;
			Send05 = true;
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
			while (true)
			{
				await PortWriteAsync(ms.ToArray(), 0, (int)ms.Length);
				try
				{
					b = await PortReadByteAsync();
				}
				catch (Exception)
				{
					b = 0;
				}
				PortLogs.Status = 0;
				switch (b)
				{
				case 21:
					break;
				default:
					Error = "Ошибка передачи кадра сообщения";
					PortLogs.Append(Error);
					return false;
				case 6:
					goto end_IL_0470;
				}
				int num4 = ColRepeat - 1;
				ColRepeat = num4;
				if (ColRepeat <= 0)
				{
					Error = "Ошибка приема кадра сообщения на ККТ";
					PortLogs.Append(Error);
					return false;
				}
				continue;
				end_IL_0470:
				break;
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
			bool IsRead = false;
			base.PortReadTimeout = 50;
			for (int i = 0; i < 10; i++)
			{
				try
				{
					await PortReadByteAsync();
					IsRead = true;
				}
				catch (Exception ex5)
				{
					_ = ex5;
					if (!(TypeProtocol == "1"))
					{
						await Task.Delay(50);
						continue;
					}
				}
				break;
			}
			if (!IsRead && TypeProtocol == "0" && !NotChangeTypeProtocol)
			{
				TypeProtocol = "1";
				PortLogs.Append("TypeProtocol = 1");
				Error = "";
				await PortCloseAsync();
				await PortOpenAsync();
			}
		}
		return Data.ToArray();
	}

	public async Task<bool> SendFrame1(uint Command, string Pass, MemoryStream Msg, int TimeOut = 15000)
	{
		_ = 9;
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
			base.PortReadTimeout = TimeOut;
			base.PortWriteTimeout = 500;
			if (Send05)
			{
				base.PortReadTimeout = 50;
			}
			byte[] PrtMsg = new byte[1] { 5 };
			PortLogs.Status = 0;
			await PortWriteAsync(PrtMsg, 0, 1);
			PortLogs.Status = 0;
			byte b;
			try
			{
				b = await PortReadByteAsync();
				PortLogs.Status = 0;
			}
			catch
			{
				b = 0;
			}
			if (b == 6)
			{
				await GetFrame1(5000);
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
			if (b == 2)
			{
				Get02 = true;
				await GetFrame1(5000);
				await PortWriteAsync(PrtMsg, 0, 1);
				try
				{
					await PortReadByteAsync();
				}
				catch (Exception)
				{
				}
			}
			Send05 = true;
			base.PortReadTimeout = TimeOut;
			await PortWriteAsync(ms.ToArray(), 0, (int)ms.Length);
			try
			{
				b = await PortReadByteAsync();
			}
			catch (Exception)
			{
				b = 0;
			}
			PortLogs.Status = 0;
			if (b == 2)
			{
				Get02 = true;
			}
			if (b != 6 && b != 2)
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

	public async Task<byte[]> GetFrame1(int TimeOut)
	{
		Error = "";
		MemoryStream Data = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(Data);
		base.PortReadTimeout = TimeOut;
		base.PortWriteTimeout = 500;
		byte b;
		try
		{
			if (Get02)
			{
				b = 2;
				Get02 = false;
			}
			else
			{
				b = await PortReadByteAsync();
			}
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
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP && TypeProtocol == "0")
		{
			bool IsRead = false;
			base.PortReadTimeout = 50;
			for (int i = 0; i < 10; i++)
			{
				try
				{
					await PortReadByteAsync();
					IsRead = true;
				}
				catch (Exception)
				{
					if (!(TypeProtocol == "1"))
					{
						await Task.Delay(50);
						continue;
					}
				}
				break;
			}
			if (!IsRead && TypeProtocol == "0" && !NotChangeTypeProtocol)
			{
				TypeProtocol = "1";
				PortLogs.Append("Set TypeProtocol = 1");
				Error = "";
			}
		}
		await Task.Delay(50);
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
		if (!SetPort.PortOpen)
		{
			PortLogs.Append("TypeProtocol = " + TypeProtocol.ToString());
			Send05 = false;
		}
		bool num = await base.PortOpenAsync();
		if (num)
		{
			NumBlockBarCode = 0;
			NumSrtGr = 0;
		}
		return num;
	}

	public new async Task<bool> PortCloseAsync()
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			Error = "Устройство не настроено";
			PortLogs.Append(Error);
			return false;
		}
		if (SetPort.PortOpen)
		{
			ClearCheck = false;
		}
		if (SetPort.TypeConnect != SetPorts.enTypeConnect.Com || !NotCloseCom)
		{
			return await base.PortCloseAsync();
		}
		return true;
	}

	public bool IsCommandBad(RezultCommand RezultCommand, byte[] Buffer, bool OpenSerial, bool ClearCheck, string ErrorText, bool Ignore55 = false)
	{
		if (Error != "")
		{
			if (ClearCheck)
			{
				string error = Error;
				Error = "";
				ClearOldCheck().Wait();
				Error = error;
			}
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
			if (Buffer != null && Buffer.Length >= 1 && Buffer[0] == byte.MaxValue)
			{
				num = 2;
			}
			if (!((Buffer != null) & (Buffer.Length > num)) || Buffer[num] == 0 || (Ignore55 && (Buffer[num] == 55 || Buffer[num] == 115)))
			{
				return false;
			}
			CreateTextError(Buffer[num], ErrorText);
			if (ClearCheck)
			{
				string error = Error;
				Error = "";
				ClearOldCheck().Wait();
				Error = error;
			}
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
		if (NewModel)
		{
			switch (ErrorByte)
			{
			case 0:
				text = "ФH: Успешное выполнение команды";
				break;
			case 1:
				text = "ФH: Неизвестная команда, неверный формат посылки или неизвестные параметры";
				break;
			case 2:
				text = "ФH: Неверное состояние ФН. Данная команда требует другого состояния ФН";
				break;
			case 3:
				text = "ФH: Ошибка ФН";
				break;
			case 4:
				text = "ФH: Ошибка КС";
				break;
			case 5:
				text = "ФH: Закончен срок эксплуатации ФН";
				break;
			case 6:
				text = "ФH: Архив ФН переполнен";
				break;
			case 7:
				text = "ФH: Неверные дата и / или время";
				break;
			case 8:
				text = "ФH: Нет запрошенных данных / Запрошенные данные отсутствуют в Архиве ФН";
				break;
			case 9:
				text = "ФH: Некорректное значение параметров команды / Параметры команды имеют правильный формат, но их значение не верно";
				break;
			case 10:
				text = "ФH: Некорректная команда";
				break;
			case 16:
				text = "ФH: Превышение размеров TLV данных";
				break;
			case 17:
				text = "ФH: Нет транспортного соединения";
				break;
			case 18:
				text = "ФH: Исчерпан ресурс КС(криптографического сопроцессора). Требуется закрытие фискального режима";
				break;
			case 20:
				text = "ФH: Ресурс для хранения документов для ОФД исчерпан";
				break;
			case 21:
				text = "ФH: Исчерпан ресурс Ожидания передачи сообщения. Время нахождения в очереди самого старого сообщения на выдачу более 30 календарных дней.";
				break;
			case 22:
				text = "ФH: Продолжительность смены более 24 часов";
				break;
			case 23:
				text = "ФH: Разница более чем на 5 минут отличается от разницы определенному по внутреннему таймеру ФН.";
				break;
			case 32:
				text = "ФH: Сообщение от ОФД не может быть принято";
				break;
			case 38:
				text = "ККТ: Вносимая клиентом сумма меньше суммы чека";
				break;
			case 43:
				text = "ККТ: Невозможно отменить предыдущую команду";
				break;
			case 44:
				text = "ККТ: Обнулённая касса(повторное гашение невозможно)";
				break;
			case 45:
				text = "ККТ: Сумма чека по секции меньше суммы сторно";
				break;
			case 46:
				text = "ККТ: В ККТ нет денег для выплаты";
				break;
			case 47:
				text = "ККТ: Таймаут обмена с ФН";
				break;
			case 48:
				text = "ККТ: ФН не отвечает";
				break;
			case 50:
				text = "ККТ: Требуется выполнение общего гашения";
				break;
			case 51:
				text = "ККТ: Некорректные параметры в команде";
				break;
			case 52:
				text = "ККТ: Нет данных";
				break;
			case 53:
				text = "ККТ: Некорректный параметр при данных настройках";
				break;
			case 54:
				text = "ККТ: Некорректные параметры в команде для данной реализации ККТ";
				break;
			case 55:
				text = "ККТ: Команда не поддерживается в данной реализации ККТ";
				break;
			case 56:
				text = "ККТ: Ошибка в ПЗУ";
				break;
			case 57:
				text = "ККТ: Внутренняя ошибка ПО ККТ";
				break;
			case 58:
				text = "ККТ: Переполнение накопления по надбавкам в смене";
				break;
			case 59:
				text = "ККТ: Переполнение накопления в смене";
				break;
			case 60:
				text = "ККТ: Смена открыта – операция невозможна";
				break;
			case 61:
				text = "ККТ: Смена не открыта или смена превысила 24 часа – операция невозможна";
				break;
			case 62:
				text = "ККТ: Переполнение накопления по секциям в смене";
				break;
			case 63:
				text = "ККТ: Переполнение накопления по скидкам в смене";
				break;
			case 64:
				text = "ККТ: Переполнение диапазона скидок";
				break;
			case 65:
				text = "ККТ: Переполнение диапазона оплаты наличными";
				break;
			case 66:
				text = "ККТ: Переполнение диапазона оплаты типом 2";
				break;
			case 67:
				text = "ККТ: Переполнение диапазона оплаты типом 3";
				break;
			case 68:
				text = "ККТ: Переполнение диапазона оплаты типом 4";
				break;
			case 69:
				text = "ККТ: Сумма всех типов оплаты меньше итога чека";
				break;
			case 70:
				text = "ККТ: Не хватает наличности в кассе";
				break;
			case 71:
				text = "ККТ: Переполнение накопления по налогам в смене";
				break;
			case 72:
				text = "ККТ: Переполнение итога чека";
				break;
			case 73:
				text = "ККТ: Операция невозможна в открытом чеке данного типа";
				break;
			case 74:
				text = "ККТ: Открыт чек – операция невозможна";
				break;
			case 75:
				text = "ККТ: Буфер чека переполнен";
				break;
			case 76:
				text = "ККТ: Переполнение накопления по обороту налогов в смене";
				break;
			case 77:
				text = "ККТ: Вносимая безналичной оплатой сумма больше суммы чека";
				break;
			case 78:
				text = "ККТ: Смена превысила 24 часа";
				break;
			case 79:
				text = "ККТ: Неверный пароль";
				break;
			case 80:
				text = "ККТ: Идет печать результатов выполнения предыдущей команды";
				break;
			case 81:
				text = "ККТ: Переполнение накоплений наличными в смене";
				break;
			case 82:
				text = "ККТ: Переполнение накоплений по типу оплаты 2 в смене";
				break;
			case 83:
				text = "ККТ: Переполнение накоплений по типу оплаты 3 в смене";
				break;
			case 84:
				text = "ККТ: Переполнение накоплений по типу оплаты 4 в смене";
				break;
			case 85:
				text = "ККТ: Чек закрыт – операция невозможна";
				break;
			case 86:
				text = "ККТ: Нет документа для повтора";
				break;
			case 88:
				text = "ККТ: Ожидание команды продолжения печати";
				break;
			case 89:
				text = "ККТ: Документ открыт другим кассиром";
				break;
			case 90:
				text = "ККТ: Скидка превышает накопления в чеке";
				break;
			case 91:
				text = "ККТ: Переполнение диапазона надбавок";
				break;
			case 92:
				text = "ККТ: Понижено напряжение 24В";
				break;
			case 93:
				text = "ККТ: Таблица не определена";
				break;
			case 94:
				text = "ККТ: Неверная операция";
				break;
			case 95:
				text = "ККТ: Отрицательный итог чека";
				break;
			case 96:
				text = "ККТ: Переполнение при умножении";
				break;
			case 97:
				text = "ККТ: Переполнение диапазона цены";
				break;
			case 98:
				text = "ККТ: Переполнение диапазона количества";
				break;
			case 99:
				text = "ККТ: Переполнение диапазона отдела";
				break;
			case 101:
				text = "ККТ: Не хватает денег в секции";
				break;
			case 102:
				text = "ККТ: Переполнение денег в секции";
				break;
			case 104:
				text = "ККТ: Не хватает денег по обороту налогов";
				break;
			case 105:
				text = "ККТ: Переполнение денег по обороту налогов";
				break;
			case 106:
				text = "ККТ: Ошибка питания в момент ответа по I2C";
				break;
			case 107:
				text = "ККТ: Нет чековой ленты";
				break;
			case 108:
				text = "ККТ: Нет операционного журнала";
				break;
			case 109:
				text = "ККТ: Не хватает денег по налогу";
				break;
			case 110:
				text = "ККТ: Переполнение денег по налогу";
				break;
			case 111:
				text = "ККТ: Переполнение по выплате в смене";
				break;
			case 113:
				text = "ККТ: Ошибка отрезчика";
				break;
			case 114:
				text = "ККТ: Команда не поддерживается в данном подрежиме";
				break;
			case 115:
				text = "ККТ: Команда не поддерживается в данном режиме";
				break;
			case 116:
				text = "ККТ: Ошибка ОЗУ";
				break;
			case 117:
				text = "ККТ: Ошибка питания";
				break;
			case 118:
				text = "ККТ: Ошибка принтера: нет импульсов с тахогенератора";
				break;
			case 119:
				text = "ККТ: Ошибка принтера: нет сигнала с датчиков";
				break;
			case 120:
				text = "ККТ: Замена ПО";
				break;
			case 122:
				text = "ККТ: Поле не редактируется";
				break;
			case 123:
				text = "ККТ: Ошибка оборудования";
				break;
			case 124:
				text = "ККТ: Не совпадает дата";
				break;
			case 125:
				text = "ККТ: Неверный формат даты";
				break;
			case 126:
				text = "ККТ: Неверное значение в поле длины";
				break;
			case 127:
				text = "ККТ: Переполнение диапазона итога чека";
				break;
			case 132:
				text = "ККТ: Переполнение наличности";
				break;
			case 133:
				text = "ККТ: Переполнение по приходу в смене";
				break;
			case 134:
				text = "ККТ: Переполнение по расходу в смене";
				break;
			case 135:
				text = "ККТ: Переполнение по возвратам прихода в смене";
				break;
			case 136:
				text = "ККТ: Переполнение по возвратам расхода в смене";
				break;
			case 137:
				text = "ККТ: Переполнение по внесению в смене";
				break;
			case 138:
				text = "ККТ: Переполнение по надбавкам в чеке";
				break;
			case 139:
				text = "ККТ: Переполнение по скидкам в чеке";
				break;
			case 140:
				text = "ККТ: Отрицательный итог надбавки в чеке";
				break;
			case 141:
				text = "ККТ: Отрицательный итог скидки в чеке";
				break;
			case 142:
				text = "ККТ: Нулевой итог чека";
				break;
			case 143:
				text = "ККТ: Касса не зарегистрирована";
				break;
			case 144:
				text = "ККТ: Поле превышает размер, установленный в настройках";
				break;
			case 145:
				text = "ККТ: Выход за границу поля печати при данных настройках шрифта";
				break;
			case 146:
				text = "ККТ: Наложение полей";
				break;
			case 147:
				text = "ККТ: Восстановление ОЗУ прошло успешно";
				break;
			case 148:
				text = "ККТ: Исчерпан лимит операций в чеке";
				break;
			case 150:
				text = "ККТ: Выполните отчет о закрытии смены";
				break;
			case 155:
				text = "ККТ: Некорректное действие";
				break;
			case 156:
				text = "ККТ: Товар не найден по коду в базе товаров";
				break;
			case 157:
				text = "ККТ: Неверные данные в записе о товаре в базе товаров";
				break;
			case 158:
				text = "ККТ: Неверный размер файла базы или регистров товаров";
				break;
			case 160:
				text = "ККТ: Запрещена работа с маркированным товарами";
				break;
			case 161:
				text = "ККТ: Нарушена правильная последовательность подачи команд для обработки маркированных товаров";
				break;
			case 162:
				text = "ККТ: Работа с маркированными товарами временно заблокирована";
				break;
			case 163:
				text = "ККТ: Переполнена таблица проверки кодов маркировки";
				break;
			case 164:
				text = "ККТ: В блоке TLV отсутствуют необходимые реквизиты";
				break;
			case 165:
				text = "ККТ: В реквизите 2007 содержится КМ, который ранее не проверялся в ФН";
				break;
			case 192:
				text = "ККТ: Контроль даты и времени(подтвердите дату и время)";
				break;
			case 194:
				text = "ККТ: Превышение напряжения в блоке питания";
				break;
			case 196:
				text = "ККТ: Несовпадение номеров смен";
				break;
			case 197:
				text = "ККТ: Буфер подкладного документа пуст";
				break;
			case 198:
				text = "ККТ: Подкладной документ отсутствует";
				break;
			case 199:
				text = "ККТ: Поле не редактируется в данном режиме";
				break;
			case 200:
				text = "ККТ: Нет связи с принтером или отсутствуют импульсы от таходатчика";
				break;
			case 201:
				text = "ККТ: Перегрев печатающей головки";
				break;
			case 202:
				text = "ККТ: Температура вне условий эксплуатации";
				break;
			case 203:
				text = "ККТ: Неверный подытог чека";
				break;
			case 206:
				text = "ККТ: Лимит минимального свободного объема ОЗУ или ПЗУ на ККТ исчерпан";
				break;
			case 207:
				text = "ККТ: Неверная дата(Часы сброшены ? Установите дату!)";
				break;
			case 208:
				text = "ККТ: Отчет операционного журнала не распечатан!";
				break;
			case 209:
				text = "ККТ: Нет данных в буфере";
				break;
			case 211:
				text = "ККТ: Код маркировки не распознан";
				break;
			case 213:
				text = "ККТ: Критическая ошибка при загрузке ERRxx";
				break;
			case 224:
				text = "ККТ: Ошибка связи с купюроприемником";
				break;
			case 225:
				text = "ККТ: Купюроприемник занят";
				break;
			case 226:
				text = "ККТ: Итог чека не соответствует итогу купюроприемника";
				break;
			case 227:
				text = "ККТ: Ошибка купюроприемника";
				break;
			case 228:
				text = "ККТ: Итог купюроприемника не нулевой";
				break;
			}
			Error = TextError + " ( " + ErrorByte + "-" + text + " )";
		}
		else
		{
			switch (ErrorByte)
			{
			case 1:
				text = "Неисправен накопитель ФП 1, ФП 2 или часы";
				break;
			case 2:
				text = "Отсутствует ФП 1";
				break;
			case 3:
				text = "Отсутствует ФП 2";
				break;
			case 4:
				text = "Некорректные параметры в команде обращения к ФП";
				break;
			case 5:
				text = "Нет запрошенных данных";
				break;
			case 6:
				text = "ФП в режиме вывода данных";
				break;
			case 7:
				text = "Некорректные параметры в команде для данной реализации ФП";
				break;
			case 8:
				text = "Команда не поддерживается в данной реализации ФП";
				break;
			case 9:
				text = "Некорректная длина команды";
				break;
			case 10:
				text = "Формат данных не BCD";
				break;
			case 11:
				text = "Неисправна ячейка памяти ФП при записи итога";
				break;
			case 17:
				text = "Не введена лицензия ";
				break;
			case 18:
				text = "Заводской номер уже введен";
				break;
			case 19:
				text = "Текущая дата меньше даты последней записи в ФП";
				break;
			case 20:
				text = "Область сменных итогов ФП переполнена";
				break;
			case 21:
				text = "Смена уже открыта";
				break;
			case 22:
				text = "Смена не открыта";
				break;
			case 23:
				text = "Номер первой смены больше номера последней смены";
				break;
			case 24:
				text = "Дата первой смены больше даты последней смены";
				break;
			case 25:
				text = "Нет данных в ФП";
				break;
			case 26:
				text = "Область перерегистраций в ФП переполнена";
				break;
			case 27:
				text = "Заводской номер не введен";
				break;
			case 28:
				text = "В заданном диапазоне есть поврежденная запись";
				break;
			case 29:
				text = "Повреждена последняя запись сменных итогов";
				break;
			case 30:
				text = "Область перерегистраций ФП переполнена";
				break;
			case 31:
				text = "Отсутствует память регистров";
				break;
			case 32:
				text = "Переполнение денежного регистра при добавлении";
				break;
			case 33:
				text = "Вычитаемая сумма больше содержимого денежного регистра";
				break;
			case 34:
				text = "Неверная дата";
				break;
			case 35:
				text = "Нет записи активизации";
				break;
			case 36:
				text = "Область активизаций переполнена";
				break;
			case 37:
				text = "Нет активизации с запрашиваемым номером";
				break;
			case 38:
				text = "Вносимая клиентом сумма меньше суммы чека";
				break;
			case 43:
				text = "Невозможно отменить предыдущую команду";
				break;
			case 44:
				text = "Обнулённая касса (повторное гашение невозможно)";
				break;
			case 45:
				text = "Сумма чека по секции меньше суммы сторно";
				break;
			case 46:
				text = "В ФР нет денег для выплаты";
				break;
			case 48:
				text = "ФР заблокирован, ждет ввода пароля налогового инспектора";
				break;
			case 50:
				text = "Требуется выполнение общего гашения";
				break;
			case 51:
				text = "Некорректные параметры в команде";
				break;
			case 52:
				text = "Нет данных";
				break;
			case 53:
				text = "Некорректный параметр при данных настройках";
				break;
			case 54:
				text = "Некорректные параметры в команде для данной реализации ФР";
				break;
			case 55:
				text = "Команда не поддерживается в данной реализации ФР";
				break;
			case 56:
				text = "Ошибка в ПЗУ";
				break;
			case 57:
				text = "Внутренняя ошибка ПО ФР";
				break;
			case 58:
				text = "Переполнение накопления по надбавкам в смене";
				break;
			case 59:
				text = "Переполнение накопления в смене";
				break;
			case 60:
				text = "Смена открыта – операция невозможна";
				break;
			case 61:
				text = "Смена не открыта – операция невозможна";
				break;
			case 62:
				text = "Переполнение накопления по секциям в смене";
				break;
			case 63:
				text = "Переполнение накопления по скидкам в смене";
				break;
			case 64:
				text = "Переполнение диапазона скидок";
				break;
			case 65:
				text = "Переполнение диапазона оплаты наличными";
				break;
			case 66:
				text = "Переполнение диапазона оплаты типом 2";
				break;
			case 67:
				text = "Переполнение диапазона оплаты типом 3";
				break;
			case 68:
				text = "Переполнение диапазона оплаты типом 4";
				break;
			case 69:
				text = "Сумма всех типов оплаты меньше итога чека";
				break;
			case 70:
				text = "Не хватает наличности в кассе";
				break;
			case 71:
				text = "Переполнение накопления по налогам в смене";
				break;
			case 72:
				text = "Переполнение итога чека";
				break;
			case 73:
				text = "Операция невозможна в открытом чеке данного типа";
				break;
			case 74:
				text = "Открыт чек – операция невозможна";
				break;
			case 75:
				text = "Буфер чека переполнен";
				break;
			case 76:
				text = "Переполнение накопления по обороту налогов в смене";
				break;
			case 77:
				text = "Вносимая безналичной оплатой сумма больше суммы чека";
				break;
			case 78:
				text = "Смена превысила 24 часа";
				break;
			case 79:
				text = "Неверный пароль";
				break;
			case 80:
				text = "Идет печать предыдущей команды";
				break;
			case 81:
				text = "Переполнение накоплений наличными в смене";
				break;
			case 82:
				text = "Переполнение накоплений по типу оплаты 2 в смене";
				break;
			case 83:
				text = "Переполнение накоплений по типу оплаты 3 в смене";
				break;
			case 84:
				text = "Переполнение накоплений по типу оплаты 4 в смене";
				break;
			case 85:
				text = "Чек закрыт – операция невозможна";
				break;
			case 86:
				text = "Нет документа для повтора";
				break;
			case 87:
				text = "ЭКЛЗ: количество закрытых смен не совпадает с ФП";
				break;
			case 88:
				text = "Ожидание команды продолжения печати";
				break;
			case 89:
				text = "Документ открыт другим оператором";
				break;
			case 90:
				text = "Скидка превышает накопления в чеке";
				break;
			case 91:
				text = "Переполнение диапазона надбавок";
				break;
			case 92:
				text = "Понижено напряжение 24В";
				break;
			case 93:
				text = "Таблица не определена";
				break;
			case 94:
				text = "Некорректная операция";
				break;
			case 95:
				text = "Отрицательный итог чека";
				break;
			case 96:
				text = "Переполнение при умножении";
				break;
			case 97:
				text = "Переполнение диапазона цены";
				break;
			case 98:
				text = "Переполнение диапазона количества";
				break;
			case 99:
				text = "Переполнение диапазона отдела";
				break;
			case 100:
				text = "ФП отсутствует";
				break;
			case 101:
				text = "Не хватает денег в секции";
				break;
			case 102:
				text = "Переполнение денег в секции";
				break;
			case 103:
				text = "Ошибка связи с ФП";
				break;
			case 104:
				text = "Не хватает денег по обороту налогов";
				break;
			case 105:
				text = "Переполнение денег по обороту налогов";
				break;
			case 106:
				text = "Ошибка питания в момент ответа по I2C";
				break;
			case 107:
				text = "Нет чековой ленты";
				break;
			case 108:
				text = "Нет контрольной ленты";
				break;
			case 109:
				text = "Не хватает денег по налогу";
				break;
			case 110:
				text = "Переполнение денег по налогу";
				break;
			case 111:
				text = "Переполнение по выплате в смене";
				break;
			case 112:
				text = "Переполнение ФП";
				break;
			case 113:
				text = "Ошибка отрезчика";
				break;
			case 114:
				text = "Команда не поддерживается в данном подрежиме";
				break;
			case 115:
				text = "Команда не поддерживается в данном режиме";
				break;
			case 116:
				text = "Ошибка ОЗУ";
				break;
			case 117:
				text = "Ошибка питания";
				break;
			case 118:
				text = "Ошибка принтера: нет импульсов с тахогенератора";
				break;
			case 119:
				text = "Ошибка принтера: нет сигнала с датчиков";
				break;
			case 120:
				text = "Замена ПО";
				break;
			case 121:
				text = "Замена ФП";
				break;
			case 122:
				text = "Поле не редактируется";
				break;
			case 123:
				text = "Ошибка оборудования";
				break;
			case 124:
				text = "Не совпадает дата";
				break;
			case 125:
				text = "Неверный формат даты";
				break;
			case 126:
				text = "Неверное значение в поле длины";
				break;
			case 127:
				text = "Переполнение диапазона итога чека";
				break;
			case 128:
				text = "Ошибка связи с ФП";
				break;
			case 129:
				text = "Ошибка связи с ФП";
				break;
			case 130:
				text = "Ошибка связи с ФП";
				break;
			case 131:
				text = "Ошибка связи с ФП";
				break;
			case 132:
				text = "Переполнение наличности";
				break;
			case 133:
				text = "Переполнение по продажам в смене";
				break;
			case 134:
				text = "Переполнение по покупкам в смене";
				break;
			case 135:
				text = "Переполнение по возвратам продаж в смене";
				break;
			case 136:
				text = "Переполнение по возвратам покупок в смене";
				break;
			case 137:
				text = "Переполнение по внесению в смене";
				break;
			case 138:
				text = "Переполнение по надбавкам в чеке";
				break;
			case 139:
				text = "Переполнение по скидкам в чеке";
				break;
			case 140:
				text = "Отрицательный итог надбавки в чеке";
				break;
			case 141:
				text = "Отрицательный итог скидки в чеке";
				break;
			case 142:
				text = "Нулевой итог чека";
				break;
			case 143:
				text = "Касса не фискализирована";
				break;
			case 144:
				text = "Поле превышает размер, установленный в настройках";
				break;
			case 145:
				text = "Выход за границу поля печати при данных настройках шрифта";
				break;
			case 146:
				text = "Наложение полей";
				break;
			case 147:
				text = "Восстановление ОЗУ прошло успешно";
				break;
			case 148:
				text = "Исчерпан лимит операций в чеке";
				break;
			case 160:
				text = "Ошибка связи с ЭКЛЗ";
				break;
			case 161:
				text = "ЭКЛЗ отсутствует";
				break;
			case 162:
				text = "ЭКЛЗ: Некорректный формат или параметр команды";
				break;
			case 163:
				text = "Некорректное состояние ЭКЛЗ";
				break;
			case 164:
				text = "Авария ЭКЛЗ";
				break;
			case 165:
				text = "Авария КС в составе ЭКЛЗ ";
				break;
			case 166:
				text = "Исчерпан временной ресурс ЭКЛЗ";
				break;
			case 167:
				text = "ЭКЛЗ переполнена";
				break;
			case 168:
				text = "ЭКЛЗ: Неверные дата и время";
				break;
			case 169:
				text = "ЭКЛЗ: Нет запрошенных данных";
				break;
			case 170:
				text = "Переполнение ЭКЛЗ (отрицательный итог документа)";
				break;
			case 176:
				text = "ЭКЛЗ: Переполнение в параметре количество";
				break;
			case 177:
				text = "ЭКЛЗ: Переполнение в параметре сумма";
				break;
			case 178:
				text = "ЭКЛЗ: Уже активизирована";
				break;
			case 192:
				text = "Контроль даты и времени (подтвердите дату и время)";
				break;
			case 194:
				text = "Превышение напряжения в блоке питания";
				break;
			case 195:
				text = "Несовпадение итогов чека и ЭКЛЗ";
				break;
			case 196:
				text = "Несовпадение номеров смен";
				break;
			case 197:
				text = "Буфер подкладного документа пуст";
				break;
			case 198:
				text = "Подкладной документ отсутствует";
				break;
			case 199:
				text = "Поле не редактируется в данном режиме";
				break;
			case 200:
				text = "ККТ: Нет связи с принтером или отсутствуют импульсы от таходатчика";
				break;
			case 201:
				text = "ККТ: Перегрев печатающей головки";
				break;
			case 202:
				text = "ККТ: Температура вне условий эксплуатации";
				break;
			case 203:
				text = "ККТ: Неверный подытог чека";
				break;
			case 206:
				text = "ККТ: Лимит минимального свободного объема ОЗУ или ПЗУ на ККТ исчерпан";
				break;
			case 207:
				text = "ККТ: Неверная дата(Часы сброшены ? Установите дату!)";
				break;
			case 208:
				text = "ККТ: Отчет операционного журнала не распечатан!";
				break;
			case 209:
				text = "ККТ: Нет данных в буфере";
				break;
			case 213:
				text = "ККТ: Критическая ошибка при загрузке ERRxx";
				break;
			case 224:
				text = "ККТ: Ошибка связи с купюроприемником";
				break;
			case 225:
				text = "ККТ: Купюроприемник занят";
				break;
			case 226:
				text = "ККТ: Итог чека не соответствует итогу купюроприемника";
				break;
			case 227:
				text = "ККТ: Ошибка купюроприемника";
				break;
			case 228:
				text = "ККТ: Итог купюроприемника не нулевой";
				break;
			}
			Error = TextError + " ( " + ErrorByte + " : " + text + " )";
		}
		return true;
	}
}
