using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KkmFactory;

public static class AddIn
{
	public enum enTypeAddIn
	{
		None,
		Chrome
	}

	public static enTypeAddIn TypeAddIn = enTypeAddIn.None;

	public static List<object> AddInData;

	public static int CounterStatServer = 0;

	public static bool IsRunHttp = false;

	public static string Brauzer = "";

	public static int IdBrauzer = 0;

	public static int CountReadLenZero = 0;

	public static void Run(string[] args)
	{
		if (Global.WorkAsAdmin(args))
		{
			return;
		}
		Global.WriteLog("AddIn_Run - Новый запуск");
		Global.DateStart = DateTime.Now;
		AddInData = new List<object>();
		bool AddInWork = true;
		new Task(delegate
		{
			Global.WriteLog("AddIn_Run - Запуск циклов ответа!");
			while (AddInWork)
			{
				try
				{
					if (Global.UnitManager != null && Global.UnitManager.ExecuteDatas != null)
					{
						foreach (UnitManager.ExecuteData executeData in Global.UnitManager.ExecuteDatas)
						{
							if (executeData.KeyCallback != "" && !executeData.ReturnedAddIn && executeData.RezultCommand.CommandEnd)
							{
								ChromeWrite(executeData.RezultCommand);
								executeData.ReturnedAddIn = true;
								Global.WriteLog("AddIn_Run - Был ответ AddInGetSettings");
							}
						}
					}
					lock (AddInData)
					{
						while (AddInData.Count > 0)
						{
							ChromeWrite(AddInData[0]);
							AddInData.RemoveAt(0);
						}
					}
					Thread.Sleep(100);
				}
				catch
				{
				}
			}
			Global.WriteLog("AddIn_Run - Стоп циклов ответа!");
		}).Start();
		string text = null;
		while (true)
		{
			Thread.Sleep(100);
			if (TypeAddIn == enTypeAddIn.Chrome)
			{
				text = ChromeRead();
			}
			if (text == null)
			{
				if (!Global.ServerRunInAnotherWindow)
				{
					Process.Start(Global.GetStartInfo());
				}
				break;
			}
			if (!(text == ""))
			{
				string text2 = string.Empty;
				try
				{
					text2 = ProcessMessage(text);
				}
				catch
				{
				}
				Global.WriteLog("AddIn_Run - Цикл: " + text);
				if (text2 == "exit" || text2 == "")
				{
					break;
				}
			}
		}
		Global.WriteLog("AddIn_Run - выход");
		AddInWork = false;
	}

