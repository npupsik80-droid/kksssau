using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace KkmFactory;

internal class Atol_5 : UnitPort
{
	private class UsbItem
	{
		public string key;

		public string description;
	}

	private Fptr Atol5;

	private string Atol5Lock = "";

	private DateTime Atol5Close = DateTime.Now;

	public string AccessPassword = "0";

	public string OperatorPasswor = "30";

	private Encoding e886 = Encoding.GetEncoding(866);

	public int MaxPrintingWidth;

	public Dictionary<int, (int Font, int Width)> PrintingWidths = new Dictionary<int, (int, int)>();

	public SortedList<int, byte> Nalogs = new SortedList<int, byte>();

	private int TimeOut = 5000;

	private int WidthPrintPix;

	private int DefPrintingWidth;

	private string Model = "500";

	private string ChanelOfd = "";

	private bool ClearCheck;

	private bool ClosePort;

	private DateTime LastReadErrorOFD;

	private uint CheckNumber;

	public static Timer TimerPortOff;

	private bool RemoteServer;

	private string RemoteServerURL = "";

	private uint RemoteServerTimeOut = 10000u;

	public Atol_5(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		Kkm.IsKKT = true;
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		TimerPortOff = new Timer(PortOffTimmer, this, 10000, 10000);
	}

	public override void Destroy()
	{
		TimerPortOff.Dispose();
		TimerPortOff = null;
		try
		{
			Atol5.Close();
		}
		catch
		{
		}
	}

