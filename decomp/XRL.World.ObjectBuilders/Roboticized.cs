using System;
using ConsoleLib.Console;
using XRL.Wish;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ObjectBuilders;

[Serializable]
[HasWishCommand]
public class Roboticized : IObjectBuilder
{
	public const string PREFIX_NAME = "{{c|mechanical}}";

	public const string POSTFIX_DESC = "There is a low, persistent hum emanating outward.";

	public string NamePrefix;

	public string DescriptionPostfix;

	public override void Initialize()
	{
		NamePrefix = "{{c|mechanical}}";
		DescriptionPostfix = "There is a low, persistent hum emanating outward.";
	}

	public override void Apply(GameObject Object, string Context)
	{
		Roboticize(Object, NamePrefix, DescriptionPostfix);
	}

	public static void Roboticize(GameObject Object, string NamePrefix = "{{c|mechanical}}", string DescriptionPostfix = "There is a low, persistent hum emanating outward.")
	{
		if (Object.HasPart<Robot>())
		{
			return;
		}
		Object.AddPart(new Robot());
		Object.RequirePart<MentalShield>();
		Object.RequirePart<Metal>();
		Object.RequirePart<MaintenanceSystems>();
		Object.RemovePart<Springy>();
		Object.RemovePart<Stomach>();
		if (!Object.HasPart<DarkVision>())
		{
			Object.GetPart<Mutations>().AddMutation(new DarkVision(), 12);
		}
		if (Object.TryGetPart<Corpse>(out var Part))
		{
			Part.CorpseChance = 0;
			Part.BurntCorpseChance = 0;
			Part.VaporizedCorpseChance = 0;
		}
		Object.GetStat("ElectricResistance").BaseValue = -50;
		Object.GetStat("HeatResistance").BaseValue = 25;
		Object.GetStat("ColdResistance").BaseValue = 25;
		Object.IsOrganic = false;
		Object.SetStringProperty("SeveredLimbBlueprint", "RobotLimb");
		Object.SetStringProperty("SeveredHeadBlueprint", "RobotHead1");
		Object.SetStringProperty("SeveredFaceBlueprint", "RobotFace");
		Object.SetStringProperty("SeveredArmBlueprint", "RobotArm");
		Object.SetStringProperty("SeveredHandBlueprint", "RobotHand");
		Object.SetStringProperty("SeveredLegBlueprint", "RobotLeg");
		Object.SetStringProperty("SeveredFootBlueprint", "RobotFoot");
		Object.SetStringProperty("SeveredFeetBlueprint", "RobotFeet");
		Object.SetStringProperty("SeveredTailBlueprint", "RobotTail");
		Object.SetStringProperty("SeveredRootsBlueprint", "RobotRoots");
		Object.SetStringProperty("SeveredFinBlueprint", "RobotFin");
		Object.SetIntProperty("Bleeds", 1);
		Object.SetStringProperty("BleedLiquid", "oil-1000");
		Object.SetStringProperty("BleedColor", "&K");
		Object.SetStringProperty("BleedPrefix", "&Koily");
		Render render = Object.Render;
		string text = (render.TileColor.IsNullOrEmpty() ? render.ColorString : render.TileColor);
		render.ColorString = "&c";
		render.TileColor = "&c";
		if (render.DetailColor == "c")
		{
			render.DetailColor = ColorUtility.FindLastForeground(text)?.ToString() ?? Crayons.GetRandomColor();
		}
		if (!NamePrefix.IsNullOrEmpty())
		{
			Object.RequirePart<DisplayNameAdjectives>().AddAdjective(NamePrefix);
		}
		Object.Body?.CategorizeAll(7);
		if (!DescriptionPostfix.IsNullOrEmpty() && Object.HasPart<Description>())
		{
			if (Object.HasTag("VerseDescription"))
			{
				Description part = Object.GetPart<Description>();
				part._Short = part._Short + "\n\n" + DescriptionPostfix;
			}
			else
			{
				Description part2 = Object.GetPart<Description>();
				part2._Short = part2._Short + " " + DescriptionPostfix;
			}
		}
	}

	[WishCommand("mechanical", null)]
	public static void Wish(string Blueprint)
	{
		WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		Roboticize(gameObject);
		The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
	}
}
