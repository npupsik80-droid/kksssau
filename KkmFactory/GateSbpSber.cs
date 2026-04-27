using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace KkmFactory;

internal class GateSbpSber : UnitPort
{
	private class OAuth
	{
		[DataMember]
		public string grant_type = "";

		[DataMember]
		public string scope = "";
	}

	private class ResponseAuthorization
	{
		[DataMember]
		public string httpCode = "";

		[DataMember]
		public string httpMessage = "";

		[DataMember]
		public string moreInformation = "";

		[DataMember]
		public string access_token = "";

		[DataMember]
		public string expires_in = "";

		[DataMember]
		public string scope = "";

		[DataMember]
		public string session_state = "";

		[DataMember]
		public string token_type = "";
	}

	private class OrderCreateQr
	{
		public class order_params
		{
			[DataMember]
			public string position_name = "";

			[DataMember]
			public ulong position_count;

			[DataMember]
			public ulong position_sum;

			[DataMember]
			public string position_description = "";
		}

		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string member_id = "";

		[DataMember]
		public string order_number = "";

		[DataMember]
		public DateTime order_create_date;

		[DataMember]
		public List<order_params> order_params_type = new List<order_params>();

		[DataMember]
		public string id_qr = "";

		[DataMember]
		public ulong order_sum;

		[DataMember]
		public string currency = "643";

		[DataMember]
		public string description = "Оплата";

		[DataMember]
		public string sbp_member_id = "";
	}

	private class OrderCreateQrRs
	{
		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string order_number = "";

		[DataMember]
		public string order_id = "";

		[DataMember]
		public string order_state = "";

		[DataMember]
		public string order_form_url = "";

		[DataMember]
		public string error_code = "";

		[DataMember]
		public string error_description = "";
	}

	private class OrderStatusRequestQr
	{
		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string order_id = "";

		[DataMember]
		public string tid = "";

		[DataMember]
		public string partner_order_number = "";
	}

	private class StatusRequestQrRs
	{
		public class order_operation_param
		{
			[DataMember]
			public string operation_id = "";

			[DataMember]
			public string rrn = "";

			[DataMember]
			public string operation_type = "";

			[DataMember]
			public ulong operation_sum;

			[DataMember]
			public string auth_code = "";

			[DataMember]
			public string response_code = "";
		}

		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string mid = "";

		[DataMember]
		public string tid = "";

		[DataMember]
		public string id_qr = "";

		[DataMember]
		public string order_id = "";

		[DataMember]
		public string order_state = "";

		[DataMember]
		public List<order_operation_param> order_operation_params = new List<order_operation_param>();

		[DataMember]
		public string error_code = "";

		[DataMember]
		public string error_description = "";
	}

	private class OrderRevokeQr
	{
		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string order_id = "";
	}

	private class OrderRevokeQrRs
	{
		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string order_id = "";

		[DataMember]
		public string order_state = "";

		[DataMember]
		public string error_code = "";

		[DataMember]
		public string error_description = "";
	}

	private class OrderCancelQr
	{
		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string order_id = "";

		[DataMember]
		public string operation_type = "";

		[DataMember]
		public string operation_id = "";

		[DataMember]
		public string auth_code = "";

		[DataMember]
		public string id_qr = "";

		[DataMember]
		public string tid = "";

		[DataMember]
		public ulong cancel_operation_sum;

		[DataMember]
		public string operation_currency = "643";
	}

	private class OrderCancelQrRs
	{
		[DataMember]
		public string rq_uid = "";

		[DataMember]
		public DateTime rq_tm;

		[DataMember]
		public string order_id = "";

		[DataMember]
		public string operation_id = "";

		[DataMember]
		public string auth_code = "";

		[DataMember]
		public string rrn = "";

		[DataMember]
		public string order_status = "";

		[DataMember]
		public string error_code = "";

		[DataMember]
		public string error_description = "";
	}

	private class OrderRegistryQr
	{
		[DataMember]
		public string rqUid = "";

		[DataMember]
		public DateTime rqTm;

		[DataMember]
		public string idQR = "";

		[DataMember]
		public DateTime startPeriod;

		[DataMember]
		public DateTime endPeriod;

		[DataMember]
		public string registryType = "";
	}

	private class OrderRegistryQrRs
	{
		public class ItemQuantityData
		{
			[DataMember]
			public ulong totalCount;

			[DataMember]
			public ulong totalPaymentAmount;

			[DataMember]
			public ulong totalRefundAmount;

			[DataMember]
			public ulong totalAmount;
		}

		public class ItemRegistryData
		{
			[DataMember]
			public orderParams orderParams;
		}

		public class orderParams
		{
			[DataMember]
			public List<ItemOrderParam> orderParam = new List<ItemOrderParam>();
		}

		public class ItemOrderParam
		{
			[DataMember]
			public string orderId = "";

			[DataMember]
			public string partnerOrderNumber = "";

			[DataMember]
			public DateTime orderCreateDate;

			[DataMember]
			public ulong amount;

			[DataMember]
			public string orderState = "";
		}

		[DataMember]
		public string rqUid = "";

		[DataMember]
		public DateTime rqTm;

		[DataMember]
		public string idQR = "";

		[DataMember]
		public ItemQuantityData quantityData;

		[DataMember]
		public ItemRegistryData registryData;

		[DataMember]
		public string errorCode = "";

		[DataMember]
		public string error_description = "";
	}

	private decimal OldAmount;

	private string OldUniversalID = "";

	private int Width = 36;

	private string MemberId = "";

	private string ClientId = "";

	private string ClientSecret = "";

	private string Sertificate = "";

	private string SertificatePassword = "";

	private string IdQr = "";

	private string SbpMemberId = "100000000111";

	private DateTime DateChange = DateTime.Now;

