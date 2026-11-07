using System;
using System.Collections;
using UnityEngine;

namespace RedShadow.Tween;

public class TweenManager : MonoBehaviour
{
	public static TweenManager Instance;

	protected void Awake()
	{
		Instance = this;
	}

	public static void delayedCall(float time, Action action)
	{
		Instance.StartCoroutine(Instance.delayedCall_co(time, action));
	}

	private IEnumerator delayedCall_co(float time, Action action)
	{
		yield return new WaitForSeconds(time);
		action();
	}
}
