using System.Windows;
using System;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;
            MessageBox.Show($"Необработанная ошибка: {exception.Message}\n{exception.StackTrace}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Ошибка в интерфейсе: {args.Exception.Message}\n{args.Exception.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}