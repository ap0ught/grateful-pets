using ConsoleLib.Console;
using Kobold;
using UnityEngine;

[ExecuteAlways]
public class ParticleVFXDecapitated : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public bool reset = true;

	public int count;

	public float delay;

	public float duration = 0.5f;

	public float delaycount;

	public Texture2D textureTop;

	public Texture2D textureBottom;

	public Sprite spriteTop;

	public Sprite spriteBottom;

	public SpriteRenderer spriteRendererTop;

	public SpriteRenderer spriteRendererBottom;

	public ParticleSystem system;

	public Vector3 topStart = new Vector3(0f, 0f, 0f);

	public Vector3 topEnd = new Vector3(6f, 1f, 0f);

	public Vector3 bottomStart = new Vector3(0f, 0f, 0f);

	public Vector3 bottomEnd = new Vector3(-6f, -1f, 0f);

	public Color color;

	public Color detail;

	private float t;

	public void configure(string configurationString)
	{
		string[] array = configurationString.Split(';');
		color = ConsoleLib.Console.ColorUtility.ColorFromString(array[1]);
		detail = ConsoleLib.Console.ColorUtility.ColorFromString(array[2]);
		Texture2D texture = SpriteManager.GetUnitySprite(array[0]).texture;
		Color[] pixels = texture.GetPixels();
		Color[] pixels2 = texture.GetPixels();
		for (int i = 0; i < texture.width; i++)
		{
			for (int j = 0; j < texture.height; j++)
			{
				int num = i + j * texture.width;
				if (j > 16)
				{
					pixels2[num] = new Color(0f, 0f, 0f, 0f);
					if (pixels[num].a > 0f)
					{
						if ((double)pixels[num].r < 0.5)
						{
							pixels[num] = color;
						}
						else
						{
							pixels[num] = detail;
						}
					}
					continue;
				}
				pixels[num] = new Color(0f, 0f, 0f, 0f);
				if (pixels2[num].a > 0f)
				{
					if ((double)pixels2[num].r < 0.5)
					{
						pixels2[num] = color;
					}
					else
					{
						pixels2[num] = detail;
					}
				}
			}
		}
		textureTop = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, mipChain: false);
		textureTop.filterMode = UnityEngine.FilterMode.Point;
		textureTop.SetPixels(pixels);
		textureTop.Apply();
		textureBottom = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, mipChain: false);
		textureBottom.filterMode = UnityEngine.FilterMode.Point;
		textureBottom.SetPixels(pixels2);
		textureBottom.Apply();
		spriteTop = Sprite.Create(textureTop, new Rect(0f, 0f, textureTop.width, textureTop.height), new Vector2(0.5f, 0.5f), 1f);
		spriteBottom = Sprite.Create(textureBottom, new Rect(0f, 0f, textureBottom.width, textureBottom.height), new Vector2(0.5f, 0.5f), 1f);
		spriteRendererTop.sprite = spriteTop;
		spriteRendererBottom.sprite = spriteBottom;
		spriteRendererTop.transform.localPosition = new Vector3(0f, 0f, 0f);
		spriteRendererBottom.transform.localPosition = new Vector3(0f, 0f, 0f);
		spriteRendererTop.gameObject.SetActive(value: true);
		spriteRendererBottom.gameObject.SetActive(value: true);
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
		if (t < duration)
		{
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
			spriteRendererTop.gameObject.transform.localPosition = Vector3.Lerp(topStart, topEnd, t / duration);
			spriteRendererBottom.gameObject.transform.localPosition = Vector3.Lerp(bottomStart, bottomEnd, t / duration);
		}
		else
		{
			if (spriteRendererTop.gameObject.activeInHierarchy)
			{
				spriteRendererTop.gameObject.SetActive(value: false);
			}
			if (spriteRendererBottom.gameObject.activeInHierarchy)
			{
				spriteRendererBottom.gameObject.SetActive(value: false);
			}
		}
	}
}
