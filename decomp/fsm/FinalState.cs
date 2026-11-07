using System;

namespace fsm;

public class FinalState : State
{
	public FinalState(string _name, State _parent = null)
		: base(_name, _parent)
	{
		onEnter = (Action<State, State>)Delegate.Combine(onEnter, new Action<State, State>(OnFinished));
	}

	private void OnFinished(State _from, State _to)
	{
	}
}
