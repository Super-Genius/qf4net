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

        public GQHSMTimeOut(string name, TimeSpan duration, IQEvent ev)
        {
            _name = name;
            _duration = duration;
            _event = ev;
            _callType = GQHSMCallType.TIMEOUT_TIMESPAN;

        }

        public GQHSMTimeOut(string name, TimeSpan duration, IQEvent ev, TimeOutType type)
        {
            _name = name;
            _duration = duration;
            _event = ev;
            _type = type;
            _callType = GQHSMCallType.TIMEOUT_TIMESPAN_TYPE;

        }

        public GQHSMTimeOut(string name, DateTime dt, IQEvent ev)
        {
            _name = name;
            _dt = dt;
            _event = ev;
            _callType = GQHSMCallType.TIMEOUT_DATETIME;

        }

        public GQHSMTimeOut(string name, DateTime dt, IQEvent ev, TimeOutType type)
        {
            _name = name;
            _dt = dt;
            _event = ev;
            _callType = GQHSMCallType.TIMEOUT_DATETIME_TYPE;

        }

        public void SetTimeOut(LQHsm sm)
        {
            switch (_callType)
            {
                case GQHSMCallType.TIMEOUT_TIMESPAN:
                    sm.SetTimeOut(_name, _duration, _event);
                    break;
                case GQHSMCallType.TIMEOUT_TIMESPAN_TYPE:
                    sm.SetTimeOut(_name, _duration, _event, _type);
                    break;
                case GQHSMCallType.TIMEOUT_DATETIME:
                    sm.SetTimeOut(_name, _dt, _event);
                    break;
                case GQHSMCallType.TIMEOUT_DATETIME_TYPE:
                    sm.SetTimeOut(_name, _dt, _event, _type);
                    break;
            }
        }

        public void ClearTimeOut(LQHsm sm)
        {
            sm.ClearTimeOut(_name);
        }
    }
}
