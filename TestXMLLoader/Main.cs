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
            TestZombineXML testZombieXML;

            testZombieXML = new TestZombineXML();

            testZombieXML.Run();
        }
    }

    class TestZombineXML
    {
        private GLQHSM _theHSM;
        private ILogger _logger;
        private bool _endOfPath = false;
        private bool _isBehindDoor = false;
        private bool _pathToPlayer = false;
        private Random _randObj = new Random();

        public TestZombineXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM = manager.LoadFromXML("ZombieAI.xml", false);
            // let's hook into the debugging

            LQHsm _hsm = _theHSM.GetHSM();

            _logger = _hsm.Logger;
			_hsm.StateChange += new EventHandler(hsm_StateChange);
			_hsm.DispatchException += new DispatchExceptionHandler(hsm_DispatchException);
			_hsm.UnhandledTransition += new DispatchUnhandledTransitionHandler(hsm_UnhandledTransition);

            manager.RegisterGuardHandler(new GQHSMHandler(this, RandomRemain), "ZombieAI", "RandomRemain(5)");
            manager.RegisterGuardHandler(new GQHSMHandler(this, IsEndOfPath), "ZombieAI", "IsEndOfPath()");
            manager.RegisterGuardHandler(new GQHSMHandler(this, PathToPlayer), "ZombieAI", "PathToPlayer()");
            manager.RegisterGuardHandler(new GQHSMHandler(this, IsBehindDoor), "ZombieAI", "IsBehindDoor()");

            manager.RegisterActionHandler(new GQHSMHandler(this, OnClimbingOut), "ZombieAI", "OnClimbingOut", "ENTRY");
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
                                _theHSM.SignalTransition("Collision.Ladder", null);
                            }
                            break;
                        case System.ConsoleKey.F:
                            {
                                _theHSM.SignalTransition("Falling", null);
                            }
                            break;
                        case System.ConsoleKey.B:
                            {
                                _theHSM.SignalTransition("Collision.Block", null);
                            }
                            break;
                        case System.ConsoleKey.H:
                            {
                                _theHSM.SignalTransition("Collision.Hole", null);
                            }
                            break;

                        case System.ConsoleKey.P:
                            {
                                _theHSM.SignalTransition("Collision.Player", null);
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

                        case System.ConsoleKey.O:
                            {
                                _theHSM.SignalTransition("DoorOpened", null);
                            }
                            break;
                    }
                }
            }

        }

        private void hsm_StateChange(object sender, EventArgs e)
        {
            LogStateEventArgs args = e as LogStateEventArgs;
            ILQHsm hsm = sender as ILQHsm;

            switch (args.LogType)
            {
                case StateLogType.Init:
                    {
                        _logger.Debug("[{0}{1}] {2} to {3}", hsm.ToString(), args.LogType.ToString(), args.State, args.NextState.Name);
                    }
                    break;

                case StateLogType.Entry:
                    {
                        _logger.Debug("[{0}{1}] {2}", hsm.ToString(), args.LogType.ToString(), args.State.Name);
                    }
                    break;

                case StateLogType.Exit:
                    {
                        _logger.Debug("[{0}{1}] {2}", hsm.ToString(), args.LogType.ToString(), args.State.Name);
                    }
                    break;

                case StateLogType.EventTransition:
                    {
                        _logger.Debug("[{0}{1}] {2} on {3} to {4} -> {5}", hsm.ToString(), args.LogType.ToString(), args.State.Name, args.EventName, args.NextState.Name, args.EventDescription);
                    }
                    break;

                case StateLogType.Log:
                    {
                        _logger.Debug("[{0}{1}] {2}", hsm.ToString(), args.LogType.ToString(), args.LogText);
                    }
                    break;

                default:
                    throw new NotSupportedException("StateLogType." + args.LogType.ToString());
            }
        }

        private void hsm_DispatchException(Exception ex, qf4net.IQHsm hsm, QState state, qf4net.IQEvent ev)
		{
            _logger.Debug("[{0}] Exception: on event {1}\n{2}", hsm.ToString(), ev.ToString(), ex.ToString());
		}

		private void hsm_UnhandledTransition(qf4net.IQHsm hsm, QState state, qf4net.IQEvent ev)
		{
            _logger.Debug("[{0}] Unhandled Event: {1}", hsm.ToString(), ev.ToString());
		}

    }

    class TestBookXML
    { 
        GLQHSM _theHSM;

        public TestBookXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            manager.RegisterActionHandler(new GQHSMHandler(null, NotOwnedByLibraryEntry), "book", "NotOwnedByLibrary", QSignals.Entry);
            manager.RegisterActionHandler(new GQHSMHandler(null, NotOwnedByLibraryExit), "book", "NotOwnedByLibrary", QSignals.Exit);
            manager.RegisterActionHandler(new GQHSMHandler(null, NotOwnedByLibraryEntry_OtherOwner), "book", "NotOwnedByLibrary.OtherOwner", QSignals.Entry);
            manager.RegisterTransitionHandler(new GQHSMHandler(null, NotOwnedByLibrary_Bought), "book", "NotOwnedByLibrary.Bought");
            manager.RegisterGuardHandler(new GQHSMHandler(null, IsLate), "book", "OwnedByLibrary.OnLoan.IsLate()");

            _theHSM = manager.LoadFromXML("book.xml", true);
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
