using System;
using System.Xml.Serialization;

namespace qf4net
{
    public class GQHSMStateMachineInfo
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public Guid Id;

        public string ImplementationVersion;

        public string ModelFileName;

        public bool HasSubMachines;

        public int StateMachineVersion;

        public string BaseStateMachine;

        public string NameSpace;

        public string UsingNameSpaces;

        public string Comment;

        public string Fields;

        public bool ReadOnly;

        public string Assembly;
	}

}
