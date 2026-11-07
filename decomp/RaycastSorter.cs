using System.Collections;
using UnityEngine;

public class RaycastSorter : IComparer
{
	int IComparer.Compare(object _a, object _b)
	{
		if (!(_a is RaycastHit) || !(_b is RaycastHit))
		{
			return 0;
		}
		RaycastHit raycastHit = (RaycastHit)_a;
		RaycastHit raycastHit2 = (RaycastHit)_b;
		return (int)Mathf.Sign(raycastHit.distance - raycastHit2.distance);
	}
}
