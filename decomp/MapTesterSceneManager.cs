using Genkit;
using UnityEngine;
using UnityEngine.UI;
using XRL.Rules;
using XRL.World.ZoneBuilders;

public class MapTesterSceneManager : MonoBehaviour
{
	public Image image;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void test1()
	{
		CatacombsMapTemplate catacombsMapTemplate = new CatacombsMapTemplate(5);
		foreach (InfluenceMapRegion region in catacombsMapTemplate.regions.Regions)
		{
			Color4 value = new Color4((float)Stat.Random(1, 100) / 100f, (float)Stat.Random(1, 100) / 100f, (float)Stat.Random(1, 100) / 100f);
			foreach (Location2D cell in region.Cells)
			{
				catacombsMapTemplate.grid.set(cell.X, cell.Y, value);
			}
		}
		foreach (Rect2D maskingArea in catacombsMapTemplate.maskingAreas)
		{
			catacombsMapTemplate.grid.box(maskingArea, () => Color4.magenta);
		}
		image.sprite = catacombsMapTemplate.grid.toSprite();
	}
}
