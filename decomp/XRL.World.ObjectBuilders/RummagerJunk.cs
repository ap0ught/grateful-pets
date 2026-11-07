using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class RummagerJunk : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		for (int i = 0; i < 3; i++)
		{
			if (25.in100())
			{
				GO.ReceiveObjectFromPopulation("Junk 2", null, NoStack: false, 0, 0, null, null, null, Context);
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (25.in100())
			{
				GO.ReceiveObjectFromPopulation("Junk 3", null, NoStack: false, 0, 0, null, null, null, Context);
			}
		}
		if (5.in100())
		{
			GO.ReceiveObjectFromPopulation("Junk 4", null, NoStack: false, 0, 0, null, null, null, Context);
		}
		string blueprint = "Desert Rifle";
		string text = "Lead Slugs";
		int num = 40;
		int num2 = Stat.Random(1, 100);
		if (num2 <= 25)
		{
			blueprint = "Desert Rifle";
			text = "Lead Slug";
			num = Stat.Random(6, 12);
		}
		else if (num2 <= 60)
		{
			blueprint = "Borderlands Revolver";
			text = "Lead Slug";
			num = Stat.Random(12, 16);
		}
		else if (num2 <= 70)
		{
			blueprint = "Semi-Automatic Pistol";
			text = "Lead Slug";
			num = Stat.Random(12, 26);
		}
		else if (num2 <= 80)
		{
			blueprint = "Carbine";
			text = "Lead Slug";
			num = Stat.Random(16, 26);
		}
		else if (num2 <= 81)
		{
			blueprint = "Grenade Launcher";
			text = "HEGrenade1";
			num = Stat.Random(3, 6);
		}
		else if (num2 <= 83)
		{
			blueprint = "Missile Launcher";
			text = "HE Missile";
			num = Stat.Random(3, 6);
		}
		else if (num2 <= 85)
		{
			blueprint = "Chaingun";
			text = "Lead Slug";
			num = Stat.Random(176, 196);
		}
		else if (num2 <= 88)
		{
			blueprint = "Flamethrower";
			text = "Oilskin";
			num = Stat.Random(1, 2);
		}
		else if (num2 <= 91)
		{
			blueprint = "Sniper Rifle";
			text = "Lead Slug";
			num = Stat.Random(14, 16);
		}
		else if (num2 <= 96)
		{
			blueprint = "Chain Pistol";
			text = "Lead Slug";
			num = Stat.Random(114, 116);
		}
		else if (num2 <= 100)
		{
			blueprint = "Combat Shotgun";
			text = "Shotgun Shell";
			num = Stat.Random(8, 16);
		}
		if (text == "Chem Cell")
		{
			foreach (GameObject item in new List<GameObject> { GameObject.Create(blueprint, 0, 0, null, null, null, Context) })
			{
				if (item.TryGetPart<EnergyCellSocket>(out var Part))
				{
					Part.Cell = GameObject.Create("Chem Cell", 0, 0, null, null, null, Context);
				}
				GO.ReceiveObject(item);
			}
			return;
		}
		GameObject gameObject = GameObject.Create(blueprint, 0, 0, null, null, null, Context);
		LiquidAmmoLoader part = gameObject.GetPart<LiquidAmmoLoader>();
		for (int k = 0; k < num; k++)
		{
			GameObject gameObject2 = GameObject.Create(text, 0, 0, null, null, null, Context);
			if (part != null)
			{
				LiquidVolume liquidVolume = gameObject2.LiquidVolume;
				if (liquidVolume != null)
				{
					liquidVolume.Empty();
					liquidVolume.ComponentLiquids.Add(part.Liquid, 1000);
					liquidVolume.Volume = Stat.Random(liquidVolume.MaxVolume / 3 + 1, liquidVolume.MaxVolume);
					liquidVolume.Update();
				}
			}
			GO.ReceiveObject(gameObject2);
		}
		GO.ReceiveObject(gameObject);
	}
}
