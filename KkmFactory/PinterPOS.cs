using System.Collections.Generic;
using System.Drawing.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PrintDirect;
using SixLabors.ImageSharp.PixelFormats;

namespace KkmFactory;

public class PinterPOS(Global.DeviceSettings SettDr, int NumUnit) : UnitPort(SettDr, NumUnit)
{
	public class DataPos
	{
		public string s = "";

		public byte[] d = new byte[0];

		public bool IsCommand;

		public DataPos(string s = "", byte[] d = null, bool IsCommand = false)
		{
			this.s = s;
			if (d != null)
			{
				this.d = d;
			}
			this.IsCommand = IsCommand;
		}
	}

	private int TypeConnect;

	private string Printer = "";

	private int CharPixhel;

	private int PrintingWidth;

	private string CodePage = "";

	private int CutEjectPaper;

	private int PrinterScalePageWidht = 100;

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = Global.Description;
		UnitEquipmentType = "ПринтерЧеков";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kkmserver.ru";
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                <Settings>\r\n                    <Page Caption=\"Параметры\">    \r\n                        <Group Caption=\"Параметры подключения\">\r\n                            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                                <ChoiceList>\r\n                                    <Item Value=\"3\">Принтер</Item>\r\n                                    <Item Value=\"2\">COM порт</Item>\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                            <Parameter Name=\"Printer\" Caption=\"Принтер\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"3\">\r\n                                <ChoiceList>\r\n                                    #ChoiceListPrn#\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                                <ChoiceList>\r\n                                    #ChoiceListCOM#\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"2\">\r\n                                <ChoiceList>\r\n                                    <Item Value=\"2400\">2400</Item>\r\n                                    <Item Value=\"4800\">4800</Item>\r\n                                    <Item Value=\"9600\">9600</Item>\r\n                                    <Item Value=\"19200\">19200</Item>\r\n                                    <Item Value=\"38400\">38400</Item>\r\n                                    <Item Value=\"57600\">57600</Item>\r\n                                    <Item Value=\"115200\">115200</Item>\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                        </Group>\r\n                        <Group Caption=\"Общие параметры\">\r\n                            <Parameter Name=\"CharPixhel\" Caption=\"Пикселей на символ\" TypeValue=\"String\" DefaultValue=\"12\" /> \r\n                            <Parameter Name=\"PrintingWidth\" Caption=\"Символов в строке\" TypeValue=\"String\" DefaultValue=\"40\" /> \r\n                            <Parameter Name=\"CodePage\" Caption=\"Кодировка символов\" TypeValue=\"String\" DefaultValue=\"866\"> \r\n                                <ChoiceList>\r\n                                    <Item Value=\"866\">cp866 (DOS)</Item>\r\n                                    <Item Value=\"1251\">cp1251 (KOI8-R)</Item>\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                            <Parameter Name=\"CutEjectPaper\" Caption=\"В конце чека\" TypeValue=\"String\" DefaultValue=\"0\"> \r\n                                <ChoiceList>\r\n                                    <Item Value=\"0\">Ничего не делать</Item>\r\n                                    <Item Value=\"1\">Отрезать чек / этикетку</Item>\r\n                                    <Item Value=\"2\">Прогнать до новой этикетки</Item>\r\n                                    <Item Value=\"3\">Отрезать по этикетке</Item>\r\n                                    <Item Value=\"4\">Прогнать до новой этикетки (для старых устройств)</Item>\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                        </Group>\r\n                  </Page>\r\n                </Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		string text3 = "";
		try
		{
			for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
			{
				string text4 = PrinterSettings.InstalledPrinters[i];
				text3 = text3 + "<Item Value=\"" + text4 + "\">" + text4 + "</Item>";
			}
		}
		catch
		{
		}
		text = text.Replace("#ChoiceListPrn#", text3);
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
				if (unitParamet.Value == "3")
				{
					TypeConnect = 3;
				}
				else if (unitParamet.Value == "2")
				{
					TypeConnect = 2;
					SetPort.TypeConnect = SetPorts.enTypeConnect.Com;
				}
				else
				{
					TypeConnect = 0;
					SetPort.TypeConnect = SetPorts.enTypeConnect.None;
				}
				break;
			case "Printer":
				Printer = unitParamet.Value.Trim();
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
			case "CharPixhel":
				CharPixhel = int.Parse(unitParamet.Value);
				break;
			case "PrintingWidth":
				PrintingWidth = int.Parse(unitParamet.Value);
				break;
			case "CodePage":
				CodePage = unitParamet.Value.Trim();
				break;
			case "CutEjectPaper":
				CutEjectPaper = int.Parse(unitParamet.Value);
				break;
			}
		}
	}

	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		Error = "";
		NameDevice = ((TypeConnect == 3) ? UnitParamets["Printer"] : SetPort.ComId) ?? "";
		Kkm.INN = "<Не определено>";
		Kkm.NumberKkm = "<Не определено>";
		Kkm.Organization = "<Не определено>";
		Kkm.PrintingWidth = PrintingWidth;
		IsInit = true;
		return true;
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task PrintDocument(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		if (DataCommand.NotPrint == true)
		{
			Error = "";
			RezultCommand.Status = ExecuteStatus.Ok;
			return;
		}
		CalkPrintOnPage(this, DataCommand);
		Error = "";
		List<DataPos> Data = new List<DataPos>();
		if (CodePage == "866")
		{
			Data.Add(new DataPos("\u001bt\u0011", null, IsCommand: true));
		}
		await ComDevice.PostCheck(DataCommand, this);
		DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
		foreach (DataCommand.CheckString checkString in checkStrings)
		{
			if (DataCommand.NotPrint == false && checkString != null && checkString.PrintText != null && DataCommand.NotPrint == false)
			{
				int num = PrintingWidth;
				if (checkString.PrintText.Font == 1 || checkString.PrintText.Font == 2)
				{
					num /= 2;
				}
				string text = checkString.PrintText.Text;
				text = Unit.GetPringString(text, num);
				string text2;
				do
				{
					if (text.Length > num)
					{
						text2 = text.Substring(num);
						text = text.Substring(0, num);
					}
					else
					{
						text2 = "";
					}
					StringBuilder stringBuilder = new StringBuilder();
					switch (checkString.PrintText.Font)
					{
					case 0:
						stringBuilder.Append("\u001b!\0");
						stringBuilder.Append(text);
						break;
					case 1:
						stringBuilder.Append("\u001b!0");
						stringBuilder.Append(text);
						stringBuilder.Append("\u001b!\0");
						break;
					case 2:
						stringBuilder.Append("\u001b! ");
						stringBuilder.Append(text);
						stringBuilder.Append("\u001b!\0");
						break;
					case 3:
						stringBuilder.Append("\u001b!\b");
						stringBuilder.Append(text);
						stringBuilder.Append("\u001b!\0");
						break;
					case 4:
						stringBuilder.Append("\u001b!\u0001");
						stringBuilder.Append(text);
						stringBuilder.Append("\u001b!\0");
						break;
					case 5:
						stringBuilder.Append("\u001b!!");
						stringBuilder.Append(text);
						stringBuilder.Append("\u001b!\0");
						break;
					case 6:
						stringBuilder.Append("\u001b!\t");
						stringBuilder.Append(text);
						stringBuilder.Append("\u001b!\0");
						break;
					}
					Data.Add(new DataPos(stringBuilder.ToString()));
					text = text2;
				}
				while (text2 != "");
			}
			if (DataCommand.NotPrint == false && checkString != null && checkString.BarCode != null && checkString.BarCode.BarcodeType != "")
			{
				PrintBarCode(checkString.BarCode, Data);
			}
			if (checkString.EndPage && CutEjectPaper != 1)
			{
				if (CutEjectPaper == 2)
				{
					Data.Add(new DataPos("\u001c(L\u0002\0C2", null, IsCommand: true));
				}
				else if (CutEjectPaper == 3)
				{
					Data.Add(new DataPos("\u001c(L\u0002\0C2", null, IsCommand: true));
				}
				else if (CutEjectPaper == 3)
				{
					Data.Add(new DataPos("\u001d\f", null, IsCommand: true));
				}
			}
		}
		if (CutEjectPaper == 1)
		{
			Data.Add(new DataPos("\u001dV\u0001", null, IsCommand: true));
		}
		else if (CutEjectPaper == 2)
		{
			Data.Add(new DataPos("\u001c(L\u0002\0C2", null, IsCommand: true));
		}
		else if (CutEjectPaper == 3)
		{
			Data.Add(new DataPos("\u001c(L\u0002\0B0", null, IsCommand: true));
			Data.Add(new DataPos("\u001dV\u0001", null, IsCommand: true));
			Data.Add(new DataPos("\u001c(L\u0002\0C2", null, IsCommand: true));
		}
		else if (CutEjectPaper == 3)
		{
			Data.Add(new DataPos("\u001d\f", null, IsCommand: true));
		}
		if (TypeConnect == 3)
		{
			using Printer printer = new Printer(Printer);
			printer.Open();
			printer.encoding = Encoding.GetEncoding(int.Parse(CodePage));
			foreach (DataPos item in Data)
			{
				if (item.s.Length != 0)
				{
					printer.Write(item.s);
				}
				if (item.d.Length != 0)
				{
					printer.Write(item.s);
				}
				if (!item.IsCommand)
				{
					printer.WriteLine();
				}
			}
			printer.Close();
		}
		else
		{
			byte[] bCRLF = new byte[2] { 13, 10 };
			Encoding encoding = Encoding.GetEncoding(int.Parse(CodePage));
			bool OpenSerial = await PortOpenAsync();
			if (Error != "")
			{
				Error = "Не удалось открыть соединение: " + Error;
				RezultCommand.Status = ExecuteStatus.Error;
				return;
			}
			try
			{
				foreach (DataPos DatePos in Data)
				{
					if (DatePos.s.Length != 0)
					{
						byte[] bytes = encoding.GetBytes(DatePos.s);
						await PortWriteAsync(bytes, 0, bytes.Length);
					}
					if (DatePos.d.Length != 0)
					{
						await PortWriteAsync(DatePos.d, 0, DatePos.d.Length);
					}
					if (!DatePos.IsCommand)
					{
						await PortWriteAsync(bCRLF, 0, bCRLF.Length);
					}
				}
			}
			finally
			{
				if (OpenSerial)
				{
					await PortCloseAsync();
				}
			}
		}
		Error = "";
		RezultCommand.Status = ExecuteStatus.Ok;
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

	public static bool TestСommunication(object Socket)
	{
		return false;
	}

	public bool PrintBarCode(DataCommand.PrintBarcode PrintBarCode, List<DataPos> Data)
	{
		int charPixhel = CharPixhel;
		float num = PrintingWidth * charPixhel * PrinterScalePageWidht / 100;
		float BarCodeWidth = num - num * 0.2f;
		float num2 = 0f;
		if (PrintBarCode != null && PrintBarCode.BarcodeType != "")
		{
			ImageBarCode imageBarCode = null;
			switch (PrintBarCode.BarcodeType.ToUpper())
			{
			case "EAN13":
				num2 = BarCodeWidth * 0.33f / 2f;
				BarCode.CalculationSize(new ScaleSize(), PrintBarCode, ref BarCodeWidth, ref num2, num);
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, (int)BarCodeWidth, (int)num2);
				break;
			case "CODE39":
				BarCodeWidth = num;
				num2 = BarCodeWidth / 4f;
				BarCode.CalculationSize(new ScaleSize(), PrintBarCode, ref BarCodeWidth, ref num2, num);
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, (int)BarCodeWidth, (int)num2);
				break;
			case "CODE128":
				BarCodeWidth = num;
				num2 = BarCodeWidth / 4f;
				BarCode.CalculationSize(new ScaleSize(), PrintBarCode, ref BarCodeWidth, ref num2, num);
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, (int)BarCodeWidth, (int)num2);
				break;
			case "PDF417":
				num2 = BarCodeWidth / 2f;
				BarCode.CalculationSize(new ScaleSize(), PrintBarCode, ref BarCodeWidth, ref num2, num);
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, (int)BarCodeWidth, (int)num2);
				num2 = BarCodeWidth * ((float)imageBarCode.Height / (float)imageBarCode.Width);
				break;
			case "QR":
				BarCodeWidth *= 0.8f;
				num2 = BarCodeWidth;
				BarCode.CalculationSize(new ScaleSize(), PrintBarCode, ref BarCodeWidth, ref num2, num);
				imageBarCode = BarCode.GetImageBarCode(PrintBarCode.BarcodeType, PrintBarCode.Barcode, (int)BarCodeWidth, (int)num2);
				break;
			}
			if (imageBarCode != null)
			{
				Data.Add(new DataPos("\u001bU\u0001\u001b3\0", null, IsCommand: true));
				string text = new string(' ', (int)((num - (float)imageBarCode.Bitmap.Width) / (float)charPixhel / 2f));
				for (int i = 0; i < imageBarCode.Bitmap.Height; i += 32)
				{
					byte[] array = new byte[imageBarCode.Bitmap.Width + 2];
					byte b = (byte)(imageBarCode.Bitmap.Width >> 8);
					array[0] = (byte)(imageBarCode.Bitmap.Width - (b << 8));
					array[1] = b;
					for (int j = 0; j < imageBarCode.Bitmap.Width; j++)
					{
						array[j + 2] = 0;
						byte b2 = 0;
						for (int k = 0; k < 32 && i + k < imageBarCode.Bitmap.Height; k += 4)
						{
							Rgba32 rgba = imageBarCode.Image[j, i + k];
							if (rgba.R + rgba.G + rgba.B < 386)
							{
								array[j + 2] += (byte)(1 << 7 - b2);
							}
							b2++;
						}
					}
					Data.Add(new DataPos(text + "\u001b*\u0001", array));
				}
				Data.Add(new DataPos("\u001bU\0\u001b2", null, IsCommand: true));
			}
		}
		return true;
	}
}
