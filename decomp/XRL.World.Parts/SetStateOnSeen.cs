using System;

namespace XRL.World.Parts;

[Serializable]
public class SetStateOnSeen : IPart
{
	public string State;

	[NonSerialized]
	private bool Activated;

	public override bool Render(RenderEvent E)
	{
		if (!Activated)
		{
			if (!State.IsNullOrEmpty())
			{
				The.Game.SetBooleanGameState(State, Value: true);
			}
			Activated = true;
		}
		return true;
	}
}
