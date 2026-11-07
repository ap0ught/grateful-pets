using System;
using XRL.World.Conversations.Parts;

namespace XRL.World.Parts;

[Serializable]
public class VehicleIntercom : IActivePart
{
	public string StartText;

	public string RequiredMessage;

	public bool Required;

	public VehicleIntercom()
	{
		WorksOnSelf = true;
		NameForStatus = "Intercom";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<AttemptConversationEvent>.ID)
		{
			return ID == BeginConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AttemptConversationEvent E)
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject != null && activePartFirstSubject.TryGetPart<Vehicle>(out var Part) && Part.Pilot != null && E.IsParticipant(activePartFirstSubject))
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (Required)
				{
					if (!E.Silent && !RequiredMessage.IsNullOrEmpty())
					{
						return E.Listener.ShowFailure(RequiredMessage);
					}
					return false;
				}
				return base.HandleEvent(E);
			}
			ConversationScript Part2;
			if (E.Listener == activePartFirstSubject)
			{
				E.Listener = Part.Pilot;
				E.Receiver = activePartFirstSubject;
			}
			else if (E.Speaker == activePartFirstSubject && Part.Pilot.TryGetPart<ConversationScript>(out Part2))
			{
				E.Speaker = Part.Pilot;
				E.Transmitter = activePartFirstSubject;
				E.Blueprint = Part2.GetActiveConversationBlueprint();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (E.IsDevice(activePartFirstSubject))
		{
			if (activePartFirstSubject == E.Transmitter && !StartText.IsNullOrEmpty())
			{
				E.StartNode.AddPart(new TextInsert
				{
					Prepend = true,
					Text = GameText.VariableReplace(StartText, activePartFirstSubject, ParentObject),
					Spoken = !StartText.Contains('['),
					NewLines = 2
				});
			}
			ConsumeCharge();
		}
		return base.HandleEvent(E);
	}
}
