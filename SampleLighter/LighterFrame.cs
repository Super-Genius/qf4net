using System;
using System.Text;
using qf4net;
using System.Collections;
using System.Collections.Generic;

namespace Samples.Lighter
{
	/// <summary>
	/// Summary description for LighterFrame.
	/// </summary>
	public class LighterFrame
	{
		private GQHSMManager _manager;
		private GQHSMVariables _globals;
		private MyLogger _logger = new MyLogger();

		private Valve _Valve;
		private Air _Air;
		private Flint _Flint;
		private FuelMixture _FuelMixture;

		public LighterFrame()
		{
			Init();
		}

		public string ValveState
		{
			get
			{
				return _Valve.CurrentStateName + "/" + _Valve.FlowValue;
			}
		}

		public string FlintState
		{
			get
			{
				return _Flint.CurrentStateName;
			}
		}

		public string AirFlowState
		{
			get
			{
				return _Air.CurrentStateName + "/" + _Air.FlowRate;
			}
		}

		public string FuelMixtureState
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(_FuelMixture.CurrentStateName);
				sb.AppendFormat(" /fuel={0}", _FuelMixture.FuelFlowRate);
				sb.AppendFormat(" /air={0}", _FuelMixture.AirFlowRate);
				if (_Air.CurrentStateName.EndsWith("Gusting "))
				{
					sb.AppendFormat(" /lastgust={0}", _FuelMixture.LastGust);
				}
				return sb.ToString();
			}
		}

		public bool FrameIsLit
		{
			get
			{
				return _FuelMixture.CurrentStateName.EndsWith("Burning");
			}
		}

		void Init()
		{
			_manager = GQHSMManager.Instance;
			_globals = _manager.Globals;

			_Valve = new Valve(_logger);
			_Air = new Air(_logger);
			_FuelMixture = new FuelMixture(_logger);
			_Flint = new Flint(_logger);

			RegisterStateChange(GQHSMManager.Instance.EventManager);

			_Valve.Init();
			_Air.Init();
			_FuelMixture.Init();
			_Flint.Init();
		}


		void RegisterStateChange(IQEventManager eventManager)
		{
			eventManager.PolledEvent += new PolledEventHandler(EventManager_PolledEvent);
		}

		public void SpinFlint()
		{
			_Flint.HSM.SendPortAction("User", "Spin");
		}

		public void PressValve()
		{
			_Valve.HSM.SendPortAction("User", "Press");
		}

		public void ReleaseValve()
		{
			_Valve.HSM.SendPortAction("User", "Release");
		}

		public void IncreaseAirFlow()
		{
			_Air.HSM.AsyncDispatch(new QEvent("PressureIncrease", null));
		}

		public void DecreaseAirFlow()
		{
			_Air.HSM.AsyncDispatch(new QEvent("PressureDecrease", null));
		}

		public void IncreaseFuelFlow()
		{
			_Valve.HSM.SendPortAction("User", "IncreaseFlow");
		}

		public void DecreaseFuelFlow()
		{
			_Valve.HSM.SendPortAction("User", "DecreaseFlow");
		}

		private void EventManager_PolledEvent(IQEventManager eventManager, IQHsm hsm, IQEvent ev, PollContext pollContext)
		{
			EventHandler handler = StateChange;
			if (null != handler)
			{
				handler(hsm, new EventArgs());
			}
		}

		public EventHandler StateChange;
	}
}
