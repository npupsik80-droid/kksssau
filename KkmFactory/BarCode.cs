using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace KkmFactory;

public static class BarCode
{
	public static ImageBarCode GetImageBarCode(string TypeBarcode, string Code, int WidthPixel, int HeightPixel)
	{
		TypeBarcode = TypeBarcode.ToUpper();
		ImageBarCode result = null;
		switch (TypeBarcode)
		{
		case "EAN13":
			result = GetImageEAN13(Code, WidthPixel, HeightPixel);
			break;
		case "CODE39":
			result = GetImageCODE39(Code, WidthPixel, HeightPixel);
			break;
		case "CODE128":
			result = GetImageCODE128(Code, WidthPixel, HeightPixel);
			break;
		case "PDF417":
			result = GetImagePDF417(Code, WidthPixel, HeightPixel);
			break;
		case "QR":
			result = GetImageCODEQR(Code, WidthPixel, HeightPixel);
			break;
		}
		return result;
	}

	public static ImageBarCode GetImageEAN13(string Code, int WidthPixel, int HeightPixel)
	{
		return GetImageBarCode(new BarcodeWriterPixelData
		{
			Format = BarcodeFormat.EAN_13,
			Options = new EncodingOptions
			{
				Height = WidthPixel / 3,
				Width = WidthPixel,
				Margin = 0
			}
		}.Write(Code), WidthPixel, HeightPixel);
	}

	public static ImageBarCode GetImageCODE39(string Code, int WidthPixel, int HeightPixel)
	{
		return GetImageBarCode(new BarcodeWriterPixelData
		{
			Format = BarcodeFormat.CODE_39,
			Options = new EncodingOptions
			{
				Height = HeightPixel,
				Width = WidthPixel,
				Margin = 0
			}
		}.Write(Code), WidthPixel, HeightPixel);
	}

	public static ImageBarCode GetImageCODE128(string Code, int WidthPixel, int HeightPixel)
	{
		return GetImageBarCode(new BarcodeWriterPixelData
		{
			Format = BarcodeFormat.CODE_128,
			Options = new EncodingOptions
			{
				Height = HeightPixel,
				Width = WidthPixel,
				Margin = 0
			}
		}.Write(Code), WidthPixel, HeightPixel);
	}

	public static ImageBarCode GetImagePDF417(string Code, int WidthPixel, int HeightPixel)
	{
		return GetImageBarCode(new BarcodeWriterPixelData
		{
			Format = BarcodeFormat.PDF_417,
			Options = new EncodingOptions
			{
				Height = HeightPixel,
				Width = WidthPixel,
				Margin = 0
			}
		}.Write(Code), WidthPixel, HeightPixel);
	}

	public static ImageBarCode GetImageCODEQR(string Code, int WidthPixel, int HeightPixel)
	{
		BarcodeWriterPixelData barcodeWriterPixelData = null;
		barcodeWriterPixelData = ((WidthPixel == 0 && HeightPixel == 0) ? new BarcodeWriterPixelData
		{
			Format = BarcodeFormat.QR_CODE,
			Options = new EncodingOptions
			{
				Margin = 0
			}
		} : new BarcodeWriterPixelData
		{
			Format = BarcodeFormat.QR_CODE,
			Options = new EncodingOptions
			{
				Height = HeightPixel,
				Width = WidthPixel,
				Margin = 0
			}
		});
		return GetImageBarCode(barcodeWriterPixelData.Write(Code), WidthPixel, HeightPixel);
	}

	public static ImageBarCode GetImageBarCode(PixelData PixelData, int WidthPixel, int HeightPixel)
	{
		ImageBarCode imageBarCode = new ImageBarCode();
		imageBarCode.Width = PixelData.Width;
		imageBarCode.Height = PixelData.Height;
		Image<Rgba32> image = (imageBarCode.Image = Image.LoadPixelData<Rgba32>(PixelData.Pixels, PixelData.Width, PixelData.Height));
		imageBarCode.Bitmap = PixelData;
		imageBarCode.PngBase64 = ImageToPngBase64(image);
		int num = imageBarCode.Width * imageBarCode.Height / 8;
		if (num * 8 < imageBarCode.Width * imageBarCode.Height)
		{
			num++;
		}
		imageBarCode.Pixels = new byte[num];
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < imageBarCode.Height; i++)
		{
			for (int j = 0; j < imageBarCode.Width; j++)
			{
				Rgba32 rgba = image[j, i];
				if (rgba.R + rgba.G + rgba.B < 386)
				{
					int num4 = 1 << num3;
					imageBarCode.Pixels[num2] = (byte)(imageBarCode.Pixels[num2] + num4);
				}
				num3++;
				if (num3 > 7)
				{
					num3 = 0;
					num2++;
				}
			}
		}
		return imageBarCode;
	}

	public static string ImageToPngBase64(Image Image)
	{
		MemoryStream memoryStream = new MemoryStream();
		Image.Save(memoryStream, new PngEncoder());
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static Image<Rgba32> ImageFromBase64(string Base64String)
	{
		byte[] array = Convert.FromBase64String(Base64String);
		Image<Rgba32> image = null;
		if (image == null)
		{
			try
			{
				image = Image.Load<Rgba32>(array);
			}
			catch
			{
			}
		}
		return image;
	}

	public static float GetBrightness(Rgba32 Color)
	{
		float num = (float)(int)Color.R / 255f;
		float num2 = (float)(int)Color.G / 255f;
		float num3 = (float)(int)Color.B / 255f;
		float num4 = num;
		float num5 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		if (num2 < num5)
		{
			num5 = num2;
		}
		if (num3 < num5)
		{
			num5 = num3;
		}
		return (num4 + num5) / 2f;
	}

	public static void CalculationSize(ScaleSize ScaleSize, Unit.DataCommand.PrintBarcode BarCode, ref float BarCodeWidth, ref float BarCodeHeight, float VisibleWidth)
	{
		string text = BarCode.BarcodeType.ToUpper();
		float num = 0f;
		float num2 = 0f;
		switch (text)
		{
		case "EAN13":
			num = ScaleSize.EAN13_ScaleWidth;
			num2 = ScaleSize.EAN13_ScaleHeight;
			break;
		case "CODE39":
			num = ScaleSize.CODE39_ScaleWidth;
			num2 = ScaleSize.CODE39_ScaleHeight;
			break;
		case "CODE128":
			num = ScaleSize.CODE128_ScaleWidth;
			num2 = ScaleSize.CODE128_ScaleHeight;
			break;
		case "PDF417":
			num = ScaleSize.PDF417_ScaleWidth;
			num2 = (float)ScaleSize.PDF417_ScaleHeight - 50f;
			break;
		case "QR":
			num = ScaleSize.CODEQR_ScaleWidth;
			num2 = ScaleSize.CODEQR_ScaleHeight;
			break;
		}
		BarCodeWidth += BarCodeWidth / 100f * num;
		if (BarCodeWidth > VisibleWidth)
		{
			BarCodeWidth = VisibleWidth;
		}
		BarCodeHeight += BarCodeHeight / 100f * num2;
	}
}
