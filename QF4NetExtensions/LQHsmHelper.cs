using System;

namespace qf4net
{
	/// <summary>
	/// LQHsmHelper.
	/// </summary>
	public class LQHsmHelper
	{
        public static void FillMementoWithStateName (ILQHsmMemento memento, ILQHsm hsm, string currentStateName)
        {
            memento.CurrentState.Name = currentStateName;
        }
    }
}
