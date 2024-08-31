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
    /// Interaction logic for dataBaseWorkForm.xaml
    /// </summary>
    public partial class dataBaseWorkForm : Window
    {
        // Статический объект для работы с базой данных
        private static helpForDB db;

        // Сервис для отслеживания таймаута сессии
        private SessionTimeoutService _sessionTimeoutService;

        // Конструктор формы
        public dataBaseWorkForm()
        {
            InitializeComponent();

            // Инициализация объекта для работы с базой данных
            db = new helpForDB();

            // Инициализация сервиса таймаута сессии
            _sessionTimeoutService = new SessionTimeoutService();

            // Подписка на событие таймаута сессии
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

            // Загрузка списка файлов резервных копий
            getFiles();
        }

        // Метод для получения списка файлов резервных копий
        private void getFiles()
        {
            // Путь к папке с резервными копиями
            string folderPath = chooseDirectories.dirBackup;

            // Создание папки, если она не существует
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Очистка списка файлов резервных копий
            dumpBox.Items.Clear();

            // Забираем только файлы с расширением .sql и добавляем их в ListBox или ComboBox
            foreach (string file in Directory.GetFiles(folderPath, "*.sql"))
            {
                dumpBox.Items.Add(System.IO.Path.GetFileName(file));
            }
        }

        // Обработчик нажатия кнопки "Назад"
        private void goBackButton_Click(object sender, RoutedEventArgs e)
        {
            // Создание и показ формы администратора
            mainAdministratorForm f = new mainAdministratorForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик нажатия кнопки "Импорт"
        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, выбран ли файл
            if (dumpBox.Text == "")
            {
                callMessageBox.ShowWarning("Файл не выбран!");
                return;
            }

            // Путь к выбранному файлу резервной копии
            string filePath = Directory.GetCurrentDirectory() + "\\backup\\" + dumpBox.Text;

            // Импорт резервной копии базы данных
            if (db.importBackup(filePath))
            {
                callMessageBox.ShowInfo("База данных успешно загружена!");
            }
            else
            {
                callMessageBox.ShowError("Ошибка при загрузке базы!");
            }
        }

        // Обработчик нажатия кнопки "Создать резервную копию"
        private void createDumpButton_Click(object sender, RoutedEventArgs e)
        {
            // Создание резервной копии базы данных
            if (db.createBackup())
            {
                callMessageBox.ShowInfo("База данных успешно сохранена!");

                // Обновление списка файлов резервных копий
                getFiles();
            }
            else
            {
                callMessageBox.ShowError("Произошла ошибка!");
            }
        }

        // Обработчик события таймаута сессии
        private void OnSessionTimedOut()
        {
            // Открытие формы авторизации при таймауте сессии
            authorizeForm f = new authorizeForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик нажатия клавиши в окне
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Сброс таймера сессии при активности пользователя
            _sessionTimeoutService.Reset();
        }

        // Обработчик движения мыши в окне
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Сброс таймера сессии при активности пользователя
            _sessionTimeoutService.Reset();
        }

        // Обработчик ввода текста в поле выбора файла
        private void dumpBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Блокировка ввода текста в поле выбора файла
            e.Handled = true;
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            // Остановка сервиса таймаута сессии при закрытии окна
            _sessionTimeoutService.Stop();
        }
    }
}

