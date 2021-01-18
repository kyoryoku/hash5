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
//using DataFormats = System.Windows.DataFormats;
//using DragEventArgs = System.Windows.DragEventArgs;
//using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
//using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace WpfApp1
{

    public partial class MainWindow : Window
    {
        
        ObservableCollection<File> FILES = new ObservableCollection<File>();
        long FILES_SIZE = 0;
        IniFile INI = new IniFile("conf.ini");
        int THREAD_COUNT = 8;
        SnackbarMessageQueue MESSAGE_QUEUE = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));



        /*==============================================================================================================================
          =                                                                                                                            =
          =                                              ЗАПУСК ПРОГРАММЫ                                                              = 
          =                                                                                                                            = 
          ==============================================================================================================================*/

        public MainWindow()
        {
            InitializeComponent();
            inicializeComponentContent();
            inicializeAppFiles();
        }

        //настраиваем компоненты на форме
        private void inicializeComponentContent()
        {
            listView.ItemsSource = FILES;
            lbl_filesCount.Content = String.Format("Файлов добавлено: {0}", FILES.Count.ToString());
            lbl_hint.Content = "Для начала работы добавьте или перетащите файлы...";
            snackbar.MessageQueue = MESSAGE_QUEUE;
        }


        //проверяем наличие ключевых файлов
        private async void inicializeAppFiles()
        {

            //проверяем наличие криптобиблиотеки
            if (!new FileInfo("bee2.dll").Exists)
            {
                string errorMessage = "Не найдена криптобиблиотека bee2.dll\nПриложение будет завершено!";
                var content = new DialogMessageForm(errorMessage, cancelDialog);
                var result = await dialogHost.ShowDialog(content);

                Environment.Exit(1);
            }

            //проверяем наличие конф файла
            if (!new FileInfo("conf.ini").Exists)
            {
                //если файла нет, создаем дефолтный
                string warningMessage = "Не найден файл настроек conf.ini\nБудет создан файл настроек по умочланию!";
                var content = new DialogMessageForm(warningMessage, cancelDialog);
                var result = await dialogHost.ShowDialog(content);

                INI.Write("DEFAULT", "THREAD_COUNT", THREAD_COUNT.ToString());
            }

            //пробуем считывать настройки из конф файла
            //THREAD_COUNT = int.Parse(INI.ReadINI("DEFAULT", "THREAD_COUNT"));
            try
            {
                THREAD_COUNT = int.Parse(INI.ReadINI("DEFAULT", "THREAD_COUNT"));
            }
            catch
            {
                //если чтото пошло не так, сообщаем и закрываем прогу
                string errorMessage = "Ошибка в файле настроек conf.ini\nПриложение будет завершено!";
                var content = new DialogMessageForm(errorMessage, cancelDialog);
                var result = await dialogHost.ShowDialog(content);

                Environment.Exit(1);
            }

        }



        /*==============================================================================================================================
          =                                                                                                                            =
          =                                       Добавление файлов в программу                                                        = 
          =                                                                                                                            = 
          ==============================================================================================================================*/


        //КНОПКА: Добавление файлов или папок
        private async void btn1_click(object sender, RoutedEventArgs e)
        {
            var content = new AddingFilesForm(openFolder, openFile, cancelDialog);
            var result = await dialogHost.ShowDialog(content);
        }

        private void openFolder()
        {
            cancelDialog();

            System.Windows.Forms.FolderBrowserDialog openDialog = new System.Windows.Forms.FolderBrowserDialog();
            openDialog.Description = "Выберите каталог для добавления";
            openDialog.ShowNewFolderButton = false;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FindInDir(new DirectoryInfo(openDialog.SelectedPath), "*", true);
            } 
            else
            {
                return;
            }

        }


        //Добавление файлов через диалог
        private void openFile()
        {
            cancelDialog();

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Выберите файлы для расчета";
            openDialog.Multiselect = true;

            if ((bool)!openDialog.ShowDialog())
            {
                return;
            }

            foreach (string f in openDialog.FileNames)
            {
                addFile(f);
            }
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
                    //если объект - папка:
                    if (Directory.Exists(obj))
                    {
                        //идем искать все что в ней
                        FindInDir(new DirectoryInfo(obj), "*", true);
                    }
                    //иначе это файл:
                    else
                    {
                        addFile(obj);
                    }
                }
            }
            
        }

        //Рекурсивный поиск в подкаталогах
        public void FindInDir(DirectoryInfo dir, string pattern, bool recursive)
        {
            //сначала добавляем все файлы в папке
            foreach (FileInfo file in dir.GetFiles(pattern))
            {
                addFile(file.FullName);
            }

            //затем рекурсивно заходим в каждую папку 
            if (recursive)
            {
                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    this.FindInDir(subdir, pattern, recursive);
                }
            }
        }

        private void addFile(string fileName)
        {
            //добавляем файл в коллекцию
            FILES.Add(new File(fileName));

            //считаем общий размер добавленных файлов
            FILES_SIZE += new FileInfo(fileName).Length;
            
            //переводим в понятный вид
            if (FILES_SIZE < 1024)
            {
                lbl_filesCount.Content = String.Format("Файлов добавлено: {0} ({1} байт)", FILES.Count, FILES_SIZE);
            }

            if (FILES_SIZE >= 1024)
            {
                lbl_filesCount.Content = String.Format("Файлов добавлено: {0} ({1:F1} Кб)", FILES.Count, FILES_SIZE * 1.0 / 1024);
            }

            if (FILES_SIZE >= 1048576)  // = 1024 * 1024
            {
                lbl_filesCount.Content = String.Format("Файлов добавлено: {0} ({1:F1} Мб)", FILES.Count, FILES_SIZE * 1.0 / 1048576);
            }

            if (FILES_SIZE >= 1073741824) // = 1024 * 1024 * 1024
            {
                lbl_filesCount.Content = String.Format("Файлов добавлено: {0} ({1:F1} Гб)", FILES.Count, FILES_SIZE * 1.0 / 1073741824);
            }

            if (FILES_SIZE >= 1099511627776) // = 1024 * 1024 * 1024 * 1024
            {
                lbl_filesCount.Content = String.Format("Файлов добавлено: {0} ({1:F1} Тб)", FILES.Count, FILES_SIZE * 1.0 / 1099511627776);
            }

            //обвновляем подсказки
            lbl_hint.Content = "Файлы добавлены! Запустите расчет...";
        }


        /*==============================================================================================================================
          =                                                                                                                            =
          =                                                   Расчет файлов                                                            = 
          =                                                                                                                            = 
          ==============================================================================================================================*/

        //КНОПКА: Запуск расчета
        private async void btn3_click(object sender, RoutedEventArgs e)
        {

            if (FILES.Count == 0)
            {
                MESSAGE_QUEUE.Enqueue("В списке нет файлов для рачета!");
                return;
            }

            lbl_hint.Content = "Расчет запущен! Ожидайте результаты...";
            updateControls();
            await Task.Run(() =>
            {
 
                FILES.AsParallel().WithDegreeOfParallelism(THREAD_COUNT).ForAll(f =>
                {  
                    f.calculateHash();
                });
            });

            string infoMessage = "Расчет контрольных сумм завершен!";
            var content = new DialogMessageForm(infoMessage, cancelDialog);
            var result = dialogHost.ShowDialog(content);
        }

        //обновление компонентов формы пока идет расчет в отдельном потоке
        private async void updateControls()
        {
            await Task.Run(() =>
            {
               
                while (FILES.Where(x => x.isDone).Count() != FILES.Count)
                {
                    Thread.Sleep(100);
                    this.Dispatcher.Invoke(() =>
                    {
                        lbl_filesCount.Content = String.Format("Завершено: {0} из {1}", FILES.Where(x => x.isDone).Count(), FILES.Count);
                    });

                }

                this.Dispatcher.Invoke(() =>
                {
                    lbl_hint.Content = "Расчет завершен! Сохраните результат...";
                });


            });
            
        }



        /*==============================================================================================================================
          =                                                                                                                            =
          =                                            Сохранить или скопировать результат                                             = 
          =                                                                                                                            = 
          ==============================================================================================================================*/

        //КНОПКА: Сохранение результатов в .csv
        private void btn4_click(object sender, RoutedEventArgs e)
        {

            if (FILES.Count != 0)
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

                MESSAGE_QUEUE.Enqueue("Результаты сохранены в файл " + saveDialog.FileName);
            }
            else
            {
                MESSAGE_QUEUE.Enqueue("В списке нет файлов для сохранения!");
            }
            

        }

        //КНОПКА: копирование в буфер
        private void btn8_click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0) {
                string text = "";
                foreach (File f in listView.SelectedItems)
                {
                    if (f.isDone)
                    {
                        text += string.Format("{0}\t{1}\n", f.fileName, f.hashValue);
                    }
                    else
                    {
                        text += string.Format("{0}\n", f.fileName);
                    }
                }

                Clipboard.Clear();
                Clipboard.SetText(text);
                MESSAGE_QUEUE.Enqueue("В буфер обмена скопировано строк: " + listView.SelectedItems.Count);
            } else
            {
                MESSAGE_QUEUE.Enqueue("Не выбраны элементы для копирования!");
            }
            


        }


        /*==============================================================================================================================
          =                                                                                                                            =
          =                                                       Очистка формы                                                        = 
          =                                                                                                                            = 
          ==============================================================================================================================*/

        //КНОПКА: Очистка формы
        private async void btn5_click(object sender, RoutedEventArgs e)
        {
            var content = new DeletionConfirmationForm(acceptDialogClear, cancelDialog);
            var result = await dialogHost.ShowDialog(content);
            
        }




        /*==============================================================================================================================
          =                                                                                                                            =
          =                                                        Настройки                                                           = 
          =                                                                                                                            = 
          ==============================================================================================================================*/

        //КНОПКА: Настройки
        private async void btn6_click(object sender, RoutedEventArgs e)
        {
            var content = new DialogMessageForm("123", cancelDialog);
            var result = await dialogHost.ShowDialog(content);

            MESSAGE_QUEUE.Enqueue("dasdasd");
        }





        /*==============================================================================================================================
          =                                                                                                                            =
          =                                                        Справка                                                             = 
          =                                                                                                                            = 
          ==============================================================================================================================*/

        //КНОПКА: Справка
        private async void btn7_click(object sender, RoutedEventArgs e)
        {
            var content = new HelpForm(cancelDialog);
            var result = await dialogHost.ShowDialog(content);
        }




        /*==============================================================================================================================
          =                                                                                                                            =
          =                                                  Кастомные диалоги                                                         = 
          =                                                                                                                            = 
          ==============================================================================================================================*/



        private void acceptDialogClear()
        {
            FILES.Clear();
            FILES_SIZE = 0;
            lbl_filesCount.Content = String.Format("Файлов добавлено: {0}", FILES.Count.ToString());
            lbl_hint.Content = "Для начала работы добавьте или перетащите файлы...";
            dialogHost.CurrentSession.Close(false);
            MESSAGE_QUEUE.Enqueue("Список файлов очищен!");
        }


        private void cancelDialog()
        {
            dialogHost.CurrentSession.Close(false);
        }

    }
}
