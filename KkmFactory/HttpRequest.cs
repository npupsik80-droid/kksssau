using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class HttpRequest
{
	public class ItemBase
	{
		public string Source { get; set; }

		public ItemBase(string source)
		{
			Source = source;
		}
	}

	public class ItemHost : ItemBase
	{
		public string Host;

		public int Port;

		public ItemHost(string source)
			: base(source)
		{
			Match match = new Regex("^(((?<host>.+?):(?<port>\\d+?))|(?<host>.+?))$").Match(source);
			Host = match.Groups["host"].Value;
			if (!int.TryParse(match.Groups["port"].Value, out Port))
			{
				Port = 80;
			}
		}
	}

	public class ItemContentType : ItemBase
	{
		public string Value { get; set; }

		public string Charset { get; set; }

		public ItemContentType(string source)
			: base(source)
		{
			if (string.IsNullOrEmpty(source))
			{
				return;
			}
			int num = source.IndexOf(";");
			if (num == -1)
			{
				Value = source.Trim().ToLower();
				return;
			}
			Value = source.Substring(0, num).Trim().ToLower();
			string text = source.Substring(num + 1, source.Length - num - 1);
			foreach (Match item in new Regex("(?<key>.+?)=((\"(?<value>.+?)\")|((?<value>[^\\;]+)))[\\;]{0,1}", RegexOptions.Singleline).Matches(text))
			{
				if (item.Groups["key"].Value.Trim().ToLower() == "charset")
				{
					Charset = item.Groups["value"].Value;
				}
			}
		}
	}

	public EndPoint RemoteEndPoint;

	public byte[] Source = new byte[0];

	public string SourceText = "";

	public string HTTPSourceText = "";

	private int _HeadersTail = -1;

	public string[] httpArg = new string[6] { "", "", "", "", "", "" };

	public string HTTPVersion { get; set; }

	public string Method { get; set; }

	public string Path { get; set; }

	public string Url { get; set; }

	public int StatusCode { get; set; }

	public string StatusMessage { get; set; }

	public Dictionary<string, ItemBase> Headers { get; set; }

	public Dictionary<string, string> Cookies { get; set; }

	public List<string> Paths { get; set; }

	public Dictionary<string, string> ArgForm { get; set; }

	public string Host
	{
		get
		{
			if (!Headers.ContainsKey("Host"))
			{
				return string.Empty;
			}
			return ((ItemHost)Headers["Host"]).Host;
		}
	}

	public int Port
	{
		get
		{
			if (!Headers.ContainsKey("Host"))
			{
				return 80;
			}
			return ((ItemHost)Headers["Host"]).Port;
		}
	}

	public static async Task<HttpRequest> CreateAsync(byte[] source, Stream ClientStream, TcpClient myClient)
	{
		HttpRequest HttpRequest = new HttpRequest();
		HttpRequest.Source = source;
		HttpRequest.RemoteEndPoint = myClient.Client.RemoteEndPoint;
		HttpRequest.SourceText = Encoding.UTF8.GetString(HttpRequest.Source);
		string text = Encoding.ASCII.GetString(HttpRequest.Source);
		if (text.IndexOf("\r\n") == -1)
		{
			return HttpRequest;
		}
		string text2 = text.Substring(0, text.IndexOf("\r\n"));
		Regex regex = new Regex("(?<method>.+)\\s+(?<path>.+)\\s+HTTP/(?<version>[\\d\\.]+)", RegexOptions.Multiline);
		if (regex.IsMatch(text2))
		{
			Match match = regex.Match(text2);
			HttpRequest.Method = match.Groups["method"].Value.ToUpper();
			HttpRequest.Path = match.Groups["path"].Value;
			HttpRequest.HTTPVersion = match.Groups["version"].Value;
		}
		HttpRequest._HeadersTail = text.IndexOf("\r\n\r\n");
		if (HttpRequest._HeadersTail != -1)
		{
			HttpRequest.HTTPSourceText = text.Substring(0, text.IndexOf("\r\n\r\n"));
			text = text.Substring(text.IndexOf("\r\n") + 2, HttpRequest._HeadersTail - text.IndexOf("\r\n") - 2);
		}
		else
		{
			HttpRequest._HeadersTail = -1;
		}
		HttpRequest.Headers = new Dictionary<string, ItemBase>(StringComparer.CurrentCultureIgnoreCase);
		HttpRequest.Cookies = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		regex = new Regex("^(?<key>[^\\x3A]+)\\:\\s{1}(?<value>.+)$", RegexOptions.Multiline);
		string[] array;
		foreach (Match item2 in regex.Matches(text))
		{
			string value = item2.Groups["key"].Value;
			if (HttpRequest.Headers.ContainsKey(value))
			{
				continue;
			}
			if (value.Trim().ToLower() == "host")
			{
				HttpRequest.Headers.Add(value, new ItemHost(item2.Groups["value"].Value.Trim("\r\n ".ToCharArray())));
			}
			else if (value.Trim().ToLower() == "content-type")
			{
				HttpRequest.Headers.Add(value, new ItemContentType(item2.Groups["value"].Value.Trim("\r\n ".ToCharArray())));
			}
			else if (value.Trim().ToLower() == "cookie")
			{
				array = item2.Groups["value"].Value.Trim("\r\n ".ToCharArray()).Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(new string[1] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					if (array2.Length == 2)
					{
						if (HttpRequest.Cookies.ContainsKey(array2[0].Trim()))
						{
							HttpRequest.Cookies[array2[0].Trim()] = array2[1].Trim();
						}
						else
						{
							HttpRequest.Cookies.Add(array2[0].Trim(), array2[1].Trim());
						}
					}
				}
			}
			else
			{
				HttpRequest.Headers.Add(value, new ItemBase(item2.Groups["value"].Value.Trim("\r\n ".ToCharArray())));
			}
		}
		if (ClientStream.GetType() == typeof(SslStream))
		{
			HttpRequest.Url = "https://";
		}
		else
		{
			HttpRequest.Url = "http://";
		}
		HttpRequest.Url = HttpRequest.Url + HttpRequest.Host + ":" + HttpRequest.Port;
		HttpRequest.Paths = new List<string>();
		string[] array3 = HttpRequest.Path.Split(new char[1] { '/' }, StringSplitOptions.None);
		bool flag = true;
		array = array3;
		foreach (string item in array)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				HttpRequest.Paths.Add(item);
			}
		}
		string text3 = HttpRequest.Path.Replace("//", "/");
		if (text3.IndexOf("?") != -1)
		{
			text3 = text3.Substring(0, text3.IndexOf("?"));
		}
		string[] array4 = text3.Split('/');
		for (int j = 1; j < array4.Length; j++)
		{
			HttpRequest.httpArg[j - 1] = array4[j].Trim();
		}
		HttpRequest.ArgForm = new Dictionary<string, string>();
		if (HttpRequest.Path.IndexOf("?") != -1)
		{
			array = HttpRequest.Path.Substring(HttpRequest.Path.IndexOf("?") + 1).Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				string text4 = HttpUtility.UrlDecode(array[i]);
				if (text4 == "")
				{
					continue;
				}
				string text5 = "";
				string text6;
				if (text4.IndexOf("=") != -1)
				{
					text6 = text4.Substring(0, text4.IndexOf("="));
					text5 = text4.Substring(text4.IndexOf("=") + 1);
				}
				else
				{
					text6 = text4;
				}
				if (text5 != "")
				{
					if (HttpRequest.ArgForm.ContainsKey(text6))
					{
						HttpRequest.ArgForm[text6] = text5;
					}
					else
					{
						HttpRequest.ArgForm.Add(text6, text5);
					}
				}
				else if (text5 == "")
				{
					if (HttpRequest.ArgForm.ContainsKey(text6))
					{
						HttpRequest.ArgForm[text6] = "";
					}
					else
					{
						HttpRequest.ArgForm.Add(text6, "");
					}
				}
			}
		}
		if (HttpRequest.Headers.ContainsKey("Content-Length") && int.Parse(HttpRequest.Headers["Content-Length"].Source) > HttpRequest.Source.Length - HttpRequest._HeadersTail - 4)
		{
			int num = int.Parse(HttpRequest.Headers["Content-Length"].Source) - HttpRequest.Source.Length + HttpRequest._HeadersTail + 4;
			byte[] Buffer = new byte[num];
			int len = HttpRequest.Source.Length;
			int num2;
			for (int RezLen = len + num; len < RezLen; len += num2)
			{
				try
				{
					num2 = await ClientStream.ReadAsync(Buffer, 0, Buffer.Length);
				}
				catch
				{
					num2 = 0;
				}
				Array.Resize(ref HttpRequest.Source, len + num2);
				Array.Copy(Buffer, 0, HttpRequest.Source, len, num2);
			}
		}
		return HttpRequest;
	}

	public byte[] GetBody()
	{
		if (_HeadersTail == -1)
		{
			return null;
		}
		byte[] array = new byte[Source.Length - _HeadersTail - 4];
		Buffer.BlockCopy(Source, _HeadersTail + 4, array, 0, array.Length);
		if (Headers != null && Headers.ContainsKey("Content-Encoding") && Headers["Content-Encoding"].Source.ToLower() == "gzip")
		{
			GZipStream gZipStream = new GZipStream(new MemoryStream(array), CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			byte[] array2 = new byte[512];
			int num = 0;
			while ((num = gZipStream.Read(array2, 0, array2.Length)) > 0)
			{
				memoryStream.Write(array2, 0, num);
			}
			array = memoryStream.ToArray();
		}
		return array;
	}

	public string GetBodyAsString()
	{
		Encoding encoding = Encoding.UTF8;
		if (Headers != null && Headers.ContainsKey("Content-Type") && !string.IsNullOrEmpty(((ItemContentType)Headers["Content-Type"]).Charset))
		{
			try
			{
				encoding = Encoding.GetEncoding(((ItemContentType)Headers["Content-Type"]).Charset);
			}
			catch
			{
			}
		}
		byte[] body = GetBody();
		string text = ((body[0] == 239 && body[1] == 187 && body[2] == 191) ? encoding.GetString(body, 3, body.Length - 3) : ((body[0] == 254 && body[1] == byte.MaxValue) ? encoding.GetString(body, 2, body.Length - 2) : ((body[0] == 0 && body[1] == 0 && body[2] == 254 && body[3] == byte.MaxValue) ? encoding.GetString(body, 4, body.Length - 4) : ((body[0] != byte.MaxValue || body[1] != 254 || body[2] != 0 || body[3] != 15) ? encoding.GetString(body) : encoding.GetString(body, 4, body.Length - 4)))));
		if (text[0] == '\uefff')
		{
			text = text.Substring(1);
		}
		while (text.Length > 0 && text[0] < ' ')
		{
			text = text.Substring(1);
		}
		return text;
	}

	public HttpResponse CreateResponse()
	{
		return new HttpResponse(this);
	}

	public ItemBase HeadersGetValues(string Key)
	{
		foreach (KeyValuePair<string, ItemBase> header in Headers)
		{
			if (header.Key.ToLower() == Key.ToLower())
			{
				return header.Value;
			}
		}
		return null;
	}
}
