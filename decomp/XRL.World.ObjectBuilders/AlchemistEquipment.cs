using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class AlchemistEquipment : IObjectBuilder
{
	public override void Apply(GameObject GO, string Context)
	{
		foreach (BaseLiquid allLiquid in LiquidVolume.getAllLiquids())
		{
			GameObject gameObject = GameObject.Create("Phial", 0, 0, null, null, null, Context);
			LiquidVolume liquidVolume = gameObject.LiquidVolume;
			liquidVolume.ComponentLiquids.Clear();
			liquidVolume.ComponentLiquids.Add(allLiquid.ID, 1000);
			liquidVolume.Volume = 1;
			liquidVolume.Update();
			GO.ReceiveObject(gameObject);
		}
	}
}
