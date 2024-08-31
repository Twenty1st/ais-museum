using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace museumAIS.userControls
{
    public class calendarItem : INotifyPropertyChanged
    {
        private DateTime _date;
        private string _dayOfWeek;
        private string _excursions;
        private SolidColorBrush _background;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged("Date");
            }
        }

        public string DayOfWeek
        {
            get { return _dayOfWeek; }
            set
            {
                _dayOfWeek = value;
                OnPropertyChanged("DayOfWeek");
            }
        }

        public string Excursions
        {
            get { return _excursions; }
            set
            {
                _excursions = value;
                OnPropertyChanged("Excursions");
            }
        }

        public SolidColorBrush Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged("Background");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
