using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KkmFactory;

public class KktKIT : UnitPort
{
	private ushort[] Table = new ushort[256];

	private string URL = "";

	private bool ClearCheck;

	private bool OfdStatusFullRead;

	private int OldSessionCheckNumber;

	public SortedList<int, byte> Nalogs = new SortedList<int, byte>();

	private bool NotCloseCom;

	private Encoding e886 = Encoding.GetEncoding(866);

	private List<string> BadBarCode = new List<string>();

	public KktKIT(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		Kkm.IsKKT = true;
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		for (int i = 0; i < Table.Length; i++)
		{
			ushort num = 0;
			ushort num2 = (ushort)(i << 8);
			for (int j = 0; j < 8; j++)
			{
				num = ((((num ^ num2) & 0x8000) == 0) ? ((ushort)(num << 1)) : ((ushort)((num << 1) ^ 0x1021)));
				num2 <<= 1;
			}
			Table[i] = num;
		}
	}

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = Global.Description;
		UnitEquipmentType = "ФискальныйРегистратор";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kit-invest.ru/";
		UnitAdditionallinks = "<a href='https://www.kit-invest.ru/Drivers'>Станица для скачивания ПО Терминал-ФА</a><br/>";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                <ChoiceList>\r\n                    <Item Value=\"1\">Сеть: Ethernet/WiFi</Item>\r\n                    <Item Value=\"2\">COM порт</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"IP\" Caption=\"IP: адрес\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"Port\" Caption=\"IP: порт\" TypeValue=\"String\" DefaultValue=\"50003\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"1\"/>\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"NotCloseCom\" Caption=\"Не закрывать COM порт\" TypeValue=\"Boolean\" DefaultValue=\"true\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\" />\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		LoadParametsFromXML(text);
		string paramsXML = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                 <Actions>\r\n                      <Action Name=\"UpdateFrimware\" Caption=\"Запустить обновление загруженной прошивки\"/> \r\n                 </Actions>";
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
			case "NotCloseCom":
				NotCloseCom = unitParamet.Value.AsBool();
				break;
			}
		}
	}

	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (Error != "")
		{
			return false;
		}
		Kkm.IsKKT = true;
		byte[] array = await RunCommand(1u, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "ККМ не подключена!"))
		{
			LastError = Error;
			return false;
		}
		Kkm.NumberKkm = StringFromStream(array, 1, 12);
		Kkm.PaperOver = array[19] != 0;
		Kkm.FN_Status = array[21];
		await ClearOldCheck();
		array = await RunCommand(4u, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "ККМ не подключена!"))
		{
			LastError = Error;
			return false;
		}
		NameDevice = StringFromStream(array, 1);
		array = await RunCommand(32u, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			LastError = Error;
			return false;
		}
		if (array[1] == 1)
		{
			SessionOpen = 2;
		}
		else
		{
			SessionOpen = 1;
		}
		array = await RunCommand(187u, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка получения ширины печати"))
		{
			LastError = Error;
			return false;
		}
		Kkm.PrintingWidth = array[1];
		if (Kkm.PrintingWidth == 0)
		{
			Kkm.PrintingWidth = 36;
		}
		if (!FullInit && IsInit)
		{
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			return true;
		}
		Kkm.INN = "";
		Kkm.RegNumber = "";
		array = await RunCommand(10u, new MemoryStream());
		if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить данные регистрации!"))
		{
			Kkm.RegNumber = StringFromStream(array, 1, 20);
			Kkm.INN = StringFromStream(array, 21, 12);
		}
		else
		{
			Error = "";
		}
		Kkm.Organization = "<Не определено>";
		await ReadStatusOFD(FullInit);
		if (Global.Settings.SetNotActiveOnPaperOver && NameDevice != "Terminal-FA")
		{
			IsInit = !Kkm.PaperOver;
		}
		else
		{
			IsInit = true;
		}
		Nalogs.Clear();
		Nalogs.Add(18, 1);
		Nalogs.Add(20, 1);
		Nalogs.Add(10, 2);
		Nalogs.Add(118, 3);
		Nalogs.Add(120, 3);
		Nalogs.Add(110, 4);
		Nalogs.Add(0, 5);
		Nalogs.Add(-1, 6);
		Nalogs.Add(5, 7);
		Nalogs.Add(7, 8);
		Nalogs.Add(105, 9);
		Nalogs.Add(107, 10);
		Nalogs.Add(22, 11);
		Nalogs.Add(122, 12);
		Nalogs.Add(1102, 1);
		Nalogs.Add(1103, 2);
		Nalogs.Add(1107, 3);
		Nalogs.Add(1106, 4);
		Nalogs.Add(1104, 5);
		Nalogs.Add(1105, 6);
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
		if (NameDevice == "Terminal-FA")
		{
			DataCommand.NotPrint = true;
		}
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		await CloseDocumentAndOpenShift(DataCommand, RezultCommand);
		decimal AllSum = default(decimal);
		await GetCheckAndSession(RezultCommand);
		RezultCommand.CheckNumber++;
		if (DataCommand.IsFiscalCheck && (DataCommand.TaxVariant == "" || DataCommand.TaxVariant == null))
		{
			string[] array = Kkm.TaxVariant.Split(',');
			string text = "";
			if (array.Length != 0)
			{
				text = array[0];
			}
			if (array.Contains(text.ToString()))
			{
				DataCommand.TaxVariant = text.ToString();
			}
			else
			{
				DataCommand.TaxVariant = array[0];
			}
		}
		PortLogs.Append("Открытие чека", "-");
		bool IsCheckCorrection = false;
		if (DataCommand.TypeCheck == 2 || DataCommand.TypeCheck == 12 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 13)
		{
			IsCheckCorrection = true;
		}
		if (DataCommand.IsFiscalCheck && !IsCheckCorrection)
		{
			if (IsCommandBad(RezultCommand, await RunCommand(35u, new MemoryStream(new byte[2] { 69, 160 })), OpenSerial, ClearCheck: false, "Не удалось открыть регистрацию чека"))
			{
				return;
			}
		}
		else if (DataCommand.IsFiscalCheck && IsCheckCorrection && IsCommandBad(RezultCommand, await RunCommand(37u, new MemoryStream(new byte[2] { 69, 160 })), OpenSerial, ClearCheck: false, "Не удалось открыть регистрацию чека"))
		{
			return;
		}
		if (DataCommand.IsFiscalCheck && !Kkm.InternetMode && !Kkm.AutomaticMode)
		{
			await SerCashier(DataCommand);
		}
		if (!IsCheckCorrection && DataCommand.IsFiscalCheck && DataCommand.AdditionalProps != null)
		{
			DataCommand.AdditionalProp[] additionalProps = DataCommand.AdditionalProps;
			for (int i = 0; i < additionalProps.Length; i++)
			{
				_ = additionalProps[i].PrintInHeader;
			}
		}
		await ComDevice.PostCheck(DataCommand, this);
		DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
		foreach (DataCommand.CheckString PrintString in checkStrings)
		{
			if (DataCommand.NotPrint == false)
			{
				_ = PrintString?.PrintImage;
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
					1 => 48, 
					2 => 32, 
					3 => 0, 
					4 => 64, 
					_ => 0, 
				};
				string text2 = PrintString.PrintText.Text;
				text2 = Unit.GetPringString(text2, CurWidth);
				string OstText;
				do
				{
					if (text2.Length > CurWidth)
					{
						OstText = text2.Substring(CurWidth);
						text2 = text2.Substring(0, CurWidth);
					}
					else
					{
						OstText = "";
					}
					MemoryStream memoryStream = new MemoryStream();
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write((byte)Font);
					binaryWriter.Write((byte)0);
					binaryWriter.Write(StringToStream(text2, 0, 32));
					if (IsCommandBad(RezultCommand, await RunCommand(97u, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать не фискальную строку"))
					{
						return;
					}
					text2 = OstText;
				}
				while (OstText != "");
			}
			if (DataCommand.IsFiscalCheck && PrintString != null && PrintString.Register != null && PrintString.Register.Quantity != 0m)
			{
				if (IsCheckCorrection && Kkm.FfdVersion < 3)
				{
					continue;
				}
				PortLogs.Append("Регистрация фискальной строки", "-");
				AllSum += PrintString.Register.Amount;
				List<DataCommand.Register> list = SplitRegisterString(PrintString);
				foreach (DataCommand.Register SplitStr in list)
				{
					Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
					dictionary.Add(1030, StringToStream(PrintString.Register.Name, Math.Min(65, PrintString.Register.Name.Length), 32));
					dictionary.Add(1079, NumberToStream((long)(SplitStr.Price * 100m), 0));
					dictionary.Add(1023, DecimalToStream(SplitStr.Quantity, 0));
					if (Nalogs.ContainsKey((int)PrintString.Register.Tax))
					{
						dictionary.Add(1199, new byte[1] { Nalogs[(int)PrintString.Register.Tax] });
					}
					else
					{
						Error = $"Ставка налога \"{PrintString.Register.Tax}\" не запрограммирована в ККМ";
					}
					dictionary.Add(1214, new byte[1] { (byte)PrintString.Register.SignMethodCalculation.Value });
					dictionary.Add(1212, new byte[1] { (byte)PrintString.Register.SignCalculationObject.Value });
					if (PrintString.Register.AgentSign.HasValue)
					{
						byte b = (byte)(1 << PrintString.Register.AgentSign).Value;
						dictionary.Add(1222, new byte[1] { b });
					}
					if (PrintString.Register.AgentData != null)
					{
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.PayingAgentOperation))
						{
							dictionary.Add(1044, StringToStream(PrintString.Register.AgentData.PayingAgentOperation, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.PayingAgentPhone))
						{
							dictionary.Add(1073, StringToStream(PrintString.Register.AgentData.PayingAgentPhone, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.ReceivePaymentsOperatorPhone))
						{
							dictionary.Add(1074, StringToStream(PrintString.Register.AgentData.ReceivePaymentsOperatorPhone, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorPhone))
						{
							dictionary.Add(1075, StringToStream(PrintString.Register.AgentData.MoneyTransferOperatorPhone, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorName))
						{
							dictionary.Add(1026, StringToStream(PrintString.Register.AgentData.MoneyTransferOperatorName, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorAddress))
						{
							dictionary.Add(1005, StringToStream(PrintString.Register.AgentData.MoneyTransferOperatorAddress, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.AgentData.MoneyTransferOperatorVATIN))
						{
							dictionary.Add(1016, StringToStream(PrintString.Register.AgentData.MoneyTransferOperatorVATIN.PadRight(12, ' '), 0, 32));
						}
					}
					if (PrintString.Register.PurveyorData != null)
					{
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorPhone))
						{
							dictionary.Add(1171, StringToStream(PrintString.Register.PurveyorData.PurveyorPhone, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorName))
						{
							dictionary.Add(1225, StringToStream(PrintString.Register.PurveyorData.PurveyorName, 0, 32));
						}
						if (!string.IsNullOrEmpty(PrintString.Register.PurveyorData.PurveyorName))
						{
							dictionary.Add(1226, StringToStream(PrintString.Register.PurveyorData.PurveyorVATIN.PadRight(12, ' '), 0, 32));
						}
					}
					bool flag = false;
					if (Kkm.FfdVersion >= 4 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.TryBarCode))
					{
						if (!BadBarCode.Contains(PrintString.Register.GoodCodeData.TryBarCode))
						{
							dictionary.Add(2000, StringToStream(PrintString.Register.GoodCodeData.TryBarCode, 0, 32));
							dictionary.Add(2100, NumberToStream(0L, 1));
							int statusMarkingCode = GetStatusMarkingCode(DataCommand.TypeCheck, PrintString.Register.MeasureOfQuantity);
							dictionary.Add(2110, NumberToStream(statusMarkingCode, 1));
							dictionary.Add(2102, NumberToStream(0L, 1));
							if (PrintString.Register.PackageQuantity.HasValue)
							{
								dictionary.Add(1293, NumberToStream((long)PrintString.Register.Quantity, 4));
								dictionary.Add(1294, NumberToStream(PrintString.Register.PackageQuantity.Value, 4));
							}
							flag = true;
						}
						else
						{
							dictionary.Add(2000, StringToStream(PrintString.Register.GoodCodeData.TryBarCode, 0, 32));
						}
					}
					if (Kkm.FfdVersion >= 4 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.IndustryProps))
					{
						Dictionary<int, byte[]> dictionary2 = new Dictionary<int, byte[]>();
						dictionary2.Add(1262, NumberToStream(long.Parse(PrintString.Register.GoodCodeData.Props1262), 1));
						dictionary2.Add(1263, StringToStream(PrintString.Register.GoodCodeData.Props1263, 0, 32));
						dictionary2.Add(1264, StringToStream(PrintString.Register.GoodCodeData.Props1264, 0, 32));
						dictionary2.Add(1265, StringToStream(PrintString.Register.GoodCodeData.IndustryProps, 0, 32));
						dictionary.Add(1260, DictionaryToStream(dictionary2));
					}
					if (Kkm.FfdVersion >= 2)
					{
						dictionary.Add(2108, NumberToStream(PrintString.Register.MeasureOfQuantity.Value, 1));
					}
					if (Kkm.FfdVersion <= 3 && PrintString.Register.GoodCodeData != null && !string.IsNullOrEmpty(PrintString.Register.GoodCodeData.MarkingCodeBase64))
					{
						dictionary.Add(1162, StringToStream(BitConverter.ToString(Convert.FromBase64String(PrintString.Register.GoodCodeData.MarkingCodeBase64)).Replace("-", ""), 0, 32));
					}
					if (PrintString.Register.CountryOfOrigin != null && PrintString.Register.CountryOfOrigin != "")
					{
						dictionary.Add(1230, StringToStream(PrintString.Register.CountryOfOrigin, 0, 32));
					}
					if (PrintString.Register.CustomsDeclaration != null && PrintString.Register.CustomsDeclaration != "")
					{
						dictionary.Add(1231, StringToStream(PrintString.Register.CustomsDeclaration, 0, 32));
					}
					if (PrintString.Register.ExciseAmount.HasValue)
					{
						dictionary.Add(1229, NumberToStream((long)(PrintString.Register.ExciseAmount * (decimal?)100).Value, 0));
					}
					if (PrintString.Register.AdditionalAttribute != null && PrintString.Register.AdditionalAttribute != "")
					{
						dictionary.Add(1191, StringToStream(PrintString.Register.AdditionalAttribute, 0, 32));
					}
					byte[] buffer;
					if (Kkm.FfdVersion >= 4 && !flag)
					{
						buffer = await RunCommand(150u, new MemoryStream(DictionaryToStream(dictionary)));
					}
					else if (Kkm.FfdVersion >= 4 && flag)
					{
						buffer = await RunCommand(151u, new MemoryStream(DictionaryToStream(dictionary)));
					}
					else
					{
						Dictionary<int, byte[]> dictionary3 = new Dictionary<int, byte[]>();
						dictionary3.Add(1059, DictionaryToStream(dictionary));
						buffer = await RunCommand(43u, new MemoryStream(DictionaryToStream(dictionary3)));
					}
					if (IsCommandBad(RezultCommand, buffer, OpenSerial, ClearCheck: true, "Не удалось зарегистрировать фискальную строку"))
					{
						return;
					}
					if (DataCommand.NotPrint == false && SplitStr.StSkidka != "")
					{
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write((byte)64);
						binaryWriter.Write((byte)0);
						binaryWriter.Write(StringToStream(SplitStr.StSkidka, 0, 32));
						if (IsCommandBad(RezultCommand, await RunCommand(97u, memoryStream), OpenSerial, ClearCheck: true, "Не удалось напечатать не фискальную строку"))
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
		if (DataCommand.IsFiscalCheck)
		{
			bool flag2 = false;
			DataCommand.CheckString[] checkStrings2 = DataCommand.CheckStrings;
			foreach (DataCommand.CheckString checkString in checkStrings2)
			{
				if (checkString != null && checkString.Register != null && checkString.Register.Quantity != 0m && checkString.Register.AgentSign.HasValue)
				{
					flag2 = true;
				}
			}
			if (((!flag2 && Kkm.SignOfAgent != "") || DataCommand.AgentSign.HasValue) && !(await WriteAgentSign(DataCommand.AgentSign, DataCommand.AgentData, DataCommand.PurveyorData)))
			{
				return;
			}
		}
		if (Kkm.FfdVersion >= 4 && DataCommand.IsFiscalCheck)
		{
			decimal Cdacha = default(decimal);
			decimal num = DataCommand.Cash + DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3 + DataCommand.AdvancePayment + DataCommand.Credit + DataCommand.CashProvision;
			decimal num2;
			if (AllSum < num)
			{
				Cdacha = num - AllSum;
				if (!(Cdacha <= DataCommand.Cash))
				{
					Error = "Не хватает наличных на сдачу!";
					if (OpenSerial)
					{
						await PortCloseAsync();
					}
					return;
				}
				num2 = DataCommand.Cash - Cdacha;
			}
			else
			{
				num2 = DataCommand.Cash;
			}
			Dictionary<int, byte[]> Dict = new Dictionary<int, byte[]>
			{
				{
					1055,
					new byte[1] { (byte)(1 << (int)byte.Parse(DataCommand.TaxVariant)) }
				},
				{
					1031,
					NumberToStream((long)(num2 * 100m), 0)
				},
				{
					1081,
					NumberToStream((long)((DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3) * 100m), 0)
				},
				{
					1215,
					NumberToStream((long)(DataCommand.AdvancePayment * 100m), 0)
				},
				{
					1216,
					NumberToStream((long)(DataCommand.Credit * 100m), 0)
				},
				{
					1217,
					NumberToStream((long)(DataCommand.CashProvision * 100m), 0)
				}
			};
			if (IsCommandBad(RezultCommand, await RunCommand(45u, new MemoryStream(DictionaryToStream(Dict))), OpenSerial, ClearCheck: true, "Не удалось передать данные оплаты чека"))
			{
				return;
			}
			new Dictionary<int, byte[]>();
			Dict.Add(1009, StringToStream(DataCommand.AddressSettle, 0, 32));
			Dict.Add(1187, StringToStream(DataCommand.PlaceMarket, 0, 32));
			if (!string.IsNullOrEmpty(Kkm.AutomaticNumber))
			{
				Dict.Add(1036, StringToStream(Kkm.AutomaticNumber, 0, 32));
			}
			Dict.Add(1021, StringToStream((DataCommand.CashierName != null) ? DataCommand.CashierName : "", 0, 32));
			Dict.Add(1203, StringToStream((DataCommand.CashierVATIN != null) ? DataCommand.CashierVATIN : "", 12, 32));
			if (!string.IsNullOrEmpty(DataCommand.ClientAddress))
			{
				Dict.Add(1008, StringToStream(DataCommand.ClientAddress, 0, 32));
			}
			if (!string.IsNullOrEmpty(DataCommand.ClientInfo) || !string.IsNullOrEmpty(DataCommand.ClientINN) || !string.IsNullOrEmpty(DataCommand.ClientAddress))
			{
				Dictionary<int, byte[]> dictionary4 = new Dictionary<int, byte[]>();
				if (!string.IsNullOrEmpty(DataCommand.ClientInfo))
				{
					dictionary4.Add(1227, StringToStream(DataCommand.ClientInfo, 0, 32));
				}
				if (!string.IsNullOrEmpty(DataCommand.ClientINN))
				{
					dictionary4.Add(1228, StringToStream(DataCommand.ClientINN, 12, 32));
				}
				Dict.Add(1256, DictionaryToStream(dictionary4));
			}
			if (IsCheckCorrection)
			{
				Dict.Add(1178, DateToStream(DataCommand.CorrectionBaseDate.Value, 4));
				Dict.Add(1179, StringToStream(DataCommand.CorrectionBaseNumber, 0, 32));
			}
			if (Kkm.FfdVersion >= 4)
			{
				bool flag3 = (DataCommand.InternetMode.HasValue ? DataCommand.InternetMode.Value : Kkm.InternetMode);
				Dict.Add(1125, DecimalToStream(flag3 ? 1 : 0, 1));
			}
			if (IsCommandBad(RezultCommand, await RunCommand(152u, new MemoryStream(DictionaryToStream(Dict))), OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			byte b2 = 0;
			if (DataCommand.TypeCheck == 0)
			{
				b2 = 1;
			}
			else if (DataCommand.TypeCheck == 1)
			{
				b2 = 2;
			}
			else if (DataCommand.TypeCheck == 10)
			{
				b2 = 3;
			}
			else if (DataCommand.TypeCheck == 11)
			{
				b2 = 4;
			}
			else if (DataCommand.TypeCheck == 2)
			{
				b2 = 1;
			}
			else if (DataCommand.TypeCheck == 3)
			{
				b2 = 3;
			}
			else if (DataCommand.TypeCheck == 12)
			{
				b2 = 3;
			}
			else if (DataCommand.TypeCheck == 13)
			{
				b2 = 1;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(b2);
			binaryWriter.Write(NumberToStream((long)(AllSum * 100m), 5));
			if (Cdacha > 0m)
			{
				binaryWriter.Write(StringToStream("Сдача:<#0#>" + Cdacha.ToString("F2"), 0, 32));
			}
			byte[] buffer = ((!IsCheckCorrection) ? (await RunCommand(36u, memoryStream)) : (await RunCommand(38u, memoryStream)));
			if (IsCommandBad(RezultCommand, buffer, OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			RezultCommand.CheckNumber = (long)NumberFromStream(buffer, 3, 4);
			ClearCheck = false;
		}
		if (Kkm.FfdVersion <= 3 && DataCommand.IsFiscalCheck && IsCheckCorrection)
		{
			Dictionary<int, byte[]> dictionary5 = new Dictionary<int, byte[]>();
			int num3 = (int)((DataCommand.CorrectionBaseDate.HasValue ? DataCommand.CorrectionBaseDate : new DateTime?(new DateTime(2001, 1, 1))).Value - new DateTime(1970, 1, 1)).TotalSeconds;
			dictionary5.Add(1178, NumberToStream(num3, 4));
			dictionary5.Add(1179, StringToStream((DataCommand.CorrectionBaseNumber != null && DataCommand.CorrectionBaseNumber != "") ? DataCommand.CorrectionBaseNumber : " ", 0, 32));
			Dictionary<int, byte[]> dictionary6 = new Dictionary<int, byte[]>();
			dictionary6.Add(1021, StringToStream((DataCommand.CashierName != null) ? DataCommand.CashierName : "", 0, 32));
			dictionary6.Add(1203, StringToStream((DataCommand.CashierVATIN != null) ? DataCommand.CashierVATIN : "", 12, 32));
			dictionary6.Add(1173, new byte[1] { (byte)DataCommand.CorrectionType });
			dictionary6.Add(1174, DictionaryToStream(dictionary5));
			dictionary6.Add(1055, new byte[1] { (byte)(1 << (int)byte.Parse(DataCommand.TaxVariant)) });
			dictionary6.Add(1031, NumberToStream((long)(DataCommand.Cash * 100m), 0));
			dictionary6.Add(1081, NumberToStream((long)((DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3) * 100m), 0));
			dictionary6.Add(1215, NumberToStream((long)(DataCommand.AdvancePayment * 100m), 0));
			dictionary6.Add(1216, NumberToStream((long)(DataCommand.Credit * 100m), 0));
			dictionary6.Add(1217, NumberToStream((long)(DataCommand.CashProvision * 100m), 0));
			dictionary6.Add(1102, NumberToStream((long)Math.Round((DataCommand.SumTax18 + DataCommand.SumTax20) / 20m * 120m, MidpointRounding.AwayFromZero) * 100, 0));
			dictionary6.Add(1103, NumberToStream((long)Math.Round(DataCommand.SumTax10 / 10m * 110m, MidpointRounding.AwayFromZero) * 100, 0));
			dictionary6.Add(1104, NumberToStream((long)(DataCommand.SumTax0 * 100m), 0));
			dictionary6.Add(1105, NumberToStream((long)(DataCommand.SumTaxNone * 100m), 0));
			dictionary6.Add(1106, NumberToStream((long)Math.Round((DataCommand.SumTax118 + DataCommand.SumTax120) / 20m * 120m, MidpointRounding.AwayFromZero) * 100, 0));
			dictionary6.Add(1107, NumberToStream((long)Math.Round(DataCommand.SumTax110 / 10m * 110m, MidpointRounding.AwayFromZero) * 100, 0));
			if (IsCommandBad(RezultCommand, await RunCommand(46u, new MemoryStream(DictionaryToStream(dictionary6))), OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			byte b3 = 0;
			if (DataCommand.TypeCheck == 2)
			{
				b3 = 1;
			}
			else if (DataCommand.TypeCheck == 3)
			{
				b3 = 3;
			}
			else if (DataCommand.TypeCheck == 12)
			{
				b3 = 3;
			}
			else if (DataCommand.TypeCheck == 13)
			{
				b3 = 1;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(b3);
			try
			{
				if (int.Parse(Kkm.Firmware_Version.Substring(4, 2)) <= 18)
				{
					binaryWriter.Write((byte)0);
				}
			}
			catch
			{
			}
			binaryWriter.Write(NumberToStream((long)(DataCommand.Amount * 100m), 0));
			byte[] buffer = await RunCommand(38u, memoryStream);
			if (IsCommandBad(RezultCommand, buffer, OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			RezultCommand.CheckNumber = (long)NumberFromStream(buffer, 3, 4);
		}
		if (Kkm.FfdVersion <= 3 && DataCommand.IsFiscalCheck && !IsCheckCorrection)
		{
			decimal Cdacha = default(decimal);
			decimal num4 = DataCommand.Cash + DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3 + DataCommand.AdvancePayment + DataCommand.Credit + DataCommand.CashProvision;
			decimal num5;
			if (AllSum < num4)
			{
				Cdacha = num4 - AllSum;
				if (!(Cdacha <= DataCommand.Cash))
				{
					Error = "Не хватает наличных на сдачу!";
					if (OpenSerial)
					{
						await PortCloseAsync();
					}
					return;
				}
				num5 = DataCommand.Cash - Cdacha;
			}
			else
			{
				num5 = DataCommand.Cash;
			}
			Dictionary<int, byte[]> dictionary7 = new Dictionary<int, byte[]>();
			dictionary7.Add(1021, StringToStream((DataCommand.CashierName != null) ? DataCommand.CashierName : "", 0, 32));
			dictionary7.Add(1203, StringToStream((DataCommand.CashierVATIN != null) ? DataCommand.CashierVATIN : "", 0, 32));
			dictionary7.Add(1055, new byte[1] { (byte)(1 << (int)byte.Parse(DataCommand.TaxVariant)) });
			dictionary7.Add(1031, NumberToStream((long)(num5 * 100m), 0));
			dictionary7.Add(1081, NumberToStream((long)((DataCommand.ElectronicPayment + DataCommand.CashLessType1 + DataCommand.CashLessType2 + DataCommand.CashLessType3) * 100m), 0));
			dictionary7.Add(1215, NumberToStream((long)(DataCommand.AdvancePayment * 100m), 0));
			dictionary7.Add(1216, NumberToStream((long)(DataCommand.Credit * 100m), 0));
			dictionary7.Add(1217, NumberToStream((long)(DataCommand.CashProvision * 100m), 0));
			dictionary7.Add(1008, StringToStream(DataCommand.ClientAddress, 0, 32));
			dictionary7.Add(1117, StringToStream(Kkm.SenderEmail, 0, 32));
			if (DataCommand.ClientInfo != null && DataCommand.ClientInfo != "")
			{
				dictionary7.Add(1227, StringToStream(DataCommand.ClientInfo, 0, 32));
			}
			if (DataCommand.ClientINN != null && DataCommand.ClientINN != "")
			{
				dictionary7.Add(1228, StringToStream(DataCommand.ClientINN, 0, 32));
			}
			if (DataCommand.AdditionalAttribute != null && DataCommand.AdditionalAttribute != "")
			{
				dictionary7.Add(1192, StringToStream(DataCommand.AdditionalAttribute, 0, 32));
			}
			if (DataCommand.UserAttribute != null)
			{
				if (DataCommand.UserAttribute.Name != null && DataCommand.UserAttribute.Name != "")
				{
					dictionary7.Add(1085, StringToStream(DataCommand.UserAttribute.Name, 0, 32));
				}
				if (DataCommand.UserAttribute.Value != null && DataCommand.UserAttribute.Value != "")
				{
					dictionary7.Add(1086, StringToStream(DataCommand.UserAttribute.Value, 0, 32));
				}
			}
			if (IsCommandBad(RezultCommand, await RunCommand(45u, new MemoryStream(DictionaryToStream(dictionary7))), OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			byte b4 = 0;
			if (DataCommand.TypeCheck == 0)
			{
				b4 = 1;
			}
			else if (DataCommand.TypeCheck == 1)
			{
				b4 = 2;
			}
			else if (DataCommand.TypeCheck == 10)
			{
				b4 = 3;
			}
			else if (DataCommand.TypeCheck == 11)
			{
				b4 = 4;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(b4);
			binaryWriter.Write(NumberToStream((long)(AllSum * 100m), 5));
			binaryWriter.Write(StringToStream((Cdacha > 0m) ? ("Сдача:<#0#>" + Cdacha.ToString("F2")) : "", 0, 32));
			byte[] buffer = await RunCommand(36u, memoryStream);
			if (IsCommandBad(RezultCommand, buffer, OpenSerial, ClearCheck: true, "Не удалось закрыть чек"))
			{
				return;
			}
			RezultCommand.CheckNumber = (long)NumberFromStream(buffer, 3, 4);
			ClearCheck = false;
		}
		PortLogs.Append("Конец регистрации чека", "-");
		RezultCommand.Error = Error;
		IsNotErrorStatus = true;
		if (Kkm.FfdVersion >= 4 && DataCommand.IsFiscalCheck)
		{
			MemoryStream memoryStream = new MemoryStream();
			if (IsCommandBad(RezultCommand, await RunCommand(149u, memoryStream), OpenSerial, ClearCheck: false, "Не удалось сбросить коды маркировки"))
			{
				Error = "";
			}
		}
		if (!DataCommand.IsFiscalCheck)
		{
			await RunCommand(98u, new MemoryStream(new byte[1] { 1 }));
			Error = "";
		}
		if (DataCommand.IsFiscalCheck)
		{
			RezultCommand.QRCode = await GetUrlDoc(ShekOrDoc: true);
			await GetCheckAndSession(RezultCommand);
		}
		for (int num6 = DataCommand.NumberCopies; num6 > 0; num6--)
		{
		}
		_ = DataCommand.Sound;
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
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)0);
			binaryWriter.Write(StringToStream(GoodCodeData.TryBarCode, GoodCodeData.TryBarCode.Length, 32));
			byte[] array = await RunCommand(145u, memoryStream);
			if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось произвести локальную проверку кода маркировки"))
			{
				Error = "";
				BadBarCode.Add(GoodCodeData.TryBarCode);
				continue;
			}
			byte CodeRezultFN = array[2];
			Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
			int statusMarkingCode = GetStatusMarkingCode(DataCommand.TypeCheck, GoodCodeData.MeasureOfQuantity);
			dictionary.Add(2003, NumberToStream(statusMarkingCode, 1));
			dictionary.Add(2102, NumberToStream(0L, 1));
			dictionary.Add(2108, NumberToStream(GoodCodeData.MeasureOfQuantity.Value, 1));
			if (GoodCodeData.PackageQuantity.HasValue)
			{
				dictionary.Add(1023, NumberToStream((long)GoodCodeData.Quantity, 2));
				dictionary.Add(1293, NumberToStream((long)GoodCodeData.Quantity, 4));
				dictionary.Add(1294, NumberToStream(GoodCodeData.PackageQuantity.Value, 4));
			}
			else
			{
				dictionary.Add(1023, NumberToStream((long)GoodCodeData.Quantity, 2));
				dictionary.Add(1293, NumberToStream((long)GoodCodeData.Quantity, 4));
				dictionary.Add(1294, NumberToStream(1L, 4));
			}
			array = await RunCommand(146u, new MemoryStream(DictionaryToStream(dictionary)));
			uint validationResult;
			if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось произвести проверку кода маркировки"))
			{
				Error = "";
				validationResult = ((CodeRezultFN != 0 && CodeRezultFN != 1) ? 1u : 0u);
			}
			else
			{
				validationResult = array[1];
			}
			bool num = MarkingCodeIsBad(validationResult);
			bool acceptMarking = true;
			if (num && !GoodCodeData.AcceptOnBad)
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
			array = await RunCommand(148u, memoryStream);
			if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось подтвердить код маркировки"))
			{
				return;
			}
			validationResult = array[1];
			ItemValidation.ValidationKKT.ValidationResult = validationResult;
			ItemValidation.ValidationKKT.DecryptionResult = GetMarkingCodeDecryptionResult(ItemValidation.ValidationKKT.ValidationResult, 0u, 1u, CodeRezultFN);
			if (acceptMarking || !InCheck)
			{
				continue;
			}
			throw new Exception("Код маркировки не прошел проверку: " + ItemValidation.ValidationKKT.DecryptionResult);
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
		if (NameDevice == "Terminal-FA")
		{
			DataCommand.NotPrint = true;
		}
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
		byte[] array = new byte[1] { (DataCommand.NotPrint == true) ? ((byte)1) : ((byte)0) };
		byte[] array2 = await RunCommand(33u, new MemoryStream(array), 60000);
		if (array2 != null && array2.Length >= 2 && array2[0] == 1 && array2[1] == 1)
		{
			array2 = await RunCommand(33u, new MemoryStream(), 60000);
		}
		if (IsCommandBad(RezultCommand, array2, OpenSerial, ClearCheck: false, "Не удалось открыть смену"))
		{
			return;
		}
		await SerCashier(DataCommand);
		array2 = await RunCommand(34u, new MemoryStream(new byte[0]));
		if (array2 != null && array2.Length >= 2 && array2[0] == 1 && array2[1] == 1)
		{
			array2 = await RunCommand(34u, new MemoryStream(new byte[1] { (DataCommand.NotPrint == true) ? ((byte)1) : ((byte)0) }));
		}
		if (!IsCommandBad(RezultCommand, array2, OpenSerial, ClearCheck: false, "Не удалось открыть смену"))
		{
			RezultCommand.QRCode = await GetUrlDoc();
			await GetCheckAndSession(RezultCommand);
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task CloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		if (NameDevice == "Terminal-FA")
		{
			DataCommand.NotPrint = true;
		}
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
		byte[] array = new byte[1] { (DataCommand.NotPrint == true) ? ((byte)1) : ((byte)0) };
		byte[] array2 = await RunCommand(41u, new MemoryStream(array));
		if (array2 != null && array2.Length >= 2 && array2[0] == 1 && array2[1] == 1)
		{
			array2 = await RunCommand(41u, new MemoryStream());
		}
		if (IsCommandBad(RezultCommand, array2, OpenSerial, ClearCheck: false, "Не удалось закрыть смену"))
		{
			return;
		}
		await SerCashier(DataCommand);
		array2 = await RunCommand(42u, new MemoryStream());
		if (array2 != null && array2.Length >= 2 && array2[0] == 1 && array2[1] == 1)
		{
			array2 = await RunCommand(42u, new MemoryStream(new byte[1] { (DataCommand.NotPrint == true) ? ((byte)1) : ((byte)0) }));
		}
		if (IsCommandBad(RezultCommand, array2, OpenSerial, ClearCheck: false, "Не удалось закрыть смену"))
		{
			return;
		}
		RezultCommand.QRCode = await GetUrlDoc();
		await GetCheckAndSession(RezultCommand);
		if (SettDr.Paramets["SetDateTime"].AsBool())
		{
			try
			{
				Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
				dictionary.Add(30000, DateToStream(DateTime.Now, 5));
				await RunCommand(114u, new MemoryStream(DictionaryToStream(dictionary)));
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
		if (NameDevice == "Terminal-FA")
		{
			DataCommand.NotPrint = true;
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
		if (!IsCommandBad(RezultCommand, await RunCommand(178u, new MemoryStream()), OpenSerial, ClearCheck: false, "Не удалось Напечатать X отчет"))
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
		await SerCashier(DataCommand);
		if (!IsCommandBad(RezultCommand, await RunCommand(39u, new MemoryStream()), OpenSerial, ClearCheck: false, "Не удалось Напечатать отчет диагностики соединения с ОФД") && !IsCommandBad(RezultCommand, await RunCommand(40u, new MemoryStream()), OpenSerial, ClearCheck: false, "Не удалось Напечатать отчет диагностики соединения с ОФД"))
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
		Error = "Команда не поддерживается оборудованием";
	}

	public override async Task DepositingCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		if (NameDevice == "Terminal-FA")
		{
			DataCommand.NotPrint = true;
		}
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
		if (!IsCommandBad(RezultCommand, await RunCommand(179u, new MemoryStream(DecimalToStream(DataCommand.Amount, 0))), OpenSerial, ClearCheck: false, "Не удалась операция Внесение денег"))
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
		if (NameDevice == "Terminal-FA")
		{
			DataCommand.NotPrint = true;
		}
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
		if (!IsCommandBad(RezultCommand, await RunCommand(180u, new MemoryStream(DecimalToStream(DataCommand.Amount, 0))), OpenSerial, ClearCheck: false, "Не удалась операция Изъятие денег"))
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
		Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
		dictionary.Add(30000, DateToStream(DateTime.Now, 5));
		byte[] array = await RunCommand(114u, new MemoryStream(DictionaryToStream(dictionary)));
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка установки даты-времени"))
		{
			return;
		}
		if (DataCommand.RegKkmOfd.Command == "Open" || DataCommand.RegKkmOfd.Command == "ChangeOFD")
		{
			dictionary = new Dictionary<int, byte[]>();
			if (DataCommand.RegKkmOfd.UrlServerOfd.Any((char c) => char.IsLetter(c)))
			{
				dictionary.Add(30040, StringToStream(DataCommand.RegKkmOfd.UrlServerOfd, 0, 32));
			}
			else
			{
				dictionary.Add(30005, StringToStream(DataCommand.RegKkmOfd.UrlServerOfd, 0, 32));
			}
			dictionary.Add(30006, NumberToStream(int.Parse(DataCommand.RegKkmOfd.PortServerOfd), 2));
			dictionary.Add(30009, NumberToStream(30L, 2));
			array = await RunCommand(118u, new MemoryStream(DictionaryToStream(dictionary)));
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка передачи данных соединения ОФД"))
			{
				return;
			}
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeOrganization")
		{
			DataCommand.RegKkmOfd.NameOFD = Kkm.NameOFD;
			DataCommand.RegKkmOfd.InnOfd = Kkm.InnOfd;
			DataCommand.RegKkmOfd.AutomaticNumber = Kkm.AutomaticNumber;
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeOFD")
		{
			DataCommand.RegKkmOfd.AutomaticNumber = Kkm.AutomaticNumber;
		}
		if (DataCommand.RegKkmOfd.Command == "ChangeKkm")
		{
			DataCommand.RegKkmOfd.NameOFD = Kkm.NameOFD;
			DataCommand.RegKkmOfd.InnOfd = Kkm.InnOfd;
		}
		if (DataCommand.RegKkmOfd.Command == "Open" || DataCommand.RegKkmOfd.Command == "ChangeFN" || DataCommand.RegKkmOfd.Command == "ChangeOrganization" || DataCommand.RegKkmOfd.Command == "ChangeKkm" || DataCommand.RegKkmOfd.Command == "ChangeOFD")
		{
			byte b = (byte)((!(DataCommand.RegKkmOfd.Command == "Open")) ? ((DataCommand.RegKkmOfd.Command == "ChangeFN") ? 1 : 2) : 0);
			if (DataCommand.RegKkmOfd.SetFfdVersion <= 3)
			{
				array = await RunCommand(18u, new MemoryStream(new byte[1] { b }));
			}
			else if (DataCommand.RegKkmOfd.SetFfdVersion >= 4)
			{
				array = await RunCommand(225u, new MemoryStream(new byte[1] { b }));
			}
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка открытия отчета о переригистрации"))
			{
				return;
			}
			dictionary = new Dictionary<int, byte[]>();
			dictionary.Add(1048, StringToStream(DataCommand.RegKkmOfd.NameOrganization, 0, 32));
			dictionary.Add(1009, StringToStream(DataCommand.RegKkmOfd.AddressSettle, 0, 32));
			dictionary.Add(1187, StringToStream(DataCommand.RegKkmOfd.PlaceSettle, 0, 32));
			dictionary.Add(1021, StringToStream((DataCommand.CashierName != null) ? DataCommand.CashierName : "", 0, 32));
			dictionary.Add(1203, StringToStream((DataCommand.CashierVATIN != null) ? DataCommand.CashierVATIN : "", 12, 32));
			dictionary.Add(1017, StringToStream(DataCommand.RegKkmOfd.InnOfd, 12, 32));
			dictionary.Add(1046, StringToStream(DataCommand.RegKkmOfd.NameOFD, 0, 32));
			dictionary.Add(1117, StringToStream(DataCommand.RegKkmOfd.SenderEmail, 0, 32));
			if (DataCommand.RegKkmOfd.AutomaticNumber != "")
			{
				dictionary.Add(1036, StringToStream(DataCommand.RegKkmOfd.AutomaticNumber, 0, 32));
			}
			string[] array2;
			if (DataCommand.RegKkmOfd.SetFfdVersion <= 3)
			{
				byte b2 = 0;
				array2 = DataCommand.RegKkmOfd.SignOfAgent.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in array2)
				{
					b2 = (byte)(b2 + (1 << int.Parse(s)));
				}
				dictionary.Add(1057, new byte[1] { b2 });
			}
			int num2 = 0;
			num2 += (DataCommand.RegKkmOfd.EncryptionMode ? 1 : 0);
			num2 += (DataCommand.RegKkmOfd.OfflineMode ? 2 : 0);
			num2 += (DataCommand.RegKkmOfd.AutomaticMode ? 4 : 0);
			num2 += (DataCommand.RegKkmOfd.ServiceMode ? 8 : 0);
			num2 += (DataCommand.RegKkmOfd.BSOMode ? 16 : 0);
			num2 += (DataCommand.RegKkmOfd.InternetMode ? 32 : 0);
			dictionary.Add(9999, new byte[1] { (byte)num2 });
			if (DataCommand.RegKkmOfd.SetFfdVersion >= 4)
			{
				num2 = 0;
				num2 += (DataCommand.RegKkmOfd.SaleExcisableGoods ? 1 : 0);
				num2 += (DataCommand.RegKkmOfd.SignOfGambling ? 2 : 0);
				num2 += (DataCommand.RegKkmOfd.SignOfLottery ? 4 : 0);
				num2 += (DataCommand.RegKkmOfd.PrinterAutomatic ? 8 : 0);
				num2 += (DataCommand.RegKkmOfd.SaleMarking ? 16 : 0);
				num2 += (DataCommand.RegKkmOfd.SignPawnshop ? 32 : 0);
				num2 += (DataCommand.RegKkmOfd.SignAssurance ? 64 : 0);
				dictionary.Add(9998, new byte[1] { (byte)num2 });
			}
			if (DataCommand.RegKkmOfd.SetFfdVersion <= 3)
			{
				array = await RunCommand(22u, new MemoryStream(DictionaryToStream(dictionary)));
			}
			else if (DataCommand.RegKkmOfd.SetFfdVersion >= 4)
			{
				array = await RunCommand(226u, new MemoryStream(DictionaryToStream(dictionary)));
			}
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка передачи данных отчета о переригистрации"))
			{
				return;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(StringToStream(DataCommand.RegKkmOfd.InnOrganization, 12, 32));
			binaryWriter.Write(StringToStream(DataCommand.RegKkmOfd.RegNumber, 20, 32));
			string[] array3 = DataCommand.RegKkmOfd.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			byte b3 = 0;
			if (array3.Length == 0)
			{
				array3 = Kkm.TaxVariant.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			}
			array2 = array3;
			foreach (string s2 in array2)
			{
				b3 = (byte)(b3 + (1 << int.Parse(s2)));
			}
			binaryWriter.Write(b3);
			byte b4 = 0;
			if (DataCommand.RegKkmOfd.Command == "ChangeFN")
			{
				b4 = 1;
			}
			else if (DataCommand.RegKkmOfd.Command == "ChangeOFD")
			{
				b4 = 2;
			}
			else if (DataCommand.RegKkmOfd.Command == "ChangeOrganization")
			{
				b4 = 3;
			}
			else if (DataCommand.RegKkmOfd.Command == "ChangeKkm")
			{
				b4 = 4;
			}
			if (b4 != 0)
			{
				binaryWriter.Write(b4);
			}
			if (DataCommand.RegKkmOfd.SetFfdVersion <= 3)
			{
				array = await RunCommand(19u, new MemoryStream(memoryStream.ToArray()));
			}
			else if (DataCommand.RegKkmOfd.SetFfdVersion >= 4)
			{
				array = await RunCommand(227u, new MemoryStream(memoryStream.ToArray()));
			}
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка закрытия отчета о переригистрации"))
			{
				return;
			}
			RezultCommand.QRCode = "Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + NumberFromStream(array, 1, 4).ToString("D0") + ", ФПД: " + ((uint)((array[8] << 24) + (array[7] << 16) + (array[6] << 8) + array[5])).ToString("D0");
			Error = "";
		}
		if (DataCommand.RegKkmOfd.Command == "Close")
		{
			if (IsCommandBad(null, await RunCommand(20u, new MemoryStream(new byte[0])), OpenSerial: false, ClearCheck: false, "Ошибка открытия отчета о закрытии ФН"))
			{
				return;
			}
			dictionary = new Dictionary<int, byte[]>();
			dictionary.Add(1021, StringToStream((DataCommand.CashierName != null) ? DataCommand.CashierName : "", 0, 32));
			dictionary.Add(1203, StringToStream((DataCommand.CashierVATIN != null) ? DataCommand.CashierVATIN : "", 12, 32));
			if (IsCommandBad(null, await RunCommand(23u, new MemoryStream(DictionaryToStream(dictionary))), OpenSerial, ClearCheck: false, "Ошибка передачи данных отчета о закрытии ФН"))
			{
				return;
			}
			array = await RunCommand(21u, new MemoryStream(new byte[0]));
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Ошибка открытия отчета о закрытии ФН"))
			{
				return;
			}
			RezultCommand.QRCode = "Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + NumberFromStream(array, 1, 4).ToString("D0") + ", ФПД: " + ((uint)((array[8] << 24) + (array[7] << 16) + (array[6] << 8) + array[5])).ToString("D0");
			Error = "";
		}
		await ProcessInitDevice(FullInit: true);
		OfdStatusFullRead = false;
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
		byte[] array;
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			Error = "";
			array = await RunCommand(8u, new MemoryStream());
			if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
			{
				Kkm.FN_Status = array[1];
			}
			if ((Kkm.FN_Status & 8) > 0)
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
				RezultCommand.Status = ExecuteStatus.Ok;
				return;
			}
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
		OfdStatusFullRead = false;
		await ReadStatusOFD(Full: true);
		SessionOpen = 1;
		array = await RunCommand(32u, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось получить статус ККМ"))
		{
			Error = "";
			SessionOpen = 1;
			Kkm.FN_IsFiscal = false;
			if (Kkm.PrintingWidth == 0)
			{
				Kkm.PrintingWidth = 36;
			}
		}
		else if (array[1] == 1)
		{
			SessionOpen = 2;
		}
		else
		{
			SessionOpen = 1;
		}
		array = await RunCommand(1u, new MemoryStream());
		if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, "ККМ не подключена!"))
		{
			Kkm.PaperOver = array[19] != 0;
			Kkm.FN_Status = array[21];
			Kkm.DateTimeKKT = DateFromStream(array, 13, 5);
			if (Global.Settings.SetNotActiveOnPaperOver && NameDevice != "Terminal-FA")
			{
				IsInit = !Kkm.PaperOver;
			}
			else
			{
				IsInit = true;
			}
			await base.GetDataKKT(DataCommand, RezultCommand);
			await GetCheckAndSession(RezultCommand);
			RezultCommand.Info.SessionState = SessionOpen;
			byte[] array2 = await RunCommand(176u, new MemoryStream());
			if (!IsCommandBad(null, array2, OpenSerial: false, ClearCheck: false, ""))
			{
				decimal num = NumberFromStream(array2, 1, 5);
				RezultCommand.Info.BalanceCash = num / 100m;
			}
			Error = "";
			if (OpenSerial)
			{
				await PortCloseAsync();
			}
			RezultCommand.Status = ExecuteStatus.Ok;
		}
	}

	public override async Task<bool> CloseDocumentAndOpenShift(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, ""))
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
		await ClearOldCheck();
		if (DataCommand.IsFiscalCheck)
		{
			byte[] array = await RunCommand(32u, new MemoryStream());
			if (IsCommandBad(RezultCommand, array, OpenSerial, ClearCheck: false, "Не удалось получить данные сессии"))
			{
				return false;
			}
			if (array[1] == 1)
			{
				SessionOpen = 2;
			}
			else
			{
				SessionOpen = 1;
			}
			if (SessionOpen == 3)
			{
				CreateTextError(22, Error);
				IsCommandBad(RezultCommand, null, OpenSerial, ClearCheck: false, "Не удалось зарегистрировать документ");
				return false;
			}
			if (DataCommand.IsFiscalCheck && SessionOpen == 1)
			{
				await OpenShift(DataCommand, new RezultCommandKKm());
			}
			BadBarCode.Clear();
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Error = "";
		RezultCommand.Status = ExecuteStatus.Ok;
		return true;
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
		byte[] CountersSummShift = await RunCommand(218u, new MemoryStream(new byte[1]));
		if (IsCommandBad(null, CountersSummShift, OpenSerial, ClearCheck: false, "Не удалось счетчики ФН"))
		{
			return;
		}
		byte[] array = await RunCommand(218u, new MemoryStream(new byte[1] { 1 }));
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, "Не удалось счетчики ФН"))
		{
			return;
		}
		string[] array2 = "Total,Shift".Split(',');
		string[] array3 = "Shell,ShellReturn,Buy,BuyReturn".Split(',');
		string[] array4 = array2;
		foreach (string text in array4)
		{
			string[] array5 = array3;
			foreach (string text2 in array5)
			{
				byte[] data = null;
				int num = 0;
				int num2 = 0;
				switch (text + text2)
				{
				case "TotalShell":
					data = array;
					num = 7;
					num2 = 315;
					break;
				case "TotalShellReturn":
					data = array;
					num = 83;
					num2 = 325;
					break;
				case "TotalBuy":
					data = array;
					num = 159;
					num2 = 335;
					break;
				case "TotalBuyReturn":
					data = array;
					num = 235;
					num2 = 345;
					break;
				case "ShiftShell":
					data = CountersSummShift;
					num = 7;
					num2 = 315;
					break;
				case "ShiftShellReturn":
					data = CountersSummShift;
					num = 83;
					num2 = 325;
					break;
				case "ShiftBuy":
					data = CountersSummShift;
					num = 159;
					num2 = 335;
					break;
				case "ShiftBuyReturn":
					data = CountersSummShift;
					num = 235;
					num2 = 345;
					break;
				}
				RezultCounters.tСounter tСounter = new RezultCounters.tСounter();
				tСounter.CountersType = text;
				tСounter.ReceiptType = text2;
				tСounter.Count += (uint)(int)NumberFromStream(data, num, 4);
				tСounter.Sum += DecimalFromStream(data, num + 4, 6);
				tСounter.Cash += DecimalFromStream(data, num + 4 + 6, 6);
				tСounter.ElectronicPayment += DecimalFromStream(data, num + 4 + 12, 6);
				tСounter.AdvancePayment += DecimalFromStream(data, num + 4 + 18, 6);
				tСounter.Credit += DecimalFromStream(data, num + 4 + 24, 6);
				tСounter.CashProvision += DecimalFromStream(data, num + 4 + 30, 6);
				tСounter.Tax22 += DecimalFromStream(data, num + 4 + 36, 6);
				tСounter.Tax10 += DecimalFromStream(data, num + 4 + 42, 6);
				tСounter.Tax0 += DecimalFromStream(data, num + 4 + 48, 6);
				tСounter.TaxNo += DecimalFromStream(data, num + 4 + 54, 6);
				tСounter.Tax122 += DecimalFromStream(data, num + 4 + 60, 6);
				tСounter.Tax110 += DecimalFromStream(data, num + 4 + 66, 6);
				tСounter.CorrectionsCount += (uint)(int)NumberFromStream(data, num2, 4);
				tСounter.CorrectionsSum = DecimalFromStream(data, num2 + 4, 6);
				RezultCommand.Counters.Add(tСounter);
			}
		}
		if (OpenSerial)
		{
			await PortCloseAsync();
		}
		RezultCommand.Error = "";
	}

	public static async Task<bool> TestСommunication(string IP, string Port, string Com)
	{
		KktKIT KktKITUnit = new KktKIT(null, 0);
		if (IP != "")
		{
			KktKITUnit.SetPort.TypeConnect = SetPorts.enTypeConnect.IP;
			KktKITUnit.SetPort.IP = IP;
			KktKITUnit.SetPort.Port = Port;
		}
		else
		{
			KktKITUnit.SetPort.TypeConnect = SetPorts.enTypeConnect.Com;
			KktKITUnit.SetPort.ComId = Com;
		}
		bool Rez = false;
		bool OpenSerial = await KktKITUnit.PortOpenAsync();
		if (KktKITUnit.Error != "")
		{
			return false;
		}
		if (!KktKITUnit.IsCommandBad(null, await KktKITUnit.RunCommand(1u, new MemoryStream(), 3000), OpenSerial: false, ClearCheck: false, "ККМ не подключена!"))
		{
			Rez = true;
		}
		if (OpenSerial)
		{
			await KktKITUnit.PortCloseAsync();
		}
		if (KktKITUnit.SetPort.SerialPort != null)
		{
			KktKITUnit.SetPort.SerialPort.Dispose();
		}
		return Rez;
	}

	public override void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		CalkPrintOnPage(this, DataCommand);
		if (DataCommand.AdditionalActions == "UpdateFrimware")
		{
			PeriodWorkCommand = 360;
			string text = null;
			DateTime dateTime = default(DateTime);
			bool result = PortOpenAsync().Result;
			if (IsCommandBad(null, null, result, ClearCheck: false, ""))
			{
				return;
			}
			GetDataKKT(DataCommand, new RezultCommandKKm()).Wait();
			if (Kkm.Firmware_Status == 1)
			{
				text = Kkm.Firmware_Version;
				dateTime = DateTime.Now;
			}
			byte[] result2 = RunCommand(224u, new MemoryStream()).Result;
			IsCommandBad(RezultCommand, result2, result, ClearCheck: false, "Ошибка обновления ККТ");
			IsInit = false;
			if (result)
			{
				PortCloseAsync().Wait();
			}
			if (text != null)
			{
				do
				{
					Task.Delay(1000).Wait();
					Error = "";
					IsInit = false;
					ProcessInitDevice().Wait();
				}
				while (!(text != Kkm.Firmware_Version) && !(dateTime.AddMinutes(5.0) < DateTime.Now));
			}
		}
		base.DoAdditionalAction(DataCommand, ref RezultCommand);
	}

	public override async Task ReadStatusOFD(bool Full = false, bool ReadInfoGer = false, bool NoInit = false)
	{
		Error = "";
		bool OpenSerial = await PortOpenAsync();
		if (IsCommandBad(null, null, OpenSerial, ClearCheck: false, ""))
		{
			return;
		}
		try
		{
			byte[] array = await RunCommand(5u, new MemoryStream());
			if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
			{
				Kkm.Fn_Number = StringFromStream(array, 1);
			}
			else
			{
				Error = "";
			}
			if (Full && !OfdStatusFullRead)
			{
				Kkm.FfdSupportVersion = 2;
				Kkm.FfdVersion = 2;
				Kkm.FfdMinimumVersion = 2;
				array = await RunCommand(15u, new MemoryStream());
				if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					Kkm.FfdVersion = array[1];
					Kkm.FfdSupportVersion = array[2];
					Kkm.FfdMinimumVersion = Kkm.FfdSupportVersion;
					if (Kkm.FfdVersion == 0)
					{
						Kkm.FfdVersion = Kkm.FfdSupportVersion;
					}
				}
				else
				{
					Error = "";
				}
				Kkm.INN = "";
				Kkm.RegNumber = "";
				Kkm.TaxVariant = "";
				Kkm.SignOfAgent = "";
				array = await RunCommand(10u, new MemoryStream());
				if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					Kkm.RegNumber = StringFromStream(array, 1, 20);
					Kkm.INN = StringFromStream(array, 21, 12);
					byte b = array[34];
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
					byte b2 = array[35];
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
					byte b3 = array[33];
					Kkm.EncryptionMode = (b3 & 1) == 1;
					Kkm.OfflineMode = (b3 & 2) == 2;
					Kkm.AutomaticMode = (b3 & 4) == 4;
					Kkm.InternetMode = (b3 & 0x20) == 32;
					Kkm.BSOMode = (b3 & 0x10) == 16;
					Kkm.ServiceMode = (b3 & 8) == 8;
				}
				else
				{
					Error = "";
				}
				Kkm.FN_IsFiscal = false;
				Kkm.FN_Status = 0;
				Kkm.FN_MemOverflowl = false;
				array = await RunCommand(8u, new MemoryStream());
				if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					Kkm.FN_Status = array[1];
					Kkm.FN_MemOverflowl = (array[5] & 4) != 0;
				}
				else
				{
					Error = "";
				}
				if (Kkm.FN_Status == 3)
				{
					Kkm.FN_IsFiscal = true;
				}
				array = await RunCommand(7u, new MemoryStream());
				if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					Kkm.FN_DateEnd = DateFromStream(array, 1, 3);
				}
				else
				{
					Error = "";
				}
				Kkm.UrlServerOfd = "";
				Dictionary<int, byte[]> dictionary = await GeSettingsTLV(119);
				if (dictionary.ContainsKey(30040))
				{
					Kkm.UrlServerOfd = StringFromStream(dictionary[30040], 0);
				}
				if (Kkm.UrlServerOfd == "" && dictionary.ContainsKey(30005))
				{
					Kkm.UrlServerOfd = StringFromStream(dictionary[30005], 0);
				}
				Kkm.PortServerOfd = NumberFromStream(dictionary[30006], 0).ToString();
				Kkm.OFD_NumErrorDoc = 0;
				Kkm.OFD_DateErrorDoc = default(DateTime);
				array = await RunCommand(80u, new MemoryStream());
				if (!IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					Kkm.OFD_NumErrorDoc = (int)NumberFromStream(array, 3, 2);
					Kkm.OFD_DateErrorDoc = DateFromStream(array, 9, 5);
				}
				else
				{
					Error = "";
				}
			}
			bool IsRegDateRead = false;
			Kkm.Organization = "<Не назначено>";
			Kkm.AddressSettle = "<Не назначено>";
			Kkm.PlaceSettle = "<Не назначено>";
			Kkm.SenderEmail = "<Не назначено>";
			Kkm.NameOFD = "";
			Kkm.UrlOfd = "";
			Kkm.InnOfd = "";
			Kkm.UrlTax = "";
			int num = 14;
			int ColReadTeg = 8;
			if (Kkm.FfdVersion >= 4)
			{
				ColReadTeg = 9;
			}
			for (int i2 = num; i2 > 0; i2--)
			{
				array = await RunCommand(59u, new MemoryStream(new byte[1] { (byte)i2 }));
				if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса данных регистрации"))
				{
					int num2 = 1;
					int num3 = 0;
					while (num2 < array.Length)
					{
						int num4 = (array[num2 + 1] << 8) + array[num2];
						int num5 = (array[num2 + 3] << 8) + array[num2 + 2];
						switch (num4)
						{
						case 1048:
							Kkm.Organization = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1009:
							Kkm.AddressSettle = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1187:
							Kkm.PlaceSettle = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1117:
							Kkm.SenderEmail = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1046:
							Kkm.NameOFD = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1017:
							Kkm.InnOfd = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1060:
							Kkm.UrlTax = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1036:
							Kkm.AutomaticNumber = StringFromStream(array, num2 + 4, num5);
							num3++;
							break;
						case 1290:
						{
							int num6 = (int)NumberFromStream(array, num2 + 4, num5);
							num3++;
							Kkm.PrinterAutomatic = (num6 & 2) > 0;
							Kkm.BSOMode = (num6 & 4) > 0;
							Kkm.InternetMode = (num6 & 0x20) > 0;
							Kkm.SaleExcisableGoods = (num6 & 0x40) > 0;
							Kkm.SaleMarking = (num6 & 0x100) > 0;
							Kkm.ServiceMode = (num6 & 0x200) > 0;
							Kkm.SignOfGambling = (num6 & 0x400) > 0;
							Kkm.SignOfLottery = (num6 & 0x800) > 0;
							Kkm.SignPawnshop = (num6 & 0x1000) > 0;
							Kkm.SignAssurance = (num6 & 0x2000) > 0;
							break;
						}
						}
						num2 = num2 + 4 + num5;
						if (num3 >= ColReadTeg)
						{
							IsRegDateRead = true;
							break;
						}
					}
				}
				else
				{
					Error = "";
				}
				if (IsRegDateRead)
				{
					break;
				}
			}
			ulong DocNumber = 0uL;
			DateTime Date = default(DateTime);
			uint FiscalSign = 0u;
			array = await RunCommand(59u, new MemoryStream(new byte[1] { 1 }));
			if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса данных регистрации"))
			{
				int num7 = 1;
				int num8 = 0;
				while (num7 < array.Length)
				{
					int num9 = (array[num7 + 1] << 8) + array[num7];
					int num10 = (array[num7 + 3] << 8) + array[num7 + 2];
					switch (num9)
					{
					case 1040:
						DocNumber = NumberFromStream(array, num7 + 4, num10);
						num8++;
						break;
					case 1012:
						Date = new DateTime(1970, 1, 1).AddSeconds(NumberFromStream(array, num7 + 4, num10));
						num8++;
						break;
					case 1077:
						FiscalSign = (uint)((array[num7 + 4 + 2] << 24) + (array[num7 + 4 + 3] << 16) + (array[num7 + 4 + 4] << 8) + array[num7 + 4 + 5]);
						num8++;
						break;
					}
					num7 = num7 + 4 + num10;
					if (num8 >= 8)
					{
						IsRegDateRead = true;
						break;
					}
				}
				Kkm.InfoRegKkt = "Дата: " + Date.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + DocNumber.ToString("D0") + ", ФПД: " + FiscalSign.ToString("D0");
				Error = "";
				Kkm.FN_DateStart = Date.Date;
			}
			else
			{
				Error = "";
			}
			if (!IsRegDateRead)
			{
				array = await RunCommand(53u, new MemoryStream(NumberToStream(1L, 4)));
				if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса документа"))
				{
					NumberFromStream(array, 3, 2);
				}
				int i2 = 0;
				do
				{
					array = await RunCommand(54u, new MemoryStream());
					if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, ""))
					{
						break;
					}
					string text = NumberFromStream(array, 1, 2).ToString();
					int countByte = (int)NumberFromStream(array, 3, 2);
					switch (text)
					{
					case "1048":
						Kkm.Organization = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1009":
						Kkm.AddressSettle = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1187":
						Kkm.PlaceSettle = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1117":
						Kkm.SenderEmail = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1046":
						Kkm.NameOFD = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1017":
						Kkm.InnOfd = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1060":
						Kkm.UrlTax = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1036":
						Kkm.AutomaticNumber = StringFromStream(array, 5, countByte);
						i2++;
						break;
					case "1040":
						DocNumber = NumberFromStream(array, 5, countByte);
						i2++;
						break;
					case "1012":
						Date = new DateTime(1970, 1, 1).AddSeconds(NumberFromStream(array, 5, countByte));
						i2++;
						break;
					case "1077":
						FiscalSign = (uint)((array[7] << 24) + (array[8] << 16) + (array[9] << 8) + array[10]);
						i2++;
						break;
					}
				}
				while (i2 < 11);
				Kkm.InfoRegKkt = "Дата: " + Date.ToString("dd.MM.yyyy HH:mm") + ", ФД: " + DocNumber.ToString("D0") + ", ФПД: " + FiscalSign.ToString("D0");
				Error = "";
				Kkm.FN_DateStart = Date.Date;
			}
			Kkm.Firmware_Status = -1;
			array = await RunCommand(223u, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса статуса обновления"))
			{
				Error = "";
			}
			else
			{
				bool flag = array[1] == 1;
				Kkm.Firmware_Status = (flag ? 1 : 0);
			}
			Kkm.Firmware_Version = "<Не определено>";
			array = await RunCommand(11u, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса статуса обновления"))
			{
				Error = "";
			}
			else
			{
				Kkm.Firmware_Version = StringFromStream(array, 1);
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

	public async Task<Dictionary<int, byte[]>> GeSettingsTLV(byte Command)
	{
		Dictionary<int, byte[]> Rez = new Dictionary<int, byte[]>();
		byte[] array = await RunCommand(Command, new MemoryStream());
		if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Не удалось получить настройку 0x" + Command.ToString("x")))
		{
			return Rez;
		}
		int num = 1;
		while (num < array.Length)
		{
			int key = (array[num + 1] << 8) + array[num];
			int num2 = (array[num + 3] << 8) + array[num + 2];
			byte[] array2 = new byte[num2];
			Array.Copy(array, num + 4, array2, 0, num2);
			num = num + array2.Length + 4;
			Rez.Add(key, array2);
		}
		return Rez;
	}

	public string StringFromStream(byte[] Data, int Pos, int CountByte = 0, bool NulTerminate = false)
	{
		if (CountByte == 0)
		{
			CountByte = Data.Length - Pos;
		}
		string text = null;
		text = e886.GetString(Data, Pos, CountByte).TrimEnd('\0');
		if (NulTerminate && text.IndexOf('\0') != -1)
		{
			text = text.Substring(0, text.IndexOf('\0'));
		}
		return text.Trim();
	}

	public ulong NumberFromStream(byte[] Data, int Pos, int CountByte = 0)
	{
		ulong num = 0uL;
		if (CountByte == 0)
		{
			CountByte = Data.Length - Pos;
		}
		for (int i = 0; i < CountByte; i++)
		{
			ulong num2 = (ulong)(Data[i + Pos] << 8 * i);
			num += num2;
		}
		return num;
	}

	public decimal DecimalFromStream(byte[] Data, int Pos, int CountByte = 0, decimal Divisor = 100m)
	{
		return (decimal)NumberFromStream(Data, Pos, CountByte) / Divisor;
	}

	public DateTime DateFromStream(byte[] Data, byte Pos, int CountByte = 0)
	{
		try
		{
			switch (CountByte)
			{
			case 3:
				return new DateTime(2000 + Data[Pos], Data[Pos + 1], Data[Pos + 2]);
			case 5:
				return new DateTime(2000 + Data[Pos], Data[Pos + 1], Data[Pos + 2], Data[Pos + 3], Data[Pos + 4], 0);
			}
		}
		catch
		{
		}
		return default(DateTime);
	}

	public byte[] NumberToStream(long Number, int CountByte)
	{
		if (CountByte == 0)
		{
			for (int i = 0; i < 100; i++)
			{
				if (Number >> i * 8 <= 255)
				{
					CountByte = i + 1;
					break;
				}
			}
		}
		if (CountByte == 0)
		{
			CountByte = 1;
		}
		byte[] array = new byte[CountByte];
		for (int j = 0; j < CountByte; j++)
		{
			array[j] = (byte)((Number >> j * 8) & 0xFF);
		}
		return array;
	}

	public byte[] DecimalToStream(decimal Number, int CountByte)
	{
		byte b = 0;
		while (decimal.Truncate(Number) != Number)
		{
			b++;
			Number *= 10m;
		}
		long num = (long)Number;
		if (CountByte == 0)
		{
			for (int i = 0; i < 100; i++)
			{
				if (num >> i * 8 <= 255)
				{
					CountByte = i + 1;
					break;
				}
			}
		}
		if (CountByte == 0)
		{
			CountByte = 1;
		}
		byte[] array = new byte[CountByte + 1];
		array[0] = b;
		for (int j = 0; j < CountByte; j++)
		{
			array[j + 1] = (byte)((num >> j * 8) & 0xFF);
		}
		return array;
	}

	public byte[] StringToStream(string Value, int CountByte = 0, byte FillChar = 32)
	{
		if (Value == null)
		{
			Value = "";
		}
		if (CountByte == 0)
		{
			CountByte = Value.Length;
		}
		if (Value.Length > CountByte)
		{
			Value = Value.Substring(0, CountByte);
		}
		byte[] array = new byte[CountByte];
		byte[] bytes = e886.GetBytes(Value);
		for (int i = 0; i < Value.Length; i++)
		{
			array[i] = bytes[i];
		}
		for (int j = Value.Length; j < CountByte; j++)
		{
			array[j] = FillChar;
		}
		return array;
	}

	public byte[] DateToStream(DateTime Date, int CountByte = 0)
	{
		try
		{
			switch (CountByte)
			{
			case 3:
				return new byte[3]
				{
					(byte)(Date.Year - 2000),
					(byte)Date.Month,
					(byte)Date.Day
				};
			case 5:
				return new byte[5]
				{
					(byte)(Date.Year - 2000),
					(byte)Date.Month,
					(byte)Date.Day,
					(byte)Date.Hour,
					(byte)Date.Minute
				};
			case 4:
			{
				int num = (int)DateTime.UtcNow.Subtract(Date).TotalSeconds;
				return NumberToStream(num, 4);
			}
			}
		}
		catch
		{
		}
		return new byte[0];
	}

	public byte[] DictionaryToStream(Dictionary<int, byte[]> Dict)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (KeyValuePair<int, byte[]> item in Dict)
		{
			binaryWriter.Write((byte)(item.Key & 0xFF));
			binaryWriter.Write((byte)((item.Key & 0xFF00) >> 8));
			int num = item.Value.Length;
			binaryWriter.Write((byte)(num & 0xFF));
			binaryWriter.Write((byte)((num & 0xFF00) >> 8));
			binaryWriter.Write(item.Value);
		}
		return memoryStream.ToArray();
	}

	public async Task SerCashier(DataCommand DataCommand)
	{
		if (!(NameDevice == "Terminal-FA") && DataCommand.CashierName != null && DataCommand.CashierName != "")
		{
			Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
			dictionary.Add(1021, StringToStream(DataCommand.CashierName, 0, 32));
			if (Kkm.FfdVersion >= 2 && DataCommand.CashierVATIN != null && DataCommand.CashierVATIN != "")
			{
				dictionary.Add(1203, StringToStream(DataCommand.CashierVATIN, 12, 32));
			}
			if (IsCommandBad(null, await RunCommand(47u, new MemoryStream(DictionaryToStream(dictionary))), OpenSerial: false, ClearCheck: true, "Не удалось зарегистрировать кассира"))
			{
				Error = "";
			}
		}
	}

	public override async Task GetCheckAndSession(RezultCommandKKm RezultCommand, bool IsSessionNumber = true, bool IsCheckNumber = true)
	{
		string SessionCheckNumber = "0";
		int SessionNumber = 0;
		int sts = 0;
		byte[] array = await RunCommand(32u, new MemoryStream());
		if (!IsCommandBad(RezultCommand, array, OpenSerial: false, ClearCheck: false, "Не удалось получить данные сессии"))
		{
			SessionCheckNumber = NumberFromStream(array, 4, 2).ToString();
			SessionNumber = (int)NumberFromStream(array, 2, 2);
			sts = array[1];
		}
		else
		{
			Error = "";
		}
		if (sts == 1)
		{
			SessionOpen = 2;
		}
		else
		{
			SessionOpen = 1;
		}
		if (IsSessionNumber)
		{
			RezultCommand.SessionNumber = SessionNumber;
		}
		if (IsCheckNumber)
		{
			RezultCommand.SessionCheckNumber = int.Parse(SessionCheckNumber);
			RezultCommand.CheckNumber = await GetLastFiscalNumber();
		}
	}

	public async Task<bool> WriteAgentSign(int? AgentSign, DataCommand.TypeAgentData AgentData, DataCommand.TypePurveyorData PurveyorData)
	{
		Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
		if (AgentSign.HasValue)
		{
			dictionary.Add(1057, new byte[1] { (byte)(1 << AgentSign).Value });
		}
		else
		{
			dictionary.Add(1057, new byte[1]);
		}
		if (AgentData != null)
		{
			if (!string.IsNullOrEmpty(AgentData.PayingAgentOperation))
			{
				dictionary.Add(1044, StringToStream(AgentData.PayingAgentOperation, 0, 32));
			}
			if (!string.IsNullOrEmpty(AgentData.PayingAgentPhone))
			{
				dictionary.Add(1073, StringToStream(AgentData.PayingAgentPhone, 0, 32));
			}
			if (!string.IsNullOrEmpty(AgentData.ReceivePaymentsOperatorPhone))
			{
				dictionary.Add(1074, StringToStream(AgentData.ReceivePaymentsOperatorPhone, 0, 32));
			}
			if (!string.IsNullOrEmpty(AgentData.MoneyTransferOperatorPhone))
			{
				dictionary.Add(1075, StringToStream(AgentData.MoneyTransferOperatorPhone, 0, 32));
			}
			if (!string.IsNullOrEmpty(AgentData.MoneyTransferOperatorName))
			{
				dictionary.Add(1026, StringToStream(AgentData.MoneyTransferOperatorName, 0, 32));
			}
			if (!string.IsNullOrEmpty(AgentData.MoneyTransferOperatorAddress))
			{
				dictionary.Add(1005, StringToStream(AgentData.MoneyTransferOperatorAddress, 0, 32));
			}
			if (!string.IsNullOrEmpty(AgentData.MoneyTransferOperatorVATIN))
			{
				dictionary.Add(1016, StringToStream(AgentData.MoneyTransferOperatorVATIN.PadRight(12, ' '), 0, 32));
			}
		}
		if (PurveyorData != null && !string.IsNullOrEmpty(PurveyorData.PurveyorPhone))
		{
			dictionary.Add(1171, StringToStream(PurveyorData.PurveyorPhone, 0, 32));
		}
		if (IsCommandBad(null, await RunCommand(44u, new MemoryStream(DictionaryToStream(dictionary))), OpenSerial: false, ClearCheck: false, "Не удалось передать данные агента"))
		{
			return false;
		}
		return true;
	}

	public async Task ClearOldCheck()
	{
		bool DocOpen = false;
		byte[] array = await RunCommand(8u, new MemoryStream());
		if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, ""))
		{
			DocOpen = array[2] != 0;
		}
		else
		{
			Error = "";
		}
		if (DocOpen)
		{
			await RunCommand(16u, new MemoryStream());
			Error = "";
		}
	}

	public async Task<bool> PrintBarCode(DataCommand.PrintBarcode PrintBarCode)
	{
		if (PrintBarCode.BarcodeType == "EAN13" || PrintBarCode.BarcodeType == "CODE39")
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			streamWriter.Write((!(PrintBarCode.BarcodeType == "EAN13")) ? 1 : 0);
			streamWriter.Write(StringToStream(PrintBarCode.Barcode, 0, 32));
			if (IsCommandBad(null, await RunCommand(105u, memoryStream), OpenSerial: false, ClearCheck: false, "Ошибка печати ШК"))
			{
				return false;
			}
		}
		if (PrintBarCode.BarcodeType == "QR" && IsCommandBad(null, await RunCommand(99u, new MemoryStream(StringToStream(PrintBarCode.Barcode, 0, 32))), OpenSerial: false, ClearCheck: false, "Ошибка печати ШК"))
		{
			return false;
		}
		return true;
	}

	public async Task<string> GetUrlDoc(bool ShekOrDoc = false)
	{
		if (UnitParamets["NoReadQrCode"].AsBool())
		{
			return "";
		}
		string result;
		try
		{
			byte[] array = await RunCommand(8u, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса документа"))
			{
				return "";
			}
			uint DocNumber = (uint)NumberFromStream(array, 27, 4);
			array = await RunCommand(53u, new MemoryStream(NumberToStream(DocNumber, 4)));
			if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса документа"))
			{
				NumberFromStream(array, 3, 2);
			}
			uint LastCheckType = 0u;
			decimal Summ = default(decimal);
			DateTime Date = default(DateTime);
			long FiscalSign = 0L;
			int i = 0;
			do
			{
				array = await RunCommand(54u, new MemoryStream());
				if (IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, ""))
				{
					break;
				}
				string text = NumberFromStream(array, 1, 2).ToString();
				int countByte = (int)NumberFromStream(array, 3, 2);
				if (ShekOrDoc)
				{
					switch (text)
					{
					case "1054":
						LastCheckType = (uint)NumberFromStream(array, 5, 1);
						i++;
						break;
					case "1020":
						Summ = (decimal)NumberFromStream(array, 5, countByte) / 100m;
						i++;
						break;
					case "1042":
						OldSessionCheckNumber = (int)NumberFromStream(array, 5, countByte);
						i++;
						break;
					}
				}
				if (!(text == "1012"))
				{
					if (text == "1077")
					{
						FiscalSign = (uint)((array[7] << 24) + (array[8] << 16) + (array[9] << 8) + array[10]);
						i++;
					}
				}
				else
				{
					Date = new DateTime(1970, 1, 1).AddSeconds(NumberFromStream(array, 5, 4));
					i++;
				}
			}
			while ((!ShekOrDoc || i < 5) && (ShekOrDoc || i < 2));
			Error = "";
			result = "t=" + Date.ToString("yyyyMMddTHHmm") + (ShekOrDoc ? ("&s=" + Summ.ToString("0.00").Replace(',', '.')) : "") + "&fn=" + Kkm.Fn_Number + "&i=" + DocNumber.ToString("D0") + "&fp=" + FiscalSign.ToString("D0") + (ShekOrDoc ? ("&n=" + LastCheckType) : "");
		}
		catch (Exception)
		{
			result = "Ошибка чтения реквизитов документа ";
		}
		return result;
	}

	public override async Task<uint> GetLastFiscalNumber()
	{
		byte[] array = await RunCommand(8u, new MemoryStream());
		if (!IsCommandBad(null, array, OpenSerial: false, ClearCheck: false, "Ошибка запроса документа"))
		{
			return (uint)NumberFromStream(array, 27, 4);
		}
		return 0u;
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
		byte[] array = await RunCommand(53u, memoryStream);
		if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
		{
			return null;
		}
		Rez.Add(0, Unit.NumberFromArray(array, 1, 2).AsString());
		while (true)
		{
			array = await RunCommand(54u, new MemoryStream());
			if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
			{
				break;
			}
			byte[] Data2 = new byte[array.Length - 1];
			Array.Copy(array, 1, Data2, 0, Data2.Length);
			int LenBloc = (Data2[3] << 8) + Data2[2];
			bool Good = true;
			while (LenBloc + 4 > Data2.Length)
			{
				array = await RunCommand(54u, new MemoryStream());
				if (IsCommandBad(null, array, OpenSerial, ClearCheck: false, ""))
				{
					Error = "";
					Good = false;
					break;
				}
				byte[] array2 = new byte[array.Length - 1];
				Array.Copy(array, 1, array2, 0, array2.Length);
				Data2 = Data2.Concat(array2).ToArray();
			}
			if (!Good)
			{
				continue;
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
					dictionary.Add(num, StringFromStream(item, 4, item.Length - 4));
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

	public async Task<byte[]> RunCommand(uint Command, MemoryStream Msg, int TimeOut = 20000, bool OpenPort = true)
	{
		bool OpenSerial;
		if (OpenPort)
		{
			OpenSerial = await PortOpenAsync(ClearBuf: false);
			if (Error != "")
			{
				return new byte[0];
			}
		}
		else
		{
			OpenSerial = true;
		}
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			OpenSerial = true;
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
			binaryWriter.Write((byte)182);
			binaryWriter.Write((byte)41);
			binaryWriter.Write((byte)(((1 + Msg.Length) & 0xFF00) >> 8));
			binaryWriter.Write((byte)((1 + Msg.Length) & 0xFF));
			binaryWriter.Write((byte)Command);
			binaryWriter.Write(Msg.ToArray());
			byte[] array = memoryStream.ToArray();
			ushort num = ushort.MaxValue;
			for (int i = 2; i < array.Length; i++)
			{
				num = (ushort)((num << 8) ^ Table[(num >> 8) ^ (0xFF & array[i])]);
			}
			binaryWriter.Write((byte)(num & 0xFF));
			binaryWriter.Write((byte)((num & 0xFF00) >> 8));
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
		if (b != 182)
		{
			Error = "Ошибка приема кадра сообщения (2)";
			PortLogs.Append(Error);
			if (Global.Settings.SetNotActiveOnError)
			{
				IsInit = false;
			}
			return Data.ToArray();
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
		if (b != 41)
		{
			Error = "Ошибка приема кадра сообщения (2)";
			PortLogs.Append(Error);
			if (Global.Settings.SetNotActiveOnError)
			{
				IsInit = false;
			}
			return Data.ToArray();
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
		byte SizeFrame2;
		try
		{
			SizeFrame2 = await PortReadByteAsync();
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
		int SizeFrame3 = (SizeFrame1 << 8) + SizeFrame2;
		base.PortReadTimeout = 500;
		for (int i = 0; i < SizeFrame3; i++)
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
		byte CRC1;
		try
		{
			CRC1 = await PortReadByteAsync();
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
		byte[] array = Data.ToArray();
		ushort num = ushort.MaxValue;
		num = (ushort)((num << 8) ^ Table[(num >> 8) ^ (0xFF & SizeFrame1)]);
		num = (ushort)((num << 8) ^ Table[(num >> 8) ^ (0xFF & SizeFrame2)]);
		for (int j = 0; j < array.Length; j++)
		{
			num = (ushort)((num << 8) ^ Table[(num >> 8) ^ (0xFF & array[j])]);
		}
		if (num != (ushort)(CRC1 + (b2 << 8)))
		{
			Error = "Не правильная контрольная сумма ответа";
			PortLogs.Append(Error);
		}
		return array.ToArray();
	}

	public virtual async Task<bool> PortOpenAsync(bool ClearBuf = true)
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			Error = "Устройство не настроено";
			PortLogs.Append(Error);
			return false;
		}
		_ = SetPort.PortOpen;
		bool OpenSerial = await base.PortOpenAsync();
		if (ClearBuf)
		{
			for (int x = 0; x < 4; x++)
			{
				Error = "";
				if (!IsCommandBad(null, await RunCommand(1u, new MemoryStream(), 20000, OpenPort: false), OpenSerial, ClearCheck: false, "ККМ не подключена!"))
				{
					break;
				}
				Error = "";
				await Task.Delay(2000);
			}
		}
		return OpenSerial;
	}

	public override async Task<bool> PortCloseAsync()
	{
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.None)
		{
			Error = "Устройство не настроено";
			PortLogs.Append(Error);
			return false;
		}
		if (SetPort.TypeConnect != SetPorts.enTypeConnect.Com || !NotCloseCom)
		{
			return await base.PortCloseAsync();
		}
		return true;
	}

	public bool IsCommandBad(RezultCommand RezultCommand, byte[] Buffer, bool OpenSerial, bool ClearCheck, string ErrorText)
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
			if (Buffer == null || Buffer.Length <= 1 || Buffer[0] == 0)
			{
				return false;
			}
			CreateTextError(Buffer[1], ErrorText);
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
		switch (ErrorByte)
		{
		case 1:
			text = "Неверный формат команды";
			break;
		case 2:
			text = "Данная команда требует другого состояния ФН";
			break;
		case 3:
			text = "Ошибка ФН 0x04 Ошибка KC 0x05 Закончен срок эксплуатации ФН";
			break;
		case 4:
			text = "Ошибка KC";
			break;
		case 5:
			text = "Закончен срок эксплуатации ФН";
			break;
		case 6:
			text = "Архив ФН переполнен";
			break;
		case 7:
			text = "Дата и время операции не соответствуют логике работы ФН";
			break;
		case 8:
			text = "Запрошенные данные отсутствуют в Архиве ФН";
			break;
		case 9:
			text = "Параметры команды имеют правильный формат, но их значение не верно";
			break;
		case 16:
			text = "Превышение размеров TLV данных";
			break;
		case 10:
			text = "В данном режиме функционирования ФН(версии ФФД) команда не разрешена";
			break;
		case 11:
			text = "Неразрешенные реквизиты(ФФД 1.2)";
			break;
		case 12:
			text = "Дублирование данных(ФФД 1.2)";
			break;
		case 13:
			text = "Отсутствуют данные, необходимые для корректного учета в ФН(ФФД 1.2)";
			break;
		case 14:
			text = "Количество позиций в документе, превысило допустимый предел(ФФД 1.2)";
			break;
		case 18:
			text = "Исчерпан ресурс КС.Требуется закрытие фискального режима";
			break;
		case 20:
			text = "Ресурс хранения документов для ОФД исчерпан";
			break;
		case 21:
			text = "Превышено время ожидания передачи сообщения(30 дней)";
			break;
		case 22:
			text = "Продолжительность смены более 24 часов";
			break;
		case 23:
			text = "Неверная разница во времени между 2 операциями(более 5 минут)";
			break;
		case 24:
			text = "Некорректный формат реквизита.Длина реквизита не соответствует формату";
			break;
		case 25:
			text = "Реквизит не соответствует установкам при регистрации";
			break;
		case 32:
			text = "Сообщение от ОФД не может быть принято";
			break;
		case 35:
			text = "Сервис обновления ключей проверки КМ уведомил об отказе в выполнении запроса";
			break;
		case 36:
			text = "Неизвестный ответ сервиса обновления ключей проверки кодов проверки";
			break;
		case 37:
			text = "Неверная структура команды, либо неверная контрольная сумма";
			break;
		case 38:
			text = "Неизвестная команда";
			break;
		case 39:
			text = "Неверная длина параметров команды";
			break;
		case 40:
			text = "Неверный формат или значение параметров команды";
			break;
		case 41:
			text = "Нарушена последовательность команд передачи данных чека, предметов расчета, итогов чека";
			break;
		case 48:
			text = "Нет связи с ФН";
			break;
		case 49:
			text = "Неверные дата/ время в ККТ";
			break;
		case 50:
			text = "Переданы не все необходимые данные";
			break;
		case 51:
			text = "РНМ сформирован неверно, проверка на данной ККТ не прошла";
			break;
		case 52:
			text = "Данные команды уже были переданы ранее";
			break;
		case 53:
			text = "Аппаратный сбой ККТ";
			break;
		case 54:
			text = "Неверно указан признак расчета, возможные значения: приход, расход, возврат прихода, возврат расхода";
			break;
		case 55:
			text = "Указанный налог не может быть применен";
			break;
		case 56:
			text = "Команда необходима только для платежного агента(указано при регистрации)";
			break;
		case 57:
			text = "Сумма расчета чека не равна сумме следующих значений по чеку: \"сумма наличными\", \"сумма электронными\", \"сумма предоплатой\", \"сумма постоплатой\", \"сумма встречным предоставлением\"";
			break;
		case 58:
			text = "Сумма оплаты соответствующими типами(за исключением наличных) превышает итог чека";
			break;
		case 59:
			text = "Некорректная разрядность итога чека";
			break;
		case 60:
			text = "Некорректная разрядность денежных величин";
			break;
		case 61:
			text = "Превышено максимально допустимое количество предметов расчета в чеке";
			break;
		case 62:
			text = "Превышено максимально допустимое количество предметов расчета c данными агента в чеке";
			break;
		case 63:
			text = "Невозможно передать данные агента, допустимы данные агента либо для всего чека, либо данные агента по предметам расчета ";
			break;
		case 64:
			text = "Некорректный статус печатающего устройства";
			break;
		case 66:
			text = "Сумма изъятия больше доступной суммы наличных в ККТа";
			break;
		case 67:
			text = "Операция внесения-изъятия денег в ККТ возможна только при открытой смене";
			break;
		case 68:
			text = "Счетчики денег не инициализированы";
			break;
		case 69:
			text = "Сумма по чеку коррекции всеми типами оплаты не равна полной сумме для расчетов по ставкам НДСа";
			break;
		case 70:
			text = "Сумма по чеку коррекции всеми типами оплаты не равна итоговой сумме чека коррекции";
			break;
		case 71:
			text = "В чеке коррекции не указано ни одной суммы для расчетов по ставкам НДС";
			break;
		case 80:
			text = "Ошибка сохранения настроек";
			break;
		case 81:
			text = "Передано некорректное значение времени";
			break;
		case 82:
			text = "В чеке не должны присутствовать иные предметы расчета помимо предмета расчета с признаком способа расчета \"Оплата кредита\"";
			break;
		case 83:
			text = "Переданы не все необходимые данные для агента";
			break;
		case 84:
			text = "Итоговая сумма расчета(в рублях без учета копеек) не равна сумме стоимости всех предметов расчета(в рублях без учета копеек)";
			break;
		case 85:
			text = "Неверно указан признак расчета для чека коррекции, возможные значения: приход, расход";
			break;
		case 86:
			text = "Неверная структура переданных данных для агента";
			break;
		case 87:
			text = "Не указан режим налогообложения";
			break;
		case 88:
			text = "Данная ставка НДС недопустима для агента. Агент не является плательщиком НДС";
			break;
		case 89:
			text = "Не указано или неверно указано значение тэга \"Признак платежного агента\"";
			break;
		case 90:
			text = "Невозможно внести товарную позицию уже после внесения данных об оплате";
			break;
		case 91:
			text = "Команда может быть выполнена только при открытом чеке";
			break;
		case 92:
			text = "Некорректный формат или длина в массиве переданных строк не фискальной информации";
			break;
		case 93:
			text = "Достигнуто максимальное количество строк не фискальной информации";
			break;
		case 94:
			text = "Не переданы данные кассира";
			break;
		case 95:
			text = "Невозможно передать параметры автоматического устройства в кассовый чек для ККТ уже зарегистрированной с этими параметрами";
			break;
		case 96:
			text = "Номер блока прошивки указан некорректно";
			break;
		case 106:
			text = "Недостаточно памяти для очередного предмета расчета";
			break;
		case 107:
			text = "Код товарной номенклатуры применим только для предмета расчета с количеством = 1";
			break;
		case 112:
			text = "Значение не зашито в ККТ";
			break;
		case 113:
			text = "Некорректное значение серийного номера";
			break;
		case 127:
			text = "Команда не выполнена";
			break;
		case 136:
			text = "Формат или значение параметров команды некорректен";
			break;
		case 144:
			text = "Некорректное значение размеров логотипа";
			break;
		case 160:
			text = "Требуется повтор процедуры обновления ключей проверки КМ";
			break;
		case 162:
			text = "Запрещена работа с маркированным товарами";
			break;
		case 163:
			text = "Неверная последовательность команд группы BxH";
			break;
		case 164:
			text = "Работа с маркированными товарами временно заблокирована. Исчерпан ресурс хранения документов для ОИСМ";
			break;
		case 165:
			text = "Переполнена таблица проверки кодов маркировки";
			break;
		case 172:
			text = "В блоке TLV отсутствуют необходимые реквизиты";
			break;
		case 174:
			text = "В реквизите 2007 содержится КМ, который ранее не проверялся в ФН";
			break;
		case 192:
			text = "Команда работает только на ККТ зарегистрированной под ФФД 1.2";
			break;
		case 193:
			text = "Данные КМ товара(тег 2000) не были переданы(ФФД 1.2)";
			break;
		case 194:
			text = "Не были переданы обязательные параметры маркированного товара(ФФД 1.2)";
			break;
		case 195:
			text = "Количество штучного маркированного товара не равно 1(ФФД 1.2)";
			break;
		case 196:
			text = "Числитель и знаменатель могут быть применены только для штучного товара(ФФД 1.2)";
			break;
		case 197:
			text = "Числитель и(или) знаменатель имеют некорректные значения(ФФД 1.2)";
			break;
		case 198:
			text = "Значение параметров отраслевого реквизита некорректно(ФФД 1.2)";
			break;
		case 199:
			text = "Количество применимо лишь для планируемых статусов(2,4) (ФФД 1.2)";
			break;
		case 200:
			text = "Команда не предназначена для ККТ в автономном режиме(ФФД 1.2)";
			break;
		case 201:
			text = "Не передан(ы) значения(адрес - место - автомат) для ККТ зарегистрированной без этих значений(ФФД 1.2)";
			break;
		case 202:
			text = "Временный код ошибки(ФФД 1.2)";
			break;
		case 203:
			text = "Ошибка передачи общих данных уведомления о реализации маркированного перед формированием чека / бсо(ФФД 1.2";
			break;
		case 204:
			text = "Ошибка связи с хостом ОИСМ(ФФД 1.2)";
			break;
		case 205:
			text = "Не найден AI - 01 GTIN(ФФД 1.2)";
			break;
		case 206:
			text = "Код товара не распознан(ФФД 1.2)";
			break;
		case 207:
			text = "Внутренняя ошибка ККТ при разборе данных кода товара(ФФД 1.2)";
			break;
		case 208:
			text = "Указанное значение распознано, но не определено как средство идентификации, выполнение команды невозможно";
			break;
		case 209:
			text = "Для указанного значения кода товара выполнение команды недоступно, необходимо использовать команду 0x96";
			break;
		case 210:
			text = "Для указанного значения кода товара выполнение команды недоступно, необходимо использовать команду 0x97";
			break;
		case 211:
			text = "Штрих - код может быть применен только к товару";
			break;
		case 212:
			text = "Некорректные данные отраслевого реквизита";
			break;
		case 213:
			text = "Превышено количество отраслевых реквизитов";
			break;
		case 223:
			text = "Маркированный товар с таким же КМ уже был внесен в формируемый кассовый чек";
			break;
		case 221:
			text = "Необходим ввод корректного кода активации";
			break;
		case 222:
			text = "Неверный формат кода активации";
			break;
		case 218:
			text = "Ошибка функционала ККТ - 0xDA";
			break;
		case 224:
			text = "Присутствуют неотправленные в ОФД документы";
			break;
		case 225:
			text = "Прошивка еще не загруженна в память";
			break;
		case 226:
			text = "Нет обновлений прошивки. Прошивка актуальная";
			break;
		case 229:
			text = "Отсутствуют неотправленные в ОФД документы";
			break;
		case 230:
			text = "Размер данных для ОФД превышает установленное значение";
			break;
		case 243:
			text = "Подключенный ФН не соответствует данным регистрации ККТ";
			break;
		case 244:
			text = "ФН еще не был активирован";
			break;
		case 245:
			text = "ФН был закрыт";
			break;
		}
		Error = TextError + " ( " + ErrorByte + "-" + text + " )";
		return true;
	}
}
