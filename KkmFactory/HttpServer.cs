using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class HttpServer
{
	public TcpListener myTCP;

	private bool FlagRun;

	private Task TaskExecuteRequest1;

	private Task TaskExecuteRequest2;

	private Task TaskExecuteRequest3;

	private Task TaskExecuteRequest4;

	private string Url = "";

	public static X509Certificate2 ServerSertificate;

	public static X509Certificate2 DefServerSertificate;

	public bool Relogin;

	public static string GetNameCert(string IdCert, string Name)
	{
		string text = IdCert;
		if (text.IndexOf(Name) != -1)
		{
			text = text.Substring(text.IndexOf(Name) + 3);
			if (text.IndexOf(",") != -1)
			{
				text = text.Substring(0, text.IndexOf(","));
			}
		}
		return text;
	}

	private static bool App_CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		switch (sslPolicyErrors)
		{
		case SslPolicyErrors.None:
			return true;
		case SslPolicyErrors.RemoteCertificateChainErrors:
			return true;
		default:
			_ = 1;
			return true;
		}
	}

	private static X509Certificate App_LocalCertificateValidation(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
	{
		if (ServerSertificate != null)
		{
			return ServerSertificate;
		}
		return DefServerSertificate;
	}

	public HttpServer()
	{
		Url = "http://localhost:" + Global.Settings.ipPort;
		Global.RunIsSllMode = false;
		if (ServerSertificate != null)
		{
			ServerSertificate.Dispose();
			ServerSertificate = null;
		}
		if (DefServerSertificate != null)
		{
			DefServerSertificate.Dispose();
			DefServerSertificate = null;
		}
		if (Global.Settings.ServerSertificate != "")
		{
			try
			{
				ServerSertificate = UtilSertificate.GetCertFromStoreS(Global.Settings.ServerSertificate);
				if (ServerSertificate == null)
				{
					Global.WriteError("Not found server certificate: " + Global.Settings.ServerSertificate);
				}
			}
			catch (Exception ex)
			{
				Global.WriteError("Error server certificate: " + ex.Message);
				ServerSertificate = null;
			}
		}
		if (ServerSertificate != null)
		{
			if (Global.GetNameCert(ServerSertificate.Subject, "CN=") != "")
			{
				Url = HttpService.GetUrl(null, "", "", "", GetHostFromSertificate: true, NotServerURL: false);
			}
			else
			{
				Url = "https://localhost:" + Global.Settings.ipPort;
			}
			Global.RunIsSllMode = true;
		}
		Global.UriProgram = Url;
	}

	public async void Start()
	{
		try
		{
			FlagRun = true;
			TaskExecuteRequest3 = Task.Run(async () => RunExecuteRequest(IPAddress.IPv6Any, Global.Settings.ipPort + 1, false, "Auto"));
			TaskExecuteRequest1 = Task.Run(async () => RunExecuteRequest(IPAddress.IPv6Any, Global.Settings.ipPort, null, Global.Settings.SSLProtocol));
			try
			{
				if (AddIn.TypeAddIn == AddIn.enTypeAddIn.None)
				{
					Global.WriteLine("Run http server as: " + Url, 0, Clear: false, true);
				}
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			await Global.Logers.AddError("Start HTTP server", Global.GetErrorMessagee(ex));
			Global.IsRun = false;
			return;
		}
		Global.IsRun = true;
		if (Global.IsRun && Global.Settings.ipPort != Global.Settings.ipPortOld)
		{
			Global.Settings.ipPortOld = Global.Settings.ipPort;
			await Global.SaveSettingsAsync();
		}
	}

	public void Stop()
	{
		FlagRun = false;
		if (TaskExecuteRequest1 != null)
		{
			TaskExecuteRequest1 = null;
		}
		if (TaskExecuteRequest2 != null)
		{
			TaskExecuteRequest2 = null;
		}
		if (TaskExecuteRequest3 != null)
		{
			TaskExecuteRequest3 = null;
		}
		if (TaskExecuteRequest4 != null)
		{
			TaskExecuteRequest4 = null;
		}
		Global.IsRun = false;
	}

	public async Task RunExecuteRequest(IPAddress IPAddress, int Port, bool? FixHTTPS, string SSL)
	{
		TcpListener Listener = null;
		for (int i = 0; i < 240; i++)
		{
			try
			{
				if (!FlagRun)
				{
					break;
				}
				IPEndPoint[] activeTcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
				bool flag = false;
				IPEndPoint[] array = activeTcpListeners;
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].Port == Port)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Thread.Sleep(1000);
					continue;
				}
				Listener = new TcpListener(IPAddress, Port);
				if (IPAddress == IPAddress.IPv6Any)
				{
					Listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
					Listener.Server.DualMode = true;
				}
				Listener.Start();
			}
			catch
			{
				Thread.Sleep(1000);
				continue;
			}
			break;
		}
		if (!FlagRun || Listener == null)
		{
			return;
		}
		while (FlagRun)
		{
			if (Listener.Pending())
			{
				if (Global.AllCancellationToken.Token.IsCancellationRequested)
				{
					break;
				}
				TcpClient myClient = await Listener.AcceptTcpClientAsync(Global.AllCancellationToken.Token);
				new Task(async delegate
				{
					await ExecuteRequest(myClient, FixHTTPS, SSL);
					myClient.Dispose();
				}).Start();
			}
			else
			{
				Thread.Sleep(200);
			}
		}
		Listener.Stop();
		Listener.Dispose();
	}

	private static async Task ExecuteRequest(TcpClient myClient, bool? FixHTTPS, string SSL)
	{
		Controllers Controllers = new Controllers();
		X509Certificate2 x509Certificate = ServerSertificate;
		if (FixHTTPS == false)
		{
			x509Certificate = null;
		}
		else if (FixHTTPS == true)
		{
			if (ServerSertificate != null)
			{
				x509Certificate = ServerSertificate;
			}
			else
			{
				if (DefServerSertificate != null)
				{
					DefServerSertificate.Dispose();
				}
				if (ServerSertificate != null)
				{
					DefServerSertificate = UtilSertificate.GetCertFromStoreS(ServerSertificate.Thumbprint);
				}
				else if (ServerSertificate == null && Global.Settings.ServerSertificate == "")
				{
					DefServerSertificate = UtilSertificate.GetCertFromStoreS(Global.Settings.ServerSertificate);
					if (DefServerSertificate == null)
					{
						return;
					}
				}
				x509Certificate = DefServerSertificate;
			}
		}
		if (myClient == null)
		{
			throw new Exception("Error myClient == null");
		}
		NetworkStream stream;
		try
		{
			stream = myClient.GetStream();
		}
		catch (Exception)
		{
			throw new Exception("Error myClient.GetStream()");
		}
		Stream ClientStream;
		if (x509Certificate == null)
		{
			ClientStream = stream;
		}
		else
		{
			try
			{
				SslStream SslStream = new SslStream(stream, false, App_CertificateValidation, App_LocalCertificateValidation);
				SslProtocols sslProtocols = SslProtocols.None;
				if (SSL == null)
				{
					sslProtocols = SslProtocols.None;
				}
				else
				{
					switch (SSL)
					{
					case "Auto":
						sslProtocols = SslProtocols.None;
						break;
					case "Old":
						sslProtocols = SslProtocols.Default;
						break;
					case "TLS13":
						sslProtocols = SslProtocols.Tls13;
						break;
					case "TLS12":
						sslProtocols = SslProtocols.Tls12;
						break;
					case "TLS11":
						sslProtocols = SslProtocols.Tls11;
						break;
					case "TLS10":
						sslProtocols = SslProtocols.Tls;
						break;
					case "SSLl3":
						sslProtocols = SslProtocols.Ssl3;
						break;
					case "SSLl2":
						sslProtocols = SslProtocols.Ssl2;
						break;
					}
				}
				await SslStream.AuthenticateAsServerAsync(x509Certificate, false, sslProtocols, false);
				ClientStream = SslStream;
			}
			catch (Exception ex2)
			{
				try
				{
					_ = ((ExternalException)ex2).ErrorCode;
					_ = -2147467259;
				}
				catch
				{
				}
				return;
			}
		}
		ClientStream.ReadTimeout = 1000;
		while (myClient.Connected)
		{
			byte[] BufferRead = new byte[0];
			int len = 0;
			byte[] Buffer = new byte[40000];
			int num = 0;
			while (len < 5 || num == Buffer.Length)
			{
				try
				{
					num = await ClientStream.ReadAsync(Buffer, 0, Buffer.Length);
				}
				catch
				{
					num = 0;
				}
				Array.Resize(ref BufferRead, len + num);
				Array.Copy(Buffer, 0, BufferRead, len, num);
				len += num;
				if (num == 0)
				{
					break;
				}
			}
			if (BufferRead.Length == 0)
			{
				break;
			}
			HttpRequest HttpRequest = null;
			HttpResponse HttpResponse = null;
			try
			{
				HttpRequest = await HttpRequest.CreateAsync(BufferRead, ClientStream, myClient);
				if (HttpRequest != null && HttpRequest.Headers != null)
				{
					HttpResponse = await Controllers.Work(HttpRequest);
					byte[] bytes = HttpResponse.GetBytes();
					ClientStream.Write(bytes, 0, bytes.Length);
					ClientStream.Flush();
				}
			}
			catch (Exception ex3)
			{
				if (Global.Settings.RegisterAllCommand)
				{
					string textCommand = ((HttpRequest != null && BufferRead.Length <= HttpRequest.Source.Length) ? Encoding.UTF8.GetString(HttpRequest.Source) : Encoding.UTF8.GetString(BufferRead));
					string netLogs = ((HttpResponse == null) ? "Нет HTTP ответа" : Encoding.UTF8.GetString(HttpResponse.Source));
					if (Global.Settings.RegisterAllCommand)
					{
						await Global.Logers.AddError("HTTP", ex3.Message, textCommand, netLogs);
					}
				}
				try
				{
					HttpResponse = new HttpResponse(HttpRequest, HttpStatusCode.ExpectationFailed, ex3.Message);
					byte[] bytes2 = HttpResponse.GetBytes();
					await ClientStream.WriteAsync(bytes2, 0, bytes2.Length);
					await ClientStream.FlushAsync();
				}
				catch
				{
				}
			}
			finally
			{
			}
		}
		myClient.Close();
	}
}
