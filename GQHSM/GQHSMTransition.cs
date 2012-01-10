using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using qf4net;

namespace qf4net
{
    public class GQHSMTransition : GQHSMGlyph
    {

        private GQHSMTransitionState[] _transitionStates;
        private int _slot;

        public string EventSignal;

        public string EventSource;

        public string GuardCondition;
        public string Action;

        public int EvaluationOrderPriority;

        public string EventType;
        
        public Boolean IsInnerTransition;

        public string TimeOutExpression;

        public enum eTransitionType {
            Normal,
            History,
            DeepHistory
        }

        public eTransitionType TransitionType;

        [XmlElement]
        public GQHSMTransitionState[] State
        {
            get { return this._transitionStates; }
            set { this._transitionStates = value; }
        }

        public GQHSMTransition()
        {
            Name = "";
            Id = System.Guid.NewGuid();
            DoNotInstrument = true;
            Note = "";
            EventSignal = "";
            EventSource = "";
            GuardCondition = "";
            Action = "";
            EventType = "";
            IsInnerTransition = false;
            TimeOutExpression = "";
            TransitionType = eTransitionType.Normal;
        }

        public override void PreInit(GQHSM parentHSM)
        {
            base.PreInit(parentHSM);
            _parentHSM.RegisterTransition(this);
        }

        public void Init(int slot)
        {
            base.Init();
            _slot = slot;
            if (TimeOutExpression.Length > 0)
            {
                _parentHSM.RegisterTimeOutExpression(this, TimeOutExpression);
            }

        }

        public Guid GetSourceStateID()
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
            return _transitionStates[0].Id;
        }

        public Guid GetDestinationStateID()
        {
            return _transitionStates[1].Id;
        }

        public int GetSlot()
        {
            return _slot;
        }

        public string GetFullName()
        {
            string retS = EventSignal;

            // default to Name if Event Signal isn't there.
            if (EventSignal.Length == 0)
            {
                if (Name.Length == 0)
                {
                    retS = "UNKNOWN";
                }
                else
                {
                    retS = Name;
                }
            }

            // if source is specified prepend it to signal name
            if (EventSource.Length > 0)
            {
                retS = EventSource + "." + retS;
            }

            return retS;

        }

    }

    public class GQHSMTransitionState
    {
        public Guid Id;
        public string Name;
    }

}

