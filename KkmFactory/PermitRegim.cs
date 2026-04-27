using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KkmFactory;

public static class PermitRegim
{
	[DataContract]
	public class Setting
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

		[DataMember]
		[DefaultValue(false)]
		public bool Active;

		public string CurToken;

		public DateTime ExpiresToken;
	}

	public class Log
	{
		public string URL;

		public DateTime Date;

		public TimeSpan Time;

		public string Error;

		public string Command;

		public HttpStatusCode StatusCode;

		public string Request;

		public string Response;

		public static List<Log> Logs = new List<Log>();

		private const int MaxSize = 50;

		public static void SaveLogs(Http.HttpRezult HttpRezult, string StCommand, string StError)
		{
			try
			{
				DateTime CurDate = DateTime.Now;
				CurDate = DateTime.ParseExact(CurDate.ToString("yyyy.MM.dd HH:mm:ss:fff"), "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
				try
				{
					while (Logs.FindIndex((Log I) => I.Date == CurDate) != -1)
					{
						CurDate = CurDate.AddMicroseconds(1.0);
					}
				}
				catch
				{
				}
				Logs.Add(new Log
				{
					Date = CurDate,
					Time = HttpRezult.Time,
					Error = ((StError == null || StError == "") ? HttpRezult.Error : StError),
					Command = StCommand,
					StatusCode = HttpRezult.StatusCode,
					URL = HttpRezult.URL,
					Request = HttpRezult.Request,
					Response = HttpRezult.Response
				});
				try
				{
					Logs.Min((Log i) => i.Date);
				}
				catch
				{
					try
					{
						Logs.Min((Log i) => i.Date);
					}
					catch
					{
						try
						{
							Logs.Min((Log i) => i.Date);
							goto end_IL_0149;
						}
						catch
						{
							Logs.Min((Log i) => i.Date);
							goto end_IL_0149;
						}
						end_IL_0149:;
					}
				}
			}
			catch
			{
			}
			try
			{
				while (Logs.Count > 50)
				{
					DateTime MinDate = Logs.Min((Log i) => i.Date);
					Logs.Remove(Logs.Find((Log i) => i.Date == MinDate));
				}
			}
			catch
			{
			}
		}

		public static void Sort()
		{
			Logs.Sort(delegate(Log I, Log X)
			{
				_ = X.Date;
				_ = I.Date;
				return X.Date.CompareTo(I.Date);
			});
		}
	}

	[DataContract]
	public class TRezult
	{
		[DataContract]
		public struct THosts
		{
			[DataMember(Name = "host")]
			public string host = null;

			[JsonConstructor]
			public THosts()
			{
			}
		}

		[DataContract]
		public struct TCode
		{
			[DataMember(Name = "cis")]
			public string cis = null;

			[DataMember(Name = "found")]
			public bool found = false;

			[DataMember(Name = "valid")]
			public bool valid = false;

			[DataMember(Name = "printView")]
			public string printView = null;

			[DataMember(Name = "gtin")]
			public string gtin = null;

			[DataMember(Name = "groupIds")]
			public List<int> groupIds = new List<int>();

			[DataMember(Name = "verified")]
			public bool verified = false;

			[DataMember(Name = "realizable")]
			public bool realizable = false;

			[DataMember(Name = "utilised")]
			public bool utilised = false;

			[DataMember(Name = "expireDate")]
			public DateTime expireDate = default(DateTime);

			[DataMember(Name = "variableExpirations")]
			public string variableExpirations = null;

			[DataMember(Name = "productionDate")]
			public DateTime productionDate = default(DateTime);

			[DataMember(Name = "productWeight")]
			public decimal productWeight = default(decimal);

			[DataMember(Name = "prVetDocument")]
			public string prVetDocument = null;

			[DataMember(Name = "isOwner")]
			public bool isOwner = false;

			[DataMember(Name = "isBlocked")]
			public bool isBlocked = false;

			[DataMember(Name = "ogvs")]
			public List<string> ogvs = new List<string>();

			[DataMember(Name = "message")]
			public string message = null;

			[DataMember(Name = "errorCode")]
			public int errorCode = 0;

			[DataMember(Name = "isTracking")]
			public bool isTracking = false;

			[DataMember(Name = "sold")]
			public bool sold = false;

			[DataMember(Name = "eliminationState")]
			public int eliminationState = 0;

			[DataMember(Name = "mrp")]
			public decimal mrp = default(decimal);

			[DataMember(Name = "smp")]
			public decimal smp = default(decimal);

			[DataMember(Name = "grayZone")]
			public bool grayZone = false;

			[DataMember(Name = "innerUnitCount")]
			public int innerUnitCount = 0;

			[DataMember(Name = "soldUnitCount")]
			public int soldUnitCount = 0;

			[DataMember(Name = "packageType")]
			public string packageType = null;

			[DataMember(Name = "parent")]
			public string parent = null;

			[DataMember(Name = "producerInn")]
			public string producerInn = null;

			[JsonConstructor]
			public TCode()
			{
			}
		}

		[DataMember(Name = "error_message")]
		public string error_message = "";

		[DataMember(Name = "code")]
		public int code;

		[DataMember(Name = "description")]
		public string description;

		[DataMember(Name = "avgTimeMs")]
		public string avgTimeMs;

		[DataMember(Name = "hosts")]
		public List<THosts> hosts = new List<THosts>();

		[DataMember(Name = "codes")]
		public List<TCode> codes = new List<TCode>();

		[DataMember(Name = "reqId")]
		public string reqId;

		[DataMember(Name = "reqTimestamp")]
		public long reqTimestamp;

		[DataMember(Name = "access_token")]
		public string access_token;

		[DataMember(Name = "id_token")]
		public string id_token;

		[DataMember(Name = "expires_in")]
		public long expires_in;

		[DataMember(Name = "token_type")]
		public string token_type;
	}

	public class TCodesCheck
	{
		public string fiscalDriveNumber;

		public List<string> codes = new List<string>();
	}

	private const string HostGetToken = "https://cdn.crpt.ru";

	private const string HostGetCdnPlatformDebug = "https://markirovka.sandbox.crptech.ru";

	private const string HostGetCdnPlatform = "https://cdn.crpt.ru";

	private static TimeSpan ExpireCdnPlatformS = new TimeSpan(6, 0, 0);

	private static TimeSpan ExpireCdnPlatform = new TimeSpan(0, 15, 0);

	public static Dictionary<int, string> NameGroupEng = new Dictionary<int, string>
	{
		[1] = "lp",
		[2] = "Shoes",
		[3] = "Tobacco",
		[4] = "Perfumery",
		[5] = "Tires",
		[6] = "Electronics",
		[7] = "Pharma",
		[8] = "Мilk",
		[9] = "Bicycle",
		[10] = "Wheelchairs",
		[12] = "Otp",
		[13] = "Water",
		[14] = "Furs",
		[15] = "Beer",
		[16] = "Ncp",
		[17] = "Bio",
		[19] = "Antiseptic",
		[20] = "Petfood",
		[21] = "Seafood",
		[22] = "Nabeer",
		[23] = "Softdrinks",
		[26] = "Vetpharma",
		[32] = "Conserve",
		[33] = "Vegetableoil"
	};

	public static Dictionary<int, string> NameGroupRus = new Dictionary<int, string>
	{
		[1] = "Предметы одежды, бельё постельное, столовое, туалетное и кухонное",
		[2] = "Обувные товары\r\n",
		[3] = "Табачная продукция",
		[4] = "Духи и туалетная вода",
		[5] = "Шины и покрышки",
		[6] = "Фотокамеры (кроме кинокамер), фотовспышки и лампывспышки",
		[7] = "Лекарства",
		[8] = "Молочная продукция",
		[9] = "Велосипеды и велосипедные рамы",
		[10] = "Медицинские изделия",
		[12] = "Альтернативная табачная продукция",
		[13] = "Упакованная вода",
		[14] = "Товары из натурального меха",
		[15] = "Пиво, напитки, изготавливаемые на основе пива, слабоалкогольные напитки",
		[16] = "Никотиносодержащая продукция",
		[17] = "Биологически активные добавки к пище",
		[19] = "Антисептики и дезинфицирующие средства",
		[20] = "Корма для животных",
		[21] = "Морепродукты",
		[22] = "Безалкогольное пиво",
		[23] = "Соковая продукция и безалкогольные напитки",
		[26] = "Ветеринарные препараты",
		[32] = "Консервированная продукция",
		[33] = "Растительные масла"
	};

	public static Dictionary<string, string> NameOgvs = new Dictionary<string, string>
	{
		["RAR"] = "Росалкогольрегулирование",
		["FTS"] = "ФТС России",
		["FNS"] = "ФНС России",
		["RSHN"] = "Россельхознадзор",
		["RPN"] = "Роспотребнадзор",
		["MVD"] = "МВД России",
		["RZN"] = "Росздравнадзор",
		["VETRF"] = "ВетИС"
	};

	public static Dictionary<string, Setting> CurSettingS = new Dictionary<string, Setting>();

	public static void LoadFromList(List<Global.PermitRegimSetting> ListSettingS)
	{
		foreach (Global.PermitRegimSetting ListSetting in ListSettingS)
		{
			if (ListSetting.Inn != null)
			{
				Setting setting = null;
				setting = ((!CurSettingS.ContainsKey(ListSetting.Inn)) ? new Setting() : CurSettingS[ListSetting.Inn]);
				setting.Inn = ListSetting.Inn;
				setting.Token = ListSetting.Token;
				setting.CertificateId = ListSetting.CertificateId;
				setting.CertificatePass = ListSetting.CertificatePass;
				if (setting.CertificatePass != null && setting.CertificatePass.Length > 0 && setting.CertificatePass[0] == 'թ')
				{
					setting.CertificatePass = Global.DeShifrovka(setting.CertificatePass.Substring(1), Global.Settings.PBKDFprotocol);
				}
				setting.GroupIds = new List<int>(ListSetting.GroupIds.ToArray());
				setting.Active = false;
				if (!CurSettingS.ContainsKey(ListSetting.Inn))
				{
					CurSettingS.Add(setting.Inn, setting);
				}
			}
		}
		ListCdnPlatformSort();
		foreach (Global.CdnPlatform item in Global.Settings.ListCdnPlatform)
		{
			item.CoutUssage = 0;
			item.LastUssage = default(DateTime);
		}
	}

	public static void ListCdnPlatformSort()
	{
		Global.Settings.ListCdnPlatform.Sort((Global.CdnPlatform I, Global.CdnPlatform X) => I.PingTime.CompareTo(X.PingTime));
	}

	public static List<Global.PermitRegimSetting> SaveInList()
	{
		List<Global.PermitRegimSetting> list = new List<Global.PermitRegimSetting>();
		foreach (KeyValuePair<string, Setting> curSetting in CurSettingS)
		{
			Global.PermitRegimSetting permitRegimSetting = new Global.PermitRegimSetting();
			permitRegimSetting.Inn = curSetting.Value.Inn;
			permitRegimSetting.Token = curSetting.Value.Token;
			permitRegimSetting.CertificateId = curSetting.Value.CertificateId;
			permitRegimSetting.CertificatePass = curSetting.Value.CertificatePass;
			if (curSetting.Value.CertificatePass != null)
			{
				permitRegimSetting.CertificatePass = "թ" + Global.Shifrovka(curSetting.Value.CertificatePass, Global.Settings.PBKDFprotocol);
			}
			CalcGroupIds(curSetting.Value.GroupIds);
			permitRegimSetting.GroupIds = new List<int>(curSetting.Value.GroupIds.ToArray());
			list.Add(permitRegimSetting);
		}
		return list;
	}

	public static void LoadFromDevice()
	{
		lock (CurSettingS)
		{
			foreach (KeyValuePair<int, Unit> unit in Global.UnitManager.Units)
			{
				string text = null;
				if (unit.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && unit.Value.Kkm.IsKKT && unit.Value.LicenseFlags != ComDevice.PaymentOption.Evotor && !string.IsNullOrEmpty(unit.Value.Kkm.INN))
				{
					text = unit.Value.Kkm.INN.Trim();
				}
				if (unit.Value.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал && unit.Value.LicenseFlags != ComDevice.PaymentOption.Evotor && !string.IsNullOrEmpty(unit.Value.UnitParamets["INN"]))
				{
					text = unit.Value.UnitParamets["INN"].Trim();
				}
				if (text != null)
				{
					if (!CurSettingS.ContainsKey(text))
					{
						Setting setting = new Setting();
						setting.Inn = text;
						setting.Active = true;
						CurSettingS.Add(setting.Inn, setting);
					}
					else
					{
						CurSettingS[text].Active = true;
					}
				}
			}
		}
	}

	public static void CalcGroupIds(List<int> GroupIds)
	{
		bool flag = false;
		foreach (int GroupId in GroupIds)
		{
			if (GroupId == 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			flag = true;
			foreach (KeyValuePair<int, string> item in NameGroupEng)
			{
				if (!GroupIds.Exists((int I) => I == item.Key))
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			GroupIds.Clear();
			GroupIds.Add(0);
		}
	}

	public static bool GetCurToken(Setting CurSetting)
	{
		if (!string.IsNullOrEmpty(CurSetting.CurToken) && CurSetting.ExpiresToken > DateTime.Now + new TimeSpan(0, 0, 10))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(CurSetting.Token))
		{
			CurSetting.CurToken = CurSetting.Token;
			CurSetting.ExpiresToken = DateTime.Now + new TimeSpan(6, 0, 0);
			return true;
		}
		if (!string.IsNullOrEmpty(CurSetting.CertificateId))
		{
			string stError = "";
			string signedCms = UtilSertificate.GetSignedCms(UtilSertificate.GetCertFromStoreS(CurSetting.CertificateId), "Запрос токена");
			string body = "{\"data\":\"{Cms}\"}".Replace("{Cms}", signedCms);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/json");
			Http.HttpRezult httpRezult = Http.HttpReqest(HttpMethod.Post, 200000, "https://cdn.crpt.ru", "/api/v3/true-api/auth/permissive-access", null, dictionary, body, typeof(TRezult));
			if (httpRezult.StatusCode == HttpStatusCode.OK)
			{
				TRezult tRezult = httpRezult.Rezult as TRezult;
				if (tRezult.code == 0)
				{
					CurSetting.CurToken = ((TRezult)httpRezult.Rezult).access_token;
					CurSetting.ExpiresToken = DateTime.Now + new TimeSpan(0, 0, (int)((TRezult)httpRezult.Rezult).expires_in);
					Log.SaveLogs(httpRezult, "Запрос токена", stError);
					return true;
				}
				stError = "Error code: " + tRezult.code + ", Description: " + tRezult.description;
			}
			else
			{
				stError = "Получение токена: " + httpRezult.Error;
				if (httpRezult.Rezult != null && (httpRezult.Rezult as TRezult).error_message != null && (httpRezult.Rezult as TRezult).error_message != "")
				{
					stError = stError + ", " + (httpRezult.Rezult as TRezult).error_message;
				}
			}
			Log.SaveLogs(httpRezult, "Запрос токена", stError);
		}
		return false;
	}

	public static string GetAnyToken()
	{
		foreach (KeyValuePair<string, Setting> curSetting in CurSettingS)
		{
			if (GetCurToken(curSetting.Value))
			{
				return curSetting.Value.CurToken;
			}
		}
		return null;
	}

	public static string GetCurToken(string INN)
	{
		foreach (KeyValuePair<string, Setting> curSetting in CurSettingS)
		{
			if (curSetting.Value.Inn == INN && GetCurToken(curSetting.Value))
			{
				return curSetting.Value.CurToken;
			}
		}
		return null;
	}

	public static Setting GetSetting(string INN)
	{
		return CurSettingS.First((KeyValuePair<string, Setting> i) => i.Key == INN).Value;
	}

	public static async Task<string> GetListServers(bool CheckTime = true)
	{
		if (!CheckTime || !Global.Settings.DateTimeCdnPlatformGet.HasValue || Global.Settings.DateTimeCdnPlatformGet.Value + ExpireCdnPlatformS < DateTime.Now)
		{
			string text = null;
			foreach (KeyValuePair<string, Setting> curSetting in CurSettingS)
			{
				if (curSetting.Value.GroupIds.Count == 0 || !GetCurToken(curSetting.Value))
				{
					continue;
				}
				string urlServer = ((!Global.Settings.PermitRegimDebug) ? "https://cdn.crpt.ru" : "https://markirovka.sandbox.crptech.ru");
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("Content-Type", "application/json");
				dictionary.Add("X-API-KEY", curSetting.Value.CurToken);
				Http.HttpRezult httpRezult = await Http.HttpReqestAsync(HttpMethod.Get, 5000, urlServer, "/api/v4/true-api/cdn/info", null, dictionary, null, typeof(TRezult));
				if (httpRezult.StatusCode == HttpStatusCode.OK)
				{
					TRezult tRezult = httpRezult.Rezult as TRezult;
					if (tRezult.code != 0)
					{
						text = "Error code: " + tRezult.code + ", Description: " + tRezult.description;
					}
					else
					{
						if (tRezult.hosts.Count > 0)
						{
							lock (Global.Settings.ListCdnPlatform)
							{
								Global.Settings.ListCdnPlatform.Clear();
								foreach (TRezult.THosts host in tRezult.hosts)
								{
									Global.CdnPlatform cdnPlatform = new Global.CdnPlatform();
									cdnPlatform.Host = host.host;
									Global.Settings.ListCdnPlatform.Add(cdnPlatform);
								}
							}
							Global.Settings.DateTimeCdnPlatformGet = DateTime.Now;
							text = null;
							Log.SaveLogs(httpRezult, "Получение списка серверов", text);
							break;
						}
						text = "Отсутствует список серверов CDN в ответе с сервера";
					}
				}
				else
				{
					text = httpRezult.Error;
					if (httpRezult.Rezult != null && (httpRezult.Rezult as TRezult).error_message != null && (httpRezult.Rezult as TRezult).error_message != "")
					{
						text = (httpRezult.Rezult as TRezult).error_message + ", " + text;
					}
				}
				Log.SaveLogs(httpRezult, "Получение списка серверов", text);
			}
			if (text != null && Global.Settings.ListCdnPlatform.Count == 0)
			{
				return "Ошибка обновления серверов маркировки: " + text;
			}
			PingServers();
		}
		return "";
	}

	public static void PingServers()
	{
		List<Task> list = new List<Task>();
		lock (list)
		{
			foreach (Global.CdnPlatform ItemCdnPlatform in Global.Settings.ListCdnPlatform)
			{
				Task task = new Task(delegate
				{
					_ = ItemCdnPlatform;
					string text = null;
					string anyToken = GetAnyToken();
					if (anyToken != null)
					{
						Dictionary<string, string> heads = new Dictionary<string, string>
						{
							{ "Content-Type", "application/json" },
							{ "X-API-KEY", anyToken }
						};
						Http.HttpRezult httpRezult = null;
						try
						{
							httpRezult = Http.HttpReqest(HttpMethod.Get, 5000, ItemCdnPlatform.Host, "/api/v4/true-api/cdn/health/check", null, heads, null, typeof(TRezult));
						}
						catch (Exception ex2)
						{
							DateTime CurDate = DateTime.Now;
							CurDate = DateTime.ParseExact(CurDate.ToString("yyyy.MM.dd HH:mm:ss:fff"), "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
							while (Log.Logs.FindIndex((Log I) => I.Date == CurDate) != -1)
							{
								CurDate = CurDate.AddMicroseconds(1.0);
							}
							Log.Logs.Add(new Log
							{
								Date = CurDate,
								Time = CurDate.TimeOfDay,
								Error = "Ошибка PingServers: " + ex2.Message,
								Command = "PingServers",
								StatusCode = (HttpStatusCode)0,
								URL = ItemCdnPlatform.Host + "/api/v4/true-api/cdn/health/check",
								Request = "",
								Response = ""
							});
							return;
						}
						if (httpRezult.StatusCode == HttpStatusCode.OK)
						{
							TRezult tRezult = httpRezult.Rezult as TRezult;
							if (tRezult.code != 0)
							{
								text = "Error code: " + tRezult.code + ", Description: " + tRezult.description;
							}
							else
							{
								ItemCdnPlatform.PingTime = httpRezult.Time;
								ItemCdnPlatform.CoutUssage++;
								ItemCdnPlatform.LastUssage = DateTime.Now;
							}
						}
						else
						{
							text = httpRezult.Error;
							if (httpRezult.Rezult != null && (httpRezult.Rezult as TRezult).error_message != null && (httpRezult.Rezult as TRezult).error_message != "")
							{
								text = (httpRezult.Rezult as TRezult).error_message + ", " + text;
							}
						}
						Log.SaveLogs(httpRezult, "Получение состояния сервера: " + ItemCdnPlatform.Host, text);
					}
					else
					{
						Log.SaveLogs(new Http.HttpRezult(), "Получение состояния сервера: " + ItemCdnPlatform.Host, "Не настроен доступ к серверам разрешительного режима");
					}
				});
				list.Add(task);
				task.Start();
			}
			try
			{
				Task.WaitAll(list.ToArray());
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task ValidationMarkingCode(Unit Unit, Unit.DataCommand DataCommand, Unit.RezultMarkingCodeValidation RezultCommand, bool InCheck = false)
	{
		string OldError = await GetListServers();
		if (OldError != "")
		{
			throw new Exception("Запретительный режим:" + OldError);
		}
		TCodesCheck CodesCheck = new TCodesCheck
		{
			fiscalDriveNumber = Unit.Kkm.Fn_Number
		};
		Setting InnSetting = GetSetting(Unit.Kkm.INN);
		if (InnSetting.GroupIds.Count == 0)
		{
			foreach (Unit.RezultMarkingCodeValidation.tMarkingCodeValidation item in RezultCommand.MarkingCodeValidation)
			{
				item.ValidationPR.ValidationResult = false;
				item.ValidationPR.ValidationDisabled = true;
				item.ValidationPR.DecryptionResult = "Проверка отключена в настройках kkmserver";
			}
			return;
		}
		foreach (Unit.DataCommand.GoodCodeData goodCodeData in DataCommand.GoodCodeDatas)
		{
			if (!string.IsNullOrEmpty(goodCodeData.TryBarCode) && string.IsNullOrEmpty(goodCodeData.IndustryProps) && !CodesCheck.codes.Contains(goodCodeData.TryBarCode))
			{
				CodesCheck.codes.Add(goodCodeData.TryBarCode);
			}
		}
		if (CodesCheck.codes.Count == 0)
		{
			return;
		}
		int RequestCdnServer = 0;
		int num = 0;
		int num2 = 0;
		TRezult tRezult;
		while (true)
		{
			if (num + 1 <= Global.Settings.ListCdnPlatform.Count)
			{
				Global.CdnPlatform cdnPlatform = Global.Settings.ListCdnPlatform[num];
				if (cdnPlatform.Blocked >= DateTime.Now)
				{
					num++;
					num2 = 0;
					continue;
				}
				Http.HttpRezult httpRezult = CheckMarkingCode(Unit, CodesCheck, cdnPlatform.Host, DataCommand.IdCommand);
				tRezult = ((httpRezult.Rezult == null) ? new TRezult() : (httpRezult.Rezult as TRezult));
				if (httpRezult.StatusCode == HttpStatusCode.Unauthorized)
				{
					throw new Exception("Запретительный режим: Ошибка авторизации на сервере ГИС МТ: " + tRezult.error_message);
				}
				if (httpRezult.StatusCode == HttpStatusCode.NonAuthoritativeInformation || httpRezult.StatusCode == (HttpStatusCode)418)
				{
					foreach (Unit.DataCommand.GoodCodeData GoodCodeData in DataCommand.GoodCodeDatas)
					{
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation? tMarkingCodeValidation = RezultCommand.MarkingCodeValidation.Find((Unit.RezultMarkingCodeValidation.tMarkingCodeValidation i) => i.BarCode == GoodCodeData.TryBarCode);
						tMarkingCodeValidation.ValidationPR.ValidationResult = false;
						tMarkingCodeValidation.ValidationPR.ValidationDisabled = true;
						tMarkingCodeValidation.ValidationPR.DecryptionResult = "В ГИС МТ введена аварийная ситуация - режим 'Запретительный режим' отключен.";
					}
					return;
				}
				if (httpRezult.StatusCode == HttpStatusCode.TooManyRequests)
				{
					if (num2 == 0)
					{
						OldError = "Ошибка: 429(TooManyRequests) - Сервер ГИС МТ перегружен";
						num2++;
						continue;
					}
					OldError = "Ошибка: 429(TooManyRequests) - Сервер ГИС МТ перегружен и заблокирован на 15 минут";
					cdnPlatform.Blocked = DateTime.Now + ExpireCdnPlatform;
					num++;
					num2 = 0;
				}
				else if (httpRezult.StatusCode >= HttpStatusCode.InternalServerError && httpRezult.StatusCode < (HttpStatusCode)600 && tRezult.error_message == "5000")
				{
					int statusCode;
					if (num2 != 0)
					{
						string[] obj = new string[5] { "Ошибка: ", null, null, null, null };
						statusCode = (int)httpRezult.StatusCode;
						obj[1] = statusCode.ToString();
						obj[2] = "(";
						obj[3] = httpRezult.StatusCode.ToString();
						obj[4] = ") - Cервис трансграничной проверки кодов недоступен";
						string.Concat(obj);
						{
							foreach (Unit.DataCommand.GoodCodeData GoodCodeData2 in DataCommand.GoodCodeDatas)
							{
								Unit.RezultMarkingCodeValidation.tMarkingCodeValidation? tMarkingCodeValidation2 = RezultCommand.MarkingCodeValidation.Find((Unit.RezultMarkingCodeValidation.tMarkingCodeValidation i) => i.BarCode == GoodCodeData2.TryBarCode);
								tMarkingCodeValidation2.ValidationPR.ValidationResult = false;
								tMarkingCodeValidation2.ValidationPR.ValidationDisabled = true;
								tMarkingCodeValidation2.ValidationPR.DecryptionResult = "В ГИС МТ введена аварийная ситуация - режим 'Запретительный режим' отключен.";
							}
							return;
						}
					}
					string[] obj2 = new string[5] { "Ошибка: ", null, null, null, null };
					statusCode = (int)httpRezult.StatusCode;
					obj2[1] = statusCode.ToString();
					obj2[2] = "(";
					obj2[3] = httpRezult.StatusCode.ToString();
					obj2[4] = ") - Cервис трансграничной проверки кодов недоступен";
					OldError = string.Concat(obj2);
					num2++;
				}
				else if (httpRezult.StatusCode >= HttpStatusCode.InternalServerError && httpRezult.StatusCode < (HttpStatusCode)600 && tRezult.error_message != "5000")
				{
					if (num2 == 0)
					{
						string[] obj3 = new string[5] { "Ошибка: ", null, null, null, null };
						int statusCode = (int)httpRezult.StatusCode;
						obj3[1] = statusCode.ToString();
						obj3[2] = "(";
						obj3[3] = httpRezult.StatusCode.ToString();
						obj3[4] = ")";
						OldError = string.Concat(obj3);
						num2++;
					}
					else
					{
						string[] obj4 = new string[5] { "Ошибка: ", null, null, null, null };
						int statusCode = (int)httpRezult.StatusCode;
						obj4[1] = statusCode.ToString();
						obj4[2] = "(";
						obj4[3] = httpRezult.StatusCode.ToString();
						obj4[4] = ") - Сервер ГИС МТ заблокирован на 15 минут";
						OldError = string.Concat(obj4);
						cdnPlatform.Blocked = DateTime.Now + ExpireCdnPlatform;
						num++;
						num2 = 0;
					}
				}
				else
				{
					if (httpRezult.StatusCode == HttpStatusCode.OK)
					{
						break;
					}
					if (num2 == 0)
					{
						string[] obj5 = new string[5] { "Ошибка: ", null, null, null, null };
						int statusCode = (int)httpRezult.StatusCode;
						obj5[1] = statusCode.ToString();
						obj5[2] = "(";
						obj5[3] = httpRezult.StatusCode.ToString();
						obj5[4] = ") - не известаня ошибка";
						OldError = string.Concat(obj5);
						num2++;
					}
					else
					{
						string[] obj6 = new string[5] { "Ошибка: ", null, null, null, null };
						int statusCode = (int)httpRezult.StatusCode;
						obj6[1] = statusCode.ToString();
						obj6[2] = "(";
						obj6[3] = httpRezult.StatusCode.ToString();
						obj6[4] = ") - не известаня ошибка - Сервер ГИС МТ заблокирован на 15 минут";
						OldError = string.Concat(obj6);
						cdnPlatform.Blocked = DateTime.Now + ExpireCdnPlatform;
						num++;
						num2 = 0;
					}
				}
				continue;
			}
			if (RequestCdnServer != 0)
			{
				string text = "Запретительный режим: Сервера ГИС МТ не отвечают. Проверьте наличее интернета. Последняя ошибка:" + OldError;
				foreach (Unit.RezultMarkingCodeValidation.tMarkingCodeValidation item2 in RezultCommand.MarkingCodeValidation)
				{
					item2.ValidationPR.ValidationResult = false;
					item2.ValidationPR.ValidationDisabled = false;
					item2.ValidationPR.DecryptionResult = text;
				}
				RezultCommand.Error = text;
				return;
			}
			await GetListServers(CheckTime: false);
			RequestCdnServer++;
			num = 0;
			num2 = 0;
		}
		if (tRezult.code != 0)
		{
			throw new Exception("Запретительный режим: Ошибка: " + tRezult.description);
		}
		foreach (TRezult.TCode code in tRezult.codes)
		{
			foreach (Unit.DataCommand.GoodCodeData GoodCodeData3 in DataCommand.GoodCodeDatas)
			{
				string tryBarCode = GoodCodeData3.TryBarCode;
				if (code.cis != tryBarCode)
				{
					continue;
				}
				Unit.RezultMarkingCodeValidation.tMarkingCodeValidation tMarkingCodeValidation3 = RezultCommand.MarkingCodeValidation.Find((Unit.RezultMarkingCodeValidation.tMarkingCodeValidation i) => i.TryBarCode == GoodCodeData3.TryBarCode);
				if (tMarkingCodeValidation3 == null)
				{
					continue;
				}
				tMarkingCodeValidation3.ValidationPR.ValidationResult = true;
				tMarkingCodeValidation3.ValidationPR.ValidationDisabled = false;
				tMarkingCodeValidation3.ValidationPR.DecryptionResult = "";
				tMarkingCodeValidation3.ValidationPR.Result = code;
				if (InnSetting.GroupIds.Contains(0))
				{
					foreach (int groupId in code.groupIds)
					{
						if (InnSetting.GroupIds.Contains(groupId))
						{
							break;
						}
					}
				}
				if (code.grayZone)
				{
					tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
					tMarkingCodeValidation3.ValidationPR.ValidationDisabled = true;
					tMarkingCodeValidation3.ValidationPR.DecryptionResult += ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "'Серая зона' - проверка отключена" : ", ");
				}
				else
				{
					if (!code.found)
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR = tMarkingCodeValidation3.ValidationPR;
						validationPR.DecryptionResult = validationPR.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Код маркировки не найден в ГИС МТ.";
						continue;
					}
					if (!code.utilised)
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR2 = tMarkingCodeValidation3.ValidationPR;
						validationPR2.DecryptionResult = validationPR2.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Код маркировки эмитирован, но нет информации о его нанесении на упаковку.";
						continue;
					}
					if (!code.verified)
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR3 = tMarkingCodeValidation3.ValidationPR;
						validationPR3.DecryptionResult = validationPR3.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Не пройдена криптографическая проверка кода маркировки.";
						continue;
					}
					if (code.isBlocked)
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR4 = tMarkingCodeValidation3.ValidationPR;
						validationPR4.DecryptionResult = validationPR4.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Код маркировки заблокирован по решению ОГВ: ";
						foreach (string ogv in code.ogvs)
						{
							if (NameOgvs.ContainsKey(ogv))
							{
								Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR5 = tMarkingCodeValidation3.ValidationPR;
								validationPR5.DecryptionResult = validationPR5.DecryptionResult + NameOgvs[ogv] + "; ";
							}
							else
							{
								Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR6 = tMarkingCodeValidation3.ValidationPR;
								validationPR6.DecryptionResult = validationPR6.DecryptionResult + ogv + "; ";
							}
						}
						continue;
					}
					if (!code.sold && !code.realizable)
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR7 = tMarkingCodeValidation3.ValidationPR;
						validationPR7.DecryptionResult = validationPR7.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Код маркировки не был введен в оборот (Товар считается не отгруженным производителем).";
						continue;
					}
					if (code.sold && !code.grayZone && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 11 || DataCommand.TypeCheck == 12))
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR8 = tMarkingCodeValidation3.ValidationPR;
						validationPR8.DecryptionResult = validationPR8.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Код маркировки выведен из оборота (Товар продан).";
						continue;
					}
					if (code.realizable && !code.grayZone && (DataCommand.TypeCheck == 1 || DataCommand.TypeCheck == 2 || DataCommand.TypeCheck == 10 || DataCommand.TypeCheck == 13))
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR9 = tMarkingCodeValidation3.ValidationPR;
						validationPR9.DecryptionResult = validationPR9.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Возврат товара запрещен: " : ", ") + "Код маркировки не был выведен из оборота (Товар не был ранее продан).";
						continue;
					}
					if (code.expireDate.Year > 1 && code.expireDate <= DateTime.Now && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 11 || DataCommand.TypeCheck == 12))
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR10 = tMarkingCodeValidation3.ValidationPR;
						validationPR10.DecryptionResult = validationPR10.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Истек срок годности товара.";
						continue;
					}
					if (!code.groupIds.Contains(16) && code.mrp != 0m && GoodCodeData3.Price > 0m && code.mrp / 100m < GoodCodeData3.Price && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 11 || DataCommand.TypeCheck == 12))
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR11 = tMarkingCodeValidation3.ValidationPR;
						validationPR11.DecryptionResult = validationPR11.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Превышена максимальная цена продажи, максимальная цена = " + code.mrp / 100m;
						continue;
					}
					if (code.groupIds.Contains(16) && code.mrp != 0m && GoodCodeData3.Price > 0m && code.mrp / 100m > GoodCodeData3.Price && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 11 || DataCommand.TypeCheck == 12))
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR12 = tMarkingCodeValidation3.ValidationPR;
						validationPR12.DecryptionResult = validationPR12.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Цена продажи ниже установленной минимальной цены, минимальная цена = " + code.mrp / 100m;
						continue;
					}
					if (code.smp != 0m && GoodCodeData3.Price > 0m && code.smp / 100m > GoodCodeData3.Price && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 11 || DataCommand.TypeCheck == 12))
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR13 = tMarkingCodeValidation3.ValidationPR;
						validationPR13.DecryptionResult = validationPR13.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + "Цена продажи ниже установленной минимальной цены, минимальная цена = " + code.smp / 100m;
						continue;
					}
					if (code.errorCode != 0)
					{
						tMarkingCodeValidation3.ValidationPR.ValidationResult = false;
						Unit.RezultMarkingCodeValidation.tMarkingCodeValidation.TValidationPR validationPR14 = tMarkingCodeValidation3.ValidationPR;
						validationPR14.DecryptionResult = validationPR14.DecryptionResult + ((tMarkingCodeValidation3.ValidationPR.DecryptionResult == "") ? "Продажа товара запрещена: " : ", ") + code.message;
						continue;
					}
				}
				tMarkingCodeValidation3.IndustryProps = "UUID=" + tRezult.reqId + "&Time=" + tRezult.reqTimestamp;
				GoodCodeData3.IndustryProps = tMarkingCodeValidation3.IndustryProps;
			}
		}
	}

	public static Http.HttpRezult CheckMarkingCode(Unit Unit, TCodesCheck CodesCheck, string CdnURL, string IdCommand)
	{
		string text = "";
		Http.HttpRezult httpRezult = null;
		string curToken = GetCurToken(Unit.Kkm.INN);
		if (curToken != null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/json");
			dictionary.Add("X-API-KEY", curToken);
			httpRezult = Http.HttpReqest(HttpMethod.Post, 200000, CdnURL, "/api/v4/true-api/codes/check", null, dictionary, CodesCheck, typeof(TRezult));
			if (httpRezult.StatusCode == HttpStatusCode.OK)
			{
				TRezult tRezult = httpRezult.Rezult as TRezult;
				if (tRezult.code != 0)
				{
					text = "Error code: " + tRezult.code + ", Description: " + tRezult.description;
				}
			}
			else
			{
				text = httpRezult.Error;
				if (httpRezult.Rezult != null && (httpRezult.Rezult as TRezult).error_message != null && (httpRezult.Rezult as TRezult).error_message != "")
				{
					text = (httpRezult.Rezult as TRezult).error_message + ", " + text;
				}
			}
			Log.SaveLogs(httpRezult, "Проверка КМ, IdCommand: " + IdCommand, text);
		}
		else
		{
			httpRezult = new Http.HttpRezult();
			Log.SaveLogs(httpRezult, "Проверка КМ, IdCommand: " + IdCommand, "Не настроен доступ к серверам разрешительного режима");
		}
		return httpRezult;
	}
}
