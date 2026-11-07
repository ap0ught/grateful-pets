using System;
using System.Collections.Generic;
using UnityEngine;

namespace fsm;

public class State
{
	public enum Mode
	{
		Exclusive,
		Parallel
	}

	public string name = "";

	public Mode mode;

	protected State parent_;

	protected Machine machine_;

	protected State initState_;

	protected List<Transition> transitionList = new List<Transition>();

	protected List<State> children = new List<State>();

	protected bool inTransition;

	protected Transition currentTransition;

	protected List<State> currentStates = new List<State>();

	public Action<Transition> onFadeIn;

	public Action<State, State> onEnter;

	public Action<State, State> onExit;

	public Action<State> onAction;

	public State parent
	{
		get
		{
			return parent_;
		}
		set
		{
			if (parent_ == value)
			{
				return;
			}
			State state = parent_;
			while (parent_ != null)
			{
				if (parent_ == this)
				{
					Debug.LogWarning("can't add self or child as parent");
					return;
				}
				parent_ = parent_.parent;
			}
			if (state != null)
			{
				if (state.initState == this)
				{
					state.initState = null;
				}
				state.children.Remove(this);
			}
			if (value != null)
			{
				value.children.Add(this);
				if (value.children.Count == 1)
				{
					value.initState = this;
				}
			}
			parent_ = value;
		}
	}

	public Machine machine
	{
		get
		{
			if (machine_ != null)
			{
				return machine_;
			}
			State state = this;
			for (State state2 = parent; state2 != null; state2 = state2.parent)
			{
				state = state2;
			}
			machine_ = state as Machine;
			return machine_;
		}
	}

	public State initState
	{
		get
		{
			return initState_;
		}
		set
		{
			if (initState_ != value)
			{
				if (value != null && children.IndexOf(value) == -1)
				{
					Debug.LogError("FSM error: You must use child state as initial state.");
					initState_ = null;
				}
				else
				{
					initState_ = value;
				}
			}
		}
	}

	public State(string _name, State _parent = null)
	{
		name = _name;
		parent = _parent;
	}

	public void ClearCurrentStatesRecursively()
	{
		currentStates.Clear();
		for (int i = 0; i < children.Count; i++)
		{
			children[i].ClearCurrentStatesRecursively();
		}
	}

	public T Add<T>(State _targetState, Func<bool> _onCheck = null, Action _onStart = null, Func<bool> _onTransition = null, Action _onEnd = null) where T : Transition, new()
	{
		T val = new T
		{
			source = this,
			target = _targetState
		};
		if (_onCheck != null)
		{
			val.onCheck = _onCheck;
		}
		if (_onStart != null)
		{
			val.onStart = _onStart;
		}
		if (_onTransition != null)
		{
			val.onTransition = _onTransition;
		}
		if (_onEnd != null)
		{
			val.onEnd = _onEnd;
		}
		transitionList.Add(val);
		return val;
	}

	public void OnAction()
	{
		if (onAction != null)
		{
			onAction(this);
		}
		for (int i = 0; i < currentStates.Count; i++)
		{
			currentStates[i].OnAction();
		}
	}

	public void CheckConditions()
	{
		if (inTransition)
		{
			return;
		}
		for (int i = 0; i < currentStates.Count; i++)
		{
			State state = currentStates[i];
			for (int j = 0; j < state.transitionList.Count; j++)
			{
				Transition transition = state.transitionList[j];
				if (transition.onCheck())
				{
					transition.source.parent.ExitStates(transition.target, transition.source);
					if (transition.onStart != null)
					{
						transition.onStart();
					}
					if (transition.target.onFadeIn != null)
					{
						transition.target.onFadeIn(transition);
					}
					currentTransition = transition;
					inTransition = true;
					break;
				}
			}
			if (!inTransition)
			{
				state.CheckConditions();
			}
		}
	}

	public void UpdateTransitions()
	{
		if (inTransition)
		{
			if (currentTransition.onTransition())
			{
				if (currentTransition.onEnd != null)
				{
					currentTransition.onEnd();
				}
				State state = currentTransition.target;
				if (state == null)
				{
					state = currentTransition.source;
				}
				if (state.parent != null)
				{
					state.parent.EnterStates(state, currentTransition.source);
				}
				else
				{
					Debug.Log("targetState = " + state.name + ", " + name);
				}
				currentTransition = null;
				inTransition = false;
			}
		}
		else
		{
			for (int i = 0; i < currentStates.Count; i++)
			{
				currentStates[i].UpdateTransitions();
			}
		}
	}

	public void EnterStates(State _toEnter, State _toExit)
	{
		currentStates.Add(_toEnter);
		if (machine != null && machine.logDebugInfo)
		{
			Debug.Log("FSM Debug: Enter State - " + _toEnter.name + " at " + Time.time);
		}
		if (_toEnter.onEnter != null)
		{
			_toEnter.onEnter(_toExit, _toEnter);
		}
		if (_toEnter.children.Count == 0)
		{
			return;
		}
		if (_toEnter.mode == Mode.Exclusive)
		{
			if (_toEnter.initState != null)
			{
				_toEnter.EnterStates(_toEnter.initState, _toExit);
			}
			else
			{
				Debug.LogError("FSM error: can't find initial state in " + _toEnter.name);
			}
		}
		else
		{
			for (int i = 0; i < _toEnter.children.Count; i++)
			{
				_toEnter.EnterStates(_toEnter.children[i], _toExit);
			}
		}
	}

	public void ExitStates(State _toEnter, State _toExit)
	{
		_toExit.ExitAllStates(_toEnter);
		if (machine != null && machine.logDebugInfo)
		{
			Debug.Log("FSM Debug: Exit State - " + _toExit.name + " at " + Time.time);
		}
		if (_toExit.onExit != null)
		{
			_toExit.onExit(_toExit, _toEnter);
		}
		currentStates.Remove(_toExit);
	}

	public bool IsInChildState(State state, bool containsTransTarget = true)
	{
		if ((inTransition & containsTransTarget) && currentTransition.target == state)
		{
			return true;
		}
		for (int i = 0; i < currentStates.Count; i++)
		{
			State state2 = currentStates[i];
			if (state2 == state)
			{
				return true;
			}
			if (state2.IsInChildState(state, containsTransTarget))
			{
				return true;
			}
		}
		return false;
	}

	protected void ExitAllStates(State _toEnter)
	{
		for (int i = 0; i < currentStates.Count; i++)
		{
			State state = currentStates[i];
			state.ExitAllStates(_toEnter);
			if (state.onExit != null)
			{
				state.onExit(state, _toEnter);
			}
			if (machine != null && machine.logDebugInfo)
			{
				Debug.Log("FSM Debug: Exit State - " + state.name + " at " + Time.time);
			}
		}
		currentStates.Clear();
	}

	public int TotalStates()
	{
		int num = 1;
		for (int i = 0; i < children.Count; i++)
		{
			num += children[i].TotalStates();
		}
		return num;
	}
}
