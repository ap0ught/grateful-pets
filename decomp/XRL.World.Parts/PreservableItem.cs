using System;

namespace XRL.World.Parts;

[Serializable]
public class PreservableItem : IPart
{
	public string Result;

	public int Number;

	public override bool SameAs(IPart p)
	{
		PreservableItem preservableItem = p as PreservableItem;
		if (preservableItem.Result != Result)
		{
			return false;
		}
		if (preservableItem.Number != Number)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
