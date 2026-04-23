using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace YALsFlipForMartials;

public class YALsFlip {
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		//
		List<Trait> traits = [
			Trait.Barbarian,
			Trait.Fighter,
			Trait.Ranger,
			Trait.Magus,
		];
		ModManager.AddFeat(new TrueFeat(
			ModManager.RegisterFeatName("YALsFlip", "Flip"), 4,
			null,
			string.Join("\n", [
				"{b}Prerequisites{/b} trained in Acrobatics",
				"{b}Trigger{/b} A creature targets you with a melee Strike and you can see the attacker.",
				"",
				"Flinging your body into a twisting somersault, you gain a +2 circumstance bonus to your AC against the triggering attack. If the attack still hits you, you can Step to an open space that's still within the triggering enemy's reach, turning the impact of the blow into momentum.",
			]),
			traits.ToArray()
		).WithActionCost(-2).WithPermanentQEffect(qfx => {
			qfx.YouAreTargeted = async (qf, attack) => {
				var self = qf.Owner;
				//
				if (!attack.HasTrait(Trait.Attack)) return;
				//
				if (!self.CanSee(attack.Owner)) return;
				//
				var rollSpec = attack.ActiveRollSpecification;
				if (rollSpec == null) return;
				if (rollSpec.TaggedDetermineDC.InvolvedDefense != Defense.AC) return;
				if (attack.ExcludeTargetFromSavingThrow != null && attack.ExcludeTargetFromSavingThrow(attack, self)) return;
				//
				var bonuses = rollSpec.TaggedDetermineDC.CalculatedNumberProducer(attack, attack.Owner, self).Bonuses;
				if (bonuses.Max(b => {
					if (b != null && b.BonusType == BonusType.Circumstance) {
						return b.Amount;
					} else return 0;
				}) < 2 && await self.AskToUseReaction(
					$"You're targeted by {attack.Owner.Name}'s {attack.Name}."
					+ "\nUse Nimble Dodge to gain a +2 circumstance bonus to AC?"
				)) {
					self.AddQEffect(new QEffect {
						ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction,
						BonusToDefenses = (QEffect effect, CombatAction? action, Defense defense) => {
							return defense == Defense.AC ? new Bonus(2, BonusType.Circumstance, "Flip") : null;
						},
						AfterYouTakeActionAgainstTarget = async (effect, action, defender, result) => {
							if (action == attack && result >= CheckResult.Success && defender == self) {
								await self.StepAsync("Flip: Step within the attacker's reach", false, true);
							}
						}
					});
				}
				//
			};
		}));
		//
	}
}