using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Consumer : IPart
{
	public int Chance = 100;

	public int WeightThresholdPercentage = 90;

	public bool SuppressCorpseDrops = true;

	public bool Active = true;

	public string Message = "{{R|=subject.T= =verb:consume= =object.an==object.directionIfAny=!}}";

	public string FloatMessage;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginMoveLate");
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMoveLate")
		{
			if (Active && E.GetStringParameter("Type").IsNullOrEmpty() && !TryToConsumeObjectsIn(E.GetParameter("DestinationCell") as Cell))
			{
				return false;
			}
		}
		else if (E.ID == "VillageInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public bool TryToConsumeObjectsIn(Cell C)
	{
		if (C == null || C.OnWorldMap())
		{
			return true;
		}
		if (!Chance.in100())
		{
			return true;
		}
		Cell cell = ParentObject.CurrentCell;
		List<GameObject> list = Event.NewGameObjectList();
		int consumeWeightCapacity = GetConsumeWeightCapacity();
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (!ShouldIgnore(gameObject))
			{
				if (!CanConsume(gameObject, consumeWeightCapacity))
				{
					list.Clear();
					ParentObject.SetIntProperty("AIKeepMoving", 1);
					Combat.AttackCell(ParentObject, C);
					return false;
				}
				list.Add(gameObject);
			}
		}
		foreach (GameObject item in list)
		{
			Consume(item);
		}
		if (ParentObject.CurrentCell != cell)
		{
			return false;
		}
		return true;
	}

	public void Consume(GameObject Object)
	{
		string text = GameText.VariableReplace(Message, ParentObject, Object);
		if (!text.IsNullOrEmpty())
		{
			EmitMessage(text, ' ', FromDialog: false, UsePopup: false, Object.IsVisible());
		}
		string text2 = GameText.VariableReplace(FloatMessage, ParentObject, Object, StripColors: true);
		if (!text2.IsNullOrEmpty())
		{
			Object.ParticleText(text2, IComponent<GameObject>.ConsequentialColorChar(null, Object));
		}
		BeingConsumedEvent.Send(ParentObject, Object);
		bool flag = false;
		if (Object.IsVisible() && Options.UseParticleVFX)
		{
			CombatJuice.playPrefabAnimation(Object.CurrentCell.Location, "Abilities/AbilityVFXConsumed", ParentObject.ID, $"{Object.Render.Tile};{Object.Render.GetTileForegroundColor()};{Object.Render.getDetailColor()}");
		}
		if (SuppressCorpseDrops)
		{
			Object.ModIntProperty("SuppressCorpseDrops", 1);
			flag = true;
		}
		try
		{
			if (Object.IsPlayer())
			{
				Achievement.SWALLOWED_WHOLE.Unlock();
			}
			if (Object.Count > 1)
			{
				Object.Obliterate("You were consumed whole by " + ParentObject.an() + ".", Silent: false, Object.It + Object.GetVerb("were") + " @@consumed whole by " + ParentObject.an() + ".");
			}
			else
			{
				Object.Die(ParentObject, null, "You were consumed whole by " + ParentObject.an() + ".", Object.It + Object.GetVerb("were") + " @@consumed whole by " + ParentObject.an() + ".");
			}
		}
		finally
		{
			if (flag && GameObject.Validate(ref Object) && !Object.IsNowhere())
			{
				Object.ModIntProperty("SuppressCorpseDrops", -1, RemoveIfZero: true);
			}
		}
	}

	public static bool CanConsume(GameObject Object, int WeightThreshold)
	{
		return Object.Weight < WeightThreshold;
	}

	public bool CanConsume(GameObject Object)
	{
		return CanConsume(Object, GetConsumeWeightCapacity());
	}

	public int GetConsumeWeightCapacity()
	{
		return ParentObject.Weight * WeightThresholdPercentage / 100;
	}

	public bool ShouldIgnore(GameObject Object)
	{
		if (!Object.IsReal)
		{
			return true;
		}
		if (Object.IsScenery)
		{
			return true;
		}
		if (Object.HasTag("ExcavatoryTerrainFeature"))
		{
			return true;
		}
		if (Object.GetMatterPhase() >= 3)
		{
			return true;
		}
		if (Object.HasPart<FungalVision>() && FungalVisionary.VisionLevel <= 0)
		{
			return true;
		}
		if (!ParentObject.PhaseAndFlightMatches(Object))
		{
			return true;
		}
		return false;
	}

	public bool WouldConsume(GameObject Object)
	{
		if (!ShouldIgnore(Object))
		{
			return CanConsume(Object);
		}
		return false;
	}

	public bool AnythingToConsume(Cell C)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (WouldConsume(@object))
			{
				return true;
			}
		}
		return false;
	}
}
