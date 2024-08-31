using museumAIS.admin_s_Forms;
using museumAIS.Classes;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для mainAdministratorForm.xaml
    /// </summary>
    public partial class mainAdministratorForm : Window
    {

        // Сервис для отслеживания таймаута сессии
        private SessionTimeoutService _sessionTimeoutService;

        // Конструктор формы
        public mainAdministratorForm()
        {
            InitializeComponent();

            // Инициализация сервиса таймаута сессии и подписка на событие таймаута
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

            // Отображение приветственного сообщения с именем пользователя
            welcommsgLabel.Content += userData.UserName;
        }

        // Обработчик кнопки перехода к форме пользователей
        private void goUserFButton_Click(object sender, RoutedEventArgs e)
        {
            usersForm f = new usersForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки перехода к форме работы с таблицами
        private void goTablesWorkButton_Click(object sender, RoutedEventArgs e)
        {
            tablesWorkForm f = new tablesWorkForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки перехода к форме работы с справочниками
        private void goDBWorkButton_Click(object sender, RoutedEventArgs e)
        {
            dataBaseWorkForm f = new dataBaseWorkForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на форму авторизации
            authorizeForm f = new authorizeForm();
            this.Close();
            f.ShowDialog();
        }

        // Обработчик события таймаута сессии
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

        private void handBooksButton_Click(object sender, RoutedEventArgs e)
        {
            handbooksForm f = new handbooksForm();
            this.Close();
            f.ShowDialog();
        }
    }
}
