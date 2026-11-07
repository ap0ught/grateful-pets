namespace XRL.World.Parts;

public abstract class BasePrefabImposter : IPart
{
	public string Prefab;

	public string Config;

	public bool Disabled;

	public int Layer;

	public int X;

	public int Y;

	public int Z;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<GetDebugInternalsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Prefab", Prefab);
		E.AddEntry(this, "Disabled", Disabled);
		return base.HandleEvent(E);
	}
}
