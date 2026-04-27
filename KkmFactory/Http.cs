using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace KkmFactory;

public static class Http
{
	public static class FactoryHttpClients
	{
		public class CachedHttpClient
		{
			public string UrlServer;

			public X509Certificate ClientCertificate;

			public Dictionary<string, string> ValidationRule;

			public HttpClient HttpClient;

			public DateTime Expired = DateTime.Now.AddMinutes(30.0);

			public bool CertificateValidationCallback(HttpRequestMessage Request, X509Certificate2 Certificate, X509Chain Chain, SslPolicyErrors SslPolicyErrors)
			{
				if (ValidationRule != null)
				{
					foreach (KeyValuePair<string, string> item in ValidationRule)
					{
						if (!(item.Key == "cn") || Certificate == null || !Certificate.Subject.ToLower().Contains(item.Value.ToLower()))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		private const int ExpiredTime = 30;

		private static List<CachedHttpClient> ListClient = new List<CachedHttpClient>();

		public static CachedHttpClient GetCachedHttpClient(string UrlServer, X509Certificate ClientCertificate, Dictionary<string, string> ValidationRule)
		{
			return ListClient.Find((CachedHttpClient i) => i.UrlServer == UrlServer && i.ClientCertificate == ClientCertificate && i.ValidationRule == ValidationRule);
		}

		public static CachedHttpClient GetCachedHttpClient(HttpClient HttpClient)
		{
			return ListClient.Find((CachedHttpClient i) => i.HttpClient == HttpClient);
		}

		public static HttpClient HttpClient(string UrlServer, X509Certificate ClientCertificate, Dictionary<string, string> ValidationRule)
		{
			CachedHttpClient cachedHttpClient = GetCachedHttpClient(UrlServer, ClientCertificate, ValidationRule);
			if (cachedHttpClient == null)
			{
				cachedHttpClient = new CachedHttpClient();
				cachedHttpClient.UrlServer = UrlServer;
				cachedHttpClient.ClientCertificate = ClientCertificate;
				cachedHttpClient.ValidationRule = ValidationRule;
				ListClient.Add(cachedHttpClient);
			}
			if (cachedHttpClient.HttpClient != null && cachedHttpClient.Expired < DateTime.Now)
			{
				cachedHttpClient.HttpClient.Dispose();
				cachedHttpClient.HttpClient = null;
			}
			if (cachedHttpClient.HttpClient == null)
			{
				HttpClientHandler httpClientHandler = new HttpClientHandler();
				httpClientHandler.AllowAutoRedirect = false;
				httpClientHandler.MaxConnectionsPerServer = 20;
				httpClientHandler.ServerCertificateCustomValidationCallback = cachedHttpClient.CertificateValidationCallback;
				if (ClientCertificate != null)
				{
					httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
					httpClientHandler.ClientCertificates.Add(ClientCertificate);
					httpClientHandler.CheckCertificateRevocationList = false;
					httpClientHandler.PreAuthenticate = true;
				}
				cachedHttpClient.HttpClient = new HttpClient(httpClientHandler);
				cachedHttpClient.Expired = DateTime.Now.AddMinutes(30.0);
			}
			return cachedHttpClient.HttpClient;
		}
	}

	public class HttpRezult
	{
		public string URL;

		public string Request;

		public string Response;

		public object Rezult;

		public TimeSpan Time;

		public HttpStatusCode StatusCode = HttpStatusCode.Forbidden;

		public string Error;
	}

	public static async Task<HttpRezult> HttpReqestAsync(HttpMethod TypeQuery, int TimeOut, string UrlServer, string UrlQuery = null, Dictionary<string, string> Params = null, Dictionary<string, string> Heads = null, object Body = null, Type TypeRezult = null, Dictionary<string, string> ValidationRule = null, X509Certificate ClientCertificate = null)
	{
		bool flag = ((UrlQuery.Length <= 0) ? (UrlServer.Substring(UrlServer.Length - 1) == "/") : (UrlQuery.Substring(UrlQuery.Length - 1) == "/"));
		Uri uri = new Uri(UrlServer.TrimEnd('/') + "/" + UrlQuery.TrimStart('/'));
		UrlServer = uri.Scheme + "://" + uri.Host + ((uri.Port == 80) ? "" : (":" + uri.Port));
		UrlQuery = uri.PathAndQuery;
		if (!flag)
		{
			UrlQuery = UrlQuery.TrimEnd('/');
		}
		HttpRezult Rezult = new HttpRezult();
		StringBuilder stringBuilder = new StringBuilder(UrlServer.TrimEnd('/'));
		if (UrlQuery != null)
		{
			stringBuilder.Append('/');
			stringBuilder.Append(UrlQuery.TrimStart('/'));
		}
		bool flag2 = false;
		if (Params != null && Params.Count > 0)
		{
			stringBuilder.Append('?');
			foreach (KeyValuePair<string, string> Param in Params)
			{
				if (flag2)
				{
					stringBuilder.Append('&');
				}
				stringBuilder.Append(HttpUtility.UrlEncode(Param.Key));
				stringBuilder.Append('=');
				stringBuilder.Append(HttpUtility.UrlEncode(Param.Value));
				flag2 = true;
			}
		}
		string URL = (Rezult.URL = stringBuilder.ToString());
		HttpClient httpClient = FactoryHttpClients.HttpClient(UrlServer, ClientCertificate, ValidationRule);
		HttpRequestMessage httpRequestMessage = null;
		if (TypeQuery == HttpMethod.Post)
		{
			httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, URL);
		}
		else if (TypeQuery == HttpMethod.Get)
		{
			httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, URL);
		}
		else if (TypeQuery == HttpMethod.Put)
		{
			httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, URL);
		}
		httpRequestMessage.Properties["RequestTimeout"] = TimeSpan.FromMilliseconds(TimeOut);
		string text = null;
		if (Heads != null)
		{
			foreach (KeyValuePair<string, string> Head in Heads)
			{
				if (Head.Key == "Content-Type")
				{
					text = Head.Value;
					httpRequestMessage.Headers.TryAddWithoutValidation(Head.Key, Head.Value);
				}
				else
				{
					httpRequestMessage.Headers.TryAddWithoutValidation(Head.Key, Head.Value);
				}
			}
		}
		HttpContent httpContent = null;
		if (TypeQuery == HttpMethod.Post || TypeQuery == HttpMethod.Put)
		{
			if (Body is string)
			{
				httpContent = new StringContent(Body as string, Encoding.UTF8);
				if (text != null)
				{
					try
					{
						httpContent.Headers.ContentType = new MediaTypeHeaderValue(text);
					}
					catch
					{
						httpContent.Headers.TryAddWithoutValidation("Content-Type", text);
					}
				}
				Rezult.Request = Body as string;
			}
			else if (Heads.ContainsKey("Content-Type") && Heads["Content-Type"] == "application/json")
			{
				JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
				jsonSerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";
				string text2 = JsonConvert.SerializeObject(Body, jsonSerializerSettings);
				httpContent = new StringContent(text2, Encoding.UTF8);
				if (text != null)
				{
					httpContent.Headers.ContentType = new MediaTypeHeaderValue(text);
				}
				Rezult.Request = text2;
			}
			else if (Body is byte[])
			{
				httpContent = new ByteArrayContent(Body as byte[]);
				try
				{
					httpContent.Headers.ContentType = new MediaTypeHeaderValue(text);
				}
				catch
				{
					httpContent.Headers.TryAddWithoutValidation("Content-Type", text);
				}
			}
		}
		httpRequestMessage.Content = httpContent;
		HttpResponseMessage ResponseT = null;
		string ResponseC = null;
		DateTime TimeStart = DateTime.Now;
		try
		{
			ResponseT = await httpClient.SendAsync(httpRequestMessage);
			if (TypeQuery == HttpMethod.Post || TypeQuery == HttpMethod.Get)
			{
				ResponseC = await ResponseT.Content.ReadAsStringAsync();
			}
			if (TypeQuery == HttpMethod.Put)
			{
				try
				{
					ResponseC = await ResponseT.Content.ReadAsStringAsync();
				}
				catch (Exception)
				{
				}
			}
		}
		catch (AggregateException)
		{
			string ResponseS = "Сервер не отвечает (таймаут)";
			Rezult.Error = "Ошибка вызова сервера, URL = " + URL + ((ResponseS != null) ? (", Ошибка: " + ResponseS) : "");
			Rezult.Time = DateTime.Now - TimeStart;
			return Rezult;
		}
		catch (Exception ex3)
		{
			string ResponseS = Global.GetInnerErrorMessagee(ex3.InnerException);
			try
			{
				ResponseS = ResponseS + "<br/>" + await ResponseT.Content.ReadAsStringAsync();
			}
			catch
			{
			}
			Rezult.Error = "Ошибка вызова сервера, URL = " + URL + ", Ошибка: " + ResponseS;
			Rezult.Time = DateTime.Now - TimeStart;
			return Rezult;
		}
		Rezult.Time = DateTime.Now - TimeStart;
		Rezult.StatusCode = ResponseT.StatusCode;
		if (ResponseT.StatusCode != HttpStatusCode.OK)
		{
			string text3 = ResponseT.StatusCode.ToString();
			Rezult.Error = "Ошибка вызова сервера: " + (int)ResponseT.StatusCode + "-" + text3 + ", URL: " + URL;
		}
		if (ResponseC != null && ResponseC != null && ResponseC != "")
		{
			string ResponseS = (Rezult.Response = ResponseC);
			if (TypeRezult != null && Heads.ContainsKey("Content-Type") && Heads["Content-Type"] == "application/json")
			{
				try
				{
					Rezult.Rezult = JsonConvert.DeserializeObject(ResponseS, TypeRezult, new JsonSerializerSettings
					{
						StringEscapeHandling = StringEscapeHandling.EscapeHtml
					});
				}
				catch (Exception ex4)
				{
					if (ResponseT.StatusCode == HttpStatusCode.OK)
					{
						Rezult.Error = "Формат ответа не соответствует объекту: " + ex4.Message;
						Rezult.StatusCode = HttpStatusCode.Forbidden;
					}
				}
			}
			else
			{
				Rezult.Rezult = ResponseS;
			}
		}
		else if (TypeQuery == HttpMethod.Put)
		{
			Rezult.Rezult = true;
		}
		return Rezult;
	}

	public static HttpRezult HttpReqest(HttpMethod TypeQuery, int TimeOut, string UrlServer, string UrlQuery = null, Dictionary<string, string> Params = null, Dictionary<string, string> Heads = null, object Body = null, Type TypeRezult = null, Dictionary<string, string> ValidationRule = null, X509Certificate ClientCertificate = null)
	{
		Task<HttpRezult> task = HttpReqestAsync(TypeQuery, TimeOut, UrlServer, UrlQuery, Params, Heads, Body, TypeRezult, ValidationRule, ClientCertificate);
		task.Wait();
		return task.Result;
	}
}
