using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace KkmFactory;

public class UnitPort : Unit
{
	public class SetPorts
	{
		public enum enTypeConnect
		{
			None = 0,
			IP = 1,
			Com = 2,
			Usb = 3,
			Bluetooth = 4,
			Software = 99
		}

		private UnitPort Unit;

		public enTypeConnect TypeConnect;

		public string IP = "";

		public string Port = "";

		public string ComId = "COM1";

		public int ComSpeed = 115200;

		public string USBPort = "";

		public string BluetoothPort = "";

		public bool PortOpen;

		public bool ErrorOff = true;

		public SerialPort SerialPort;

		public Socket Socket;

		public char NetLogDirection = ' ';

		public SetPorts(UnitPort Unit)
		{
			this.Unit = Unit;
		}
	}

	[Serializable]
	public class CancellationToken : Exception
	{
		public CancellationToken(string message)
			: base(message)
		{
		}
	}

	private static Dictionary<string, string> ListComPort = new Dictionary<string, string>();

	private static DateTime DateListComPort = default(DateTime);

	public CancellationTokenSource IpCancellToken;

	public SetPorts SetPort;

	private byte[] Response = new byte[1];

	private int ResponseCount;

	private SocketFlags SocketFlags;

	public const byte Prt_ENQ = 5;

	public const byte Prt_ACK = 6;

	public const byte Prt_STX = 2;

	public const byte Prt_ETX = 3;

	public const byte Prt_EOT = 4;

	public const byte Prt_LF = 10;

	public const byte Prt_NAK = 21;

	public const byte Prt_DLE = 16;

	public const byte Prt_FS = 28;

	public const byte Prt_SEP = 179;

	public const byte Prt_RS = 30;

	public const byte Prt_SOH = 1;

