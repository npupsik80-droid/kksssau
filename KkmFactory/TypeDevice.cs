using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace KkmFactory;

public class TypeDevice
{
	[DataContract]
	public enum enType
	{
		НеВыбрано = 0,
		ФискальныйРегистратор = 1,
		ЭквайринговыйТерминал = 3,
		ДисплеиПокупателя = 12,
		ПринтерЧеков = 2,
		ЭлектронныеВесы = 9,
		СканерШтрихкода = 4,
		СчитывательМагнитныхКарт = 5,
		ЭлектронныеЗамки = 11
	}

	[DataContract]
	public enum enUnitDevice
	{
		None = 0,
		Dll = 1,
		KkmServerStrihM = 3,
		KkmServerPinterPOS = 5,
		KkmServerPinterWin = 6,
		KkmServerBarCode = 7,
		KkmServerSbRfGate = 8,
		KktEmulator = 10,
		KktRrElectro = 11,
		ScanerBarCodeCOM = 12,
		KktPaykiosk = 13,
		Dreamkas = 14,
		Crystals = 15,
		KktKIT = 16,
		LibraMassaK = 17,
		KktUmka = 18,
		GateSbRf = 20,
		GateINPAS = 21,
		GateUCS = 23,
		GateArcus2 = 24,
		GateEmulator = 25,
		KkmServerAtol_5 = 26,
		Evotor = 27,
		LibraStrichM = 28,
		KktRetail = 38,
		LibraAtol = 39,
		GateSbpSber = 40,
		KktElves = 41,
		GateTTK2 = 42,
		MertechQrPay = 43,
		QrScreen = 44,
		GateSbpTinkoff = 45
	}

	public static int[] NumNameType = new int[8] { 0, 1, 3, 12, 2, 9, 4, 11 };

	[Obfuscation(Exclude = true)]
	public static string[] NameType = new string[13]
	{
		"<Не указано>", "Фискальный регистратор", "Принтер чеков", "Эквайринговый терминал", "Сканер штрихкода", "Считыватель магнитных карт", "Принтер этикеток", "Дисплей покупателя", "Терминал сбора данных", "Электронные весы",
		"Весы с печатью этикеток", "Электронные замки", "Дисплеи покупателя"
	};

	public string Id;

	public int Number;

	public enType Type;

	public List<enType> Types = new List<enType>();

	public string Protocol = "";

	public enUnitDevice UnitDevice;

	public bool MainThread;

	public string SupportModels;

	public bool IsEoU = true;

	public string Description = "";

	public TypeDevice()
	{
	}

	public TypeDevice(string Id, enUnitDevice UnitDevice, enType Type, string Protocol, string SupportModels = "")
	{
		NewTypeDevice(Id, UnitDevice, new enType[1] { Type }, Protocol, SupportModels);
	}

	public TypeDevice(string Id, enUnitDevice UnitDevice, enType[] Types, string Protocol, string SupportModels = "")
	{
		NewTypeDevice(Id, UnitDevice, Types, Protocol, SupportModels);
	}

	public void NewTypeDevice(string Id, enUnitDevice UnitDevice, enType[] Types, string Protocol, string SupportModels = "")
	{
		this.Id = Id;
		this.UnitDevice = UnitDevice;
		Type = Types[0];
		this.Types = new List<enType>(Types);
		this.Protocol = Protocol.Replace(":", ": ");
		if (Global.Settings.Marke == "GainUp")
		{
			this.Protocol = Protocol.Replace("(KkmServer.ru) ", "");
		}
		else if (Global.Settings.Marke == "YClients")
		{
			this.Protocol = Protocol.Replace("(KkmServer.ru) ", "");
		}
		this.SupportModels = SupportModels;
		Number = Global.UnitManager.ListTypeDevice.Count() + 1;
		Global.UnitManager.ListTypeDevice.Add(Id, this);
	}

	public string Name()
	{
		return NameType[(int)Type];
	}

