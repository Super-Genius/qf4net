using System;
using System.Xml.Serialization;

namespace qf4net
{
    public class GQHSMGlyphs
    {
        [XmlElement("StateGlyph")]
        public GQHSMState[] States;

        [XmlElement("TransitionGlyph")]
        public GQHSMTransition[] Transitions;

        [XmlElement("ComponentGlyph")]
        public GQHSMComponent[] Components;

        [XmlElement("PortLinkGlyph")]
        public GQHSMPortLink[] PortLinks;

        [XmlElement("StateTransitionPortGlyph")]
        public GQHSMPort[] Ports;
    }

}
