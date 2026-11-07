using System.Collections.Generic;

namespace XRL.UI.Framework;

public interface IFrameworkDataList
{
	IEnumerable<FrameworkDataElement> getChildren();
}
