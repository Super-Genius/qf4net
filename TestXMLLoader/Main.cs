using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using qf4net;
using MurphyPA.Logging;

namespace TestXMLLoader
{
	class MainClass
	{
		public static void Main (string[] args)
		{
#if true
            Lighter lighter;

            lighter = new Lighter();
			
			//testZombieXML.WriteStateMachine("testzombie-write.xml");
			lighter.Run();

#else
            TestZombieXML testZombieXML;

            testZombieXML = new TestZombieXML();
			
			testZombieXML.WriteStateMachine("testzombie-write.xml");
			testZombieXML.Run();
#endif
        }
    }

    class Lighter
    {
        private GQHSM[] _theHSM = new GQHSM[4];

        public Lighter()
        {

            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM[0] = manager.LoadFromXML("Air.xml");
            // let's hook into the debugging
            new MyLogger(_theHSM[0]);
            _theHSM[1] = manager.LoadFromXML("Fuelmixture.xml");
            new MyLogger(_theHSM[1]);
            _theHSM[2] = manager.LoadFromXML("Valve.xml");
            new MyLogger(_theHSM[2]);
            _theHSM[3] = manager.LoadFromXML("Flint.xml");
            new MyLogger(_theHSM[3]);
            _theHSM[0].Init();
            _theHSM[1].Init();
            _theHSM[2].Init();
            _theHSM[3].Init();

        }

