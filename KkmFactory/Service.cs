using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace KkmFactory;

public class Service : ServiceBase
{
	private IContainer components;

	public Service()
	{
		InitializeComponent();
	}

	protected override void OnStart(string[] args)
	{
		try
		{
			Task.Delay(1000).Wait();
			Global.UnitManager = new UnitManager();
			Global.LoadSettingAsyncs(InitLoad: true).Wait();
			if (Global.Settings.TypeRun != "Service")
			{
				Global.Settings.TypeRun = "Service";
				Global.SaveSettingsAsync().Wait();
			}
			new Task(delegate
			{
				Global.StartServer();
				Global.DateStart = DateTime.Now;
			}).Start();
		}
		catch (Exception ex)
		{
			Global.WriteError("Неудача запуска kkmserver как сервиса: " + ex.Message);
			throw new Exception("Неудача запуска kkmserver как сервиса.", ex);
		}
	}

	protected override void OnStop()
	{
		try
		{
			if (Global.AllCancellationToken != null)
			{
				Global.AllCancellationToken.Cancel();
			}
		}
		catch
		{
		}
		if (Global.UnitManager != null)
		{
			Global.UnitManager.FreeUnit();
			Global.StopServer();
		}
	}

	private void InitializeComponent()
	{
		components = new Container();
		base.ServiceName = Global.NameService;
		Global.DateStart = default(DateTime);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}
}
