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
	class GameObject
	{
		public string Name;

		public GameObject(string inName)
		{
			Name = inName;
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
            TestZombieXML testZombieXML;

            testZombieXML = new TestZombieXML();
			
			//testZombieXML.WriteStateMachine("testzombie-write.xml");
			testZombieXML.Run();
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

		private GameObject playerWaitingFor;

        public TestZombieXML()
        {
            GQHSMManager manager = GQHSMManager.Instance;

            _theHSM = manager.LoadFromXML("ZombieAI.xml");
            // let's hook into the debugging

            _mLogger.InitLogger(_theHSM);

            _theHSM.Init(this);


        }

        public bool RandomRemain(int modulo)
        {
            return (bool)((_randObj.Next() % modulo) != 0);

        }

        public bool IsEndOfPath()
        {
            return _endOfPath;
        }

        public bool PathToPlayer()
        {
            return _pathToPlayer;
        }

        public bool IsBehindDoor()
        {
            return _isBehindDoor;
        }

        public void OnClimbingOut()
        {
            _theHSM.SignalTransition("Trigger.ClimbedOut", null);
        }

		public void OnCaughtPlayer(GameObject other)
		{

			Console.WriteLine("CaughtPlayer\n");
			_theHSM.DoTransitionAsync("Idle");

		}

		public bool PlayerCatchable(GameObject other)
		{
			if (other != null)
				Console.WriteLine("Testing Player Catchable\n");

			return true;
		}

		public void SetWaitingForPlayer(GameObject other)
		{
			if (other != null)
			{
				Console.WriteLine("Setting Waiting for {0}\n", other.Name);
			}

			playerWaitingFor = other;
		}

		public void UnsetWaitingForPlayer()
		{
			if (playerWaitingFor != null)
			{
				Console.WriteLine("Unsetting Waiting for Player {0}\n", playerWaitingFor.Name);
			}
			playerWaitingFor = null;
		}


        public void Run()
        {
            GQHSMManager manager = GQHSMManager.Instance;
			bool bIsInPlayer = false;

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
                                _theHSM.SignalTransition("Trigger.Falling", null);
                            }
                            break;
                        case System.ConsoleKey.G:
                            {
                                _theHSM.SignalTransition("Trigger.OnGround", null);
                            }
                            break;
                        case System.ConsoleKey.H:
                            {
                                _theHSM.SignalTransition("Trigger.FallThroughEnter.Enter", null);
                            }
                            break;

                        case System.ConsoleKey.P:
                            {
								string st = "Trigger.Player.Enter";
								if (bIsInPlayer)
									st = "Trigger.Player.Exit";
								bIsInPlayer = !bIsInPlayer;
                                _theHSM.SignalTransition(st, new GameObject("Player"));
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
								if (_endOfPath)
								{
									_theHSM.SignalTransition("EndOfPath", null);
								}

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
