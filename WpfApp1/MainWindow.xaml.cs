using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;



namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        
        ObservableCollection<File> FILES = new ObservableCollection<File>();
        IniFile INI = new IniFile("conf.ini");
        int THREAD_COUNT = 8;
        SnackbarMessageQueue MESSAGE_QUEUE = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        public MainWindow()
        {
            inicializeAppFiles();
            InitializeComponent();
            inicializeComponentContent();
        }

        private void inicializeComponentContent()
        {
            listView.ItemsSource = FILES;
            lbl_filesCount.Content = String.Format("Файлов добавлено: {0}", FILES.Count.ToString());
            lbl_hint.Content = "Для начала работы добавьте или перетащите файлы...";
            snackbar.MessageQueue = MESSAGE_QUEUE;
        }

        //проверка наличия библиотек, загрузка .ini файла
        private void inicializeAppFiles()
        {

            //проверяем наличие криптобиблиотеки
            if (!new FileInfo("bee2.dll").Exists)
            {
                MessageBox.Show("не найдена bee2.dll");
                Environment.Exit(1);
            }

            //проверяем наличие конф файла
            if (!new FileInfo("conf.ini").Exists)
            {
                //если файла нет, создаем дефолтный
                MessageBox.Show("не найдена conf.ini. Будет создан по умолчанию");
                INI.Write("DEFAULT", "THREAD_COUNT", THREAD_COUNT.ToString());
            } 
           
            //пробуем считывать настройки
            try
            {
                THREAD_COUNT = int.Parse(INI.ReadINI("DEFAULT", "THREAD_COUNT"));
            } catch
            {
                //если чтото пошло не так, сообщаем и закрываем прогу
                MessageBox.Show("ошибка в файле conf.ini");
                Environment.Exit(1);
            }
            
        }

        //КНОПКА: Добавление папок через диалог
        private void btn1_click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openDialog = new System.Windows.Forms.FolderBrowserDialog();
            openDialog.ShowDialog();

        }

        //КНОПКА: Добавление файлов через диалог
        private void btn2_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Выберите файлы для расчета";
            openDialog.Multiselect = true;

            if ((bool)!openDialog.ShowDialog())
            {
                return;
            }

            foreach (string f in openDialog.FileNames)
            {
                FILES.Add(new File(f));
            }
        }

        //КНОПКА: Запуск расчета
        private async void btn3_click(object sender, RoutedEventArgs e)
        {
            lbl_hint.Content = "Расчет запущен! Ожидайте результаты...";
            test();
            await Task.Run(() =>
            {
                FILES.AsParallel().WithDegreeOfParallelism(THREAD_COUNT).ForAll(f =>
                    {
                        f.calculateHash();
                    });
            });
        }

        //обновление компонентов формы пока идет расчет в отдельном потоке
        private async void test()
        {
            await Task.Run(() =>
            {
                while (FILES.Where(x => x.isDone).Count() != FILES.Count)
                {
                    Thread.Sleep(100);
                    this.Dispatcher.Invoke(() =>
                    {
                        lbl_filesCount.Content = String.Format("Завершено {0} из {1}", FILES.Where(x => x.isDone).Count(), FILES.Count);
                    });

                }

                this.Dispatcher.Invoke(() =>
                {
                    lbl_hint.Content = "Расчет завершен! Сохраните результат...";
                });

                MessageBox.Show("Расчет контрольных сумм завершен!");
            });
        }

        //КНОПКА: Сохранение результатов в .csv
        private void btn4_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Текстовый формат (csv)|*.csv";
            saveDialog.Title = "Сохранить результаты расчета";
            saveDialog.DefaultExt = "csv";
            saveDialog.OverwritePrompt = true;

            if ((bool)!saveDialog.ShowDialog())
                return;

            string str = "";
            foreach (File f in FILES)
            {
                str += f.fileName + ";" + f.hashValue + ";\n";
            }


            System.IO.File.WriteAllText(saveDialog.FileName, str, Encoding.UTF8);

        }

        //КНОПКА: Очистка формы
        private async void btn5_click(object sender, RoutedEventArgs e)
        {
            var content = new DeletionConfirmationForm(acceptDialogClear, cancelDialog);
            var result = await dialogHost.ShowDialog(content);
            MESSAGE_QUEUE.Enqueue("Список очищен!");
        }

        private void acceptDialogClear()
        {
            FILES.Clear();
            lbl_filesCount.Content = String.Format("Файлов добавлено: {0}", FILES.Count.ToString());
            lbl_hint.Content = "Для начала работы добавьте или перетащите файлы...";
            dialogHost.CurrentSession.Close(false);
        }

        private void cancelDialog()
        {
            dialogHost.CurrentSession.Close(false);
        }

        //КНОПКА: Настройки
        private async void btn6_click(object sender, RoutedEventArgs e)
        {


            MESSAGE_QUEUE.Enqueue("dasdasd");
        }

        //КНОПКА: Справка
        private async void btn7_click(object sender, RoutedEventArgs e)
        {
            var content = new HelpForm(cancelDialog);
            var result = await dialogHost.ShowDialog(content);
        }


        //Перетаскивание на форму объектов
        private void dragDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // получаем строки объектов что нам дропнулись
                string[] dropedObj = (string[])e.Data.GetData(DataFormats.FileDrop);

                // проходим по каждому объекту и пытаемся вытасчить оттуда все файлы
                foreach (string obj in dropedObj)
                {
                    if (Directory.Exists(obj))
                    {
                        FindInDir(new DirectoryInfo(obj), "*", true);
                    }
                    else
                    {
                        FILES.Add(new File(obj));
                    }

                }
            }
            lbl_filesCount.Content = "Файлов добавлено: " + FILES.Count.ToString();
            lbl_hint.Content = "Файлы добавлены! Запустите расчет...";
        }

        //Рекурсивный поиск в подкаталогах
        public void FindInDir(DirectoryInfo dir, string pattern, bool recursive)
        {
            foreach (FileInfo file in dir.GetFiles(pattern))
            {
                FILES.Add(new File(file.FullName));
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
}
