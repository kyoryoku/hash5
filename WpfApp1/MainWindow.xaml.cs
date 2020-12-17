using HandyControl.Data;
using System;
using System.Collections;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ArrayList files = new ArrayList();
            for (int i = 0; i < 10000; i++)
            {
                files.Add(new File("file_" + i + "_/asdasd/asdasdas/sadasdsad/asdasdsad/asdsadsad/sdaasdasdsa/asdsadsadasd/asdasd"));
            }



            listView.ItemsSource = files;
        }
    }


    public class File
    {
        public string fileName { get; set; }
        public string hashValue { get; set; }

        public File(string fileName)
        {
            this.fileName = fileName;
            this.hashValue = "00112233445566778899AABBCCDDEEFF00112233445566778899AABBCCDDEEFF";     
        }
    }


}
