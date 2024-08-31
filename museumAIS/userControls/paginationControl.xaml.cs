using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace museumAIS.userControls
{
    /// <summary>
    /// Interaction logic for paginationControl.xaml
    /// </summary>
    public partial class paginationControl : UserControl, INotifyPropertyChanged
    {
        private string _prevPageNum;
        private string _curPageNum;
        private string _nextPageNum;

        public string PrevPageNum
        {
            get => _prevPageNum;
            set
            {
                _prevPageNum = value;
                OnPropertyChanged();
            }
        }

        public string CurPageNum
        {
            get => _curPageNum;
            set
            {
                _curPageNum = value;
                OnPropertyChanged();
            }
        }

        public string NextPageNum
        {
            get => _nextPageNum;
            set
            {
                _nextPageNum = value;
                OnPropertyChanged();
            }
        }

        public paginationControl()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler exitBut_Click;

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            if (exitBut_Click != null) { exitBut_Click.Invoke(sender, e); }
        }

        public event RoutedEventHandler prevPageBut_Click;
        public event RoutedEventHandler nextPageBut_Click;
        public event RoutedEventHandler firstPageBut_Click;
        public event RoutedEventHandler lastPageBut_Click;

        private void prevPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (prevPageBut_Click != null) { prevPageBut_Click.Invoke(sender, e); }
        }

        private void firstPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (firstPageBut_Click != null) { firstPageBut_Click.Invoke(sender, e); }
        }

        private void nextPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (nextPageBut_Click != null) { nextPageBut_Click.Invoke(sender, e); }
        }

        private void lastPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lastPageBut_Click != null) { lastPageBut_Click.Invoke(sender, e); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
