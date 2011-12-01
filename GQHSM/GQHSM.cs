using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using qf4net;

namespace qf4net
{

    public class GQHSM
    {
        [XmlElement("StateGlyph")]
        public GQHSMState[] m_States;

        [XmlElement("TransitionGlyph")]
        public GQHSMTransition[] m_Transitions;
    }

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

    public class GLQHSMDum : LQHsm
    {
        public QState startState;

        public void InitState(QState newState)
        {
            InitializeState(newState);
        }

        public QState GetTopState()
        {
            return TopState;
        }

        protected override void InitializeStateMachine()
        {
            InitState(startState);
        }

        /// <summary>
        /// Performs a dynamic transition; i.e., the transition path is determined on the fly and not recorded.
        /// </summary>
        /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
        public void DoTransitionTo(QState targetState, int slot)
        {
            TransitionTo(targetState, slot);
        }

        public int GetOpenSlot()
        {
            return s_TransitionChainStore.GetOpenSlot();
        }

        public QState GetCurrentState()
        {
            return CurrentState;
        }


    }

    [XmlRoot("Glyphs")]
    public class GLQHSM : GQHSM
    {
        private GLQHSMDum _lQHsm = new GLQHSMDum();
        private string _name;
		private object _data = null;

        /// <summary>
        /// Maps a GUID to a GHQMState
        /// </summary>
        private Dictionary<Guid, GQHSMState> _guidToStateMap = new Dictionary<Guid, GQHSMState>();
        /// <summary>
        /// Maps a string (Name) to GHQMState class
        /// </summary>
        private Dictionary<string, GQHSMState> _nameToStateMap = new Dictionary<string, GQHSMState>();

        /// <summary>
        /// Names to Unguarded Transition map
        /// </summary>
        private Dictionary<string, GQHSMTransition> _nameToTransitionMap = new Dictionary<string, GQHSMTransition>();

        /// <summary>
        /// Names to Guarded Transition map
        /// </summary>
        private MultiMap<string, GQHSMTransition> _nameToGuardedTransitionMultiMap = new MultiMap<string, GQHSMTransition>();

        /// <summary>
        /// Names to Timeout class map
        /// </summary>
        private Dictionary<Guid, GQHSMTimeOut> _nameToTimeOutMap = new Dictionary<Guid, GQHSMTimeOut>();

        public void Init()
        {		
            GQHSMManager.Instance.RegisterHsm(_lQHsm);
			
			//GQHSMManager.Instance.RegisterStateCommandHandler(theHSM.SetName(fileName);

            if (m_States != null)
            {
                foreach (GQHSMState gs in m_States)
                {
                    gs.Init(this, null);
                    if (gs.IsStartState)
                    {
                        _lQHsm.startState = gs.GetStateHandler();
                    }
                }
            }

            if (m_Transitions != null)
            {
                foreach (GQHSMTransition gt in m_Transitions)
                {
                    gt.Init(this, _lQHsm.GetOpenSlot());
                }
            }

            _lQHsm.Init();

        }

        public string AddStateNameMap(GQHSMState state)
        {
            GQHSMState curState = state;
            string fullName = state.Name;
            while ((curState = curState.GetParent()) != null)
            {
                fullName = curState.Name + "." + fullName;
            }

            _nameToStateMap.Add(fullName, state);

            return fullName;
        }

        public void AddTransitionNameMap(GQHSMTransition transition)
        {
            string tName = transition.EventSignal;
            if (tName.Length == 0)
            {
                tName = transition.Name;
            }

            // find state this transition is associated with
            GQHSMState state = GetState(transition.GetSourceStateID());
            if (state != null)
            {
                tName = state.GetFullName() + "." + tName;
            }

            if (transition.GuardCondition.Length == 0)
            {
                if (!_nameToTransitionMap.ContainsKey(tName))
                {
                    _nameToTransitionMap.Add(tName, transition);
                }
                else
                {
                    _lQHsm.Logger.Error("Duplicate name in transitions {0}\n", tName);
                }
            }
            else
            {
               _nameToGuardedTransitionMultiMap.Add(tName, transition);
            }
        }

        public bool DoStateTransition(string signalName, object data)
        {
            GQHSMTransition transition;

            transition = GetGuardedTransition(signalName, data);

            if (transition != null)
            {

                GQHSMState toState = GetState(transition.GetDestinationStateID());
                GQHSMState fromState = GetState(transition.GetSourceStateID());
                QState stateHandler = _lQHsm.GetTopState();
                if (toState != null)
                {
                    stateHandler = toState.GetStateHandler();
                }

                if (!transition.DoNotInstrument)
                    _lQHsm.LogStateEvent(StateLogType.EventTransition, _lQHsm.GetCurrentState(), stateHandler, transition.Name, signalName);

                string fullTransitionName = fromState.GetFullName() + "." + transition.GetFullName();
                GQHSMManager.Instance.CallTransitionHandler(_name, fullTransitionName, data);
				
				_data = data;

                _lQHsm.DoTransitionTo(stateHandler, transition.GetSlot());
				
				_data = null;

                return true;
            }

            return false;
        }
		
		public object GetData()
		{
			return _data;
		}
		
        public void SignalTransition(string transitionName, object data)
        {
            _lQHsm.AsyncDispatch(new QEvent(transitionName, data));

        }

