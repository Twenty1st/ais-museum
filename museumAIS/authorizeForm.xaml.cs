using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
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
using museumAIS.Classes;

using System.Windows.Threading;
using System.ComponentModel;
using museumAIS.curator_s_Forms;
using System.Text.RegularExpressions;

namespace museumAIS
{
    /// <summary>
    /// Логика взаимодействия для authorizeForm.xaml
    /// </summary>
    public partial class authorizeForm : Window, INotifyPropertyChanged
    {
        private List<string> loginList; // Список логинов пользователей
        private List<string> idList; // Список id пользователей

        private helpForDB BD; // Объект для работы с базой данных
        private hashFunction hashing; // Объект для хеширования паролей
        private generateCaptcha genCaptcha; // Объект для генерации капчи
        private static userRoot userRoot;

        private int countTry = 0; // Счетчик попыток входа
        private DispatcherTimer timer;
        private int countdown = 10; // Таймер для блокировки кнопки авторизации
        private string captchaImageText = ""; // Текст капчи

        private bool isCheckPWD = false; // Флаг состояния видимости пароля
        private System.Drawing.Image eyeImage;

        private ImageSource _checkImage;
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource checkImage
        {
            get { return _checkImage; }
            set
            {
                _checkImage = value;
                OnPropertyChanged("checkImage");
            }
        }

        // Конструктор формы авторизации
        public authorizeForm()
        {
            chooseDirectories.createDirectories(); // Создание необходимых директорий

            InitializeComponent();

            this.DataContext = this;

            BD = new helpForDB();
            hashing = new hashFunction();
            genCaptcha = new generateCaptcha();
            userRoot = new userRoot();

            checkImage = new BitmapImage(new Uri("/icons/icon-hidePWD.png", UriKind.RelativeOrAbsolute));

            getDataToCmbBox(); // Загрузка данных в комбобокс
        }

        // Загрузка данных в комбобокс
        private void getDataToCmbBox()
        {
            string query = "SELECT user_name, user_lastName, user_patronamyc, user_login, id_user FROM users";
            DataTable table = BD.selectData(query);
            if (table == null)
            {
                MessageBox.Show("Connection Error", "Error");
                return;
            }

            loginList = new List<string>();
            idList = new List<string>();

            string fio;
            foreach (DataRow row in table.Rows)
            {
                if (row["user_patronamyc"].ToString().Equals(""))
                {
                    fio = row["user_name"].ToString() + " " + row["user_lastName"].ToString().Substring(0, 1) + ".";
                }
                else
                {
                    fio = row["user_name"].ToString() + " " + row["user_patronamyc"].ToString() +
                          " " + row["user_lastName"].ToString().Substring(0, 1) + ".";
                }

                loginBox.Items.Add(fio);
                loginList.Add(row["user_login"].ToString());
                idList.Add(row["id_user"].ToString());
            }
        }

        // Обработчик кнопки авторизации
        private void authorizeButton_Click(object sender, RoutedEventArgs e)
        {
            //skipForm(); // Пропуск формы для отладки           

            if (pwdNBox.Password == "root123")
            {
                if (userRoot.checkUsers())
                {
                    userData.UserName = "root";
                    this.Hide();
                    mainAdministratorForm f1 = new mainAdministratorForm();
                    this.Close();
                    f1.ShowDialog();
                }
            }
            if (!isAllFill())
            {
                callMessageBox.ShowWarning("Заполните все поля!");
                return;
            }

            string hashPWD = hashing.HashData(pwdNBox.Password);
            string login = loginList[loginBox.SelectedIndex];

            string privilege = getUserData(login, hashPWD);

            if (!checkCorrectData(privilege))
            {
                return;
            }

            userData.UserName = loginBox.Text;
            userData.UserID = idList[loginBox.SelectedIndex];

            helpForDB db = new helpForDB();
            if (!db.createBackup())
            {
                callMessageBox.ShowWarning("Произошла ошибка при создании бэкапа");
            }

            switch (privilege)
            {
                case "Администратор":
                    this.Hide();
                    mainAdministratorForm f1 = new mainAdministratorForm();
                    this.Close();
                    f1.ShowDialog();
                    break;
                case "Хранитель":
                    this.Hide();
                    mainKeeperForm f2 = new mainKeeperForm();
                    this.Close();
                    f2.ShowDialog();
                    break;
                default:
                    this.Hide();
                    mainCuratorForm f3 = new mainCuratorForm();
                    this.Close();
                    f3.ShowDialog();
                    break;
            }
        }

