using museumAIS.Classes;
using museumAIS.userControls;
using System;
using System.Collections.Generic;
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

namespace museumAIS.curator_s_Forms
{
    /// <summary>
    /// Interaction logic for editExcurForm.xaml
    /// </summary>
    public partial class editExcurForm : Window
    {
        private static string _excID; // Идентификатор экскурсии
        private static bool _isEdit;  // Флаг режима редактирования

        private static helpForDB db;  // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        private List<string> guidIDList; // Список идентификаторов гидов
        private List<string> hallIDList; // Список идентификаторов залов

        private DataTable hallTable; // Таблица данных о залах

        // Конструктор формы редактирования экскурсии
        public editExcurForm(bool isEdit, string id)
        {
            InitializeComponent();

            // Инициализация объектов
            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            _isEdit = isEdit; // Установка режима редактирования
            _excID = id; // Установка идентификатора экскурсии

            hallIDList = new List<string>();

            loadDatainCmbBox(); // Загрузка данных в комбобоксы
            // Если режим редактирования, то заполняем поля данными текущей экскурсии
            if (isEdit)
            {
                editButton.Content = "Изменить";
                setDataForCurID(); // Загрузка данных для текущей экскурсии
            }
            LoadHallData(); // Загрузка данных о залах

        }

        // Метод для загрузки данных в комбобоксы
        private void loadDatainCmbBox()
        {
            string query = "SELECT * FROM guids";
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return;
            }
            guidIDList = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                string fio = row["guid_lastName"].ToString() + " " + row["guid_name"].ToString() + " " + row["guid_patronamyc"].ToString();
                guidsBox.Items.Add(fio);
                guidIDList.Add(row["guid_id"].ToString());
            }

            durationBox.Items.Add("1");
            durationBox.Items.Add("2");
            durationBox.Items.Add("3");
            durationBox.Items.Add("4");

            timeBox.Items.Add("09:00");
            timeBox.Items.Add("10:00");
            timeBox.Items.Add("11:00");
            timeBox.Items.Add("12:00");
            timeBox.Items.Add("13:00");
            timeBox.Items.Add("14:00");
            timeBox.Items.Add("15:00");
            timeBox.Items.Add("16:00");
            timeBox.Items.Add("17:00");

