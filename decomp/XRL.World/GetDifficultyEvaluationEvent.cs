using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class GetDifficultyEvaluationEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetDifficultyEvaluationEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<GetDifficultyEvaluationEvent> Pool;

	private static int PoolCounter;

	public GameObject Subject;

	public GameObject Actor;

	public string SubjectRole;

	public string ActorRole;

	public int SubjectLevel;

	public int ActorLevel;

	public int SubjectEffectiveLevel;

	public int ActorEffectiveLevel;

	public int BaseRating;

	public int Rating;

	public int MinRating = int.MinValue;

	public int MaxRating = int.MaxValue;

	public GetDifficultyEvaluationEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref GetDifficultyEvaluationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetDifficultyEvaluationEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Subject = null;
		Actor = null;
		SubjectRole = null;
		ActorRole = null;
		SubjectLevel = 0;
		ActorLevel = 0;
		SubjectEffectiveLevel = 0;
		ActorEffectiveLevel = 0;
		BaseRating = 0;
		Rating = 0;
		MinRating = int.MinValue;
		MaxRating = int.MaxValue;
	}

	public void MinimumRating(int Value)
	{
		if (MinRating < Value)
		{
			MinRating = Value;
		}
		if (Rating < Value)
		{
			Rating = Value;
		}
	}

	public void MaximumRating(int Value)
	{
		if (MaxRating > Value)
		{
			MaxRating = Value;
		}
		if (Rating > Value)
		{
			Rating = Value;
		}
	}

	public static int GetFor(GameObject Subject, GameObject Actor, string SubjectRole, string ActorRole, int SubjectLevel, int ActorLevel, int SubjectEffectiveLevel, int ActorEffectiveLevel, int BaseRating, int MinRating = int.MinValue, int MaxRating = int.MaxValue)
	{
		int num = BaseRating;
		bool flag = true;
		if (flag && GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("GetDifficultyEvaluation"))
		{
			Event obj = Event.New("GetDifficultyEvaluation");
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("SubjectRole", SubjectRole);
			obj.SetParameter("ActorRole", ActorRole);
			obj.SetParameter("SubjectLevel", SubjectLevel);
			obj.SetParameter("ActorLevel", ActorLevel);
			obj.SetParameter("SubjectEffectiveLevel", SubjectEffectiveLevel);
			obj.SetParameter("ActorEffectiveLevel", ActorEffectiveLevel);
			obj.SetParameter("BaseRating", BaseRating);
			obj.SetParameter("Rating", num);
			obj.SetParameter("MinRating", MinRating);
			obj.SetParameter("MaxRating", MaxRating);
			flag = Subject.FireEvent(obj);
			num = obj.GetIntParameter("Rating");
			MinRating = obj.GetIntParameter("MinRating");
			MaxRating = obj.GetIntParameter("MaxRating");
		}
		if (flag && GameObject.Validate(ref Subject) && Subject.WantEvent(ID, CascadeLevel))
		{
			GetDifficultyEvaluationEvent getDifficultyEvaluationEvent = FromPool();
			getDifficultyEvaluationEvent.Subject = Subject;
			getDifficultyEvaluationEvent.Actor = Actor;
			getDifficultyEvaluationEvent.SubjectRole = SubjectRole;
			getDifficultyEvaluationEvent.ActorRole = ActorRole;
			getDifficultyEvaluationEvent.SubjectLevel = SubjectLevel;
			getDifficultyEvaluationEvent.ActorLevel = ActorLevel;
			getDifficultyEvaluationEvent.SubjectEffectiveLevel = SubjectEffectiveLevel;
			getDifficultyEvaluationEvent.ActorEffectiveLevel = ActorEffectiveLevel;
			getDifficultyEvaluationEvent.BaseRating = BaseRating;
			getDifficultyEvaluationEvent.Rating = num;
			getDifficultyEvaluationEvent.MinRating = MinRating;
			getDifficultyEvaluationEvent.MaxRating = MaxRating;
			flag = Subject.HandleEvent(getDifficultyEvaluationEvent);
			num = getDifficultyEvaluationEvent.Rating;
			MinRating = getDifficultyEvaluationEvent.MinRating;
			MaxRating = getDifficultyEvaluationEvent.MaxRating;
		}
		return Math.Max(Math.Min(num, MaxRating), MinRating);
	}
}
