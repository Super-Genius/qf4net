/// <summary>
/// GQHSMstate Implementation file.
/// </summary>
/// <remarks>
/// Copyright (c) 2011 Sony Ericsson
/// Author: Kenneth Hurley
/// </remarks>

using System;
using System.Runtime.Serialization;
using qf4net;

namespace qf4net
{
	public class GQHSMState
	{
		/// <summary>
		/// The parent of this state for hierarchy
		/// </summary> 
		private GHQSMState parent;
		/// <summary>
		/// The previous state at this level (sibling)
		/// </summary>
		private GQHSMState previous;
		/// <summary>
		/// The next state at this level (sibling)
		/// </summary>
		private GQHSMState next;
		
		/// <summary>
		/// The name of the State
		/// </summary>
		[GHQSMXMLData("Name")]
		private string Name;
		
		/// <summary>
		/// The Guid of this state, good for better comparison
		/// </summary>
		[GHQSMXMLData("Id")]
		private Guid ID;
		
		/// <summary>
		/// The entry action that gets executed when the state is entered
		/// </summary> 
		[GHQSMXMLData("EntryAction")]
		private string EntryAction;
		
		/// <summary>
		/// The exit action that gets executed when the state is exited
		/// </summary>
		[GHQSMXMLData("ExitAction")]
		private string ExitAction;
		
		/// <summary>
		/// The state commands that get executed from transitions
		/// </summary>
		private string[] StateCommands;
		
		/// <summary>
		/// Any note for the state
		/// </summary>
		private string Note;
	
}
