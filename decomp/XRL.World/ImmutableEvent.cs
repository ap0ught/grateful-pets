using System;

namespace XRL.World;

[Serializable]
public class ImmutableEvent : Event
{
	public ImmutableEvent(string _ID)
		: base(_ID)
	{
	}

	public ImmutableEvent(string _ID, int ObjParams, int StrParams, int IntParams)
		: base(_ID, ObjParams, StrParams, IntParams)
	{
	}

	public ImmutableEvent(string _ID, string ParameterName, string Parameter)
		: this(_ID, 0, 1, 0)
	{
		SetParameterInternal(ParameterName, Parameter);
	}

	public ImmutableEvent(string _ID, string ParameterName, int Parameter)
		: this(_ID, 0, 0, 1)
	{
		SetParameterInternal(ParameterName, Parameter);
	}

	public ImmutableEvent(string _ID, string ParameterName, object Parameter)
		: this(_ID, 1, 0, 0)
	{
		SetParameterInternal(ParameterName, Parameter);
	}

	public ImmutableEvent(string _ID, string ParameterName1, string Parameter1, string ParameterName2, object Parameter2)
		: this(_ID, 1, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, object Parameter1, string ParameterName2, string Parameter2)
		: this(_ID, 1, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, string Parameter1, string ParameterName2, string Parameter2)
		: this(_ID, 0, 2, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, int Parameter1, string ParameterName2, string Parameter2)
		: this(_ID, 0, 1, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, int Parameter1, string ParameterName2, object Parameter2)
		: this(_ID, 1, 0, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, object Parameter1, string ParameterName2, int Parameter2)
		: this(_ID, 1, 0, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, int Parameter1, string ParameterName2, int Parameter2)
		: this(_ID, 0, 0, 2)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, object Parameter1, string ParameterName2, object Parameter2)
		: this(_ID, 2, 0, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public ImmutableEvent(string _ID, string ParameterName1, object Parameter1, string ParameterName2, string Parameter2, string ParameterName3, object Parameter3)
		: this(_ID, 2, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public override void Clear()
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event SetParameter(string Name, object Value)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event SetParameter(string Name, string Value)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event SetParameter(string Name, int Value)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event AddParameter(string Name, string Value)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event AddParameter(string Name, int Value)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event AddParameter(string Name, object Value)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override Event SetSilent(bool Silent)
	{
		throw new Exception("Cannot modify immutable event");
	}

	public override void RequestInterfaceExit()
	{
		throw new Exception("Cannot modify immutable event");
	}
}
