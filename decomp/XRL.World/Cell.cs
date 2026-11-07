using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using XRL.Collections;
using XRL.Core;
using XRL.Liquids;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.World;

/// <summary>
/// Base type representing a single spatial cell ("square" or "space"
/// in player-facing terminology) within a zone.
/// </summary>
public class Cell : IRenderable, IInventory
{
	public class ObjectRack : Rack<GameObject>
	{
		[NonSerialized]
		public Cell ParentCell;

		public ObjectRack()
		{
		}

		public ObjectRack(Cell ParentCell)
		{
			this.ParentCell = ParentCell;
		}

		public bool WantEvent(int ID, int Cascade)
		{
			if (ID == PooledEvent<GetContentsEvent>.ID)
			{
				return true;
			}
			int i = 0;
			for (int length = Length; i < length; i++)
			{
				if (Items[i].WantEvent(ID, Cascade))
				{
					return true;
				}
			}
			return false;
		}

		public bool HandleEvent(MinEvent E)
		{
			if (E is GetContentsEvent getContentsEvent)
			{
				for (int i = 0; i < Length; i++)
				{
					getContentsEvent.Objects.Add(Items[i]);
				}
			}
			int num = -1;
			int length = Length;
			int cascadeLevel = E.GetCascadeLevel();
			for (int j = 0; j < length; j++)
			{
				if (Items[j].WantEvent(E.ID, cascadeLevel))
				{
					num = j;
					break;
				}
			}
			if (num != -1)
			{
				for (int k = num; k < length; k++)
				{
					GameObject gameObject = Items[k];
					if (!E.Dispatch(gameObject))
					{
						return false;
					}
					if (length != Length)
					{
						length = Length;
						if (k < length && Items[k] != gameObject)
						{
							k--;
						}
					}
				}
			}
			return true;
		}

		public override void Add(GameObject Item)
		{
			base.Add(Item);
			if (ParentCell.Live && (Item.Physics == null || Item.Physics._CurrentCell != ParentCell))
			{
				ParentCell.LogInvalidPhysics(Item);
			}
		}
	}

	public struct SpiralEnumerator
	{
		private Zone Zone;

		private Cell Cell;

		private int X;

		private int Y;

		private int StepX;

		private int StepY;

		private int Position;

		private int Length;

		private int Count;

		private bool LocalOnly;

		private bool BuiltOnly;

		public Cell Current => Cell;

		public SpiralEnumerator(Cell Origin, int Radius = 1, bool IncludeSelf = false, bool LocalOnly = false, bool BuiltOnly = true)
		{
			Zone = Origin.ParentZone;
			Cell = Origin;
			X = Origin.X;
			Y = Origin.Y;
			StepX = 1;
			StepY = 0;
			Position = 0;
			Length = 1;
			Radius = Radius * 2 + 1;
			Count = Radius * Radius;
			this.LocalOnly = LocalOnly;
			this.BuiltOnly = BuiltOnly;
			if (!IncludeSelf)
			{
				X += StepX;
				Y += StepY;
				Position++;
				Count--;
			}
		}

