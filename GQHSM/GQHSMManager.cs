using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace qf4net
{
    public delegate void GQHSMDelegate();
    public delegate object GQHSMDelegateOO(object Params);
 
    public class GQHSMHandler
    {
        private object _sourceObject;
        private GQHSMDelegateOO _classDelegateOO = null;
        private GQHSMDelegate _classDelegate = null;

        public GQHSMHandler(object sourceObject, GQHSMDelegateOO sourceDelegate)
        {
            _sourceObject = sourceObject;
            _classDelegateOO = sourceDelegate;
        }

        public GQHSMHandler(object sourceObject, GQHSMDelegate sourceDelegate)
        {
            _sourceObject = sourceObject;
            _classDelegate = sourceDelegate;
        }

        public object Invoke(object Params = null)
        {
            if (_classDelegate != null)
                return _classDelegate.Method.Invoke(_sourceObject, null);
            if (_classDelegateOO != null)
                return _classDelegateOO.Method.Invoke(_sourceObject, new object[] { Params });

            return null;
        }

    }

    
    public class GQHSMManager : LoggingUserBase
    {

        /// <summary>
        /// Action Handlers for HSMs
        /// </summary>
 
        private class ActionHandlers
        {
            public MultiMap<String, GQHSMHandler> ActionEntryMap = new MultiMap<String, GQHSMHandler>();
            public MultiMap<String, GQHSMHandler> ActionExitMap = new MultiMap<String, GQHSMHandler>();
        }
        private Dictionary<string, ActionHandlers> m_QHSMActionHandlers = new Dictionary<string, ActionHandlers>();

        /// transition change handlers for HSMs
        //public MultiMap<String, GQHSMHandler> TransistionMap = new MultiMap<String, GQHSMHandler>();
        private Dictionary<string, MultiMap<String, GQHSMHandler>> m_QHSMTransitionHandlers = new Dictionary<string, MultiMap<String, GQHSMHandler>>();

        /// guard function delegates for HSMs
        //public MultiMap<String, GQHSMHandler> GuardMap = new MultiMap<String, GQHSMHandler>();
        private Dictionary<string, MultiMap<String, GQHSMHandler>> m_QHSMGuardHandlers = new Dictionary<string, MultiMap<String, GQHSMHandler>>();
		
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

        public void SaveToXML(string filePathName, GLQHSM hsm)
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
		
		public void SaveToXML(Stream fs, GLQHSM hsm)
		{
			// Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(GLQHSM));

            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);


            serializer.Serialize(fs, hsm);

		}
	
        public GLQHSM LoadFromXML(string filePathName, bool bInit)
        {
            GLQHSM theHSM;

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
		
        public GLQHSM LoadFromXML(string fileName, Stream fs, bool bInit)
        {
            GLQHSM theHSM;

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(GLQHSM));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);


            theHSM = (GLQHSM)serializer.Deserialize(fs);
			theHSM.SetName(fileName);
            if (bInit)
            {
                theHSM.Init();
            }

            return theHSM;
        }
		
        public GLQHSM LoadFromXML(string fileName, string sXML, bool bInit)
        {
            GLQHSM theHSM;

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(GLQHSM));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

			StringReader srXML = new StringReader(sXML);
            theHSM = (GLQHSM)serializer.Deserialize(srXML);
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

        // Allow other classes to register Action Handlers for StateMachine Actions OnEntry/OnExit
        public void RegisterActionHandler(GQHSMHandler scHandler, string sMachineName, string sActionName, string sSignalType)
        {
            ActionHandlers actionHandlers;

            if (!m_QHSMActionHandlers.TryGetValue(sMachineName, out actionHandlers))
            {
                actionHandlers = new ActionHandlers();
                m_QHSMActionHandlers.Add(sMachineName, actionHandlers);
            }

            switch (sSignalType)
            {
                case QSignals.Entry:
                    actionHandlers.ActionEntryMap.Add(sActionName, scHandler);
                    break;
                case QSignals.Exit:
                    actionHandlers.ActionExitMap.Add(sActionName, scHandler);
                    break;
            }
        }

        public void CallActionHandler(string sMachineName, string sActionName, string sSignalType)
        {
            List<GQHSMHandler> actionList = null;
            ActionHandlers actionHandlers;

            if (m_QHSMActionHandlers.TryGetValue(sMachineName, out actionHandlers))
            {
                switch (sSignalType)
                {
                    case QSignals.Entry:
                        actionList = actionHandlers.ActionEntryMap[sActionName];
                        break;
                    case QSignals.Exit:
                        actionList = actionHandlers.ActionExitMap[sActionName];
                        break;
                }

                if (actionList != null)
                {
                    foreach (GQHSMHandler scHandler in actionList)
                    {
                       scHandler.Invoke();
                    }

                }

            }
        }

        // Allow other classes to register transitions Handlers for StateMachine Transition events
        public void RegisterTransitionHandler(GQHSMHandler tHandler, string sMachineName, string sTransitionName)
        {
            MultiMap<String, GQHSMHandler> transitionHandlers;

            if (!m_QHSMTransitionHandlers.TryGetValue(sMachineName, out transitionHandlers))
            {
                transitionHandlers = new MultiMap<String, GQHSMHandler>();
                m_QHSMTransitionHandlers.Add(sMachineName, transitionHandlers);
            }

            transitionHandlers.Add(sTransitionName, tHandler);
        }

        public void CallTransitionHandler(string sMachineName, string sTransitionName, object data)
        {
            List<GQHSMHandler> transitionList = null;
            MultiMap<String, GQHSMHandler> transitionHandlers;

            if (m_QHSMTransitionHandlers.TryGetValue(sMachineName, out transitionHandlers))
            {
                transitionList = transitionHandlers[sTransitionName];
                foreach (GQHSMHandler tHandler in transitionList)
                {
                    tHandler.Invoke( data );
                }

            }
        }

        // Allow other classes to register guard Handlers for StateMachine guard checking
        public void RegisterGuardHandler(GQHSMHandler gHandler, string sMachineName, string sGuardName)
        {
            MultiMap<String, GQHSMHandler> guardHandlers;

            if (!m_QHSMGuardHandlers.TryGetValue(sMachineName, out guardHandlers))
            {
                guardHandlers = new MultiMap<String, GQHSMHandler>();
                m_QHSMGuardHandlers.Add(sMachineName, guardHandlers);
            }

            guardHandlers.Add(sGuardName, gHandler);
        }

        public bool CallGuardHandler(string sMachineName, string sGuardCondition)
        {
            List<GQHSMHandler> guardList = null;
            MultiMap<String, GQHSMHandler> GQHSMHandlers;

            if (m_QHSMGuardHandlers.TryGetValue(sMachineName, out GQHSMHandlers))
            {
                guardList = GQHSMHandlers[sGuardCondition];
                foreach (GQHSMHandler gHandler in guardList)
                {
                    bool validated = (bool)gHandler.Invoke();
                    if (validated)
                        return true;
                }

            }

            return false;
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
