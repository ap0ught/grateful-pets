using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class ModQuantumReverb : IModification
{
	public ModQuantumReverb()
	{
	}

	public ModQuantumReverb(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		IsTechScannable = true;
		WorksOnSelf = true;
		NameForStatus = "QuantumReverb";
	}

	public static GameObject CreateHologramOf(GameObject Object)
	{
		GameObject gameObject = GameObject.Create("Hologram Distraction");
		gameObject.Render.Tile = Object.Render.Tile;
		gameObject.Render.RenderString = Object.Render.RenderString;
		gameObject.Render.DisplayName = "hologram of " + Object.an();
		gameObject.GetPart<Description>().Short = "Light stammers in parallax to form the image of an object. " + Object.GetPart<Description>().Short;
		gameObject.RemovePart<Distraction>();
		return gameObject;
	}

	public void PlaceHologram(GameObject Owner, Cell Target)
	{
		GameObject gameObject = CreateHologramOf(Owner);
		gameObject.RequirePart<Inventory>();
		Body body = gameObject.RequirePart<Body>();
		body.built = false;
		body._Body = Owner.Body._Body.DeepCopy(gameObject, body, null, null, CopyGameObjects: false);
		body.built = true;
		body.CategorizeAll(22);
		body.UpdateBodyParts();
		GameObject gameObject2 = ParentObject.DeepCopy();
		BodyPart firstPart = body.GetFirstPart("Missile Weapon");
		gameObject2.Physics._Equipped = gameObject;
		firstPart.DoEquip(gameObject2);
		AIShootRound aIShootRound = gameObject.RequirePart<AIShootRound>();
		aIShootRound.SetTarget(Target);
		aIShootRound.Cooldown = 1;
		Temporary.AddHierarchically(gameObject, 4);
		Owner.CurrentCell.AddObject(gameObject);
		gameObject.Physics.DidX("appear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{quantumreverb|quantum reverb}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandFireMissile");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFireMissile")
		{
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			Cell cell = E.GetParameter("TargetCell") as Cell;
			if (ParentObject.IsTemporary || !GameObject.Validate(gameObject) || gameObject.IsTemporary || cell == null)
			{
				return true;
			}
			PlaceHologram(gameObject, cell);
		}
		return base.FireEvent(E);
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(3, 3);
	}

	public static string GetDescription(int Tier)
	{
		return "Quantum reverb: When fired, this weapon creates a hologram of its wielder who continues to fire along the same path.";
	}
}
