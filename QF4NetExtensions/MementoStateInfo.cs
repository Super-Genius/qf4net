using System;
using System.Reflection;

namespace qf4net
{
	/// <summary>
	/// MementoStateInfo.
	/// </summary>
	[Serializable]
	public class MementoStateInfo : IStateInfo
	{
		public MementoStateInfo(string name, QState state)
		{
			_Name = name;
			_State = state;
		}

		string _Name;
		public string Name { get { return _Name; } }

		QState _State;
		public QState State { get { return _State; } }
	}
}
