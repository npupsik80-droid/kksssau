using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using CompiledAvaloniaXaml;
using KkmFactory.ViewModels;
using KkmFactory.Views;

namespace KkmFactory;

public class App : Application
{
	private static Action<object> _0021XamlIlPopulateOverride;

	public override void Initialize()
	{
		_0021XamlIlPopulateTrampoline(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (Global.ViewModel == null)
		{
			Global.ViewModel = new ViewModel();
		}
		else
		{
			Global.ViewModel.SetVisible();
		}
		base.DataContext = Global.ViewModel;
		if (base.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime)
		{
			MainWindow mainWindow = (MainWindow)(classicDesktopStyleApplicationLifetime.MainWindow = new MainWindow
			{
				DataContext = Global.ViewModel
			});
			Global.ViewModel.Window = mainWindow;
			classicDesktopStyleApplicationLifetime.MainWindow.Closing += delegate(object? s, WindowClosingEventArgs e)
			{
				Global.ViewModel.EventClosing(s, e);
			};
			classicDesktopStyleApplicationLifetime.MainWindow.Loaded += delegate(object? s, RoutedEventArgs e)
			{
				Global.ViewModel.WindowStart(s, e);
			};
			Global.MainForm = mainWindow;
		}
		else if (base.ApplicationLifetime is ISingleViewApplicationLifetime singleViewApplicationLifetime)
		{
			MainView mainView = new MainView
			{
				DataContext = Global.ViewModel
			};
			singleViewApplicationLifetime.MainView = mainView;
		}
		Global.ViewModel.SetVisible();
		base.OnFrameworkInitializationCompleted();
	}

	static void _0021XamlIlPopulate(IServiceProvider P_0, App P_1)
	{
		CompiledAvaloniaXaml.XamlIlContext.Context<App> context = new CompiledAvaloniaXaml.XamlIlContext.Context<App>(P_0, new object[1] { _0021AvaloniaResources.NamespaceInfo_003A_002FForm_002FApp_002Eaxaml.Singleton }, "avares://kkmserver/Form/App.axaml");
		context.RootObject = P_1;
		context.IntermediateRoot = P_1;
		App app2;
		App app = (app2 = P_1);
		context.PushParent(app2);
		app2.RequestedThemeVariant = ThemeVariant.Default;
		app2.Styles.Add(new FluentTheme(context));
		TrayIcons trayIcons2;
		TrayIcons trayIcons = (trayIcons2 = new TrayIcons());
		context.PushParent(trayIcons2);
		TrayIcon trayIcon2;
		TrayIcon trayIcon = (trayIcon2 = new TrayIcon());
		context.PushParent(trayIcon2);
		trayIcon2.Icon = (WindowIcon)new IconTypeConverter().ConvertFrom(context, CultureInfo.InvariantCulture, "/Assets/favicon.ico");
		trayIcon2.ToolTipText = "Kkm web-server";
		StyledProperty<ICommand?> commandProperty = TrayIcon.CommandProperty;
		CompiledBindingExtension compiledBindingExtension = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Tray_DoubleClick", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BTray_DoubleClick_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = TrayIcon.CommandProperty;
		CompiledBindingExtension compiledBindingExtension2 = compiledBindingExtension.ProvideValue(context);
		context.ProvideTargetProperty = null;
		trayIcon2.Bind(commandProperty, (IBinding)compiledBindingExtension2);
		StyledProperty<bool> isVisibleProperty = TrayIcon.IsVisibleProperty;
		CompiledBindingExtension compiledBindingExtension3 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002ETrayVisible_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = TrayIcon.IsVisibleProperty;
		CompiledBindingExtension compiledBindingExtension4 = compiledBindingExtension3.ProvideValue(context);
		context.ProvideTargetProperty = null;
		trayIcon2.Bind(isVisibleProperty, (IBinding)compiledBindingExtension4);
		NativeMenu nativeMenu;
		NativeMenu menu = (nativeMenu = new NativeMenu());
		context.PushParent(nativeMenu);
		NativeMenu nativeMenu2 = nativeMenu;
		IList<NativeMenuItemBase> items = nativeMenu2.Items;
		NativeMenuItem nativeMenuItem;
		NativeMenuItem item = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem2 = nativeMenuItem;
		nativeMenuItem2.Header = "О программе";
		StyledProperty<ICommand?> commandProperty2 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension5 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("About_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BAbout_Click_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension6 = compiledBindingExtension5.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem2.Bind(commandProperty2, (IBinding)compiledBindingExtension6);
		context.PopParent();
		items.Add(item);
		IList<NativeMenuItemBase> items2 = nativeMenu2.Items;
		NativeMenuItem item2 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem3 = nativeMenuItem;
		nativeMenuItem3.Header = "Сайт разработчика";
		StyledProperty<ICommand?> commandProperty3 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension7 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Site_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BSite_Click_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension8 = compiledBindingExtension7.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem3.Bind(commandProperty3, (IBinding)compiledBindingExtension8);
		context.PopParent();
		items2.Add(item2);
		IList<NativeMenuItemBase> items3 = nativeMenu2.Items;
		NativeMenuItem item3 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem4 = nativeMenuItem;
		nativeMenuItem4.Header = "Форум поддержки";
		StyledProperty<ICommand?> commandProperty4 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension9 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Forum_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BForum_Click_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension10 = compiledBindingExtension9.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem4.Bind(commandProperty4, (IBinding)compiledBindingExtension10);
		context.PopParent();
		items3.Add(item3);
		nativeMenu2.Items.Add(new NativeMenuItemSeparator());
		IList<NativeMenuItemBase> items4 = nativeMenu2.Items;
		NativeMenuItem item4 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem5 = nativeMenuItem;
		nativeMenuItem5.Header = "Перезапустить kkmserver";
		StyledProperty<ICommand?> commandProperty5 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension11 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Reboot_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BReboot_Click_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension12 = compiledBindingExtension11.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem5.Bind(commandProperty5, (IBinding)compiledBindingExtension12);
		context.PopParent();
		items4.Add(item4);
		IList<NativeMenuItemBase> items5 = nativeMenu2.Items;
		NativeMenuItem item5 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem6 = nativeMenuItem;
		nativeMenuItem6.Header = "Устройства: 'Отключить/Подключить'";
		StyledProperty<Bitmap?> iconProperty = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension13 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002EDeviceIcon_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension14 = compiledBindingExtension13.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem6.Bind(iconProperty, (IBinding)compiledBindingExtension14);
		StyledProperty<ICommand?> commandProperty6 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension15 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Device_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BDevice_Click_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension16 = compiledBindingExtension15.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem6.Bind(commandProperty6, (IBinding)compiledBindingExtension16);
		context.PopParent();
		items5.Add(item5);
		IList<NativeMenuItemBase> items6 = nativeMenu2.Items;
		NativeMenuItem item6 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem7 = nativeMenuItem;
		nativeMenuItem7.Header = "Установить тип запуска";
		NativeMenu menu2 = (nativeMenu = new NativeMenu());
		context.PushParent(nativeMenu);
		NativeMenu nativeMenu3 = nativeMenu;
		IList<NativeMenuItemBase> items7 = nativeMenu3.Items;
		NativeMenuItem item7 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem8 = nativeMenuItem;
		nativeMenuItem8.Header = "Ручной запуск: 'В обычном окне'";
		StyledProperty<Bitmap?> iconProperty2 = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension17 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002ESettingsRunIcon_1_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension18 = compiledBindingExtension17.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem8.Bind(iconProperty2, (IBinding)compiledBindingExtension18);
		StyledProperty<ICommand?> commandProperty7 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension19 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("SettingsRun_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BSettingsRun_Click_1_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension20 = compiledBindingExtension19.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem8.Bind(commandProperty7, (IBinding)compiledBindingExtension20);
		nativeMenuItem8.CommandParameter = "Windows";
		context.PopParent();
		items7.Add(item7);
		IList<NativeMenuItemBase> items8 = nativeMenu3.Items;
		NativeMenuItem item8 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem9 = nativeMenuItem;
		nativeMenuItem9.Header = "Автозапуск: 'Свернутое в Трей'";
		StyledProperty<Bitmap?> iconProperty3 = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension21 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002ESettingsRunIcon_2_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension22 = compiledBindingExtension21.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem9.Bind(iconProperty3, (IBinding)compiledBindingExtension22);
		StyledProperty<ICommand?> commandProperty8 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension23 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("SettingsRun_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BSettingsRun_Click_1_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension24 = compiledBindingExtension23.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem9.Bind(commandProperty8, (IBinding)compiledBindingExtension24);
		nativeMenuItem9.CommandParameter = "Tray";
		context.PopParent();
		items8.Add(item8);
		IList<NativeMenuItemBase> items9 = nativeMenu3.Items;
		NativeMenuItem item9 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem10 = nativeMenuItem;
		nativeMenuItem10.Header = "Автозапуск: 'Windows Service'";
		StyledProperty<Bitmap?> iconProperty4 = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension25 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002ESettingsRunIcon_3_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = NativeMenuItem.IconProperty;
		CompiledBindingExtension compiledBindingExtension26 = compiledBindingExtension25.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem10.Bind(iconProperty4, (IBinding)compiledBindingExtension26);
		StyledProperty<ICommand?> commandProperty9 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension27 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("SettingsRun_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BSettingsRun_Click_1_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension28 = compiledBindingExtension27.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem10.Bind(commandProperty9, (IBinding)compiledBindingExtension28);
		nativeMenuItem10.CommandParameter = "Service";
		context.PopParent();
		items9.Add(item9);
		context.PopParent();
		nativeMenuItem7.Menu = menu2;
		context.PopParent();
		items6.Add(item6);
		nativeMenu2.Items.Add(new NativeMenuItemSeparator());
		IList<NativeMenuItemBase> items10 = nativeMenu2.Items;
		NativeMenuItem item10 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem11 = nativeMenuItem;
		StyledProperty<string?> headerProperty = NativeMenuItem.HeaderProperty;
		CompiledBindingExtension compiledBindingExtension29 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002ESettingsHttpsText_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = NativeMenuItem.HeaderProperty;
		CompiledBindingExtension compiledBindingExtension30 = compiledBindingExtension29.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem11.Bind(headerProperty, (IBinding)compiledBindingExtension30);
		StyledProperty<ICommand?> commandProperty10 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension31 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Settings_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BSettings_Click_1_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension32 = compiledBindingExtension31.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem11.Bind(commandProperty10, (IBinding)compiledBindingExtension32);
		nativeMenuItem11.CommandParameter = "HTTPS";
		context.PopParent();
		items10.Add(item10);
		IList<NativeMenuItemBase> items11 = nativeMenu2.Items;
		NativeMenuItem item11 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem12 = nativeMenuItem;
		StyledProperty<string?> headerProperty2 = NativeMenuItem.HeaderProperty;
		CompiledBindingExtension compiledBindingExtension33 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Property(CompiledAvaloniaXaml.XamlIlHelpers.KkmFactory_002EViewModels_002EViewModel_002Ckkmserver_002ESettingsHttpText_0021Property(), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor).Build());
		context.ProvideTargetProperty = NativeMenuItem.HeaderProperty;
		CompiledBindingExtension compiledBindingExtension34 = compiledBindingExtension33.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem12.Bind(headerProperty2, (IBinding)compiledBindingExtension34);
		StyledProperty<ICommand?> commandProperty11 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension35 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Settings_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BSettings_Click_1_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension36 = compiledBindingExtension35.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem12.Bind(commandProperty11, (IBinding)compiledBindingExtension36);
		nativeMenuItem12.CommandParameter = "HTTP";
		context.PopParent();
		items11.Add(item11);
		nativeMenu2.Items.Add(new NativeMenuItemSeparator());
		IList<NativeMenuItemBase> items12 = nativeMenu2.Items;
		NativeMenuItem item12 = (nativeMenuItem = new NativeMenuItem());
		context.PushParent(nativeMenuItem);
		NativeMenuItem nativeMenuItem13 = nativeMenuItem;
		nativeMenuItem13.Header = "Выход";
		StyledProperty<ICommand?> commandProperty12 = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension37 = new CompiledBindingExtension(new CompiledBindingPathBuilder().Command("Exit_Click", CompiledAvaloniaXaml.XamlIlTrampolines.kkmserver_003AKkmFactory_002EViewModels_002EViewModel_002BExit_Click_0_0021CommandExecuteTrampoline, null, null).Build());
		context.ProvideTargetProperty = NativeMenuItem.CommandProperty;
		CompiledBindingExtension compiledBindingExtension38 = compiledBindingExtension37.ProvideValue(context);
		context.ProvideTargetProperty = null;
		nativeMenuItem13.Bind(commandProperty12, (IBinding)compiledBindingExtension38);
		context.PopParent();
		items12.Add(item12);
		context.PopParent();
		trayIcon2.Menu = menu;
		context.PopParent();
		trayIcons2.Add(trayIcon);
		context.PopParent();
		TrayIcon.SetIcons(app2, trayIcons);
		context.PopParent();
		if (app is StyledElement styledElement)
		{
			NameScope.SetNameScope(styledElement, context.AvaloniaNameScope);
		}
		context.AvaloniaNameScope.Complete();
	}

	private static void _0021XamlIlPopulateTrampoline(App P_0)
	{
		if (_0021XamlIlPopulateOverride != null)
		{
			_0021XamlIlPopulateOverride(P_0);
		}
		else
		{
			_0021XamlIlPopulate(XamlIlRuntimeHelpers.CreateRootServiceProviderV3(null), P_0);
		}
	}
}
