using System;
using System.Reflection;

namespace qf4net
{
	/// <summary>
	/// IStateInfo.
	/// </summary>
	public interface IStateInfo
	{
		string Name { get; }
		QState State { get; }
	}
}
