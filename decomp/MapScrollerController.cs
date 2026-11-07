using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Genkit;
using Kobold;
using Qud.API;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

public class MapScrollerController : MonoBehaviour
{
	public class MapPinData
	{
		public int x;

		public int y;

		public string title;

		public string details;
	}

	public RectTransform mapTarget;

	public RectTransform mapTransform;

	public Image mapImage;

	private float MAP_LERP_DURATION = 1.25f;

	private float mapLerpTime;

	private Vector2 mapLerpStart;

	private Vector2 mapLerpEnd;

	public UnityEngine.GameObject pinPrefab;

	public List<UnityEngine.GameObject> pins = new List<UnityEngine.GameObject>();

	private Texture2D mapTexture;

	private List<Location2D> highlights = new List<Location2D>(3000);

	private static Queue<UnityEngine.GameObject> pinPool = new Queue<UnityEngine.GameObject>();

	public UITextSkin marginaliaText;

	public UITextSkin marginaliaTime;

	private static Dictionary<string, Color[]> texturePixels = new Dictionary<string, Color[]>();

	private static Color[] mapPixels = null;

	private bool wasCoda;

	public bool highlightsUpdated = true;

	private void Start()
	{
	}

	public void SetHighlights(IEnumerable<Location2D> highlights)
	{
		if ((highlights == null || highlights.Count() == 0) && (this.highlights == null || this.highlights.Count == 0))
		{
			return;
		}
		if (highlights == null || this.highlights == null || highlights.Count() != this.highlights.Count)
		{
			highlightsUpdated = true;
		}
		else
		{
			highlightsUpdated = false;
			int num = 0;
			foreach (Location2D highlight in highlights)
			{
				if (this.highlights[num] != highlight)
				{
					highlightsUpdated = true;
					break;
				}
				num++;
			}
		}
		this.highlights.Clear();
		if (highlights != null)
		{
			this.highlights.AddRange(highlights);
		}
	}

	public void SetPins(IEnumerable<MapPinData> data)
	{
		foreach (UnityEngine.GameObject pin in pins)
		{
			pinPool.Enqueue(pin);
			pin.SetActive(value: false);
		}
		pins.Clear();
		foreach (MapPinData datum in data)
		{
			UnityEngine.GameObject gameObject;
			if (pinPool.Count > 0)
			{
				gameObject = pinPool.Dequeue();
				gameObject.SetActive(value: true);
			}
			else
			{
				gameObject = Object.Instantiate(pinPrefab);
			}
			pins.Add(gameObject);
			gameObject.transform.SetParent(mapTransform, worldPositionStays: false);
			gameObject.GetComponent<MapScrollerPinItem>().SetData(datum);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(datum.x * 32 + 20, datum.y * -48 + 24);
			gameObject.SetActive(value: true);
		}
	}

