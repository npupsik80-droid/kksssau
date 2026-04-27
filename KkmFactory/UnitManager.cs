using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KkmFactory;

public class UnitManager
{
	[DataContract]
	public class ExecuteData
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
		public Unit.DataCommand DataCommand;

		[DataMember]
		public Unit.RezultCommand RezultCommand;

		[DataMember]
		public string INN;

		[DataMember]
		public string KeyCallback = "";

		[DataMember]
		public bool ReturnedCallback;

		[DataMember]
		public bool ReturnedAddIn;

		[DataMember]
		public bool NotRelevant;

		public string CurrentNameCommand = "";

		public ExecuteData CurrentExecuteData;
	}

	[DataContract]
	public class IsExecuteData
	{
		public ExecuteData ExecuteData;

		public AutoResetEvent WaitHandle = new AutoResetEvent(false);

		public Unit Unit;

		public bool IsFind;

		public string TextCommand = "";

		public bool NeedLock;

		public Task Task;

		public string Mess = "";
	}

	public Dictionary<string, TypeDevice> ListTypeDevice = new Dictionary<string, TypeDevice>();

	public SortedLisSem<int, Unit> Units = new SortedLisSem<int, Unit>();

	public QueueSem<ExecuteData> ExecuteDatas = new QueueSem<ExecuteData>(100);

	public ListSem<IsExecuteData> IsExecuteDatas = new ListSem<IsExecuteData>(100);

	public AutoResetEvent WaitWorkStack = new AutoResetEvent(false);

	public const int CountExecuteDatas = 100;

	public UnitManager()
	{
		Global.UnitManager = this;
		TypeDevice.RegTypeUnitDrivers();
	}

	public async Task InitUnitManager()
	{
		await LoadUnit();
	}

	public async Task LoadUnit()
	{
		List<Task> ListTask = new List<Task>();
		try
		{
			await Global.Settings.Devices.Semaphore.WaitAsync();
			await Units.Semaphore.WaitAsync();
			foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
			{
				try
				{
					Task task = LoadUnit(device.Key);
					if (task != null)
					{
						ListTask.Add(task);
					}
				}
				catch (Exception)
				{
				}
			}
		}
		finally
		{
			Units.Semaphore.Release();
			Global.Settings.Devices.Semaphore.Release();
		}
		new Task(delegate
		{
			Task.WaitAll(ListTask.ToArray());
			PermitRegim.LoadFromDevice();
		}).Start();
	}

	public Task LoadUnit(int Num, bool IsGlbTask = false, object AhtungData = null)
	{
		Global.DeviceSettings SettDr = Global.Settings.Devices[Num];
		FreeUnit(Num);
		if (SettDr.IdTypeDevice == "")
		{
			Global.Settings.Devices.Remove(Num);
			return null;
		}
		Unit Unit = TypeDevice.GetDeviceClass(SettDr, Num);
		if (typeof(Unit) == Unit.GetType())
		{
			return null;
		}
		Unit.NumUnit = Num;
		Unit.Block = true;
		if (AhtungData != null)
		{
			Unit.SetAhtungData(AhtungData);
		}
		Unit.Semaphore.Wait();
		try
		{
			Units.Add(Num, Unit);
		}
		catch
		{
		}
		finally
		{
			Unit.Semaphore.Release();
		}
		Task task = new Task(async delegate
		{
			try
			{
				await Unit.Semaphore.WaitAsync();
				Unit.LastError = "Идет инициализация....";
				Unit.StartCommandDate = DateTime.Now;
				Unit.InitDll();
				await Unit.ProcessInitDevice(FullInit: true, Program: true);
				Unit.SaveParemeterSearhKKT();
			}
			catch (Exception ex)
			{
				Unit.Error = "Не обработанная ошибка инициализации: " + Global.GetErrorMessagee(ex);
				try
				{
					await ((UnitPort)Unit).PortCloseAsync();
				}
				catch
				{
				}
				await Global.Logers.AddError("InitDll", SettDr, Unit.Error);
				Unit unit = new Unit(SettDr, Num);
				Units[Num] = unit;
				unit.Error = Unit.Error;
			}
			finally
			{
				try
				{
					if ((Unit.Error != null && Unit.Error != "") || Global.Settings.RegisterAllCommand)
					{
						ExecuteData executeData = new ExecuteData();
						executeData.DataCommand = new Unit.DataCommand();
						executeData.DataCommand.Command = "InitDevice";
						executeData.RezultCommand = new Unit.RezultCommand();
						executeData.RezultCommand.Error = Unit.Error;
						await Global.Logers.AddError(Unit, executeData, "<Нет>");
					}
					Unit.UpdateSettingsServer();
				}
				finally
				{
					Unit.Semaphore.Release();
					Unit.LastError = Unit.Error;
					Unit.Block = false;
				}
			}
		});
		task.Start();
		return task;
	}

	public void FreeUnit()
	{
		Global.Settings.Devices.Semaphore.Wait();
		Units.Semaphore.Wait();
		int[] array = new int[Units.Count];
		int num = 0;
		foreach (KeyValuePair<int, Unit> unit in Units)
		{
			array[num] = unit.Key;
			num++;
		}
		int[] array2 = array;
		foreach (int num2 in array2)
		{
			FreeUnit(num2);
		}
		Units.Semaphore.Release();
		Global.Settings.Devices.Semaphore.Release();
	}

	public void ClosePortUnit()
	{
		Global.Settings.Devices.Semaphore.Wait();
		Units.Semaphore.Wait();
		foreach (KeyValuePair<int, Unit> Unit in Units)
		{
			if (!(Unit.Value is UnitPort))
			{
				continue;
			}
			Task.Run(async delegate
			{
				try
				{
					await ((UnitPort)Unit.Value).PortCloseAsync();
				}
				catch
				{
				}
			});
		}
		Task.Delay(200).Wait();
		Units.Semaphore.Release();
		Global.Settings.Devices.Semaphore.Release();
	}

	public async void FreeUnit(int Num)
	{
		try
		{
			if (!Units.ContainsKey(Num))
			{
				return;
			}
			Unit OldUnit = Units[Num];
			OldUnit.Semaphore.Wait();
			try
			{
				try
				{
					Units[Num].UnitOpen = 0;
					await ((UnitPort)Units[Num]).PortCloseAsync();
				}
				catch
				{
				}
				Units[Num].Destroy();
				Units[Num] = null;
				Units.Remove(Num);
			}
			catch
			{
			}
			finally
			{
				OldUnit.Semaphore.Release();
			}
		}
		catch
		{
		}
	}

	public bool AddUnit(int NumberUnit, string IdDevice, string IdTypeDevice, Dictionary<string, string> Paramets = null, bool InspectionPort = false, bool SetActive = false)
	{
		bool result = true;
		Global.Settings.Devices.Semaphore.Wait();
		Units.Semaphore.Wait();
		try
		{
			if (NumberUnit == -1)
			{
				for (int i = 1; i < 3000; i++)
				{
					if (!Global.Settings.Devices.ContainsKey(i))
					{
						NumberUnit = i;
						break;
					}
				}
			}
			Global.DeviceSettings deviceSettings = null;
			Unit unit = null;
			bool active = true;
			DateTime addDate = DateTime.Now;
			foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
			{
				if (device.Value.NumDevice == NumberUnit || (IdDevice != null && IdDevice != "" && device.Value.IdDevice == IdDevice))
				{
					deviceSettings = device.Value;
				}
			}
			foreach (KeyValuePair<int, Unit> unit2 in Global.UnitManager.Units)
			{
				if (unit2.Key == NumberUnit)
				{
					unit = unit2.Value;
				}
			}
			if (deviceSettings != null)
			{
				deviceSettings = Global.Settings.Devices[NumberUnit];
				active = deviceSettings.Active;
				addDate = deviceSettings.AddDate;
				Global.Settings.Devices.Remove(NumberUnit);
			}
			object ahtungData = null;
			if (unit != null)
			{
				ahtungData = unit.GetAhtungData();
			}
			Global.UnitManager.FreeUnit(NumberUnit);
			if (IdTypeDevice == null)
			{
				Global.SaveSettingsAsync().Wait();
				return result;
			}
			if (NumberUnit > Global.MaxUnit)
			{
				new Exception("Превышен лимит на количество устройств.");
			}
			if (!(IdTypeDevice == "KkmAtol") && !(IdTypeDevice == "KkmStrihM"))
			{
				_ = IdTypeDevice == "PinterPOS";
			}
			deviceSettings = new Global.DeviceSettings();
			deviceSettings.Active = active;
			foreach (KeyValuePair<string, TypeDevice> item in Global.UnitManager.ListTypeDevice)
			{
				if (item.Value.Id == IdTypeDevice)
				{
					deviceSettings.IdTypeDevice = IdTypeDevice;
					deviceSettings.TypeDevice = item.Value;
					deviceSettings.NumDevice = NumberUnit;
					if (SetActive)
					{
						deviceSettings.Active = true;
					}
					break;
				}
			}
			if (Paramets != null)
			{
				deviceSettings.Paramets = Paramets;
			}
			else
			{
				deviceSettings.Paramets = new Dictionary<string, string>();
			}
			deviceSettings.AddDate = addDate;
			if (IdDevice != null && IdDevice != "")
			{
				deviceSettings.IdDevice = IdDevice;
			}
			deviceSettings.INN = "";
			deviceSettings.NumberKkm = "";
			deviceSettings.TaxVariant = "";
			Global.Settings.Devices.Add(NumberUnit, deviceSettings);
			Global.SaveSettingsAsync().Wait();
			Global.UnitManager.LoadUnit(NumberUnit, IsGlbTask: false, ahtungData);
		}
		catch (Exception)
		{
			result = false;
		}
		finally
		{
			Units.Semaphore.Release();
			Global.Settings.Devices.Semaphore.Release();
			Task.Delay(1000).Wait();
		}
		return result;
	}

	public Unit.RezultCommand AddCommand_OLD(Unit.DataCommand DataCommand, string TypeSunc, string TextCommand, string KeyCallback = "", string CurrentIdCommand = null)
	{
		Task<Unit.RezultCommand> task = AddCommand(DataCommand, TypeSunc, TextCommand, KeyCallback, CurrentIdCommand);
		task.Wait();
		return task.Result;
	}

	public async Task<Unit.RezultCommand> AddCommand(Unit.DataCommand DataCommand, string TypeSunc, string TextCommand, string KeyCallback = "", string CurrentIdCommand = null)
	{
		ExecuteData ExecuteData = new ExecuteData();
		ExecuteData.DataCommand = DataCommand;
		ExecuteData.NumDevice = DataCommand.NumDevice;
		ExecuteData.IdCommand = DataCommand.IdCommand;
		ExecuteData.DateStart = DateTime.Now;
		ExecuteData.INN = ((DataCommand.InnKkm == null) ? "" : DataCommand.InnKkm);
		ExecuteData.RezultCommand = new Unit.RezultCommand();
		ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.NotRun;
		ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
		ExecuteData.KeyCallback = KeyCallback;
		if (CurrentIdCommand != null)
		{
			bool Sem = false;
			try
			{
				Sem = await ExecuteDatas.Semaphore.WaitAsync(1000);
				foreach (ExecuteData executeData in ExecuteDatas)
				{
					if (executeData.IdCommand == CurrentIdCommand && !executeData.NotRelevant)
					{
						executeData.CurrentExecuteData = ExecuteData;
						break;
					}
				}
			}
			finally
			{
				if (Sem)
				{
					ExecuteDatas.Semaphore.Release();
				}
			}
		}
		switch (DataCommand.Command)
		{
		case "DoAdditionalAction":
			ExecuteData.Type = TypeDevice.enType.НеВыбрано;
			break;
		case "AddInGetSettings":
			ExecuteData.Type = TypeDevice.enType.НеВыбрано;
			break;
		case "Exit":
			Global.HaltServer();
			break;
		case "InitDevice":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "OpenShift":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Открытие смены";
			break;
		case "CloseShift":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Закрытие смены";
			break;
		case "ZReport":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Закрытие смены";
			break;
		case "XReport":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Отчет по смене";
			break;
		case "OfdReport":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "OpenCashDrawer":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "RequestCash":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "DepositingCash":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Внесение ДС в ККТ";
			break;
		case "PaymentCash":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Выплата ДС из ККТ";
			break;
		case "RegisterCheck":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Регистрация чека";
			break;
		case "ValidationMarkingCode":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Проверка кода маркировки";
			break;
		case "GetLineLength":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "KkmRegOfd":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			ExecuteData.CurrentNameCommand = "Регистрация чека";
			break;
		case "GetDataKKT":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "GetDataCheck":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "GetCounters":
			ExecuteData.Type = TypeDevice.enType.ФискальныйРегистратор;
			break;
		case "PrintDocument":
			ExecuteData.Type = TypeDevice.enType.ПринтерЧеков;
			ExecuteData.CurrentNameCommand = "Печать слип-чека";
			break;
		case "PrintLineLength":
			ExecuteData.Type = TypeDevice.enType.ПринтерЧеков;
			break;
		case "PrintOpenCashDrawer":
			ExecuteData.Type = TypeDevice.enType.ПринтерЧеков;
			break;
		case "OutputOnCustomerDisplay":
			ExecuteData.Type = TypeDevice.enType.ДисплеиПокупателя;
			break;
		case "ClearCustomerDisplay":
			ExecuteData.Type = TypeDevice.enType.ДисплеиПокупателя;
			break;
		case "OptionsCustomerDisplay":
			ExecuteData.Type = TypeDevice.enType.ДисплеиПокупателя;
			break;
		case "GetBarcode":
			ExecuteData.Type = TypeDevice.enType.СканерШтрихкода;
			break;
		case "OpenBarcode":
			ExecuteData.Type = TypeDevice.enType.СканерШтрихкода;
			break;
		case "CloseBarcode":
			ExecuteData.Type = TypeDevice.enType.СканерШтрихкода;
			break;
		case "Calibrate":
			ExecuteData.Type = TypeDevice.enType.ЭлектронныеВесы;
			break;
		case "GetWeight":
			ExecuteData.Type = TypeDevice.enType.ЭлектронныеВесы;
			break;
		case "PayByPaymentCard":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Оплата по Карте/СБП";
			break;
		case "ReturnPaymentByPaymentCard":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Возврат по Карте/СБП";
			break;
		case "CancelPaymentByPaymentCard":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Отмена по Карте/СБП";
			break;
		case "AuthorisationByPaymentCard":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Блокировка по Карте/СБП";
			break;
		case "AuthConfirmationByPaymentCard":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Оплата по Карте/СБП";
			break;
		case "CancelAuthorisationByPaymentCard":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Отмена по Карте/СБП";
			break;
		case "EmergencyReversal":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Аварийная отмена";
			break;
		case "Settlement":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Сверка итогов по смене";
			break;
		case "TerminalReport":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			ExecuteData.CurrentNameCommand = "Отчет за смену";
			break;
		case "TransactionDetails":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			break;
		case "PrintSlipOnTerminal":
			ExecuteData.Type = TypeDevice.enType.ЭквайринговыйТерминал;
			break;
		case "GetRezult":
			ExecuteData.Type = TypeDevice.enType.НеВыбрано;
			break;
		case "TrackingStatusCommand":
			ExecuteData.Type = TypeDevice.enType.НеВыбрано;
			break;
		case "CancelCommand":
			ExecuteData.Type = TypeDevice.enType.НеВыбрано;
			break;
		default:
			throw new ArgumentException($"Неопознанная команда {DataCommand.Command}");
		case "Version":
		case "List":
		case "OnOffUnut":
		case "GetServerData":
		case "GetGoodCodeData":
		case "GetSettingsServer":
		case "SetSettingsServer":
		case "GetDeviceParamets":
		case "GetDeviceSettings":
		case "SetDeviceSettings":
		case "GetTypesDevice":
		case "GetDataLicense":
		case "GetLicense":
		case "FreeLicense":
			break;
		}
		if (ExecuteData.Type == TypeDevice.enType.ЭквайринговыйТерминал && ExecuteData.NumDevice != 0)
		{
			int num = 0;
			foreach (KeyValuePair<int, Unit> unit in Global.UnitManager.Units)
			{
				num = Math.Max(num, unit.Value.NumUnit);
			}
			if (ExecuteData.NumDevice > num)
			{
				foreach (KeyValuePair<int, Unit> unit2 in Global.UnitManager.Units)
				{
					if (unit2.Value.SettDr.TypeDevice.Types.Contains(TypeDevice.enType.ЭквайринговыйТерминал) && unit2.Value.SupportsSBP && ++num == ExecuteData.NumDevice)
					{
						ExecuteData.DataCommand.OnSBP = true;
						ExecuteData.DataCommand.NumDevice = unit2.Value.NumUnit;
						ExecuteData.NumDevice = unit2.Value.NumUnit;
						break;
					}
				}
			}
		}
		bool IsDone = false;
		Unit.RezultCommandUseAddInDialog RezultCommandUseAddInDialog = null;
		if (DataCommand.UseAddInDialogSelectDevice || DataCommand.UseAddInDialogPrintCheck)
		{
			bool Sem = false;
			List<Unit> ListSortUnits = Global.UnitManager.Units.Select(delegate(KeyValuePair<int, Unit> u)
			{
				KeyValuePair<int, Unit> keyValuePair = u;
				return keyValuePair.Value;
			}).ToList();
			List<Unit> ListUnits = GetListUnitsForCommand(DataCommand, ExecuteData.Type, ref ListSortUnits);
			if ((ExecuteData.Type == TypeDevice.enType.ФискальныйРегистратор || ExecuteData.Type == TypeDevice.enType.ПринтерЧеков) && DataCommand.Command == "RegisterCheck" && DataCommand.IsFiscalCheck && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 1))
			{
				foreach (Unit item in ListUnits)
				{
					Sem = Unit.NeedPayByProcessing(DataCommand, item);
					if (Sem)
					{
						break;
					}
				}
			}
			if (Sem)
			{
				try
				{
					if (ListUnits.Count < 1)
					{
						throw new Exception("Указанная ККТ не найдена. (Или неправильный 'Номер ККТ' или 'ИНН ККТ' или 'Система налогообложения')");
					}
					await ListUnits[0].CheckDataForFfd(DataCommand);
				}
				catch (Exception ex)
				{
					ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
					ExecuteData.RezultCommand.Error = ex.Message;
					IsDone = true;
				}
			}
			if (Sem && DataCommand.UseAddInDialogPrintCheck)
			{
				RezultCommandUseAddInDialog = new Unit.RezultCommandUseAddInDialog();
				Unit.HtmlDialogPrintCheck(DataCommand, ExecuteData.RezultCommand, RezultCommandUseAddInDialog, ListUnits, ExecuteData.Type);
				if (RezultCommandUseAddInDialog != null && (RezultCommandUseAddInDialog.UseAddInDialogHTML == null || RezultCommandUseAddInDialog.UseAddInDialogHTML == ""))
				{
					RezultCommandUseAddInDialog = null;
				}
				else
				{
					ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
					ExecuteData.RezultCommand.Error = "Отменено пользователем";
					ExecuteData.RezultCommand.Command = DataCommand.Command;
					ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
					RezultCommandUseAddInDialog.Status = Unit.ExecuteStatus.Ok;
					RezultCommandUseAddInDialog.IdCommand = DataCommand.IdCommand;
					DataCommand.Command = "UseAddInDialog";
					ExecuteData.IdCommand = Guid.NewGuid().ToString();
					IsDone = true;
				}
			}
			else if (DataCommand.UseAddInDialogSelectDevice && ListUnits.Count > 1)
			{
				RezultCommandUseAddInDialog = new Unit.RezultCommandUseAddInDialog();
				Unit.HtmlDialogSelectDevice(DataCommand, ExecuteData.RezultCommand, RezultCommandUseAddInDialog, ListUnits, ExecuteData.Type);
				if (RezultCommandUseAddInDialog != null && (RezultCommandUseAddInDialog.UseAddInDialogHTML == null || RezultCommandUseAddInDialog.UseAddInDialogHTML == ""))
				{
					RezultCommandUseAddInDialog = null;
				}
				else
				{
					ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
					ExecuteData.RezultCommand.Error = "Отменено пользователем";
					ExecuteData.RezultCommand.Command = DataCommand.Command;
					ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
					RezultCommandUseAddInDialog.Status = Unit.ExecuteStatus.Ok;
					RezultCommandUseAddInDialog.IdCommand = DataCommand.IdCommand;
					DataCommand.Command = "UseAddInDialog";
					ExecuteData.IdCommand = Guid.NewGuid().ToString();
					IsDone = true;
				}
			}
		}
		switch (ExecuteData.Type)
		{
		case TypeDevice.enType.ФискальныйРегистратор:
			if (DataCommand.Command == "GetDataCheck")
			{
				ExecuteData.RezultCommand = new Unit.RezultCommandCheck();
			}
			else if (DataCommand.Command == "ValidationMarkingCode")
			{
				ExecuteData.RezultCommand = new Unit.RezultMarkingCodeValidation();
			}
			else if (DataCommand.Command == "GetCounters")
			{
				ExecuteData.RezultCommand = new Unit.RezultCounters();
			}
			else
			{
				ExecuteData.RezultCommand = new Unit.RezultCommandKKm();
			}
			break;
		case TypeDevice.enType.ПринтерЧеков:
			ExecuteData.RezultCommand = new Unit.RezultCommandKKm();
			break;
		case TypeDevice.enType.СканерШтрихкода:
			ExecuteData.RezultCommand = new Unit.RezultCommandBarCode();
			break;
		case TypeDevice.enType.ЭлектронныеВесы:
			ExecuteData.RezultCommand = new Unit.RezultCommandLibra();
			break;
		case TypeDevice.enType.ЭлектронныеЗамки:
			ExecuteData.RezultCommand = new Unit.RezultCommandLocks();
			break;
		case TypeDevice.enType.ЭквайринговыйТерминал:
			ExecuteData.RezultCommand = new Unit.RezultCommandProcessing();
			break;
		case TypeDevice.enType.ДисплеиПокупателя:
			ExecuteData.RezultCommand = new Unit.RezultCommand();
			break;
		case TypeDevice.enType.НеВыбрано:
			ExecuteData.RezultCommand = new Unit.RezultCommand();
			break;
		}
		if (DataCommand.Command == "OptionsCustomerDisplay")
		{
			ExecuteData.RezultCommand = new Unit.RezultCommandCD();
		}
		ExecuteData.RezultCommand.RunComPort = DataCommand.RunComPort;
		ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.NotRun;
		if (DataCommand.IdCommand != null && DataCommand.IdCommand != "" && DataCommand.Command != "GetRezult" && DataCommand.Command != "TrackingStatusCommand" && DataCommand.Command != "CancelCommand" && DataCommand.Command != "AddInGetSettings" && !IsDone)
		{
			bool Sem = false;
			bool Sem2 = false;
			try
			{
				Sem2 = await ExecuteDatas.Semaphore.WaitAsync(1000);
				ExecuteDatas.TrimExcess();
				foreach (ExecuteData executeData2 in ExecuteDatas)
				{
					if (executeData2.IdCommand == DataCommand.IdCommand && !executeData2.NotRelevant)
					{
						if (executeData2.RezultCommand.Status == Unit.ExecuteStatus.Error)
						{
							executeData2.NotRelevant = true;
						}
						else
						{
							Sem = true;
						}
						break;
					}
				}
			}
			finally
			{
				if (Sem2)
				{
					ExecuteDatas.Semaphore.Release();
				}
			}
			if (Sem)
			{
				ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.AlreadyDone;
				ExecuteData.RezultCommand.Error = "Команда уже была выполнена или выполняется в текущий момент";
			}
		}
		if (ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun)
		{
			if (DataCommand.Command == "Version")
			{
				ExecuteData.RezultCommand = new Unit.RezultCommand();
				ExecuteData.RezultCommand.Verson = Global.Verson;
				ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Ok;
				IsDone = true;
			}
			else if (DataCommand.Command == "GetRezult")
			{
				ExecuteData.RezultCommand = new Unit.RezultCommandGetRezult();
				ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.NotFound;
				ExecuteData.RezultCommand.Command = DataCommand.Command;
				ExecuteData.RezultCommand.Error = "Команда с таким IdCommand не найдена";
				((Unit.RezultCommandGetRezult)ExecuteData.RezultCommand).Rezult = new Unit.RezultCommand();
				((Unit.RezultCommandGetRezult)ExecuteData.RezultCommand).Rezult.IdCommand = DataCommand.IdCommand;
				((Unit.RezultCommandGetRezult)ExecuteData.RezultCommand).Rezult.Status = Unit.ExecuteStatus.NotFound;
				((Unit.RezultCommandGetRezult)ExecuteData.RezultCommand).Rezult.Command = "<Не определено>";
				((Unit.RezultCommandGetRezult)ExecuteData.RezultCommand).Rezult.Error = "<Не определено>";
				ExecuteData FindEx = null;
				if (DataCommand.IdCommand == null || DataCommand.IdCommand == "")
				{
					ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
					ExecuteData.RezultCommand.Error = "Не указан Id команды!";
				}
				else
				{
					bool Sem2 = false;
					try
					{
						Sem2 = await ExecuteDatas.Semaphore.WaitAsync(1000);
						ExecuteDatas.TrimExcess();
						foreach (ExecuteData executeData3 in ExecuteDatas)
						{
							if (executeData3.IdCommand == DataCommand.IdCommand && !executeData3.NotRelevant)
							{
								FindEx = executeData3;
								ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Ok;
								((Unit.RezultCommandGetRezult)ExecuteData.RezultCommand).Rezult = executeData3.RezultCommand;
								ExecuteData.RezultCommand.Error = "";
								break;
							}
						}
					}
					finally
					{
						if (Sem2)
						{
							ExecuteDatas.Semaphore.Release(1000);
						}
					}
				}
				if (FindEx != null)
				{
					foreach (KeyValuePair<int, Unit> unit3 in Global.UnitManager.Units)
					{
						if (FindEx.NumDevice == unit3.Value.NumUnit)
						{
							unit3.Value.GetRezult(FindEx.DataCommand, FindEx.RezultCommand);
						}
					}
				}
				DataCommand.IdCommand = "";
				IsDone = true;
			}
			else if (DataCommand.Command == "TrackingStatusCommand")
			{
				ExecuteData.RezultCommand = new Unit.RezultCommandGetRezult();
				ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.NotFound;
				ExecuteData.RezultCommand.Command = DataCommand.Command;
				ExecuteData.RezultCommand.Error = "Команда с таким IdCommand не найдена";
				ExecuteData FindEx = null;
				if (DataCommand.IdCommand == null || DataCommand.IdCommand == "")
				{
					ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
					ExecuteData.RezultCommand.Error = "Не указан Id команды!";
				}
				else
				{
					bool Sem2 = false;
					try
					{
						Sem2 = await ExecuteDatas.Semaphore.WaitAsync(1000);
						ExecuteDatas.TrimExcess();
						foreach (ExecuteData executeData4 in ExecuteDatas)
						{
							if (executeData4.IdCommand == DataCommand.IdCommand && !executeData4.NotRelevant)
							{
								FindEx = executeData4;
								ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Ok;
								ExecuteData.RezultCommand.Error = "";
								break;
							}
						}
						if (FindEx != null && FindEx.CurrentExecuteData != null && !FindEx.CurrentExecuteData.NotRelevant && FindEx.CurrentExecuteData.RezultCommand.Status != Unit.ExecuteStatus.Ok && FindEx.CurrentExecuteData.RezultCommand.Status != Unit.ExecuteStatus.Error)
						{
							FindEx = FindEx.CurrentExecuteData;
						}
					}
					finally
					{
						if (Sem2)
						{
							ExecuteDatas.Semaphore.Release();
						}
					}
				}
				if (FindEx != null)
				{
					foreach (KeyValuePair<int, Unit> unit4 in Global.UnitManager.Units)
					{
						if (FindEx.NumDevice == unit4.Value.NumUnit)
						{
							unit4.Value.GetRezult(FindEx.DataCommand, FindEx.RezultCommand);
						}
					}
				}
				if (FindEx != null && FindEx.RezultCommand.Status != Unit.ExecuteStatus.Ok && FindEx.RezultCommand.Status != Unit.ExecuteStatus.Error && FindEx.RezultCommand.MessageHTML != null && FindEx.RezultCommand.TypeMessageHTM != "EndCommand")
				{
					ExecuteData.RezultCommand.MessageHTML = FindEx.RezultCommand.MessageHTML;
					ExecuteData.RezultCommand.TypeMessageHTM = FindEx.RezultCommand.TypeMessageHTM;
					FindEx.RezultCommand.MessageHTML = null;
					FindEx.RezultCommand.TypeMessageHTM = null;
				}
				if (FindEx == null)
				{
					ExecuteData.RezultCommand.Command = "StopTrackingStatus";
				}
				IsDone = true;
				ExecuteData.IdCommand = Guid.NewGuid().ToString();
			}
			else if (DataCommand.Command == "CancelCommand")
			{
				foreach (KeyValuePair<int, Unit> unit5 in Units)
				{
					if (unit5.Value.NumUnit == DataCommand.NumDevice)
					{
						unit5.Value.CancellationCommand = true;
						break;
					}
				}
				DataCommand.IdCommand = "";
				IsDone = true;
				ExecuteData.IdCommand = Guid.NewGuid().ToString();
			}
			else if (DataCommand.Command == "List")
			{
				ExecuteData FindEx = ExecuteData;
				FindEx.RezultCommand = await MenegerServer.List(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "OnOffUnut")
			{
				ExecuteData FindEx = ExecuteData;
				FindEx.RezultCommand = await MenegerServer.OnOffUnut(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetServerData")
			{
				ExecuteData.RezultCommand = MenegerServer.GetServerData(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetGoodCodeData")
			{
				ExecuteData FindEx = ExecuteData;
				FindEx.RezultCommand = await MenegerServer.GetGoodCodeData(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetSettingsServer")
			{
				ExecuteData.RezultCommand = MenegerServer.GetSettingsServer(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetTypesDevice")
			{
				ExecuteData.RezultCommand = MenegerServer.GetTypesDevice(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetDeviceParamets")
			{
				ExecuteData.RezultCommand = MenegerServer.GetDeviceParamets(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetDeviceSettings")
			{
				ExecuteData.RezultCommand = MenegerServer.GetDeviceSettings(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "SetSettingsServer")
			{
				ExecuteData.RezultCommand = MenegerServer.SetSettingsServer(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "SetDeviceSettings")
			{
				ExecuteData.RezultCommand = MenegerServer.SetDeviceSettings(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetDataLicense")
			{
				ExecuteData.RezultCommand = MenegerServer.GetDataLicense(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "GetLicense")
			{
				ExecuteData.RezultCommand = MenegerServer.GetLicense(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "FreeLicense")
			{
				ExecuteData.RezultCommand = MenegerServer.FreeLicense(ExecuteData, DataCommand, TypeSunc);
				IsDone = true;
			}
			else if (DataCommand.Command == "AddInGetSettings")
			{
				if (Global.Settings.Marke != DataCommand.Marke && DataCommand.Marke != null && DataCommand.Marke != "" && DataCommand.Marke != Global.Settings.Marke)
				{
					Global.Settings.Marke = DataCommand.Marke;
					await Global.SaveSettingsAsync();
					Global.TextLines.Clear();
					if (AddIn.TypeAddIn == AddIn.enTypeAddIn.None)
					{
						Global.RunAbout();
					}
					Global.UnitManager = new UnitManager();
					await Global.LoadSettingAsyncs();
				}
				Global.WriteLog("Выполняем локально - 3.1");
				ExecuteData.RezultCommand.Command = "AddInGetSettings";
				ExecuteData.RezultCommand.Url = Global.UriProgram;
				ExecuteData.RezultCommand.RunAsAddIn = false;
				ExecuteData.RezultCommand.RunAsAddIn = AddIn.TypeAddIn != AddIn.enTypeAddIn.None;
				ExecuteData.RezultCommand.LoginAdmin = Global.Settings.LoginAdmin;
				ExecuteData.RezultCommand.PassAdmin = Global.Settings.PassAdmin;
				if (Global.TypeProduct == "KkmServer")
				{
					ExecuteData.RezultCommand.Verson = Global.Verson;
				}
				else
				{
					ExecuteData.RezultCommand.Verson = "u_" + Global.Verson;
				}
				ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
				Global.WriteLog("Выполняем локально - 3.1.1");
				Unit.RezultCommand rezultCommand = ExecuteData.RezultCommand;
				rezultCommand.List = await MenegerServer.List(new ExecuteData(), new Unit.DataCommand(), "");
				Global.WriteLog("Выполняем локально - 3.1.4");
				ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Ok;
				IsDone = true;
				Global.WriteLog("Поставлен ответ - " + ExecuteData.RezultCommand.Command);
			}
		}
		if (!IsDone && ExecuteData.Type == TypeDevice.enType.ЭквайринговыйТерминал)
		{
			bool Sem2 = false;
			try
			{
				Sem2 = await ExecuteDatas.Semaphore.WaitAsync(1000);
				ExecuteDatas.TrimExcess();
				foreach (ExecuteData executeData5 in ExecuteDatas)
				{
					if (executeData5.DataCommand != null && executeData5.DataCommand.NumDevice == DataCommand.NumDevice && executeData5.Type == TypeDevice.enType.ЭквайринговыйТерминал && (executeData5.RezultCommand.Status == Unit.ExecuteStatus.Run || executeData5.RezultCommand.Status == Unit.ExecuteStatus.NotRun))
					{
						ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
						ExecuteData.RezultCommand.Error = "На терминале еще выполнятеся транзакция. Отмените транзакцию на терминале!";
						IsDone = true;
						ExecuteDatas.Enqueue(ExecuteData);
						break;
					}
				}
			}
			finally
			{
				if (Sem2)
				{
					ExecuteDatas.Semaphore.Release();
				}
			}
		}
		if (RezultCommandUseAddInDialog != null)
		{
			ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
			ExecuteData.RezultCommand.Error = "Отменено пользователем";
			ExecuteData.RezultCommand.Command = DataCommand.Command;
			ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
			RezultCommandUseAddInDialog.RezultCommand = ExecuteData.RezultCommand;
			ExecuteData.RezultCommand = RezultCommandUseAddInDialog;
			ExecuteData.RezultCommand.Command = "UseAddInDialog";
			ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
			ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Ok;
			ExecuteData.KeyCallback = KeyCallback;
			ExecuteData.RezultCommand.MessageFrom = "KkmServer";
			ExecuteData.RezultCommand.MessageTo = ExecuteData.DataCommand.MessageFrom;
		}
		else
		{
			ExecuteData.RezultCommand.Command = DataCommand.Command;
			ExecuteData.RezultCommand.IdCommand = DataCommand.IdCommand;
			ExecuteData.KeyCallback = KeyCallback;
			ExecuteData.RezultCommand.MessageFrom = "KkmServer";
			ExecuteData.RezultCommand.MessageTo = ExecuteData.DataCommand.MessageFrom;
			if (ExecuteData.RezultCommand is Unit.RezultCommandKKm)
			{
				(ExecuteData.RezultCommand as Unit.RezultCommandKKm).RezultProcessing = ExecuteData.DataCommand.RezultCommandProcessing;
			}
		}
		if (ExecuteData.CurrentNameCommand != "")
		{
			Unit.WindowTrackingStatus(ExecuteData.DataCommand, null, "Ожидание готовности устройства");
		}
		IsExecuteData IsExecuteData = null;
		if (!IsDone || KeyCallback != "")
		{
			IsExecuteData = new IsExecuteData
			{
				ExecuteData = ExecuteData,
				TextCommand = TextCommand
			};
			bool Sem2 = false;
			try
			{
				Sem2 = await IsExecuteDatas.Semaphore.WaitAsync(1000);
				bool Sem = false;
				try
				{
					Sem = await ExecuteDatas.Semaphore.WaitAsync(1000);
					ExecuteDatas.TrimExcess();
					ExecuteDatas.Enqueue(ExecuteData);
					IsExecuteDatas.Add(IsExecuteData);
					while (ExecuteDatas.Count > 100)
					{
						ExecuteDatas.Dequeue();
					}
				}
				finally
				{
					if (Sem)
					{
						ExecuteDatas.Semaphore.Release();
					}
				}
			}
			finally
			{
				if (Sem2)
				{
					IsExecuteDatas.Semaphore.Release();
				}
			}
		}
		if (IsDone || KeyCallback != "" || IsExecuteData == null || ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.AlreadyDone)
		{
			if (IsDone)
			{
				ExecuteData.RezultCommand.CommandEnd = true;
			}
			return ExecuteData.RezultCommand;
		}
		if (ExecuteData.DataCommand.Command == "RegisterCheck")
		{
			IsExecuteData.WaitHandle.WaitOne(100);
		}
		int num2 = 60000;
		ExecuteData.DataCommand.Timeout = int.Min(ExecuteData.DataCommand.Timeout, 600);
		if (ExecuteData.DataCommand.Timeout != 0)
		{
			num2 = (ExecuteData.DataCommand.Timeout - 1) * 1000;
		}
		if (ExecuteData.Type == TypeDevice.enType.ЭквайринговыйТерминал && ExecuteData.DataCommand.Timeout <= 120 && ExecuteData.DataCommand.Timeout != 1)
		{
			num2 = 125000;
			DataCommand.Timeout = 180;
		}
		if (!IsExecuteData.ExecuteData.RezultCommand.CommandEnd)
		{
			ExecuteData.DataCommand.WaitTimeout = num2 - 1000;
			IsExecuteData.WaitHandle.WaitOne(num2);
		}
		return ExecuteData.RezultCommand;
	}

	public async Task ManagerCommand(object Params)
	{
		bool MainThread = (bool)Params;
		List<Unit> UnitSorted = null;
		if (!(await IsExecuteDatas.Semaphore.WaitAsync(40)))
		{
			return;
		}
		try
		{
			WaitWorkStack.Reset();
			_ = Task.CurrentId;
			bool Sem = false;
			try
			{
				if (Global.UnitManager != null)
				{
					await Global.UnitManager.Units.Semaphore.WaitAsync();
					Sem = true;
				}
				foreach (IsExecuteData IsExecuteData in IsExecuteDatas)
				{
					IsExecuteData.IsFind = false;
					if (IsExecuteData.ExecuteData.RezultCommand.Status != Unit.ExecuteStatus.NotRun)
					{
						continue;
					}
					bool NeedLock = IsExecuteData.ExecuteData.Type != TypeDevice.enType.СканерШтрихкода && IsExecuteData.ExecuteData.Type != TypeDevice.enType.СчитывательМагнитныхКарт;
					List<Unit> listUnitsForCommand = GetListUnitsForCommand(IsExecuteData.ExecuteData.DataCommand, IsExecuteData.ExecuteData.Type, ref UnitSorted);
					foreach (Unit gUnit in listUnitsForCommand)
					{
						IsExecuteData.IsFind = true;
						if (gUnit.SettDr.TypeDevice.MainThread != MainThread)
						{
							continue;
						}
						if (NeedLock && gUnit.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал && gUnit.Semaphore.CurrentCount == 0)
						{
							IsExecuteData.Unit = gUnit;
							IsExecuteData.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
							IsExecuteData.ExecuteData.RezultCommand.Error = "Терминал заблокирован предыдущей оплатой/транзакцией. Команда отменена.";
							IsExecuteData.ExecuteData.DateRun = DateTime.Now;
							IsExecuteData.ExecuteData.DateEnd = DateTime.Now;
							IsExecuteData.WaitHandle.Set();
							await Global.Logers.AddError(IsExecuteData.Unit, IsExecuteData.ExecuteData, IsExecuteData.TextCommand);
							break;
						}
						bool flag = NeedLock;
						if (flag)
						{
							flag = !(await gUnit.Semaphore.WaitAsync(0));
						}
						if (flag)
						{
							continue;
						}
						try
						{
							IsExecuteData.Unit = gUnit;
							IsExecuteData.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Run;
							IsExecuteData.ExecuteData.DateRun = DateTime.Now;
							IsExecuteData.ExecuteData.INN = IsExecuteData.Unit.Kkm.INN;
							IsExecuteData.NeedLock = NeedLock;
							IsExecuteData.ExecuteData.RezultCommand.NumDevice = gUnit.SettDr.NumDevice;
							Global.Logers.RunCommand(IsExecuteData.ExecuteData);
						}
						catch (Exception ex)
						{
							_ = ex.Message;
						}
						finally
						{
							if (NeedLock)
							{
								gUnit.Semaphore.Release();
							}
						}
						if (MainThread)
						{
							await RunCommand(IsExecuteData);
						}
						else
						{
							IsExecuteData.Task = new Task(async delegate
							{
								await RunCommand(IsExecuteData);
							});
							IsExecuteData.Task.Start();
						}
						WaitWorkStack.WaitOne();
						break;
					}
				}
				while (true)
				{
					try
					{
						foreach (IsExecuteData IsExecuteData2 in IsExecuteDatas)
						{
							if (!IsExecuteData2.IsFind && IsExecuteData2.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun)
							{
								string text = "Устройство (с параметрами &Par) не найдено: не настроено или отключено.";
								if (IsExecuteData2.ExecuteData.DataCommand.NumDevice != 0)
								{
									text = text.Replace("&Par", "NumDevice=" + IsExecuteData2.ExecuteData.DataCommand.NumDevice + " &Par");
								}
								if (IsExecuteData2.ExecuteData.DataCommand.IdDevice != null && IsExecuteData2.ExecuteData.DataCommand.IdDevice != "")
								{
									text = text.Replace("&Par", "IdDevice=" + IsExecuteData2.ExecuteData.DataCommand.IdDevice + " &Par");
								}
								if (IsExecuteData2.ExecuteData.DataCommand.KktNumber != null && IsExecuteData2.ExecuteData.DataCommand.KktNumber != "")
								{
									text = text.Replace("&Par", "KktNumber=" + IsExecuteData2.ExecuteData.DataCommand.KktNumber.Trim() + " &Par");
								}
								if (IsExecuteData2.ExecuteData.DataCommand.InnKkm != null && IsExecuteData2.ExecuteData.DataCommand.InnKkm.Trim() != "")
								{
									text = text.Replace("&Par", "InnKkm=" + IsExecuteData2.ExecuteData.DataCommand.InnKkm.Trim() + " &Par");
								}
								if (IsExecuteData2.ExecuteData.DataCommand.TaxVariant != null && IsExecuteData2.ExecuteData.DataCommand.TaxVariant.Trim() != "")
								{
									text = text.Replace("&Par", "TaxVariant=" + IsExecuteData2.ExecuteData.DataCommand.TaxVariant.Trim() + " &Par");
								}
								text = text.Replace(" &Par", "");
								IsExecuteData2.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
								IsExecuteData2.ExecuteData.RezultCommand.Error = text;
								IsExecuteData2.ExecuteData.DateRun = DateTime.Now;
								IsExecuteData2.ExecuteData.DateEnd = DateTime.Now;
								IsExecuteData2.ExecuteData.RezultCommand.CommandEnd = true;
								IsExecuteData2.WaitHandle.Set();
								Unit.DataCommand dataCommand = new Unit.DataCommand();
								MenegerServer.RezultCommandList obj = await MenegerServer.List(new ExecuteData(), dataCommand, "");
								string netLogs = JsonConvert.SerializeObject(settings: new JsonSerializerSettings
								{
									DateFormatString = "yyyy-MM-ddTHH:mm:ss"
								}, value: obj.ListUnit);
								await Global.Logers.AddError(IsExecuteData2.Unit, IsExecuteData2.ExecuteData, IsExecuteData2.TextCommand, netLogs);
							}
						}
					}
					catch
					{
						await Task.Delay(100);
						continue;
					}
					break;
				}
				if (!MainThread)
				{
					Global.Logers.Update(null);
				}
				if (!MainThread)
				{
					foreach (IsExecuteData isExecuteData in IsExecuteDatas)
					{
						if (isExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.NotRun && Global.Settings.RemoveCommandInterval != 0 && isExecuteData.ExecuteData.DateStart.AddSeconds(Global.Settings.RemoveCommandInterval) < DateTime.Now)
						{
							isExecuteData.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
							isExecuteData.ExecuteData.RezultCommand.Error = "Истек допустимый период постановки команды на исполнение из очереди. Команда отменена.";
							isExecuteData.ExecuteData.DateRun = DateTime.Now;
							isExecuteData.ExecuteData.DateEnd = DateTime.Now;
							isExecuteData.WaitHandle.Set();
							await Global.Logers.AddError(isExecuteData.Unit, isExecuteData.ExecuteData, isExecuteData.TextCommand);
						}
					}
				}
				if (MainThread)
				{
					return;
				}
				List<IsExecuteData> list = new List<IsExecuteData>();
				foreach (IsExecuteData isExecuteData2 in IsExecuteDatas)
				{
					if (isExecuteData2.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Error || isExecuteData2.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Ok)
					{
						Global.Logers.EndCommand(isExecuteData2.ExecuteData);
						list.Add(isExecuteData2);
					}
				}
				foreach (IsExecuteData item in list)
				{
					IsExecuteDatas.Remove(item);
				}
			}
			finally
			{
				if (Sem)
				{
					Global.UnitManager.Units.Semaphore.Release();
				}
			}
		}
		finally
		{
			IsExecuteDatas.Semaphore.Release();
		}
	}

	public static List<Unit> GetListUnitsForCommand(Unit.DataCommand DataCommand, TypeDevice.enType Type, ref List<Unit> ListSortUnits)
	{
		List<Unit> list = new List<Unit>();
		if (ListSortUnits == null)
		{
			ListSortUnits = Global.UnitManager.Units.OrderByDescending(delegate(KeyValuePair<int, Unit> u)
			{
				KeyValuePair<int, Unit> keyValuePair = u;
				return keyValuePair.Value.Active;
			}).ThenByDescending(delegate(KeyValuePair<int, Unit> u)
			{
				KeyValuePair<int, Unit> keyValuePair = u;
				return keyValuePair.Value.IsInit;
			}).ThenBy(delegate(KeyValuePair<int, Unit> u)
			{
				KeyValuePair<int, Unit> keyValuePair = u;
				return keyValuePair.Value?.Kkm.FN_MemOverflowl;
			})
				.ThenBy(delegate(KeyValuePair<int, Unit> u)
				{
					KeyValuePair<int, Unit> keyValuePair = u;
					return keyValuePair.Value.LastCommandDate;
				})
				.Select(delegate(KeyValuePair<int, Unit> u)
				{
					KeyValuePair<int, Unit> keyValuePair = u;
					return keyValuePair.Value;
				})
				.ToList();
		}
		if (Type == TypeDevice.enType.ЭквайринговыйТерминал && DataCommand.NumDevice == 0)
		{
			int num = 0;
			foreach (KeyValuePair<int, Unit> unit3 in Global.UnitManager.Units)
			{
				num = Math.Max(num, unit3.Value.NumUnit);
			}
			foreach (KeyValuePair<int, Unit> unit4 in Global.UnitManager.Units)
			{
				if (unit4.Value.SettDr.TypeDevice.Types.Contains(TypeDevice.enType.ЭквайринговыйТерминал) && unit4.Value.SupportsSBP)
				{
					Unit unit = new Unit(unit4.Value.SettDr, ++num);
					unit.Active = unit4.Value.Active;
					unit.IsInit = unit4.Value.IsInit;
					unit.UnitParamets = unit4.Value.UnitParamets;
					unit.SettDr = unit4.Value.SettDr;
					unit.Kkm = unit4.Value.Kkm;
					unit.NumUnit = num;
					if (unit4.Value.UnitName != "")
					{
						unit.UnitName = unit4.Value.UnitName + " СБП QR";
					}
					if (unit4.Value.NameDevice != "")
					{
						unit.NameDevice = unit4.Value.NameDevice + " СБП QR";
					}
					ListSortUnits.Add(unit);
				}
			}
		}
		foreach (Unit ListSortUnit in ListSortUnits)
		{
			if ((DataCommand.Command != "DoAdditionalAction" && DataCommand.Command != "DeviceTest" && (Type != TypeDevice.enType.ФискальныйРегистратор || ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ПринтерЧеков || !ListSortUnit.SettDr.Paramets["EmulationCheck"].AsBool()) && !ListSortUnit.SettDr.TypeDevice.Types.Contains(Type)) || !ListSortUnit.Active || (!ListSortUnit.IsInit && DataCommand.NumDevice == 0 && DataCommand.IdDevice == null && DataCommand.IdDevice == "" && DataCommand.KktNumber == null && DataCommand.KktNumber == "") || (DataCommand.Command == "InitDevice" && DataCommand.NumDevice == 0 && (DataCommand.IdDevice == null || DataCommand.IdDevice == "") && (DataCommand.KktNumber == null || DataCommand.KktNumber == "")) || (DataCommand.NumDevice != 0 && DataCommand.NumDevice != ListSortUnit.SettDr.NumDevice) || (DataCommand.IdDevice != null && DataCommand.IdDevice != "" && DataCommand.IdDevice != ListSortUnit.SettDr.IdDevice.Trim()))
			{
				continue;
			}
			string text = "<не определено>";
			string text2 = "";
			string text3 = "";
			if (ListSortUnit.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал && DataCommand.InnKkm != null && DataCommand.InnKkm.Trim() != "" && ListSortUnit.UnitParamets.ContainsKey("NumDeviceByPrintSlip") && ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "")
			{
				Unit unit2 = null;
				if (ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "" && ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "0" && Global.UnitManager.Units.ContainsKey(int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])) && Global.UnitManager.Units[int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])] != null && (Global.UnitManager.Units[int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])].SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || Global.UnitManager.Units[int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])].SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков))
				{
					unit2 = Global.UnitManager.Units[int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])];
				}
				else if (ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] == "0")
				{
					foreach (KeyValuePair<int, Unit> unit5 in Global.UnitManager.Units)
					{
						if (unit5.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && unit5.Value.Active && unit5.Value.IsInit)
						{
							unit2 = unit5.Value;
							break;
						}
					}
				}
				if (unit2 != null)
				{
					text = unit2.SettDr.INN.Trim();
					text3 = unit2.SettDr.NumberKkm.Trim();
					if (unit2.Kkm.IsKKT)
					{
						text2 = unit2.SettDr.TaxVariant;
					}
				}
			}
			if (ListSortUnit.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал && ListSortUnit.UnitParamets["INN"] != "")
			{
				text = ListSortUnit.UnitParamets["INN"];
			}
			if (ListSortUnit.SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
			{
				if (ListSortUnit.UnitParamets["INN"] != "")
				{
					text = ListSortUnit.UnitParamets["INN"];
				}
				else if (ListSortUnit.UnitParamets.ContainsKey("NumDeviceByPrintSlip") && ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "" && ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "0" && Global.UnitManager.Units.ContainsKey(int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])) && Global.UnitManager.Units[int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])] != null && Global.UnitManager.Units[int.Parse(ListSortUnit.UnitParamets["NumDeviceByPrintSlip"])].SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
				{
					text = Global.UnitManager.Units[ListSortUnit.UnitParamets["NumDeviceByPrintSlip"].AsInt()].Kkm.INN;
				}
			}
			if (ListSortUnit.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал && ListSortUnit.UnitParamets["INN"] != "" && text != "" && ListSortUnit.UnitParamets.ContainsKey("NumDeviceByPrintSlip") && ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "")
			{
				ListSortUnit.Kkm.INN = text;
			}
			if (ListSortUnit.SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков && ListSortUnit.UnitParamets["INN"] != "" && text != "" && ListSortUnit.UnitParamets.ContainsKey("NumDeviceByPrintSlip") && ListSortUnit.UnitParamets["NumDeviceByPrintSlip"] != "")
			{
				ListSortUnit.Kkm.INN = text;
			}
			if ((DataCommand.InnKkm == null || !(DataCommand.InnKkm.Trim() != "") || ((ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ФискальныйРегистратор || !(DataCommand.InnKkm != ListSortUnit.SettDr.INN.Trim())) && (ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ЭквайринговыйТерминал || !(DataCommand.InnKkm != text)))) && (DataCommand.TaxVariant == null || !(DataCommand.TaxVariant.Trim() != "") || ((ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ФискальныйРегистратор || !ListSortUnit.Kkm.IsKKT || !(ListSortUnit.SettDr.TaxVariant != "") || ListSortUnit.SettDr.TaxVariant.IndexOf(DataCommand.TaxVariant.Trim()) != -1) && (ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ЭквайринговыйТерминал || !(text2 != "") || text2.IndexOf(DataCommand.TaxVariant.Trim()) != -1))) && (DataCommand.KktNumber == null || !(DataCommand.KktNumber != "") || ((ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ФискальныйРегистратор || ListSortUnit.SettDr.NumberKkm == null || !(DataCommand.KktNumber != ListSortUnit.SettDr.NumberKkm.Trim())) && (ListSortUnit.SettDr.TypeDevice.Type != TypeDevice.enType.ЭквайринговыйТерминал || !(text3 != "") || !(DataCommand.KktNumber != text3)))))
			{
				list.Add(ListSortUnit);
			}
		}
		return list;
	}

	public static void GetInfoCommand(Unit.DataCommand DataCommand, out string NameOperation, out decimal Summ, out string CommandForAddOperation)
	{
		NameOperation = "";
		Summ = default(decimal);
		CommandForAddOperation = "";
		if (DataCommand.Command == "RegisterCheck" && DataCommand.IsFiscalCheck)
		{
			Summ = DataCommand.Cash + DataCommand.ElectronicPayment + DataCommand.AdvancePayment + DataCommand.Credit + DataCommand.CashProvision;
			if (DataCommand.TypeCheck == 0)
			{
				NameOperation = "Чек продажи/прихода";
				CommandForAddOperation = DataCommand.Command;
			}
			else if (DataCommand.TypeCheck == 1)
			{
				NameOperation = "Чек возврата продажи/прихода";
				CommandForAddOperation = DataCommand.Command;
				Summ = -Summ;
			}
			else if (DataCommand.TypeCheck == 2)
			{
				NameOperation = "Чек корректировки продажи/прихода";
			}
			else if (DataCommand.TypeCheck == 3)
			{
				NameOperation = "Чек корректировки возврата продажи/прихода;";
				Summ = -Summ;
			}
			else if (DataCommand.TypeCheck == 10)
			{
				NameOperation = "Чек покупки/расхода";
				CommandForAddOperation = DataCommand.Command;
				Summ = -Summ;
			}
			else if (DataCommand.TypeCheck == 11)
			{
				NameOperation = "Чек возврата покупки/расхода";
				CommandForAddOperation = DataCommand.Command;
			}
			else if (DataCommand.TypeCheck == 12)
			{
				NameOperation = "Чек корректировки покупки/расхода";
				Summ = -Summ;
			}
			else if (DataCommand.TypeCheck == 13)
			{
				NameOperation = "Чек корректировки возврата покупки/расхода";
			}
		}
		else if (DataCommand.Command == "CloseShift")
		{
			NameOperation = "Закрытие смены";
		}
		else if (DataCommand.Command == "DepositingCash")
		{
			NameOperation = "Внесение ДС в кассу";
			Summ = DataCommand.Amount;
		}
		else if (DataCommand.Command == "PaymentCash")
		{
			NameOperation = "Выплата ДС из кассы";
			Summ = -DataCommand.Amount;
		}
		else if (DataCommand.Command == "PayByPaymentCard")
		{
			NameOperation = "Оплата по Эквайрингу";
			CommandForAddOperation = DataCommand.Command;
			Summ = DataCommand.Amount;
		}
		else if (DataCommand.Command == "ReturnPaymentByPaymentCard")
		{
			NameOperation = "Возврат оплаты по Эквайрингу";
			Summ = -DataCommand.Amount;
		}
		else if (DataCommand.Command == "CancelPaymentByPaymentCard")
		{
			NameOperation = "Отмена оплаты по Эквайрингу";
			Summ = -DataCommand.Amount;
		}
		else if (DataCommand.Command == "AuthorisationByPaymentCard")
		{
			NameOperation = "Авторизация оплаты по Эквайрингу";
			Summ = DataCommand.Amount;
		}
		else if (DataCommand.Command == "AuthConfirmationByPaymentCard")
		{
			NameOperation = "Оплата по авторизации Эквайринга";
			CommandForAddOperation = DataCommand.Command;
			Summ = DataCommand.Amount;
		}
		else if (DataCommand.Command == "CancelAuthorisationByPaymentCard")
		{
			NameOperation = "Расблокировка суммы по авторизации Эквайринга";
			Summ = -DataCommand.Amount;
		}
		else if (DataCommand.Command == "EmergencyReversal")
		{
			NameOperation = "Аварийная отмена операции по Эквайрингу";
			Summ = -DataCommand.Amount;
		}
		else if (DataCommand.Command == "Settlement")
		{
			NameOperation = "Итоги по Эквайрингу";
		}
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public async Task RunCommand(IsExecuteData IsExecuteData)
	{
		if (IsExecuteData.Unit == null)
		{
			return;
		}
		bool Sem = false;
		try
		{
			_ = 1;
			try
			{
				if (IsExecuteData.NeedLock)
				{
					Sem = await IsExecuteData.Unit.Semaphore.WaitAsync(1000);
				}
				WaitWorkStack.Set();
				await IsExecuteData.Unit.ExecuteCommand(IsExecuteData.ExecuteData.DataCommand, IsExecuteData.ExecuteData.RezultCommand);
			}
			catch (Exception ex)
			{
				IsExecuteData.ExecuteData.RezultCommand.Error = Global.GetErrorMessagee(ex);
				IsExecuteData.ExecuteData.DateEnd = DateTime.Now;
				IsExecuteData.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
			}
		}
		finally
		{
			try
			{
				IsExecuteData.ExecuteData.DateEnd = DateTime.Now;
				if (IsExecuteData.ExecuteData.RezultCommand.Error == "")
				{
					if (!IsExecuteData.Unit.IsNotSetOrStatus)
					{
						IsExecuteData.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Ok;
					}
				}
				else if (!IsExecuteData.Unit.IsNotErrorStatus)
				{
					IsExecuteData.ExecuteData.RezultCommand.Status = Unit.ExecuteStatus.Error;
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				if (IsExecuteData.NeedLock && Sem)
				{
					IsExecuteData.Unit.Semaphore.Release();
				}
				IsExecuteData.WaitHandle.Set();
				if (IsExecuteData.ExecuteData.RezultCommand.Status == Unit.ExecuteStatus.Ok)
				{
					bool RegisterAllCommand = Global.Settings.RegisterAllCommand;
					if (IsExecuteData.ExecuteData.Type == TypeDevice.enType.ЭквайринговыйТерминал)
					{
						RegisterAllCommand = true;
					}
					string CommandForAddOperation = "";
					string NameOperation = "";
					decimal Summ = default(decimal);
					string text = "";
					string comment = "";
					bool flag = false;
					GetInfoCommand(IsExecuteData.ExecuteData.DataCommand, out NameOperation, out Summ, out CommandForAddOperation);
					if (IsExecuteData.ExecuteData.Type == TypeDevice.enType.ФискальныйРегистратор)
					{
						text = IsExecuteData.Unit.SettDr.INN;
						comment = IsExecuteData.ExecuteData.DataCommand.ClientAddress;
					}
					else if (IsExecuteData.ExecuteData.Type == TypeDevice.enType.ЭквайринговыйТерминал)
					{
						text = IsExecuteData.Unit.SettDr.INN;
						flag = true;
					}
					if (flag)
					{
						text = IsExecuteData.Unit.SettDr.INN.Trim();
						if (text == "" && IsExecuteData.Unit.UnitParamets["NumDeviceByPrintSlip"] != "" && IsExecuteData.Unit.UnitParamets["NumDeviceByPrintSlip"] != "0" && Global.UnitManager.Units.ContainsKey(int.Parse(IsExecuteData.Unit.UnitParamets["NumDeviceByPrintSlip"])) && Global.UnitManager.Units[int.Parse(IsExecuteData.Unit.UnitParamets["NumDeviceByPrintSlip"])] != null && Global.UnitManager.Units[int.Parse(IsExecuteData.Unit.UnitParamets["NumDeviceByPrintSlip"])].SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
						{
							text = Global.UnitManager.Units[int.Parse(IsExecuteData.Unit.UnitParamets["NumDeviceByPrintSlip"])].SettDr.INN.Trim();
						}
						if (((Unit.RezultCommandProcessing)IsExecuteData.ExecuteData.RezultCommand).UniversalID != null && ((Unit.RezultCommandProcessing)IsExecuteData.ExecuteData.RezultCommand).UniversalID != "")
						{
							comment = ((Unit.RezultCommandProcessing)IsExecuteData.ExecuteData.RezultCommand).UniversalID;
						}
					}
					else if (IsExecuteData.ExecuteData.Type == TypeDevice.enType.ФискальныйРегистратор)
					{
						try
						{
							comment = ((Unit.RezultCommandKKm)IsExecuteData.ExecuteData.RezultCommand).QRCode;
						}
						catch
						{
						}
					}
					if (NameOperation != "")
					{
						Logers.Operation operation = new Logers.Operation();
						operation.NumUnit = IsExecuteData.Unit.SettDr.NumDevice;
						operation.INN = text;
						operation.NameOperation = NameOperation;
						operation.Summ = Summ;
						operation.DeviceType = IsExecuteData.ExecuteData.Type;
						operation.Comment = comment;
						if (CommandForAddOperation != "")
						{
							operation.UID = Guid.NewGuid().ToString();
							operation.TextCommand = IsExecuteData.TextCommand;
						}
						await Global.Logers.OperationsHistory.Add(operation, DateTime.Now);
					}
					if (RegisterAllCommand || IsExecuteData.Unit.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал)
					{
						await Global.Logers.AddError(IsExecuteData.Unit, IsExecuteData.ExecuteData, IsExecuteData.TextCommand, "", IsExecuteData.ExecuteData.RezultCommand);
					}
				}
				else if (!IsExecuteData.ExecuteData.DataCommand.NoError)
				{
					await Global.Logers.AddError(IsExecuteData.Unit, IsExecuteData.ExecuteData, IsExecuteData.TextCommand, "", IsExecuteData.ExecuteData.RezultCommand);
				}
			}
		}
	}
}
