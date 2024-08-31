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
using System.IO;
using Microsoft.Win32;
using museumAIS.Classes;
using System.Text.RegularExpressions;

namespace museumAIS.keeper_s_Forms
{
    /// <summary>
    /// Interaction logic for editEcspDataForm.xaml
    /// </summary>
    public partial class editEcspDataForm : Window
    {
        private static helpForDB db; // Объект для работы с базой данных
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        private static bool _isEdit; // Флаг режима редактирования
        private static string _ecspID; // Идентификатор текущего экспоната

        private List<string> hallIDList; // Список идентификаторов залов
        private string[] statusIDList; // Массив идентификаторов статусов

        // Конструктор формы редактирования/добавления экспоната
        public editEcspDataForm(bool isEdit, string id)
        {
            InitializeComponent();

            _isEdit = isEdit;
            _ecspID = id;

            db = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            LoadDataInCmbBox(); // Загрузка данных в комбобоксы
            if (isEdit)
            {
                setDataForCurID(); // Установка данных для текущего экспоната
                editButton.Content = "Изменить";
            }
        }

        // Загрузка данных в комбобоксы
        private void LoadDataInCmbBox()
        {
            string query = "SELECT * FROM halls";
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }
            hallIDList = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                hallBox.Items.Add(row["hall_name"].ToString());
                hallIDList.Add(row["hall_num"].ToString());
            }

            statusBox.Items.Add("Выставлен");
            statusBox.Items.Add("В резерве");

            statusIDList = new string[] { "1", "2" };

