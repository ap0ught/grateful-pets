using System;

namespace XRL.World.Parts;

[Serializable]
public class CheckPatternSpawner : IPart
{
	public string ObjectBaseName = "Coral Polyp";

	public string WhiteVariant = "A";

	public string BlackVariant = "B";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		return ID == EnteredCellEvent.ID;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		_ = WhiteVariant;
		if ((E.Cell.X + E.Cell.Y) % 2 == 0)
		{
			E.Cell.AddObject(ObjectBaseName + " " + BlackVariant);
		}
		else
		{
			E.Cell.AddObject(ObjectBaseName + " " + WhiteVariant);
		}
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}
}
