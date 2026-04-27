using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public static class MenegerServer
{
	[DataContract]
	public class RezultCommandList : Unit.RezultCommand
	{
		[DataContract]
		public class ListStr
		{
			[DataMember(Name = "NumDevice")]
			public int NumDevice;

			[DataMember(Name = "IdDevice")]
			public string IdDevice = "";

			[DataMember(Name = "OnOff")]
			public bool OnOff;

			[DataMember(Name = "Active")]
			public bool Active;

			[DataMember(Name = "TypeDevice")]
			public string TypeDevice = "";

			[DataMember(Name = "IdTypeDevice")]
			public string IdTypeDevice = "";

			[DataMember(Name = "Firmware_Version")]
			public string Firmware_Version = "";

			[DataMember(Name = "IP")]
			public string IP = "";

			[DataMember(Name = "Port")]
			public string Port = "";

			[DataMember(Name = "NameDevice")]
			public string NameDevice = "";

			[DataMember(Name = "UnitName")]
			public string UnitName = "";

			[DataMember(Name = "KktNumber")]
			public string KktNumber = "";

			[DataMember(Name = "INN")]
			public string INN = "";

			[DataMember(Name = "RegNumber")]
			public string RegNumber = "";

			[DataMember(Name = "FnNumber")]
			public string FnNumber = "";

			[DataMember(Name = "InnOfd")]
			public string InnOfd = "";

			[DataMember(Name = "NameOrganization")]
			public string NameOrganization = "";

			[DataMember(Name = "AddressSettle")]
			public string AddressSettle = "";

			[DataMember(Name = "TaxVariant")]
			public string TaxVariant = "";

			[DataMember(Name = "AddDate")]
			public DateTime AddDate;

			[DataMember(Name = "BSOMode")]
			public bool BSOMode;

			[DataMember(Name = "ServiceMode")]
			public bool ServiceMode;

			[DataMember(Name = "OFD_Error")]
			public string OFD_Error = "";

			[DataMember(Name = "OFD_NumErrorDoc")]
			public int OFD_NumErrorDoc;

			[DataMember(Name = "OFD_DateErrorDoc")]
			public DateTime OFD_DateErrorDoc;

			[DataMember(Name = "FN_DateEnd")]
			public DateTime FN_DateEnd;

			[DataMember(Name = "FN_MemOverflowl")]
			public bool FN_MemOverflowl;

			[DataMember(Name = "FN_IsFiscal")]
			public bool FN_IsFiscal;

			[DataMember(Name = "PaperOver")]
			public bool PaperOver;

			[DataMember(Name = "FFDVersion")]
			public string FFDVersion;

			[DataMember(Name = "FFDVersionFN")]
			public string FFDVersionFN;

			[DataMember(Name = "FFDVersionKKT")]
			public string FFDVersionKKT;

			[DataMember(Name = "IsRegisterCheck")]
			public bool IsRegisterCheck;
		}

		[DataMember(Name = "ListUnit")]
		public List<ListStr> ListUnit;
	}

	[DataContract]
	public class RezultServerData : Unit.RezultCommand
	{
		[DataContract]
		public class tServerData
		{
			[DataMember(Name = "ServerName")]
			public string ServerName;

			[DataMember(Name = "ServerVersion")]
			public string ServerVersion = "";

			[DataMember(Name = "ServerDateStart")]
			public DateTime ServerDateStart;

			[DataMember(Name = "ServerUpTime")]
			public string ServerUpTime;

			[DataMember(Name = "LicenseExpirationDate")]
			public DateTime LicenseExpirationDate;

			[DataMember(Name = "LicenseCount")]
			public int LicenseCount;

			[DataMember(Name = "OSVersion")]
			public string OSVersion = "";

			[DataMember(Name = "OSPlatform")]
			public string OSPlatform = "";

			[DataMember(Name = "OSVersionString")]
			public string OSVersionString = "";

			[DataMember(Name = "OSName")]
			public string OSName;

			[DataMember(Name = "OSDateStart")]
			public DateTime OSDateStart;

			[DataMember(Name = "OSUpTime")]
			public string OSUpTime;

			[DataMember(Name = "OSCurDateTime")]
			public DateTime OSCurDateTime;

			[DataMember(Name = "PCServerName")]
			public string PCServerName;

			[DataMember(Name = "PCUserName")]
			public string PCUserName;

			[DataMember(Name = "PCPhysicalMemory")]
			public string PCPhysicalMemory;

			[DataMember(Name = "PCFreePhysicalMemory")]
			public string PCFreePhysicalMemory;

			[DataMember(Name = "PCFreeDiskSpace")]
			public string PCFreeDiskSpace;

			[DataMember(Name = "PCProcessorName")]
			public string PCProcessorName;

			[DataMember(Name = "PCNumberOfCores")]
			public string PCNumberOfCores;
		}

		[DataMember(Name = "ServerData")]
		public tServerData ServerData = new tServerData();
	}

	[DataContract]
	public class RezultDataLicense : Unit.RezultCommand
	{
		[DataMember(Name = "LicenseExpirationDate")]
		public DateTime LicenseExpirationDate;

		[DataMember(Name = "LicenseCount")]
		public int LicenseCount;

		[DataMember(Name = "SerialNuber")]
		public string SerialNuber;
	}

	[DataContract]
	public class RezultGoodCodeData : Unit.RezultCommand
	{
		[DataMember(Name = "DataProductCode")]
		public MarkingCode.DataProductCode DataProductCode = new MarkingCode.DataProductCode();

		[DataMember(Name = "ValidationPR")]
		public Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR ValidationPR = new Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR();

		[DataMember(Name = "ValidationKKT")]
		public Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationKKT ValidationKKT = new Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationKKT();
	}

	public static async Task<RezultCommandList> List(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		RezultCommandList Rez = new RezultCommandList
		{
			Error = "",
			Command = DataCommand.Command,
			Status = Unit.ExecuteStatus.Error
		};
		bool Sem = false;
		try
		{
			Sem = await Global.UnitManager.Units.Semaphore.WaitAsync(1000);
			int num = 0;
			foreach (KeyValuePair<int, Unit> unit in Global.UnitManager.Units)
			{
				num = Math.Max(num, unit.Value.NumUnit);
			}
			Rez.ListUnit = new List<RezultCommandList.ListStr>();
			List<RezultCommandList.ListStr> list = new List<RezultCommandList.ListStr>();
			foreach (KeyValuePair<int, Unit> unit2 in Global.UnitManager.Units)
			{
				if (unit2.Value == null || (DataCommand.NumDevice != 0 && DataCommand.NumDevice != unit2.Value.SettDr.NumDevice) || (DataCommand.InnKkm != null && !(DataCommand.InnKkm == "") && !(DataCommand.InnKkm == unit2.Value.Kkm.INN)) || (DataCommand.OnOff.HasValue && !(DataCommand.OnOff.GetType() != typeof(bool)) && DataCommand.OnOff != unit2.Value.Active) || (DataCommand.Active.HasValue && !(DataCommand.Active.GetType() != typeof(bool)) && DataCommand.Active != unit2.Value.IsInit) || (DataCommand.OFD_Error.HasValue && !(DataCommand.OFD_Error.GetType() != typeof(bool)) && DataCommand.OFD_Error != (unit2.Value.Kkm.OFD_Error != "")) || (DataCommand.OFD_DateErrorDoc.HasValue && !(DataCommand.OFD_DateErrorDoc < new DateTime(2000, 1, 1)) && !(DataCommand.OFD_DateErrorDoc >= unit2.Value.Kkm.OFD_DateErrorDoc)) || (DataCommand.FN_DateEnd.HasValue && !(DataCommand.FN_DateEnd < new DateTime(2000, 1, 1)) && !(DataCommand.FN_DateEnd >= unit2.Value.Kkm.FN_DateEnd)) || (DataCommand.FN_MemOverflowl.HasValue && !(DataCommand.FN_MemOverflowl.GetType() != typeof(bool)) && DataCommand.FN_MemOverflowl != unit2.Value.Kkm.FN_MemOverflowl) || (DataCommand.FN_IsFiscal.HasValue && !(DataCommand.FN_IsFiscal.GetType() != typeof(bool)) && DataCommand.FN_IsFiscal != unit2.Value.Kkm.FN_IsFiscal))
				{
					continue;
				}
				for (int i = 0; i <= 1; i++)
				{
					if (i != 1 || (unit2.Value.SettDr.TypeDevice.Types.Contains(TypeDevice.enType.ЭквайринговыйТерминал) && unit2.Value.SupportsSBP))
					{
						RezultCommandList.ListStr listStr = new RezultCommandList.ListStr();
						listStr.NumDevice = unit2.Value.NumUnit;
						listStr.IdDevice = unit2.Value.SettDr.IdDevice;
						listStr.OnOff = unit2.Value.SettDr.Active;
						listStr.Active = unit2.Value.IsInit;
						listStr.TypeDevice = TypeDevice.NameType[(int)unit2.Value.SettDr.TypeDevice.Type];
						listStr.IdTypeDevice = unit2.Value.SettDr.TypeDevice.Id;
						listStr.Firmware_Version = unit2.Value.Kkm.Firmware_Version;
						listStr.IP = ((unit2.Value is UnitPort) ? ((UnitPort)unit2.Value).SetPort.IP : "<Не определено>");
						listStr.Port = ((unit2.Value is UnitPort) ? ((UnitPort)unit2.Value).SetPort.Port : "<Не определено>");
						listStr.NameDevice = unit2.Value.NameDevice;
						try
						{
							listStr.UnitName = unit2.Value.UnitParamets["NameDevice"];
						}
						catch
						{
							listStr.UnitName = unit2.Value.UnitName;
						}
						listStr.KktNumber = unit2.Value.SettDr.NumberKkm;
						listStr.INN = unit2.Value.SettDr.INN;
						listStr.FnNumber = unit2.Value.Kkm.Fn_Number;
						listStr.RegNumber = unit2.Value.Kkm.RegNumber;
						listStr.InnOfd = unit2.Value.Kkm.InnOfd;
						listStr.NameOrganization = unit2.Value.Kkm.Organization;
						listStr.AddressSettle = unit2.Value.Kkm.AddressSettle;
						listStr.TaxVariant = unit2.Value.SettDr.TaxVariant;
						listStr.AddDate = unit2.Value.SettDr.AddDate;
						listStr.BSOMode = unit2.Value.Kkm.BSOMode;
						listStr.ServiceMode = unit2.Value.Kkm.ServiceMode;
						listStr.OFD_Error = unit2.Value.Kkm.OFD_Error;
						listStr.OFD_NumErrorDoc = unit2.Value.Kkm.OFD_NumErrorDoc;
						listStr.OFD_DateErrorDoc = unit2.Value.Kkm.OFD_DateErrorDoc;
						listStr.FN_DateEnd = unit2.Value.Kkm.FN_DateEnd;
						listStr.FN_IsFiscal = unit2.Value.Kkm.FN_IsFiscal;
						listStr.FN_MemOverflowl = unit2.Value.Kkm.FN_MemOverflowl;
						listStr.PaperOver = unit2.Value.Kkm.PaperOver;
						if (unit2.Value.Kkm.FfdVersion == 1)
						{
							listStr.FFDVersion = "1.0";
							listStr.FFDVersionFN = "1.0";
							listStr.FFDVersionKKT = "1.0";
						}
						else if (unit2.Value.Kkm.FfdVersion == 2)
						{
							listStr.FFDVersion = "1.05";
							listStr.FFDVersionFN = "1.0";
							listStr.FFDVersionKKT = "1.1";
						}
						else if (unit2.Value.Kkm.FfdVersion == 3)
						{
							listStr.FFDVersion = "1.1";
							listStr.FFDVersionFN = "1.1";
							listStr.FFDVersionKKT = "1.1";
						}
						if (listStr.TypeDevice == "Фискальный регистратор")
						{
							listStr.IsRegisterCheck = true;
						}
						else if (listStr.TypeDevice == "Принтер чеков" && unit2.Value.SettDr.Paramets["EmulationCheck"].AsBool())
						{
							listStr.IsRegisterCheck = true;
						}
						else
						{
							listStr.IsRegisterCheck = false;
						}
						if (i == 0)
						{
							Rez.ListUnit.Add(listStr);
						}
						else
						{
							list.Add(listStr);
						}
					}
				}
			}
			foreach (RezultCommandList.ListStr item in list)
			{
				num = (item.NumDevice = num + 1);
				item.NameDevice += " СБП QR";
				item.IdTypeDevice += "_QR";
				Rez.ListUnit.Add(item);
			}
		}
		catch (Exception ex)
		{
			Rez.Error = Global.GetErrorMessagee(ex);
		}
		finally
		{
			if (Sem)
			{
				Global.UnitManager.Units.Semaphore.Release();
			}
		}
		Rez.Status = Unit.ExecuteStatus.Ok;
		ExecuteData.RezultCommand = Rez;
		return Rez;
	}

	public static async Task<Unit.RezultCommand> OnOffUnut(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		Unit.RezultCommand Rez = new Unit.RezultCommand
		{
			Error = "",
			Command = DataCommand.Command,
			Status = Unit.ExecuteStatus.Error
		};
		try
		{
			await Global.Settings.Devices.Semaphore.WaitAsync();
			await Global.UnitManager.Units.Semaphore.WaitAsync();
			if (Global.Settings.Devices.ContainsKey(DataCommand.NumDevice))
			{
				Global.Settings.Devices[DataCommand.NumDevice].Active = DataCommand.Active.Value;
				Global.UnitManager.Units[DataCommand.NumDevice].Active = DataCommand.Active.Value;
				await Global.SaveSettingsAsync();
				Rez.Status = Unit.ExecuteStatus.Ok;
			}
			else
			{
				Rez.Error = "Устройство не найдено";
				Rez.Status = Unit.ExecuteStatus.Error;
			}
		}
		catch (Exception ex)
		{
			Rez.Status = Unit.ExecuteStatus.Error;
			Rez.Error = Global.GetErrorMessagee(ex);
		}
		finally
		{
			Global.Settings.Devices.Semaphore.Release();
			Global.UnitManager.Units.Semaphore.Release();
		}
		if (DataCommand.Active == true)
		{
			Unit.DataCommand dataCommand = new Unit.DataCommand();
			dataCommand.Command = "InitDevice";
			dataCommand.NumDevice = Global.Settings.Devices[DataCommand.NumDevice].NumDevice;
			await Global.UnitManager.AddCommand(dataCommand, "", "");
		}
		ExecuteData.RezultCommand = Rez;
		return Rez;
	}

	public static RezultServerData GetServerData(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		RezultServerData rezultServerData = new RezultServerData();
		try
		{
			rezultServerData.ServerData.ServerName = Global.NameService;
			rezultServerData.ServerData.ServerVersion = Global.Verson;
			rezultServerData.ServerData.ServerDateStart = Global.DateStart;
			rezultServerData.ServerData.ServerUpTime = (DateTime.Now - Global.DateStart).ToString("dd\\ hh\\:mm\\:ss");
		}
		catch
		{
			rezultServerData.ServerData.ServerName = "Не удалось получить";
			rezultServerData.ServerData.ServerVersion = "Не удалось получить";
		}
		try
		{
			ComDevice.InDate result = ComDevice.ReadComDevice(null, AllowDateAction: false, AllowCount: true, OnlySerial: false, PluzBesplatnoe: true).Result;
			rezultServerData.ServerData.LicenseCount = result.Int;
			rezultServerData.ServerData.LicenseExpirationDate = ((result.DateTime != default(DateTime)) ? result.DateTime.AddDays(1.0) : result.DateTime.AddDays(1.0));
		}
		catch
		{
		}
		try
		{
			rezultServerData.ServerData.OSVersionString = Environment.OSVersion.VersionString;
			rezultServerData.ServerData.OSPlatform = Environment.OSVersion.Platform.ToString();
			rezultServerData.ServerData.OSVersion = Environment.OSVersion.Version.ToString();
		}
		catch
		{
			rezultServerData.ServerData.OSVersionString = "Не удалось получить";
			rezultServerData.ServerData.OSPlatform = "Не удалось получить";
			rezultServerData.ServerData.OSVersion = "Не удалось получить";
		}
		try
		{
			PerformanceCounter performanceCounter = new PerformanceCounter("System", "System Up Time");
			performanceCounter.NextValue();
			TimeSpan timeSpan = TimeSpan.FromSeconds(performanceCounter.NextValue());
			rezultServerData.ServerData.OSUpTime = timeSpan.ToString("dd\\ hh\\:mm\\:ss");
			rezultServerData.ServerData.OSDateStart = DateTime.Now - timeSpan;
			rezultServerData.ServerData.OSCurDateTime = DateTime.Now;
		}
		catch
		{
		}
		try
		{
			rezultServerData.ServerData.PCUserName = Environment.UserName;
		}
		catch
		{
			rezultServerData.ServerData.PCUserName = "Не удалось получить";
		}
		try
		{
			using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get().GetEnumerator();
			if (managementObjectEnumerator.MoveNext())
			{
				ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
				rezultServerData.ServerData.OSName = managementObject["Name"].ToString().Substring(0, managementObject["Name"].ToString().IndexOf('|'));
				rezultServerData.ServerData.PCFreePhysicalMemory = (double.Parse(managementObject["FreePhysicalMemory"].ToString()) / 1024.0 / 1024.0).ToString("N") + " Gb";
				rezultServerData.ServerData.PCServerName = managementObject.ClassPath.Server;
			}
		}
		catch
		{
		}
		string text = "C:";
		try
		{
			text = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).Substring(0, 2);
		}
		catch
		{
		}
		try
		{
			foreach (ManagementObject item in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Volume").Get())
			{
				if (item["DriveLetter"] != null && item["DriveLetter"].ToString() == text)
				{
					rezultServerData.ServerData.PCFreeDiskSpace = (double.Parse(item["FreeSpace"].ToString()) / 1024.0 / 1024.0 / 1024.0).ToString("N") + " Gb";
					break;
				}
			}
		}
		catch (Exception ex)
		{
			rezultServerData.ServerData.PCFreeDiskSpace = "path=" + text + ", " + ex.Message;
		}
		try
		{
			using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get().GetEnumerator();
			if (managementObjectEnumerator.MoveNext())
			{
				ManagementObject managementObject3 = (ManagementObject)managementObjectEnumerator.Current;
				rezultServerData.ServerData.PCProcessorName = managementObject3["Name"].ToString();
				rezultServerData.ServerData.PCNumberOfCores = managementObject3["NumberOfCores"].ToString();
			}
		}
		catch
		{
		}
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
			double num = 0.0;
			foreach (ManagementObject item2 in managementObjectSearcher.Get())
			{
				num += Math.Round(Convert.ToDouble(item2["Capacity"]) / 1024.0 / 1024.0 / 1024.0, 2);
			}
			rezultServerData.ServerData.PCPhysicalMemory = num.ToString("N") + " Gb";
		}
		catch
		{
		}
		return rezultServerData;
	}

	public static async Task<RezultGoodCodeData> GetGoodCodeData(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		RezultGoodCodeData RezultGoodCodeData = new RezultGoodCodeData();
		try
		{
			RezultGoodCodeData.DataProductCode = MarkingCode.ParseBarCode(DataCommand.BarCode);
			bool flag = false;
			foreach (KeyValuePair<int, Unit> u in Global.UnitManager.Units)
			{
				if (u.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && u.Value.Active && u.Value.IsInit && u.Value.Kkm.IsKKT && u.Value.Kkm.FfdVersion >= 4 && u.Value.Kkm.SaleMarking)
				{
					Unit.DataCommand Command = new Unit.DataCommand();
					Unit.DataCommand.GoodCodeData goodCodeData = new Unit.DataCommand.GoodCodeData();
					goodCodeData.Name = "Проверка кода маркировки";
					goodCodeData.BarCode = DataCommand.BarCode;
					goodCodeData.TryBarCode = RezultGoodCodeData.DataProductCode.TryBarCode;
					goodCodeData.AcceptOnBad = false;
					goodCodeData.WaitForResult = true;
					Unit.RezultMarkingCodeValidation Rezult = new Unit.RezultMarkingCodeValidation();
					Command.GoodCodeDatas.Add(goodCodeData);
					Command.TypeCheck = 0;
					if (await u.Value.Semaphore.WaitAsync(0))
					{
						u.Value.Error = "";
						await u.Value.ProcessValidationMarkingCode(Command, Rezult);
						u.Value.Semaphore.Release();
						RezultGoodCodeData.ValidationKKT = Rezult.MarkingCodeValidation[0].ValidationKKT;
						RezultGoodCodeData.ValidationPR = Rezult.MarkingCodeValidation[0].ValidationPR;
					}
					if (u.Value.Error != "")
					{
						RezultGoodCodeData.ValidationKKT.ValidationResult = 0u;
						RezultGoodCodeData.ValidationKKT.DecryptionResult = "[М-] Ошибка проверки на ККТ: " + u.Value.Error;
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				RezultGoodCodeData.ValidationKKT.ValidationResult = 0u;
				RezultGoodCodeData.ValidationKKT.DecryptionResult = "[М-] Не найдена ККТ на которой можно проверить код маркировки";
			}
		}
		catch (Exception ex)
		{
			RezultGoodCodeData.DataProductCode = new MarkingCode.DataProductCode();
			RezultGoodCodeData.DataProductCode.Errors = ex.Message;
		}
		return RezultGoodCodeData;
	}

	public static Unit.RezultCommandGetTypeDevice GetSettingsServer(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		if (DataCommand.IdDevice == "OpenWindow" && AddIn.TypeAddIn == AddIn.enTypeAddIn.None)
		{
			Global.MainForm.MyPost(delegate
			{
				Global.OpenWindow();
			});
		}
		Unit.RezultCommandGetTypeDevice rezultCommandGetTypeDevice = new Unit.RezultCommandGetTypeDevice();
		rezultCommandGetTypeDevice.SettingsServer = new Unit.RezultCommandGetTypeDevice.iSettingsServer();
		rezultCommandGetTypeDevice.SettingsServer = new Unit.RezultCommandGetTypeDevice.iSettingsServer();
		rezultCommandGetTypeDevice.SettingsServer.ipPort = Global.Settings.ipPort;
		rezultCommandGetTypeDevice.SettingsServer.LoginAdmin = Global.Settings.LoginAdmin;
		rezultCommandGetTypeDevice.SettingsServer.PassAdmin = Global.Settings.PassAdmin;
		rezultCommandGetTypeDevice.SettingsServer.LoginUser = Global.Settings.LoginUser;
		rezultCommandGetTypeDevice.SettingsServer.PassUser = Global.Settings.PassUser;
		rezultCommandGetTypeDevice.SettingsServer.ServerSertificate = Global.Settings.ServerSertificate;
		rezultCommandGetTypeDevice.SettingsServer.TypeRun = Global.Settings.TypeRun;
		rezultCommandGetTypeDevice.SettingsServer.RegisterAllCommand = Global.Settings.RegisterAllCommand;
		rezultCommandGetTypeDevice.SettingsServer.RemoveCommandInterval = Global.Settings.RemoveCommandInterval;
		rezultCommandGetTypeDevice.SettingsServer.SetNotActiveOnError = Global.Settings.SetNotActiveOnError;
		rezultCommandGetTypeDevice.SettingsServer.SetNotActiveOnPaperOver = Global.Settings.SetNotActiveOnPaperOver;
		rezultCommandGetTypeDevice.SettingsServer.KkmIniter = Global.Settings.KkmIniter;
		rezultCommandGetTypeDevice.SettingsServer.KkmIniterInterval = Global.Settings.KkmIniterInterval;
		return rezultCommandGetTypeDevice;
	}

	public static Unit.RezultCommandGetTypeDevice GetTypesDevice(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		Unit.RezultCommandGetTypeDevice rezultCommandGetTypeDevice = new Unit.RezultCommandGetTypeDevice();
		rezultCommandGetTypeDevice.Types = new Dictionary<int, string>();
		int[] numNameType = TypeDevice.NumNameType;
		foreach (int num in numNameType)
		{
			rezultCommandGetTypeDevice.Types.Add(num, TypeDevice.NameType[num]);
		}
		rezultCommandGetTypeDevice.TypesDevices = new List<Unit.RezultCommandGetTypeDevice.iTypesDevice>();
		foreach (KeyValuePair<string, TypeDevice> item in Global.UnitManager.ListTypeDevice)
		{
			Unit.RezultCommandGetTypeDevice.iTypesDevice iTypesDevice = new Unit.RezultCommandGetTypeDevice.iTypesDevice();
			iTypesDevice.Id = item.Value.Id;
			iTypesDevice.Type = (int)item.Value.Type;
			iTypesDevice.Protocol = item.Value.Protocol;
			iTypesDevice.SupportModels = item.Value.SupportModels;
			rezultCommandGetTypeDevice.TypesDevices.Add(iTypesDevice);
		}
		return rezultCommandGetTypeDevice;
	}

	public static Unit.RezultCommandGetTypeDevice GetDeviceParamets(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		Unit.RezultCommandGetTypeDevice rezultCommandGetTypeDevice = new Unit.RezultCommandGetTypeDevice();
		foreach (KeyValuePair<string, TypeDevice> item in Global.UnitManager.ListTypeDevice)
		{
			if (!(item.Value.Id != DataCommand.IdTypeDevice))
			{
				rezultCommandGetTypeDevice.TypeDevice = item.Value;
				Global.DeviceSettings deviceSettings = new Global.DeviceSettings();
				deviceSettings.TypeDevice = new TypeDevice();
				deviceSettings.TypeDevice.UnitDevice = item.Value.UnitDevice;
				deviceSettings.TypeDevice.Type = item.Value.Type;
				Unit deviceClass = TypeDevice.GetDeviceClass(deviceSettings, 100);
				try
				{
					deviceClass.LoadParamets();
				}
				catch (Exception)
				{
				}
				rezultCommandGetTypeDevice.Paramets = deviceClass.UnitSettings.Paramerts;
			}
		}
		return rezultCommandGetTypeDevice;
	}

	public static Unit.RezultCommandGetTypeDevice GetDeviceSettings(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		Unit.RezultCommandGetTypeDevice rezultCommandGetTypeDevice = new Unit.RezultCommandGetTypeDevice();
		foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
		{
			if (device.Value.NumDevice == DataCommand.NumDevice)
			{
				Unit.RezultCommandGetTypeDevice.iUnitParamets iUnitParamets = new Unit.RezultCommandGetTypeDevice.iUnitParamets();
				iUnitParamets.NumDevice = device.Value.NumDevice;
				iUnitParamets.Id = device.Value.IdTypeDevice;
				iUnitParamets.Paramets = device.Value.Paramets;
				rezultCommandGetTypeDevice.UnitParamets = iUnitParamets;
			}
		}
		return rezultCommandGetTypeDevice;
	}

	public static Unit.RezultCommand SetSettingsServer(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		bool RestartHTTTP = DataCommand.SettingsServer.ServerSertificate == Global.Settings.ServerSertificate;
		Unit.RezultCommand result = new Unit.RezultCommand();
		Global.Settings.ipPort = DataCommand.SettingsServer.ipPort;
		Global.Settings.LoginAdmin = DataCommand.SettingsServer.LoginAdmin;
		Global.Settings.PassAdmin = DataCommand.SettingsServer.PassAdmin;
		Global.Settings.LoginUser = DataCommand.SettingsServer.LoginUser;
		Global.Settings.PassUser = DataCommand.SettingsServer.PassUser;
		Global.Settings.ServerSertificate = DataCommand.SettingsServer.ServerSertificate;
		Global.Settings.TypeRun = DataCommand.SettingsServer.TypeRun;
		Global.Settings.RegisterAllCommand = DataCommand.SettingsServer.RegisterAllCommand;
		Global.Settings.RemoveCommandInterval = DataCommand.SettingsServer.RemoveCommandInterval;
		Global.Settings.SetNotActiveOnError = DataCommand.SettingsServer.SetNotActiveOnError;
		Global.Settings.SetNotActiveOnPaperOver = DataCommand.SettingsServer.SetNotActiveOnPaperOver;
		Global.Settings.KkmIniter = DataCommand.SettingsServer.KkmIniter;
		Global.Settings.KkmIniterInterval = DataCommand.SettingsServer.KkmIniterInterval;
		Global.SaveSettingsAsync().Wait();
		if (Global.Settings.ipPortOld == Global.Settings.ipPort && RestartHTTTP)
		{
			RestartHTTTP = false;
		}
		new Task(delegate
		{
			Task.Delay(1000);
			Global.RestartServer(RestartHTTTP);
			if (Global.Settings.RegisterAllCommand)
			{
				Global.Logers.AddError("RestartServer", "", "MenegerServer: Рестартуем при установке новых настроек").Wait();
			}
		}).Start();
		return result;
	}

	public static Unit.RezultCommand SetDeviceSettings(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		Unit.RezultCommand rezultCommand = new Unit.RezultCommand();
		rezultCommand.Status = Unit.ExecuteStatus.Ok;
		if (!Global.UnitManager.Units.ContainsKey(DataCommand.DeviceSettings.NumDevice) && DataCommand.DeviceSettings.Paramets != null)
		{
			Global.UnitManager.AddUnit(DataCommand.DeviceSettings.NumDevice, null, DataCommand.DeviceSettings.IdTypeDevice);
			Thread.Sleep(5);
		}
		else if (Global.UnitManager.Units.ContainsKey(DataCommand.DeviceSettings.NumDevice) && DataCommand.DeviceSettings.Paramets == null)
		{
			Global.UnitManager.AddUnit(DataCommand.DeviceSettings.NumDevice, null, null);
			Thread.Sleep(5);
			return rezultCommand;
		}
		Unit unit = Global.UnitManager.Units[DataCommand.DeviceSettings.NumDevice];
		Dictionary<string, string> dictionary = new Dictionary<string, string>(unit.SettDr.Paramets);
		foreach (Unit.iUnitSettings.Paramert paramert in unit.UnitSettings.Paramerts)
		{
			if (paramert.TypeValue == "Boolean" && !DataCommand.DeviceSettings.Paramets.ContainsKey(paramert.Name))
			{
				dictionary[paramert.Name] = ExtensionMethods.AsString(Val: false);
			}
			else if (DataCommand.DeviceSettings.Paramets.ContainsKey(paramert.Name))
			{
				dictionary[paramert.Name] = DataCommand.DeviceSettings.Paramets[paramert.Name];
			}
		}
		unit.SaveParametrs(dictionary);
		Global.UnitManager.AddUnit(DataCommand.DeviceSettings.NumDevice, unit.SettDr.IdDevice, unit.SettDr.IdTypeDevice, dictionary, InspectionPort: false, SetActive: true);
		return rezultCommand;
	}

	public static RezultDataLicense GetDataLicense(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		ComDevice.InDate result = ComDevice.ReadComDevice(null, AllowDateAction: false, AllowCount: true, OnlySerial: false, PluzBesplatnoe: true).Result;
		return new RezultDataLicense
		{
			SerialNuber = ComDevice.ck,
			LicenseCount = result.Int,
			LicenseExpirationDate = ((result.DateTime != default(DateTime)) ? result.DateTime.AddDays(1.0) : result.DateTime.AddDays(1.0)),
			Status = Unit.ExecuteStatus.Ok
		};
	}

	public static RezultDataLicense GetLicense(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc, int CountGetLic = 1)
	{
		ComDevice.InDate result = ComDevice.ReadComDevice(null, AllowDateAction: false, AllowCount: true, OnlySerial: false, PluzBesplatnoe: true).Result;
		RezultDataLicense rezultDataLicense = new RezultDataLicense();
		rezultDataLicense.SerialNuber = ComDevice.ck;
		rezultDataLicense.LicenseCount = result.Int;
		rezultDataLicense.LicenseExpirationDate = ((result.DateTime != default(DateTime)) ? result.DateTime.AddDays(1.0) : result.DateTime.AddDays(1.0));
		rezultDataLicense.Error = Global.ErrorLicense;
		if (rezultDataLicense.Error != "")
		{
			rezultDataLicense.Status = Unit.ExecuteStatus.Error;
		}
		else
		{
			rezultDataLicense.Status = Unit.ExecuteStatus.Ok;
		}
		return rezultDataLicense;
	}

	public static RezultDataLicense FreeLicense(UnitManager.ExecuteData ExecuteData, Unit.DataCommand DataCommand, string TypeSunc)
	{
		return GetLicense(ExecuteData, DataCommand, TypeSunc, 0);
	}
}
