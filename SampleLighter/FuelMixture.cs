using System;
using qf4net;

namespace Samples.Lighter
{

	class FuelMixture : ClassHandler
	{
		int _FuelFlowRate;
		int _AirFlowRate;
		int _LastGust;

		public int FuelFlowRate 
		{	
			get 
			{ 
				return _FuelFlowRate; 
			} 
		}

		public int AirFlowRate
		{ 
			get
			{ 
				return _AirFlowRate; 
			} 
		}

		public int LastGust 
		{ 
			get 
			{ 
				return _LastGust; 
			} 
		}

		public FuelMixture(MyLogger logger)
			: base("FuelMixture", logger)
		{
		}

		public void MixFuel(int newFuelFlowRate)
		{
			_FuelFlowRate = newFuelFlowRate;
		}

		public void DissipateFuel(int newAirFlowRate)
		{
			_AirFlowRate = newAirFlowRate;
		}

		public void StopFlow()
		{
			_FuelFlowRate = 0;
		}

		public void TestRates()
		{
			if (_FuelFlowRate > _AirFlowRate)
			{
				HSM.AsyncDispatch(new QEvent("FuelInMixture"));
			}
			else
			{
				HSM.AsyncDispatch(new QEvent("FuelDissipated"));
			}
		}

		public void SetLastGust(int newLastGust)
		{
			_LastGust = newLastGust;
		}

		public bool FuelFlowIsStrongEnough(int newGustLevel)
		{
			SetLastGust(newGustLevel);
			int gustLevel = newGustLevel;
			return gustLevel < _FuelFlowRate;
		}
	}
}