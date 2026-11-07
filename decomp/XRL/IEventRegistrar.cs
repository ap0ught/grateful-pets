using System;
using XRL.World;

namespace XRL;

/// <summary>Registers and unregisters event handlers to a source.</summary>
/// <remarks>This allows implementing a single method for both registration and unregistration.</remarks>
public interface IEventRegistrar
{
	/// <summary>Whether this instance will unregister provided events.</summary>
	bool IsUnregister { get; }

	/// <summary>Register a handler to receive a <see cref="T:XRL.World.MinEvent" /> from the specified source.</summary>
	void Register(IEventSource Source, IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false);

	/// <summary>Register this handler to receive a <see cref="T:XRL.World.MinEvent" /> from the specified source.</summary>
	void Register(IEventSource Source, int EventID, int Order = 0, bool Serialize = false);

	/// <summary>Register this handler to receive a <see cref="T:XRL.World.MinEvent" /> from the default source.</summary>
	void Register(int EventID, int Order = 0, bool Serialize = false);

	/// <summary>Register this handler to receive a <see cref="T:XRL.World.Event" /> from the default source.</summary>
	/// <remarks>It's recommended to avoid using string events where possible.</remarks>
	void Register(string EventID);

	[Obsolete("Use Register(string EventId)")]
	void RegisterPartEvent(IPart Ef, string Event)
	{
		Register(Event);
	}
}
