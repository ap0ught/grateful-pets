using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CryoZone : IPart
{
	public GameObject Owner;

	public int FrameOffset = Stat.RandomCosmetic(0, 60);

	public int Duration = 3;

	public int Turn = 1;

	public int Level = 1;

	public int GroupID;

	public bool Control;

	public string DependsOn;

	public string StartMessage = "The air =subject.generalDirectionIfAny= bursts into a field of frigid mist!";

	public string StopMessage = "The frigid mist =subject.generalDirectionIfAny= dissipates.";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == SingletonEvent<GeneralAmnestyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.Validate(ref Owner);
		if (Duration > 0)
		{
			DieRoll cachedDieRoll = (Level + "d" + Cryokinesis.GetDamageDieSize(Turn)).GetCachedDieRoll();
			int phase = ParentObject.GetPhase();
			foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Physics"))
			{
				if (item != ParentObject)
				{
					item.TakeDamage((int)Math.Ceiling((double)cachedDieRoll.Resolve() / 2.0), "from %t cryokinesis!", "Ice Cold", null, null, null, Owner, null, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, UsePopups: false, phase);
					item.TemperatureChange((-20 - 60 * Level) / 2, Owner);
				}
			}
			ParentObject.TemperatureChange((-20 - 60 * Level) / 2, Owner);
			PlayWorldSound("Sounds/Abilities/sfx_ability_cryokinesis_passive");
			Turn++;
		}
		if (Duration != 9999)
		{
			Duration--;
		}
		if (Duration <= 0 || !CheckDependsOn())
		{
			if (Control)
			{
				Stopped();
				foreach (GameObject item2 in GetGroup())
				{
					item2.Destroy(null, Silent: true);
				}
				return false;
			}
			if (GetControl() == null)
			{
				ParentObject.Destroy(null, Silent: true);
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (!Visible())
		{
			return true;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
		if (num < 15)
		{
			E.BackgroundString = "^C";
			E.DetailColor = "C";
		}
		else if (num < 30)
		{
			E.BackgroundString = "^c";
			E.DetailColor = "c";
		}
		else if (num < 45)
		{
			E.BackgroundString = "^Y";
			E.DetailColor = "Y";
		}
		else
		{
			E.BackgroundString = "^c";
			E.DetailColor = "c";
		}
		if (Stat.RandomCosmetic(1, 5) == 1)
		{
			E.RenderString = "°";
			E.ColorString = "&C";
		}
		else if (Stat.RandomCosmetic(1, 5) == 1)
		{
			E.RenderString = "±";
			E.ColorString = "&Y";
		}
		else
		{
			E.RenderString = "±";
			E.ColorString = "&c";
		}
		ParentObject.Render.ColorString = "&Y^C";
		return base.FinalRender(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
		if (num < 15)
		{
			E.RenderString = "°";
			E.BackgroundString = "^C";
		}
		else if (num < 30)
		{
			E.RenderString = "±";
			E.BackgroundString = "^c";
		}
		else if (num < 45)
		{
			E.RenderString = "²";
			E.BackgroundString = "^Y";
		}
		else
		{
			E.RenderString = "Û";
			E.BackgroundString = "^C";
		}
		ParentObject.Render.ColorString = "&Y^C";
		return true;
	}

	public bool AnyOtherVisibleInGroup()
	{
		Zone activeZone = The.ZoneManager.ActiveZone;
		if (activeZone != null)
		{
			for (int i = 0; i < activeZone.Height; i++)
			{
				for (int j = 0; j < activeZone.Width; j++)
				{
					Cell cell = activeZone.GetCell(j, i);
					if (cell == null)
					{
						continue;
					}
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						GameObject gameObject = cell.Objects[k];
						if (gameObject.Blueprint == ParentObject.Blueprint && gameObject.TryGetPart<CryoZone>(out var Part) && Part.GroupID == GroupID && gameObject.IsVisible())
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool AnyOtherInGroupInSameCellAs(GameObject Object)
	{
		if (GameObject.Validate(ref Object))
		{
			Zone activeZone = The.ZoneManager.ActiveZone;
			if (activeZone != null)
			{
				for (int i = 0; i < activeZone.Height; i++)
				{
					for (int j = 0; j < activeZone.Width; j++)
					{
						Cell cell = activeZone.GetCell(j, i);
						if (cell == null)
						{
							continue;
						}
						int k = 0;
						for (int count = cell.Objects.Count; k < count; k++)
						{
							GameObject gameObject = cell.Objects[k];
							if (gameObject.Blueprint == ParentObject.Blueprint && gameObject.TryGetPart<CryoZone>(out var Part) && Part.GroupID == GroupID && gameObject.InSameCellAs(Object))
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	public List<GameObject> GetGroup()
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (Zone value in The.ZoneManager.CachedZones.Values)
		{
			for (int i = 0; i < value.Height; i++)
			{
				for (int j = 0; j < value.Width; j++)
				{
					Cell cell = value.GetCell(j, i);
					if (cell == null)
					{
						continue;
					}
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						GameObject gameObject = cell.Objects[k];
						if (gameObject.Blueprint == ParentObject.Blueprint && gameObject.TryGetPart<CryoZone>(out var Part) && Part.GroupID == GroupID)
						{
							list.Add(gameObject);
						}
					}
				}
			}
		}
		return list;
	}

	public GameObject GetVisibleInGroupClosestTo(GameObject Object)
	{
		if (GameObject.Validate(ref Object))
		{
			GameObject gameObject = null;
			int num = 0;
			Zone currentZone = Object.CurrentZone;
			if (currentZone != null)
			{
				for (int i = 0; i < currentZone.Height; i++)
				{
					for (int j = 0; j < currentZone.Width; j++)
					{
						Cell cell = currentZone.GetCell(j, i);
						if (cell == null)
						{
							continue;
						}
						int k = 0;
						for (int count = cell.Objects.Count; k < count; k++)
						{
							GameObject gameObject2 = cell.Objects[k];
							if (gameObject2.Blueprint == ParentObject.Blueprint && gameObject2.TryGetPart<CryoZone>(out var Part) && Part.GroupID == GroupID && gameObject2.IsVisible())
							{
								int num2 = gameObject2.DistanceTo(Object);
								if (gameObject == null || num2 < num)
								{
									gameObject = gameObject2;
									num = num2;
								}
							}
						}
					}
				}
			}
		}
		return null;
	}

	public GameObject GetVisibleGroupMemberInSameCellAs(GameObject Object)
	{
		if (GameObject.Validate(ref Object))
		{
			Cell cell = Object.CurrentCell;
			if (cell != null)
			{
				int i = 0;
				for (int count = cell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = cell.Objects[i];
					if (gameObject.Blueprint == ParentObject.Blueprint && gameObject.TryGetPart<CryoZone>(out var Part) && Part.GroupID == GroupID && IComponent<GameObject>.Visible(gameObject))
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public GameObject GetControl()
	{
		if (Control)
		{
			return ParentObject;
		}
		foreach (Zone value in The.ZoneManager.CachedZones.Values)
		{
			for (int i = 0; i < value.Height; i++)
			{
				for (int j = 0; j < value.Width; j++)
				{
					Cell cell = value.GetCell(j, i);
					if (cell == null)
					{
						continue;
					}
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						GameObject gameObject = cell.Objects[k];
						if (gameObject.Blueprint == ParentObject.Blueprint && gameObject.TryGetPart<CryoZone>(out var Part) && Part.GroupID == GroupID && Part.Control)
						{
							return gameObject;
						}
					}
				}
			}
		}
		return null;
	}

	public GameObject GetControlIfVisible()
	{
		GameObject control = GetControl();
		if (control != null && IComponent<GameObject>.Visible(control))
		{
			return control;
		}
		return null;
	}

	public GameObject GetDescriptionReference()
	{
		return GetVisibleGroupMemberInSameCellAs(The.Player) ?? GetControlIfVisible() ?? GetVisibleInGroupClosestTo(The.Player);
	}

	public void Started()
	{
		if (StartMessage.IsNullOrEmpty())
		{
			return;
		}
		GameObject descriptionReference = GetDescriptionReference();
		if (descriptionReference != null)
		{
			string text = GameText.VariableReplace(StartMessage, descriptionReference);
			if (!text.IsNullOrEmpty())
			{
				IComponent<GameObject>.AddPlayerMessage(text);
			}
		}
	}

	public void Stopped()
	{
		if (StopMessage.IsNullOrEmpty())
		{
			return;
		}
		GameObject descriptionReference = GetDescriptionReference();
		if (descriptionReference != null)
		{
			string text = GameText.VariableReplace(StopMessage, descriptionReference);
			if (!text.IsNullOrEmpty())
			{
				IComponent<GameObject>.AddPlayerMessage(text);
			}
		}
	}

	public bool CheckDependsOn()
	{
		if (DependsOn.IsNullOrEmpty())
		{
			return true;
		}
		GameObject gameObject = GameObject.FindByID(DependsOn);
		if (gameObject != null)
		{
			return !gameObject.IsNowhere();
		}
		return false;
	}
}
