using System;

namespace XRL.World.Parts;

[Serializable]
[WantLoadBlueprint]
public class HasMakersMark : IPart
{
	public string Mark;

	public string Color;

	public static void LoadBlueprint(GameObjectBlueprint Blueprint)
	{
		if (Blueprint.TryGetPartParameter<string>("HasMakersMark", "Mark", out var Result) && !Result.IsNullOrEmpty())
		{
			MakersMark.RecordUsage(Result);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		Mark = MakersMark.Generate();
		Color = Crayons.GetRandomColor();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
