using System.Collections.Generic;

namespace AiUnity.Common.Tags;

public interface ITagAccess
{
	IEnumerable<string> TagPaths { get; }
}
