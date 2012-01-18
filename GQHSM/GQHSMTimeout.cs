using System;

namespace qf4net
{
    public class GQHSMTimeOut
    {
        private string _name;
        private TimeSpan _duration;
        private DateTime _dt;
        private IQEvent _event;
        private TimeOutType _type;
        private GQHSMAction _action = null;

        private enum GQHSMCallType
        {
            TIMEOUT_NONE,
            TIMEOUT_TIMESPAN,
            TIMEOUT_TIMESPAN_TYPE,
            TIMEOUT_DATETIME,
            TIMEOUT_DATETIME_TYPE
        }
        private GQHSMCallType _callType = GQHSMCallType.TIMEOUT_NONE;

        private GQHSMTimeOut() {}

        public TimeSpan Duration
        {
			get
			{
				if (_action != null)
				{
					object retObject = _action.InvokeActionHandler();
					if (retObject is Double)
					{
						_duration = TimeSpan.FromSeconds((Double)retObject);
					}
					else
					{
						throw new InvalidCastException(
							string.Format("Timeout Expression public double {0}({1}); not found!", _action.Name, _action.Params.GetParametersString()));
					}
				}

				return _duration;
			}

        }

        public DateTime At
        {
			get
			{
				if (_action != null)
				{
					object retObject = _action.InvokeActionHandler();
					if (retObject is DateTime)
					{
						_dt = (DateTime)retObject;
					}
					else
					{
						throw new InvalidCastException(string.Format("Timeout Expression public DateTime {0}({1}); not found!", _action.Name, _action.Params.GetParametersString()));
					}
				}

				return _dt;
			}
		}

        public GQHSMTimeOut(string name, TimeSpan duration, IQEvent ev, GQHSMAction action)
        {
            _name = name;
            _duration = duration;
            _event = ev;
			_action = action;
            _callType = GQHSMCallType.TIMEOUT_TIMESPAN;

        }

        public GQHSMTimeOut(string name, TimeSpan duration, IQEvent ev, TimeOutType type, GQHSMAction action)
        {
            _name = name;
            _duration = duration;
            _event = ev;
            _type = type;
			_action = action;
            _callType = GQHSMCallType.TIMEOUT_TIMESPAN_TYPE;

        }

        public GQHSMTimeOut(string name, DateTime dt, IQEvent ev, GQHSMAction action)
        {
            _name = name;
            _dt = dt;
            _event = ev;
			_action = action;
            _callType = GQHSMCallType.TIMEOUT_DATETIME;

        }

        public GQHSMTimeOut(string name, DateTime dt, IQEvent ev, TimeOutType type, GQHSMAction action)
        {
            _name = name;
            _dt = dt;
            _event = ev;
			_action = action;
            _callType = GQHSMCallType.TIMEOUT_DATETIME_TYPE;
        }

        public void SetTimeOut(LQHsm sm)
        {
            switch (_callType)
            {
                case GQHSMCallType.TIMEOUT_TIMESPAN:
                    sm.SetTimeOut(_name, Duration, _event);
                    break;
                case GQHSMCallType.TIMEOUT_TIMESPAN_TYPE:
                    sm.SetTimeOut(_name, Duration, _event, _type);
                    break;
                case GQHSMCallType.TIMEOUT_DATETIME:
                    sm.SetTimeOut(_name, At, _event);
                    break;
                case GQHSMCallType.TIMEOUT_DATETIME_TYPE:
                    sm.SetTimeOut(_name, At, _event, _type);
                    break;
            }
        }

        public void ClearTimeOut(LQHsm sm)
        {
            sm.ClearTimeOut(_name);
        }
    }
}