	public override void LoadParamets()
	{
		Error = "";
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "Драйвер ККМ для моделей: " + SettDr.TypeDevice.SupportModels;
		UnitEquipmentType = "ФискальныйРегистратор";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kkmserver.ru";
		UnitAdditionallinks = "<a href='https://kkmserver.ru/Donload/Atol_DTO_x32_setup.exe'>Дистрибутив 'Атол ДТО' для Windows x32</a><br/>";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Удаленный сервер Атол\">\r\n            <Parameter Name=\"RemoteServer\" Caption=\"Через удаленный сервер\" TypeValue=\"Boolean\" DefaultValue=\"false\"\r\n                Description=\"Доступ к драйверу через сервер удаленного доступа Атол\r\n                                Если не знаете что это - то не включайте\r\n                                На удаленном ПК должен быть запущен сервис 'ATOL Fiscal Printer Remote Server'\" />\r\n            <Parameter Name=\"RemoteServerURL\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" DefaultValue=\"\" MasterParameterName=\"RemoteServer\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"true\"\r\n                Description=\"Адрес сервера удаленного доступа Атола\r\n                            если текущий ПК указывайте 'localhost'\" />\r\n            <Parameter Name=\"RemoteServerTimeOut\" Caption=\"Тайм-аут, мс\" TypeValue=\"Number\" DefaultValue=\"10000\" MasterParameterName=\"RemoteServer\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"true\" />\r\n        </Group>\r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"Model\" Caption=\"Модель\" TypeValue=\"String\" DefaultValue=\"500\">\r\n                <ChoiceList>\r\n                    #ChoiceListModel#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\" DefaultValue=\"3\"\r\n                Description=\"ККТ 'Атол' может быть подключен через следующие интерфейсы (типы соединений):\r\n\r\n                            Ehternet/WiFi: сеть (Предпочтительный способ подключения)\r\n                            USB-to-COM: эмуляция СОМ порта через USB\r\n                            COM порт: через СОМ порт ПК\r\n\r\n                            Внимание! При работе по 'USB-to-COM' необходимо установить Атоловские драйвера USB-COM.\r\n                            Иначе не будет работать EoU, и не будет связи с ОФД!\r\n \r\n                            Для переключения интерфейса подключения надо сделать следующие:\r\n \r\n                            1. Выключить питание\r\n                            2. Нажать и удерживать единственную кнопку 'Прогон  чековой лены'\r\n                            3. Включить питание (не отпуская кнопки)\r\n                            4. Дождаться 5 гудков: 1 гудок сразу, длинная пауза, 4 гудка через паузу\r\n                            5. После 5 гудка отпустить кнопку\r\n                                            \r\n                            ККТ напечатает меню:\r\n                            1. Выход\r\n                            2. Канал обмена\r\n                            3. ...\r\n                            Нажмите два раза кнопку (вход в меню 'Канал обмена')\r\n                                            \r\n                            ККТ напечатает меню:\r\n                            1. Выход\r\n                            2. RS-232 (COM порт)\r\n                            3. USB\r\n                            4. Ethernet\r\n                            5. Bluetooth\r\n                            6. WiFi\r\n                            Нажмите кнопку с количеством соответствующему требуемую номеру канала обмена\r\n\r\n                            Нажмите кнопку 1 раз для записи параметра и выхода \r\n                            Выключите-включите ККТ\r\n                            \">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                    <Item Value=\"3\">USB порт</Item>\r\n                    <Item Value=\"4\">Bluetooth порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP адрес/имя хоста\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\" DefaultValue=\"192.168.10.1\"\r\n                Description=\"В режиме интерфейса 'Ehternet/WiFi' KKТ 'Атол' при включении напечатает свой IP адрес.\r\n\r\n                            ККТ по умолчанию настроен на автоматическое получение IP адресп по dhcp. \r\n                            По умолчанию адрес: 192.168.10.1, шлюз 192.168.10.0.\r\n                            \" />\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"5555\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"\r\n                Description=\"Порт по умолчанию 5555\r\n                            Для Атол Sigma порт 15000\r\n                            \" />\r\n           <Parameter Name=\"ChanelOfd\" Caption=\"Канал доступа к ОФД\" TypeValue=\"String\" DefaultValue=\"\" Description=\"Канал доступа к сереверу ОФД\" >\r\n                <ChoiceList>\r\n                    <Item Value=\"\">Автоматически</Item>\r\n                    <Item Value=\"Ethernet\">Сеть: Ethernet</Item>\r\n                    <Item Value=\"WiFi\">Сеть: WiFi</Item>\r\n                    <Item Value=\"EOU\">EOU (Ethernet over USB)</Item>\r\n                    <Item Value=\"EOT\">EOT (Ethernet over Transport)</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\"\r\n                Description=\"В режиме интерфейса 'USB-to-COM' узнать к какому номеру COM-порта подключен ККТ 'Атол' можно так:\r\n\r\n                            1. На иконке 'Этот компьютер' вызвать контекстное меню\r\n                            2. В контекстном меню выбрать пункт 'Управление'\r\n                            3. Далее в дереве свойств выбрать 'Диспечер устройств'\r\n                            4. В 'Диспечер устройств' раскрыть ветку 'Порты (COM и LTP)'\r\n                            5. Включая и выключая ККТ можно увидитеть какой COM порт возникает и пропадает - это и есть нужный Вам порт.\r\n\r\n                            Если при включении/выключении порт не появляется значит:\r\n                            1. Или ККТ не в режиме интерфейса 'USB-to-COM'\r\n                            2. Или не установлены драйвера для эмуляции COM порта через USB\r\n                            \">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"14400\">14400</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"USBPort\" Caption=\"USB: порт\" TypeValue=\"String\" DefaultValue=\"\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"3\">\r\n                <ChoiceList>\r\n                    #ChoiceListUSB#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"BluetoothPort\" Caption=\"Bluetooth: MAC-адрес\" TypeValue=\"String\" DefaultValue=\"\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"4\"/>\r\n            <Parameter Name=\"TimeOut\" Caption=\"Тайм-аут выполнения команды\" TypeValue=\"String\" DefaultValue=\"15000\" />\r\n            <Parameter Name=\"ClosePort\" Caption=\"Закрывать порт после команды\" TypeValue=\"Boolean\" DefaultValue=\"false\"\r\n                Help=\"Закрывать порт ККТ после каждой команды (для разблокировки ККТ для других программ)\"\r\n            />\r\n        </Group>\r\n        <Group Caption=\"Общие параметры\">\r\n            <Parameter Name=\"AccessPassword\" Caption=\"Пароль доступа\" TypeValue=\"String\" DefaultValue=\" \" /> \r\n            <Parameter Name=\"OperatorPasswor\" Caption=\"Пароль администратора\" TypeValue=\"String\" DefaultValue=\"30\" /> \r\n            <Parameter Name=\"PrintFiscalQRBarCodr\" Caption=\"Печатать QR код с данными чека\" TypeValue=\"Boolean\" DefaultValue=\"true\" />  \r\n            <Parameter Name=\"DefPrintingWidth\" Caption=\"Ширина ленты в знаках\" TypeValue=\"String\" DefaultValue=\"\">\r\n                <ChoiceList>\r\n                    <Item Value=\"0\">Определять автоматически</Item>\r\n                    <Item Value=\"32\">32</Item>\r\n                    <Item Value=\"36\">36</Item>\r\n                    <Item Value=\"42\">42</Item>\r\n                    <Item Value=\"48\">48</Item>\r\n                    <Item Value=\"50\">50</Item>\r\n                    <Item Value=\"57\">57</Item>\r\n                    <Item Value=\"64\">64</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		try
		{
			if (Atol5 == null)
			{
				Atol5 = new Fptr(this);
			}
			if (RemoteServer)
			{
				Atol5.setSingleSetting("RemoteServerAddr", RemoteServerURL);
				Atol5.setSingleSetting("RemoteServerConnectionTimeout", RemoteServerTimeOut.ToString());
			}
			else
			{
				Atol5.setSingleSetting("RemoteServerAddr", "");
			}
			Atol5.setParam(65652, "UsbDevicePath");
			if (Atol5.utilMapping() == 0)
			{
				UsbItem[]? array = JsonConvert.DeserializeObject<UsbItem[]>(Atol5.getParamString(65653), new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				string text3 = "";
				UsbItem[] array2 = array;
				foreach (UsbItem usbItem in array2)
				{
					text3 = text3 + "<Item Value=\"" + usbItem.key + "\">" + usbItem.description + "</Item>";
				}
				text = text.Replace("#ChoiceListUSB#", text3);
			}
			Atol5.setParam(65652, "Model");
			if (Atol5.utilMapping() == 0)
			{
				UsbItem[]? array3 = JsonConvert.DeserializeObject<UsbItem[]>(Atol5.getParamString(65653), new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				string text4 = "";
				UsbItem[] array2 = array3;
				foreach (UsbItem usbItem2 in array2)
				{
					text4 = text4 + "<Item Value=\"" + usbItem2.key + "\">" + usbItem2.description + "</Item>";
				}
				text = text.Replace("#ChoiceListModel#", text4);
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			PortOff(IsOll: true, IsOllDes: true);
		}
		LoadParametsFromXML(text);
		string paramsXML = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                <Actions>\r\n                    <Action Name=\"RestartKKT\" Caption=\"Перезагрузить ККТ\"/> \r\n                </Actions>";
		LoadAdditionalActionsFromXML(paramsXML);
	}

	public override void WriteParametsToUnits()
	{
		PortOff(IsOll: true);
		bool flag = false;
		base.WriteParametsToUnits();
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			switch (unitParamet.Key)
			{
			case "Model":
				Model = unitParamet.Value.Trim();
				break;
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
					SetPort.TypeConnect = SetPorts.enTypeConnect.Usb;
				}
				else if (unitParamet.Value == "4")
				{
					SetPort.TypeConnect = SetPorts.enTypeConnect.Bluetooth;
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
			case "OfdOverWiFi":
				flag = unitParamet.Value.AsBool();
				break;
			case "ChanelOfd":
				ChanelOfd = unitParamet.Value.Trim();
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
			case "TimeOut":
				try
				{
					TimeOut = unitParamet.Value.AsInt();
				}
				catch
				{
				}
				break;
			case "ClosePort":
				ClosePort = unitParamet.Value.AsBool();
				break;
			case "DefPrintingWidth":
				DefPrintingWidth = unitParamet.Value.AsInt();
				break;
			case "RemoteServer":
				RemoteServer = unitParamet.Value.AsBool();
				break;
			case "RemoteServerURL":
				RemoteServerURL = unitParamet.Value.Trim();
				break;
			case "RemoteServerTimeOut":
				RemoteServerTimeOut = unitParamet.Value.AsUInt();
				break;
			}
		}
		if (flag)
		{
			ChanelOfd = "WiFi";
			UnitParamets["ChanelOfd"] = ChanelOfd;
			SettDr.Paramets["ChanelOfd"] = ChanelOfd;
		}
	}

	public override object GetAhtungData()
	{
		return Atol5;
	}

	public override void SetAhtungData(object Data)
	{
		if (Data != null && Data.GetType() == typeof(Fptr))
		{
			Atol5 = (Fptr)Data;
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		NameDevice = "<Не определено>";
		IsInit = false;
		Error = "";
		bool OpenSerial;
		try
		{
			OpenSerial = await PortOpenAsync();
		}
		catch (Exception ex)
		{
			LastError = ex.Message;
			return false;
		}
		if (Error != "")
		{
			return false;
		}
		Atol5.setParam(65587, 0u);
		int StatRun = Atol5.queryData();
		if (!IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
		{
			NameDevice = Atol5.getParamString(65603).Trim();
			IdModel = (int)Atol5.getParamInt(65544);
			switch (IdModel)
			{
			case 30:
				MaxPrintingWidth = 50;
				break;
			case 31:
				MaxPrintingWidth = 32;
				break;
			case 32:
				MaxPrintingWidth = 42;
				break;
			case 35:
				MaxPrintingWidth = 36;
				break;
			case 47:
				MaxPrintingWidth = 36;
				break;
			case 51:
				MaxPrintingWidth = 32;
				break;
			case 52:
				MaxPrintingWidth = 48;
				break;
			case 53:
				MaxPrintingWidth = 57;
				break;
			case 54:
				MaxPrintingWidth = 32;
				break;
			case 57:
				MaxPrintingWidth = 64;
				break;
			case 61:
				MaxPrintingWidth = 32;
				break;
			case 62:
				MaxPrintingWidth = 48;
				break;
			case 63:
				MaxPrintingWidth = 64;
				break;
			case 64:
				MaxPrintingWidth = 48;
				break;
			case 67:
				MaxPrintingWidth = 42;
				break;
			case 69:
				MaxPrintingWidth = 64;
				break;
			case 72:
				MaxPrintingWidth = 32;
				break;
			case 75:
				MaxPrintingWidth = 42;
				break;
			case 77:
				MaxPrintingWidth = 42;
				break;
			case 78:
				MaxPrintingWidth = 42;
				break;
			case 80:
				MaxPrintingWidth = 42;
				break;
			case 82:
				MaxPrintingWidth = 32;
				break;
			case 87:
				MaxPrintingWidth = 48;
				break;
			}
			Error = "";
			MaxPrintingWidth = (int)Atol5.getParamInt(65601);
			Kkm.PrintingWidth = MaxPrintingWidth;
			WidthPrintPix = (int)Atol5.getParamInt(65602);
			Kkm.NumberKkm = Atol5.getParamString(65559).Trim();
			Kkm.PaperOver = !Atol5.getParamBool(65594);
			SessionOpen = (int)(Atol5.getParamInt(65592) + 1);
			bool paramBool = Atol5.getParamBool(65662);
			Atol5.setParam(65587, 2u);
			Atol5.setParam(65609, 1u);
			StatRun = Atol5.queryData();
			if (!IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения версии прошивки!"))
			{
				Kkm.Firmware_Version = Atol5.getParamString(65604).Trim();
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			PrintingWidths.Clear();
			for (int i = 0; i <= 4; i++)
			{
				Atol5.setParam(65587, 47u);
				Atol5.setParam(65539, i);
				StatRun = Atol5.queryData();
				int num = (IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!") ? MaxPrintingWidth : ((int)Atol5.getParamInt(65601)));
				if (i == 0)
				{
					PrintingWidths.Add(i, (0, num));
					MaxPrintingWidth = num;
					Kkm.PrintingWidth = num;
				}
				else
				{
					dictionary.Add(i, num);
				}
			}
			int num2 = 1;
			foreach (KeyValuePair<int, int> item in dictionary.OrderBy((KeyValuePair<int, int> x) => x.Value))
			{
				PrintingWidths.Add(num2++, (item.Key, item.Value));
			}
			if (DefPrintingWidth != 0)
			{
				MaxPrintingWidth = DefPrintingWidth;
				Kkm.PrintingWidth = DefPrintingWidth;
			}
			Kkm.IsKKT = true;
			Kkm.INN = "";
			Kkm.Organization = "<Не определено>";
			Kkm.TaxVariant = "";
			if (paramBool)
			{
				Atol5.setParam(65622, 0u);
				Atol5.setParam(65623, 1018u);
				StatRun = Atol5.fnQueryData();
				if (!IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения ИНН организации!"))
				{
					Kkm.INN = Atol5.getParamString(65624).Trim();
				}
				Atol5.setParam(65622, 0u);
				Atol5.setParam(65623, 1048u);
				StatRun = Atol5.fnQueryData();
				if (!IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения наименования организации!"))
				{
					Kkm.Organization = Atol5.getParamString(65624).Trim();
				}
				Atol5.setParam(65622, 0u);
				Atol5.setParam(65623, 1062u);
				StatRun = Atol5.fnQueryData();
				if (!IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения СНО!"))
				{
					byte b = (byte)Atol5.getParamInt(65624);
					for (int num3 = 0; num3 <= 5; num3++)
					{
						if (((b >> num3) & 1) == 1)
						{
							if (Kkm.TaxVariant != "")
							{
								Kkm.TaxVariant += ",";
							}
							Kkm.TaxVariant += num3;
						}
					}
				}
				Atol5.setParam(65622, 2u);
				StatRun = Atol5.fnQueryData();
				if (!IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения номера ФН!"))
				{
					Kkm.Fn_Number = Atol5.getParamString(65559).Trim();
				}
			}
			await ReadStatusOFD(FullInit);
			Nalogs.Clear();
			Nalogs.Add(-1, 6);
			Nalogs.Add(0, 5);
			Nalogs.Add(5, 9);
			Nalogs.Add(7, 10);
			Nalogs.Add(10, 2);
			Nalogs.Add(18, 1);
			Nalogs.Add(20, 7);
			Nalogs.Add(22, 13);
			Nalogs.Add(105, 11);
			Nalogs.Add(107, 12);
			Nalogs.Add(110, 4);
			Nalogs.Add(118, 3);
			Nalogs.Add(120, 8);
			Nalogs.Add(122, 14);
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
				string text = ChanelOfd;
				if (text == "")
				{
					text = ((SetPort.TypeConnect != SetPorts.enTypeConnect.IP) ? "EOT" : "Ethernet");
				}
				switch (text)
				{
				case "EOU":
					Atol5.setParam(65650, 276u);
					Atol5.setParam(65651, 1u);
					Atol5.writeDeviceSetting();
					break;
				case "Ethernet":
					Atol5.setParam(65650, 276u);
					Atol5.setParam(65651, 2u);
					Atol5.writeDeviceSetting();
					break;
				case "WiFi":
					Atol5.setParam(65650, 276u);
					Atol5.setParam(65651, 3u);
					Atol5.writeDeviceSetting();
					break;
				case "EOT":
					Atol5.setParam(65650, 276u);
					Atol5.setParam(65651, 5u);
					Atol5.writeDeviceSetting();
					break;
				}
				if (IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
				{
					Error = "";
				}
				Atol5.commitSettings();
				if (IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
				{
					Error = "";
				}
				int num4 = 0;
				if (UnitParamets.ContainsKey("PaymentCashOnClouseShift") && UnitParamets["PaymentCashOnClouseShift"] == "Zreport")
				{
					num4 = 1;
				}
				Atol5.setParam(65650, 4u);
				Atol5.setParam(65651, num4);
				Atol5.writeDeviceSetting();
				if (IsCommandBad(null, StatRun, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
				{
					Error = "";
				}
				Atol5.commitSettings();
				Atol5.setParam(65650, 372u);
				Atol5.setParam(65651, 0u);
				Atol5.writeDeviceSetting();
			}
			LastError = Error;
			IsFullInitDate = DateTime.Now;
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return true;
		}
		IsInit = false;
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		return false;
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task RegisterCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		await CloseDocumentAndOpenShift(DataCommand, RezultCommand);
		int value;
		if (DataCommand.IsFiscalCheck && (DataCommand.TaxVariant == "" || DataCommand.TaxVariant == null))
		{
			string[] array = Kkm.TaxVariant.Split(',');
			Atol5.setParam(65622, 0u);
			Atol5.setParam(65623, 1062u);
			value = Atol5.fnQueryData();
			if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Ошибка установки СНО по умолчанию: "))
			{
				return;
			}
			byte b = (byte)Atol5.getParamInt(65624);
			for (int i = 0; i <= 5; i++)
			{
				if ((byte)(1 << i) == b)
				{
					b = (byte)i;
					break;
				}
			}
			if (array.Contains(b.ToString()))
			{
				DataCommand.TaxVariant = b.ToString();
			}
			else
			{
				DataCommand.TaxVariant = array[0];
			}
		}
		byte[] value2 = new byte[0];
		if (DataCommand.TypeCheck == 2 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 12 || DataCommand.TypeCheck == 13)
		{
			Atol5.setParam(1178, DataCommand.CorrectionBaseDate.Value);
			string value3 = ((DataCommand.CorrectionBaseNumber != null && DataCommand.CorrectionBaseNumber != "") ? DataCommand.CorrectionBaseNumber : " ");
			Atol5.setParam(1179, value3);
			Atol5.utilFormTlv();
			value2 = Atol5.getParamByteArray(65624);
		}
		byte[] value4 = new byte[0];
		if (DataCommand.UserAttribute != null)
		{
			if (!string.IsNullOrEmpty(DataCommand.UserAttribute.Name))
			{
				Atol5.setParam(1085, DataCommand.UserAttribute.Name);
			}
			if (!string.IsNullOrEmpty(DataCommand.UserAttribute.Value))
			{
				Atol5.setParam(1086, DataCommand.UserAttribute.Value);
			}
			Atol5.utilFormTlv();
			value4 = Atol5.getParamByteArray(65624);
		}
		byte[] value5 = new byte[0];
		if (Kkm.FfdVersion >= 4 && ((DataCommand.ClientInfo != null && DataCommand.ClientInfo != "") || (DataCommand.ClientINN != null && DataCommand.ClientINN != "")))
		{
			if (DataCommand.ClientInfo != null && DataCommand.ClientInfo != "")
			{
				Atol5.setParam(1227, DataCommand.ClientInfo);
			}
			if (DataCommand.ClientINN != null && DataCommand.ClientINN != "")
			{
				Atol5.setParam(1228, DataCommand.ClientINN.PadRight(12, ' '));
			}
			Atol5.utilFormTlv();
			value5 = Atol5.getParamByteArray(65624);
		}
		if (DataCommand.NotPrint == true)
		{
			Atol5.setParam(65572, value: true);
		}
		if (DataCommand.IsFiscalCheck)
		{
			SerCashier(DataCommand);
		}
		if (Kkm.FfdVersion >= 4)
		{
			bool flag = (DataCommand.InternetMode.HasValue ? DataCommand.InternetMode.Value : Kkm.InternetMode);
			Atol5.setParam(1125, flag ? 1u : 0u);
			if (Error != "")
			{
				Error = "";
				Warning += "Не поддерживается передача поля InternetMode (1125); ";
			}
		}
		PortLogs.Append("Открытие чека", "-");
		Atol5.setParam(65743, value: true);
		if (DataCommand.NotPrint == true)
		{
			Atol5.setParam(65572, value: true);
		}
		bool IsCheckCorrection = false;
		int TypeCheck = 0;
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.TypeCheck == 0)
			{
				TypeCheck = 1;
			}
			else if (DataCommand.TypeCheck == 1)
			{
				TypeCheck = 2;
			}
			else if (DataCommand.TypeCheck == 2)
			{
				TypeCheck = 7;
				IsCheckCorrection = true;
			}
			else if (DataCommand.TypeCheck == 3)
			{
				TypeCheck = 8;
				IsCheckCorrection = true;
			}
			else if (DataCommand.TypeCheck == 10)
			{
				TypeCheck = 4;
			}
			else if (DataCommand.TypeCheck == 11)
			{
				TypeCheck = 5;
			}
			else if (DataCommand.TypeCheck == 12)
			{
				TypeCheck = 9;
				IsCheckCorrection = true;
			}
			else
			{
				if (DataCommand.TypeCheck != 13)
				{
					if (OpenSerial)
					{
						await PortCloseAsync();
					}
					return;
				}
				TypeCheck = 10;
				IsCheckCorrection = true;
			}
			if (Kkm.FfdVersion >= 4 && ((DataCommand.ClientInfo != null && DataCommand.ClientInfo != "") || (DataCommand.ClientINN != null && DataCommand.ClientINN != "")))
			{
				Atol5.setParam(1256, value5);
			}
			Atol5.setParam(65545, TypeCheck);
		}
		if (DataCommand.IsFiscalCheck)
		{
			try
			{
				int num = (byte)(1 << (int)byte.Parse(DataCommand.TaxVariant));
				Atol5.setParam(1055, num);
			}
			catch
			{
			}
		}
		if (Kkm.FfdVersion <= 3 && DataCommand.ClientInfo != null && DataCommand.ClientInfo != "" && !IsCheckCorrection)
		{
			Atol5.setParam(1227, DataCommand.ClientInfo);
		}
		if (Kkm.FfdVersion <= 3 && DataCommand.ClientINN != null && DataCommand.ClientINN != "" && !IsCheckCorrection)
		{
			Atol5.setParam(1228, DataCommand.ClientINN.PadRight(12, ' '));
		}
		if (DataCommand.IsFiscalCheck && (!IsCheckCorrection || Kkm.FfdVersion >= 3))
		{
			if (DataCommand.ClientAddress != null && DataCommand.ClientAddress != "")
			{
				Atol5.setParam(1008, DataCommand.ClientAddress);
			}
			if (DataCommand.SenderEmail != null && DataCommand.SenderEmail != "")
			{
				Atol5.setParam(1117, DataCommand.SenderEmail);
			}
			if (DataCommand.AddressSettle != null && DataCommand.AddressSettle != "")
			{
				Atol5.setParam(1009, DataCommand.AddressSettle);
			}
			if (DataCommand.PlaceMarket != null && DataCommand.PlaceMarket != "")
			{
				Atol5.setParam(1187, DataCommand.PlaceMarket);
			}
		}
		if (DataCommand.AgentSign.HasValue)
		{
			byte value6 = (byte)(1 << DataCommand.AgentSign).Value;
			Atol5.setParam(1057, value6);
		}
		if (DataCommand.AgentData != null)
		{
			if (!string.IsNullOrEmpty(DataCommand.AgentData.PayingAgentOperation))
			{
				Atol5.setParam(1044, DataCommand.AgentData.PayingAgentOperation);
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.PayingAgentPhone))
			{
				Atol5.setParam(1073, DataCommand.AgentData.PayingAgentPhone);
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.ReceivePaymentsOperatorPhone))
			{
				Atol5.setParam(1074, DataCommand.AgentData.ReceivePaymentsOperatorPhone);
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorPhone))
			{
				Atol5.setParam(1075, DataCommand.AgentData.MoneyTransferOperatorPhone);
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorName))
			{
				Atol5.setParam(1026, DataCommand.AgentData.MoneyTransferOperatorName);
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorAddress))
			{
				Atol5.setParam(1005, DataCommand.AgentData.MoneyTransferOperatorAddress);
			}
			if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorVATIN))
			{
				Atol5.setParam(1016, DataCommand.AgentData.MoneyTransferOperatorVATIN);
			}
		}
		if (DataCommand.PurveyorData != null && !string.IsNullOrEmpty(DataCommand.PurveyorData.PurveyorPhone))
		{
			Atol5.setParam(1171, DataCommand.PurveyorData.PurveyorPhone);
		}
		if (DataCommand.IsFiscalCheck && DataCommand.UserAttribute != null && !string.IsNullOrEmpty(DataCommand.UserAttribute.Name) && !string.IsNullOrEmpty(DataCommand.UserAttribute.Value))
		{
			Atol5.setParam(1084, value4);
		}
		if (!string.IsNullOrEmpty(DataCommand.AdditionalAttribute))
		{
			Atol5.setParam(1192, DataCommand.AdditionalAttribute);
		}
		if (IsCheckCorrection)
		{
			Atol5.setParam(1173, DataCommand.CorrectionType);
			Atol5.setParam(1174, value2);
		}
		if (DataCommand.IsFiscalCheck)
		{
			value = Atol5.openReceipt();
			ClearCheck = true;
		}
		else
		{
			value = Atol5.beginNonfiscalDocument();
		}
		if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалось открыть регистрацию чека"))
		{
			return;
		}
		await ComDevice.PostCheck(DataCommand, this);
		DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
		foreach (DataCommand.CheckString PrintString in checkStrings)
		{
			if (DataCommand.NotPrint == false && PrintString != null && PrintString.PrintImage != null)
			{
				PortLogs.Append("Печать картинки", "-");
				PrintImage(PrintString.PrintImage);
			}
			if (DataCommand.NotPrint == false && PrintString != null && PrintString.PrintText != null)
			{
				string text = PrintString.PrintText.Text;
				int item = PrintingWidths[PrintString.PrintText.Font].Width;
				text = Unit.GetPringString(text, item);
				string text2;
				do
				{
					if (text.Length > item)
					{
						text2 = text.Substring(item);
						text = text.Substring(0, item);
					}
					else
					{
						text2 = "";
					}
					Atol5.setParam(65536, text);
					Atol5.setParam(65539, PrintingWidths[PrintString.PrintText.Font].Font);
					Atol5.setParam(65543, PrintString.PrintText.Intensity);
					value = Atol5.printText();
					if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось напечатать не фискальную строку"))
					{
						return;
					}
					text = text2;
				}
				while (text2 != "");
			}
			if (DataCommand.IsFiscalCheck && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m)
			{
				if (IsCheckCorrection && Kkm.FfdVersion < 3)
				{
					continue;
				}
				PortLogs.Append("Регистрация фискальной строки", "-");
				List<DataCommand.Register> list = SplitRegisterString(PrintString);
				foreach (DataCommand.Register SplitStr in list)
				{
					decimal Amount = SplitStr.Amount;
					decimal price = SplitStr.Price;
					decimal quantity = SplitStr.Quantity;
					byte[] value7 = new byte[0];
					if (PrintString.Register.AgentData != null)
					{
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.PayingAgentOperation))
						{
							Atol5.setParam(1044, PrintString.Register.AgentData.PayingAgentOperation);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.PayingAgentPhone))
						{
							Atol5.setParam(1073, PrintString.Register.AgentData.PayingAgentPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.ReceivePaymentsOperatorPhone))
						{
							Atol5.setParam(1074, PrintString.Register.AgentData.ReceivePaymentsOperatorPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorPhone))
						{
							Atol5.setParam(1075, PrintString.Register.AgentData.MoneyTransferOperatorPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorName))
						{
							Atol5.setParam(1026, PrintString.Register.AgentData.MoneyTransferOperatorName);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorAddress))
						{
							Atol5.setParam(1005, PrintString.Register.AgentData.MoneyTransferOperatorAddress);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorVATIN))
						{
							Atol5.setParam(1016, PrintString.Register.AgentData.MoneyTransferOperatorVATIN.PadRight(12, ' '));
						}
						Atol5.utilFormTlv();
						value7 = Atol5.getParamByteArray(65624);
					}
					byte[] value8 = new byte[0];
					if (PrintString.Register.PurveyorData != null)
					{
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorPhone))
						{
							Atol5.setParam(1171, PrintString.Register.PurveyorData.PurveyorPhone);
						}
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorName))
						{
							Atol5.setParam(1225, PrintString.Register.PurveyorData.PurveyorName);
						}
						Atol5.utilFormTlv();
						value8 = Atol5.getParamByteArray(65624);
					}
					if (Kkm.FfdVersion <= 3 && PrintString.Register.MeasurementUnit != "" && PrintString.Register.MeasurementUnit != null)
					{
						Atol5.setParam(1197, PrintString.Register.MeasurementUnit);
					}
					if (Kkm.FfdVersion >= 4 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.IndustryProps))
					{
						Atol5.setParam(1262, PrintString.Register.GoodCodeData.Props1262);
						Atol5.setParam(1263, PrintString.Register.GoodCodeData.Props1263);
						Atol5.setParam(1264, PrintString.Register.GoodCodeData.Props1264);
						Atol5.setParam(1265, PrintString.Register.GoodCodeData.IndustryProps);
						Atol5.utilFormTlv();
						byte[] paramByteArray = Atol5.getParamByteArray(65624);
						Atol5.setParam(1260, paramByteArray);
					}
					if (Kkm.FfdVersion >= 4 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.BarCode))
					{
						int statusMarkingCode = GetStatusMarkingCode(DataCommand.TypeCheck, PrintString.Register.MeasureOfQuantity);
						RezultCommandKKm.tMarkingCodeValidation tMarkingCodeValidation = RezultCommand.MarkingCodeValidation.Find((RezultCommandKKm.tMarkingCodeValidation Item) => Item.BarCode == PrintString.Register.GoodCodeData.BarCode);
						Atol5.setParam(65760, PrintString.Register.GoodCodeData.TryBarCode);
						Atol5.setParam(65846, statusMarkingCode);
						Atol5.setParam(65826, 256u);
						Atol5.setParam(65886, tMarkingCodeValidation.ValidationKKT.ValidationResult);
						Atol5.setParam(65852, 0u);
					}
					if (Kkm.FfdVersion <= 3 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.MarkingCodeBase64))
					{
						Atol5.setParam(1162, Convert.FromBase64String(PrintString.Register.GoodCodeData.MarkingCodeBase64));
					}
					if (PrintString.Register.AgentSign.HasValue)
					{
						byte value9 = (byte)(1 << PrintString.Register.AgentSign).Value;
						Atol5.setParam(1222, value9);
					}
					if (PrintString.Register.AgentData != null)
					{
						Atol5.setParam(1223, value7);
					}
					if (PrintString.Register.PurveyorData != null)
					{
						Atol5.setParam(1224, value8);
						if (PrintString.Register.PurveyorData.PurveyorVATIN != null && PrintString.Register.PurveyorData.PurveyorVATIN != "")
						{
							Atol5.setParam(1226, PrintString.Register.PurveyorData.PurveyorVATIN.Trim());
						}
					}
					if (!string.IsNullOrEmpty(PrintString.Register.AdditionalAttribute))
					{
						Atol5.setParam(1191, PrintString.Register.AdditionalAttribute);
					}
					if (PrintString.Register.CountryOfOrigin != null && PrintString.Register.CountryOfOrigin != "")
					{
						Atol5.setParam(1230, PrintString.Register.CountryOfOrigin);
					}
					if (PrintString.Register.CustomsDeclaration != null && PrintString.Register.CustomsDeclaration != "")
					{
						Atol5.setParam(1231, PrintString.Register.CustomsDeclaration);
					}
					if (PrintString.Register.ExciseAmount.HasValue)
					{
						Atol5.setParam(1229, (double)PrintString.Register.ExciseAmount.Value);
					}
					Atol5.setParam(65631, PrintString.Register.Name);
					Atol5.setParam(65632, (double)price);
					if (PrintString.Register.GoodCodeData != null && PrintString.Register.PackageQuantity.HasValue)
					{
						Atol5.setParam(65633, (double)quantity);
						Atol5.setParam(65853, Math.Truncate(PrintString.Register.Quantity) + "/" + PrintString.Register.PackageQuantity);
					}
					else
					{
						Atol5.setParam(65633, (double)quantity);
					}
					if (Nalogs.ContainsKey((int)PrintString.Register.Tax))
					{
						Atol5.setParam(65569, Nalogs[(int)PrintString.Register.Tax]);
					}
					else
					{
						try
						{
							await ProcessInitDevice(FullInit: true);
						}
						catch
						{
						}
						Error = $"Ставка налога \"{PrintString.Register.Tax}\" не запрограммирована в ККМ";
					}
					Atol5.setParam(65634, (double)Amount);
					Atol5.setParam(65568, PrintString.Register.Department);
					if (PrintString.Register.SignCalculationObject.HasValue)
					{
						Atol5.setParam(1212, (uint)PrintString.Register.SignCalculationObject.Value);
					}
					else
					{
						Error = "Не указан признак предмета расчета! ККТ работает по ФФД 1.05/1.1 Признак обязателен!!";
					}
					if (PrintString.Register.SignMethodCalculation.HasValue)
					{
						Atol5.setParam(1214, (uint)PrintString.Register.SignMethodCalculation.Value);
					}
					else
					{
						Error = "Не указан признак способа расчета! ККТ работает по ФФД 1.05/1.1 Признак обязателен!!";
					}
					if (Kkm.FfdVersion >= 2)
					{
						Atol5.setParam(65851, PrintString.Register.MeasureOfQuantity.Value);
						if (Error != "")
						{
							Error = "";
							Warning += "Не поддерживается передача поля MeasureOfQuantity; ";
						}
					}
					value = Atol5.registration();
					if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось зарегистрировать фискальную строку"))
					{
						return;
					}
					if (SplitStr.StSkidka != "")
					{
						Atol5.setParam(65536, SplitStr.StSkidka);
						Atol5.setParam(65539, 3u);
						value = Atol5.printText();
						if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось напечатать информацию о скидке"))
						{
							return;
						}
					}
				}
			}
			if (DataCommand.NotPrint == false && PrintString != null && PrintString.BarCode != null && PrintString.BarCode.BarcodeType != "" && !PrintBarCode(PrintString.BarCode))
			{
				return;
			}
		}
		PortLogs.Append("Закрытие чека", "-");
		if (Kkm.FfdVersion < 3 && DataCommand.IsFiscalCheck && IsCheckCorrection && Kkm.FfdVersion >= 2)
		{
			decimal num2 = ((Kkm.FfdVersion < 2) ? (DataCommand.Cash + DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3 + DataCommand.AdvancePayment + DataCommand.Credit + DataCommand.CashProvision) : DataCommand.Amount);
			Atol5.setParam(65613, (double)num2);
			value = Atol5.receiptTotal();
			if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось передать сумму итога"))
			{
				return;
			}
			for (int num3 = 1; num3 <= 10; num3++)
			{
				num2 = default(decimal);
				int num4 = -2;
				switch (num3)
				{
				case 1:
					num2 = DataCommand.SumTax18;
					num4 = 18;
					break;
				case 2:
					num2 = DataCommand.SumTax10;
					num4 = 10;
					break;
				case 3:
					num2 = DataCommand.SumTax0;
					num4 = 0;
					break;
				case 4:
					num2 = default(decimal);
					num4 = -1;
					break;
				case 5:
					num2 = DataCommand.SumTax118;
					num4 = 118;
					break;
				case 6:
					num2 = DataCommand.SumTax110;
					num4 = 110;
					break;
				case 7:
					num2 = DataCommand.SumTax20;
					num4 = 20;
					break;
				case 8:
					num2 = DataCommand.SumTax120;
					num4 = 120;
					break;
				case 9:
					num2 = DataCommand.SumTax22;
					num4 = 22;
					break;
				case 10:
					num2 = DataCommand.SumTax122;
					num4 = 122;
					break;
				}
				if (num2 != 0m)
				{
					Atol5.setParam(65570, (double)num2);
					Atol5.setParam(65569, Nalogs[num4]);
					value = Atol5.receiptTax();
					if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось передать сумму НДС"))
					{
						return;
					}
				}
			}
		}
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType1 + DataCommand.CashLessType1 != 0m)
			{
				Atol5.setParam(65564, 1u);
				Atol5.setParam(65565, (double)(DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType1 + DataCommand.CashLessType1));
				value = Atol5.payment();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалась зарегистрировать оплату безналичными"))
				{
					return;
				}
			}
			if (DataCommand.AdvancePayment != 0m)
			{
				Atol5.setParam(65564, 2u);
				Atol5.setParam(65565, (double)DataCommand.AdvancePayment);
				value = Atol5.payment();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалась зарегистрировать оплату зачетом аванса"))
				{
					return;
				}
			}
			if (DataCommand.Credit != 0m)
			{
				Atol5.setParam(65564, 3u);
				Atol5.setParam(65565, (double)DataCommand.Credit);
				value = Atol5.payment();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалась зарегистрировать оплату в кредит"))
				{
					return;
				}
			}
			if (DataCommand.CashProvision != 0m)
			{
				Atol5.setParam(65564, 4u);
				Atol5.setParam(65565, (double)DataCommand.CashProvision);
				value = Atol5.payment();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалась зарегистрировать оплату встречным платежем"))
				{
					return;
				}
			}
			if (DataCommand.Cash != 0m)
			{
				Atol5.setParam(65564, 0u);
				Atol5.setParam(65565, (double)DataCommand.Cash);
				value = Atol5.payment();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалась зарегистрировать оплату наличными"))
				{
					return;
				}
			}
		}
		if (DataCommand.IsFiscalCheck)
		{
			value = Atol5.closeReceipt();
			if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			value = Atol5.checkDocumentClosed();
			if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			if (!Atol5.getParamBool(65644))
			{
				Error = "Не удалось закрыть чек";
				return;
			}
			ClearCheck = false;
		}
		else
		{
			value = Atol5.endNonfiscalDocument();
			if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
		}
		PortLogs.Append("Конец регистрации чека", "-");
		RezultCommand.Error = Error;
		IsNotErrorStatus = true;
		_ = DataCommand.IsFiscalCheck;
		if (DataCommand.IsFiscalCheck)
		{
			RezultCommand.QRCode = GetUrlDoc(ShekOrDoc: true, TypeCheck);
			RezultCommand.CheckNumber = CheckNumber;
			await GetCheckAndSession(RezultCommand, IsSessionNumber: true, IsCheckNumber: false);
		}
		for (int num5 = DataCommand.NumberCopies; num5 > 0; num5--)
		{
			Atol5.setParam(65546, 2u);
			Atol5.report();
		}
		if (DataCommand.Sound)
		{
			Atol5.beep();
		}
		if (Kkm.FfdVersion >= 4)
		{
			Atol5.clearMarkingCodeValidationResult();
		}
		if (DataCommand.IsFiscalCheck)
		{
			await GetCheckAndSession(RezultCommand, IsSessionNumber: true, IsCheckNumber: false);
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Error = "";
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
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		await CloseDocumentAndOpenShift(DataCommand, RezultCommand);
		if (Kkm.FfdVersion >= 4)
		{
			Atol5.clearMarkingCodeValidationResult();
		}
		foreach (DataCommand.GoodCodeData GoodCodeData in DataCommand.GoodCodeDatas)
		{
			RezultMarkingCodeValidation.tMarkingCodeValidation ItemValidation = RezultCommand.MarkingCodeValidation.Find((RezultMarkingCodeValidation.tMarkingCodeValidation i) => i.TryBarCode == GoodCodeData.TryBarCode);
			ItemValidation.ValidationKKT.ValidationResult = 1u;
			ItemValidation.ValidationKKT.DecryptionResult = GetMarkingCodeDecryptionResult(ItemValidation.ValidationKKT.ValidationResult);
			if (string.IsNullOrEmpty(GoodCodeData.TryBarCode))
			{
				continue;
			}
			Atol5.setParam(65826, 256u);
			Atol5.setParam(65760, GoodCodeData.TryBarCode);
			Atol5.setParam(65852, 0u);
			int statusMarkingCode = GetStatusMarkingCode(DataCommand.TypeCheck, GoodCodeData.MeasureOfQuantity);
			Atol5.setParam(65845, value: true);
			if (GoodCodeData.PackageQuantity.HasValue)
			{
				Atol5.setParam(65851, GoodCodeData.MeasureOfQuantity.Value);
				Atol5.setParam(65633, (uint)GoodCodeData.Quantity);
				Atol5.setParam(65853, Math.Truncate(GoodCodeData.Quantity) + "/" + GoodCodeData.PackageQuantity);
			}
			else if (GoodCodeData.MeasureOfQuantity != 0)
			{
				Atol5.setParam(65851, GoodCodeData.MeasureOfQuantity.Value);
				Atol5.setParam(65633, (uint)GoodCodeData.Quantity);
			}
			Atol5.setParam(65846, statusMarkingCode);
			int value = Atol5.beginMarkingCodeValidation();
			if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалось начать проверку кода маркировки"))
			{
				return;
			}
			await Task.Delay(200);
			int count = 1000;
			while (true)
			{
				value = Atol5.getMarkingCodeValidationStatus();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Ошибка при проверке кода маркировки"))
				{
					return;
				}
				if (Atol5.getParamBool(65850))
				{
					break;
				}
				await Task.Delay(100);
				if (count-- == 0)
				{
					throw new Exception("Очень долго нет ответа от ККТ по проверке кода маркировки! Проверьте настройки ОФД в ККТ. И проверьте что в ОФД для вас включена опция по работе с маркированными товарами");
				}
			}
			uint paramInt = Atol5.getParamInt(65886);
			uint paramInt2 = Atol5.getParamInt(65848);
			bool flag;
			if (!MarkingCodeIsBad(paramInt) || GoodCodeData.AcceptOnBad)
			{
				flag = true;
				value = Atol5.acceptMarkingCode();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Ошибка при подтверждении кода проверки маркировки"))
				{
					return;
				}
			}
			else
			{
				flag = false;
				value = Atol5.declineMarkingCode();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Ошибка при подтверждении кода проверки маркировки"))
				{
					return;
				}
			}
			ItemValidation.ValidationKKT.ValidationResult = paramInt;
			uint paramInt3 = Atol5.getParamInt(2105);
			uint paramInt4 = Atol5.getParamInt(2109);
			ItemValidation.ValidationKKT.DecryptionResult = GetMarkingCodeDecryptionResult(ItemValidation.ValidationKKT.ValidationResult, paramInt3, paramInt4, paramInt2);
			if (Atol5.getParamInt(65849) != 0)
			{
				RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationKKT validationKKT = ItemValidation.ValidationKKT;
				validationKKT.DecryptionResult = validationKKT.DecryptionResult + "; Ошибка: " + Atol5.getParamString(65887);
			}
			if (!flag && InCheck)
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
		uint OldLIBFPTR_PARAM_TIMEOUT = Atol5.getParamInt(65899);
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		if (Atol5.checkDocumentClosed() == 0)
		{
			if (!Atol5.getParamBool(65709))
			{
				Atol5.continuePrint();
			}
			if (!Atol5.getParamBool(65644))
			{
				Atol5.cancelReceipt();
			}
		}
		Atol5.resetError();
		if (Kkm.FfdVersion >= 4)
		{
			Atol5.setParam(65899, 120000u);
			Atol5.updateFnmKeys();
			Atol5.setParam(65899, OldLIBFPTR_PARAM_TIMEOUT);
			if (Atol5.getParamInt(65861) != 0)
			{
				Warning = "Не удалось обновить ключи КМ: " + Atol5.getParamString(65862);
			}
			Error = "";
			Atol5.resetError();
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		SerCashier(DataCommand);
		if (DataCommand.NotPrint == true)
		{
			Atol5.setParam(65749, value: true);
		}
		Atol5.setParam(65899, 120000u);
		int value = Atol5.openShift();
		Atol5.setParam(65899, OldLIBFPTR_PARAM_TIMEOUT);
		if (IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка начала открытия смены!"))
		{
			return;
		}
		value = Atol5.checkDocumentClosed();
		if (value == 0 && !Atol5.getParamBool(65709))
		{
			Atol5.continuePrint();
		}
		if (!IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Не удалось открыть смену"))
		{
			await GetCheckAndSession(RezultCommand);
			RezultCommand.QRCode = GetUrlDoc(ShekOrDoc: false);
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task CloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		CalkPrintOnPage(this, DataCommand, Repot: true);
		Error = "";
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		if (Atol5.checkDocumentClosed() == 0)
		{
			if (!Atol5.getParamBool(65709))
			{
				Atol5.continuePrint();
			}
			if (!Atol5.getParamBool(65644))
			{
				Atol5.cancelReceipt();
			}
		}
		Atol5.resetError();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		await GetCheckAndSession(RezultCommand);
		SerCashier(DataCommand);
		if (DataCommand.NotPrint == true)
		{
			Atol5.setParam(65749, value: true);
		}
		Atol5.setParam(65546, 0u);
		int value = Atol5.report();
		if (IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка начала закрытия смены!"))
		{
			return;
		}
		SerCashier(DataCommand, Command: false);
		value = Atol5.checkDocumentClosed();
		if (value == 0 && !Atol5.getParamBool(65709))
		{
			Atol5.continuePrint();
		}
		if (IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Не удалось открыть смену"))
		{
			return;
		}
		await GetCheckAndSession(RezultCommand, IsSessionNumber: false, IsCheckNumber: false);
		RezultCommand.QRCode = GetUrlDoc(ShekOrDoc: false);
		if (SettDr.Paramets["SetDateTime"].AsBool())
		{
			Atol5.setParam(65590, DateTime.Now);
			if (Atol5.writeDateTime() != 0)
			{
				Warning += "Ошибка установки даты-времени в ККТ";
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
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		Atol5.cancelReceipt();
		Atol5.resetError();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		SerCashier(DataCommand);
		Atol5.setParam(65546, 1u);
		int value = Atol5.report();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Не удалось Напечатать X отчет"))
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
		Error = "";
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		Atol5.cancelReceipt();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		SerCashier(DataCommand);
		Atol5.setParam(65546, 3u);
		int value = Atol5.report();
		if (IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Не удалось Напечатать отчет"))
		{
			return;
		}
		Atol5.setParam(65546, 6u);
		value = Atol5.report();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Не удалось Напечатать отчет"))
		{
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
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		int value = Atol5.openDrawer();
		if (!IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалось Открыть денежный ящик"))
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
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		Atol5.cancelReceipt();
		Atol5.resetError();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		SerCashier(DataCommand);
		if (DataCommand.NotPrint == true)
		{
			Atol5.setParam(65749, value: true);
		}
		Atol5.setParam(65613, (double)DataCommand.Amount);
		int value = Atol5.cashIncome();
		if (!IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалась операция Внесения денег"))
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
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		Atol5.cancelReceipt();
		Atol5.resetError();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		SerCashier(DataCommand);
		if (DataCommand.NotPrint == true)
		{
			Atol5.setParam(65749, value: true);
		}
		Atol5.setParam(65613, (double)DataCommand.Amount);
		int value = Atol5.cashOutcome();
		if (!IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалась операция Изъятие денег"))
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
		RezultCommand.LineLength = Kkm.PrintingWidth;
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public override async Task KkmRegOfd(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		Error = "";
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		Atol5.cancelReceipt();
		Atol5.resetError();
		await ProcessInitDevice(FullInit: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		if (SessionOpen == 2 || SessionOpen == 3)
		{
			RezultCommand.Status = ExecuteStatus.Error;
			Error = "Нельзя выполнить команду при открытой смене.";
		}
		Atol5.setParam(65590, DateTime.Now);
		if (Atol5.writeDateTime() != 0)
		{
			Warning += "Ошибка установки даты-времени в ККТ";
		}
		int value;
		if (DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "Open")
		{
			if (DataCommand.RegKkmOfd.UrlServerOfd != "")
			{
				Atol5.setParam(65650, 273u);
				Atol5.setParam(65651, DataCommand.RegKkmOfd.UrlServerOfd);
				value = Atol5.writeDeviceSetting();
				IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка записи UrlServerOfd!");
			}
			if (DataCommand.RegKkmOfd.PortServerOfd != "")
			{
				Atol5.setParam(65650, 274u);
				Atol5.setParam(65651, DataCommand.RegKkmOfd.PortServerOfd);
				value = Atol5.writeDeviceSetting();
				IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка записи PortServerOfd!");
			}
			_ = DataCommand.RegKkmOfd.UrlOfd != "";
		}
		SerCashier(DataCommand);
		if (DataCommand.RegKkmOfd.Command == "Open")
		{
			Atol5.setParam(65647, 0u);
		}
		else if (DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "ChangeOrganization")
		{
			Atol5.setParam(65647, 2u);
		}
		else if (DataCommand.RegKkmOfd.Command == "ChangeFN")
		{
			Atol5.setParam(65647, 1u);
		}
		else if (DataCommand.RegKkmOfd.Command == "Close")
		{
			Atol5.setParam(65647, 3u);
		}
		if (DataCommand.RegKkmOfd.Command != "Close")
		{
			if (DataCommand.RegKkmOfd.Command == "Open")
			{
				Atol5.setParam(1018, DataCommand.RegKkmOfd.InnOrganization.Trim());
				Atol5.setParam(1037, DataCommand.RegKkmOfd.RegNumber.Trim());
			}
			Atol5.setParam(1017, DataCommand.RegKkmOfd.InnOfd.Trim());
			Atol5.setParam(1046, DataCommand.RegKkmOfd.NameOFD.Trim());
			Atol5.setParam(1048, DataCommand.RegKkmOfd.NameOrganization.Trim());
			Atol5.setParam(1009, DataCommand.RegKkmOfd.AddressSettle.Trim());
			Atol5.setParam(1187, DataCommand.RegKkmOfd.PlaceSettle.Trim());
			Atol5.setParam(1117, DataCommand.RegKkmOfd.SenderEmail.Trim());
			byte b = 0;
			string[] array = DataCommand.RegKkmOfd.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0)
			{
				array = Kkm.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			}
			string[] array2 = array;
			foreach (string s in array2)
			{
				b = (byte)(b + (1 << int.Parse(s)));
			}
			Atol5.setParam(1062, (int)b);
			byte b2 = 0;
			array2 = DataCommand.RegKkmOfd.SignOfAgent.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s2 in array2)
			{
				b2 = (byte)(b2 + (1 << int.Parse(s2)));
			}
			Atol5.setParam(1057, (int)b2);
			Atol5.setParam(1060, "www.nalog.gov.ru");
			int num = 0;
			switch (DataCommand.RegKkmOfd.SetFfdVersion)
			{
			case 1:
				num = 100;
				break;
			case 2:
				num = 105;
				break;
			case 3:
				num = 110;
				break;
			case 4:
				num = 120;
				break;
			}
			Atol5.setParam(1209, num);
			Atol5.setParam(1002, DataCommand.RegKkmOfd.OfflineMode);
			Atol5.setParam(1056, DataCommand.RegKkmOfd.EncryptionMode);
			Atol5.setParam(1109, DataCommand.RegKkmOfd.ServiceMode);
			Atol5.setParam(1110, DataCommand.RegKkmOfd.BSOMode);
			Atol5.setParam(1001, DataCommand.RegKkmOfd.AutomaticMode);
			Atol5.setParam(1108, DataCommand.RegKkmOfd.InternetMode);
			Atol5.setParam(1036, DataCommand.RegKkmOfd.AutomaticNumber);
			Atol5.setParam(1221, DataCommand.RegKkmOfd.PrinterAutomatic);
			Atol5.setParam(1193, DataCommand.RegKkmOfd.SignOfGambling);
			Atol5.setParam(1126, DataCommand.RegKkmOfd.SignOfLottery);
			Atol5.setParam(1207, DataCommand.RegKkmOfd.SaleExcisableGoods);
			if (Kkm.FfdSupportVersion >= 4)
			{
				Atol5.setParam(65855, DataCommand.RegKkmOfd.SaleMarking);
				Atol5.setParam(65857, DataCommand.RegKkmOfd.SignPawnshop);
				Atol5.setParam(65856, DataCommand.RegKkmOfd.SignAssurance);
			}
		}
		if (DataCommand.RegKkmOfd.SetFfdVersion >= 3 && (DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "ChangeOFD" || DataCommand.RegKkmOfd.Command == "ChangeOrganization"))
		{
			int codeChangeKkmReg = GetCodeChangeKkmReg(DataCommand.RegKkmOfd);
			Atol5.setParam(1205, codeChangeKkmReg);
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeKkm")
		{
			Atol5.setParam(1101, 4u);
		}
		else if (DataCommand.RegKkmOfd.Command == "ChangeOFD")
		{
			Atol5.setParam(1101, 2u);
		}
		else if (DataCommand.RegKkmOfd.Command == "ChangeOrganization")
		{
			Atol5.setParam(1101, 3u);
		}
		else if (DataCommand.RegKkmOfd.Command == "Open")
		{
			Atol5.setParam(1101, 0u);
		}
		value = Atol5.fnOperation();
		if (!IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалось выполнить команду регистрации: "))
		{
			await ProcessInitDevice(FullInit: true);
			await ReadStatusOFD(Full: true);
			DateTime dateTime = default(DateTime);
			uint num2 = 0u;
			string text = "";
			Atol5.setParam(65622, 3u);
			value = Atol5.fnQueryData();
			if (!IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка чтения данных первой регистрации!"))
			{
				dateTime = Atol5.getParamDateTime(65590);
				num2 = Atol5.getParamInt(65598);
				text = Atol5.getParamString(65626).Trim();
			}
			RezultCommand.QRCode = "Дата: " + dateTime.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + num2.ToString("D0") + ", ФПД: " + text;
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task GetDataKKT(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		Atol5.cancelReceipt();
		Atol5.resetError();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		await ReadStatusOFD(Full: true);
		Error = "";
		Atol5.setParam(65587, 0u);
		int value = Atol5.queryData();
		if (!IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
		{
			NameDevice = Atol5.getParamString(65603).Trim();
			IdModel = (int)Atol5.getParamInt(65544);
			if (DefPrintingWidth != 0)
			{
				MaxPrintingWidth = DefPrintingWidth;
				Kkm.PrintingWidth = DefPrintingWidth;
			}
			Kkm.NumberKkm = Atol5.getParamString(65559).Trim();
			Kkm.PaperOver = !Atol5.getParamBool(65594);
			SessionOpen = (int)(Atol5.getParamInt(65592) + 1);
			Kkm.DateTimeKKT = Atol5.getParamDateTime(65590);
			Atol5.setParam(65587, 2u);
			Atol5.setParam(65609, 1u);
			Atol5.queryData();
			Kkm.Firmware_Version = Atol5.getParamString(65604).Trim();
			if (Global.Settings.SetNotActiveOnPaperOver)
			{
				IsInit = !Kkm.PaperOver;
			}
			await base.GetDataKKT(DataCommand, RezultCommand);
			await GetCheckAndSession(RezultCommand);
			RezultCommand.Info.SessionState = SessionOpen;
			Atol5.setParam(65587, 1u);
			value = Atol5.queryData();
			if (!IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка чтения баланса наличных!"))
			{
				double paramDouble = Atol5.getParamDouble(65613);
				RezultCommand.Info.BalanceCash = (decimal)paramDouble;
			}
			else
			{
				Error = "";
				RezultCommand.Info.BalanceCash = default(decimal);
			}
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task GetCounters(DataCommand DataCommand, RezultCounters RezultCommand)
	{
		Error = "";
		Error = "";
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return;
		}
		string[] array = "Total,Shift".Split(',');
		string[] array2 = "Shell,ShellReturn,Buy,BuyReturn".Split(',');
		string[] array3 = array;
		foreach (string text in array3)
		{
			string[] array4 = array2;
			foreach (string text2 in array4)
			{
				Atol5.setParam(65622, 18u);
				if (!(text == "Total"))
				{
					if (text == "Shift")
					{
						Atol5.setParam(65816, 0u);
					}
				}
				else
				{
					Atol5.setParam(65816, 1u);
				}
				switch (text2)
				{
				case "Shell":
					Atol5.setParam(65545, 1u);
					break;
				case "ShellReturn":
					Atol5.setParam(65545, 2u);
					break;
				case "Buy":
					Atol5.setParam(65545, 4u);
					break;
				case "BuyReturn":
					Atol5.setParam(65545, 5u);
					break;
				}
				int value = Atol5.fnQueryData();
				if (IsCommandBad(RezultCommand, value, OpenSerial, fClearCheck: false, "Не удалось получить счетчик"))
				{
					return;
				}
				RezultCounters.tСounter tСounter = new RezultCounters.tСounter();
				tСounter.CountersType = text;
				tСounter.ReceiptType = text2;
				tСounter.Count = Atol5.getParamInt(65802);
				tСounter.Sum = (decimal)Atol5.getParamDouble(65820);
				tСounter.Cash = (decimal)Atol5.getParamDouble(65803);
				tСounter.ElectronicPayment = (decimal)Atol5.getParamDouble(65804);
				tСounter.AdvancePayment = (decimal)Atol5.getParamDouble(65805);
				tСounter.Credit = (decimal)Atol5.getParamDouble(65806);
				tСounter.CashProvision = (decimal)Atol5.getParamDouble(65807);
				tСounter.Tax22 = (decimal)Atol5.getParamDouble(65808);
				tСounter.Tax10 = (decimal)Atol5.getParamDouble(65810);
				tСounter.Tax0 = (decimal)Atol5.getParamDouble(65809);
				tСounter.TaxNo = (decimal)Atol5.getParamDouble(65811);
				tСounter.Tax122 = (decimal)Atol5.getParamDouble(65812);
				tСounter.Tax110 = (decimal)Atol5.getParamDouble(65813);
				tСounter.CorrectionsCount = Atol5.getParamInt(65814);
				tСounter.CorrectionsSum = (decimal)Atol5.getParamDouble(65815);
				RezultCommand.Counters.Add(tСounter);
			}
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
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
		bool flag = await PortOpenAsync(ClearCheck: true);
		if (!IsCommandBad(RezultCommand, null, flag, fClearCheck: false, ""))
		{
			Atol5.deviceReboot();
			if (flag)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task<bool> CloseDocumentAndOpenShift(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		bool OpenSerial = await PortOpenAsync(ClearCheck: true);
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return false;
		}
		if (Atol5.checkDocumentClosed() == 0)
		{
			if (!Atol5.getParamBool(65709))
			{
				Atol5.continuePrint();
			}
			if (!Atol5.getParamBool(65644))
			{
				Atol5.cancelReceipt();
			}
		}
		Atol5.resetError();
		if (!IsInit)
		{
			await ProcessInitDevice();
		}
		if (IsCommandBad(RezultCommand, null, OpenSerial, fClearCheck: false, ""))
		{
			return false;
		}
		Atol5.clearPictures();
		if (DataCommand.IsFiscalCheck)
		{
			Atol5.setParam(65587, 0u);
			int value = Atol5.queryData();
			if (!IsCommandBad(null, value, OpenSerial, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
			{
				SessionOpen = (int)(Atol5.getParamInt(65592) + 1);
			}
			if (SessionOpen == 1)
			{
				await OpenShift(DataCommand, new RezultCommandKKm());
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
		Error = "";
		bool flag = await PortOpenAsync();
		if (IsCommandBad(null, null, flag, fClearCheck: false, ""))
		{
			return;
		}
		bool flag2 = false;
		Atol5.setParam(65587, 0u);
		int value = Atol5.queryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения информации о ККТ!"))
		{
			flag2 = Atol5.getParamBool(65662);
		}
		Error = "";
		Kkm.IsKKT = true;
		Kkm.INN = "";
		Kkm.Organization = "<Не определено>";
		Kkm.TaxVariant = "";
		Kkm.FN_Status = 0;
		Kkm.FN_IsFiscal = false;
		Atol5.setParam(65622, 2u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения номера ФН!"))
		{
			Kkm.Fn_Number = Atol5.getParamString(65559).Trim();
			Kkm.FN_Status = (byte)Atol5.getParamInt(65648);
			if (Kkm.FN_Status == 3)
			{
				Kkm.FN_IsFiscal = true;
			}
			Kkm.FN_MemOverflowl = Atol5.getParamBool(65688);
			if ((Kkm.FN_Status & 4) == 4)
			{
				Kkm.FN_MemOverflowl = true;
			}
		}
		Error = "";
		Kkm.FfdVersion = 1;
		Atol5.setParam(65622, 0u);
		Atol5.setParam(65623, 1209u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения версии ФФД!"))
		{
			switch ((int)Atol5.getParamInt(65624))
			{
			case 100:
				Kkm.FfdVersion = 1;
				break;
			case 105:
				Kkm.FfdVersion = 2;
				break;
			case 110:
				Kkm.FfdVersion = 3;
				break;
			case 120:
				Kkm.FfdVersion = 4;
				break;
			}
		}
		Error = "";
		Kkm.FfdSupportVersion = Kkm.FfdVersion;
		Kkm.FfdMinimumVersion = Kkm.FfdVersion;
		Atol5.setParam(65622, 7u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения версии ФФД!"))
		{
			switch ((int)Atol5.getParamInt(65693))
			{
			case 100:
				Kkm.FfdSupportVersion = 1;
				break;
			case 105:
				Kkm.FfdSupportVersion = 2;
				break;
			case 110:
				Kkm.FfdSupportVersion = 3;
				break;
			case 120:
				Kkm.FfdSupportVersion = 4;
				break;
			}
			switch ((int)Atol5.getParamInt(65692))
			{
			case 100:
				Kkm.FfdMinimumVersion = 1;
				break;
			case 105:
				Kkm.FfdMinimumVersion = 2;
				break;
			case 110:
				Kkm.FfdMinimumVersion = 3;
				break;
			case 120:
				Kkm.FfdMinimumVersion = 4;
				break;
			}
		}
		Error = "";
		Atol5.setParam(65622, 0u);
		Atol5.setParam(65623, 1018u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения ИНН организации!"))
		{
			Kkm.INN = Atol5.getParamString(65624).Trim();
		}
		Error = "";
		Atol5.setParam(65622, 0u);
		Atol5.setParam(65623, 1048u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения наименования организации!"))
		{
			Kkm.Organization = Atol5.getParamString(65624).Trim();
		}
		Error = "";
		Atol5.setParam(65622, 0u);
		Atol5.setParam(65623, 1062u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения СНО!"))
		{
			byte b = (byte)Atol5.getParamInt(65624);
			for (int i = 0; i <= 5; i++)
			{
				if (((b >> i) & 1) == 1)
				{
					if (Kkm.TaxVariant != "")
					{
						Kkm.TaxVariant += ",";
					}
					Kkm.TaxVariant += i;
				}
			}
		}
		Error = "";
		Atol5.setParam(65622, 0u);
		Atol5.setParam(65623, 1060u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения URL налоговой!"))
		{
			Kkm.UrlTax = Atol5.getParamString(65624).Trim();
		}
		Error = "";
		Atol5.setParam(65622, 9u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения PrinterAutomatic!"))
		{
			Kkm.InnOfd = Atol5.getParamString(1017).Trim();
			Kkm.NameOFD = Atol5.getParamString(1046).Trim();
			Kkm.SenderEmail = Atol5.getParamString(1117).Trim();
			Kkm.AddressSettle = Atol5.getParamString(1009).Trim();
			Kkm.PlaceSettle = Atol5.getParamString(1187).Trim();
			Kkm.RegNumber = Atol5.getParamString(1037).Trim();
			Kkm.AutomaticNumber = Atol5.getParamString(1036).Trim();
			Kkm.EncryptionMode = Atol5.getParamBool(1056);
			Kkm.OfflineMode = Atol5.getParamBool(1002);
			Kkm.AutomaticMode = Atol5.getParamBool(1001);
			Kkm.ServiceMode = Atol5.getParamBool(1109);
			Kkm.BSOMode = Atol5.getParamBool(1110);
			Kkm.InternetMode = Atol5.getParamBool(1108);
			Kkm.SignOfGambling = Atol5.getParamBool(1193);
			Kkm.SignOfLottery = Atol5.getParamBool(1126);
			Kkm.SaleExcisableGoods = Atol5.getParamBool(1207);
			Kkm.PrinterAutomatic = Atol5.getParamBool(1221);
			if (Kkm.FfdSupportVersion >= 4)
			{
				Kkm.SaleMarking = Atol5.getParamBool(65855);
				Kkm.SignPawnshop = Atol5.getParamBool(65857);
				Kkm.SignAssurance = Atol5.getParamBool(65856);
			}
		}
		Error = "";
		Atol5.setParam(65622, 0u);
		Atol5.setParam(65623, 1057u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения SignOfAgent!"))
		{
			byte b2 = (byte)Atol5.getParamInt(65624);
			Kkm.SignOfAgent = "";
			for (int j = 0; j <= 6; j++)
			{
				if (((b2 >> j) & 1) == 1)
				{
					if (Kkm.SignOfAgent != "")
					{
						Kkm.SignOfAgent += ",";
					}
					Kkm.SignOfAgent += j;
				}
			}
		}
		Error = "";
		Atol5.setParam(65650, 273u);
		value = Atol5.readDeviceSetting();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения UrlServerOfd!"))
		{
			Kkm.UrlServerOfd = Atol5.getParamString(65651).Trim();
		}
		Error = "";
		Atol5.setParam(65650, 274u);
		value = Atol5.readDeviceSetting();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения PortServerOfd!"))
		{
			Kkm.PortServerOfd = Atol5.getParamInt(65651).ToString();
		}
		Error = "";
		Kkm.UrlOfd = "";
		Atol5.setParam(65622, 8u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения FN_DateEnd!"))
		{
			Kkm.FN_DateEnd = Atol5.getParamDateTime(65590);
		}
		Error = "";
		Kkm.OFD_NumErrorDoc = 0;
		Kkm.OFD_DateErrorDoc = default(DateTime);
		Atol5.setParam(65622, 1u);
		value = Atol5.fnQueryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения статуса обмена!"))
		{
			Atol5.setParam(65622, 1u);
			Atol5.fnQueryData();
			Kkm.OFD_NumErrorDoc = (int)Atol5.getParamInt(65625);
			Kkm.OFD_DateErrorDoc = Atol5.getParamDateTime(65590);
		}
		Error = "";
		Kkm.InfoRegKkt = "Ошибка чтения параметров регистрации";
		if (flag2)
		{
			DateTime dateTime = default(DateTime);
			uint num = 0u;
			string text = "";
			Atol5.setParam(65622, 13u);
			Atol5.setParam(65598, 1u);
			value = Atol5.fnQueryData();
			if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения данных первой регистрации!"))
			{
				dateTime = Atol5.getParamDateTime(65590);
				num = Atol5.getParamInt(65598);
				text = Atol5.getParamString(65626).Trim();
			}
			Error = "";
			Kkm.InfoRegKkt = "Дата: " + dateTime.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + num.ToString("D0") + ", ФПД: " + text;
			Kkm.FN_DateStart = dateTime.Date;
		}
		if (flag)
		{
			await PortCloseAsync();
		}
	}

	public override async Task GetCheckAndSession(RezultCommandKKm RezultCommand, bool IsSessionNumber = true, bool IsCheckNumber = true)
	{
		Atol5.setParam(65587, 0u);
		int value = Atol5.queryData();
		if (!IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка чтения номера чека и смены!"))
		{
			long checkNumber = Atol5.getParamInt(65597);
			int paramInt = (int)Atol5.getParamInt(65599);
			if (IsSessionNumber)
			{
				RezultCommand.SessionNumber = paramInt;
			}
			if (IsCheckNumber)
			{
				RezultCommand.CheckNumber = checkNumber;
			}
		}
	}

	public void SerCashier(DataCommand DataCommand, bool Command = true)
	{
		if (DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			WriteProp(1021, DataCommand.CashierName, 64);
		}
		if (Kkm.FfdVersion >= 2 && DataCommand.CashierVATIN != null && DataCommand.CashierVATIN != "")
		{
			WriteProp(1203, DataCommand.CashierVATIN, 12);
		}
		else
		{
			WriteProp(1203, "", 12);
		}
		if (Command)
		{
			int value = Atol5.operatorLogin();
			IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка установка кассира!");
			Error = "";
		}
	}

	public int MaxLenghtProp(int Teg, ref object Prop)
	{
		int result = -1;
		if (Prop.GetType() == typeof(string))
		{
			switch (Teg)
			{
			case 1005:
				result = 256;
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
			case 1162:
				result = 32;
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
			case 1117:
				result = 64;
				break;
			case 1119:
				result = 19;
				break;
			case 1171:
				result = 19;
				break;
			case 1179:
				result = 32;
				break;
			case 1187:
				result = 255;
				break;
			case 1197:
				result = 16;
				break;
			case 1203:
				result = 12;
				break;
			case 1225:
				result = 256;
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

	public void WriteProp(int Teg, object Prop, int MaxLenght = -1, int Fill = 0)
	{
		if (Prop == null)
		{
			return;
		}
		if (MaxLenght == -1)
		{
			MaxLenght = MaxLenghtProp(Teg, ref Prop);
		}
		if (MaxLenght == -1 && IsCommandBad(null, null, OpenSerial: false, fClearCheck: false, "Попытка записать не разрешенный тег"))
		{
			return;
		}
		if (Prop.GetType() == typeof(string))
		{
			if (MaxLenght == 0)
			{
				MaxLenght = (Prop as string).Length;
			}
			if ((Prop as string).Length < MaxLenght)
			{
				MaxLenght = (Prop as string).Length;
			}
			if ((Prop as string).Length > MaxLenght)
			{
				Prop = (Prop as string).Substring(0, MaxLenght);
			}
		}
		if (Prop.GetType() == typeof(string))
		{
			Atol5.setParam(Teg, (string)Prop);
		}
		else if (Prop.GetType() == typeof(int))
		{
			Atol5.setParam(Teg, (int)Prop);
		}
		else if (Prop.GetType() == typeof(uint))
		{
			Atol5.setParam(Teg, (uint)Prop);
		}
		else if (Prop.GetType() == typeof(short))
		{
			Atol5.setParam(Teg, (short)Prop);
		}
		else if (Prop.GetType() == typeof(long))
		{
			Atol5.setParam(Teg, (long)Prop);
		}
		else if (Prop.GetType() == typeof(double))
		{
			Atol5.setParam(Teg, (double)Prop);
		}
		else if (Prop.GetType() == typeof(decimal))
		{
			Atol5.setParam(Teg, (double)(decimal)Prop);
		}
		else if (Prop.GetType() == typeof(byte))
		{
			Atol5.setParam(Teg, (byte)Prop);
		}
		else if (Prop.GetType() == typeof(byte[]))
		{
			Atol5.setParam(Teg, (byte[])Prop);
		}
	}

	public string GetUrlDoc(bool ShekOrDoc, int LastCheckType = 0)
	{
		if (UnitParamets["NoReadQrCode"].AsBool())
		{
			return "";
		}
		string result = "";
		if (ShekOrDoc)
		{
			DateTime dateTime = default(DateTime);
			uint num = 0u;
			string text = "";
			decimal num2 = default(decimal);
			Atol5.setParam(65622, 4u);
			if (Atol5.fnQueryData() != 0)
			{
				Warning += "Ошибка запроса документа; ";
				result = "Ошибка чтения регистра данных докумнета";
			}
			dateTime = Atol5.getParamDateTime(65590);
			num = Atol5.getParamInt(65598);
			text = Atol5.getParamString(65626).Trim();
			num2 = (decimal)Atol5.getParamDouble(65600);
			result = "t=" + dateTime.ToString("yyyyMMddTHHmm") + "&s=" + num2.ToString("0.00").Replace(',', '.') + "&fn=" + Kkm.Fn_Number + "&i=" + num.ToString("D0") + "&fp=" + text + "&n=" + LastCheckType;
			CheckNumber = num;
		}
		if (!ShekOrDoc)
		{
			DateTime dateTime2 = default(DateTime);
			uint num3 = 0u;
			string text2 = "";
			Atol5.setParam(65622, 5u);
			if (Atol5.fnQueryData() != 0)
			{
				Warning += "Ошибка запроса документа; ";
				result = "Ошибка чтения регистра данных докумнета";
			}
			dateTime2 = Atol5.getParamDateTime(65590);
			num3 = Atol5.getParamInt(65598);
			text2 = Atol5.getParamString(65626).Trim();
			result = "t=" + dateTime2.ToString("yyyyMMddTHHmm") + "&fn=" + Kkm.Fn_Number + "&i=" + num3.ToString("D0") + "&fp=" + text2;
		}
		return result;
	}

	public bool PrintBarCode(DataCommand.PrintBarcode PrintBarCode)
	{
		int num = 0;
		int num2 = 0;
		switch (PrintBarCode.BarcodeType)
		{
		case "EAN8":
			num2 = 0;
			Atol5.setParam(65578, value: true);
			break;
		case "EAN13":
			num2 = 1;
			Atol5.setParam(65578, value: true);
			break;
		case "CODE39":
			num2 = 4;
			break;
		case "CODE128":
			num2 = 6;
			break;
		case "QR":
			num2 = 11;
			Atol5.setParam(65574, 4u);
			break;
		case "PDF417":
			num2 = 12;
			Atol5.setParam(65581, 5u);
			break;
		default:
			num2 = 0;
			break;
		}
		Atol5.setParam(65576, PrintBarCode.Barcode);
		Atol5.setParam(65577, num2);
		Atol5.setParam(65538, 1u);
		num = Atol5.printBarcode();
		if (IsCommandBad(null, num, OpenSerial: false, fClearCheck: false, ""))
		{
			Error = "";
		}
		return true;
	}

	public bool PrintImage(DataCommand.PrintImage PrintImage)
	{
		int num = 0;
		Image<Rgba32> image = BarCode.ImageFromBase64(PrintImage.Image);
		byte[] array = new byte[image.Width * image.Height];
		int num2 = 0;
		for (int i = 0; i < image.Height; i++)
		{
			for (int j = 0; j < image.Width; j++)
			{
				int num3 = ((!((double)BarCode.GetBrightness(image[j, i]) > 0.5)) ? 1 : 0);
				array[num2++] = (byte)num3;
			}
		}
		Atol5.setParam(65757, array);
		Atol5.setParam(65584, image.Width);
		Atol5.setParam(65538, 1u);
		num = Atol5.printPixelBuffer();
		if (IsCommandBad(null, num, OpenSerial: false, fClearCheck: false, ""))
		{
			Error = "";
		}
		return true;
	}

	public override async Task<uint> GetLastFiscalNumber()
	{
		bool flag = await PortOpenAsync();
		if (IsCommandBad(null, null, flag, fClearCheck: false, ""))
		{
			return 0u;
		}
		Atol5.setParam(65622, 4u);
		int value = Atol5.fnQueryData();
		if (IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, ""))
		{
			Error = "";
		}
		uint Rez = Atol5.getParamInt(65598);
		if (flag)
		{
			await PortCloseAsync();
		}
		return Rez;
	}

	public override async Task<Dictionary<int, string>> GetRegisterCheck(uint FiscalNumber, Dictionary<int, Type> Types)
	{
		bool flag = await PortOpenAsync();
		if (IsCommandBad(null, null, flag, fClearCheck: false, ""))
		{
			return null;
		}
		Dictionary<int, string> Rez = new Dictionary<int, string>();
		List<Dictionary<int, string>> list = new List<Dictionary<int, string>>();
		Rez.Add(1059, list.AsString());
		Atol5.setParam(65668, 1u);
		Atol5.setParam(65598, (int)FiscalNumber);
		int value = Atol5.beginReadRecords();
		if (IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка получения документа из ФН"))
		{
			return null;
		}
		Rez.Add(0, Atol5.getParamInt(65697).AsString());
		int paramInt = (int)Atol5.getParamInt(65614);
		List<byte[]> list2 = new List<byte[]>();
		for (int i = 0; i < paramInt; i++)
		{
			value = Atol5.readNextRecord();
			if (IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка получения документа из ФН"))
			{
				Error = "";
				break;
			}
			int paramInt2 = (int)Atol5.getParamInt(65623);
			if (paramInt2 == 1059)
			{
				byte[] paramByteArray = Atol5.getParamByteArray(65624);
				list2.Add(paramByteArray);
			}
			else
			{
				ReadTeg(paramInt2, Types, Rez);
			}
		}
		foreach (byte[] item in list2)
		{
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			Atol5.setParam(65668, 6u);
			Atol5.setParam(65624, item);
			value = Atol5.beginReadRecords();
			if (IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка получения документа из ФН"))
			{
				return null;
			}
			while (true)
			{
				value = Atol5.readNextRecord();
				if (IsCommandBad(null, value, OpenSerial: false, fClearCheck: false, "Ошибка получения документа из ФН"))
				{
					break;
				}
				int paramInt3 = (int)Atol5.getParamInt(65623);
				ReadTeg(paramInt3, Types, dictionary);
			}
			Error = "";
			list.Add(dictionary);
		}
		Rez[1059] = list.AsString();
		if (flag)
		{
			await PortCloseAsync();
		}
		return Rez;
	}

	private void ReadTeg(int Teg, Dictionary<int, Type> Types, Dictionary<int, string> CurRez)
	{
		int paramInt = (int)Atol5.getParamInt(65740);
		if (!Types.ContainsKey(Teg))
		{
			return;
		}
		switch (paramInt)
		{
		case 7:
			CurRez.Add(Teg, Atol5.getParamInt(65624).AsString());
			break;
		case 8:
			CurRez.Add(Teg, Atol5.getParamInt(65624).AsString());
			break;
		case 5:
			if (Teg == 1054)
			{
				CurRez.Add(Teg, Atol5.getParamInt(65624).AsString());
			}
			else
			{
				CurRez.Add(Teg, Atol5.getParamInt(65624).AsString());
			}
			break;
		case 4:
			CurRez.Add(Teg, Atol5.getParamInt(65624).AsString());
			break;
		case 6:
			CurRez.Add(Teg, Atol5.getParamDouble(65624).AsString());
			break;
		case 3:
			CurRez.Add(Teg, Atol5.getParamDouble(65624).AsString());
			break;
		case 1:
			CurRez.Add(Teg, Atol5.getParamString(65624));
			break;
		case 10:
			CurRez.Add(Teg, Atol5.getParamBool(65624).AsString());
			break;
		case 9:
			CurRez.Add(Teg, Atol5.getParamDateTime(65624).AsString());
			break;
		case 2:
			if (Teg == 1077)
			{
				byte[] paramByteArray = Atol5.getParamByteArray(65624);
				CurRez.Add(Teg, ((paramByteArray[2] << 24) + (paramByteArray[3] << 16) + (paramByteArray[4] << 8) + paramByteArray[5]).AsString());
			}
			else
			{
				CurRez.Add(Teg, Atol5.getParamByteArray(65624).AsString());
			}
			break;
		}
	}

	public async Task<bool> PortOpenAsync(bool ClearCheck = false)
	{
		if (Atol5 != null && Atol5.isOpened())
		{
			if (ClearCheck)
			{
				Atol5.cancelReceipt();
			}
			else
			{
				Atol5.setParam(65587, 0u);
				Atol5.queryData();
			}
			int num = Atol5.errorCode();
			if (num != 241 && num != 1 && num != 2 && num != 3 && num != 4)
			{
				return false;
			}
			PortLogs.Append("Соединение с ККТ потеряно, переоткрываем порт");
			await PortCloseAsync();
			PortOff(IsOll: true);
		}
		if (Atol5 == null)
		{
			try
			{
				Atol5 = new Fptr(this);
			}
			catch
			{
				Error = "Не установлены ДТО Атола x32!!!!!<br/>";
				try
				{
					Atol5.FreeLibrary();
					Atol5 = null;
				}
				catch
				{
				}
				return false;
			}
		}
		string textError = "";
		string data = "";
		if (!Atol5.isOpened())
		{
			for (int i = 0; i <= 4; i++)
			{
				Atol5.Close();
				if (RemoteServer)
				{
					Atol5.setSingleSetting("RemoteServerAddr", RemoteServerURL);
					Atol5.setSingleSetting("RemoteServerConnectionTimeout", RemoteServerTimeOut.ToString());
				}
				else
				{
					Atol5.setSingleSetting("RemoteServerAddr", "");
				}
				Atol5.setSingleSetting("AutoReconnect", "true");
				if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
				{
					Atol5.setSingleSetting("Model", Model);
					Atol5.setSingleSetting("Port", 0.ToString());
					Atol5.setSingleSetting("ComFile", SetPort.ComId);
					Atol5.setSingleSetting("BaudRate", SetPort.ComSpeed.ToString());
					Atol5.setSingleSetting("OfdChannel", 2.ToString());
					textError = "Ошибка открытия COM порта: ";
					data = "COM порт открыт.";
				}
				else if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
				{
					Atol5.setSingleSetting("Model", Model);
					Atol5.setSingleSetting("Port", 2.ToString());
					Atol5.setSingleSetting("IPAddress", SetPort.IP);
					Atol5.setSingleSetting("IPPort", SetPort.Port);
					textError = "Ошибка установки соединения: ";
					data = "Socket открыт.";
				}
				else if (SetPort.TypeConnect == SetPorts.enTypeConnect.Usb)
				{
					Atol5.setSingleSetting("Model", Model);
					Atol5.setSingleSetting("Port", 1.ToString());
					Atol5.setSingleSetting("UsbDevicePath", SetPort.USBPort);
					Atol5.setSingleSetting("OfdChannel", 2.ToString());
					textError = "Ошибка установки соединения: ";
					data = "Socket открыт.";
				}
				else if (SetPort.TypeConnect == SetPorts.enTypeConnect.Bluetooth)
				{
					Atol5.setSingleSetting("Model", Model);
					Atol5.setSingleSetting("Port", 3.ToString());
					Atol5.setSingleSetting("MACAddress", SetPort.BluetoothPort);
					textError = "Ошибка установки соединения: ";
					data = "Socket открыт.";
				}
				else if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
				{
					PortLogs.Append("Не настроено подключение");
					return false;
				}
				Atol5.applySingleSettings();
				if (Atol5.open() != 0)
				{
					GetTextError(textError);
					SetPort.PortOpen = false;
					Task.Delay(1000).Wait();
					continue;
				}
				SetPort.PortOpen = true;
				PortLogs.Append(data);
				PortLogs.Stopwatch.Start();
				if (ClearCheck)
				{
					Atol5.cancelReceipt();
				}
				return true;
			}
		}
		PortLogs.Append(Error);
		return false;
	}

	public override async Task<bool> PortCloseAsync()
	{
		string data = "";
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			data = "COM порт закрыт.";
		}
		else if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			data = "Socket закрыт.";
		}
		if (SetPort.PortOpen)
		{
			SetPort.PortOpen = false;
			PortLogs.Append(data);
			Atol5Close = DateTime.Now;
		}
		PortOff();
		return true;
	}

	public void PortOff(bool IsOll = false, bool IsOllDes = false)
	{
		if (IsOllDes)
		{
			if (Atol5 != null)
			{
				Atol5.Close();
			}
			return;
		}
		lock (Atol5Lock)
		{
			if (Atol5 != null && IsOll)
			{
				Atol5.Close();
			}
			else if (Atol5 != null && !SetPort.PortOpen && Atol5.isOpened() && ClosePort)
			{
				_ = Atol5.getSingleSetting("OfdChannel") != 2.ToString();
				Atol5.setParam(65622, 1u);
				Atol5.fnQueryData();
				Kkm.OFD_NumErrorDoc = (int)Atol5.getParamInt(65625);
				int paramInt = (int)Atol5.getParamInt(65640);
				if (Kkm.OFD_NumErrorDoc == 0 && (paramInt & 1) == 0 && Atol5 != null)
				{
					Atol5.Close();
				}
			}
		}
	}

	public static void PortOffTimmer(object state = null)
	{
		lock (((Atol_5)state).Atol5Lock)
		{
			((Atol_5)state).PortOff();
		}
	}

	public bool IsCommandBad(RezultCommand RezultCommand, int? StatRun, bool OpenSerial, bool fClearCheck, string ErrorText)
	{
		if (Error != "")
		{
			if (ClearCheck)
			{
				string error = Error;
				Error = "";
				StatRun = Atol5.cancelReceipt();
				GetTextError(ErrorText);
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
				Error = ErrorText + " (" + Error + ")";
			}
			if (StatRun.HasValue && StatRun != 0)
			{
				string error2 = Error;
				GetTextError(ErrorText);
				Error = error2;
			}
		}
		else
		{
			if (!StatRun.HasValue)
			{
				return false;
			}
			if (!StatRun.HasValue || StatRun == 0)
			{
				return false;
			}
			GetTextError(ErrorText);
			if (fClearCheck)
			{
				string error = Error;
				Error = "";
				StatRun = Atol5.cancelReceipt();
				GetTextError(ErrorText);
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

	public void GetTextError(string TextError)
	{
		string text = Atol5.errorCode().ToString();
		string text2 = Atol5.errorDescription();
		Atol5.resetError();
		Error = TextError + " ( " + text + " : " + text2 + " )";
	}
}
