using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KkmFactory;

public class Logers
{
	[DataContract]
	public class ItemLog
	{
		[DataMember]
		public int NumUnit;

		[DataMember]
		public string TypeDevice = "";

		[DataMember]
		public string IdDevice = "";

		[DataMember]
		public string NameDevice = "";

		[DataMember]
		public string NumberKkm = "";

		[DataMember]
		public string INN = "";

		[DataMember]
		public string Command = "";

		[DataMember]
		public string Error = "";

		[DataMember]
		public string TextCommand = "";

		[DataMember]
		public string NetLogs = "";

		[DataMember]
		public string Rezult = "";
	}

	[DataContract]
	public class Operation
	{
		[DataMember]
		public int NumUnit;

		[DataMember]
		public string INN = "";

		[DataMember]
		public string NameOperation = "";

		[DataMember]
		public decimal Summ;

		[DataMember]
		public TypeDevice.enType DeviceType;

		[DataMember]
		public string Comment = "";

		[DataMember(Name = "UID", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string UID;

		[DataMember(Name = "TextCommand", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string TextCommand;
	}

	[DataContract]
	public class Logs
	{
		[DataContract]
		public class INNItemPerfomance
		{
			[DataMember]
			public string INN = "";

			[DataMember]
			public int KkmCount;

			[DataMember]
			public int KkmNotActive;

			[DataMember]
			public int KkmNotInit;

			[DataMember]
			public long CommandMaxQueue;

			[DataMember]
			public long CommandMaxRun;

			[DataMember]
			public int CommandCount;

			[DataMember]
			public int CommandError;

			[DataMember]
			public int CommandWork;

			[DataMember]
			public long CommandWorkTime;

			[DataMember]
			public long CommandMaxWorkTime;

			[DataMember]
			public int CommandQueue;

			[DataMember]
			public long CommandQueueTime;

			[DataMember]
			public long CommandMaxQueueTime;
		}

		[DataContract]
		public class ItemPerfomance
		{
			[DataMember]
			public DateTime Date;

			[DataMember]
			public int KkmCount;

			[DataMember]
			public int KkmNotActive;

			[DataMember]
			public int KkmNotInit;

			[DataMember]
			public long CommandMaxQueue;

			[DataMember]
			public long CommandMaxRun;

			[DataMember]
			public int CommandCount;

			[DataMember]
			public int CommandError;

			[DataMember]
			public int CommandWork;

			[DataMember]
			public long CommandWorkTime;

			[DataMember]
			public long CommandMaxWorkTime;

			[DataMember]
			public int CommandQueue;

			[DataMember]
			public long CommandQueueTime;

			[DataMember]
			public long CommandMaxQueueTime;

			[DataMember]
			public long Memory;

			public SortedList<string, INNItemPerfomance> INN = new SortedList<string, INNItemPerfomance>();
		}

		[DataMember]
		public Queue<ItemPerfomance> SecStackPerfomance = new Queue<ItemPerfomance>(240);

		[DataMember]
		public Queue<ItemPerfomance> MinStackPerfomance = new Queue<ItemPerfomance>(240);

		[DataMember]
		public Queue<ItemPerfomance> Min5StackPerfomance = new Queue<ItemPerfomance>(240);
	}

	public const int CountSec = 240;

	public const int CountMin = 240;

	public const int CountMin5 = 240;

	public const int CountLogs = 200;

	public DateTime DateLastLog = DateTime.Now;

	public FileLog<ItemLog> CommandLogs = new FileLog<ItemLog>("Logs.dta");

	public FileLog<Operation> OperationsHistory = new FileLog<Operation>("OperationsHistory.dta");

	public Logs Log = new Logs();

	private Logs.ItemPerfomance SecCur;

	private Logs.ItemPerfomance MinCur;

	private Logs.ItemPerfomance Min5Cur;

	public object LockFile = new object();

	public Logers()
	{
		LoadSettings();
		SecCur = new Logs.ItemPerfomance();
		SecCur.Date = DateTime.Now;
		MinCur = new Logs.ItemPerfomance();
		MinCur.Date = DateTime.Now;
		Min5Cur = new Logs.ItemPerfomance();
		Min5Cur.Date = DateTime.Now;
	}

	public void SaveSettings(object state = null)
	{
		CommandLogs.Save();
		OperationsHistory.Save();
		try
		{
			string text = Path.Combine(Global.GerPahtSettings(), "Logs.dat");
			FileStream fileStream = null;
			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Logs));
			fileStream = File.Open(text, FileMode.Create);
			dataContractJsonSerializer.WriteObject(fileStream, Log);
			fileStream.Close();
		}
		catch
		{
		}
	}

	public void LoadSettings()
	{
		string text = Path.Combine(Global.GerPahtSettings(), "Logs.dat");
		FileStream fileStream = null;
		try
		{
			fileStream = File.Open(text, FileMode.Open);
		}
		catch (Exception)
		{
			return;
		}
		DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Logs));
		try
		{
			Log = (Logs)dataContractJsonSerializer.ReadObject(fileStream);
			if (Log == null)
			{
				Log = new Logs();
			}
			else
			{
				if (Log.SecStackPerfomance == null)
				{
					Log.SecStackPerfomance = new Queue<Logs.ItemPerfomance>(240);
				}
				if (Log.MinStackPerfomance == null)
				{
					Log.MinStackPerfomance = new Queue<Logs.ItemPerfomance>(240);
				}
				if (Log.Min5StackPerfomance == null)
				{
					Log.Min5StackPerfomance = new Queue<Logs.ItemPerfomance>(240);
				}
			}
		}
		catch (Exception)
		{
			Log = new Logs();
		}
		fileStream?.Close();
	}

