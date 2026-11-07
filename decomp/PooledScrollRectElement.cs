using System.Collections.Generic;
using UnityEngine;

public abstract class PooledScrollRectElement<T> : MonoBehaviour
{
	public abstract void Setup(int placement, List<T> allData);
}
