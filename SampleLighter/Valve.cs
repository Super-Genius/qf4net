using System;
using qf4net;

namespace Samples.Lighter
{
	public class Valve : ClassHandler
	{
		int _FlowRate = 10;
		int _MaxFlowRate = 20;

		public Valve(MyLogger logger)
			: base("Valve", logger)
		{
		}

		public int FlowValue 
		{ 
			get 
			{ 
				return _FlowRate; 
			} 
		}

		public void ChangeFlowRate(int delta)
		{
			int newRate = _FlowRate + delta;
			if (newRate >= 0 && newRate <= _MaxFlowRate)
			{
				_FlowRate = newRate;
			}
		}
	}
}
