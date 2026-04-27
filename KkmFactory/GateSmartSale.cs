using System;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KkmFactory;

internal class GateSmartSale : Unit
{
	private class Message
	{
		public string _00_Amount;

		public string _01_AdditionalAmount;

		public string _04_CurrencyCode;

		public DateTime? _06_DateTimeHost;

		public int? _08_CardEntryMode;

		public string _10_PAN;

		public string _13_AuthorizationCode;

		public string _14_ReferenceNumber;

		public string _15_ResponseCodeHost;

		public string _19_TextResponse;

		public DateTime? _21_TerminalDateTime;

		public int? _23_CardEntryMode;

		public int? _25_OperationCode;

		public int? _26_TerminalTrxID;

		public string _27_TerminalID;

		public string _28_MerchantID;

		public int? _39_Status;

		public int? _64_CommandMode;

		public int? _65_CommandMode2;

		public int? _67_CommandResult;

		public string _70_FileData;

		public string _76_ReceiptData;

		public string _80_CommodityCode;

		public string _86_AdditionalData;

		public string _89_ModelNo;

		public string _90_ReceiptData;

		public int? _51_KktResult;

		public int? _52_SlipNumber;

		public int? _108_SlipNumber;
	}

	private bool DllIsLosd;

	private readonly Encoding CurEncoding = Encoding.GetEncoding(1251);

	private const string ModelNo = "EFT:SW:KkmServer.ru;2.2;;SA:;2;";

	private readonly int Timeout1 = 5000;

	private readonly GateINPAS GateINPAS;

	public GateSmartSale(Global.DeviceSettings SettDr, int NumUnit, GateINPAS GateINPAS)
		: base(SettDr, NumUnit)
	{
		this.GateINPAS = GateINPAS;
	}

