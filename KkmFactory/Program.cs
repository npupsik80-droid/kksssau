using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using KkmFactory.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace KkmFactory;

public class Program
{
	public static bool FlagAppExit;

	public static Action ActionOnRun;

	public static void Main(string[] args)
	{
		try
		{
			AppDomain.CurrentDomain.UnhandledException += UnhandledException;
			if (Global.WorkAsAdmin(args))
			{
				return;
			}
			string directoryName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			if (directoryName == null)
			{
				throw new Exception("baseFolder = null");
			}
			AddIn.IdBrauzer = new Random().Next(10, 99);
			Global.WriteLog("Новый запуск kkmserver");
			if (args.Length != 0 && args[0].IndexOf("chrome-extension") != -1)
			{
				AddIn.TypeAddIn = AddIn.enTypeAddIn.Chrome;
				AddIn.Brauzer = "ChromeW";
			}
			if (args.Length != 0 && (args[0].IndexOf("AddIn_Firefox.json") != -1 || args[0].IndexOf("mozilla") != -1))
			{
				AddIn.TypeAddIn = AddIn.enTypeAddIn.Chrome;
				AddIn.Brauzer = "Mozilla";
			}
			if (args.Length != 0 && args[0].ToLower() == "run")
			{
				AddIn.TypeAddIn = AddIn.enTypeAddIn.Chrome;
				AddIn.Brauzer = "DebugRu";
			}
			if (args.Length == 0 || (args.Length != 0 && args[0] == "SaveRegistry"))
			{
				string text = Path.Combine(directoryName, "AddIn_Chrome.json");
				try
				{
					Registry.SetValue("HKEY_CURRENT_USER\\Software\\Google\\Chrome\\NativeMessagingHosts\\kkmserver.addin.io", string.Empty, text);
				}
				catch
				{
				}
				text = Path.Combine(directoryName, "AddIn_Firefox.json");
				try
				{
					Registry.SetValue("HKEY_CURRENT_USER\\Software\\Mozilla\\NativeMessagingHosts\\kkmserver.addin.io", string.Empty, text);
				}
				catch
				{
				}
			}
			if (args.Length != 0 && args[0] == "SaveRegistry")
			{
				return;
			}
			try
			{
				Global.SettingsServer settingsServer = JsonConvert.DeserializeObject<Global.SettingsServer>(File.ReadAllText(Path.Combine(Global.GerPahtSettings(), "SettingsServ.ini")));
				IPEndPoint[] activeTcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
				for (int i = 0; i < activeTcpListeners.Length; i++)
				{
					if (activeTcpListeners[i].Port == settingsServer.ipPort)
					{
						Global.ServerRunInAnotherWindow = true;
						break;
					}
				}
			}
			catch
			{
			}
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Global.UnitManager = new UnitManager();
			Global.LoadSettingAsyncs(InitLoad: true).Wait();
			try
			{
				if (ParsingArgs(args))
				{
					return;
				}
			}
			catch (Exception)
			{
			}
			if (AddIn.TypeAddIn != AddIn.enTypeAddIn.None)
			{
				if (AddIn.CounterStatServer == 0 && !Global.ServerRunInAnotherWindow)
				{
					new Task(delegate
					{
						Global.StartServer();
					}).Start();
				}
				else
				{
					Global.Logers = new Logers();
				}
				AddIn.CounterStatServer++;
				AddIn.Run(args);
				AddIn.CounterStatServer--;
				if (AddIn.CounterStatServer == 0 && !Global.ServerRunInAnotherWindow)
				{
					if (Global.Settings.RegisterAllCommand)
					{
						Global.Logers.AddError("StopServer", "", "AddIn: Останавливаем при CounterStatServer = 0").Wait();
					}
					Global.StopServer();
				}
				return;
			}
			Global.DateStart = DateTime.Now;
			if (Global.Settings.TypeRun == "Tray")
			{
				Global.WriteLinesVisble = false;
			}
			bool flag = false;
			if (args.Length != 0 && args[0] == "RunAsService")
			{
				flag = true;
			}
			string text2 = Global.StatusService();
			if (Global.Settings.TypeRun == "Service" && flag && text2 == "stop")
			{
				ServiceBase.Run(new ServiceBase[1]
				{
					new Service()
				});
				return;
			}
			if (Global.Settings.TypeRun == "Service" && !flag && text2 == "stop")
			{
				Global.RegisterService(Register: false, "", Run: true);
			}
			if (Global.Settings.TypeRun == "Service" && !flag)
			{
				Run(delegate
				{
					Global.RunAbout();
					Global.ViewModel.TrayVisible = false;
				}, args);
				return;
			}
			if (!(Global.Settings.TypeRun == "Windows"))
			{
				_ = Global.Settings.TypeRun == "Tray";
			}
			if (Global.ServerRunInAnotherWindow)
			{
				Global.RunBrauser();
				return;
			}
			if (Global.Settings.TypeRun == "Windows")
			{
				Global.ViewModel = new ViewModel();
				Global.RunAbout(All: true);
				Global.StartServer();
				Run(delegate
				{
					Global.ViewModel.TrayVisible = false;
				}, args);
			}
			else if (Global.Settings.TypeRun == "Tray")
			{
				Global.ViewModel = new ViewModel();
				Global.RunAbout(All: true);
				Global.StartServer();
				Run(delegate
				{
					Global.ViewModel.TrayVisible = true;
				}, args);
			}
			else
			{
				_ = Global.Settings.TypeRun == "Service";
			}
			if (Global.Settings.TypeRun == "Windows" || Global.Settings.TypeRun == "Tray")
			{
				try
				{
					Global.UnitManager.ClosePortUnit();
				}
				catch
				{
				}
				Global.StopServer(RestartHTTP: false, FreeUnit: false);
			}
		}
		catch (Exception ex2)
		{
			SaveGlobalException(ex2);
			Global.UnitManager.ClosePortUnit();
			Global.HaltServer();
		}
	}

