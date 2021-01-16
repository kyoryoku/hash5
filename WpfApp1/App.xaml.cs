using System;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {

        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            String log = "log.txt";
            MessageBox.Show("Непредвиденное исключение: " + e.Exception.Message + 
                '\n' + "Смотрите подробности в log.txt",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);


            System.IO.File.AppendAllText(log, DateTime.Now + "  " + e.Exception.Source + '\n');
            System.IO.File.AppendAllText(log, e.Exception.Message + '\n');
            System.IO.File.AppendAllText(log, e.Exception.StackTrace + '\n');
            System.IO.File.AppendAllText(log, "==========\n");

            e.Handled = true;
        }
    }



}
