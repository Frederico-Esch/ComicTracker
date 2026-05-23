using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.Configuration;
using System;
using Persistence;
using ComicTracking.Windows;
using Utils.Extensions;
using Microsoft.Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking
{
    public partial class App : Application
    {
        private Window? _window;
        private readonly ServiceProvider services;

        public App()
        {
            var serviceCollection = new ServiceCollection();

            ConfigWindows(serviceCollection);
            ConfigAppSettings(serviceCollection);

            serviceCollection.ConfigRepositories();

            services = serviceCollection.BuildServiceProvider();

            InitializeComponent();
        }
        
        private void ConfigWindows(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<EditComicWindow>();
            serviceCollection.AddTransient<ManageTagsWindow>();
            serviceCollection.AddTransient<FilterTagsWindow>();
            serviceCollection.AddTransient<ComicWindow>();
        }

        private void ConfigAppSettings(ServiceCollection services)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile($"{AppContext.BaseDirectory}\\appsettings.json");
            services.AddSingleton<IConfiguration>(builder.Build());
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = services.GetRequiredService<MainWindow>();
            _window.NavigateTo();
        }
    }
}
