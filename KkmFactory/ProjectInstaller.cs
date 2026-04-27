using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace KkmFactory;

[RunInstaller(true)]
public class ProjectInstaller : Installer
{
	private IContainer components;

	private ServiceInstaller serviceInstaller;

	public ProjectInstaller()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		serviceInstaller = new ServiceInstaller();
		ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
		serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
		serviceProcessInstaller.Password = null;
		serviceProcessInstaller.Username = null;
		serviceInstaller.ServiceName = Global.NameService;
		serviceInstaller.Description = Global.Product;
		serviceInstaller.StartType = ServiceStartMode.Automatic;
		base.Installers.AddRange(new Installer[2] { serviceProcessInstaller, serviceInstaller });
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
