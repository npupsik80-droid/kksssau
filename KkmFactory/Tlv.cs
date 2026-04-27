using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KkmFactory;

public class Tlv
{
	public enum TagDataType
	{
		None,
		Nested,
		String,
		RawData,
		Int,
		Decimal
	}

	public enum TagDataFormat
	{
		Dafault,
		Bin_LitEndian,
		Bin_BigEndian,
		BCD,
		HEX_LitEndian,
		HEX_BigEndian,
		RawData
	}

	public enum ETlvSettings
	{
		Standart,
		Teg2Len2
	}

	public class InfoTeg
	{
		public int NumTeg;

		public TagDataType DataType;

		public int Lenght;

		public TagDataFormat DataFormat;

		public Encoding Encoding;

		public InfoTeg(int NumTeg, TagDataType DataType, TagDataFormat DataFormat = TagDataFormat.Dafault, int Lenght = 0)
		{
			this.NumTeg = NumTeg;
			this.DataType = DataType;
			this.Lenght = Lenght;
			this.DataFormat = DataFormat;
			if (DataType == TagDataType.String && DataFormat == TagDataFormat.Dafault)
			{
				Encoding = Encoding.ASCII;
			}
			else if (DataType == TagDataType.String)
			{
				Encoding = Encoding.GetEncoding((int)DataFormat);
			}
		}
	}

	public class Teg
	{
		public InfoTeg InfoTeg;

		public object Value;

		public Teg(InfoTeg InfoTeg, object Value)
		{
			this.InfoTeg = InfoTeg;
			this.Value = Value;
		}

		private long ValueToInt64()
		{
			if (Value is int)
			{
				return (int)Value;
			}
			if (Value is uint)
			{
				return (uint)Value;
			}
			if (Value is int)
			{
				return (int)Value;
			}
			if (Value is uint)
			{
				return (uint)Value;
			}
			if (Value is long)
			{
				return (long)Value;
			}
			if (Value is ulong)
			{
				return (long)(ulong)Value;
			}
			return 0L;
		}

		private ulong ValueToUInt64()
		{
			if (Value is int)
			{
				return (ulong)(int)Value;
			}
			if (Value is uint)
			{
				return (uint)Value;
			}
			if (Value is int)
			{
				return (ulong)(int)Value;
			}
			if (Value is uint)
			{
				return (uint)Value;
			}
			if (Value is long)
			{
				return (ulong)(long)Value;
			}
			if (Value is ulong)
			{
				return (ulong)Value;
			}
			return 0uL;
		}

		public Teg(int tag, Span<byte> Data, Tlv Tlv)
		{
			try
			{
				InfoTeg = Tlv.FindTag(tag);
			}
			catch
			{
				InfoTeg = new InfoTeg(tag, TagDataType.RawData);
			}
			if (InfoTeg.DataType == TagDataType.Nested)
			{
				Tlv tlv = Activator.CreateInstance(Tlv.GetType()) as Tlv;
				tlv.ParceArray(Data);
				Value = tlv;
			}
			else if (InfoTeg.DataType == TagDataType.String)
			{
				Value = InfoTeg.Encoding.GetString(Data.ToArray());
			}
			else if (InfoTeg.DataType == TagDataType.RawData)
			{
				Value = Data.ToArray();
			}
			else if (InfoTeg.DataType == TagDataType.Int && (InfoTeg.DataFormat == TagDataFormat.Bin_LitEndian || InfoTeg.DataFormat == TagDataFormat.Bin_BigEndian))
			{
				ulong num = 0uL;
				if (InfoTeg.DataFormat == TagDataFormat.Bin_LitEndian)
				{
					for (int num2 = Data.Length - 1; num2 >= 0; num2--)
					{
						num = (num << 8) | Data[num2];
					}
				}
				else
				{
					for (int i = 0; i < Data.Length; i++)
					{
						num = (num << 8) | Data[i];
					}
				}
				Value = num;
			}
			else if (InfoTeg.DataType == TagDataType.Int && InfoTeg.DataFormat == TagDataFormat.BCD)
			{
				string s = Encoding.ASCII.GetString(Data.ToArray());
				Value = ulong.Parse(s);
			}
			else if (InfoTeg.DataType == TagDataType.Int && (InfoTeg.DataFormat == TagDataFormat.HEX_LitEndian || InfoTeg.DataFormat == TagDataFormat.HEX_BigEndian))
			{
				string text = Encoding.ASCII.GetString(Data.ToArray());
				StringBuilder stringBuilder = new StringBuilder(text);
				if (InfoTeg.DataFormat == TagDataFormat.HEX_LitEndian)
				{
					for (int j = 0; j < stringBuilder.Length / 2; j++)
					{
						stringBuilder[stringBuilder.Length - j * 2 - 2] = text[j * 2];
						stringBuilder[stringBuilder.Length - j * 2 - 1] = text[j * 2 + 1];
					}
				}
				Value = ulong.Parse(stringBuilder.ToString(), NumberStyles.HexNumber);
			}
			else
			{
				if (InfoTeg.DataType != TagDataType.Decimal || InfoTeg.DataFormat != TagDataFormat.BCD)
				{
					throw new Exception("Некорректный тип");
				}
				string text2 = Encoding.ASCII.GetString(Data.ToArray());
				text2 = text2.Replace(',', '.');
				Value = decimal.Parse(text2, CultureInfo.InvariantCulture);
			}
		}

