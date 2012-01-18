/// <summary>
/// GQHSMstate Implementation file.
/// </summary>
/// <remarks>
/// Copyright (c) 2011 Sony Ericsson
/// Author: Kenneth Hurley
/// </remarks>

using System;
using System.Xml.Serialization;
using qf4net;

namespace qf4net
{
    public class GQHSMState : GQHSMGlyph
	{

        private string[] _stateCommands;
        private QState _stateHandler;
        private string _fullName;       // fully qualified name, i.e.  State1.State2.State3, etc.
        private GQHSMState _parent;
        private GQHSMState _childStartState = null;
        private GQHSMActions _entryActions = null;
        private GQHSMActions _exitActions = null;
        private GQHSMActions _actions = null;
				
		/// <summary>
		/// The parent identifier.
		/// </summary>
        public Guid ParentId;
		
		/// <summary>
		/// if this is a starting state
		/// </summary>
        public Boolean IsStartState;

		/// <summary>
		/// The entry action that gets executed when the state is entered
		/// </summary> 
		public string EntryAction;
		
		/// <summary>
		/// The exit action that gets executed when the state is exited
		/// </summary>
		public string ExitAction;
		
		/// <summary>
		/// The state commands that get executed from transitions
		/// </summary>
        [XmlArray]
        [XmlArrayItem("Command")]
        public string[] StateCommands
        {
            get { return this._stateCommands; }
            set { this._stateCommands = value; }
        }
		
		/// <summary>
		/// The child states.
		/// </summary>
        [XmlElement("StateGlyph")]
        public GQHSMState[] ChildStates;
		
        /// <summary>
        /// The default constructor for the object
        /// </summary>
        public GQHSMState()
        {
            Name = "";
            DoNotInstrument = false;
            Id = System.Guid.NewGuid();
            EntryAction = "";
            ExitAction = "";
            IsStartState = false;
            Note = "";
            _parent = null;
            _parentHSM = null;
			_stateCommands = null;
        }

        public override void PreInit(GQHSM parentHSM)
        {
            base.PreInit(parentHSM);
            _fullName = _parentHSM.RegisterState(this);
            if (ChildStates != null)
            {
                foreach (GQHSMState gs in ChildStates)
                {
                    gs.PreInit(parentHSM);
                    if (gs.IsStartState)
                    {
                        _childStartState = gs;
                    }
                }
            }

            _stateHandler = new QState(this, StateHandler, Name);
        }

        public void Init(GQHSMState parent)
        {
            base.Init();
            _parent = parent;
            if (ChildStates != null)
            {
                foreach (GQHSMState gs in ChildStates)
                {
                    gs.Init(this);
                }
            }

            if (EntryAction.Length > 0)
            {
                _entryActions = new GQHSMActions(_parentHSM, EntryAction);
            }

            if (ExitAction.Length > 0)
            {
                _exitActions = new GQHSMActions(_parentHSM, ExitAction);
            }

            if ((_stateCommands != null) && (_stateCommands.Length > 0))
            {
                _actions = new GQHSMActions(_parentHSM, _stateCommands);
            }
        }

        private GQHSMState GetChildStartState()
        {
            return _childStartState;
        }

        public QState StateHandler(IQEvent ev)
        {
            GQHSMManager manager =  GQHSMManager.Instance;
            switch (ev.QSignal)
            {
                case QSignals.Init:
                    {
                        GQHSMState childStartState = GetChildStartState();
                        if (childStartState != null)
                        {
                            if (_parentHSM.Instrument && !DoNotInstrument)
                                _parentHSM.LogStateEvent(StateLogType.Init, _stateHandler, childStartState.GetStateHandler());
                            _parentHSM.InitState(childStartState.GetStateHandler());
                            return null;
                        }
                    }
                    break;

                case QSignals.Entry:
                    {
                        if (_parentHSM.Instrument && !DoNotInstrument)
                            _parentHSM.LogStateEvent(StateLogType.Entry, _stateHandler);
                        if (_entryActions != null)
                            _entryActions.InvokeActionHandlers();
                        _parentHSM.SetTimeOuts(Id);
                    }
                    return null;
                case QSignals.Exit:
                    {
                        if (_parentHSM.Instrument && !DoNotInstrument)
                            _parentHSM.LogStateEvent(StateLogType.Exit, _stateHandler);
                        if (_exitActions != null)
                            _exitActions.InvokeActionHandlers();
                        _parentHSM.ClearTimeOuts(Id);
                    }
                    return null;
                case QSignals.Empty:
                    {
                    }
                    break;
                default:
                    {
						GQHSMParameters evParams = (GQHSMParameters)ev.QData;
						if (ev.QData is GQHSMParameters)
                        {
							if (evParams[0] != null)
							{
								manager.Globals["ev"].Copy(evParams[0]);
							}
                        }

                        if (_parentHSM.StateTransitionInternal(_fullName, evParams, ev.QSignal))
                        {
                            return null;
                        }
                    }
                    break;

            }

            return _parentHSM.GetStateHandler(ParentId);
        }

        public QState GetStateHandler()
        {
            return _stateHandler;
        }

        public GQHSMState GetParent()
        {
            return _parent;
        }

        public string GetFullName()
        {
            return _fullName;
        }

	}

}

