using System;
using System.Collections.Generic;

namespace XRL.World.Encounters;

[Serializable]
[Obsolete]
public class PsychicManager : IGamestatePostload
{
	public List<PsychicFaction> PsychicFactions = new List<PsychicFaction>();

	public List<ExtraDimension> ExtraDimensions = new List<ExtraDimension>();

	public void OnGamestatePostload(XRLGame game, SerializationReader reader)
	{
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string dimensionalTraining = reader.ReadString();
			if (i < PsychicFactions.Count)
			{
				PsychicFactions[i].dimensionalTraining = dimensionalTraining;
			}
		}
		int num2 = reader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			string training = reader.ReadString();
			if (j < ExtraDimensions.Count)
			{
				ExtraDimensions[j].Training = training;
			}
		}
		DimensionManager dimensionManager = new DimensionManager();
		dimensionManager.PsychicFactions = PsychicFactions;
		dimensionManager.ExtraDimensions = ExtraDimensions;
		game.ObjectGameState["DimensionManager"] = dimensionManager;
		game.ObjectGameState.Remove("PsychicManager");
	}
}