		public Span<byte> ValueToBytes(Tlv Tlv)
		{
			if (InfoTeg.DataType == TagDataType.Nested)
			{
				return (Value as Tlv).ToArray();
			}
			if (InfoTeg.DataType == TagDataType.String)
			{
				return InfoTeg.Encoding.GetBytes(Value as string);
			}
			if (InfoTeg.DataType == TagDataType.RawData)
			{
				return Value as byte[];
			}
			if (InfoTeg.DataType == TagDataType.Int && (InfoTeg.DataFormat == TagDataFormat.Bin_LitEndian || InfoTeg.DataFormat == TagDataFormat.Bin_BigEndian))
			{
				byte[] array = new byte[8];
				ulong num = ValueToUInt64();
				for (int i = 0; i < 8; i++)
				{
					byte b = (byte)((int)num & 0xFF);
					num >>= 8;
					if (InfoTeg.DataFormat == TagDataFormat.Bin_LitEndian)
					{
						array[i] = b;
					}
					else
					{
						array[7 - i] = b;
					}
				}
				if (InfoTeg.Lenght != 0)
				{
					if (InfoTeg.DataFormat == TagDataFormat.Bin_LitEndian)
					{
						return new Span<byte>(array, 0, InfoTeg.Lenght);
					}
					return new Span<byte>(array, 8 - InfoTeg.Lenght, InfoTeg.Lenght);
				}
				for (int num2 = 8; num2 > 0; num2--)
				{
					if (InfoTeg.DataFormat == TagDataFormat.Bin_LitEndian && array[num2 - 1] != 0)
					{
						return new Span<byte>(array, 0, num2);
					}
					if (InfoTeg.DataFormat == TagDataFormat.Bin_BigEndian && array[7 - num2] != 0)
					{
						return new Span<byte>(array, 8 - num2, num2);
					}
				}
				return array;
			}
			if (InfoTeg.DataType == TagDataType.Int && InfoTeg.DataFormat == TagDataFormat.BCD)
			{
				string text = ValueToUInt64().ToString();
				if (InfoTeg.Lenght != 0)
				{
					if (text.Length > InfoTeg.Lenght)
					{
						throw new ArgumentException("tlv");
					}
					if (text.Length < InfoTeg.Lenght)
					{
						text = text.PadLeft(InfoTeg.Lenght, '0');
					}
				}
				return Encoding.ASCII.GetBytes(text);
			}
			if (InfoTeg.DataType == TagDataType.Int && (InfoTeg.DataFormat == TagDataFormat.HEX_LitEndian || InfoTeg.DataFormat == TagDataFormat.HEX_BigEndian))
			{
				byte[] array2 = new byte[8];
				ulong num3 = ValueToUInt64();
				for (int j = 0; j < 8; j++)
				{
					byte b2 = (byte)((int)num3 & 0xFF);
					num3 >>= 8;
					if (InfoTeg.DataFormat == TagDataFormat.HEX_LitEndian)
					{
						array2[j] = b2;
					}
					else
					{
						array2[7 - j] = b2;
					}
				}
				Span<byte> arr = null;
				if (InfoTeg.Lenght != 0)
				{
					arr = ((InfoTeg.DataFormat != TagDataFormat.HEX_LitEndian) ? new Span<byte>(array2, 8 - InfoTeg.Lenght, InfoTeg.Lenght) : new Span<byte>(array2, 0, InfoTeg.Lenght));
				}
				else
				{
					for (int num4 = 8; num4 > 0; num4--)
					{
						if (InfoTeg.DataFormat == TagDataFormat.HEX_LitEndian && array2[num4 - 1] != 0)
						{
							arr = new Span<byte>(array2, 0, num4);
							break;
						}
						if (InfoTeg.DataFormat == TagDataFormat.HEX_BigEndian && array2[7 - num4] != 0)
						{
							arr = new Span<byte>(array2, 8 - num4, num4);
							break;
						}
					}
				}
				string hexString = GetHexString(arr);
				return Encoding.ASCII.GetBytes(hexString);
			}
			if (InfoTeg.DataType == TagDataType.Decimal && InfoTeg.DataFormat == TagDataFormat.BCD)
			{
				string text2 = ((decimal)Value).ToString(CultureInfo.InvariantCulture);
				if (InfoTeg.Lenght != 0)
				{
					if (text2.Length > InfoTeg.Lenght)
					{
						throw new ArgumentException("tlv");
					}
					if (text2.Length < InfoTeg.Lenght)
					{
						text2 = text2.PadLeft(InfoTeg.Lenght, '0');
					}
				}
				return Encoding.ASCII.GetBytes(text2);
			}
			throw new Exception("Некорректный тип");
		}
	}

