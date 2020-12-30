using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для AddingFilesForm.xaml
    /// </summary>
    public partial class AddingFilesForm : UserControl
    {
        CancelDialogHandler cancelDialog;
        OpenFoldeDialogHandler openFoldeDialogHandler;
        OpenFileDialogHandler openFileDialogHandler;
        public AddingFilesForm(OpenFoldeDialogHandler openFolder, OpenFileDialogHandler openFile, CancelDialogHandler cancel)
        {
            InitializeComponent();
            this.openFoldeDialogHandler = openFolder;
            this.openFileDialogHandler = openFile;
            this.cancelDialog = cancel;
        }

        //КНОПКА: открыть папку
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            openFoldeDialogHandler();
        }

        //КНОПКА: открыть файлы
        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            openFileDialogHandler();
        }

        //КНОПКА: Отмена
        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            cancelDialog();
        }
    }
}
