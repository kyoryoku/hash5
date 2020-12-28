using System.Windows;
using System.Windows.Controls;

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
