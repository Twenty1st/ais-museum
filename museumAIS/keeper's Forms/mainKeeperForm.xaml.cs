using museumAIS.Classes;
using museumAIS.keeper_s_Forms;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace museumAIS
{
    /// <summary>
    /// Логика взаимодействия для mainKeeperForm.xaml
    /// </summary>
    public partial class mainKeeperForm : Window
    {
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии

        // Конструктор формы главного хранителя
        public mainKeeperForm()
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

        // Обработчик кнопки перехода к форме экспонатов
        private void goEcspFButton_Click(object sender, RoutedEventArgs e)
        {
            ecsponatForm f = new ecsponatForm(); // Создание новой формы экспонатов
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы экспонатов в диалоговом режиме
        }

        // Обработчик кнопки перехода к форме документов (заглушка)
        private void goDocsFButton_Click(object sender, RoutedEventArgs e)
        {
            createDocForm f = new createDocForm(); // Создание новой формы 
            this.Close(); // Закрытие текущей формы
            f.ShowDialog(); // Открытие формы  в диалоговом режиме
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
