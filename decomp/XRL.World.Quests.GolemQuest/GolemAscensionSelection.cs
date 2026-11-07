using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public class GolemAscensionSelection : GolemMaterialSelection<string, string>
{
	public const string BASE_ID = "Power";

	public const string LIQUID = "neutronflux";

	public const int DRAMS = 3;

	public string _Material;

	public override string Material
	{
		get
		{
			return _Material;
		}
		set
		{
			_Material = value;
		}
	}

	public override string ID => "Power";

	public override string DisplayName => "ascension power source";

	public override char Key => 'p';

	public override bool IsValid(string Material)
	{
		if (base.IsValid(Material))
		{
			if (!(Material != "neutronflux"))
			{
				return GetValidHolders().Any((GameObject x) => x.GetFreeDrams("neutronflux") >= 3);
			}
			return true;
		}
		return false;
	}

	public override List<string> GetValidMaterials()
	{
		List<string> list = new List<string>();
		if (GetValidHolders().Any((GameObject x) => x.GetFreeDrams("neutronflux") >= 3))
		{
			list.Add("neutronflux");
		}
		if (The.Game.HasFinishedQuest("If, Then, Else"))
		{
			list.Add("chavvah");
		}
		return list;
	}

	public override string GetNameFor(string Material)
	{
		if (Material == "neutronflux")
		{
			return string.Format("{0} {1} of {2}", 3, "drams", LiquidVolume.GetLiquid(Material)?.Name ?? Material);
		}
		return "Chavvah";
	}

	public override IRenderable GetIconFor(string Material)
	{
		if (Material == "neutronflux")
		{
			return System.Catalyst.GetIconFor(Material);
		}
		return new Renderable(GameObjectFactory.Factory.Blueprints["TerrainEynRoj"]);
	}

	public override void Apply(GameObject Object)
	{
		base.Apply(Object);
		if (!(Material == "neutronflux"))
		{
			return;
		}
		foreach (GameObject validHolder in GetValidHolders())
		{
			if (validHolder.GetFreeDrams("neutronflux") >= 3)
			{
				validHolder.UseDrams(3, Material);
				break;
			}
		}
	}

	[WishCommand("golemquest:power", null)]
	private static void Wish()
	{
		ReceiveMaterials(The.Player);
	}

	public static void ReceiveMaterials(GameObject Object)
	{
		Object.ReceiveObject(GameObjectFactory.Factory.CreateObject("Phial", (GameObject x) =>
		{
			LiquidVolume liquidVolume = x.LiquidVolume;
			liquidVolume.InitialLiquid = "neutronflux-1000";
			liquidVolume.MaxVolume = 3;
			liquidVolume.StartVolume = 3.ToString();
		}));
	}
}
