using System;

namespace AiUnity.NLog.Core.Config;

internal interface IFactory
{
	void Clear();

	void ScanTypes(Type[] type, string prefix);

	void RegisterType(Type type, string itemNamePrefix);
}
