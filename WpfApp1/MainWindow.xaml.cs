﻿using MaterialDesignThemes.Wpf;
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
        IniFile INI = 
        ObservableCollection<File> files = new ObservableCollection<File>();

        public MainWindow()
        {
            inicializeData();
            InitializeComponent();
            listView.ItemsSource = files;
            lbl_filesCount.Content = String.Format("Файлов добавлено: {0}", files.Count.ToString());
            lbl_hint.Content = "Для начала работы добавьте или перетащите файлы...";
        }

        //проверка наличия библиотек, загрузка .ini файла
        private void inicializeData()
        {
            
            if (!new FileInfo("bee2.dll").Exists)
            {
                MessageBox.Show("не найдена bee2.dll");
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
                files.Add(new File(f));
            }
        }

        //КНОПКА: Запуск расчета
        private async void btn3_click(object sender, RoutedEventArgs e)
        {
            lbl_hint.Content = "Расчет запущен! Ожидайте результаты...";
            test();
            await Task.Run(() =>
            {
                files.AsParallel().WithDegreeOfParallelism(8).ForAll(f =>
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
                while (files.Where(x => x.isDone).Count() != files.Count)
                {
                    Thread.Sleep(100);
                    this.Dispatcher.Invoke(() =>
                    {
                        lbl_filesCount.Content = String.Format("Завершено {0} из {1}", files.Where(x => x.isDone).Count(), files.Count);
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
            foreach (File f in files)
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
        }

        private void acceptDialogClear()
        {
            files.Clear();
            lbl_filesCount.Content = String.Format("Файлов добавлено: {0}", files.Count.ToString());
            lbl_hint.Content = "Для начала работы добавьте или перетащите файлы...";
            dialogHost.CurrentSession.Close(false);
        }

        private void cancelDialog()
        {
            dialogHost.CurrentSession.Close(false);
        }


        //КНОПКА: Справка
        private async void btn6_click(object sender, RoutedEventArgs e)
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
                        files.Add(new File(obj));
                    }

                }
            }
            lbl_filesCount.Content = "Файлов добавлено: " + files.Count.ToString();
            lbl_hint.Content = "Файлы добавлены! Запустите расчет...";
        }

        //Рекурсивный поиск в подкаталогах
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
}
