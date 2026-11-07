using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class RealityStabilized : Effect, ITierInitialized
{
	public enum ContestResult
	{
		Abjection,
		Surrender,
		Failure,
		Success
	}

	public const int DEFAULT_VISIBILITY = 2;

	public int Strength;

	public int Visibility = 2;

	public bool Projective;

	public GameObject Owner;

	public bool ScanAdjacentCells;

	public int IndependentStrength;

	public int IndependentStrengthDecline;

	public int IndependentVisibility = 2;

	public bool IndependentlyProjective;

	private int FrameOffset;

	public RealityStabilized()
	{
		Duration = 1;
		DisplayName = "{{Y|astrally tethered}}";
	}

	public RealityStabilized(int Strength = 0, GameObject Owner = null, int Duration = 1, int IndependentStrength = 0, int IndependentStrengthDecline = 0, int IndependentVisibility = 2, bool IndependentlyProjective = false)
		: this()
	{
		this.Strength = Strength;
		this.Owner = Owner;
		base.Duration = Duration;
		this.IndependentStrength = IndependentStrength;
		this.IndependentStrengthDecline = IndependentStrengthDecline;
		this.IndependentVisibility = IndependentVisibility;
		this.IndependentlyProjective = IndependentlyProjective;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 300);
		Strength = Stat.Random(1, 200);
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override bool SameAs(Effect e)
	{
		RealityStabilized realityStabilized = e as RealityStabilized;
		if (realityStabilized.Strength != Strength)
		{
			return false;
		}
		if (realityStabilized.Visibility != Visibility)
		{
			return false;
		}
		if (realityStabilized.Projective != Projective)
		{
			return false;
		}
		if (realityStabilized.Owner != Owner)
		{
			return false;
		}
		if (realityStabilized.ScanAdjacentCells != ScanAdjacentCells)
		{
			return false;
		}
		if (realityStabilized.IndependentStrength != IndependentStrength)
		{
			return false;
		}
		if (realityStabilized.IndependentStrengthDecline != IndependentStrengthDecline)
		{
			return false;
		}
		if (realityStabilized.IndependentVisibility != IndependentVisibility)
		{
			return false;
		}
		if (realityStabilized.IndependentlyProjective != IndependentlyProjective)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDescription()
	{
		if (Visibility < 1)
		{
			return null;
		}
		if (Strength < 100)
		{
			return "{{y|astrally burdened}}";
		}
		return "{{Y|astrally tethered}}";
	}

	public override string GetDetails()
	{
		if (Strength < 100)
		{
			return "Has trouble distorting spacetime in the local region.";
		}
		return "Cannot distort spacetime in the local region.";
	}

	public override bool CanApplyToStack()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<RealityStabilized>())
		{
			return false;
		}
		if (!Object.FireEvent("ApplyRealityStabilized"))
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_spacetimeAnchored");
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID && ID != PooledEvent<CheckRealityDistortionAccessibilityEvent>.ID && ID != PooledEvent<CheckRealityDistortionAdvisabilityEvent>.ID && ID != PooledEvent<CheckRealityDistortionUsabilityEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == MissileTraversingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(MissileTraversingCellEvent E)
	{
		if ((E.Cell == null || Object == GetStrongestRealityStabilizationSource(E.Cell)) && RandomlyTakeEffect() && Stabilize(E.Object))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckRealityDistortionAccessibilityEvent E)
	{
		Cell cell = Object.CurrentCell;
		if (cell == null || Object == GetStrongestRealityStabilizationSource(cell))
		{
			if (RandomlyTakeEffect(-E.Penetration))
			{
				E.Allow = false;
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckRealityDistortionUsabilityEvent E)
	{
		if (E.Threshold.HasValue)
		{
			if (Math.Min(E.Threshold.Value, 100) <= Strength - E.Penetration)
			{
				return false;
			}
		}
		else if (RandomlyTakeEffect(-E.Penetration))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckRealityDistortionAdvisabilityEvent E)
	{
		if (Strength - E.Penetration > E.Threshold)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (Object?.Brain != null)
		{
			if (Maintain())
			{
				Stabilize();
			}
			if (Duration > 1 && Duration != 9999)
			{
				Duration--;
				if (Duration <= 1 && Owner != null)
				{
					Owner = null;
				}
			}
			if (IndependentStrengthDecline != 0)
			{
				IndependentStrength = Math.Max(IndependentStrength - IndependentStrengthDecline, 0);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Object?.Brain == null)
		{
			if (Maintain())
			{
				Stabilize();
			}
			if (Duration > 1 && Duration != 9999)
			{
				Duration--;
				if (Duration <= 1 && Owner != null)
				{
					Owner = null;
				}
			}
			if (IndependentStrengthDecline != 0)
			{
				IndependentStrength = Math.Max(IndependentStrength - IndependentStrengthDecline, 0);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EquipperUnequipped");
		Registrar.Register("InitiateRealityDistortionLocal");
		Registrar.Register("InitiateRealityDistortionRemote");
		Registrar.Register("InitiateRealityDistortionTransit");
		Registrar.Register("MaintainRealityStabilization");
		Registrar.Register("ObjectEnteringCellBlockedBySolid");
		Registrar.Register("SpaceTimeVortexContact");
		base.Register(Object, Registrar);
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (!Object.IsVisible())
		{
			return true;
		}
		for (int i = 0; i < E.Imposters.Count; i++)
		{
			if (E.Imposters[i].prefab == "Prefabs/Particles/NormalityField")
			{
				return true;
			}
		}
		E.Imposters.Add(new ImposterExtra.ImposterInfo("Prefabs/Particles/NormalityField"));
		return base.FinalRender(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (!Options.DisableImposters)
		{
			return true;
		}
		if (Visibility > 1)
		{
			if (FrameOffset == 0)
			{
				FrameOffset = Stat.Random(1, 1000);
			}
			if ((XRLCore.CurrentFrameLong10 + FrameOffset) % 2000 >= 1000)
			{
				E.ColorString = ((Visibility > 2) ? "&y^Y" : "&y");
				E.DetailColor = "K";
			}
		}
		return true;
	}

	protected void ProcessGameObject(GameObject obj)
	{
		RealityStabilization part = obj.GetPart<RealityStabilization>();
		if (part == null || !part.IsObjectActivePartSubject(Object))
		{
			return;
		}
		if (part.WorksOnAdjacentCellContents && !ScanAdjacentCells && !obj.InSameCellAs(Object))
		{
			ScanAdjacentCells = true;
		}
		int effectiveStrength = part.EffectiveStrength;
		if (effectiveStrength > 0)
		{
			if (effectiveStrength > Strength)
			{
				Strength = effectiveStrength;
			}
			int visibilityFor = part.GetVisibilityFor(Object);
			if (visibilityFor > Visibility)
			{
				Visibility = visibilityFor;
			}
			if (part.Projective && obj.CurrentCell == null)
			{
				Projective = true;
			}
		}
	}

	public virtual bool Maintain()
	{
		if (Duration > 1)
		{
			Strength = IndependentStrength;
			Visibility = IndependentVisibility;
			Projective = IndependentlyProjective;
		}
		else
		{
			Strength = 0;
			Visibility = 0;
			Projective = false;
		}
		if (Object == null)
		{
			return false;
		}
		ProcessGameObject(Object);
		AmbientRealityStabilized effect = Object.GetEffect<AmbientRealityStabilized>();
		if (effect != null && effect.Duration > 0)
		{
			if (Strength < effect.Strength)
			{
				Strength = effect.Strength;
			}
			if (Visibility < effect.Visibility)
			{
				Visibility = effect.Visibility;
			}
			if (effect.Projective && !Projective)
			{
				Projective = true;
			}
		}
		Cell cell = Object.CurrentCell;
		if (cell != null)
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				if (cell.Objects[i] != Object)
				{
					ProcessGameObject(cell.Objects[i]);
				}
			}
			if (ScanAdjacentCells)
			{
				ScanAdjacentCells = false;
				List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells();
				int j = 0;
				for (int count2 = localAdjacentCells.Count; j < count2; j++)
				{
					Cell cell2 = localAdjacentCells[j];
					int k = 0;
					for (int count3 = cell2.Objects.Count; k < count3; k++)
					{
						ProcessGameObject(cell2.Objects[k]);
					}
				}
			}
		}
		List<GameObject> equippedObjectsAndInstalledCyberneticsReadonly = Object.GetEquippedObjectsAndInstalledCyberneticsReadonly();
		if (equippedObjectsAndInstalledCyberneticsReadonly != null)
		{
			int l = 0;
			for (int count4 = equippedObjectsAndInstalledCyberneticsReadonly.Count; l < count4; l++)
			{
				ProcessGameObject(equippedObjectsAndInstalledCyberneticsReadonly[l]);
			}
		}
		if (Strength <= 0 && Duration <= 1)
		{
			Duration = 0;
			Object.RemoveEffect(this);
			return false;
		}
		return true;
	}

	public bool SynchronizeEffect()
	{
		if (Object == null)
		{
			return false;
		}
		int strength = Strength;
		int visibility = Visibility;
		bool projective = Projective;
		if (!Maintain())
		{
			return false;
		}
		if (Strength <= 0)
		{
			return false;
		}
		if (strength == Strength && visibility == Visibility && projective == Projective)
		{
			return false;
		}
		Stabilize();
		return true;
	}

	public bool RandomlyTakeEffect()
	{
		return Strength.in100();
	}

	public bool RandomlyTakeEffect(int Adjust)
	{
		return (Strength + Adjust).in100();
	}

	public bool TakeEffectBasedOnPerformance(int Performance)
	{
		return Strength > Performance;
	}

	private void FailedToContest(GameObject GO)
	{
		BodyPart bodyPart = GO.Body?.GetFirstPart("Face");
		if (GO.IsPlayer())
		{
			if (bodyPart != null && bodyPart.VariantType == "Face")
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("You try to push through the normality lattice, but it snaps back into place.");
				if (GO.GetIntProperty("Analgesia") <= 0)
				{
					stringBuilder.Append(" You wince in pain.");
				}
				Popup.Show(stringBuilder.ToString());
			}
			else
			{
				Popup.Show("You try to push through the normality lattice, but it snaps back into place.");
			}
		}
		else
		{
			SensePsychicEffect sensePsychicEffect = SensePsychic.SensePsychicFromPlayer(GO);
			if (sensePsychicEffect != null)
			{
				IComponent<GameObject>.AddPlayerMessage("You feel a psychic thud as " + (sensePsychicEffect.Identified ? GO.does("push") : "someone pushes") + " against the structure of spacetime and " + (sensePsychicEffect.Identified ? GO.GetVerb("fail") : "fails") + " to break through.");
			}
			if (bodyPart != null && bodyPart.VariantType == "Face" && GO.GetIntProperty("Analgesia") <= 0 && IComponent<GameObject>.Visible(GO))
			{
				IComponent<GameObject>.AddPlayerMessage(GO.Does("wince") + ".");
			}
		}
		if (GO.TakeDamage(Stat.Random(1, 6), "from %t failed assault on the structure of spacetime.", "Cosmic RealityDistortionFailure Unavoidable", "You dashed " + GO.itself + " against the crags of spacetime.", GO.It + " @@dashed " + GO.itself + " against the crags of spacetime.", null, GO, null, null, null, Accidental: true) && !GO.MakeSave("Willpower", 20, null, null, "RealityStabilization Daze"))
		{
			GO.ApplyEffect(new Dazed(Stat.Random(1, 6)));
		}
		GO.UseEnergy(1000, "Effect");
	}

	private ContestResult TryContest(GameObject GO, int Adjust = 0, int Performance = int.MinValue)
	{
		if ((Performance == int.MinValue) ? RandomlyTakeEffect(Adjust) : TakeEffectBasedOnPerformance(Performance))
		{
			FailedToContest(GO);
			return ContestResult.Failure;
		}
		if (!GO.IsPlayer())
		{
			SensePsychicEffect sensePsychicEffect = SensePsychic.SensePsychicFromPlayer(GO);
			if (sensePsychicEffect != null)
			{
				IComponent<GameObject>.AddPlayerMessage("You feel a psychic whiff as " + (sensePsychicEffect.Identified ? GO.does("push") : "something pushes") + " past resistance in the structure of spacetime.");
			}
		}
		return ContestResult.Success;
	}

	private int ProcessEvent(Event E, out bool Dual, bool MarkHandled = false)
	{
		Dual = false;
		int num = -E.GetIntParameter("RealityStabilizationPenetration");
		if (E.HasParameter("Mutation") && E.GetParameter("Mutation") is IPart { ParentObject: { } parentObject } && parentObject != Object)
		{
			RealityStabilized effect = parentObject.GetEffect<RealityStabilized>();
			if (effect != null)
			{
				Dual = true;
				int strength = effect.Strength;
				if (strength > Strength + num)
				{
					num += strength - (Strength + num);
				}
			}
			if (MarkHandled)
			{
				E.SetParameter("InterdictionHandled", 1);
				MarkHandled = false;
			}
		}
		if (E.HasParameter("Cell"))
		{
			Cell c = E.GetParameter("Cell") as Cell;
			GameObject strongestRealityStabilizationSource = GetStrongestRealityStabilizationSource(c);
			if (strongestRealityStabilizationSource != null && strongestRealityStabilizationSource != Object)
			{
				RealityStabilized effect2 = strongestRealityStabilizationSource.GetEffect<RealityStabilized>();
				if (effect2 != null)
				{
					Dual = true;
					int strength2 = effect2.Strength;
					if (strength2 > Strength + num)
					{
						num += strength2 - (Strength + num);
					}
				}
				if (MarkHandled)
				{
					E.SetParameter("InterdictionHandled", 1);
					MarkHandled = false;
				}
			}
		}
		return num;
	}

	private int ProcessEvent(Event E, bool MarkHandled = false)
	{
		bool Dual;
		return ProcessEvent(E, out Dual, MarkHandled);
	}

	public ContestResult OptionToContest(GameObject GO, int Adjust = 0, bool Dual = false)
	{
		int num = 100 - (Strength + Adjust);
		int num2 = GO.Stat("Intelligence");
		Mutations part = GO.GetPart<Mutations>();
		if (part != null)
		{
			if (part.HasMutation("Intuition"))
			{
				num2 += 10;
			}
			if (part.HasMutation("Precognition"))
			{
				num2 += 5;
			}
			if (part.HasMutation("SpacetimeVortex"))
			{
				num2 += 5;
			}
			if (part.HasMutation("Skittish"))
			{
				num -= 10;
			}
		}
		if (GO.HasSkill("Discipline_IronMind"))
		{
			num2 += 2;
		}
		if (GO.HasSkill("Discipline_Lionheart"))
		{
			num += 3;
		}
		if (num2 <= 10)
		{
			num += 20;
		}
		else if (num2 <= 15)
		{
			num += 10;
		}
		else if (num2 <= 20)
		{
			num -= 10;
		}
		else if (num2 <= 25)
		{
			num -= 5;
		}
		int num3;
		if (num2 <= 10)
		{
			num3 = 20;
		}
		else
		{
			num3 = ((num2 <= 20) ? 10 : ((num2 > 30) ? 1 : 5));
		}
		if (num3 > 1)
		{
			num -= num % num3;
		}
		if (num > 100 - num3)
		{
			num = 100 - num3;
		}
		bool flag = GO.IsPlayer() && Options.SifrahRealityDistortion;
		if (num <= 0 && !flag)
		{
			return ContestResult.Abjection;
		}
		if (GO.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			string chanceColor = Stat.GetChanceColor(num);
			if (Dual)
			{
				stringBuilder.Append("A normality lattice prevents you from altering spacetime in both your local region and the local region you're trying to interact with.");
			}
			else
			{
				stringBuilder.Append("A normality lattice prevents you from altering spacetime in ").Append((GO.CurrentCell == Object.CurrentCell) ? "the" : "that").Append(" local region.");
			}
			stringBuilder.Append(" You can try to push through at some risk.");
			if (flag)
			{
				int num4 = GO.Stat("Ego");
				int num5 = 20 - num / 5 - num4 / 5;
				string value;
				if (num5 >= 12)
				{
					value = "almost impossible";
				}
				else if (num5 >= 9)
				{
					value = "challenging";
				}
				else if (num5 >= 6)
				{
					value = "moderately difficult";
				}
				else
				{
					value = ((num5 < 3) ? "very easy" : "easy");
				}
				stringBuilder.Append(" Your feeling is that success would be ").Append(value).Append(". Do you want to try?");
				if (Popup.ShowYesNo(stringBuilder.ToString()) == DialogResult.Yes)
				{
					GameObject contextObject = Owner ?? Object ?? GO;
					RealityDistortionSifrah realityDistortionSifrah = new RealityDistortionSifrah(contextObject, "RealityStabilizationPenetration", "overcoming a normality lattice", num4, Math.Max((Strength + Adjust) / 5, 1));
					realityDistortionSifrah.Play(contextObject);
					if (realityDistortionSifrah.Abort || realityDistortionSifrah.InterfaceExitRequested)
					{
						return ContestResult.Surrender;
					}
					return TryContest(GO, 0, realityDistortionSifrah.Performance);
				}
				return ContestResult.Surrender;
			}
			stringBuilder.Append(" You estimate");
			if (num < num3)
			{
				stringBuilder.Append(" less than a {{").Append(chanceColor).Append('|')
					.Append(num3.ToString())
					.Append("%}}");
			}
			else
			{
				stringBuilder.Append(" about a {{").Append(chanceColor).Append('|')
					.Append(num.ToString())
					.Append("%}}");
			}
			stringBuilder.Append(chanceColor).Append(" chance of success. Do you want to try?");
			if (Popup.ShowYesNo(stringBuilder.ToString()) == DialogResult.Yes)
			{
				return TryContest(GO, Adjust);
			}
			return ContestResult.Surrender;
		}
		if (num.in100() && num.in100())
		{
			return TryContest(GO, Adjust);
		}
		return ContestResult.Surrender;
	}

	private bool AttemptInterdictedAction(Event E)
	{
		if (E.HasFlag("Forced"))
		{
			return true;
		}
		if (E.HasParameter("Mutation"))
		{
			IPart part = E.GetParameter("Mutation") as IPart;
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? part.ParentObject;
			int num = ProcessEvent(E, out var Dual, MarkHandled: true);
			ContestResult contestResult = OptionToContest(gameObject, num, Dual && num > 0);
			if (contestResult == ContestResult.Abjection)
			{
				if (Dual && num > 0)
				{
					ShowDualInterdictMessage(gameObject, E);
				}
				else if (gameObject.CurrentCell == Object.CurrentCell)
				{
					ShowGenericInterdictMessage(gameObject, E);
				}
				else
				{
					ShowDistantInterdictMessage(gameObject, E);
				}
			}
			switch (contestResult)
			{
			case ContestResult.Success:
				return true;
			case ContestResult.Failure:
				E.RequestInterfaceExit();
				break;
			}
		}
		else if (E.HasParameter("Device"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Device");
			GameObject user = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Operator");
			int adjust = -GetRealityStabilizationPenetrationEvent.GetFor(gameObjectParameter) + ProcessEvent(E, MarkHandled: true);
			if (!RandomlyTakeEffect(adjust))
			{
				return true;
			}
			ShortCircuitDevice(gameObjectParameter, user, E);
		}
		else if (E.HasParameter("Food"))
		{
			int adjust2 = -GetRealityStabilizationPenetrationEvent.GetFor(E.GetGameObjectParameter("Food")) + ProcessEvent(E, MarkHandled: true);
			if (!RandomlyTakeEffect(adjust2))
			{
				return true;
			}
		}
		else if (E.HasParameter("Source"))
		{
			int adjust3 = -GetRealityStabilizationPenetrationEvent.GetFor(E.GetGameObjectParameter("Source")) + ProcessEvent(E, MarkHandled: true);
			if (!RandomlyTakeEffect(adjust3))
			{
				return true;
			}
		}
		else if (!RandomlyTakeEffect())
		{
			return true;
		}
		return false;
	}

	public static void ShowGenericInterdictMessage(GameObject CheckObject = null, Event E = null)
	{
		if (CheckObject == null || CheckObject.IsPlayer())
		{
			string text = E?.GetStringParameter("Purpose");
			Popup.Show("You cannot alter spacetime through the normality lattice in the local region" + ((!string.IsNullOrEmpty(text)) ? (", in order to " + text) : "") + ".");
		}
	}

	public static void ShowDistantInterdictMessage(GameObject CheckObject = null, Event E = null)
	{
		if (CheckObject == null || CheckObject.IsPlayer())
		{
			string text = E?.GetStringParameter("Purpose");
			Popup.Show("You cannot alter spacetime through the normality lattice in the local region" + ((!string.IsNullOrEmpty(text)) ? (", in order to " + text) : "") + ".");
		}
	}

	public static void ShowDualInterdictMessage(GameObject CheckObject = null, Event E = null)
	{
		if (CheckObject == null || CheckObject.IsPlayer())
		{
			string text = E?.GetStringParameter("Purpose");
			Popup.Show("You cannot alter spacetime through either the normality lattice in your local region or the local region you're trying to interact with" + ((!string.IsNullOrEmpty(text)) ? (", in order to " + text) : "") + ".");
		}
	}

	private static void ShortCircuitDevice(GameObject Device, GameObject User, Event FromEvent = null)
	{
		if (User == null)
		{
			User = Device.Equipped ?? Device.Implantee ?? Device.InInventory;
		}
		if (User == null)
		{
			IComponent<GameObject>.AddPlayerMessage(Device.Does("shower", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, true) + " sparks everywhere.");
		}
		else if (User.IsPlayer())
		{
			Popup.Show(Device.Does("emit", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, true, AsPossessed: true, User) + " a shower of sparks!");
		}
		else if (IComponent<GameObject>.Visible(User))
		{
			IComponent<GameObject>.AddPlayerMessage(Device.Does("shower", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, true, AsPossessed: true, User) + " sparks everywhere.");
		}
		if (User != null)
		{
			if (User.TakeDamage(Stat.Random(1, 6), "from %t electrical discharge!", "Electric", null, null, User, Device, null, null, Device, Accidental: true, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, User.IsPlayer()))
			{
				for (int i = 0; i < 1; i++)
				{
					User.ParticleText("&Wú");
				}
				for (int j = 0; j < 1; j++)
				{
					User.ParticleText("&Yú");
				}
			}
			Device.UseCharge("10d100".RollCached(), LiveOnly: false, 0L);
			User.UseEnergy(1000, "Item Use Failure");
		}
		FromEvent?.RequestInterfaceExit();
	}

	private static void ShortCircuitDevice(Event E)
	{
		ShortCircuitDevice(E.GetGameObjectParameter("Device"), E.GetGameObjectParameter("Operator"), E);
	}

	public bool Stabilize(bool Projecting = false)
	{
		bool Relevant;
		bool CanDestroy;
		return Stabilize(Object, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, bool Projecting = false)
	{
		bool Relevant;
		bool CanDestroy;
		return Stabilize(GO, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, out bool Relevant)
	{
		bool CanDestroy;
		return Stabilize(GO, Projecting: false, out Relevant, out CanDestroy);
	}

	public bool Stabilize(out bool Relevant, out bool CanDestroy)
	{
		return Stabilize(Object, Projecting: false, out Relevant, out CanDestroy);
	}

	public bool Stabilize(bool Projecting, out bool Relevant, out bool CanDestroy)
	{
		return Stabilize(Object, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, bool Projecting, out bool Relevant)
	{
		bool CanDestroy;
		return Stabilize(GO, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, bool Projecting, out bool Relevant, out bool CanDestroy)
	{
		RealityStabilizeEvent.Send(this, GO, Projecting, out Relevant, out CanDestroy);
		if (CanDestroy)
		{
			return GO?.IsInvalid() ?? true;
		}
		return false;
	}

	private GameObject GetStrongestRealityStabilizationSource(Cell C)
	{
		GameObject result = null;
		int num = 0;
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			RealityStabilized effect = C.Objects[i].GetEffect<RealityStabilized>();
			if (effect != null && effect.Strength > num)
			{
				result = C.Objects[i];
				num = effect.Strength;
			}
		}
		return result;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteringCellBlockedBySolid")
		{
			if (Projective)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				if (Stabilize(gameObjectParameter, Projecting: true, out var _, out var CanDestroy))
				{
					return false;
				}
				if (CanDestroy)
				{
					if (Object.IsPlayer())
					{
						StringBuilder stringBuilder = Event.NewStringBuilder();
						stringBuilder.Append(gameObjectParameter.T());
						if (gameObjectParameter.HasPart<Metal>())
						{
							stringBuilder.Append(" {{electrical|").Append(gameObjectParameter.GetVerb("spark", PrependSpace: false)).Append(" and")
								.Append(gameObjectParameter.GetVerb("buzz"))
								.Append("}}");
						}
						else
						{
							stringBuilder.Append(" {{C|").Append(gameObjectParameter.GetVerb("shift", PrependSpace: false)).Append(" and")
								.Append(gameObjectParameter.GetVerb("waver"))
								.Append("}}");
						}
						stringBuilder.Append(" for a moment, but nothing else happens.");
						Popup.Show(stringBuilder.ToString());
					}
					Object.UseEnergy(1000, "Movement Failure");
				}
			}
		}
		else if (E.ID == "InitiateRealityDistortionLocal")
		{
			if (!AttemptInterdictedAction(E))
			{
				return false;
			}
		}
		else if (E.ID == "InitiateRealityDistortionRemote" || E.ID == "InitiateRealityDistortionTransit")
		{
			if (E.GetIntParameter("InterdictionHandled") < 1 && (Object == E.GetGameObjectParameter("Object") || Object == E.GetGameObjectParameter("Subject") || Object.CurrentCell == null || Object == GetStrongestRealityStabilizationSource(Object.CurrentCell)))
			{
				return AttemptInterdictedAction(E);
			}
		}
		else if (E.ID == "SpaceTimeVortexContact")
		{
			if (Stabilize(E.GetGameObjectParameter("Object")))
			{
				return false;
			}
		}
		else if (E.ID == "MaintainRealityStabilization" || E.ID == "EquipperUnequipped")
		{
			Maintain();
		}
		return base.FireEvent(E);
	}
}
