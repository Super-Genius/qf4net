using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using qf4net;
using MurphyPA.Logging;
using System.Collections;
using System.Collections.Generic;

namespace TestXMLLoader
{
	class MainClass
	{
		public static void Main (string[] args)
		{
#if false
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

    class TestZombieXML
    {
        private GQHSM _theHSM;
        private bool _endOfPath = false;
        private bool _isBehindDoor = false;
        private bool _pathToPlayer = false;
        private Random _randObj = new Random();
        private MyLogger _mLogger = new MyLogger();

        public TestZombieXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM = manager.LoadFromXML("ZombieAI.xml");
            // let's hook into the debugging

            _mLogger.InitLogger(_theHSM);

            _theHSM.RegisterActionHandler(new GQHSMHandler(this, RandomRemain));
            _theHSM.RegisterActionHandler(new GQHSMHandler(this, IsEndOfPath));
            _theHSM.RegisterActionHandler(new GQHSMHandler(this, PathToPlayer));
            _theHSM.RegisterActionHandler(new GQHSMHandler(this, OnClimbingOut));

            _theHSM.Init();


        }

        public object RandomRemain(object modulo)
        {
            return (bool)((_randObj.Next() % (int)modulo) != 0);

        }

        public object IsEndOfPath()
        {
            return _endOfPath;
        }

        public object PathToPlayer()
        {
            return _pathToPlayer;
        }

        public object IsBehindDoor()
        {
            return _isBehindDoor;
        }

        public object OnClimbingOut()
        {
            _theHSM.SignalTransition("ClimbedOut", null);
            return null;
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

    class TestBookXML
    { 
        GQHSM _theHSM;

        public TestBookXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM = manager.LoadFromXML("book.xml");
            _theHSM.Init(this);
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

        public object NotOwnedByLibraryEntry()
        {
            Console.WriteLine("NotOwnedByLibrary Entry Signal\n");
            return null;
        }

        public object NotOwnedByLibraryExit()
        {
            Console.WriteLine("NotOwnedByLibrary Exit Signal\n");
            return null;
        }

        public object NotOwnedByLibraryEntry_OtherOwner()
        {
            Console.WriteLine("NotOwnedByLibraryEntry_OtherOwner Entry Signal\n");
            return null;
        }

        public object NotOwnedByLibrary_Bought(object data)
        {
           Console.WriteLine("NotOwnedByLibrary_Bought Transition Signal\n" + (string)data);
           return null;
        }

        public object IsLate()
        {
            return true;
        }
    }

}
