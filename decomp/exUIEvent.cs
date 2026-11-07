public class exUIEvent
{
	public bool bubbles = true;

	public bool cancelable = true;

	public exUIControl target;

	public exUIControl currentTarget;

	public exUIEventPhase eventPhase = exUIEventPhase.Target;

	private bool isPropagationStopped_;

	public bool isPropagationStopped => isPropagationStopped_;

	public void StopPropagation()
	{
		isPropagationStopped_ = true;
	}

	public void Reset()
	{
		isPropagationStopped_ = false;
		target = null;
		currentTarget = null;
	}
}
