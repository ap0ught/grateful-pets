using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class PotentialHindrenLookClue : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated" && KithAndKinGameState.Instance != null && KithAndKinGameState.Instance.lookClues.Any((HindrenClueLook c) => c.target == ParentObject.Blueprint))
		{
			ParentObject.AddPart(new HindrenClueItem());
		}
		return base.FireEvent(E);
	}
}
