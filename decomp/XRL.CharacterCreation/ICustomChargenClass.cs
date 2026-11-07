using System.Collections.Generic;
using XRL.World;

namespace XRL.CharacterCreation;

public interface ICustomChargenClass
{
	void BuildCharacterBody(GameObject body);

	IEnumerable<string> GetChargenInfo();
}