		public SpiralEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			do
			{
				if (--Count < 0)
				{
					Cell = null;
					return false;
				}
				if (Position >= Length)
				{
					int stepX = StepX;
					StepX = -StepY;
					StepY = stepX;
					Position = 0;
					if (StepY == 0)
					{
						Length++;
					}
				}
				Cell = Zone.GetCellGlobal(X, Y, LocalOnly, BuiltOnly);
				X += StepX;
				Y += StepY;
				Position++;
			}
			while (Cell == null);
			return true;
		}
	}

	[NonSerialized]
	private static RenderEvent eRender = new RenderEvent();

	[NonSerialized]
	private static Color ColorBlack = The.Color.DarkBlack;

	[NonSerialized]
	private static Color ColorBrightCyan = new Color(0f, 1f, 1f);

	[NonSerialized]
	private static Color ColorBrightGreen = new Color(0f, 0f, 1f);

	[NonSerialized]
	private static Color ColorBrightRed = new Color(1f, 0f, 0f);

	[NonSerialized]
	private static Color ColorDarkCyan = new Color(0f, 0.5f, 0.5f);

	[NonSerialized]
	private static Color ColorDarkGreen = new Color(0f, 0.5f, 0f);

	[NonSerialized]
	private static Color ColorDarkRed = new Color(0.5f, 0f, 0f);

	[NonSerialized]
	private static Color ColorGray = The.Color.Gray;

	public const int DISTANCE_INDEFINITE = 9999999;

	private const int FLAG_RENDER_INITIALIZED = 1;

	private const int FLAG_RENDER_COMPONENT = 2;

	private const int FLAG_RENDER_OVERLAY = 4;

	private const int FLAG_RENDER_FINAL = 8;

	public string PaintRenderString;

	public string PaintTileColor;

	public string PaintColorString;

	public string PaintTile;

	public string PaintDetailColor;

	[NonSerialized]
	internal bool Live;

	[NonSerialized]
	private int TransientCache;

	private static StringBuilder addressBuilder = new StringBuilder();

	public int X;

	public int Y;

	public Zone ParentZone;

	public ObjectRack Objects;

	public List<string> SemanticTags;

	public string _GroundLiquid;

	public const int DEFAULT_ALPHA = 230;

	[NonSerialized]
	private bool minimapCacheValid;

	[NonSerialized]
	public Color32 minimapCacheColor;

	[NonSerialized]
	public bool lastMinimapVisibile;

	[NonSerialized]
	public bool lastMinimapLit;

	[NonSerialized]
	public bool lastMinimapExplored;

	[NonSerialized]
	private static Color32 WhiteAlphaD = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 230);

	[NonSerialized]
	private static Color32 BlackAlpha32 = new Color32(0, 0, 0, 32);

	[NonSerialized]
	private static Color32 BlackAlpha128 = new Color32(0, 0, 0, 128);

	[NonSerialized]
	private static Color32 BlackAlpha164 = new Color32(0, 0, 0, 164);

	[NonSerialized]
	private static Color32 VioletAlphaD = new Color32(byte.MaxValue, 0, byte.MaxValue, 230);

	[NonSerialized]
	private static Color32 RedAlphaD = new Color32(byte.MaxValue, 0, 0, 230);

	[NonSerialized]
	private static Color32 GreenAlphaD = new Color32(0, byte.MaxValue, 0, 230);

	[NonSerialized]
	private static Color32 DarkYellowAlphaD = new Color32(128, 128, 0, 230);

	[NonSerialized]
	private static Color32 DarkBlueAlphaD = new Color32(0, 0, 128, 230);

	[NonSerialized]
	private static Color32 GrayAlphaD = new Color32(128, 128, 128, 230);

	[NonSerialized]
	private static Color32 CanaryAlphaD = new Color32(byte.MaxValue, byte.MaxValue, 128, 230);

	[NonSerialized]
	public Dictionary<int, int> NavigationWeightCache = new Dictionary<int, int>();

	public static readonly Color32 INVALID_CACHE = new Color32(1, 2, 3, 4);

	[NonSerialized]
	private static Event eBeforePhysicsRejectObjectEntringCell = new Event("BeforePhysicsRejectObjectEntringCell", "Object", null);

	public int OccludeCache = -1;

	private static List<GameObject> EventList = new List<GameObject>(8);

	private static bool EventListInUse = false;

	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListCardinalFirst = new string[8] { "N", "E", "S", "W", "NW", "SE", "SW", "NE" };

	public static string[] DirectionListCardinalOnly = new string[4] { "N", "E", "S", "W" };

	[NonSerialized]
	private List<Cell> _LocalAdjacentCellCache;

	[NonSerialized]
	private List<Cell> _LocalCardinalAdjacentCellCache;

	[NonSerialized]
	internal static bool AskedReport = false;

	public Point2D Pos2D => new Point2D(X, Y);

	public Point Point => new Point(X, Y);

	public Location2D Location => Location2D.Get(X, Y);

	public int LocalCoordKey => (X << 16) | Y;

	public string DebugName => X + "," + Y;

	public int RenderedObjectsCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < Objects.Count; i++)
			{
				if (Objects[i].Render != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public string GroundLiquid
	{
		get
		{
			if (_GroundLiquid != null)
			{
				return _GroundLiquid;
			}
			if (ParentZone != null)
			{
				return ParentZone.GroundLiquid;
			}
			return null;
		}
		set
		{
			_GroundLiquid = value;
		}
	}

	public bool Explored => ParentZone.GetExplored(X, Y);

	public bool InActiveZone
	{
		get
		{
			if (ParentZone != null)
			{
				return ParentZone.IsActive();
			}
			return false;
		}
	}

	public bool juiceEnabled => Options.UseOverlayCombatEffects;

	[Obsolete("Use Location")]
	public Location2D location => Location2D.Get(X, Y);

	public int GetCourseFrom(Cell Target)
	{
		int num = 0;
		if (Target == null || Target == this)
		{
			return num;
		}
		Zone parentZone = Target.ParentZone;
		int num2;
		int num3;
		int num4;
		int num5;
		if (parentZone != ParentZone)
		{
			if (parentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return num;
			}
			num2 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			num3 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			num4 = parentZone.GetZonewX() * Definitions.Width * 80 + parentZone.GetZoneX() * 80 + Target.X;
			num5 = parentZone.GetZonewY() * Definitions.Height * 25 + parentZone.GetZoneY() * 25 + Target.Y;
		}
		else
		{
			num2 = X;
			num3 = Y;
			num4 = Target.X;
			num5 = Target.Y;
		}
		if (num2 < num4)
		{
			num |= 2;
		}
		else if (num2 > num4)
		{
			num |= 8;
		}
		if (num3 < num5)
		{
			num |= 4;
		}
		else if (num3 > num5)
		{
			num |= 1;
		}
		return num;
	}

	public int GetCourseTo(Cell Target)
	{
		return Courses.Opposite[GetCourseFrom(Target)];
	}

	public int GetGeneralCourseFrom(Cell Target)
	{
		int num = 0;
		if (Target == null)
		{
			return num;
		}
		Zone parentZone = Target.ParentZone;
		int num2;
		int num3;
		int num4;
		int num5;
		if (parentZone != ParentZone)
		{
			if (parentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return num;
			}
			num2 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			num3 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			num4 = parentZone.GetZonewX() * Definitions.Width * 80 + parentZone.GetZoneX() * 80 + Target.X;
			num5 = parentZone.GetZonewY() * Definitions.Height * 25 + parentZone.GetZoneY() * 25 + Target.Y;
		}
		else
		{
			num2 = X;
			num3 = Y;
			num4 = Target.X;
			num5 = Target.Y;
		}
		int num6 = Math.Abs(num2 - num4);
		int num7 = Math.Abs(num3 - num5);
		if (num6 * 2 < num7)
		{
			if (num3 < num5)
			{
				num |= 4;
			}
			else if (num3 > num5)
			{
				num |= 1;
			}
		}
		else if (num7 * 2 < num6)
		{
			if (num2 < num4)
			{
				num |= 2;
			}
			else if (num2 > num4)
			{
				num |= 8;
			}
		}
		else
		{
			if (num3 < num5)
			{
				num |= 4;
			}
			else if (num3 > num5)
			{
				num |= 1;
			}
			if (num2 < num4)
			{
				num |= 2;
			}
			else if (num2 > num4)
			{
				num |= 8;
			}
		}
		return num;
	}

	public int GetGeneralCourseTo(Cell Target)
	{
		return Courses.Opposite[GetGeneralCourseFrom(Target)];
	}

	public void Render(RenderEvent E, bool Visible = true, LightLevel Lit = LightLevel.Light, bool Explored = true, bool Alt = false, bool DisableFullscreenColorEffects = false, List<GameObject> WantsToPaint = null)
	{
		E.Reset();
		E.DisableFullscreenColorEffects = DisableFullscreenColorEffects;
		E.RenderString = (Explored ? "." : " ");
		E.Lit = Lit;
		E.Alt = Alt;
		E.Visible = Visible;
		bool flag = Globals.RenderMode == RenderModeType.Tiles;
		GameObject player = The.Player;
		Cell currentCell = player.CurrentCell;
		GameObject[] array = Objects.GetArray();
		int count = Objects.Count;
		bool flag2 = false;
		bool flag3 = (int)Lit > 1;
		int num = TransientCache;
		if ((num & 1) == 0)
		{
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = array[i];
				if ((num & 2) == 0 && gameObject.HasRegisteredEvent(RenderEvent.ID))
				{
					num |= 2;
				}
				if ((num & 4) == 0 && gameObject.HasRegisteredEvent(RenderEvent.OverlayID))
				{
					num |= 4;
				}
				if ((num & 8) == 0 && gameObject.HasRegisteredEvent(RenderEvent.FinalID))
				{
					num |= 8;
				}
			}
			num = (TransientCache = num | 1);
		}
		GameObject gameObject2;
		Render render2;
		bool flag7;
		bool flag8;
		if (Explored)
		{
			if (Alt)
			{
				E.DetailColor = "k";
				E.ColorString = "&k";
				E.BackgroundString = "^k";
			}
			else if (XRLCore.RenderFloorTextures)
			{
				if (flag && !PaintTile.IsNullOrEmpty())
				{
					E.Tile = PaintTile;
				}
				if (!PaintRenderString.IsNullOrEmpty())
				{
					E.RenderString = PaintRenderString;
				}
				if (Visible)
				{
					if (flag && !PaintTileColor.IsNullOrEmpty())
					{
						E.ColorString = PaintTileColor;
					}
					else if (!PaintColorString.IsNullOrEmpty())
					{
						E.ColorString = PaintColorString;
					}
					if (!PaintDetailColor.IsNullOrEmpty())
					{
						E.DetailColor = PaintDetailColor;
					}
				}
			}
			bool flag4 = flag3 & Visible;
			bool flag5 = currentCell != null && currentCell.ParentZone == ParentZone && currentCell.DistanceTo(X, Y) <= 1;
			gameObject2 = null;
			for (int j = 0; j < count; j++)
			{
				GameObject gameObject3 = array[j];
				Render render = gameObject3.Render;
				if (render == null || render.Never)
				{
					continue;
				}
				render.PartyFlip = gameObject3 == player || gameObject3.PartyLeader?.Render?.PartyFlip == true;
				bool flag6 = XRLCore.RenderHiddenPlayer && gameObject3.IsPlayer();
				if ((!flag2 | flag6) || (flag5 ? gameObject3.ConsiderSolidInRenderingContextFor(player) : gameObject3.ConsiderSolidInRenderingContext()))
				{
					if (render.CustomRender && gameObject3.HasRegisteredEvent("CustomRender"))
					{
						gameObject3.FireEvent(Event.New("CustomRender", "RenderEvent", E));
					}
					if (flag6 || (render.Visible && (flag4 || render.RenderIfDark)))
					{
						if (render.RenderLayer >= E.HighestLayer)
						{
							gameObject2 = gameObject3;
							E.HighestLayer = render.RenderLayer;
						}
						gameObject3.Seen();
						if (gameObject3 == Sidebar.CurrentTarget)
						{
							XRLCore.CludgeTargetRendered = true;
						}
						if (!flag2 && (flag5 ? gameObject3.ConsiderSolidInRenderingContextFor(player) : gameObject3.ConsiderSolidInRenderingContext()))
						{
							flag2 = true;
						}
					}
					if (WantsToPaint != null && (gameObject3.Flags & 0x80) != 0)
					{
						WantsToPaint.Add(gameObject3);
					}
				}
				if (Alt && (gameObject3.Flags & 0x100) != 0)
				{
					break;
				}
			}
			if (gameObject2 != null)
			{
				if (flag2 && count > 1 && (Lit == LightLevel.Radar || Lit == LightLevel.LitRadar) && XRLCore.CurrentFrame10 % 125 >= 95)
				{
					gameObject2 = GetRadarReveal(array, count) ?? gameObject2;
				}
				render2 = gameObject2.Render;
				ParentZone.RenderedObjects++;
				if (XRLCore.RenderFloorTextures || render2.RenderLayer > 0)
				{
					if (!flag3 && gameObject2 == player)
					{
						E.RenderString = render2.RenderString;
						if (flag && !string.IsNullOrEmpty(render2.TileColor))
						{
							E.ColorString = render2.TileColor;
						}
						else
						{
							E.ColorString = render2.ColorString;
						}
						E.HighestLayer = render2.RenderLayer;
						if (flag)
						{
							E.Tile = render2.Tile;
							E.HFlip = render2.getHFlip();
							E.VFlip = render2.getVFlip();
							E.DetailColor = "y";
						}
						E.WantsToPaint = false;
						if ((num & 2) != 0)
						{
							gameObject2.ComponentRender(E);
						}
						if (WantsToPaint != null && E.WantsToPaint)
						{
							WantsToPaint.Add(gameObject2);
						}
					}
					else if (flag4 || render2.RenderIfDark)
					{
						if (flag)
						{
							E.Tile = render2.Tile;
							E.HFlip = render2.getHFlip();
							E.VFlip = render2.getVFlip();
						}
						if (Alt)
						{
							RenderAlt(E, player, gameObject2, render2);
							goto IL_0580;
						}
						E.RenderString = render2.RenderString;
						E.DetailColor = render2.DetailColor;
						if (flag && !string.IsNullOrEmpty(render2.TileColor))
						{
							E.ColorString = render2.TileColor;
						}
						else
						{
							E.ColorString = render2.ColorString;
						}
						flag7 = gameObject2 == Sidebar.CurrentTarget;
						if (flag7)
						{
							int currentFrame = XRLCore.CurrentFrame;
							if (currentFrame > 30)
							{
								if (currentFrame < 45)
								{
									goto IL_0566;
								}
							}
							else if (currentFrame < 15)
							{
								goto IL_0566;
							}
							flag8 = false;
							goto IL_056e;
						}
						goto IL_0572;
					}
				}
			}
		}
		goto IL_05d8;
		IL_0580:
		E.HighestLayer = render2.RenderLayer;
		E.WantsToPaint = false;
		if (Alt && (num & 4) != 0)
		{
			gameObject2.OverlayRender(E);
		}
		else if ((num & 2) != 0)
		{
			gameObject2.ComponentRender(E);
		}
		if (WantsToPaint != null && E.WantsToPaint && !WantsToPaint.Contains(gameObject2))
		{
			WantsToPaint.Add(gameObject2);
		}
		goto IL_05d8;
		IL_0572:
		if (flag7)
		{
			RenderTarget(E, player, gameObject2);
		}
		goto IL_0580;
		IL_0566:
		flag8 = true;
		goto IL_056e;
		IL_05d8:
		if ((num & 8) != 0)
		{
			for (int k = 0; k < count; k++)
			{
				GameObject gameObject4 = array[k];
				E.WantsToPaint = false;
				gameObject4.FinalRender(E);
				if (WantsToPaint != null && E.WantsToPaint && !WantsToPaint.Contains(gameObject4))
				{
					WantsToPaint.Add(gameObject4);
				}
			}
		}
		if (E.RenderString.Length == 1)
		{
			if (E.RenderString[0] == '^')
			{
				E.RenderString = "^^";
			}
			else if (E.RenderString[0] == '&')
			{
				E.RenderString = "&&";
			}
		}
		if (Alt || E.CustomDraw)
		{
			return;
		}
		if (!Visible || !flag3)
		{
			E.ColorString = "&K";
			if (flag)
			{
				E.DetailColor = "k";
			}
		}
		else
		{
			if (DisableFullscreenColorEffects)
			{
				return;
			}
			switch (Lit)
			{
			case LightLevel.Safelight:
				E.ColorString = "&r";
				E.DetailColor = "R";
				break;
			case LightLevel.Radar:
			case LightLevel.LitRadar:
			{
				int currentFrame2 = XRLCore.CurrentFrame;
				if (currentFrame2 >= 27 && currentFrame2 <= 44 && BlocksRadar())
				{
					E.ColorString = "&R";
					E.DetailColor = "r";
				}
				else if (Lit == LightLevel.Radar)
				{
					E.ColorString = "&C";
					E.DetailColor = "c";
				}
				break;
			}
			}
		}
		return;
		IL_056e:
		flag7 = flag8;
		goto IL_0572;
	}

	private GameObject GetRadarReveal(GameObject[] Objects, int Count)
	{
		GameObject result = null;
		int i = 0;
		int num = -1;
		for (; i < Count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Physics != null && !gameObject.Physics.Solid && gameObject.IsReal && gameObject.Render != null && gameObject.Render.Visible && gameObject.Render.RenderLayer > num && gameObject.Render.RenderLayer > 0)
			{
				num = gameObject.Render.RenderLayer;
				result = gameObject;
			}
		}
		return result;
	}

	private void RenderTarget(RenderEvent E, GameObject Player, GameObject Object)
	{
		Brain brain = Object.Brain;
		if (Object.IsPlayer())
		{
			E.BackgroundString = "^B";
			return;
		}
		if (brain != null)
		{
			GameObject partyLeader = brain.PartyLeader;
			if (partyLeader != null && partyLeader.IsPlayer())
			{
				E.BackgroundString = "^b";
				return;
			}
		}
		if (brain != null && brain.IsHostileTowards(Player))
		{
			E.BackgroundString = "^r";
		}
		else
		{
			E.BackgroundString = "^g";
		}
	}

	private void RenderAlt(RenderEvent E, GameObject Player, GameObject Object, Render Render)
	{
		E.RenderString = "Û";
		string propertyOrTag = Object.GetPropertyOrTag("OverlayColor");
		if (propertyOrTag != null)
		{
			E.RenderString = Render.RenderString;
			E.ColorString = propertyOrTag;
			E.BackgroundString = "^k";
			if (E.ColorString.Length > 1)
			{
				E.DetailColor = E.ColorString.Substring(1, 1);
			}
		}
		else
		{
			E.ColorString = "&k";
			E.BackgroundString = "^k";
			E.DetailColor = "k";
		}
		string stringProperty = Object.GetStringProperty("OverlayDetailColor");
		if (stringProperty != null)
		{
			E.DetailColor = stringProperty;
		}
		string stringProperty2 = Object.GetStringProperty("OverlayRenderString");
		if (stringProperty2 != null)
		{
			E.RenderString = stringProperty2;
		}
		string stringProperty3 = Object.GetStringProperty("OverlayTile");
		if (stringProperty3 != null)
		{
			E.Tile = stringProperty3;
		}
		Tinkering_Mine Part;
		if (Object.Brain != null && Player != null)
		{
			Brain brain = Object.Brain;
			if (Object.IsPlayer())
			{
				E.BackgroundString = "^k";
				E.ColorString = "&B";
				E.DetailColor = "B";
				return;
			}
			GameObject partyLeader = brain.PartyLeader;
			if (partyLeader != null && partyLeader.IsPlayer())
			{
				E.BackgroundString = "^k";
				E.ColorString = "&b";
				E.DetailColor = "b";
			}
			else if (brain.IsHostileTowards(Player))
			{
				E.RenderString = Render.RenderString;
				E.ColorString = "&R";
				E.BackgroundString = "^k";
				E.DetailColor = "R";
			}
			else
			{
				E.RenderString = Render.RenderString;
				E.ColorString = "&G";
				E.BackgroundString = "^k";
				E.DetailColor = "G";
			}
		}
		else if (Object.TryGetPart<Tinkering_Mine>(out Part))
		{
			if (Part.Timer != -1 || Part.ConsiderHostile(Player))
			{
				E.RenderString = Render.RenderString;
				E.ColorString = "&R";
				E.BackgroundString = "^k";
				E.DetailColor = "R";
			}
			else
			{
				E.RenderString = Render.RenderString;
				E.ColorString = "&G";
				E.BackgroundString = "^k";
				E.DetailColor = "G";
			}
		}
		else if (Player != null)
		{
			Harvestable Part2;
			if (Player.HasPart<TrashRifling>() && Object.HasPart<Garbage>())
			{
				E.RenderString = Render.RenderString;
				E.DetailColor = "w";
				E.ColorString = "&w";
				E.BackgroundString = "^k";
			}
			else if (Player.HasPart<CookingAndGathering_Harvestry>() && Object.TryGetPart<Harvestable>(out Part2) && Part2 != null && Part2.Ripe)
			{
				E.RenderString = Render.RenderString;
				E.DetailColor = "w";
				E.ColorString = "&w";
				E.BackgroundString = "^k";
			}
		}
	}

	public RenderEvent Render(bool Visible = true, LightLevel Lit = LightLevel.Light, bool Explored = true, bool Alt = false, bool DisableFullscreenColorEffects = false, List<GameObject> WantsToPaint = null)
	{
		RenderEvent renderEvent = eRender;
		Render(renderEvent, Visible, Lit, Explored, Alt, DisableFullscreenColorEffects, WantsToPaint);
		return renderEvent;
	}

	[Obsolete("ConsoleChar is no longer provided directly")]
	public RenderEvent Render(ConsoleChar Char, bool Visible, LightLevel Lit, bool Explored, bool Alt, bool DisableFullscreenColorEffects = false, List<GameObject> WantsToPaint = null)
	{
		RenderEvent renderEvent = eRender;
		Render(renderEvent, Visible, Lit, Explored, Alt, DisableFullscreenColorEffects, WantsToPaint);
		return renderEvent;
	}

	[Obsolete]
	public void RenderSound(ConsoleChar Char, ConsoleChar[,] Buffer)
	{
	}

	public void PlayWorldSound(string Clip, float Volume = 0.5f, float PitchVariance = 0f, bool Combat = false, float Delay = 0f, float Pitch = 1f, float CostMultiplier = 1f, int CostMaximum = int.MaxValue)
	{
		if (!Clip.IsNullOrEmpty() && Options.Sound && (!Combat || Options.UseCombatSounds) && ParentZone != null && ParentZone.IsActive() && TryGetSoundPropagation(Mathf.RoundToInt(105f * Volume), out var Cost, out var Occluded))
		{
			Cost = Mathf.RoundToInt((float)Cost * CostMultiplier);
			Cost = Math.Min(Cost, CostMaximum);
			SoundManager.PlayWorldSound(Clip, Cost, Occluded, Volume, Location, PitchVariance, Delay, Pitch);
		}
	}

	public void CheckSoundWall(GameObject Object, int Expected)
	{
		if (!Zone.SoundMapDirty && Zone.SoundWalls[X, Y] != Expected && Object.IsWall())
		{
			Zone.SoundMapDirty = true;
		}
	}

	public bool TryGetSoundPropagation(int Range, out int Cost, out bool Occluded)
	{
		Zone parentZone = ParentZone;
		Cell playerCell = The.PlayerCell;
		if (playerCell == null || playerCell.ParentZone != parentZone || (!parentZone.IsWorldMap() && PathDistanceTo(playerCell) > Range))
		{
			Cost = int.MaxValue;
			Occluded = true;
			return false;
		}
		if (Zone.SoundMapDirty)
		{
			parentZone.UpdateSoundMap();
		}
		Location2D p = Location;
		Occluded = !IsVisible();
		Cost = Zone.SoundMap.GetCostAtPoint(p);
		if (Cost == int.MaxValue)
		{
			Cost = Zone.SoundMap.GetCostFromPointDirection(p, Zone.SoundMap.GetLowestCostDirectionFrom(p));
		}
		return true;
	}

	public Cell()
	{
		Objects = new ObjectRack(this);
	}

	public Cell(Zone ParentZone)
		: this()
	{
		this.ParentZone = ParentZone;
	}

	public bool AllInDirections(IEnumerable<string> directions, int distance, Predicate<Cell> test)
	{
		foreach (string direction in directions)
		{
			if (!AllInDirection(direction, distance, test))
			{
				return false;
			}
		}
		return true;
	}

	public GlobalLocation GetGlobalLocation()
	{
		return new GlobalLocation(this);
	}

	public bool AnyInDirection(string Direction, int Distance, Predicate<Cell> Test)
	{
		Cell cell = this;
		for (int i = 0; i < Distance; i++)
		{
			cell = cell.GetCellFromDirection(Direction);
			if (cell == null)
			{
				return false;
			}
			if (Test(cell))
			{
				return true;
			}
		}
		return false;
	}

	public bool AllInDirection(string direction, int distance, Predicate<Cell> test)
	{
		Cell cell = this;
		for (int i = 0; i < distance; i++)
		{
			cell = cell.GetCellFromDirection(direction);
			if (!test(cell))
			{
				return false;
			}
			if (cell == null)
			{
				break;
			}
		}
		return true;
	}

	public string GetAddress()
	{
		addressBuilder.Length = 0;
		addressBuilder.Append(ParentZone.ZoneID).Append('@').Append(X)
			.Append(',')
			.Append(Y);
		return addressBuilder.ToString();
	}

	public void walk(Func<Cell, Cell> next, Predicate<Cell> walker)
	{
		if (walker(this))
		{
			next(this)?.walk(next, walker);
		}
	}

	public static Cell FromAddress(string CellAddress)
	{
		try
		{
			string[] array = CellAddress.Split('@');
			string[] array2 = array[1].Split(',');
			return XRLCore.Core.Game.ZoneManager.GetZone(array[0]).GetCell(int.Parse(array2[0]), int.Parse(array2[1]));
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
			return null;
		}
	}

	public bool ConsideredOutside()
	{
		if (ParentZone.IsOutside())
		{
			return true;
		}
		if (HasObject("FlyingWhitelistArea"))
		{
			return true;
		}
		return false;
	}

	public bool HasSemanticTag(string tag)
	{
		if (SemanticTags != null)
		{
			return SemanticTags.Any((string t) => t.EqualsNoCase(tag));
		}
		return false;
	}

	public void AddSemanticTag(string tag)
	{
		if (SemanticTags == null)
		{
			SemanticTags = new List<string>();
		}
		if (!HasSemanticTag(tag))
		{
			SemanticTags.Add(tag);
		}
	}

	public void RemoveSemanticTag(string tag)
	{
		if (SemanticTags != null && HasSemanticTag(tag))
		{
			SemanticTags.Remove(tag);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(X);
		stringBuilder.Append(",");
		stringBuilder.Append(Y);
		stringBuilder.Append(" Objects:");
		foreach (GameObject @object in Objects)
		{
			stringBuilder.Append(@object.ToString() + " ");
		}
		return stringBuilder.ToString();
	}

	public LightLevel GetLight()
	{
		return ParentZone?.GetLight(X, Y) ?? LightLevel.None;
	}

	internal void SetMinimapColorFrom(Color32 c)
	{
		minimapCacheValid = true;
		minimapCacheColor.r = c.r;
		minimapCacheColor.g = c.g;
		minimapCacheColor.b = c.b;
		minimapCacheColor.a = c.a;
	}

	public void RefreshMinimapColor()
	{
		if (lastMinimapVisibile != IsVisible())
		{
			lastMinimapVisibile = IsVisible();
			minimapCacheValid = false;
		}
		if (lastMinimapLit != IsLit())
		{
			lastMinimapVisibile = IsVisible();
		}
		if (lastMinimapExplored != IsExplored())
		{
			lastMinimapVisibile = IsVisible();
		}
		if (minimapCacheValid)
		{
			return;
		}
		if (this == The.Player?.GetCurrentCell())
		{
			SetMinimapColorFrom(WhiteAlphaD);
			return;
		}
		if (!IsExplored())
		{
			SetMinimapColorFrom(BlackAlpha32);
			return;
		}
		if (IsLit())
		{
			SetMinimapColorFrom(BlackAlpha164);
		}
		else
		{
			SetMinimapColorFrom(BlackAlpha128);
		}
		GameObject firstObjectWithPropertyOrTag;
		if (ParentZone.IsWorldMap())
		{
			if ((firstObjectWithPropertyOrTag = GetFirstObjectWithPropertyOrTag("MinimapColor")) != null)
			{
				SetMinimapColorFrom(ConsoleLib.Console.ColorUtility.ColorFromString(firstObjectWithPropertyOrTag.GetPropertyOrTag("MinimapColor")));
			}
			else
			{
				SetMinimapColorFrom(BlackAlpha32);
			}
		}
		else if ((HasObjectWithPart("StairsUp") || HasObjectWithPart("StairsDown")) && !HasObjectWithPart("HiddenRender"))
		{
			SetMinimapColorFrom(VioletAlphaD);
		}
		else if (IsVisible() && IsLit() && HasObjectWithPart("Combat"))
		{
			GameObject firstObjectWithPart = GetFirstObjectWithPart("Combat");
			if (firstObjectWithPart.Brain != null)
			{
				if (firstObjectWithPart.Brain.IsHostileTowards(The.Player))
				{
					SetMinimapColorFrom(RedAlphaD);
				}
				else
				{
					SetMinimapColorFrom(GreenAlphaD);
				}
			}
			else
			{
				SetMinimapColorFrom(RedAlphaD);
			}
		}
		else if (HasObjectWithTag("Chest"))
		{
			SetMinimapColorFrom(DarkYellowAlphaD);
		}
		else if (HasObjectWithTag("LiquidVolume"))
		{
			SetMinimapColorFrom(DarkBlueAlphaD);
		}
		else if (HasWall())
		{
			SetMinimapColorFrom(GrayAlphaD);
		}
		else if (HasObjectWithTag("Door"))
		{
			SetMinimapColorFrom(CanaryAlphaD);
		}
		else if ((firstObjectWithPropertyOrTag = GetFirstObjectWithPropertyOrTag("MinimapColor")) != null)
		{
			SetMinimapColorFrom(ConsoleLib.Console.ColorUtility.ColorFromString(firstObjectWithPropertyOrTag.GetPropertyOrTag("MinimapColor")));
		}
	}

	public List<GameObject> FastFloodSearch(string SearchPart, int Radius)
	{
		return ParentZone.FastFloodSearch(X, Y, Radius, SearchPart);
	}

	public List<GameObject> FastFloodVisibility(string SearchPart, int Radius)
	{
		return ParentZone.FastFloodVisibility(X, Y, Radius, SearchPart);
	}

	public static Cell Load(SerializationReader Reader, int x, int y, Zone ParentZone)
	{
		Cell cell = new Cell(ParentZone);
		cell.X = x;
		cell.Y = y;
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = Reader.ReadGameObject();
			cell.Objects.Add(gameObject);
			Reader.Locations[gameObject] = cell;
		}
		num = Reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			cell.AddObjectWithoutEvents(GameObject.CreateUnmodified(Reader.ReadString()));
		}
		cell.PaintTile = Reader.ReadString();
		cell.PaintTileColor = Reader.ReadString();
		cell.PaintRenderString = Reader.ReadString();
		cell.PaintColorString = Reader.ReadString();
		cell.PaintDetailColor = Reader.ReadString();
		cell.Live = true;
		int num2 = Reader.ReadInt32();
		if (num2 > 0)
		{
			cell.SemanticTags = new List<string>(num2);
			for (int k = 0; k < num2; k++)
			{
				cell.SemanticTags.Add(Reader.ReadString());
			}
		}
		return cell;
	}

	public bool ShouldWrite(GameObject Object)
	{
		return true;
	}

	public void Save(SerializationWriter Writer)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < Objects.Count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Physics == null || gameObject.Physics._CurrentCell != this)
			{
				LogInvalidPhysics(gameObject);
				Objects.RemoveAt(i--);
			}
			else if (!ShouldWrite(gameObject))
			{
				num2++;
			}
			else
			{
				num++;
			}
		}
		Writer.Write(num);
		int j = 0;
		for (int count = Objects.Count; j < count; j++)
		{
			GameObject gameObject2 = Objects[j];
			if (ShouldWrite(gameObject2))
			{
				Writer.WriteGameObject(gameObject2);
			}
		}
		Writer.Write(num2);
		int k = 0;
		for (int count2 = Objects.Count; k < count2; k++)
		{
			GameObject gameObject3 = Objects[k];
			if (!ShouldWrite(gameObject3))
			{
				Writer.Write(gameObject3.Blueprint);
			}
		}
		Writer.Write(PaintTile);
		Writer.Write(PaintTileColor);
		Writer.Write(PaintRenderString);
		Writer.Write(PaintColorString);
		Writer.Write(PaintDetailColor);
		if (SemanticTags == null)
		{
			Writer.Write(0);
			return;
		}
		Writer.Write(SemanticTags.Count);
		foreach (string semanticTag in SemanticTags)
		{
			Writer.Write(semanticTag);
		}
	}

	public void FlushRenderCache()
	{
		TransientCache &= -16;
	}

	public void FlushNavigationCache()
	{
		NavigationWeightCache.Clear();
	}

	public int GetNavigationWeightFor(GameObject Looker, bool Autoexploring = false, bool Juggernaut = false, bool IgnoreCreatures = false, bool IgnoreGases = false, bool FlexPhase = false)
	{
		return NavigationWeight(Looker, Zone.CalculateNav(Looker, Autoexploring, Juggernaut, IgnoreCreatures, IgnoreGases, FlexPhase));
	}

	public int NavigationWeight(GameObject Looker = null, bool Smart = false, bool Burrower = false, bool Autoexploring = false, bool Flying = false, bool WallWalker = false, bool IgnoresWalls = false, bool Swimming = false, bool Slimewalking = false, bool Aquatic = false, bool Polypwalking = false, bool Strutwalking = false, bool Juggernaut = false, bool Reefer = false, bool IgnoreCreatures = false, bool IgnoreGases = false, bool Unbreathing = false, bool FilthAffinity = false, bool OutOfPhase = false, bool Omniphase = false, bool Nullphase = false, bool FlexPhase = false)
	{
		return NavigationWeight(Looker, Zone.CalculateNav(Smart, Burrower, Autoexploring, Flying, WallWalker, IgnoresWalls, Swimming, Slimewalking, Aquatic, Polypwalking, Strutwalking, Juggernaut, Reefer, IgnoreCreatures, IgnoreGases, Unbreathing, FilthAffinity, OutOfPhase, Omniphase, Nullphase, FlexPhase));
	}

	public int NavigationWeight(GameObject Looker, int Nav)
	{
		NavigationWeight(Looker, Nav, out var Weight, out var _);
		return Weight;
	}

	internal void NavigationWeight(GameObject Looker, int Nav, out short Weight, out short SocialWeight)
	{
		if ((Nav & 4) != 0 && GetLight() == LightLevel.Blackout)
		{
			Weight = (SocialWeight = 101);
			return;
		}
		if ((Nav & 0x800) != 0)
		{
			Weight = (SocialWeight = 1);
			return;
		}
		if (NavigationWeightCache.TryGetValue(Nav, out var value))
		{
			Weight = (short)(value & 0xFFFF);
			SocialWeight = (short)((value >> 16) & 0xFFFF);
			return;
		}
		Weight = 0;
		bool Uncacheable = false;
		if ((Nav & 0x10) != 0)
		{
			bool flag = false;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].IsWalkableWall(Looker, ref Uncacheable))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Weight = 101;
			}
			else if (HasObjectWithPart("Combat"))
			{
				Weight = 99;
			}
		}
		if ((Nav & 0x100) != 0 && Weight < 100 && !HasAquaticSupportFor(Looker))
		{
			Weight = 101;
		}
		if (Weight < 3)
		{
			switch (Nav & 0x600)
			{
			case 1536:
				if (!HasObjectWithTag("Reef"))
				{
					Weight = 3;
				}
				break;
			case 512:
				if (!HasObjectWithTag("NavigationPolyp"))
				{
					Weight = 3;
				}
				break;
			case 1024:
				if (!HasObjectWithTag("NavigationStrut"))
				{
					Weight = 3;
				}
				break;
			}
		}
		if (Weight < 25 && (Nav & 0x1000) != 0 && HasObjectWithTag("Reef"))
		{
			Weight = 25;
		}
		GetNavigationWeightEvent E = GetNavigationWeightEvent.HandleFor(this, Looker, Uncacheable, Weight, Nav);
		GetAdjacentNavigationWeightEvent.Process(E);
		ActorGetNavigationWeightEvent.Process(E);
		Weight = (short)Math.Clamp(E.Weight, -32768, 32767);
		SocialWeight = (short)Math.Clamp(E.SocialWeight, -32768, 32767);
		if (!E.Uncacheable)
		{
			NavigationWeightCache[Nav] = (ushort)Weight | ((ushort)SocialWeight << 16);
		}
		GetNavigationWeightEvent.ResetTo(ref E);
	}

	public int NavigationWeight(GameObject Looker, ref int Nav)
	{
		if ((Nav & 0x10000000) != 0)
		{
			Nav = Zone.CalculateNav(Looker) | (Nav & -268435457);
		}
		return NavigationWeight(Looker, Nav);
	}

	[Obsolete]
	public bool IsGraveyard()
	{
		return false;
	}

	public GameObject FastFloodVisibilityFirstBlueprint(string blueprint, GameObject looker)
	{
		return ParentZone.FastSquareVisibilityFirstBlueprint(X, Y, 10, blueprint, looker);
	}

	public bool HasObjectNearby(string blueprint, GameObject looker = null, bool visibleOnly = false, bool exploredOnly = true)
	{
		return ParentZone.FastFloodFindAnyBlueprint(X, Y, 24, blueprint, looker ?? The.Player, visibleOnly, exploredOnly);
	}

	public GameObject FindObjectNearby(string blueprint, GameObject looker = null, bool visibleOnly = false, bool exploredOnly = true)
	{
		return ParentZone.FastFloodFindFirstBlueprint(X, Y, 24, blueprint, looker ?? The.Player, visibleOnly, exploredOnly);
	}

	public bool HasObjectWithBlueprint(string[] Blueprints)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			int j = 0;
			for (int num = Blueprints.Length; j < num; j++)
			{
				if (Objects[i].Blueprint == Blueprints[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectInDirection(string Direction, string Blueprint)
	{
		return GetCellFromDirection(Direction)?.HasObject(Blueprint) ?? false;
	}

	public bool HasStairs()
	{
		return HasObjectWithTagOrProperty("Stairs");
	}

	public bool HasObjectWithTag(string Tag)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithTag(string Tag, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSpawnBlocker()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSpawnBlocker())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartOrTagOrProperty(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Name))
			{
				return true;
			}
			if (Objects[i].HasTagOrProperty(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithTagOrProperty(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTagOrProperty(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPropertyOrTag(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPropertyOrTagEqualToValue(string Name, string Value)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name))
			{
				return string.Equals(Objects[i].GetPropertyOrTag(Name), Value);
			}
		}
		return false;
	}

	public bool HasObjectWithPropertyOrTagOtherThan(string Name, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPropertyOrTag(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithTagAdjacent(string Tag)
	{
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells())
		{
			if (localAdjacentCell.HasObjectWithTag(Tag))
			{
				return true;
			}
		}
		return false;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> Filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && Filter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> Filter)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, Predicate<GameObject> Filter, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2)) && Filter(Objects[i]))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2, string Name3)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> Filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && Filter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2, string Name3)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2, Name3);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> Filter)
	{
		int objectCountWithTagsOrProperties = GetObjectCountWithTagsOrProperties(Name1, Name2, Name3);
		if (objectCountWithTagsOrProperties == 0)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(objectCountWithTagsOrProperties);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public List<GameObject> GetObjectsWithTagsOrProperties(string Name1, string Name2, string Name3, Predicate<GameObject> Filter, List<GameObject> result)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((Objects[i].HasPropertyOrTag(Name1) || Objects[i].HasPropertyOrTag(Name2) || Objects[i].HasPropertyOrTag(Name3)) && Filter(Objects[i]))
			{
				result.Add(Objects[i]);
			}
		}
		return result;
	}

	public int CountObjectsWithTag(string Tag)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Tag))
			{
				num++;
			}
		}
		return num;
	}

	public int CountObjectWithTagCardinalAdjacent(string Tag)
	{
		int n = 0;
		ForeachCardinalAdjacentLocalCell((Cell c) =>
		{
			if (c.HasObjectWithTag(Tag))
			{
				n++;
			}
		});
		return n;
	}

	public GameObject GetFirstObjectWithTag(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTag(string Name, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTag(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTag(string Name, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTagExcept(string Name, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPropertyOrTag(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithTagAndNotTag(string Name1, string Name2)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name1) && !Objects[i].HasTag(Name2))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTagAndNotPropertyOrTag(string Name1, string Name2)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) && !Objects[i].HasPropertyOrTag(Name2))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTagAndNotPropertyOrTag(string Name1, string Name2, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPropertyOrTag(Name1) && !Objects[i].HasPropertyOrTag(Name2) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObject()
	{
		if (Objects.Count <= 0)
		{
			return null;
		}
		return Objects[0];
	}

	public GameObject GetFirstVisibleObject()
	{
		if (!IsVisible())
		{
			return null;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectExcept(string Blueprint, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectExcept(string Blueprint, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].Blueprint == Blueprint && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObject(Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public IPart GetFirstObjectPart(Type Type)
	{
		GameObject[] array = Objects.GetArray();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			IPart part = array[i].GetPart(Type);
			if (part != null)
			{
				return part;
			}
		}
		return null;
	}

	public T GetFirstObjectPart<T>() where T : IPart
	{
		GameObject[] array = Objects.GetArray();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (array[i].TryGetPart<T>(out var Part))
			{
				return Part;
			}
		}
		return null;
	}

	public bool TryGetFirstObjectPart<T>(out T Part) where T : IPart
	{
		GameObject[] array = Objects.GetArray();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (array[i].TryGetPart<T>(out Part))
			{
				return true;
			}
		}
		Part = null;
		return false;
	}

	public GameObject GetFirstObjectWithPart(string Part)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(Type Type)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Type))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part))
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Objects[i].IsVisible())
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(List<string> Parts)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = Parts.Count; j < count2; j++)
			{
				if (Objects[i].HasPart(Parts[j]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(string Part, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetFirstObjectWithPart(Part);
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(string Part, Predicate<GameObject> Filter1, Predicate<GameObject> Filter2)
	{
		if (Filter1 == null)
		{
			if (Filter2 == null)
			{
				return GetFirstObjectWithPart(Part);
			}
			return GetFirstObjectWithPart(Part, Filter2);
		}
		if (Filter2 == null)
		{
			return GetFirstObjectWithPart(Part, Filter1);
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part) && Filter1(Objects[i]) && Filter2(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]) && Objects[i].IsVisible())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstVisibleObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject skip, int IgnoreEasierThan, GameObject Looker)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i].HasPart(Part) && Filter(Objects[i]) && Objects[i].IsVisible())
			{
				int? num = Objects[i].Con(Looker);
				if (num.HasValue && num >= IgnoreEasierThan)
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPart(string Part, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(Part) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				return gameObject;
			}
		}
		return null;
	}

	public GameObject GetFirstRealObject(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Physics != null && gameObject.Physics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				return gameObject;
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithAnyPart(List<string> Parts)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithAnyPart(List<string> Parts, Predicate<GameObject> filter)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]) && filter(Objects[i]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetHighestRenderLayerObject()
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObject(Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithPart(string Part)
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer) && Objects[i].HasPart(Part))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithPart(string Part, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer) && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithAnyPart(List<string> Parts)
	{
		GameObject gameObject = null;
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if (gameObject2.Render == null || (gameObject != null && gameObject2.Render.RenderLayer <= gameObject.Render.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerObjectWithAnyPart(List<string> Parts, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if (gameObject2.Render == null || (gameObject != null && gameObject2.Render.RenderLayer <= gameObject.Render.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]) && Filter(gameObject2))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectFor(GameObject Actor)
	{
		GameObject gameObject = null;
		bool flag = IsSolidFor(Actor);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((!flag || Objects[i].CanInteractInCellWithSolid(Actor)) && Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectFor(GameObject Actor, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		bool flag = IsSolidFor(Actor);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((!flag || Objects[i].CanInteractInCellWithSolid(Actor)) && Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithPartFor(GameObject Actor, string Part)
	{
		GameObject gameObject = null;
		bool flag = IsSolidFor(Actor);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((!flag || Objects[i].CanInteractInCellWithSolid(Actor)) && Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer) && Objects[i].HasPart(Part))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithPartFor(GameObject Actor, string Part, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		bool flag = IsSolidFor(Actor);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if ((!flag || Objects[i].CanInteractInCellWithSolid(Actor)) && Objects[i].Render != null && (gameObject == null || Objects[i].Render.RenderLayer > gameObject.Render.RenderLayer) && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				gameObject = Objects[i];
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithAnyPartFor(GameObject Actor, List<string> Parts)
	{
		GameObject gameObject = null;
		bool flag = IsSolidFor(Actor);
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if ((flag && !gameObject2.CanInteractInCellWithSolid(Actor)) || gameObject2.Render == null || (gameObject != null && gameObject2.Render.RenderLayer <= gameObject.Render.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public GameObject GetHighestRenderLayerInteractableObjectWithAnyPartFor(GameObject Actor, List<string> Parts, Predicate<GameObject> Filter)
	{
		GameObject gameObject = null;
		bool flag = IsSolidFor(Actor);
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			GameObject gameObject2 = Objects[i];
			if ((flag && !gameObject2.CanInteractInCellWithSolid(Actor)) || gameObject2.Render == null || (gameObject != null && gameObject2.Render.RenderLayer <= gameObject.Render.RenderLayer))
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				if (gameObject2.HasPart(Parts[j]) && Filter(gameObject2))
				{
					gameObject = gameObject2;
					break;
				}
			}
		}
		return gameObject;
	}

	public bool HasObjectWithEffect(string EffectType)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithEffect(Type EffectType)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithEffect(string EffectType, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithEffect(Type EffectType, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(EffectType) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRelevantFriendlyExcept(GameObject Actor, GameObject Except)
	{
		GameObject[] array = Objects.GetArray();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = array[i];
			if (gameObject.IsCombatObject() && gameObject != Except && !Actor.IsHostileTowards(gameObject) && gameObject.PhaseMatches(Actor))
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetFirstObjectWithEffect(string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(Name))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithEffect(string Name)
	{
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasEffect(Name))
			{
				if (list == null)
				{
					list = new List<GameObject> { Objects[i] };
				}
				else
				{
					list.Add(Objects[i]);
				}
			}
		}
		return list;
	}

	public bool HasFurniture()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].isFurniture())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCombatObject(bool NoBrainOnly = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsCombatObject())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVisibleCombatObject(bool NoBrainOnly = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsVisible() && Objects[i].IsCombatObject())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRealObject()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsReal)
			{
				return true;
			}
		}
		return false;
	}

	public Cell GetInventoryCell()
	{
		return this;
	}

	public Zone GetInventoryZone()
	{
		return ParentZone;
	}

	public bool InventoryContains(GameObject Object)
	{
		return HasObject(Object);
	}

	public bool HasObject()
	{
		return Objects.Count > 0;
	}

	public bool HasObject(GameObject Object)
	{
		return Objects.Contains(Object);
	}

	public bool HasObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObject(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return Objects.Count > 0;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectOtherThan(Predicate<GameObject> Filter, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPart(string Part)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPart(Type Part)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartOtherThan(Type Part, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartExcept(string Part, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVisibleObjectWithPartExcept(string Part, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part) && Objects[i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPart(string Part, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVisibleObjectWithPartExcept(string Part, Predicate<GameObject> Filter, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part) && Filter(Objects[i]) && Objects[i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithPartOtherThan(string Part, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].HasPart(Part))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithAnyPart(List<string> Parts)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectWithAnyPart(List<string> Parts, Predicate<GameObject> Filter)
	{
		int count = Parts.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(Parts[j]) && Filter(Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectWithBlueprint(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithBlueprintEndsWith(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint != null && Objects[i].Blueprint.EndsWith(Blueprint))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithBlueprintStartsWith(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint != null && Objects[i].Blueprint.StartsWith(Blueprint))
			{
				return true;
			}
		}
		return false;
	}

	public List<GameObject> GetObjectsInCell()
	{
		return new List<GameObject>(Objects);
	}

	public int GetAdjacentObjectTestCount(Predicate<GameObject> test, int Radius = 1)
	{
		int num = 0;
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells(Radius))
		{
			num += localAdjacentCell.Objects.Where((GameObject o) => test(o)).Count();
		}
		return num;
	}

	public int GetAdjacentObjectCount(string Blueprint, int Radius = 1)
	{
		int num = 0;
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells(Radius))
		{
			num += localAdjacentCell.GetObjectCount(Blueprint);
		}
		return num;
	}

	public int GetObjectCount()
	{
		return Objects.Count;
	}

	public int GetObjectCount(string Blueprint)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCount(Predicate<GameObject> Filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public bool AnyObject()
	{
		return Objects.Count > 0;
	}

	public bool AnyObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyObject(Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyTakeable()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			XRL.World.Parts.Physics physics = Objects[i].Physics;
			if (physics != null && physics.Takeable)
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetObjectInCell(int n)
	{
		if (Objects.Count > n)
		{
			return Objects[n];
		}
		return null;
	}

	public GameObject FindObject(Predicate<GameObject> test)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != null && test(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectExcept(Predicate<GameObject> test, GameObject skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != null && Objects[i] != skip && test(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObject(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectExcept(string Blueprint, GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint && Objects[i] != Skip)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public void FindObjects(List<GameObject> List, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			List.AddRange(Objects);
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != null && Filter(Objects[i]))
			{
				List.Add(Objects[i]);
			}
		}
	}

	public GameObject ClearAndAddObject(string Blueprint)
	{
		Clear();
		return AddObject(Blueprint);
	}

	public GameObject AddTableObject(string Table)
	{
		GameObject gameObject = GameObjectFactory.create(PopulationManager.RollOneFrom(Table).Blueprint);
		if (gameObject == null)
		{
			throw new Exception("failed to roll a population result from " + Table);
		}
		return AddObject(gameObject);
	}

	public List<GameObject> AddPopulation(string Table)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (PopulationResult item in PopulationManager.Generate(Table, new Dictionary<string, string> { 
		{
			"zonetier",
			ZoneManager.zoneGenerationContextTier.ToString()
		} }))
		{
			list.Add(AddObject(item.Blueprint));
		}
		return list;
	}

	public void AddObject(string Blueprint, int Number, List<GameObject> Tracking = null, Action<GameObject> BeforeObjectCreated = null, Action<GameObject> AfterObjectCreated = null)
	{
		for (int i = 0; i < Number; i++)
		{
			AddObject(Blueprint, null, Tracking, BeforeObjectCreated, AfterObjectCreated);
		}
	}

	public GameObject ClearAndAddObject(GameObject obj, bool Clear = true, List<GameObject> Tracking = null)
	{
		if (Clear)
		{
			this.Clear();
		}
		return AddObject(obj, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, null, null, null, Tracking);
	}

	public GameObject ClearAndAddObject(string Blueprint, bool Clear = true, List<GameObject> Tracking = null)
	{
		if (Clear)
		{
			this.Clear();
		}
		return AddObject(Blueprint, null, Tracking, null, null);
	}

	public void ClearAndAddObject(string Blueprint, int Number, bool Clear = true, List<GameObject> Tracking = null)
	{
		if (Clear)
		{
			this.Clear();
		}
		AddObject(Blueprint, Number, Tracking);
	}

	public GameObject RequireObject(string Blueprint, string Context = null, List<GameObject> Tracking = null, Action<GameObject> BeforeObjectCreated = null, Action<GameObject> AfterObjectCreated = null)
	{
		if (HasObject(Blueprint))
		{
			return FindObject(Blueprint);
		}
		GameObject gameObject = GameObject.Create(Blueprint, 0, 0, null, BeforeObjectCreated, AfterObjectCreated, Context);
		if (gameObject == null)
		{
			throw new Exception("failed to generate object from blueprint " + Blueprint);
		}
		return AddObject(gameObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, null, null, null, Tracking);
	}

	public GameObject AddObject(string Blueprint, string Context = null, List<GameObject> Tracking = null, Action<GameObject> BeforeObjectCreated = null, Action<GameObject> AfterObjectCreated = null)
	{
		GameObject gameObject = GameObject.Create(Blueprint, 0, 0, null, BeforeObjectCreated, AfterObjectCreated, Context);
		if (gameObject == null)
		{
			throw new Exception("failed to generate object from blueprint " + Blueprint);
		}
		return AddObject(gameObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, null, null, null, Tracking);
	}

	public GameObject AddObject(string Blueprint, Action<GameObject> BeforeObjectCreated, List<GameObject> Tracking = null)
	{
		GameObject gameObject = GameObject.Create(Blueprint, 0, 0, null, BeforeObjectCreated);
		if (gameObject == null)
		{
			throw new Exception("failed to generate object from blueprint " + Blueprint);
		}
		return AddObject(gameObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, null, null, null, null, null, Tracking);
	}

	public GameObject AddObjectToInventory(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, string Context = null, IEvent ParentEvent = null)
	{
		bool silent = Silent;
		return AddObject(Object, Forced: false, System: false, IgnoreGravity: false, NoStack, silent, Repaint: true, FlushTransient, null, null, null, Actor, null, null, null, ParentEvent);
	}

	public GameObject AddObject(GameObject Object, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, bool Silent = false, bool Repaint = true, bool FlushTransient = true, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null, List<GameObject> Tracking = null, IEvent ParentEvent = null)
	{
		minimapCacheValid = false;
		OccludeCache = -1;
		if (FlushTransient)
		{
			FlushNavigationCache();
			TransientCache = 0;
		}
		XRL.World.Parts.Physics physics = Object.Physics;
		if (physics != null && !physics.EnterCell(this))
		{
			return Object;
		}
		Objects.Add(Object);
		Tracking?.Add(Object);
		Object.ProcessEnterCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, Actor, ForceSwap, Ignore, ParentEvent);
		Object.ProcessEnteredCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, Actor, ForceSwap, Ignore, ParentEvent);
		Object.ProcessObjectEnteredCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, Actor, ForceSwap, Ignore, ParentEvent);
		if (Object.IsPlayer())
		{
			Zone.SoundMapDirty = true;
		}
		if (Repaint && ParentZone.Built)
		{
			CheckPaintWallsAround(Object);
			CheckPaintLiquidsAround(Object);
			CheckSoundWall(Object, 1);
		}
		if (!Object.Live && ParentZone.Built && ParentZone.IsActive())
		{
			Object.MakeLive();
		}
		return Object;
	}

	protected GameObject AddObjectWithoutEvents(GameObject GO, List<GameObject> Tracking = null)
	{
		XRL.World.Parts.Physics physics = GO.Physics;
		if (physics != null && !physics.EnterCell(this))
		{
			return GO;
		}
		Objects.Add(GO);
		Tracking?.Add(GO);
		return GO;
	}

	public void SetReachable(bool State)
	{
		if (ParentZone.ReachableMap == null)
		{
			ParentZone.ClearReachableMap();
		}
		ParentZone.ReachableMap[X, Y] = State;
	}

	public void SetExplored(bool State)
	{
		ParentZone.SetExplored(X, Y, State);
	}

	public void SetExplored()
	{
		ParentZone.SetExplored(X, Y, state: true);
	}

	[Obsolete("Legacy amnesia behaviour, use SetExplored")]
	public void SetFakeUnexplored(bool State)
	{
		ParentZone.SetFakeUnexplored(X, Y, State);
	}

	[Obsolete("Legacy amnesia behaviour, use SetExplored")]
	public void MakeFakeUnexplored()
	{
		ParentZone.SetFakeUnexplored(X, Y, state: true);
	}

	public bool IsExplored()
	{
		return ParentZone.GetExplored(X, Y);
	}

	[Obsolete("Legacy amnesia behaviour, use IsExplored")]
	public bool IsReallyExplored()
	{
		return IsExplored();
	}

	public bool IsExploredFor(GameObject obj)
	{
		return ParentZone.GetExplored(X, Y);
	}

	public bool IsLit()
	{
		return (int)GetLight() > 1;
	}

	public bool IsVisible()
	{
		if (ParentZone == null || !ParentZone.IsActive())
		{
			return false;
		}
		return ParentZone.GetVisibility(X, Y);
	}

	public Cell getClosestCellFromList(List<Cell> cells)
	{
		cells.Sort((Cell a, Cell b) => a.ManhattanDistanceTo(this).CompareTo(b.ManhattanDistanceTo(this)));
		return cells.FirstOrDefault();
	}

	public void PaintWallsAround()
	{
		ZoneManager.PaintWalls(ParentZone, X - 1, Y - 1, X + 1, Y + 1);
	}

	public void CheckPaintWallsAround(GameObject GO)
	{
		if (GO.HasTagOrProperty("PaintedWall") || GO.HasTagOrProperty("PaintedFence") || GO.HasTagOrProperty("ForceRepaintSolid"))
		{
			PaintWallsAround();
		}
		if (GO.HasTagOrProperty("PaintedCrystal"))
		{
			ZoneManager.PaintWalls(ParentZone, X - 3, Y - 3, X + 3, Y + 3);
		}
	}

	public void PaintLiquidsAround()
	{
		ZoneManager.PaintWater(ParentZone, X - 1, Y - 1, X + 1, Y + 1);
	}

	public void CheckPaintLiquidsAround(GameObject GO)
	{
		if (GO.HasTagOrProperty("PaintedLiquidAtlas") || GO.HasTagOrProperty("ForceRepaintLiquid"))
		{
			PaintLiquidsAround();
		}
	}

	public List<GameObject> RemoveObjects(Predicate<GameObject> test)
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject @object in GetObjects())
		{
			if (test(@object))
			{
				list.Add(@object);
			}
		}
		list.ForEach((GameObject o) =>
		{
			RemoveObject(o);
		});
		return list;
	}

	public bool RemoveObjectFromInventory(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, string Context = null, IEvent ParentEvent = null)
	{
		if (!Objects.Contains(Object))
		{
			return false;
		}
		return RemoveObject(Object, Forced: false, System: false, IgnoreGravity: false, Silent, NoStack, Repaint: true, FlushTransient, null, null, null, Actor, null, null, ParentEvent);
	}

	public bool RemoveObject(GameObject Object, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool Silent = false, bool NoStack = false, bool Repaint = true, bool FlushTransient = true, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null, IEvent ParentEvent = null)
	{
		GameObject Blocking = null;
		Object.ProcessLeavingCell(this, ref Blocking, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, Actor, ForceSwap, Ignore, ParentEvent);
		if (FlushTransient)
		{
			TransientCache = 0;
			minimapCacheValid = false;
			OccludeCache = -1;
			FlushNavigationCache();
		}
		Objects.Remove(Object);
		Object.ProcessLeftCell(this, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging, Actor, ForceSwap, Ignore, ParentEvent);
		if (Repaint && ParentZone.Built)
		{
			CheckPaintWallsAround(Object);
			CheckPaintLiquidsAround(Object);
			CheckSoundWall(Object, 0);
		}
		return true;
	}

	public Cell getClosestPassableCell()
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCell(Predicate<Cell> filter)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable() && filter(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCell(Cell alsoClosestTo)
	{
		if (alsoClosestTo == null)
		{
			return getClosestPassableCell();
		}
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => (c1.Location.Distance(Location) + c1.Location.Distance(alsoClosestTo.Location)).CompareTo(c2.Location.Distance(Location) + c2.Location.Distance(alsoClosestTo.Location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCellExcept(List<Cell> except)
	{
		List<Cell> cellsViaEventList = ParentZone.GetCellsViaEventList((Cell c) => c.IsPassable() && !except.Contains(c));
		if (cellsViaEventList.Count == 0)
		{
			return this;
		}
		if (cellsViaEventList.Count > 1)
		{
			cellsViaEventList.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cellsViaEventList[0];
	}

	public Cell getClosestPassableCellFor(GameObject who)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable(who));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestPassableCellForExcept(GameObject who, List<Cell> except)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsPassable(who) && !except.Contains(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestEmptyCell()
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestEmptyCell(Cell alsoClosestTo)
	{
		if (alsoClosestTo == null)
		{
			return getClosestEmptyCell();
		}
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => (c1.Location.Distance(Location) + c1.Location.Distance(alsoClosestTo.Location)).CompareTo(c2.Location.Distance(Location) + c2.Location.Distance(alsoClosestTo.Location)));
		}
		return cells[0];
	}

	public Cell getClosestEmptyCellExcept(List<Cell> except)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty() && !except.Contains(c));
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestReachableCell()
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmpty() && c.IsReachable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public Cell getClosestReachableCellFor(GameObject who)
	{
		List<Cell> cells = ParentZone.GetCells((Cell c) => c.IsEmptyFor(who) && c.IsReachable());
		if (cells.Count == 0)
		{
			return this;
		}
		if (cells.Count > 1)
		{
			cells.Sort((Cell c1, Cell c2) => c1.Location.Distance(Location).CompareTo(c2.Location.Distance(Location)));
		}
		return cells[0];
	}

	public bool IsReachable()
	{
		return ParentZone.IsReachable(X, Y);
	}

	public int DistanceToEdge()
	{
		int val = Math.Min(ParentZone.Width - X, X);
		int val2 = Math.Min(ParentZone.Height - Y, Y);
		return Math.Min(val, val2);
	}

	public int DistanceTo(GameObject GO)
	{
		return PathDistanceTo(GO?.CurrentCell);
	}

	public int DistanceTo(Cell C)
	{
		return PathDistanceTo(C);
	}

	public int CosmeticDistanceTo(Point2D location)
	{
		return CosmeticDistanceTo(location.x, location.y);
	}

	public int CosmeticDistanceto(Location2D location)
	{
		return CosmeticDistanceTo(location.X, location.Y);
	}

	public int CosmeticDistanceTo(int x, int y)
	{
		return (int)Math.Sqrt((float)(x - X) * 0.6666f * ((float)(x - X) * 0.6666f) + (float)((y - Y) * (y - Y)));
	}

	public int DistanceTo(int x, int y)
	{
		return Math.Max(Math.Abs(X - x), Math.Abs(Y - y));
	}

	public int ManhattanDistanceTo(GameObject Object)
	{
		if (Object == null)
		{
			return 9999999;
		}
		return ManhattanDistanceTo(Object.CurrentCell);
	}

	public int ManhattanDistanceTo(Cell C)
	{
		if (C == null)
		{
			return 9999999;
		}
		if (C.ParentZone.IsWorldMap() != ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (C.ParentZone != ParentZone)
		{
			if (C.ParentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return 9999999;
			}
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			if (num == num3 && num2 == num4)
			{
				return Math.Abs(zoneZ2 - zoneZ);
			}
			return Math.Abs(num3 - num) + Math.Abs(num4 - num2) + Math.Abs(zoneZ2 - zoneZ);
		}
		return Math.Abs(C.X - X) + Math.Abs(C.Y - Y);
	}

	public int ManhattanDistanceTo(int X, int Y)
	{
		return Math.Abs(this.X - X) + Math.Abs(this.Y - Y);
	}

	public int PathDistanceTo(int X, int Y)
	{
		return Math.Max(Math.Abs(this.X - X), Math.Abs(this.Y - Y));
	}

	public int PathDistanceTo(Location2D L)
	{
		if (L == null)
		{
			return 9999999;
		}
		return Math.Max(Math.Abs(L.X - X), Math.Abs(L.Y - Y));
	}

	public int PathDistanceTo(Cell C)
	{
		if (C == null)
		{
			return 9999999;
		}
		if (C.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (C.ParentZone != ParentZone)
		{
			if (C.ParentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return 9999999;
			}
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			if (num == num3 && num2 == num4)
			{
				return Math.Abs(zoneZ2 - zoneZ);
			}
			return Math.Max(Math.Max(Math.Abs(num3 - num), Math.Abs(num4 - num2)), Math.Abs(zoneZ2 - zoneZ));
		}
		return Math.Max(Math.Abs(C.X - X), Math.Abs(C.Y - Y));
	}

	public Point2D PathDifferenceTo(Cell C)
	{
		if (C.ParentZone != ParentZone)
		{
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			return new Point2D(num3 - num, num4 - num2);
		}
		return new Point2D(X - C.X, Y - C.Y);
	}

	public double RealDistanceTo(GameObject GO)
	{
		if (GO == null)
		{
			return 9999999.0;
		}
		if (GO.Physics == null)
		{
			return 9999999.0;
		}
		return RealDistanceTo(GO.CurrentCell);
	}

	public double RealDistanceTo(Cell C, bool indefiniteWorld = true)
	{
		if (C == null)
		{
			return 9999999.0;
		}
		if (indefiniteWorld)
		{
			if (C.ParentZone.IsWorldMap())
			{
				return 9999999.0;
			}
			if (ParentZone.IsWorldMap())
			{
				return 9999999.0;
			}
		}
		if (C.ParentZone != ParentZone)
		{
			if (C.ParentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return 9999999.0;
			}
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			return Math.Sqrt((num - num3) * (num - num3) + (num2 - num4) * (num2 - num4) + (zoneZ - zoneZ2) * (zoneZ - zoneZ2));
		}
		return Math.Sqrt((C.X - X) * (C.X - X) + (C.Y - Y) * (C.Y - Y));
	}

	public int DistanceToRespectStairs(Cell C)
	{
		if (C == null)
		{
			return 9999999;
		}
		if (C.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		if (C.ParentZone != ParentZone)
		{
			if (C.ParentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return 9999999;
			}
			int num = C.ParentZone.GetZonewX() * Definitions.Width * 80 + C.ParentZone.GetZoneX() * 80 + C.X;
			int num2 = C.ParentZone.GetZonewY() * Definitions.Height * 25 + C.ParentZone.GetZoneY() * 25 + C.Y;
			int zoneZ = C.ParentZone.GetZoneZ();
			int num3 = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			int num4 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			int zoneZ2 = ParentZone.GetZoneZ();
			if (zoneZ != zoneZ2)
			{
				if (num != num3 || num2 != num4)
				{
					return int.MaxValue;
				}
				if (Math.Abs(zoneZ2 - zoneZ) == 1)
				{
					if (zoneZ < zoneZ2)
					{
						if (!HasObjectWithPart("StairsUp"))
						{
							return int.MaxValue;
						}
					}
					else if (!HasObjectWithPart("StairsDown"))
					{
						return int.MaxValue;
					}
				}
			}
			if (num == num3 && num2 == num4)
			{
				return Math.Abs(zoneZ2 - zoneZ);
			}
			return Math.Max(Math.Max(Math.Abs(num3 - num), Math.Abs(num4 - num2)), Math.Abs(zoneZ2 - zoneZ));
		}
		return Math.Max(Math.Abs(C.X - X), Math.Abs(C.Y - Y));
	}

	public List<GameObject> GetObjectsWithRegisteredEvent(string EventName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithRegisteredEvent(string EventName, Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName) && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public GameObject GetFirstObjectWithRegisteredEvent(string EventName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithRegisteredEvent(string EventName, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName) && Filter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public bool HasObjectWithRegisteredEvent(string EventName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithRegisteredEvent(string EventName, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName) && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public void ForeachObjectWithRegisteredEvent(string EventName, Action<GameObject> aProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				aProc(Objects[i]);
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
	}

	public void GetObjectsWithTagOrProperty(List<GameObject> List, string Name)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasTagOrProperty(Name))
			{
				List.Add(gameObject);
			}
		}
	}

	public List<GameObject> GetObjectsWithTagOrProperty(string Name)
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetObjectsWithTagOrProperty(list, Name);
		return list;
	}

	public bool ForeachObjectWithRegisteredEvent(string EventName, Predicate<GameObject> pProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			if (Objects[i].HasRegisteredEvent(EventName))
			{
				if (!pProc(Objects[i]))
				{
					return false;
				}
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
		return true;
	}

	public List<GameObject> GetObjectsWithProperty(string PropertyName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasProperty(PropertyName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public void GetObjectsWithProperty(string PropertyName, List<GameObject> Return)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasProperty(PropertyName))
			{
				Return.Add(Objects[i]);
			}
		}
	}

	public int GetObjectCountWithProperty(string PropertyName)
	{
		int num = 0;
		for (int i = 0; i < Objects.Count; i++)
		{
			if (Objects[i].HasProperty(PropertyName))
			{
				num++;
			}
		}
		return num;
	}

	public List<GameObject> GetObjectsWithIntProperty(string PropertyName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0)
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithIntProperty(string PropertyName, Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0 && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public int GetObjectCountWithIntProperty(string PropertyName)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasObjectWithIntProperty(string PropertyName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasObjectWithIntProperty(string PropertyName, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(PropertyName) > 0 && Filter(Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetObjectWithTagOrProperty(string TagName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTagOrProperty(TagName))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetObjectWithTag(string TagName)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTag(string TagName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTag(string TagName, Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName) && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public void ForeachObjectWithTagOrProperty(string Name, Action<GameObject> aProc)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name) || Objects[i].HasProperty(Name))
			{
				aProc(Objects[i]);
			}
		}
	}

	public void ForeachObjectWithTag(string TagName, Action<GameObject> aProc)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				aProc(Objects[i]);
			}
		}
	}

	public void ForeachObjectWithTag(string TagName, Predicate<GameObject> aProc)
	{
		int i = 0;
		for (int count = Objects.Count; i < count && (!Objects[i].HasTag(TagName) || aProc(Objects[i])); i++)
		{
		}
	}

	public int GetObjectCountWithTag(string TagName)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(TagName))
			{
				num++;
			}
		}
		return num;
	}

	public void ForeachObjectWithPart(string PartName, Action<GameObject> aProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName))
			{
				aProc(gameObject);
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
	}

	public bool ForeachObjectWithPart(string PartName, Predicate<GameObject> pProc)
	{
		int i = 0;
		for (int num = Objects.Count; i < num; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.HasPart(PartName))
			{
				if (!pProc(gameObject))
				{
					return false;
				}
				if (Objects.Count < num)
				{
					int num2 = num - Objects.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
		return true;
	}

	public void ForeachObject(Action<GameObject> aProc)
	{
		switch (Objects.Count)
		{
		case 1:
			aProc(Objects[0]);
			break;
		case 2:
		{
			GameObject obj4 = Objects[0];
			GameObject obj5 = Objects[1];
			aProc(obj4);
			aProc(obj5);
			break;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			GameObject obj3 = Objects[2];
			aProc(obj);
			aProc(obj2);
			aProc(obj3);
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				aProc(list[i]);
			}
			break;
		}
		case 0:
			break;
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc)
	{
		switch (Objects.Count)
		{
		case 1:
			if (!pProc(Objects[0]))
			{
				return false;
			}
			break;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (!pProc(obj))
			{
				return false;
			}
			if (!pProc(obj2))
			{
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (!pProc(obj3))
			{
				return false;
			}
			if (!pProc(obj4))
			{
				return false;
			}
			if (!pProc(obj5))
			{
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (!pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return true;
	}

	public void ForeachObject(Action<GameObject> aProc, Predicate<GameObject> Filter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (Filter(Objects[0]))
			{
				aProc(Objects[0]);
			}
			return;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (Filter(obj))
			{
				aProc(obj);
			}
			if (Filter(obj2))
			{
				aProc(obj2);
			}
			return;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (Filter(obj3))
			{
				aProc(obj3);
			}
			if (Filter(obj4))
			{
				aProc(obj4);
			}
			if (Filter(obj5))
			{
				aProc(obj5);
			}
			return;
		}
		case 0:
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (Filter(list[i]))
			{
				aProc(list[i]);
			}
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> Filter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (Filter(Objects[0]))
			{
				pProc(Objects[0]);
				return false;
			}
			break;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (Filter(obj) && !pProc(obj))
			{
				return false;
			}
			if (Filter(obj2))
			{
				pProc(obj2);
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (Filter(obj3) && !pProc(obj3))
			{
				return false;
			}
			if (Filter(obj4) && !pProc(obj4))
			{
				return false;
			}
			if (Filter(obj5))
			{
				pProc(obj5);
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (Filter(list[i]) && !pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return false;
	}

	public void SafeForeachObject(Action<GameObject> aProc)
	{
		switch (Objects.Count)
		{
		case 1:
			aProc(Objects[0]);
			return;
		case 2:
		{
			GameObject obj4 = Objects[0];
			GameObject obj5 = Objects[1];
			aProc(obj4);
			if (obj5.IsValid())
			{
				aProc(obj5);
			}
			return;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			GameObject obj3 = Objects[2];
			aProc(obj);
			if (obj2.IsValid())
			{
				aProc(obj2);
			}
			if (obj3.IsValid())
			{
				aProc(obj3);
			}
			return;
		}
		case 0:
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		aProc(list[0]);
		int i = 1;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i].IsValid())
			{
				aProc(list[i]);
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc)
	{
		switch (Objects.Count)
		{
		case 1:
			if (!pProc(Objects[0]))
			{
				return false;
			}
			break;
		case 2:
		{
			GameObject obj4 = Objects[0];
			GameObject obj5 = Objects[1];
			if (!pProc(obj4))
			{
				return false;
			}
			if (obj5.IsValid() && !pProc(obj5))
			{
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			GameObject obj3 = Objects[2];
			if (!pProc(obj))
			{
				return false;
			}
			if (obj2.IsValid() && !pProc(obj2))
			{
				return false;
			}
			if (obj3.IsValid() && !pProc(obj3))
			{
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			if (!pProc(list[0]))
			{
				return false;
			}
			int i = 1;
			for (int count = list.Count; i < count; i++)
			{
				if (list[i].IsValid() && !pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return true;
	}

	public void SafeForeachObject(Action<GameObject> aProc, Predicate<GameObject> Filter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (Filter(Objects[0]))
			{
				aProc(Objects[0]);
			}
			return;
		case 2:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			if (Filter(obj))
			{
				aProc(obj);
			}
			if (obj2.IsValid() && Filter(obj2))
			{
				aProc(obj2);
			}
			return;
		}
		case 3:
		{
			GameObject obj3 = Objects[0];
			GameObject obj4 = Objects[1];
			GameObject obj5 = Objects[2];
			if (Filter(obj3))
			{
				aProc(obj3);
			}
			if (obj4.IsValid() && Filter(obj4))
			{
				aProc(obj4);
			}
			if (obj5.IsValid() && Filter(obj5))
			{
				aProc(obj5);
			}
			return;
		}
		case 0:
			return;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		aProc(list[0]);
		int i = 1;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i].IsValid() && Filter(list[i]))
			{
				aProc(list[i]);
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> Filter)
	{
		switch (Objects.Count)
		{
		case 1:
			if (Filter(Objects[0]) && !pProc(Objects[0]))
			{
				return false;
			}
			break;
		case 2:
		{
			GameObject obj4 = Objects[0];
			GameObject obj5 = Objects[1];
			if (Filter(obj4) && !pProc(obj4))
			{
				return false;
			}
			if (obj5.IsValid() && Filter(obj5) && !pProc(obj5))
			{
				return false;
			}
			break;
		}
		case 3:
		{
			GameObject obj = Objects[0];
			GameObject obj2 = Objects[1];
			GameObject obj3 = Objects[2];
			if (Filter(obj) && !pProc(obj))
			{
				return false;
			}
			if (obj2.IsValid() && Filter(obj2) && !pProc(obj2))
			{
				return false;
			}
			if (obj3.IsValid() && Filter(obj3) && !pProc(obj3))
			{
				return false;
			}
			break;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			if (Filter(list[0]) && !pProc(list[0]))
			{
				return false;
			}
			int i = 1;
			for (int count = list.Count; i < count; i++)
			{
				if (list[i].IsValid() && Filter(list[i]) && !pProc(list[i]))
				{
					return false;
				}
			}
			break;
		}
		case 0:
			break;
		}
		return true;
	}

	public List<GameObject> GetObjectsViaEventList()
	{
		return Event.NewGameObjectList(Objects);
	}

	public List<GameObject> GetObjects()
	{
		return new List<GameObject>(Objects);
	}

	public void GetObjects(List<GameObject> Store)
	{
		Store.AddRange(Objects);
	}

	public List<GameObject> GetObjects(string Blueprint)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				num++;
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			int j = 0;
			for (int count2 = Objects.Count; j < count2; j++)
			{
				if (Objects[j].Blueprint == Blueprint)
				{
					list.Add(Objects[j]);
					if (list.Count >= num)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public void GetObjects(string Blueprint, List<GameObject> List)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				List.Add(Objects[i]);
			}
		}
	}

	public List<GameObject> GetObjectsViaEventList(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjects();
		}
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				num++;
			}
		}
		List<GameObject> list = Event.NewGameObjectList();
		if (num > 0)
		{
			int j = 0;
			for (int count2 = Objects.Count; j < count2; j++)
			{
				if (Filter(Objects[j]))
				{
					list.Add(Objects[j]);
					if (list.Count >= num)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public List<GameObject> GetObjects(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjects();
		}
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				num++;
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			int j = 0;
			for (int count2 = Objects.Count; j < count2; j++)
			{
				if (Filter(Objects[j]))
				{
					list.Add(Objects[j]);
					if (list.Count >= num)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public void GetObjects(Predicate<GameObject> Filter, List<GameObject> Store)
	{
		if (Filter == null)
		{
			GetObjects(Store);
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Filter(Objects[i]))
			{
				Store.Add(Objects[i]);
			}
		}
	}

	public List<GameObject> GetObjectsThatInheritFrom(string Blueprint)
	{
		List<GameObject> list = new List<GameObject>();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetBlueprint().InheritsFrom(Blueprint))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public GameObject GetFirstObjectThatInheritsFrom(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetBlueprint().InheritsFrom(Blueprint))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsWithPartReadonly(string PartName)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithPart(string PartName)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public void GetObjectsWithPart(string PartName, List<GameObject> Return)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				Return.Add(Objects[i]);
			}
		}
	}

	public IEnumerable<GameObject> LoopObjects()
	{
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			yield return Objects[i];
		}
	}

	public IEnumerable<GameObject> LoopObjectsWithPart(string PartName)
	{
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				yield return Objects[i];
			}
		}
	}

	public List<GameObject> GetObjectsWithPart(List<string> PartNames)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartNames));
		int count = PartNames.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(PartNames[j]))
				{
					list.Add(Objects[i]);
					break;
				}
			}
		}
		return list;
	}

	public IEnumerable<GameObject> LoopObjectsWithPart(List<string> PartNames)
	{
		int l = PartNames.Count;
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			for (int k = 0; k < l; k++)
			{
				if (Objects[i].HasPart(PartNames[k]))
				{
					yield return Objects[i];
					break;
				}
			}
		}
	}

	public void GetObjectsWithPart(List<string> PartNames, List<GameObject> Return)
	{
		int count = PartNames.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(PartNames[j]))
				{
					Return.Add(Objects[i]);
					break;
				}
			}
		}
	}

	public List<GameObject> GetObjectsWithPart(string PartName, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetObjectsWithPart(PartName);
		}
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName, Filter));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName) && Filter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public IEnumerable<GameObject> LoopObjectsWithPart(string PartName, Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int j = Objects.Count; i < j; i++)
		{
			if (Objects[i].HasPart(PartName) && (Filter == null || Filter(Objects[i])))
			{
				yield return Objects[i];
			}
		}
	}

	public List<GameObject> GetObjectsWithPart(string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName, Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, Skip, SkipList, CheckFlight, CheckAttackable, CheckSolidity));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithPart(string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = new List<GameObject>(GetObjectCountWithPart(PartName, Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, Skip, SkipList, CheckFlight, CheckAttackable, CheckSolidity, Filter));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetRealObjects(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		List<GameObject> list = new List<GameObject>(GetRealObjectCount(Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, Skip, SkipList, CheckFlight, CheckAttackable, CheckSolidity));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetRealObjects(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = new List<GameObject>(GetRealObjectCount(Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, Skip, SkipList, CheckFlight, CheckAttackable, CheckSolidity, Filter));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetRealNonSceneryObjects()
	{
		List<GameObject> list = new List<GameObject>(GetRealNonSceneryObjectCount());
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery)
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetRealNonSceneryObjects(Predicate<GameObject> Filter)
	{
		List<GameObject> list = new List<GameObject>(GetRealNonSceneryObjectCount(Filter));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && (Filter == null || Filter(gameObject)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetRealNonSceneryObjects(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		List<GameObject> list = new List<GameObject>(GetRealNonSceneryObjectCount(Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, Skip, SkipList, CheckFlight, CheckAttackable, CheckSolidity));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public List<GameObject> GetRealNonSceneryObjects(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = new List<GameObject>(GetRealNonSceneryObjectCount(Phase, FlightPOV, AttackPOV, SolidityPOV, Projectile, Skip, SkipList, CheckFlight, CheckAttackable, CheckSolidity, Filter));
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}

	public void GetObjectsWithPart(string PartName, List<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				Return.Add(gameObject);
			}
		}
	}

	public void GetObjectsWithPart(string PartName, IList<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				Return.Add(gameObject);
			}
		}
	}

	public void GetRealObjects(List<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)))
			{
				XRL.World.Parts.Physics physics = gameObject.Physics;
				if (physics != null && physics.IsReal && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
				{
					Return.Add(gameObject);
				}
			}
		}
	}

	public void GetRealNonSceneryObjects(List<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				Return.Add(gameObject);
			}
		}
	}

	public void GetRealNonSceneryObjects(IList<GameObject> Return, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				Return.Add(gameObject);
			}
		}
	}

	public int GetObjectCountWithPart(string PartName)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithPart(List<string> PartNames)
	{
		int num = 0;
		int count = PartNames.Count;
		int i = 0;
		for (int count2 = Objects.Count; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (Objects[i].HasPart(PartNames[j]))
				{
					num++;
					break;
				}
			}
		}
		return num;
	}

	public int GetObjectCountWithPart(string PartName, Predicate<GameObject> Filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasPart(PartName) && Filter(Objects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithPart(string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountWithPart(string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountAndFirstObjectWithPart(out GameObject FirstObject, string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public int GetObjectCountAndFirstObjectWithPart(out GameObject FirstObject, string PartName, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.HasPart(PartName) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public int GetRealObjectCount(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRealObjectCount(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRealObjectCountAndFirstObject(out GameObject FirstObject, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public int GetRealObjectCountAndFirstObject(out GameObject FirstObject, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public int GetRealNonSceneryObjectCount()
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery)
			{
				num++;
			}
		}
		return num;
	}

	public int GetRealNonSceneryObjectCount(Predicate<GameObject> Filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && (Filter == null || Filter(gameObject)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRealNonSceneryObjectCount(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRealNonSceneryObjectCount(int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetRealNonSceneryObjectCountAndFirstObject(out GameObject FirstObject, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public int GetRealNonSceneryObjectCountAndFirstObject(out GameObject FirstObject, int Phase = 5, GameObject FlightPOV = null, GameObject AttackPOV = null, GameObject SolidityPOV = null, GameObject Projectile = null, GameObject Skip = null, List<GameObject> SkipList = null, bool CheckFlight = false, bool CheckAttackable = false, bool CheckSolidity = false, Predicate<GameObject> Filter = null)
	{
		FirstObject = null;
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			XRL.World.Parts.Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && !gameObject.IsScenery && gameObject != Skip && (SkipList == null || !SkipList.Contains(gameObject)) && gameObject.PhaseMatches(Phase) && (!CheckFlight || gameObject.FlightMatches(FlightPOV)) && (!CheckAttackable || CheckAttackableEvent.Check(gameObject, AttackPOV)) && (!CheckSolidity || gameObject.ConsiderSolidForProjectile(Projectile, AttackPOV, null, null, false, false)) && (Filter == null || Filter(gameObject)))
			{
				if (num == 0)
				{
					FirstObject = gameObject;
				}
				num++;
			}
		}
		return num;
	}

	public bool IsEmptyExcludingCombat(bool NoBrainOnly = false)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null && gameObject.Render.RenderLayer > 5 && !gameObject.IsCombatObject() && (!gameObject.IsDoor() || gameObject.GetPart<Door>().Locked) && !gameObject.HasPart<StairsDown>() && !gameObject.HasPart<StairsUp>())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmptyAtRenderLayer(int Layer)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null && gameObject.Render.Visible && gameObject.Render.RenderLayer >= Layer)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsOpenForPlacement()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null)
			{
				if (gameObject.Physics.Solid)
				{
					return false;
				}
				if (gameObject.HasPart<StairsDown>())
				{
					return false;
				}
				if (gameObject.HasPart<StairsUp>())
				{
					return false;
				}
				if (gameObject.IsCombat)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsEmptyForPopulation()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Physics.Solid)
			{
				return false;
			}
			if (gameObject.IsCombatObject())
			{
				return false;
			}
			if (gameObject.HasTag("Furniture"))
			{
				return false;
			}
			if (gameObject.HasTag("Door"))
			{
				return false;
			}
			if (gameObject.IsWall())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSpawnable()
	{
		if (HasSpawnBlocker())
		{
			return false;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Physics.Solid)
			{
				return false;
			}
			if (gameObject.IsCombatObject())
			{
				return false;
			}
			StairsDown part = gameObject.GetPart<StairsDown>();
			if (part != null && part.PullDown)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsCorner()
	{
		if (X == 0 || X == 79 || Y == 0 || Y == 24)
		{
			return false;
		}
		if (GetCellFromDirection("N").HasWall() && GetCellFromDirection("NE").HasWall() && GetCellFromDirection("E").HasWall())
		{
			return true;
		}
		if (GetCellFromDirection("E").HasWall() && GetCellFromDirection("SE").HasWall() && GetCellFromDirection("S").HasWall())
		{
			return true;
		}
		if (GetCellFromDirection("S").HasWall() && GetCellFromDirection("SW").HasWall() && GetCellFromDirection("W").HasWall())
		{
			return true;
		}
		if (GetCellFromDirection("W").HasWall() && GetCellFromDirection("NW").HasWall() && GetCellFromDirection("N").HasWall())
		{
			return true;
		}
		return false;
	}

	public bool IsEmpty()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null && gameObject.Render.RenderLayer > 5 && (!gameObject.TryGetPart<Door>(out var Part) || Part.Locked) && !gameObject.HasPart<StairsDown>() && !gameObject.HasPart<StairsUp>())
			{
				return false;
			}
			if (gameObject.IsCombatObject())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmptyFor(GameObject Object)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null && gameObject.Render.RenderLayer > 5 && (!gameObject.TryGetPart<Door>(out var Part) || Part.Locked) && !gameObject.HasPart<StairsDown>() && !gameObject.HasPart<StairsUp>() && gameObject.PhaseMatches(Object))
			{
				return false;
			}
			if (gameObject.IsCombatObject() && gameObject.PhaseAndFlightMatches(Object))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmptyIgnoring(Predicate<GameObject> Filter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (!Filter(gameObject))
			{
				if (gameObject.Render != null && gameObject.Render.RenderLayer > 5 && (!gameObject.TryGetPart<Door>(out var Part) || Part.Locked) && !gameObject.HasPart<StairsDown>() && !gameObject.HasPart<StairsUp>())
				{
					return false;
				}
				if (gameObject.IsCombatObject())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ClearImpassableObjects(GameObject Object = null, bool IncludeCombatObjects = false)
	{
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.ConsiderSolidFor(Object) && (!gameObject.TryGetPart<Door>(out var Part) || Part.Locked))
			{
				StairsDown part = gameObject.GetPart<StairsDown>();
				if (part != null && part.PullDown && (Object == null || part.IsValidForPullDown(Object)))
				{
					if (list == null)
					{
						list = Event.NewGameObjectList();
					}
					list.Add(Objects[i]);
					continue;
				}
				if (part == null && !gameObject.HasPart<StairsUp>())
				{
					if (Object == null)
					{
						if (list == null)
						{
							list = Event.NewGameObjectList();
						}
						list.Add(Objects[i]);
						continue;
					}
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", Object);
					bool flag = gameObject.FireEvent(eBeforePhysicsRejectObjectEntringCell);
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", null);
					if (flag)
					{
						if (list == null)
						{
							list = Event.NewGameObjectList();
						}
						list.Add(Objects[i]);
						continue;
					}
				}
			}
			if (IncludeCombatObjects && gameObject.IsCombatObject() && gameObject != Object)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
		}
		list?.ForEach((GameObject o) =>
		{
			Objects.Remove(o);
		});
		return true;
	}

	public bool IsPassable(GameObject Object = null, bool IncludeCombatObjects = true)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			StairsDown Part2;
			if (gameObject.ConsiderSolidFor(Object))
			{
				if (!gameObject.TryGetPart<Door>(out var Part) || Part.Locked)
				{
					if (Object == null)
					{
						return false;
					}
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", Object);
					bool flag = gameObject.FireEvent(eBeforePhysicsRejectObjectEntringCell);
					eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", null);
					if (flag)
					{
						return false;
					}
				}
			}
			else if (gameObject.TryGetPart<StairsDown>(out Part2) && Part2.PullDown && (Object == null || Part2.IsValidForPullDown(Object)))
			{
				return false;
			}
			if (IncludeCombatObjects && gameObject.IsCombatObject() && gameObject != Object)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmptyOfSolid(bool IncludeCombatObjects = true)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null && gameObject.Render.RenderLayer > 5 && (!gameObject.IsDoor() || gameObject.GetPart<Door>().Locked) && !gameObject.HasPart<StairsDown>() && !gameObject.HasPart<StairsUp>() && gameObject.ConsiderSolid())
			{
				return false;
			}
			if (IncludeCombatObjects && gameObject.IsCombatObject())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmptyOfSolidFor(GameObject Object, bool IncludeCombatObjects = true)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.Render != null && gameObject.Render.RenderLayer > 5 && (!gameObject.IsDoor() || gameObject.GetPart<Door>().Locked) && !gameObject.HasPart<StairsDown>() && !gameObject.HasPart<StairsUp>() && gameObject.ConsiderSolidFor(Object) && gameObject.PhaseAndFlightMatches(Object))
			{
				return false;
			}
			if (IncludeCombatObjects && gameObject.IsCombatObject() && gameObject.PhaseAndFlightMatches(Object))
			{
				return false;
			}
		}
		return true;
	}

	public void ClearOccludeCache()
	{
		OccludeCache = -1;
	}

	public bool IsEdge()
	{
		if (X != 0 && X != ParentZone.Width - 1 && Y != 0)
		{
			return Y == ParentZone.Height - 1;
		}
		return true;
	}

	public bool HasExternalWall(Predicate<Cell> test = null)
	{
		if (HasWall())
		{
			string[] directionList = Directions.DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirection = GetCellFromDirection(direction);
				if (cellFromDirection != null && !cellFromDirection.HasWall() && (test == null || test(cellFromDirection)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasWallInDirection(string dir)
	{
		return GetCellFromDirection(dir)?.HasWall() ?? true;
	}

	public bool HasWall()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWall())
			{
				return true;
			}
		}
		return false;
	}

	public List<GameObject> GetWalls()
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWall())
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public GameObject GetFirstWall()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWall())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public bool IsOccluding()
	{
		if (OccludeCache != -1)
		{
			return OccludeCache == 1;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i]?.Render != null && Objects[i].Render.Occluding)
			{
				OccludeCache = 1;
				return true;
			}
		}
		OccludeCache = 0;
		return false;
	}

	public bool IsOccludingFor(GameObject What)
	{
		if (OccludeCache == 0)
		{
			return false;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i]?.Render != null && Objects[i].Render.Occluding && Objects[i].PhaseAndFlightMatches(What))
			{
				OccludeCache = 1;
				return true;
			}
		}
		return false;
	}

	public bool IsOccludingOtherThan(GameObject skip)
	{
		if (OccludeCache == 0)
		{
			return false;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != skip && Objects[i]?.Render != null && Objects[i].Render.Occluding)
			{
				OccludeCache = 1;
				return true;
			}
		}
		return false;
	}

	public bool IsSolid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolid(bool ForFluid)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid(ForFluid))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolid(bool ForFluid, int Phase)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid(ForFluid, Phase))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolidOtherThan(GameObject Skip)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].ConsiderSolid())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolidFor(GameObject Actor, bool ForFluid = false)
	{
		if (Actor == null)
		{
			return IsSolid();
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(Actor, ForFluid))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolidForOtherThan(GameObject Actor, GameObject Skip, bool ForFluid = false)
	{
		if (Actor == null)
		{
			return IsSolidOtherThan(Skip);
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] != Skip && Objects[i].ConsiderSolidFor(Actor))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolidForProjectile(GameObject Projectile, GameObject Attacker, out bool RecheckHit, out bool RecheckPhase, GameObject Launcher = null, GameObject ApparentTarget = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, bool Prospective = false)
	{
		RecheckHit = false;
		RecheckPhase = false;
		if (Projectile == null)
		{
			return IsSolidFor(Attacker, ForFluid: true);
		}
		bool flag = PenetrateCreatures == true;
		bool flag2 = PenetrateWalls == true;
		if (!PenetrateCreatures.HasValue || !PenetrateWalls.HasValue)
		{
			GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = GetMissileWeaponPerformanceEvent.GetFor(Attacker, Launcher, Projectile);
			flag = PenetrateCreatures ?? getMissileWeaponPerformanceEvent.PenetrateCreatures;
			flag2 = PenetrateWalls ?? getMissileWeaponPerformanceEvent.PenetrateWalls;
		}
		TreatAsSolid treatAsSolid = Projectile?.GetPart<TreatAsSolid>();
		if (treatAsSolid != null)
		{
			bool flag3 = treatAsSolid.Match(this, Attacker, out var _, out var RecheckHit2, out var RecheckPhase2, Launcher, ApparentTarget, Projectile, flag, flag2, Prospective);
			if (RecheckPhase2)
			{
				RecheckPhase = true;
			}
			if (RecheckHit2)
			{
				RecheckHit = true;
			}
			if (flag3)
			{
				return true;
			}
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			bool flag4 = Objects[i].ConsiderSolidForProjectile(Projectile, Attacker, out var RecheckHit3, out var RecheckPhase3, Launcher, ApparentTarget, flag, flag2, Prospective, TreatAsSolidHandled: true);
			if (RecheckPhase3)
			{
				RecheckPhase = true;
			}
			if (RecheckHit3)
			{
				RecheckHit = true;
			}
			if (flag4)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSolidForProjectile(GameObject Projectile = null, GameObject Attacker = null, GameObject Launcher = null, GameObject ApparentTarget = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, bool Prospective = false)
	{
		bool RecheckHit;
		bool RecheckPhase;
		return IsSolidForProjectile(Projectile, Attacker, out RecheckHit, out RecheckPhase, Launcher, ApparentTarget, PenetrateCreatures, PenetrateWalls, Prospective);
	}

	public void FindSolidObjectForMissile(GameObject Attacker, GameObject Launcher, GameObject Projectile, out GameObject SolidObject, out bool IsSolid, out bool IsCover, out bool RecheckHit, out bool RecheckPhase, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, GameObject ApparentTarget = null)
	{
		RecheckHit = false;
		RecheckPhase = false;
		bool flag = PenetrateCreatures == true;
		bool flag2 = PenetrateWalls == true;
		if ((!PenetrateCreatures.HasValue || !PenetrateWalls.HasValue) && Launcher != null)
		{
			GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = GetMissileWeaponPerformanceEvent.GetFor(Attacker, Launcher, Projectile);
			flag = PenetrateCreatures ?? getMissileWeaponPerformanceEvent.PenetrateCreatures;
			flag2 = PenetrateWalls ?? getMissileWeaponPerformanceEvent.PenetrateWalls;
		}
		IsSolid = false;
		IsCover = false;
		TreatAsSolid treatAsSolid = Projectile?.GetPart<TreatAsSolid>();
		if (treatAsSolid != null)
		{
			GameObject phaseFrom = Projectile ?? Attacker;
			bool penetrateCreatures = flag;
			bool penetrateWalls = flag2;
			bool flag3 = treatAsSolid.Match(this, Attacker, out SolidObject, out var RecheckHit2, out var RecheckPhase2, null, ApparentTarget, phaseFrom, penetrateCreatures, penetrateWalls);
			if (RecheckPhase2)
			{
				RecheckPhase = true;
			}
			if (RecheckHit2)
			{
				RecheckHit = true;
			}
			if (flag3)
			{
				IsSolid = true;
				return;
			}
		}
		SolidObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			bool? penetrateCreatures2 = flag;
			bool? penetrateWalls2 = flag2;
			bool flag4 = gameObject.ConsiderSolidForProjectile(Projectile, Attacker, out var RecheckHit3, out var RecheckPhase3, null, ApparentTarget, penetrateCreatures2, penetrateWalls2, Prospective: false, TreatAsSolidHandled: true);
			if (RecheckPhase3)
			{
				RecheckPhase = true;
			}
			if (RecheckHit3)
			{
				RecheckHit = true;
			}
			if (flag4 && (!flag2 || !Objects[i].IsWall()))
			{
				IsSolid = true;
				SolidObject = Objects[i];
				return;
			}
		}
		int j = 0;
		for (int count2 = Objects.Count; j < count2; j++)
		{
			if (GetMissileCoverPercentageEvent.GetFor(Objects[j], Attacker, Projectile, flag, flag2).in100() && Objects[j].PhaseMatches(Projectile ?? Attacker) && (FungalVisionary.VisionLevel > 0 || !Objects[j].HasPart<FungalVision>() || Attacker.HasPart<FungalVision>()))
			{
				GameObject phaseFrom = Launcher;
				if (BeforeProjectileHitEvent.Check(Projectile, Attacker, Objects[j], out var Recheck, out var RecheckPhase4, flag, flag2, phaseFrom, ApparentTarget) && ((!RecheckPhase4 && !Recheck) || Objects[j].PhaseMatches(Projectile ?? Attacker)))
				{
					IsSolid = true;
					IsCover = true;
					SolidObject = Objects[j];
					break;
				}
			}
		}
	}

	public GameObject FindSolidObjectForMissile(GameObject Attacker, GameObject Launcher = null, GameObject Projectile = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, GameObject ApparentTarget = null)
	{
		FindSolidObjectForMissile(Attacker, Launcher, Projectile, out var SolidObject, out var _, out var _, out var _, out var _, PenetrateCreatures, PenetrateWalls, ApparentTarget);
		return SolidObject;
	}

	public bool HasSolidObjectForMissile(GameObject Attacker, GameObject Launcher = null, GameObject Projectile = null, bool PenetrateCreatures = false, bool PenetrateWalls = false)
	{
		FindSolidObjectForMissile(Attacker, Launcher, Projectile, out var _, out var IsSolid, out var _, out var _, out var _, PenetrateCreatures, PenetrateWalls);
		return IsSolid;
	}

	public bool BroadcastEvent(Event E)
	{
		switch (Objects.Count)
		{
		case 0:
			return true;
		case 1:
			return Objects[0].BroadcastEvent(E);
		case 2:
		{
			GameObject gameObject = Objects[1];
			if (!Objects[0].BroadcastEvent(E))
			{
				return false;
			}
			if (!gameObject.BroadcastEvent(E))
			{
				return false;
			}
			return true;
		}
		default:
		{
			List<GameObject> list = Event.NewGameObjectList(Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (!list[i].BroadcastEvent(E))
				{
					return false;
				}
			}
			return true;
		}
		}
	}

	public bool BroadcastEvent(Event E, IEvent PE)
	{
		bool result = BroadcastEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool BroadcastEvent(Event E, Event PE)
	{
		bool result = BroadcastEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEvent(string ID)
	{
		if (ID.IndexOf(',') != -1)
		{
			bool result = true;
			{
				foreach (string item in ID.CachedCommaExpansion())
				{
					if (!FireEvent(item))
					{
						result = false;
					}
				}
				return result;
			}
		}
		switch (Objects.Count)
		{
		case 0:
			return true;
		case 1:
			return Objects[0].FireEvent(ID);
		case 2:
		{
			GameObject gameObject4 = Objects[0];
			GameObject gameObject5 = Objects[1];
			if (!gameObject4.FireEvent(ID))
			{
				return false;
			}
			if (gameObject5 != null && gameObject5.IsValid() && !gameObject5.FireEvent(ID))
			{
				return false;
			}
			return true;
		}
		case 3:
		{
			GameObject gameObject = Objects[0];
			GameObject gameObject2 = Objects[1];
			GameObject gameObject3 = Objects[2];
			if (!gameObject.FireEvent(ID))
			{
				return false;
			}
			if (gameObject2 != null && gameObject2.IsValid() && !gameObject2.FireEvent(ID))
			{
				return false;
			}
			if (gameObject3 != null && gameObject3.IsValid() && !gameObject3.FireEvent(ID))
			{
				return false;
			}
			return true;
		}
		case 4:
		{
			GameObject gameObject6 = Objects[0];
			GameObject gameObject7 = Objects[1];
			GameObject gameObject8 = Objects[2];
			GameObject gameObject9 = Objects[3];
			if (!gameObject6.FireEvent(ID))
			{
				return false;
			}
			if (gameObject7 != null && gameObject7.IsValid() && !gameObject7.FireEvent(ID))
			{
				return false;
			}
			if (gameObject8 != null && gameObject8.IsValid() && !gameObject8.FireEvent(ID))
			{
				return false;
			}
			if (gameObject9 != null && gameObject9.IsValid() && !gameObject9.FireEvent(ID))
			{
				return false;
			}
			return true;
		}
		default:
			if (EventListInUse)
			{
				List<GameObject> list = new List<GameObject>(Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					if (list[i].IsValid() && !list[i].FireEvent(ID))
					{
						return false;
					}
				}
			}
			else
			{
				EventListInUse = true;
				try
				{
					EventList.Clear();
					EventList.AddRange(Objects);
					int j = 0;
					for (int count2 = EventList.Count; j < count2; j++)
					{
						if (EventList[j].IsValid() && !EventList[j].FireEvent(ID))
						{
							return false;
						}
					}
				}
				finally
				{
					EventList.Clear();
					EventListInUse = false;
				}
			}
			return true;
		}
	}

	public bool FireEvent(Event E)
	{
		switch (Objects.Count)
		{
		case 0:
			return true;
		case 1:
			return Objects[0].FireEvent(E);
		case 2:
		{
			GameObject gameObject4 = Objects[0];
			GameObject gameObject5 = Objects[1];
			if (!gameObject4.FireEvent(E))
			{
				return false;
			}
			if (gameObject5 != null && gameObject5.IsValid() && !gameObject5.FireEvent(E))
			{
				return false;
			}
			return true;
		}
		case 3:
		{
			GameObject gameObject = Objects[0];
			GameObject gameObject2 = Objects[1];
			GameObject gameObject3 = Objects[2];
			if (!gameObject.FireEvent(E))
			{
				return false;
			}
			if (gameObject2 != null && gameObject2.IsValid() && !gameObject2.FireEvent(E))
			{
				return false;
			}
			if (gameObject3 != null && gameObject3.IsValid() && !gameObject3.FireEvent(E))
			{
				return false;
			}
			return true;
		}
		case 4:
		{
			GameObject gameObject6 = Objects[0];
			GameObject gameObject7 = Objects[1];
			GameObject gameObject8 = Objects[2];
			GameObject gameObject9 = Objects[3];
			if (!gameObject6.FireEvent(E))
			{
				return false;
			}
			if (gameObject7 != null && gameObject7.IsValid() && !gameObject7.FireEvent(E))
			{
				return false;
			}
			if (gameObject8 != null && gameObject8.IsValid() && !gameObject8.FireEvent(E))
			{
				return false;
			}
			if (gameObject9 != null && gameObject9.IsValid() && !gameObject9.FireEvent(E))
			{
				return false;
			}
			return true;
		}
		default:
			if (EventListInUse)
			{
				List<GameObject> list = new List<GameObject>(Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					if (list[i].IsValid() && !list[i].FireEvent(E))
					{
						return false;
					}
				}
			}
			else
			{
				EventListInUse = true;
				try
				{
					EventList.Clear();
					EventList.AddRange(Objects);
					int j = 0;
					for (int count2 = EventList.Count; j < count2; j++)
					{
						if (EventList[j].IsValid() && !EventList[j].FireEvent(E))
						{
							return false;
						}
					}
				}
				finally
				{
					EventList.Clear();
					EventListInUse = false;
				}
			}
			return true;
		}
	}

	public bool FireEvent(Event E, IEvent PE)
	{
		bool result = FireEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEvent(Event E, Event PE)
	{
		bool result = FireEvent(E);
		PE?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEventDirect(Event E)
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			if (!Objects[i].FireEvent(E))
			{
				return false;
			}
		}
		return true;
	}

	public void QuickGetAdjacentCells(List<Cell> Return, bool bLocalOnly)
	{
		Return.Add(this);
		for (int i = 0; i < DirectionList.Length; i++)
		{
			string direction = DirectionList[i];
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				Return.Add(localCellFromDirection);
			}
		}
	}

	public IEnumerable<Cell> YieldAdjacentCells(int Radius, bool LocalOnly = false, bool BuiltOnly = true)
	{
		Radius = Radius * 2 + 1;
		int x = X;
		int y = Y;
		int x2 = 1;
		int y2 = 0;
		int l = 1;
		int i = 0;
		int p = 1;
		int c = Radius * Radius - 1;
		while (i < c)
		{
			x += x2;
			y += y2;
			if (p >= l)
			{
				p = 0;
				int num = x2;
				x2 = -y2;
				y2 = num;
				if (y2 == 0)
				{
					l++;
				}
			}
			Cell cellGlobal = ParentZone.GetCellGlobal(x, y, LocalOnly, BuiltOnly);
			if (cellGlobal != null)
			{
				yield return cellGlobal;
			}
			i++;
			p++;
		}
	}

	public void GetAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true, bool BuiltOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		Return.Add(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly, BuiltOnly);
				if (cellFromDirectionGlobal != null && !Return.CleanContains(cellFromDirectionGlobal) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public List<Cell> GetAdjacentCells(int Radius, bool LocalOnly = true, bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2) + 1);
		GetAdjacentCells(Radius, list, LocalOnly, BuiltOnly);
		return list;
	}

	public void GetRealAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		Return.Add(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && !Return.CleanContains(cellFromDirectionGlobal) && cellFromDirectionGlobal.RealDistanceTo(this) <= (double)Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public List<Cell> GetRealAdjacentCells(int Radius, bool LocalOnly = true)
	{
		List<Cell> list = Event.NewCellList();
		GetRealAdjacentCells(Radius, list, LocalOnly);
		return list;
	}

	public bool IsAdjacentTo(Cell C, bool BuiltOnly = true)
	{
		if (C == null)
		{
			return false;
		}
		return GetCellFromDirectionOfCell(C, BuiltOnly) == C;
	}

	public Cell GetCellOrFirstConnectedSpawnLocation(bool bLocalOnly = true)
	{
		if (IsEmpty())
		{
			return this;
		}
		return GetConnectedSpawnLocation(bLocalOnly) ?? this;
	}

	public Cell GetConnectedSpawnLocation(bool bLocalOnly = true)
	{
		List<Cell> list = Event.NewCellList();
		GetConnectedSpawnLocations(1, list, bLocalOnly);
		if (list.Count <= 0)
		{
			return GetFirstEmptyAdjacentCell();
		}
		return list[0] ?? this;
	}

	public List<Cell> GetConnectedSpawnLocations(int HowMany)
	{
		List<Cell> list = Event.NewCellList();
		GetConnectedSpawnLocations(HowMany, list);
		return list;
	}

	public void GetConnectedSpawnLocations(int HowMany, List<Cell> Return, bool bLocalOnly = true)
	{
		List<Cell> list = new List<Cell>();
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0 && Return.Count < HowMany)
		{
			Cell cell = cleanQueue.Dequeue();
			list.Add(cell);
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, bLocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !list.Contains(cellFromDirectionGlobal))
				{
					if (cellFromDirectionGlobal.IsPassable() && cellFromDirectionGlobal.IsSpawnable())
					{
						Return.Add(cellFromDirectionGlobal);
					}
					if (cellFromDirectionGlobal.IsPassable(null, IncludeCombatObjects: false))
					{
						cleanQueue.Enqueue(cellFromDirectionGlobal);
					}
				}
			}
		}
	}

	public void GetPassableConnectedAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true, GameObject Object = null, bool IncludeCombatObjects = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.Contains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsPassable(Object, IncludeCombatObjects) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public Cell GetFirstPassableConnectedAdjacentCell(int Radius = 80, bool LocalOnly = true, Predicate<Cell> Filter = null, GameObject Object = null, bool IncludeCombatObjects = true)
	{
		HashSet<Cell> hashSet = new HashSet<Cell>();
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		List<string> list = new List<string>(DirectionList);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			list.ShuffleInPlace();
			foreach (string item in list)
			{
				Cell cell2 = cell?.GetCellFromDirectionGlobal(item, LocalOnly);
				if (cell2 != null && cell2 != this && cell2.IsPassable(Object, IncludeCombatObjects) && cell2.PathDistanceTo(this) <= Radius && (Filter == null || Filter(cell2)))
				{
					return cell2;
				}
				if (!hashSet.Contains(cell2))
				{
					hashSet.Add(cell2);
					cleanQueue.Enqueue(cell2);
				}
			}
		}
		return null;
	}

	public List<Cell> GetPassableConnectedAdjacentCells(int Radius, bool LocalOnly = true, GameObject Object = null, bool IncludeCombatObjects = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		GetPassableConnectedAdjacentCells(Radius, list, LocalOnly, Object, IncludeCombatObjects);
		return list;
	}

	public void GetEmptyConnectedAdjacentCells(int Radius, List<Cell> Return, bool LocalOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.Contains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsEmpty() && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public Cell GetFirstEmptyConnectedAdjacentCell(int Radius, bool LocalOnly = true, Predicate<Cell> Filter = null)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		List<string> list = new List<string>(DirectionList);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			list.ShuffleInPlace();
			foreach (string item in list)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(item, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && cellFromDirectionGlobal.IsEmpty() && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius && (Filter == null || Filter(cellFromDirectionGlobal)))
				{
					return cellFromDirectionGlobal;
				}
			}
		}
		return null;
	}

	public List<Cell> GetEmptyConnectedAdjacentCells(int Radius, bool LocalOnly = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		GetEmptyConnectedAdjacentCells(Radius, list, LocalOnly);
		return list;
	}

	public void GetEmptyConnectedAdjacentCellsIgnoring(int Radius, List<Cell> Return, Predicate<GameObject> Filter, bool LocalOnly = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.CleanContains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsEmptyIgnoring(Filter) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					Return.Add(cellFromDirectionGlobal);
				}
			}
		}
	}

	public List<Cell> GetEmptyConnectedAdjacentCellsIgnoring(int Radius, Predicate<GameObject> Filter, bool LocalOnly = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		GetEmptyConnectedAdjacentCellsIgnoring(Radius, list, Filter, LocalOnly);
		return list;
	}

	public void GetPassableConnectedAdjacentCellsFor(List<Cell> Return, GameObject For, int Radius, Predicate<Cell> Filter = null, bool LocalOnly = true, bool ExcludeWalls = true)
	{
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		cleanQueue.Enqueue(this);
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirectionGlobal = cell.GetCellFromDirectionGlobal(direction, LocalOnly);
				if (cellFromDirectionGlobal != null && cellFromDirectionGlobal != this && !Return.Contains(cellFromDirectionGlobal) && cellFromDirectionGlobal.IsPassable(For) && cellFromDirectionGlobal.PathDistanceTo(this) <= Radius)
				{
					cleanQueue.Enqueue(cellFromDirectionGlobal);
					if (Filter == null || Filter(cellFromDirectionGlobal))
					{
						Return.Add(cellFromDirectionGlobal);
					}
				}
			}
		}
	}

	public List<Cell> GetPassableConnectedAdjacentCellsFor(GameObject For, int Radius, Predicate<Cell> Filter = null, bool LocalOnly = true, bool ExcludeWalls = true)
	{
		List<Cell> list = new List<Cell>();
		GetPassableConnectedAdjacentCellsFor(list, For, Radius, Filter, LocalOnly, ExcludeWalls);
		return list;
	}

	public List<Cell> GetCardinalAdjacentCellsWhere(Predicate<Cell> test, bool bLocalOnly = false, bool BuiltOnly = true, bool IncludeThis = false)
	{
		List<Cell> list = new List<Cell>(DirectionListCardinalOnly.Length);
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cell = (bLocalOnly ? GetLocalCellFromDirection(direction) : GetCellFromDirection(direction));
			if (cell != null && test(cell))
			{
				list.Add(cell);
			}
		}
		if (IncludeThis)
		{
			list.Add(this);
		}
		return list;
	}

	public List<Cell> GetCardinalAdjacentCells(bool bLocalOnly = false, bool BuiltOnly = true, bool IncludeThis = false)
	{
		List<Cell> list = new List<Cell>(DirectionListCardinalOnly.Length);
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cell = (bLocalOnly ? GetLocalCellFromDirection(direction) : GetCellFromDirection(direction));
			if (cell != null)
			{
				list.Add(cell);
			}
		}
		if (IncludeThis)
		{
			list.Add(this);
		}
		return list;
	}

	public void ForeachCardinalAdjacentCell(Action<Cell> aProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null)
			{
				aProc(cellFromDirection);
			}
		}
	}

	public bool ForeachCardinalAdjacentCell(Predicate<Cell> pProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && !pProc(cellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public void ForeachCardinalAdjacentLocalCell(Action<Cell> aProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				aProc(localCellFromDirection);
			}
		}
	}

	public bool ForeachCardinalAdjacentLocalCell(Predicate<Cell> pProc)
	{
		string[] directionListCardinalOnly = DirectionListCardinalOnly;
		foreach (string direction in directionListCardinalOnly)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !pProc(localCellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasLOSTo(GameObject go)
	{
		if (ParentZone == null)
		{
			return false;
		}
		if (go == null)
		{
			return false;
		}
		Cell currentCell = go.GetCurrentCell();
		if (currentCell == null)
		{
			return false;
		}
		return ParentZone.CalculateLOS(X, Y, currentCell.X, currentCell.Y);
	}

	public bool HasAdjacentAquaticSupportFor(GameObject obj)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.HasAquaticSupportFor(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAdjacentNonAquaticSupportFor(GameObject obj)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !localCellFromDirection.HasAquaticSupportFor(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAdjacentLocalNonwallCell()
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !localCellFromDirection.HasWall())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAdjacentLocalWallCell()
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.HasWall())
			{
				return true;
			}
		}
		return false;
	}

	public List<Cell> GetEmptyAdjacentCells(int MinRadius, int MaxRadius)
	{
		List<Cell> list = new List<Cell>();
		for (int i = Y - MaxRadius; i <= Y + MaxRadius; i++)
		{
			for (int j = X - MaxRadius; j <= X + MaxRadius; j++)
			{
				Cell cell = ParentZone.GetCell(j, i);
				if (cell != null && PathDistanceTo(cell) >= MinRadius && PathDistanceTo(cell) <= MaxRadius && cell.IsEmpty())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyAdjacentCells()
	{
		List<Cell> list = new List<Cell>();
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && cellFromDirection.IsEmpty())
			{
				list.Add(cellFromDirection);
			}
		}
		return list;
	}

	public Cell GetFirstEmptyAdjacentCell(int MinRadius, int MaxRadius)
	{
		for (int i = Y - MaxRadius; i <= Y + MaxRadius; i++)
		{
			for (int j = X - MaxRadius; j <= X + MaxRadius; j++)
			{
				Cell cell = ParentZone.GetCell(j, i);
				if (cell != null && PathDistanceTo(cell) >= MinRadius && PathDistanceTo(cell) <= MaxRadius && cell.IsEmpty())
				{
					return cell;
				}
			}
		}
		return null;
	}

	public Cell GetFirstEmptyAdjacentCell()
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && cellFromDirection.IsEmpty())
			{
				return cellFromDirection;
			}
		}
		return null;
	}

	public List<Cell> GetLocalEmptyAdjacentCells()
	{
		List<Cell> list = new List<Cell>();
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.IsEmpty())
			{
				list.Add(localCellFromDirection);
			}
		}
		return list;
	}

	public List<Cell> GetLocalEmptyOfSolidAdjacentCells()
	{
		List<Cell> list = new List<Cell>();
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && localCellFromDirection.IsEmptyOfSolid())
			{
				list.Add(localCellFromDirection);
			}
		}
		return list;
	}

	public List<Cell> GetNavigableAdjacentCells(GameObject who, int MaxWeight = 5, bool builtOnly = true)
	{
		List<Cell> list = Event.NewCellList();
		if (ParentZone != null)
		{
			ParentZone.CalculateNavigationMap(who);
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell cellFromDirection = GetCellFromDirection(direction, builtOnly);
				if (cellFromDirection != null && ParentZone.NavigationMap[cellFromDirection.X, cellFromDirection.Y].Weight <= MaxWeight)
				{
					list.Add(cellFromDirection);
				}
			}
		}
		return list;
	}

	public List<Cell> GetLocalNavigableAdjacentCells(GameObject who, int MaxWeight = 5)
	{
		List<Cell> list = Event.NewCellList();
		if (ParentZone != null)
		{
			ParentZone.CalculateNavigationMap(who);
			string[] directionList = DirectionList;
			foreach (string direction in directionList)
			{
				Cell localCellFromDirection = GetLocalCellFromDirection(direction);
				if (localCellFromDirection != null && ParentZone.NavigationMap[localCellFromDirection.X, localCellFromDirection.Y].Weight <= MaxWeight)
				{
					list.Add(localCellFromDirection);
				}
			}
		}
		return list;
	}

	public List<Cell> GetAdjacentCells(bool BuiltOnly = true)
	{
		List<Cell> list = Event.NewCellList();
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null)
			{
				list.Add(cellFromDirection);
			}
		}
		return list;
	}

	public bool AnyAdjacentCell(Predicate<Cell> Filter, bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && Filter(cellFromDirection))
			{
				return true;
			}
		}
		return false;
	}

	public Cell GetFirstAdjacentCell(Predicate<Cell> Filter)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction);
			if (cellFromDirection != null && Filter(cellFromDirection))
			{
				return cellFromDirection;
			}
		}
		return null;
	}

	/// <summary>Get nearest navigable cell, a random cell is picked among the ones with the same distance.</summary>
	/// <param name="Object">An actor used for recalculating the navigation map.</param>
	/// <param name="MaxWeight">The maximum navigation weight to allow for returned cell.</param>
	/// <param name="Random">A generator used to select a pivot point when multiple cells are the same distance away.</param>
	/// <param name="Recalculate">Recalculate the zone navigation map before processing.</param>
	public Cell GetNearestNavigableCell(GameObject Object, int MaxWeight = 5, System.Random Random = null, bool Recalculate = true)
	{
		if (ParentZone == null)
		{
			return null;
		}
		if (Recalculate)
		{
			ParentZone.CalculateNavigationMap(Object);
		}
		if (ParentZone.NavigationMap[X, Y].Weight <= MaxWeight)
		{
			return this;
		}
		if (Random == null)
		{
			Random = Stat.Rnd;
		}
		Cell result = null;
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = Random.Next(ParentZone.Width) + Random.Next(ParentZone.Height) * ParentZone.Width;
		for (int i = 0; i < ParentZone.Height; i++)
		{
			for (int j = 0; j < ParentZone.Width; j++)
			{
				if (ParentZone.NavigationMap[j, i].Weight > MaxWeight)
				{
					continue;
				}
				int num4 = DistanceTo(j, i);
				if (num4 > num)
				{
					continue;
				}
				if (num4 == num)
				{
					int num5 = Math.Abs(num3 - j - i * ParentZone.Width);
					if (num5 >= num2)
					{
						continue;
					}
					num2 = num5;
				}
				num = num4;
				result = ParentZone.Map[j][i];
			}
		}
		return result;
	}

	public void ForeachAdjacentCell(Action<Cell> aProc, bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null)
			{
				aProc(cellFromDirection);
			}
		}
	}

	public bool ForeachAdjacentCell(Predicate<Cell> pProc, bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && !pProc(cellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasUnexploredAdjacentCell(bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && !cellFromDirection.IsExplored())
			{
				return true;
			}
		}
		return false;
	}

	public Cell GetFirstNonOccludingAdjacentCell(bool BuiltOnly = true)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell cellFromDirection = GetCellFromDirection(direction, BuiltOnly);
			if (cellFromDirection != null && !cellFromDirection.IsOccluding())
			{
				return cellFromDirection;
			}
		}
		return null;
	}

	public List<Cell> GetLocalAdjacentCells(int Radius, bool IncludeSelf = false)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2) + (IncludeSelf ? 1 : 0));
		if (IncludeSelf)
		{
			list.Add(this);
		}
		for (int i = Math.Max(Y - Radius, 0); i <= Y + Radius && i <= ParentZone.Height - 1; i++)
		{
			for (int j = Math.Max(X - Radius, 0); j <= X + Radius && j <= ParentZone.Width - 1; j++)
			{
				Cell cell = ParentZone.GetCell(j, i);
				if (cell != this)
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell GetRandomLocalAdjacentCell()
	{
		List<Cell> localAdjacentCells = GetLocalAdjacentCells(1);
		if (localAdjacentCells == null || localAdjacentCells.Count == 0)
		{
			return this;
		}
		return localAdjacentCells.GetRandomElement();
	}

	public Cell GetRandomLocalAdjacentCell(Predicate<Cell> Filter)
	{
		List<Cell> localAdjacentCells = GetLocalAdjacentCells();
		if (localAdjacentCells == null || localAdjacentCells.Count == 0)
		{
			return this;
		}
		return localAdjacentCells.GetRandomElement(Filter);
	}

	public Cell GetRandomLocalAdjacentCell(int Radius, bool IncludeSelf = false)
	{
		return GetLocalAdjacentCellsAtRadius(Radius, IncludeSelf).GetRandomElement();
	}

	public Cell GetRandomLocalAdjacentCell(int Radius, Predicate<Cell> Filter, bool IncludeSelf = false)
	{
		return (from C in GetLocalAdjacentCellsAtRadius(Radius, IncludeSelf)
			where Filter(C)
			select C).GetRandomElement();
	}

	public List<Cell> GetLocalAdjacentCellsCircular(int Radius, bool includeSelf = false)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		for (int i = Math.Max(Y - Radius, 0); i <= Y + Radius && i <= ParentZone.Height - 1; i++)
		{
			for (int j = Math.Max(X - Radius, 0); j <= X + Radius && j <= ParentZone.Width - 1; j++)
			{
				Cell cell = ParentZone.GetCell(j, i);
				if (cell != this && cell.Location.Distance(Location) <= Radius)
				{
					list.Add(cell);
				}
			}
		}
		if (includeSelf)
		{
			list.Add(this);
		}
		return list;
	}

	public List<Cell> GetLocalAdjacentCellsAtRadius(int Radius, bool includeSelf = true)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2) - (Radius + 1) * (Radius + 1));
		for (int i = Math.Max(Y - Radius, 0); i <= Y + Radius && i <= ParentZone.Height - 1; i++)
		{
			for (int j = Math.Max(X - Radius, 0); j <= X + Radius && j <= ParentZone.Width - 1; j++)
			{
				Cell cell = ParentZone.GetCell(j, i);
				if ((cell != this || includeSelf) && cell.DistanceTo(this) == Radius)
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetLocalAdjacentCellsAtRadius(int Radius, Predicate<Cell> Filter)
	{
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2) - (Radius + 1) * (Radius + 1));
		for (int i = Math.Max(Y - Radius, 0); i <= Y + Radius && i <= ParentZone.Height - 1; i++)
		{
			for (int j = Math.Max(X - Radius, 0); j <= X + Radius && j <= ParentZone.Width - 1; j++)
			{
				Cell cell = ParentZone.GetCell(j, i);
				if (cell.DistanceTo(this) == Radius && (Filter == null || Filter(cell)))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell GetRandomLocalAdjacentCellAtRadius(int Radius)
	{
		return GetLocalAdjacentCellsAtRadius(Radius).GetRandomElement();
	}

	public Cell GetRandomLocalAdjacentCellAtRadius(int Radius, Predicate<Cell> Filter)
	{
		return GetLocalAdjacentCellsAtRadius(Radius, Filter).GetRandomElement();
	}

	public List<Cell> GetLocalAdjacentCells()
	{
		if (_LocalAdjacentCellCache == null)
		{
			_LocalAdjacentCellCache = new List<Cell>();
			for (int i = 0; i < DirectionList.Length; i++)
			{
				string direction = DirectionList[i];
				Cell localCellFromDirection = GetLocalCellFromDirection(direction);
				if (localCellFromDirection != null)
				{
					_LocalAdjacentCellCache.Add(localCellFromDirection);
				}
			}
		}
		return _LocalAdjacentCellCache;
	}

	public void ForeachLocalAdjacentCell(Action<Cell> aProc)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				aProc(localCellFromDirection);
			}
		}
	}

	public bool ForeachLocalAdjacentCell(Predicate<Cell> pProc)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !pProc(localCellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public void ForeachLocalAdjacentCellAndSelf(Action<Cell> aProc)
	{
		aProc(this);
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null)
			{
				aProc(localCellFromDirection);
			}
		}
	}

	public bool ForeachLocalAdjacentCellAndSelf(Predicate<Cell> pProc)
	{
		if (!pProc(this))
		{
			return false;
		}
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && !pProc(localCellFromDirection))
			{
				return false;
			}
		}
		return true;
	}

	public Cell FindLocalAdjacentCell(Predicate<Cell> Filter)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && Filter(localCellFromDirection))
			{
				return localCellFromDirection;
			}
		}
		return null;
	}

	public bool AnyLocalAdjacentCell(Predicate<Cell> Filter)
	{
		string[] directionList = DirectionList;
		foreach (string direction in directionList)
		{
			Cell localCellFromDirection = GetLocalCellFromDirection(direction);
			if (localCellFromDirection != null && Filter(localCellFromDirection))
			{
				return true;
			}
		}
		return false;
	}

	public List<Cell> GetLocalCardinalAdjacentCells()
	{
		if (_LocalCardinalAdjacentCellCache == null)
		{
			_LocalCardinalAdjacentCellCache = new List<Cell>(DirectionListCardinalOnly.Length);
			for (int i = 0; i < DirectionListCardinalOnly.Length; i++)
			{
				string direction = DirectionListCardinalOnly[i];
				Cell localCellFromDirection = GetLocalCellFromDirection(direction);
				if (localCellFromDirection != null)
				{
					_LocalCardinalAdjacentCellCache.Add(localCellFromDirection);
				}
			}
		}
		return _LocalCardinalAdjacentCellCache;
	}

	public Cell GetRandomLocalCardinalAdjacentCell()
	{
		List<Cell> localCardinalAdjacentCells = GetLocalCardinalAdjacentCells();
		if (localCardinalAdjacentCells == null || localCardinalAdjacentCells.Count == 0)
		{
			return null;
		}
		return localCardinalAdjacentCells.GetRandomElement();
	}

	public Cell GetRandomLocalCardinalAdjacentCell(Predicate<Cell> Filter)
	{
		if (Filter == null)
		{
			return GetRandomLocalCardinalAdjacentCell();
		}
		List<Cell> localCardinalAdjacentCells = GetLocalCardinalAdjacentCells();
		if (localCardinalAdjacentCells == null || localCardinalAdjacentCells.Count == 0)
		{
			return null;
		}
		return localCardinalAdjacentCells.GetRandomElement(Filter);
	}

	public string GetDirectionFrom(Location2D Target)
	{
		if (Target == null || (Target.X == X && Target.Y == Y))
		{
			return ".";
		}
		int x = X;
		int y = Y;
		int x2 = Target.X;
		int y2 = Target.Y;
		if (x == x2)
		{
			if (y == y2)
			{
				return ".";
			}
			if (y < y2)
			{
				return "S";
			}
			return "N";
		}
		if (x < x2)
		{
			if (y == y2)
			{
				return "E";
			}
			if (y < y2)
			{
				return "SE";
			}
			return "NE";
		}
		if (y == y2)
		{
			return "W";
		}
		if (y < y2)
		{
			return "SW";
		}
		return "NW";
	}

	public string GetGeneralDirectionFrom(Location2D Target)
	{
		if (Target == null)
		{
			return ".";
		}
		int x = X;
		int y = Y;
		int x2 = Target.X;
		int y2 = Target.Y;
		bool num = x == x2 || Math.Abs(x - x2) < Math.Abs(y - y2) / 2;
		bool flag = y == y2 || Math.Abs(y - y2) < Math.Abs(x - x2) / 2;
		if (num)
		{
			if (flag)
			{
				return ".";
			}
			if (y < y2)
			{
				return "S";
			}
			return "N";
		}
		if (x < x2)
		{
			if (flag)
			{
				return "E";
			}
			if (y < y2)
			{
				return "SE";
			}
			return "NE";
		}
		if (flag)
		{
			return "W";
		}
		if (y < y2)
		{
			return "SW";
		}
		return "NW";
	}

	public string GetDirectionFromCell(Cell Target, bool NullIfSame = false)
	{
		if (Target == null || Target == this)
		{
			if (!NullIfSame)
			{
				return ".";
			}
			return null;
		}
		Zone parentZone = Target.ParentZone;
		int num;
		int num2;
		int num3;
		int num4;
		if (parentZone != ParentZone)
		{
			if (parentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return ".";
			}
			num = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			num2 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			num3 = parentZone.GetZonewX() * Definitions.Width * 80 + parentZone.GetZoneX() * 80 + Target.X;
			num4 = parentZone.GetZonewY() * Definitions.Height * 25 + parentZone.GetZoneY() * 25 + Target.Y;
		}
		else
		{
			num = X;
			num2 = Y;
			num3 = Target.X;
			num4 = Target.Y;
		}
		if (num == num3)
		{
			if (num2 == num4)
			{
				if (!NullIfSame)
				{
					return ".";
				}
				return null;
			}
			if (num2 < num4)
			{
				return "S";
			}
			return "N";
		}
		if (num < num3)
		{
			if (num2 == num4)
			{
				return "E";
			}
			if (num2 < num4)
			{
				return "SE";
			}
			return "NE";
		}
		if (num2 == num4)
		{
			return "W";
		}
		if (num2 < num4)
		{
			return "SW";
		}
		return "NW";
	}

	public string GetGeneralDirectionFromCell(Cell Target)
	{
		if (Target == null)
		{
			return ".";
		}
		Zone parentZone = Target.ParentZone;
		int num;
		int num2;
		int num3;
		int num4;
		if (parentZone != ParentZone)
		{
			if (parentZone.ZoneWorld != ParentZone.ZoneWorld)
			{
				return ".";
			}
			num = ParentZone.GetZonewX() * Definitions.Width * 80 + ParentZone.GetZoneX() * 80 + X;
			num2 = ParentZone.GetZonewY() * Definitions.Height * 25 + ParentZone.GetZoneY() * 25 + Y;
			num3 = parentZone.GetZonewX() * Definitions.Width * 80 + parentZone.GetZoneX() * 80 + Target.X;
			num4 = parentZone.GetZonewY() * Definitions.Height * 25 + parentZone.GetZoneY() * 25 + Target.Y;
		}
		else
		{
			num = X;
			num2 = Y;
			num3 = Target.X;
			num4 = Target.Y;
		}
		bool num5 = num == num3 || Math.Abs(num - num3) < Math.Abs(num2 - num4) / 2;
		bool flag = num2 == num4 || Math.Abs(num2 - num4) < Math.Abs(num - num3) / 2;
		if (num5)
		{
			if (flag)
			{
				return ".";
			}
			if (num2 < num4)
			{
				return "S";
			}
			return "N";
		}
		if (num < num3)
		{
			if (flag)
			{
				return "E";
			}
			if (num2 < num4)
			{
				return "SE";
			}
			return "NE";
		}
		if (flag)
		{
			return "W";
		}
		if (num2 < num4)
		{
			return "SW";
		}
		return "NW";
	}

	public Cell GetCellFromDelta(ref float xp, ref float yp, float xd, float yd, bool global = false)
	{
		if (xp == 0f && yp == 0f)
		{
			return this;
		}
		while ((int)xp == X && (int)yp == Y)
		{
			xp += xd;
			yp += yd;
		}
		int num = (int)xp;
		int num2 = (int)yp;
		if (num < 0)
		{
			return null;
		}
		if (num > 79)
		{
			return null;
		}
		if (num2 < 0)
		{
			return null;
		}
		if (num2 > 24)
		{
			return null;
		}
		return ParentZone.GetCell(num, num2);
	}

	public Cell GetCellFromOffset(int xd, int yd)
	{
		return ParentZone.GetCell(X + xd, Y + yd);
	}

	public IEnumerable<Cell> GetCellsInACosmeticCircle(int radius)
	{
		int yradius = (int)Math.Max(1.0, (double)radius * 0.66);
		float radius_squared = radius * radius;
		for (int x = X - radius; x <= X + radius; x++)
		{
			for (int y = Y - yradius; y <= Y + yradius; y++)
			{
				float num = Math.Abs(x - X);
				float num2 = (float)Math.Abs(y - Y) * 1.3333f;
				float num3 = num * num + num2 * num2;
				Debug.Log("xd: " + num + " yd:" + num2 + " d=" + num3);
				if (num3 <= radius_squared && ParentZone.GetCell(x, y) != null)
				{
					yield return ParentZone.GetCell(x, y);
				}
			}
		}
	}

	public IEnumerable<Cell> GetCellsInABox(int width, int height)
	{
		for (int x = X; x <= X + width; x++)
		{
			for (int y = Y; y <= Y + height; y++)
			{
				yield return ParentZone.GetCell(x, y);
			}
		}
	}

	public Cell GetCellFromDirection(string Direction, bool BuiltOnly = true)
	{
		return GetCellFromDirectionGlobal(Direction, bLocalOnly: false, BuiltOnly);
	}

	public List<Cell> GetDirectionAndAdjacentCells(string Direction, bool LocalOnly = true, bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>((Direction == "." || Direction == "?") ? 10 : 3);
		Cell cellFromDirectionGlobal = GetCellFromDirectionGlobal(Direction, LocalOnly, BuiltOnly);
		if (cellFromDirectionGlobal != null)
		{
			list.Add(cellFromDirectionGlobal);
		}
		string[] adjacentDirections = Directions.GetAdjacentDirections(Direction);
		foreach (string direction in adjacentDirections)
		{
			Cell cellFromDirectionGlobal2 = GetCellFromDirectionGlobal(direction, LocalOnly, BuiltOnly);
			if (cellFromDirectionGlobal2 != null)
			{
				list.Add(cellFromDirectionGlobal2);
			}
		}
		return list;
	}

	public Dictionary<string, Cell> GetAdjacentDirectionCellMap(string Direction, bool BuiltOnly = true)
	{
		Dictionary<string, Cell> dictionary = new Dictionary<string, Cell>((Direction == "." || Direction == "?") ? 10 : 3);
		Cell cellFromDirection = GetCellFromDirection(Direction, BuiltOnly);
		if (cellFromDirection != null)
		{
			dictionary.Add(Direction, cellFromDirection);
		}
		string[] adjacentDirections = Directions.GetAdjacentDirections(Direction);
		foreach (string text in adjacentDirections)
		{
			Cell cellFromDirection2 = GetCellFromDirection(text, BuiltOnly);
			if (cellFromDirection2 != null)
			{
				dictionary.Add(text, cellFromDirection2);
			}
		}
		return dictionary;
	}

	public Cell GetLocalCellFromDirection(string Direction, bool BuiltOnly = true)
	{
		return GetCellFromDirectionGlobal(Direction, bLocalOnly: true, BuiltOnly);
	}

	public List<Cell> GetLocalDirectionAndAdjacentCells(string Direction, bool BuiltOnly = true)
	{
		List<Cell> list = new List<Cell>((Direction == "." || Direction == "?") ? 10 : 3);
		Cell localCellFromDirection = GetLocalCellFromDirection(Direction, BuiltOnly);
		if (localCellFromDirection != null)
		{
			list.Add(localCellFromDirection);
		}
		string[] adjacentDirections = Directions.GetAdjacentDirections(Direction);
		foreach (string direction in adjacentDirections)
		{
			if (GetLocalCellFromDirection(direction, BuiltOnly) != null)
			{
				list.Add(localCellFromDirection);
			}
		}
		return list;
	}

	public Cell GetCellFromDirectionOfCell(Cell C, bool BuiltOnly = true)
	{
		string directionFromCell = GetDirectionFromCell(C);
		if (directionFromCell == "." || directionFromCell == "?")
		{
			return null;
		}
		return GetCellFromDirection(directionFromCell, BuiltOnly);
	}

	public Cell GetLocalCellFromDirectionOfCell(Cell C, bool BuiltOnly = true)
	{
		string directionFromCell = GetDirectionFromCell(C);
		if (directionFromCell == "." || directionFromCell == "?")
		{
			return null;
		}
		return GetLocalCellFromDirection(directionFromCell, BuiltOnly);
	}

	/// <summary>
	/// We use this in the inner loop of pathfinding where it's really slammed thousands of times.
	///
	/// Performance matters a lot.
	///
	/// Don't generate garbage!
	/// </summary>
	/// <param name="Direction" />
	/// <param name="validExternalDirection" />
	/// <param name="validDestinationZone" />
	/// <returns />
	public Cell GetCellFromDirectionWithOneValidExternalDirection(string Direction, string validExternalDirection, string validDestinationZone)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap())
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			if (Direction != validExternalDirection)
			{
				return null;
			}
			ParentZone.GetZoneWorld();
			int num;
			int num2;
			int zoneX;
			int zoneY;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				zoneX = ParentZone.GetZoneX();
				zoneY = ParentZone.GetZoneY();
				ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && zoneX == 0)
				{
					num--;
					zoneX = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					zoneX--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && zoneX == Definitions.Width - 1)
				{
					num++;
					zoneX = 0;
					x = 0;
				}
				else
				{
					zoneX++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && zoneY == 0)
				{
					num2--;
					zoneY = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					zoneY--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && zoneY == Definitions.Height - 1)
				{
					num2++;
					zoneY = 0;
					y = 0;
				}
				else
				{
					zoneY++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zone = zoneManager.GetZone(validDestinationZone);
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public Cell GetCellFromDirectionFiltered(string Direction, List<string> ZoneFilter)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap())
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			string zoneWorld = ParentZone.GetZoneWorld();
			int num;
			int num2;
			int num3;
			int num4;
			int zoneZ;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				num3 = ParentZone.GetZoneX();
				num4 = ParentZone.GetZoneY();
				zoneZ = ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && num3 == 0)
				{
					num--;
					num3 = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					num3--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && num3 == Definitions.Width - 1)
				{
					num++;
					num3 = 0;
					x = 0;
				}
				else
				{
					num3++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && num4 == 0)
				{
					num2--;
					num4 = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					num4--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && num4 == Definitions.Height - 1)
				{
					num2++;
					num4 = 0;
					y = 0;
				}
				else
				{
					num4++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zoneZ = z;
			string text = ZoneID.Assemble(zoneWorld, num, num2, num3, num4, zoneZ);
			if (!ZoneFilter.Contains(text))
			{
				return null;
			}
			zone = zoneManager.GetZone(text);
			if (zone == null)
			{
				return null;
			}
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public Cell GetCellFromDirectionGlobalIfBuilt(string Direction)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap())
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			string zoneWorld = ParentZone.GetZoneWorld();
			int num;
			int num2;
			int num3;
			int num4;
			int zoneZ;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				num3 = ParentZone.GetZoneX();
				num4 = ParentZone.GetZoneY();
				zoneZ = ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && num3 == 0)
				{
					num--;
					num3 = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					num3--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && num3 == Definitions.Width - 1)
				{
					num++;
					num3 = 0;
					x = 0;
				}
				else
				{
					num3++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && num4 == 0)
				{
					num2--;
					num4 = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					num4--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && num4 == Definitions.Height - 1)
				{
					num2++;
					num4 = 0;
					y = 0;
				}
				else
				{
					num4++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zoneZ = z;
			string zoneID = ZoneID.Assemble(zoneWorld, num, num2, num3, num4, zoneZ);
			if (!XRLCore.Core.Game.ZoneManager.IsZoneBuilt(zoneID))
			{
				return null;
			}
			zone = zoneManager.GetZone(zoneID);
			if (zone == null)
			{
				return null;
			}
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public Cell GetCellFromDirectionGlobal(string Direction, bool bLocalOnly = true, bool bLiveZonesOnly = true)
	{
		int x = X;
		int y = Y;
		int z = ParentZone.GetZoneZ();
		if (Direction == "." || Direction == "?")
		{
			return this;
		}
		Directions.ApplyDirection(Direction, ref x, ref y, ref z);
		Zone zone = null;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		if (ParentZone.IsWorldMap() | bLocalOnly)
		{
			if (x < 0)
			{
				return null;
			}
			if (y < 0)
			{
				return null;
			}
			if (x > 79)
			{
				return null;
			}
			if (y > 24)
			{
				return null;
			}
		}
		if (x < 0 || y < 0 || x >= ParentZone.Width || y >= ParentZone.Height || z != ParentZone.GetZoneZ())
		{
			string zoneWorld = ParentZone.GetZoneWorld();
			int num;
			int num2;
			int num3;
			int num4;
			int zoneZ;
			try
			{
				num = ParentZone.GetZonewX();
				num2 = ParentZone.GetZonewY();
				num3 = ParentZone.GetZoneX();
				num4 = ParentZone.GetZoneY();
				zoneZ = ParentZone.GetZoneZ();
			}
			catch
			{
				return null;
			}
			if (x < 0 || x >= ParentZone.Width)
			{
				if (x < 0 && num3 == 0)
				{
					num--;
					num3 = Definitions.Width - 1;
					x = ParentZone.Width - 1;
				}
				else if (x < 0)
				{
					num3--;
					x = ParentZone.Width - 1;
				}
				else if (x >= ParentZone.Width && num3 == Definitions.Width - 1)
				{
					num++;
					num3 = 0;
					x = 0;
				}
				else
				{
					num3++;
					x = 0;
				}
			}
			if (y < 0 || y >= ParentZone.Height)
			{
				if (y < 0 && num4 == 0)
				{
					num2--;
					num4 = Definitions.Height - 1;
					y = ParentZone.Height - 1;
				}
				else if (y < 0)
				{
					num4--;
					y = ParentZone.Height - 1;
				}
				else if (y >= ParentZone.Height && num4 == Definitions.Height - 1)
				{
					num2++;
					num4 = 0;
					y = 0;
				}
				else
				{
					num4++;
					y = 0;
				}
			}
			if (num < 0)
			{
				return null;
			}
			if (num2 < 0)
			{
				return null;
			}
			if (num > 79)
			{
				return null;
			}
			if (num2 > 24)
			{
				return null;
			}
			zoneZ = z;
			if (bLiveZonesOnly && (zoneManager == null || !zoneManager.IsZoneLive(zoneWorld, num, num2, num3, num4, zoneZ)))
			{
				return null;
			}
			zone = zoneManager.GetZone(zoneWorld, num, num2, num3, num4, zoneZ);
			if (zone == null)
			{
				return null;
			}
		}
		if (zone == null)
		{
			return ParentZone.GetCell(x, y);
		}
		return zone.GetCell(x, y);
	}

	public void ClearObjectsWithPart(string Part, bool Important = false, bool Combat = false)
	{
		GameObject gameObject = null;
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasPart(Part) || !Objects[i].CanClear(Important, Combat))
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list == null)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			RemoveObject(item);
		}
	}

	public void ClearObjectsWithProperty(string Property)
	{
		GameObject gameObject = null;
		List<GameObject> list = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasStringProperty(Property) || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list != null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				RemoveObject(list[j]);
			}
		}
	}

	public void ClearObjectsWithIntProperty(string Property)
	{
		List<GameObject> list = null;
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].GetIntProperty(Property) <= 0 || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list != null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				RemoveObject(list[j]);
			}
		}
	}

	public void ClearObjectsWithTag(string Tag)
	{
		List<GameObject> list = null;
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag(Tag) || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list != null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				RemoveObject(list[j]);
			}
		}
	}

	public void ClearWalls()
	{
		List<GameObject> list = null;
		GameObject gameObject = null;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].IsWall() || !Objects[i].CanClear())
			{
				continue;
			}
			if (gameObject != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(Objects[i]);
			}
			else
			{
				gameObject = Objects[i];
			}
		}
		if (gameObject != null)
		{
			RemoveObject(gameObject);
		}
		if (list == null)
		{
			return;
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			if (list[j] != null && !list[j].IsImportant())
			{
				RemoveObject(list[j]);
			}
		}
	}

	public Cell ClearTerrain()
	{
		List<GameObject> list = Event.NewGameObjectList(Objects);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWall() && Objects[i].CanClear())
			{
				list.Add(Objects[i]);
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			GameObject gameObject = list[j];
			if (gameObject != null && gameObject.CanClear())
			{
				RemoveObject(list[j]);
			}
		}
		return this;
	}

	/// <summary>
	/// Clear this cell's <see cref="F:XRL.World.Cell.Objects" />.
	/// </summary>
	/// <param name="Blueprint">A replacement blueprint to place in this cell after clearing.</param>
	/// <param name="Important"><c>true</c> if important objects should be cleared; otherwise, <c>false</c>.</param>
	/// <param name="Combat"><c>true</c> if combat objects should be cleared; otherwise, <c>false</c>.</param>
	/// <param name="alsoExclude">A predicate which will prevent objects from being cleared if it returns true.</param>
	/// <seealso cref="M:XRL.World.GameObject.IsImportant" />
	/// <seealso cref="M:XRL.World.GameObject.IsCombatObject" />
	public Cell Clear(string Blueprint = null, bool Important = false, bool Combat = false, Func<GameObject, bool> alsoExclude = null)
	{
		if (Objects.Count == 0)
		{
			return this;
		}
		if (Objects.Count == 1)
		{
			GameObject gameObject = Objects[0];
			if (gameObject != null && gameObject.CanClear(Important, Combat) && (alsoExclude == null || !alsoExclude(Objects[0])))
			{
				RemoveObject(Objects[0]);
			}
			return this;
		}
		List<GameObject> list = Event.NewGameObjectList(Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject2 = list[i];
			if (gameObject2 != null && gameObject2.CanClear(Important, Combat) && (alsoExclude == null || !alsoExclude(list[i])))
			{
				RemoveObject(list[i]);
			}
		}
		if (Blueprint != null)
		{
			AddObject(Blueprint);
		}
		return this;
	}

	public void DustPuff()
	{
		if (!InActiveZone)
		{
			return;
		}
		for (int i = 0; i < 15; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 4f;
			num2 = (float)Math.Cos(num3) / 4f;
			if (Stat.Random(1, 4) <= 3)
			{
				The.ParticleManager.Add("&y.", X, Y, num, num2, 15, 0f, 0f, 0L);
			}
			else
			{
				The.ParticleManager.Add("&y±", X, Y, num, num2, 15, 0f, 0f, 0L);
			}
		}
	}

	public void FugueVFX()
	{
		CombatJuice.playPrefabAnimation(Location, "Particles/FugueClone");
	}

	/// <summary>
	/// Creates an ImpactVFXFlameburst if particle VFX are enabled. Otherwise it creates text particles if screenbuffer is not provided, otherwise renders directly into the buffer.
	/// </summary>
	/// <param name="Buffer" />
	public void Flameburst(ScreenBuffer Buffer = null)
	{
		if (Options.UseParticleVFX)
		{
			CombatJuice.playPrefabAnimation(Location, "Impacts/ImpactVFXFlameburst");
			return;
		}
		if (Buffer == null)
		{
			for (int i = 0; i < 3; i++)
			{
				string text = "&C";
				int num = Stat.Random(1, 3);
				if (num == 1)
				{
					text = "&R";
				}
				if (num == 2)
				{
					text = "&r";
				}
				if (num == 3)
				{
					text = "&W";
				}
				int num2 = Stat.Random(1, 3);
				if (num2 == 1)
				{
					text += "^R";
				}
				if (num2 == 2)
				{
					text += "^r";
				}
				if (num2 == 3)
				{
					text += "^W";
				}
				XRLCore.ParticleManager.Add(text + (char)(219 + Stat.Random(0, 4)), X, Y, 0f, 0f, 10 + (6 - 2 * i));
			}
			return;
		}
		Buffer.Goto(X, Y);
		string text2 = "&C";
		int num3 = Stat.Random(1, 3);
		if (num3 == 1)
		{
			text2 = "&R";
		}
		if (num3 == 2)
		{
			text2 = "&r";
		}
		if (num3 == 3)
		{
			text2 = "&W";
		}
		int num4 = Stat.Random(1, 3);
		if (num4 == 1)
		{
			text2 += "^R";
		}
		if (num4 == 2)
		{
			text2 += "^r";
		}
		if (num4 == 3)
		{
			text2 += "^W";
		}
		if (ParentZone == The.ActiveZone)
		{
			Stat.Random(1, 3);
			Buffer.Write(text2 + (char)(219 + Stat.Random(0, 4)));
			Popup._TextConsole.DrawBuffer(Buffer);
			Thread.Sleep(10);
		}
	}

	public void Smoke(int StartAngle, int EndAngle)
	{
		if (this != null && ParentZone.IsActive())
		{
			ParticleFX.Smoke(X, Y, StartAngle, EndAngle);
		}
	}

	public void Smoke()
	{
		Smoke(85, 185);
	}

	public void PsychicPulse()
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				ParticleText("&B" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
			for (int k = 0; k < 5; k++)
			{
				ParticleText("&b" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
			for (int l = 0; l < 5; l++)
			{
				ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
		}
	}

	public void LargeFireblast()
	{
		for (int i = 2; i < 5; i++)
		{
			ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 2);
		}
		for (int j = 2; j < 5; j++)
		{
			ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 2);
		}
		for (int k = 2; k < 5; k++)
		{
			ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 2);
		}
	}

	public void SmallFireblast()
	{
		for (int i = 0; i < 3; i++)
		{
			ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
		}
		for (int j = 0; j < 3; j++)
		{
			ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
		}
		for (int k = 0; k < 3; k++)
		{
			ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
		}
	}

	public void SpatialDistortionBlip(string Color = "&C")
	{
		if (InActiveZone)
		{
			The.ParticleManager.AddRadial(Color + "ù", X, Y, Stat.Random(0, 5), Stat.Random(4, 8), 0.01f * (float)Stat.Random(3, 5), -0.05f * (float)Stat.Random(2, 6));
		}
	}

	public void TileParticleBlip(string Tile, string ColorString, string DetailColor, int Duration = 10, bool IgnoreVisibility = false, bool HFlip = false, bool VFlip = false, long DelayMS = 0L)
	{
		if (InActiveZone && (IgnoreVisibility || IsVisible()))
		{
			The.ParticleManager.AddTile(Tile, ColorString, DetailColor, X, Y, 0f, 0f, Duration, 0f, 0f, HFlip, VFlip, DelayMS);
		}
	}

	public void ParticleBlip(string Text, int Duration = 10, long DelayMS = 0L, bool IgnoreVisibility = false)
	{
		if (InActiveZone && (IgnoreVisibility || IsVisible()))
		{
			The.ParticleManager.Add(Text, X, Y, 0f, 0f, Duration, 0f, 0f, DelayMS);
		}
	}

	public void ParticleText(string Text, float Velocity, int Life)
	{
		if (InActiveZone)
		{
			float num = (float)Stat.Random(0, 359) / 58f;
			float num2 = (float)Math.Sin(num) / 4f;
			float num3 = (float)Math.Cos(num) / 4f;
			num2 *= Velocity;
			num3 *= Velocity;
			The.ParticleManager.Add(Markup.Transform(Text), X, Y, num2, num3, Life, 0f, 0f, 0L);
		}
	}

	public void ParticleText(string Text, bool IgnoreVisibility = true)
	{
		if (IgnoreVisibility || IsVisible())
		{
			ParticleText(Text, 1f, 999);
		}
	}

	public void ParticleText(string Text, int angleMin = 0, int angleMax = 359, bool IgnoreVisibility = true)
	{
		if (InActiveZone && (IgnoreVisibility || IsVisible()))
		{
			float num = (float)Stat.Random(0, 359) / 58f;
			float xDel = (float)Math.Sin(num) / 4f;
			float yDel = (float)Math.Cos(num) / 4f;
			The.ParticleManager.Add(Text, X, Y, xDel, yDel, 999, 0f, 0f, 0L);
		}
	}

	public void ParticleText(string Text, float xVel, float yVel, char Color = ' ', bool IgnoreVisibility = true)
	{
		if (InActiveZone && (IgnoreVisibility || IsVisible()))
		{
			if (Color != ' ')
			{
				Text = "{{" + Color + "|" + Text + "}}";
			}
			The.ParticleManager.Add(Text, X, Y, xVel, yVel, 999, 0f, 0f, 0L);
		}
	}

	public void ParticleText(string Text, char Color, bool IgnoreVisibility = true, float juiceDuration = 1.5f, float floatLength = -8f, GameObject emitting = null)
	{
		if (!IgnoreVisibility && !IsVisible())
		{
			return;
		}
		if (juiceEnabled)
		{
			if (Color == ' ')
			{
				Text = Markup.Transform(Text);
				Color = ConsoleLib.Console.ColorUtility.ParseForegroundColor(Text);
				Text = ConsoleLib.Console.ColorUtility.StripFormatting(Text);
			}
			CombatJuice.floatingText(this, Text, ConsoleLib.Console.ColorUtility.ColorMap[Color], juiceDuration, floatLength, 1f, ignoreVisibility: true, emitting);
			return;
		}
		float num = (float)Stat.Random(0, 359) / 58f;
		float xDel = (float)Math.Sin(num) / 4f;
		float yDel = (float)Math.Cos(num) / 4f;
		if (Color != ' ')
		{
			Text = "{{" + Color + "|" + Text + "}}";
		}
		The.ParticleManager.Add(Text, X, Y, xDel, yDel, 999, 0f, 0f, 0L);
	}

	public void ParticleText(string Text)
	{
		if (InActiveZone)
		{
			float num = (float)Stat.Random(0, 359) / 58f;
			float xDel = (float)Math.Sin(num) / 4f;
			float yDel = (float)Math.Cos(num) / 4f;
			The.ParticleManager.Add(Text, X, Y, xDel, yDel, 999, 0f, 0f, 0L);
		}
	}

	public GameObject GetCombatObject()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsCombatObject())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetCombatTarget(GameObject Attacker = null, bool IgnoreFlight = false, bool IgnoreAttackable = false, bool IgnorePhase = false, int Phase = 0, GameObject Projectile = null, GameObject Launcher = null, GameObject CheckPhaseAgainst = null, GameObject Skip = null, List<GameObject> SkipList = null, bool AllowInanimate = true, bool InanimateSolidOnly = false, Predicate<GameObject> Filter = null)
	{
		GameObject gameObject = null;
		if (IgnorePhase)
		{
			Phase = 5;
		}
		else if (Phase == 0)
		{
			if (CheckPhaseAgainst == null)
			{
				CheckPhaseAgainst = Projectile ?? Attacker;
			}
			Phase = CheckPhaseAgainst?.GetPhase() ?? 5;
		}
		bool checkFlight = !IgnoreFlight;
		bool checkAttackable = !IgnoreAttackable;
		bool flag = false;
		GameObject solidityPOV = Projectile ?? Attacker;
		using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
		GetObjectsWithPart("Combat", scopeDisposedList, Phase, Attacker, Attacker, solidityPOV, Projectile, Skip, SkipList, checkFlight, checkAttackable, flag, Filter);
		if (scopeDisposedList.Count == 1)
		{
			gameObject = scopeDisposedList[0];
		}
		else if (scopeDisposedList.Count > 0)
		{
			scopeDisposedList.Sort(new CombatSorter(Attacker));
			gameObject = scopeDisposedList.FirstOrDefault();
		}
		if ((gameObject == null) & AllowInanimate)
		{
			if (!flag)
			{
				if (InanimateSolidOnly)
				{
					flag = true;
				}
				else if (IsSolidForProjectile(Projectile, Attacker, Launcher, Attacker?.Target))
				{
					flag = true;
				}
			}
			GetRealNonSceneryObjects(scopeDisposedList, Phase, Attacker, Attacker, solidityPOV, Projectile, Skip, SkipList, checkFlight, checkAttackable, flag, Filter);
			if (scopeDisposedList.Count == 1)
			{
				gameObject = scopeDisposedList[0];
			}
			else if (scopeDisposedList.Count > 1)
			{
				scopeDisposedList.Sort(new CombatSorter(Attacker));
				gameObject = scopeDisposedList.FirstOrDefault();
			}
		}
		return gameObject;
	}

	public int GetTotalHostileDifficultyLevel(GameObject who)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsHostileTowards(who))
			{
				num += Objects[i].Con(who).GetValueOrDefault();
			}
		}
		return num;
	}

	public int GetTotalAdjacentHostileDifficultyLevel(GameObject who)
	{
		int num = 0;
		foreach (Cell adjacentCell in GetAdjacentCells())
		{
			num += adjacentCell.GetTotalHostileDifficultyLevel(who);
		}
		return num;
	}

	public GameObject GetOpenLiquidVolume()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsOpenLiquidVolume())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetDangerousOpenLiquidVolume()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsDangerousOpenLiquidVolume())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetWadingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWadingDepthLiquid())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject GetSwimmingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSwimmingDepthLiquid())
			{
				return Objects[i];
			}
		}
		return null;
	}

	public bool HasSwimmingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSwimmingDepthLiquid())
			{
				return true;
			}
		}
		return false;
	}

	public bool MightBlockPaths()
	{
		if (X <= 0 || X >= ParentZone.Width - 1)
		{
			return false;
		}
		if (Y <= 0 || Y >= ParentZone.Height - 1)
		{
			return false;
		}
		if (GetCellFromDirection("N").HasWall() && GetCellFromDirection("S").HasWall())
		{
			return true;
		}
		if (GetCellFromDirection("E").HasWall() && GetCellFromDirection("W").HasWall())
		{
			return true;
		}
		return false;
	}

	public bool HasOpenLiquidVolume()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsOpenLiquidVolume())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWadingDepthLiquid()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWadingDepthLiquid())
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetAquaticSupportFor(GameObject who)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsSwimmableFor(who))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public bool HasAquaticSupportFor(GameObject who)
	{
		bool flag = false;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsBridge)
			{
				return false;
			}
			if (!flag && Objects[i].IsSwimmableFor(who))
			{
				flag = true;
			}
		}
		return flag;
	}

	public bool HasWalkableWallFor(GameObject who)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsWalkableWall(who))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHealingPool()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsHealingPool())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTryingToJoinPartyLeader()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsTryingToJoinPartyLeader())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTryingToJoinPartyLeaderForZoneUncaching()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsTryingToJoinPartyLeaderForZoneUncaching())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPlayerLed()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsPlayerLed())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWasPlayer()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WasPlayer())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLeftBehindByPlayer()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].LeftBehindByPlayer())
			{
				return true;
			}
		}
		return false;
	}

	public int GetHealingLocationValue(GameObject Actor)
	{
		return PollForHealingLocationEvent.GetFor(Actor, this);
	}

	public bool IsHealingLocation(GameObject Actor)
	{
		return PollForHealingLocationEvent.GetFor(Actor, this, First: true) > 0;
	}

	public void UseHealingLocation(GameObject Actor)
	{
		UseHealingLocationEvent.Send(Actor, this);
	}

	public GameObject FindObjectByID(string id)
	{
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = FindObjectByIdEvent.Find(Objects[num], id);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public GameObject FindObjectByID(int ID)
	{
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = FindObjectByIdEvent.Find(Objects[num], ID);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public bool OnWorldMap()
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.IsWorldMap();
	}

	public bool WantEvent(int ID, int Cascade)
	{
		return Objects.WantEvent(ID, Cascade);
	}

	public bool HandleEvent(MinEvent E)
	{
		return Objects.HandleEvent(E);
	}

	public bool HandleEvent(MinEvent E, IEvent ParentEvent)
	{
		bool result = HandleEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public List<GameObject> GetObjectsThatWantEvent(int ID, int cascade)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantEvent(ID, cascade))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public int TemperatureChange(int Amount, GameObject Actor = null, bool Radiant = false, bool MinAmbient = false, bool MaxAmbient = false, bool IgnoreResistance = false, int Phase = 0)
	{
		if (Phase == 0 && Actor != null)
		{
			Phase = Actor.GetPhase();
		}
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = Objects[i];
			if (gameObject.TemperatureChange(Amount, Actor, Radiant, MinAmbient, MaxAmbient, IgnoreResistance, Phase))
			{
				num++;
			}
			if (count != Objects.Count)
			{
				count = Objects.Count;
				if (i < count && Objects[i] != gameObject)
				{
					i--;
				}
			}
		}
		return num;
	}

	public void Splash(string Particle)
	{
		if (InActiveZone)
		{
			for (int i = 0; i < 3; i++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = (float)Stat.RandomCosmetic(0, 359) / 58f;
				num = (float)Math.Sin(num3) / 3f;
				num2 = (float)Math.Cos(num3) / 3f;
				The.ParticleManager.Add(Particle, X, Y, num, num2, 5, 0f, 0f, 0L);
			}
		}
	}

	public void LiquidSplash(string Color)
	{
		if (!InActiveZone)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 3f;
			num2 = (float)Math.Cos(num3) / 3f;
			char c = '.';
			switch (Stat.Random(0, 5))
			{
			case 1:
				c = 'ú';
				break;
			case 2:
				c = 'ø';
				break;
			case 3:
				c = '~';
				break;
			case 4:
				c = 'ù';
				break;
			case 5:
				c = '\a';
				break;
			}
			The.ParticleManager.Add("&" + Color + c, X, Y, num, num2, 5, 0f, 0f, 0L);
			Thread.Sleep(Stat.Random(5, 15));
		}
	}

	public void LiquidSplash(List<string> Colors)
	{
		if (!ParentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 3f;
			num2 = (float)Math.Cos(num3) / 3f;
			char c = '.';
			switch (Stat.Random(0, 5))
			{
			case 1:
				c = 'ú';
				break;
			case 2:
				c = 'ø';
				break;
			case 3:
				c = '~';
				break;
			case 4:
				c = 'ù';
				break;
			case 5:
				c = '\a';
				break;
			}
			The.ParticleManager.Add("&" + Colors.GetRandomElement() + c, X, Y, num, num2, 5, 0f, 0f, Stat.Random(5, 15) * i);
		}
	}

	public void LiquidSplash(BaseLiquid Liquid)
	{
		if (Liquid != null)
		{
			LiquidSplash(Liquid.GetColors());
		}
	}

	public void TelekinesisBlip()
	{
		if (InActiveZone)
		{
			int i = 0;
			for (int num = Stat.Random(2, 4); i < num; i++)
			{
				int num2 = Stat.Random(0, 359);
				float num3 = (float)Stat.RandomCosmetic(4, 14) / 5f;
				The.ParticleManager.Add("@", X, Y, (float)Math.Sin((double)num2 * 0.017) / num3, (float)Math.Cos((double)num2 * 0.017) / num3, Stat.Random(3, 20));
			}
		}
	}

	public void DilationSplat()
	{
		if (InActiveZone)
		{
			for (int i = 0; i < 360; i++)
			{
				float num = (float)Stat.RandomCosmetic(4, 14) / 3f;
				The.ParticleManager.Add("@", X, Y, (float)Math.Sin((double)i * 0.017) / num, (float)Math.Cos((double)i * 0.017) / num);
			}
		}
	}

	public void ImplosionSplat(int Radius = 12)
	{
		if (InActiveZone)
		{
			for (int i = 0; i < 360; i++)
			{
				float num = (float)Stat.RandomCosmetic(1, 5) / ((float)Radius / 4f);
				The.ParticleManager.Add("@", (int)Math.Round((double)X + (double)Radius * Math.Sin((double)i * 0.017)), (int)Math.Round((double)Y + (double)Radius * Math.Cos((double)i * 0.017)), (float)(0.0 - Math.Sin((double)i * 0.017)) / num, (float)(0.0 - Math.Cos((double)i * 0.017)) / num, (int)Math.Round((float)Radius * num));
			}
		}
	}

	public bool HasBridge()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].IsBridge)
			{
				return true;
			}
		}
		return false;
	}

	public List<GameObject> GetSolidObjects()
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolid())
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetSolidObjectsFor(GameObject Actor)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].ConsiderSolidFor(Actor))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetCanInteractInCellWithSolidObjectsFor(GameObject Actor)
	{
		List<GameObject> list = Event.NewGameObjectList();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].CanInteractInCellWithSolid(Actor))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public bool IsAudible(GameObject By, int Volume = 20)
	{
		if (ParentZone == null || ParentZone.IsWorldMap())
		{
			return false;
		}
		return ParentZone.FastFloodAudibilityAny(X, Y, Volume, (GameObject GO) => GO == By, By);
	}

	public bool IsSmellable(GameObject By, int Intensity = 5)
	{
		if (ParentZone == null || ParentZone.IsWorldMap())
		{
			return false;
		}
		return ParentZone.FastFloodOlfactionAny(X, Y, Intensity, (GameObject GO) => GO == By, By);
	}

	public Cell GetReversibleAccessUpCell()
	{
		if (HasObjectWithPart("StairsUp"))
		{
			Cell cellFromDirectionGlobalIfBuilt = GetCellFromDirectionGlobalIfBuilt("U");
			if (cellFromDirectionGlobalIfBuilt != null && cellFromDirectionGlobalIfBuilt.HasObjectWithPart("StairsDown"))
			{
				return cellFromDirectionGlobalIfBuilt;
			}
		}
		return null;
	}

	public Cell GetReversibleAccessDownCell()
	{
		if (HasObjectWithPart("StairsDown"))
		{
			Cell cellFromDirectionGlobalIfBuilt = GetCellFromDirectionGlobalIfBuilt("D");
			if (cellFromDirectionGlobalIfBuilt != null && cellFromDirectionGlobalIfBuilt.HasObjectWithPart("StairsUp"))
			{
				return cellFromDirectionGlobalIfBuilt;
			}
		}
		return null;
	}

	public bool BlocksRadar()
	{
		return BlocksRadarEvent.Check(this);
	}

	public bool IsSameOrAdjacent(Cell C, bool BuiltOnly = true)
	{
		if (C != this)
		{
			return IsAdjacentTo(C, BuiltOnly);
		}
		return true;
	}

	public bool Is(GlobalLocation loc)
	{
		if (loc.ZoneID != ParentZone?.ZoneID)
		{
			return false;
		}
		if (loc.CellX != X)
		{
			return false;
		}
		if (loc.CellY != Y)
		{
			return false;
		}
		return true;
	}

	public bool FastFloodVisibilityAny(int Radius, string SearchPart, GameObject Looker)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodVisibilityAny(X, Y, Radius, SearchPart, Looker);
	}

	public bool FastFloodVisibilityAny(int Radius, Predicate<GameObject> Filter, GameObject Looker)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodVisibilityAny(X, Y, Radius, Filter, Looker);
	}

	public bool FastFloodAudibilityAny(int Radius, Predicate<GameObject> Filter, GameObject Hearer)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodAudibilityAny(X, Y, Radius, Filter, Hearer);
	}

	public bool FastFloodOlfactionAny(int Radius, Predicate<GameObject> Filter, GameObject Smeller)
	{
		if (ParentZone == null)
		{
			return false;
		}
		return ParentZone.FastFloodOlfactionAny(X, Y, Radius, Filter, Smeller);
	}

	public List<Point> LineFromAngle(int degrees)
	{
		if (ParentZone == null)
		{
			return new List<Point>();
		}
		return ParentZone.LineFromAngle(X, Y, degrees);
	}

	public void Indicate(bool AsThreat = false)
	{
		if (!InActiveZone)
		{
			return;
		}
		if (juiceEnabled)
		{
			if (Options.UseTextAutoactInterruptionIndicator)
			{
				ParticleText("v", AsThreat ? 'R' : 'W');
			}
			else
			{
				CombatJuice.playPrefabAnimation(Location, AsThreat ? "Particles/AutostopNotificationThreat" : "Particles/AutostopNotificationNonThreat");
			}
		}
		else
		{
			ParticleBlip(AsThreat ? "&RX" : "&WX", 10, 0L, IgnoreVisibility: true);
		}
	}

	public bool IsSolidGround()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].TryGetPart<StairsDown>(out var Part) && Part.PullDown)
			{
				return false;
			}
		}
		return true;
	}

	public string getTile()
	{
		return PaintTile;
	}

	public string getRenderString()
	{
		return PaintRenderString;
	}

	public string getColorString()
	{
		return PaintColorString;
	}

	public string getTileColor()
	{
		return PaintTileColor;
	}

	public char getDetailColor()
	{
		if (!PaintDetailColor.IsNullOrEmpty())
		{
			return PaintDetailColor[0];
		}
		return '\0';
	}

	public ColorChars getColorChars()
	{
		char foreground = 'y';
		char background = 'k';
		string text;
		if (Globals.RenderMode == RenderModeType.Tiles)
		{
			text = getTileColor();
			if (text.IsNullOrEmpty())
			{
				text = getColorString();
			}
		}
		else
		{
			text = getColorString();
		}
		if (!string.IsNullOrEmpty(text))
		{
			int num = text.LastIndexOf(ColorChars.FOREGROUND_INDICATOR);
			int num2 = text.LastIndexOf(ColorChars.BACKGROUND_INDICATOR);
			if (num >= 0 && num < text.Length - 1)
			{
				foreground = text[num + 1];
			}
			if (num2 >= 0 && num2 < text.Length - 1)
			{
				background = text[num2 + 1];
			}
		}
		return new ColorChars
		{
			detail = getDetailColor(),
			foreground = foreground,
			background = background
		};
	}

	public bool getHFlip()
	{
		return false;
	}

	public bool getVFlip()
	{
		return false;
	}

	public bool IsBlackedOut()
	{
		return GetLight() == LightLevel.Blackout;
	}

	public bool IsAnyLocalAdjacentCellVisible()
	{
		foreach (Cell localAdjacentCell in GetLocalAdjacentCells())
		{
			if (localAdjacentCell.IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	public int GetAdjacencyCount(List<Cell> Cells)
	{
		int num = 0;
		foreach (Cell Cell in Cells)
		{
			if (Cell != this && IsAdjacentTo(Cell))
			{
				num++;
			}
		}
		return num;
	}

	public int GetTotalDistance(List<Cell> Cells)
	{
		int num = 0;
		foreach (Cell Cell in Cells)
		{
			if (Cell != this)
			{
				num += DistanceTo(Cell);
			}
		}
		return num;
	}

	public void WakeCreatures()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			Objects[i].Brain?.Wake();
		}
	}

	public void WakeCreaturesInArea()
	{
		WakeCreatures();
		List<Cell> localAdjacentCells = GetLocalAdjacentCells();
		int i = 0;
		for (int count = localAdjacentCells.Count; i < count; i++)
		{
			localAdjacentCells[i].WakeCreatures();
		}
	}

	public float GetCover()
	{
		return ParentZone?.GetCoverAt(X, Y) ?? 0f;
	}

	public SpiralEnumerator IterateAdjacent(int Radius = 1, bool IncludeSelf = false, bool LocalOnly = false, bool BuiltOnly = true)
	{
		return new SpiralEnumerator(this, Radius, IncludeSelf, LocalOnly, BuiltOnly);
	}

	[Obsolete("Use FindObjectById")]
	public GameObject findObjectById(string id)
	{
		return FindObjectByID(id);
	}

	internal void LogInvalidPhysics(GameObject Object)
	{
		Cell cell = Object.Physics?._CurrentCell;
		string text = $"Invalid physics object '{Object.DebugName}' in {ParentZone?._ZoneID}, cell [{X},{Y},{HasStairs()}], actual [{cell?.X ?? (-1)},{cell?.Y ?? (-1)},{cell?.HasStairs()}] in {cell?.ParentZone?.ZoneID}";
		string stringProperty = Object.GetStringProperty("LastInventoryActionCommand");
		if (!stringProperty.IsNullOrEmpty())
		{
			text = text + ", last action [" + stringProperty + "]";
		}
		RingDeque<Cell> ringDeque = The.Game?.Player?.PlayerCells;
		int num = ringDeque?.IndexOf(this) ?? (-1);
		if (num != -1)
		{
			Cell cell2 = ((num > 0) ? ringDeque[num - 1] : null);
			Cell cell3 = ((num < ringDeque.Count - 1) ? ringDeque[num + 1] : null);
			text += $", last [{cell2?.X ?? (-1)},{cell2?.Y ?? (-1)},{cell2?.HasStairs()}], next [{cell3?.X ?? (-1)},{cell3?.Y ?? (-1)},{cell3?.HasStairs()}]";
		}
		MetricsManager.LogError(text);
		if (Object.IsPlayer() && !AskedReport)
		{
			Popup.Show("There was a duplication glitch involving your player character. It'd be helpful to send the save folder and Player.log to support@freeholdgames.com along with what you were currently doing.");
			AskedReport = true;
		}
	}
}
