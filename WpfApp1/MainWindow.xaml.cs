using HandyControl.Data;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        ObservableCollection<File> files = new ObservableCollection<File>();

        public MainWindow()
        {
            InitializeComponent();
            listView.ItemsSource = files;
            lbl_filesCount.Content = "Файлов добавлено: " + files.Count.ToString();
            
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void dragDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // получаем строки объектов что нам дропнулись
                string[] dropedObj = (string[])e.Data.GetData(DataFormats.FileDrop);

                // проходим по каждому объекту и пытаемся вытасчить оттуда все файлы
                foreach(string obj in dropedObj)
                {
                    if (Directory.Exists(obj)) {
                        FindInDir(new DirectoryInfo(obj), "*", true);
                    } else
                    {
                        files.Add(new File(obj));
                    }
                   
                }
            }
            lbl_filesCount.Content = "Файлов добавлено: " + files.Count.ToString();
        }

        public void FindInDir(DirectoryInfo dir, string pattern, bool recursive)
        {
            foreach (FileInfo file in dir.GetFiles(pattern))
            {
                files.Add(new File(file.FullName));
            }
            if (recursive)
            {
                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    
                    this.FindInDir(subdir, pattern, recursive);
                }
            }
        }
    }



    public class File : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _fileName;
        public string fileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        private string _hashValue;
        public string hashValue
        {
            get { return _hashValue; }
            set { SetProperty(ref _hashValue, value); }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public File(string fileName)
        {
            this.fileName = fileName;
        }

    }


}
