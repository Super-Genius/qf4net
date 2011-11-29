using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace qf4net
{
    [Serializable]
    public class GQHSMTransition
    {

        private GQHSMTransitionState[] _transitionStates;
        private GQHSM _parentHSM;
        private int _slot;


        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public Guid Id;
        [XmlAttribute]
        public Boolean DoNotInstrument;

        public string Note;

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

        public void Init(GLQHSM parentHSM, int slot)
        {
            _parentHSM = parentHSM;
            _slot = slot;
            parentHSM.RegisterTransition(this);
            if (TimeOutExpression.Length > 0)
            {
                parentHSM.RegisterTimeOutExpression(this, TimeOutExpression);
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

            if (Name.Length > 0)
            {
                if (EventSignal.Length > 0)
                {
                    retS = Name + "." + EventSignal;
                }
                else
                {
                    retS = Name;
                }
            }

            return retS;

        }

    }

    [Serializable]
    public class GQHSMTransitionState
    {
        public Guid Id;
        public string Name;
    }

}

