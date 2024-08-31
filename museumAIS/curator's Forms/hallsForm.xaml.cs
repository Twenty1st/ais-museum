using museumAIS.Classes;
using museumAIS.curator_s_Forms;
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

namespace museumAIS
{
    /// <summary>
    /// Логика взаимодействия для hallsForm.xaml
    /// </summary>
    public partial class hallsForm : Window
    {
        private helpForDB db; // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        private string search_query = ""; // Поисковый запрос
        private string full_query = "SELECT * FROM halls"; // Полный запрос
        private string _curID = ""; // Текущий идентификатор зала

        private readonly int row_amount = 15; // Количество строк на странице
        private int allPages = 0; // Общее количество страниц
        private int currentPage = 1; // Текущая страница

        // Конструктор формы управления залами
        public hallsForm()
        {
            InitializeComponent();

            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            pagin_init(); // Инициализация пагинации
            LoadData(); // Загрузка данных
        }

        // Инициализация пагинации
        void pagin_init()
        {
            try
            {
                this.DataContext = paginationControl;
                // Считаем количество записей
                string query = "SELECT count(*) FROM halls";
                string response = db.executSclrQuey(query);
                if (response.Equals("error"))
                {
                    callMessageBox.ShowError("Ошибка подключения к базе данных!");
                    return;
                }
                int count_field = Convert.ToInt32(response);
                // Показываем страницы 1, 2, 3
                allPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(count_field) / Convert.ToDouble(row_amount)));

                if (allPages > 1)
                {
                    paginationControl.PrevPageNum = "";
                    paginationControl.CurPageNum = "1";
                    paginationControl.NextPageNum = "2";
                }
            }
            catch (Exception) // Обработка ошибок
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

            }
        }

        // Загрузка данных
        private void LoadData()
        {
            string query = full_query;
            if (search_query != "")
            {
                query = search_query;
            }
            if (query == full_query)
            {
                query += " order by hall_num DESC ";
            }

            query += " limit " + row_amount + " offset " + row_amount * (currentPage - 1);

            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            int recordNumber = row_amount * (currentPage - 1);
            table.Columns.Add("Num");

            foreach (DataRow row in table.Rows)
            {
                row["Num"] = Convert.ToString(++recordNumber);
            }

            dataGridV.ItemsSource = table.DefaultView;
            HighlightSearchResults(table, searchBox.Text);
        }

        // Подсветка результатов поиска
        private void HighlightSearchResults(DataTable table, string searchText)
        {
            if (searchText == "")
            {
                return;
            }
            dataGridV.SelectedItems.Clear();
            foreach (DataRow row in table.Rows)
            {
                string hallName = row["hall_name"].ToString();

                if (hallName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    DataRowView rowView = table.DefaultView[table.Rows.IndexOf(row)];
                    dataGridV.SelectedItems.Add(rowView);
                }
            }
        }

        private bool checkDependence()
        {
            string query = "SELECT COUNT(*) FROM excursion_halls WHERE num_hall = " + _curID;

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

            query = "SELECT COUNT(*) FROM ecsponat_hall WHERE num_hall = " + _curID;

            // Выполнение запроса к базе данных
            table = db.selectData(query);
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

        // Удаление строки (зала)
        private void deleteRow()
        {
            if (checkDependence())
            {
                callMessageBox.ShowWarning("Нельзя удалить зависимые данные! Сначала уберите зависимость.");
                return;
            }

            string query = "DELETE FROM halls WHERE hall_num = " + _curID;
            if (db.editDBData(query))
            {
                callMessageBox.ShowInfo("Успешно!");
                LoadData();
                editButton.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой!");
                return;
            }
        }

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

            LoadData();
        }
        #endregion

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            mainCuratorForm f = new mainCuratorForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки добавления нового зала
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            editHallForm f = new editHallForm(false, "");
            f.ShowDialog();
            LoadData();
        }

        // Обработчик пункта меню удаления строки
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

        // Обработчик изменения текста в поле поиска
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchBox.Text;

            if (searchText == "")
            {
                search_query = "";
            }
            else
            {
                search_query = "SELECT * FROM halls " +
                    "WHERE (hall_name LIKE '%" + searchText + "%') " +
                    "UNION ALL  " +
                    "SELECT * FROM halls " +
                    "WHERE (hall_name NOT LIKE '%" + searchText + "%')";
            }
            LoadData();
        }

        // Обработчик изменения выделенной строки в DataGrid
        private void dataGridV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid.SelectedItem == null) return;

            // Получаем выбранную строку как DataRowView
            DataRowView row = grid.SelectedItem as DataRowView;
            if (row != null)
            {
                // Получаем значение из конкретной ячейки
                _curID = row[0].ToString(); // Используем индекс или имя колонки
                editButton.Visibility = Visibility.Visible;
            }
        }

        // Обработчик кнопки редактирования зала
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            editHallForm f = new editHallForm(true, _curID);
            f.ShowDialog();
            LoadData();
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
            _sessionTimeoutService.Reset();
        }

        // Обработчик события движения мыши в окне
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        // Обработчики для поля поиска, позволяющие использование клавиши Backspace и пробел
        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчик ввода текста в поле поиска (позволяем ввод только кириллицы, цифр и дефисов)
        private void searchBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\-]");
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }
    }
}
