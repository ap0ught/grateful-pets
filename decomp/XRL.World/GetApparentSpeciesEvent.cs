namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetApparentSpeciesEvent : PooledEvent<GetApparentSpeciesEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject Viewer;

	public string Species;

	public string ApparentSpecies;

	public int Priority;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Viewer = null;
		Species = null;
		ApparentSpecies = null;
		Priority = 0;
	}

	public static string GetFor(GameObject Object, GameObject Viewer = null, string Species = null)
	{
		if (Species == null)
		{
			Species = Object?.GetSpecies();
		}
		string text = Species;
		int num = 0;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetApparentSpecies"))
		{
			Event obj = Event.New("GetApparentSpecies");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Viewer", Viewer);
			obj.SetParameter("Species", Species);
			obj.SetParameter("ApparentSpecies", text);
			obj.SetParameter("Priority", num);
			flag = Object.FireEvent(obj);
			text = obj.GetStringParameter("ApparentSpecies");
			num = obj.GetIntParameter("Priority");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetApparentSpeciesEvent>.ID, CascadeLevel))
		{
			GetApparentSpeciesEvent getApparentSpeciesEvent = PooledEvent<GetApparentSpeciesEvent>.FromPool();
			getApparentSpeciesEvent.Object = Object;
			getApparentSpeciesEvent.Viewer = Viewer;
			getApparentSpeciesEvent.Species = Species;
			getApparentSpeciesEvent.ApparentSpecies = text;
			getApparentSpeciesEvent.Priority = num;
			flag = Object.HandleEvent(getApparentSpeciesEvent);
			text = getApparentSpeciesEvent.ApparentSpecies;
			num = getApparentSpeciesEvent.Priority;
		}
		return text;
	}
}
