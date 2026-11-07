using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class PistonPressController : IPart
{
	public string Directions;

	public int Frequency = 4;

	public int Counter;

	[NonSerialized]
	private Dictionary<string, PistonPressElement> elements;

	[NonSerialized]
	private static Dictionary<string, string> extendTile = new Dictionary<string, string>
	{
		{ "S", "Items/sw_crusher_s_extend.bmp" },
		{ "N", "Items/sw_crusher_n_extend.bmp" },
		{ "W", "Items/sw_crusher_w_extend.bmp" },
		{ "E", "Items/sw_crusher_e_extend.bmp" }
	};

	[NonSerialized]
	private static Dictionary<string, string> pressGlyph = new Dictionary<string, string>
	{
		{
			"S",
			'Ò'.ToString()
		},
		{
			"N",
			'Ð'.ToString()
		},
		{
			"W",
			'µ'.ToString()
		},
		{
			"E",
			'Æ'.ToString()
		}
	};

	[NonSerialized]
	private static Dictionary<string, string> pressTile = new Dictionary<string, string>
	{
		{ "S", "Items/sw_crusher_s_press.bmp" },
		{ "N", "Items/sw_crusher_n_press.bmp" },
		{ "W", "Items/sw_crusher_w_press.bmp" },
		{ "E", "Items/sw_crusher_e_press.bmp" }
	};

	[NonSerialized]
	private static Dictionary<string, string> extendGlyph = new Dictionary<string, string>
	{
		{
			"S",
			'º'.ToString()
		},
		{
			"N",
			'º'.ToString()
		},
		{
			"W",
			'Í'.ToString()
		},
		{
			"E",
			'ú'.ToString()
		}
	};

	[NonSerialized]
	private static Dictionary<string, string> centerGlyph = new Dictionary<string, string>
	{
		{
			"NS",
			'×'.ToString()
		},
		{
			"EW",
			'Ø'.ToString()
		}
	};

	[NonSerialized]
	private static Dictionary<string, string> centerTile = new Dictionary<string, string>
	{
		{ "NS", "Items/sw_crusher_ns_closed.bmp" },
		{ "EW", "Items/sw_crusher_we_open.bmp" }
	};

	[NonSerialized]
	private static Dictionary<string, string> centerGlyphOne = new Dictionary<string, string>
	{
		{
			"N",
			'Ð'.ToString()
		},
		{
			"S",
			'Ò'.ToString()
		},
		{
			"E",
			'Æ'.ToString()
		},
		{
			"W",
			'µ'.ToString()
		}
	};

	[NonSerialized]
	private static Dictionary<string, string> centerTileOne = new Dictionary<string, string>
	{
		{ "N", "Items/sw_crusher_n_closed.png" },
		{ "S", "Items/sw_crusher_s_closed.png" },
		{ "E", "Items/sw_crusher_e_closed.png" },
		{ "W", "Items/sw_crusher_w_closed.png" }
	};

	public GameObject centerElement;

	public override bool SameAs(IPart p)
	{
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Counter == Frequency - 1)
		{
			int num = XRLCore.CurrentFrame % 30;
			if (num > 0 && num < 15)
			{
				E.Tile = null;
				E.RenderString = "X";
				E.ColorString = "&R";
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (elements == null && Directions != null)
			{
				elements = new Dictionary<string, PistonPressElement>();
				for (int i = 0; i < Directions.Length; i++)
				{
					Cell cellFromDirection = ParentObject.Physics.currentCell.GetCellFromDirection(Directions[i].ToString());
					if (cellFromDirection != null)
					{
						while (cellFromDirection.GetObjectCountWithPart("PistonPressElement") > 1)
						{
							cellFromDirection.GetObjectsWithPart("PistonPressElement").FirstOrDefault().Obliterate();
						}
						GameObject gameObject = cellFromDirection.GetObjectsWithPart("PistonPressElement").FirstOrDefault();
						if (gameObject != null)
						{
							elements.Add(Directions[i].ToString(), gameObject.GetPart<PistonPressElement>());
							gameObject.GetPart<PistonPressElement>().Direction = XRL.Rules.Directions.GetOppositeDirection(Directions[i].ToString());
						}
					}
				}
			}
			if (elements != null)
			{
				elements.RemoveAll((KeyValuePair<string, PistonPressElement> e) => e.Value == null || e.Value.ParentObject == null || !e.Value.ParentObject.IsValid());
				List<string> list = null;
				foreach (string key in elements.Keys)
				{
					PistonPressElement pistonPressElement = elements[key];
					if (pistonPressElement == null || pistonPressElement.ParentObject == null || pistonPressElement.ParentObject.Physics == null || pistonPressElement.ParentObject.Physics.CurrentCell == null || pistonPressElement.ParentObject.Physics.CurrentCell != ParentObject.Physics.currentCell.GetCellFromDirection(key))
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(key);
					}
				}
				list?.ForEach((string d) =>
				{
					elements.Remove(d);
				});
			}
			Counter++;
			if (elements != null && elements.Count != 0)
			{
				if (Counter < Frequency - 1)
				{
					if (centerElement != null)
					{
						centerElement.Destroy();
						centerElement = null;
					}
					foreach (KeyValuePair<string, PistonPressElement> element in elements)
					{
						element.Value.ParentObject.GetPart<Render>().RenderString = pressGlyph[element.Key];
						element.Value.ParentObject.GetPart<Render>().Tile = pressTile[element.Key];
						element.Value.Danger = false;
					}
					ParentObject.Render.Visible = false;
				}
				else if (Counter == Frequency - 1)
				{
					if (centerElement != null)
					{
						centerElement.Destroy();
						centerElement = null;
					}
					foreach (KeyValuePair<string, PistonPressElement> element2 in elements)
					{
						element2.Value.ParentObject.GetPart<Render>().RenderString = pressGlyph[element2.Key];
						element2.Value.ParentObject.GetPart<Render>().Tile = pressTile[element2.Key];
						element2.Value.Danger = true;
					}
					ParentObject.Render.Visible = false;
				}
				else if (Counter >= Frequency)
				{
					Counter = 0;
					if (elements.Any((KeyValuePair<string, PistonPressElement> v) => v.Value.ParentObject.IsVisible()))
					{
						ParentObject.PistonPuff("&y", 6);
					}
					foreach (KeyValuePair<string, PistonPressElement> element3 in elements)
					{
						element3.Value.ParentObject.GetPart<Render>().RenderString = extendGlyph[element3.Key];
						element3.Value.ParentObject.GetPart<Render>().Tile = extendTile[element3.Key];
						element3.Value.Danger = false;
					}
					ParentObject.Render.Visible = true;
					ParentObject.Render.RenderString = centerGlyph[Directions];
					ParentObject.Render.Tile = centerTile[Directions];
					if (centerElement == null)
					{
						centerElement = ParentObject.CurrentCell.AddObject("PistonPressElement");
					}
					if (centerElement != null)
					{
						centerElement.Render.RenderString = centerGlyph[Directions];
						centerElement.Render.Tile = centerTile[Directions];
					}
					int phase = ParentObject.GetPhase();
					List<GameObject> list2 = Event.NewGameObjectList();
					list2.AddRange(ParentObject.CurrentCell.Objects);
					list2.RemoveAll((GameObject o) => o.Render == null || o.Render.RenderLayer <= 1 || o.Physics == null || !o.Physics.IsReal);
					if (elements.Count == 1)
					{
						if (centerElement != null)
						{
							centerElement.Render.RenderString = centerGlyphOne[elements.Keys.FirstOrDefault()];
							centerElement.Render.Tile = centerTileOne[elements.Keys.FirstOrDefault()];
						}
						ParentObject.Render.RenderString = centerGlyphOne[elements.Keys.FirstOrDefault()];
						ParentObject.Render.Tile = centerTileOne[elements.Keys.FirstOrDefault()];
						string oppositeDirection = XRL.Rules.Directions.GetOppositeDirection(elements.Keys.FirstOrDefault());
						foreach (GameObject item in list2)
						{
							Cudgel_Slam.Cast(elements.Values.FirstOrDefault().ParentObject, null, oppositeDirection, item, requireWeapon: false, 100, "1d10");
						}
					}
					else
					{
						foreach (GameObject item2 in list2)
						{
							if (item2 != ParentObject && item2.Physics != null && item2.Render != null && item2.Render.RenderLayer > 5)
							{
								item2.TakeDamage(Stat.Random(80, 120), "from being crushed by a machine press.", "Bludgeoning", null, null, null, ParentObject, null, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, UsePopups: false, phase);
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
