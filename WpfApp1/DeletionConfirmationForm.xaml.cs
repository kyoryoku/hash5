using MaterialDesignThemes.Wpf;
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
    /// Логика взаимодействия для DeletionConfirmationForm.xaml
    /// </summary>
    public partial class DeletionConfirmationForm : UserControl
    {

        AcceptDialogHandler acceptDialog;
        CancelDialogHandler cancelDialog;
        public DeletionConfirmationForm(AcceptDialogHandler acceptDialog, CancelDialogHandler cancelDialog)
        {
            this.acceptDialog = acceptDialog;
            this.cancelDialog = cancelDialog;
            InitializeComponent();
        }


        //КНОПКА: принять
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            acceptDialog();
        }

        //КНОПКА: отменить
        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            cancelDialog();
        }
    }
}
