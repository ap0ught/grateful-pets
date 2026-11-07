using System;
using UnityEngine;

namespace Battlehub.UIControls;

public class Rotation : MonoBehaviour
{
	private Vector3 m_rand;

	private float m_prevT;

	private void Start()
	{
		m_rand = UnityEngine.Random.onUnitSphere;
	}

	private void Update()
	{
		if (Time.time - m_prevT > 10f)
		{
			m_rand = UnityEngine.Random.onUnitSphere;
			m_prevT = Time.time;
		}
		transform.rotation *= Quaternion.AngleAxis(MathF.PI * 4f * Time.deltaTime, m_rand);
	}
}