	public void RenderAccomplishment(JournalAccomplishment a)
	{
		int num = 144;
		int num2 = 120;
		if (mapTexture == null)
		{
			mapTexture = new Texture2D(num, num2, TextureFormat.RGBA32, mipChain: false);
			mapTexture.filterMode = UnityEngine.FilterMode.Point;
			mapImage.sprite = Sprite.Create(mapTexture, new Rect(0f, 0f, num, num2), new Vector2(0f, 0f));
		}
		marginaliaText.SetText(a.GetDisplayText());
		marginaliaTime.SetText(Calendar.GetMarginaliaTime(a.Time));
		if (a.Screenshot == null)
		{
			return;
		}
		int num3 = 16;
		int num4 = 24;
		Color color = ConsoleLib.Console.ColorUtility.FromWebColor("041312");
		Color b = ConsoleLib.Console.ColorUtility.ColorMap['K'];
		if (mapPixels == null)
		{
			mapPixels = mapTexture.GetPixels();
		}
		for (int i = 0; i < mapPixels.Length; i++)
		{
			mapPixels[i] = new Color(0f, 0f, 0f, 0f);
		}
		for (int j = 0; j < 9; j++)
		{
			for (int k = 0; k < 5; k++)
			{
				SnapshotRenderable snapshotRenderable = a.Screenshot[j + k * 9];
				Sprite unitySprite = SpriteManager.GetUnitySprite(snapshotRenderable.GetSpriteName());
				ColorChars colorChars = snapshotRenderable.getColorChars();
				string spriteName = snapshotRenderable.GetSpriteName();
				Color a2 = ConsoleLib.Console.ColorUtility.colorFromChar(colorChars.foreground);
				Color a3 = ConsoleLib.Console.ColorUtility.colorFromChar(snapshotRenderable.getDetailColor());
				if (!texturePixels.ContainsKey(spriteName))
				{
					texturePixels.Add(spriteName, unitySprite.texture.GetPixels());
				}
				Color[] array = texturePixels[spriteName];
				for (int l = 0; l < num3; l++)
				{
					for (int m = 0; m < num4; m++)
					{
						Color color2 = (snapshotRenderable.getHFlip() ? array[num3 - l - 1 + m * num3] : array[l + m * num3]);
						Color color3 = color;
						int num5 = j * num3 + l;
						int num6 = (5 - k - 1) * num4 + m;
						float num7 = 0f;
						if (highlights == null || highlights.Count == 0)
						{
							num7 = 0f;
						}
						else
						{
							float f = 0f;
							num7 = Mathf.Min(1f, Mathf.Max(0f, Mathf.Sqrt(f) / 48f));
						}
						if (color2.a <= 0f)
						{
							color3 = color;
						}
						else if ((double)color2.r < 0.5)
						{
							color3 = Color.Lerp(a2, b, num7);
						}
						else if ((double)color2.r > 0.5)
						{
							color3 = Color.Lerp(a3, b, num7);
						}
						mapPixels[num5 + num6 * (9 * num3)] = color3;
					}
				}
			}
		}
		mapTexture.SetPixels(mapPixels);
		mapTexture.Apply();
	}

