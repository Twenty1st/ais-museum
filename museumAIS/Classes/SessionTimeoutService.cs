using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace museumAIS.Classes
{
    public class SessionTimeoutService
    {
        // Последнее время активности
        private DateTime _lastActivity;

        // Таймер для отслеживания времени
        private DispatcherTimer _timer;

        // Событие, которое будет вызвано при истечении времени сессии
        public event Action SessionTimedOut;

        // Конструктор
        public SessionTimeoutService()
        {
            // Инициализация таймера
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(10); // Интервал проверок
            _timer.Tick += Timer_Tick; // Подписка на событие Tick
            Reset(); // Сброс таймера
        }

        // Метод для сброса таймера и обновления времени последней активности
        public void Reset()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start(); // Запуск таймера, если он не запущен
            }
            _lastActivity = DateTime.UtcNow; // Обновление времени последней активности
        }

        // Обработчик события Tick таймера
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Проверка, прошло ли больше времени, чем интервал таймера
            if (DateTime.UtcNow - _lastActivity > _timer.Interval)
            {
                _timer.Stop(); // Остановка таймера
                SessionTimedOut?.Invoke(); // Вызов события SessionTimedOut
            }
        }

        // Метод для остановки таймера
        public void Stop()
        {
            _timer.Stop();
        }
    }
}
