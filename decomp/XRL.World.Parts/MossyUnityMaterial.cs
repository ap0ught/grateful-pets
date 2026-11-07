using System;
using ConsoleLib.Console;
using UnityEngine;

namespace XRL.World.Parts;

[Serializable]
[Obsolete]
public class MossyUnityMaterial : IPartWithPrefabImposter
{
	public MossyUnityMaterial()
	{
		prefabID = "Prefabs/Imposters/Mossy";
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (ImposterActive && (!VisibleOnly || ParentObject.IsVisible()))
		{
			E.Imposters.Add(new ImposterExtra.ImposterInfo(prefabID, new Vector3(X, Y, -1f), 0, ParentObject?.Render.Tile + "~" + ParentObject?.ID));
		}
		return base.FinalRender(E);
	}
}