	public static string ProcessMessage(string message)
	{
		string text = message.ToLower();
		switch (text)
		{
		default:
			if (text.Length != 0)
			{
				break;
			}
			Global.WriteLog("Команда - " + message);
			return "exit";
		case "test":
			Global.WriteLog("Команда - " + message);
			return "testing!";
		case "exit":
			Global.WriteLog("Команда - " + message);
			return "exit";
		case null:
			break;
		}
		Unit.DataCommand DataCommand = null;
		if (message != null)
		{
			try
			{
				DataCommand = Controllers.JsonToDataCommand(message);
			}
			catch (Exception ex)
			{
				Global.Logers.AddError("<Не опознана>", "Ошибка разбора (парсинга) команды", message, Global.GetErrorMessagee(ex)).Wait();
			}
			Global.WriteLog("Команда - " + DataCommand.Command);
		}
		if (DataCommand.Command.ToLower() == "Exit".ToLower())
		{
			return "exit";
		}
		new Task(delegate
		{
			if (!Global.ServerRunInAnotherWindow)
			{
				Global.WriteLog("Выполняем локально");
				try
				{
					if (message != null)
					{
						string keyCallback = Guid.NewGuid().ToString();
						Global.WriteLog("Выполняем локально - AddCommand");
						Global.UnitManager.AddCommand(DataCommand, "sync", message, keyCallback).Wait();
					}
					return;
				}
				catch (Exception ex2)
				{
					Global.WriteLog("Выполняем локально - ошибка: " + ex2.Message);
					Global.Logers.AddError(DataCommand.Command, "Ошибка выполнения команды", message, Global.GetErrorMessagee(ex2)).Wait();
					if (!Global.IsRun)
					{
						Global.WriteLog("Начинаем переключение на свой сервер");
						if (!IsRunHttp)
						{
							IsRunHttp = true;
							new Task(delegate
							{
								Global.StartServer();
								Global.WriteLog("Статус запуска своего сервера: - " + Global.IsRun);
							}).Start();
						}
					}
					return;
				}
			}
			Global.WriteLog("Выполняем на внешнем сервере");
			Task<HttpResponseMessage> task = null;
			Task<string> task2 = null;
			try
			{
				string text2 = "";
				text2 = ((!(Global.Settings.ServerSertificate == "") && Global.Settings.ServerSertificate != null) ? ("https://localhost:" + Global.Settings.ipPort + "/Execute") : ("http://localhost:" + Global.Settings.ipPort + "/Execute"));
				HttpClient httpClient = new HttpClient(new HttpClientHandler
				{
					AllowAutoRedirect = false,
					ServerCertificateCustomValidationCallback = (HttpRequestMessage Request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => false
				})
				{
					Timeout = TimeSpan.FromMilliseconds(70000.0)
				};
				if (DataCommand.Timeout != 0)
				{
					httpClient.Timeout = TimeSpan.FromMilliseconds((DataCommand.Timeout + 10) * 1000);
				}
				byte[] bytes = Encoding.UTF8.GetBytes(Global.Settings.LoginAdmin + ":" + Global.Settings.PassAdmin);
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytes));
				httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
				message = message.Replace("SetDeviceSettings", "AddInGetSettings");
				task = httpClient.PostAsync(text2, new StringContent(message, Encoding.UTF8, "application/json"));
				task.Wait();
				Global.WriteLog("Выполняем на внешнем сервере: выполнено");
				if (task.Result.StatusCode == HttpStatusCode.OK)
				{
					Global.WriteLog("Выполняем на внешнем сервере: выполнено успешно");
					task2 = task.Result.Content.ReadAsStringAsync();
					task2.Wait();
					string result = task2.Result;
					lock (AddInData)
					{
						AddInData.Add(result);
						return;
					}
				}
			}
			catch (WebException ex3)
			{
				string text3 = "";
				text3 = Global.GetInnerErrorMessagee(ex3.InnerException);
				try
				{
					task2 = task.Result.Content.ReadAsStringAsync();
					task2.Wait();
					text3 = text3 + "<br/>" + task2.Result;
				}
				catch
				{
					Global.WriteLog("Начинаем переключение на свой сервер");
					Global.LoadSettingAsyncs(InitLoad: true).Wait();
					Global.ServerRunInAnotherWindow = false;
					if (!Global.IsRun && !IsRunHttp)
					{
						IsRunHttp = true;
						new Task(delegate
						{
							Global.StartServer();
							Global.WriteLog("Статус запуска своего сервера: - " + Global.IsRun);
						}).Start();
					}
				}
				Unit.RezultCommand item = new Unit.RezultCommand
				{
					Command = DataCommand.Command,
					Status = Unit.ExecuteStatus.Error,
					Error = text3,
					IdCommand = DataCommand.IdCommand
				};
				lock (AddInData)
				{
					AddInData.Add(item);
				}
			}
		}).Start();
		return "command";
	}

	public static string ChromeRead()
	{
		Stream stream = Console.OpenStandardInput();
		int num = 0;
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) == 0)
		{
			return "";
		}
		num = BitConverter.ToInt32(array, 0);
		if (num > 400000)
		{
			return "--";
		}
		if (num == 0)
		{
			CountReadLenZero++;
			if (CountReadLenZero >= 100)
			{
				return null;
			}
		}
		else
		{
			CountReadLenZero = 0;
		}
		char[] array2 = new char[num];
		int i = 0;
		using (StreamReader streamReader = new StreamReader(stream))
		{
			for (; i < num; i += streamReader.Read(array2, i, array2.Length))
			{
				if (streamReader.Peek() < 0)
				{
					break;
				}
			}
		}
		return new string(array2);
	}

	public static void ChromeWrite(byte[] bytes)
	{
		Global.WriteLog("ChromeWrite: Начало передачи в порт");
		Stream stream = Console.OpenStandardOutput();
		Thread.Sleep(10);
		stream.WriteByte((byte)(bytes.Length & 0xFF));
		Thread.Sleep(10);
		stream.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
		stream.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
		stream.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
		stream.Write(bytes, 0, bytes.Length);
		stream.Flush();
		Global.WriteLog("ChromeWrite: КонецПередачи в порт");
	}

	public static void ChromeWrite(Unit.RezultCommand RezultCommand)
	{
		Global.WriteLog("Возврат - " + RezultCommand.Command);
		string text = JsonConvert.SerializeObject(RezultCommand);
		ChromeWrite(Encoding.UTF8.GetBytes(text));
	}

	public static void ChromeWrite(object Rezult)
	{
		string text = ((!(Rezult.GetType() == typeof(string))) ? JsonConvert.SerializeObject(Rezult) : ((string)Rezult));
		Global.WriteLog("Возврат - Тестовое сообщение: " + text.Substring(0, 70));
		ChromeWrite(Encoding.UTF8.GetBytes(text));
	}
}