	private bool PrintOnKkt = true;

	private bool PrintOnWindow = true;

	private int TimeOut = 60;

	private string AuthorizationCode = "";

	private string AccessToken = "";

	private string IdToken = "";

	private string RqUID = "";

	private const string UrlOautc = "https://mc.api.sberbank.ru:443/prod/tokens/v2/oauth";

	private const string UrlService = "https://mc.api.sberbank.ru:443";

	private List<Unit> ListUnits;

	private X509Certificate2 Cert;

	public GateSbpSber(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.AllKkt;
		IsCommandCancelled = true;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n        <Group Caption='Настройки доступа к системе СБП'>\r\n        <Parameter Name=\"Width\" Caption=\"Ширина чека\" TypeValue=\"String\" DefaultValue=\"36\">\r\n                <ChoiceList>\r\n                    <Item Value=\"48\">48</Item>\r\n                    <Item Value=\"42\">42</Item>\r\n                    <Item Value=\"36\">36</Item>\r\n                    <Item Value=\"32\">32</Item>\r\n                </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"IdQr\" Caption=\"Идентификатор терминала (IdQr)\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Будет доступен на сайте в момент заключения договора c системой СБП.\r\n\r\n                            РЕМ: После подписания договора с системой СБП\r\n                            \"/>\r\n        <Parameter Name=\"MemberId\" Caption=\"Member Id\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Банк вам его пришлет как только будет зарегистрирована организация в сбербанке.\r\n\r\n                            - После регистрации организации в Сбербанке\r\n                            \"/>\r\n        <Parameter Name=\"ClientId\" Caption=\"Client Id\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Будет доступен на сайте в момент регистрации приложения.\r\n\r\n                            - После регистрации организации в Сбербанке\r\n                            \"/>\r\n        <Parameter Name=\"ClientSecret\" Caption=\"Client Secret\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Будет доступен на сайте в момент регистрации приложения.\r\n\r\n                            - После регистрации организации в Сбербанке\r\n                            \"/>\r\n        <Parameter Name=\"Sertificate\" Caption=\"Путь к файлу-сертификату p12\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Будет доступен на сайте в момент регистрации приложения.\r\n                                                 \r\n                            Сертификат нужно скачать с сайта банка и положить в папку на ПК где стоит kkmserver\r\n\r\n                            - После регистрации организации в Сбербанке\r\n                            \"/>\r\n        <Parameter Name=\"SertificatePassword\" Caption=\"Пароль от сертификата\" TypeValue=\"String\" DefaultValue=\"\"\r\n            Description=\"Тот который вводили на сайте при генерации сертификата.\r\n\r\n                            - После регистрации организации в Сбербанке\r\n                            \"/>\r\n        </Group>\r\n    </Page>\r\n    <Page Caption='Параметры'> \r\n        <Parameter Name=\"TimeOut\" Caption=\"Вемя ожидания оплаты (секунд)\" TypeValue=\"Number\" DefaultValue=\"60\"/>\r\n        <Parameter Name=\"DateChange\" Caption=\"Дата открытия смены\" TypeValue=\"String\" DefaultValue=\"\" ReadOnly=\"true\"/>\r\n    </Page>\r\n    <Page Caption='Вывод QR кода для сканирование клиента'> \r\n        <Parameter Name=\"PrintOnKkt\" Caption=\"Печать на ККТ\" TypeValue=\"Boolean\" DefaultValue=\"true\"/>\r\n        <Parameter Name=\"PrintOnWindow\" Caption=\"Печать на экране\" TypeValue=\"Boolean\" DefaultValue=\"true\"/>\r\n    </Page>\r\n</Settings>";
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "Система быстрых платежей (СБП) по QR коду из приложения на телефоне";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		NameDevice = "СБП Сбербанк";
		UnitAdditionallinks = "<a href='https://sbp.nspk.ru/business/'>О системе быстрых платежей</a> - Общая информация<br/>\r\n                                <a href='https://www.sberbank.ru/ru/s_m_business/bankingservice/platiqr'>Регистрация СБП в сбербанке</a> - Там нужно выбрать \"На сайте или в приложении\"<br/>\r\n                                <a href='https://www.sberbank.ru/help/business/sbbol/100013'>Регистрация СБП для Для клиентов Сбер-Бизнес</a> - Выбирать оборудование там не нужно, сразу заполняйте \"Торговая точка\"<br/>\r\n                                <a href='https://sbp.nspk.ru/sbpay/'>Приложение для оплаты по QR коду на мобильнике для других банков</a> - Для сбербанка в приложении \"Сбербанк\"<br/>";
		text = text.Replace("'", "\"");
		LoadParametsFromXML(text);
	}

