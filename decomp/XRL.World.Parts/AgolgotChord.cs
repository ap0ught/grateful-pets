using System;
using System.Text;
using XRL.Language;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

public class AgolgotChord : INephalChord
{
	public Guid MultipleArmsID;

	[NonSerialized]
	private string DefaultBehaviorDesc;

	public override string Source => "Agolgot";

	public virtual int ArmPairs => 2;

	public virtual string DefaultBehavior => "Nephal_Claw_Chord";

	public override void Initialize()
	{
		if (!ParentObject.TryGetPart<MultipleArms>(out var Part))
		{
			MultipleArmsID = ParentObject.RequirePart<Mutations>().AddMutationMod(typeof(MultipleArms), null, 1, Mutations.MutationModifierTracker.SourceType.External, Grammar.MakePossessive(SourceName) + " chord");
			Part = ParentObject.GetPart<MultipleArms>();
		}
		if (Part.Pairs < ArmPairs)
		{
			Part.AdjustRank(ArmPairs - Part.Pairs);
		}
		foreach (BodyPart item in ParentObject.GetBodyPartsByManager(Part.AdditionsManagerID, EvenIfDismembered: true))
		{
			if (item.Type == "Hand")
			{
				item.DefaultBehavior?.Obliterate();
				item.DefaultBehavior = null;
				item.DefaultBehaviorBlueprint = DefaultBehavior;
			}
		}
		ParentObject.Body.RegenerateDefaultEquipment();
	}

	public override void Remove()
	{
		if (MultipleArmsID != Guid.Empty)
		{
			ParentObject.RequirePart<Mutations>().RemoveMutationMod(MultipleArmsID);
		}
	}

	public override void AppendRules(StringBuilder Postfix)
	{
		if (DefaultBehaviorDesc.IsNullOrEmpty())
		{
			string cachedDisplayNameStripped = GameObjectFactory.Factory.GetBlueprint(DefaultBehavior).CachedDisplayNameStripped;
			DefaultBehaviorDesc = Grammar.Pluralize(cachedDisplayNameStripped);
		}
		Postfix.Append("\n• +").Append(ArmPairs * 2).Append(" arms with ")
			.Append(DefaultBehaviorDesc);
	}
}
