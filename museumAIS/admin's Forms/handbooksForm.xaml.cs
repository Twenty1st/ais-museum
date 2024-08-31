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

namespace museumAIS.admin_s_Forms
{
    /// <summary>
    /// Interaction logic for handbooksForm.xaml
    /// </summary>
    public partial class handbooksForm : Window
    {
        // Сервис для отслеживания таймаута сессии
        private SessionTimeoutService _sessionTimeoutService;
        // Объект для взаимодействия с базой данных
        private static helpForDB db;

        // Запросы для получения данных из таблиц статусов и привилегий
        private static string st_query = "SELECT status_id as id, status_name as name FROM museum.status_ecsponat";
        private static string pr_query = "SELECT privilegeID as id, privilegeName as name FROM museum.user_privilege";

        // Текущий ID записи для редактирования
        private string _curID;
        // Список для хранения имен таблиц и полей базы данных
        private List<string> dbData;

        // Конструктор формы
        public handbooksForm()
        {
            InitializeComponent();

            // Инициализация объекта для работы с базой данных
            db = new helpForDB();

            // Инициализация сервиса таймаута сессии
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

            // Инициализация списка данных базы данных
            dbData = new List<string>();
            dbData.Add("user_privilege");
            dbData.Add("privilegeName");
            dbData.Add("privilegeID");
            dbData.Add(pr_query);

            // Загрузка данных привилегий пользователей
            LoadData(pr_query);
        }

        // Метод для загрузки данных из базы данных по заданному запросу
        private void LoadData(string query)
        {
            // Выполнение запроса к базе данных
            DataTable table = db.selectData(query);
            // Проверка на успешность выполнения запроса
            if (table == null || table.Rows.Count == 0)
            {
                // Если запрос не удался, показываем сообщение об ошибке
                callMessageBox.ShowError("Ошибка подключения к базе!");
                return;
            }

            // Установка данных в DataGrid
            dataGridV.ItemsSource = table.DefaultView;
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на форму главного администратора
            mainAdministratorForm f = new mainAdministratorForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик таймаута сессии
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

        // Обработчик закрытия окна для остановки сервиса таймаута
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }

        // Метод для очистки поля ввода и снятия выделения в DataGrid
        private void clearField()
        {
            nameBox.Text = "";
            dataGridV.SelectedItem = null;
        }

        // Метод для вставки или обновления данных в базе данных
        private void insertData(bool isEdit)
        {
            string name = nameBox.Text;

            string query;

            // Формирование запроса для обновления или вставки данных
            if (isEdit)
            {
                query = "UPDATE " + dbData[0] + " SET " + dbData[1] + " = '" + name + "' WHERE " + dbData[2] + " = " + _curID;
            }
            else
            {
                query = "INSERT INTO " + dbData[0] + " (" + dbData[1] + ") VALUES ('" + name + "')";
            }

            // Выполнение запроса и проверка на успешность
            if (db.editDBData(query))
            {
                // Успешное выполнение запроса
                callMessageBox.ShowInfo("Успешно!");
                if (!isEdit)
                {
                    // Очистка поля ввода при добавлении новой записи
                    clearField();
                }
                LoadData(dbData[3]);
                return;
            }
            else
            {
                // Ошибка выполнения запроса
                callMessageBox.ShowError("Ошибка при работе с базой!");
                return;
            }
        }

        // Обработчик кнопки добавления данных
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустое поле ввода
            if (nameBox.Text == "")
            {
                callMessageBox.ShowWarning("Поле пустое!");
                return;
            }
            // Вставка данных в базу
            insertData(false);
        }

        // Обработчик кнопки редактирования данных
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустое поле ввода
            if (nameBox.Text == "")
            {
                callMessageBox.ShowWarning("Поле пустое!");
                return;
            }
            // Обновление данных в базе
            insertData(true);
        }

        // Обработчик изменения выделения в DataGrid
        private void dataGridV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверка на выбранный элемент
            if (dataGridV.SelectedItem is DataRowView rowView)
            {
                // Установка текущего ID и текста поля ввода
                _curID = rowView[0].ToString();
                nameBox.Text = rowView[1].ToString();
                // Показ кнопки редактирования
                //editButton.Visibility = Visibility.Visible;
            }
        }

        // Обработчик ввода текста для ограничения ввода только кириллическими символами
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем ввод только кириллических символов
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}]");
        }

        // Обработчик нажатия клавиш для разрешения использования клавиш Backspace и пробела
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Разрешаем использование клавиш Backspace и пробела
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчик нажатия мыши для очистки поля ввода и скрытия кнопки редактирования
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clearField();
            editButton.Visibility = Visibility.Hidden;
        }

        // Обработчик кнопки выбора данных привилегий
        private void prButton_Click(object sender, RoutedEventArgs e)
        {
            // Установка цвета фона для выбранной кнопки
            SolidColorBrush selectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD7B2"));
            prButton.Background = selectedColor;
            stButton.Background = Brushes.White;

            // Обновление списка данных базы данных
            dbData = new List<string>();
            dbData.Add("user_privilege");
            dbData.Add("privilegeName");
            dbData.Add("privilegeID");
            dbData.Add(pr_query);
            // Загрузка данных привилегий
            LoadData(pr_query);
        }

        // Обработчик кнопки выбора данных статусов
        private void stButton_Click(object sender, RoutedEventArgs e)
        {
            // Установка цвета фона для выбранной кнопки
            SolidColorBrush selectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD7B2"));
            stButton.Background = selectedColor;
            prButton.Background = Brushes.White;

            // Обновление списка данных базы данных
            dbData = new List<string>();
            dbData.Add("status_ecsponat");
            dbData.Add("status_name");
            dbData.Add("status_id");
            dbData.Add(st_query);
            // Загрузка данных статусов
            LoadData(st_query);
        }

        private bool checkDependence()
        {
            string query;
            if (dbData[3] == st_query)
            {
                query = "SELECT COUNT(*) FROM ecsponat WHERE status_ecsponat = " + _curID;
            }
            else
            {
                query = "SELECT COUNT(*) FROM users WHERE privilege_user = " + _curID;
            }


            // Выполнение запроса к базе данных
            DataTable table = db.selectData(query);
            // Проверка на успешность выполнения запроса
            if (table == null || table.Rows.Count == 0)
            {
                // Если запрос не удался, показываем сообщение об ошибке
                callMessageBox.ShowError("Ошибка подключения к базе!");
                return true;
            }

            if (table.Rows[0][0].ToString() != "0")
            {
                return true;
            }
            return false;
        }

        // Удаление строки
        private void deleteRow()
        {
            if (checkDependence())
            {
                callMessageBox.ShowWarning("Нельзя удалить зависимые данные! Сначала уберите зависимость.");
                return;
            }

            string query = $"DELETE FROM {dbData[0]} WHERE {dbData[2]} = {_curID}";
            if (db.editDBData(query))
            {
                callMessageBox.ShowInfo("Успешно!");
                LoadData(dbData[3]);
                editButton.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой данных!");
                return;
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текущую выделенную строку
            if (dataGridV.SelectedItem is DataRowView rowView)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Логика, если пользователь нажмет "Да"
                    deleteRow();
                }
            }
        }
    }
}
