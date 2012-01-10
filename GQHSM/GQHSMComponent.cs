using System;
using System.Xml.Serialization;
using qf4net;

namespace qf4net
{
    public class GQHSMComponent : GQHSMGlyph
    {
        /// <summary>
        /// Identifier name for this component type.
        /// </summary>
        public String TypeName;

        /// <summary>
        /// The parent identifier.
        /// </summary>
        public Guid ParentId;

        /// <summary>
        /// If there are multiple instances of this component
        /// </summary>
        public bool IsMultiInstance;
    }
}
