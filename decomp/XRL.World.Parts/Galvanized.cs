using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class Galvanized : IPart
{
	public bool Inventory = true;

	public bool Equipment = true;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GetRustableContents");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetRustableContents")
		{
			if (Inventory)
			{
				E.SetParameter("Inventory", 0);
			}
			if (Equipment)
			{
				E.SetParameter("Equipment", 0);
			}
		}
		return base.FireEvent(E);
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
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		if (Inventory)
		{
			SB.Append("Inventory ");
			if (Equipment)
			{
				SB.Append("and equipment ");
			}
		}
		else
		{
			SB.Append("Equipment ");
		}
		SB.Append("immune to rusting");
	}
}
