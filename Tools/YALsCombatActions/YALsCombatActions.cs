using Dawnsbury.Core;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;

public class YALsCombatActions {
	public static List<Option> GetStrikePossibilities(Creature self, Func<Item, bool> isValidWeapon, Func<Creature, bool>? isValidTarget) {
		var list = new List<Option>();
		foreach (var item in self.Weapons) if (isValidWeapon(item)) {
			var combatAction = self.CreateStrike(item);
			combatAction.WithActionCost(0);
			if (isValidTarget != null) {
				((CreatureTarget)combatAction.Target).CreatureTargetingRequirements.Add(
					new LegacyCreatureTargetingRequirement((Creature a, Creature d) => (
					!isValidTarget(d)) ? Usability.NotUsableOnThisCreature("excluded") : Usability.Usable
				));
			}
			GameLoop.AddDirectUsageOnCreatureOptions(combatAction, list);
		}
		return list;
	}

	public static async Task<bool> StrikeCreature(
		Creature self,
		Func<Creature, bool>? isValidTarget,
		Func<Item, bool> isValidWeapon,
		bool allowCancel, string? allowPass
	) {
		var possibilities = GetStrikePossibilities(self, isValidWeapon, isValidTarget);
		if (allowCancel) {
			possibilities.Add(new CancelOption(midspell: true));
		} else if (allowPass != null) {
			possibilities.Add(new PassViaButtonOption(allowPass));
		}

		if (possibilities.Count > 0) {
			if (possibilities.Count == 1) {
				await possibilities[0].Action();
				return possibilities[0] is not CancelOption && possibilities[0] is not PassViaButtonOption;
			}

			var result = await self.Battle.SendRequest(new AdvancedRequest(self,
				"Choose a creature to Strike" + (allowCancel ? " or right-click to cancel." : "."), possibilities
			) {
				TopBarText = "Choose a creature to Strike.",
				TopBarIcon = IllustrationName.Fist
			});
			await result.ChosenOption.Action();
			return result.ChosenOption is not CancelOption && result.ChosenOption is not PassViaButtonOption;
		}

		return false;
	}
}