	public static void Run(Action callback, string[] args)
	{
		ActionOnRun = callback;
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	public static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont()
			.LogToTrace(LogEventLevel.Warning)
			.UseReactiveUI();
	}

	public static bool ParsingArgs(string[] args)
	{
		int num = 0;
		while (num < args.Length)
		{
			string text = args[num];
			text = text.ToUpper();
			if (text == "RESTART_SERVICE")
			{
				ServiceController serviceController = new ServiceController(Global.NameService);
				try
				{
					serviceController.Stop();
				}
				catch
				{
				}
				serviceController.Start();
				return true;
			}
			if (text == "SERVICE")
			{
				Global.SetTypeRun("Service");
			}
			switch (text)
			{
			case "-A":
				Global.SetTypeRun("Tray");
				Global.WriteLine("Запуск в трей утсновлено");
				return true;
			case "-D":
				Global.SetTypeRun("Service");
				Global.WriteLine("Сервис запущен.");
				return true;
			case "WINDOWS":
				Global.SetTypeRun("Windows");
				break;
			}
			switch (text)
			{
			case "-N":
				Global.SetTypeRun("Windows");
				Global.WriteLine("Сервис остановлен.");
				return true;
			case "-B":
				Global.RunBrauser();
				Task.Delay(1000).Wait();
				return true;
			case "-R":
				Global.RightsUp("InstallAddInJson " + Environment.UserName);
				Global.WriteLine("Перерегистрация компоненты расширения браузера выполнена");
				return true;
			default:
				if (!(text.ToUpper() == "-HELP") && !(text.ToUpper() == "--HELP"))
				{
					if (text == "-S")
					{
						Global.RunAbout();
						Global.WriteLine("");
						if (!Global.ServerRunInAnotherWindow)
						{
							Global.StartServer();
						}
						Global.WriteLine("");
						ParsingArgs(new string[1] { "-h" });
						Global.WriteLine("Нажмите \"x\" для выхода");
						bool flag;
						do
						{
							if (AddIn.TypeAddIn == AddIn.enTypeAddIn.None)
							{
								Console.Write(">");
							}
							flag = false;
							while (true)
							{
								if (Console.KeyAvailable)
								{
									ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
									Global.WriteLine("");
									if (consoleKeyInfo.KeyChar == 'x')
									{
										Global.WriteLine("");
										flag = true;
										break;
									}
								}
								if (FlagAppExit)
								{
									flag = true;
									break;
								}
								Task.Delay(2000).Wait();
							}
						}
						while (!flag);
						Global.StopServer();
						return true;
					}
					if (text == "TRAY")
					{
						Global.SetTypeRun("Tray");
					}
					if (text.Substring(0, 1) == "P")
					{
						short num2 = short.Parse(text.Substring(1));
						Global.Settings.ipPort = num2;
						Global.Settings.ipPortOld = num2;
						Global.SaveSettingsAsync().Wait();
						return true;
					}
					if (text == "-U")
					{
						Global.StartServer();
						return true;
					}
					if (text == "WINDOWS".ToUpper())
					{
						Global.LoadSettingAsyncs().Wait();
						Global.Settings.LoginAdmin = "";
						Global.Settings.PassAdmin = "";
						Global.Settings.LoginUser = "";
						Global.Settings.PassUser = "";
						Global.Settings.RunCallback = true;
						Global.Settings.TimeCallback = 5;
						Global.Settings.TypeCallback = 1;
						Global.Settings.ListCallback.Clear();
						Global.SetCallback setCallback = new Global.SetCallback();
						setCallback.URL = "";
						setCallback.Login = "";
						setCallback.Password = "";
						setCallback.Token = "";
						Global.Settings.ListCallback.Add(Guid.NewGuid().ToString(), setCallback);
						Global.SaveSettingsAsync().Wait();
						Global.SetTypeRun("Tray");
						return true;
					}
					num++;
					break;
				}
				goto case "-H";
			case "-H":
			case "--H":
				Global.WriteLine("Параметры запуска:");
				Global.WriteLine("-s - Запустить сервер");
				Global.WriteLine("-d - Зарегистрировать и запустить сервис/deamon");
				Global.WriteLine("       (если используется расширение браузера - то запускать как сервис не надо:");
				Global.WriteLine("       (расширение браузера само запустит сервер");
				Global.WriteLine("-n - Остановить и удалить сервис");
				Global.WriteLine("-r - Регистрация компоненты расширения браузера");
				Global.WriteLine("-b - Открыть страницу настроек в браузере");
				Global.WriteLine("");
				return true;
			}
		}
		return false;
	}

	private static void UnhandledException(object sender, UnhandledExceptionEventArgs args)
	{
		try
		{
			SaveGlobalException((Exception)args.ExceptionObject);
		}
		catch
		{
		}
	}

	public static void SaveGlobalException(Exception Ex)
	{
		string errorMessagee = Global.GetErrorMessagee(Ex);
		File.AppendAllText(Path.Combine(Global.GerPahtSettings(), "ErrorCrach.log"), DateTime.Now.ToString() + "\r\n" + errorMessagee + "\r\n\r\n");
		if (Global.Logers != null)
		{
			Global.Logers.AddError("Crach system", errorMessagee).Wait();
		}
		if (Global.Logers != null)
		{
			Global.Logers.SaveSettings();
		}
	}

	public static void SaveGlobalText(string TextError)
	{
		File.AppendAllText(Path.Combine(Global.GerPahtSettings(), "ErrorCrach.log"), DateTime.Now.ToString() + "\r\n" + TextError + "\r\n\r\n");
		if (Global.Logers != null)
		{
			Global.Logers.AddError("Crach system", TextError).Wait();
		}
		if (Global.Logers != null)
		{
			Global.Logers.SaveSettings();
		}
	}
}
