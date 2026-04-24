using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using HarmonyLib;

namespace YALsWrestlerTweaks;

public class YALsWrestlerTweaks {
	public static readonly QEffectId DisengagingTwistQI = ModManager.RegisterEnumMember<QEffectId>("YALsWrestlerDisengagingTwist");
	public static readonly QEffectId ClinchStrikeQI = ModManager.RegisterEnumMember<QEffectId>("YALsWrestlerClinchStrike");

	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		//
		ModManager.AddFeat(
			new TrueFeat(
				ModManager.RegisterFeatName("YALsWrestlerDisengagingTwist", "Disengaging Twist"), 4,
				null,
				string.Join("\n", [
					"{b}Trigger{/b} A creature gives you the grabbed or restrained condition.",
					"",
					"Your ability to twist your opponents' bodies into painful locks and holds makes you particularly adept at escaping such predicaments."
					+ " Attempt an Athletics check to Escape the triggering condition."
					+ " You gain a +2 circumstance bonus to this check."
				]),
				[]
			)
			.WithActionCost(-2)
			.WithAvailableAsArchetypeFeat(Trait.Wrestler)
			.WithPermanentQEffect(qf => {
				qf.Id = DisengagingTwistQI;
			})
		);
		//
		ModManager.AddFeat(
			new TrueFeat(
				ModManager.RegisterFeatName("YALsWrestlerClinchStrike", "Clinch Strike"), 6,
				null,
				string.Join("\n", [
					"{b}Trigger{/b} A creature you had grabbed or restrained successfully Escapes.",
					"",
					"Your opponents can’t slip your grasp without punishment. Make an unarmed melee Strike against the triggering creature."
				]),
				[]
			)
			.WithActionCost(-2)
			.WithAvailableAsArchetypeFeat(Trait.Wrestler)
			.WithPermanentQEffect(qf => {
				qf.Id = ClinchStrikeQI;
			})
		);
		//
		var harmony = new Harmony("YAL-Wrestler");
		harmony.PatchAll();
		//
	}

	public static async Task<bool> StrikeCreatureWithUnarmed(Creature self, Func<Creature, bool>? isValidTarget, bool allowCancel, string? allowPass, bool meleeOnly) {
		
		return false;
	}
}

[HarmonyPatch(typeof(CommonAbilityEffects), nameof(CommonAbilityEffects.Grapple))]
class YALsWrestlerTweaks_CommonAbilityEffects_CreateEscape {
	[HarmonyPostfix]
	public static async Task Postfix(Task orig, Creature grappler, Creature target, bool restrainInsteadOfGrab) {
		await orig;
		if (!target.HasEffect(YALsWrestlerTweaks.DisengagingTwistQI)) return;
		//
		var maybeGrapple = target.FindQEffect(QEffectId.Grappled);
		if (maybeGrapple is not QEffect grapple) return;
		//
		if (!await target.AskToUseReaction(
			$"You have been {(restrainInsteadOfGrab ? "restrained" : "grabbed")} by {grappler.Name}."
			+ "\nWould you like to attempt to Escape with Disengaging Twist?"
		)) return;
		//
		var effect = new QEffect {
			BonusToSkillChecks = (skill, action, creature) => {
				return action.ActionId == ActionId.Escape ? new Bonus(2, BonusType.Circumstance, "Disengaging Twist") : null;
			},
			ExpiresAt = ExpirationCondition.ExpiresAtEndOfAnyTurn
		};
		target.AddQEffect(effect);
		//
		var escape = Possibilities.CreateEscape(target, grapple);
		escape.ActionCost = 0;
		await target.Battle.GameLoop.FullCast(escape);
		//
		effect.ExpiresAt = ExpirationCondition.Immediately;
	}
}

[HarmonyPatch(typeof(Possibilities), nameof(Possibilities.CreateEscape))]
class YALsWrestlerTweaks_Possibilities_CreateEscape {
	[HarmonyPostfix]
	public static void Postfix(ref CombatAction __result, Creature self, QEffect grappled) {
		var grappler = grappled.Source;
		if (grappler == null || !grappler.HasEffect(YALsWrestlerTweaks.ClinchStrikeQI)) return;
		//
		__result.WithEffectOnEachTarget(async (spell, a, d, cr) => {
			if (cr >= Dawnsbury.Core.Mechanics.Core.CheckResult.Success
				&& await grappler.AskToUseReaction(
					$"{self.Name} has escaped your Grapple."
					+ "\nHit it with Clinch Strike?"
				)
			) {
				if (!await YALsCombatActions.StrikeCreature(grappler,
					cr => cr == self,
					item => item.HasTrait(Trait.Unarmed),
					true, null
				)) {
					grappler.Actions.RefundReaction();
				}
				//await CommonCombatActions.StrikeAdjacentCreature(grappler, cr => cr == self);
			}
		});
	}
}
