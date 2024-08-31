using museumAIS.Classes;
using museumAIS.userControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using System.Windows.Threading;

namespace museumAIS.curator_s_Forms
{
    /// <summary>
    /// Interaction logic for excursForm.xaml
    /// </summary>
    public partial class excursForm : Window
    {
        private DataTable calendarTable; // Таблица данных календаря
        private DataTable excursTable; // Таблица данных экскурсий
        private DataTable excursInCurDayTable; // Таблица экскурсий в текущий день
        private DateTime currentDate; // Текущая дата

        private static helpForDB db; // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        private List<calendarItem> calendarItems = new List<calendarItem>(); // Список элементов календаря

        private bool isVisible = true; // Флаг видимости календаря

        private static string full_query = "SELECT exc_id, exc_name, guid_lastName, guid_name, guid_patronamyc, exc_time, " +
            "exc_start_date, exc_end_date, exc_duration  " +
            "FROM excursions " +
            "INNER JOIN guids ON guid_id = exc_gid ";
        private string search_query = ""; // Поисковый запрос

        private readonly int row_amount = 15; // Количество строк на странице
        private int allPages = 12; // Общее количество страниц
        private int currentPage = 3; // Текущая страница

        private DateTime startDate; // Дата начала
        private DateTime endDate; // Дата окончания
        private string _curID; // Текущий идентификатор

        // Конструктор формы управления экскурсиями
        public excursForm()
        {
            InitializeComponent();

            excursDataGrid.ItemsSource = null; // Очистка источника данных

            db = new helpForDB(); // Инициализация объекта для работы с базой данных
            _sessionTimeoutService = new SessionTimeoutService(); // Инициализация сервиса таймаута сессии
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            pagin_init(); // Инициализация пагинации
            loadExcursData(); // Загрузка данных экскурсии
            LoadCalendarItems(); // Загрузка элементов календаря
            generateExcursCurDayDataTable(); // Генерация таблицы экскурсий на текущий день
        }

        // Инициализация пагинации
        private void pagin_init()
        {
            try
            {
                this.DataContext = paginationControl;

                currentDate = DateTime.Now.Date;
                startDate = currentDate.AddDays(-7); // Неделя до текущей даты
                endDate = currentDate.AddDays(27); // 4 недели после текущей даты

                paginationControl.PrevPageNum = "2";
                paginationControl.CurPageNum = "3";
                paginationControl.NextPageNum = "4";

                // Рассчитываем общее количество страниц, исходя из количества дней между начальной и конечной датами
                int totalDays = (currentDate.AddMonths(9) - currentDate.AddMonths(-2)).Days;
                allPages = (int)Math.Ceiling(totalDays / (double)(7 * 4)); // Количество страниц, если каждая страница содержит 4 недели
            }
            catch (Exception) // Обработка ошибок
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
            }
        }

        // Загрузка данных о экскурсиях
        private void loadExcursData()
        {
            string query = full_query;
            if (search_query != "")
            {
                query = search_query;
            }
            excursTable = db.selectData(query);
            if (excursTable == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return;
            }

            // Создание нового столбца
            DataColumn newCol = new DataColumn("newStartDate", typeof(string));
            // Добавление нового столбца в таблицу
            excursTable.Columns.Add(newCol);
            // Установка порядка столбцов
            newCol.SetOrdinal(6);
            // Создание нового столбца
            newCol = new DataColumn("newEndDate", typeof(string));
            // Добавление нового столбца в таблицу
            excursTable.Columns.Add(newCol);
            // Установка порядка столбцов
            newCol.SetOrdinal(7);
            // Преобразование даты в строку и сохранение в новом столбце
            foreach (DataRow row in excursTable.Rows)
            {
                row["newStartDate"] = row["exc_start_date"].ToString();
                row["newEndDate"] = row["exc_end_date"].ToString();

                row["guid_lastName"] += " " + row["guid_name"].ToString() + " " + row["guid_patronamyc"].ToString();
            }

            excursTable.Columns.RemoveAt(3);
            excursTable.Columns.RemoveAt(3);
            excursTable.Columns.RemoveAt(6);
            excursTable.Columns.RemoveAt(6);
        }

        // Загрузка данных для поиска
        private void LoadSearchData()
        {
            loadExcursData();
            setDatainDataG(excursTable);
        }

