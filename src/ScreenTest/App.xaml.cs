using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScreenTest;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.ToString(), "AT ScreenTest — unexpected error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            MessageBox.Show(args.ExceptionObject.ToString(), "AT ScreenTest — fatal error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }
}
