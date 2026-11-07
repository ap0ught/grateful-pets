using System;
using ConsoleLib.Console;
using HistoryKit;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Skulk_Tonic : ITonicEffect, ITierInitialized
{
	public Guid BurrowingClawsID;

	public Guid DarkVisionID;

	public bool bOverdose;

	[NonSerialized]
	private static readonly Color ColorBrightBlue = new Color(0f, 0f, 1f);

	[NonSerialized]
	private static readonly Color ColorDarkBlue = new Color(0f, 0f, 0.5f);

	public Skulk_Tonic()
	{
	}

	public Skulk_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		if (If.CoinFlip())
		{
			bOverdose = true;
		}
		Duration = Stat.Roll(1000, 1200);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{skulk|skulk}} tonic";
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{skulk|skulk}} tonic";
	}

	public override string GetDetails()
	{
		if (Object.IsTrueKin())
		{
			return "+40% Move Speed at night and underground.\n+4 Agility at night and underground.\n-20% Move Speed in the daylight.\n-3 Agility in the daylight.\nCan see in the dark (radius 8).\nSuffers double damage from light-based attacks.";
		}
		return "Has grown burrowing claws.\n+25% Move Speed at night and underground.\n+3 Agility at night and underground.\n-20% Move Speed in the daylight.\n-3 Agility in the daylight.\nCan see in the dark (radius 8).\nSuffers double damage from light-based attacks.";
	}

	public void RemoveBonus()
	{
		StatShifter.RemoveStatShifts();
	}

	public void ApplyBonus()
	{
		if (Object.CurrentCell != null)
		{
			int num;
			int amount;
			if (Object.CurrentCell.ParentZone.Z <= 10 && Calendar.IsDay())
			{
				num = (int)((double)(-Object.BaseStat("MoveSpeed")) * 0.2);
				amount = -3;
			}
			else if (Object.IsTrueKin())
			{
				num = (int)((float)Object.BaseStat("MoveSpeed") * 0.4f);
				amount = 4;
			}
			else
			{
				num = (int)((float)Object.BaseStat("MoveSpeed") * 0.25f);
				amount = 3;
			}
			StatShifter.SetStatShift("MoveSpeed", -num);
			StatShifter.SetStatShift("Agility", amount);
		}
	}

	private void ApplyChanges()
	{
		ApplyBonus();
		Mutations mutations = Object.RequirePart<Mutations>();
		if (Object.IsMutant())
		{
			int level = (Object.HasPart(typeof(BurrowingClaws)) ? "2-3".RollCached() : 6);
			BurrowingClawsID = mutations.AddMutationMod("BurrowingClaws", null, level, Mutations.MutationModifierTracker.SourceType.Tonic);
		}
		DarkVisionID = mutations.AddMutationMod("DarkVision", null, 1, Mutations.MutationModifierTracker.SourceType.Tonic);
		Object.GetPart<DarkVision>().Radius += 3;
	}

	private void UnapplyChanges()
	{
		RemoveBonus();
		Mutations mutations = Object.RequirePart<Mutations>();
		if (BurrowingClawsID != Guid.Empty)
		{
			mutations.RemoveMutationMod(BurrowingClawsID);
		}
		Object.GetPart<DarkVision>().Radius -= 3;
		if (DarkVisionID != Guid.Empty)
		{
			mutations.RemoveMutationMod(DarkVisionID);
		}
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetHeartCount() > 1)
			{
				Popup.Show("Your hearts begin to beat faster and your pupils dilate.");
			}
			else
			{
				Popup.Show("Your heart begins to beat faster and your pupils dilate.");
			}
		}
		if (Object.GetLongProperty("Overdosing", 0L) == 1 || bOverdose)
		{
			FireEvent(Event.New("Overdose"));
		}
		if (Object.TryGetEffect<Skulk_Tonic>(out var Effect))
		{
			Effect.Duration += Duration;
			return false;
		}
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your heart rate returns to normal and your pupils shrink.");
		}
		UnapplyChanges();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			ApplyBonus();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeApplyDamage");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.IsLightDamage())
			{
				damage.Amount *= 2;
			}
		}
		else if (E.ID == "Overdose")
		{
			if (Duration > 0)
			{
				Duration = 0;
				ApplyOverdose(Object);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			RemoveBonus();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyBonus();
		}
		return base.FireEvent(E);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		ApplyOverdose(Object);
	}

	public static void ApplyOverdose(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetLongProperty("Overdosing", 0L) == 1)
			{
				Popup.Show("Your mutant physiology reacts adversely to the tonic. Your field of vision erupts into a plane of blinding, white light.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. Your field of vision erupts into a plane of blinding, white light.");
			}
		}
		Object.ApplyEffect(new Blind(Stat.Random(1, 10) + 20));
	}

	public override bool Render(RenderEvent E)
	{
		E.WantsToPaint = !E.DisableFullscreenColorEffects && !E.Alt && Object.IsPlayer();
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 15 && num < 25)
		{
			E.Tile = null;
			E.RenderString = "|";
			E.ColorString = "&K";
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer Buffer)
	{
		Zone currentZone = Object.CurrentZone;
		Color blue = The.Color.Blue;
		Color darkBlue = The.Color.DarkBlue;
		Color darkBlack = The.Color.DarkBlack;
		int i = 0;
		int num = 0;
		for (int height = Buffer.Height; i < height; i++)
		{
			int num2 = 0;
			int width = Buffer.Width;
			while (num2 < width)
			{
				ConsoleChar consoleChar = Buffer[num2, i];
				consoleChar._Background = darkBlack;
				if (currentZone.VisibilityMap[num])
				{
					consoleChar._Foreground = blue;
					consoleChar._TileForeground = ColorBrightBlue;
					consoleChar._Detail = ColorDarkBlue;
				}
				else
				{
					consoleChar._Foreground = darkBlue;
					consoleChar._TileForeground = ColorDarkBlue;
					consoleChar._Detail = ColorDarkBlue;
				}
				num2++;
				num++;
			}
		}
	}
}
