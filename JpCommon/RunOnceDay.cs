using System;
using System.Threading.Tasks;
using System.Timers;

namespace JpCommon
{
    /// <summary>
    /// Clase que ejecuta una función una vez al día. La función proporcionada debe retornar un valor `bool`
    /// indicando `true` si la ejecución fue exitosa.
    /// </summary>
    public class RunOnceADay
    {
        private DateTime _lastDateRun;
        private readonly System.Timers.Timer _runnerTimer;
        private readonly Func<Task<bool>> _callback;
        private bool _isRunning;
        private readonly int _maxAttempts;
        private int _attempts;

        /// <summary>
        /// Inicializa una nueva instancia de RunOnceADay.
        /// </summary>
        /// <param name="callback">Función asíncrona que se ejecutará una vez al día.</param>
        /// <param name="maxAttempts">Número máximo de intentos si la ejecución falla.</param>
        /// <param name="intervalMs">Intervalo en milisegundos para comprobar si debe ejecutarse.</param>
        public RunOnceADay(Func<Task<bool>> callback, int maxAttempts = 60, double intervalMs = 120_000)
        {
            _lastDateRun = DateTime.MinValue;
            _isRunning = false;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _maxAttempts = maxAttempts;
            _attempts = 0;

            _runnerTimer = new System.Timers.Timer
            {
#if DEBUG
                Interval = 1_000, // 1 segundo para pruebas en modo DEBUG.
#else
                Interval = intervalMs, // 2 minutos (por defecto) en producción.
#endif
                AutoReset = true
            };

            _runnerTimer.Elapsed += RunnerTimerElapsedAsync;
            _runnerTimer.Start();
        }

        /// <summary>
        /// Manejador del evento Timer.Elapsed. Ejecuta la función si cumple con los criterios.
        /// </summary>
        private async void RunnerTimerElapsedAsync(object? sender, ElapsedEventArgs e)
        {
            if (_attempts > _maxAttempts || _isRunning || _lastDateRun.Date == DateTime.Now.Date)
            {
                return;
            }

            _isRunning = true;
            try
            {
                bool wasSuccessful = await _callback();
                if (wasSuccessful)
                {
                    _lastDateRun = DateTime.Now.Date;
                    _attempts = 0;
                }
                else
                {
                    _attempts++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la ejecución del callback: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// Detiene el temporizador y libera recursos.
        /// </summary>
        public void Stop()
        {
            _runnerTimer.Stop();
            _runnerTimer.Dispose();
        }
    }
}
