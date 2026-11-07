using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EelSpawn : IPart
{
	public override bool SameAs(IPart p)
	{
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.Render.CustomRender = true;
		ParentObject.Render.Visible = false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object.Blueprint != "SewageEel" && E.Object.IsCombatObject() && ParentObject.CurrentCell != null && ParentObject.CurrentCell.HasWadingDepthLiquid() && 50.in100())
		{
			foreach (Cell item in new List<Cell>(ParentObject.CurrentCell.GetAdjacentCells()).ShuffleInPlace())
			{
				if (!item.HasWadingDepthLiquid())
				{
					continue;
				}
				GameObject gameObject = GameObject.Create("SewageEel");
				gameObject.MakeActive();
				item.AddObject(gameObject);
				if (gameObject.IsHostileTowards(E.Object))
				{
					BodyPart bodyPart = E.Object.GetFirstBodyPart("Feet") ?? E.Object.GetFirstBodyPart("Roots");
					string text = ((bodyPart == null) ? " around you" : (" around your " + bodyPart.GetOrdinalName()));
					gameObject.Brain.Target = E.Object;
					if (!gameObject.FlightMatches(E.Object))
					{
						if (E.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("A sewage eel tries to wrap itself" + text + ", but cannot reach!");
						}
					}
					else if (!gameObject.PhaseMatches(E.Object))
					{
						if (E.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("A sewage eel tries to wrap itself" + text + ", but passes through you!");
						}
					}
					else
					{
						if (E.Object.IsPlayer())
						{
							Popup.Show("A sewage eel wraps itself" + text + "!");
						}
						if (!E.Object.MakeSave("Agility", 16, null, null, "EelSpawn Knockdown", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, gameObject))
						{
							if (E.Object.IsPlayer())
							{
								E.Object.ApplyEffect(new Prone());
							}
						}
						else if (E.Object.IsPlayer())
						{
							if (bodyPart != null && bodyPart.Type == "Feet" && E.Object.CanMoveExtremities())
							{
								Popup.Show("You maintain your balance and kick the eel away.");
							}
							else
							{
								Popup.Show("You maintain your balance and shake the eel off.");
							}
						}
					}
				}
				else if (IComponent<GameObject>.Visible(gameObject))
				{
					IComponent<GameObject>.XDidY(IComponent<GameObject>.ThePlayer, "spot", "a sewage eel " + IComponent<GameObject>.ThePlayer.DescribeDirectionToward(gameObject), "!", null, null, IComponent<GameObject>.ThePlayer);
				}
				ParentObject.Obliterate();
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CustomRender");
		Registrar.Register("Searched");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CustomRender")
		{
			if (E.GetParameter("RenderEvent") is RenderEvent renderEvent && (renderEvent.Lit == LightLevel.Radar || renderEvent.Lit == LightLevel.LitRadar))
			{
				Reveal();
			}
		}
		else if (E.ID == "Searched")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Searcher");
			if (gameObjectParameter.CurrentCell != ParentObject.CurrentCell && ParentObject.CurrentCell.HasWadingDepthLiquid() && E.GetIntParameter("Bonus") + Stat.Random(1, gameObjectParameter.Stat("Intelligence")) >= 16)
			{
				Reveal(gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}

	public void Reveal(GameObject who = null)
	{
		if (who == null)
		{
			who = IComponent<GameObject>.ThePlayer;
		}
		if (who != null)
		{
			IComponent<GameObject>.XDidY(who, "spot", "a sewage eel " + who.DescribeDirectionToward(ParentObject), "!", null, null, who);
		}
		GameObject gameObject = GameObject.Create("SewageEel");
		gameObject.MakeActive();
		ParentObject.CurrentCell.AddObject(gameObject);
		if (IComponent<GameObject>.Visible(gameObject) && AutoAct.IsInterruptable() && IComponent<GameObject>.ThePlayer.IsRelevantHostile(gameObject))
		{
			AutoAct.Interrupt(null, null, gameObject, IsThreat: true);
		}
		ParentObject.Destroy();
	}
}