	public ETlvSettings TlvSettings;

	private char TypeTeg = '-';

	private char TypeLem = '-';

	public List<Teg> List;

	public virtual void SetSettings()
	{
	}

	public void ParseSettings()
	{
		if (TlvSettings == ETlvSettings.Standart)
		{
			TypeTeg = 'S';
			TypeLem = 'S';
		}
		else if (TlvSettings == ETlvSettings.Teg2Len2)
		{
			TypeTeg = '2';
			TypeLem = '2';
		}
	}

	public Tlv()
	{
		SetSettings();
		List = new List<Teg>();
	}

	public Tlv ParceHexString(string tlv, StringBuilder ParcerLog = null)
	{
		if (tlv == null)
		{
			throw new ArgumentException("tlv");
		}
		ParceArray(GetBytes(tlv), ParcerLog);
		return this;
	}

	public Tlv ParceArray(Span<byte> rawTlv, StringBuilder ParcerLog = null)
	{
		if (rawTlv == null)
		{
			throw new ArgumentException("tlv");
		}
		ParcerLog?.Append("> Приняли пакет:");
		StringBuilder stringBuilder = new StringBuilder();
		ParseSettings();
		int num = 0;
		int num2 = 0;
		while (num < rawTlv.Length)
		{
			stringBuilder.Clear();
			if (rawTlv[num] == 0)
			{
				num++;
			}
			else
			{
				int tag = 0;
				if (TypeTeg == 'S')
				{
					bool flag = (rawTlv[num] & 0x1F) == 31;
					while (flag && (rawTlv[++num] & 0x80) != 0)
					{
					}
					num++;
					tag = (int)GetInt(rawTlv, num2, num - num2);
					stringBuilder.Append(rawTlv[num2].ToString("X2"));
					if (num - num2 == 2)
					{
						stringBuilder.Append(rawTlv[num2 + 1].ToString("X2"));
					}
				}
				else if (TypeTeg == '2')
				{
					num += 2;
					tag = rawTlv[num2] + (rawTlv[num2 + 1] << 8);
					stringBuilder.Append(rawTlv[num2].ToString("X2"));
					stringBuilder.Append(rawTlv[num2 + 1].ToString("X2"));
				}
				stringBuilder.Append(" ");
				int num3 = 0;
				if (TypeLem == 'S')
				{
					bool num4 = (rawTlv[num] & 0x80) != 0;
					if (num4)
					{
						stringBuilder.Append(rawTlv[num].ToString("X2"));
						stringBuilder.Append(rawTlv[num + 1].ToString("X2"));
					}
					else
					{
						stringBuilder.Append(rawTlv[num].ToString("X2"));
					}
					num3 = (int)(num4 ? GetInt(rawTlv, num + 1, rawTlv[num] & 0x1F) : rawTlv[num]);
					num = (num4 ? (num + (rawTlv[num] & 0x1F) + 1) : (num + 1));
				}
				else if (TypeLem == '2')
				{
					num3 = rawTlv[num] + (rawTlv[num + 1] << 8);
					stringBuilder.Append(rawTlv[num].ToString("X2"));
					stringBuilder.Append(rawTlv[num + 1].ToString("X2"));
					num += 2;
				}
				stringBuilder.Append(" ");
				num2 = num;
				num += num3;
				Span<byte> span = rawTlv.Slice(num2, num3);
				Teg teg = new Teg(tag, span, this);
				List.Add(teg);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
				handler.AppendLiteral("+ ");
				handler.AppendFormatted(span.Length.ToString());
				handler.AppendLiteral(" byte");
				stringBuilder3.Append(ref handler);
				if (ParcerLog != null && ParcerLog != null)
				{
					if (teg.Value.GetType() == typeof(byte[]))
					{
						stringBuilder2 = ParcerLog;
						StringBuilder stringBuilder4 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(5, 2, stringBuilder2);
						handler.AppendLiteral("\r\n");
						handler.AppendFormatted(GatName(teg.InfoTeg));
						handler.AppendLiteral(" = ");
						handler.AppendFormatted(GetHexString(span));
						stringBuilder4.Append(ref handler);
					}
					else if (teg.Value.GetType() == typeof(string))
					{
						stringBuilder2 = ParcerLog;
						StringBuilder stringBuilder5 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(7, 2, stringBuilder2);
						handler.AppendLiteral("\r\n");
						handler.AppendFormatted(GatName(teg.InfoTeg));
						handler.AppendLiteral(" = \"");
						handler.AppendFormatted(teg.Value.ToString());
						handler.AppendLiteral("\"");
						stringBuilder5.Append(ref handler);
					}
					else
					{
						stringBuilder2 = ParcerLog;
						StringBuilder stringBuilder6 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(5, 2, stringBuilder2);
						handler.AppendLiteral("\r\n");
						handler.AppendFormatted(GatName(teg.InfoTeg));
						handler.AppendLiteral(" = ");
						handler.AppendFormatted(teg.Value.ToString());
						stringBuilder6.Append(ref handler);
					}
					ParcerLog.Append(" " + stringBuilder);
				}
			}
			num2 = num;
		}
		return this;
	}