        public void InitState(QState newState)
        {
            _lQHsm.InitState(newState);
        }

        public GQHSMTransition GetGuardedTransition(string signalName, object data)
        {
            List<GQHSMTransition> guardTansitions;
            GQHSMTransition foundTransition = null;


            guardTansitions = _nameToGuardedTransitionMultiMap[signalName];

            foreach (GQHSMTransition gTransition in guardTansitions)
            {
                if (GQHSMManager.Instance.CallGuardHandler(_name, gTransition.GuardCondition, data))
                {
                    return gTransition;
                }
            }

            // retrieve unguarded transition if any
            _nameToTransitionMap.TryGetValue(signalName, out foundTransition);

            return foundTransition;
        }

        public GQHSMTransition GetTransition(string transistionName)
        {
            GQHSMTransition foundTransition = null;
            _nameToTransitionMap.TryGetValue(transistionName, out foundTransition);
            return foundTransition;
        }

        public GQHSMState GetState(Guid Id)
        {
            GQHSMState foundState = null;
            // see if it is in the list and if so return it's QState delegate
            _guidToStateMap.TryGetValue(Id, out foundState);

            return foundState;


        }
        /// <summary>
        /// Get a QState delegate for a particular state
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public QState GetStateHandler(Guid Id)
        {
            QState returnState = _lQHsm.GetTopState();
            GQHSMState foundState;

            foundState = GetState(Id);
            if (foundState != null)
            {
                returnState = foundState.GetStateHandler();
            }

            return returnState;
        }

        public string RegisterState(GQHSMState qState)
        {
            // see if it is in the list and if so return it's QState delegate
            if (_guidToStateMap.ContainsKey(qState.Id))
            {
                _lQHsm.Logger.Error("Can't have duplicate Guids for states : {0}", qState.Id);
                return null;
            }

            _guidToStateMap.Add(qState.Id, qState);
            return AddStateNameMap(qState);

        }

        public void RegisterTransition(GQHSMTransition transition)
        {
            AddTransitionNameMap(transition);
        }

        public void RegisterTimeOutExpression(Guid stateGuid, GQHSMTimeOut to)
        {
            if (!_nameToTimeOutMap.ContainsKey(stateGuid))
            {
                _nameToTimeOutMap.Add(stateGuid, to);
            }
            else
            {
                _lQHsm.Logger.Error("Duplicate Guid in TimeOut {0}\n", stateGuid);
            }

        }

        public void SetTimeOut(Guid stateGuid)
        {
            GQHSMTimeOut foundTO = null;

            // see if it is in the list and if so return it's QState delegate
            _nameToTimeOutMap.TryGetValue(stateGuid, out foundTO);

            if (foundTO != null)
            {
                foundTO.SetTimeOut(_lQHsm);
            }
        }

        public void ClearTimeOut(Guid stateGuid)
        {
            GQHSMTimeOut foundTO = null;

            // see if it is in the list and if so return it's QState delegate
            _nameToTimeOutMap.TryGetValue(stateGuid, out foundTO);

            if (foundTO != null)
            {
                foundTO.ClearTimeOut(_lQHsm);
            }
        }

        /// <summary>
        /// Parse Timeout Expressions
        ///     single float (i.e. 1.0, 0.1, etc converts to TimeSpan.FromSeconds(value)
        ///     every float (i.e. "every 1.0") repeats timeout every TimeSpan.FromSeconds(value)
        ///     at DateTime (i.e. "at Sat, 01 Nov 2008 19:35:00 GMT") does single at DateTime.Parse(value)
        /// </summary>
        /// <param name="transition"></param>
        /// <param name="expression"></param>
        public void RegisterTimeOutExpression(GQHSMTransition transition, string expression)
        {
                string timeOutExpression = expression.Trim ();
                string eventSignal = transition.EventSignal;
                if (transition.EventSignal.Length > 0)
                {
                    eventSignal = transition.EventSignal;
                }
                else
                {
                    eventSignal = transition.Name;
                }

				if (timeOutExpression.IndexOf (" ") == -1)
				{
                    RegisterTimeOutExpression(transition.GetSourceStateID(),
                        new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), TimeSpan.FromSeconds(Convert.ToSingle(timeOutExpression)), new QEvent(eventSignal)));
				} 
				else 
				{
					string[] strList = timeOutExpression.Split (' ');
					string timeOut = strList [strList.Length - 1].Trim ();
                    TimeOutType flag = TimeOutType.Single;
					if (timeOutExpression.StartsWith ("every"))
					{
                        flag = TimeOutType.Repeat;
					}

					if (timeOutExpression.StartsWith ("at"))
					{
                        flag = TimeOutType.Single;
                        DateTime dt;

                        dt = DateTime.Parse(timeOut);
                        RegisterTimeOutExpression(transition.GetSourceStateID(),
                            new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), dt, new QEvent(eventSignal)));

					} 
					else 
					{
                        RegisterTimeOutExpression(transition.GetSourceStateID(),
                            new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), TimeSpan.FromSeconds(Convert.ToSingle(timeOut)), new QEvent(eventSignal), flag));
					}
				}
        }
        public void SetName(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }

        public LQHsm GetHSM()
        {
            return _lQHsm;
        }

        ~GLQHSM()
        {
            GQHSMManager.Instance.UnregisterHsm(_lQHsm);

        }
    }

}
