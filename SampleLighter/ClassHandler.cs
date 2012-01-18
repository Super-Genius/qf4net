using System;
using qf4net;
using System.IO;

public class ClassHandler
{
	private GQHSM _theHSM = null;
	private GQHSMManager _manager = GQHSMManager.Instance;

	public GQHSM HSM
	{
		get
		{
			return _theHSM;
		}

		set
		{
			_theHSM = value;
		}
	}

	public string CurrentStateName
	{
		get
		{
			return _theHSM.CurrentStateName;
		}
	}

	public ClassHandler(string Name, MyLogger logger)
	{
		_theHSM = _manager.LoadFromXML("XMLStateMachines" + Path.DirectorySeparatorChar + Name + ".xml");
		// attach the logger to this HSM.
		if (_theHSM != null)
		{
			logger.InitLogger(_theHSM);
		}

	}

	public virtual void Init()
	{
		if (_theHSM != null)
		{
			// init the HSM with this class to expose public properties
			_theHSM.Init(this);
		}

	}
}