            dateInComePicker.DisplayDateEnd = DateTime.Today;
        }

        // Установка данных для текущего экспоната
        private void setDataForCurID()
        {
            string query = "SELECT cipher_fund_ecsponat, cipher_stock_ecsponat, reg_number_ecsponat," +
                " name_ecsponat, size_ecponat, date_create_ecsponat, " +
                "GROUP_CONCAT(name_material ORDER BY name_material SEPARATOR ', ') AS materials, " +
                "image_ecsponat, description_ecsponat, date_income_ecsponat, status_name FROM ecsponat " +
                "JOIN status_ecsponat ON status_id = status_ecsponat " +
                "JOIN material_ecsponat ON ecsponatM_id = id_ecsponat WHERE id_ecsponat = " + _ecspID;

            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return;
            }

            cipher_fund_ecsponatBox.Text = table.Rows[0][0].ToString();
            cipher_stock_ecsponatBox.Text = table.Rows[0][1].ToString();
            reg_number_ecsponatBox.Text = table.Rows[0][2].ToString();
            nameBox.Text = table.Rows[0][3].ToString();
            sizeBox.Text = table.Rows[0][4].ToString();
            dateCreateBox.Text = table.Rows[0][5].ToString();
            materialBox.Text = table.Rows[0][6].ToString();

            string path = Directory.GetCurrentDirectory() + "\\images\\" + table.Rows[0][7].ToString();
            BitmapImage bitmap;
            if (!File.Exists(path))
            {
                bitmap = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\images\\picture.png"));
            }
            else
            {
                bitmap = new BitmapImage(new Uri(path));
            }
            imageBox.Source = bitmap;

            descBox.Text = table.Rows[0][8].ToString();
            dateInComePicker.SelectedDate = DateTime.Parse(table.Rows[0][9].ToString());
            statusBox.Text = table.Rows[0][10].ToString();
        }

        // Проверка на заполненность всех необходимых полей
        private bool isAllFill()
        {
            if (cipher_fund_ecsponatBox.Text == "" || cipher_stock_ecsponatBox.Text == "" || reg_number_ecsponatBox.Text == "" ||
                nameBox.Text == "" || materialBox.Text == "" || statusBox.Text == "" || sizeBox.Text == "" ||
                dateCreateBox.Text == "" || !dateInComePicker.SelectedDate.HasValue)
            {
                return false;
            }
            else if (statusBox.SelectedIndex == 0)
            {
                if (hallBox.Text == "")
                {
                    return false;
                }
            }
            return true;
        }

        // Получение списка материалов
        private List<string> getMaterials()
        {
            string materials = materialBox.Text;
            return materials.Replace(" ", "").Split(',').ToList();
        }

        // Сохранение изображения
        private bool SaveImage(Image image, string directory, string fileName)
        {
            try
            {
                if (image.Source is BitmapSource bitmapSource)
                {
                    // Создание PngBitmapEncoder для сохранения изображения в файл
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    // Указание пути и имени файла для сохранения изображения
                    string filePath = System.IO.Path.Combine(directory, fileName);

                    // Сохранение изображения в файл
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }

                    return true;
                }
                else
                {
                    MessageBox.Show("Ошибка: Изображение не является BitmapSource.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изображения: " + ex.Message);
                return false;
            }
        }

        // Очистка полей формы
        private void clearFields()
        {
            cipher_fund_ecsponatBox.Text = "";
            cipher_stock_ecsponatBox.Text = "";
            reg_number_ecsponatBox.Text = "";
            nameBox.Text = "";
            materialBox.Text = "";
            statusBox.Text = "";
            sizeBox.Text = "";
            hallBox.Text = "";
            descBox.Text = "";
            dateCreateBox.Text = "";
            dateInComePicker.Text = "";

            hallLabel.Visibility = Visibility.Hidden;
            hallBox.Visibility = Visibility.Hidden;

            BitmapImage bitmap = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\images\\picture.png"));
            imageBox.Source = bitmap;
        }

        // Поиск индекса в комбобоксе по значению
        private int FindComboBoxIndexByValue(ComboBox comboBox)
        {
            string searchText = comboBox.Text;
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i].ToString() == searchText)
                {
                    return i;
                }
            }
            return -1;
        }

        // Получение зала экспоната
        private void getHallofEcsp()
        {
            string query = "SELECT hall_name FROM ecsponat_hall JOIN halls ON num_hall = hall_num WHERE id_ecsp = " + _ecspID;
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return;
            }
            string hall_name = table.Rows.Count == 0 ? "" : table.Rows[0][0].ToString();
            if (hall_name != "")
            {
                hallBox.Text = hall_name;
            }
        }

        // Вставка данных в базу
        private void insertData()
        {
            List<string> materialsList = getMaterials();
            string cipher_fund_ecsponat = cipher_fund_ecsponatBox.Text;
            string cipher_stock_ecsponat = cipher_stock_ecsponatBox.Text;
            string reg_number_ecsponat = reg_number_ecsponatBox.Text;
            string name_ecsponat = nameBox.Text;
            string dateCreate = dateCreateBox.Text;
            string inComeDate = dateInComePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            string description = descBox.Text;
            string size = sizeBox.Text;
            string status = statusIDList[FindComboBoxIndexByValue(statusBox)];

            string savePath = Directory.GetCurrentDirectory() + "\\images\\";
            string imgName = cipher_fund_ecsponat + cipher_stock_ecsponat + reg_number_ecsponat + ".png";

            string query = "";

            if (!SaveImage(imageBox, savePath, imgName))
            {
                return;
            }
            if (_isEdit)
            {
                query = "DELETE FROM material_ecsponat WHERE ecsponatM_id = " + _ecspID;

                if (!db.editDBData(query))
                {
                    callMessageBox.ShowError("Ошибка при работе с базой!");
                    return;
                }

                query = "UPDATE ecsponat SET cipher_fund_ecsponat = '" + cipher_fund_ecsponat + "', cipher_stock_ecsponat = '" + cipher_stock_ecsponat + "', " +
                                    "reg_number_ecsponat = '" + reg_number_ecsponat + "', name_ecsponat = '" + name_ecsponat + "', " +
                    "size_ecponat = '" + size + "', date_create_ecsponat = '" + dateCreate + "', " +
                    "image_ecsponat = '" + imgName + "', description_ecsponat =  '" + description + "', " +
                    "date_income_ecsponat = '" + inComeDate + "', status_ecsponat = " + status + " WHERE " +
                    "id_ecsponat = " + _ecspID;
            }
            else
            {
                query = "INSERT INTO ecsponat (cipher_fund_ecsponat, cipher_stock_ecsponat, reg_number_ecsponat, name_ecsponat, " +
                                "size_ecponat, date_create_ecsponat, image_ecsponat, description_ecsponat, date_income_ecsponat, status_ecsponat) " +
                                " VALUES ('" + cipher_fund_ecsponat + "', '" + cipher_stock_ecsponat + "', '" + reg_number_ecsponat + "', '" + name_ecsponat + "', " +
                                " '" + size + "', '" + dateCreate + "', '" + imgName + "', '" + description + "', '" + inComeDate + "', " + status + ")";
            }
            if (db.transactionEcsponat(query, materialsList))
            {
                callMessageBox.ShowInfo("Успешно!");
                if (!_isEdit)
                {
                    clearFields();
                }
                return;
            }
            else
            {
                callMessageBox.ShowError("Ошибка при работе с базой!");
                return;
            }
        }

        // Обработчики пользовательских событий

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            ecsponatForm f = new ecsponatForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки сохранения/редактирования данных экспоната
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAllFill())
            {
                callMessageBox.ShowWarning("Заполните все поля!");
                return;
            }

            string cipher_fund_ecsponat = cipher_fund_ecsponatBox.Text;
            string cipher_stock_ecsponat = cipher_stock_ecsponatBox.Text;
            string reg_number_ecsponat = reg_number_ecsponatBox.Text;

            string query = "SELECT * FROM ecsponat WHERE cipher_fund_ecsponat = '" + cipher_fund_ecsponat + "' AND " +
                "cipher_stock_ecsponat = '" + cipher_stock_ecsponat + "' AND " +
                "reg_number_ecsponat = '" + reg_number_ecsponat + "' ";

            if (_isEdit)
            {
                query += " AND NOT id_ecsponat =" + _ecspID;
            }

            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Произошла ошибка!\nНевозможно выполнить запрос.");
                return;
            }
            if (table.Rows.Count != 0)
            {
                callMessageBox.ShowWarning("Данный музейный номер уже существует!");
                return;
            }

            insertData();
        }

        // Обработчик кнопки выбора файла изображения
        private void chooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;

                FileInfo fileInfo = new FileInfo(selectedFileName);
                if (fileInfo.Length > 3 * 1024 * 1024) // Проверка на размер больше 3 Мб
                {
                    MessageBox.Show("Размер файла слишком большой. Пожалуйста, выберите файл размером не более 3 Мб.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                BitmapImage bitmap = new BitmapImage(new Uri(selectedFileName));
                imageBox.Source = bitmap;
            }
        }

        // Обработчик изменения выбранного статуса
        private void statusBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (statusBox.SelectedIndex == 0)
            {
                hallBox.Visibility = Visibility.Visible;
                hallLabel.Visibility = Visibility.Visible;
                getHallofEcsp();
            }
            else
            {
                hallBox.Visibility = Visibility.Hidden;
                hallLabel.Visibility = Visibility.Hidden;
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
            _sessionTimeoutService.Reset();
        }

        // Обработчик события движения мыши в окне
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        // Обработчики для различных полей, запрещающие ввод текста
        private void dateInComePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void statusBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void hallBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void cipher_fund_ecsponatBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void cipher_stock_ecsponatBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void reg_number_ecsponatBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void descBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void materialBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void sizeBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Позволяем использование клавиши Backspace и пробел
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        // Обработчики для полей, разрешающие ввод только кириллицы, цифр и специальных символов
        private void descBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]"))
            {
                e.Handled = true;
            }
        }

        private void sizeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]"))
            {
                e.Handled = true;
            }
        }

        private void cipher_fund_ecsponatBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\-]");
        }

        private void cipher_stock_ecsponatBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\-]");
        }

        private void reg_number_ecsponatBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\-]");
        }

        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\,-]");
        }

        private void materialBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]");
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }
    }
}
