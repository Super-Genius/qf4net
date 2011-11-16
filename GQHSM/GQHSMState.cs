/// <summary>
/// GQHSMstate Implementation file.
/// </summary>
/// <remarks>
/// Copyright (c) 2011 Sony Ericsson
/// Author: Kenneth Hurley
/// </remarks>

using System;
using System.Xml.Serialization;

namespace qf4net
{
	public class GQHSMState
	{

        private string[] _stateCommands;
        private QState _stateHandler;
        private GLQHSM _parentHSM;
        private string _fullName;       // fully qualified name, i.e.  State1::State2::State3, etc.
        private GQHSMState _parent;
        private GQHSMState _childStartState = null;

		/// <summary>
		/// The name of the State
		/// </summary>
		[XmlAttribute]
		public string Name;

        [XmlAttribute]
        public Boolean DoNotInstrument;
		/// <summary>
		/// The Guid of this state, good for better comparison
		/// </summary>
        [XmlAttribute]
		public Guid Id;
		
		/// <summary>
		/// The entry action that gets executed when the state is entered
		/// </summary> 
		public string EntryAction;
		
		/// <summary>
		/// The exit action that gets executed when the state is exited
		/// </summary>
		public string ExitAction;

        public Boolean IsStartState;

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
		/// Any note for the state
		/// </summary>
 		public string Note;

        public Guid ParentId;

        [XmlElement("StateGlyph")]
        public GQHSMState[] ChildStates;

        /// <summary>
        /// The default constructor for the object
        /// </summary>
        public GQHSMState()
        {
            Name = "";
            DoNotInstrument = true;
            Id = System.Guid.NewGuid();
            EntryAction = "";
            ExitAction = "";
            IsStartState = false;
            Note = "";
            _parent = null;
            _parentHSM = null;
        }

        public void Init(GLQHSM parentHSM, GQHSMState parent)
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

        protected virtual QState StateHandler(IQEvent ev)
        {
            GQHSMManager manager =  GQHSMManager.Instance;
            switch (ev.QSignal)
            {
                case QSignals.Init:
                    {
                        GQHSMState childStartState = GetChildStartState();
                        if (childStartState != null)
                        {
                            _parentHSM.InitState(childStartState.GetStateHandler());
                            return null;
                        }
                    }
                    break;

                case QSignals.Entry:
                    {
                        manager.CallActionHandler(_parentHSM.GetName(), _fullName, QSignals.Entry);
                    }
                    return null;
                case QSignals.Exit:
                    {
                        manager.CallActionHandler(_parentHSM.GetName(), _fullName, QSignals.Exit);
                    }
                    return null;
                case QSignals.Empty:
                    {
                    }
                    break;
                default:
                    {
                        string transitionName = _fullName + "::" + ev.QSignal;
                        if (_parentHSM.DoTransition(transitionName, ev.QData))
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

