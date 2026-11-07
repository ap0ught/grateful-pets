using ConsoleLib.Console;
using UnityEngine;
using XRL;
using XRL.World;

public class IRenderableDisplayManager : MonoBehaviour
{
	public UIThreeColorProperties threeColorTile;

	public BackdropWindowManager backdropManager;

	public Transform imposterRoot;

	public void FromRenderable(IRenderable renderable)
	{
		if (renderable is RenderEvent renderEvent && !string.IsNullOrEmpty(renderEvent.WantsBackdrop) && renderEvent.BackdropBleedthrough)
		{
			backdropManager.gameObject.SetActive(value: true);
			threeColorTile.gameObject.SetActive(value: false);
			backdropManager.SetBackdrop(renderEvent.WantsBackdrop);
			backdropManager.SetTile(renderEvent.x, renderEvent.y);
			return;
		}
		backdropManager.gameObject.SetActive(value: false);
		threeColorTile.gameObject.SetActive(value: true);
		threeColorTile.FromRenderable(renderable);
		if (threeColorTile.Background == The.Color.DarkBlack)
		{
			threeColorTile.Background = Color.clear;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
