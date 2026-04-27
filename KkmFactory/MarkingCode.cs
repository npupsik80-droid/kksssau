using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace KkmFactory;

public static class MarkingCode
{
	public class IdentifiersGS1
	{
		public string GTIN = "01";

		public string SerialNumber = "21";

		public string KeyCode = "91";

		public string CheckCode = "92";

		public string CodeTNVD = "240";
	}

	public enum TypeBarcode
	{
		NotParse,
		EAN8,
		EAN13,
		ITF14,
		GS1,
		FurBarCode,
		EGAIS2,
		EGAIS3
	}

	public class DataProductCode
	{
		private string _DescriptionErrors = "";

		[DataMember(Name = "КktValidationResult")]
		public uint КktValidationResult;

		[DataMember(Name = "КktDecryptionResult")]
		public string КktDecryptionResult = "";

		[DataMember]
		public string BarCode = "";

		[DataMember]
		public bool isParsed;

		[DataMember]
		public string RepresentationBarCode = "";

		[DataMember]
		public string TryBarCode = "";

		public string TryBarCodeWithRazdelitel = "";

		[IgnoreDataMember]
		public TypeBarcode TypeBarcode;

		[DataMember]
		public string EAN = "";

		[DataMember]
		public string GTIN = "";

		[DataMember]
		public string SerialNumber = "";

		[DataMember]
		public bool ContainsSerialNumber;

		[DataMember]
		public string MarkingCodeBase64 = "";

		[IgnoreDataMember]
		public byte[] MarkingCodeHEX = new byte[0];

		[DataMember]
		public Dictionary<string, string> DataBarCode = new Dictionary<string, string>();

		[DataMember]
		public string ProductCodeType => TypeBarcode switch
		{
			TypeBarcode.NotParse => "NotParse", 
			TypeBarcode.EAN8 => "EAN8", 
			TypeBarcode.EAN13 => "EAN13", 
			TypeBarcode.ITF14 => "ITF14", 
			TypeBarcode.GS1 => "GS1", 
			TypeBarcode.FurBarCode => "Fur", 
			TypeBarcode.EGAIS2 => "EGAIS2", 
			TypeBarcode.EGAIS3 => "EGAIS3", 
			_ => "NotParse", 
		};

		[DataMember]
		public string Errors
		{
			get
			{
				return _DescriptionErrors;
			}
			set
			{
				if (_DescriptionErrors == "")
				{
					_DescriptionErrors = value;
				}
			}
		}

		public void ClearDescriptionErrors()
		{
			_DescriptionErrors = "";
		}
	}

	private class DescriptionFieldGS1
	{
		public string Code;

		public string Name;

		public int FixedLength;

		public string TypeFixedValue;

		public int VariableLength;

		public string TypeVariableValue;

		public bool IsSeparator;

		public bool IsDecimalPointPosition;
	}

	private const string TypeGS1Digit = "N";

	private const string TypeGS1String = "X";

	private const string SeparatorGS1 = "\u001d";

	private const string EscapedCharGS1 = "\\x1d";

	public static DataProductCode DeleteEnd29Code(DataProductCode ProductCode)
	{
		string text = ProductCode.TryBarCode;
		if (text.Substring(text.Length - 1, 1) == "\u001d")
		{
			text = text.Substring(0, text.Length - 1);
		}
		ProductCode.TryBarCode = text;
		return ProductCode;
	}

