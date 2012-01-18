using System;
using qf4net;

namespace Samples.Lighter
{
	/// <summary>
	/// MultipleHsmsPerThread.
	/// </summary>
    public class MultipleHsmsPerThread : IHsmExecutionModel
    {
        IQEventManager _EventManager;
        
	    public MultipleHsmsPerThread()
	    {
	        InitHsmRunner ();   
	    }
	    
        private void InitHsmRunner()
        {
			_EventManager = GQHSMManager.Instance.EventManager;
            IQEventManagerRunner runner = new QThreadedEventManagerRunner ("ThrShared", _EventManager);	 
            runner.Start ();
        }

        public LighterFrame CreateHsm(string id)
        {
            LighterFrame ligherFrame
                = new LighterFrame ();
            return ligherFrame;
        }

    }
}