	public static void RegTypeUnitDrivers()
	{
		string text = "";
		new TypeDevice("KkmAtol_5", enUnitDevice.KkmServerAtol_5, enType.ФискальныйРегистратор, text + "Atol (Платформа 3/5): ККТ (ФФД 1.2)", "АТОЛ 1Ф, АТОЛ 11Ф, АТОЛ 15Ф, АТОЛ 20Ф, АТОЛ 22Ф/FPrint-22ПТК, АТОЛ 22v2Ф, АТОЛ 25Ф, АТОЛ 27Ф, АТОЛ 30Ф, АТОЛ 42ФС, АТОЛ 47ФА, АТОЛ 50Ф, АТОЛ 55Ф, АТОЛ 77Ф, АТОЛ 91Ф(нужен специальный код защиты), АТОЛ 92Ф(нужен специальный код защиты), АТОЛ Sigma 10/АТОЛ 150Ф, АТОЛ Sigma 7Ф, АТОЛ Sigma 8Ф, СТ-5Ф, Казначей ФА. Для Windows - нужны ДТО Атол x32, Для Linux - нужны ДТО Атол x64<br/><a href='https://www.atol.ru/catalog/fiscalnyi-registrator/'>https://www.atol.ru/catalog/fiscalnyi-registrator/</a>");
		new TypeDevice("KkmStrihM", enUnitDevice.KkmServerStrihM, enType.ФискальныйРегистратор, text + "Штрих-М: ККТ (ФФД 1.2)", "ШТРИХ-М-01Ф, ШТРИХ-ON-LINE, ШТРИХ-ЛАЙТ-01Ф, ШТРИХ-ЛАЙТ-02Ф, ШТРИХ-М-02-Ф, ШТРИХ-МИНИ-02Ф, ШТРИХ-ФР-02Ф, ШТРИХ-МИНИ-01Ф, РИТЕЙЛ-01Ф, ШТРИХ-ФР-01Ф, ШТРИХ-ЗНАК-М3<br/><a href='https://www.shtrih-m.ru/catalog/onlayn-kassy/'>https://www.shtrih-m.ru/catalog/onlayn-kassy/</a>");
		new TypeDevice("Dreamkas", enUnitDevice.Dreamkas, enType.ФискальныйРегистратор, text + "Dreamkas (Viki Print): ККТ (ФФД 1.2)", "Viki-Print-57Ф, Viki-Print-57+Ф, Viki-Print-80+Ф<br/><a href='https://dreamkas.ru/fiskalnye-registratory/'>https://dreamkas.ru/fiskalnye-registratory/</a>");
		new TypeDevice("KkmKIT", enUnitDevice.KktKIT, enType.ФискальныйРегистратор, text + "КИТ (КАСБИ): ККТ (ФФД 1.2)", "КИТ Онлайн-Ф, Терминал-ФА<br/><a href='https://kit-invest.ru/'>https://kit-invest.ru/</a>");
		new TypeDevice("RrElectro", enUnitDevice.KktRrElectro, enType.ФискальныйРегистратор, text + "RR-Electro: ККТ (ФФД 1.2)", "РР-01Ф, РР-02Ф, РР-03Ф, РР-04Ф<br/><a href='https://rr-electro.com/products/'>https://rr-electro.com/products/</a>");
		new TypeDevice("Retail", enUnitDevice.KktRetail, enType.ФискальныйРегистратор, text + "Ритейл: ККТ (ФФД 1.2)", "Ритейл-01Ф, Ритейл-02Ф, Ритейл-Комбо-01Ф<br/><a href='https://www.retail.ru/products/equipment/kassa/'>https://www.retail.ru/products/equipment/kassa/</a>");
		new TypeDevice("Elves", enUnitDevice.KktElves, enType.ФискальныйРегистратор, text + "ЭЛВЕС: ККТ (ФФД 1.2)", "ЭЛВЕС-ФР-Ф<br/><a href='https://www.shtrih-m.ru/catalog/onlayn-kassy/kkt-shtrikh-mf/'>https://www.shtrih-m.ru/catalog/onlayn-kassy/kkt-shtrikh-mf/</a>");
		new TypeDevice("Paykiosk", enUnitDevice.KktPaykiosk, enType.ФискальныйРегистратор, text + "Paykiosk.ru: ККТ (ФФД 1.2)", "PayOnline-01-ФА, Pay VKP-80К-ФА<br/><a href='https://www.paykiosk.ru/'>https://www.paykiosk.ru/</a>");
		new TypeDevice("Crystals", enUnitDevice.Crystals, enType.ФискальныйРегистратор, text + "Crystals (Pirit): ККТ", "Pirit-1Ф, Pirit-2Ф, Pirit-2СФ");
		new TypeDevice("Evotor", enUnitDevice.Evotor, enType.ФискальныйРегистратор, text + "Эвотор: ККТ", "Эвотор-5, Эвотор-5i, Эвотор-7.2, Эвотор-7.3, Эвотор-10");
		new TypeDevice("KktEmulator", enUnitDevice.KktEmulator, enType.ФискальныйРегистратор, text + "Эмулятор: ККТ").IsEoU = false;
		new TypeDevice("LibraMassaK", enUnitDevice.LibraMassaK, enType.ЭлектронныеВесы, text + "Масса-К: электронные весы", "Все весы МАССА-К которые поддерживают протоколы: Протокол №2, Протокол STANDART, Протокол 1c<br/><a href='http://www.massa.ru/'>http://www.massa.ru/</a>");
		new TypeDevice("LibraStrichM", enUnitDevice.LibraStrichM, enType.ЭлектронныеВесы, text + "Штрих-М: электронные весы", "Весы Штрих-М<br/><a href='https://www.shtrih-m.ru/catalog/vesy-elektronnye/'>https://www.shtrih-m.ru/catalog/vesy-elektronnye/</a>");
		new TypeDevice("LibraAtol", enUnitDevice.LibraAtol, enType.ЭлектронныеВесы, text + "Атол: электронные весы", "Весы Атол<br/><a href='https://www.atol.ru/catalog/vesovoe-oborudovanie/'>https://www.atol.ru/catalog/vesovoe-oborudovanie/</a>");
		new TypeDevice("GateTTK2", enUnitDevice.GateTTK2, enType.ЭквайринговыйТерминал, text + "TTK2: Эквайринговые терминалы (бета)", "Все Эквайринговые терминалы которые поддерживают протокол TTK2 (Сбербанк и пр..");
		new TypeDevice("GateSbRf", enUnitDevice.GateSbRf, new enType[1] { enType.ЭквайринговыйТерминал }, text + "СБРФ: Эквайринговые терминалы", "Все Эквайринговые терминалы которые поддерживает Сбербанк-РФ");
		new TypeDevice("GateINPAS", enUnitDevice.GateINPAS, enType.ЭквайринговыйТерминал, text + "INPAS: Эквайринговые терминалы", "Эквайринговые терминалы INPAS (Verifone, IRAS, PAX)");
		new TypeDevice("GateUCS", enUnitDevice.GateUCS, enType.ЭквайринговыйТерминал, text + "UCS: Эквайринговые терминалы", "Эквайринговые терминалы UCS (United Card Services)");
		new TypeDevice("GateArcus2", enUnitDevice.GateArcus2, enType.ЭквайринговыйТерминал, text + "ARCUS 2: Эквайринговые терминалы", "Эквайринговые терминалы ARCUS 2 (Ingenico)");
		new TypeDevice("GateEmulator", enUnitDevice.GateEmulator, enType.ЭквайринговыйТерминал, text + "Эмулятор: Эквайринговый терминал", "Эмулятор эквайрингово терминала для отладки");
		new TypeDevice("GateSbpSber", enUnitDevice.GateSbpSber, enType.ЭквайринговыйТерминал, text + "СБП: Сбербанк", "Система быстрых платежей (СБП) по QR коду из приложения на телефоне<br/>К нему желательно дисплей покупателя с выводом QR кода, смотрите ниже..");
		new TypeDevice("GateSbpTinkoff", enUnitDevice.GateSbpTinkoff, enType.ЭквайринговыйТерминал, text + "СБП: Тинькофф (Beta)", "Система быстрых платежей (СБП) по QR коду из приложения на телефоне<br/>К нему желательно дисплей покупателя с выводом QR кода, смотрите ниже..");
		new TypeDevice("MertechQrPay", enUnitDevice.MertechQrPay, enType.ДисплеиПокупателя, text + "MERTECH QR-PA: Дисплеи покупателя", "Дисплей QR кодов MERTECH QR-PA<br/><a href='https://mertech.ru/qr-kod-displei/'>https://mertech.ru/qr-kod-displei/</a>");
		new TypeDevice("QrScreen", enUnitDevice.QrScreen, enType.ДисплеиПокупателя, text + "QR-Screen: Дисплеи покупателя", "Дисплей QR кодов QR-Screen<br/><a href='https://qr-screen.ru/product/'>https://qr-screen.ru/product/</a>");
		new TypeDevice("ScanerBarCodeCOM", enUnitDevice.ScanerBarCodeCOM, enType.СканерШтрихкода, text + "Сканер Штрих-кодов: COM");
		new TypeDevice("PinterWin", enUnitDevice.KkmServerPinterWin, enType.ПринтерЧеков, text + "Windows принтер чеков");
		new TypeDevice("PinterPOS", enUnitDevice.KkmServerPinterPOS, enType.ПринтерЧеков, text + "OPOS: Принтер чеков");
	}

