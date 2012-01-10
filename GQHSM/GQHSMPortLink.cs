using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using qf4net;

namespace qf4net
{
    public class GQHSMPortLink : GQHSMGlyph
    {
        private GQHSMComponentLink[] _componentLinks;

        /// <summary>
        /// the port name the port link is coming from
        /// </summary>
        public string FromPortName;

        /// <summary>
        /// The index of the send in case multiPort?
        /// </summary>
        public string SendIndex;

        /// <summary>
        /// The name of the port to send the action to.
        /// </summary>
        public string ToPortName;

        /// <summary>
        /// Unknown at this time.
        /// </summary>
        public string Interface;

        /// <summary>
        /// The components that are port linked togther
        /// </summary>
        [XmlElement]
        public GQHSMComponentLink[] Component
        {
            get { return this._componentLinks; }
            set { this._componentLinks = value; }
        }

        public override void PreInit(GQHSM parentHSM)
        {
            base.PreInit(parentHSM);

            if (Component == null)
            {
                Component = new GQHSMComponentLink[2];
                Component[0] = new GQHSMComponentLink();
                Component[0].Id = new Guid();
                Component[0].Name = FromPortName;
                Component[1] = new GQHSMComponentLink();
                Component[1].Id = new Guid();
                Component[1].Name = ToPortName;
            }

            GQHSMManager.Instance.RegisterPortLink(this);
        }
    }

    public class GQHSMComponentLink
    {
        public Guid Id;
        public string Name;
    }

}
