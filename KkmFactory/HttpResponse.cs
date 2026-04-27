using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class HttpResponse
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

		public ItemHost(string Host, int Port)
			: base("")
		{
			this.Host = Host;
			this.Port = Port;
		}
	}

	public class ItemContentType : ItemBase
	{
		public string Value { get; set; }

		public string Charset { get; set; }

		public ItemContentType(string Value, string Charset = "utf-8")
			: base("")
		{
			this.Value = Value;
			this.Charset = Charset;
			base.Source = Value;
			if (Charset != "")
			{
				base.Source = base.Source + "; charset=" + Charset;
			}
		}
	}

	public byte[] Source = new byte[0];

	public string HTTPVersion { get; set; }

	public string Method { get; set; }

	public string Path { get; set; }

	public int StatusCode { get; set; }

	public string StatusMessage { get; set; }

	public Dictionary<string, ItemBase> Headers { get; set; }

	public List<string> SetCookies { get; set; }

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

	public HttpResponse(HttpRequest Request)
	{
		Init(Request);
	}

	public HttpResponse(HttpRequest Request, HttpStatusCode HttpStatusCode, string Content, string ContentType = "text/html")
	{
		Init(Request);
		SetStatusCode(HttpStatusCode);
		HeadersAdd("content-type", new ItemContentType(ContentType));
		HeadersAdd("Cache-Control", new ItemBase("no-store, no-cache, must-revalidate"));
		SetBody(Content);
	}

	public HttpResponse(HttpRequest Request, HttpStatusCode HttpStatusCode, byte[] Content, string ContentType = "")
	{
		Init(Request);
		SetStatusCode(HttpStatusCode);
		if (ContentType != null)
		{
			HeadersAdd("content-type", new ItemContentType(ContentType));
		}
		SetBody(Content);
	}

	public HttpResponse(HttpRequest Request, HttpStatusCode HttpStatusCode, object Content, string ContentType = "application/json")
	{
		Init(Request);
		SetStatusCode(HttpStatusCode);
		HeadersAdd("content-type", new ItemContentType(ContentType));
		HeadersAdd("Cache-Control", new ItemBase("no-store, no-cache, must-revalidate"));
		SetBody(Content);
	}

	public void Init(HttpRequest HttpRequest)
	{
		StatusCode = 200;
		StatusMessage = "OK";
		Source = new byte[0];
		Headers = new Dictionary<string, ItemBase>(StringComparer.CurrentCultureIgnoreCase);
		SetCookies = new List<string>();
		HeadersAdd("Access-Control-Allow-Origin", new ItemBase("*"));
		if (HttpRequest == null)
		{
			HTTPVersion = "1.1";
			Method = "GET";
			Path = "/";
			return;
		}
		HTTPVersion = HttpRequest.HTTPVersion;
		Method = HttpRequest.Method;
		Path = HttpRequest.Path;
		foreach (KeyValuePair<string, HttpRequest.ItemBase> header in HttpRequest.Headers)
		{
			if (header.Key == "content-type")
			{
				HeadersAdd(header.Key, new ItemContentType(((HttpRequest.ItemContentType)header.Value).Value, ((HttpRequest.ItemContentType)header.Value).Charset));
			}
		}
	}

	public void SetStatusCode(HttpStatusCode status)
	{
		StatusCode = (int)status;
		StatusMessage = StatusCode + " " + Enum.GetName(typeof(HttpStatusCode), StatusCode);
	}

	public Encoding GetEncoding()
	{
		Encoding result = Encoding.UTF8;
		foreach (KeyValuePair<string, ItemBase> header in Headers)
		{
			if (header.Key.ToLower() == "content-type" && !string.IsNullOrEmpty(((ItemContentType)header.Value).Charset))
			{
				try
				{
					result = Encoding.GetEncoding(((ItemContentType)header.Value).Charset);
				}
				catch
				{
				}
			}
		}
		return result;
	}

	public void HeadersAdd(string Key, ItemBase Item)
	{
		if (Headers.ContainsKey(Key.Trim()))
		{
			Headers[Key] = Item;
		}
		else
		{
			Headers.Add(Key, Item);
		}
	}

	public void SetBody(string newBody)
	{
		Encoding encoding = GetEncoding();
		Source = encoding.GetBytes(newBody);
	}

	public void SetBody(byte[] newBody)
	{
		Source = newBody;
	}

	public void SetBody(object newBody)
	{
		Encoding encoding = GetEncoding();
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
		string text = "  " + JsonConvert.SerializeObject(newBody, jsonSerializerSettings);
		Source = encoding.GetBytes(text);
	}

	public byte[] GetBytes()
	{
		HeadersAdd("content-length", new ItemBase(Source.Length.ToString()));
		Encoding encoding = GetEncoding();
		string text = $"HTTP/{HTTPVersion} {StatusCode} {StatusMessage}";
		foreach (string key in Headers.Keys)
		{
			ItemBase itemBase = Headers[key];
			if (!string.IsNullOrEmpty(text))
			{
				text += "\r\n";
			}
			text += $"{key}: {itemBase.Source}";
		}
		foreach (string setCookie in SetCookies)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\r\n";
			}
			text += string.Format("{0}: {1}", "Set-Cookie", setCookie);
		}
		text += "\r\n\r\n";
		byte[] array = encoding.GetBytes(text);
		int destinationIndex = array.Length;
		Array.Resize(ref array, array.Length + Source.Length);
		Array.Copy(Source, 0, array, destinationIndex, Source.Length);
		return array;
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

	public void AddSetCookies(CookieHeaderValue Cookie)
	{
		SetCookies.Add(Cookie.ToString());
	}
}
