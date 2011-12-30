using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace qf4net
{
	[XmlRootAttribute(ElementName = "Glyphs", Namespace = "", DataType = "")]
	public class GHQSMSerializable
	{
        [XmlElement("StateGlyph")]
        public GQHSMState[] m_States;

        [XmlElement("TransitionGlyph")]
        public GQHSMTransition[] m_Transitions;
	}
		
    public class GQHSM : LQHsm
    {
        private string _name;
		private object _data = null;
		private bool _instrument = false;
		private GHQSMSerializable _hsmS;
		
		private GQHSMState[] m_States
		{
			get
			{
				return _hsmS.m_States;
			}
		}
		
		private GQHSMTransition[] m_Transitions
		{
			
			get
			{
				return _hsmS.m_Transitions;
			}
		}
		
        /// <summary>
        /// Action Handlers for HSMs
        /// </summary>
 
        private class ActionHandlers
        {
            public MultiMap<String, GQHSMHandler> ActionEntryMap = new MultiMap<String, GQHSMHandler>();
            public MultiMap<String, GQHSMHandler> ActionExitMap = new MultiMap<String, GQHSMHandler>();
            public MultiMap<String, GQHSMHandler> ActionMap = new MultiMap<String, GQHSMHandler>();
        }
        private Dictionary<string, ActionHandlers> m_QHSMActionHandlers = new Dictionary<string, ActionHandlers>();

        /// transition change handlers for HSMs
        //public MultiMap<String, GQHSMHandler> TransistionMap = new MultiMap<String, GQHSMHandler>();
        private Dictionary<string, MultiMap<String, GQHSMHandler>> m_QHSMTransitionHandlers = new Dictionary<string, MultiMap<String, GQHSMHandler>>();

        /// guard function delegates for HSMs
        //public MultiMap<String, GQHSMHandler> GuardMap = new MultiMap<String, GQHSMHandler>();
        private Dictionary<string, MultiMap<String, GQHSMHandler>> m_QHSMGuardHandlers = new Dictionary<string, MultiMap<String, GQHSMHandler>>();

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

        private QState _startState;
					
		public GHQSMSerializable HSMData
		{
			get	
			{
				return _hsmS;
			}
			
			set
			{
				_hsmS = value;
			}
		}
		
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
            InitState(_startState);
        }

        /// <summary>
        /// Performs a dynamic transition; i.e., the transition path is determined on the fly and not recorded.
        /// </summary>
        /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
        public void DoTransitionTo(QState targetState, int slot)
        {
            TransitionTo(targetState, slot);
        }
		
		public void DoTransitionTo(QState targetState)
		{
			DoTransitionTo(targetState, GetOpenSlot());
		}
		
		public bool DoTransitionTo(string targetStateName)
		{
			GQHSMState targetState;
			
			if (_nameToStateMap.TryGetValue(targetStateName, out targetState))
			{
				DoTransitionTo(targetState.GetStateHandler(), GetOpenSlot());
			}
			else
			{
				return false;
			}
			
			return true;	
		}

		public bool DoTransitionAsync(string targetStateName)
		{
			GQHSMState targetState;
			
			if (_nameToStateMap.TryGetValue(targetStateName, out targetState))
			{
				AsyncDispatch(new QEvent("Internal.TransitionTo", targetState));
			}
			else
			{
				return false;
			}
			
			return true;	
			
		}
		
        public int GetOpenSlot()
        {
            return s_TransitionChainStore.GetOpenSlot();
        }

        public QState GetCurrentState()
        {
            return CurrentState;
        }
		
        ~GQHSM()
        {
            GQHSMManager.Instance.UnregisterHsm(this);

        }
		
        public override void Init()
        {		
            GQHSMManager.Instance.RegisterHsm(this);
			
			//GQHSMManager.Instance.RegisterStateCommandHandler(theHSM.SetName(fileName);

            if (m_States != null)
            {
                foreach (GQHSMState gs in m_States)
                {
                    gs.Init(this, null);
                    if (gs.IsStartState)
                    {
                        _startState = gs.GetStateHandler();
                    }
                }
            }

            if (m_Transitions != null)
            {
                foreach (GQHSMTransition gt in m_Transitions)
                {
                    gt.Init(this, GetOpenSlot());
                }
            }

            base.Init();

        }

		public bool Instrument
		{
			get
			{
				return _instrument;
			}
			
			set
			{
				_instrument = value;
			}
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
                    Logger.Error("Duplicate name in transitions {0}\n", tName);
                }
            }
            else
            {
               _nameToGuardedTransitionMultiMap.Add(tName, transition);
            }
        }
		
		/// <summary>
		/// Do a internal transition to another state using signalName
		/// </summary>
		/// <returns>
		/// true if handled
		/// </returns>
		/// <param name='signalName'>
		/// The name of the signal that will transition to another state
		/// </param>
		/// <param name='data'>
		/// any object that is passed between states
		/// </param>
		public bool StateTransitionInternal(string stateName, string signalName, object data)
        {
            GQHSMTransition transition = null;
			string fullTransitionName = stateName + "." + signalName;
            // if we know about the transition
            bool retVal = false;

            retVal = GetGuardedTransition(fullTransitionName, data, ref transition);

            if (transition != null)
            {

                GQHSMState toState = GetState(transition.GetDestinationStateID());
                GQHSMState fromState = GetState(transition.GetSourceStateID());
                QState stateHandler = GetTopState();
                if (toState != null)
                {
                    stateHandler = toState.GetStateHandler();
                }

                if (_instrument && !transition.DoNotInstrument)
                    LogStateEvent(StateLogType.EventTransition, GetCurrentState(), stateHandler, transition.Name, signalName);

                CallTransitionHandler(_name, fullTransitionName, data);
				
				_data = data;

                if (!transition.IsInnerTransition)
                {
                    DoTransitionTo(stateHandler, transition.GetSlot());
                }
				
				_data = null;

                retVal = true;
            }
			else if (signalName == "Internal.TransitionTo")
			{
				GQHSMState state = (GQHSMState)data;
   	        	DoTransitionTo(state.GetStateHandler());
                retVal = true;
			}

            return retVal;
        }
		
		/// <summary>
		/// Gets the current data object that is passing between states/transitions
		/// </summary>
		/// <returns>
		/// The generic data object that is being passed
		/// </returns>
		public object GetData()
		{
			return _data;
		}
		
        public void SignalTransition(string transitionName, object data)
        {
            AsyncDispatch(new QEvent(transitionName, data));

        }

        public bool GetGuardedTransition(string signalName, object data, ref GQHSMTransition transition)
        {
            List<GQHSMTransition> guardTansitions;
            GQHSMTransition foundTransition = null;
            bool retValue = false;


            guardTansitions = _nameToGuardedTransitionMultiMap[signalName];

            foreach (GQHSMTransition gTransition in guardTansitions)
            {
                // we had a guard condition and handled the transition but guard may have failed
                retValue = true;
                if (CallGuardHandler(_name, gTransition.GuardCondition, data))
                {
                    transition = gTransition;
                    return true;
                }
            }

            // retrieve unguarded transition if any
            _nameToTransitionMap.TryGetValue(signalName, out foundTransition);
            transition = foundTransition;

            return retValue;
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
            QState returnState = GetTopState();
            GQHSMState foundState;

			if (Id != Guid.Empty)
			{
            	foundState = GetState(Id);
            	if (foundState != null)
            	{
                	returnState = foundState.GetStateHandler();
            	}
			}

            return returnState;
        }

        public string RegisterState(GQHSMState qState)
        {
            // see if it is in the list and if so return it's QState delegate
            if (_guidToStateMap.ContainsKey(qState.Id))
            {
                Logger.Error("Can't have duplicate Guids for states : {0}", qState.Id);
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
                Logger.Error("Duplicate Guid in TimeOut {0}\n", stateGuid);
            }

        }

        public void SetTimeOut(Guid stateGuid)
        {
            GQHSMTimeOut foundTO = null;

            // see if it is in the list and if so return it's QState delegate
            _nameToTimeOutMap.TryGetValue(stateGuid, out foundTO);

            if (foundTO != null)
            {
                foundTO.SetTimeOut(this);
            }
        }

        public void ClearTimeOut(Guid stateGuid)
        {
            GQHSMTimeOut foundTO = null;

            // see if it is in the list and if so return it's QState delegate
            _nameToTimeOutMap.TryGetValue(stateGuid, out foundTO);

            if (foundTO != null)
            {
                foundTO.ClearTimeOut(this);
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

        // Allow other classes to register Action Handlers for StateMachine Actions OnEntry/OnExit
        public void RegisterActionHandler(GQHSMHandler scHandler, string sMachineName, string sActionName, string sSignalType)
        {
            ActionHandlers actionHandlers;

            if (!m_QHSMActionHandlers.TryGetValue(sMachineName, out actionHandlers))
            {
                actionHandlers = new ActionHandlers();
                m_QHSMActionHandlers.Add(sMachineName, actionHandlers);
            }

            switch (sSignalType)
            {
                case QSignals.Entry:
                    actionHandlers.ActionEntryMap.Add(sActionName, scHandler);
                    break;
                case QSignals.Exit:
                    actionHandlers.ActionExitMap.Add(sActionName, scHandler);
                    break;
                default:
                    actionHandlers.ActionMap.Add(sActionName, scHandler);
                    break;

            }
        }

        public void CallActionHandler(string sMachineName, string sActionName, string sSignalType, object data)
        {
            List<GQHSMHandler> actionList = null;
            ActionHandlers actionHandlers;

            if (m_QHSMActionHandlers.TryGetValue(sMachineName, out actionHandlers))
            {
                switch (sSignalType)
                {
                    case QSignals.Entry:
                        actionList = actionHandlers.ActionEntryMap[sActionName];
                        break;
                    case QSignals.Exit:
                        actionList = actionHandlers.ActionExitMap[sActionName];
                        break;
                }

                if (actionList != null)
                {
                    foreach (GQHSMHandler scHandler in actionList)
                    {
                       scHandler.Invoke(data);
                    }

                }

            }
        }

        // Allow other classes to register transitions Handlers for StateMachine Transition events
        public void RegisterTransitionHandler(GQHSMHandler tHandler, string sMachineName, string sTransitionName)
        {
            MultiMap<String, GQHSMHandler> transitionHandlers;

            if (!m_QHSMTransitionHandlers.TryGetValue(sMachineName, out transitionHandlers))
            {
                transitionHandlers = new MultiMap<String, GQHSMHandler>();
                m_QHSMTransitionHandlers.Add(sMachineName, transitionHandlers);
            }

            transitionHandlers.Add(sTransitionName, tHandler);
        }

        public void CallTransitionHandler(string sMachineName, string sTransitionName, object data)
        {
            List<GQHSMHandler> transitionList = null;
            MultiMap<String, GQHSMHandler> transitionHandlers;

            if (m_QHSMTransitionHandlers.TryGetValue(sMachineName, out transitionHandlers))
            {
                transitionList = transitionHandlers[sTransitionName];
                foreach (GQHSMHandler tHandler in transitionList)
                {
                    tHandler.Invoke( data );
                }

            }
        }

        // Allow other classes to register guard Handlers for StateMachine guard checking
        public void RegisterGuardHandler(GQHSMHandler gHandler, string sMachineName, string sGuardName)
        {
            MultiMap<String, GQHSMHandler> guardHandlers;

            if (!m_QHSMGuardHandlers.TryGetValue(sMachineName, out guardHandlers))
            {
                guardHandlers = new MultiMap<String, GQHSMHandler>();
                m_QHSMGuardHandlers.Add(sMachineName, guardHandlers);
            }

            guardHandlers.Add(sGuardName, gHandler);
        }

        public bool CallGuardHandler(string sMachineName, string sGuardCondition, object data)
        {
            List<GQHSMHandler> guardList = null;
            MultiMap<String, GQHSMHandler> GQHSMHandlers;

            if (m_QHSMGuardHandlers.TryGetValue(sMachineName, out GQHSMHandlers))
            {
                guardList = GQHSMHandlers[sGuardCondition];
                foreach (GQHSMHandler gHandler in guardList)
                {
                    bool validated = (bool)gHandler.Invoke(data);
                    if (validated)
                        return true;
                }

            }

            return false;
        }


    }
}