	public int PortReadTimeout
	{
		get
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
			{
				if (SetPort.SerialPort != null)
				{
					return SetPort.SerialPort.ReadTimeout;
				}
			}
			else if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP && SetPort.Socket != null)
			{
				return SetPort.Socket.ReceiveTimeout;
			}
			return 0;
		}
		set
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
			{
				if (SetPort.SerialPort != null)
				{
					SetPort.SerialPort.ReadTimeout = value;
				}
			}
			else if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP && SetPort.Socket != null)
			{
				SetPort.Socket.ReceiveTimeout = value;
			}
		}
	}

	public int PortWriteTimeout
	{
		get
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
			{
				return SetPort.SerialPort.WriteTimeout;
			}
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
			{
				return SetPort.Socket.SendTimeout;
			}
			return 0;
		}
		set
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
			{
				SetPort.SerialPort.WriteTimeout = value;
			}
			else if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
			{
				SetPort.Socket.SendTimeout = value;
			}
		}
	}

	public UnitPort(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		SetPort = new SetPorts(this);
	}

	public override async Task ExecuteCommand(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		ClearNetLogs();
		await base.ExecuteCommand(DataCommand, RezultCommand);
	}

	public void CheckAllCancellationToken()
	{
		if (Global.AllCancellationToken.Token.IsCancellationRequested)
		{
			throw new CancellationToken("Команда прервана из за остановки сервера");
		}
	}

	public Dictionary<string, string> GetListComPort(string AddMask = null)
	{
		if (DateTime.Now.Subtract(DateListComPort).TotalSeconds <= 5.0)
		{
			return ListComPort;
		}
		lock (ListComPort)
		{
			if (DateTime.Now.Subtract(DateListComPort).TotalSeconds <= 5.0)
			{
				return ListComPort;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] portNames = SerialPort.GetPortNames();
			foreach (string text in portNames)
			{
				if (!dictionary.ContainsKey(text))
				{
					dictionary.Add(text, HttpUtility.HtmlEncode("Порт: " + text));
				}
			}
			List<string> list = new List<string>();
			List<ManagementObject> list2 = new List<ManagementObject>();
			try
			{
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
				list2 = managementObjectSearcher.Get().Cast<ManagementObject>().ToList();
				managementObjectSearcher.Dispose();
			}
			catch (Exception)
			{
				return dictionary;
			}
			foreach (ManagementObject item in list2)
			{
				object obj = item["Caption"];
				if (obj != null)
				{
					string text2 = obj.ToString();
					if (text2.Contains("(COM"))
					{
						list.Add(text2);
					}
				}
			}
			portNames = (from s in list.Distinct()
				orderby s
				select s).ToArray();
			foreach (string text3 in portNames)
			{
				if (text3 == null)
				{
					continue;
				}
				int num = text3.IndexOf("(COM", StringComparison.Ordinal) + 1;
				if (num < 0)
				{
					continue;
				}
				string text4 = text3.Remove(num).Trim('(');
				string text5 = text3.Replace(text4, "").TrimStart('(').TrimEnd(')');
				string text6 = ((text5.Length <= 4) ? text5 : text5.Substring(0, 4));
				if (text5.Length > 4)
				{
					text6 = text5.Substring(0, 5);
					if (!char.IsDigit(text6.Last()))
					{
						text6.Remove(text6.Length - 1);
					}
				}
				if (dictionary.ContainsKey(text6))
				{
					dictionary.Remove(text6);
					dictionary.Add(text6, HttpUtility.HtmlEncode("Порт: " + text6 + " - " + text4));
				}
			}
			ListComPort = dictionary;
			DateListComPort = DateTime.Now;
		}
		if (SettDr.Paramets.ContainsKey("ComId") && !ListComPort.ContainsKey(SettDr.Paramets["ComId"]))
		{
			ListComPort.Add(SettDr.Paramets["ComId"], "Порт: " + SettDr.Paramets["ComId"] + " - (порт не найден)");
		}
		return ListComPort;
	}

	public override async Task<bool> PortOpenAsync(Parity Parity = Parity.None)
	{
		CheckAllCancellationToken();
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			return await PortComOpenAsync(Parity);
		}
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			return await PortIpOpenAsync();
		}
		PortLogs.Stopwatch.Stop();
		return false;
	}

	public override async Task<bool> PortOpenAsync()
	{
		CheckAllCancellationToken();
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			return await PortComOpenAsync();
		}
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			return await PortIpOpenAsync();
		}
		PortLogs.Stopwatch.Stop();
		return false;
	}

	public override async Task<bool> PortCloseAsync()
	{
		CheckAllCancellationToken();
		PortLogs.Stopwatch.Stop();
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			return await PortComCloseAsync();
		}
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			return await PortIpCloseAsync();
		}
		return false;
	}

	public virtual async Task<bool> PortIpOpenAsync()
	{
		if (SetPort.Socket != null && SetPort.Socket.Connected)
		{
			SetPort.PortOpen = true;
			return false;
		}
		if (SetPort.IP == "" || SetPort.Port == "")
		{
			SetPort.PortOpen = false;
			return false;
		}
		try
		{
			int.Parse(SetPort.Port);
		}
		catch (Exception)
		{
			Error = "Не правильно задан IP порт соединения.";
			return false;
		}
		IPAddress iPAddress;
		try
		{
			iPAddress = IPAddress.Parse(SetPort.IP);
		}
		catch
		{
			IPAddress[] hostAddresses;
			try
			{
				hostAddresses = Dns.GetHostAddresses(SetPort.IP);
			}
			catch
			{
				Error = "Не удалось получить IP адрес имени хоста: " + SetPort.IP;
				return false;
			}
			if (hostAddresses.Length == 0)
			{
				Error = "Не удалось получить IP адрес имени хоста: " + SetPort.IP;
				return false;
			}
			iPAddress = hostAddresses[0];
		}
		IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, int.Parse(SetPort.Port));
		SetPort.Socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		SetPort.Socket.ReceiveTimeout = 2000;
		SetPort.Socket.SendTimeout = 2000;
		string text = "";
		try
		{
			if (!SetPort.ErrorOff)
			{
				SetPort.Socket.Connect(iPEndPoint);
			}
			else
			{
				if (!SetPort.Socket.BeginConnect(iPAddress, int.Parse(SetPort.Port), null, null).AsyncWaitHandle.WaitOne(4000, true))
				{
					SetPort.Socket.Close();
					SetPort.Socket.Dispose();
					SetPort.Socket = null;
					throw new ApplicationException("Ошибка установки соединения.");
				}
				if (!SetPort.Socket.Connected)
				{
					SetPort.Socket.Close();
					SetPort.Socket.Dispose();
					SetPort.Socket = null;
					throw new ApplicationException("Ошибка установки соединения.");
				}
			}
		}
		catch (SecurityException)
		{
			text = "Отказ в доступе к сокету";
			IsInit = false;
		}
		catch (Exception ex3)
		{
			text = "Ошибка открытия сокета: " + Global.GetErrorMessagee(ex3);
			IsInit = false;
		}
		if (text != "")
		{
			Error = text;
			PortLogs.Append(Error);
			SetPort.PortOpen = false;
			return false;
		}
		SetPort.PortOpen = true;
		PortLogs.Append("Socket открыт.");
		PortLogs.Stopwatch.Start();
		return true;
	}

	public virtual async Task<bool> PortIpCloseAsync()
	{
		if (SetPort.Socket != null && SetPort.Socket.Connected)
		{
			try
			{
				SetPort.Socket.Shutdown(SocketShutdown.Both);
				SetPort.Socket.Close();
				SetPort.Socket.Dispose();
				PortLogs.Append("Socket закрыт.");
			}
			catch (Exception)
			{
			}
		}
		SetPort.PortOpen = false;
		return true;
	}

	public virtual async Task<bool> PortComOpenAsync(Parity Parity = Parity.None)
	{
		if (SetPort.SerialPort != null && SetPort.SerialPort.IsOpen)
		{
			SetPort.PortOpen = true;
			return false;
		}
		if (SetPort.ComId == "")
		{
			SetPort.PortOpen = false;
			return false;
		}
		if (SetPort.SerialPort == null)
		{
			if (SetPort.ComSpeed == -1)
			{
				SetPort.SerialPort = new SerialPort(SetPort.ComId);
			}
			else
			{
				switch (Parity)
				{
				case Parity.None:
					SetPort.SerialPort = new SerialPort(SetPort.ComId, SetPort.ComSpeed);
					break;
				case Parity.Even:
					SetPort.SerialPort = new SerialPort(SetPort.ComId, SetPort.ComSpeed, Parity, 8, StopBits.One);
					break;
				default:
					SetPort.SerialPort = new SerialPort(SetPort.ComId, SetPort.ComSpeed, Parity);
					break;
				}
			}
		}
		SetPort.SerialPort.ReadTimeout = 2000;
		SetPort.SerialPort.WriteTimeout = 2000;
		string text = "";
		int num = 3;
		for (int i = 0; i < num; i++)
		{
			text = "";
			try
			{
				SetPort.SerialPort.Open();
				if (SetPort.SerialPort.IsOpen)
				{
					break;
				}
			}
			catch (UnauthorizedAccessException)
			{
				text = "Отказ в доступе к порту или порт уже открыт";
				Thread.Sleep(200);
			}
			catch (Exception ex2)
			{
				text = "Ошибка открытия порта: " + Global.GetErrorMessagee(ex2);
				Thread.Sleep(200);
			}
		}
		if (text != "")
		{
			IsInit = false;
		}
		if (text != "")
		{
			Error = text;
			PortLogs.Append(Error);
			SetPort.PortOpen = false;
			return false;
		}
		SetPort.PortOpen = true;
		PortLogs.Append("COM порт открыт.");
		PortLogs.Stopwatch.Start();
		return true;
	}

	public virtual async Task<bool> PortComCloseAsync()
	{
		if (SetPort.SerialPort != null && SetPort.SerialPort.IsOpen)
		{
			SetPort.SerialPort.Close();
			SetPort.SerialPort.Dispose();
			SetPort.SerialPort = null;
			PortLogs.Append("COM порт закрыт.");
		}
		SetPort.PortOpen = false;
		return true;
	}

	public virtual async Task PortWriteAsync(byte[] buffer, int offset, int count)
	{
		CheckAllCancellationToken();
		CheckTime();
		try
		{
			if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
			{
				if (SetPort.SerialPort.WriteTimeout == 0)
				{
					SetPort.SerialPort.WriteTimeout = 200;
				}
				SetPort.SerialPort.Write(buffer, offset, count);
			}
			else if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
			{
				if (SetPort.Socket.SendTimeout == 0)
				{
					SetPort.Socket.SendTimeout = 200;
				}
				if (!SetPort.ErrorOff)
				{
					SetPort.Socket.Send(buffer, offset, count, SocketFlags);
				}
				else if (!SetPort.Socket.BeginSend(buffer, offset, count, SocketFlags, null, null).AsyncWaitHandle.WaitOne(SetPort.Socket.SendTimeout + 100, true))
				{
					throw new ApplicationException("Ошибка передачи кадра сообщения.");
				}
			}
		}
		catch (Exception)
		{
			PortLogs.Append("Error write", "<");
			throw;
		}
		PortLogs.Write(buffer);
	}

	public virtual async Task PortWriteByteAsync(byte Value)
	{
		byte[] buffer = new byte[1] { Value };
		await PortWriteAsync(buffer, 0, 1);
	}

	public virtual async Task<byte> PortReadByteAsync(bool IsNotTimeout = false)
	{
		CheckAllCancellationToken();
		CheckTime();
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.Com)
		{
			int readTimeout = SetPort.SerialPort.ReadTimeout;
			if (SetPort.SerialPort.ReadTimeout == 0)
			{
				SetPort.SerialPort.ReadTimeout = 200;
			}
			if (IsNotTimeout)
			{
				SetPort.SerialPort.ReadTimeout = 0;
			}
			byte b = 0;
			try
			{
				b = (byte)SetPort.SerialPort.ReadByte();
				PortLogs.Read(new byte[1] { b });
			}
			catch (Exception ex)
			{
				PortLogs.Append("Error read");
				throw new Exception("Ошибка приема кадра сообщения: " + Global.GetErrorMessagee(ex), ex);
			}
			finally
			{
				if (IsNotTimeout)
				{
					SetPort.SerialPort.ReadTimeout = readTimeout;
				}
			}
			return b;
		}
		if (SetPort.TypeConnect == SetPorts.enTypeConnect.IP)
		{
			int receiveTimeout = SetPort.Socket.ReceiveTimeout;
			if (SetPort.Socket.ReceiveTimeout == 0)
			{
				SetPort.Socket.ReceiveTimeout = 200;
			}
			if (IsNotTimeout)
			{
				SetPort.Socket.ReceiveTimeout = 1;
			}
			try
			{
				if (!SetPort.ErrorOff)
				{
					ResponseCount = SetPort.Socket.Receive(Response, 1, SocketFlags);
				}
				else
				{
					ResponseCount = 0;
					if (!SetPort.Socket.BeginReceive(Response, 0, 1, SocketFlags, null, null).AsyncWaitHandle.WaitOne(SetPort.Socket.ReceiveTimeout + 100, true))
					{
						throw new ApplicationException("Ошибка приема кадра сообщения.");
					}
					ResponseCount = 1;
				}
				if (ResponseCount == 1)
				{
					PortLogs.Read(new byte[1] { Response[0] });
					return Response[0];
				}
				throw new Exception("Нет данных");
			}
			catch (Exception ex2)
			{
				PortLogs.Append("Error read");
				throw new Exception("Ошибка приема кадра сообщения: " + Global.GetErrorMessagee(ex2));
			}
			finally
			{
				if (IsNotTimeout)
				{
					SetPort.Socket.ReceiveTimeout = receiveTimeout;
				}
			}
		}
		return 0;
	}

	private void CheckTime()
	{
		if (StartCommandDate.AddSeconds(PeriodWorkCommand) < DateTime.Now)
		{
			throw new CancellationToken("Время исполнения команды истекло. Исполнение команды прервано.");
		}
	}

	public void ClearNetLogs()
	{
		SetPort.NetLogDirection = ' ';
	}
}
