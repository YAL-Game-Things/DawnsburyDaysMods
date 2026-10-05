using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace YALsAwakenedAnimals.Feats;

public class AASenses {
	public static readonly Trait TSense = ModManager.RegisterTrait("Awakened Animal Sense");
	public static readonly FeatName NaturalSenses = AATools.RegisterFeatName("Natural Senses");
	static FeatName RegisterFeatName(string name) {
		return ModManager.RegisterFeatName($"{YALsAwakenedAnimals.ModID}:Sense:{name}", name);
	}
	public static void Init() {
		var TAnimal = YALsAwakenedAnimals.TAnimal;
		var TAdjusted = YALsAwakenedAnimals.TAdjusted;

		ModManager.AddFeat(
			new Feat(
				RegisterFeatName("Sharpened Vision"),
				null,
				string.Join("\n", [
					"You gain a +1 circumstance bonus to your Perception DC and to your checks to Seek."
				]),
				[TSense],
				null
			)
			.WithPermanentQEffect(null, qfx => {
				qfx.BonusToDefenses = (qf, action, defense) => {
					if (defense == Defense.Perception) {
						return new Bonus(1, BonusType.Circumstance, "Sharpened Vision");
					} else return null;
				};
				qfx.BonusToAttackRolls = (qf, action, creature) => {
					if (action.ActionId == ActionId.Seek) {
						return new Bonus(1, BonusType.Circumstance, "Sharpened Vision");
					} else return null;
				};
			})
		);

		ModManager.AddFeat(
			new Feat(
				RegisterFeatName("Scent"),
				null,
				string.Join("\n", [
					"You gain scent as an imprecise sense with a range of 30 feet.",
					"You detect creatures within 30 feet."
				]),
				[TSense],
				null
			)
			.WithPermanentQEffect(null, qfx => {
				qfx.StateCheck = qf => {
					var self = qf.Owner;
					if (self.HasEffect(QEffectId.Unconscious)) return;
					foreach (var enemy in self.Battle.AllCreatures) {
						if (enemy.EnemyOf(self) && enemy.DistanceTo(self) <= 6) {
							enemy.DetectionStatus.Undetected = false;
						}
					}
				};
			})
		);

		ModManager.AddFeat(
			new Feat(
				RegisterFeatName("Echolocation"),
				null,
				string.Join("\n", [
					"You gain echolocation as a precise sense with a range of 10 feet.",
					"You detect creatures within 10 feet, and they cannot be hidden to you."
				]),
				[TSense],
				null
			)
			.WithPermanentQEffect(null, qfx => {
				qfx.StateCheck = qf => {
					var self = qf.Owner;
					if (self.HasEffect(QEffectId.Unconscious)) return;
					foreach (var enemy in self.Battle.AllCreatures) {
						if (enemy.EnemyOf(self) && enemy.DistanceTo(self) <= 2) {
							enemy.DetectionStatus.Undetected = false;
							if (enemy.HasEffect(QEffectId.Invisible)) {
								enemy.DetectionStatus.HiddenTo.Remove(self);
							}
						}
					}
				};
			})
		);

		ModManager.AddFeat(
			new TrueFeat(
				NaturalSenses, 1,
				"You have retained your sharp animal senses even after awakening.",
				"Choose a sense appropriate to your kind of animal.",
				[TAnimal]
			)
			.WithOnSheet(sheet => {
				sheet.AddSelectionOption(new SingleFeatSelectionOption(
					$"{YALsAwakenedAnimals.ModID}:NaturalSenses:Sense",
					"Natural Sense", 1,
					feat => feat.HasTrait(TSense)
				));
			})
		);
	}
}
