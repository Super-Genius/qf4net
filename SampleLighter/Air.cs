
using System;
using qf4net;

namespace Samples.Lighter
{
	public class Air : ClassHandler
	{
		int _FlowRate;
		public int FlowRate { get { return _FlowRate; } }

		const int _StandardFlowMin = 1;
		const int _StandardFlowMax = 10;
		const int _GustFlowMin = 7;
		const int _GustFlowMax = 15;
		const int _GustMin = 12;
		const int _GustMax = 19;
		Random _rnd = new Random();

		public Air(MyLogger logger) 
			: base("Air", logger)
		{
		}

		public double RandomDraftInterval()
		{
			double result = 1.0 + _rnd.NextDouble();
			return result;
		}

		public void RandomlyChangeFlowRate()
		{
			int result = _rnd.Next(_StandardFlowMin, _StandardFlowMax);
			_FlowRate = result;
		}

		public void SetFlowToZero()
		{
			_FlowRate = 0;
		}

		public void RandomlyChangeGustFlowRate()
		{
			int result = _rnd.Next(_GustFlowMin, _GustFlowMax);
			_FlowRate = result;
		}

		public bool JustChangeFlowRate()
		{
			return _rnd.Next(10) > 6;
		}

		public double RandomGustInterval()
		{
			double result = 1.0 + _rnd.NextDouble();
			return result;
		}

		public int RandomGustRate
		{
			get
			{
				int result = _rnd.Next(_FlowRate, _GustMax);
				return result;
			}
		}
	}
}
