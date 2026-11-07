using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLowtierRegen_StopBleeding_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they stop bleeding.";
	}

	public override void Apply(GameObject go)
	{
		int num = 10;
		while (go.HasEffect<Bleeding>())
		{
			go.RemoveEffect<Bleeding>();
			num--;
			if (num <= 0)
			{
				break;
			}
		}
	}
}
