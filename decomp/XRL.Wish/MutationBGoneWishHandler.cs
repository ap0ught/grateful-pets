using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.Wish;

/// <summary>
///     Community member helado created this wish which helps a lot with debugging.
/// </summary>
[HasWishCommand]
public static class MutationBGoneWishHandler
{
	[WishCommand(null, null, Command = "mutationbgone")]
	public static bool MutationBGone()
	{
		Mutations mutations = GetMutations();
		List<BaseMutation> mutationList = mutations.MutationList;
		if (mutationList.Count > 0)
		{
			int num = Popup.PickOption("Choose a mutation for me to gobble up!", null, "", "Sounds/UI/ui_notification", mutationList.ConvertAll((BaseMutation mutation) => mutation.GetDisplayName()).ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
			if (num != -1)
			{
				RemoveMutation(mutations, mutationList[num]);
			}
		}
		else
		{
			Popup.Show("Huh? Get some mutations first if you want me to eat them!");
		}
		return true;
	}

	/// <summary>
	///  Given a specific mutation class name remove that mutation.
	/// </summary>
	[WishCommand(null, null, Command = "mutationbgone")]
	public static bool MutationBGone(string argument)
	{
		Mutations part = The.Player.GetPart<Mutations>();
		BaseMutation mutation = part.GetMutation(argument);
		if (mutation == null)
		{
			Popup.Show("Didn't find that one. Try again?");
		}
		else
		{
			RemoveMutation(part, mutation);
		}
		return true;
	}

	public static Mutations GetMutations()
	{
		return The.Player.GetPart<Mutations>();
	}

	public static void RemoveMutation(Mutations mutations, BaseMutation mutation)
	{
		mutations.RemoveMutation(mutation);
		Popup.Show("Om nom nom! " + mutation.GetDisplayName() + " is gone! {{w|*belch*}}");
	}
}
