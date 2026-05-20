using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsSpiritInvoker;

public class SpiritTrance {
	public static readonly IllustrationName Icon = IllustrationName.SpiritBlast;
	public static readonly QEffectId Effect = YALsSpiritInvoker.RegisterEnumMember<QEffectId>("SpiritTrance");
	public static readonly QEffectId AlreadyUsed = YALsSpiritInvoker.RegisterEnumMember<QEffectId>("HadSpiritTrance");
	public static string? MustBeInSpiritTrance(Creature self) {
		if (!self.HasEffect(Effect)) {
			return "Have to be in Spirit Trance!";
		}
		return null;
	}
	public static int GetTemporaryHP(Creature self) {
		return self.Level + self.Abilities.Constitution;
	}
	public static string? WhyNot(Creature self) {
		if (self.HasEffect(Effect)) {
			return "You're already in Spirit Trance.";
		}
		if (self.HasEffect(AlreadyUsed)) {
			return "You've already entered Spirit Trance this encounter.";
		}
		return null;
	}

	public static void Apply(Creature self) {
		self.GainTemporaryHP(GetTemporaryHP(self));
		self.AddQEffect(new QEffect { Id = AlreadyUsed });
		self.AddQEffect(new QEffect(
			"Spirit Trance",
			string.Join("\n", [
				"You gain a +1 status bonus to Fortitude and Will saving throws",
				"Your melee Strikes deal 1 additional spirit damage."
			]),
			ExpirationCondition.ExpiresAtStartOfSourcesTurn, self, Icon
		) {
			Id = Effect,
			RoundsLeft = 10,
			DoNotShowUpOverhead = true,
			BonusToDefenses = (qf, action, defense) => {
				if (defense == Defense.Fortitude || defense == Defense.Will) {
					return new Bonus(1, BonusType.Status, "Spirit Trance");
				}
				return null;
			},
			YouDealDamageWithStrike = (qf, action, diceFormula, target) => {
				if (action.HasTrait(Trait.Melee)) {
					return diceFormula.Add(DiceFormula.FromText("1", "Spirit Trance"));
				}
				return diceFormula;
			},
			StateCheck = delegate (QEffect qfTrance) {
				if (qfTrance.Owner.HasEffect(QEffectId.Unconscious)) {
					qfTrance.ExpiresAt = ExpirationCondition.Immediately;
				}
			}
		});
	}

	public static CombatAction CreateAction(Creature owner) {
		return new CombatAction(owner,
			Icon,
			"Enter Spirit Trance",
			[YALsSpiritInvoker.Trait, Trait.Concentrate, Trait.Divine, Trait.Mental],
			string.Join(" ", [
				"You enter a self-imposed trance that helps you push the physical limits of your body.",
				"",
				$"You gain {{Blue}}{GetTemporaryHP(owner)}{{/Blue}} temporary HP.",
				"",
				"For 10 rounds or until you fall unconscious, whichever comes first, you're in a trance and:",
				"• You gain a +1 status bonus to Fortitude and Will saving throws",
				"• Your melee Strikes deal 1 additional spirit damage."
			]),
			Target.Self().WithAdditionalRestriction(WhyNot)
		).WithEffectOnSelf(Apply);
	}
	public static ActionPossibility? CreatePossibility(Creature owner) {
		if (owner.HasEffect(Effect)) return null;
		return (ActionPossibility)SpiritTrance.CreateAction(owner);
	}
}
