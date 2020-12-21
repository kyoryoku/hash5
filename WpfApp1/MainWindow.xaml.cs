using HandyControl.Data;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    public partial class MainWindow : Window
    {
        ObservableCollection<File> files = new ObservableCollection<File>();

        public MainWindow()
        {
            InitializeComponent();
            listView.ItemsSource = files;
            lbl_filesCount.Content = "Файлов добавлено: " + files.Count.ToString();
            
            
        }

        private async void btn1_click(object sender, RoutedEventArgs e)
        {
            foreach (File f in files)
            {
                f.calculateHash();
            }
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




        [DllImport("bee2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void beltHashStart(byte[] state);

        [DllImport("bee2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void beltHashStepH(byte[] buf, int size, byte[] state);

        [DllImport("bee2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void beltHashStepG(byte[] hash, byte[] state);

        private byte[] State1;
        private byte[] Hash1;

        async public void calculateHash()
        {
            //1. подготовка общих буферов
            State1 = new byte[4096];
            Hash1 = new byte[32];
            for (int i = 0; i < 4096; i++){
                State1[i] = 0;
            }
            for (int i = 0; i < 32; i++){
                Hash1[i] = 0;
            }
            beltHashStart(State1);

            //2. считываение файла и расчет блоков
            await Task.Run(() => stepH());

            //3. расчет окончательного значения и приведение к виду
            beltHashStepG(Hash1, State1);
            string res = "";
            for (int i = 0; i < 32; i++){
                if (Hash1[i] <= 15){
                    res = res + "0" + Convert.ToString(Hash1[i], 16);
                }
                else{
                    res = res + Convert.ToString(Hash1[i], 16);
                }
            }

            this.hashValue = res;
        }

        private void stepH()
        {
            FileStream fstream = new FileStream(fileName, FileMode.Open);
            int readSize = 0;
            byte[] readBuff = new byte[4096];
            while ((readSize = fstream.Read(readBuff, 0, 4096)) != 0)
            {
                beltHashStepH(readBuff, readSize, State1);
            }
            fstream.Close();
        }

        

    }


}
