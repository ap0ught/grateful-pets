using ConsoleLib.Console;
using Kobold;
using UnityEngine;

[ExecuteAlways]
public class ParticleVFXAnimated : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public bool reset = true;

	public int count;

	public float delay;

	public float duration = 0.05f;

	public float delaycount;

	public float speed = 1f;

	public ex3DSprite2 mask;

	public ex3DSprite2 spriteRenderer;

	public ParticleSystem system;

	public Vector3 start = new Vector3(0f, 0f, 0f);

	public Vector3 end = new Vector3(0f, 0f, 0f);

	public Color color;

	public Color detail;

	public exTextureInfo sprite;

	private float t;

	public void configure(string configurationString)
	{
		string[] array = configurationString.Split(';');
		sprite = SpriteManager.GetTextureInfo(array[0]);
		this.color = ConsoleLib.Console.ColorUtility.ColorFromString(array[1]);
		detail = ConsoleLib.Console.ColorUtility.ColorFromString(array[2]);
		Color color = ConsoleLib.Console.ColorUtility.ColorFromString("k");
		mask.color = color;
		mask.detailcolor = color;
		mask.backcolor = color;
		spriteRenderer.textureInfo = sprite;
		spriteRenderer.detailcolor = detail;
		spriteRenderer.color = this.color;
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
			Color color = ConsoleLib.Console.ColorUtility.ColorFromString("k");
			mask.color = color;
			mask.detailcolor = color;
			mask.backcolor = color;
			spriteRenderer.textureInfo = sprite;
			spriteRenderer.detailcolor = detail;
			spriteRenderer.color = this.color;
			spriteRenderer.backcolor = new Color(0f, 0f, 0f, 0f);
			spriteRenderer.transform.localPosition = new Vector3(0f, 0f, -1f);
			spriteRenderer.gameObject.SetActive(value: true);
			if (system != null)
			{
				system?.Stop();
				system?.Clear();
			}
			reset = false;
			if (system != null)
			{
				count = 0;
				system?.Play();
				system?.Emit(count);
			}
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
		if ((double)t < 0.5)
		{
			spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 8f * Mathf.Sin(t * 18f * speed));
		}
		else if (t > 1f && (double)t < 1.2)
		{
			spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 4f * Mathf.Sin(t * 21f));
			if ((double)t < 1.1)
			{
				spriteRenderer.gameObject.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, 4f, (t - 1f) * 10f), 0f);
			}
			else
			{
				spriteRenderer.gameObject.transform.localPosition = new Vector3(0f, Mathf.Lerp(4f, 0f, (t - 1.1f) * 10f), 0f);
			}
		}
		else if (t > 2f && t < 3f)
		{
			spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 16f * Mathf.Sin(t * 12f));
			if ((double)t < 2.5)
			{
				spriteRenderer.gameObject.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, 8f, Easing.QuarticEaseOut((t - 2f) * 2f)), 0f);
			}
			else
			{
				spriteRenderer.gameObject.transform.localPosition = new Vector3(0f, Mathf.Lerp(8f, 0f, Easing.QuarticEaseIn((t - 2.5f) * 2f)), 0f);
			}
		}
		else
		{
			spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			spriteRenderer.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
	}
}
