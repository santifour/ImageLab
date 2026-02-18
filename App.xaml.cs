using System.Windows;

namespace ImageToolkit
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Software rendering fallback for maximum compatibility
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Ciddi bir hata oluştu: {args.Exception.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            base.OnStartup(e);
            
            var window = new MainWindow();
            window.Show();
        }
    }
}
