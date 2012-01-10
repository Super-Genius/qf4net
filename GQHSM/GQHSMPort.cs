using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using qf4net;

namespace qf4net
{
    public class GQHSMPort : GQHSMGlyph
    {
        private IQPort _port;
        private string _fullName;

        /// <summary>
        /// Any note for the state
        /// </summary>
        public bool IsMultiPort;

        public IQPort Port 
        { 
            get 
            { 
                return _port; 
            } 
        }

        public override void PreInit(GQHSM parentHSM)
        {
            base.PreInit(parentHSM);
            if (IsMultiPort)
            {
                _port = (IQPort)_parentHSM.CreateMultiPort(Name);
            }
            else
            {
                _port = _parentHSM.CreatePort(Name);
            }

            _fullName = parentHSM.GetName() + "." + Name;
            parentHSM.RegisterPort(this);
        }

        public override void Init()
        {
            base.Init();

            List<GQHSMPort> srcPorts = GQHSMManager.Instance.GetSourcePorts(_parentHSM.GetName(), Name);

            foreach (GQHSMPort srcPort in srcPorts)
            {
                srcPort.RegisterEventHandler(Port.Receive);
            }

        }

        public void RegisterEventHandler(QEventHandler qev)
        {
            _port.QEvents += qev;
        }

    }
}
