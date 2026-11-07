using System;

namespace XRL.World;

/// <summary>Marks a class for generation of min event partials.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class GameEventAttribute : Attribute
{
	/// <summary>Seed for event's integer ID, defaults to class name.</summary>
	/// <remarks>This will be serialized for registrations and needs to be consistent across versions.</remarks>
	public string Seed;

	private int? _Cascade;

	/// <summary>Mark event as a base type for derived events.</summary>
	public bool Base;

	/// <summary>How to create instances of this event.</summary>
	public MinEvent.Cache Cache;

	/// <summary>The cascading depth of this event, defaults to the parent event's cascade level.</summary>
	public int Cascade
	{
		get
		{
			return _Cascade.GetValueOrDefault();
		}
		set
		{
			_Cascade = value;
		}
	}

	public bool HasCascade => _Cascade.HasValue;
}
