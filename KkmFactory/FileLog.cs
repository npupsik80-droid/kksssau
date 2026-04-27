using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KkmFactory;

public class FileLog<T>
{
	public class Field
	{
		public DateTime DateLog;

		public T Value;
	}

	private DateTime DateLastLog = DateTime.Now;

	private SemaphoreSlim Semaphore = new SemaphoreSlim(1);

	private string FileName = "";

	public Dictionary<DateTime, string> CachData = new Dictionary<DateTime, string>();

	private StreamReader FileRead;

	public FileLog(string FileName)
	{
		this.FileName = FileName;
	}

	public async Task Add(T Log, DateTime DateLog)
	{
		await Semaphore.WaitAsync();
		try
		{
			if (DateLastLog >= DateLog)
			{
				DateLog = DateLastLog.AddMilliseconds(1.0);
			}
			DateLastLog = DateLog;
			string text = JsonConvert.SerializeObject(Log, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.Default,
				Formatting = Formatting.None
			});
			byte[] bytes = Encoding.Unicode.GetBytes(text);
			MemoryStream memoryStream = new MemoryStream();
			GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
			gZipStream.Write(bytes, 0, bytes.Length);
			gZipStream.Close();
			string value = Convert.ToBase64String(memoryStream.ToArray());
			CachData.Add(DateLog, value);
		}
		finally
		{
			Semaphore.Release();
		}
	}

	public void Save()
	{
		if (CachData.Count == 0)
		{
			return;
		}
		try
		{
			ClouseNext();
			string text = Global.GerPahtSettings();
			string text2 = "Temp.dta";
			string text3 = Path.Combine(text, FileName);
			string text4 = Path.Combine(text, text2);
			if (File.Exists(text4))
			{
				try
				{
					File.Delete(text4);
				}
				catch
				{
				}
			}
			if (!File.Exists(text3))
			{
				File.WriteAllText(text3, "");
			}
			using (StreamWriter streamWriter = new StreamWriter(File.Open(text4, FileMode.Create)))
			{
				Semaphore.Wait();
				try
				{
					foreach (KeyValuePair<DateTime, string> item in CachData.Reverse())
					{
						streamWriter.Write(item.Key.ToString("yyyy.MM.dd HH:mm:ss:fff"));
						streamWriter.Write("=");
						streamWriter.Write(item.Value);
						streamWriter.Write("!===");
						streamWriter.Write("\r\n");
					}
					CachData.Clear();
				}
				finally
				{
					Semaphore.Release();
				}
				using (StreamReader streamReader = new StreamReader(text3))
				{
					int num = 10000000;
					while (num > 0 && !streamReader.EndOfStream)
					{
						string text5 = streamReader.ReadLine();
						num -= text5.Length;
						streamWriter.WriteLine(text5);
					}
				}
				streamWriter.Close();
			}
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			if (File.Exists(text4))
			{
				File.Move(text4, text3);
			}
			if (File.Exists(text4))
			{
				File.Delete(text4);
			}
		}
		catch
		{
		}
	}

	public async Task<Field> GetNext()
	{
		if (FileRead == null)
		{
			string text = Path.Combine(Global.GerPahtSettings(), FileName);
			FileRead = new StreamReader(text);
		}
		if (!FileRead.EndOfStream)
		{
			Field Field = new Field();
			try
			{
				StringBuilder stringBuilder = new StringBuilder(await FileRead.ReadLineAsync());
				string s = stringBuilder.ToString(0, 23);
				Field.DateLog = DateTime.ParseExact(s, "yyyy.MM.dd HH:mm:ss:fff", CultureInfo.InvariantCulture);
				if (stringBuilder.Length < 25)
				{
					return null;
				}
				stringBuilder.Remove(0, 24);
				stringBuilder.Remove(stringBuilder.Length - 6, 4);
				byte[] array = Convert.FromBase64String(stringBuilder.ToString());
				MemoryStream memoryStream = new MemoryStream();
				memoryStream.Write(array, 0, array.Length);
				memoryStream.Position = 0L;
				GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
				byte[] array2 = new byte[0];
				int num = 0;
				int num2;
				do
				{
					Array.Resize(ref array2, array2.Length + 10000);
					num2 = gZipStream.Read(array2, num, 10000);
					num += num2;
				}
				while (num2 > 0);
				gZipStream.Close();
				string value = Encoding.Unicode.GetString(array2, 0, num);
				Field.Value = JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.Default
				});
			}
			catch (Exception)
			{
				ClouseNext();
				return null;
			}
			return Field;
		}
		return null;
	}

	public void ClouseNext()
	{
		if (FileRead != null)
		{
			FileRead.Close();
			FileRead = null;
		}
	}
}
