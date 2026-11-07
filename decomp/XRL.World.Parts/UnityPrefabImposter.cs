using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete]
public class UnityPrefabImposter : IPartWithPrefabImposter
{
	public string PrefabID;

	public override bool SameAs(IPart p)
	{
		if ((p as UnityPrefabImposter).PrefabID != PrefabID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		prefabID = PrefabID;
		base.Initialize();
	}
}
