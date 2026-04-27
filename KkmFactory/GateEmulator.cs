using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateEmulator : UnitPort
{
	public class PayItem
	{
		public int Command;

		public decimal Amount;

		public string AuthorizationCode = "";

		public string ReceiptNumber = "";

		public string RRNCode = "";

		public string CardNumber = "";
	}

	private decimal OldAmount;

	private string OldAuthorizationCode = "";

	private string OldReceiptNumber = "";

	private string OldRRNCode = "";

	private string OldCardNumber = "";

	private int ReceiptNumber = 25;

	private int WorkTime;

	private int Width = 36;

	private string Action = "Execute";

	private List<PayItem> ListPays = new List<PayItem>();

	public GateEmulator(Global.DeviceSettings SettDr, int NumUnit)
		: base(SettDr, NumUnit)
	{
		LicenseFlags = ComDevice.PaymentOption.None;
		IsCommandCancelled = true;
	}

	public override void LoadParamets()
	{
		string text = "<?xml version='1.0' encoding='UTF-8' ?>\r\n<Settings>\r\n    <Page Caption='Параметры'>    \r\n    <Group Caption='Настройки'>\r\n        <Parameter Name=\"Width\" Caption=\"Ширина чека\" TypeValue=\"String\" DefaultValue=\"36\">\r\n                <ChoiceList>\r\n                    <Item Value=\"48\">48</Item>\r\n                    <Item Value=\"42\">42</Item>\r\n                    <Item Value=\"36\">36</Item>\r\n                    <Item Value=\"32\">32</Item>\r\n                </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"Action\" Caption=\"Действие при транзакции\" TypeValue=\"String\" DefaultValue=\"36\">\r\n                <ChoiceList>\r\n                    <Item Value=\"Execute\">Проводить транзакцию</Item>\r\n                    <Item Value=\"Failure\">Эмулировать отказ</Item>\r\n                    <Item Value=\"Error\">Эмулировать ошибку оборудования</Item>\r\n                </ChoiceList>\r\n        </Parameter>\r\n        <Parameter Name=\"WorkTime\" Caption=\"Макс. время выполнения команды\" TypeValue=\"String\" DefaultValue=\"0\"\r\n                Help=\"Время в секундах.\r\n                        Если = 0 - то команда выполняется сразу.\r\n                        Если &lt;&gt; 0 - то команда выполнится со случайным временем задержки, но не больше чем казано.\r\n                        Для имитации задержек клинта по вводу карты поставьте 180 (три минуты) \">\r\n        </Parameter>\r\n        </Group>\r\n    </Page>\r\n</Settings>";
		UnitName = SettDr.TypeDevice.Protocol;
		UnitDescription = "Эмулятор эк.терминала";
		UnitEquipmentType = "ЭквайринговыйТерминал";
		NameDevice = "Эмулятор эк.терминала";
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
			case "Action":
				Action = unitParamet.Value.Trim();
				break;
			case "WorkTime":
				WorkTime = unitParamet.Value.AsInt();
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
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		if (WorkTime != 0)
		{
			int num = new Random().Next(0, WorkTime);
			for (int i = 1; i <= 10; i++)
			{
				if (CancellationCommand)
				{
					throw new ArgumentException("157 Команда отменена", "original");
				}
				Unit.WindowTrackingStatus(DataCommand, this, "Выполнятся... " + (10 - i));
				Thread.Sleep(num / 10 * 1000);
			}
		}
		if (Action == "Error")
		{
			throw new ArgumentException("234 Ошибка связи с терминалом", "original");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Организация:<#0#>ООО Тестовая организация", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("ИНН:<#0#>123456789012", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Терминал:<#0#>21094544", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Мерчант:<#0#>781000055557", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		string text;
		switch (Command)
		{
		case 0:
			ReceiptNumber++;
			RezultCommand.CardNumber = "1254********6845";
			RezultCommand.ReceiptNumber = ReceiptNumber.ToString();
			RezultCommand.RRNCode = "5486" + ReceiptNumber + "5211";
			RezultCommand.AuthorizationCode = "783451" + ReceiptNumber + "4186418";
			RezultCommand.Amount = DataCommand.Amount;
			stringBuilder.Append(Unit.GetPringString(">#2#<ОПЛАТА", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			ReceiptNumber++;
			RezultCommand.CardNumber = "1254********6845";
			RezultCommand.ReceiptNumber = ReceiptNumber.ToString();
			RezultCommand.RRNCode = "5486" + ReceiptNumber + "5211";
			RezultCommand.AuthorizationCode = "783451" + ReceiptNumber + "4186418";
			RezultCommand.Amount = DataCommand.Amount;
			stringBuilder.Append(Unit.GetPringString(">#2#<ВОЗВРАТ ОПЛАТЫ", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			break;
		case 2:
		{
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			stringBuilder.Append(Unit.GetPringString(">#2#<ОТМЕНА ОПЛАТЫ", Width) + "\r\n");
			PayItem payItem3 = FindPay(0, DataCommand.AuthorizationCode, DataCommand.ReceiptNumber, DataCommand.RRNCode, DataCommand.CardNumber);
			if (payItem3 == null)
			{
				stringBuilder.Append(Unit.GetPringString("Ошибка:<#0#>Исходная транзакция не найдена", Width) + "\r\n");
				text = "Отказ";
				break;
			}
			if (payItem3.Amount != DataCommand.Amount)
			{
				stringBuilder.Append(Unit.GetPringString("Ошибка:<#0#>Суммы не совпадают", Width) + "\r\n");
				text = "Отказ";
				break;
			}
			RezultCommand.CardNumber = payItem3.CardNumber;
			RezultCommand.ReceiptNumber = payItem3.ReceiptNumber;
			RezultCommand.RRNCode = payItem3.RRNCode;
			RezultCommand.AuthorizationCode = payItem3.AuthorizationCode;
			RezultCommand.Amount = payItem3.Amount;
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			payItem3.Amount = default(decimal);
			break;
		}
		case 3:
			ReceiptNumber++;
			RezultCommand.CardNumber = "1254********6845";
			RezultCommand.ReceiptNumber = ReceiptNumber.ToString();
			RezultCommand.RRNCode = "5486" + ReceiptNumber + "5211";
			RezultCommand.AuthorizationCode = "783451" + ReceiptNumber + "4186418";
			RezultCommand.Amount = DataCommand.Amount;
			stringBuilder.Append(Unit.GetPringString(">#2#<ПРЕД-АВТОРИЗАЦИЯ", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			break;
		case 4:
		{
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			stringBuilder.Append(Unit.GetPringString(">#2#<ПОДТВЕРЖДЕНИЕ АВТОРИЗАЦИИ", Width) + "\r\n");
			PayItem payItem2 = FindPay(3, DataCommand.AuthorizationCode, DataCommand.ReceiptNumber, DataCommand.RRNCode, DataCommand.CardNumber);
			if (payItem2 == null)
			{
				stringBuilder.Append(Unit.GetPringString("Ошибка:<#0#>Исходная транзакция не найдена", Width) + "\r\n");
				text = "Отказ";
				break;
			}
			if (payItem2.Amount < DataCommand.Amount)
			{
				stringBuilder.Append(Unit.GetPringString("Ошибка:<#0#>НЕ хватает суммы в пред-авторизации", Width) + "\r\n");
				text = "Отказ";
				break;
			}
			RezultCommand.CardNumber = payItem2.CardNumber;
			RezultCommand.ReceiptNumber = payItem2.ReceiptNumber;
			RezultCommand.RRNCode = payItem2.RRNCode;
			RezultCommand.AuthorizationCode = payItem2.AuthorizationCode;
			RezultCommand.Amount = DataCommand.Amount;
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			payItem2.Amount -= DataCommand.Amount;
			break;
		}
		case 5:
		{
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			stringBuilder.Append(Unit.GetPringString(">#2#<ОТМЕНА АВТОРИЗАЦИИ", Width) + "\r\n");
			PayItem payItem = FindPay(3, DataCommand.AuthorizationCode, DataCommand.ReceiptNumber, DataCommand.RRNCode, DataCommand.CardNumber);
			if (payItem == null)
			{
				stringBuilder.Append(Unit.GetPringString("Ошибка:<#0#>Исходная транзакция не найдена", Width) + "\r\n");
				text = "Отказ";
				break;
			}
			RezultCommand.CardNumber = payItem.CardNumber;
			RezultCommand.ReceiptNumber = payItem.ReceiptNumber;
			RezultCommand.RRNCode = payItem.RRNCode;
			RezultCommand.AuthorizationCode = payItem.AuthorizationCode;
			RezultCommand.Amount = payItem.Amount;
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			payItem.Amount += DataCommand.Amount;
			break;
		}
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			new StringBuilder();
			return;
		}
		RezultCommand.Status = ExecuteStatus.Ok;
		if (Action == "Failure")
		{
			text = "Отказ авторизации";
			RezultCommand.CardNumber = "1254********6845";
			RezultCommand.ReceiptNumber = "";
			RezultCommand.RRNCode = "";
			RezultCommand.AuthorizationCode = "";
			RezultCommand.Amount = default(decimal);
			Error = "23 - Отказ авторизации";
			RezultCommand.Status = ExecuteStatus.Error;
		}
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Статус:<#0#>" + text, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Код авторизации:<#0#>" + RezultCommand.AuthorizationCode, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Номер ссылки:<#0#>" + RezultCommand.RRNCode, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Номер чека:<#0#>" + RezultCommand.ReceiptNumber, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		OldAmount = RezultCommand.Amount;
		OldAuthorizationCode = RezultCommand.AuthorizationCode;
		OldRRNCode = RezultCommand.RRNCode;
		OldReceiptNumber = RezultCommand.ReceiptNumber;
		OldCardNumber = RezultCommand.CardNumber;
		if (RezultCommand.Amount != 0m && (Command == 0 || Command == 3))
		{
			PayItem item = new PayItem
			{
				Command = Command,
				Amount = RezultCommand.Amount,
				AuthorizationCode = RezultCommand.AuthorizationCode,
				ReceiptNumber = RezultCommand.ReceiptNumber,
				RRNCode = RezultCommand.RRNCode,
				CardNumber = RezultCommand.CardNumber
			};
			ListPays.Add(item);
		}
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		RezultCommand.Slip = stringBuilder.ToString();
	}

	private PayItem FindPay(int Command, string AuthorizationCode, string ReceiptNumber, string RRNCode, string CardNumber)
	{
		foreach (PayItem listPay in ListPays)
		{
			if (listPay.Command == Command && listPay.AuthorizationCode == AuthorizationCode && listPay.ReceiptNumber == ReceiptNumber && listPay.RRNCode == RRNCode && listPay.CardNumber == CardNumber)
			{
				return listPay;
			}
		}
		return null;
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (Action == "Error")
		{
			throw new ArgumentException("234 Ошибка связи с терминалом", "original");
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Организация:<#0#>ООО Тестовая организация", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("ИНН:<#0#>123456789012", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Терминал:<#0#>21094544", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Мерчант:<#0#>781000055557", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString(">#2#<ОТМЕНА ОПЛАТЫ", Width) + "\r\n");
		PayItem payItem = FindPay(0, OldAuthorizationCode, OldReceiptNumber, OldRRNCode, OldCardNumber);
		string text;
		if (payItem == null)
		{
			stringBuilder.Append(Unit.GetPringString("Ошибка:<#0#>Исходная транзакция не найдена", Width) + "\r\n");
			text = "Отказ";
		}
		else
		{
			ReceiptNumber++;
			RezultCommand.CardNumber = OldCardNumber;
			RezultCommand.ReceiptNumber = OldReceiptNumber;
			RezultCommand.RRNCode = OldRRNCode;
			RezultCommand.AuthorizationCode = OldAuthorizationCode;
			RezultCommand.Amount = OldAmount;
			stringBuilder.Append(Unit.GetPringString("Карта:<#0#>Visa Credit", Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Номер:<#0#>" + RezultCommand.CardNumber, Width) + "\r\n");
			stringBuilder.Append(Unit.GetPringString("Сумма (руб):<#0#>" + RezultCommand.Amount, Width) + "\r\n");
			text = "Одобрено";
			payItem.Amount = default(decimal);
		}
		RezultCommand.Status = ExecuteStatus.Ok;
		if (Action == "Failure")
		{
			text = "Отказ авторизации";
			RezultCommand.CardNumber = "1254********6845";
			RezultCommand.ReceiptNumber = "";
			RezultCommand.RRNCode = "";
			RezultCommand.AuthorizationCode = "";
			RezultCommand.Amount = default(decimal);
			Error = "23 - Отказ авторизации";
			RezultCommand.Status = ExecuteStatus.Error;
		}
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Статус:<#0#>" + text, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Код авторизации:<#0#>" + RezultCommand.AuthorizationCode, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Номер ссылки:<#0#>" + RezultCommand.RRNCode, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Номер чека:<#0#>" + RezultCommand.ReceiptNumber, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		OldAmount = RezultCommand.Amount;
		OldAuthorizationCode = RezultCommand.AuthorizationCode;
		OldRRNCode = RezultCommand.RRNCode;
		OldReceiptNumber = RezultCommand.ReceiptNumber;
		OldCardNumber = RezultCommand.CardNumber;
		RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		RezultCommand.Slip = stringBuilder.ToString();
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (Action == "Error")
		{
			throw new ArgumentException("234 Ошибка связи с терминалом", "original");
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Организация:<#0#>ООО Тестовая организация", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("ИНН:<#0#>123456789012", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Терминал:<#0#>21094544", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Мерчант:<#0#>781000055557", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		string text = "Отчет закончен";
		RezultCommand.Status = ExecuteStatus.Ok;
		if (Action == "Failure")
		{
			text = "Отказ авторизации";
			Error = "23 - Отказ авторизации";
			RezultCommand.Status = ExecuteStatus.Error;
		}
		else
		{
			stringBuilder.Append(Unit.GetPringString("Итоги совпали", Width) + "\r\n");
		}
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Статус:<#0#>" + text, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		RezultCommand.Slip = stringBuilder.ToString();
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.Detailed)
		{
			_ = DataCommand.Detailed;
		}
		if (Action == "Error")
		{
			throw new ArgumentException("234 Ошибка связи с терминалом", "original");
		}
		Unit.WindowTrackingStatus(DataCommand, this, "Ожидание операции на терминале... ");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Организация:<#0#>ООО Тестовая организация", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("ИНН:<#0#>123456789012", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Терминал:<#0#>21094544", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Мерчант:<#0#>781000055557", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		string text = "Отчет закончен";
		RezultCommand.Status = ExecuteStatus.Ok;
		if (Action == "Failure")
		{
			text = "Отказ авторизации";
			Error = "23 - Отказ авторизации";
			RezultCommand.Status = ExecuteStatus.Error;
		}
		else if (!DataCommand.Detailed)
		{
			stringBuilder.Append(Unit.GetPringString("Краткий отчет", Width) + "\r\n");
		}
		else if (DataCommand.Detailed)
		{
			stringBuilder.Append(Unit.GetPringString("Полный отчет", Width) + "\r\n");
		}
		stringBuilder.Append(Unit.GetPringString("<<->>", Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("Статус:<#0#>" + text, Width) + "\r\n");
		stringBuilder.Append(Unit.GetPringString("<<=>>", Width) + "\r\n");
		RezultCommand.Slip = stringBuilder.ToString();
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
	}
}
