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
    /// Interaction logic for createDocForm.xaml
    /// </summary>
    public partial class createDocForm : Window
    {
        private SessionTimeoutService _sessionTimeoutService;
        private static helpForDB db;

        private Dictionary<string, string> dataDict;
        private List<string> ecspIDList;

        public createDocForm()
        {
            InitializeComponent();

            db = new helpForDB();

            _sessionTimeoutService = new SessionTimeoutService();
            _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;


            setDataInCmbBox();
        }

        private void setDataInCmbBox()
        {
            typeSend.Items.Add("выставку");
            typeSend.Items.Add("временное хранение");

            typeTrasportBox.Items.Add("Сухопутная (специализированный транспорт)");
            typeTrasportBox.Items.Add("Воздушная (специализированный авиарейс)");
            typeTrasportBox.Items.Add("Морская (контейнерные перевозки)");
            typeTrasportBox.Items.Add("Железнодорожная (специализированные вагоны)");
            typeTrasportBox.Items.Add("Курьерская (специализированные службы доставки)");

            string query = "SELECT id_ecsponat, cipher_fund_ecsponat, cipher_stock_ecsponat, reg_number_ecsponat, " +
                "name_ecsponat FROM ecsponat WHERE status_ecsponat = 2";
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return;
            }

            ecspIDList = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                ecspIDList.Add(row["id_ecsponat"].ToString());
                
                string regNum = row["cipher_fund_ecsponat"].ToString() + " " +
                    row["cipher_stock_ecsponat"].ToString() + " " + row["reg_number_ecsponat"].ToString();

                string ecsp = "[" + regNum + "] " + row["name_ecsponat"].ToString();
                ecspBox.Items.Add(ecsp);
            }

            dateSend.DisplayDateStart = DateTime.Today.AddDays(1);
            dateComeBack.DisplayDateStart = DateTime.Today.AddDays(2);


            dateComeBack.DisplayDateEnd = DateTime.Today.AddYears(1);
            dateSend.DisplayDateEnd = DateTime.Today.AddYears(1);
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

        private void Window_Closed(object sender, EventArgs e)
        {
            _sessionTimeoutService.Stop();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            mainKeeperForm f = new mainKeeperForm();
            this.Close();
            f.ShowDialog();
        }

        private bool setAbsentEcsp()
        {
            //string queryHall = "DELETE FROM ecsponat_hall WHERE id_ecsp = " + ecspIDList[ecspBox.SelectedIndex];

            int status = typeSend.SelectedIndex == 0 ? 3 : 5;
            string queryStatus = "UPDATE ecsponat SET status_ecsponat = "+ status +" " +
                "WHERE id_ecsponat = " + ecspIDList[ecspBox.SelectedIndex];

            string queryInsert = "INSERT INTO absent_ecsponat (ecsponat_id, date_comeback) VALUES " +
                "("+ecspIDList[ecspBox.SelectedIndex]+", '"+dateComeBack.DisplayDate.ToString("yyyy-MM-dd")+"')";

            if (!db.changeStatusTransaction(queryStatus, queryInsert, "", ""))
            {
                callMessageBox.ShowError("Ошибка при работе с базой!");
                return false;
            }

            return true;
        }

        private void clearField()
        {
            typeSend.SelectedIndex = -1;
            nameBox.Text = "";
            addressBox.Text = "";
            manBox.Text = "";

            ecspBox.SelectedIndex = -1;
            typeTrasportBox.SelectedIndex = -1;

            dateSend.Text = "";
            dateComeBack.Text = "";
        }

        private bool isAllFill()
        {
            if(typeSend.SelectedIndex == -1 || nameBox.Text == "" || addressBox.Text == "" || 
                manBox.Text == "" || ecspBox.SelectedIndex == -1 || typeTrasportBox.SelectedIndex == -1 || 
                !dateSend.SelectedDate.HasValue || !dateComeBack.SelectedDate.HasValue || 
                (nameVBox.Visibility == Visibility.Visible && nameVBox.Text == ""))
            {
                callMessageBox.ShowWarning("Заполните все поля!");
                return false;
            }

            return true;
        }

        private void createDocButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isAllFill())
            {
                return;
            }

            if (takeData())
            {
                if (setAbsentEcsp())
                {
                    createSendDoc.createDocument(dataDict);
                }
            }
        }

        private bool takeData()
        {
            dataDict = new Dictionary<string, string>();

            dataDict["where"] = typeSend.Text;

            DateTime currentDate = DateTime.Now; // Получаем текущую дату
            string customFormattedDate = currentDate.ToString("d MMMM yyyy 'года'", new System.Globalization.CultureInfo("ru-RU"));
            dataDict["curDate"] = customFormattedDate;

            string fio = getCurKeeper();
            if(fio == "")
            {
                return false;
            }
            dataDict["curKeeper"] = fio;

            dataDict["recMuseum"] = nameBox.Text;
            dataDict["recVistavka"] = nameVBox.Text;

            dataDict["recAddress"] = addressBox.Text;
            dataDict["recKeeper"] = manBox.Text;

            dataDict["type"] = typeTrasportBox.Text;

            customFormattedDate = dateSend.SelectedDate.Value.ToString("d MMMM yyyy 'года'", new System.Globalization.CultureInfo("ru-RU"));
            dataDict["dateSend"] = customFormattedDate;

            customFormattedDate = dateComeBack.SelectedDate.Value.ToString("d MMMM yyyy 'года'", new System.Globalization.CultureInfo("ru-RU"));
            dataDict["dateComeBack"] = customFormattedDate;

            if (!getEcspData())
            {
                return false;
            }

            return true;
        }

        private string getCurKeeper()
        {
            string query = "SELECT user_name, user_lastName, user_patronamyc FROM users WHERE id_user = " + userData.UserID;
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return "";
            }

            string fio = table.Rows[0][1].ToString() + " " + table.Rows[0][0].ToString() +
                " " + table.Rows[0][2].ToString();

            return fio;
        }

        private bool getEcspData()
        {
            string query = "SELECT cipher_fund_ecsponat, cipher_stock_ecsponat, reg_number_ecsponat, " +
                "name_ecsponat, size_ecponat, date_create_ecsponat, description_ecsponat, " +
                "GROUP_CONCAT(name_material ORDER BY name_material SEPARATOR ', ') AS materials " +
                "FROM ecsponat JOIN material_ecsponat ON ecsponatM_id = id_ecsponat " +
                "WHERE id_ecsponat = " + ecspIDList[ecspBox.SelectedIndex];
            DataTable table = db.selectData(query);
            if (table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return false;
            }

            dataDict["curEcspRegNum"] = table.Rows[0][0].ToString() + " " +table.Rows[0][1].ToString() + " " +table.Rows[0][2].ToString();
            dataDict["curEcspName"] = table.Rows[0][3].ToString();

            dataDict["curEcspSize"] = table.Rows[0][4].ToString();
            dataDict["curEcspCreateDate"] = table.Rows[0][5].ToString();
            dataDict["curEcspDesc"] = table.Rows[0][6].ToString();
            dataDict["curEcspMaterial"] = table.Rows[0][7].ToString();

            return true;
        }

        private void typeSend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(typeSend.SelectedIndex == 0)
            {
                label.Visibility = Visibility.Visible;
                nameVBox.Visibility = Visibility.Visible;

                nameBox.Width = 330;
                labelM.Content = "Наименование музея где проходит выставка";
            }
            else
            {
                label.Visibility = Visibility.Hidden;
                nameVBox.Visibility = Visibility.Hidden;

                nameBox.Width = 428;
                labelM.Content = "Наименование музея";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clearField();
        }

        private void typeSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ecspBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void typeTrasportBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void dateSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void dateComeBack_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]"))
            {
                e.Handled = true;
            }
        }

        private void addressBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void addressBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]"))
            {
                e.Handled = true;
            }
        }

        private void manBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void manBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}\-]"))
            {
                e.Handled = true;
            }
        }

        private void nameVBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"[\p{IsCyrillic}0-9\.,-]"))
            {
                e.Handled = true;
            }
        }

        private void nameVBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
