using System;

namespace XRL.World.Parts;

[Serializable]
public class OriginalItemType : IPart
{
	public string DescriptionInject;

	public override void Initialize()
	{
		base.Initialize();
		DescriptionInject = ParentObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: true, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true);
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
		E.Postfix.Insert(0, "\nThis item is a named " + DescriptionInject + ".\n");
		return base.HandleEvent(E);
	}
}
