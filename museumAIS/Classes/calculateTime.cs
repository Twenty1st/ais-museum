using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS
{
    public class calculateTime
    {
        public int CalculateTotalHours(DataTable events)
        {
            int totalHours = 0;

            // Получение текущей даты
            DateTime currentDate = DateTime.Now;

            // Получение первого дня текущего месяца
            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

            // Получение последнего дня текущего месяца
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Подсчет общей продолжительности мероприятий за текущий месяц
            foreach (DataRow ev in events.Rows)
            {
                DateTime exc_start_date = DateTime.Parse(ev["exc_start_date"].ToString());
                DateTime exc_end_date = DateTime.Parse(ev["exc_end_date"].ToString());
                int exc_duration = Convert.ToInt32(ev["exc_duration"].ToString());
                // Проверка, что мероприятие начинается и заканчивается в пределах текущего месяца
                if (exc_start_date >= firstDayOfMonth)
                {
                    if (exc_end_date <= lastDayOfMonth)
                    {
                        totalHours += exc_duration * (exc_end_date.Day - exc_start_date.Day+1);
                    }
                    else
                    {
                        totalHours += exc_duration * (lastDayOfMonth.Day - exc_start_date.Day+1);
                    }
                }
                    
            }

            // Возврат общей продолжительности мероприятий за текущий месяц
            return totalHours;
        }
    }
}