	public void RefreshMapCoda()
	{
		if (!wasCoda)
		{
			highlightsUpdated = true;
			wasCoda = false;
		}
		if (!highlightsUpdated)
		{
			return;
		}
		highlightsUpdated = false;
		highlights.Clear();
		Stat.ReseedFrom("CODAMAP");
		int x = Stat.Random(5, 75);
		int y = Stat.Random(5, 20);
		Location2D location2D = Location2D.Get(x, y);
		highlights.Add(location2D);
		SetTarget(location2D.X, location2D.Y);
		int num = 1280;
		int num2 = 600;
		if (mapTexture == null)
		{
			mapTexture = new Texture2D(num, num2, TextureFormat.RGBA32, mipChain: false);
			mapTexture.filterMode = UnityEngine.FilterMode.Point;
			mapImage.sprite = Sprite.Create(mapTexture, new Rect(0f, 0f, num, num2), new Vector2(0f, 0f));
		}
		The.Game.ZoneManager.GetZone("JoppaWorld");
		int num3 = 16;
		int num4 = 24;
		Color color = ConsoleLib.Console.ColorUtility.FromWebColor("041312");
		Color b = ConsoleLib.Console.ColorUtility.FromWebColor("041312");
		BallBag<string> ballBag = new BallBag<string>();
		if (The.Player.CurrentZone?.GetZoneProperty("Region") == "Saltmarsh")
		{
			ballBag.Add("saltmarsh", 100);
			ballBag.Add("canyon", 10);
			ballBag.Add("hills", 10);
		}
		else if (The.Player.CurrentZone?.GetZoneProperty("Region") == "DesertCanyon")
		{
			ballBag.Add("saltmarsh", 10);
			ballBag.Add("canyon", 100);
			ballBag.Add("hills", 10);
		}
		else if (The.Player.CurrentZone?.GetZoneProperty("Region") == "Hills")
		{
			ballBag.Add("saltmarsh", 10);
			ballBag.Add("canyon", 10);
			ballBag.Add("hills", 100);
		}
		int count = highlights.Count;
		if (mapPixels == null)
		{
			mapPixels = mapTexture.GetPixels();
		}
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				string text = "Text/32.bmp";
				int num5 = location2D.Distance(i, j);
				Color a = ConsoleLib.Console.ColorUtility.colorFromChar('g');
				Color a2 = ConsoleLib.Console.ColorUtility.colorFromChar('k');
				if (num5 == 0)
				{
					text = "Terrain/sw_joppa.bmp";
					a = ConsoleLib.Console.ColorUtility.colorFromChar(Crayons.GetRandomColor()[0]);
					a2 = ConsoleLib.Console.ColorUtility.colorFromChar('w');
				}
				else if (num5 < 4)
				{
					switch (ballBag.PluckOne())
					{
					case "saltmarsh":
						text = "terrain/tile_swamp1.bmp";
						a = ConsoleLib.Console.ColorUtility.colorFromChar('g');
						a2 = ConsoleLib.Console.ColorUtility.colorFromChar('k');
						break;
					case "canyon":
						text = ((Stat.Random(1, 2) == 1) ? "Terrain/sw_canyon.bmp" : "Terrain/sw_hills2.bmp");
						a = ConsoleLib.Console.ColorUtility.colorFromChar('w');
						a2 = ConsoleLib.Console.ColorUtility.colorFromChar('k');
						break;
					case "hills":
						text = "Terrain/sw_hills.bmp";
						a = ConsoleLib.Console.ColorUtility.colorFromChar('y');
						a2 = ConsoleLib.Console.ColorUtility.colorFromChar('k');
						break;
					}
				}
				else
				{
					a = ConsoleLib.Console.ColorUtility.colorFromChar('k');
					a2 = ConsoleLib.Console.ColorUtility.colorFromChar('k');
				}
				Sprite unitySprite = SpriteManager.GetUnitySprite(text);
				if (!texturePixels.ContainsKey(text))
				{
					texturePixels.Add(text, unitySprite.texture.GetPixels());
				}
				Color[] array = texturePixels[text];
				for (int k = 0; k < num3; k++)
				{
					for (int l = 0; l < num4; l++)
					{
						Color color2 = array[k + l * num3];
						Color color3 = color;
						int num6 = i * num3 + k;
						int num7 = (24 - j) * num4 + l;
						float num8 = 0f;
						if (highlights == null || count == 0)
						{
							num8 = 0f;
						}
						else
						{
							float num9 = float.MaxValue;
							for (int m = 0; m < highlights.Count; m++)
							{
								Location2D location2D2 = highlights[m];
								float num10 = location2D2.X * num3 + num3 / 2;
								float num11 = location2D2.Y * num4 + num4 / 4;
								int num12 = i * num3 + k;
								int num13 = j * (num4 + 1) - l;
								float num14 = (num10 - (float)num12) * (num10 - (float)num12) + (num11 - (float)num13) * (num11 - (float)num13);
								if (num14 < num9)
								{
									num9 = num14;
								}
							}
							num8 = ((!(num9 < 2400f)) ? 1f : Mathf.Min(1f, Mathf.Max(0f, Mathf.Sqrt(num9) / 48f)));
						}
						if (color2.a <= 0f)
						{
							color3 = color;
						}
						else if ((double)color2.r < 0.5)
						{
							color3 = Color.Lerp(a, b, num8);
						}
						else if ((double)color2.r > 0.5)
						{
							color3 = Color.Lerp(a2, b, num8);
						}
						mapPixels[num6 + num7 * (80 * num3)] = color3;
					}
				}
			}
		}
		mapTexture.SetPixels(mapPixels);
		mapTexture.Apply();
	}

	public void RefreshMap()
	{
		if (CodaSystem.InCoda())
		{
			RefreshMapCoda();
		}
		else
		{
			if (!highlightsUpdated)
			{
				return;
			}
			highlightsUpdated = false;
			int num = 1280;
			int num2 = 600;
			if (mapTexture == null)
			{
				mapTexture = new Texture2D(num, num2, TextureFormat.RGBA32, mipChain: false);
				mapTexture.filterMode = UnityEngine.FilterMode.Point;
				mapImage.sprite = Sprite.Create(mapTexture, new Rect(0f, 0f, num, num2), new Vector2(0f, 0f));
			}
			Zone zone = The.Game.ZoneManager.GetZone("JoppaWorld");
			int num3 = 16;
			int num4 = 24;
			Color color = ConsoleLib.Console.ColorUtility.FromWebColor("041312");
			Color b = ConsoleLib.Console.ColorUtility.ColorMap['K'];
			using RenderEvent renderEvent = RenderEvent.Pool.Get();
			int count = highlights.Count;
			if (mapPixels == null)
			{
				mapPixels = mapTexture.GetPixels();
			}
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					zone.GetCell(i, j).Render(renderEvent);
					Sprite unitySprite = SpriteManager.GetUnitySprite(renderEvent.GetSpriteName());
					if (!texturePixels.ContainsKey(renderEvent.GetSpriteName()))
					{
						texturePixels.Add(renderEvent.GetSpriteName(), unitySprite.texture.GetPixels());
					}
					Color[] array = texturePixels[renderEvent.GetSpriteName()];
					Color foregroundColor = renderEvent.GetForegroundColor();
					for (int k = 0; k < num3; k++)
					{
						for (int l = 0; l < num4; l++)
						{
							Color color2 = array[k + l * num3];
							Color color3 = color;
							int num5 = i * num3 + k;
							int num6 = (24 - j) * num4 + l;
							float num7 = 0f;
							if (highlights == null || count == 0)
							{
								num7 = 0f;
							}
							else
							{
								float num8 = float.MaxValue;
								for (int m = 0; m < highlights.Count; m++)
								{
									Location2D location2D = highlights[m];
									if (location2D != null)
									{
										float num9 = location2D.X * num3 + num3 / 2;
										float num10 = location2D.Y * num4 + num4 / 4;
										int num11 = i * num3 + k;
										int num12 = j * (num4 + 1) - l;
										float num13 = (num9 - (float)num11) * (num9 - (float)num11) + (num10 - (float)num12) * (num10 - (float)num12);
										if (num13 < num8)
										{
											num8 = num13;
										}
									}
								}
								num7 = ((!(num8 < 2400f)) ? 1f : Mathf.Min(1f, Mathf.Max(0f, Mathf.Sqrt(num8) / 48f)));
							}
							if (color2.a <= 0f)
							{
								color3 = color;
							}
							else if ((double)color2.r < 0.5)
							{
								color3 = Color.Lerp(foregroundColor, b, num7);
							}
							else if ((double)color2.r > 0.5)
							{
								color3 = Color.Lerp(renderEvent.GetDetailColor(), b, num7);
							}
							mapPixels[num5 + num6 * (80 * num3)] = color3;
						}
					}
				}
			}
			mapTexture.SetPixels(mapPixels);
			mapTexture.Apply();
		}
	}

	public void SetTarget(int x, int y)
	{
		mapTarget.anchoredPosition = new Vector2(x * 32, y * -48);
		mapLerpStart = mapTransform.anchoredPosition;
		Vector2 sizeDelta = (transform as RectTransform).sizeDelta;
		mapLerpEnd = new Vector2(0f - ((float)(x * 32 + 16) - sizeDelta.x / 2f), (float)(y * 48 + 24) - sizeDelta.y / 2f);
		mapLerpTime = MAP_LERP_DURATION;
	}

	private void Update()
	{
		if (mapTransform != null)
		{
			if (mapLerpTime > 0f)
			{
				mapTransform.anchoredPosition = Vector2.Lerp(mapLerpStart, mapLerpEnd, Easing.CubicEaseInOut(MAP_LERP_DURATION - mapLerpTime));
				mapLerpTime -= Time.deltaTime;
			}
			float num = 512f;
			string text = ControlManager.ResolveAxisDirection("UI:DetailsNavigate");
			if (text != null && text.Contains("N"))
			{
				mapTransform.anchoredPosition += new Vector2(0f, (0f - num) * Time.deltaTime);
			}
			if (text != null && text.Contains("S"))
			{
				mapTransform.anchoredPosition += new Vector2(0f, num * Time.deltaTime);
			}
			if (text != null && text.Contains("E"))
			{
				mapTransform.anchoredPosition += new Vector2((0f - num) * Time.deltaTime, 0f);
			}
			if (text != null && text.Contains("W"))
			{
				mapTransform.anchoredPosition += new Vector2(num * Time.deltaTime, 0f);
			}
		}
	}
}