        // Проверка корректности данных
        private bool checkCorrectData(string privilege)
        {
            countTry++;
            if (countTry >= 2)
            {
                if (!captchaImageText.Equals(captchaText.Text))
                {
                    callMessageBox.ShowWarning("Неверная капча!");
                    authorizeButton.IsEnabled = false;
                    timerLabel.Visibility = Visibility.Visible;
                    timerCreate();
                    return false;
                }
            }

            if (privilege == "")
            {
                if (countTry == 1)
                {
                    captchaImage.Source = genCaptcha.GenerateImageCaptcha();
                    captchaImageText = genCaptcha.captchaText;
                    this.Height = 450;
                    captchaImage.Visibility = Visibility.Visible;
                    refreshCaptButton.Visibility = Visibility.Visible;
                    captchaText.Visibility = Visibility.Visible;
                }
                else
                {
                    authorizeButton.IsEnabled = false;
                    timerLabel.Visibility = Visibility.Visible;
                    timerCreate();
                }
                callMessageBox.ShowWarning("Неправильный логин или пароль!");
                return false;
            }
            return true;
        }

        // Пропуск формы для отладки
        private void skipForm()
        {
            ecsponatForm f3 = new ecsponatForm();
            this.Close();
            f3.ShowDialog();
        }

        // Создание таймера
        private async void timerCreate()
        {
            // Запуск таймера
            StartTimer();

            // Ожидание 10 секунд
            await Task.Delay(10000);

            // Разблокировка кнопки
            authorizeButton.IsEnabled = true;

            // Остановка таймера
            StopTimer();
        }

        // Получение данных пользователя
        private string getUserData(string login, string pwd)
        {
            string query = "SELECT privilegeName FROM users JOIN user_privilege ON privilegeID = privilege_user WHERE user_login = '" + login + "' AND user_pwd = '" + pwd + "'";
            DataTable table = BD.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return "";
            }

            if (table.Rows.Count != 0)
            {
                return table.Rows[0][0].ToString();
            }
            else
            {
                return "";
            }
        }

        // Проверка заполненности всех необходимых полей
        private bool isAllFill()
        {
            if (loginBox.Text == "" || (pwdVBox.Text == "" && pwdNBox.Password == ""))
            {
                return false;
            }
            return true;
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            helpForDB db = new helpForDB();
            if (!db.createBackup())
            {
                callMessageBox.ShowWarning("Произошла ошибка при создании бэкапа");
            }

            // Закрытие приложения
            Application.Current.Shutdown();
        }

        // Уведомление об изменении свойства
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        // Обработчик клика по значку видимости пароля
        private void checkPWD_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCheckPWD)
            {
                isCheckPWD = false;
                checkImage = new BitmapImage(new Uri("/icons/icon-hidePWD.png", UriKind.RelativeOrAbsolute));
                pwdNBox.Password = pwdVBox.Text;
                pwdNBox.Visibility = Visibility.Visible;
                pwdVBox.Visibility = Visibility.Hidden;
            }
            else
            {
                isCheckPWD = true;
                checkImage = new BitmapImage(new Uri("/icons/icon-hidePWD.png", UriKind.RelativeOrAbsolute));
                pwdVBox.Text = pwdNBox.Password;
                pwdNBox.Visibility = Visibility.Hidden;
                pwdVBox.Visibility = Visibility.Visible;
            }
        }

        // Обновление капчи
        private void refreshCaptButton_Click(object sender, RoutedEventArgs e)
        {
            captchaImage.Source = genCaptcha.GenerateImageCaptcha();
            captchaImageText = genCaptcha.captchaText;
        }

        #region [Timer]
        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void StopTimer()
        {
            timer.Stop();
            countdown = 10;
            UpdateTimerLabel();
            timerLabel.Visibility = Visibility.Hidden;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdown--;
            UpdateTimerLabel();
        }

        private void UpdateTimerLabel()
        {
            timerLabel.Content = $"{countdown}";
        }
        #endregion

        // Обработчики, запрещающие ввод текста в комбобоксы и поля ввода
        private void loginBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void pwdNBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Запрет ввода пробелов
            if (e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            // Запрет ввода русских букв
            if (System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"\p{IsCyrillic}"))
            {
                e.Handled = true;
                return;
            }
        }

        private void captchaText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Запрет ввода пробелов
            if (e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            // Запрет ввода русских букв
            if (System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"\p{IsCyrillic}"))
            {
                e.Handled = true;
                return;
            }

            // Запрет ввода специальных символов
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$"))
            {
                e.Handled = true;
            }
        }

        private void pwdVBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Запрет ввода пробелов
            if (e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            // Запрет ввода русских букв
            if (System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"\p{IsCyrillic}"))
            {
                e.Handled = true;
                return;
            }
        }

        private void pwdNBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            // Пустой обработчик для предотвращения ввода
        }

        private void pwdVBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Синхронизация текста между VisiblePasswordBox и PasswordBox
            pwdNBox.Password = pwdVBox.Text;
        }
    }
}
