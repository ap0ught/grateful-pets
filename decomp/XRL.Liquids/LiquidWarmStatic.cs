using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
[HasModSensitiveStaticCache]
[HasWishCommand]
public class LiquidWarmStatic : BaseLiquid
{
	public new const string ID = "warmstatic";

	public const int NUM_FRAMES = 4;

	public const string MSG_GLITCH_OBJECT = "=subject.T= =verb:glitch= into =object.an=.";

	[NonSerialized]
	public static List<string> Colors = new List<string>(3) { "Y", "y", "K" };

	[NonSerialized]
	public static Dictionary<int, List<string>> MaskAnimationPureTiles = new Dictionary<int, List<string>>();

	[NonSerialized]
	public static Dictionary<int, List<string>> MaskAnimationImpureTiles = new Dictionary<int, List<string>>();

	public int PurePaintGroup = -1;

	[ModSensitiveStaticCache(false)]
	public static List<Type> _RandomEffects;

	public static List<Type> RandomEffects
	{
		get
		{
			if (_RandomEffects == null)
			{
				_RandomEffects = ModManager.GetTypesAssignableFrom(typeof(ITierInitialized), Cache: false);
				_RandomEffects.RemoveAll((Type x) => !x.IsSubclassOf(typeof(Effect)) || x.IsAbstract);
				for (int num = _RandomEffects.Count - 1; num >= 0; num--)
				{
					RequiresMod customAttribute = _RandomEffects[num].GetCustomAttribute<RequiresMod>();
					if (customAttribute != null && (customAttribute.ID.IsNullOrEmpty() || ModManager.GetMod(customAttribute.ID) == null) && (customAttribute.WorkshopID == 0 || ModManager.GetMod(customAttribute.WorkshopID) == null))
					{
						_RandomEffects.RemoveAt(num);
					}
				}
			}
			return _RandomEffects;
		}
	}

	public LiquidWarmStatic()
		: base("warmstatic")
	{
		VaporTemperature = 2000;
		VaporObject = "GlitterGas";
		Combustibility = 30;
		PureElectricalConductivity = 100;
		MixedElectricalConductivity = 100;
		ThermalConductivity = 40;
		Fluidity = 10;
		Evaporativity = 1;
		Staining = 3;
		InterruptAutowalk = true;
		ConversionProduct = "acid::1;;algae::1;;blood::1;;brainbrine::1;;cider::1;;cloning::1;;convalessence::1;;gel::1;;goo::1;;honey::1;;ink::1;;lava::1;;neutronflux::1;;oil::1;;ooze::1;;proteangunk::1;;putrid::1;;salt::5;;sap::1;;slime::1;;sludge::1;;sunslag::1;;asphalt::1;;water::1;;wax::1;;wine::1;;";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "Y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{Y|warm static}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|entropic}}";
	}

