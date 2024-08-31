using museumAIS.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для usersForm.xaml
    /// </summary>
    public partial class usersForm : Window
    {
        private helpForDB db;
        private SessionTimeoutService _sessionTimeoutService;

        private string curID = "";

        private static List<string> loginList;
        DataRowView selectionRow = null;

        private readonly int row_amount = 15;
        private int allPages = 0;
        private int currentPage = 1;

        private static string full_query = @"SELECT id_user, user_lastName, user_name, user_patronamyc, 
                user_login, user_privilege.privilegeName FROM users 
                JOIN user_privilege ON privilegeID = privilege_user";

        private string search_query = "";

        public usersForm()
        {
            InitializeComponent();

            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

            pagin_init();
            LoadData();
        }

        void pagin_init() //инициализация пагинации
        {
            try
            {
                this.DataContext = paginationControl;
                //считаем количество записей
                string query = "SELECT count(*) FROM users";
                string respone = db.executSclrQuey(query);
                if (respone.Equals("error"))
                {
                    callMessageBox.ShowError("Ошибка подключения к базе данных!");
                    return;
                }
                int count_field = Convert.ToInt32(respone);
                //показываем старницы 1, 2, 3
                allPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(count_field) / Convert.ToDouble(row_amount)));

                if (allPages > 1)
                {
                    paginationControl.PrevPageNum = "";
                    paginationControl.CurPageNum = "1";
                    paginationControl.NextPageNum = "2";
                }
            }
            catch (Exception) //если возникла ошибка
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
            }
        }

        private void LoadData()
        {
            string query = full_query;
            if (search_query != "")
            {
                query = search_query;
            }
            if (query == full_query)
            {
                query += " order by id_user DESC ";
            }

            query += " limit " + row_amount + " offset " + row_amount * (currentPage - 1);

            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            table = setNumericRows(table);

            string fio;
            foreach (DataRow row in table.Rows)
            {
                if (row["user_patronamyc"].ToString().Equals(""))
                {
                    fio = row["user_lastName"].ToString() + " " + row["user_name"].ToString().Substring(0, 1) + ".";
                }
                else
                {
                    fio = row["user_lastName"].ToString() + " " + row["user_name"].ToString().Substring(0, 1) + "." +
                                        " " + row["user_patronamyc"].ToString().Substring(0, 1) + ".";
                }
                row["user_lastName"] = fio;
            }

            

            table.Columns.RemoveAt(3);
            table.Columns.RemoveAt(3);

            table.Columns[1].ColumnName = "id";

            table.Columns[2].ColumnName = "ФИО пользователя";
            table.Columns[3].ColumnName = "Логин";
            table.Columns[4].ColumnName = "Привилегия";

            dataGridV.ItemsSource = table.DefaultView;

            HighlightSearchResults(table, searchBox.Text);
        }

        private void HighlightSearchResults(DataTable table, string searchText)
        {
            if(searchText == "")
            {
                return;
            }
            dataGridV.SelectedItems.Clear();
            foreach (DataRow row in table.Rows)
            {
                string lastName = row["ФИО пользователя"].ToString();
                string firstName = row["ФИО пользователя"].ToString();
                string patronymic = row["ФИО пользователя"].ToString();

                if (lastName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) ||
                    firstName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) ||
                    patronymic.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    DataRowView rowView = table.DefaultView[table.Rows.IndexOf(row)];
                    dataGridV.SelectedItems.Add(rowView);
                }
            }
        }

        private DataTable setNumericRows(DataTable table)
        {
            if (!table.Columns.Contains("№"))
            {
                DataColumn indexColumn = new DataColumn("№", typeof(int))
                {
                    AutoIncrement = true,
                    AutoIncrementSeed = 1,
                    AutoIncrementStep = 1
                };
                table.Columns.Add(indexColumn);
                table.Columns["№"].SetOrdinal(0); // Переместите столбец на первое место
            }

            int index = 1;
            loginList = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                row["№"] = index++;
                string login = row["user_login"].ToString();
                loginList.Add(login);
                login = maskLogin(login);
                row["user_login"] = login;

            }

            return table;
        }

        private string maskLogin(string login)
        {
            // Если длина строки меньше 3, ничего не заменяем
            if (login.Length >= 3)
            {
                char[] maskedChars = login.ToCharArray();

                for (int i = 1; i < login.Length - 1; i++)
                {
                    maskedChars[i] = '*';
                }
                login = new string(maskedChars);
            }
            return login;
        }

        private void deleteRow()
        {
            string query = "DELETE FROM users WHERE id_user = " + curID;
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

        private void DataGridV_AutoGeneratedColumns(object sender, EventArgs e)
        {
            dataGridV.Columns[1].Visibility = Visibility.Collapsed; // Скрытие первой колонки
        }

        private void goBackButton_Click(object sender, RoutedEventArgs e)
        {
            mainAdministratorForm f = new mainAdministratorForm();
            this.Close();
            f.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            editUserForm f = new editUserForm(false, "");
            this.Close();
            f.ShowDialog();
        }

        #region[pagination]

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

        private void paginationControl_prevPageBut_Click(object sender, RoutedEventArgs e)
        {
            if (paginationControl.PrevPageNum == "")
            {
                return;
            }
            currentPage--;
            setNewPage();
        }

        private void paginationControl_nextPageBut_Click(object sender, RoutedEventArgs e)
        {
            if (paginationControl.NextPageNum == "")
            {
                return;
            }
            currentPage++;
            setNewPage();
        }

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

        private void dataGridV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectionRow != null)
            {
                selectionRow[3] = maskLogin(selectionRow[3].ToString());
                selectionRow = null;
            }

            var grid = sender as DataGrid;
            if (grid.SelectedItem == null) return;

            // Получаем выбранную строку как DataRowView
            selectionRow = grid.SelectedItem as DataRowView;
            if (selectionRow != null)
            {
                // Получаем значение из конкретной ячейки
                curID = selectionRow[1].ToString(); // Используем индекс или имя колонки
                editButton.Visibility = Visibility.Visible;
                selectionRow[3] = loginList[grid.SelectedIndex];
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

        private void DataGridV_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Если нет выделенной строки, отключаем контекстное меню
            if (dataGridV.SelectedItem == null)
            {
                e.Handled = true;
            }
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchBox.Text;

            if (searchText == "")
            {
                search_query = "";
            }
            else
            {
                search_query = "SELECT id_user, user_lastName, user_name, user_patronamyc, user_login, " +
                    "user_privilege.privilegeName " +
                    "FROM users " +
                    "JOIN user_privilege ON privilegeID = privilege_user  " +
                    "WHERE (user_lastName LIKE '" + searchText + "%' OR  user_name LIKE '" + searchText + "%' " +
                    "OR  user_patronamyc LIKE '" + searchText + "%')   " +
                    "UNION ALL  " +
                    "SELECT id_user, user_lastName, user_name, user_patronamyc, user_login,  " +
                    "user_privilege.privilegeName  " +
                    "FROM users  " +
                    "JOIN user_privilege ON privilegeID = privilege_user    " +
                    "WHERE (user_lastName NOT LIKE '" + searchText + "%' AND " +
                    " user_name NOT LIKE '" + searchText + "%' AND  user_patronamyc NOT LIKE '" + searchText + "%')";
            }
            LoadData();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            editUserForm f = new editUserForm(true, curID);
            this.Close();
            f.ShowDialog();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dataGridV.SelectedItems.Clear();
            if (selectionRow != null)
            {
                selectionRow[3] = maskLogin(selectionRow[3].ToString());
                selectionRow = null;
            }

        }

        private void OnSessionTimedOut()
        {
            // Open login form
            authorizeForm f = new authorizeForm();
            this.Close();
            f.ShowDialog();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }

        private void searchBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]");
        }
    }
}
