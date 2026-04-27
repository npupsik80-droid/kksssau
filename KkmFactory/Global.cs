using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using KkmFactory.ViewModels;
using KkmFactory.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public static class Global
{
	[DataContract]
	public class DeviceSettings
	{
		[DataMember]
		public int NumDevice = -1;

		[DataMember]
		public string IdDevice = Guid.NewGuid().ToString();

		[DataMember]
		public bool Active = true;

		[DataMember]
		public string IdTypeDevice = "";

		public TypeDevice TypeDevice;

		[DataMember]
		public Dictionary<string, string> Paramets = new Dictionary<string, string>();

		[DataMember]
		public DateTime AddDate;

		[DataMember]
		public string INN = "<Не определено>";

		[DataMember]
		public string NumberKkm = "<Не определено>";

		[DataMember]
		public string TaxVariant = "";
	}

	[DataContract]
	public class PermitRegimSetting
	{
		[DataMember]
		[DefaultValue(null)]
		public string Inn;

		[DataMember]
		[DefaultValue(null)]
		public string Token;

		[DataMember]
		[DefaultValue(null)]
		public string CertificateId;

		[DataMember]
		[DefaultValue(null)]
		public string CertificatePass;

		[DataMember]
		public List<int> GroupIds = new List<int>();
	}

	[DataContract]
	public class CdnPlatform
	{
		[DataMember]
		[DefaultValue(null)]
		public string Host;

		[DataMember]
		[DefaultValue(999999)]
		public TimeSpan PingTime = new TimeSpan(24, 0, 0);

		public int CoutUssage;

		public DateTime LastUssage;

		public DateTime? Blocked;
	}

	[DataContract]
	public class SetCallback : MySemaphore
	{
		[DataMember]
		public string URL = "";

		[DataMember]
		public string Login = "";

		[DataMember]
		public string Password = "";

		[DataMember]
		public string Token = "";
	}

	[DataContract]
	public class SettingsServer
	{
		[DataMember]
		public int ipPort = 5893;

		[DataMember]
		public int ipPortOld = 5893;

		[DataMember]
		public bool StatisticsСollection;

		[DataMember]
		public bool OldSSLProtocol;

		[DataMember]
		public string SSLProtocol = "";

		[DataMember]
		public string IdSSLProtocol = "";

		[DataMember]
		public string PBKDFprotocol = "PBKDF1";

		[DataMember]
		public string LoginAdmin = "Admin";

		[DataMember]
		public string PassAdmin = "";

		[DataMember(Name = "LoginUser")]
		public string LoginUser = "User";

		[DataMember]
		public string PassUser = "";

		[DataMember]
		public string ServerSertificate = "";

		[DataMember]
		public string UserRootSertificate = "";

		[DataMember]
		public string LicenseEmail = "";

		[DataMember]
		public string LicensePass = "";

		[DataMember]
		public string TypeRun = "Windows";

		[DataMember]
		public string SerialNumber = "";

		[DataMember]
		public DateTime OldRun = DateTime.MinValue;

		[DataMember]
		public SortedLisSem<int, DeviceSettings> Devices = new SortedLisSem<int, DeviceSettings>();

		[DataMember]
		public string Verson = "";

		[DataMember]
		public DateTime? DateInstall;

		[DataMember]
		public DateTime? DateStat;

		[DataMember]
		public int CountLic;

		[DataMember]
		public bool RegisterAllCommand;

		[DataMember]
		public int RemoveCommandInterval = 30;

		[DataMember]
		public bool SetNotActiveOnError;

		[DataMember]
		public bool SetNotActiveOnPaperOver;

		[DataMember]
		public bool KkmIniter;

		[DataMember]
		public int KkmIniterInterval = 5;

		public int MaxThreads = 300;

		[DataMember]
		public bool RunCallback;

		[DataMember]
		public int TypeCallback;

		[DataMember]
		public int TimeCallback = 30;

		[DataMember]
		public bool RegisterCallback;

		[DataMember]
		public DictionarySem<string, SetCallback> ListCallback = new DictionarySem<string, SetCallback>();

		[DataMember]
		public bool RemoveSettingsFromAddIn;

		[DataMember]
		public bool RemoveSettingsFromYClients;

		[DataMember]
		public string Marke = "";

		[DataMember]
		public bool MarkingCodeAcceptOnBad;

		[DataMember]
		public string NewFirmwares = "";

		[DataMember]
		public List<PermitRegimSetting> PermitRegimSetting = new List<PermitRegimSetting>();

		[DataMember]
		public bool PermitRegimDebug;

		[DataMember]
		public DateTime? DateTimeCdnPlatformGet = default(DateTime);

		[DataMember]
		public List<CdnPlatform> ListCdnPlatform = new List<CdnPlatform>();

		[DataMember]
		public Dictionary<string, string> Store = new Dictionary<string, string>();

		[DataMember]
		public bool AlreadyLaunchedService;
	}

	public delegate void DelegateOpenWindow();

	public class TextLine
	{
		public string Text;

		public int Font;

		public string Image;

		public TextLine(string Text, int Font = 0)
		{
			this.Text = Text;
			this.Font = Font;
		}

		public TextLine(string Image)
		{
			this.Image = Image;
		}
	}

	[DataContract]
	public class BaseExecuteData
	{
		[DataMember]
		public int NumDevice;

		[DataMember]
		public string IdCommand = "";

		[DataMember]
		public TypeDevice.enType Type;

		[DataMember]
		public DateTime DateStart;

		[DataMember]
		public DateTime DateRun;

		[DataMember]
		public DateTime DateEnd;

		[DataMember]
		public Unit.RezultCommandKKm RezultCommandKKm;

		[DataMember]
		public Unit.RezultCommandCheck RezultCommandCheck;

		[DataMember]
		public Unit.RezultCommandProcessing RezultCommandProcessing;

		[DataMember]
		public string INN;

		[DataMember]
		public string KeyCallback = "";

		[DataMember]
		public bool ReturnedCallback;
	}

	[DataContract]
	public class CallbackCommand
	{
		[DataMember(Name = "ListCommand")]
		public Unit.DataCommand[] ListCommand = new Unit.DataCommand[10];
	}

	public const string UseYClients = "YClients";

	public const string UseGainUp = "GainUp";

	public static string NameRuFile;

	public static short MaxUnit;

	public static string NameProduct;

	public static string TypeProduct;

	public static string NameService;

	public static string Product;

	public static string Description;

	public static string Verson;

	public static string Developer;

	public static string Website;

	public static string Copyright;

	public static SemaphoreSlim Semaphore;

	public static SettingsServer Settings;

	public static UnitManager UnitManager;

	public static HttpServer HttpServer;

	public static Logers Logers;

	public static bool RunIsClientSertivicate;

	public static bool RunIsSllMode;

	public static X509Certificate2 ClientRootSertificate;

	public static string UriProgram;

	public static bool IsRun;

	public static DateTime DateStart;

	public static MainWindow MainForm;

	public static ViewModel ViewModel;

	public static bool NeedRun;

	public static string ErrorLicense;

	public static List<TextLine> TextLines;

	public static bool RunStopWriteLines;

	public static bool WriteLinesVisble;

	public static nint Info;

	public static bool SetShowWindow;

	public static bool RunAsAdmin;

	public static bool SettingsServerModified;

	public static CancellationTokenSource AllCancellationToken;

	public static Timer TimerWorkStackCommand;

	public static Timer TimerSaveLogs;

	public static Timer TimerTaskKkmIniter;

	public static Timer TimerOnStart;

	public static Timer TimerCheckHttp;

	private static bool TimerCheckHttpIsRun;

	public static Timer TimerTaskCallback;

	public static string CallbackErrorConnect;

	public static string CallbackErrorAut;

	public static string CallbackErrorReceive;

	public static string CallbackErrorSend;

	public static string CallbackError;

	private static Thread BigThread;

	public static bool ServerRunInAnotherWindow;

	public static int IsAdminSave;

	public static string ResultExecuteCommand;

	public static bool isRunFromModuleTest;

	public static int TimeZone;

	private static string BufLog;

	public static string ChromeAddIn;

	public static string ChromiumAddIn;

	public static string MozillaAddIn;

	public static string Mozilla64AddIn;

	public static string BashSettings;

	public static string PatchDeamon;

	public static string PatchPid;

	static Global()
	{
		NameRuFile = "KkmServer.exe";
		MaxUnit = 20;
		NameProduct = "KkmServer";
		TypeProduct = "KkmServer";
		NameService = "KkmServer";
		Product = "Kkm web-server";
		Description = "Kkm Web-сервер";
		Verson = "1.1.1.1";
		Developer = "Гарбуз Д.В.";
		Website = "kkmserver.ru";
		Copyright = "© Garbuz D.V. 2016";
		Semaphore = new SemaphoreSlim(1);
		Settings = new SettingsServer();
		RunIsClientSertivicate = false;
		RunIsSllMode = false;
		ClientRootSertificate = null;
		UriProgram = "";
		IsRun = false;
		MainForm = null;
		ViewModel = null;
		NeedRun = false;
		ErrorLicense = "";
		TextLines = new List<TextLine>();
		RunStopWriteLines = true;
		WriteLinesVisble = true;
		Info = 0;
		SetShowWindow = false;
		RunAsAdmin = false;
		SettingsServerModified = false;
		AllCancellationToken = null;
		TimerWorkStackCommand = null;
		TimerSaveLogs = null;
		TimerTaskKkmIniter = null;
		TimerOnStart = null;
		TimerCheckHttp = null;
		TimerCheckHttpIsRun = false;
		TimerTaskCallback = null;
		CallbackErrorConnect = "";
		CallbackErrorAut = "";
		CallbackErrorReceive = "";
		CallbackErrorSend = "";
		CallbackError = "";
		BigThread = null;
		ServerRunInAnotherWindow = false;
		IsAdminSave = -1;
		ResultExecuteCommand = "";
		isRunFromModuleTest = false;
		TimeZone = 0;
		BufLog = "";
		ChromeAddIn = "/etc/opt/chrome/native-messaging-hosts";
		ChromiumAddIn = "/etc/chromium/native-messaging-hosts";
		MozillaAddIn = "/usr/lib/mozilla/native-messaging-hosts";
		Mozilla64AddIn = "/usr/lib64/mozilla/native-messaging-hosts";
		BashSettings = "/etc/bash.bashrc";
		PatchDeamon = "/etc/systemd/system/kkmserver.service";
		PatchPid = "/var/run/kkmserver.pid";
		foreach (Attribute customAttribute in Assembly.GetExecutingAssembly().GetCustomAttributes())
		{
			if (customAttribute.GetType().Name == "AssemblyTitleAttribute")
			{
				NameService = ((AssemblyTitleAttribute)customAttribute).Title;
			}
			if (customAttribute.GetType().Name == "AssemblyFileVersionAttribute")
			{
				Verson = ((AssemblyFileVersionAttribute)customAttribute).Version;
			}
			if (customAttribute.GetType().Name == "AssemblyDescriptionAttribute")
			{
				Description = ((AssemblyDescriptionAttribute)customAttribute).Description;
			}
			if (customAttribute.GetType().Name == "AssemblyCompanyAttribute")
			{
				Developer = ((AssemblyCompanyAttribute)customAttribute).Company;
			}
			if (customAttribute.GetType().Name == "AssemblyProductAttribute")
			{
				Product = ((AssemblyProductAttribute)customAttribute).Product;
			}
			if (customAttribute.GetType().Name == "AssemblyCopyrightAttribute")
			{
				Copyright = ((AssemblyCopyrightAttribute)customAttribute).Copyright;
			}
		}
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		TimeZone = TimeZoneInfo.Local.BaseUtcOffset.Hours - 1;
		if (TimeZone <= 0)
		{
			TimeZone = 2;
		}
	}

	public static void StartServer(bool RestartHTTP = true)
	{
		AllCancellationToken = new CancellationTokenSource();
		Logers = new Logers();
		UnitManager = new UnitManager();
		LoadBase();
		HttpServer = new HttpServer();
		try
		{
			if (RestartHTTP)
			{
				HttpServer.Start();
			}
		}
		catch (Exception ex)
		{
			string errorMessagee = GetErrorMessagee(ex);
			Logers.AddError("Init HTTP server", errorMessagee).Wait();
			WriteError("Init HTTP server: " + errorMessagee);
		}
		if (!IsRun)
		{
			return;
		}
		UnitManager.InitUnitManager().Wait();
		ThreadPool.SetMinThreads(Math.Min(30, Settings.MaxThreads), Settings.MaxThreads);
		TimerWorkStackCommand = new Timer(async delegate(object? Param)
		{
			await UnitManager.ManagerCommand(Param);
		}, false, 100, 100);
		TimerSaveLogs = new Timer(async delegate(object? Param)
		{
			await AutoSaveSettings(Param);
		}, false, 60000, 600000);
		if (Settings.SetNotActiveOnError && Settings.KkmIniter)
		{
			TimerTaskKkmIniter = new Timer(async delegate(object? Param)
			{
				await RegTaskKkmIniter(Param);
			}, null, 110000, 60000 * Settings.KkmIniterInterval);
		}
		if (Settings.RunCallback)
		{
			TimerTaskCallback = new Timer(async delegate(object? Param)
			{
				await RegTaskCallback(Param);
			}, null, 5000, 1000 * Settings.TimeCallback);
		}
		TimerOnStart = new Timer(delegate
		{
			if (!AllCancellationToken.Token.IsCancellationRequested)
			{
				ComDevice.NewComDevice().Wait();
				TimerOnStart.Dispose();
				TimerOnStart = null;
			}
		}, null, 20000, 0);
	}

	public static void StopServer(bool RestartHTTP = true, bool FreeUnit = true, bool SaveLogs = true)
	{
		if (AllCancellationToken != null)
		{
			if (Settings.RegisterAllCommand)
			{
				Logers.AddError("TokenCancel", "").Wait();
			}
			AllCancellationToken.Cancel();
			if (UnitManager != null)
			{
				foreach (KeyValuePair<int, Unit> unit in UnitManager.Units)
				{
					if (unit.Value is UnitPort && ((UnitPort)unit.Value).IpCancellToken != null)
					{
						try
						{
							((UnitPort)unit.Value).IpCancellToken.Cancel();
						}
						catch
						{
						}
					}
				}
			}
		}
		if (TimerWorkStackCommand != null)
		{
			TimerWorkStackCommand.Dispose();
			TimerWorkStackCommand = null;
		}
		if (TimerSaveLogs != null)
		{
			TimerSaveLogs.Dispose();
			TimerSaveLogs = null;
		}
		if (TimerTaskKkmIniter != null)
		{
			TimerTaskKkmIniter.Dispose();
			TimerTaskKkmIniter = null;
		}
		if (TimerCheckHttp != null)
		{
			TimerCheckHttp.Dispose();
			TimerCheckHttp = null;
		}
		if (TimerTaskCallback != null)
		{
			TimerTaskCallback.Dispose();
			TimerTaskCallback = null;
		}
		if (RestartHTTP && HttpServer != null)
		{
			HttpServer.Stop();
			IsRun = false;
			HttpServer = null;
		}
		SaveBase();
		UnitManager.FreeUnit();
		UnitManager = null;
		if (SaveLogs && Logers != null)
		{
			Logers.SaveSettings();
			Logers = null;
		}
	}

	public static void HaltServer()
	{
		StopServer(RestartHTTP: false, FreeUnit: false, SaveLogs: false);
		Environment.Exit(0);
	}

	public static void RestartServer(bool RestartHTTP = true)
	{
		if (Settings.RegisterAllCommand)
		{
			Logers.AddError("RestartServer", "").Wait();
		}
		if (AddIn.TypeAddIn == AddIn.enTypeAddIn.None)
		{
			WriteLine("Restart server....", 0, Clear: false, WriteLinesVisble);
		}
		StopServer(RestartHTTP);
		Thread.Sleep(3000);
		StartServer(RestartHTTP);
		if (AddIn.TypeAddIn == AddIn.enTypeAddIn.None)
		{
			WriteLine("Server is run!", 0, Clear: false, WriteLinesVisble);
		}
	}

	public static string GerPahtSettings()
	{
		return Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Settings");
	}

	public static string GetPaht()
	{
		return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
	}

	public static string GetExePaht()
	{
		string text = "";
		if (string.Equals(Path.GetExtension(Process.GetCurrentProcess().MainModule.FileName), ".exe", StringComparison.OrdinalIgnoreCase))
		{
			return Process.GetCurrentProcess().MainModule.FileName;
		}
		if (File.Exists(Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".exe")))
		{
			return Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".exe");
		}
		if (File.Exists(Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, "")))
		{
			return Process.GetCurrentProcess().MainModule.FileName;
		}
		return Process.GetCurrentProcess().MainModule.FileName;
	}

	public static ProcessStartInfo GetStartInfo()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo
		{
			WorkingDirectory = GetPaht()
		};
		if (string.Equals(Path.GetExtension(Process.GetCurrentProcess().MainModule.FileName), ".exe", StringComparison.OrdinalIgnoreCase))
		{
			processStartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
		}
		else if (File.Exists(Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".exe")))
		{
			processStartInfo.FileName = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".exe");
		}
		else if (File.Exists(Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, "")))
		{
			processStartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
		}
		else
		{
			processStartInfo.FileName = "dotnet";
			processStartInfo.Arguments = Process.GetCurrentProcess().MainModule.FileName;
		}
		return processStartInfo;
	}

	public static async Task SaveSettingsAsync()
	{
		string text = GerPahtSettings();
		string fullPath = Path.Combine(text, "SettingsServ.ini");
		string fullPath2 = Path.Combine(text, "SettingsServ1.ini");
		if (File.Exists(fullPath2))
		{
			File.Delete(fullPath2);
		}
		bool num = await Semaphore.WaitAsync(10000000);
		bool flag = num;
		if (!num)
		{
			return;
		}
		Settings.Verson = Verson;
		string passAdmin = Settings.PassAdmin;
		string passUser = Settings.PassUser;
		string licensePass = Settings.LicensePass;
		try
		{
			Settings.PBKDFprotocol = "PBKDF2";
			Settings.PassAdmin = "թ" + Shifrovka(passAdmin, Settings.PBKDFprotocol);
			Settings.PassUser = "թ" + Shifrovka(passUser, Settings.PBKDFprotocol);
			Settings.LicensePass = "թ" + Shifrovka(licensePass, Settings.PBKDFprotocol);
			Settings.PermitRegimSetting = PermitRegim.SaveInList();
			string text2 = JsonConvert.SerializeObject(Settings, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml,
				Formatting = Formatting.Indented
			});
			if (text2.Contains(Settings.ipPort.ToString()))
			{
				File.WriteAllText(fullPath2, text2);
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
				if (File.Exists(fullPath2))
				{
					File.Move(fullPath2, fullPath);
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			Settings.PassAdmin = passAdmin;
			Settings.PassUser = passUser;
			Settings.LicensePass = licensePass;
		}
		if (flag)
		{
			Semaphore.Release();
		}
	}

	public static async Task LoadSettingAsyncs(bool InitLoad = false)
	{
		string text = GerPahtSettings();
		string fullPath = Path.Combine(text, "SettingsServ.ini");
		bool num = await Semaphore.WaitAsync(10000000);
		bool flag = num;
		if (!num)
		{
			return;
		}
		try
		{
			Settings = JsonConvert.DeserializeObject<SettingsServer>(File.ReadAllText(fullPath), new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			});
		}
		catch (Exception)
		{
		}
		_ = InitLoad;
		if (Settings.PassAdmin.Length > 0 && Settings.PassAdmin[0] == 'թ')
		{
			Settings.PassAdmin = DeShifrovka(Settings.PassAdmin.Substring(1), Settings.PBKDFprotocol);
		}
		if (Settings.PassUser.Length > 0 && Settings.PassUser[0] == 'թ')
		{
			Settings.PassUser = DeShifrovka(Settings.PassUser.Substring(1), Settings.PBKDFprotocol);
		}
		if (Settings.LicensePass.Length > 0 && Settings.LicensePass[0] == 'թ')
		{
			Settings.LicensePass = DeShifrovka(Settings.LicensePass.Substring(1), Settings.PBKDFprotocol);
		}
		if (Settings.SSLProtocol == "")
		{
			if (!Settings.OldSSLProtocol)
			{
				Settings.SSLProtocol = "Auto";
			}
			else
			{
				Settings.SSLProtocol = "Old";
			}
		}
		foreach (KeyValuePair<int, DeviceSettings> device in Settings.Devices)
		{
			device.Value.TypeDevice = null;
			foreach (KeyValuePair<string, TypeDevice> item in UnitManager.ListTypeDevice)
			{
				if (device.Value.IdTypeDevice == item.Value.Id)
				{
					device.Value.TypeDevice = item.Value;
				}
			}
		}
		do
		{
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, DeviceSettings> device2 in Settings.Devices)
			{
				if (device2.Value.TypeDevice == null)
				{
					list.Add(device2.Key);
				}
			}
			foreach (int item2 in list)
			{
				Settings.Devices.Remove(item2);
			}
		}
		while (false);
		PermitRegim.LoadFromList(Settings.PermitRegimSetting);
		if (flag)
		{
			Semaphore.Release();
		}
		string[] array = Settings.Verson.Split('.');
		if (!string.IsNullOrEmpty(array[0]) && int.Parse(array[0]) <= 2 && !string.IsNullOrEmpty(array[1]) && int.Parse(array[1]) <= 2)
		{
			MigrateFrom_2_2_To_2_3();
		}
		if (!Settings.DateInstall.HasValue)
		{
			Settings.DateInstall = DateTime.Now;
			await SaveSettingsAsync();
		}
	}

	public static void SaveBase()
	{
	}

	public static void LoadBase()
	{
	}

	public static async Task AutoSaveSettings(object state = null)
	{
		try
		{
			if (SettingsServerModified)
			{
				SettingsServerModified = false;
				await SaveSettingsAsync();
			}
		}
		catch
		{
		}
		Logers.SaveSettings(state);
	}

	public static void MigrateFrom_2_2_To_2_3()
	{
		if (!string.IsNullOrEmpty(Settings.ServerSertificate))
		{
			X509Certificate2 certFromStoreS = UtilSertificate.GetCertFromStoreS(null, Settings.ServerSertificate);
			if (certFromStoreS != null)
			{
				UtilSertificate.SetCertToStore(certFromStoreS, "", GetKey: true, (StoreLocation)0, StoreName.My);
				Settings.ServerSertificate = certFromStoreS.Thumbprint;
			}
			SaveSettingsAsync().Wait();
		}
	}

	public static async Task ChekHTTP(object state)
	{
		int TimerCheckHttpIsPing = 0;
		bool IsFirst = true;
		while (true)
		{
			try
			{
				if (TimerCheckHttpIsRun || !IsRun)
				{
					break;
				}
				TimerCheckHttpIsRun = true;
				bool ServerRun = false;
				try
				{
					TimerCheckHttpIsPing++;
					if (TimerCheckHttpIsPing > 2)
					{
						TimerCheckHttpIsPing = 0;
						string urlServer = "http://localhost:" + (Settings.ipPort + 1);
						Dictionary<string, string> dictionary = new Dictionary<string, string>();
						byte[] bytes = Encoding.ASCII.GetBytes(Settings.LoginAdmin.Trim() + ":" + Settings.PassAdmin.Trim());
						dictionary.Add("Authorization", "Basic " + Convert.ToBase64String(bytes));
						if ((await Http.HttpReqestAsync(HttpMethod.Get, 20, urlServer, "Settings", null, dictionary)).StatusCode == HttpStatusCode.OK)
						{
							ServerRun = true;
						}
					}
					else
					{
						ServerRun = true;
					}
				}
				catch (Exception ex)
				{
					Program.SaveGlobalText("Error ping server: " + ex.Message);
					if (Logers != null)
					{
						await Logers.AddError("Error ping server: ", ex.Message);
					}
				}
				if (!ServerRun)
				{
					Program.SaveGlobalText("HTTP doesn't answer - Restart HTTP server");
					if (Logers != null)
					{
						await Logers.AddError("HTTP doesn't answer - Restart HTTP server", "");
					}
				}
				if (!IsFirst && !Settings.AlreadyLaunchedService && Settings.TypeRun == "Service")
				{
					Settings.AlreadyLaunchedService = true;
					await SaveSettingsAsync();
					await (Logers?.AddError("Restart HTTP server on first start as service", ""));
					ServerRun = false;
				}
				if (!IsFirst && Logers == null)
				{
					Settings.AlreadyLaunchedService = true;
					await SaveSettingsAsync();
					Program.SaveGlobalText("Restart HTTP server on Global.Logers == null");
					if (Logers != null)
					{
						await Logers.AddError("Restart HTTP server on Global.Logers == null", "");
					}
					ServerRun = false;
				}
				if (!ServerRun)
				{
					RightsUp("RESTART_SERVICE", WaitForExit: false);
				}
				TimerCheckHttpIsRun = false;
				IsFirst = false;
				await Task.Delay(15000);
			}
			catch (Exception ex2)
			{
				Program.SaveGlobalException(ex2);
			}
		}
	}

	public static async Task RegTaskKkmIniter(object state)
	{
		foreach (KeyValuePair<int, Unit> gUnit in UnitManager.Units)
		{
			if (gUnit.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && gUnit.Value.Active && (!gUnit.Value.IsInit || gUnit.Value.IsFullInitDate < DateTime.Now.AddMinutes(-5.0)))
			{
				new Task(async delegate
				{
					Unit.DataCommand dataCommand = new Unit.DataCommand
					{
						Command = "InitDevice",
						NumDevice = gUnit.Key
					};
					await UnitManager.AddCommand(dataCommand, "", "");
				}, gUnit).Start();
			}
		}
	}

	public static async Task RegTaskCallback(object state)
	{
		if (Settings.TypeCallback == 0 || Settings.TypeCallback == 2)
		{
			await HTTPCallback(state);
		}
		else
		{
			if (Settings.TypeCallback != 3)
			{
				return;
			}
			foreach (KeyValuePair<string, SetCallback> item in Settings.ListCallback)
			{
				(new object[1])[0] = item.Value;
				await WebSocketsCallback(item.Value);
			}
		}
	}

	public static async Task HTTPCallback(object state)
	{
		if (!(await Settings.ListCallback.Semaphore.WaitAsync(40)))
		{
			return;
		}
		string SendJSON = "";
		string ReciveJSON = "";
		try
		{
			int num;
			_ = num - 1;
			_ = 3;
			try
			{
				CallbackErrorConnect = "";
				foreach (KeyValuePair<string, SetCallback> SettCallBack in Settings.ListCallback)
				{
					string json = "";
					try
					{
						List<UnitManager.ExecuteData> ListExecuteDatas = new List<UnitManager.ExecuteData>();
						try
						{
							UnitManager.ExecuteDatas.Semaphore.Wait();
							foreach (UnitManager.ExecuteData executeData in UnitManager.ExecuteDatas)
							{
								if (executeData.KeyCallback == SettCallBack.Key && !executeData.ReturnedCallback)
								{
									UnitManager.ExecuteData item = executeData;
									ListExecuteDatas.Add(item);
								}
							}
						}
						finally
						{
							UnitManager.ExecuteDatas.Semaphore.Release();
						}
						var anon = new
						{
							Command = "GetCommand",
							Token = SettCallBack.Value.Token,
							ListRezult = new Unit.RezultCommand[ListExecuteDatas.Count],
							LabelTime = DateTime.Now
						};
						int num2 = 0;
						foreach (UnitManager.ExecuteData item2 in ListExecuteDatas)
						{
							anon.ListRezult[num2++] = item2.RezultCommand;
						}
						JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
						jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
						string text = JsonConvert.SerializeObject(anon, jsonSerializerSettings);
						SendJSON = text;
						byte[] bytes = Encoding.UTF8.GetBytes(SettCallBack.Value.Login + ":" + SettCallBack.Value.Password);
						Dictionary<string, string> dictionary = new Dictionary<string, string>();
						dictionary.Add("Authorization", "Basic " + Convert.ToBase64String(bytes));
						dictionary.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36");
						dictionary.Add("Cache-Control", "no-store");
						dictionary.Add("Content-Type", "application/json");
						Http.HttpRezult httpRezult = null;
						if (Settings.TypeCallback == 0)
						{
							httpRezult = await Http.HttpReqestAsync(HttpMethod.Put, 200000, SettCallBack.Value.URL, "", null, dictionary, text, typeof(CallbackCommand));
						}
						else if (Settings.TypeCallback == 2)
						{
							httpRezult = await Http.HttpReqestAsync(HttpMethod.Post, 200000, SettCallBack.Value.URL, "", null, dictionary, text, typeof(CallbackCommand));
						}
						if (httpRezult == null || httpRezult.StatusCode != HttpStatusCode.OK || httpRezult.Rezult == null)
						{
							CallbackErrorConnect = httpRezult.Error;
							continue;
						}
						try
						{
							UnitManager.ExecuteDatas.Semaphore.Wait();
							foreach (UnitManager.ExecuteData item3 in ListExecuteDatas)
							{
								item3.ReturnedCallback = true;
							}
						}
						finally
						{
							UnitManager.ExecuteDatas.Semaphore.Release();
						}
						CallbackCommand callbackCommand = (CallbackCommand)httpRezult.Rezult;
						Unit.DataCommand[] listCommand = callbackCommand.ListCommand;
						foreach (Unit.DataCommand dataCommand in listCommand)
						{
							dataCommand.IP_client = SettCallBack.Value.URL;
							string textCommand = JsonConvert.SerializeObject(dataCommand);
							await UnitManager.AddCommand(dataCommand, "sync", textCommand, SettCallBack.Key);
						}
					}
					catch (Exception ex)
					{
						await Logers.AddError("Callback", ex.Message + ", URL:" + SettCallBack.Value.URL, json);
					}
				}
			}
			catch (Exception ex2)
			{
				CallbackErrorConnect = ex2.Message;
			}
		}
		finally
		{
			Settings.ListCallback.Semaphore.Release();
			if (Settings.RegisterCallback)
			{
				await Logers.AddError("Callback", CallbackErrorConnect, "JSON передан:\r\n" + SendJSON + "\r\n\r\nJSON принят:\r\n" + ReciveJSON);
			}
		}
	}

	public static async Task WebSocketsCallback(SetCallback SetCallback)
	{
		bool flag;
		bool Sem = (flag = !(await SetCallback.Semaphore.WaitAsync(40)));
		if (flag)
		{
			return;
		}
		WebSocket ws = null;
		bool IsBase64 = false;
		string SendJSON = "";
		string ReciveJSON = "";
		try
		{
			int num;
			_ = num - 1;
			_ = 1;
			try
			{
				try
				{
					ws = new ClientWebSocket();
					((ClientWebSocket)ws).ConnectAsync(new Uri(SetCallback.URL), AllCancellationToken.Token).Wait(AllCancellationToken.Token);
					if (ws.State != WebSocketState.Open)
					{
						ws.Dispose();
						return;
					}
				}
				catch (Exception ex)
				{
					if (!AllCancellationToken.Token.IsCancellationRequested)
					{
						CallbackErrorConnect = GetInnerErrorMessagee(ex);
					}
					goto end_IL_0129;
				}
				CallbackErrorConnect = "";
				byte[] Buf;
				try
				{
					string text = ((!(SetCallback.Token == "")) ? ("Basic=" + SetCallback.Token + ":" + SetCallback.Login + ":" + SetCallback.Password) : ("Basic=" + SetCallback.Login + ":" + SetCallback.Password));
					Buf = Encoding.UTF8.GetBytes(text);
					await ws.SendAsync(new ArraySegment<byte>(Buf, 0, Buf.Length), WebSocketMessageType.Text, true, AllCancellationToken.Token);
					if (ws.State != WebSocketState.Open)
					{
						if (!AllCancellationToken.Token.IsCancellationRequested)
						{
							CallbackErrorAut = ws.CloseStatus.ToString();
						}
						return;
					}
				}
				catch (Exception ex2)
				{
					if (!AllCancellationToken.Token.IsCancellationRequested)
					{
						CallbackErrorAut = GetInnerErrorMessagee(ex2);
					}
					goto end_IL_0129;
				}
				CallbackErrorAut = "";
				Buf = new byte[1024000];
				while (true)
				{
					if (ws.State != WebSocketState.Open)
					{
						ws.Dispose();
						break;
					}
					if (AllCancellationToken.Token.IsCancellationRequested)
					{
						ws.CloseAsync(WebSocketCloseStatus.Empty, "", default(CancellationToken)).Wait(AllCancellationToken.Token);
						break;
					}
					byte[] BufferRead;
					bool IsRead;
					try
					{
						BufferRead = new byte[0];
						int len = 0;
						IsRead = false;
						while (true)
						{
							WebSocketReceiveResult webSocketReceiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(Buf, 0, Buf.Length), AllCancellationToken.Token);
							if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
							{
								Array.Resize(ref BufferRead, len + webSocketReceiveResult.Count);
								Array.Copy(Buf, 0, BufferRead, len, webSocketReceiveResult.Count);
								len += webSocketReceiveResult.Count;
								if (webSocketReceiveResult.EndOfMessage)
								{
									IsRead = true;
									break;
								}
								continue;
							}
							break;
						}
					}
					catch (Exception ex3)
					{
						if (!AllCancellationToken.Token.IsCancellationRequested)
						{
							CallbackErrorReceive = GetInnerErrorMessagee(ex3);
						}
						break;
					}
					CallbackErrorReceive = "";
					if (IsRead)
					{
						string text2 = Encoding.UTF8.GetString(BufferRead, 0, BufferRead.Length);
						ReciveJSON = text2;
						object[] array = new object[2] { ws, text2 };
						new Task(async delegate(object? Str)
						{
							WebSocket ws2 = (WebSocket)((object[])Str)[0];
							string jsonContent = (string)((object[])Str)[1];
							Unit.RezultCommand RezultCommand;
							try
							{
								Unit.DataCommand dataCommand = JsonConvert.DeserializeObject<Unit.DataCommand>(jsonContent);
								IsBase64 = false;
								dataCommand.IP_client = SetCallback.URL;
								RezultCommand = await UnitManager.AddCommand(dataCommand, "", jsonContent);
							}
							catch (Exception ex5)
							{
								_ = ex5;
								try
								{
									jsonContent = Encoding.UTF8.GetString(Convert.FromBase64String(jsonContent));
									Unit.DataCommand dataCommand = JsonConvert.DeserializeObject<Unit.DataCommand>(jsonContent);
									IsBase64 = true;
									dataCommand.IP_client = SetCallback.URL;
									RezultCommand = await UnitManager.AddCommand(dataCommand, "", jsonContent);
								}
								catch (Exception ex6)
								{
									RezultCommand = new Unit.RezultCommand
									{
										Error = GetErrorMessagee(ex6),
										Status = Unit.ExecuteStatus.Error
									};
									await Logers.AddError("<Не опознана>", "Ошибка разбора (парсинга) команды", jsonContent, RezultCommand.Error);
									string text3 = JsonConvert.SerializeObject(RezultCommand, new JsonSerializerSettings
									{
										DateFormatString = "yyyy-MM-ddTHH:mm:ss"
									});
									if (AllCancellationToken != null && !AllCancellationToken.Token.IsCancellationRequested)
									{
										if (ws2.State != WebSocketState.Open)
										{
											ws2.Dispose();
										}
										else
										{
											byte[] bytes = Encoding.UTF8.GetBytes(text3);
											if (IsBase64)
											{
												bytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes));
											}
											await ws2.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, AllCancellationToken.Token);
										}
									}
									return;
								}
							}
							try
							{
								string text4 = (SendJSON = JsonConvert.SerializeObject(RezultCommand, new JsonSerializerSettings
								{
									DateFormatString = "yyyy-MM-ddTHH:mm:ss"
								}));
								if (AllCancellationToken == null || AllCancellationToken.Token.IsCancellationRequested)
								{
									return;
								}
								if (ws2.State != WebSocketState.Open)
								{
									ws2.Dispose();
									return;
								}
								byte[] bytes2 = Encoding.UTF8.GetBytes(text4);
								if (IsBase64)
								{
									bytes2 = Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes2));
								}
								await ws2.SendAsync(new ArraySegment<byte>(bytes2, 0, bytes2.Length), WebSocketMessageType.Text, true, AllCancellationToken.Token);
							}
							catch (Exception ex7)
							{
								if (!AllCancellationToken.Token.IsCancellationRequested)
								{
									CallbackErrorSend = GetInnerErrorMessagee(ex7);
								}
								return;
							}
							CallbackErrorSend = "";
						}, array).Start();
					}
					CallbackError = "";
				}
				end_IL_0129:;
			}
			catch (Exception ex4)
			{
				if (!AllCancellationToken.Token.IsCancellationRequested)
				{
					CallbackError = GetInnerErrorMessagee(ex4);
				}
			}
		}
		finally
		{
			if (Sem)
			{
				SetCallback.Semaphore.Release();
			}
			try
			{
				ws.Dispose();
			}
			catch
			{
			}
			if (Settings.RegisterCallback)
			{
				await Logers.AddError("Callback", CallbackErrorConnect, "JSON передан:\r\n" + SendJSON + "\r\n\r\nJSON принят:\r\n" + ReciveJSON);
			}
		}
	}

	public static void WriteLine(object PrnObject, int Font = 0, bool Clear = false, bool? RunStop = null, bool AsCheck = false, Unit Unit = null)
	{
		bool flag = false;
		List<TextLine> list = new List<TextLine>();
		flag = AddIn.AddInData != null && AddIn.TypeAddIn != AddIn.enTypeAddIn.None;
		if (Unit != null && Unit.CurDataCommand != null && Unit.CurDataCommand.RunAsAddIn)
		{
			flag = true;
		}
		list = ((Unit != null) ? Unit.TextLines : TextLines);
		if (RunAsAdmin)
		{
			return;
		}
		if (!flag && ViewModel == null)
		{
			if (PrnObject == null || !(PrnObject.GetType() == typeof(string)))
			{
				return;
			}
			if (!AsCheck)
			{
				Console.WriteLine((string)PrnObject);
				WriteLog("Console.WriteLine: " + (string)PrnObject);
				return;
			}
			int num = 48;
			string text = (string)PrnObject;
			text = Unit.GetPringString(text, num);
			string text2 = "";
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
				Console.WriteLine(text);
				WriteLog("Console.WriteLine: " + text);
				list.Add(new TextLine(text, Font));
				text = text2;
			}
			while (text2 != "");
			return;
		}
		if (RunStop == false)
		{
			RunStopWriteLines = false;
		}
		else if (RunStop == true)
		{
			RunStopWriteLines = true;
		}
		if (Clear)
		{
			list.Clear();
		}
		if (PrnObject != null && PrnObject.GetType() == typeof(string))
		{
			int num2 = 48;
			switch (Font)
			{
			case 1:
				num2 = 27;
				break;
			case 2:
				num2 = 41;
				break;
			case 3:
				num2 = 58;
				break;
			case 4:
				num2 = 68;
				break;
			}
			string text3 = (string)PrnObject;
			text3 = Unit.GetPringString(text3, num2);
			string text4 = "";
			do
			{
				if (text3.Length > num2)
				{
					text4 = text3.Substring(num2);
					text3 = text3.Substring(0, num2);
				}
				else
				{
					text4 = "";
				}
				list.Add(new TextLine(text3, Font));
				text3 = text4;
			}
			while (text4 != "");
		}
		if (PrnObject != null && PrnObject.GetType() == typeof(SixLabors.ImageSharp.Image))
		{
			list.Add(new TextLine(BarCode.ImageToPngBase64(PrnObject as SixLabors.ImageSharp.Image)));
		}
		else if (PrnObject != null && PrnObject.GetType() == typeof(ImageBarCode))
		{
			list.Add(new TextLine((PrnObject as ImageBarCode).PngBase64));
		}
		while (list.Count > 400)
		{
			list.RemoveAt(0);
		}
		if (!RunStopWriteLines)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (flag)
		{
			stringBuilder.Append("<style>\r\n                        .ras {\r\n                                all: revert;\r\n                            }\r\n                        div.CanvasShowMessage {\r\n                            all: initial;\r\n                            font-family: unset;\r\n                            z-index: 100000411;\r\n                            width: 100%;\r\n                            height: 100%;\r\n                            position: fixed;\r\n                            top: 0;\r\n                            left: 0;\r\n                            overflow: auto;\r\n                            background-color: rgba(0, 0, 0, 0.4);\r\n                        }\r\n                        div.WindowsShowMessage { \r\n                            padding: 16px;\r\n                            background-color: #f8f8f8;\r\n                            color: #333;\r\n                            border-radius: 10px;\r\n                            font-family:monospace; \r\n                            white-space:nowrap; \r\n                            align-items: center; \r\n                            justify-content: center; \r\n                            font-size:150%;\r\n                            &width\r\n                            height: auto;\r\n                            position: absolute;\r\n                            right: 0;\r\n                            left: 0;\r\n                            top: 30px;\r\n                            margin: auto;\r\n                        }\r\n                        div.ButtonCloseShowMessage {\r\n                            position: absolute;\r\n                            transform: translate(40%, -40%);\r\n                            top: 0;\r\n                            right: 0;\r\n                            z-index: 100000412;\r\n                            width: 28px;\r\n                            height: 28px;\r\n                            color: #333;\r\n                        }\r\n                        .closeBtnShowMessage {\r\n                            width: 28px;\r\n                            height: 28px;\r\n                            background-color: #fff;\r\n                            border: 2px solid #d5d5d5;\r\n                            -webkit-border-radius: 50%;\r\n                            border-radius: 50%;\r\n                            -webkit-box-shadow: 0 4px 12px 0 rgb(0 0 0 / 12%);\r\n                            box-shadow: 0 4px 12px 0 rgb(0 0 0 / 12%);\r\n                            background-size: 19px;\r\n                            display: block;\r\n                            font-size: 21px;\r\n                            color: #919191;\r\n                        }\r\n                    </style>\r\n                    <div class=\"ras CanvasShowMessage\">\r\n                        <div class=\"ras WindowsShowMessage\">\r\n                            <div class=\"ras ButtonCloseShowMessage\">\r\n                                <button id=\"closeBtnShowMessage\" class=\"ras closeBtnShowMessage\" tabindex=\"1\" aria-label=\"Закрыть\" \r\n                                        onclick=\"KkmServer.CloseMessageWindows('kkmserver.addin.ShowWindows._TypeWindows_')\"\r\n                                >×</button>\r\n                            </div>\r\n                            <style>\r\n                                .f0ShowMessage { \r\n                                    font-size:16.6px; \r\n                                    margin-top:0;\r\n                                    margin-bottom:0;\r\n                                }\r\n                                .f1ShowMessage { \r\n                                    font-size:29.2px; \r\n                                    margin-top:0;\r\n                                    margin-bottom:0;\r\n                                } \r\n                                .f2ShowMessage { \r\n                                    font-size:19.5px; \r\n                                    margin-top:0;\r\n                                    margin-bottom:0;\r\n                                }\r\n                                .f3ShowMessage { \r\n                                    font-size:13.65px; \r\n                                    margin-top:0;\r\n                                    margin-bottom:0;\r\n                                }\r\n                                .f4ShowMessage { \r\n                                    font-size:11.7px; \r\n                                    margin-top:0;\r\n                                    margin-bottom:0;\r\n                                } \r\n                                .divShowMessage { \r\n                                    width:100%;\r\n                                }\r\n                            </style>\r\n                            &Тело\r\n                        </div>\r\n                    </div>\r\n                </div>");
		}
		else
		{
			stringBuilder.Append("<style>\r\n                    .f0ShowMessage { \r\n                        font-size:96%; \r\n                        margin-top:0;\r\n                        margin-bottom:0;}\r\n                    .f1ShowMessage { \r\n                        font-size:169%; \r\n                        margin-top:0;\r\n                        margin-bottom:0;}\r\n                    } \r\n                    .f2ShowMessage { \r\n                        font-size:113%; \r\n                        margin-top:0;\r\n                        margin-bottom:0;}\r\n                    }\r\n                    .f3ShowMessage { \r\n                        font-size:79%; \r\n                        margin-top:0;\r\n                        margin-bottom:0;}\r\n                    }\r\n                    .f4ShowMessage { \r\n                        font-size:68%; \r\n                        margin-top:0;\r\n                        margin-bottom:0;}\r\n                    } \r\n                    .divShowMessage { \r\n                        width:100%;\r\n                    }\r\n                </style>\r\n                <body style=\"overflow-x:hidden; font-family:monospace; white-space:nowrap; align-items: center; justify-content: center; background-color: #fff1d3;\">\r\n                    <div class=\"ras divShowMessage\" style=\"padding: 16px; background-color: #fff;\">\r\n                        &Тело\r\n                    </div>\r\n                </body>");
		}
		if (flag)
		{
			stringBuilder.Replace("&width", "width: 440px;");
		}
		foreach (TextLine item in list)
		{
			if (item.Text != null)
			{
				stringBuilder.Replace("&Тело", "<p class=\"ras &pstyle\">&Строка</p>\r\n                            &Тело");
				stringBuilder.Replace("&Строка", item.Text.Replace(" ", "&nbsp;") + "&nbsp;");
				stringBuilder.Replace("&pstyle", "f" + item.Font + "ShowMessage");
			}
			if (item.Image != null)
			{
				stringBuilder.Replace("&Тело", "<div class=\"ras divShowMessage\" style=\"text-align: center;\"><img src=\"data:image/png;base64,&base64\"></div>\r\n                            &Тело");
				stringBuilder.Replace("&base64", item.Image);
			}
		}
		stringBuilder.Replace("&Тело", "");
		if (Unit != null && Unit.CurDataCommand != null && Unit.CurDataCommand.RunAsAddIn && Unit.CurRezultCommand != null)
		{
			Unit.CurRezultCommand.MessageHTML = stringBuilder.ToString();
			Unit.CurRezultCommand.TypeMessageHTM = "EndCommand";
			return;
		}
		string stBody = "<!DOCTYPE html>\r\n                <html>\r\n                &sbBody\r\n                </html>".Replace("&sbBody", stringBuilder.ToString());
		if (MainForm != null)
		{
			MainForm.MyPost(delegate
			{
				WriteHTML(stBody, RunStop);
			});
		}
		else if (ViewModel != null)
		{
			ViewModel.HTML = stBody;
		}
	}

	public static void WriteHTML(string stBody, bool? RunStop)
	{
		if (MainForm == null)
		{
			return;
		}
		ViewModel.HTML = stBody;
		if (RunStop == true && Settings.TypeRun == "Tray")
		{
			if (ViewModel.IsHide)
			{
				ViewModel.Tray_DoubleClick();
			}
			MainForm.WindowState = WindowState.Normal;
			MainForm.Activate();
			MainForm.Focus();
		}
	}

	public static bool RunAbout(bool All = false)
	{
		WriteLinesVisble = true;
		WriteLine("", 0, Clear: true);
		if (Settings.Marke == "GainUp")
		{
			WriteLine(string.Format("{0,12} : {1}", "Product", "GainUp"));
			WriteLine(string.Format("{0,12} : {1}", "Developer", Developer));
			WriteLine(string.Format("{0,12} : {1}", "Description", Description));
			WriteLine(string.Format("{0,12} : {1}", "Version", Verson));
			WriteLine(string.Format("{0,12} : {1}", "Copyright", Copyright));
			WriteLine(string.Format("{0,12} : {1}", "Web-site", "http://GainUp.ru"));
		}
		else if (Settings.Marke == "YClients")
		{
			WriteLine(string.Format("{0,12} : {1}", "Product", "YClients"));
			WriteLine(string.Format("{0,12} : {1}", "Developer", Developer));
			WriteLine(string.Format("{0,12} : {1}", "Description", Description));
			WriteLine(string.Format("{0,12} : {1}", "Version", Verson));
			WriteLine(string.Format("{0,12} : {1}", "Copyright", Copyright));
			WriteLine(string.Format("{0,12} : {1}", "Web-site", "https://yclients.com"));
		}
		else
		{
			WriteLine(string.Format("{0,12} : {1}", "Product", Product));
			WriteLine(string.Format("{0,12} : {1}", "Developer", Developer));
			WriteLine(string.Format("{0,12} : {1}", "Description", Description));
			WriteLine(string.Format("{0,12} : {1}", "Version", Verson));
			WriteLine(string.Format("{0,12} : {1}", "Copyright", Copyright));
			WriteLine(string.Format("{0,12} : {1}", "Web-site", "https://kkmserver.ru"));
		}
		string text = "";
		foreach (ComDevice.ItemComDevices item in ComDevice.ReadComDevices().Result)
		{
			if (item.Id.Length <= 12 && item.Id.Length >= 10 && item.Option != ComDevice.PaymentOption.Evotor && item.Option != ComDevice.PaymentOption.None)
			{
				text = text + ((text == "") ? "" : ", ") + item.Id;
				if (text.Length > 14)
				{
					WriteLine(string.Format("{0,12} : {1}", "Lic. INN", text));
					text = "";
				}
			}
		}
		if (text.Length > 4)
		{
			WriteLine(string.Format("{0,12} : {1}", "Lic. INN", text));
		}
		WriteLine("", 0, Clear: false, WriteLinesVisble);
		if (All)
		{
			string[] copyright = GetCopyright();
			for (int i = 0; i < copyright.Length; i++)
			{
				WriteLine(copyright[i]);
			}
		}
		WriteLine("", 0, Clear: false, WriteLinesVisble);
		return true;
	}

	public static string[] GetCopyright()
	{
		return new string[7] { "Copyright © .NET Foundation and Contributors", "Copyright © Microsoft Corporation", "Copyright © 2007 James Newton-King", "Copyright © 2023 AvaloniaUI OÜ (14839404)", "Copyright © Six Labors", "Copyright © Michael Jahn", "Copyright © 2016 Garbuz D.V." };
	}

	public static void WriteError(string sEvent)
	{
		try
		{
			EventLog.WriteEntry(NameService, sEvent);
		}
		catch
		{
		}
		try
		{
			WriteLine(sEvent, 0, Clear: false, true);
		}
		catch
		{
		}
		string text = DateTime.Now.ToString() + ": " + sEvent;
		File.AppendAllText(Path.Combine(GerPahtSettings(), "Log.txt"), text + "\r\n");
	}

	public static void WriteLog(string sEvent)
	{
	}

	public static string GetErrorMessagee(Exception ex)
	{
		return "Message:<br/>\n\r" + ex.Message + "<br/>\n\rTrase:<br/>\n\r" + ex.StackTrace;
	}

	public static string GetInnerErrorMessagee(Exception ex)
	{
		string text = ex.Message;
		for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
		{
			text = text + ((text != "") ? ": " : "") + innerException.Message;
		}
		return text;
	}

	public static async Task<bool> ExecuteCommandAsync(string Command, string WorkingDirectory = null, string Arguments = null, bool RunAsAdmin = true, bool RegisterError = true, string HeadError = "", object CancellToken = null)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.UseShellExecute = true;
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		processStartInfo.FileName = Command;
		processStartInfo.Arguments = Arguments;
		if (WorkingDirectory != null)
		{
			processStartInfo.WorkingDirectory = WorkingDirectory;
		}
		processStartInfo.RedirectStandardOutput = false;
		if (RunAsAdmin)
		{
			processStartInfo.Verb = "runas";
			processStartInfo.UseShellExecute = true;
		}
		Process Process = null;
		try
		{
			if (CancellToken == null)
			{
				CancellToken = default(CancellationToken);
			}
			Process = Process.Start(processStartInfo);
			await Process.WaitForExitAsync((CancellationToken)CancellToken);
			ResultExecuteCommand = "";
			try
			{
				ResultExecuteCommand = await Process.StandardOutput.ReadToEndAsync();
			}
			catch (Exception)
			{
			}
			if (ResultExecuteCommand != null && ResultExecuteCommand != "")
			{
				WriteLine(HeadError + ResultExecuteCommand);
			}
			if (Process.ExitCode != 0 && RegisterError)
			{
				try
				{
					WriteError(HeadError + ResultExecuteCommand);
					return false;
				}
				catch (Exception)
				{
					WriteError(HeadError + "Ошибка выполнения команды: " + Command);
				}
			}
		}
		catch (Exception)
		{
			if (RegisterError)
			{
				WriteError(HeadError + "Ошибка запуска команды: " + Command);
			}
		}
		finally
		{
			if (Process != null)
			{
				if (!Process.HasExited)
				{
					Process.Kill(true);
				}
				Process.Dispose();
			}
		}
		return true;
	}

	public static bool ExecuteWinCommand(string Command, bool RegisterError = true, string FileName = "", bool NoRegErrorOnRun = false, bool RunAsAdmin = true, string WorkingDirectory = null, string HeadError = "", object CancellToken = null, bool WaitForExit = true)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.UseShellExecute = true;
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		if (FileName == "" && Command.IndexOf(" ") == -1)
		{
			processStartInfo.FileName = Command;
		}
		else if (FileName == "")
		{
			processStartInfo.FileName = Command.Substring(0, Command.IndexOf(" "));
			processStartInfo.Arguments = Command.Substring(Command.IndexOf(" ") + 1);
		}
		else
		{
			processStartInfo.FileName = FileName;
			processStartInfo.Arguments = Command;
		}
		if (WorkingDirectory != null)
		{
			processStartInfo.WorkingDirectory = WorkingDirectory;
		}
		processStartInfo.RedirectStandardOutput = false;
		if (RunAsAdmin)
		{
			processStartInfo.Verb = "runas";
			processStartInfo.UseShellExecute = true;
		}
		Process process = null;
		try
		{
			if (CancellToken == null)
			{
				CancellToken = default(CancellationToken);
			}
			process = Process.Start(processStartInfo);
			if (!WaitForExit)
			{
				return true;
			}
			process.WaitForExitAsync((CancellationToken)CancellToken).Wait();
			ResultExecuteCommand = "";
			try
			{
				ResultExecuteCommand = process.StandardOutput.ReadToEnd();
			}
			catch (Exception)
			{
			}
			if (ResultExecuteCommand != null && ResultExecuteCommand != "")
			{
				WriteLine(HeadError + ResultExecuteCommand);
			}
			if (process.ExitCode != 0 && RegisterError)
			{
				try
				{
					WriteError(HeadError + ResultExecuteCommand);
					return false;
				}
				catch (Exception)
				{
					WriteError(HeadError + "Ошибка выполнения команды:" + FileName + " " + Command);
				}
			}
		}
		catch (Exception)
		{
			if (!NoRegErrorOnRun)
			{
				WriteError(HeadError + "Ошибка запуска команды:" + FileName + " " + Command);
			}
		}
		finally
		{
			if (process != null)
			{
				if (!process.HasExited)
				{
					process.Kill(true);
				}
				process.Dispose();
			}
		}
		return true;
	}

	public static bool RunBrauser(bool Is5894 = false)
	{
		if (!Is5894)
		{
			UriProgram = "http://localhost:" + Settings.ipPort;
		}
		else
		{
			UriProgram = "http://localhost:" + (Settings.ipPort + 1);
		}
		if (!Is5894 && Settings.ServerSertificate != "")
		{
			UriProgram = HttpService.GetUrl(null, "", "", "", GetHostFromSertificate: true, NotServerURL: false);
		}
		string uriProgram = UriProgram;
		string text = Settings.LoginAdmin + ":" + Settings.PassAdmin;
		string text2 = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(text));
		uriProgram = UriProgram + "?Basic=" + text2;
		try
		{
			Process.Start(uriProgram);
		}
		catch (Exception)
		{
			try
			{
				Process.Start("cmd", "/C start " + uriProgram);
			}
			catch (Exception ex2)
			{
				WriteError("Ошибка выполнения команды:" + ex2.Message);
			}
		}
		return true;
	}

	public static bool RunSite()
	{
		string text = "";
		text = ((Settings.Marke == "GainUp") ? "http://GainUp.ru/" : ((!(Settings.Marke == "YClients")) ? "https://kkmserver.ru/" : "https://yclients.com/"));
		ExecuteWinCommand(text, RegisterError: false, "", NoRegErrorOnRun: false, RunAsAdmin: false);
		return true;
	}

	public static bool RunForum()
	{
		ExecuteWinCommand("http://forum.kkmserver.ru/", RegisterError: false, "", NoRegErrorOnRun: false, RunAsAdmin: false);
		return true;
	}

	public static bool RightsUp(string Command, bool WaitForExit = true)
	{
		WriteLine("Повышаем права...", 0, Clear: false, WriteLinesVisble);
		return ExecuteWinCommand(Command, RegisterError: false, "\"" + Path.Combine(GetPaht(), "kkmserver.exe") + "\"", NoRegErrorOnRun: false, RunAsAdmin: true, null, "", null, WaitForExit);
	}

	public static bool IsAdmin()
	{
		if (IsAdminSave == 0)
		{
			return false;
		}
		if (IsAdminSave == 1)
		{
			return true;
		}
		bool flag = false;
		WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
		flag = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
		return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
	}

	public static bool WorkAsAdmin(string[] args)
	{
		if (args.Length >= 1 && args[0] == "EoURestart")
		{
			if (!IsAdmin())
			{
				return true;
			}
			RunAsAdmin = true;
			return true;
		}
		if (args.Length >= 1 && args[0] == "SetTypeRun")
		{
			if (!IsAdmin())
			{
				return true;
			}
			RunAsAdmin = true;
			if (args.Length >= 2)
			{
				SetTypeRun("", args[1]);
			}
			else
			{
				SetTypeRun("");
			}
			return true;
		}
		if (args.Length >= 1 && args[0] == "InstallNewSertificate")
		{
			if (!IsAdmin())
			{
				return true;
			}
			WriteLine("Run InstallNewSertificate");
			RunAsAdmin = true;
			UtilSertificate.InstallNewSertificate(args[1]);
			return true;
		}
		if (args.Length >= 1 && args[0] == "SbRfCopyFile")
		{
			GateSbRf.CopyFiles(args[1], args[2]);
			return true;
		}
		if (args.Length >= 1 && args[0] == "InstallAddInJson")
		{
			RunAsAdmin = true;
			RegisterAddIn(Del: false);
			return true;
		}
		if (args.Length >= 1 && args[0] == "RemoveAddInJson")
		{
			RunAsAdmin = true;
			RegisterAddIn(Del: true);
			return true;
		}
		if (args.Length >= 1 && args[0] == "Reboot")
		{
			RunAsAdmin = true;
			RestartKkmservr();
			return true;
		}
		return false;
	}

	public static void CalcPath(string User = "")
	{
		if (IsAdmin())
		{
			ChromeAddIn = "/etc/opt/chrome/native-messaging-hosts";
		}
		if (User != "")
		{
			ChromeAddIn = ChromeAddIn.Replace("%LOGNAME%", User);
			ChromiumAddIn = ChromiumAddIn.Replace("%LOGNAME%", User);
			MozillaAddIn = MozillaAddIn.Replace("%LOGNAME%", User);
			Mozilla64AddIn = Mozilla64AddIn.Replace("%LOGNAME%", User);
			BashSettings = BashSettings.Replace("%LOGNAME%", User);
			PatchDeamon = PatchDeamon.Replace("%LOGNAME%", User);
		}
		else
		{
			ChromeAddIn = Environment.ExpandEnvironmentVariables(ChromeAddIn);
			ChromiumAddIn = Environment.ExpandEnvironmentVariables(ChromiumAddIn);
			MozillaAddIn = Environment.ExpandEnvironmentVariables(MozillaAddIn);
			Mozilla64AddIn = Environment.ExpandEnvironmentVariables(Mozilla64AddIn);
			BashSettings = Environment.ExpandEnvironmentVariables(BashSettings);
			PatchDeamon = Environment.ExpandEnvironmentVariables(PatchDeamon);
		}
	}

	public static void RegisterAddIn(bool Del)
	{
		CalcPath();
		try
		{
			if (!Del)
			{
				UtilSertificate.AddNameDomenInHosts("kkmserver");
				string text = "{\r\n  \"name\": \"kkmserver.addin.io\",\r\n  \"description\": \"Kkm Server\",\r\n  \"path\": \"KkmServer.exe\",\r\n  \"type\": \"stdio\",\r\n  \"allowed_origins\": [\r\n    \"chrome-extension://dkbekbmeodgkglklclonfbglkbglinlm/\",\r\n    \"chrome-extension://mjeeklofjbnodnnfibjolokichkhcpog/\",\r\n    \"chrome-extension://fjfjdfimgfmmcplafmiakajgemeghpdp/\"\r\n  ]\r\n}";
				try
				{
					text = text.Replace("KkmServer.exe", Path.Combine(GetPaht(), "kkmserver"));
					File.WriteAllText(Path.Combine(GetPaht(), "AddIn_Chrome.json"), text);
				}
				catch (Exception)
				{
				}
				try
				{
					Directory.CreateDirectory(ChromeAddIn);
				}
				catch
				{
				}
				try
				{
					File.WriteAllText(ChromeAddIn + "/kkmserver.addin.io.json", text);
				}
				catch
				{
				}
				try
				{
					Directory.CreateDirectory(ChromiumAddIn);
				}
				catch
				{
				}
				try
				{
					File.WriteAllText(ChromiumAddIn + "/kkmserver.addin.io.json", text);
				}
				catch
				{
				}
				text = "{\r\n  \"name\": \"kkmserver.addin.io\",\r\n  \"description\": \"Kkm Server\",\r\n  \"path\": \"KkmServer.exe\",\r\n  \"type\": \"stdio\",\r\n  \"allowed_extensions\": [\r\n    \"addin@kkmserver.ru\"\r\n  ]\r\n}";
				text = text.Replace("KkmServer.exe", Path.Combine(GetPaht(), "kkmserver"));
				try
				{
					Directory.CreateDirectory(MozillaAddIn);
				}
				catch
				{
				}
				try
				{
					File.WriteAllText(MozillaAddIn + "/kkmserver.addin.io.json", text);
				}
				catch
				{
				}
				try
				{
					Directory.CreateDirectory(Mozilla64AddIn);
				}
				catch
				{
				}
				try
				{
					File.WriteAllText(Mozilla64AddIn + "/kkmserver.addin.io.json", text);
				}
				catch
				{
				}
				ExecuteWinCommand(Environment.ExpandEnvironmentVariables("-a %LOGNAME% dialout"), RegisterError: true, "gpasswd");
			}
			else
			{
				if (!Del)
				{
					return;
				}
				try
				{
					string text2 = Path.Combine(GetPaht(), "AddIn_Chrome.json");
					if (Directory.Exists(text2))
					{
						File.Delete(text2);
					}
				}
				catch
				{
				}
				try
				{
					File.Delete(ChromeAddIn + "/kkmserver.addin.io.json");
				}
				catch
				{
				}
				try
				{
					File.Delete(ChromiumAddIn + "/kkmserver.addin.io.json");
				}
				catch
				{
				}
				try
				{
					File.Delete(MozillaAddIn + "/kkmserver.addin.io.json");
				}
				catch
				{
				}
				try
				{
					File.Delete(Mozilla64AddIn + "/kkmserver.addin.io.json");
					return;
				}
				catch
				{
					return;
				}
			}
		}
		catch (Exception ex2)
		{
			WriteLine(ex2.Message);
		}
	}

	public static void ShowWindow()
	{
		SetShowWindow = true;
	}

	public static void RunShowWindow()
	{
		if (AddIn.TypeAddIn != AddIn.enTypeAddIn.None || !SetShowWindow || MainForm == null)
		{
			return;
		}
		try
		{
			SetShowWindow = false;
			if (ViewModel.IsHide)
			{
				ViewModel.Tray_DoubleClick();
			}
			MainForm.Show();
			MainForm.WindowState = WindowState.Normal;
		}
		catch (Exception)
		{
		}
	}

	public static void OpenWindow()
	{
		if (ViewModel.IsHide)
		{
			ViewModel.Tray_DoubleClick();
		}
		MainForm.WindowState = WindowState.Normal;
		MainForm.Activate();
		MainForm.Focus();
	}

	public static string StatusService()
	{
		try
		{
			if (new ServiceController(NameService).Status == ServiceControllerStatus.Running)
			{
				return "start";
			}
			return "stop";
		}
		catch
		{
			return "null";
		}
	}

	public static void RegisterService(bool Register = false, string UserName = "", bool Run = false)
	{
		CalcPath();
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		bool flag = false;
		if (new ServiceController("Spooler").Status == ServiceControllerStatus.Running)
		{
			flag = true;
		}
		text = " create " + NameService + " binPath= \"" + GetPaht() + "\\" + NameRuFile + " RunAsService\"  DisplayName= \"" + Product + "\" start= auto";
		text2 = " failure " + NameService + " reset=60 actions=run/60000/run/60000/run/60000";
		text3 = " delete " + NameService;
		text4 = " start " + NameService;
		text5 = "sc";
		text += " depend= http";
		if (flag)
		{
			text += "/Spooler";
		}
		string text6 = StatusService();
		if (text6 == "stop" && Run)
		{
			ExecuteWinCommand(text4, RegisterError: true, text5);
			return;
		}
		if (text6 == "start")
		{
			new ServiceController(NameService).Stop();
		}
		text6 = StatusService();
		if (text6 == "start" || text6 == "stop")
		{
			ExecuteWinCommand(text3, RegisterError: true, text5);
		}
		text6 = StatusService();
		if (Register && text6 == "null")
		{
			ExecuteWinCommand(text, RegisterError: true, text5);
			ExecuteWinCommand(text2, RegisterError: true, text5);
		}
	}

	public static void SetTypeRun(string TypeRun, string UserName = "")
	{
		CalcPath(UserName);
		bool flag = false;
		if (UnitManager == null)
		{
			UnitManager = new UnitManager();
			LoadSettingAsyncs().Wait();
			flag = true;
			TypeRun = Settings.TypeRun;
		}
		if (!flag)
		{
			Settings.TypeRun = TypeRun;
			SaveSettingsAsync().Wait();
			if (HttpServer != null)
			{
				WriteLine("Stop server....", 0, Clear: false, WriteLinesVisble);
				HttpServer.Stop();
				IsRun = false;
			}
		}
		if (!IsAdmin())
		{
			RightsUp("SetTypeRun " + Environment.UserName);
		}
		if (IsAdmin())
		{
			if (TypeRun == "Service")
			{
				RegisterService(Register: true, UserName);
				new ServiceController(NameService).Start();
			}
			else
			{
				RegisterService(Register: false, UserName);
			}
		}
		if (!flag)
		{
			RegistryKey registryKey = null;
			try
			{
				registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
				if (TypeRun == "Tray")
				{
					string exePaht = GetExePaht();
					registryKey.SetValue(NameService, exePaht);
				}
				else
				{
					registryKey.DeleteValue(NameService);
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				registryKey.Close();
			}
		}
		if (!flag)
		{
			if (TypeRun != "Service")
			{
				Process.Start(GetStartInfo());
			}
			HaltServer();
		}
	}

	public static void RestartKkmservr()
	{
		if (UnitManager == null)
		{
			UnitManager = new UnitManager();
			LoadSettingAsyncs().Wait();
		}
		else
		{
			IsAdminSave = 0;
		}
		if (AddIn.TypeAddIn == AddIn.enTypeAddIn.Chrome)
		{
			UnitManager.FreeUnit();
			StopServer();
			HaltServer();
		}
		else if (Settings.TypeRun == "Windows" || Settings.TypeRun == "Tray")
		{
			UnitManager.FreeUnit();
			StopServer();
			string location = Assembly.GetEntryAssembly().Location;
			Process process = new Process();
			if (Path.GetExtension(location) == ".dll")
			{
				process.StartInfo.FileName = "dotnet";
				process.StartInfo.Arguments = Environment.CommandLine;
			}
			else
			{
				process.StartInfo.FileName = location;
			}
			process.Start();
			Program.FlagAppExit = true;
			HaltServer();
		}
		else
		{
			if (!(Settings.TypeRun == "Service"))
			{
				return;
			}
			if (!IsAdmin())
			{
				UnitManager.FreeUnit();
				StopServer();
			}
			if (!IsAdmin())
			{
				RightsUp("Reboot");
				Program.FlagAppExit = true;
			}
			if (!IsAdmin())
			{
				return;
			}
			try
			{
				Thread.Sleep(2000);
				ServiceController serviceController = new ServiceController(NameService);
				if (serviceController.Status == ServiceControllerStatus.Running)
				{
					serviceController.Stop();
					serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 20));
				}
				serviceController.Start();
			}
			catch
			{
			}
		}
	}

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

	public static int IntParse(string str)
	{
		if (str == "" || str == null)
		{
			return 0;
		}
		return int.Parse(str);
	}

	public static string Shifrovka(string ishText, string PBKDFprotocol = "PBKDF2")
	{
		string text = "bgf3grtb45bf";
		string text2 = "kkmserv";
		string text3 = "kkmserv1";
		string text4 = "SHA1";
		int num = 2;
		string text5 = "a8doSuDitOz1hZe#";
		int num2 = 256;
		ishText = ishText.Trim();
		if (string.IsNullOrEmpty(ishText) || ishText == "թ")
		{
			return "";
		}
		byte[] bytes = Encoding.ASCII.GetBytes(text5);
		byte[] bytes2 = Encoding.ASCII.GetBytes(text2);
		byte[] bytes3 = Encoding.ASCII.GetBytes(text3);
		byte[] array = null;
		if (PBKDFprotocol == "PBKDF1")
		{
			array = new PasswordDeriveBytes(text, bytes2, text4, num).GetBytes(num2 / 8);
		}
		else if (PBKDFprotocol == "PBKDF2")
		{
			array = new Rfc2898DeriveBytes(text, bytes3, num, HashAlgorithmName.SHA1).GetBytes(num2 / 8);
		}
		byte[] inArray = new byte[0];
		using (Aes aes = Aes.Create())
		{
			aes.Mode = CipherMode.CBC;
			ICryptoTransform cryptoTransform = aes.CreateEncryptor(array, bytes);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
				using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
				{
					streamWriter.Write(ishText);
					streamWriter.Flush();
					cryptoStream.FlushFinalBlock();
				}
				inArray = memoryStream.ToArray();
			}
			aes.Clear();
		}
		return Convert.ToBase64String(inArray);
	}

	public static string DeShifrovka(string ciphText, string PBKDFprotocol = "PBKDF2")
	{
		string text = "";
		try
		{
			while (true)
			{
				string text2 = "bgf3grtb45bf";
				string text3 = "kkmserv";
				string text4 = "kkmserv1";
				string text5 = "SHA1";
				int num = 2;
				string text6 = "a8doSuDitOz1hZe#";
				int num2 = 256;
				if (string.IsNullOrEmpty(ciphText))
				{
					return "";
				}
				byte[] bytes = Encoding.ASCII.GetBytes(text6);
				byte[] bytes2 = Encoding.ASCII.GetBytes(text3);
				byte[] bytes3 = Encoding.ASCII.GetBytes(text4);
				byte[] array = Convert.FromBase64String(ciphText);
				byte[] array2 = null;
				if (PBKDFprotocol == "PBKDF1")
				{
					array2 = new PasswordDeriveBytes(text2, bytes2, text5, num).GetBytes(num2 / 8);
				}
				else if (PBKDFprotocol == "PBKDF2")
				{
					array2 = new Rfc2898DeriveBytes(text2, bytes3, num, HashAlgorithmName.SHA1).GetBytes(num2 / 8);
				}
				string text7 = "";
				using (Aes aes = Aes.Create())
				{
					aes.Mode = CipherMode.CBC;
					ICryptoTransform cryptoTransform = aes.CreateDecryptor(array2, bytes);
					using (MemoryStream memoryStream = new MemoryStream(array))
					{
						using CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
						using StreamReader streamReader = new StreamReader(cryptoStream);
						text7 = streamReader.ReadToEnd();
					}
					aes.Clear();
				}
				if (text7.Length == 0 || text7[0] != 'թ')
				{
					return text7;
				}
				if (text == text7)
				{
					break;
				}
				text = text7;
			}
			return "";
		}
		catch
		{
			return "";
		}
	}
}
