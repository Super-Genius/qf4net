using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace qf4net
{
    
    public class GQHSMManager : LoggingUserBase
    {
		
        public float UpdateRate = .1f;

        private static GQHSMManager _instance = null;

        private IQEventManager m_EventManager;
        private QHsmLifeCycleManagerWithHsmEventsBaseAndEventManager m_LifeCycleManager;

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
            XmlSerializer serializer = new XmlSerializer(typeof(GHQSMSerializable));

            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);


            serializer.Serialize(fs, hsm.HSMData, ns);

		}
	
        public GQHSM LoadFromXML(string filePathName, bool bInit)
        {
            GQHSM theHSM;

            FileStream fs = new FileStream(filePathName, FileMode.Open);
            if (fs == null)
            {
                Logger.Warn("Unable to open {0}", filePathName);
                return null;
            }
			
			theHSM = LoadFromXML(Path.GetFileNameWithoutExtension(filePathName), fs, bInit);

            fs.Close();

            return theHSM;
        }
		
        public GQHSM LoadFromXML(string fileName, Stream fs, bool bInit)
        {
            GQHSM theHSM;

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(GHQSMSerializable));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);


            theHSM = new GQHSM();
			theHSM.HSMData = (GHQSMSerializable)serializer.Deserialize(fs);
			theHSM.SetName(fileName);
            if (bInit)
            {
                theHSM.Init();
            }

            return theHSM;
        }
		
        public GQHSM LoadFromXML(string fileName, string sXML, bool bInit)
        {
            GQHSM theHSM;

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(GHQSMSerializable));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

			StringReader srXML = new StringReader(sXML);
			theHSM = new GQHSM();
            theHSM.HSMData = (GHQSMSerializable)serializer.Deserialize(srXML);
			theHSM.SetName(fileName);
            if (bInit)
            {
                theHSM.Init();
            }

            return theHSM;
        }
		
        private GQHSMManager()
        {
            // create an QHSM Event manager	
            m_EventManager = new QMultiHsmEventManager(new QSystemTimer());

            // create a life cycle manager to hold multiple HSM's for us
            m_LifeCycleManager = new QHsmLifeCycleManagerWithHsmEventsBaseAndEventManager(m_EventManager);

            m_LifeCycleManager.UnhandledTransition += new DispatchUnhandledTransitionHandler(events_UnhandledTransition);
            m_LifeCycleManager.DispatchException += new DispatchExceptionHandler(events_DispatchException);
        }

        private void events_UnhandledTransition(IQHsm hsm, QState state, IQEvent ev)
        {
            Logger.Warn("Unhandled Transition Event {0}: From State Machine: {1}", ev.ToString(), hsm.ToString());
        }

        private void events_DispatchException(Exception ex, IQHsm hsm, QState state, IQEvent ev)
        {
            Logger.Warn("Dispatch Exception {0} Event: {1} From State Machine: {2}", ex.ToString(), ev.ToString(), hsm.ToString());
        }

        public void RegisterHsm(ILQHsm hsm)
        {
            m_LifeCycleManager.RegisterHsm(hsm);
        }

        public void UnregisterHsm(ILQHsm hsm)
        {
            m_LifeCycleManager.UnregisterHsm(hsm);
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
