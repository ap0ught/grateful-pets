using ConsoleLib.Console;
using UnityEngine;

namespace XRL.World.Parts;

public class PrefabImposter : BasePrefabImposter
{
	public override bool Render(RenderEvent E)
	{
		if (!Disabled && E.Visible && (int)E.Lit > 1)
		{
			E.Imposters.Add(new ImposterExtra.ImposterInfo(Prefab, new Vector3(X, Y, Z), Layer, Config));
		}
		return true;
	}
}
