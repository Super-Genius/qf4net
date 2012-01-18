using System;
using System.Collections.Generic;
using qf4net;
using MurphyPA.Logging;


public class MyLogger
{
	private ILogger _logger;

	public void InitLogger(GQHSM theHsm)
	{
		_logger = theHsm.Logger;
		theHsm.Instrument = true;
		theHsm.StateChange += new EventHandler(hsm_StateChange);
		theHsm.DispatchException += new DispatchExceptionHandler(hsm_DispatchException);
		theHsm.UnhandledTransition += new DispatchUnhandledTransitionHandler(hsm_UnhandledTransition);

	}

	private void hsm_StateChange(object sender, EventArgs e)
	{
		LogStateEventArgs args = e as LogStateEventArgs;
		GQHSM hsm = (GQHSM)sender;

		switch (args.LogType)
		{
			case StateLogType.Init:
				{
					_logger.Debug("[{0}{1}] {2} to {3}", hsm.GetName(), args.LogType.ToString(), args.State, args.NextState.Name);
				}
				break;

			case StateLogType.Entry:
				{
					_logger.Debug("[{0}{1}] {2}", hsm.GetName(), args.LogType.ToString(), args.State.Name);
				}
				break;

			case StateLogType.Exit:
				{
					_logger.Debug("[{0}{1}] {2}", hsm.GetName(), args.LogType.ToString(), args.State.Name);
				}
				break;

			case StateLogType.EventTransition:
				{
					_logger.Debug("[{0}{1}] {2} on {3} to {4} -> {5}", hsm.GetName(), args.LogType.ToString(), args.State.Name, args.EventName, args.NextState.Name, args.EventDescription);
				}
				break;

			case StateLogType.Log:
				{
					_logger.Debug("[{0}{1}] {2}", hsm.GetName(), args.LogType.ToString(), args.LogText);
				}
				break;

			default:
				throw new NotSupportedException("StateLogType." + args.LogType.ToString());
		}
	}

	private void hsm_DispatchException(Exception ex, qf4net.IQHsm hsm, QState state, qf4net.IQEvent ev)
	{
		_logger.Debug("[{0}.{1}] Exception: on event {2}\n{3}", ((GQHSM)(hsm)).GetName(), state.Name, ev.ToString(), ex.ToString());
	}

	private void hsm_UnhandledTransition(qf4net.IQHsm hsm, QState state, qf4net.IQEvent ev)
	{
		_logger.Debug("[{0}.{1}] Unhandled Event: {2}", ((GQHSM)(hsm)).GetName(), state.Name, ev.ToString());
	}
}

