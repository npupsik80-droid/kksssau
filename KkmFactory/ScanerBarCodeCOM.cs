using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

public class ScanerBarCodeCOM(Global.DeviceSettings SettDr, int NumUnit) : UnitPort(SettDr, NumUnit)
{
	public Task WorkTask;

	private byte[] ReadData = new byte[8000];

	private int ColReadData;

	private int StateReadData;

	private List<string> BufferBarCodes = new List<string>();

	private bool CreateResponce;

	private bool AddInBuffer;

	private Stopwatch Stopwatch = new Stopwatch();

	private Encoding Encoding;

	public override void LoadParamets()
	{
		UnitVersion = Global.Verson;
		UnitName = "Сканер Штрих-кодов (COM)";
		UnitDescription = "Сканер Штрих-кодов, работающий через COM порт";
		UnitEquipmentType = "СканерШтрихкода";
		UnitInterfaceRevision = 1006.0;
		UnitIntegrationLibrary = false;
		UnitMainDriverInstalled = true;
		UnitDownloadURL = "https://kkmserver.ru";
		NameDevice = UnitName;
		string text = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n<Settings>\r\n    <Page Caption=\"Параметры\">    \r\n        <Group Caption=\"Параметры подключения\">\r\n            <Parameter Name=\"ComId\" Caption=\"COM: порт\" TypeValue=\"String\" >\r\n                <ChoiceList>\r\n                    #ChoiceListCOM#\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"ComSpeed\" Caption=\"COM: скорость\" TypeValue=\"String\" DefaultValue=\"115200\" >\r\n                <ChoiceList>\r\n                    <Item Value=\"2400\">2400</Item>\r\n                    <Item Value=\"4800\">4800</Item>\r\n                    <Item Value=\"9600\">9600</Item>\r\n                    <Item Value=\"19200\">19200</Item>\r\n                    <Item Value=\"38400\">38400</Item>\r\n                    <Item Value=\"57600\">57600</Item>\r\n                    <Item Value=\"115200\">115200</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n        <Group Caption=\"Параметры устройства\">\r\n            <Parameter Name=\"Prefics\" Caption=\"Префикс\" TypeValue=\"String\" DefaultValue=\"\"> \r\n                <ChoiceList>\r\n                    <Item Value=\"\">None</Item>\r\n                    <Item Value=\"0\">0 NUL</Item>\r\n                    <Item Value=\"1\">1 (SOH)</Item>\r\n                    <Item Value=\"2\">2 (STX)</Item>\r\n                    <Item Value=\"3\">3 (ETX)</Item>\r\n                    <Item Value=\"4\">4 (EOT)</Item>\r\n                    <Item Value=\"5\">5 (ENQ)</Item>\r\n                    <Item Value=\"6\">6 (ACK)</Item>\r\n                    <Item Value=\"7\">7 (BEL)</Item>\r\n                    <Item Value=\"8\">8 (BS)</Item>\r\n                    <Item Value=\"9\">9 (TAB)</Item>\r\n                    <Item Value=\"10\">10 (LF)</Item>\r\n                    <Item Value=\"11\">11 (VT)</Item>\r\n                    <Item Value=\"12\">12 (FF)</Item>\r\n                    <Item Value=\"13\">13 (CR)</Item>\r\n                    <Item Value=\"14\">14 (SO)</Item>\r\n                    <Item Value=\"15\">15 (SI)</Item>\r\n                    <Item Value=\"16\">16 (DLE)</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"Suffics\" Caption=\"Суффикс\" TypeValue=\"String\" DefaultValue=\"13\"> \r\n                <ChoiceList>\r\n                    <Item Value=\"0\">0 NUL</Item>\r\n                    <Item Value=\"1\">1 (SOH)</Item>\r\n                    <Item Value=\"2\">2 (STX)</Item>\r\n                    <Item Value=\"3\">3 (ETX)</Item>\r\n                    <Item Value=\"4\">4 (EOT)</Item>\r\n                    <Item Value=\"5\">5 (ENQ)</Item>\r\n                    <Item Value=\"6\">6 (ACK)</Item>\r\n                    <Item Value=\"7\">7 (BEL)</Item>\r\n                    <Item Value=\"8\">8 (BS)</Item>\r\n                    <Item Value=\"9\">9 (TAB)</Item>\r\n                    <Item Value=\"10\">10 (LF)</Item>\r\n                    <Item Value=\"11\">11 (VT)</Item>\r\n                    <Item Value=\"12\">12 (FF)</Item>\r\n                    <Item Value=\"13\">13 (CR)</Item>\r\n                    <Item Value=\"14\">14 (SO)</Item>\r\n                    <Item Value=\"15\">15 (SI)</Item>\r\n                    <Item Value=\"16\">16 (DLE)</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n            <Parameter Name=\"CodePage\" Caption=\"Кодировка\" TypeValue=\"String\" DefaultValue=\"UTF8\"> \r\n                <ChoiceList>\r\n                    <Item Value=\"UTF8\">UTF-8</Item>\r\n                    <Item Value=\"CP1251\">CP-1251</Item>\r\n                </ChoiceList>\r\n            </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		Dictionary<string, string> listComPort = GetListComPort();
		string text2 = "";
		foreach (KeyValuePair<string, string> item in listComPort)
		{
			text2 = text2 + "<Item Value=\"" + item.Key + "\">" + item.Value + "</Item>";
		}
		text = text.Replace("#ChoiceListCOM#", text2);
		LoadParametsFromXML(text);
		string paramsXML = "";
		LoadAdditionalActionsFromXML(paramsXML);
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
		SetPort.TypeConnect = SetPorts.enTypeConnect.Com;
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			string key = unitParamet.Key;
			if (!(key == "ComId"))
			{
				if (key == "ComSpeed")
				{
					SetPort.ComSpeed = unitParamet.Value.AsInt();
				}
			}
			else
			{
				SetPort.ComId = unitParamet.Value.Trim();
			}
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		Error = "";
		if (!SettDr.Active)
		{
			return true;
		}
		if (AddIn.TypeAddIn != AddIn.enTypeAddIn.None)
		{
			await OpenBarcode(new DataCommand(), new RezultCommandBarCode());
		}
		IsInit = true;
		return true;
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public async Task Work()
	{
		if (Global.Settings.RegisterAllCommand)
		{
			Global.Logers.AddError("Work", SettDr, "").Wait();
		}
		Thread.Sleep(15000);
		Encoding Encoding = ((!(UnitParamets["CodePage"] == "UTF8")) ? Encoding.GetEncoding(1251) : Encoding.UTF8);
		Stopwatch Stopwatch = new Stopwatch();
		Stopwatch.Restart();
		try
		{
			await PortOpenAsync();
		}
		catch (Exception ex)
		{
			if (Global.Settings.RegisterAllCommand)
			{
				Global.Logers.AddError("StopWork", SettDr, "Соm-порт не открыт: " + ex.Message).Wait();
			}
			return;
		}
		while (UnitOpen != 0)
		{
			try
			{
				await PortOpenAsync();
				base.PortReadTimeout = 3000;
				byte b = await PortReadByteAsync();
				TimeSpan elapsed = Stopwatch.Elapsed;
				Stopwatch.Restart();
				if (StateReadData != 0 && elapsed.TotalMilliseconds > 200.0)
				{
					StateReadData = 0;
					ColReadData = 0;
					if (Global.Settings.RegisterAllCommand)
					{
						Global.Logers.AddError("ResetBarcodeOnTimeout", SettDr, "").Wait();
					}
				}
				if (StateReadData != 0)
				{
					goto IL_03d4;
				}
				if (b != 10 || !(UnitParamets["Suffics"] == "13"))
				{
					if (UnitParamets["Prefics"] == "")
					{
						StateReadData = 1;
						goto IL_03d4;
					}
					if (byte.Parse(UnitParamets["Prefics"]) == b)
					{
						StateReadData = 1;
						continue;
					}
					StateReadData = 0;
					ColReadData = 0;
				}
				goto end_IL_0267;
				IL_0421:
				if (StateReadData != 2 || ColReadData <= 1)
				{
					continue;
				}
				string text = Encoding.GetString(ReadData, 0, ColReadData);
				if (AddInBuffer)
				{
					lock (BufferBarCodes)
					{
						BufferBarCodes.Add(text);
						if (BufferBarCodes.Count > 10)
						{
							BufferBarCodes.RemoveRange(0, BufferBarCodes.Count - 10);
						}
					}
				}
				if (CreateResponce)
				{
					RezultCommandBarCode rezultCommandBarCode = new RezultCommandBarCode();
					rezultCommandBarCode.Status = ExecuteStatus.Ok;
					rezultCommandBarCode.Error = "";
					rezultCommandBarCode.Command = "EventBarcode";
					rezultCommandBarCode.NumDevice = SettDr.NumDevice;
					rezultCommandBarCode.Event = new RezultCommandBarCode.UnitEvent();
					rezultCommandBarCode.Event.Data = text;
					rezultCommandBarCode.Event.Message = "";
					rezultCommandBarCode.Event.Source = SettDr.NumDevice.ToString();
					UnitManager.ExecuteData executeData = new UnitManager.ExecuteData();
					executeData.KeyCallback = "EventBarcode";
					executeData.NumDevice = SettDr.NumDevice;
					executeData.ReturnedCallback = false;
					executeData.RezultCommand = rezultCommandBarCode;
					executeData.DataCommand = new DataCommand();
					executeData.DataCommand.NumDevice = SettDr.NumDevice;
					executeData.DataCommand.Command = "EventBarcode";
					executeData.DateRun = DateTime.Now;
					executeData.DateStart = executeData.DateRun;
					executeData.DateEnd = executeData.DateRun;
					executeData.Type = TypeDevice.enType.СканерШтрихкода;
					if (Global.UnitManager.IsExecuteDatas.Semaphore.Wait(new TimeSpan(0, 0, 1)))
					{
						try
						{
							if (Global.UnitManager.ExecuteDatas.Semaphore.Wait(new TimeSpan(0, 0, 1)))
							{
								try
								{
									Global.UnitManager.ExecuteDatas.Enqueue(executeData);
									if (Global.Settings.RegisterAllCommand)
									{
										Global.Logers.AddError(this, executeData, "").Wait();
									}
									NetLogs.Clear();
								}
								catch (Exception ex2)
								{
									Global.Logers.AddError(this, executeData, "Ошибка поставновки в очередь выполненых команд: " + ex2.Message).Wait();
								}
								finally
								{
									Global.UnitManager.ExecuteDatas.Semaphore.Release();
								}
							}
							else if (Global.Settings.RegisterAllCommand)
							{
								Global.Logers.AddError(this, executeData, "Не удалось захватить ExecuteDatas").Wait();
							}
						}
						catch (Exception ex3)
						{
							Global.Logers.AddError(this, executeData, "Не удалось захватить очередь IsExecuteDatas: " + ex3.Message).Wait();
						}
						finally
						{
							Global.UnitManager.IsExecuteDatas.Semaphore.Release();
						}
					}
					else if (Global.Settings.RegisterAllCommand)
					{
						Global.Logers.AddError(this, executeData, "Не удалось захватить IsExecuteDatas").Wait();
					}
					executeData.RezultCommand.CommandEnd = true;
				}
				StateReadData = 0;
				ColReadData = 0;
				goto end_IL_0267;
				IL_03d4:
				if (StateReadData != 1)
				{
					goto IL_0421;
				}
				if (byte.Parse(UnitParamets["Suffics"]) == b)
				{
					StateReadData = 2;
					goto IL_0421;
				}
				ReadData[ColReadData] = b;
				ColReadData++;
				end_IL_0267:;
			}
			catch (System.ServiceProcess.TimeoutException)
			{
				StateReadData = 0;
				ColReadData = 0;
			}
			catch (Exception ex5)
			{
				StateReadData = 0;
				ColReadData = 0;
				TimeSpan elapsed = Stopwatch.Elapsed;
				Stopwatch.Restart();
				if (Global.Settings.RegisterAllCommand)
				{
					UnitManager.ExecuteData executeData2 = new UnitManager.ExecuteData();
					executeData2.RezultCommand = new RezultCommandBarCode();
					executeData2.DataCommand = new DataCommand();
					executeData2.DataCommand.Command = "EventBarcode";
					string text2 = "IsOpen: " + SetPort.SerialPort.IsOpen + ", PortName: " + SetPort.SerialPort.PortName + ", ReadTimeout: " + SetPort.SerialPort.ReadTimeout + ", BytesToRead: " + SetPort.SerialPort.BytesToRead;
					try
					{
						Global.Logers.AddError(this, executeData2, "Ошибка сканирования: " + ex5.Message + ", ComStatus = " + text2).Wait();
					}
					catch
					{
					}
				}
				if (elapsed.TotalMilliseconds < 2000.0)
				{
					if (Global.Settings.RegisterAllCommand)
					{
						try
						{
							Global.Logers.AddError("EventBarcode", "PortClose по Timeout < 2 сек.").Wait();
						}
						catch
						{
						}
					}
					try
					{
						await PortCloseAsync();
					}
					catch
					{
					}
				}
				if (!Global.AllCancellationToken.Token.IsCancellationRequested)
				{
					continue;
				}
				if (Global.Settings.RegisterAllCommand)
				{
					try
					{
						Global.Logers.AddError("EventBarcode", "Выход по Token.IsCancel").Wait();
					}
					catch
					{
					}
				}
				try
				{
					await PortCloseAsync();
					return;
				}
				catch
				{
					return;
				}
			}
		}
		try
		{
			await PortCloseAsync();
		}
		catch
		{
		}
		if (Global.Settings.RegisterAllCommand)
		{
			Global.Logers.AddError("StopWork", SettDr, "").Wait();
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
	{
		SerialPort serialPort = (SerialPort)sender;
		while (serialPort.BytesToRead >= 1)
		{
			try
			{
				byte b = (byte)serialPort.ReadByte();
				TimeSpan elapsed = Stopwatch.Elapsed;
				Stopwatch.Restart();
				if (StateReadData != 0 && elapsed.TotalMilliseconds > 200.0)
				{
					StateReadData = 0;
					ColReadData = 0;
					if (Global.Settings.RegisterAllCommand)
					{
						Global.Logers.AddError("ResetBarcodeOnTimeout", SettDr, "").Wait();
					}
				}
				if (StateReadData != 0)
				{
					goto IL_010c;
				}
				if (b != 10 || !(UnitParamets["Suffics"] == "13"))
				{
					if (UnitParamets["Prefics"] == "")
					{
						StateReadData = 1;
						goto IL_010c;
					}
					if (byte.Parse(UnitParamets["Prefics"]) == b)
					{
						StateReadData = 1;
						continue;
					}
					StateReadData = 0;
					ColReadData = 0;
				}
				goto end_IL_000d;
				IL_0157:
				if (StateReadData != 2 || ColReadData <= 1)
				{
					continue;
				}
				string text = Encoding.GetString(ReadData, 0, ColReadData);
				if (AddInBuffer)
				{
					lock (BufferBarCodes)
					{
						BufferBarCodes.Add(text);
						if (BufferBarCodes.Count > 10)
						{
							BufferBarCodes.RemoveRange(0, BufferBarCodes.Count - 10);
						}
					}
				}
				if (CreateResponce)
				{
					RezultCommandBarCode rezultCommandBarCode = new RezultCommandBarCode();
					rezultCommandBarCode.Status = ExecuteStatus.Ok;
					rezultCommandBarCode.Error = "";
					rezultCommandBarCode.Command = "EventBarcode";
					rezultCommandBarCode.NumDevice = SettDr.NumDevice;
					rezultCommandBarCode.Event = new RezultCommandBarCode.UnitEvent();
					rezultCommandBarCode.Event.Data = text;
					rezultCommandBarCode.Event.Message = "";
					rezultCommandBarCode.Event.Source = SettDr.NumDevice.ToString();
					UnitManager.ExecuteData executeData = new UnitManager.ExecuteData();
					executeData.KeyCallback = "EventBarcode";
					executeData.NumDevice = SettDr.NumDevice;
					executeData.ReturnedCallback = false;
					executeData.RezultCommand = rezultCommandBarCode;
					executeData.DataCommand = new DataCommand();
					executeData.DataCommand.NumDevice = SettDr.NumDevice;
					executeData.DataCommand.Command = "EventBarcode";
					executeData.DateRun = DateTime.Now;
					executeData.DateStart = executeData.DateRun;
					executeData.DateEnd = executeData.DateRun;
					executeData.Type = TypeDevice.enType.СканерШтрихкода;
					if (Global.UnitManager.IsExecuteDatas.Semaphore.Wait(new TimeSpan(0, 0, 1)))
					{
						try
						{
							if (Global.UnitManager.ExecuteDatas.Semaphore.Wait(new TimeSpan(0, 0, 1)))
							{
								try
								{
									Global.UnitManager.ExecuteDatas.Enqueue(executeData);
									if (Global.Settings.RegisterAllCommand)
									{
										Global.Logers.AddError(this, executeData, "").Wait();
									}
									NetLogs.Clear();
								}
								finally
								{
									Global.UnitManager.ExecuteDatas.Semaphore.Release();
								}
							}
						}
						finally
						{
							Global.UnitManager.IsExecuteDatas.Semaphore.Release();
						}
					}
					executeData.RezultCommand.CommandEnd = true;
				}
				StateReadData = 0;
				ColReadData = 0;
				goto end_IL_000d;
				IL_010c:
				if (StateReadData != 1)
				{
					goto IL_0157;
				}
				if (byte.Parse(UnitParamets["Suffics"]) == b)
				{
					StateReadData = 2;
					goto IL_0157;
				}
				ReadData[ColReadData] = b;
				ColReadData++;
				end_IL_000d:;
			}
			catch (Exception)
			{
				StateReadData = 0;
				ColReadData = 0;
			}
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task GetBarcode(DataCommand DataCommand, RezultCommandBarCode RezultCommand)
	{
		if (Global.Settings.RegisterAllCommand)
		{
			await Global.Logers.AddError("GetBarcode", SettDr, "");
		}
		try
		{
			await Semaphore.WaitAsync();
			UnitOpen++;
			AddInBuffer = true;
			if (UnitOpen == 1)
			{
				WriteParametsToUnits();
				bool flag = await PortOpenAsync();
				if (Error != "" || !flag)
				{
					return;
				}
				if (UnitParamets["CodePage"] == "UTF8")
				{
					Encoding = Encoding.UTF8;
				}
				else
				{
					Encoding = Encoding.GetEncoding(1251);
				}
				Stopwatch.Restart();
				SetPort.SerialPort.DataReceived += DataReceivedHandler;
			}
		}
		finally
		{
			Semaphore.Release();
		}
		DateTime dateTime = DateTime.Now.AddSeconds(10.0);
		while (dateTime > DateTime.Now)
		{
			lock (BufferBarCodes)
			{
				if (BufferBarCodes.Count != 0)
				{
					string data = BufferBarCodes[0];
					BufferBarCodes.RemoveAt(0);
					RezultCommand.Event = new RezultCommandBarCode.UnitEvent();
					RezultCommand.Event.Data = data;
				}
			}
			if (RezultCommand.Event != null)
			{
				break;
			}
			Thread.Sleep(200);
		}
		new Task(async delegate
		{
			Thread.Sleep(5000);
			try
			{
				await Semaphore.WaitAsync();
				UnitOpen--;
				if (UnitOpen == 0)
				{
					AddInBuffer = false;
					try
					{
						await PortCloseAsync();
						Stopwatch.Stop();
					}
					catch
					{
					}
					SetPort.SerialPort = null;
				}
			}
			finally
			{
				Semaphore.Release();
			}
		}).Start();
		if (Global.Settings.RegisterAllCommand)
		{
			await Global.Logers.AddError("EndGetBarcode", SettDr, "");
		}
		await ComDevice.PostCheck(RezultCommand, this, NewTask: true);
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task OpenBarcode(DataCommand DataCommand, RezultCommandBarCode RezultCommand)
	{
		_ = 2;
		try
		{
			await Semaphore.WaitAsync();
			if (SetPort.SerialPort == null || !SetPort.SerialPort.IsOpen)
			{
				UnitOpen = 0;
			}
			UnitOpen++;
			CreateResponce = true;
			if (Global.Settings.RegisterAllCommand)
			{
				await Global.Logers.AddError("OpenBarcode", SettDr, "");
			}
			if (UnitOpen == 1)
			{
				WriteParametsToUnits();
				bool flag = await PortOpenAsync();
				if (Error != "" || !flag)
				{
					return;
				}
				if (UnitParamets["CodePage"] == "UTF8")
				{
					Encoding = Encoding.UTF8;
				}
				else
				{
					Encoding = Encoding.GetEncoding(1251);
				}
				Stopwatch.Restart();
				SetPort.SerialPort.DataReceived += DataReceivedHandler;
			}
		}
		finally
		{
			Semaphore.Release();
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task CloseBarcode(DataCommand DataCommand, RezultCommandBarCode RezultCommand)
	{
		if (UnitOpen > 0)
		{
			UnitOpen--;
		}
		if (UnitOpen == 0)
		{
			try
			{
				await PortCloseAsync();
				Stopwatch.Stop();
			}
			catch
			{
			}
			SetPort.SerialPort = null;
			CreateResponce = false;
		}
		RezultCommand.Status = ExecuteStatus.Ok;
		if (Global.Settings.RegisterAllCommand)
		{
			await Global.Logers.AddError("CloseBarcode", SettDr, "");
		}
	}
}
