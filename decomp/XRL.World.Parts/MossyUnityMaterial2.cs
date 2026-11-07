namespace XRL.World.Parts;

public class MossyUnityMaterial2 : PrefabImposter
{
	public MossyUnityMaterial2()
	{
		Prefab = "Prefabs/Imposters/Mossy";
		Z = -1;
	}

	public override void Initialize()
	{
		Config = ParentObject?.Render.Tile + "~" + ParentObject?.ID;
	}
}
