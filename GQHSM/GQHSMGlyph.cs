using System;
using System.Xml.Serialization;

namespace qf4net
{
    public class GQHSMGlyph
    {
        protected GQHSM _parentHSM;

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

        public string Note;

        public virtual void PreInit(GQHSM parentHSM)
        {
            _parentHSM = parentHSM;
        }

        public virtual void Init()
        {
        }
    }
}
