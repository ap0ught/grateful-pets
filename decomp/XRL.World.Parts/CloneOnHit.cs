using System;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CloneOnHit : IPart
{
	public int Chance = 100;

	public string ReplicationContext = "CloningDraught";

	private int DestabilizeChance = 33;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part CloneOnHit Activation", Chance, subject).in100())
			{
				subject = gameObjectParameter;
				if (Cloning.CanBeCloned(gameObjectParameter2, subject, ReplicationContext))
				{
					gameObjectParameter2.ApplyEffect(new Budding(gameObjectParameter, 1, ReplicationContext));
					if (DestabilizeChance.in100())
					{
						DidX("destabilize");
						ParentObject.Destroy();
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
