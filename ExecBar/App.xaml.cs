using System.Windows;

namespace ExecBar
{
    public partial class App : Application
    {
        private static MainWindow _mainWindow;

        public static MainWindow MainWindowInstance
        {
            get
            {
                if (_mainWindow == null)
                {
                    _mainWindow = new MainWindow();
                }
                return _mainWindow;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindowInstance.Show();
            MainWindowInstance.Hide();
        }
    }
}
