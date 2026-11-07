using System;

namespace XRL.World.Parts;

[Serializable]
public class RulesDescription : IPart
{
	public string Text;

	public string AltForGenotype;

	public string GenotypeAlt;

	public int PrecedingNewlines;

	public RulesDescription()
	{
	}

	public RulesDescription(string Text)
		: this()
	{
		this.Text = Text;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		string text = ((!AltForGenotype.IsNullOrEmpty() && IComponent<GameObject>.ThePlayer.GetGenotype() == AltForGenotype) ? GenotypeAlt : Text);
		if (!text.IsNullOrEmpty())
		{
			for (int i = 0; i < PrecedingNewlines; i++)
			{
				E.Postfix.Compound("", "\n");
			}
			E.Postfix.AppendRules(text);
		}
		return base.HandleEvent(E);
	}
}
