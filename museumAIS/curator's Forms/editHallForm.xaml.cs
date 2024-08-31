using museumAIS.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace museumAIS.curator_s_Forms
{
    /// <summary>
    /// Interaction logic for editHallForm.xaml
    /// </summary>
    public partial class editHallForm : Window
    {
        private static string _hallID; // Идентификатор зала
        private static bool _isEdit; // Флаг режима редактирования

        private static helpForDB db; // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        // Конструктор формы редактирования зала
        public editHallForm(bool isEdit, string id)
        {
            InitializeComponent();

            // Инициализация объектов
            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            _isEdit = isEdit; // Установка режима редактирования
            _hallID = id; // Установка идентификатора зала

            if (isEdit)
            {
                setDataForCurID(); // Загрузка данных для текущего зала
                saveButton.Content = "Изменить"; // Изменение текста кнопки
            }
        }

        // Метод для загрузки данных текущего зала
        private void setDataForCurID()
        {
            string query = "SELECT hall_name FROM halls WHERE hall_num =" + _hallID;
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return;
            }

            nameBox.Text = table.Rows[0][0].ToString();
        }

        // Метод для вставки данных о зале
        private void insertData()
        {
            if (!isAllFill()) // Проверка на заполненность всех полей
            {
                return;
            }

            // Получение значения из поля формы
            string name = nameBox.Text;

            string query;

            // Создание SQL-запроса в зависимости от режима (редактирование/добавление)
            if (_isEdit)
            {
                query = "UPDATE halls SET hall_name = '" + name + "' WHERE hall_num = " + _hallID;
            }
            else
            {
                query = "INSERT INTO halls (hall_name) VALUES ('" + name + "')";
            }

            // Выполнение запроса и обработка результата
            if (db.editDBData(query))
            {
                callMessageBox.ShowInfo("Успешно!");
                if (!_isEdit)
                {
                    clearField(); // Очистка полей формы, если это добавление нового зала
                }
                return;
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой данных!");
                return;
            }
        }

        // Метод для проверки заполненности всех необходимых полей
        private bool isAllFill()
        {
            if (nameBox.Text == "")
            {
                callMessageBox.ShowWarning("Заполните поле!");
                return false;
            }
            return true;
        }

        // Метод для очистки полей формы
        private void clearField()
        {
            nameBox.Text = "";
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрытие текущей формы
        }

        // Обработчик нажатия на кнопку сохранения/добавления зала
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            insertData(); // Вставка данных о зале в базу данных
        }

        // Обработчик события таймаута сессии
        private void OnSessionTimedOut()
        {
            // Открытие формы авторизации
            authorizeForm f = new authorizeForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик события нажатия клавиши в окне
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _sessionTimeoutService.Reset(); // Сброс таймера сессии
        }

        // Обработчик события движения мыши в окне
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset(); // Сброс таймера сессии
        }

        // Обработчик нажатия клавиши в поле имени зала (разрешаем только Backspace и пробел)
        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчик ввода текста в поле имени зала (позволяем ввод только кириллицы, цифр и дефисов)
        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\-]");
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop(); // Остановка таймера сессии
        }
    }
}
