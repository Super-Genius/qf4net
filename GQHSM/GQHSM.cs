using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace qf4net
{
    public class GQHSM : LQHsm
    {
        private string _name;
		private bool _instrument = false;
        // the class that can handle action commands
        private object _handlerClass = null;
		private GQHSMStateMachine _hsmS;
        private GQHSMVariables _globals = GQHSMManager.Instance.Globals;

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

        private MultiMap<String, MethodInfo> m_classHanderMethods = new MultiMap<String, MethodInfo>();
        /// <summary>
        /// Action Handlers for HSMs
        /// </summary>
        private MultiMap<String, GQHSMHandler> m_ActionHandlersMap = new MultiMap<String, GQHSMHandler>();

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
        /// Names to Timeouts class map
        /// </summary>
		private MultiMap<Guid, GQHSMTimeOut> _nameToTimeOutMap = new MultiMap<Guid, GQHSMTimeOut>();

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
					
		public GQHSMStateMachine HSMData
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

        public object HandlerClass
        {
            get
            {
                return _handlerClass;
            }
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
			string Name = Path.GetFileNameWithoutExtension(fileName);
            SetName(Name);
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

        public void Init(object handlerClass)
        {

            _handlerClass = handlerClass;
            GatherHandlerMethodsAndVariables();

            Init();

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
		public bool StateTransitionInternal(string stateName, GQHSMParameters Params, string signalName)
        {
            GQHSMTransition transition = null;
			string fullTransitionName = stateName + "." + signalName;
            // if we know about the transition
            bool retVal = false;

            retVal = GetGuardedTransition(fullTransitionName, Params, ref transition);

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

                if (!transition.IsInnerTransition)
                {
                    DoTransitionTo(stateHandler, transition.GetSlot());
                }

                // still execute actions on this inner transition
                transition.InvokeActions();
				
                retVal = true;
            }
			else if (signalName == "Internal.TransitionTo")
			{
                if (Params.Count == 1)
                {
                    GQHSMState state = (GQHSMState)Params[0].Value;
                    DoTransitionTo(state.GetStateHandler());
                    return true;
                }

                return false;
			}

            return retVal;
        }
		
        public void SignalTransition(string transitionName, object data)
        {
            AsyncDispatch(new QEvent(transitionName, data));
        }

        public void SignalTransitionSync(string transitionName, object data)
        {
            Dispatch(new QEvent(transitionName, data));
        }

        public bool GetGuardedTransition(string signalName, GQHSMParameters Params, ref GQHSMTransition transition)
        {
            List<GQHSMTransition> guardTansitions;

            guardTansitions = _nameToGuardedTransitionMultiMap[signalName];

            // retrieve unguarded transition if any
            _nameToTransitionMap.TryGetValue(signalName, out transition);
			if ((guardTansitions.Count == 0) && (transition == null))
			{
				return false;
			}

            foreach (GQHSMTransition gTransition in guardTansitions)
            {

                // call methods directly if any.
                if (gTransition.InvokeGuards())
                {
					transition = gTransition;
					break;
                }

                if (CallGuardHandler(gTransition.GuardCondition, Params))
                {
                    transition = gTransition;
					break;
                }
            }


			return true;

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
			_nameToTimeOutMap.Add(stateGuid, to);
        }

        public void SetTimeOuts(Guid stateGuid)
        {
			List<GQHSMTimeOut> timeOuts;

			timeOuts = _nameToTimeOutMap[stateGuid];

			foreach (GQHSMTimeOut timeOut in timeOuts)
			{
                timeOut.SetTimeOut(this);
            }
        }

        public void ClearTimeOuts(Guid stateGuid)
        {
			List<GQHSMTimeOut> timeOuts;

			timeOuts = _nameToTimeOutMap[stateGuid];

			foreach (GQHSMTimeOut timeOut in timeOuts)
			{
				timeOut.ClearTimeOut(this);
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
                    double timeOut = 0.0f;
					GQHSMAction action = null;
                    if (!Double.TryParse(timeOutExpression, out timeOut))
                    {
                        action = new GQHSMAction(this, timeOutExpression);
					}
                    RegisterTimeOutExpression(transition.GetSourceStateID(),
                        new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), TimeSpan.FromSeconds(timeOut), new QEvent(eventSignal), action));
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
						GQHSMAction action = null;
                        if (!DateTime.TryParse(timeOutValue, out dt))
                        {
							action = new GQHSMAction(this, timeOutValue);
						}
                        RegisterTimeOutExpression(transition.GetSourceStateID(),
                            new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), dt, new QEvent(eventSignal), action));
					} 
					else 
					{
                        Double timeOut = 0.0f;
						GQHSMAction action = null;
						if (!Double.TryParse(timeOutValue, out timeOut))
						{
							action = new GQHSMAction(this, timeOutValue);
						}

						RegisterTimeOutExpression(transition.GetSourceStateID(),
							new GQHSMTimeOut(transition.State[0].Name + "." + transition.GetFullName(), TimeSpan.FromSeconds(timeOut), new QEvent(eventSignal), flag, action));
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
        public void RegisterActionHandler(string sActionName, GQHSMHandler scHandler)
        {
            m_ActionHandlersMap.Add(sActionName, scHandler);
        }

        // Allow other classes to register Action Handlers for StateMachine Actions OnEntry/OnExit
        public void RegisterActionHandler(GQHSMHandler scHandler)
        {
            RegisterActionHandler(scHandler.ToString(), scHandler);
        }

        public object CallActionHandler(GQHSMAction action)
        {
            string actionName = action.Name;
            List<GQHSMHandler> actionList;
            List<object> retObjects = new List<object>();

            if (actionName.StartsWith("^"))
            {
                CallPortForward(action);
                return null;
            }
            else
            {
                actionList = m_ActionHandlersMap[actionName];
            }

            if (actionList != null)
            {
                foreach (GQHSMHandler scHandler in actionList)
                {
                    retObjects.Add(scHandler.Invoke(action.Params));
                }
            }

            if (retObjects.Count == 1)
            {
                return retObjects[0];
            }
            else if (retObjects.Count > 1)
            {
                return retObjects.ToArray();
            }

            return null;
        }

        public bool CallGuardHandler(string sGuardCondition, GQHSMParameters Params)
        {
            List<GQHSMHandler> guardList = null;

            guardList = m_ActionHandlersMap[sGuardCondition];
            foreach (GQHSMHandler gHandler in guardList)
            {
                bool validated = (bool)gHandler.Invoke(Params);
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

        public void CallPortForward(GQHSMAction action)
        {
            string[] splitS = action.Name.Split(new char[] { '^', '.'}, StringSplitOptions.RemoveEmptyEntries);
            if (splitS.Length <= 1)
            {
                Logger.Error("Need Source port and Action, i.e. ^Sourceport.Action");
                return;
            }

            // use port links to get destination port, if available
            GQHSMPort gp = GetPort(splitS[0]);
            if (gp != null)
            {
                gp.Port.Send(new QEvent(splitS[1], action.Params));
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
		public void SendPortAction(string sourcePortName, string actionName)
		{
			SendPortAction(sourcePortName, actionName, null);
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

        public void GatherHandlerMethodsAndVariables()
        {
            MethodInfo[] methodInfos;
            FieldInfo[] fieldInfos;
			PropertyInfo[] propertyInfos;

			Type classType = _handlerClass.GetType();
            methodInfos = classType.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                m_classHanderMethods.Add(methodInfo.Name, methodInfo);
            }

            fieldInfos = classType.GetFields();
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                // only take on public fields
                if (fieldInfo.IsPublic)
                {
                    _globals[fieldInfo.Name].SetReference(_handlerClass, fieldInfo);
                }
            }

			propertyInfos = classType.GetProperties();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				MethodInfo getMInfo = propertyInfo.GetGetMethod();
				MethodInfo setMInfo = propertyInfo.GetSetMethod();
				// only take on public fields
				if (((getMInfo != null) && (getMInfo.IsPublic)) || ((setMInfo != null) && (setMInfo.IsPublic)))
				{
					_globals[propertyInfo.Name].SetProperty(_handlerClass, propertyInfo);
				}
			}
		}

        /// <summary>
        /// Get Method Info structures that match
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public MethodInfo GetMethod(string methodName, GQHSMParameters Params)
        {
            List<MethodInfo> methodInfos = m_classHanderMethods[methodName];
            foreach (MethodInfo methodInfo in methodInfos)
            {
                ParameterInfo[] paramInfo = methodInfo.GetParameters();
                bool varsMatch = true;
                if (paramInfo.Length == Params.Count)
                {
                    for (int i = 0; i < Params.Count; i++)
                    {
                        // if Value is null, this is an unknown GQHSMVariable that can be mapped later
                        if ((Params[i].Value != null) && (Params[i].Value.GetType() != paramInfo[i].ParameterType))
                        {
                            varsMatch = false;
                            break;
                        }
                    }
                }
                if (varsMatch)
                {
                    return methodInfo;
                }

            }
            
            if (methodInfos.Count != 0)
            {
                Logger.Debug("Unable to find matching method signature for action handler function: {0}({1});", _name, Params.GetParametersString());
            }
            return null;
        }

        public override string ToString()
        {
            return _name;
        }
    }

}
