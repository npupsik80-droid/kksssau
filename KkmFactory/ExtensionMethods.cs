using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace KkmFactory;

public static class ExtensionMethods
{
	public static string AsString(this bool Val)
	{
		if (Val)
		{
			return "true";
		}
		return "false";
	}

	public static bool AsBool(this string Val)
	{
		if (Val.ToLower() == "true")
		{
			return true;
		}
		return false;
	}

	public static string AsString(this byte Val)
	{
		return Val.ToString();
	}

	public static string AsString(this int Val)
	{
		return Val.ToString();
	}

	public static string AsString(this long Val)
	{
		return Val.ToString();
	}

	public static string AsString(this uint Val)
	{
		return Val.ToString();
	}

	public static string AsString(this ulong Val)
	{
		return Val.ToString();
	}

	public static byte AsByte(this string Val)
	{
		if (Val == "")
		{
			return 0;
		}
		return (byte)Val.AsDouble();
	}

	public static int AsInt(this string Val)
	{
		if (Val == "")
		{
			return 0;
		}
		return (int)Val.AsDouble();
	}

	public static long AsInt64(this string Val)
	{
		if (Val == "")
		{
			return 0L;
		}
		return (long)Val.AsDouble();
	}

	public static uint AsUInt(this string Val)
	{
		if (Val == "")
		{
			return 0u;
		}
		return (uint)Val.AsDouble();
	}

	public static ulong AsUInt64(this string Val)
	{
		if (Val == "")
		{
			return 0uL;
		}
		return (ulong)Val.AsDouble();
	}

	public static string AsString(this double Val)
	{
		return Val.ToString(CultureInfo.InvariantCulture);
	}

	public static double AsDouble(this string Val)
	{
		if (Val == "")
		{
			return 0.0;
		}
		return double.Parse(Val, CultureInfo.InvariantCulture);
	}

	public static string AsString(this decimal Val)
	{
		return Val.ToString(CultureInfo.InvariantCulture);
	}

	public static decimal AsDecimal(this string Val)
	{
		if (Val == "")
		{
			return 0m;
		}
		return decimal.Parse(Val, CultureInfo.InvariantCulture);
	}

	public static string AsString(this DateTime Val)
	{
		return Val.ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
	}

	public static DateTime AsDateTime(this string Val)
	{
		return DateTime.ParseExact(Val, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
	}

	public static string AsString(this Dictionary<int, string> Val)
	{
		return JsonConvert.SerializeObject(Val, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
			Formatting = Formatting.Indented
		});
	}

	public static Dictionary<int, string> AsDictionaryIntString(this string Val)
	{
		return JsonConvert.DeserializeObject<Dictionary<int, string>>(Val, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml
		});
	}

	public static string AsString(this List<Dictionary<int, string>> Val)
	{
		return JsonConvert.SerializeObject(Val, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
			Formatting = Formatting.Indented
		});
	}

	public static List<Dictionary<int, string>> AsListDictionaryIntString(this string Val)
	{
		return JsonConvert.DeserializeObject<List<Dictionary<int, string>>>(Val, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml
		});
	}

	public static string AsString(this byte[] Val)
	{
		return JsonConvert.SerializeObject(Val, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
			Formatting = Formatting.Indented
		});
	}

	public static byte[] AsArrayByte(this string Val)
	{
		return JsonConvert.DeserializeObject<byte[]>(Val, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml
		});
	}
}
