using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Face3DReconstruction.Services;
using Face3DReconstruction.ViewModels;

namespace Face3DReconstruction
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<IFilesService, FilesService>()
                .AddSingleton<IRequestService, RequestService>()
                .AddSingleton<ISettingsService, SettingsService>()
                .AddSingleton<ILoggingService, LoggingService>()
                .AddSingleton<Viewport2DConfigViewModel, Viewport2DConfigViewModel>()
                .BuildServiceProvider());

            MainWindow app = new MainWindow();
            app.Show();
        }
    }
}
