using System;
using ConsoleLib.Console;
using Genkit;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Interesting : IPart
{
	public string DisplayName;

	public string Explanation;

	public string IconTile;

	public string IconRenderString = " ";

	public string IconColorString = "";

	public string IconTileColor;

	public string IconDetailColor;

	public string IfPartOperational;

	public string Key;

	public string Preposition;

	public int Order;

	public int X;

	public int Y;

	public int Radius = -1;

	public bool EvenIfUnknown;

	public bool EvenIfHostile;

	public bool EvenIfCompanion;

	public bool EvenIfInvisible;

	public bool EvenIfUnexplored;

	public bool NearestKeyOnly = true;

	public bool TranslateToLocation;

	public bool UseBaseDisplayNameAsKey;

	public bool KeepOnReplicas;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID)
		{
			if (ID == ReplicaCreatedEvent.ID)
			{
				return !KeepOnReplicas;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "DisplayName", DisplayName);
		E.AddEntry(this, "Explanation", Explanation);
		E.AddEntry(this, "IconTile", IconTile);
		E.AddEntry(this, "IconRenderString", IconRenderString);
		E.AddEntry(this, "IconColorString", IconColorString);
		E.AddEntry(this, "IconTileColor", IconTileColor);
		E.AddEntry(this, "IconDetailColor", IconDetailColor);
		E.AddEntry(this, "IfPartOperational", IfPartOperational);
		E.AddEntry(this, "Key", Key);
		E.AddEntry(this, "Preposition", Preposition);
		E.AddEntry(this, "Order", Order);
		E.AddEntry(this, "X", X);
		E.AddEntry(this, "Y", Y);
		E.AddEntry(this, "Radius", Radius);
		E.AddEntry(this, "EvenIfUnknown", EvenIfUnknown);
		E.AddEntry(this, "EvenIfHostile", EvenIfHostile);
		E.AddEntry(this, "EvenIfCompanion", EvenIfCompanion);
		E.AddEntry(this, "EvenIfInvisible", EvenIfInvisible);
		E.AddEntry(this, "EvenIfUnexplored", EvenIfUnexplored);
		E.AddEntry(this, "NearestKeyOnly", NearestKeyOnly);
		E.AddEntry(this, "TranslateToLocation", TranslateToLocation);
		E.AddEntry(this, "UseBaseDisplayNameAsKey", UseBaseDisplayNameAsKey);
		E.AddEntry(this, "KeepOnReplicas", KeepOnReplicas);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (RequirementsMet(E.Actor))
		{
			bool flag = true;
			string text = Explanation;
			string text2 = (UseBaseDisplayNameAsKey ? ("Interesting " + ParentObject.BaseDisplayName) : Key);
			if (NearestKeyOnly && !string.IsNullOrEmpty(text2))
			{
				PointOfInterest pointOfInterest = E.Find(text2);
				if (pointOfInterest != null)
				{
					if (pointOfInterest.Object == ParentObject)
					{
						E.Remove(pointOfInterest);
					}
					else if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
					{
						E.Remove(pointOfInterest);
						text = (string.IsNullOrEmpty(text) ? "nearest" : (text + ", nearest"));
					}
					else
					{
						flag = false;
						if (!string.IsNullOrEmpty(pointOfInterest.Explanation))
						{
							if (!pointOfInterest.Explanation.Contains("nearest"))
							{
								pointOfInterest.Explanation += ", nearest";
							}
						}
						else
						{
							pointOfInterest.Explanation = "nearest";
						}
					}
				}
			}
			else
			{
				PointOfInterest pointOfInterest2 = E.Find(ParentObject);
				if (pointOfInterest2 != null)
				{
					E.Remove(pointOfInterest2);
				}
			}
			if (flag)
			{
				Renderable renderable = null;
				if (!string.IsNullOrEmpty(IconTile))
				{
					renderable = new Renderable();
					renderable.Tile = IconTile;
					renderable.RenderString = IconRenderString;
					renderable.ColorString = IconColorString;
					if (!string.IsNullOrEmpty(IconTileColor))
					{
						renderable.TileColor = IconTileColor;
					}
					if (!string.IsNullOrEmpty(IconDetailColor))
					{
						renderable.DetailColor = IconDetailColor[0];
					}
				}
				GameObject gameObject = ParentObject;
				Location2D location = null;
				if (TranslateToLocation)
				{
					location = ParentObject.CurrentCell.Location;
					gameObject = null;
				}
				else if (X != 0 && Y != 0)
				{
					location = Location2D.Get(X, Y);
				}
				E.Add(gameObject, DisplayName ?? ParentObject.GetReferenceDisplayName(), text, text2, Preposition, location, null, Radius, renderable, Order);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (!KeepOnReplicas && E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public bool RequirementsMet(GameObject who)
	{
		if (ParentObject == who)
		{
			return false;
		}
		if (!EvenIfUnexplored && !ParentObject.CurrentCell.IsExplored())
		{
			return false;
		}
		if (!EvenIfInvisible)
		{
			Render render = ParentObject.Render;
			if (render == null || !render.Visible)
			{
				return false;
			}
		}
		if (!EvenIfUnknown && !ParentObject.Understood())
		{
			return false;
		}
		if (!EvenIfHostile && ParentObject.IsHostileTowards(who))
		{
			return false;
		}
		if (!EvenIfCompanion && ParentObject.IsLedBy(who))
		{
			return false;
		}
		if (ParentObject.HasPart<FungalVision>() && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(IfPartOperational) && (!(ParentObject.GetPart(IfPartOperational) is IActivePart activePart) || activePart.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
		{
			return false;
		}
		return true;
	}
}