            startDatePicker.DisplayDateStart = DateTime.Today;
            startDatePicker.DisplayDateEnd = DateTime.Today.AddDays(60);

        }

        // Метод для загрузки данных текущей экскурсии
        private void setDataForCurID()
        {
            string query = "SELECT exc_name, guid_lastName, guid_name, guid_patronamyc, exc_time, " +
            "exc_start_date, exc_end_date, exc_duration  " +
            "FROM excursions " +
            "INNER JOIN guids ON guid_id = exc_gid WHERE exc_id =" + _excID;
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            foreach (DataRow row in table.Rows)
            {
                row["guid_lastName"] += " " + row["guid_name"].ToString() + " " + row["guid_patronamyc"].ToString();
            }

            nameBox.Text = table.Rows[0][0].ToString();
            startDatePicker.Text = table.Rows[0][5].ToString();
            endDatePicker.Text = table.Rows[0][6].ToString();
            durationBox.Text = table.Rows[0][7].ToString();
            guidsBox.Text = table.Rows[0][1].ToString();
            timeBox.Text = table.Rows[0][4].ToString();

            query = "SELECT hall_name FROM museum.excursion_halls " +
                "JOIN halls ON num_hall = hall_num " +
                "WHERE id_excursion = " + _excID;
            table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            List<string> hallList = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                hallList.Add(row["hall_name"].ToString());
            }
            hallsBox.Text = string.Join(", ", hallList);
        }

        // Метод для получения свободного времени для экскурсий
        private void getFreeTime()
        {
            string query;
            if (_isEdit)
            {
                query = "SELECT exc_time, exc_duration FROM excursions " +
                                "WHERE ((exc_start_date <= '" + startDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "' AND " +
                                " exc_end_date >= '" + startDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "') " +
                                " OR (exc_start_date <= '" + endDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "' AND " +
                                " exc_end_date >= '" + endDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "')) " +
                                "AND NOT exc_id = " + _excID;

            }
            else
            {
                query = "SELECT exc_time, exc_duration FROM excursions " +
                                "WHERE (exc_start_date <= '" + startDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "' AND " +
                                " exc_end_date >= '" + startDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "') " +
                                " OR (exc_start_date <= '" + endDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "' AND " +
                                " exc_end_date >= '" + endDatePicker.DisplayDate.Date.ToString("yyyy-MM-dd") + "')";
            }

            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            List<string> timesToRemove = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                string excTime = row["exc_time"].ToString();
                int excDuration = Convert.ToInt32(row["exc_duration"]);

                DateTime startTime = DateTime.ParseExact(excTime, "HH:mm", CultureInfo.InvariantCulture);
                for (int i = 0; i <= excDuration; i++)
                {
                    string timeToRemove = startTime.AddMinutes(i * 60).ToString("HH:mm");
                    if (timeBox.Items.Contains(timeToRemove))
                    {
                        timesToRemove.Add(timeToRemove);
                    }
                }
            }

            foreach (string time in timesToRemove.Distinct())
            {
                timeBox.Items.Remove(time);
            }

        }

        // Метод для загрузки данных о залах
        private void LoadHallData()
        {
            string query = "SELECT hall_num, hall_name FROM halls";
            hallTable = db.selectData(query);
            if (hallTable == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            List<hallItem> hallItems = new List<hallItem>();

            // Создаем список всех залов
            foreach (DataRow row in hallTable.Rows)
            {
                hallItems.Add(new hallItem
                {
                    ID = row["hall_num"].ToString(),
                    HallName = row["hall_name"].ToString(),
                    Enabled = false
                });
            }

            // Имея список всех залов, теперь необходимо пометить те, которые уже выбраны 
            // Извлекаем список названий уже выбранных залов
            if (_isEdit)
            {
                string[] selectedHalls = hallsBox.Text.Split(',');

                // Для каждого выбранного зала находим соответствующий hallItem и устанавливаем Enabled в true
                foreach (string hallName in selectedHalls)
                {
                    foreach (hallItem item in hallItems)
                    {
                        if (item.HallName.Trim() == hallName.Trim())
                        {
                            item.Enabled = true;
                            break;
                        }
                    }
                }
            }

            // Применяем список объектов hallItem к ItemSource ListView
            HallListView.ItemsSource = null;
            HallListView.ItemsSource = hallItems;
        }

        // Метод для вставки данных об экскурсии
        private void insertData()
        {
            if (!isAllFill()) // Проверка на заполненность всех полей
            {
                return;
            }

            // Получение значений из полей формы
            string name = nameBox.Text;
            string guid = guidIDList[guidsBox.SelectedIndex];
            string excTime = timeBox.Text;
            string startDate = startDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            string endDate = endDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            string excDuration = durationBox.Text;

            string query;

            // Создание SQL-запроса в зависимости от режима (редактирование/добавление)
            if (_isEdit)
            {
                query = "DELETE FROM excursion_halls WHERE id_excursion = " + _excID;

                if (!db.editDBData(query))
                {
                    callMessageBox.ShowError("Ошибка при работе с базой!");
                    return;
                }

                query = "UPDATE excursions SET exc_name = '" + name + "', exc_gid = " + guid + ", " +
                    "exc_time = '" + excTime + "', exc_start_date = '" + startDate + "', exc_end_date = '" + endDate + "', " +
                    "exc_duration = " + excDuration + " WHERE exc_id = " + _excID;
            }
            else
            {
                query = "INSERT INTO excursions (exc_name, exc_gid, exc_time, exc_start_date, exc_end_date, exc_duration) " +
                " VALUES (" +
                " '" + name + "', " + guid + ", '" + excTime + "', '" + startDate + "', " + "'" + endDate + "'," +
                " " + excDuration + ")";
            }

            // Выполнение транзакции для сохранения экскурсии и связанных залов
            if (db.transactionExcursion(query, hallIDList))
            {
                callMessageBox.ShowInfo("Успешно!");
                if (!_isEdit)
                {
                    clearField(); // Очистка полей формы, если это добавление новой экскурсии
                }
                return;
            }
            else
            {
                callMessageBox.ShowError("Произошла ошибка!\nНевозможно выполнить запрос.");
                return;
            }
        }

        // Метод для проверки заполненности всех необходимых полей
        private bool isAllFill()
        {
            if (nameBox.Text == "" || timeBox.Text == "" || durationBox.Text == "" || 
                guidsBox.SelectedIndex == -1 || !startDatePicker.SelectedDate.HasValue || 
                !endDatePicker.SelectedDate.HasValue || hallsBox.Text == "")
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
            startDatePicker.Text = "";
            endDatePicker.Text = "";
            endDatePicker.IsEnabled = false;
            timeBox.IsEnabled = false;
            timeBox.SelectedIndex = -1;
            durationBox.SelectedIndex = -1;
            guidsBox.SelectedIndex = -1;
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            excursForm f = new excursForm(); // Создание новой формы для списка экскурсий
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие новой формы в диалоговом режиме
        }

        // Обработчик изменения выбранной даты начала экскурсии
        private void startDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(startDatePicker.SelectedDate == null)
            {
                return;
            }
            endDatePicker.IsEnabled = true; // Включение доступности выбора даты окончания
            endDatePicker.Text = startDatePicker.SelectedDate.Value.AddDays(1).ToString();
            endDatePicker.DisplayDateStart = startDatePicker.DisplayDate.AddDays(1);
            endDatePicker.DisplayDateEnd = startDatePicker.DisplayDate.AddDays(101);
        }

        // Обработчик изменения выбранной даты окончания экскурсии
        private void endDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Этот метод пока пуст, но можно добавить логику при изменении даты окончания
        }

        // Обработчик закрытия календаря даты окончания экскурсии
        private void endDatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            getFreeTime(); // Получение доступного времени для экскурсии
            timeBox.IsEnabled = true; // Включение доступности выбора времени
        }

        // Обработчик нажатия на кнопку редактирования/добавления экскурсии
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            insertData(); // Вставка данных об экскурсии в базу данных
        }

        // Обработчик двойного клика по элементу списка залов
        private void hallsView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HallListView.SelectedItem is hallItem item)
            {
                if (item != null)
                {
                    // Добавление или удаление зала в списке выбранных залов
                    if (!item.Enabled)
                    {
                        if (hallsBox.Text == "")
                        {
                            hallsBox.Text = item.HallName;
                        }
                        else
                        {
                            hallsBox.Text += ", " + item.HallName;
                        }
                        hallIDList.Add(item.ID);
                    }
                    else
                    {
                        // Если элемент уже есть в TextBox, удаляем его
                        string currentText = hallsBox.Text;

                        var lines = currentText.Split(',').ToList();
                        for (int i = 0; i < lines.Count; i++) { lines[i] = lines[i].Trim(); }
                        lines.Remove(item.HallName);
                        hallsBox.Text = string.Join(", ", lines);
                        hallIDList.Remove(item.ID);
                    }
                    item.Enabled = !item.Enabled; // Переключение состояния зала
                }
            }
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

        // Обработчик события нажатия клавиши в поле имени экскурсии
        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчик ввода текста в поле списка залов (запрещаем ввод текста)
        private void hallsBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в поле продолжительности экскурсии (запрещаем ввод текста)
        private void durationBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в поле даты начала экскурсии (запрещаем ввод текста)
        private void startDatePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в поле даты окончания экскурсии (запрещаем ввод текста)
        private void endDatePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в поле времени экскурсии (запрещаем ввод текста)
        private void timeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в поле гида (запрещаем ввод текста)
        private void guidsBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик ввода текста в поле имени экскурсии (позволяем только кириллицу и цифры)
        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]");
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop(); // Остановка таймера сессии
        }

        // Обработчик загрузки списка залов
        private void HallListView_Loaded(object sender, RoutedEventArgs e)
        {
            // Этот метод пока пуст, но можно добавить логику при загрузке списка залов
        }

        // Метод для проверки времени работы гида
        private void checkGuidTime()
        {
            // Получаем идентификатор гида
            if (guidsBox.SelectedIndex < 0)
            {
                return;
            }
            string guid = guidIDList[guidsBox.SelectedIndex];

            // Вычисляем время работы гида по месяцам
            var workTimeByMonth = CalculateGuidWorkTimeByMonth(guid);

            // Проходим по всем месяцам и ищем те, в которых время работы превышает 100 часов
            foreach (var monthHours in workTimeByMonth)
            {
                if (monthHours.Value > 100)
                {
                    callMessageBox.ShowWarning("Гид работает больше 100 часов в месяце: " + monthHours.Key);
                    guidsBox.SelectedIndex = -1;
                    return;
                }
            }
            
        }

        // Метод для вычисления времени работы гида по месяцам
        private Dictionary<int, int> CalculateGuidWorkTimeByMonth(string guid)
        {
            string query = "SELECT exc_start_date, exc_end_date, exc_duration FROM excursions WHERE exc_gid = " + guid;
            if (_isEdit)
            {
                query += " AND NOT exc_id = " + _excID;
            }
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return null;
            }

            Dictionary<int, int> workTimeByMonth = new Dictionary<int, int>();
            int duration;
            foreach (DataRow row in table.Rows)
            {
                DateTime current = DateTime.Parse(row["exc_start_date"].ToString());
                DateTime end = DateTime.Parse(row["exc_end_date"].ToString());
                duration = int.Parse(row["exc_duration"].ToString());

                while (current <= end)
                {
                    int month = current.Month;
                    if (!workTimeByMonth.ContainsKey(month))
                    {
                        workTimeByMonth[month] = 0;
                    }

                    workTimeByMonth[month] += duration;

                    current = current.AddDays(1);
                }
            }

            // Учет текущей экскурсии
            duration = Convert.ToInt32(durationBox.Text);
            DateTime current2 = startDatePicker.SelectedDate.Value;
            while (current2 <= endDatePicker.SelectedDate.Value)
            {
                int month = current2.Month;
                if (!workTimeByMonth.ContainsKey(month))
                {
                    workTimeByMonth[month] = 0;
                }
                workTimeByMonth[month] += duration;

                current2 = current2.AddDays(1);
            }

            return workTimeByMonth;
        }

        // Обработчик изменения выбранного гида
        private void guidsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (endDatePicker.Text == "" || durationBox.Text == "")
            {
                guidsBox.SelectedIndex = -1;
                callMessageBox.ShowWarning("Заполните поля даты и продолжительность!");
                return;
            }

            checkGuidTime(); // Проверка времени работы выбранного гида
        }
    }
}
