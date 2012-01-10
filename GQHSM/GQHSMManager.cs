﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace qf4net
{
    
    public class GQHSMManager : LoggingUserBase
    {
		
        public float UpdateRate = .1f;

        private static GQHSMManager _instance = null;

        private IQEventManager m_EventManager;
        private QHsmLifeCycleManagerWithHsmEventsBaseAndEventManager m_LifeCycleManager;

        private Dictionary<string, GQHSM> m_NameToHSM = new Dictionary<string, GQHSM>();

        /// <summary>
        /// the destination compoment to a list of port links
        /// </summary>
        private MultiMap<string, GQHSMPortLink> m_DestNameToPortLinks = new MultiMap<string, GQHSMPortLink>();

        public static GQHSMManager Instance
        {
            get 
            { 
                if (_instance == null)
                {
                    _instance = new GQHSMManager();
                }

                return _instance; 
            }
        }

        public void SaveToXML(string filePathName, GQHSM hsm)
        {
            FileStream fs = new FileStream(filePathName, FileMode.Create);
            if (fs == null)
            {
                Logger.Warn("Unable to create {0}", filePathName);
                return;
            }
			
			SaveToXML(fs, hsm);
			
            fs.Close();
        }
		
		public void SaveToXML(Stream fs, GQHSM hsm)
		{
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("","");
			
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(StateMachine));

            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);


            serializer.Serialize(fs, hsm.HSMData, ns);

		}
	
        public GQHSM LoadFromXML(string filePathName)
        {
            GQHSM theHSM;

            FileStream fs = new FileStream(filePathName, FileMode.Open);
            if (fs == null)
            {
                Logger.Warn("Unable to open {0}", filePathName);
                return null;
            }
			
			theHSM = LoadFromXML(Path.GetFileNameWithoutExtension(filePathName), fs);

            fs.Close();

            return theHSM;
        }
		
        public GQHSM LoadFromXML(string fileName, Stream fs)
        {
            GQHSM theHSM;

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(StateMachine));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);


            theHSM = new GQHSM(fileName);
			theHSM.HSMData = (StateMachine)serializer.Deserialize(fs);
            theHSM.PreInit();

            return theHSM;
        }
		
        public GQHSM LoadFromXML(string fileName, string sXML)
        {
            GQHSM theHSM;

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(StateMachine));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

			StringReader srXML = new StringReader(sXML);
            theHSM = new GQHSM(fileName);
            theHSM.HSMData = (StateMachine)serializer.Deserialize(srXML);
            theHSM.PreInit();

            return theHSM;
        }
		
        private GQHSMManager()
        {
            // create an QHSM Event manager	
            m_EventManager = new QMultiHsmEventManager(new QSystemTimer());

            // create a life cycle manager to hold multiple HSM's for us
            m_LifeCycleManager = new QHsmLifeCycleManagerWithHsmEventsBaseAndEventManager(m_EventManager);
        }

        public void SendPortAction(string sourcePortName, string actionName, object data)
        {

            foreach (GQHSM hsm in m_NameToHSM.Values)
            {
                hsm.SendPortAction(sourcePortName, actionName, data);
            }
        }

        public void RegisterPortLink(GQHSMPortLink portLink)
        {
            m_DestNameToPortLinks.Add(portLink.Component[1].Name, portLink);
        }

        public List<GQHSMPort> GetSourcePorts(string destHsmName, string destPortName)
        {
            List<GQHSMPort> retPorts = new List<GQHSMPort>();

            List<GQHSMPortLink> portLinks = m_DestNameToPortLinks[destHsmName];

            GQHSM sourceHsm;
            GQHSMPort srcPort;

            // lookup a portlink if there is one
            foreach (GQHSMPortLink portLink in portLinks)
            {
                if (portLink.ToPortName == destPortName)
                {
                    if (m_NameToHSM.ContainsKey(portLink.Component[0].Name))
                    {
                        sourceHsm = m_NameToHSM[portLink.Component[0].Name];
                        srcPort = sourceHsm.GetPort(portLink.FromPortName);
                        if (srcPort != null)
                        {
                            retPorts.Add(srcPort);
                        }
                    }
                }
            }

            // if no port links, then try to map destination hsm name to port in the source HSM
            if (portLinks.Count == 0)
            {
                // treat the portName == Component Name
                if (m_NameToHSM.ContainsKey(destPortName))
                {
                    sourceHsm = m_NameToHSM[destPortName];
                    // and the port equals the component name
                    // this maps Air.FuelMixture to FuelMixture.Air port names
                    srcPort = sourceHsm.GetPort(destHsmName);
                    if (srcPort != null)
                    {
                        retPorts.Add(srcPort);
                    }
                }
            }

            return retPorts;
        }

        public void RegisterHsm(GQHSM hsm)
        {
            m_NameToHSM.Add(hsm.GetName(), hsm);
            m_LifeCycleManager.RegisterHsm((ILQHsm)hsm);
        }

        public void UnregisterHsm(GQHSM hsm)
        {
            m_LifeCycleManager.UnregisterHsm((ILQHsm)hsm);
            m_NameToHSM.Remove(hsm.GetName());
        }

        /// <summary>
        /// Update function called from Timer or in Loop.
        /// </summary>
 
        public void Update()
        {
             m_EventManager.Poll();
        }

        protected void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Logger.Warn("Unknown Node: {0}\t {1}", e.Name, e.Text);
        }

        protected void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Logger.Warn("Unknown attribute {0} = '{1}'", attr.Name, attr.Value);
        }


    }
}
