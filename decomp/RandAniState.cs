using System;
using UnityEngine;
using fsm;

public class RandAniState : AniState
{
	public string[] animList;

	public RandAniState(string _name, Animation _anim, string _animList, State _parent = null)
		: base(_name, _anim, _parent)
	{
		animList = _animList.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
	}

	public override void Play(Transition transition)
	{
		string animName = animList[UnityEngine.Random.Range(0, animList.Length)];
		DoPlay(transition, animName);
	}
}
