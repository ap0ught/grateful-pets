using System.Text;

namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IShortDescriptionEvent : MinEvent
{
	public GameObject Object;

	public StringBuilder Base = new StringBuilder(512);

	public StringBuilder Prefix = new StringBuilder(512);

	public StringBuilder Infix = new StringBuilder(512);

	public StringBuilder Postfix = new StringBuilder(512);

	public string Context;

	public bool AsIfKnown;

	public bool NoStatus;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Base.Clear();
		Prefix.Clear();
		Infix.Clear();
		Postfix.Clear();
		Context = null;
		AsIfKnown = false;
		NoStatus = false;
	}

	public void ApplyTo(IShortDescriptionEvent E)
	{
		E.Object = Object;
		if (E.Base == null)
		{
			E.Base = new StringBuilder(512);
		}
		E.Base.Clear().Append(Base);
		if (E.Prefix == null)
		{
			E.Prefix = new StringBuilder(512);
		}
		E.Prefix.Clear().Append(Prefix);
		if (E.Infix == null)
		{
			E.Infix = new StringBuilder(512);
		}
		E.Infix.Clear().Append(Infix);
		if (E.Postfix == null)
		{
			E.Postfix = new StringBuilder(512);
		}
		E.Postfix.Clear().Append(Postfix);
		E.Context = Context;
		E.AsIfKnown = AsIfKnown;
	}

	public bool Process(GameObject Handler, IShortDescriptionEvent ParentEvent = null)
	{
		bool flag = false;
		try
		{
			string registeredEventID = GetRegisteredEventID();
			if (!string.IsNullOrEmpty(registeredEventID) && Handler.HasRegisteredEvent(registeredEventID))
			{
				if (!flag && ParentEvent != null)
				{
					ParentEvent.ApplyTo(this);
					flag = true;
				}
				string text = Base.ToString();
				string text2 = Prefix.ToString();
				string text3 = Infix.ToString();
				string text4 = Postfix.ToString();
				Event obj = Event.New(registeredEventID);
				obj.SetParameter("Object", Object);
				obj.SetParameter("Handler", Handler);
				obj.SetParameter("ShortDescription", text);
				obj.SetParameter("BaseBuilder", Base);
				obj.SetParameter("Prefix", text2);
				obj.SetParameter("PrefixBuilder", Prefix);
				obj.SetParameter("Infix", text3);
				obj.SetParameter("InfixBuilder", Infix);
				obj.SetParameter("Postfix", text4);
				obj.SetParameter("PostfixBuilder", Postfix);
				obj.SetParameter("Context", Context);
				obj.SetFlag("AsIfKnown", AsIfKnown);
				bool flag2 = Handler.FireEvent(obj);
				string stringParameter = obj.GetStringParameter("Prefix");
				if (stringParameter != text2)
				{
					Prefix.Clear().Append(stringParameter);
				}
				string stringParameter2 = obj.GetStringParameter("ShortDescription");
				if (stringParameter2 != text)
				{
					Base.Clear().Append(stringParameter2);
				}
				string stringParameter3 = obj.GetStringParameter("Infix");
				if (stringParameter3 != text3)
				{
					Infix.Clear().Append(stringParameter3);
				}
				string stringParameter4 = obj.GetStringParameter("Postfix");
				if (stringParameter4 != text4)
				{
					Postfix.Clear().Append(stringParameter4);
				}
				if (!flag2)
				{
					return false;
				}
			}
			if (Handler.WantEvent(GetID(), GetCascadeLevel()))
			{
				if (!flag && ParentEvent != null)
				{
					ParentEvent.ApplyTo(this);
					flag = true;
				}
				if (!Handler.HandleEvent(this))
				{
					return false;
				}
			}
		}
		finally
		{
			if (flag)
			{
				ApplyTo(ParentEvent);
			}
		}
		return true;
	}

	public bool Understood()
	{
		if (!AsIfKnown)
		{
			return Object.Understood();
		}
		return true;
	}

	public abstract string GetRegisteredEventID();
}
