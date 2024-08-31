using museumAIS.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace museumAIS
{
    /// <summary>
    /// Interaction logic for tablesWorkForm.xaml
    /// </summary>
    public partial class tablesWorkForm : Window
    {
        // Список имен таблиц
        private List<string> tableNamesList;
        // Экземпляр класса для работы с базой данных
        private static helpForDB db;
        // Сервис для отслеживания таймаута сессии
        private SessionTimeoutService _sessionTimeoutService;
        // Путь к папке для экспорта файлов CSV
        private static string folderPath =chooseDirectories.dirCSV;

        // Конструктор формы
        public tablesWorkForm()
        {
            InitializeComponent();

            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

            // Инициализация данных в комбобоксах
            setDataInCmbBox();
            // Получение списка файлов в директории
            getFiles();
        }

        // Инициализация данных в комбобоксах
        private void setDataInCmbBox()
        {
            tableNamesList = new List<string>
        {
            "users", "ecsponat", "guids", "halls", "excursions"
        };

            // Очистка и добавление элементов в expTableBox
            expTableBox.Items.Clear();
            expTableBox.Items.Add("Пользователи");
            expTableBox.Items.Add("Экспонаты");
            expTableBox.Items.Add("Экскурсоводы");
            expTableBox.Items.Add("Залы");
            expTableBox.Items.Add("Экскурсии");


            // Очистка и добавление элементов в impTableNameBox
            impTableNameBox.Items.Clear();
            impTableNameBox.Items.Add("Пользователи");
            impTableNameBox.Items.Add("Экспонаты");
            impTableNameBox.Items.Add("Экскурсоводы");
            impTableNameBox.Items.Add("Залы");
            impTableNameBox.Items.Add("Экскурсии");
        }

        // Очистка полей формы
        private void clearFields()
        {
            expTableBox.Text = "";
            impTableNameBox.Text = "";
            fileNameBox.Text = "";
        }

        // Получение списка файлов в директории
        private void getFiles()
        {
            // Создание директории, если она не существует
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Очистка и добавление файлов в fileNameBox
            fileNameBox.Items.Clear();
            foreach (string file in Directory.GetFiles(folderPath, "*.csv"))
            {
                fileNameBox.Items.Add(System.IO.Path.GetFileName(file));
            }
        }

        // Экспорт данных в CSV
        private void ExportData()
        {
            // Получение имени таблицы из списка
            string tableName = tableNamesList[expTableBox.SelectedIndex];

            // Выполнение экспорта данных и отображение результата
            if (db.exportData(tableName, folderPath))
            {
                callMessageBox.ShowInfo("Успешно!");
                clearFields();
            }
            else
            {
                callMessageBox.ShowError("Произошла ошибка при экспорте!");
            }
        }

        // Импорт данных из CSV
        private void importData()
        {
            // Получение пути к файлу CSV и имени таблицы из списка
            string csvFilePath = fileNameBox.Text;
            string tableName = tableNamesList[impTableNameBox.SelectedIndex];

            // Выполнение импорта данных и отображение результата
            if (db.importData(folderPath + "\\" + csvFilePath, tableName))
            {
                callMessageBox.ShowInfo("Успешно!");
                clearFields();
            }
            else
            {
                callMessageBox.ShowError("Произошла ошибка при импорте!");
            }
        }

        // Обработчик кнопки перехода назад
        private void goBackButton_Click(object sender, RoutedEventArgs e)
        {
            mainAdministratorForm f = new mainAdministratorForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки экспорта
        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            if (expTableBox.Text == "")
            {
                callMessageBox.ShowWarning("Таблица не выбрана!");
                return;
            }
            ExportData();
            getFiles();
        }

        // Обработчик кнопки импорта
        private void importButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (impTableNameBox.Text == "" || fileNameBox.Text == "")
            {
                callMessageBox.ShowWarning("Таблица не выбрана!");
                return;
            }
            importData();
        }

        // Обработчик события таймаута сессии
        private void OnSessionTimedOut()
        {
            // Открытие формы авторизации при таймауте сессии
            authorizeForm f = new authorizeForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик нажатия клавиш для сброса таймера сессии
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        // Обработчик движения мыши для сброса таймера сессии
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        // Обработчик ввода текста в expTableBox для предотвращения ввода
        private void expTableBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в fileNameBox для предотвращения ввода
        private void fileNameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в impTableNameBox для предотвращения ввода
        private void impTableNameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик закрытия окна для остановки сервиса таймаута
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }
    }
}
