using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using XRL;
using XRL.Rules;
using XRL.UI;
using XRL.World;

namespace Qud.UI;

[UIView("WorldGenerationScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "WorldGeneration", UICanvasHost = 1)]
public class WorldGenerationScreen : SingletonWindowBase<WorldGenerationScreen>, ControlManager.IControllerChangedEvent
{
	public UITextSkin[] progressTexts;

	public UITextSkin quoteText;

	public UITextSkin attributionText;

	public UITextSkin progressText;

	private List<string> progressLines = new List<string>();

	private static string ProgressBasis = "{{Y|}}{{Y| . . . . . . . . ■ .  . . . . . . . ■  . . . . . . . . ■  . . . . . . . . ■  . . . . . . . . {{Y|}}";

	public int myStep;

	public float totalProgress;

	public List<(string icon, string color, string detail)>[] iconOptions = new List<(string, string, string)>[4]
	{
		new List<(string, string, string)> { ("Mutations/spacetime_vortex.bmp", "B", "m") },
		new List<(string, string, string)>
		{
			("Terrain/sw_mountains.bmp", "Y", "Y"),
			("Terrain/sw_mountains_2.bmp", "Y", "Y"),
			("Terrain/sw_hills.bmp", "y", "y"),
			("Terrain/sw_hills_2.bmp", "y", "y"),
			("Terrain/sw_canyon.bmp", "w", "w"),
			("Terrain/sw_hills2.bmp", "w", "w"),
			("Tiles/wall_rock-00000000.bmp", "r", "w"),
			("Tiles/wall_rock-00000000.bmp", "y", "K"),
			("Tiles/wall_rock-00000000.bmp", "r", "W")
		},
		new List<(string, string, string)>
		{
			("Terrain/sw_ruins_9.bmp", "Y", "y"),
			("Terrain/sw_ruins_11.bmp", "Y", "y"),
			("Terrain/sw_ruins_20.bmp", "Y", "y"),
			("Terrain/sw_ruins_deep_1.bmp", "Y", "y"),
			("Terrain/sw_ruins_deep_10.bmp", "Y", "y"),
			("Terrain/sw_ruins_deep_12.bmp", "Y", "y"),
			("Terrain/sw_rusted_archway.bmp", "r", "y"),
			("Terrain/tile_location8.bmp", "W", "C")
		},
		new List<(string, string, string)>
		{
			("Terrain/sw_joppa.bmp", "w", "y"),
			("Terrain/tile_kya.bmp", "Y", "R"),
			("Terrain/terrainpalladiumreef_3l.bmp", "c", "W"),
			("Terrain/sw_joppa.bmp", "b", "G"),
			("Terrain/sw_joppa.bmp", "r", "W"),
			("Terrain/sw_joppa.bmp", "m", "B")
		}
	};

	public UIThreeColorProperties[] eonIcons;

	public int[] iconSelections = new int[4];

	private HashSet<int> iconsUpdated = new HashSet<int>();

	public int totalSteps;

	public bool wasInScroller;

	public static async Task<bool> ShowWorldGenerationScreen(int totalSteps)
	{
		await The.UiContext;
		await (SingletonWindowBase<WorldGenerationScreen>.instance?._ShowWorldGenerationScreen(totalSteps));
		return true;
	}

	public static void HideWorldGenerationScreen()
	{
		SingletonWindowBase<WorldGenerationScreen>.instance?._HideWorldGenerationScreen();
	}

	public static void AddMessage(string message)
	{
		SingletonWindowBase<WorldGenerationScreen>.instance._AddMessage(message);
	}

	public static void IncrementProgress()
	{
		SingletonWindowBase<WorldGenerationScreen>.instance._IncrementProgress();
	}

	public async void _IncrementProgress()
	{
		await The.UiContext;
		myStep++;
		int num = Math.Min(ProgressBasis.Length - 18, (int)((float)myStep / (float)totalSteps * (float)(ProgressBasis.Length - 18)));
		totalProgress = (float)num / (float)(ProgressBasis.Length - 18);
		string text = ProgressBasis.Remove(num + 10, 1).Insert(num + 10, "}}>{{K|");
		progressText.SetText(text);
		UpdateIcons();
	}

	public async void _AddMessage(string message)
	{
		await The.UiContext;
		if (progressLines.Last() == message)
		{
			return;
		}
		progressLines.Add(message);
		int num = 0;
		foreach (string item in progressLines.TakeLast(5))
		{
			progressTexts[num].SetText(item);
			num++;
		}
		UpdateIcons();
	}

	public void InitIcons()
	{
		UIThreeColorProperties uIThreeColorProperties;
		for (int i = 0; i < 4; uIThreeColorProperties.image.color = uIThreeColorProperties.image.color.WithAlpha(0f), i++)
		{
			uIThreeColorProperties = eonIcons[i];
			string text;
			switch (i)
			{
			case 0:
				switch (Stat.Random(1, 3))
				{
				case 1:
					uIThreeColorProperties.FromRenderable(new Renderable("Mutations/spacetime_vortex.bmp", " ", "", "&B", 'm'));
					break;
				case 2:
					uIThreeColorProperties.FromRenderable(new Renderable("Creatures/sw_monad.bmp", " ", "", "&Y", 'm'));
					break;
				default:
					uIThreeColorProperties.FromRenderable(new Renderable("Mutations/light_manipulation.bmp", " ", "", "&Y", 'C'));
					break;
				}
				continue;
			case 1:
				text = "Geological";
				break;
			case 2:
				text = "Historical";
				break;
			default:
				text = "Contemporary";
				break;
			}
			string text2 = "unknown";
			Renderable renderable = new Renderable(GameObjectFactory.Factory.Blueprints["Snapjaw"]);
			for (int j = 0; j < 20; j++)
			{
				try
				{
					text2 = PopulationManager.RollOneFrom("DynamicSemanticTable:" + text).Blueprint;
					GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(text2);
					string paintedTile = blueprint.GetPaintedTile();
					if (!paintedTile.IsNullOrEmpty())
					{
						if (SpriteManager.HasTextureInfo(paintedTile))
						{
							renderable.Set(blueprint);
							renderable.Tile = paintedTile;
							break;
						}
						MetricsManager.LogError("Invalid tile [" + text2 + "]: " + paintedTile);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("invalid worldgen blueprint " + text2, x);
				}
			}
			try
			{
				uIThreeColorProperties.FromRenderable(renderable);
				uIThreeColorProperties.name = text2;
				if (i == 1)
				{
					Debug.LogWarning("walltype: " + text2);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogException("invalid worldgen blueprint " + text2, x2);
			}
		}
		iconsUpdated.Clear();
	}

	public void UpdateIcons()
	{
		for (int i = 0; i < 4; i++)
		{
			Stat.RandomCosmetic(0, iconOptions[i].Count - 1);
			float a = Math.Min(1f, totalProgress - (float)i * 0.1f);
			eonIcons[i].image.color = new Color(eonIcons[i].image.color.r, eonIcons[i].image.color.g, eonIcons[i].image.color.b, a);
		}
	}

	public async Task _ShowWorldGenerationScreen(int totalSteps)
	{
		await The.UiContext;
		this.totalSteps = totalSteps;
		for (int i = 0; i < progressTexts.Length; i++)
		{
			progressTexts[i].SetText(" ");
		}
		for (int j = 0; j < 4; j++)
		{
			eonIcons[j].image.color = new Color(eonIcons[j].image.color.r, eonIcons[j].image.color.g, eonIcons[j].image.color.b, 0f);
		}
		attributionText.SetText("");
		quoteText.SetText("");
		progressLines.Clear();
		for (int k = 0; k < 5; k++)
		{
			progressLines.Add("   ");
		}
		InitIcons();
		ControlManager.ResetInput();
		Show();
		_IncrementProgress();
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>(BookUI.Books["Quotes"][Stat.RandomCosmetic(0, BookUI.Books["Quotes"].Count - 1)].Lines);
		list.RemoveAll((string l) => l.Trim().Length == 0);
		if (list.Count == 0)
		{
			quoteText.SetText("");
			attributionText.SetText("");
			return;
		}
		if (list.Count == 1)
		{
			quoteText.SetText(list[0]);
			attributionText.SetText("");
			return;
		}
		foreach (string item in list.Take(list.Count - 1))
		{
			stringBuilder.Append(item);
		}
		quoteText.SetText(stringBuilder.ToString());
		attributionText.SetText(list.Last());
	}

	public async void _HideWorldGenerationScreen()
	{
		await The.UiContext;
		Hide();
	}

	public void Exit()
	{
	}

	public void Update()
	{
	}

	public void ControllerChanged()
	{
	}

	public void SetupContext()
	{
	}

	public override void Show()
	{
		base.Show();
		SetupContext();
		myStep = 0;
	}

	public override void Hide()
	{
		base.Hide();
		ControlManager.ResetInput();
		gameObject.SetActive(value: false);
	}
}
