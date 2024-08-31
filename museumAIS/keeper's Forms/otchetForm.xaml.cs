using museumAIS.Classes;
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

namespace museumAIS.keeper_s_Forms
{
    /// <summary>
    /// Interaction logic for otchetForm.xaml
    /// </summary>
    public partial class otchetForm : Window
    {
        createWordDoc createWord = new createWordDoc(); // Создание объекта для работы с документами Word
        private SessionTimeoutService _sessionTimeoutService; // Сервис для обработки таймаута сессии
        private static helpForDB DBhelp; // Объект для работы с базой данных

        List<string> queries; // Список запросов

        // Конструктор формы создания отчетов
        public otchetForm()
        {
            InitializeComponent();

            DBhelp = new helpForDB();
            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut; // Подписка на событие таймаута сессии

            setDataInCmb(); // Установка данных в комбобокс
        }

        // Установка данных в комбобокс
        private void setDataInCmb()
        {
            statusBox.Items.Add("Все");
            statusBox.Items.Add("Выставлен");
            statusBox.Items.Add("В резерве");
            statusBox.Items.Add("На выставке");
            statusBox.Items.Add("На реставрации");
            statusBox.Items.Add("Временно в другом музее");

            queries = new List<string>();

            queries.Add(@"SELECT name_ecsponat, description_ecsponat, date_create_ecsponat, 
            hall_name as where_ecsp, image_ecsponat
            FROM ecsponat_hall
            JOIN ecsponat ON id_ecsp = id_ecsponat
            JOIN halls ON num_hall = hall_num
            WHERE ecsponat.status_ecsponat = 1");
            queries.Add(@"SELECT name_ecsponat, description_ecsponat, date_create_ecsponat, 
                    status_ecsponat as where_ecsp, image_ecsponat
                    FROM ecsponat
                    WHERE ecsponat.status_ecsponat = 2");
            queries.Add(@"SELECT name_ecsponat, description_ecsponat, date_create_ecsponat, 
                date_comeback as where_ecsp,  image_ecsponat
                FROM absent_ecsponat
                JOIN ecsponat ON ecsponat_id = id_ecsponat
                WHERE ecsponat.status_ecsponat = 3");
            queries.Add(@"SELECT name_ecsponat, description_ecsponat, date_create_ecsponat, 
                    date_comeback as where_ecsp, image_ecsponat
                    FROM absent_ecsponat
                    JOIN ecsponat ON ecsponat_id = id_ecsponat
                    WHERE ecsponat.status_ecsponat = 4");
            queries.Add(@"SELECT name_ecsponat, description_ecsponat, date_create_ecsponat, 
                    date_comeback as where_ecsp, image_ecsponat
                    FROM absent_ecsponat
                    JOIN ecsponat ON ecsponat_id = id_ecsponat
                    WHERE ecsponat.status_ecsponat = 5");
        }

        // Получение запроса в зависимости от выбранного статуса
        private string getQuery()
        {
            string query = "";
            if (statusBox.SelectedIndex == 0)
            {
                query = string.Join(" union all ", queries);
                return query;
            }

            query = queries[statusBox.SelectedIndex - 1];

            return query;
        }

        // Загрузка текущих данных из базы
        private DataTable loadCurData()
        {
            string query = getQuery();

            DataTable table = DBhelp.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");

                return null;
            }

            return table;
        }

        // Обработчик кнопки выхода
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Обработчик кнопки создания отчета
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (statusBox.SelectedIndex < 0)
            {
                callMessageBox.ShowWarning("Выберете вариант отчета!");
                return;
            }

            createWord.createWordDocument(loadCurData(), statusBox.Text);
        }

        // Обработчик запрещающий ввод текста в комбобокс
        private void statusBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
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
            _sessionTimeoutService.Reset();
        }

        // Обработчик события движения мыши в окне
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _sessionTimeoutService.Reset();
        }
    }
}
