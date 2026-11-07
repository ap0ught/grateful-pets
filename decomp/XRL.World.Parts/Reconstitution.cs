using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Reconstitution : IPart
{
	/// <summary>A marker to drop on death which can in turn be destroyed to prevent reconstitution, defaults to invisible widget.</summary>
	public string Marker = "Widget";

	/// <summary>Comma separated filters to obliterate items from inventory on reconstitution.</summary>
	/// <example>Obliterate="Magnetic Bottle,Part:DataDisk,Tag:Golem,Inherits:Raw Meat"</example>
	public string Obliterate;

	/// <summary>The ID of the original object to retrieve from cache, or blueprint to create.</summary>
	/// <remarks>Do not set manually.</remarks>
	public string Object;

	/// <summary>Roll of time ticks before reconstitution is available.</summary>
	public string Turns;

	/// <summary>The specific XRLGame.TimeTicks to make reconstitution available.</summary>
	/// <remarks>Do not set manually.</remarks>
	public long CompleteTurn = -1L;

	/// <summary>Clear GameUnique.DeathState on reconstitution.</summary>
	public bool RemoveDeathState = true;

	/// <summary>Do not award additional XP after first death.</summary>
	public bool RemoveXP = true;

	/// <summary>Clear hostile memory on reconstitution.</summary>
	public bool StopFighting = true;

	/// <summary>Preserve and restore original game object, otherwise create new from blueprint.</summary>
	public bool Restore = true;

	/// <summary>Allow reconstituting on turn tick, with effects if the object is visible.</summary>
	public bool Tick;

	/// <summary>Message to display when dropping marker.</summary>
	public string DropMessage;

	/// <summary>Whether to use a popup for DropMessage.</summary>
	public bool DropMessageUsePopup;

	/// <summary>Whether to ignore visibility for DropMessage.</summary>
	public bool DropMessageAlwaysVisible;

	/// <summary>Activated reconstitution part on marker.</summary>
	/// <remarks>Do not set manually.</remarks>
	public bool Activated;

	[NonSerialized]
	private bool Destroyed;

	public override bool WantEvent(int ID, int cascade)
	{
		if (Activated)
		{
			if (ID != ZoneActivatedEvent.ID && ID != ZoneDeactivatedEvent.ID)
			{
				return ID == ZoneThawedEvent.ID;
			}
			return true;
		}
		if (ID == BeforeDestroyObjectEvent.ID)
		{
			return true;
		}
		return base.WantEvent(ID, cascade);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		TryReconstitute(Tick);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneDeactivatedEvent E)
	{
		TryReconstitute();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		TryReconstitute();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		if (Activated)
		{
			return Tick;
		}
		return false;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		TryReconstitute(WithEffect: true);
		base.TurnTick(TimeTick, Amount);
	}

	public bool TryReconstitute(bool WithEffect = false)
	{
		if (!Activated)
		{
			return false;
		}
		if (The.Game.TimeTicks < CompleteTurn)
		{
			return false;
		}
		if (Object.IsNullOrEmpty())
		{
			ParentObject.Obliterate(null, Silent: true);
			return false;
		}
		GameObject gameObject = (Restore ? The.ZoneManager.CachedObjects.PullValue(Object) : GameObjectFactory.Factory.CreateObject(Object));
		if (gameObject == null)
		{
			MetricsManager.LogError("Reconstitution", "No '" + Object + "' object found to " + (Restore ? "restore" : "create") + ".");
			return false;
		}
		if (gameObject.TryGetPart<GameUnique>(out var Part))
		{
			string stringGameState = The.Game.GetStringGameState(Part.State);
			if (stringGameState.IsNullOrEmpty() || stringGameState.HasDelimitedSubstring(',', "Dead"))
			{
				The.Game.SetStringGameState(Part.State, gameObject.ID);
			}
			if (RemoveDeathState && !Part.DeathState.IsNullOrEmpty())
			{
				The.Game.RemoveBooleanGameState(Part.DeathState);
			}
		}
		if (Restore)
		{
			gameObject.RestorePristineHealth();
			gameObject.FireEvent(Event.New("Regenera", "Level", 10, "Source", ParentObject));
			RepairedEvent.Send(ParentObject, gameObject, ParentObject);
			if (StopFighting)
			{
				gameObject.StopFighting();
			}
		}
		if (RemoveXP && gameObject.HasStat("XPValue"))
		{
			gameObject.Statistics.Remove("XPValue");
		}
		try
		{
			ParseObliterate(gameObject);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Reconstitution", x);
		}
		Object = null;
		ParentObject.CurrentCell.AddObject(gameObject);
		ParentObject.Obliterate(null, Silent: true);
		if (WithEffect && gameObject.IsVisible())
		{
			gameObject.DustPuff();
			string clip = gameObject.GetPropertyOrTag("AmbientIdleSound").Coalesce("Sounds/StatusEffects/sfx_statusEffect_budding");
			gameObject.PlayWorldSound(clip);
		}
		return true;
	}

	public void ParseObliterate(GameObject Object)
	{
		if (Obliterate.IsNullOrEmpty() || Object.Inventory == null)
		{
			return;
		}
		if (Obliterate == "*")
		{
			Object.Inventory.Clear();
			return;
		}
		string[] array = Obliterate.Split(',');
		foreach (string text in array)
		{
			string[] parts = text.Split(':');
			if (parts.Length > 1)
			{
				switch (parts[0])
				{
				case "Descends":
					Object.Inventory.RemoveAll((GameObject x) => x.GetBlueprint().DescendsFrom(parts[1]));
					break;
				case "Inherits":
					Object.Inventory.RemoveAll((GameObject x) => x.GetBlueprint().InheritsFrom(parts[1]));
					break;
				case "Tag":
					Object.Inventory.RemoveAll((GameObject x) => x.HasTag(parts[1]));
					break;
				case "Property":
					Object.Inventory.RemoveAll((GameObject x) => x.HasProperty(parts[1]));
					break;
				case "TagOrProperty":
					Object.Inventory.RemoveAll((GameObject x) => x.HasTagOrProperty(parts[1]));
					break;
				case "Part":
					Object.Inventory.RemoveAll((GameObject x) => x.HasPart(parts[1]));
					break;
				}
			}
			else if (parts.Length != 0)
			{
				Object.Inventory.RemoveAll((GameObject x) => x.Blueprint == parts[1]);
			}
		}
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		if (!Activated && !ParentObject.IsPlayer())
		{
			if (Destroyed && !E.Obliterate)
			{
				return !Restore;
			}
			Destroyed = true;
			DropMarker();
			if (Restore)
			{
				ParentObject.Physics.TeardownForDestroy(MoveToGraveyard: false, Silent: true);
				The.ZoneManager.CachedObjects[ParentObject.ID] = ParentObject;
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public void DropMarker(Cell Cell = null)
	{
		GameObject gameObject = GameObject.Create(Marker);
		if ((gameObject.Render == null || !gameObject.Render.Visible) && Cell == null)
		{
			Cell = ParentObject.Brain?.StartingCell?.ResolveCell();
		}
		Reconstitution reconstitution = (Reconstitution)gameObject.AddPart(DeepCopy(gameObject));
		reconstitution.Activated = true;
		reconstitution.Object = (Restore ? ParentObject.ID : ParentObject.Blueprint);
		reconstitution.CompleteTurn = (Turns.IsNullOrEmpty() ? (-1) : (The.Game.TimeTicks + Stat.Roll(Turns)));
		if (Cell == null)
		{
			Cell = ParentObject.CurrentCell;
		}
		Cell.AddObject(gameObject);
		if (!DropMessage.IsNullOrEmpty())
		{
			string text = GameText.VariableReplace(DropMessage, ParentObject, gameObject);
			if (!text.IsNullOrEmpty())
			{
				EmitMessage(text, ' ', FromDialog: false, DropMessageUsePopup, DropMessageAlwaysVisible);
			}
		}
	}
}
