using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;

namespace YALsMiscAncestryFeats;

public class YALsMiscAncestryFeats {
	public const string ModID = "YALsMiscAncestryFeats";
	public static FeatName RegisterFeatName(string name) {
		return ModManager.RegisterFeatName(ModID + ":" + name, name);
	}
	public static Trait RegisterPseudoAncestryTrait(string name) {
		return ModManager.RegisterTrait(name, new TraitProperties(name, true));
	}
	//
	public static readonly Trait TCentaur = RegisterPseudoAncestryTrait("Centaur");
	public static readonly Trait TAwakenedAnimal = RegisterPseudoAncestryTrait("Awakened Animal");
	//
	public static readonly FeatName PracticedBrawn = RegisterFeatName("Practiced Brawn");
	//
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		ModManager.AddFeat(new TrueFeat(
			PracticedBrawn, 1,
			"You're accustomed to long days filled with hard physical labor.",
			string.Join(" ", [
				"You gain a +1 circumstance bonus to Athletics checks to Force Open and Shove, and to Fortitude saving throws to resist becoming fatigued.",
				"When you roll a success on an Athletics check to Shove, you get a critical success instead.",
			]),
			[Trait.AllAncestries, TCentaur]
		).WithPermanentQEffect(qfx => {
			qfx.AdjustActiveRollCheckResult = (qf, action, creature, result) => {
				return action.ActionId is ActionId.Shove && result == CheckResult.Success ? CheckResult.CriticalSuccess : result;
			};
			qfx.BonusToAttackRolls = (qf, action, target) => {
				if (action.ActionId is ActionId.Shove) {
					return new Bonus(1, BonusType.Circumstance, "Practiced Brawn");
				}
				return null;
			};
			// "to resist becoming fatigued", can't do that
		}));

		FierceGrasp.Init();
	}
}