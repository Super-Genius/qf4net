using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace qf4net
{

    public class StateMachineInfo
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public Guid Id;

        public string ImplementationVersion;

        public string ModelFileName;

        public bool HasSubMachines;

        public int StateMachineVersion;

        public string BaseStateMachine;

        public string NameSpace;

        public string UsingNameSpaces;

        public string Comment;

        public string Fields;

        public bool ReadOnly;

        public string Assembly;
	}

    public class Glyphs
    {
        [XmlElement("StateGlyph")]
        public GQHSMState[] States;

        [XmlElement("TransitionGlyph")]
        public GQHSMTransition[] Transitions;

        [XmlElement("ComponentGlyph")]
        public GQHSMComponent[] Components;

        [XmlElement("PortLinkGlyph")]
        public GQHSMPortLink[] PortLinks;

        [XmlElement("StateTransitionPortGlyph")]
        public GQHSMPort[] Ports;
    }

	[XmlRoot("StateMachine")]
	public class StateMachine
	{
        private StateMachineInfo _stateMachineInfo;

        public StateMachineInfo StateMachineInfo
        {
            get
            {
                return _stateMachineInfo;
            }

            set
            {
                _stateMachineInfo = value;
            }
        }

        public Glyphs Glyphs;

    }
		
    public class GQHSM : LQHsm
    {
        private string _name;
		private object _data = null;
		private bool _instrument = false;
		private StateMachine _hsmS;
		
		private GQHSMState[] States
		{
			get
			{
				return _hsmS.Glyphs.States;
			}
		}
		
		private GQHSMTransition[] Transitions
		{
			
			get
			{
				return _hsmS.Glyphs.Transitions;
			}
		}

        private GQHSMComponent[] Components
        {

            get
            {
                return _hsmS.Glyphs.Components;
            }
        }

        private GQHSMPortLink[] PortLinks
        {

            get
            {
                return _hsmS.Glyphs.PortLinks;
            }
        }

        private GQHSMPort[] Ports
        {

            get
            {
                return _hsmS.Glyphs.Ports;
            }
        }

        /// <summary>
        /// Action Handlers for HSMs
        /// </summary>

        public MultiMap<String, GQHSMHandler> m_ActionHandlersEntryMap = new MultiMap<String, GQHSMHandler>();
        public MultiMap<String, GQHSMHandler> m_ActionHandlersExitMap = new MultiMap<String, GQHSMHandler>();
        public MultiMap<String, GQHSMHandler> m_ActionHandlersMap = new MultiMap<String, GQHSMHandler>();

        /// transition change handlers for HSMs
        private MultiMap<String, GQHSMHandler> m_QHSMTransitionHandlers = new MultiMap<String, GQHSMHandler>();

        /// guard function delegates for HSMs
        private MultiMap<String, GQHSMHandler> m_QHSMGuardHandlers = new MultiMap<String, GQHSMHandler>();

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

        /// <summary>
        /// Single ports that mapped by name
        /// </summary>
        private Dictionary<string, GQHSMPort> _nameToPort = new Dictionary<string, GQHSMPort>();

        /// <summary>
        /// Multi Ports that are mapped my name
        /// </summary>
        private MultiMap<string, GQHSMPort> _nameToMultiPort = new MultiMap<string, GQHSMPort>();

        /// <summary>
        /// a list of ports that we sink in our transitions signals
        /// </summary>
        private List<string> Portsinks = new List<string>();

        /// <summary>
        /// the start state of the machine looked up on initialze when states are loading
        /// </summary>
        private QState _startState;
					
		public StateMachine HSMData
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

        public GQHSM(string fileName)
        {
            SetName(fileName);
        }

        ~GQHSM()
        {
            GQHSMManager.Instance.UnregisterHsm(this);

        }

        public void PreInit()
        {
            _startState = TopState;
            if (_hsmS.StateMachineInfo.Name != null)
            {
                _name = _hsmS.StateMachineInfo.Name;
            }

            GQHSMManager.Instance.RegisterHsm(this);

            if (States != null)
            {
                foreach (GQHSMState gs in States)
                {
                    gs.PreInit(this);
                    if (gs.IsStartState)
                    {
                        _startState = gs.GetStateHandler();
                    }
                }
            }

            if (Transitions != null)
            {
                foreach (GQHSMTransition gt in Transitions)
                {
                    gt.PreInit(this);
                }
            }

            if (Components != null)
            {
                foreach (GQHSMComponent gc in Components)
                {
                    gc.PreInit(this);
                }
            }

            if (PortLinks != null)
            {
                foreach (GQHSMPortLink gpl in PortLinks)
                {
                    gpl.PreInit(this);
                }
            }

            if (Ports != null)
            {
                foreach (GQHSMPort gp in Ports)
                {
                    gp.PreInit(this);
                }
            }

        }

        public override void Init()
        {

            // let states initialize now.
            if (States != null)
            {
                foreach (GQHSMState gs in States)
                {
                    gs.Init(null);
                }
            }

            if (Transitions != null)
            {
                foreach (GQHSMTransition gt in Transitions)
                {
                    gt.Init(GetOpenSlot());
                }
            }

            if (Components != null)
            {
                foreach (GQHSMComponent gc in Components)
                {
                    gc.Init();
                }
            }

            if (PortLinks != null)
            {
                foreach (GQHSMPortLink gpl in PortLinks)
                {
                    gpl.Init();
                }
            }

            if (Ports != null)
            {
                foreach (GQHSMPort gp in Ports)
                {
                    gp.Init();
                }
            }

            // init state machine after all objects are inited.
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
            string tName = transition.GetFullName();

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

                CallTransitionHandler(fullTransitionName, data);
				
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
                if (CallGuardHandler(gTransition.GuardCondition, data))
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
                    Single timeOut = 0.0f;
                    if (!Single.TryParse(timeOutExpression, out timeOut))
                    {
                        object actionRet = CallActionHandler(timeOutExpression, "", null);
                        if (actionRet != null)
                        {
                            timeOut = (float)actionRet;
                        }
                    }
                    RegisterTimeOutExpression(transition.GetSourceStateID(),
                        new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), TimeSpan.FromSeconds(timeOut), new QEvent(eventSignal)));
				} 
				else 
				{
					string[] strList = timeOutExpression.Split (' ');
					string timeOutValue = strList [strList.Length - 1].Trim ();
                    TimeOutType flag = TimeOutType.Single;
					if (timeOutExpression.StartsWith ("every"))
					{
                        flag = TimeOutType.Repeat;
					}

					if (timeOutExpression.StartsWith ("at"))
					{
                        flag = TimeOutType.Single;
                        DateTime dt;

                        dt = DateTime.Parse(timeOutValue);
                        RegisterTimeOutExpression(transition.GetSourceStateID(),
                            new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), dt, new QEvent(eventSignal)));

					} 
					else 
					{
                        Single timeOut = 0.0f;
                        if (!Single.TryParse(timeOutValue, out timeOut))
                        {
                            object actionRet = CallActionHandler(timeOutValue, "", null);
                            if (actionRet != null)
                            {
                                timeOut = (float)actionRet;
                            }
                        }

                        RegisterTimeOutExpression(transition.GetSourceStateID(),
                            new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), TimeSpan.FromSeconds(timeOut), new QEvent(eventSignal), flag));
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
        public void RegisterActionHandler(GQHSMHandler scHandler, string sActionName, string sSignalType)
        {
            switch (sSignalType)
            {
                case QSignals.Entry:
                    m_ActionHandlersEntryMap.Add(sActionName, scHandler);
                    break;
                case QSignals.Exit:
                    m_ActionHandlersExitMap.Add(sActionName, scHandler);
                    break;
                default:
                    m_ActionHandlersMap.Add(sActionName, scHandler);
                    break;
            }
        }

        public object CallActionHandler(string sActionName, string sSignalType, object data)
        {
            List<GQHSMHandler> actionList = null;
            List<object> retObjects = new List<object>();

            string[] sActions = sActionName.Split(new string[] {"()"}, StringSplitOptions.RemoveEmptyEntries);

            foreach (string sNewAction in sActions)
            {

                if (sNewAction.StartsWith("^"))
                {
                    CallPortForward(sNewAction, sSignalType, data);
                }
                else
                {
                    switch (sSignalType)
                    {
                        case QSignals.Entry:
                            actionList = m_ActionHandlersEntryMap[sNewAction];
                            break;
                        case QSignals.Exit:
                            actionList = m_ActionHandlersExitMap[sNewAction];
                            break;
                    }

                    if (actionList != null)
                    {
                        foreach (GQHSMHandler scHandler in actionList)
                        {
                            retObjects.Add(scHandler.Invoke(data));
                        }

                    }
                }
            } // foreach (string sNewAction in sActions)

            if (retObjects.Count == 0)
                return null;
            if (retObjects.Count == 1)
                return retObjects[0];

            return retObjects.ToArray();
        }

        // Allow other classes to register transitions Handlers for StateMachine Transition events
        public void RegisterTransitionHandler(GQHSMHandler tHandler, string sTransitionName)
        {
            m_QHSMTransitionHandlers.Add(sTransitionName, tHandler);
        }

        public void CallTransitionHandler(string sTransitionName, object data)
        {
            List<GQHSMHandler> transitionList = null;

            transitionList = m_QHSMTransitionHandlers[sTransitionName];
            foreach (GQHSMHandler tHandler in transitionList)
            {
                tHandler.Invoke( data );
            }

        }

        // Allow other classes to register guard Handlers for StateMachine guard checking
        public void RegisterGuardHandler(GQHSMHandler gHandler, string sGuardName)
        {
            m_QHSMGuardHandlers.Add(sGuardName, gHandler);
        }

        public bool CallGuardHandler(string sGuardCondition, object data)
        {
            List<GQHSMHandler> guardList = null;

            guardList = m_QHSMGuardHandlers[sGuardCondition];
            foreach (GQHSMHandler gHandler in guardList)
            {
                bool validated = (bool)gHandler.Invoke(data);
                if (validated)
                    return true;
            }

            return false;
        }

        public void RegisterPort(GQHSMPort port)
        {
            if (port.IsMultiPort)
            {
                _nameToMultiPort.Add(port.Name, port);
            }
            else
            {
                if (!_nameToPort.ContainsKey(port.Name))
                {
                    _nameToPort.Add(port.Name, port);
                }
                else
                {
                    Logger.Error("Duplicate name in single ports {0}\n", port.Name);
                }
            } 
        }

        public void CallPortForward(string sActionName, string sSignalType, object data)
        {
            string[] splitS = sActionName.Split(new char[] { '^', '.'}, StringSplitOptions.RemoveEmptyEntries);
            if (splitS.Length <= 1)
            {
                Logger.Error("Need Source port and Action, i.e. ^Sourceport.Action");
                return;
            }

            // use port links to get destination port, if available
            GQHSMPort gp = GetPort(splitS[0]);
            if (gp != null)
            {
                gp.Port.Send(new QEvent(splitS[1], data));
            }
        }

        public GQHSMPort GetPort(string portName)
        {
            if (_nameToPort.ContainsKey(portName))
            {
                return _nameToPort[portName];
            }

            return null;
        }

        /// <summary>
        /// Send an action to a list of ports for this state machine.
        /// </summary>
        /// <param name="actionName">the string for the action, i.e. PortName.ActionName</param>
        /// <param name="data">Generic data </param>
        public void SendPortAction(string sourcePortName, string actionName, object data)
        {
            GQHSMPort destPort = GetPort(sourcePortName);
            if (destPort != null)
            {
                QEvent ev = new QEvent(actionName, data);
                destPort.Port.Receive(destPort.Port, ev);
            }
        }
    }

}
