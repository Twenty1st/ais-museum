using museumAIS.Classes;
using museumAIS.curator_s_Forms;
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
    /// Логика взаимодействия для mainCuratorForm.xaml
    /// </summary>
    public partial class mainCuratorForm : Window
    {
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        // Конструктор формы главного куратора
        public mainCuratorForm()
        {
            InitializeComponent();

            // Инициализация сервиса таймаута сессии
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            // Отображение приветственного сообщения с именем пользователя
            welcommsgLabel.Content += userData.UserName;
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            authorizeForm f = new authorizeForm(); // Создание новой формы авторизации
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы авторизации в диалоговом режиме
        }

        // Обработчик кнопки перехода к форме экскурсий
        private void goExcursFButton_Click(object sender, RoutedEventArgs e)
        {
            excursForm f = new excursForm(); // Создание новой формы экскурсий
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы экскурсий в диалоговом режиме
        }

        // Обработчик кнопки перехода к форме гидов
        private void goGuidsFButton_Click(object sender, RoutedEventArgs e)
        {
            guidsForm f = new guidsForm(); // Создание новой формы гидов
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы гидов в диалоговом режиме
        }

        // Обработчик кнопки перехода к форме залов
        private void goHallsFButton_Click(object sender, RoutedEventArgs e)
        {
            hallsForm f = new hallsForm(); // Создание новой формы залов
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы залов в диалоговом режиме
        }

        // Обработчик события таймаута сессии
        private void OnSessionTimedOut()
        {
            // Открытие формы авторизации
            authorizeForm f = new authorizeForm();
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы авторизации в диалоговом режиме
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

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop(); // Остановка таймера сессии
        }
    }
}
