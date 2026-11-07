using System;
using UnityEngine;
using fsm;

public class AniState : State
{
	public Animation anim;

	public string curAniName;

	public float normalizedTime
	{
		get
		{
			AnimationState animationState = anim[curAniName];
			if (!(animationState != null))
			{
				return 0f;
			}
			return animationState.normalizedTime;
		}
	}

	public AniState(string _name, Animation _anim, State _parent = null)
		: base(_name, _parent)
	{
		anim = _anim;
		if (anim == null)
		{
			Debug.LogError("");
		}
		onFadeIn = (Action<Transition>)Delegate.Combine(onFadeIn, new Action<Transition>(Play));
	}

	public AniTransition to(AniState _targetState, float _duration)
	{
		AniTransition aniTransition = new AniTransition
		{
			source = this,
			target = _targetState,
			duration = _duration
		};
		transitionList.Add(aniTransition);
		return aniTransition;
	}

	public virtual void Play(Transition transition)
	{
		DoPlay(transition, name);
	}

	protected void DoPlay(Transition transition, string animName)
	{
		if (string.IsNullOrEmpty(animName))
		{
			anim.Stop();
			curAniName = null;
			return;
		}
		if (transition == null || ((TimerTransition)transition).duration == 0f)
		{
			anim.Play(animName);
		}
		else
		{
			anim.CrossFade(animName, ((TimerTransition)transition).duration);
		}
		curAniName = animName;
	}
}
