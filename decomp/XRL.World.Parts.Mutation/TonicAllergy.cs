using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TonicAllergy : BaseMutation
{
	private static Func<ITonicEffect>[] getOverdoseTonic = new Func<ITonicEffect>[7]
	{
		() => new Hoarshroom_Tonic(Stat.Roll("180-220")),
		() => new Blaze_Tonic(Stat.Roll("41-50")),
		() => new Skulk_Tonic(Stat.Roll("1001-1200")),
		() => new HulkHoney_Tonic(Stat.Roll("41-50")),
		() => new Rubbergum_Tonic(Stat.Roll("41-50")),
		() => new ShadeOil_Tonic(Stat.Roll("41-50")),
		() => new SphynxSalt_Tonic(Stat.Roll("18-22"))
	};

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You are allergic to tonics.\n\nThe chance your mutant physiology reacts adversely to a tonic is increased to 33%.\nIf you react adversely this way to a salve or ubernostrum tonic, the adverse reaction effect is chosen randomly from among other tonic effects. You will still heal.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public static void SalveOverdose(GameObject go)
	{
		if (go.IsPlayer())
		{
			getOverdoseTonic.GetRandomElement()().ApplyAllergy(go);
		}
	}
}
