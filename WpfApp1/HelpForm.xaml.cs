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
    /// Логика взаимодействия для HelpForm.xaml
    /// </summary>
    public partial class HelpForm : UserControl
    {
        CancelDialogHandler cancelDialog;
        public HelpForm(CancelDialogHandler cancelDialog)
        {
            this.cancelDialog = cancelDialog;
            InitializeComponent();
        }

        //КНОПКА: закрыть
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            cancelDialog();
        }

    }
}
