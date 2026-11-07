using ConsoleLib.Console;
using Kobold;
using UnityEngine;

[ExecuteAlways]
public class ParticleVFXConsumed : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public bool reset = true;

	public int count;

	public float delay;

	public float duration = 0.5f;

	public float delaycount;

	public ex3DSprite2 spriteRenderer;

	public ParticleSystem system;

	public Color color;

	public Color detail;

	private float t;

	public void configure(string configurationString)
	{
		string[] array = configurationString.Split(';');
		exTextureInfo textureInfo = SpriteManager.GetTextureInfo(array[0]);
		color = ConsoleLib.Console.ColorUtility.ColorFromString(array[1]);
		detail = ConsoleLib.Console.ColorUtility.ColorFromString(array[2]);
		spriteRenderer.textureInfo = textureInfo;
		spriteRenderer.detailcolor = detail;
		spriteRenderer.color = color;
		spriteRenderer.backcolor = new Color(0f, 0f, 0f, 0f);
		spriteRenderer.transform.localPosition = new Vector3(0f, 0f, -1f);
		spriteRenderer.gameObject.SetActive(value: true);
		reset = true;
	}

	private void Awake()
	{
		reset = true;
	}

	private void Update()
	{
		if (reset)
		{
			system.Stop();
			system.Clear();
			reset = false;
			count = 0;
			system.Play();
			system.Emit(count);
			t = 0f;
			delaycount = delay;
		}
		if (!(t < duration))
		{
			return;
		}
		if (delaycount > 0f)
		{
			bool flag = false;
			if (delaycount == delay)
			{
				flag = true;
			}
			delaycount -= Time.deltaTime;
			if (!flag)
			{
				return;
			}
		}
		else
		{
			t += Time.deltaTime;
		}
		if ((double)t < 0.25)
		{
			spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 98f, t / duration));
		}
		else
		{
			spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(98f, -120f, t / duration));
		}
		float num = Mathf.Lerp(1f, 0f, t / duration);
		spriteRenderer.gameObject.transform.localScale = new Vector3(num, num, 1f);
	}
}
