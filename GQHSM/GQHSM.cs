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


    }

    [XmlRoot("Glyphs")]
    public class GLQHSM : GQHSM
    {
        private GLQHSMDum _lQHsm;
        private string _name;

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
        private Dictionary<string, GQHSMTransition> _nameToGuardedTransitionMap = new Dictionary<string, GQHSMTransition>();

        public void Init()
        {
            _lQHsm = new GLQHSMDum();
            GQHSMManager.Instance.RegisterHsm(_lQHsm);

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
                fullName = curState.Name + "::" + fullName;
            }

            _nameToStateMap.Add(fullName, state);

            return fullName;
        }

        public void AddTransitionNameMap(GQHSMTransition transition)
        {
            string tName;
            if (transition.EventSignal.Length != 0)
            {
                tName = transition.EventSignal;
            }
            else
            {
                tName = transition.Name;
            }

            // find state this transition is associated with
            GQHSMState state = GetState(transition.GetSourceStateID());
            if (state != null)
            {
                tName = state.GetFullName() + "::" + tName;
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
                if (!_nameToGuardedTransitionMap.ContainsKey(tName))
                {
                    _nameToGuardedTransitionMap.Add(tName, transition);
                }
                else
                {
                    _lQHsm.Logger.Error("Duplicate name in transitions {0}\n", tName);
                }

            }
        }

        public bool DoTransition(string fullTransitionName, object data)
        {
            GQHSMTransition transition;

            transition = GetGuardedTransition(fullTransitionName);
            if (transition != null)
            {
                string fullGuardedTransitionName = fullTransitionName;
                if (transition.GuardCondition.Length != 0)
                {
                    fullGuardedTransitionName = fullTransitionName + "::" + transition.GuardCondition;
                }

                GQHSMManager.Instance.CallTransitionHandler(_name, fullGuardedTransitionName, data);

                GQHSMState toState = GetState(transition.GetDestinationStateID());
                QState stateHandler = _lQHsm.GetTopState();
                if (toState != null)
                {
                    stateHandler = toState.GetStateHandler();
                }

                _lQHsm.DoTransitionTo(stateHandler, transition.GetSlot());
            }

            return false;
        }

        public void SignalTransition(string transitionName, object data)
        {
            _lQHsm.AsyncDispatch(new QEvent(transitionName, data));

        }

        public void InitState(QState newState)
        {
            _lQHsm.InitState(newState);
        }

        public GQHSMTransition GetGuardedTransition(string transistionName)
        {
            GQHSMTransition foundTransition = null;

            _nameToGuardedTransitionMap.TryGetValue(transistionName, out foundTransition);
            if (foundTransition != null)
            {
                string fullTranstionGuardName = transistionName + "::" + foundTransition.GuardCondition;
                // call the guard function if so
                if (!GQHSMManager.Instance.CallGuardHandler(_name, fullTranstionGuardName))
                {
                    foundTransition = null;
                }
            }

            if (foundTransition == null)
            {
                _nameToTransitionMap.TryGetValue(transistionName, out foundTransition);
            }
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

        public void SetName(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }

        ~GLQHSM()
        {
            GQHSMManager.Instance.UnregisterHsm(_lQHsm);

        }
    }

}
