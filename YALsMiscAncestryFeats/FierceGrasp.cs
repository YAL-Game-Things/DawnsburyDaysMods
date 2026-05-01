using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsMiscAncestryFeats;

public class FierceGrasp {
	public static readonly FeatName FeatName = YALsMiscAncestryFeats.RegisterFeatName("Fierce Grasp");
	public static readonly IllustrationName Icon = IllustrationName.ChillTouch;
	public static readonly QEffectId PenaltyQI = ModManager.RegisterEnumMember<QEffectId>("YALsFierceGraspPenalty");
	public static readonly QEffectId BonusQI = ModManager.RegisterEnumMember<QEffectId>("YALsFierceGraspBonus");
	public static bool IsGrabbedOrRestrainedBy(Creature creature, Creature grabber) {
		foreach (var qf in creature.QEffects) {
			if (qf.Id is QEffectId.Grappled or QEffectId.Restrained
				&& qf.Source == grabber
			) return true;
		}
		return false;
	}
	public static void ApplyGrasp(Creature grabber, Creature target) {
		grabber.AddQEffect(new QEffect() {
			Id = BonusQI,
			Illustration = Icon,
			Name = "Fierce Grasping",
			ExpiresAt = ExpirationCondition.Never,
			BonusToDefenses = (effect, action, defense) => {
				return (defense == Defense.AC
					&& action != null && action.Owner == target
				) ? new Bonus(1, BonusType.Circumstance, "Fierce Grasp") : null;
			},
			StateCheck = (qf) => {
				if (!IsGrabbedOrRestrainedBy(target, grabber)) {
					qf.ExpiresAt = ExpirationCondition.Immediately;
				}
			}
		});
		
		target.AddQEffect(new QEffect() {
			Id = PenaltyQI,
			Illustration = Icon,
			Name = "Fierce Grasped",
			ExpiresAt = ExpirationCondition.Never,
			BonusToAttackRolls = (effect, action, defender) => {
				if (action.ActionId is ActionId.Escape
					&& action.Tag is Creature fromWhom
					&& fromWhom == grabber
				) {
					return new Bonus(-2, BonusType.Circumstance, "Fierce Grasp");
				} else return null;
			},
			AfterYouTakeAction = async (effect, action) => {
				if (action.ActionId is ActionId.Escape
					&& action.Tag is Creature fromWhom
					&& fromWhom == grabber
				) {
					effect.ExpiresAt = ExpirationCondition.Immediately;
				}
			}
		});
	}
	public static void Init() {
		ModManager.AddFeat(new TrueFeat(
			FeatName, 5,
			"Once you get your hands on someone, it's hard for them to get away.",
			string.Join("\n", [
				"{b}Requirements{/b} You have an opponent grabbed or restrained.",
				"",
				"Your opponent takes a –2 circumstance penalty to their next attempt to Escape from being grabbed or restrained by you,"
				+ " and you gain a +1 circumstance bonus to your AC against any attacks they make against you while you have them grabbed.",
			]),
			[Trait.AllAncestries, YALsMiscAncestryFeats.TAwakenedAnimal]
		).WithPermanentQEffect(qfx => {
			qfx.ProvideActionIntoPossibilitySection = (qf, section) => {
				if (section.PossibilitySectionId == PossibilitySectionId.AttackManeuvers) {
					return (ActionPossibility)(new CombatAction(qf.Owner,
						Icon,
						"Fierce Grasp",
						[],
						string.Join("\n", [
							"{b}Requirements{/b} You have an opponent grabbed or restrained.",
							"",
							"Your opponent takes a –2 circumstance penalty to their next attempt to Escape from being grabbed or restrained by you,"
							+ " and you gain a +1 circumstance bonus to your AC against any attacks they make against you while you have them grabbed.",
						]),
						(Target
							.Distance(100)
							.WithAdditionalConditionOnTargetCreature((grabber, target) => {
								return IsGrabbedOrRestrainedBy(target, grabber)
									? Usability.Usable : Usability.CommonReasons.NotGrappledByYou;
							})
						)
					) {
						EffectOnOneTarget = async (spell, attacker, defender, result) => {
							ApplyGrasp(attacker, defender);
						}
					});
				}
				return null;
			};
		}));
	}
}
