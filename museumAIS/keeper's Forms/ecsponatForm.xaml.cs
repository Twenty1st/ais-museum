using museumAIS.Classes;
using museumAIS.keeper_s_Forms;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для ecsponatForm.xaml
    /// </summary>
    public partial class ecsponatForm : Window
    {
        private helpForDB DBhelp; // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        private string curID = ""; // Текущий идентификатор экспоната

        private readonly int row_amount = 10; // Количество строк на странице
        private int allPages = 0; // Общее количество страниц
        private int currentPage = 1; // Текущая страница

        private List<string> statusIDLIST = new List<string>(); // Список идентификаторов статусов
        private List<string> hallsIDList = new List<string>(); // Список идентификаторов залов

        private string fullQuery = "SELECT id_ecsponat, cipher_fund_ecsponat, cipher_stock_ecsponat, " +
            "reg_number_ecsponat, name_ecsponat, size_ecponat, date_create_ecsponat, image_ecsponat, " +
            "description_ecsponat, date_income_ecsponat, status_name " +
            "FROM ecsponat " +
            "JOIN status_ecsponat ON status_id = status_ecsponat ";

        private string search_query = ""; // Поисковый запрос
        private string not_search_query = ""; // Запрос для данных, не соответствующих поиску
        private string filter_query = ""; // Запрос фильтрации
        private string sort_query = ""; // Запрос сортировки

        // Конструктор формы управления экспонатами
        public ecsponatForm()
        {
            InitializeComponent();

            DBhelp = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            pagin_init(); // Инициализация пагинации
            LoadData(); // Загрузка данных
            setDatainComboBox(); // Установка данных в комбобоксы
        }

        // Инициализация пагинации
        void pagin_init()
        {
            try
            {
                this.DataContext = paginationControl;
                // Считаем количество записей
                string query = "SELECT count(*) FROM ecsponat " + filter_query;
                string response = DBhelp.executSclrQuey(query);
                if (response.Equals("error"))
                {
                    callMessageBox.ShowError("Ошибка подключения к базе данных!");

                    return;
                }

                currentPage = 1;

                int count_field = Convert.ToInt32(response);
                // Показываем страницы 1, 2, 3
                allPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(count_field) / Convert.ToDouble(row_amount)));

                if (allPages > 1)
                {
                    paginationControl.PrevPageNum = "";
                    paginationControl.CurPageNum = "1";
                    paginationControl.NextPageNum = "2";
                }
                else
                {
                    paginationControl.PrevPageNum = "";
                    paginationControl.CurPageNum = "";
                    paginationControl.NextPageNum = "";
                }
            }
            catch (Exception) // Обработка ошибок
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

            }
        }

        // Конструирование полного запроса
        private string constructQuery()
        {
            string query = fullQuery;
            if (search_query != "")
            {
                query = "SELECT * FROM (" + fullQuery + search_query + "  UNION ALL " + "" +
                    " " + fullQuery + not_search_query + " ) AS combined_results ";
            }
            if (filter_query != "")
            {
                query += filter_query;
            }
            if (sort_query != "")
            {
                query += sort_query;
            }
            return query;
        }

        // Загрузка данных
        private void LoadData()
        {
            dataViewG.Children.Clear();

            string query = constructQuery();

            query += " limit " + row_amount + " offset " + row_amount * (currentPage - 1);

            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            foreach (DataRow tableRow in table.Rows)
            {
                // Установка данных в элементы управления
                if (!setDataInControl(tableRow)) 
                {
                    callMessageBox.ShowError("Ошибка подключения к базе данных!");
                    return;
                }
                    
            }
        }

        // Установка данных в комбобоксы
        private void setDatainComboBox()
        {
            string query = "SELECT * FROM status_ecsponat";
            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            foreach (DataRow row in table.Rows)
            {
                string status_name = row["status_name"].ToString();
                statusIDLIST.Add(row["status_id"].ToString());
                changeStatusBox.Items.Add(status_name);
                filtersListBox.Items.Add(status_name);
            }

            query = "SELECT * FROM halls";
            table = DBhelp.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            foreach (DataRow row in table.Rows)
            {
                hallsIDList.Add(row["hall_num"].ToString());
                selectHallBox.Items.Add(row["hall_name"].ToString());
            }

            dateComeBack.DisplayDateStart = DateTime.Now.Date;

            sortBox.Items.Add("-");
            sortBox.Items.Add("от А до Я");
            sortBox.Items.Add("от Я до А");
            sortBox.Items.Add("дата прибытия В");
            sortBox.Items.Add("дата прибытия Н");

            filterBox.Items.Add("-");
            filterBox.Items.Add("Статус");
            filterBox.Items.Add("Дата прибытия");

            dateInComeP.DisplayDateEnd = DateTime.Today;
        }

        // Обработка события клика на UserControl
        private void Row_Click(object sender, rowItemClickEvent e)
        {
            curID = e.IdEcsp;

            changeStatusBox.SelectedIndex = -1;
            selectHallBox.SelectedIndex = -1;
            changeStatusBox.Text = e.Status;

            changeStatusBox.IsEnabled = true;
            dateComeBack.IsEnabled = true;

            editButton.Visibility = Visibility.Visible;
        }

        // Установка данных в элементы управления
        private bool setDataInControl(DataRow tableRow)
        {
            rowEcsponat row = new rowEcsponat();
            row.Click += Row_Click;
            row.del_Click += rowEcsponat_DeleteClick;
            string materials = "";
            string query = "SELECT name_material FROM material_ecsponat WHERE ecsponatM_id = " + tableRow["id_ecsponat"].ToString();
            DataTable mTable = DBhelp.selectData(query);
            if (mTable == null)
            {
                return false;
            }
            foreach (DataRow mRow in mTable.Rows)
            {
                if (materials != "")
                {
                    materials += ", ";
                }
                materials += mRow["name_material"];
            }

            
            row.IdEcsp = tableRow["id_ecsponat"].ToString();

            string backcolor = getBackround(tableRow["id_ecsponat"].ToString());
            if(backcolor == "e")
            {
                return false;
            }
            else if(backcolor != "")
            {
                row.ChangeBackgroundColor(backcolor);
            }

            row.ArticleEcsp = tableRow["cipher_fund_ecsponat"].ToString() + "/" +
                tableRow["cipher_stock_ecsponat"].ToString() + "/" + tableRow["reg_number_ecsponat"].ToString();
            row.Name_ecsp = tableRow["name_ecsponat"].ToString();
            row.SizeEcsp = tableRow["size_ecponat"].ToString();
            row.DateCreate = tableRow["date_create_ecsponat"].ToString();
            row.Materials = materials;
            row.Desc = tableRow["description_ecsponat"].ToString();
            row.Status = tableRow["status_name"].ToString();
            Image isDownload = new Image();
            string img_path = Directory.GetCurrentDirectory() + "\\images\\" + tableRow["image_ecsponat"].ToString();
            isDownload.Source = getImage(img_path);
            if (isDownload == null)
            {
                return false;
            }
            row.ImageEcsp = isDownload;

            dataViewG.Children.Add(row);

            return true;
        }

        // Получение изображения из файла
        private BitmapImage getImage(string path)
        {
            if (!File.Exists(path))
            {
                callMessageBox.ShowError("Файл изображения не найден.");
                path = Directory.GetCurrentDirectory() + "\\images\\picture.png";
            }
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(path, UriKind.Absolute);
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch (Exception)
            {
                callMessageBox.ShowError("Файл изображения не найден.");
                return null;
            }
        }

        // получения цвета фона
        private string getBackround(string id)
        {
            //ffdaa3 - светло оранжево-красный
            //e7ffa3 - светло зелено-желтый
            string query = "SELECT date_comeback FROM museum.absent_ecsponat WHERE ecsponat_id = "+id;
            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                return "e";
            }
            if(table.Rows.Count == 0)
            {
                return "";
            }
            DateTime date = DateTime.Parse(table.Rows[0][0].ToString());
            if(DateTime.Today >= date)
            {
                return "#e7ffa3";
            }
            else
            {
                return "#ffdaa3";
            }

        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            mainKeeperForm f = new mainKeeperForm();
            this.Close();
            f.ShowDialog();
        }

        // Установка пустых данных в поля формы
        private void setEmptyData()
        {
            dateComeBack.DisplayDate = DateTime.Now.Date;
            selectHallBox.Text = "";
            changeStatusBox.Text = "";

            changeStatusBox.IsEnabled = false;
            selectHallBox.IsEnabled = false;
            dateComeBack.IsEnabled = false;
        }

        // Обработчик кнопки изменения статуса
        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectHallBox.Text == "" && dateComeBack.Text == "" && changeStatusBox.Text == "В резерве")
            {
                callMessageBox.ShowWarning("Заполните поля!");
                return;
            }
            // Обработка изменения статуса экспоната
            if (changeStatusBox.Text.Equals("Выставлен"))
            {
                viewMessageChangeStatus(isInHallStatus());
            }
            else if (changeStatusBox.Text.Equals("В резерве"))
            {
                viewMessageChangeStatus(isInReserveStatus());
            }
            else
            {
                viewMessageChangeStatus(isOutofMuseumStatus());
            }
        }

        // Показ сообщения о результате изменения статуса
        private void viewMessageChangeStatus(bool isOk)
        {
            if (isOk)
            {
                callMessageBox.ShowInfo("Успешно!");
                setEmptyData();
                LoadData();
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой!");

            }
        }

        // Обработчик кнопки добавления нового экспоната
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            editEcspDataForm f = new editEcspDataForm(false, "");
            this.Close();
            f.ShowDialog();
        }

        // Обработчики пагинации
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

        // Обработчик изменения текста в поле поиска
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchBox.Text == "")
            {
                search_query = "";
            }
            else
            {
                search_query = " WHERE ((cipher_fund_ecsponat LIKE '" + searchBox.Text + "%' " +
                            "OR cipher_stock_ecsponat LIKE '" + searchBox.Text + "%' " +
                            "OR reg_number_ecsponat LIKE '" + searchBox.Text + "%') " +
                            "OR name_ecsponat LIKE '" + searchBox.Text + "%') ";
                not_search_query = " WHERE ((cipher_fund_ecsponat NOT LIKE '" + searchBox.Text + "%' " +
                            "AND cipher_stock_ecsponat NOT LIKE '" + searchBox.Text + "%' " +
                            "AND reg_number_ecsponat NOT  LIKE '" + searchBox.Text + "%') " +
                            "AND name_ecsponat NOT  LIKE '" + searchBox.Text + "%') ";
            }
            
            
            LoadData();
        }

        // Обработчик изменения выбранного статуса
        private void changeStatusBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changeStatusBox.SelectedItem == null)
            {
                return;
            }

            string selectedItem = changeStatusBox.SelectedItem.ToString();
            if (selectedItem.Equals("Выставлен"))
            {
                helpLabel.Content = "Выберете зал:";
                dateComeBack.Visibility = Visibility.Collapsed;
                selectHallBox.IsEnabled = true;
                dateComeBack.IsEnabled = false;
                selectHallBox.Visibility = Visibility.Visible;
                getHallofEcsp();
            }
            else if (selectedItem.Equals("В резерве"))
            {
                helpLabel.Content = "Выберете зал:";
                selectHallBox.IsEnabled = false;
                dateComeBack.IsEnabled = false;
                dateComeBack.Visibility = Visibility.Collapsed;
            }
            else
            {
                helpLabel.Content = "Дата прибытия:";
                selectHallBox.Visibility = Visibility.Collapsed;
                dateComeBack.IsEnabled = true;
                selectHallBox.IsEnabled = false;
                dateComeBack.Visibility = Visibility.Visible;
                getDateComeBack();
            }
        }

        // Получение даты возвращения экспоната
        private void getDateComeBack()
        {
            string query = "SELECT date_comeback FROM absent_ecsponat WHERE ecsponat_id = " + curID;
            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                return;
            }
            string date_str = table.Rows.Count == 0 ? "" : table.Rows[0][0].ToString();
            if (date_str != "")
            {
                dateComeBack.SelectedDate = DateTime.Parse(date_str);
            }
        }

        // Получение зала, в котором находится экспонат
        private void getHallofEcsp()
        {
            string query = "SELECT hall_name FROM ecsponat_hall JOIN halls ON num_hall = hall_num WHERE id_ecsp = " + curID;
            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }
            string hall_name = table.Rows.Count == 0 ? "" : table.Rows[0][0].ToString();
            if (hall_name != "")
            {
                selectHallBox.Text = hall_name;
            }
        }

        // Обновление статуса экспоната на "Выставлен"
        private bool isInHallStatus()
        {
            string queryStatus = "UPDATE ecsponat SET status_ecsponat = 1 " +
                " WHERE id_ecsponat = " + curID;

            string queryDellHall = "DELETE FROM ecsponat_hall WHERE id_ecsp = " + curID;

            string queryDellAbsent = "DELETE FROM absent_ecsponat WHERE ecsponat_id = " + curID;         

            string queryHall = "INSERT ecsponat_hall (id_ecsp, num_hall) VALUES (" + curID + ", " + hallsIDList[selectHallBox.SelectedIndex] + ")";
            if (!DBhelp.changeStatusTransaction(queryDellHall, queryDellAbsent, queryStatus, queryHall))
            {
                return false;
            }

            return true;
        }

        // Обновление статуса экспоната на "В резерве"
        private bool isInReserveStatus()
        {
            string queryStatus = "UPDATE ecsponat SET status_ecsponat = " + statusIDLIST[changeStatusBox.SelectedIndex] + " " +
                " WHERE id_ecsponat = " + curID;

            string queryAbsent = "DELETE FROM absent_ecsponat WHERE ecsponat_id = " + curID;

            string queryHall = "DELETE FROM ecsponat_hall WHERE id_ecsp = " + curID;
            if (!DBhelp.changeStatusTransaction(queryHall, queryAbsent, queryStatus, ""))
            {

                return false;
            }

            return true;
        }

        // Обновление статуса экспоната на "Выведен из музея"
        private bool isOutofMuseumStatus()
        {
            string queryStatus = "UPDATE ecsponat SET status_ecsponat = " + statusIDLIST[changeStatusBox.SelectedIndex] + " " +
                " WHERE id_ecsponat = " + curID;


            string queryHall = "DELETE FROM ecsponat_hall WHERE id_ecsp = " + curID;
            
            string queryDellAbsent = "DELETE FROM absent_ecsponat WHERE ecsponat_id = " + curID;

            string queryAbsent = "INSERT absent_ecsponat (ecsponat_id, date_comeback) VALUES " +
                "(" + curID + ", '" + dateComeBack.SelectedDate.Value.ToString("yyyy-MM-dd") + "')";
            
            if (!DBhelp.changeStatusTransaction(queryDellAbsent, queryHall, queryStatus, queryAbsent))
            {
                return false;
            }
            return true;
        }

        // Обработчик кнопки редактирования экспоната
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            editEcspDataForm f = new editEcspDataForm(true, curID);
            this.Close();
            f.ShowDialog();
        }

        // Обработчик изменения сортировки
        private void sortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = sortBox.SelectedIndex;
            switch (index)
            {
                case 0:
                    sort_query = "";
                    break;
                case 1:
                    sort_query = " ORDER BY name_ecsponat";
                    break;
                case 2:
                    sort_query = " ORDER BY name_ecsponat DESC";
                    break;
                case 3:
                    sort_query = " ORDER BY date_income_ecsponat";
                    break;
                case 4:
                    sort_query = " ORDER BY date_income_ecsponat DESC";
                    break;
            }
            LoadData();
        }

        // Обработчик изменения фильтра
        private void filterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filterBox.SelectedIndex == 0)
            {
                filter_query = "";
                pagin_init();
                LoadData();
            }
            else if (filterBox.SelectedIndex == 1)
            {
                borderPanel.Visibility = Visibility.Visible;
                fLabel.Content = "Выберете статус";
                filtersListBox.Visibility = Visibility.Visible;
                dateInComeP.Visibility = Visibility.Collapsed;
            }
            else
            {
                borderPanel.Visibility = Visibility.Visible;
                fLabel.Content = "Выберете дату";
                filtersListBox.Visibility = Visibility.Collapsed;
                dateInComeP.Visibility = Visibility.Visible;
            }
        }

        // Обработчик изменения выбранного фильтра статуса
        private void filtersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter_query = " WHERE status_ecsponat = " + statusIDLIST[filtersListBox.SelectedIndex] + "";
            pagin_init();
            filter_query = " WHERE status_name = '" + filtersListBox.Items[filtersListBox.SelectedIndex] + "'";
            LoadData();
        }

        // Обработчик изменения даты прибытия для фильтрации
        private void dateInComeP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            filter_query = " WHERE date_income_ecsponat = '" + dateInComeP.SelectedDate.Value.ToString("yyyy-MM-dd") + "'";
            pagin_init();
            LoadData();
        }

        // Удаление строки (экспоната)
        private void deleteRow(string id)
        {
            string query = "DELETE FROM ecsponat WHERE  id_ecsponat = " + id;
            if (DBhelp.editDBData(query))
            {
                callMessageBox.ShowInfo("Успешно!");
                editButton.Visibility = Visibility.Hidden;
                LoadData();
                return;
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой данных!");
                return;
            }
        }

        // Обработчик события DeleteClick
        private void rowEcsponat_DeleteClick(object sender, dellButtonClickEvent e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Логика, если пользователь нажмет "Да"
                deleteRow(e.Id);
            }
        }

        // Обработчик события нажатия мыши в окне
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            borderPanel.Visibility = Visibility.Collapsed;
        }

        // Обработчик кнопки отчета
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            otchetForm f = new otchetForm();
            f.ShowDialog();
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

        // Обработчики для различных комбобоксов и полей ввода, запрещающие ввод текста
        private void sortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void filterBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void changeStatusBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void dateComeBack_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void selectHallBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void filtersListBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void dateInComeP_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчики для поля поиска, позволяющие использование клавиши Backspace и пробел
        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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
            _sessionTimeoutService.Stop();
        }
    }
}
