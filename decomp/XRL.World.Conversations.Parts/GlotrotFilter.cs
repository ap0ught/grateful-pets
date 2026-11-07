using XRL.Rules;

namespace XRL.World.Conversations.Parts;

public class GlotrotFilter : IConversationPart
{
	public GlotrotFilter()
	{
		Priority = -2000;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != PrepareTextEvent.ID && ID != GetTargetElementEvent.ID)
		{
			return ID == IsElementVisibleEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Text.Clear();
		E.Text.Append((Stat.RandomCosmetic(1, 100) <= 70) ? "N" : "G");
		E.Text.Append('n', Stat.RandomCosmetic(3, 11));
		E.Text.Append('.');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTargetElementEvent E)
	{
		E.Target = "End";
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		if (E.Element is Choice choice)
		{
			if (choice.HasPart<Trade>())
			{
				return base.HandleEvent(E);
			}
			if (choice.Target != "End")
			{
				return false;
			}
			choice.Parts?.ForEach((IConversationPart x) =>
			{
				x.Propagation = 0;
			});
			choice.Actions?.Clear();
		}
		return base.HandleEvent(E);
	}
}
