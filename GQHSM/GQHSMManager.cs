using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using qf4net;

namespace qf4net
{
    public delegate void StateChangeDelegate();
    public class StateChangeHandler
    {
        public object _sourceClass;
        public StateChangeDelegate classDelegate;

        public StateChangeHandler(object sourceClass, StateChangeDelegate sourceDelegate)
        {
            _sourceClass = sourceClass;
            classDelegate = sourceDelegate;
        }
    }

    public delegate void TransitionChangeDelegate(object data);
    public class TransitionChangeHandler
    {
        public object _sourceClass;
        public TransitionChangeDelegate classDelegate;

        public TransitionChangeHandler(object sourceClass, TransitionChangeDelegate sourceDelegate)
        {
            _sourceClass = sourceClass;
            classDelegate = sourceDelegate;
        }
    }

    public delegate bool GuardDelegate();
    public class GuardHandler
    {
        public object _sourceClass;
        public GuardDelegate classDelegate;

        public GuardHandler(object sourceClass, GuardDelegate sourceDelegate)
        {
            _sourceClass = sourceClass;
            classDelegate = sourceDelegate;
        }
    }

    public class GQHSMManager : LoggingUserBase
    {

        /// <summary>
        /// Action Handlers for HSMs
        /// </summary>
 
        private class ActionHandlers
        {
            public MultiMap<String, StateChangeHandler> ActionEntryMap = new MultiMap<String, StateChangeHandler>();
            public MultiMap<String, StateChangeHandler> ActionExitMap = new MultiMap<String, StateChangeHandler>();
        }
        private Dictionary<string, ActionHandlers> m_QHSMActionHandlers = new Dictionary<string, ActionHandlers>();

        /// transition change handlers for HSMs
        public MultiMap<String, TransitionChangeHandler> TransistionMap = new MultiMap<String, TransitionChangeHandler>();
        private Dictionary<string, MultiMap<String, TransitionChangeHandler>> m_QHSMTransitionHandlers = new Dictionary<string, MultiMap<String, TransitionChangeHandler>>();

        /// guard function delegates for HSMs
        public MultiMap<String, GuardHandler> GuardMap = new MultiMap<String, GuardHandler>();
        private Dictionary<string, MultiMap<String, GuardHandler>> m_QHSMGuardHandlers = new Dictionary<string, MultiMap<String, GuardHandler>>();

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
        public void RegisterActionHandler(StateChangeHandler scHandler, string sMachineName, string sActionName, string sSignalType)
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
            List<StateChangeHandler> actionList = null;
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
                    foreach (StateChangeHandler scHandler in actionList)
                    {
                        scHandler.classDelegate.Method.Invoke(scHandler._sourceClass, null);
                    }

                }

            }
        }

        // Allow other classes to register transitions Handlers for StateMachine Transition events
        public void RegisterTransitionHandler(TransitionChangeHandler tHandler, string sMachineName, string sTransitionName)
        {
            MultiMap<String, TransitionChangeHandler> transitionHandlers;

            if (!m_QHSMTransitionHandlers.TryGetValue(sMachineName, out transitionHandlers))
            {
                transitionHandlers = new MultiMap<String, TransitionChangeHandler>();
                m_QHSMTransitionHandlers.Add(sMachineName, transitionHandlers);
            }

            transitionHandlers.Add(sTransitionName, tHandler);
        }

        public void CallTransitionHandler(string sMachineName, string sTransitionName, object data)
        {
            List<TransitionChangeHandler> transitionList = null;
            MultiMap<String, TransitionChangeHandler> transitionHandlers;

            if (m_QHSMTransitionHandlers.TryGetValue(sMachineName, out transitionHandlers))
            {
                transitionList = transitionHandlers[sTransitionName];
                foreach (TransitionChangeHandler tHandler in transitionList)
                {
                    tHandler.classDelegate.Method.Invoke(tHandler._sourceClass, new object[] { data });
                }

            }
        }

        // Allow other classes to register guard Handlers for StateMachine guard checking
        public void RegisterGuardHandler(GuardHandler gHandler, string sMachineName, string sGuardName)
        {
            MultiMap<String, GuardHandler> guardHandlers;

            if (!m_QHSMGuardHandlers.TryGetValue(sMachineName, out guardHandlers))
            {
                guardHandlers = new MultiMap<String, GuardHandler>();
                m_QHSMGuardHandlers.Add(sMachineName, guardHandlers);
            }

            guardHandlers.Add(sGuardName, gHandler);
        }

        public bool CallGuardHandler(string sMachineName, string sTransitionName)
        {
            List<GuardHandler> guardList = null;
            MultiMap<String, GuardHandler> guardHandlers;

            if (m_QHSMGuardHandlers.TryGetValue(sMachineName, out guardHandlers))
            {
                guardList = guardHandlers[sTransitionName];
                foreach (GuardHandler gHandler in guardList)
                {
                    return (bool)gHandler.classDelegate.Method.Invoke(gHandler._sourceClass, null);
                }

            }

            return true;
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
