using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainArtifact_IdentifyAllInZone_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they identify all artifacts on the local map.";
	}

	public override string GetNotification()
	{
		return "";
	}

	public override void Apply(GameObject go)
	{
		go.CurrentZone?.ForeachCell((Cell c) =>
		{
			c.ForeachObject((GameObject o) =>
			{
				if (!o.Understood())
				{
					o.MakeUnderstood();
					if (go.IsPlayer())
					{
						IComponent<GameObject>.XDidYToZ(go, "flush", "with understanding of", o, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
					}
				}
				o.ForeachInventoryEquipmentDefaultBehaviorAndCybernetics((GameObject e) =>
				{
					if (!e.Understood())
					{
						e.MakeUnderstood();
						if (go.IsPlayer())
						{
							IComponent<GameObject>.XDidYToZ(go, "flush", "with understanding of", e, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
						}
					}
				});
			});
		});
	}
}
