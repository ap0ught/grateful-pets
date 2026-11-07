using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SifrahRulesDescription : IPart
{
	public string Text;

	public int PrecedingNewlines;

	public SifrahRulesDescription()
	{
	}

	public SifrahRulesDescription(string Text)
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
		if (Options.AnySifrah && !GameText.VariableReplace(Text, ParentObject, The.Player).IsNullOrEmpty())
		{
			for (int i = 0; i < PrecedingNewlines; i++)
			{
				E.Postfix.Compound("", "\n");
			}
			E.Postfix.AppendRules(Text);
		}
		return base.HandleEvent(E);
	}
}
