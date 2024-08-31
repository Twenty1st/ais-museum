using museumAIS.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для setUserDataForm.xaml
    /// </summary>
    public partial class editGuidForm : Window
    {
        private static bool _isEdit; // Флаг режима редактирования
        private static string _guidID; // Идентификатор гида

        private static helpForDB db; // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        private bool isUpdatingText = false; // Флаг для предотвращения рекурсивного вызова обработки текста

        // Конструктор формы редактирования гида
        public editGuidForm(bool isEdit, string id)
        {
            InitializeComponent();

            // Инициализация объектов
            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            _isEdit = isEdit; // Установка режима редактирования
            _guidID = id; // Установка идентификатора гида

            if (isEdit)
            {
                setDataForCurID(); // Загрузка данных для текущего гида
                editButton.Content = "Изменить"; // Изменение текста кнопки
            }
        }

        // Метод для загрузки данных текущего гида
        private void setDataForCurID()
        {
            string query = "SELECT guid_name, guid_lastName, guid_patronamyc  " +
            "FROM guids WHERE guid_id =" + _guidID;
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            nameBox.Text = table.Rows[0][0].ToString();
            lastnameBox.Text = table.Rows[0][1].ToString();
            patronamycBox.Text = table.Rows[0][2].ToString();
        }

        // Метод для вставки данных о гиде
        private void insertData()
        {
            if (!isAllFill()) // Проверка на заполненность всех полей
            {           
                return;
            }

            // Получение значений из полей формы
            string name = nameBox.Text;
            string lastName = lastnameBox.Text;
            string patr = patronamycBox.Text;

            string query;

            // Создание SQL-запроса в зависимости от режима (редактирование/добавление)
            if (_isEdit)
            {
                query = "UPDATE guids SET guid_name = '" + name + "', " +
                    "guid_lastName = '" + lastName + "', guid_patronamyc = '" + patr + "' " +
                    "WHERE guid_id = " + _guidID;
            }
            else
            {
                query = "INSERT INTO guids (guid_name, guid_lastName, guid_patronamyc) " +
                " VALUES ('" + name + "', '" + lastName + "', '" + patr + "')";
            }

            // Выполнение запроса и обработка результата
            if (db.editDBData(query))
            {
                callMessageBox.ShowInfo("Успешно!");
                if (!_isEdit)
                {
                    clearField(); // Очистка полей формы, если это добавление нового гида
                }
                return;
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой!");
                return;
            }
        }

        // Метод для проверки заполненности всех необходимых полей
        private bool isAllFill()
        {
            if (nameBox.Text == "" || lastnameBox.Text == "")
            {
                callMessageBox.ShowWarning("Заполните все поля!");
                return false;
            }
            return true;
        }

        // Метод для очистки полей формы
        private void clearField()
        {
            nameBox.Text = "";
            lastnameBox.Text = "";
            patronamycBox.Text = "";
        }

        // Обработчик нажатия на кнопку редактирования/добавления гида
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            insertData(); // Вставка данных о гиде в базу данных
        }

        // Обработчик нажатия клавиши в поле фамилии (форматирование текста)
        private void lastnameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (isUpdatingText) return; // Предотвращение рекурсивного вызова

            isUpdatingText = true;
            TextBox textBox = sender as TextBox;
            int originalSelectionStart = textBox.SelectionStart;
            string formattedText = ToTitleCase(textBox.Text);

            if (textBox.Text != formattedText)
            {
                textBox.Text = formattedText;
                textBox.SelectionStart = originalSelectionStart;
            }

            isUpdatingText = false;
        }

        // Метод для преобразования текста в заглавный формат
        private string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            StringBuilder result = new StringBuilder();
            bool isNewWord = true;

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c) || c == '-')
                {
                    isNewWord = true;
                    result.Append(c);
                }
                else
                {
                    if (isNewWord)
                    {
                        result.Append(textInfo.ToUpper(c));
                        isNewWord = false;
                    }
                    else
                    {
                        result.Append(textInfo.ToLower(c));
                    }
                }
            }

            return result.ToString();
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрытие текущей формы
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

        // Обработчики для полей, позволяющие использование клавиши Backspace и пробел
        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void lastnameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void patronamycBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчики ввода текста в поля (позволяем ввод только кириллицы)
        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}]");
        }

        private void lastnameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}]");
        }

        private void patronamycBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}]");
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop(); // Остановка таймера сессии
        }

        // Обработчики изменения текста в полях для автоматического преобразования текста
        private void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (nameBox.Text.Length == 1)
                nameBox.Text = nameBox.Text.ToUpper(); // Преобразование первой буквы в заглавную
            nameBox.Select(nameBox.Text.Length, 0);
        }

        private void lastnameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text.Length > 0)
            {
                // Преобразуем первую букву в заглавную
                textBox.Text = char.ToUpper(textBox.Text[0]) + textBox.Text.Substring(1);
            }
            if (textBox.Text.Contains("-") && textBox.Text.Length > textBox.Text.IndexOf('-') + 1)
            {
                // Находим позицию дефиса, и делаем следующий символ заглавным
                int pos = textBox.Text.IndexOf('-');
                textBox.Text = textBox.Text.Remove(pos + 1, 1).Insert(pos + 1, textBox.Text[pos + 1].ToString().ToUpper());
            }
            textBox.Select(textBox.Text.Length, 0);
        }

        private void patronamycBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patronamycBox.Text.Length == 1)
                patronamycBox.Text = patronamycBox.Text.ToUpper(); // Преобразование первой буквы в заглавную
            patronamycBox.Select(patronamycBox.Text.Length, 0);
        }
    }
}