	public void Update(object stat)
	{
		if (!Global.Settings.StatisticsСollection)
		{
			return;
		}
		lock (Log)
		{
			try
			{
				SecCur.Memory = Math.Max(SecCur.Memory, GC.GetTotalMemory(false));
				DateTime now = DateTime.Now;
				bool flag = SecCur.Date < now.AddSeconds(-1.0);
				bool flag2 = MinCur.Date < now.AddMinutes(-1.0);
				bool flag3 = Min5Cur.Date < now.AddMinutes(-5.0);
				if (flag)
				{
					try
					{
						int num = 0;
						int num2 = 0;
						foreach (UnitManager.IsExecuteData isExecuteData in Global.UnitManager.IsExecuteDatas)
						{
							if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun)
							{
								num++;
							}
							if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Run)
							{
								num2++;
							}
						}
						SecCur.CommandMaxQueue = Math.Max(SecCur.CommandMaxQueue, num);
						SecCur.CommandMaxRun = Math.Max(SecCur.CommandMaxRun, num2);
						foreach (KeyValuePair<int, Unit> unit in Global.UnitManager.Units)
						{
							if (unit.Value == null)
							{
								continue;
							}
							SecCur.KkmCount++;
							SecCur.KkmNotActive += ((!unit.Value.Active) ? 1 : 0);
							SecCur.KkmNotInit += ((unit.Value.Active && !unit.Value.IsInit) ? 1 : 0);
							if (SecCur.INN == null)
							{
								SecCur.INN = new SortedList<string, Logs.INNItemPerfomance>();
							}
							if (!SecCur.INN.ContainsKey(unit.Value.Kkm.INN))
							{
								SecCur.INN.Add(unit.Value.Kkm.INN, new Logs.INNItemPerfomance());
							}
							Logs.INNItemPerfomance iNNItemPerfomance = SecCur.INN[unit.Value.Kkm.INN];
							iNNItemPerfomance.INN = unit.Value.Kkm.INN;
							iNNItemPerfomance.KkmCount++;
							iNNItemPerfomance.KkmNotActive += ((!unit.Value.Active) ? 1 : 0);
							iNNItemPerfomance.KkmNotInit += ((unit.Value.Active && !unit.Value.IsInit) ? 1 : 0);
							int num3 = 0;
							int num4 = 0;
							foreach (UnitManager.IsExecuteData isExecuteData2 in Global.UnitManager.IsExecuteDatas)
							{
								if (isExecuteData2.ExecuteData.INN == unit.Value.Kkm.INN && isExecuteData2.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun)
								{
									num3++;
								}
								if (isExecuteData2.ExecuteData.INN == unit.Value.Kkm.INN && isExecuteData2.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Run)
								{
									num4++;
								}
							}
							iNNItemPerfomance.CommandMaxQueue = Math.Max(iNNItemPerfomance.CommandMaxQueue, num3);
							iNNItemPerfomance.CommandMaxRun = Math.Max(iNNItemPerfomance.CommandMaxRun, num4);
						}
					}
					catch
					{
					}
				}
				if (flag)
				{
					CopyLogs(SecCur, MinCur);
				}
				if (flag2)
				{
					CopyLogs(MinCur, Min5Cur);
				}
				if (flag)
				{
					Log.SecStackPerfomance.Enqueue(SecCur);
					SecCur = new Logs.ItemPerfomance();
					SecCur.Date = DateTime.Now;
					while (Log.SecStackPerfomance.Count > 240)
					{
						Log.SecStackPerfomance.Dequeue();
					}
				}
				if (flag2)
				{
					Log.MinStackPerfomance.Enqueue(MinCur);
					MinCur = new Logs.ItemPerfomance();
					MinCur.Date = DateTime.Now;
					while (Log.MinStackPerfomance.Count > 240)
					{
						Log.MinStackPerfomance.Dequeue();
					}
				}
				if (flag3)
				{
					Log.Min5StackPerfomance.Enqueue(Min5Cur);
					Min5Cur = new Logs.ItemPerfomance();
					Min5Cur.Date = DateTime.Now;
					while (Log.Min5StackPerfomance.Count > 240)
					{
						Log.Min5StackPerfomance.Dequeue();
					}
				}
			}
			catch
			{
			}
		}
	}

	private void CopyLogs(Logs.ItemPerfomance From, Logs.ItemPerfomance Dest)
	{
		Dest.Memory = Math.Max(Dest.Memory, From.Memory);
		Dest.KkmCount = Math.Max(Dest.KkmCount, From.KkmCount);
		Dest.KkmNotActive = Math.Max(Dest.KkmNotActive, From.KkmNotActive);
		Dest.KkmNotInit = Math.Max(Dest.KkmNotInit, From.KkmNotInit);
		Dest.CommandMaxQueue = Math.Max(Dest.CommandMaxQueue, From.CommandMaxQueue);
		Dest.CommandMaxRun = Math.Max(Dest.CommandMaxRun, From.CommandMaxRun);
		Dest.CommandCount += From.CommandCount;
		Dest.CommandError += From.CommandError;
		Dest.CommandWork += From.CommandWork;
		Dest.CommandWorkTime += From.CommandWorkTime;
		Dest.CommandMaxWorkTime = Math.Max(Dest.CommandMaxWorkTime, From.CommandMaxWorkTime);
		Dest.CommandQueue += From.CommandQueue;
		Dest.CommandQueueTime += From.CommandQueueTime;
		Dest.CommandMaxQueueTime = Math.Max(Dest.CommandMaxQueueTime, From.CommandMaxQueueTime);
		if (From.INN == null)
		{
			From.INN = new SortedList<string, Logs.INNItemPerfomance>();
		}
		foreach (KeyValuePair<string, Logs.INNItemPerfomance> item in From.INN)
		{
			if (!Dest.INN.ContainsKey(item.Value.INN))
			{
				Dest.INN.Add(item.Value.INN, new Logs.INNItemPerfomance());
			}
			Logs.INNItemPerfomance iNNItemPerfomance = Dest.INN[item.Value.INN];
			iNNItemPerfomance.INN = item.Value.INN;
			iNNItemPerfomance.KkmCount = Math.Max(iNNItemPerfomance.KkmCount, item.Value.KkmCount);
			iNNItemPerfomance.KkmNotActive = Math.Max(iNNItemPerfomance.KkmNotActive, item.Value.KkmNotActive);
			iNNItemPerfomance.KkmNotInit = Math.Max(iNNItemPerfomance.KkmNotInit, item.Value.KkmNotInit);
			iNNItemPerfomance.CommandMaxQueue = Math.Max(iNNItemPerfomance.CommandMaxQueue, item.Value.CommandMaxQueue);
			iNNItemPerfomance.CommandMaxRun = Math.Max(iNNItemPerfomance.CommandMaxRun, item.Value.CommandMaxRun);
			iNNItemPerfomance.CommandWork += item.Value.CommandWork;
			iNNItemPerfomance.CommandWorkTime += item.Value.CommandWorkTime;
			iNNItemPerfomance.CommandMaxWorkTime = Math.Max(iNNItemPerfomance.CommandMaxWorkTime, item.Value.CommandMaxWorkTime);
			iNNItemPerfomance.CommandQueue += item.Value.CommandQueue;
			iNNItemPerfomance.CommandQueueTime += item.Value.CommandQueueTime;
			iNNItemPerfomance.CommandMaxQueueTime = Math.Max(iNNItemPerfomance.CommandMaxQueueTime, item.Value.CommandMaxQueueTime);
		}
	}

	public void CopyLogsINN(Logs.INNItemPerfomance From, Logs.ItemPerfomance Dest)
	{
		Dest.KkmCount = Math.Max(Dest.KkmCount, From.KkmCount);
		Dest.KkmNotActive = Math.Max(Dest.KkmNotActive, From.KkmNotActive);
		Dest.KkmNotInit = Math.Max(Dest.KkmNotInit, From.KkmNotInit);
		Dest.CommandMaxQueue = Math.Max(Dest.CommandMaxQueue, From.CommandMaxQueue);
		Dest.CommandMaxRun = Math.Max(Dest.CommandMaxRun, From.CommandMaxRun);
		Dest.CommandCount += From.CommandCount;
		Dest.CommandError += From.CommandError;
		Dest.CommandWork += From.CommandWork;
		Dest.CommandWorkTime += From.CommandWorkTime;
		Dest.CommandMaxWorkTime = Math.Max(Dest.CommandMaxWorkTime, From.CommandMaxWorkTime);
		Dest.CommandQueue += From.CommandQueue;
		Dest.CommandQueueTime += From.CommandQueueTime;
		Dest.CommandMaxQueueTime = Math.Max(Dest.CommandMaxQueueTime, From.CommandMaxQueueTime);
	}

	public void RunCommand(UnitManager.ExecuteData ExecuteData)
	{
		if (!Global.Settings.StatisticsСollection)
		{
			return;
		}
		lock (Log)
		{
			try
			{
				SecCur.CommandQueue++;
				SecCur.CommandQueueTime += (int)(ExecuteData.DateRun - ExecuteData.DateStart).TotalMilliseconds;
				SecCur.CommandMaxQueueTime = Math.Max(SecCur.CommandMaxQueueTime, (int)(ExecuteData.DateRun - ExecuteData.DateStart).TotalMilliseconds);
				int num = 0;
				int num2 = 0;
				foreach (UnitManager.IsExecuteData isExecuteData in Global.UnitManager.IsExecuteDatas)
				{
					if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun)
					{
						num++;
					}
					if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Run)
					{
						num2++;
					}
				}
				SecCur.CommandMaxQueue = Math.Max(SecCur.CommandMaxQueue, num);
				SecCur.CommandMaxRun = Math.Max(SecCur.CommandMaxRun, num2);
			}
			catch
			{
			}
		}
	}

	public void EndCommand(UnitManager.ExecuteData ExecuteData)
	{
		if (!Global.Settings.StatisticsСollection)
		{
			return;
		}
		lock (Log)
		{
			try
			{
				if (ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Ok)
				{
					SecCur.CommandCount++;
				}
				if (ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Error)
				{
					SecCur.CommandError++;
				}
				SecCur.CommandWork++;
				SecCur.CommandWorkTime += (int)(ExecuteData.DateEnd - ExecuteData.DateRun).TotalMilliseconds;
				SecCur.CommandMaxWorkTime = Math.Max(SecCur.CommandMaxWorkTime, (int)(ExecuteData.DateEnd - ExecuteData.DateRun).TotalMilliseconds);
				int num = 0;
				int num2 = 0;
				foreach (UnitManager.IsExecuteData isExecuteData in Global.UnitManager.IsExecuteDatas)
				{
					if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun)
					{
						num++;
					}
					if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Run)
					{
						num2++;
					}
				}
				SecCur.CommandMaxQueue = Math.Max(SecCur.CommandMaxQueue, num);
				SecCur.CommandMaxRun = Math.Max(SecCur.CommandMaxRun, num2);
			}
			catch
			{
			}
		}
	}

	public async Task AddError(Unit Unit, UnitManager.ExecuteData ExecuteData, string TextCommand, string NetLogs = "", object RezultCommand = null)
	{
		if (Global.AllCancellationToken.Token.IsCancellationRequested)
		{
			return;
		}
		TextCommand = EraseKeySubLicensing(TextCommand);
		if (Unit == null)
		{
			await AddError(ExecuteData.DataCommand.Command, ExecuteData.RezultCommand.Error, TextCommand, NetLogs);
			return;
		}
		if (NetLogs == "")
		{
			try
			{
				NetLogs = Unit.NetLogs.ToString();
			}
			catch (Exception ex)
			{
				NetLogs = ex.Message;
			}
		}
		if (ExecuteData.RezultCommand.Error == "Нет лицензии.")
		{
			NetLogs = ComDevice.Logs + "\r\n" + NetLogs;
		}
		string text = "-";
		if (Unit.Kkm != null)
		{
			if (Unit.Kkm.FfdVersion == 4)
			{
				text = "1.2";
			}
			else if (Unit.Kkm.FfdVersion == 3)
			{
				text = "1.1";
			}
			else if (Unit.Kkm.FfdVersion == 2)
			{
				text = "1.05";
			}
			else if (Unit.Kkm.FfdVersion == 1)
			{
				text = "1.0";
			}
		}
		ItemLog itemLog = new ItemLog();
		itemLog.NumUnit = Unit.NumUnit;
		itemLog.TypeDevice = TypeDevice.NameType[(int)Unit.SettDr.TypeDevice.Type];
		itemLog.IdDevice = Unit.SettDr.TypeDevice.Id;
		itemLog.NameDevice = Unit.NameDevice;
		itemLog.NumberKkm = Unit.Kkm.NumberKkm;
		itemLog.INN = Unit.Kkm.INN;
		itemLog.Command = ExecuteData.DataCommand.Command;
		itemLog.Error = ExecuteData.RezultCommand.Error;
		itemLog.TextCommand = "IdType: " + Unit.SettDr.IdTypeDevice + ", IdModel: " + Unit.IdModel + ", ver: " + Global.Verson + ", Width: " + ((Unit.Kkm != null) ? Unit.Kkm.PrintingWidth.ToString() : "-") + ", Firmware: " + ((Unit.Kkm != null) ? Unit.Kkm.Firmware_Version : "-") + ", FFD: " + text + ", IP client: " + ExecuteData.DataCommand.IP_client + "\n\r" + TextCommand;
		itemLog.NetLogs = NetLogs;
		if (RezultCommand != null)
		{
			object obj = Activator.CreateInstance(RezultCommand.GetType());
			Unit.CopyObject(RezultCommand, obj, "MessageHTML");
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
			itemLog.Rezult = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
		}
		await CommandLogs.Add(itemLog, DateTime.Now);
	}

	public async Task AddError(string Command, string TextError, string TextCommand = "<Нет>", string NetLogs = "<Нет>")
	{
		if (!Global.AllCancellationToken.Token.IsCancellationRequested)
		{
			TextCommand = EraseKeySubLicensing(TextCommand);
			if (TextError == "Нет лицензии.")
			{
				NetLogs = ComDevice.Logs + "\r\n" + NetLogs;
			}
			ItemLog itemLog = new ItemLog();
			itemLog.NumUnit = 0;
			itemLog.TypeDevice = "<Нет>";
			itemLog.IdDevice = "<Нет>";
			itemLog.NameDevice = "<Нет>";
			itemLog.NumberKkm = "<Нет>";
			itemLog.INN = "<Нет>";
			itemLog.Command = Command;
			itemLog.Error = TextError;
			itemLog.TextCommand = "ver: " + Global.Verson + "\n\r" + TextCommand;
			itemLog.NetLogs = NetLogs;
			await CommandLogs.Add(itemLog, DateTime.Now);
		}
	}

	public async Task AddError(string Command, Global.DeviceSettings Device, string TextError)
	{
		if (!Global.AllCancellationToken.Token.IsCancellationRequested)
		{
			string text = "<Нет>";
			if (TextError == "Нет лицензии.")
			{
				text = ComDevice.Logs + "\r\n" + text;
			}
			ItemLog itemLog = new ItemLog();
			itemLog.NumUnit = Device.NumDevice;
			itemLog.TypeDevice = TypeDevice.NameType[(int)Device.TypeDevice.Type];
			itemLog.IdDevice = Device.IdTypeDevice;
			itemLog.NameDevice = "<Нет>";
			itemLog.NumberKkm = "<Нет>";
			itemLog.INN = "<Нет>";
			itemLog.Command = Command;
			itemLog.Error = TextError;
			itemLog.TextCommand = "<Нет>";
			itemLog.NetLogs = text;
			await CommandLogs.Add(itemLog, DateTime.Now);
		}
	}

	private string EraseKeySubLicensing(string TextCommand)
	{
		int num = TextCommand.ToLower().IndexOf("keysublicensing");
		if (num != -1)
		{
			num = TextCommand.IndexOf(':', ++num);
			if (num != -1)
			{
				int num2 = TextCommand.IndexOf(',', ++num);
				num = TextCommand.IndexOf('"', ++num);
				if (num != -1 && num2 > num)
				{
					int num3 = TextCommand.IndexOf('"', ++num);
					if (num3 != -1 && num != num3 && num3 - num > 4)
					{
						TextCommand = TextCommand.Substring(0, num) + "<Значение указано>" + TextCommand.Substring(num3);
					}
				}
			}
		}
		return TextCommand;
	}
}
