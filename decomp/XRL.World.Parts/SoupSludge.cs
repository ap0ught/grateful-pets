using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class SoupSludge : IPart
{
	public static readonly string[] Prefixes = new string[20]
	{
		"mono", "di", "tri", "tetra", "penta", "hexa", "hepta", "octa", "ennea", "deca",
		"hendeca", "dodeca", "triskaideca", "tetrakaideca", "pentakaideca", "hexakaideca", "heptakaideca", "octakaideca", "enneakaideca", "icosa"
	};

	public List<string> ComponentLiquids = new List<string>();

	public string LiquidID;

	public int MaxAdjectives = int.MaxValue;

	public const int CLR_DIV = 240;

	[NonSerialized]
	private byte Hero;

	[NonSerialized]
	private string Detail;

	[NonSerialized]
	private List<int> Components = new List<int>();

	[NonSerialized]
	private long DisplayTurn = -1L;

	public string ManagerID => ParentObject.ID + "::SoupSludge";

	public SoupSludge()
	{
	}

	public SoupSludge(string LiquidID)
	{
		this.LiquidID = LiquidID;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Initialize()
	{
		EvolveSludge();
	}

	public override void Remove()
	{
		ParentObject.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		base.Remove();
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		SoupSludge obj = base.DeepCopy(Parent) as SoupSludge;
		obj.ComponentLiquids = new List<string>(ComponentLiquids);
		return obj;
	}

	public static string GetPrefix(int Count)
	{
		if (Count <= 0)
		{
			return "";
		}
		if (Count <= Prefixes.Length)
		{
			return Prefixes[Count - 1];
		}
		return "poly";
	}

	public void CatalyzeLiquid(string ID)
	{
		if (!ComponentLiquids.Contains(ID))
		{
			Mutations mutations = ParentObject.RequirePart<Mutations>();
			LiquidSpitter liquidSpitter = mutations.GetMutation("LiquidSpitter") as LiquidSpitter;
			if (liquidSpitter == null)
			{
				liquidSpitter = new LiquidSpitter(ID);
				mutations.AddMutation(liquidSpitter);
			}
			liquidSpitter.AddLiquid(ID);
			ComponentLiquids.Add(ID);
			EquipPseudopod(ID);
			ParentObject.GetStat("Speed").BaseValue += 10;
			ParentObject.GetStat("Hitpoints").BaseValue += 20;
			ParentObject.GetStat("Level").BaseValue += 3;
		}
	}

	public void EvolveSludge()
	{
		if (LiquidID != null)
		{
			string oldName = ParentObject.t();
			CatalyzeLiquid(LiquidID);
			CatalyzeName();
			CatalyzeMessage(LiquidID, oldName);
			LiquidID = null;
		}
	}

	public void CatalyzeName()
	{
		if (ComponentLiquids.Count == 0)
		{
			return;
		}
		string text = GetPrefix(ComponentLiquids.Count) + "sludge";
		if (ParentObject.HasProperName)
		{
			string old = GetPrefix(ComponentLiquids.Count - 1) + "sludge";
			ParentObject.DisplayName = ParentObject.DisplayNameOnlyDirect.Replace(old, text, StringComparison.OrdinalIgnoreCase, RespectTitleCase: true);
			if (ParentObject.TryGetPart<SocialRoles>(out var Part))
			{
				for (int i = 0; i < Part.RoleList.Count; i++)
				{
					Part.RoleList[i] = Part.RoleList[i].Replace(old, text, StringComparison.OrdinalIgnoreCase, RespectTitleCase: true);
				}
				Part.UpdateRoles();
			}
		}
		else
		{
			ParentObject.DisplayName = text;
		}
	}

	public void CatalyzeMessage(string ID, string OldName)
	{
		if (ComponentLiquids.Count >= 5)
		{
			JournalAPI.AddAccomplishment("You witnessed the rare formation of " + Grammar.A(GetPrefix(ComponentLiquids.Count) + "sludge") + ".", "=name= was blessed to witness the rare formation of " + Grammar.A(GetPrefix(ComponentLiquids.Count) + "sludge") + ".", "Deep in " + JournalAPI.GetLandmarkNearestPlayer().Text + ", =name= stumbled on a soup sludge performing a secret ritual. Because of " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " <spice.elements." + The.Player.GetMythicDomain() + ".quality.!random>, the sludge taught " + The.Player.BaseDisplayNameStripped + " the secret to becoming " + Grammar.A(GetPrefix(ComponentLiquids.Count)) + ".", null, "general", MuralCategory.HasInspiringExperience, MuralWeight.Medium, null, -1L);
		}
		if (ParentObject.HasProperName)
		{
			EmitMessage("The " + LiquidVolume.GetLiquid(ID).GetName() + " catalyzes " + OldName + " into " + Grammar.A(GetPrefix(ComponentLiquids.Count) + "sludge") + ".");
		}
		else
		{
			EmitMessage("The " + LiquidVolume.GetLiquid(ID).GetName() + " catalyzes " + OldName + " into " + ParentObject.an() + ".");
		}
	}

	public BodyPart GetEmptyHand()
	{
		foreach (BodyPart item in ParentObject.Body.GetPart("Hand"))
		{
			if (!item.Extrinsic && item.Equipped == null)
			{
				return item;
			}
		}
		return null;
	}

	public BodyPart RequireEmptyHand()
	{
		BodyPart bodyPart = GetEmptyHand();
		if (bodyPart == null)
		{
			BodyPart body = ParentObject.Body.GetBody();
			bodyPart = body.AddPartAt("Pseudopod", 2, null, null, null, null, ManagerID, null, null, null, null, null, null, null, null, null, null, null, null, null, "Hand", "Missile Weapon");
			body.AddPartAt(bodyPart, "Pseudopod", 1, null, null, null, null, ManagerID);
		}
		return bodyPart;
	}

	public void EquipPseudopod(string ID)
	{
		GameObject gameObject = null;
		switch (ID)
		{
		case "water":
			gameObject = GameObject.Create("Watery Pseudopod");
			break;
		case "salt":
			gameObject = GameObject.Create("Salty Pseudopod");
			break;
		case "asphalt":
			gameObject = GameObject.Create("Tarry Pseudopod");
			break;
		case "lava":
			gameObject = GameObject.Create("Magmatic Pseudopod");
			ParentObject.AddPart(new LavaSludge());
			ParentObject.GetStat("HeatResistance").BaseValue += 100;
			ParentObject.Physics.Temperature = 1000;
			break;
		case "slime":
			gameObject = GameObject.Create("Slimy Pseudopod");
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "oil":
			gameObject = GameObject.Create("Oily Pseudopod");
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "blood":
			gameObject = GameObject.Create("Bloody Pseudopod");
			break;
		case "acid":
			gameObject = GameObject.Create("Acidic Pseudopod");
			ParentObject.GetStat("AcidResistance").BaseValue += 100;
			break;
		case "honey":
			gameObject = GameObject.Create("Honeyed Pseudopod");
			break;
		case "wine":
			gameObject = GameObject.Create("Lush Pseudopod");
			break;
		case "sludge":
			gameObject = GameObject.Create("Sludgy Pseudopod2");
			break;
		case "goo":
			gameObject = GameObject.Create("Gooey Pseudopod2");
			break;
		case "putrid":
			gameObject = GameObject.Create("Putrid Pseudopod");
			break;
		case "gel":
			gameObject = GameObject.Create("Unctuous Pseudopod");
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "ooze":
			gameObject = GameObject.Create("Oozing Pseudopod2");
			break;
		case "cider":
			gameObject = GameObject.Create("Spiced Pseudopod");
			break;
		case "convalessence":
			gameObject = GameObject.Create("Luminous Pseudopod2");
			ParentObject.GetStat("ColdResistance").BaseValue += 100;
			ParentObject.RequirePart<LightSource>().Radius = 6;
			break;
		case "neutronflux":
			gameObject = GameObject.Create("Neutronic Pseudopod");
			break;
		case "cloning":
			gameObject = GameObject.Create("Homogenized Pseudopod");
			ParentObject.AddPart(new CloneOnHit());
			break;
		case "wax":
			gameObject = GameObject.Create("Waxen Pseudopod");
			break;
		case "ink":
			gameObject = GameObject.Create("Inky Pseudopod");
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "sap":
			gameObject = GameObject.Create("Sugary Pseudopod");
			break;
		case "brainbrine":
			gameObject = GameObject.Create("Nervous Pseudopod");
			break;
		case "algae":
			gameObject = GameObject.Create("Algal Pseudopod");
			break;
		case "sunslag":
			gameObject = GameObject.Create("Radiant Pseudopod");
			ParentObject.GetStat("Speed").BaseValue += 20;
			ParentObject.RequirePart<LightSource>().Radius = 6;
			break;
		case "warmstatic":
			gameObject = GameObject.Create("Entropic Pseudopod");
			break;
		}
		if (gameObject != null && !RequireEmptyHand().Equip(gameObject, 0, Silent: false, ForDeepCopy: false, Forced: false, SemiForced: true))
		{
			gameObject.Destroy();
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (ComponentLiquids.Count > 0)
		{
			long elapsedMilliseconds = XRLCore.FrameTimer.ElapsedMilliseconds;
			if (Hero == 1)
			{
				if (elapsedMilliseconds % 480 > 240)
				{
					return true;
				}
				if (Detail == null)
				{
					Detail = ColorUtility.FindLastForeground(E.ColorString)?.ToString() ?? "M";
				}
				int num = (int)(elapsedMilliseconds % (480 * ComponentLiquids.Count));
				E.ColorString = E.ColorString + "&" + LiquidVolume.GetLiquid(ComponentLiquids[num / 480]).GetColor();
				E.DetailColor = Detail;
			}
			else if (Hero == 0)
			{
				if (ParentObject.HasIntProperty("Hero") || ParentObject.HasPart<GivesRep>())
				{
					Hero = 1;
				}
				else
				{
					Hero = 2;
				}
			}
			else
			{
				int num2 = (int)(elapsedMilliseconds % (240 * ComponentLiquids.Count));
				E.ColorString = E.ColorString + "&" + LiquidVolume.GetLiquid(ComponentLiquids[num2 / 240]).GetColor();
			}
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != EnteredCellEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Object.HasProperName)
		{
			return true;
		}
		if (ComponentLiquids.Count > MaxAdjectives)
		{
			long num = The.Game?.Turns ?? 0;
			if (num != DisplayTurn)
			{
				DisplayTurn = num;
				Components.Clear();
				Random random = new Random(DisplayTurn.GetHashCode());
				int num2 = Math.Min(MaxAdjectives, ComponentLiquids.Count);
				while (Components.Count < num2)
				{
					int item = random.Next(0, ComponentLiquids.Count);
					if (!Components.Contains(item))
					{
						Components.Add(item);
					}
				}
			}
			foreach (int component in Components)
			{
				BaseLiquid liquid = LiquidVolume.GetLiquid(ComponentLiquids[component]);
				E.AddAdjective(liquid.GetAdjective(null));
			}
		}
		else
		{
			foreach (string componentLiquid in ComponentLiquids)
			{
				E.AddAdjective(LiquidVolume.GetLiquid(componentLiquid).GetAdjective(null));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (LiquidID != null)
		{
			return true;
		}
		GameObject openLiquidVolume = E.Cell.GetOpenLiquidVolume();
		if (openLiquidVolume == null)
		{
			return true;
		}
		LiquidVolume liquidVolume = openLiquidVolume.LiquidVolume;
		if (!ReactWith(liquidVolume.GetPrimaryLiquidID(), liquidVolume))
		{
			ReactWith(liquidVolume.GetSecondaryLiquidID(), liquidVolume);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (E.Killer != null && E.Killer.IsPlayer())
		{
			switch (ComponentLiquids.Count)
			{
			case 3:
				Achievement.KILL_TRISLUDGE.Unlock();
				break;
			case 5:
				Achievement.KILL_PENTASLUDGE.Unlock();
				break;
			case 10:
				Achievement.KILL_DECASLUDGE.Unlock();
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (LiquidID != null)
		{
			EvolveSludge();
		}
		return base.HandleEvent(E);
	}

	public bool ReactWith(string ID, LiquidVolume LV)
	{
		if (ID == null || ComponentLiquids.Contains(ID))
		{
			return false;
		}
		if (ID == "cloning" || ID == "proteangunk")
		{
			return false;
		}
		DidX("start", "reacting with the " + LiquidVolume.GetLiquid(ID).GetName(LV));
		LV.UseDrams(500);
		LiquidID = ID;
		return true;
	}
}