	public byte[] ToArray(StringBuilder ParcerLog = null)
	{
		ParseSettings();
		ParcerLog?.Append("< Передаем пакет:");
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		char c = '-';
		char c2 = '-';
		if (TlvSettings == ETlvSettings.Standart)
		{
			c = 'S';
			c2 = 'S';
		}
		else if (TlvSettings == ETlvSettings.Teg2Len2)
		{
			c = '2';
			c2 = '2';
		}
		foreach (Teg item in List)
		{
			Span<byte> arr = item.ValueToBytes(this);
			StringBuilder stringBuilder = new StringBuilder();
			switch (c)
			{
			case 'S':
				if (((item.InfoTeg.NumTeg >> 8) & 0x1F) == 31)
				{
					binaryWriter.Write((byte)((item.InfoTeg.NumTeg >> 8) & 0xFF));
					binaryWriter.Write((byte)(item.InfoTeg.NumTeg & 0xFF));
					stringBuilder.Append(((item.InfoTeg.NumTeg >> 8) & 0xFF).ToString("X2"));
					stringBuilder.Append((item.InfoTeg.NumTeg & 0xFF).ToString("X2"));
				}
				else
				{
					binaryWriter.Write((byte)(item.InfoTeg.NumTeg & 0xFF));
					stringBuilder.Append((item.InfoTeg.NumTeg & 0xFF).ToString("X2"));
				}
				break;
			case '2':
				binaryWriter.Write((byte)(item.InfoTeg.NumTeg & 0xFF));
				binaryWriter.Write((byte)((item.InfoTeg.NumTeg >> 8) & 0xFF));
				stringBuilder.Append((item.InfoTeg.NumTeg & 0xFF).ToString("X2"));
				stringBuilder.Append(((item.InfoTeg.NumTeg >> 8) & 0xFF).ToString("X2"));
				break;
			}
			stringBuilder.Append(" ");
			int length = arr.Length;
			switch (c2)
			{
			case 'S':
				if (length >= 128)
				{
					binaryWriter.Write((byte)130);
					binaryWriter.Write((byte)(length & 0xFF));
					binaryWriter.Write((byte)((length >> 8) & 0xFF));
					stringBuilder.Append(130.ToString("X2"));
					stringBuilder.Append((length & 0xFF).ToString("X2"));
					stringBuilder.Append(((length >> 8) & 0xFF).ToString("X2"));
				}
				else
				{
					binaryWriter.Write((byte)length);
					stringBuilder.Append(length.ToString("X2"));
				}
				break;
			case '2':
				binaryWriter.Write((byte)(length & 0xFF));
				binaryWriter.Write((byte)((length >> 8) & 0xFF));
				stringBuilder.Append((length & 0xFF).ToString("X2"));
				stringBuilder.Append(((length >> 8) & 0xFF).ToString("X2"));
				break;
			}
			stringBuilder.Append(" ");
			binaryWriter.Write(arr.ToArray());
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
			handler.AppendLiteral("+ ");
			handler.AppendFormatted(arr.Length.ToString());
			handler.AppendLiteral(" byte");
			stringBuilder3.Append(ref handler);
			if (ParcerLog != null)
			{
				if (item.Value.GetType() == typeof(byte[]))
				{
					stringBuilder2 = ParcerLog;
					StringBuilder stringBuilder4 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(5, 2, stringBuilder2);
					handler.AppendLiteral("\r\n");
					handler.AppendFormatted(GatName(item.InfoTeg));
					handler.AppendLiteral(" = ");
					handler.AppendFormatted(GetHexString(arr));
					stringBuilder4.Append(ref handler);
				}
				else if (item.Value.GetType() == typeof(string))
				{
					stringBuilder2 = ParcerLog;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(7, 2, stringBuilder2);
					handler.AppendLiteral("\r\n");
					handler.AppendFormatted(GatName(item.InfoTeg));
					handler.AppendLiteral(" = \"");
					handler.AppendFormatted(item.Value.ToString());
					handler.AppendLiteral("\"");
					stringBuilder5.Append(ref handler);
				}
				else
				{
					stringBuilder2 = ParcerLog;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(5, 2, stringBuilder2);
					handler.AppendLiteral("\r\n");
					handler.AppendFormatted(GatName(item.InfoTeg));
					handler.AppendLiteral(" = ");
					handler.AppendFormatted(item.Value.ToString());
					stringBuilder6.Append(ref handler);
				}
				ParcerLog.Append(" " + stringBuilder);
			}
		}
		return memoryStream.ToArray();
	}

