using System;
using System.Collections.Generic;
using System.Text;
using XRL;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;

namespace GratefulPetsMod
{
    /// <summary>
    /// Deterministic Wish-console diagnostics for Grateful Pets.
    /// These commands exercise the real digestion state without requiring
    /// long waits or destructive game actions.
    /// </summary>
    [HasWishCommand]
    public class DebugWishes
    {
        [WishCommand("gratefulpets:status", null)]
        public static bool Status()
        {
            GameObject player = The.Player;
            if (player == null || player.IsInvalid())
            {
                Popup.ShowFail("No player character found.");
                return false;
            }

            StringBuilder text = new StringBuilder();
            text.AppendLine("{{W|Grateful Pets status}}");
            text.AppendLine("Player part: " + (player.GetPart<GratefulPets>() != null ? "yes" : "no"));
            text.AppendLine("Current tick: " + GratefulPets.CurrentTick());

            int count = 0;
            foreach (GameObject pet in NearbyPets(player))
            {
                count++;
                GratefulPetsDigestion digestion = pet.GetPart<GratefulPetsDigestion>();
                text.AppendLine();
                text.AppendLine("{{C|" + pet.GetDisplayName(Single: true, NoColor: true, Short: true, BaseOnly: true) + "}}");
                text.AppendLine("  Blueprint: " + pet.Blueprint);
                text.AppendLine("  IsPet: " + GratefulPets.IsPet(pet));
                text.AppendLine("  IsRobot: " + GratefulPets.IsRobotPet(pet));
                text.AppendLine("  Distance: " + pet.DistanceTo(player));
                if (digestion == null)
                {
                    text.AppendLine("  Digestion: none");
                }
                else
                {
                    text.AppendLine("  RemainingToProduce: " + digestion.RemainingToProduce);
                    text.AppendLine("  NextProductionTick: " + digestion.NextProductionTick);
                    text.AppendLine("  HeldForDelivery: " + digestion.HeldForDelivery);
                    text.AppendLine("  CorpseCount: " + digestion.CorpseCount);
                    text.AppendLine("  TotalCorpsesConsumed: " + digestion.TotalCorpsesConsumed);
                }
            }

            if (count == 0)
            {
                text.AppendLine();
                text.AppendLine("No eligible pet is on the current or adjacent tiles.");
            }

            Popup.ShowBlock(text.ToString());
            return true;
        }

        [WishCommand("gratefulpets:advance", null)]
        public static bool Advance(string args)
        {
            int turns;
            if (!int.TryParse(args == null ? "" : args.Trim(), out turns) || turns <= 0)
            {
                Popup.ShowFail("Usage: gratefulpets:advance:<turns>");
                return false;
            }

            int affected = 0;
            foreach (GameObject pet in NearbyPets(The.Player))
            {
                GratefulPetsDigestion digestion = pet.GetPart<GratefulPetsDigestion>();
                if (digestion == null || digestion.RemainingToProduce <= 0)
                {
                    continue;
                }

                digestion.DebugAdvanceTurns(turns);
                affected++;
            }

            GratefulPets.Msg("&gGrateful Pets advanced " + turns + " digestion turns for " + affected + " nearby pet(s). Use gratefulpets:status to inspect the result.");
            return true;
        }

        static IEnumerable<GameObject> NearbyPets(GameObject player)
        {
            if (player == null || player.IsInvalid() || player.CurrentCell == null)
            {
                yield break;
            }

            HashSet<GameObject> seen = new HashSet<GameObject>();
            List<Cell> cells = new List<Cell>();
            cells.Add(player.CurrentCell);
            foreach (Cell cell in player.CurrentCell.GetAdjacentCells(1, LocalOnly: true, BuiltOnly: false))
            {
                if (cell != null)
                {
                    cells.Add(cell);
                }
            }

            foreach (Cell cell in cells)
            {
                foreach (GameObject obj in cell.GetObjects())
                {
                    if (obj != null && !obj.IsInvalid() && seen.Add(obj) && GratefulPets.IsPet(obj))
                    {
                        yield return obj;
                    }
                }
            }
        }
    }
}