	public override string GetWaterRitualName()
	{
		return "warm static";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{Y|entropic}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{Y|entropic}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{Y|warm static}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.IsPlayer())
		{
			GameManager.Instance.StaticEffecting = 2;
			SoundManager.PlayUISound("sfx_warmStaticSizzle");
		}
		if (Liquid.IsPure())
		{
			if (Target.IsMutant() && 50.in100())
			{
				GlitchMutations(Target);
				int num = Stat.Random(4, 12);
				Target.GetStat("MP").BaseValue += num;
				Target.ShowSuccess("You gained {{C|" + num + "}} mutation points!");
			}
			else
			{
				GlitchSkills(Target);
				int num2 = Stat.Random(400, 1200);
				Target.GetStat("SP").BaseValue += num2;
				Target.ShowSuccess("You gained {{C|" + num2 + "}} skill points!");
			}
		}
		else
		{
			ApplyRandomEffectTo(Target, 0, EmitMessage: true, FromDialog: true);
		}
		return true;
	}

	public override bool Douse(LiquidVolume Liquid, GameObject Actor, ref int PourAmount, ref bool RequestInterfaceExit)
	{
		return RequestInterfaceExit = true;
	}

	private static bool TryGetValidEntry(BaseSkill Part, out SkillEntry Skill, out PowerEntry Power)
	{
		if (SkillFactory.Factory.SkillByClass.TryGetValue(Part.Name, out Skill))
		{
			Power = null;
			return Skill.Cost > 0;
		}
		if (SkillFactory.Factory.PowersByClass.TryGetValue(Part.Name, out Power))
		{
			if (Power.ParentSkill != null)
			{
				return Power.Cost > 0;
			}
			return false;
		}
		return false;
	}

	private static bool GlitchSkills(GameObject Object)
	{
		Skills part = Object.GetPart<Skills>();
		if (part == null)
		{
			return false;
		}
		List<BaseSkill> partsDescendedFrom = Object.GetPartsDescendedFrom<BaseSkill>();
		if (partsDescendedFrom.IsNullOrEmpty())
		{
			return false;
		}
		bool flag = Object.IsPlayerControlled();
		StringBuilder stringBuilder = new StringBuilder("{{W|").Append(Object.Poss("mind starts to fluctuate in and out of coherence.")).Append("}}");
		Object.EmitMessage(stringBuilder.ToString(), null, null, flag);
		stringBuilder.Clear();
		List<int> list = partsDescendedFrom.Select((BaseSkill x) => 2).ToList();
		int num = 0;
		for (int count = partsDescendedFrom.Count; num < count; num++)
		{
			BaseSkill baseSkill = partsDescendedFrom[num];
			if (!TryGetValidEntry(baseSkill, out var Skill, out var Power))
			{
				list[num] = 0;
				continue;
			}
			for (int num2 = 100; num2 > 0; num2--)
			{
				string name = baseSkill.Name;
				if (Skill != null)
				{
					SkillEntry randomElement = SkillFactory.GetSkills().GetRandomElement();
					if (randomElement.Cost <= 0 || randomElement.ExcludeFromPool)
					{
						continue;
					}
					name = randomElement.Class;
				}
				else if (Power != null)
				{
					PowerEntry randomElement2 = SkillFactory.GetPowers().GetRandomElement();
					if (randomElement2.Cost <= 0 || randomElement2.ExcludeFromPool)
					{
						continue;
					}
					name = randomElement2.Class;
				}
				int num3 = partsDescendedFrom.FindIndex((BaseSkill x) => x.Name == name);
				if (num3 == -1)
				{
					num3 = partsDescendedFrom.Count;
					partsDescendedFrom.Add(ModManager.CreateInstance<BaseSkill>("XRL.World.Parts.Skill." + name));
					list.Add(1);
				}
				else
				{
					if (list[num3] != 2)
					{
						continue;
					}
					list[num3] = 0;
				}
				if (flag && num != num3)
				{
					stringBuilder.Compound(Object.Poss("knowledge of {{rules|"), '\n').Append(partsDescendedFrom[num].DisplayName).Append("}} distorts into knowledge of {{rules|")
						.Append(partsDescendedFrom[num3].DisplayName)
						.Append("}}.");
				}
				break;
			}
		}
		if (flag)
		{
			Object.EmitMessage(stringBuilder.ToString(), null, null, UsePopup: true);
			if (Object.IsPlayer())
			{
				JournalAPI.AddAccomplishment("You drank the raw entropy of the universe and your mind fluctuated in and out of coherence.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
			}
		}
		int num4 = 0;
		for (int count2 = partsDescendedFrom.Count; num4 < count2; num4++)
		{
			if (list[num4] == 1)
			{
				part.AddSkill(partsDescendedFrom[num4]);
			}
			else if (list[num4] == 2)
			{
				part.RemoveSkill(partsDescendedFrom[num4]);
				Object.RemovePart(partsDescendedFrom[num4]);
			}
		}
		return true;
	}

	private static bool GlitchMutations(GameObject Object)
	{
		Mutations part = Object.GetPart<Mutations>();
		if (part == null)
		{
			return false;
		}
		List<BaseMutation> list = new List<BaseMutation>(part.MutationList);
		list.RemoveAll((BaseMutation x) => x.BaseLevel <= 0);
		if (list.IsNullOrEmpty())
		{
			return false;
		}
		List<MutationEntry> list2 = new List<MutationEntry>();
		List<MutationEntry> list3 = new List<MutationEntry>(list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			if (!MutationFactory.TryGetMutationEntry(list[num], out var Entry))
			{
				list.RemoveAt(num--);
			}
			else if (Entry.Category == MutationFactory.Morphotypes)
			{
				list.RemoveAt(num--);
				list2.Add(Entry);
			}
			else
			{
				list3.Add(Entry);
			}
		}
		bool flag = Object.IsPlayerControlled();
		StringBuilder stringBuilder = new StringBuilder("{{W|").Append(Object.Poss("genome fluctuates and genes start turning on and off at random.")).Append("}}");
		Object.EmitMessage(stringBuilder.ToString(), null, null, flag);
		stringBuilder.Clear();
		List<MutationEntry> list4 = new List<MutationEntry>();
		List<BaseMutation> list5 = new List<BaseMutation>(list);
		List<int> list6 = list.Select((BaseMutation x) => x.BaseLevel).ToList();
		List<int> rapids = list.Select((BaseMutation x) => x.GetRapidLevelAmount()).ToList();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			BaseMutation baseMutation = list[i];
			MutationEntry entry = list3[i];
			bool defect = entry.IsDefect();
			bool single = entry.Cost <= 1;
			list4.Clear();
			list4.AddRange(from x in (from x in MutationFactory.GetCategories()
					where (rapids[i] <= 0) ? (x != MutationFactory.Morphotypes) : (x == entry.Category)
					select x).SelectMany((MutationCategory x) => x.Entries)
				where !x.ExcludeFromPool && defect == x.IsDefect() && single == x.Cost <= 1
				select x);
			for (int num2 = 100; num2 > 0; num2--)
			{
				MutationEntry mutation = list4.GetRandomElement();
				if (!list2.Contains(mutation) && !list2.Exists((MutationEntry x) => !x.OkWith(mutation, CheckOther: true, allowMultipleDefects: true)))
				{
					int num3 = list.FindIndex((BaseMutation x) => x.Name == mutation.Class);
					BaseMutation baseMutation2 = ((num3 < 0) ? mutation.CreateInstance() : list[num3]);
					if (baseMutation2.CanLevel() == baseMutation.CanLevel())
					{
						list5[i] = baseMutation2;
						list6[i] = list[i].BaseLevel;
						rapids[i] = list[i].GetRapidLevelAmount();
						list2.Add(mutation);
						if (flag && list[i] != list5[i])
						{
							stringBuilder.Compound(Object.Poss("mutation {{rules|"), '\n').Append(list[i].GetDisplayName()).Append("}} transmutes into the mutation {{rules|")
								.Append(list5[i].GetDisplayName())
								.Append("}}.");
						}
						break;
					}
				}
			}
		}
		if (flag)
		{
			Object.EmitMessage(stringBuilder.ToString(), null, null, UsePopup: true);
			if (Object.IsPlayer())
			{
				JournalAPI.AddAccomplishment("You drank the raw entropy of the universe and your genome fluctuated in and out of coherence.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
			}
		}
		int num4 = 0;
		for (int count2 = list.Count; num4 < count2; num4++)
		{
			if (!list5.Contains(list[num4]))
			{
				list[num4].SetRapidLevelAmount(0, Sync: false);
				part.RemoveMutation(list[num4], Sync: false);
			}
		}
		int num5 = 0;
		for (int count3 = list.Count; num5 < count3; num5++)
		{
			if (list5[num5].ParentObject != null)
			{
				list5[num5].BaseLevel = list6[num5];
			}
			else
			{
				part.AddMutation(list5[num5], list6[num5], Sync: false);
			}
			list5[num5].SetRapidLevelAmount(rapids[num5], Sync: false);
		}
		Object.SyncMutationLevelAndGlimmer();
		return true;
	}

	public static bool GlitchObject(GameObject Object)
	{
		if (!GameObject.Validate(Object))
		{
			return false;
		}
		if (Object.IsOpenLiquidVolume())
		{
			return GlitchLiquidComponents(Object);
		}
		bool flag = Object.IsPlayer();
		if (Transmutation.TransmuteObject(Object, null, null, null, null, "=subject.T= =verb:glitch= into =object.an=."))
		{
			if (flag)
			{
				Achievement.GLITCH_SELF.Unlock();
			}
			return true;
		}
		return false;
	}

	public static bool GlitchLiquidComponents(GameObject Object, string LiquidTable = "RandomLiquid", int Consume = 0, bool Silent = false)
	{
		LiquidVolume liquidVolume = Object.LiquidVolume;
		if (liquidVolume == null)
		{
			return false;
		}
		GameObject gameObject = Object.Equipped ?? Object.InInventory;
		if (!Silent)
		{
			if (liquidVolume.IsOpenVolume())
			{
				liquidVolume.EmitMessage(Object.T() + " starts to glitch.", 'W', FromDialog: false, gameObject != null);
			}
			else
			{
				liquidVolume.EmitMessage("The liquid mixture inside " + Object.t() + " starts to glitch.", 'W', FromDialog: false, gameObject != null);
			}
		}
		if (liquidVolume.ComponentLiquids.Count > 1)
		{
			liquidVolume.ComponentLiquids.Remove("warmstatic");
			liquidVolume.NormalizeProportions();
		}
		liquidVolume.Volume -= Consume;
		List<string> list = new List<string>(liquidVolume.ComponentLiquids.Keys);
		int num = 0;
		int num2 = 0;
		int count = list.Count;
		while (num < count && num2 < 100)
		{
			string text = PopulationManager.GenerateOne(LiquidTable)?.Blueprint;
			if (text != null && !list.Contains(text))
			{
				int value = liquidVolume.ComponentLiquids[list[num]];
				liquidVolume.ComponentLiquids.Remove(list[num]);
				liquidVolume.ComponentLiquids[text] = value;
				list[num++] = text;
			}
			num2++;
		}
		liquidVolume.Update();
		return true;
	}

	public override bool PourIntoCell(LiquidVolume Liquid, GameObject Pourer, Cell TargetCell, ref int PourAmount, bool CanPourOn, ref bool RequestInterfaceExit)
	{
		if (CanPourOn && Liquid.IsPure() && Pourer != null && Pourer.IsPlayer())
		{
			GameManager.Instance.StaticEffecting = 3;
			SoundManager.PlayUISound("sfx_warmStaticSizzle");
			if (TargetCell.HasRealObject() && !TargetCell.IsEmptyAtRenderLayer(1))
			{
				int num = 0;
				using (List<GameObject>.Enumerator enumerator = TargetCell.GetObjectsViaEventList().GetEnumerator())
				{
					while (enumerator.MoveNext() && (!GlitchObject(enumerator.Current) || ++num < PourAmount * 5))
					{
					}
				}
				int num2 = (int)Math.Ceiling((float)num / 5f);
				PourAmount -= num2;
				Liquid.Volume -= num2;
				if (Pourer.IsPlayer() && num > 0)
				{
					JournalAPI.AddAccomplishment("You poured the raw entropy of the universe onto things and caused them to glitch into other things.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
					Achievement.GLITCH_OBJECT.Unlock();
				}
			}
			else
			{
				PourAmount--;
				Liquid.Volume--;
				Popup.Show("{{W|The space around you starts to glitch.");
				GlitchZone(TargetCell.ParentZone);
				if (Pourer.IsPlayer())
				{
					JournalAPI.AddAccomplishment("You poured the raw entropy of the universe on the ground and caused the space around you to glitch.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
					Achievement.GLITCH_OBJECT.Unlock();
				}
			}
			RequestInterfaceExit = true;
			if (PourAmount <= 0)
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsValidObject(GameObject Object)
	{
		if (Object != null && Object.IsReal && Object.Render != null)
		{
			return Object.Render.Visible;
		}
		return false;
	}

	public static bool MoveToRandomIn(Zone Z, GameObject Object)
	{
		for (int i = 0; i < 10; i++)
		{
			Cell cell = Z.GetCell(Stat.Rnd.Next(Z.Width), Stat.Rnd.Next(Z.Height));
			if (cell != null && cell.IsPassable(Object) && Object.SystemMoveTo(cell, 0, forced: true))
			{
				return true;
			}
		}
		List<Cell> passableCells = Z.GetPassableCells(Object);
		if (!passableCells.IsNullOrEmpty() && Object.SystemMoveTo(passableCells.GetRandomElement(), 0, forced: true))
		{
			return true;
		}
		return false;
	}

	public static void GlitchZone(Zone Z)
	{
		GameManager.Instance.StaticEffecting = 4;
		SoundManager.PlayUISound("sfx_warmStaticSizzle");
		List<GameObject> objects = Z.GetObjects(IsValidObject);
		Stat.PushState("WarmStaticGlitchZone" + Z.ZoneID);
		try
		{
			foreach (GameObject item in objects)
			{
				if (20.in100())
				{
					MoveToRandomIn(Z, item);
				}
				if (2.in100())
				{
					GlitchObject(item);
				}
			}
		}
		finally
		{
			Stat.PopState();
		}
	}

	public static Effect GetFirstApplicableTo(GameObject Object)
	{
		int num = 0;
		Effect effect;
		do
		{
			effect = Activator.CreateInstance(RandomEffects.GetRandomElement()) as Effect;
		}
		while (num++ < 100 && (effect == null || !effect.CanBeAppliedTo(Object)));
		return effect;
	}

	public static Effect ApplyRandomEffectTo(GameObject Object, int Tier = 0, bool EmitMessage = true, bool FromDialog = false)
	{
		if (EmitMessage)
		{
			Object.Physics.DidX("feel", "a bit glitchy", null, null, "W", null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog);
			if (Object.IsPlayer())
			{
				GameManager.Instance.StaticEffecting = 1;
				SoundManager.PlayUISound("sfx_warmStaticSizzle");
			}
		}
		for (int i = 0; i < 10; i++)
		{
			Effect firstApplicableTo = GetFirstApplicableTo(Object);
			if (firstApplicableTo == null)
			{
				return null;
			}
			((ITierInitialized)firstApplicableTo).Initialize((Tier > 0) ? Tier : Object.CurrentZone.NewTier);
			if (Object.ApplyEffect(firstApplicableTo))
			{
				return firstApplicableTo;
			}
		}
		return null;
	}

	public override void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		if (Target.CurrentCell != null && Target.IsCreature)
		{
			if (Liquid.IsPure())
			{
				GlitchObject(Target);
			}
			else if (!(Target.GetStringProperty("WarmStaticSmearedIn") == Target.CurrentZone.ZoneID))
			{
				ApplyRandomEffectTo(Target);
				Target.SetStringProperty("WarmStaticSmearedIn", Target.CurrentZone.ZoneID);
			}
		}
	}

	public override void MixedWith(LiquidVolume Liquid, LiquidVolume NewLiquid, int Amount, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy, ref bool ExitInterface)
	{
		if ((Liquid.ParentObject == null || !Liquid.IsOpenVolume()) && (NewLiquid.ParentObject == null || !NewLiquid.IsOpenVolume()) && Liquid.IsPure())
		{
			(Liquid.ParentObject ?? NewLiquid.ParentObject)?.ApplyEffect(new ContainedStaticGlitching());
		}
	}

	public override void BeforeRender(LiquidVolume Liquid)
	{
		if (!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed)
		{
			Cell currentCell = Liquid.ParentObject.CurrentCell;
			if (currentCell != null)
			{
				int num = Liquid.Amount("warmstatic");
				int r = ((num >= 1000) ? 3 : ((num < 500) ? 1 : 2));
				currentCell.ParentZone?.AddLight(currentCell.X, currentCell.Y, r, LightLevel.Light);
			}
		}
	}

	public override void BeforeRenderSecondary(LiquidVolume Liquid)
	{
		if (!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed)
		{
			Liquid.AddLight(1);
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&Y";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^Y" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&y^Y";
		Liquid.ParentObject.Render.TileColor = "&y";
		Liquid.ParentObject.Render.DetailColor = "Y";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&y";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth())
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&y^Y", "&y", "Y");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		Animate(Liquid, render, (Liquid.Secondary == null) ? MaskAnimationPureTiles : MaskAnimationImpureTiles);
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.TileVariantColors("&Y^Y", "&Y", "Y");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&Y^Y";
			render.TileColor = "&Y";
			render.DetailColor = "Y";
			if (num >= 15 && num >= 30)
			{
				_ = 45;
			}
		}
	}

	public void Animate(LiquidVolume Liquid, Render RenderPart, Dictionary<int, List<string>> FrameSets)
	{
		RenderPart.RenderString = "±";
		int lastPaintMask = Liquid.LastPaintMask;
		if (lastPaintMask < 0)
		{
			return;
		}
		if (!FrameSets.TryGetValue(lastPaintMask, out var value))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder().Append(GetPaintAtlas(Liquid)).Append(GetPaint(Liquid))
				.Append('-')
				.AppendMask(lastPaintMask, 8)
				.Append(GetPaintExtension(Liquid));
			value = (FrameSets[lastPaintMask] = new List<string>(4));
			int index = stringBuilder.IndexOf("deep_") + 5;
			for (int i = 0; i < 4; i++)
			{
				stringBuilder[index] = (char)(49 + i);
				value.Add(stringBuilder.ToString());
			}
		}
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 20;
		if (num < 5)
		{
			RenderPart.Tile = value[0];
			RenderPart.ColorString = "&Y^k";
		}
		else if (num < 10)
		{
			RenderPart.Tile = value[1];
			RenderPart.ColorString = "&k^Y";
		}
		else if (num < 15)
		{
			RenderPart.Tile = value[2];
			RenderPart.ColorString = "&Y^k";
		}
		else
		{
			RenderPart.Tile = value[3];
			RenderPart.ColorString = "&k^Y";
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&Y";
		}
	}

	public override string GetPaint(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "deep_1";
		}
		return base.GetPaint(Liquid);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			if (Liquid.Secondary != null)
			{
				return "Liquids/Static/Impure/";
			}
			return "Liquids/Static/Pure/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override int GetPaintGroup(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth() && Liquid.Secondary == null)
		{
			if (PurePaintGroup == -1)
			{
				return PurePaintGroup = AllocatePaintGroup(Liquid);
			}
			return PurePaintGroup;
		}
		return base.GetPaintGroup(Liquid);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("chance", 5);
	}

	public override float GetValuePerDram()
	{
		return 50f;
	}

	public override float GetPureLiquidValueMultipler()
	{
		return 20f;
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, bool FilthAffinity, ref bool Uncacheable)
	{
		return 95;
	}

	[WishCommand("warmplayer", null)]
	public static void WishTransmute()
	{
		GlitchObject(The.Player);
	}

	[WishCommand("warmskills", null)]
	public static void WishWarmSkills()
	{
		GlitchSkills(The.Player);
	}

	[WishCommand("warmmutations", null)]
	public static void WishWarmMutations()
	{
		GlitchMutations(The.Player);
	}

	[WishCommand("warmeffect", null)]
	public static void WishWarmEffect()
	{
		Effect effect = ApplyRandomEffectTo(The.Player);
		The.Player.EmitMessage(effect?.DisplayNameStripped + " applied to " + The.Player.DisplayNameOnlyDirectAndStripped + ".", null, "K");
	}

	[WishCommand("warmeffect", null)]
	public static void WishWarmEffectSpec(string Name)
	{
		Type type = RandomEffects.FirstOrDefault((Type x) => x.Name.EqualsNoCase(Name));
		if (type == null || !(Activator.CreateInstance(type) is Effect effect))
		{
			return;
		}
		((ITierInitialized)effect).Initialize(The.Player.CurrentZone.NewTier);
		GameObject gameObject = The.Player;
		for (int num = 0; num < 10; num++)
		{
			if (gameObject.ApplyEffect(effect))
			{
				The.Player.EmitMessage(effect.DisplayNameStripped + " applied to " + gameObject.DisplayNameOnlyDirectAndStripped + ".", null, "K");
				return;
			}
			gameObject = The.Player.Inventory.Objects.GetRandomElement();
		}
		The.Player.EmitMessage("No valid targets for " + effect.DisplayNameStripped + ".", null, "K");
	}
}