        public void Run()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            // OK, run the state machine for a while.
            while (true)
            {
                manager.Update();
                ConsoleKeyInfo input;

                if (Console.KeyAvailable)
                {
                    input = Console.ReadKey(true);
                    switch (input.Key)
                    {
                        case System.ConsoleKey.S:
                            {
                                manager.SendPortAction("User", "Spin", null);
                            }
                            break;
                        case System.ConsoleKey.P:
                            {
                                manager.SendPortAction("User", "Press", null);
                            }
                            break;
                        case System.ConsoleKey.R:
                            {
                                manager.SendPortAction("User", "Release", null);
                            }
                            break;
                        case System.ConsoleKey.OemPlus:
                            {
                                manager.SendPortAction("User", "IncreaseFlow", null);
                            }
                            break;

                        case System.ConsoleKey.OemMinus:
                            {
                                manager.SendPortAction("User", "DecreaseFlow", null);
                            }
                            break;
                    }
                }
            }
        }
    }

    class TestZombieXML
    {
        private GQHSM _theHSM;
        private bool _endOfPath = false;
        private bool _isBehindDoor = false;
        private bool _pathToPlayer = false;
        private Random _randObj = new Random();
        private MyLogger _mLogger;

        public TestZombieXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM = manager.LoadFromXML("ZombieAI.xml");
            // let's hook into the debugging

            _mLogger = new MyLogger(_theHSM);

            _theHSM.RegisterGuardHandler(new GQHSMHandler(this, RandomRemain), "RandomRemain(5)");
            _theHSM.RegisterGuardHandler(new GQHSMHandler(this, IsEndOfPath), "IsEndOfPath()");
            _theHSM.RegisterGuardHandler(new GQHSMHandler(this, PathToPlayer), "PathToPlayer()");
            //_theHSM.RegisterGuardHandler(new GQHSMHandler(this, IsBehindDoor), "ZombieAI", "IsBehindDoor()");

            _theHSM.RegisterActionHandler(new GQHSMHandler(this, OnClimbingOut), "OnClimbingOut", "ENTRY");
            _theHSM.Init();


        }

        public object RandomRemain(object modulo)
        {

            return (bool)((_randObj.Next() % 5) != 0);

        }

        public object IsEndOfPath(object dum)
        {
            return _endOfPath;
        }

        public object PathToPlayer(object dum)
        {
            return _pathToPlayer;
        }

        public object IsBehindDoor(object dum)
        {
            return _isBehindDoor;
        }

        public void OnClimbingOut()
        {
            _theHSM.SignalTransition("ClimbedOut", null);
        }

        public void Run()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            // OK, run the state machine for a while.
            while (true)
            {
                manager.Update();
                ConsoleKeyInfo input;

                if (Console.KeyAvailable)
                {
                    input = Console.ReadKey(true);
                    switch (input.Key)
                    {
                        case System.ConsoleKey.L:
                            {
                                _theHSM.SignalTransition("Trigger.Ladder.Enter", null);
                            }
                            break;
                        case System.ConsoleKey.F:
                            {
                                _theHSM.SignalTransition("Move.Horizontal", null);
                            }
                            break;
                        case System.ConsoleKey.G:
                            {
                                _theHSM.SignalTransition("Trigger.OnGround", null);
                            }
                            break;
                        case System.ConsoleKey.H:
                            {
                                _theHSM.SignalTransition("Trigger.Hole.Enter", null);
                            }
                            break;

                        case System.ConsoleKey.P:
                            {
                                _theHSM.SignalTransition("Trigger.Player.Enter", null);
                            }
                            break;

                        case System.ConsoleKey.D:
                            {
                                _isBehindDoor = !_isBehindDoor;
                            }
                            break;

                        case System.ConsoleKey.E:
                            {
                                _endOfPath = !_endOfPath;
                            }
                            break;

                        case System.ConsoleKey.T:
                            {
                                _pathToPlayer = !_pathToPlayer;
                            }
                            break;

                        case System.ConsoleKey.V:
                            {
                                _theHSM.SignalTransition("Move.Vertical", null);
                            }
                            break;

                        case System.ConsoleKey.O:
                            {
                                _theHSM.SignalTransition("DoorOpened", null);
                            }
                            break;
                    }
                }
            }

        }

		public void WriteStateMachine(string name)
		{
            GQHSMManager.Instance.SaveToXML(name, _theHSM);
			
		}

    }

    class MyLogger
    {
        private ILogger _logger;

        public MyLogger(GQHSM theHsm)
        {
            _logger = theHsm.Logger;
            theHsm.Instrument = true;
            theHsm.StateChange += new EventHandler(hsm_StateChange);
            theHsm.DispatchException += new DispatchExceptionHandler(hsm_DispatchException);
            theHsm.UnhandledTransition += new DispatchUnhandledTransitionHandler(hsm_UnhandledTransition);

        }

        private void hsm_StateChange(object sender, EventArgs e)
        {
            LogStateEventArgs args = e as LogStateEventArgs;
            GQHSM hsm = (GQHSM)sender;

            switch (args.LogType)
            {
                case StateLogType.Init:
                    {
                        _logger.Debug("[{0}{1}] {2} to {3}", hsm.GetName(), args.LogType.ToString(), args.State, args.NextState.Name);
                    }
                    break;

                case StateLogType.Entry:
                    {
                        _logger.Debug("[{0}{1}] {2}", hsm.GetName(), args.LogType.ToString(), args.State.Name);
                    }
                    break;

                case StateLogType.Exit:
                    {
                        _logger.Debug("[{0}{1}] {2}", hsm.GetName(), args.LogType.ToString(), args.State.Name);
                    }
                    break;

                case StateLogType.EventTransition:
                    {
                        _logger.Debug("[{0}{1}] {2} on {3} to {4} -> {5}", hsm.GetName(), args.LogType.ToString(), args.State.Name, args.EventName, args.NextState.Name, args.EventDescription);
                    }
                    break;

                case StateLogType.Log:
                    {
                        _logger.Debug("[{0}{1}] {2}", hsm.GetName(), args.LogType.ToString(), args.LogText);
                    }
                    break;

                default:
                    throw new NotSupportedException("StateLogType." + args.LogType.ToString());
            }
        }

        private void hsm_DispatchException(Exception ex, qf4net.IQHsm hsm, QState state, qf4net.IQEvent ev)
        {
            _logger.Debug("[{0}.{1}] Exception: on event {2}\n{3}", ((GQHSM)(hsm)).GetName(), state.Name, ev.ToString(), ex.ToString());
        }

        private void hsm_UnhandledTransition(qf4net.IQHsm hsm, QState state, qf4net.IQEvent ev)
        {
            _logger.Debug("[{0}.{1}] Unhandled Event: {2}", ((GQHSM)(hsm)).GetName(), state.Name, ev.ToString());
        }
    }

    class TestBookXML
    { 
        GQHSM _theHSM;

        public TestBookXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM.RegisterActionHandler(new GQHSMHandler(null, NotOwnedByLibraryEntry), "NotOwnedByLibrary", QSignals.Entry);
            _theHSM.RegisterActionHandler(new GQHSMHandler(null, NotOwnedByLibraryExit), "NotOwnedByLibrary", QSignals.Exit);
            _theHSM.RegisterActionHandler(new GQHSMHandler(null, NotOwnedByLibraryEntry_OtherOwner), "NotOwnedByLibrary.OtherOwner", QSignals.Entry);
            _theHSM.RegisterTransitionHandler(new GQHSMHandler(null, NotOwnedByLibrary_Bought), "NotOwnedByLibrary.Bought");
            _theHSM.RegisterGuardHandler(new GQHSMHandler(null, IsLate), "OwnedByLibrary.OnLoan.IsLate()");

            _theHSM = manager.LoadFromXML("book.xml");
    	}

        public void Run()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            // OK, run the state machine for a while.
            while (true)
            {
                manager.Update();
                ConsoleKeyInfo input;

                if (Console.KeyAvailable)
                {
                    input = Console.ReadKey(true);
                    switch (input.Key)
                    {
                        case System.ConsoleKey.L:
                            {
                                _theHSM.SignalTransition("Lost", null);
                            }
                            break;
                        case System.ConsoleKey.C:
                            {
                                _theHSM.SignalTransition("Loan", null);
                            }
                            break;
                        case System.ConsoleKey.B:
                            {
                                _theHSM.SignalTransition("BoughtByLibrary", "Kenneth L. Hurley");
                            }
                            break;
                        case System.ConsoleKey.T:
                            {
                                _theHSM.SignalTransition("Late", null);
                            }
                            break;
                    }
                }       
            }

        }

        public void NotOwnedByLibraryEntry()
        {
            Console.WriteLine("NotOwnedByLibrary Entry Signal\n");
        }

        public void NotOwnedByLibraryExit()
        {
            Console.WriteLine("NotOwnedByLibrary Exit Signal\n");
        }

        public void NotOwnedByLibraryEntry_OtherOwner()
        {
            Console.WriteLine("NotOwnedByLibraryEntry_OtherOwner Entry Signal\n");
        }

        public object NotOwnedByLibrary_Bought(object data)
        {
           Console.WriteLine("NotOwnedByLibrary_Bought Transition Signal\n" + (string)data);
           return null;
        }

        public object IsLate(object dummy)
        {
            return true;
        }
    }

}