	public string ToHexString(StringBuilder ParcerLog = null)
	{
		return GetHexString(ToArray(ParcerLog));
	}

	public Tlv Add(InfoTeg tag, object value, bool AddEmpty = false)
	{
		if (!AddEmpty || (value != null && value.ToString() != "0" && value.ToString() != ""))
		{
			Teg item = new Teg(tag, value);
			List.Add(item);
		}
		return this;
	}

	public bool Contains(InfoTeg tag)
	{
		return List.Find((Teg t) => t.InfoTeg == tag) != null;
	}

	public object Find(InfoTeg tag)
	{
		return List.Find((Teg t) => t.InfoTeg == tag)?.Value;
	}

	public List<Teg> FindAll(InfoTeg tag)
	{
		return List.FindAll((Teg t) => t.InfoTeg == tag);
	}

	public InfoTeg FindTag(int tag)
	{
		FieldInfo[] fields = GetType().GetFields();
		for (int i = 0; i < fields.Length; i++)
		{
			object value = fields[i].GetValue(this);
			if (value is InfoTeg && (value as InfoTeg).NumTeg == tag)
			{
				return (InfoTeg)value;
			}
		}
		throw new Exception("Некорректный номер тега");
	}

	public string GatName(InfoTeg tag)
	{
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(this);
			if (value is InfoTeg && (value as InfoTeg).NumTeg == tag.NumTeg)
			{
				return fieldInfo.Name;
			}
		}
		return "0x" + tag.NumTeg.ToString("X2");
	}

	private static byte[] GetBytes(string hexString)
	{
		return (from x in Enumerable.Range(0, hexString.Length)
			where x % 2 == 0
			select Convert.ToByte(hexString.Substring(x, 2), 16)).ToArray();
	}

	private static ulong GetInt(Span<byte> data, int offset, int length)
	{
		ulong num = 0uL;
		for (int i = 0; i < length; i++)
		{
			num = (num << 8) | data[offset + i];
		}
		return num;
	}

	public static string GetHexString(Span<byte> arr)
	{
		StringBuilder stringBuilder = new StringBuilder(arr.Length * 2);
		Span<byte> span = arr;
		for (int i = 0; i < span.Length; i++)
		{
			byte b = span[i];
			stringBuilder.AppendFormat("{0:X2}", b);
		}
		return stringBuilder.ToString();
	}
}