	public override async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		Message message = new Message();
		message._00_Amount = ((ulong)(DataCommand.Amount * 100m)).ToString();
		message._04_CurrencyCode = "643";
		switch (Command)
		{
		case 0:
			message._25_OperationCode = 1;
			break;
		case 1:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			message._25_OperationCode = 29;
			message._14_ReferenceNumber = DataCommand.RRNCode;
			message._13_AuthorizationCode = DataCommand.AuthorizationCode;
			try
			{
				message._108_SlipNumber = int.Parse(DataCommand.ReceiptNumber);
			}
			catch
			{
			}
			try
			{
				message._52_SlipNumber = int.Parse(DataCommand.ReceiptNumber);
			}
			catch
			{
			}
			break;
		case 2:
			SetDictFromString(DataCommand.UniversalID, DataCommand);
			message._25_OperationCode = 4;
			message._14_ReferenceNumber = DataCommand.RRNCode;
			message._13_AuthorizationCode = DataCommand.AuthorizationCode;
			try
			{
				message._108_SlipNumber = int.Parse(DataCommand.ReceiptNumber);
			}
			catch
			{
			}
			try
			{
				message._52_SlipNumber = int.Parse(DataCommand.ReceiptNumber);
			}
			catch
			{
			}
			break;
		default:
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Драйвер не поддерживает эту команду";
			return;
		}
		message._27_TerminalID = GateINPAS.TerminalID;
		message._89_ModelNo = "EFT:SW:KkmServer.ru;2.2;;SA:;2;";
		Message message2 = await RunCommand(message, DataCommand);
		try
		{
			RezultCommand.Slip = GetSlip(message2);
		}
		catch
		{
		}
		if (!IsCommandBad(RezultCommand, message2._39_Status, message2._19_TextResponse))
		{
			RezultCommand.CardNumber = message2._10_PAN;
			if (message2._108_SlipNumber.HasValue)
			{
				RezultCommand.ReceiptNumber = message2._108_SlipNumber.ToString();
			}
			else if (message2._52_SlipNumber.HasValue)
			{
				RezultCommand.ReceiptNumber = message2._52_SlipNumber.ToString();
			}
			RezultCommand.RRNCode = message2._14_ReferenceNumber;
			RezultCommand.AuthorizationCode = message2._13_AuthorizationCode;
			RezultCommand.Amount = decimal.Parse(message2._00_Amount) / 100m;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
			RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		}
	}

	public override async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Message message = new Message();
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		message._00_Amount = ((ulong)(DataCommand.Amount * 100m)).ToString();
		message._04_CurrencyCode = "643";
		message._25_OperationCode = 53;
		message._14_ReferenceNumber = DataCommand.RRNCode;
		message._13_AuthorizationCode = DataCommand.AuthorizationCode;
		message._27_TerminalID = GateINPAS.TerminalID;
		message._89_ModelNo = "EFT:SW:KkmServer.ru;2.2;;SA:;2;";
		Message message2 = await RunCommand(message, DataCommand);
		try
		{
			RezultCommand.Slip = GetSlip(message2);
		}
		catch
		{
		}
		if (!IsCommandBad(RezultCommand, message2._39_Status, message2._19_TextResponse))
		{
			RezultCommand.CardNumber = message2._10_PAN;
			RezultCommand.ReceiptNumber = message2._80_CommodityCode;
			RezultCommand.RRNCode = message2._14_ReferenceNumber;
			RezultCommand.AuthorizationCode = message2._13_AuthorizationCode;
			RezultCommand.Amount = decimal.Parse(message2._00_Amount) / 100m;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
			RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		}
	}

	public override async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		Message message = new Message();
		message._25_OperationCode = 59;
		message._27_TerminalID = GateINPAS.TerminalID;
		message._89_ModelNo = "EFT:SW:KkmServer.ru;2.2;;SA:;2;";
		Message message2 = await RunCommand(message, DataCommand);
		try
		{
			RezultCommand.Slip = GetSlip(message2, Del2Slip: false);
		}
		catch
		{
		}
		if (!IsCommandBad(RezultCommand, message2._39_Status, message2._19_TextResponse))
		{
			RezultCommand.CardNumber = "";
			RezultCommand.ReceiptNumber = message2._80_CommodityCode;
			RezultCommand.RRNCode = "";
			RezultCommand.AuthorizationCode = "";
			RezultCommand.Amount = default(decimal);
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
		}
	}

	public override async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		Message message = new Message();
		message._25_OperationCode = 63;
		message._27_TerminalID = GateINPAS.TerminalID;
		if (!DataCommand.Detailed)
		{
			message._65_CommandMode2 = 20;
		}
		else if (DataCommand.Detailed)
		{
			message._65_CommandMode2 = 21;
		}
		message._89_ModelNo = "EFT:SW:KkmServer.ru;2.2;;SA:;2;";
		Message message2 = await RunCommand(message, DataCommand);
		try
		{
			RezultCommand.Slip = GetSlip(message2, Del2Slip: false);
		}
		catch
		{
		}
		if (!IsCommandBad(RezultCommand, message2._39_Status, message2._19_TextResponse))
		{
			RezultCommand.CardNumber = "";
			RezultCommand.ReceiptNumber = message2._80_CommodityCode;
			RezultCommand.RRNCode = "";
			RezultCommand.AuthorizationCode = "";
			RezultCommand.Amount = default(decimal);
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
		}
	}

	public override async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		Unit.WindowTrackingStatus(DataCommand, GateINPAS, "Ожидание операции на терминале... ");
		Message message = new Message();
		SetDictFromString(DataCommand.UniversalID, DataCommand);
		message._25_OperationCode = 63;
		message._65_CommandMode2 = 22;
		message._14_ReferenceNumber = DataCommand.RRNCode;
		message._13_AuthorizationCode = DataCommand.AuthorizationCode;
		try
		{
			message._108_SlipNumber = int.Parse(DataCommand.ReceiptNumber);
		}
		catch
		{
		}
		try
		{
			message._52_SlipNumber = int.Parse(DataCommand.ReceiptNumber);
		}
		catch
		{
		}
		message._27_TerminalID = GateINPAS.TerminalID;
		message._89_ModelNo = "EFT:SW:KkmServer.ru;2.2;;SA:;2;";
		Message message2 = await RunCommand(message, DataCommand);
		try
		{
			RezultCommand.Slip = GetSlip(message2);
		}
		catch
		{
		}
		if (!IsCommandBad(RezultCommand, message2._39_Status, message2._19_TextResponse))
		{
			RezultCommand.CardNumber = message2._10_PAN;
			RezultCommand.ReceiptNumber = message2._80_CommodityCode;
			RezultCommand.RRNCode = message2._14_ReferenceNumber;
			RezultCommand.AuthorizationCode = message2._13_AuthorizationCode;
			RezultCommand.Amount = decimal.Parse(message2._00_Amount) / 100m;
			RezultCommand.Status = ExecuteStatus.Ok;
			RezultCommand.Error = "";
			RezultCommand.UniversalID = GetStringFromDict(RezultCommand);
		}
	}

	public override async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.PrintSlipOnTerminal = false;
		RezultCommand.Status = ExecuteStatus.Ok;
		RezultCommand.Error = "Драйвер не поддерживает эту команду";
	}

	private string GetSlip(Message response, bool Del2Slip = true)
	{
		if (response != null && response._90_ReceiptData != null)
		{
			string text = response._90_ReceiptData;
			if (text.IndexOf("0xDF") == 0 && text.Length >= 5)
			{
				text = text.Substring(4);
			}
			if (text.IndexOf("^") == 0 && text.Length >= 2)
			{
				text = text.Substring(1);
			}
			if (text.IndexOf("^") == 0 && text.Length >= 2)
			{
				text = text.Substring(1);
			}
			if (text.IndexOf("^") == 0 && text.Length >= 2)
			{
				text = text.Substring(1);
			}
			if (!Del2Slip)
			{
				text = text.Replace("~0xDA^^", "\r\n");
			}
			if (text.IndexOf("~") != -1)
			{
				text = text.Substring(0, text.IndexOf("~") - 1);
			}
			return text;
		}
		return "";
	}

	public override void Test()
	{
	}

	private async Task<Message> RunCommand(Message query, DataCommand DataCommand, int Timeout = 45000)
	{
		Message CurResponce = null;
		TcpClient TcpClient = null;
		NetworkStream TcpStream = null;
		int ColInf = 0;
		bool OpenSerial = await GateINPAS.PortOpenAsync();
		if (Error != "")
		{
			string error = GateINPAS.Error;
			GateINPAS.Error = "";
			throw new Exception("Ошибка установки связи с терминалом: " + error);
		}
		GateINPAS.PortWriteTimeout = Timeout1;
		try
		{
			int num;
			_ = num - 1;
			_ = 5;
			try
			{
				await SendFrame(query);
				while (true)
				{
					CurResponce = await GetFrame(Timeout);
					if (CurResponce._25_OperationCode == query._25_OperationCode && CurResponce._39_Status.HasValue)
					{
						break;
					}
					if (CurResponce._25_OperationCode == 21)
					{
						continue;
					}
					if (CurResponce._25_OperationCode == 52)
					{
						if (query._76_ReceiptData != null && query._76_ReceiptData != "")
						{
							string[] array = query._76_ReceiptData.Split(new char[1] { '^' }, StringSplitOptions.None);
							if (array.Length >= 4 && array[4] != null && array[4] != "")
							{
								GateINPAS gateINPAS = GateINPAS;
								int num2 = ColInf + 1;
								ColInf = num2;
								Unit.WindowTrackingStatus(DataCommand, gateINPAS, num2 + ": " + array[4] + "...");
							}
						}
					}
					else if (CurResponce._25_OperationCode == 63 && CurResponce._65_CommandMode2 == 16 && CurResponce._64_CommandMode == 1)
					{
						Message message = new Message();
						message._25_OperationCode = 63;
						message._65_CommandMode2 = 16;
						try
						{
							string[] array2 = CurResponce._70_FileData.Split(new char[1] { ';' }, StringSplitOptions.None);
							TcpClient = new TcpClient(array2[0].Trim(), int.Parse(array2[1].Trim()));
							TcpStream = TcpClient.GetStream();
							message._67_CommandResult = 0;
							GateINPAS.PortLogs.Append("Открыто соединение к хосту: " + array2[0] + ":" + array2[1].Trim(), "<-");
						}
						catch (Exception ex)
						{
							TcpClient.Dispose();
							TcpClient = null;
							message._67_CommandResult = 1;
							GateINPAS.PortLogs.Append("Ошибка открытия канала к банку: " + ex.Message);
						}
						await SendFrame(message);
					}
					else if (CurResponce._25_OperationCode == 63 && CurResponce._65_CommandMode2 == 16 && CurResponce._64_CommandMode == 0)
					{
						try
						{
							TcpClient.Close();
							GateINPAS.PortLogs.Append("Закрыто соединение к хосту", "<-");
						}
						catch (Exception ex2)
						{
							GateINPAS.PortLogs.Append("Ошибка закрытия канала к банку: " + ex2.Message);
						}
						TcpClient.Dispose();
						TcpClient = null;
						Message message = new Message();
						message._25_OperationCode = 63;
						message._65_CommandMode2 = 16;
						message._67_CommandResult = 0;
						await SendFrame(message);
					}
					else if (CurResponce._25_OperationCode == 63 && CurResponce._65_CommandMode2 == 17 && CurResponce._64_CommandMode == 0)
					{
						Message message = new Message();
						message._25_OperationCode = 63;
						message._65_CommandMode2 = 17;
						try
						{
							byte[] bytes = CurEncoding.GetBytes(CurResponce._70_FileData);
							TcpStream.Write(bytes, 0, bytes.Length);
							TcpStream.Flush();
							message._67_CommandResult = 0;
							GateINPAS.PortLogs.Append("Передача хосту: " + bytes.Length + " byte", "<-");
						}
						catch (Exception ex3)
						{
							message._67_CommandResult = 1;
							GateINPAS.PortLogs.Append("Ошибка передачи данных банку: " + ex3.Message);
						}
						await SendFrame(message);
					}
					else
					{
						if (CurResponce._25_OperationCode != 63 || CurResponce._65_CommandMode2 != 17 || CurResponce._64_CommandMode != 1)
						{
							continue;
						}
						Message message = new Message();
						message._25_OperationCode = 63;
						message._65_CommandMode2 = 17;
						try
						{
							TcpClient.ReceiveTimeout = 1000;
							TcpStream.ReadTimeout = 1000;
							byte[] array3 = new byte[100000];
							int num3 = -1;
							while (true)
							{
								try
								{
									if (num3 >= 100000 || TcpStream.Read(array3, num3 + 1, 1) != 1)
									{
										break;
									}
									num3++;
									continue;
								}
								catch
								{
								}
								break;
							}
							if (num3 != -1)
							{
								message._70_FileData = CurEncoding.GetString(array3, 0, num3 + 1);
								message._67_CommandResult = 0;
							}
							else
							{
								message._70_FileData = "";
								message._67_CommandResult = 1;
							}
						}
						catch (Exception ex4)
						{
							message._67_CommandResult = 1;
							GateINPAS.PortLogs.Append("Ошибка приема данных от банка: " + ex4.Message);
						}
						GateINPAS.PortLogs.Append("Принято от хоста: " + message._70_FileData.Length + " byte", "->");
						await SendFrame(message);
					}
				}
				if (CurResponce._39_Status != 1)
				{
					if (CurResponce._39_Status == 0 && (query._25_OperationCode == 59 || (query._25_OperationCode == 63 && (query._65_CommandMode2 == 20 || query._65_CommandMode2 == 21 || query._65_CommandMode2 == 22))))
					{
						throw new Exception("Команда не поддерживается Вашим терминалом: " + CurResponce._19_TextResponse);
					}
					throw new Exception("Ошибка операции: " + CurResponce._19_TextResponse);
				}
			}
			catch (Exception ex5)
			{
				throw new Exception(Global.GetInnerErrorMessagee(ex5));
			}
		}
		finally
		{
			if (OpenSerial)
			{
				await GateINPAS.PortCloseAsync();
			}
			if (TcpClient != null)
			{
				TcpClient.Close();
				TcpClient.Dispose();
			}
		}
		return CurResponce;
	}

	private async Task SendFrame(Message query)
	{
		byte[] array = new byte[0];
		Type type = query.GetType();
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			string s = fieldInfo.Name.Substring(1, 3).Replace("_", "");
			int result = 0;
			if (!int.TryParse(s, out result))
			{
				continue;
			}
			object value = type.GetField(fieldInfo.Name).GetValue(query);
			if (value != null && value.GetType() == typeof(string))
			{
				array = SetAddBuf(array, SetTeg(result, (string)value));
				if (result == 70 || result == 90)
				{
					GateINPAS.PortLogs.Append($"[{result.ToString()}] = '{((string)value).Length} byte'", "<");
				}
				else
				{
					GateINPAS.PortLogs.Append($"[{result.ToString()}] = '{(string)value}'", "<");
				}
			}
			else if (value != null && value.GetType() == typeof(int))
			{
				array = SetAddBuf(array, SetTeg(result, ((int)value).ToString()));
				GateINPAS.PortLogs.Append($"[{result.ToString()}] = '{((int)value).ToString()}'", "<");
			}
			else if (value != null && value.GetType() == typeof(DateTime))
			{
				array = SetAddBuf(array, SetTeg(result, (DateTime)value));
				GateINPAS.PortLogs.Append(string.Format("[{0}] = '{1}'", result.ToString(), ((DateTime)value).ToString("yyyy.MM.dd:HH.mm.ss")), "<");
			}
		}
		byte[] data = new byte[0];
		data = SetAddBuf(data, 2);
		short num = (short)array.Length;
		data = SetAddBuf(data, (byte)(num & 0xFF));
		data = SetAddBuf(data, (byte)((num & 0xFF00) >> 8));
		data = SetAddBuf(data, array);
		long num2 = data.LongLength;
		int num3 = 0;
		ushort num4 = 0;
		while (num2 > 0)
		{
			byte b = data[num3];
			for (int j = 0; j < 8; j++)
			{
				int num5 = ((((b & 0x80) <= 0 || (num4 & 0x8000) <= 0) && ((b & 0x80) != 0 || (num4 & 0x8000) != 0)) ? 1 : 0);
				int num6 = (((num5 <= 0 || (num4 & 0x4000) <= 0) && (num5 != 0 || (num4 & 0x4000) != 0)) ? 1 : 0);
				int num7 = (((num5 <= 0 || (num4 & 2) <= 0) && (num5 != 0 || (num4 & 2) != 0)) ? 1 : 0);
				num4 <<= 1;
				b <<= 1;
				num4 = (ushort)((uint)num4 | ((num5 > 0) ? 1u : 0u));
				num4 = ((num7 > 0) ? ((ushort)(num4 | 4)) : ((ushort)(num4 & 0xFFFB)));
				num4 = ((num6 > 0) ? ((ushort)(num4 | 0x8000)) : ((ushort)(num4 & 0x7FFF)));
			}
			num2--;
			num3++;
		}
		num4 = (ushort)((num4 << 8) + (num4 >> 8));
		data = SetAddBuf(data, (byte)(num4 & 0xFF));
		data = SetAddBuf(data, (byte)((num4 & 0xFF00) >> 8));
		await GateINPAS.PortWriteAsync(data, 0, data.Length);
		GateINPAS.PortReadTimeout = Timeout1;
		int NumTry = 0;
		do
		{
			byte b2;
			try
			{
				b2 = await GateINPAS.PortReadByteAsync();
				GateINPAS.PortLogs.Write(new byte[0]);
			}
			catch (Exception)
			{
				throw new Exception("Ошибка приема кадра сообщения (1)");
			}
			switch (b2)
			{
			case 21:
				continue;
			case 6:
				return;
			}
			throw new Exception("Ошибка приема кадра сообщения (3)");
		}
		while (NumTry++ <= 3);
		await GateINPAS.PortWriteByteAsync(4);
		throw new Exception("Ошибка приема кадра сообщения (2)");
	}

	private async Task<Message> GetFrame(int Timeout)
	{
		Message Rez = new Message();
		byte[] OldMessResponce = new byte[0];
		int NumTry = 0;
		byte[] Rez2;
		int Pos;
		while (true)
		{
			IL_0046:
			GateINPAS.PortReadTimeout = Timeout;
			byte[] Responce = new byte[0];
			int CountRepit = 30;
			while (true)
			{
				byte Response;
				try
				{
					Response = await GateINPAS.PortReadByteAsync();
				}
				catch (Exception)
				{
					await GateINPAS.PortWriteByteAsync(4);
					throw new Exception("Ошибка приема кадра сообщения (4)");
				}
				if (Responce.Length == 0 && Response != 2 && Response != 1)
				{
					if (CountRepit-- == 0)
					{
						throw new Exception("Ошибка приема кадра сообщения (20)");
					}
					continue;
				}
				Responce = SetAddBuf(Responce, Response);
				if (Responce.Length >= 3)
				{
					int num = Responce[1] + (Responce[2] << 8);
					if (Responce.Length >= num + 5)
					{
						break;
					}
				}
			}
			long num2 = Responce.LongLength - 2;
			int num3 = 0;
			ushort num4 = 0;
			while (num2 > 0)
			{
				byte b = Responce[num3];
				for (int i = 0; i < 8; i++)
				{
					int num5 = ((((b & 0x80) <= 0 || (num4 & 0x8000) <= 0) && ((b & 0x80) != 0 || (num4 & 0x8000) != 0)) ? 1 : 0);
					int num6 = (((num5 <= 0 || (num4 & 0x4000) <= 0) && (num5 != 0 || (num4 & 0x4000) != 0)) ? 1 : 0);
					int num7 = (((num5 <= 0 || (num4 & 2) <= 0) && (num5 != 0 || (num4 & 2) != 0)) ? 1 : 0);
					num4 <<= 1;
					b <<= 1;
					num4 = (ushort)((uint)num4 | ((num5 > 0) ? 1u : 0u));
					num4 = ((num7 > 0) ? ((ushort)(num4 | 4)) : ((ushort)(num4 & 0xFFFB)));
					num4 = ((num6 > 0) ? ((ushort)(num4 | 0x8000)) : ((ushort)(num4 & 0x7FFF)));
				}
				num2--;
				num3++;
			}
			num4 = (ushort)((num4 << 8) + (num4 >> 8));
			if (Responce[Responce.LongLength - 2] != (byte)(num4 & 0xFF) || Responce[Responce.LongLength - 1] != (byte)((num4 & 0xFF00) >> 8))
			{
				if (NumTry++ > 3)
				{
					await GateINPAS.PortWriteByteAsync(4);
					throw new Exception("Ошибка приема кадра сообщения (5)");
				}
				await GateINPAS.PortWriteByteAsync(21);
				continue;
			}
			await GateINPAS.PortWriteByteAsync(6);
			Rez2 = null;
			Pos = 1;
			if (Responce[0] == 2)
			{
				GetTegBuf(Responce, ref Pos, out Rez2);
				Pos = 0;
				break;
			}
			if (Responce[0] == 1)
			{
				byte[] Rez3 = null;
				GetTegBuf(Responce, ref Pos, out Rez2);
				Pos = 0;
				GetTegBuf(Rez2, ref Pos, out Rez3);
				byte[] Rez4 = null;
				int Pos2 = 0;
				int Field = 0;
				int num8 = OldMessResponce.Length;
				byte[] array = new byte[num8 + Rez2.Length - Pos];
				Array.Copy(OldMessResponce, 0, array, 0, OldMessResponce.Length);
				Array.Copy(Rez2, Pos, array, num8, Rez2.Length - Pos);
				OldMessResponce = array;
				while (GetTegNext(Rez3, ref Pos2, out Field, out Rez4))
				{
					if (Field == 2 && int.Parse(CurEncoding.GetString(Rez4)) == 0)
					{
						Rez2 = OldMessResponce;
						Pos = 0;
					}
					else if (Field == 2 && int.Parse(CurEncoding.GetString(Rez4)) != 0)
					{
						goto IL_0046;
					}
				}
				break;
			}
			throw new Exception("Ошибка приема кадра сообщения (6)");
		}
		int Field2 = 0;
		byte[] Rez5 = null;
		while (GetTegNext(Rez2, ref Pos, out Field2, out Rez5))
		{
			string value = ((Field2 >= 100) ? ("_" + Field2.ToString("000") + "_") : ("_" + Field2.ToString("00") + "_"));
			Type type = Rez.GetType();
			FieldInfo[] fields = type.GetFields();
			bool flag = false;
			FieldInfo[] array2 = fields;
			foreach (FieldInfo fieldInfo in array2)
			{
				if (fieldInfo.Name.IndexOf(value) != 0)
				{
					continue;
				}
				FieldInfo field = type.GetField(fieldInfo.Name);
				if (field.FieldType == typeof(string))
				{
					string text = CurEncoding.GetString(Rez5);
					string text2 = (string)field.GetValue(Rez);
					if (text2 != null)
					{
						_ = text2 + text;
					}
					field.SetValue(Rez, text);
					if (Field2 == 70 || Field2 == 90)
					{
						GateINPAS.PortLogs.Append($"[{Field2.ToString()}] = '{text.Length} byte'");
					}
					else
					{
						GateINPAS.PortLogs.Append($"[{Field2.ToString()}] = '{text}'");
					}
					flag = true;
				}
				else if (field.FieldType == typeof(int?))
				{
					int num9 = int.Parse(CurEncoding.GetString(Rez5));
					field.SetValue(Rez, num9);
					GateINPAS.PortLogs.Append($"[{Field2.ToString()}] = '{num9.ToString()}'");
					flag = true;
				}
				else if (field.FieldType == typeof(DateTime?))
				{
					DateTime dateTime = DateTime.ParseExact(CurEncoding.GetString(Rez5), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
					field.SetValue(Rez, dateTime);
					GateINPAS.PortLogs.Append(string.Format("[{0}] = '{1}'", Field2.ToString(), dateTime.ToString("yyyy.MM.dd:HH.mm.ss")));
					flag = true;
				}
			}
			if (!flag)
			{
				GateINPAS.PortLogs.Append($"[{Field2.ToString()}]! = '{CurEncoding.GetString(Rez5)}'");
			}
		}
		return Rez;
	}

	private byte[] SetTeg(int Field, DateTime Val)
	{
		string val = Val.ToString("yyyyMMddHHmmss");
		return SetTeg(Field, val);
	}

	private byte[] SetTeg(int Field, string Val)
	{
		byte[] bytes = CurEncoding.GetBytes(Val);
		return SetTeg(Field, bytes);
	}

	private byte[] SetTeg(int Field, byte[] Val)
	{
		byte[] array = new byte[3 + Val.Length];
		array[0] = (byte)Field;
		short num = (short)Val.Length;
		array[1] = (byte)(num & 0xFF);
		array[2] = (byte)((num & 0xFF00) >> 8);
		Array.Copy(Val, 0, array, 3, Val.Length);
		return array;
	}

	private byte[] SetAddBuf(byte[] Data, byte[] Val)
	{
		byte[] array = new byte[Data.Length + Val.Length];
		Array.Copy(Data, 0, array, 0, Data.Length);
		Array.Copy(Val, 0, array, Data.Length, Val.Length);
		return array;
	}

	private byte[] SetAddBuf(byte[] Data, byte Val)
	{
		byte[] array = new byte[Data.Length + 1];
		Array.Copy(Data, 0, array, 0, Data.Length);
		array[Data.Length] = Val;
		return array;
	}

	private bool GetTegBuf(byte[] Data, ref int Pos, out byte[] Rez)
	{
		Rez = new byte[0];
		if (Pos + 2 >= Data.Length)
		{
			return false;
		}
		short num = (short)(Data[Pos] + (Data[Pos + 1] << 8));
		if (Pos + 2 + num > Data.Length)
		{
			return false;
		}
		Rez = new byte[num];
		Array.Copy(Data, Pos + 2, Rez, 0, num);
		Pos = Pos + 2 + num;
		return true;
	}

	private bool GetTegNext(byte[] Data, ref int Pos, out int Field, out byte[] Rez)
	{
		Field = 0;
		Rez = new byte[0];
		if (Pos + 2 >= Data.Length)
		{
			return false;
		}
		Field = Data[Pos];
		Pos++;
		return GetTegBuf(Data, ref Pos, out Rez);
	}

	private bool IsCommandBad(RezultCommand RezultCommand, int? StatusByte = 1, string ErrorText = "")
	{
		if (GateINPAS.Error != "")
		{
			if (DllIsLosd)
			{
				DllIsLosd = false;
			}
			if (RezultCommand != null)
			{
				RezultCommand.Status = ExecuteStatus.Error;
				if (ErrorText != "")
				{
					GateINPAS.Error = ErrorText + " (" + GateINPAS.Error + ")";
				}
			}
			return true;
		}
		return false;
	}
}
