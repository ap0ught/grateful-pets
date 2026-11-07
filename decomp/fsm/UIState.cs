using System;

namespace fsm;

[Serializable]
public class UIState : State
{
	public exUIPanel panel;

	public UIState(exUIPanel _panel, State _parent = null)
		: base(_panel.name, _parent)
	{
		panel = _panel;
		onEnter = (Action<State, State>)Delegate.Combine(onEnter, (Action<State, State>)((State _from, State _to) =>
		{
			panel.Enter();
		}));
		onExit = (Action<State, State>)Delegate.Combine(onExit, (Action<State, State>)((State _from, State _to) =>
		{
			panel.Exit();
		}));
	}

	public void to(UIState _targetState, Func<bool> _onCheck, float _duration)
	{
		UITransition uITransition = new UITransition();
		uITransition.source = this;
		uITransition.target = _targetState;
		if (_onCheck != null)
		{
			uITransition.onCheck = _onCheck;
		}
		uITransition.duration = _duration;
		uITransition.from = panel;
		uITransition.to = _targetState.panel;
		transitionList.Add(uITransition);
	}
}
