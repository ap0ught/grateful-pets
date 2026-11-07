using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class GunslingerEquipment1 : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		string blueprint = "Laser Pistol";
		string text = "Chem Cell";
		int num = 2;
		int num2 = Stat.Random(1, 100);
		if (num2 <= 25)
		{
			blueprint = "Laser Pistol";
		}
		else if (num2 <= 55)
		{
			blueprint = "Borderlands Revolver";
			text = "Lead Slug";
			num = Stat.Random(36, 46);
		}
		else if (num2 <= 75)
		{
			blueprint = "Semi-Automatic Pistol";
			text = "Lead Slug";
			num = Stat.Random(76, 96);
		}
		else if (num2 <= 100)
		{
			blueprint = "Chain Pistol";
			text = "Lead Slug";
			num = Stat.Random(136, 246);
		}
		if (text == "Chem Cell")
		{
			GO.ReceiveObject(SpawnWithChemCell(GameObject.Create(blueprint, 45, 0, null, null, null, Context)));
			GO.ReceiveObject(SpawnWithChemCell(GameObject.Create(blueprint, 0, 0, null, null, null, Context)));
			return;
		}
		GameObject gameObject = GameObject.Create(blueprint, 45, 0, null, null, null, Context);
		GameObject gameObject2 = GameObject.Create(blueprint, 0, 0, null, null, null, Context);
		LiquidAmmoLoader part = gameObject.GetPart<LiquidAmmoLoader>();
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject3 = GameObject.Create(text, 0, 0, null, null, null, Context);
			if (part != null)
			{
				LiquidVolume liquidVolume = gameObject3.LiquidVolume;
				if (liquidVolume != null)
				{
					liquidVolume.Empty();
					liquidVolume.ComponentLiquids.Add(part.Liquid, 1000);
					liquidVolume.Volume = Stat.Random(liquidVolume.MaxVolume / 3 + 1, liquidVolume.MaxVolume);
					liquidVolume.Update();
				}
			}
			GO.ReceiveObject(gameObject3);
		}
		GO.ReceiveObject(gameObject);
		GO.ReceiveObject(gameObject2);
		GameObject SpawnWithChemCell(GameObject gun)
		{
			if (gun.TryGetPart<EnergyCellSocket>(out var Part))
			{
				Part.Cell = GameObject.Create("Chem Cell", 0, 0, null, null, null, Context);
			}
			return gun;
		}
	}
}
