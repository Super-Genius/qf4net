using System;
using System.Xml.Serialization;

namespace qf4net
{

    [XmlRoot("StateMachine")]
    public class GQHSMStateMachine
    {
        private GQHSMStateMachineInfo _stateMachineInfo;

        [XmlElement("StateMachineInfo")]
        public GQHSMStateMachineInfo StateMachineInfo
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

        [XmlElement("Glyphs")]
        public GQHSMGlyphs Glyphs;

    }
		

}