        // Генерация таблицы экскурсий на текущий день
        private void generateExcursCurDayDataTable()
        {
            excursInCurDayTable = new DataTable();
            excursInCurDayTable.Columns.Add("exc_id");
            excursInCurDayTable.Columns.Add("exc_name");
            excursInCurDayTable.Columns.Add("guid_lastName");
            excursInCurDayTable.Columns.Add("exc_time");
            excursInCurDayTable.Columns.Add("newStartDate");
            excursInCurDayTable.Columns.Add("newEndDate");
            excursInCurDayTable.Columns.Add("exc_duration");
        }

        // Загрузка элементов календаря
        private void LoadCalendarItems()
        {
            CalendarView.ItemsSource = null;
            calendarItems.Clear();
            DateTime startDate = this.startDate;
            DateTime endDate = this.endDate;
            CultureInfo russianCulture = new CultureInfo("ru-RU");

            while (startDate <= endDate)
            {
                calendarItem item = new calendarItem
                {
                    Date = startDate,
                    DayOfWeek = startDate.ToString("dddd", new CultureInfo("ru-RU")), // Название дня недели на русском
                    Excursions = getExcursForCurDay(startDate)
                };

                if (startDate < currentDate)
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(226, 227, 232)); // Светло-серый для прошедших дат
                }
                else if (startDate == currentDate)
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // Светло-голубой для текущей даты
                }

                calendarItems.Add(item);
                startDate = startDate.AddDays(1);
            }

            CalendarView.ItemsSource = calendarItems;
        }

        // Получение экскурсий на текущий день
        private string getExcursForCurDay(DateTime current_date)
        {
            string excurs = "";
            int count = 1;

            foreach (DataRow row in excursTable.Rows)
            {
                DateTime startDate = Convert.ToDateTime(row["newStartDate"]);
                DateTime endDate = Convert.ToDateTime(row["newEndDate"]);

                if (current_date >= startDate && current_date <= endDate)
                {
                    excurs += $"{count}) {row["exc_name"]}\n";
                    count++;
                }
            }

            return excurs;
        }

        // Загрузка данных о мероприятиях из базы данных на определенную дату
        private void LoadEventDataFromDatabase(DateTime date)
        {
            excursInCurDayTable.Clear();

            foreach (DataRow row in excursTable.Rows)
            {
                DateTime startDate = Convert.ToDateTime(row[4]);
                DateTime endDate = Convert.ToDateTime(row[5]);

                if (date >= startDate && date <= endDate)
                {
                    excursInCurDayTable.ImportRow(row);
                }
            }

            setDatainDataG(excursInCurDayTable);
        }

        // Установка данных в DataGrid
        private void setDatainDataG(DataTable table)
        {
            foreach (DataRow rowView in table.Rows)
            {
                string startDate = DateTime.Parse(rowView[4].ToString()).ToString("dd MMM yyyy");
                string endDate = DateTime.Parse(rowView[5].ToString()).ToString("dd MMM yyyy");

                rowView[4] = startDate;
                rowView[5] = endDate;
            }

            excursDataGrid.ItemsSource = table.DefaultView;

            excursDataGrid.Columns[1].Header = "Наименование";
            excursDataGrid.Columns[2].Header = "Экскурсовод";
            excursDataGrid.Columns[3].Header = "Время начала";
            excursDataGrid.Columns[4].Header = "Дата старта";
            excursDataGrid.Columns[5].Header = "Дата окончания";
            excursDataGrid.Columns[6].Header = "Продолжительность (ч.)";

            excursDataGrid.Columns[0].Visibility = Visibility.Collapsed;

            foreach (DataRowView rowView in excursDataGrid.ItemsSource)
            {
                string startDate = DateTime.Parse(rowView[4].ToString()).ToString("dd MMM yyyy");
                string endDate = DateTime.Parse(rowView[5].ToString()).ToString("dd MMM yyyy");

                rowView[4] = startDate;
                rowView[5] = endDate;
            }
        }

        // Подсветка элемента календаря
        private void HighlightItem(DateTime startDate, DateTime endDate)
        {
            foreach (var item in calendarItems)
            {
                if (item.Date >= startDate && item.Date <= endDate)
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(102, 255, 163)); //  для выбранного элемента
                }
                else
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); // Белый для невыбранных элементов
                }
            }
        }

        // Проверка дат экскурсий
        private void checkExcursDate()
        {
            if (!isVisible)
            {
                DateTime today = DateTime.Now.Date;

                foreach (DataRowView rowView in excursDataGrid.ItemsSource)
            {
                    DateTime startDate = DateTime.Parse(rowView["newStartDate"].ToString());
                    DateTime endDate = DateTime.Parse(rowView["newEndDate"].ToString());

                    if (today > endDate)
                    {
                        var row = excursDataGrid.ItemContainerGenerator.ContainerFromItem(rowView) as DataGridRow;
                        if (row != null)
                        {
                            row.Background = new SolidColorBrush(Color.FromRgb(252, 184, 184)); // Светло-красный для прошедших дат
                        }
                    }
                    else if (today < startDate)
                    {
                        var row = excursDataGrid.ItemContainerGenerator.ContainerFromItem(rowView) as DataGridRow;
                        if (row != null)
                        {
                            row.Background = new SolidColorBrush(Color.FromRgb(217, 217, 217)); // Серый для будущих дат
                        }
                    }
                }
            }
        }

        // Получение залов для текущей экскурсии
        private void getHallsForCurExcur(string excur_id)
        {
            string query = "SELECT hall_name FROM museum.excursion_halls " +
                "JOIN halls ON num_hall = hall_num " +
                "WHERE id_excursion = " + excur_id;
            DataTable table = db.selectData(query);
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

        // Удаление строки (экскурсии)
        private void deleteRow()
        {
            string query = "DELETE FROM excursions WHERE exc_id = " + _curID;
            if (db.editDBData(query))
            {
                callMessageBox.ShowInfo("Успешно!");
                editButton.Visibility = Visibility.Hidden;
            }
            else
            {

                callMessageBox.ShowError("Ошибка при работе с базой!");
                return;
            }
            loadExcursData();
            setDatainDataG(excursTable);
        }

        // Обработчики пользовательских событий

        #region [pagination]

        // Переход на первую страницу
        private void paginationControl_firstPageBut_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != 1)
            {
                if (currentPage - 2 >= 1)
                {
                    currentPage -= 2;
                    setNewPage();
                }
                else if (currentPage - 1 == 1)
                {
                    currentPage--;
                    setNewPage();
                }
            }
        }

        // Переход на предыдущую страницу
        private void paginationControl_prevPageBut_Click(object sender, RoutedEventArgs e)
        {
            if (paginationControl.PrevPageNum == "")
            {
                return;
            }
            currentPage--;
            setNewPage();
        }

        // Переход на следующую страницу
        private void paginationControl_nextPageBut_Click(object sender, RoutedEventArgs e)
        {
            if (paginationControl.NextPageNum == "")
            {
                return;
            }
            currentPage++;
            setNewPage();
        }

        // Переход на последнюю страницу
        private void paginationControl_lastPageBut_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != allPages)
            {
                if (currentPage + 2 <= allPages)
                {
                    currentPage += 2;
                    setNewPage();
                }
                else if (currentPage + 1 == allPages)
                {
                    currentPage++;
                    setNewPage();
                }
            }
        }

        // Установка новой страницы
        private void setNewPage()
        {
            paginationControl.PrevPageNum = "";
            paginationControl.NextPageNum = "";

            if (currentPage != 1)
            {
                paginationControl.PrevPageNum = "" + (currentPage - 1);
            }

            paginationControl.CurPageNum = "" + currentPage;
            if (currentPage != allPages)
            {
                paginationControl.NextPageNum = "" + (currentPage + 1);
            }

            // Обновляем диапазон дат для текущей страницы
            currentDate = DateTime.Now.Date;
            startDate = currentDate.AddDays(-7); // Неделя до текущей даты
            endDate = currentDate.AddDays(27); // 4 недели после текущей даты

            // Переход по месяцам
            startDate = startDate.AddDays((currentPage - 3) * 30);
            endDate = endDate.AddDays((currentPage - 3) * 30);

            LoadCalendarItems();
        }

        #endregion

        // Обработчик двойного клика на элемент календаря
        private void CalendarView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as calendarItem;
            if (item != null)
            {
                LoadEventDataFromDatabase(item.Date);
            }
        }

        // Обработчик нажатия мышью на окно
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in calendarItems)
            {
                if (item.Date < DateTime.Now.Date)
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(226, 227, 232)); // Светло-серый для прошедших дат
                }
                else if (item.Date == DateTime.Now.Date)
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // Светло-голубой для текущей даты
                }
                else
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); // Белый для будущих дат
                }
            }

            editButton.Visibility = Visibility.Hidden;
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            mainCuratorForm f = new mainCuratorForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки добавления новой экскурсии
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            editExcurForm f = new editExcurForm(false, "");
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки сворачивания календаря
        private void rollupButton_Click(object sender, RoutedEventArgs e)
        {
            if (isVisible)
            {
                isVisible = false;
                CalendarView.Visibility = Visibility.Collapsed;
                paginationControl.Visibility = Visibility.Hidden;
                rollupButtonRotateTransform.Angle = 90;

                excursTable.Clear();
                loadExcursData();
                setDatainDataG(excursTable);

                def_one.Height = new GridLength(1.3, GridUnitType.Star);
                def_two.Height = new GridLength(3.7, GridUnitType.Star);

                hallsBox.Visibility = Visibility.Visible;
                label1.Visibility = Visibility.Visible;

                greyColor.Visibility = Visibility.Visible;
                greyColorLabel.Visibility = Visibility.Visible;
                redColor.Visibility = Visibility.Visible;
                redColorLabel.Visibility = Visibility.Visible;

                //checkExcursDate();
            }
            else
            {
                isVisible = true;
                CalendarView.Visibility = Visibility.Visible;
                paginationControl.Visibility = Visibility.Visible;
                rollupButtonRotateTransform.Angle = 180;
                excursDataGrid.ItemsSource = null;

                hallsBox.Visibility = Visibility.Hidden;
                label1.Visibility = Visibility.Hidden;

                greyColor.Visibility = Visibility.Hidden;
                greyColorLabel.Visibility = Visibility.Hidden;
                redColor.Visibility = Visibility.Hidden;
                redColorLabel.Visibility = Visibility.Hidden;

                def_one.Height = new GridLength(3.3, GridUnitType.Star);
                def_two.Height = new GridLength(1.7, GridUnitType.Star);
            }
        }

        // Обработчик изменения выделенной строки в DataGrid
        private void excursDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (excursDataGrid.SelectedItem is DataRowView rowView)
            {
                if (isVisible)
                {
                    DateTime startDate = DateTime.Parse(rowView["newStartDate"].ToString());
                    DateTime endDate = DateTime.Parse(rowView["newEndDate"].ToString());
                    HighlightItem(startDate, endDate); // Подсветка элемента календаря
                }
                else
                {
                    _curID = rowView["exc_id"].ToString();
                    getHallsForCurExcur(_curID); // Получение залов для текущей экскурсии
                    editButton.Visibility = Visibility.Visible;
                }
            }
        }

        // Обработчик загрузки строки в DataGrid
        private void excursDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            checkExcursDate(); // Проверка дат экскурсий
        }

        // Обработчик изменения текста в поле поиска
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchBox.Text;

            if (searchText == "")
            {
                search_query = "";
                excursDataGrid.ItemsSource = null;
            }
            else
            {
                if (isVisible)
                {
                    search_query = full_query +
                    "WHERE (exc_name LIKE '" + searchText + "%') ";
                }
                else
                {
                    search_query = full_query + "WHERE (exc_name LIKE '" + searchText + "%') " +
                    "UNION ALL  " +
                    full_query +
                    "WHERE (exc_name NOT LIKE '" + searchText + "%')";
                }
            }
            LoadSearchData(); // Загрузка данных для поиска
        }

        // Обработчик кнопки редактирования экскурсии
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            editExcurForm f = new editExcurForm(true, _curID);
            // Закрываем текущую форму
            this.Close();
            f.ShowDialog();
        }

        // Обработчик пункта меню удаления строки
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (isVisible)
            {
                return;
            }
            // Получаем текущую выделенную строку
            if (excursDataGrid.SelectedItem is DataRowView rowView)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Логика, если пользователь нажмет "Да"
                    deleteRow();
                }
            }
        }

        // Обработчик события таймаута сессии
        private void OnSessionTimedOut()
        {
            // Открытие формы авторизации
            this.Hide();
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

        // Обработчики для поля поиска, позволяющие использование клавиши Backspace и пробел
        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчик ввода текста в поле поиска (позволяем ввод только кириллицы, цифр и специальных символов)
        private void searchBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]");
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop(); // Остановка таймера сессии
        }
    }
}
