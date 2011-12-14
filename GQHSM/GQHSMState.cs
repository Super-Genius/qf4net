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
	public class GQHSMState
	{

        private string[] _stateCommands;
        private QState _stateHandler;
        private GQHSM _parentHSM;
        private string _fullName;       // fully qualified name, i.e.  State1.State2.State3, etc.
        private GQHSMState _parent;
        private GQHSMState _childStartState = null;

		/// <summary>
		/// The name of the State
		/// </summary>
		[XmlAttribute]
		public string Name;

		/// <summary>
		/// The Guid of this state, good for better comparison
		/// </summary>
        [XmlAttribute]
		public Guid Id;
		
		/// <summary>
		/// if true, does not log and instrument the state
		/// </summary>
        [XmlAttribute]
        public Boolean DoNotInstrument;
		
		/// <summary>
		/// Any note for the state
		/// </summary>
 		public string Note;
		
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

        public void Init(GQHSM parentHSM, GQHSMState parent)
        {
            _parentHSM = parentHSM;
            _parent = parent;
            _fullName = parentHSM.RegisterState(this);

            _stateHandler = new QState(this, StateHandler, Name);

            if (ChildStates != null)
            {
                foreach (GQHSMState gs in ChildStates)
                {
                    if (gs.IsStartState)
                    {
                        _childStartState = gs;
                    }

                    gs.Init(parentHSM, this);
                }
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
                        string actionName = _fullName;
                        if (EntryAction.Length > 0)
                        {
                            actionName = EntryAction;
                        }
                        _parentHSM.CallActionHandler(_parentHSM.GetName(), actionName, QSignals.Entry, _parentHSM.GetData());
                        _parentHSM.SetTimeOut(Id);
                    }
                    return null;
                case QSignals.Exit:
                    {
                        if (_parentHSM.Instrument && !DoNotInstrument)
                            _parentHSM.LogStateEvent(StateLogType.Exit, _stateHandler);
                        string actionName = _fullName;
                        if (ExitAction.Length > 0)
                        {
                            actionName = ExitAction;
                        }
                       	_parentHSM.CallActionHandler(_parentHSM.GetName(), actionName, QSignals.Exit, _parentHSM.GetData());
                        _parentHSM.ClearTimeOut(Id);
                    }
                    return null;
                case QSignals.Empty:
                    {
                    }
                    break;
                default:
                    {
                        if (_parentHSM.StateTransitionInternal(_fullName, ev.QSignal, ev.QData))
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

