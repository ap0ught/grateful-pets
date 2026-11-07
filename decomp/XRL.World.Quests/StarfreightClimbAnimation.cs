using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.Wish;

namespace XRL.World.Quests;

[HasWishCommand]
public static class StarfreightClimbAnimation
{
	[WishCommand("climb", null)]
	public static void WishRunClimbAnimation()
	{
		AscensionSystem.WishStart();
		Task.Run(() => RunSpindleIntroSequence(() =>
		{
			FadeToBlack.FadeIn(0f);
		})).Wait();
	}

	public static async Task RunSpindleIntroSequence(Action OnFadeOutComplete)
	{
		Thread.Sleep(10);
		Renderable renderable = new Renderable("Creatures/sw_golem_oddity.png", " ", "&y", null, 'w');
		GameObject gameObject = The.Game.GetSystem<AscensionSystem>()?.Climber;
		if (gameObject != null)
		{
			renderable.Copy(gameObject.Render);
		}
		SemaphoreSlim complete = new SemaphoreSlim(0);
		GameManager.Instance.PushGameView("Cinematic");
		SoundManager.StopMusic("music", Crossfade: true, 3f);
		await The.UiContext;
		float? num = 0f;
		float? to = 1f;
		Color? toColor = Camera.main.backgroundColor;
		FadeToBlack.Fade(3f, num, to, null, toColor);
		await Task.Delay(3000);
		OnFadeOutComplete?.Invoke();
		AscendAnimation.Play(() =>
		{
			complete.Release(1);
		}, renderable);
		Debug.Log("Arrival Starting");
		await complete.WaitAsync();
		Debug.Log("Arrival Complete");
	}
}
