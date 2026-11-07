using System;
using Newtonsoft.Json;

namespace XRL.CharacterBuilds;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AbstractEmbarkBuilderModuleData
{
	public Version Version = new Version(1);
}
