using System;
using UnityEngine;
using fsm;

public class FSMBase : MonoBehaviour
{
	[NonSerialized]
	public Machine stateMachine;

	public virtual void Init()
	{
		if (stateMachine == null)
		{
			stateMachine = new Machine();
		}
	}

	public void StartFSM()
	{
		if (stateMachine != null)
		{
			stateMachine.Start();
		}
	}

	public void RestartFSM()
	{
		if (stateMachine != null)
		{
			stateMachine.Restart();
		}
	}

	public virtual void Tick()
	{
		if (stateMachine != null)
		{
			stateMachine.Tick();
		}
	}
}
