using System.Diagnostics;

namespace Utils
{
    public interface ITimeHelper
    {
        public string Timetable { get; }
        public TimeSpan TimeLeftToNextCheck()
        {
            TimeSpan ts = new TimeSpan(24, 0, 0);

            foreach (string strtime in this.Timetable.Split(','))
            {
                TimeSpan nextTs;
                if (!TimeSpan.TryParse(strtime, out nextTs))
                    continue;
                nextTs = nextTs - DateTime.Now.TimeOfDay;
                if (nextTs.Ticks < 0)
                    nextTs = nextTs.Add(new TimeSpan(24, 0, 0)); // ближайшее время задания приходится на завтра, т.к. сегодняшнее время уже прошло
                if (nextTs < ts)
                    ts = nextTs; // ищем самое ближайшее по времени задание
            }
            return ts;
        }
    }

    public class PreciseTiming
	{
		private Stopwatch _Stopwatch = new Stopwatch();
		public void Start()
		{
			_Stopwatch.Start();
		}
		public double Finish()
		{
			if (_Stopwatch != null && _Stopwatch.IsRunning)
			{
				TimeSpan _ts = _Stopwatch.Elapsed;
				return _ts.TotalMilliseconds;
			}
			return 0;
		}

		/*
		[DllImport( "Kernel32.dll" ), SuppressUnmanagedCodeSecurity()]		
		[DllImport( "Kernel32.dll" ), SuppressUnmanagedCodeSecurity()]

		private Int64 _Start;
		private static readonly float _Frequency;
		private static extern bool QueryPerformanceFrequency( ref Int64 frequency );
		private static extern bool QueryPerformanceCounter( ref Int64 performanceCount );		
		static PreciseTiming()
		{
			Int64 iFrequency = 0;
			PreciseTiming.QueryPerformanceFrequency( ref iFrequency );
			_Frequency = (float)iFrequency;
		}		
		public void Start()
		{
			_Start = 0; PreciseTiming.QueryPerformanceCounter( ref _Start );
		}
		public float Finish()
		{
			Int64 _Finish = 0; PreciseTiming.QueryPerformanceCounter( ref _Finish );
			return ( (float)( _Finish - _Start ) / _Frequency );
		}*/
	}
}
