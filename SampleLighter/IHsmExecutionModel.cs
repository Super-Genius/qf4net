using System;
using qf4net;

namespace Samples.Lighter
{
	/// <summary>
	/// IHsmExecutionModel.
	/// </summary>
	public interface IHsmExecutionModel
	{
	    LighterFrame CreateHsm (string id);
	}
}