	public static Unit GetDeviceClass(Global.DeviceSettings SettDr, int Num)
	{
		try
		{
			return SettDr.TypeDevice.UnitDevice switch
			{
				enUnitDevice.KkmServerAtol_5 => new Atol_5(SettDr, Num), 
				enUnitDevice.KkmServerStrihM => new StrihM(SettDr, Num), 
				enUnitDevice.KktRrElectro => new RrElectro(SettDr, Num), 
				enUnitDevice.KktRetail => new StrihM(SettDr, Num), 
				enUnitDevice.KktElves => new StrihM(SettDr, Num), 
				enUnitDevice.KktPaykiosk => new StrihM(SettDr, Num), 
				enUnitDevice.KktEmulator => new KktEmulator(SettDr, Num), 
				enUnitDevice.KktKIT => new KktKIT(SettDr, Num), 
				enUnitDevice.Dreamkas => new Dreamkas(SettDr, Num), 
				enUnitDevice.Crystals => new Dreamkas(SettDr, Num), 
				enUnitDevice.Evotor => new Evotor(SettDr, Num), 
				enUnitDevice.GateTTK2 => new GateTTK2(SettDr, Num), 
				enUnitDevice.GateINPAS => new GateINPAS(SettDr, Num), 
				enUnitDevice.GateSbRf => new GateSbRf(SettDr, Num), 
				enUnitDevice.GateUCS => new GateUCS(SettDr, Num), 
				enUnitDevice.GateArcus2 => new GateArcus2(SettDr, Num), 
				enUnitDevice.GateEmulator => new GateEmulator(SettDr, Num), 
				enUnitDevice.GateSbpSber => new GateSbpSber(SettDr, Num), 
				enUnitDevice.GateSbpTinkoff => new GateSbpTinkoff(SettDr, Num), 
				enUnitDevice.MertechQrPay => new MertechQrPay(SettDr, Num), 
				enUnitDevice.QrScreen => new QrScreen(SettDr, Num), 
				enUnitDevice.LibraMassaK => new LibraMassaK(SettDr, Num), 
				enUnitDevice.LibraStrichM => new LibraStrichM(SettDr, Num), 
				enUnitDevice.LibraAtol => new LibraAtol(SettDr, Num), 
				enUnitDevice.ScanerBarCodeCOM => new ScanerBarCodeCOM(SettDr, Num), 
				enUnitDevice.KkmServerPinterPOS => new PinterPOS(SettDr, Num), 
				enUnitDevice.KkmServerPinterWin => new PinterWin(SettDr, Num), 
				_ => new Unit(null, Num), 
			};
		}
		catch (Exception ex)
		{
			Global.Logers.AddError("Ошибка инициализации: ", SettDr, Global.GetErrorMessagee(ex)).Wait();
			return new Unit(null, Num);
		}
	}
}
