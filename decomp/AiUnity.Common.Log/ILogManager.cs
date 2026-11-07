using System;
using UnityEngine;

namespace AiUnity.Common.Log;

public interface ILogManager
{
	ILogger GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider = null);
}
