using System;

namespace qf4net
{
	/// <summary>
	/// Summary description for DispatchExceptionFailureEventArgs.
	/// </summary>
	[Serializable]
	public class DispatchExceptionFailureEventArgs
	{
		public DispatchExceptionFailureEventArgs(Exception ex, IQHsm hsm, QState state, IQEvent ev)
		{
			_Exception = ex;
			_Hsm = hsm;
			_State = state;
			_OriginalEvent = ev;
		}

		Exception _Exception;
		public Exception Exception { get { return _Exception; } }

		IQHsm _Hsm;
		public IQHsm Hsm { get { return _Hsm; } }

		QState _State;
		public QState state { get { return _State; } }

		IQEvent _OriginalEvent;
		public IQEvent OriginalEvent { get { return _OriginalEvent; } }
	}
}
