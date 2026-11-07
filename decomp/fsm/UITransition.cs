using System;

namespace fsm;

[Serializable]
public class UITransition : TimerTransition
{
	public exUIPanel from;

	public exUIPanel to;

	public UITransition()
	{
		onStart = (Action)Delegate.Combine(onStart, (Action)(() =>
		{
			exUIMng.inst.enabled = false;
			from.StartFadeOut();
			to.StartFadeIn();
		}));
		onEnd = (Action)Delegate.Combine(onEnd, (Action)(() =>
		{
			exUIMng.inst.enabled = true;
			from.FinishFadeOut();
			to.FinishFadeIn();
		}));
		onTick = (Action<float>)Delegate.Combine(onTick, (Action<float>)((float _ratio) =>
		{
			from.FadeOut(_ratio);
			to.FadeIn(_ratio);
		}));
	}
}