	public static DataProductCode ParseBarCode(string ШтриховойКодТовара)
	{
		IdentifiersGS1 identifiersGS = new IdentifiersGS1();
		DataProductCode dataProductCode = FillDataProductCode();
		dataProductCode.BarCode = ШтриховойКодТовара;
		dataProductCode.TryBarCode = ШтриховойКодТовара;
		int length = ШтриховойКодТовара.Length;
		dataProductCode.TypeBarcode = TypeBarcode.NotParse;
		switch (length)
		{
		case 0:
			return DeleteEnd29Code(dataProductCode);
		case 8:
			if (OnlyNumbersInString(ШтриховойКодТовара) && CalculateCheckCharGTIN8(ШтриховойКодТовара) == ШтриховойКодТовара[ШтриховойКодТовара.Length - 1])
			{
				dataProductCode.TypeBarcode = TypeBarcode.EAN8;
				GenerateEAN(dataProductCode, ШтриховойКодТовара);
				GenerateBinaryDataForNumbers(dataProductCode, ШтриховойКодТовара);
				dataProductCode.isParsed = true;
				return DeleteEnd29Code(dataProductCode);
			}
			break;
		}
		if (length == 13 && OnlyNumbersInString(ШтриховойКодТовара) && CalculateCheckCharGTIN13(ШтриховойКодТовара) == ШтриховойКодТовара[ШтриховойКодТовара.Length - 1])
		{
			dataProductCode.TypeBarcode = TypeBarcode.EAN13;
			GenerateEAN(dataProductCode, ШтриховойКодТовара);
			GenerateBinaryDataForNumbers(dataProductCode, ШтриховойКодТовара);
			dataProductCode.isParsed = true;
			return DeleteEnd29Code(dataProductCode);
		}
		if (length == 14 && OnlyNumbersInString(ШтриховойКодТовара) && CalculateCheckCharGTIN14(ШтриховойКодТовара) == ШтриховойКодТовара[ШтриховойКодТовара.Length - 1])
		{
			dataProductCode.TypeBarcode = TypeBarcode.ITF14;
			GenerateEAN(dataProductCode, ШтриховойКодТовара);
			GenerateBinaryDataForNumbers(dataProductCode, ШтриховойКодТовара);
			dataProductCode.isParsed = true;
			return DeleteEnd29Code(dataProductCode);
		}
		if (length == 20 && ШтриховойКодТовара.Substring(2, 1) == "-" && ШтриховойКодТовара.Substring(9, 1) == "-")
		{
			string text = ШтриховойКодТовара.Substring(0, 2);
			string text2 = ШтриховойКодТовара.Substring(3, 6);
			string text3 = ШтриховойКодТовара.Substring(10, 10);
			if (StringValidation(text, ДопустимыЛатПрописные: true) && OnlyNumbersInString(text2) && StringValidation(text3, ДопустимыЛатПрописные: true, ДопустимыЛатСтрочные: false, ДопустимыЦифры: true))
			{
				dataProductCode.TypeBarcode = TypeBarcode.FurBarCode;
				GenerateBinaryDataForStrings(dataProductCode, ШтриховойКодТовара);
				dataProductCode.isParsed = true;
				dataProductCode.ContainsSerialNumber = true;
				dataProductCode.DataBarCode.Add("422", text + " ");
				dataProductCode.DataBarCode.Add("240", text2);
				dataProductCode.DataBarCode.Add("21", text3);
				dataProductCode.SerialNumber = text3;
				dataProductCode.RepresentationBarCode = "";
				foreach (KeyValuePair<string, string> item in dataProductCode.DataBarCode)
				{
					dataProductCode.RepresentationBarCode = dataProductCode.RepresentationBarCode + "(" + item.Key + ")" + item.Value;
				}
				return DeleteEnd29Code(dataProductCode);
			}
		}
		if ((length == 29 || length == 25) && StringValidation(ШтриховойКодТовара, ДопустимыЛатПрописные: true, ДопустимыЛатСтрочные: true, ДопустимыЦифры: true, "«!”\\\"%&’'()*+-.,/_:;=<>?»"))
		{
			string text4 = ШтриховойКодТовара.Substring(0, 14);
			string text5 = ШтриховойКодТовара.Substring(14, 7);
			string text6 = ШтриховойКодТовара.Substring(21, 4);
			if (CalculateCheckCharGTIN14(text4) == text4[text4.Length - 1])
			{
				dataProductCode.TypeBarcode = TypeBarcode.GS1;
				dataProductCode.SerialNumber = text5;
				GenerateEAN(dataProductCode, text4);
				string text7 = text5 + text6;
				while (text7.Length < 13)
				{
					text7 += Convert.ToChar(32);
				}
				GenerateBinaryDataForNumbers(dataProductCode, text4, text7);
				dataProductCode.isParsed = true;
				dataProductCode.ContainsSerialNumber = true;
				dataProductCode.DataBarCode.Add("10", text4);
				dataProductCode.DataBarCode.Add("92", text6);
				dataProductCode.DataBarCode.Add("21", text5);
				dataProductCode.RepresentationBarCode = "";
				foreach (KeyValuePair<string, string> item2 in dataProductCode.DataBarCode)
				{
					dataProductCode.RepresentationBarCode = dataProductCode.RepresentationBarCode + "(" + item2.Key + ")" + item2.Value;
				}
				return DeleteEnd29Code(dataProductCode);
			}
		}
		Dictionary<string, DescriptionFieldGS1> fieldsGS = GetFieldsGS1(1);
		string text8 = ШтриховойКодТовара;
		if (text8.IndexOf("]d2") != -1)
		{
			text8 = text8.Substring(3);
		}
		if (text8[0] == '(')
		{
			ParseStringBarcodeGS1WithBrackets(text8, dataProductCode, fieldsGS);
		}
		if (!dataProductCode.isParsed)
		{
			string[] array = "\u001d,\\\\x1d,\\\\x001d,\\\\u001d,\\x001d,\\u001d,\\x1d,<FNC1>,<GS> ,<GS>".Split(',');
			string errors = dataProductCode.Errors;
			bool flag = false;
			for (int i = 0; i <= 2; i++)
			{
				string[] array2 = array;
				foreach (string text9 in array2)
				{
					if (errors == "" && dataProductCode.Errors != "")
					{
						errors = dataProductCode.Errors;
					}
					dataProductCode.ClearDescriptionErrors();
					dataProductCode.DataBarCode.Clear();
					Dictionary<string, DescriptionFieldGS1> dictionary = null;
					dictionary = GetFieldsGS1(i);
					if (ШтриховойКодТовара.IndexOf(text9) != -1)
					{
						string[] array3 = text8.Split(new string[1] { text9 }, StringSplitOptions.None);
						for (int k = 0; k < array3.Length; k++)
						{
							ParseStringBarcodeGS1Service(array3[k], dataProductCode, dictionary);
						}
						if (dataProductCode.isParsed)
						{
							flag = true;
							break;
						}
						dataProductCode.Errors = errors;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!dataProductCode.isParsed)
		{
			for (int l = 0; l <= 2; l++)
			{
				dataProductCode.ClearDescriptionErrors();
				dataProductCode.DataBarCode.Clear();
				Dictionary<string, DescriptionFieldGS1> dictionary2 = null;
				dictionary2 = GetFieldsGS1(l, IpAllFix: true);
				ParseStringBarcodeGS1Service(text8, dataProductCode, dictionary2);
				if (dataProductCode.isParsed)
				{
					break;
				}
			}
		}
		if (dataProductCode.isParsed)
		{
			string value = "";
			if (!dataProductCode.DataBarCode.TryGetValue(identifiersGS.GTIN, out value))
			{
				value = null;
			}
			if (value != null && CalculateCheckCharGTIN14(value) == value[value.Length - 1])
			{
				string value2 = "";
				if (dataProductCode.DataBarCode.TryGetValue(identifiersGS.SerialNumber, out value2))
				{
					dataProductCode.TypeBarcode = TypeBarcode.GS1;
					dataProductCode.SerialNumber = value2;
					GenerateEAN(dataProductCode, value);
					GenerateBinaryDataForNumbers(dataProductCode, value, dataProductCode.SerialNumber);
					dataProductCode.isParsed = true;
					dataProductCode.ContainsSerialNumber = true;
					dataProductCode.RepresentationBarCode = "";
					foreach (KeyValuePair<string, string> item3 in dataProductCode.DataBarCode)
					{
						dataProductCode.RepresentationBarCode = dataProductCode.RepresentationBarCode + "(" + item3.Key + ")" + item3.Value;
					}
					dataProductCode.TryBarCode = GenerateTryBarCode(dataProductCode);
					return DeleteEnd29Code(dataProductCode);
				}
			}
		}
		if (Substring(ШтриховойКодТовара, 0, 2) == identifiersGS.GTIN)
		{
			string text10 = Substring(ШтриховойКодТовара, 2, 14);
			if (CalculateCheckCharGTIN14(text10) == text10[text10.Length - 1] && Substring(ШтриховойКодТовара, 16, 2) == identifiersGS.SerialNumber)
			{
				string text11 = Substring(ШтриховойКодТовара, 18, 10000);
				if (text11.Length > 13)
				{
					string serialNumber = Substring(text11, 0, 13);
					string text12 = Substring(text11, 13, 2);
					string text13 = Substring(text11, 13, 3);
					string text14 = Substring(text11, 13, 4);
					if (text12 == identifiersGS.KeyCode || text12 == identifiersGS.CheckCode || text12 == "17" || text14 == "7003" || text13 == identifiersGS.CodeTNVD)
					{
						dataProductCode.TypeBarcode = TypeBarcode.GS1;
						dataProductCode.SerialNumber = serialNumber;
						GenerateEAN(dataProductCode, text10);
						GenerateBinaryDataForNumbers(dataProductCode, text10, dataProductCode.SerialNumber);
						dataProductCode.isParsed = true;
						dataProductCode.ContainsSerialNumber = true;
						dataProductCode.TryBarCode = GenerateTryBarCode(dataProductCode);
						return DeleteEnd29Code(dataProductCode);
					}
				}
			}
		}
		switch (length)
		{
		case 68:
			if (StringValidation(ШтриховойКодТовара, ДопустимыЛатПрописные: true, ДопустимыЛатСтрочные: false, ДопустимыЦифры: true))
			{
				dataProductCode.ClearDescriptionErrors();
				dataProductCode.DataBarCode.Clear();
				dataProductCode.TypeBarcode = TypeBarcode.EGAIS2;
				string значениеСтроки2 = Substring(ШтриховойКодТовара, 8, 23);
				GenerateBinaryDataForStrings(dataProductCode, значениеСтроки2);
				dataProductCode.isParsed = true;
				dataProductCode.ContainsSerialNumber = true;
				return DeleteEnd29Code(dataProductCode);
			}
			break;
		case 150:
			if (StringValidation(ШтриховойКодТовара, ДопустимыЛатПрописные: true, ДопустимыЛатСтрочные: false, ДопустимыЦифры: true))
			{
				dataProductCode.ClearDescriptionErrors();
				dataProductCode.DataBarCode.Clear();
				dataProductCode.TypeBarcode = TypeBarcode.EGAIS3;
				string значениеСтроки = Substring(ШтриховойКодТовара, 0, 14);
				GenerateBinaryDataForStrings(dataProductCode, значениеСтроки);
				dataProductCode.isParsed = true;
				dataProductCode.ContainsSerialNumber = true;
				return DeleteEnd29Code(dataProductCode);
			}
			break;
		}
		GenerateBinaryDataForStrings(dataProductCode, Substring(ШтриховойКодТовара, 0, 30));
		return DeleteEnd29Code(dataProductCode);
	}

	public static void TestParseBarCode()
	{
		ParseBarCode("46120441");
		ParseBarCode("46120441");
		ParseBarCode("2900001462105");
		ParseBarCode("02900001462105");
		ParseBarCode("(01)12345678901231(253)1234567890123(8003)1234567890123456(10)12345678901234567890(21)12345678");
		ParseBarCode("0104300943734342212413195240818\u001d2406402\u001d91ffd0\u001d92MDEwNDMwMDk0MzczNDM");
		ParseBarCode("010460043993125621JgXJ5.T\\u001d8005112000\\u001d930001\\u001d923zbrLA ==\\u001d24014276281");
		ParseBarCode("010460406000600021N4N57RSCBUZTQ\\x1d2403004002910161218\\x1d1724010191ffd0\\x1d92tIAF/YVoU4roQS3M/m4");
		ParseBarCode("010460406000600021N4N57RSCBUZTQ\\x001d2403004002910161218\\x001d1724010191ffd0\\x001d92tIAF/YVoU4roQS3M/m4");
		ParseBarCode("<FNC1>0108691234567890211323424679<FNC1>1707011910AX785910BC");
		ParseBarCode("0183525492885520210000000859314<GS> 91ee05<GS> 92r7fLjLdSQBRRL8KgReiJ0mgdFWhlR9gsfe1QS3ibhck=");
		ParseBarCode("0183525492885520210000000859314<GS>91ee05<GS>92r7fLjLdSQBRRL8KgReiJ0mgdFWhlR9gsfe1QS3ibhck=");
		ParseBarCode("010460620309891021MCEb6/r890123800511700093EBBm\u001d240FA068592.14");
		ParseBarCode("RU-430301-ABCDEF1234");
		ParseBarCode("00000046186195Xp4k=xyAQDPtFEa");
		ParseBarCode("00000046186195Xp4k=xyAQDP");
		ParseBarCode("22N00002NU5DBKYDOT17ID980726019019608CW1A4XR5EJ7JKFX50FHHGV92ZR2GZRZ");
		ParseBarCode("136222000058810918QWERDFEWT5123456YGHFDSWERT56YUIJHGFDSAERTYUIOKJ8HGFVCXZSDLKJHGFDSAOIPLMNBGHJYTRDFGHJKIREWSDFGHJIOIUTDWQASDFRETYUIUYGTREDFGHUYTREWQWE");
	}

	private static Dictionary<string, DescriptionFieldGS1> GetFieldsGS1(int Fix21 = 0, bool IpAllFix = false)
	{
		Dictionary<string, DescriptionFieldGS1> dictionary = new Dictionary<string, DescriptionFieldGS1>();
		AddFieldGS1(dictionary, "00", "SSCC", 18);
		AddFieldGS1(dictionary, "01", "GTIN", 14);
		AddFieldGS1(dictionary, "02", "CONTENT", 14);
		AddFieldGS1(dictionary, "10", "BATCH_LOT", 0, 20, null, "X");
		AddFieldGS1(dictionary, "11", "PROD_DATE", 6);
		AddFieldGS1(dictionary, "12", "DUE_DATE", 6);
		AddFieldGS1(dictionary, "13", "PACK_DATE", 6);
		AddFieldGS1(dictionary, "15", "BEST_BEFORE", 6);
		AddFieldGS1(dictionary, "16", "SELL_BY", 6);
		AddFieldGS1(dictionary, "17", "EXPIRE", 6);
		AddFieldGS1(dictionary, "20", "VARIANT", 2);
		if (Fix21 == 0 && !IpAllFix)
		{
			AddFieldGS1(dictionary, "21", "SERIAL", 0, 20, null, "X");
		}
		else if (Fix21 == 0 && IpAllFix)
		{
			AddFieldGS1(dictionary, "21", "SERIAL", 20, 0, "X");
		}
		else if (Fix21 == 1)
		{
			AddFieldGS1(dictionary, "21", "SERIAL", 13, 0, "X");
		}
		else if (Fix21 == 2 && !IpAllFix)
		{
			AddFieldGS1(dictionary, "21", "SERIAL", 20, 0, "X");
		}
		else if (Fix21 == 2 && IpAllFix)
		{
			AddFieldGS1(dictionary, "21", "SERIAL", 0, 20, null, "X");
		}
		AddFieldGS1(dictionary, "22", "CPV", 0, 20, null, "X");
		AddFieldGS1(dictionary, "240", "ADDITIONAL_ID", 0, 30, null, "X");
		AddFieldGS1(dictionary, "241", "CUSTOMER_PART_NO", 0, 30, null, "X");
		AddFieldGS1(dictionary, "242", "MTO_VARIANT", 0, 6);
		AddFieldGS1(dictionary, "243", "PCN", 0, 20, null, "X");
		AddFieldGS1(dictionary, "250", "SECONDARY_SERIAL", 0, 30, null, "X");
		AddFieldGS1(dictionary, "251", "REF_TO_SOURCE", 0, 30, null, "X");
		AddFieldGS1(dictionary, "253", "GDTI", 13, 17, "N", "X");
		AddFieldGS1(dictionary, "254", "GLN_EXTENSION_COMPONENT", 0, 20, null, "X");
		AddFieldGS1(dictionary, "255", "GСТ", 13, 12);
		AddFieldGS1(dictionary, "30", "VAR_COUNT", 0, 8);
		AddFieldGS1(dictionary, "310n", "NET_WEIGHT_kg", 6);
		AddFieldGS1(dictionary, "311n", "LENGTH_m", 6);
		AddFieldGS1(dictionary, "312n", "WIDTH_m", 6);
		AddFieldGS1(dictionary, "313n", "HEIGHT_m", 6);
		AddFieldGS1(dictionary, "314n", "AREA_m2", 6);
		AddFieldGS1(dictionary, "315n", "NET_VOLUME_l", 6);
		AddFieldGS1(dictionary, "316n", "NET_VOLUME_m3", 6);
		AddFieldGS1(dictionary, "320n", "NET_WEIGHT_lb", 6);
		AddFieldGS1(dictionary, "321n", "LENGTH_i", 6);
		AddFieldGS1(dictionary, "322n", "LENGTH_f", 6);
		AddFieldGS1(dictionary, "323n", "LENGTH_y", 6);
		AddFieldGS1(dictionary, "324n", "WIDTH_i", 6);
		AddFieldGS1(dictionary, "325n", "WIDTH_f", 6);
		AddFieldGS1(dictionary, "326n", "WIDTH_y", 6);
		AddFieldGS1(dictionary, "327n", "HEIGHT_i", 6);
		AddFieldGS1(dictionary, "328n", "HEIGHT_f", 6);
		AddFieldGS1(dictionary, "329n", "HEIGHT_y", 6);
		AddFieldGS1(dictionary, "330n", "GROSS_WEIGHT_kg", 6);
		AddFieldGS1(dictionary, "331n", "LENGTH_m_log", 6);
		AddFieldGS1(dictionary, "332n", "WIDTH_m_log", 6);
		AddFieldGS1(dictionary, "333n", "HEIGHT_m_log", 6);
		AddFieldGS1(dictionary, "334n", "AREA_m2_log", 6);
		AddFieldGS1(dictionary, "335n", "VOLUME_l_log", 6);
		AddFieldGS1(dictionary, "336n", "VOLUME_m3_log", 6);
		AddFieldGS1(dictionary, "337n", "KG_PER_m2", 6);
		AddFieldGS1(dictionary, "340n", "GROSS_WEIGHT_lb", 6);
		AddFieldGS1(dictionary, "341n", "LENGTH_i_log", 6);
		AddFieldGS1(dictionary, "342n", "LENGTH_f_log", 6);
		AddFieldGS1(dictionary, "343n", "LENGTH_y_log", 6);
		AddFieldGS1(dictionary, "344n", "WIDTH_i_log", 6);
		AddFieldGS1(dictionary, "345n", "WIDTH_f_log", 6);
		AddFieldGS1(dictionary, "346n", "WIDTH_y_log", 6);
		AddFieldGS1(dictionary, "347n", "HEIGHT_i_log", 6);
		AddFieldGS1(dictionary, "348n", "HEIGHT_f_log", 6);
		AddFieldGS1(dictionary, "349n", "HEIGHT_y_log", 6);
		AddFieldGS1(dictionary, "350n", "AREA_i2", 6);
		AddFieldGS1(dictionary, "351n", "AREA_f2", 6);
		AddFieldGS1(dictionary, "352n", "AREA_y2", 6);
		AddFieldGS1(dictionary, "353n", "AREA_i2_log", 6);
		AddFieldGS1(dictionary, "354n", "AREA_f2_log", 6);
		AddFieldGS1(dictionary, "355n", "AREA_y2_log", 6);
		AddFieldGS1(dictionary, "356n", "NET_WEIGHT_t", 6);
		AddFieldGS1(dictionary, "357n", "NET_VOLUME_oz", 6);
		AddFieldGS1(dictionary, "360n", "NET_VOLUME_q", 6);
		AddFieldGS1(dictionary, "361n", "NET_VOLUME_g", 6);
		AddFieldGS1(dictionary, "362n", "VOLUME_q", 6);
		AddFieldGS1(dictionary, "363n", "VOLUME_g", 6);
		AddFieldGS1(dictionary, "364n", "VOLUME_i3", 6);
		AddFieldGS1(dictionary, "365n", "VOLUME_f3", 6);
		AddFieldGS1(dictionary, "366n", "VOLUME_y3", 6);
		AddFieldGS1(dictionary, "367n", "VOLUME_i3_log", 6);
		AddFieldGS1(dictionary, "368n", "VOLUME_f3_log", 6);
		AddFieldGS1(dictionary, "369n", "VOLUME_y3_log", 6);
		AddFieldGS1(dictionary, "37", "COUNT", 0, 8);
		AddFieldGS1(dictionary, "390n", "AMOUNT", 0, 15);
		AddFieldGS1(dictionary, "391n", "AMOUNT_ISO", 3, 15);
		AddFieldGS1(dictionary, "392n", "PRICE", 0, 15);
		AddFieldGS1(dictionary, "393n", "PRICE_ISO", 3, 15);
		AddFieldGS1(dictionary, "394n", "PRCNT_OFF", 4, 0, null, null, true);
		AddFieldGS1(dictionary, "400", "ORDER_NUMBER", 0, 30, null, "X");
		AddFieldGS1(dictionary, "401", "GINC", 0, 30, null, "X");
		AddFieldGS1(dictionary, "402", "GSIN", 17, 0, null, null, true);
		AddFieldGS1(dictionary, "403", "ROUTE", 0, 30, null, "X");
		AddFieldGS1(dictionary, "410", "SHIP_TO_LOC", 13);
		AddFieldGS1(dictionary, "411", "BILL_TO", 13);
		AddFieldGS1(dictionary, "412", "PURCHASE_FROM", 13);
		AddFieldGS1(dictionary, "413", "SHIP_FOR_LOC", 13);
		AddFieldGS1(dictionary, "414", "LOC_No", 13);
		AddFieldGS1(dictionary, "415", "PAY_TO", 13);
		AddFieldGS1(dictionary, "416", "PROD_SERV_LOC", 13);
		AddFieldGS1(dictionary, "420", "SHIP_TO_POST", 0, 20, null, "X");
		AddFieldGS1(dictionary, "421", "SHIP_TO_POST_ISO", 3, 9, null, "X");
		AddFieldGS1(dictionary, "422", "ORIGIN", 3, 0, null, null, true);
		AddFieldGS1(dictionary, "423", "CONTRY_INITIAL_PROCESS", 3, 12);
		AddFieldGS1(dictionary, "424", "CONTRY_PROCESS", 3, 0, null, null, true);
		AddFieldGS1(dictionary, "425", "CONTRY_DISASSEMBLY", 3, 12);
		AddFieldGS1(dictionary, "426", "CONTRY_FULL_PROCESS", 3, 0, null, null, true);
		AddFieldGS1(dictionary, "427", "ORIGIN_SUBDIVISION", 0, 3, null, "X");
		AddFieldGS1(dictionary, "7001", "NSN", 13, 0, null, null, true);
		AddFieldGS1(dictionary, "7002", "MEAT_CUT", 0, 30, null, "X");
		AddFieldGS1(dictionary, "7003", "EXPIRY_TIME", 10, 0, null, null, true);
		AddFieldGS1(dictionary, "7004", "ACTIVE_POTENCY", 0, 4);
		AddFieldGS1(dictionary, "7005", "CATCH_AREA", 0, 12, null, "X");
		AddFieldGS1(dictionary, "7006", "FIRST_FREEZE_DATE", 6, 0, null, null, true);
		AddFieldGS1(dictionary, "7007", "HARVEST_DATE", 6, 6);
		AddFieldGS1(dictionary, "7008", "AQUATIC_SPECIES", 0, 3, null, "X");
		AddFieldGS1(dictionary, "7009", "FISHING_GEAR_TYPE", 0, 10, null, "X");
		AddFieldGS1(dictionary, "7010", "PROD_METHOD", 0, 2, null, "X");
		AddFieldGS1(dictionary, "7020", "REFURB_LOT", 0, 20, null, "X");
		AddFieldGS1(dictionary, "7021", "FUNC_STAT", 0, 20, null, "X");
		AddFieldGS1(dictionary, "7022", "REV_STAT", 0, 20, null, "X");
		AddFieldGS1(dictionary, "7023", "GIAI_ASSEMBLY", 0, 30, null, "X");
		AddFieldGS1(dictionary, "703s", "PROCESSOR_s", 3, 27, "N", "X");
		AddFieldGS1(dictionary, "710", "NHRN_PZN", 0, 20, null, "X");
		AddFieldGS1(dictionary, "711", "NHRN_CIP", 0, 20, null, "X");
		AddFieldGS1(dictionary, "712", "NHRN_CN", 0, 20, null, "X");
		AddFieldGS1(dictionary, "713", "NHRN_DRN", 0, 20, null, "X");
		AddFieldGS1(dictionary, "8001", "DIMENSIONS", 14, 0, null, null, true);
		AddFieldGS1(dictionary, "8002", "CMT_No", 0, 20, null, "X");
		AddFieldGS1(dictionary, "8003", "GRAI", 14, 16, "N", "X");
		AddFieldGS1(dictionary, "8004", "GIAI", 0, 30, null, "X");
		AddFieldGS1(dictionary, "8005", "PRICE_PER_UNIT", 6, 0, null, null, true);
		AddFieldGS1(dictionary, "8006", "ITIP_or_GCTIN", 18, 0, null, null, true);
		AddFieldGS1(dictionary, "8007", "IBAN", 0, 34, null, "X");
		AddFieldGS1(dictionary, "8008", "PROD_TIME", 8, 4, null, null, true);
		AddFieldGS1(dictionary, "8010", "CPID", 0, 30, null, "X");
		AddFieldGS1(dictionary, "8011", "CPID_SERIAL", 0, 12);
		AddFieldGS1(dictionary, "8012", "VERSION", 0, 20, null, "X");
		AddFieldGS1(dictionary, "8017", "GSRN_PROVIDER", 18, 0, null, null, true);
		AddFieldGS1(dictionary, "8018", "GSRN_RECIPIENT", 18, 0, null, null, true);
		AddFieldGS1(dictionary, "8019", "SRIN", 0, 10);
		AddFieldGS1(dictionary, "8020", "REF_No", 0, 25, null, "X");
		AddFieldGS1(dictionary, "8110", "COUPON_CODE_ID", 0, 70, null, "X");
		AddFieldGS1(dictionary, "8111", "POINTS", 4, 0, null, null, true);
		AddFieldGS1(dictionary, "8112", "PAPPERLESS_COUPON_CODE_ID", 0, 70, null, "X");
		AddFieldGS1(dictionary, "8200", "PRODUCT_URL", 0, 70, null, "X");
		AddFieldGS1(dictionary, "90", "INTERNAL", 0, 30, null, "X");
		if (Fix21 == 0)
		{
			AddFieldGS1(dictionary, "91", "INTERNAL1", 0, 90, null, "X");
		}
		else
		{
			AddFieldGS1(dictionary, "91", "INTERNAL1", 4, 0, "X");
		}
		AddFieldGS1(dictionary, "92", "INTERNAL2", 0, 90, null, "X");
		AddFieldGS1(dictionary, "93", "INTERNAL3", 0, 90, null, "X");
		AddFieldGS1(dictionary, "94", "INTERNAL4", 0, 90, null, "X");
		AddFieldGS1(dictionary, "95", "INTERNAL5", 0, 90, null, "X");
		AddFieldGS1(dictionary, "96", "INTERNAL6", 0, 90, null, "X");
		AddFieldGS1(dictionary, "97", "INTERNAL7", 0, 90, null, "X");
		AddFieldGS1(dictionary, "98", "INTERNAL8", 0, 90, null, "X");
		AddFieldGS1(dictionary, "99", "INTERNAL9", 0, 90, null, "X");
		return dictionary;
	}

	private static void AddFieldGS1(Dictionary<string, DescriptionFieldGS1> Codes, string Код, string Имя, int FixedLength = 0, int VariableLength = 0, string TypeFixedValue = null, string TypeVariableValue = null, bool? IsSeparator = null)
	{
		string text = Код.Substring(Код.Length - 1, 1);
		if (!"0123456789".Contains(text))
		{
			string text2 = Код.Substring(0, Код.Length - 1);
			if (text == "n")
			{
				DescriptionFieldGS1 fieldGS = GetFieldGS1(text2, Имя, FixedLength, VariableLength, TypeFixedValue, TypeVariableValue, IsSeparator);
				fieldGS.IsDecimalPointPosition = true;
				Codes.Add(text2, fieldGS);
				return;
			}
			for (int i = 0; i <= 9; i++)
			{
				string text3 = text2 + i;
				Codes.Add(text3, GetFieldGS1(text3, Имя, FixedLength, VariableLength, TypeFixedValue, TypeVariableValue, IsSeparator));
			}
		}
		else
		{
			Codes.Add(Код, GetFieldGS1(Код, Имя, FixedLength, VariableLength, TypeFixedValue, TypeVariableValue, IsSeparator));
		}
	}

	private static DescriptionFieldGS1 GetFieldGS1(string Код, string Имя, int FixedLength = 0, int VariableLength = 0, string TypeFixedValue = null, string TypeVariableValue = null, bool? IsSeparator = null)
	{
		DescriptionFieldGS1 descriptionFieldGS = new DescriptionFieldGS1();
		descriptionFieldGS.Code = Код;
		descriptionFieldGS.Name = Имя;
		descriptionFieldGS.FixedLength = FixedLength;
		if (FixedLength > 0)
		{
			if (TypeFixedValue == null)
			{
				descriptionFieldGS.TypeFixedValue = "N";
			}
			else
			{
				descriptionFieldGS.TypeFixedValue = TypeFixedValue;
			}
		}
		descriptionFieldGS.VariableLength = VariableLength;
		if (VariableLength > 0)
		{
			if (TypeVariableValue == null)
			{
				descriptionFieldGS.TypeVariableValue = "N";
			}
			else
			{
				descriptionFieldGS.TypeVariableValue = TypeVariableValue;
			}
		}
		if (VariableLength > 0)
		{
			descriptionFieldGS.IsSeparator = true;
		}
		else
		{
			descriptionFieldGS.IsSeparator = IsSeparator.HasValue;
		}
		descriptionFieldGS.IsDecimalPointPosition = false;
		return descriptionFieldGS;
	}

	private static DataProductCode FillDataProductCode(bool CodeParsed = false)
	{
		return new DataProductCode
		{
			isParsed = CodeParsed
		};
	}

	private static bool OnlyNumbersInString(string СтрокаПроверки)
	{
		if (СтрокаПроверки.Length == 0)
		{
			return true;
		}
		foreach (char c in СтрокаПроверки)
		{
			if (c < '0' || c > '9')
			{
				return false;
			}
		}
		return true;
	}

	private static bool StringValidation(string СтрокаПроверки, bool ДопустимыЛатПрописные = false, bool ДопустимыЛатСтрочные = false, bool ДопустимыЦифры = false, string ДопустимыеСимволы = "")
	{
		foreach (char c in СтрокаПроверки)
		{
			bool flag = false;
			if (ДопустимыЛатПрописные)
			{
				flag = c > '@' && c < '[';
			}
			bool flag2 = false;
			if (ДопустимыЛатСтрочные)
			{
				flag2 = c > '`' && c < '{';
			}
			bool flag3 = false;
			if (ДопустимыЦифры)
			{
				flag3 = c > '/' && c < ':';
			}
			if (!flag && !flag2 && !flag3 && !ДопустимыеСимволы.Contains(c))
			{
				return false;
			}
		}
		return true;
	}

	private static string Substring(string Value, int startIndex, int length)
	{
		if (startIndex > Value.Length - 1)
		{
			return "";
		}
		if (startIndex + length > Value.Length)
		{
			return Value.Substring(startIndex, Value.Length - startIndex);
		}
		return Value.Substring(startIndex, length);
	}

	private static char CalculateCheckCharGTIN8(string GTIN)
	{
		int num = 0;
		int num2 = 3;
		for (int i = 0; i < 7; i++)
		{
			char c = GTIN[i];
			num += num2 * (c - 48);
			num2 = 4 - num2;
		}
		num = (10 - num % 10) % 10;
		return Convert.ToChar(num + 48);
	}

	private static char CalculateCheckCharGTIN13(string GTIN)
	{
		int num = 0;
		int num2 = 1;
		for (int i = 0; i < 12; i++)
		{
			char c = GTIN[i];
			num += num2 * (c - 48);
			num2 = 4 - num2;
		}
		num = (10 - num % 10) % 10;
		return Convert.ToChar(num + 48);
	}

	private static char CalculateCheckCharGTIN14(string GTIN)
	{
		int num = 0;
		int num2 = 3;
		for (int i = 0; i < 13; i++)
		{
			char c = GTIN[i];
			num += num2 * (c - 48);
			num2 = 4 - num2;
		}
		num = (10 - num % 10) % 10;
		return Convert.ToChar(num + 48);
	}

	private static void GenerateEAN(DataProductCode ДанныеМаркировки, string GTIN)
	{
		ДанныеМаркировки.GTIN = GTIN;
		while (ДанныеМаркировки.GTIN.Length < 14)
		{
			ДанныеМаркировки.GTIN = "0" + ДанныеМаркировки.GTIN;
		}
		string text = GTIN;
		while (text[0] == '0' && text.Length > 8)
		{
			text = text.Substring(1);
		}
		ДанныеМаркировки.EAN = text;
	}

	private static long PrefixMarkingCode(TypeBarcode TypeBarcode)
	{
		long num = 0L;
		return TypeBarcode switch
		{
			TypeBarcode.EAN8 => 17672L, 
			TypeBarcode.EAN13 => 17677L, 
			TypeBarcode.ITF14 => 18702L, 
			TypeBarcode.GS1 => 17485L, 
			TypeBarcode.FurBarCode => 21062L, 
			TypeBarcode.EGAIS2 => 50452L, 
			TypeBarcode.EGAIS3 => 50462L, 
			_ => 0L, 
		};
	}

	private static void GenerateBinaryDataForNumbers(DataProductCode ДанныеМаркировки, string ЗначениеЧисла = null, string ЗначениеСтроки = null)
	{
		long num = PrefixMarkingCode(ДанныеМаркировки.TypeBarcode);
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		byte[] bytes = BitConverter.GetBytes(ulong.Parse(ЗначениеЧисла));
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes);
		}
		binaryWriter.Write(bytes);
		if (ЗначениеСтроки != null && ЗначениеСтроки != "")
		{
			binaryWriter.Write(Encoding.UTF8.GetBytes(ЗначениеСтроки));
		}
		binaryWriter.Seek(0, SeekOrigin.Begin);
		bytes = BitConverter.GetBytes((ushort)num);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes);
		}
		binaryWriter.Write(bytes);
		binaryWriter.Close();
		ДанныеМаркировки.MarkingCodeHEX = memoryStream.ToArray();
		ДанныеМаркировки.MarkingCodeBase64 = Convert.ToBase64String(ДанныеМаркировки.MarkingCodeHEX);
	}

	private static void GenerateBinaryDataForStrings(DataProductCode ДанныеМаркировки, string ЗначениеСтроки = null)
	{
		long num = PrefixMarkingCode(ДанныеМаркировки.TypeBarcode);
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		byte[] bytes = BitConverter.GetBytes((ushort)num);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes);
		}
		binaryWriter.Write(bytes);
		foreach (char c in ЗначениеСтроки)
		{
			if (c < 'Ā')
			{
				binaryWriter.Write((byte)c);
			}
		}
		binaryWriter.Close();
		ДанныеМаркировки.MarkingCodeHEX = memoryStream.ToArray();
		ДанныеМаркировки.MarkingCodeBase64 = Convert.ToBase64String(ДанныеМаркировки.MarkingCodeHEX);
	}

	private static void ParseStringBarcodeGS1WithBrackets(string Штрихкод, DataProductCode РезультатРазбора, Dictionary<string, DescriptionFieldGS1> CodesGS1)
	{
		РезультатРазбора.RepresentationBarCode = Штрихкод;
		int length = Штрихкод.Length;
		int num = 2;
		int num2 = 4;
		int num3 = 0;
		while (num3 < length)
		{
			if (Штрихкод[num3] != '(')
			{
				РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Отсутствует \"(\".";
				return;
			}
			num3++;
			int num4 = Штрихкод.IndexOf(')', num3);
			if (num4 == -1)
			{
				РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Отсутствует \")\".";
				return;
			}
			string text = Штрихкод.Substring(num3, num4 - num3);
			int length2 = text.Length;
			if (length2 < num || length2 > num2)
			{
				РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Неизвестный идентификатор применения(AI) {text}.";
				return;
			}
			string text2 = "";
			DescriptionFieldGS1 value = null;
			if (!CodesGS1.TryGetValue(text, out value) && length2 == num2)
			{
				if (!CodesGS1.TryGetValue(Substring(text, 0, num2 - 1), out value))
				{
					РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Неизвестный идентификатор применения(AI) {text}.";
					return;
				}
				text2 = Substring(text, text.Length - 1, 1);
			}
			num3 = num4 + 1;
			string text3 = "";
			if (value.FixedLength > 0)
			{
				text3 = Substring(Штрихкод, num3, value.FixedLength);
				if (text3.Length != value.FixedLength)
				{
					РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Длина значения ({text3.Length}) для идентификатора применения(AI) \"{text} {value.Name}\" меньше требуемой ({value.FixedLength})";
					return;
				}
				if (value.TypeFixedValue == "N" && !OnlyNumbersInString(text3))
				{
					РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Значение ({text3.Length}) для идентификатора применения(AI) \"{text} {value.Name}\" должно содержать только цифры";
					return;
				}
				num3 += value.FixedLength;
			}
			if (value.VariableLength > 0 && num4 < length - 1)
			{
				int num5 = Штрихкод.IndexOf('(', num3);
				bool flag = false;
				while (num5 >= 0 && !flag)
				{
					int num6 = Штрихкод.IndexOf(')', num5);
					string text4 = Substring(Штрихкод, num5 + 1, num6 - num5 - 1);
					flag = text4.Length > 1 || text4.Length < 5 || OnlyNumbersInString(text4);
					if (num5 >= length - 1)
					{
						num5 = 0;
					}
					else if (!flag)
					{
						num5 = Штрихкод.IndexOf('(', num5 + 1);
					}
				}
				string text5 = "";
				text5 = ((num5 <= 0) ? Substring(Штрихкод, num3, 100000) : Substring(Штрихкод, num3, num5 - num3));
				if (text5.Length > value.VariableLength)
				{
					РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Длина значения ({text5.Length}) переменной части для идентификатора применения(AI) \"{text} {value.Name}\" больше требуемой ({value.VariableLength})";
					return;
				}
				if (value.TypeVariableValue == "N" && !OnlyNumbersInString(text5))
				{
					РезультатРазбора.Errors = $"Номер символа {num3 + 1}. Значение ({text5.Length}) для идентификатора применения(AI) \"{text} {value.Name}\" должно содержать только цифры";
					return;
				}
				num3 += text5.Length;
				text3 += text5;
			}
			int num7 = 0;
			if (text2 != "")
			{
				num7 = int.Parse(text2);
				if (num7 > 0)
				{
					for (int i = 0; i < num7 - text3.Length; i++)
					{
						text3 = "0" + text3;
					}
					text3 = Substring(text3, 0, text3.Length - num7) + "." + Substring(text3, num7, 10000);
				}
			}
			if (РезультатРазбора.DataBarCode.ContainsKey(value.Code))
			{
				РезультатРазбора.DataBarCode[value.Code] = text3;
			}
			else
			{
				РезультатРазбора.DataBarCode.Add(value.Code, text3);
			}
		}
		РезультатРазбора.isParsed = true;
	}

	private static void ParseStringBarcodeGS1Service(string Штрихкод, DataProductCode РезультатРазбора, Dictionary<string, DescriptionFieldGS1> CodesGS1)
	{
		int length = Штрихкод.Length;
		string text = "";
		int num = 0;
		while (num < length)
		{
			string text2 = Substring(Штрихкод, num, 2);
			DescriptionFieldGS1 value = null;
			if (!CodesGS1.TryGetValue(text2, out value))
			{
				text2 = Substring(Штрихкод, num, 3);
				if (!CodesGS1.TryGetValue(text2, out value))
				{
					text2 = Substring(Штрихкод, num, 4);
					if (!CodesGS1.TryGetValue(text2, out value))
					{
						РезультатРазбора.Errors = $"Неизвестный идентификатор применения(AI) {text2}.";
						return;
					}
				}
			}
			num += text2.Length;
			string text3 = "";
			if (value.IsDecimalPointPosition)
			{
				text3 = Substring(Штрихкод, num, 1);
				num++;
			}
			string text4 = "";
			if (value.FixedLength > 0)
			{
				text4 = Substring(Штрихкод, num, value.FixedLength);
				if (text4.Length != value.FixedLength)
				{
					РезультатРазбора.Errors = $"Длина значения ({text4.Length}) для идентификатора применения(AI) \"{text2} {value.Name}\" меньше требуемой ({value.FixedLength})";
					return;
				}
				if (value.TypeFixedValue == "N" && !OnlyNumbersInString(text4))
				{
					РезультатРазбора.Errors = $"Значение ({text4.Length}) для идентификатора применения(AI) \"{text2} {value.Name}\" должно содержать только цифры";
					return;
				}
				num += value.FixedLength;
			}
			if (value.VariableLength > 0)
			{
				string text5 = Substring(Штрихкод, num, 10000);
				if (text5.Length > value.VariableLength)
				{
					РезультатРазбора.Errors = $"Длина значения ({text5.Length}) переменной части для идентификатора применения(AI) \"{text2} {value.Name}\" больше требуемой ({value.VariableLength})";
					return;
				}
				if (value.TypeVariableValue == "N" && !OnlyNumbersInString(text5))
				{
					РезультатРазбора.Errors = $"Значение ({text5.Length}) для идентификатора применения(AI) \"{text2} {value.Name}\" должно содержать только цифры";
					return;
				}
				num += text5.Length;
				text4 += text5;
			}
			text = text + "(" + text2 + text3 + ")" + text4;
			int num2 = 0;
			if (text3 != "")
			{
				num2 = int.Parse(text3);
				if (num2 > 0)
				{
					for (int i = 0; i <= num2 - text4.Length; i++)
					{
						text4 = "0" + text4;
					}
					text4 = Substring(text4, 0, text4.Length - num2) + "." + Substring(text4, num2, 10000);
				}
			}
			if (РезультатРазбора.DataBarCode.ContainsKey(value.Code))
			{
				РезультатРазбора.DataBarCode[value.Code] = text4;
			}
			else
			{
				РезультатРазбора.DataBarCode.Add(value.Code, text4);
			}
		}
		РезультатРазбора.RepresentationBarCode += text;
		if (РезультатРазбора.Errors == "")
		{
			РезультатРазбора.isParsed = true;
		}
	}

	private static string GenerateTryBarCode(DataProductCode DataMarkingCode)
	{
		if (DataMarkingCode.DataBarCode == null || DataMarkingCode.DataBarCode.Count == 0)
		{
			return DataMarkingCode.TryBarCode;
		}
		Dictionary<string, DescriptionFieldGS1> fieldsGS = GetFieldsGS1();
		string text = "";
		foreach (KeyValuePair<string, string> item in DataMarkingCode.DataBarCode)
		{
			text = ((fieldsGS[item.Key].FixedLength == 0) ? (text + item.Key + item.Value + "\u001d") : (text + item.Key + item.Value));
		}
		return text;
	}
}
