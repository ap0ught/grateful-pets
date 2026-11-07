using UnityEngine;

public class SimpleTextMeshTweener : MonoBehaviour
{
	public string poolName;

	public float time;

	public float duration;

	public Vector3 start;

	public Vector3 end;

	public Color startColor;

	public Color endColor;

	public TextMesh mesh;

	public SimpleTextOutline outline;

	public void doTweens(float t)
	{
		if (mesh != null)
		{
			gameObject.transform.position = Vector3.Lerp(start, end, t);
		}
	}

	public void init(Vector3 start, Vector3 end, Color startColor, Color endColor, float duration, string poolName = null)
	{
		this.start = start;
		this.end = end;
		this.startColor = startColor;
		this.endColor = endColor;
		this.duration = duration;
		this.poolName = poolName;
		mesh = gameObject.GetComponent<TextMesh>();
		time = 0f;
		doTweens(0f);
	}

	public void Finish()
	{
		if (poolName != null)
		{
			CombatJuice.pool(poolName, gameObject);
		}
		else
		{
			Object.Destroy(gameObject);
		}
	}

	private void Update()
	{
		time += Time.deltaTime;
		doTweens(time / duration);
		if (time > duration)
		{
			Finish();
		}
	}
}
