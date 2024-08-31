using museumAIS.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
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
    public partial class editUserForm : Window, INotifyPropertyChanged
    {
        // Объект для хеширования паролей
        private hashFunction hashing;
        // Объект для взаимодействия с базой данных
        private helpForDB DBhelp;
        // Сервис для отслеживания таймаута сессии
        private SessionTimeoutService _sessionTimeoutService;

        // Список привилегий пользователей
        private List<string> statusList;
        // Флаг для проверки состояния пароля
        private bool isCheckPWD = false;

        // Флаг для режима редактирования пользователя
        private bool _isEdit;
        // ID пользователя
        private string _userID;
        // Текущий пароль пользователя
        private string _userPWD;

        // Источник изображения для пароля
        private ImageSource _pwdImage;
        // Событие изменения свойства
        public event PropertyChangedEventHandler PropertyChanged;

        // Свойство для работы с изображением пароля
        public ImageSource pwdImage
        {
            get { return _pwdImage; }
            set
            {
                _pwdImage = value;
                OnPropertyChanged("pwdImage");
            }
        }

        // Конструктор формы
        public editUserForm(bool isEdit, string userID)
        {
            InitializeComponent();

            // Инициализация сервиса таймаута сессии
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

            // Установка режима редактирования и ID пользователя
            _isEdit = isEdit;
            _userID = userID;

            // Изменение текста кнопки в зависимости от режима
            if (isEdit)
            {
                editButton.Content = "Редактировать";
                lbl.Content = "Пароль";
            }

            // Установка стиля окна без стандартных кнопок
            //this.WindowStyle = System.Windows.WindowStyle.None;

            // Разрешение изменения размера формы через правый нижний угол
            this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;

            // Инициализация объектов для хеширования и работы с базой данных
            hashing = new hashFunction();
            DBhelp = new helpForDB();

            // Установка контекста данных для привязки
            this.DataContext = this;

            // Установка изображения для скрытого пароля
            pwdImage = new BitmapImage(new Uri("/icons/icon-hidePWD.png", UriKind.RelativeOrAbsolute));

            // Получение данных для ComboBox
            getDataToCmbBox();
        }

        // Метод для получения данных для ComboBox
        private void getDataToCmbBox()
        {
            // Запрос на получение привилегий пользователей
            string query = "SELECT * FROM user_privilege";
            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return;
            }

            // Заполнение списка привилегий
            statusList = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                privelegeBox.Items.Add(row["privilegeName"].ToString());
                statusList.Add(row["privilegeID"].ToString());
            }

            // Если режим редактирования, то заполнение полей формы данными пользователя
            if (_isEdit)
            {
                query = @"SELECT user_name, user_lastName, user_patronamyc, 
                      user_login, user_pwd, user_privilege.privilegeName 
                      FROM users 
                      JOIN user_privilege ON privilegeID = privilege_user 
                      WHERE id_user = " + _userID;
                table = DBhelp.selectData(query);
                if (table == null)
                {
                    callMessageBox.ShowError("Ошибка подключения к базе данных!");

                    return;
                }

                // Заполнение полей формы данными пользователя
                nameBox.Text = table.Rows[0][0].ToString();
                lastnBox.Text = table.Rows[0][1].ToString();
                patrBox.Text = table.Rows[0][2].ToString();
                loginBox.Text = table.Rows[0][3].ToString();
                _userPWD = table.Rows[0][4].ToString();
                privelegeBox.Text = table.Rows[0][5].ToString();
            }
        }

        // Метод для поиска индекса ComboBox по значению
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

        // Метод для получения пароля
        private string getPWD()
        {
            string pwd = "";
            if (_isEdit)
            {
                // Если новый пароль не введен, используем старый
                if (pwdNBox.Password == "")
                {
                    pwd = _userPWD;
                }
                else
                {
                    pwd = hashing.HashData(pwdNBox.Password);
                }
            }
            else
            {
                pwd = hashing.HashData(pwdNBox.Password);
            }
            return pwd;
        }

        // Обработчик нажатия кнопки редактирования/создания пользователя
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAllFill())
            {
                callMessageBox.ShowWarning("Заполните все поля!");
                return;
            }

            // Получение данных из полей формы
            string name = nameBox.Text;
            string lastN = lastnBox.Text;
            string patronamyc = patrBox.Text == "" ? "" : patrBox.Text;
            string login = loginBox.Text;
            string hashPWD = getPWD();
            string statusID = statusList[FindComboBoxIndexByValue(privelegeBox)];
            string query = "";

            // Формирование SQL-запроса в зависимости от режима
            if (!_isEdit)
            {
                query = "INSERT INTO users (user_name, user_lastName, user_patronamyc, user_login, user_pwd, privilege_user) " +
                        "VALUES ('" + name + "', '" + lastN + "', '" + patronamyc + "', '" + login + "', '" + hashPWD + "', " + statusID + ")";

                // Выполнение запроса на добавление пользователя
                if (!DBhelp.editDBData(query))
                {
                    callMessageBox.ShowError("Ошибка при работе с базой!");
                    return;
                }
                clearBoxes();
            }
            else
            {
                query = "UPDATE users SET user_name = '" + name + "', user_lastName = '" + lastN + "', " +
                        "user_patronamyc = '" + patronamyc + "', user_login = '" + login + "', " +
                        "user_pwd = '" + hashPWD + "', privilege_user = " + statusID + " WHERE " +
                        "id_user = " + _userID;

                // Выполнение запроса на обновление пользователя
                if (!DBhelp.editDBData(query))
                {
                    callMessageBox.ShowError("Ошибка при работе с базой!");
                    return;
                }
            }

            callMessageBox.ShowInfo("Успешно!");
        }

        // Метод для проверки заполненности всех обязательных полей
        private bool isAllFill()
        {
            if (loginBox.Text == "" || nameBox.Text == "" || lastnBox.Text == "" || privelegeBox.Text == "")
            {
                return false;
            }
            if (!_isEdit && pwdNBox.Password == "")
            {
                return false;
            }
            return true;
        }

        // Метод для очистки полей формы
        private void clearBoxes()
        {
            nameBox.Text = "";
            lastnBox.Text = "";
            patrBox.Text = "";
            loginBox.Text = "";
            pwdVBox.Text = "";
            pwdNBox.Password = "";
        }

        // Обработчик нажатия на кнопку генерации пароля
        private void generateButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            generatePassword rndPWD = new generatePassword();
            pwdNBox.Password = Convert.ToString(rndPWD.getRandmPwd());
            pwdVBox.Text = Convert.ToString(rndPWD.getRandmPwd());
        }

        // Метод для уведомления об изменении свойства
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        // Обработчик нажатия на иконку для показа/скрытия пароля
        private void checkPWD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCheckPWD)
            {
                isCheckPWD = false;
                pwdImage = new BitmapImage(new Uri("/icons/icon-hidePWD.png", UriKind.RelativeOrAbsolute));
                pwdNBox.Password = pwdVBox.Text;
                pwdNBox.Visibility = Visibility.Visible;
                pwdVBox.Visibility = Visibility.Hidden;
            }
            else
            {
                isCheckPWD = true;
                pwdImage = new BitmapImage(new Uri("/icons/icon-checkPWD.png", UriKind.RelativeOrAbsolute));
                pwdVBox.Text = pwdNBox.Password;
                pwdNBox.Visibility = Visibility.Hidden;
                pwdVBox.Visibility = Visibility.Visible;
            }
        }

        // Обработчик нажатия кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            usersForm f = new usersForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик изменения пароля в PasswordBox
        private void pwdNBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
           // _userNewPWD = pwdNBox.Password;

        }

        // Обработчик изменения пароля в TextBox
        private void pwdVBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            pwdNBox.Password = pwdVBox.Text;
        }

        // Обработчик события таймаута сессии
        private void OnSessionTimedOut()
        {
            // Открытие формы авторизации
            authorizeForm f = new authorizeForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчики событий для сброса таймаута сессии при активности пользователя
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }

        // Обработчики событий для запрета ввода пробелов и русских букв
        private void loginBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"\p{IsCyrillic}"))
            {
                e.Handled = true;
                return;
            }
        }

        private void pwdNBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"\p{IsCyrillic}"))
            {
                e.Handled = true;
                return;
            }
        }

        private void privelegeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчики событий для обработки ввода в текстовые поля
        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
            
        }

        private void lastnBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void patrBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Space)
            {
                e.Handled = false;
            }
        }

        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}]");
        }

        private void lastnBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}\-]");
        }

        private void patrBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\p{IsCyrillic}]");
        }

        // Обработчик события закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }

        // Обработчики событий для автоматического преобразования первой буквы в заглавную
        private void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (nameBox.Text.Length == 1)
                nameBox.Text = nameBox.Text.ToUpper();
            nameBox.Select(nameBox.Text.Length, 0);
        }

        private void lastnBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text.Length > 0)
            {
                textBox.Text = char.ToUpper(textBox.Text[0]) + textBox.Text.Substring(1);
            }
            if (textBox.Text.Contains("-") && textBox.Text.Length > textBox.Text.IndexOf('-') + 1)
            {
                int pos = textBox.Text.IndexOf('-');
                textBox.Text = textBox.Text.Remove(pos + 1, 1).Insert(pos + 1, textBox.Text[pos + 1].ToString().ToUpper());
            }
            textBox.Select(textBox.Text.Length, 0);
        }

        private void patrBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patrBox.Text.Length == 1)
                patrBox.Text = patrBox.Text.ToUpper();
            patrBox.Select(patrBox.Text.Length, 0);
        }

        private void pwdNBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (pwdNBox.Password.Length == 6)
            {
                e.Handled = true;
            }
        }
    }
}
