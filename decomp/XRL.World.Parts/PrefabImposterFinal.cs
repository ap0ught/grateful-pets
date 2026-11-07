using ConsoleLib.Console;
using UnityEngine;

namespace XRL.World.Parts;

public class PrefabImposterFinal : BasePrefabImposter
{
	public override bool FinalRender(RenderEvent E)
	{
		if (!Disabled)
		{
			E.Imposters.Add(new ImposterExtra.ImposterInfo(Prefab, new Vector3(X, Y, Z), Layer, Config));
		}
		return base.FinalRender(E);
	}
}
