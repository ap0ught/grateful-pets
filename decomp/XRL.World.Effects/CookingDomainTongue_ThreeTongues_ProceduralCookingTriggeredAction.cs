using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTongue_ThreeTongues_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they shoot@s out up to three sticky tongues per Sticky Tongue at rank 10.";
	}

	public override string GetNotification()
	{
		return "@they shoot@s out a trio of sticky tongues.";
	}

	public override void Apply(GameObject go)
	{
		go.EmitMessage("A trio of tongues vegetate from =subject.t's= =bodypart:Face=!");
		StickyTongue.HarpoonNearest(go, StickyTongue.GetRange(10), "&M", 3);
	}
}
