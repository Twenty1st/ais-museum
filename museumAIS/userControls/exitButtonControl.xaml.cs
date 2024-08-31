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
    /// Interaction logic for exitButtonControl.xaml
    /// </summary>
    public partial class exitButtonControl : UserControl
    {
        public exitButtonControl()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler exitBut_Click;

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            if(exitBut_Click != null) { exitBut_Click.Invoke(sender, e); }
        }
    }
}
