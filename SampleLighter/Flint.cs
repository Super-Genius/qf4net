using System;
using qf4net;

namespace Samples.Lighter
{
	public class Flint : ClassHandler
	{
		private Random _rnd = new Random();

		public Flint(MyLogger logger)
			: base("Flint", logger)
		{
		}

		public double SparkFrequencyInterval()
		{
			return 0.2;
		}

		public double RandomSpinInterval()
		{
			double result = 0.1 + _rnd.Next(5) * 0.1;
			return result;
		}
	} // Flint
}
