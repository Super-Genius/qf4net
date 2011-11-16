using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using qf4net;

namespace TestXMLLoader
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            GQHSMManager manager = GQHSMManager.Instance;

            manager.RegisterActionHandler(new StateChangeHandler(null, NotOwnedByLibraryEntry), "book", "NotOwnedByLibrary", QSignals.Entry);
            manager.RegisterActionHandler(new StateChangeHandler(null, NotOwnedByLibraryExit), "book", "NotOwnedByLibrary", QSignals.Exit);
            manager.RegisterActionHandler(new StateChangeHandler(null, NotOwnedByLibraryEntry_OtherOwner), "book", "NotOwnedByLibrary::OtherOwner", QSignals.Entry);
            manager.RegisterTransitionHandler(new TransitionChangeHandler(null, NotOwnedByLibrary_Bought), "book", "NotOwnedByLibrary::Bought");
            manager.RegisterGuardHandler(new GuardHandler(null, IsLate), "book", "OwnedByLibrary::OnLoan::IsLate()");

            GLQHSM theHSM = manager.LoadFromXML("book.xml", true);

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
                                theHSM.SignalTransition("Lost", null);
                            }
                            break;
                        case System.ConsoleKey.C:
                            {
                                theHSM.SignalTransition("Loan", null);
                            }
                            break;
                        case System.ConsoleKey.B:
                            {
                                theHSM.SignalTransition("BoughtByLibrary", "Kenneth L. Hurley");
                            }
                            break;
                        case System.ConsoleKey.T:
                            {
                                theHSM.SignalTransition("Late", null);
                            }
                            break;
                    }
                }       
            }
		}

        public static void NotOwnedByLibraryEntry()
        {
            Console.WriteLine("NotOwnedByLibrary Entry Signal\n");
        }

        public static void NotOwnedByLibraryExit()
        {
            Console.WriteLine("NotOwnedByLibrary Entry Signal\n");
        }

        public static void NotOwnedByLibraryEntry_OtherOwner()
        {
            Console.WriteLine("NotOwnedByLibraryEntry_OtherOwner Entry Signal\n");
        }

        public static void NotOwnedByLibrary_Bought(object data)
        {
            Console.WriteLine("NotOwnedByLibrary_Bought Transition Signal\n"+ (string)data);
        }

        public static bool IsLate()
        {

            return true;

        }
    }
}
