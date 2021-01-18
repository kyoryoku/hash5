using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace WpfApp1
{
    public class File : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Хранит абсолютный путь
        private string _fileName;
        public string fileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        //Хранит хэш значение
        private string _hashValue;
        public string hashValue
        {
            get { return _hashValue; }
            set { SetProperty(ref _hashValue, value); }
        }

        //Хранит состояние: true - хэш посчитан, false - нет
        private bool _isDone;
        public bool isDone
        {
            get { return _isDone; }
            set { SetProperty(ref _isDone, value); }
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
            this.hashValue = "";
            this.isDone = false;
        }

        //Импорт библиотеки
        [DllImport("bee2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void beltHashStart(byte[] state);

        [DllImport("bee2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void beltHashStepH(byte[] buf, int size, byte[] state);

        [DllImport("bee2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void beltHashStepG(byte[] hash, byte[] state);



        private byte[] State1;
        private byte[] Hash1;

        //расчет хэша
        public void calculateHash()
        {
            //1. подготовка общих буферов
            State1 = new byte[4096];
            Hash1 = new byte[32];
            beltHashStart(State1);

            //2. считываение файла и расчет блоков          
            FileInfo fileInf = new FileInfo(fileName);
            long fileSize = fileInf.Length;
            long readed = 0;

            FileStream fstream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] readBuff = new byte[4096];
            int readSize;
            while ((readSize = fstream.Read(readBuff, 0, 4096)) != 0)
            {
                beltHashStepH(readBuff, readSize, State1);
                readed += readSize;
                this.hashValue = String.Format("   {0:P1}", readed * 1.0 / fileSize);
            }
            fstream.Close();


            //3. расчет окончательного значения и приведение к виду
            beltHashStepG(Hash1, State1);
            string res = "";
            for (int i = 0; i < 32; i++)
            {
                if (Hash1[i] <= 15)
                {
                    res = res + "0" + Convert.ToString(Hash1[i], 16);
                }
                else
                {
                    res = res + Convert.ToString(Hash1[i], 16);
                }
            }

            this.hashValue = res.ToUpper();
            this.isDone = true;
        }

        //private void stepH()
        //{
        //    FileInfo fileInf = new FileInfo(fileName);
        //    long fileSize = fileInf.Length;
        //    long readed = 0;

        //    FileStream fstream = new FileStream(fileName, FileMode.Open, FileAccess.Read);     
        //    byte[] readBuff = new byte[4096];
        //    int readSize;
        //    while ((readSize = fstream.Read(readBuff, 0, 4096)) != 0)
        //    {
        //        beltHashStepH(readBuff, readSize, State1);
        //        readed += readSize;
        //        this.hashValue = String.Format("   {0:P1}", readed * 1.0 / fileSize);


        //    }
        //    fstream.Close();
        //}
    }
}
