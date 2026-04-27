using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Png;

namespace KkmFactory;

public class PinterWin(Global.DeviceSettings SettDr, int NumUnit) : UnitPort(SettDr, NumUnit)
{
	private class PrintFont
	{
		public Font Font;

		public float Hight = 14f;

		public float Indent = 14f;
	}

	private int TypeConnect;

	private string Printer = "";

	private int PrintingWidth;

	private int PrinterScalePageWidht = 100;

	public new DataCommand DataCommand;

	public int CurrenPrintData;

	private PrintDocument PD;

	private PrintAction printAction;

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
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n                <Settings>\r\n                    <Page Caption=\"Параметры\">    \r\n                        <Group Caption=\"Параметры подключения\">\r\n                            <Parameter Name=\"TypeConnect\" Caption=\"Тип соединения\" TypeValue=\"String\">\r\n                                <ChoiceList>\r\n                                    <Item Value=\"3\">Принтер</Item>\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                            <Parameter Name=\"Printer\" Caption=\"Принтер\" TypeValue=\"String\" MasterParameterName=\"TypeConnect\" MasterParameterOperation=\"Equal\" MasterParameterValue=\"3\">\r\n                                <ChoiceList>\r\n                                    #ChoiceListPrn#\r\n                                </ChoiceList>\r\n                            </Parameter>\r\n                        </Group>\r\n                        <Group Caption=\"Общие параметры\">\r\n                            <Parameter Name=\"PrinterScalePageWidht\" Caption=\"Масштаб печати страницы %\" TypeValue=\"String\" DefaultValue=\"100\" /> \r\n                            <Parameter Name=\"PrintingWidth\" Caption=\"Символов в строке\" TypeValue=\"String\" DefaultValue=\"40\" /> \r\n                        </Group>\r\n                  </Page>\r\n                </Settings>";
		string text2 = "";
		try
		{
			for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
			{
				string text3 = PrinterSettings.InstalledPrinters[i];
				text2 = text2 + "<Item Value=\"" + text3 + "\">" + text3 + "</Item>";
			}
		}
		catch
		{
		}
		text = text.Replace("#ChoiceListPrn#", text2);
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
					break;
				}
				TypeConnect = 0;
				SetPort.TypeConnect = SetPorts.enTypeConnect.None;
				break;
			case "Printer":
				Printer = unitParamet.Value.Trim();
				break;
			case "PrintingWidth":
				PrintingWidth = unitParamet.Value.AsInt();
				break;
			case "PrinterScalePageWidht":
				PrinterScalePageWidht = unitParamet.Value.AsInt();
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
	public override async Task PrintDocument(DataCommand DataCommandPar, RezultCommandKKm RezultCommand)
	{
		if (DataCommandPar.NotPrint == true)
		{
			Error = "";
			RezultCommand.Status = ExecuteStatus.Ok;
			return;
		}
		CalkPrintOnPage(this, DataCommandPar);
		Error = "";
		DataCommand = DataCommandPar;
		CurrenPrintData = 0;
		await ComDevice.PostCheck(DataCommand, this);
		PD = new PrintDocument();
		PD.PrintController = new StandardPrintController();
		PrinterSettings printerSettings = new PrinterSettings();
		printerSettings.PrinterName = Printer;
		PD.PrinterSettings = printerSettings;
		PD.DocumentName = "Печать чека ККМ";
		PD.BeginPrint += PD_BeginPrint;
		PD.PrintPage += PD_PrintPage;
		PD.Print();
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

	private void PD_BeginPrint(object sender, PrintEventArgs e)
	{
		printAction = e.PrintAction;
		PD.OriginAtMargins = false;
		PD.DefaultPageSettings.Landscape = false;
	}

	private void PD_PrintPage(object sender, PrintPageEventArgs e)
	{
		float num = 0f;
		SortedList<int, PrintFont> sortedList = new SortedList<int, PrintFont>();
		sortedList.Add(0, new PrintFont());
		sortedList.Add(1, new PrintFont());
		sortedList.Add(2, new PrintFont());
		sortedList.Add(3, new PrintFont());
		sortedList.Add(4, new PrintFont());
		float num2 = 20.5f;
		float num3 = 0f;
		string text = "";
		for (int i = 0; i < PrintingWidth + 5; i++)
		{
			text += "*";
		}
		do
		{
			num2 -= 0.5f;
			sortedList[0].Font = new Font("Lucida Console", num2, FontStyle.Regular);
			SizeF sizeF = e.Graphics.MeasureString(text, sortedList[0].Font);
			num3 = sizeF.Width + 10f;
			sortedList[0].Hight = sizeF.Height * 0.83f;
			sortedList[0].Indent = 3f - sortedList[0].Hight / 5f;
		}
		while (PD.DefaultPageSettings.PrintableArea.Width * (float)PrinterScalePageWidht / 100f < (float)(int)num3);
		sortedList[1].Font = new Font("Lucida Console", num2 * 2f - 0.2f, FontStyle.Regular);
		sortedList[1].Hight = sortedList[0].Hight * 2f;
		sortedList[1].Indent = 3f - sortedList[1].Hight / 5f;
		sortedList[2].Font = new Font("Lucida Console", num2 * 1.5f - 0.2f, FontStyle.Regular);
		sortedList[2].Hight = sortedList[0].Hight * 1.5f;
		sortedList[2].Indent = 3f - sortedList[2].Hight / 5f;
		sortedList[3].Font = sortedList[0].Font;
		sortedList[3].Hight = sortedList[0].Hight;
		sortedList[3].Indent = 3f - sortedList[3].Hight / 5f;
		sortedList[4].Font = new Font("Lucida Console", num2 * 0.8f - 0.2f, FontStyle.Regular);
		sortedList[4].Hight = sortedList[0].Hight * 0.8f;
		sortedList[4].Indent = 3f - sortedList[4].Hight / 5f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (e.Graphics.VisibleClipBounds.Height / e.Graphics.VisibleClipBounds.Width < 2.5f)
		{
			flag2 = true;
		}
		for (int j = CurrenPrintData; j < DataCommand.CheckStrings.Length; j++)
		{
			if (flag2 && flag)
			{
				CurrenPrintData = j;
				e.HasMorePages = true;
				break;
			}
			float num6;
			float BarCodeWidth;
			float BarCodeHeight;
			DataCommand.PrintBarcode barCode;
			DataCommand.PrintImage printImage;
			if (flag2 && !flag3)
			{
				num = 0f;
				for (int k = j; k < DataCommand.CheckStrings.Length; k++)
				{
					if (DataCommand.CheckStrings[k] != null && DataCommand.CheckStrings[k].PrintText != null && DataCommand.CheckStrings[k].PrintText.Text != "")
					{
						string text2 = DataCommand.CheckStrings[k].PrintText.Text;
						int num4 = (int)((float)Kkm.PrintingWidth * sortedList[0].Font.SizeInPoints / sortedList[DataCommand.CheckStrings[k].PrintText.Font].Font.SizeInPoints);
						int num5 = (int)Math.Ceiling((decimal)(Unit.GetPringString(text2, num4).Length / num4));
						num += sortedList[DataCommand.CheckStrings[k].PrintText.Font].Hight * (float)num5;
					}
					num6 = e.Graphics.VisibleClipBounds.Width * (float)PrinterScalePageWidht / 100f;
					BarCodeWidth = num6 - num6 * 0.2f;
					BarCodeHeight = 0f;
					barCode = DataCommand.CheckStrings[k].BarCode;
					if (barCode != null && barCode.BarcodeType != "")
					{
						ImageBarCode imageBarCode = null;
						switch (barCode.BarcodeType.ToUpper())
						{
						case "EAN13":
							BarCodeHeight = BarCodeWidth * 0.33f;
							break;
						case "CODE39":
							BarCodeHeight = BarCodeWidth * 0.25f;
							break;
						case "CODE128":
							BarCodeHeight = BarCodeWidth * 0.25f;
							break;
						case "PDF417":
							imageBarCode = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 320, 140);
							BarCodeHeight = BarCodeWidth * ((float)imageBarCode.Height / (float)imageBarCode.Width);
							break;
						case "QR":
							imageBarCode = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 0, 0);
							BarCodeHeight = (float)imageBarCode.Width * 4f;
							if (BarCodeHeight > num6)
							{
								BarCodeHeight = (float)imageBarCode.Width * 2f;
							}
							break;
						}
						BarCode.CalculationSize(new ScaleSize(), DataCommand.CheckStrings[k].BarCode, ref BarCodeWidth, ref BarCodeHeight, num6);
						num = num + BarCodeHeight + 3f;
					}
					printImage = DataCommand.CheckStrings[k].PrintImage;
					if (printImage != null && printImage.Image != "")
					{
						byte[] array = Convert.FromBase64String(printImage.Image);
						MemoryStream memoryStream = new MemoryStream(array, 0, array.Length);
						memoryStream.Write(array, 0, array.Length);
						Image image = Image.FromStream(memoryStream, useEmbeddedColorManagement: true);
						num = num + (float)image.Height + 3f;
					}
					if (DataCommand.CheckStrings[k].EndPage)
					{
						break;
					}
				}
				num = (e.Graphics.VisibleClipBounds.Height - num) / 2f - 0.1f;
				flag3 = true;
			}
			if (DataCommand.CheckStrings[j] != null && DataCommand.CheckStrings[j].PrintText != null && DataCommand.CheckStrings[j].PrintText.Text != "")
			{
				string text3 = DataCommand.CheckStrings[j].PrintText.Text;
				int num7 = (int)((float)Kkm.PrintingWidth * sortedList[0].Font.SizeInPoints / sortedList[DataCommand.CheckStrings[j].PrintText.Font].Font.SizeInPoints);
				text3 = Unit.GetPringString(text3, num7);
				string text4 = "";
				do
				{
					if (text3.Length > num7)
					{
						text4 = text3.Substring(num7);
						text3 = text3.Substring(0, num7);
					}
					else
					{
						text4 = "";
					}
					e.Graphics.DrawString(text3, sortedList[DataCommand.CheckStrings[j].PrintText.Font].Font, Brushes.Black, new PointF(sortedList[DataCommand.CheckStrings[j].PrintText.Font].Indent, num));
					num += sortedList[DataCommand.CheckStrings[j].PrintText.Font].Hight;
					text3 = text4;
				}
				while (text4 != "");
			}
			num6 = (float)((double)e.Graphics.VisibleClipBounds.Width * 0.85) * (float)PrinterScalePageWidht / 100f;
			BarCodeWidth = num6 - num6 * 0.2f;
			BarCodeHeight = 0f;
			barCode = ((DataCommand.CheckStrings[j] != null) ? DataCommand.CheckStrings[j].BarCode : null);
			if (barCode != null && barCode.BarcodeType != "")
			{
				ImageBarCode imageBarCode2 = null;
				switch (barCode.BarcodeType.ToUpper())
				{
				case "EAN13":
					imageBarCode2 = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 240, 80);
					BarCodeHeight = BarCodeWidth * 0.33f;
					break;
				case "CODE39":
					imageBarCode2 = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 320, 80);
					BarCodeWidth = num6;
					BarCodeHeight = BarCodeWidth * 0.25f;
					break;
				case "CODE128":
					imageBarCode2 = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 320, 80);
					BarCodeWidth = num6;
					BarCodeHeight = BarCodeWidth * 0.25f;
					break;
				case "PDF417":
					imageBarCode2 = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 320, 140);
					BarCodeHeight = BarCodeWidth * ((float)imageBarCode2.Height / (float)imageBarCode2.Width);
					break;
				case "QR":
					imageBarCode2 = BarCode.GetImageBarCode(barCode.BarcodeType, barCode.Barcode, 0, 0);
					BarCodeWidth = (float)imageBarCode2.Width * 4f;
					BarCodeHeight = (float)imageBarCode2.Width * 4f;
					if (BarCodeHeight > num6)
					{
						BarCodeWidth = (float)imageBarCode2.Width * 2f;
						BarCodeHeight = (float)imageBarCode2.Width * 2f;
					}
					break;
				}
				if (imageBarCode2 != null)
				{
					BarCode.CalculationSize(new ScaleSize(), DataCommand.CheckStrings[j].BarCode, ref BarCodeWidth, ref BarCodeHeight, num6);
					RectangleF rect = new RectangleF((num6 - BarCodeWidth) / 2f + num6 * 0.025f, num, BarCodeWidth, BarCodeHeight);
					MemoryStream memoryStream2 = new MemoryStream();
					imageBarCode2.Image.Save(memoryStream2, new PngEncoder());
					Bitmap image2 = new Bitmap(memoryStream2);
					e.Graphics.DrawImage(image2, rect);
					num = num + BarCodeHeight + 3f;
				}
			}
			printImage = ((DataCommand.CheckStrings[j] != null) ? DataCommand.CheckStrings[j].PrintImage : null);
			if (printImage != null && printImage.Image != "")
			{
				byte[] array2 = Convert.FromBase64String(printImage.Image);
				MemoryStream memoryStream3 = new MemoryStream(array2, 0, array2.Length);
				memoryStream3.Write(array2, 0, array2.Length);
				Image image3 = Image.FromStream(memoryStream3, useEmbeddedColorManagement: true);
				RectangleF rect2 = new RectangleF((num6 - (float)image3.Width) / 2f + num6 * 0.025f, num, image3.Width, image3.Height);
				e.Graphics.DrawImage(image3, rect2);
				num = num + (float)image3.Height + 3f;
			}
			if (DataCommand.CheckStrings[j] != null && DataCommand.CheckStrings[j].EndPage)
			{
				flag = true;
				flag3 = false;
			}
		}
	}
}
