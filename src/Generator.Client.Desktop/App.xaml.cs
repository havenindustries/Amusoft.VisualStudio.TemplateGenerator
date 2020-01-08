using System;
using System.Reflection;
using System.Windows;
using Generator.Client.Desktop.DependencyInjection;
using Generator.Client.Desktop.Utility;
using Generator.Client.Desktop.Views;
using Generator.Shared.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Behaviors;
using MonkeyCache.FileStore;
using NLog.Config;
using Path = System.IO.Path;

namespace Generator.Client.Desktop
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			ServiceLocatorInitializer.Initialize();

			Barrel.ApplicationId = "TemplateGenerator" + new Guid();
			Barrel.Current.Add(key: "AppName", data: "YOURAPPNAME", TimeSpan.FromDays(30));
			var appName = Barrel.Current.Get<string>(key: "AppName");

			base.OnStartup(e);
		}
	}

	public class ServiceLocatorInitializer
	{
		public static void Initialize()
		{
			ServiceLocator.Build(Configure);
		}

		private static void Configure(ServiceCollection services)
		{
			var viewModelPresenter = new StaticViewModelPresenter();
			viewModelPresenter.Build();
			services.AddSingleton<IViewModelPresenter>(viewModelPresenter);
			services.AddSingleton<IUIService, UIService>();
		}
	}
}
