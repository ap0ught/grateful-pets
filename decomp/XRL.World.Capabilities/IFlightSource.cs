using System;

namespace XRL.World.Capabilities;

public interface IFlightSource
{
	int FlightLevel { get; }

	int FlightBaseFallChance { get; }

	bool FlightRequiresOngoingEffort { get; }

	string FlightEvent { get; }

	string FlightActivatedAbilityClass { get; }

	string FlightSourceDescription { get; }

	bool FlightFlying { get; set; }

	Guid FlightActivatedAbilityID { get; set; }
}
