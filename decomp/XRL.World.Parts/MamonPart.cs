using System;

namespace XRL.World.Parts;

[Serializable]
public class MamonPart : IPart
{
	[NonSerialized]
	private bool Rendered;

	public override bool Render(RenderEvent E)
	{
		if (!Rendered)
		{
			Rendered = true;
			The.Game.FinishQuestStep("Raising Indrix", "Find Mamon Souldrinker");
		}
		return true;
	}
}
