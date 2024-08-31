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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace museumAIS
{
    /// <summary>
    /// Interaction logic for rowEcsponat.xaml
    /// </summary>
    public partial class rowEcsponat : UserControl
    {
        private string _IdEcsp;
        private string _ArticleEcsp;
        private string _NameEcsp;
        private string _Desc;
        private string _SizeEcsp;
        private string _DateCreate;
        private string _Status;
        private Image _ImageEcsp;
        private string _Materials;

        //d:DesignHeight="130" d:DesignWidth="520" FontSize="14">

        private static rowEcsponat _currentSelected;

        public event EventHandler<rowItemClickEvent> Click;
        public event EventHandler<dellButtonClickEvent> del_Click; // Новое событие для клика по delButton

        public rowEcsponat()
        {
            InitializeComponent();
        }

        public string IdEcsp {
            get { return _IdEcsp; }
            set { _IdEcsp = value; idEcspL.Content = value; }  
        }
        public string ArticleEcsp
        {
            get { return _ArticleEcsp; }
            set { _ArticleEcsp = value; articleEcspL.Content = value; }
        }
        public string Name_ecsp {
            get { return _NameEcsp; }
            set { _NameEcsp = value; nameL.Content = value; } 
        }
        public string Desc {
            get { return _Desc; }
            set { _Desc = value; descpL.Text = value; } 
        }
        public string SizeEcsp {
            get { return _SizeEcsp; }
            set { _SizeEcsp = value; sizeL.Content = value; }
        }
        public string DateCreate {
            get { return _DateCreate; }
            set { _DateCreate = value; dateCreateL.Content = value; }
        }
        public string Status {
            get { return _Status; }
            set { _Status = value; statusL.Content = value; }
        }
        public Image ImageEcsp {
            get { return _ImageEcsp; }
            set { 
                _ImageEcsp = value; 
                ecspImage.Source = value.Source; 
            }
        }
        public string Materials {
            get { return _Materials; }
            set { _Materials = value; materialL.Text = value; }
        }

        private void delButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие, если оно > не null
            del_Click?.Invoke(this, new dellButtonClickEvent(_IdEcsp));
        }

        private void rowControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, new rowItemClickEvent(_IdEcsp, _Status));

            if (_currentSelected != null && _currentSelected != this)
            {
                // Сброс цвета границы предыдущего контрола
                _currentSelected.border.BorderBrush = new SolidColorBrush(Colors.Black); // Замените OriginalColor на оригинальный цвет
            }

            // Выделение текущего контрола
            this.border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF66FFA3"));

            // Сохранение ссылки на текущий контрол
            _currentSelected = this;
        }

        public void ChangeBackgroundColor(string colorName)
        {
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
        }
    }
}
