using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class PlayerDanceRitual : IPart
{
	public int n;

	public int Approval;

	public string CurrentLeader = "";

	public string CurrentState = "";

	public List<string> PlayerMovementLog = new List<string>();

	public List<string> OpponentMovementLog = new List<string>();

	public int TurnsLeft;

	public int StepsFailed;

	public int StepsPassed;

	[NonSerialized]
	private Dictionary<string, RitualTypeEntry> _RitualTypes;

	[NonSerialized]
	private List<RitualTypeEntry> _Rituals;

	[NonSerialized]
	private GameObject _Opponent;

	public List<RitualTypeEntry> Rituals
	{
		get
		{
			if (_Rituals == null)
			{
				InitRituals();
			}
			return _Rituals;
		}
	}

	public Dictionary<string, RitualTypeEntry> RitualTypes
	{
		get
		{
			if (_RitualTypes == null)
			{
				InitRituals();
			}
			return _RitualTypes;
		}
	}

	private GameObject Opponent
	{
		get
		{
			if (_Opponent == null)
			{
				_Opponent = ParentObject.Physics.CurrentCell.ParentZone.FindObject("Angor");
			}
			return _Opponent;
		}
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public void InitRituals()
	{
		_RitualTypes = new Dictionary<string, RitualTypeEntry>();
		Random randomGenerator = new Random(XRLCore.Core.Game.GetWorldSeed("DanceRitual"));
		List<string> list = new List<string>();
		list.Add("B");
		list.Add("R");
		list.Add("C");
		list.Add("W");
		list.Add("Y");
		list.Add("K");
		list.Add("M");
		list.Add("b");
		list.Add("r");
		list.Add("c");
		list.Add("w");
		list.Add("y");
		list.Add("g");
		list.Add("m");
		Algorithms.RandomShuffleInPlace(list, randomGenerator);
		_Rituals = new List<RitualTypeEntry>();
		_Rituals.Add(new RitualTypeEntry("Attack", list[0]));
		_Rituals.Add(new RitualTypeEntry("Mimic", list[1]));
		_Rituals.Add(new RitualTypeEntry("Mirror", list[2]));
		for (int i = 0; i < _Rituals.Count; i++)
		{
			_RitualTypes.Add(_Rituals[i].Name, _Rituals[i]);
		}
	}

	public void ExecuteMove(string Actor, string Direction)
	{
		MessageQueue.AddPlayerMessage("&K" + Actor + " steps " + Direction);
		if (Actor == "Opponent")
		{
			OpponentMovementLog.Add(Direction);
		}
		if (Actor == "Player")
		{
			PlayerMovementLog.Add(Direction);
		}
	}

	public void EndTurn(string Actor)
	{
	}

	public void PassStep(string Reason = "")
	{
		MessageQueue.AddPlayerMessage("&GYou executed that step correctly! [" + Reason + "]");
		StepsPassed++;
	}

	public void FailStep(string Reason = "")
	{
		MessageQueue.AddPlayerMessage("&RYou executed that step incorrectly! [" + Reason + "]");
		StepsFailed++;
	}

	public void FailDance(string Reason = "")
	{
		Popup.Show("The dance ended in failure! [" + Reason + "]");
		Opponent.RemovePart<DanceRitualOpponent>();
		ParentObject.RemovePart(this);
	}

	public void SuccessDance(string Reason = "")
	{
		Popup.Show("The dance ended in success! [" + Reason + "]");
		Opponent.RemovePart<DanceRitualOpponent>();
		ParentObject.RemovePart(this);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeTakeAction");
		Registrar.Register("AfterMoved");
		Registrar.Register("CanHaveConversation");
		Registrar.Register("EndAction");
		Registrar.Register("EndTurn");
		Registrar.Register("DanceOpponentDied");
		if (Opponent == null)
		{
			FailDance("Angor couldn't be found here");
		}
		else
		{
			Opponent.AddPart(new DanceRitualOpponent());
		}
		CurrentLeader = "Angor";
		CurrentState = "#MakeChoice";
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndAction")
		{
			ParentObject.Energy.BaseValue = 999;
			Opponent.Energy.BaseValue = 1001;
			return true;
		}
		if (E.ID == "BeforeTakeAction")
		{
			return true;
		}
		if (E.ID == "AfterMoved")
		{
			ExecuteMove("Player", E.GetStringParameter("Direction"));
			return true;
		}
		if (E.ID == "DanceOpponentDied")
		{
			SuccessDance("Opponent died!");
			return true;
		}
		if (E.ID == "DanceOpponentDied")
		{
			SuccessDance("Your opponent perished!");
			return true;
		}
		if (E.ID == "EndTurn")
		{
			EndTurn("Player");
			MessageQueue.AddPlayerMessage("&KDebug: Dance party turn tick " + n + " Current Approval:" + Approval);
			n++;
			if (ParentObject.Physics.CurrentCell.ParentZone.FindObject("Angor") == null)
			{
				FailDance("You left the dance early!");
				return true;
			}
			return true;
		}
		return true;
	}
}