	public override void WriteParametsToUnits()
	{
		base.WriteParametsToUnits();
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			switch (unitParamet.Key)
			{
			case "Width":
				Width = unitParamet.Value.AsInt();
				break;
			case "MemberId":
				MemberId = unitParamet.Value.Trim();
				break;
			case "ClientId":
				ClientId = unitParamet.Value.Trim();
				break;
			case "ClientSecret":
				ClientSecret = unitParamet.Value.Trim();
				break;
			case "Sertificate":
				Sertificate = unitParamet.Value.Trim();
				break;
			case "SertificatePassword":
				SertificatePassword = unitParamet.Value.Trim();
				break;
			case "IdQr":
				IdQr = unitParamet.Value.Trim();
				break;
			case "PrintOnKkt":
				PrintOnKkt = unitParamet.Value.AsBool();
				break;
			case "PrintOnWindow":
				PrintOnWindow = unitParamet.Value.AsBool();
				break;
			case "TimeOut":
				TimeOut = unitParamet.Value.AsInt();
				break;
			case "DateChange":
				try
				{
					DateChange = unitParamet.Value.AsDateTime();
				}
				catch
				{
				}
				break;
			}
		}
	}

	public override void SaveParametrs(Dictionary<string, string> NewParamets)
	{
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	public override async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		await base.InitDevice(FullInit, Program);
		IsInit = true;
		return true;
	}

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		ListUnits = null;
		DateTime StartTime = DateTime.Now;
		StringBuilder Slip = new StringBuilder();
		string OrderStateName = "";
		string OrderNumber = Guid.NewGuid().ToString().Replace("-", "");
		Slip.Append(Unit.GetPringString("<<=>>", Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("ИНН:<#0#>" + Kkm.INN, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("Терминал:<#0#>" + IdQr, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("ID пользователя:<#0#>" + MemberId, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
		PortLogs.Append("Start");
		string PngBase64 = null;
		string TextQR = null;
		switch (Command)
		{
		case 0:
		{
			Unit.WindowTrackingStatus(DataCommand, this, "Получение QR оплаты... ", TextQR, PngBase64);
			OutputOnCustomerDisplay("Получение QR оплаты... ", "Сумма: " + DataCommand.Amount + " руб.");
			PortLogs.Append("Запрос на создание QR");
			await QueryToAuthorizationCode(DataCommand, "https://api.sberbank.ru/qr/order.create", 5);
			OrderCreateQr body2 = new OrderCreateQr
			{
				rq_uid = RqUID,
				rq_tm = DateTime.Now.ToUniversalTime(),
				member_id = MemberId,
				order_number = OrderNumber,
				order_create_date = DateTime.Now.ToUniversalTime(),
				id_qr = IdQr,
				order_sum = (ulong)(DataCommand.Amount * 100m),
				currency = "643",
				description = "Payment",
				sbp_member_id = SbpMemberId
			};
			Http.HttpRezult httpRezult2 = await QueryToServ("/prod/qr/order/v3/creation", body2, typeof(OrderCreateQrRs));
			OrderCreateQrRs OrderCreateQrRs = (OrderCreateQrRs)httpRezult2.Rezult;
			string TextRez = httpRezult2.Response;
			if (CreateTextError(OrderCreateQrRs.error_code, OrderCreateQrRs.error_description))
			{
				return;
			}
			RezultCommand.ReceiptNumber = OrderCreateQrRs.order_id;
			RezultCommand.TerminalID = IdQr;
			RezultCommand.Url = OrderCreateQrRs.order_form_url;
			RezultCommand.Amount = DataCommand.Amount;
			string OrderState = OrderCreateQrRs.order_state;
			PortLogs.Append("Запрос на создание QR = " + OrderState, "<");
			PortLogs.Append("Запрос на создание QR = '" + TextRez + "'");
			Slip.Append(Unit.GetPringString(">#2#<ОПЛАТА", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Система:<#0#>Сбербанк СБП", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Номер:<#0#>" + OrderNumber, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("ID QR:<#0#>" + OrderCreateQrRs.order_id, Width, "\r\n") + "\r\n");
			if (PrintOnWindow)
			{
				PortLogs.Append("Выводим QR код для СБП на экран");
				if (DataCommand.RunAsAddIn)
				{
					TextQR = "ОТСКАНИРУЙТЕ КОД ИЗ МОБИЛЬНОГО ПРИЛОЖЕНИЯ";
					PngBase64 = BarCode.GetImageBarCode("QR", OrderCreateQrRs.order_form_url, 400, 400).PngBase64;
				}
				else
				{
					WriteLine(Unit.GetPringString("<<=>>", Width));
					WriteLine(Unit.GetPringString(">#2#<QR КОД ОПЛАТЫ", Width));
					ImageBarCode imageBarCode = BarCode.GetImageBarCode("QR", OrderCreateQrRs.order_form_url, 200, 200);
					WriteLine(imageBarCode, 0, Clear: false, null, AsCheck: true);
					WriteLine(Unit.GetPringString(">#2#<ОТСКАНИРУЙТЕ КОД ИЗ МОБИЛЬНОГО ПРИЛОЖЕНИЯ", Width));
					WriteLine(Unit.GetPringString("Система:<#0#>Сбербанк СБП", Width));
					WriteLine(Unit.GetPringString("Номер:<#0#>" + OrderNumber, Width));
					WriteLine(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width));
					WriteLine(Unit.GetPringString("<<->>", Width));
					WriteLine(Unit.GetPringString("ID:<#0#>" + OrderCreateQrRs.order_id, Width));
					WriteLine(Unit.GetPringString("<<=>>", Width));
					WriteLine("", 0, Clear: false, true, AsCheck: true);
					RezultCommand.TypeMessageHTM = "TrackingMessage";
				}
			}
			if (PrintOnKkt)
			{
				PortLogs.Append("Печатаем QR код для СБП на ККТ");
				int num = int.Parse(SettDr.Paramets["NumDeviceByPrintSlip"]);
				if (num == 0)
				{
					foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
					{
						if ((device.Value.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && Kkm.INN == "") || device.Value.INN == Kkm.INN)
						{
							num = device.Value.NumDevice;
							break;
						}
					}
				}
				DataCommand dataCommand = new DataCommand();
				dataCommand.Command = "RegisterCheck";
				dataCommand.IsFiscalCheck = false;
				dataCommand.NumDevice = num;
				dataCommand.RunComPort = true;
				List<DataCommand.CheckString> list = new List<DataCommand.CheckString>();
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("<<=>>", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(">#2#<QR КОД ОПЛАТЫ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(" ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					BarCode = new DataCommand.PrintBarcode
					{
						BarcodeType = "QR",
						Barcode = OrderCreateQrRs.order_form_url
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(" ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString(">#2#<ОТСКАНИРУЙТЕ КОД ИЗ МОБИЛЬНОГО ПРИЛОЖЕНИЯ", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("Система:<#0#>Сбербанк СБП", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("Номер:<#0#>" + OrderNumber, Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("<<->>", Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("ID: " + OrderCreateQrRs.order_id, Width)
					}
				});
				list.Add(new DataCommand.CheckString
				{
					PrintText = new DataCommand.PrintString
					{
						Text = Unit.GetPringString("<<=>>", Width)
					}
				});
				dataCommand.CheckStrings = list.ToArray();
				try
				{
					Unit unit = Global.UnitManager.Units[num];
					RezultCommand rezultCommand = new RezultCommandKKm();
					await unit.ExecuteCommand(dataCommand, rezultCommand);
				}
				catch
				{
				}
			}
			await Task.Delay(3000);
			Unit.WindowTrackingStatus(DataCommand, this, "Ожидение оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
			OutputOnCustomerDisplay("Отсканируйте код для оплаты...", DataCommand.Amount + " руб.", OrderCreateQrRs.order_form_url);
			await Task.Delay(7000);
			DateTime? DateTokenStatus = null;
			string AccessTokenStatus = null;
			Unit.WindowTrackingStatus(DataCommand, this, "Проверка оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
			while (true)
			{
				if (StartTime.AddSeconds(TimeOut) < DateTime.Now || CancellationCommand)
				{
					Unit.WindowTrackingStatus(DataCommand, this, "Отмена QR оплаты... ", TextQR, PngBase64);
					OutputOnCustomerDisplay("Отмена QR оплаты...", DataCommand.Amount + " руб.");
					try
					{
						Error = "";
						PortLogs.Append("Запрос на отмену QR");
						await QueryToAuthorizationCode(DataCommand, "https://api.sberbank.ru/qr/order.revoke");
						OrderRevokeQr body3 = new OrderRevokeQr
						{
							rq_uid = RqUID,
							rq_tm = DateTime.Now.ToUniversalTime(),
							order_id = RezultCommand.ReceiptNumber
						};
						httpRezult2 = await QueryToServ("/prod/qr/order/v3/revocation", body3, typeof(OrderRevokeQrRs));
						OrderRevokeQrRs orderRevokeQrRs = (OrderRevokeQrRs)httpRezult2.Rezult;
						TextRez = httpRezult2.Response;
						if (CreateTextError(orderRevokeQrRs.error_code, orderRevokeQrRs.error_description))
						{
							throw new Exception(Error);
						}
						OrderState = orderRevokeQrRs.order_state;
						PortLogs.Append("Запрос на отмену QR = " + OrderState, "<");
						switch (OrderState)
						{
						case "DECLINED":
							break;
						case "EXPIRED":
							break;
						default:
							goto IL_1004;
						}
					}
					catch
					{
						Unit.WindowTrackingStatus(DataCommand, this, "Ошибка отмены оплаты... ", TextQR, PngBase64);
						OutputOnCustomerDisplay("Ошибка отмены оплаты... ", DataCommand.Amount + " руб.");
						goto IL_1004;
					}
					break;
				}
				goto IL_1004;
				IL_141d:
				if (OrderState != "CREATED" && OrderState != "ON_PAYMENT")
				{
					break;
				}
				if (StartTime.AddSeconds(TimeOut + 60) < DateTime.Now)
				{
					OrderState = "EXPIRED";
					break;
				}
				await Task.Delay(5000);
				continue;
				IL_1004:
				try
				{
					Error = "";
					if (!DateTokenStatus.HasValue || DateTokenStatus.Value.AddSeconds(45.0) < DateTime.Now)
					{
						PortLogs.Append("Запрос статуса QR");
						await QueryToAuthorizationCode(DataCommand, "https://api.sberbank.ru/qr/order.status");
						DateTokenStatus = DateTime.Now;
						AccessTokenStatus = AccessToken;
					}
					else
					{
						AccessToken = AccessTokenStatus;
					}
					OrderStatusRequestQr body4 = new OrderStatusRequestQr
					{
						rq_uid = RqUID,
						rq_tm = DateTime.Now.ToUniversalTime(),
						order_id = RezultCommand.ReceiptNumber,
						tid = IdQr,
						partner_order_number = RezultCommand.ReceiptNumber
					};
					httpRezult2 = await QueryToServ("/prod/qr/order/v3/status", body4, typeof(StatusRequestQrRs));
					StatusRequestQrRs statusRequestQrRs = (StatusRequestQrRs)httpRezult2.Rezult;
					TextRez = httpRezult2.Response;
					if (CreateTextError(statusRequestQrRs.error_code, statusRequestQrRs.error_description))
					{
						throw new Exception(Error);
					}
					OrderState = statusRequestQrRs.order_state;
					if (OrderState == "PAID" && statusRequestQrRs.order_operation_params.Count > 0 && statusRequestQrRs.order_operation_params[0].operation_type == "PAY")
					{
						RezultCommand.RRNCode = statusRequestQrRs.order_operation_params[0].rrn;
						RezultCommand.AuthorizationCode = statusRequestQrRs.order_operation_params[0].auth_code;
						RezultCommand.CardDPAN = statusRequestQrRs.order_operation_params[0].operation_id;
						Slip.Append(Unit.GetPringString("RRN:<#0#>" + RezultCommand.RRNCode, Width, "\r\n") + "\r\n");
						Slip.Append(Unit.GetPringString("Auth code:<#0#>" + RezultCommand.AuthorizationCode, Width, "\r\n") + "\r\n");
						Slip.Append(Unit.GetPringString("Operation id:<#0#>" + RezultCommand.CardDPAN, Width, "\r\n") + "\r\n");
					}
					switch (OrderState)
					{
					case "DECLINED":
						break;
					case "EXPIRED":
						break;
					default:
						PortLogs.Append("Запрос статуса QR = " + OrderState, "<");
						Unit.WindowTrackingStatus(DataCommand, this, "Проверка статуса оплаты... " + GetTimeEnd(StartTime), TextQR, PngBase64);
						goto IL_141d;
					}
				}
				catch
				{
					Unit.WindowTrackingStatus(DataCommand, this, "Ошибка проверка статуса... " + GetTimeEnd(StartTime), TextQR, PngBase64);
					goto IL_141d;
				}
				break;
			}
			PortLogs.Append("Запрос статуса QR = '" + TextRez + "'");
			switch (OrderState)
			{
			case "CREATED":
				Error = "Оплата создана";
				break;
			case "PAID":
				OrderStateName = "Оплачено";
				break;
			case "REVERSED":
				Error = "Оплата отменена";
				break;
			case "REFUNDED":
				Error = "Оплата возвращена";
				break;
			case "REVOKED":
				Error = "Счет отменен";
				break;
			case "DECLINED":
				Error = "Счет отклонен";
				break;
			case "EXPIRED":
				Error = "Счет просрочен";
				break;
			case "AUTHORIZED":
				Error = "Зарезервировано (1)";
				break;
			case "CONFIRMED":
				Error = "Зарезервировано (2)";
				break;
			case "ON_PAYMENT":
				Error = "Ожидание оплаты";
				break;
			}
			break;
		}
		case 1:
		case 2:
		{
			Dictionary<string, object> Dict = SetDictFromString(DataCommand.UniversalID, DataCommand);
			PortLogs.Append("Запрос на возврат оплаты QR");
			await QueryToAuthorizationCode(DataCommand, "https://api.sberbank.ru/qr/order.cancel");
			string operation_type = "";
			string AccessTokenStatus = "";
			switch (Command)
			{
			case 1:
				operation_type = "REFUND";
				AccessTokenStatus = "ВОЗВРАТ ОПЛАТЫ";
				break;
			case 2:
				operation_type = "REVERSE";
				AccessTokenStatus = "ОТМЕНА ОПЛАТЫ";
				break;
			}
			OrderCancelQr body = new OrderCancelQr
			{
				rq_uid = RqUID,
				rq_tm = DateTime.Now.ToUniversalTime(),
				order_id = DataCommand.ReceiptNumber,
				operation_type = operation_type,
				operation_id = (Dict.ContainsKey("CD") ? Dict["CD"].ToString() : ""),
				auth_code = DataCommand.AuthorizationCode,
				id_qr = IdQr,
				tid = IdQr,
				cancel_operation_sum = (ulong)(DataCommand.Amount * 100m),
				operation_currency = "643"
			};
			Http.HttpRezult httpRezult = await QueryToServ("/prod/qr/order/v3/cancel", body, typeof(OrderCancelQrRs));
			OrderCancelQrRs orderCancelQrRs = (OrderCancelQrRs)httpRezult.Rezult;
			string TextRez = httpRezult.Response;
			if (CreateTextError(orderCancelQrRs.error_code, orderCancelQrRs.error_description))
			{
				return;
			}
			RezultCommand.RRNCode = orderCancelQrRs.rrn;
			RezultCommand.AuthorizationCode = orderCancelQrRs.auth_code;
			RezultCommand.CardDPAN = orderCancelQrRs.operation_id;
			RezultCommand.TerminalID = IdQr;
			RezultCommand.Amount = DataCommand.Amount;
			string OrderState = orderCancelQrRs.order_status;
			PortLogs.Append("Запрос на возврат оплаты QR = " + OrderState, "<");
			Slip.Append(Unit.GetPringString(">#2#<" + AccessTokenStatus, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Система:<#0#>Сбербанк СБП", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("ID QR:<#0#>" + DataCommand.ReceiptNumber, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("RRN:<#0#>" + RezultCommand.RRNCode, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Auth code:<#0#>" + RezultCommand.AuthorizationCode, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Operation id:<#0#>" + RezultCommand.CardDPAN, Width, "\r\n") + "\r\n");
			PortLogs.Append("Запрос статуса QR = '" + TextRez + "'");
			switch (OrderState)
			{
			case "CREATED":
				Error = "Оплата создана";
				break;
			case "PAID":
				OrderStateName = "Оплачено";
				break;
			case "REVERSED":
				OrderStateName = "Оплата отменена";
				break;
			case "REFUNDED":
				OrderStateName = "Оплата возвращена";
				break;
			case "REVOKED":
				Error = "Счет отменен";
				break;
			case "DECLINED":
				Error = "Счет отклонен";
				break;
			case "EXPIRED":
				Error = "Счет просрочен";
				break;
			case "AUTHORIZED":
				Error = "Зарезервировано (1)";
				break;
			case "CONFIRMED":
				Error = "Зарезервировано (2)";
				break;
			case "ON_PAYMENT":
				Error = "Ожидание оплаты";
				break;
			}
			break;
		}
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			new StringBuilder();
			return;
		}
		PortLogs.Append("End");
		if (Error != "")
		{
			Slip.Append(Unit.GetPringString("Статус:<#0#>" + Error, Width, "\r\n") + "\r\n");
			RezultCommand.Status = ExecuteStatus.Error;
		}
		else
		{
			Slip.Append(Unit.GetPringString("Статус:<#0#>" + OrderStateName, Width, "\r\n") + "\r\n");
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		Slip.Append(Unit.GetPringString("<<=>>", Width, "\r\n") + "\r\n");
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		RezultCommand.Slip = Slip.ToString();
		TextLines.Clear();
		if (Command == 0)
		{
			OldAmount = RezultCommand.Amount;
			OldUniversalID = RezultCommand.UniversalID;
		}
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.UniversalID == "")
		{
			DataCommand.UniversalID = OldUniversalID;
		}
		await CommandPayTerminal(DataCommand, RezultCommand, 2);
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await TerminalReport(DataCommand, RezultCommand);
		DateChange = DateTime.Now;
		SettDr.Paramets["DateChange"] = DateChange.ToString("yyyy.MM.dd HH:mm:ss");
		await Global.SaveSettingsAsync();
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
		StringBuilder Slip = new StringBuilder();
		Slip.Append(Unit.GetPringString("<<=>>", Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("ИНН:<#0#>" + Kkm.INN, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("Терминал:<#0#>" + IdQr, Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("ID пользователя:<#0#>" + MemberId, Width, "\r\n") + "\r\n");
		PortLogs.Append("Start");
		if (DataCommand.Command == "Settlement")
		{
			await TerminalReportList(Full: true, OnStartDate: true, DataCommand, Slip);
			await TerminalReportList(Full: false, OnStartDate: true, DataCommand, Slip);
		}
		else if (!DataCommand.Detailed)
		{
			await TerminalReportList(Full: false, OnStartDate: false, DataCommand, Slip);
		}
		else if (DataCommand.Detailed)
		{
			await TerminalReportList(Full: true, OnStartDate: false, DataCommand, Slip);
			await TerminalReportList(Full: false, OnStartDate: false, DataCommand, Slip);
		}
		PortLogs.Append("End");
		Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("Статус:<#0#>000000", Width, "\r\n") + "\r\n");
		Slip.Append(Unit.GetPringString("<<=>>", Width, "\r\n") + "\r\n");
		RezultCommand.Slip = Slip.ToString();
	}

	public async Task TerminalReportList(bool Full, bool OnStartDate, DataCommand DataCommand, StringBuilder Slip)
	{
		TimeZoneInfo local = TimeZoneInfo.Local;
		TimeSpan Offset = local.GetUtcOffset(DateTime.Now);
		PortLogs.Append("Запрос на отчет по QR");
		await QueryToAuthorizationCode(DataCommand, "auth://qr/order.registry");
		_ = DateChange;
		string registryType = ((!Full) ? "QUANTITY" : "REGISTRY");
		DateTime dateTime = ((!OnStartDate) ? DateChange : DateChange);
		OrderRegistryQr body = new OrderRegistryQr
		{
			rqUid = RqUID,
			rqTm = DateTime.Now.ToUniversalTime(),
			idQR = IdQr,
			startPeriod = dateTime.Add(Offset).ToUniversalTime(),
			endPeriod = DateTime.Now.Add(Offset).ToUniversalTime(),
			registryType = registryType
		};
		Http.HttpRezult obj = await QueryToServ("/prod/qr/order/v3/registry", body, typeof(OrderRegistryQrRs));
		OrderRegistryQrRs orderRegistryQrRs = (OrderRegistryQrRs)obj.Rezult;
		_ = obj.Response;
		if (CreateTextError(orderRegistryQrRs.errorCode, orderRegistryQrRs.error_description))
		{
			return;
		}
		PortLogs.Append("Запрос на отчет по QR = " + orderRegistryQrRs.errorCode, "<");
		if (orderRegistryQrRs.quantityData != null)
		{
			Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Всего операций:<#0#>" + ((decimal)orderRegistryQrRs.quantityData.totalCount).ToString("0.00"), Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Всего на сумму:<#0#>" + ((decimal)orderRegistryQrRs.quantityData.totalPaymentAmount / 100m).ToString("0.00"), Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Из них возвращено:<#0#>" + ((decimal)orderRegistryQrRs.quantityData.totalRefundAmount / 100m).ToString("0.00"), Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Итого:<#0#>" + ((decimal)orderRegistryQrRs.quantityData.totalAmount / 100m).ToString("0.00"), Width, "\r\n") + "\r\n");
		}
		if (orderRegistryQrRs.registryData == null || orderRegistryQrRs.registryData.orderParams == null)
		{
			return;
		}
		foreach (OrderRegistryQrRs.ItemOrderParam item in orderRegistryQrRs.registryData.orderParams.orderParam)
		{
			Slip.Append(Unit.GetPringString("<<->>", Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("ID QR:<#0#>" + item.orderId, Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Дата:<#0#>" + item.orderCreateDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"), Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Сумма:<#0#>" + ((decimal)item.amount / 100m).ToString("0.00"), Width, "\r\n") + "\r\n");
			Slip.Append(Unit.GetPringString("Статус операции:<#0#>" + item.orderState, Width, "\r\n") + "\r\n");
		}
	}

	private void OutputOnCustomerDisplay(string TopString, string BottomString, string CodeQR = null)
	{
		DataCommand DataCommandCustomerDisplay = new DataCommand();
		DataCommandCustomerDisplay.NumDevice = int.Parse(UnitParamets["NumDeviceCustomerDisplay"]);
		DataCommandCustomerDisplay.NoError = DataCommandCustomerDisplay.NumDevice == 0;
		DataCommandCustomerDisplay.Command = "OutputOnCustomerDisplay";
		DataCommandCustomerDisplay.TopString = TopString;
		DataCommandCustomerDisplay.BottomString = BottomString;
		DataCommandCustomerDisplay.CodeQR = CodeQR;
		if (ListUnits == null)
		{
			List<Unit> ListSortUnits = Global.UnitManager.Units.Select(delegate(KeyValuePair<int, Unit> u)
			{
				KeyValuePair<int, Unit> keyValuePair = u;
				return keyValuePair.Value;
			}).ToList();
			ListUnits = UnitManager.GetListUnitsForCommand(DataCommandCustomerDisplay, TypeDevice.enType.ДисплеиПокупателя, ref ListSortUnits);
		}
		if (ListUnits.Count <= 0)
		{
			return;
		}
		Task.Run(async delegate
		{
			string textCommand = JsonConvert.SerializeObject(DataCommandCustomerDisplay, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			});
			try
			{
				await Global.UnitManager.AddCommand(DataCommandCustomerDisplay, "", textCommand);
			}
			catch (Exception)
			{
			}
		});
	}

	public void PrintBarCode(string Barcode)
	{
		ImageBarCode imageBarCode = null;
		imageBarCode = BarCode.GetImageBarCode("QR", Barcode, 200, 200);
		if (imageBarCode != null)
		{
			WriteLine(imageBarCode, 0, Clear: false, null, AsCheck: true);
		}
	}

	private string GetTimeEnd(DateTime StartTime)
	{
		return "отмена через " + (StartTime.AddSeconds(TimeOut) - DateTime.Now).ToString("m\\:ss") + " с";
	}

	private async Task QueryToAuthorizationCode1(DataCommand DataCommand, string Scope, int ColRepet = 1)
	{
		for (int Repet = 1; Repet <= ColRepet; Repet++)
		{
			AuthorizationCode = "";
			RqUID = DataCommand.IdCommand.ToString().Replace("-", "");
			PortLogs.Append("RqUID = " + RqUID);
			AccessToken = "";
			try
			{
				if (Cert == null && File.Exists(Sertificate))
				{
					Cert = new X509Certificate2(Sertificate, SertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
				}
			}
			catch (Exception)
			{
				throw new Exception("Не загружен сертификат для связи с сервером sberbank.ru.");
			}
			string urlServer = "https://mc.api.sberbank.ru:443/prod/tokens/v2/oauth";
			byte[] bytes = Encoding.ASCII.GetBytes(ClientId.Trim() + ":" + ClientSecret.Trim());
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/x-www-form-urlencoded");
			dictionary.Add("Authorization", "Basic " + Convert.ToBase64String(bytes));
			dictionary.Add("RqUID", RqUID);
			dictionary.Add("accept", "application/json");
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("cn", "sberbank.ru");
			string text = $"grant_type=client_credentials&scope={HttpUtility.UrlEncode(Scope)}";
			byte[] bytes2 = Encoding.UTF8.GetBytes(text);
			Http.HttpRezult httpRezult = await Http.HttpReqestAsync(HttpMethod.Post, 10000, urlServer, "", null, dictionary, bytes2, typeof(string), dictionary2, Cert);
			if (httpRezult.StatusCode == HttpStatusCode.OK && httpRezult.Rezult != null)
			{
				ResponseAuthorization responseAuthorization = JsonConvert.DeserializeObject<ResponseAuthorization>((string)httpRezult.Rezult, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				if (responseAuthorization.access_token != "")
				{
					AccessToken = responseAuthorization.access_token;
					PortLogs.Append("Получен токен, Token = " + AccessToken + ", Scope = " + Scope, "<");
				}
				break;
			}
			if (httpRezult.Error.IndexOf("Сервер не отвечает") == -1 || Repet >= ColRepet)
			{
				PortLogs.Append("Ошибка вызова сервиса аутенфикации СБП: " + httpRezult.Error, "<");
				throw new Exception("Ошибка вызова сервиса аутенфикации СБП: " + httpRezult.Error);
			}
		}
	}

	private async Task QueryToAuthorizationCode(DataCommand DataCommand, string Scope, int ColRepet = 1)
	{
		for (int i = 1; i <= ColRepet; i++)
		{
			AuthorizationCode = "";
			RqUID = DataCommand.IdCommand.ToString().Replace("-", "");
			PortLogs.Append("RqUID = " + RqUID);
			AccessToken = "";
			try
			{
				if (Cert == null && File.Exists(Sertificate))
				{
					Cert = new X509Certificate2(Sertificate, SertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
				}
			}
			catch (Exception)
			{
				throw new Exception("Не загружен сертификат для связи с сервером sberbank.ru.");
			}
			string text = "https://mc.api.sberbank.ru:443/prod/tokens/v2/oauth";
			HttpClientHandler httpClientHandler = new HttpClientHandler();
			httpClientHandler.AllowAutoRedirect = false;
			httpClientHandler.ServerCertificateCustomValidationCallback = (HttpRequestMessage Request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => (certificate != null && certificate.Subject.ToLower().Contains("sberbank.ru".ToLower())) ? true : false;
			httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
			httpClientHandler.ClientCertificates.Add(Cert);
			httpClientHandler.CheckCertificateRevocationList = false;
			httpClientHandler.PreAuthenticate = true;
			HttpClient httpClient = new HttpClient(httpClientHandler);
			httpClient.Timeout = TimeSpan.FromMilliseconds(10000.0);
			byte[] bytes = Encoding.ASCII.GetBytes(ClientId.Trim() + ":" + ClientSecret.Trim());
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytes));
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("RqUID", RqUID);
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
			string text2 = $"grant_type=client_credentials&scope={HttpUtility.UrlEncode(Scope)}";
			ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(text2));
			byteArrayContent.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
			Task<HttpResponseMessage> task = null;
			Task<string> task2;
			try
			{
				task = httpClient.PostAsync(text, byteArrayContent);
				task.Wait();
				task2 = task.Result.Content.ReadAsStringAsync();
				task2.Wait();
			}
			catch (AggregateException ex2)
			{
				string text3 = "Сервер не отвечает (таймаут)";
				if (ex2.GetType() == typeof(AggregateException) && ex2.InnerExceptions[0].GetType() == typeof(TaskCanceledException) && i < ColRepet)
				{
					continue;
				}
				try
				{
					text3 = text3 + " - " + ex2.Message;
				}
				catch
				{
				}
				try
				{
					text3 = text3 + " - " + ex2.InnerException.Message;
				}
				catch
				{
				}
				try
				{
					text3 = text3 + " - " + ex2.InnerException.InnerException.Message;
				}
				catch
				{
				}
				try
				{
					text3 = text3 + " - " + ex2.InnerException.InnerException.InnerException.Message;
				}
				catch
				{
				}
				try
				{
					text3 = text3 + " - StatusCode: " + task.Result.StatusCode;
				}
				catch
				{
				}
				try
				{
					task2 = task.Result.Content.ReadAsStringAsync();
					task2.Wait();
					text3 = text3 + "<br/>" + task2.Result;
				}
				catch
				{
				}
				throw new Exception("Ошибка вызова сервиса аутенфикации СБП: " + text3);
			}
			catch (Exception ex3)
			{
				string text3 = ex3.InnerException.Message;
				try
				{
					task2 = task.Result.Content.ReadAsStringAsync();
					task2.Wait();
					text3 = text3 + "<br/>" + task2.Result;
				}
				catch
				{
				}
				throw new Exception("Ошибка вызова сервиса аутенфикации СБП: " + text3);
			}
			if (task.Result.StatusCode != HttpStatusCode.OK)
			{
				string text4 = task.Result.StatusCode.ToString();
				try
				{
					ResponseAuthorization responseAuthorization = JsonConvert.DeserializeObject<ResponseAuthorization>(task2.Result, new JsonSerializerSettings
					{
						StringEscapeHandling = StringEscapeHandling.EscapeHtml
					});
					if (responseAuthorization.httpMessage != "")
					{
						text4 = responseAuthorization.httpMessage;
					}
					if (responseAuthorization.moreInformation != "")
					{
						text4 = text4 + ", " + responseAuthorization.moreInformation;
					}
				}
				catch
				{
					try
					{
						text4 = text4 + ", " + task2.Result;
					}
					catch
					{
					}
				}
				throw new Exception("Ошибка вызова сервиса аутенфикации СБП: " + text4);
			}
			if (task.Result.StatusCode == HttpStatusCode.OK)
			{
				ResponseAuthorization responseAuthorization2 = JsonConvert.DeserializeObject<ResponseAuthorization>(task2.Result, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				if (responseAuthorization2.access_token != "")
				{
					AccessToken = responseAuthorization2.access_token;
				}
			}
			break;
		}
	}

	private async Task<Http.HttpRezult> QueryToServ(string UrlQuery, object body, Type TypeRez)
	{
		string urlServer = "https://mc.api.sberbank.ru:443" + UrlQuery;
		Encoding.ASCII.GetBytes(ClientId.Trim() + ":" + ClientSecret.Trim());
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Content-Type", "application/json");
		dictionary.Add("Authorization", "Bearer " + AccessToken);
		dictionary.Add("RqUID", RqUID.Replace("-", ""));
		dictionary.Add("accept", "application/json");
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		dictionary2.Add("cn", "sberbank.ru");
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";
		string body2 = JsonConvert.SerializeObject(body, jsonSerializerSettings);
		Http.HttpRezult httpRezult = await Http.HttpReqestAsync(HttpMethod.Post, 10000, urlServer, "", null, dictionary, body2, TypeRez, dictionary2, Cert);
		if (httpRezult.StatusCode == HttpStatusCode.OK && httpRezult.Rezult != null)
		{
			return httpRezult;
		}
		PortLogs.Append("Ошибка вызова сервиса СБП, UrlQuery = " + UrlQuery + ": " + httpRezult.Error, "<");
		throw new Exception("Ошибка вызова сервиса СБП: " + httpRezult.Error);
	}

	private bool CreateTextError(string CodeError, string error_description)
	{
		string text = "";
		switch (CodeError)
		{
		case "170000":
			text = "Превышен допустимый промежуток времени";
			break;
		case "180000":
			text = "Превышено допустимое количество записей";
			break;
		case "990000":
			text = "Операция в обработке";
			break;
		}
		if (CodeError.Length == 1)
		{
			CodeError = "0" + CodeError;
		}
		if (CodeError.Length >= 2)
		{
			switch (CodeError.Substring(0, 2))
			{
			case "01":
				text = "Общий отказ";
				break;
			case "02":
				text = "ТСТ не найдено в системе";
				break;
			case "03":
				text = "Оплата в ТСТ приостановлена";
				break;
			case "04":
				text = "Операция не разрешена Партнеру";
				break;
			case "05":
				text = "Некорректный формат запроса или данные не найдены";
				break;
			case "06":
				text = "Подозрительная операция";
				break;
			case "07":
				text = "Сумма отмены / возврата в рамках заказа больше суммы оригинальной операции";
				break;
			case "08":
				text = "Нарушена последовательность запросов";
				break;
			case "09":
				text = "Оригинальный заказ для отмены / возврата не найден";
				break;
			case "10":
				text = "Указанный заказ не найден";
				break;
			case "11":
				text = "Недопустимая сумма в заказе";
				break;
			case "12":
				text = "Партнер не найден в системе";
				break;
			case "13":
				text = "Операция не была проведена успешно";
				break;
			case "14":
				text = "Номер наклейки QR не найден в системе";
				break;
			case "15":
				text = "Операция не найдена в системе";
				break;
			case "16":
				text = "Некорректный Код валюты (Currency)";
				break;
			}
		}
		if (text != "")
		{
			if (Error != "")
			{
				Error = Error + " ( " + Error + ": " + text + " )";
			}
			else
			{
				Error = Error + ": " + text;
			}
			if (error_description != "" && error_description != null)
			{
				Error = Error + " - " + error_description;
			}
			return true;
		}
		return false;
	}
}
