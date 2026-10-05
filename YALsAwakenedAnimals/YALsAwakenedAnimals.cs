using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using YALsAwakenedAnimals.Feats;

namespace YALsAwakenedAnimals;

public class YALsAwakenedAnimals {
	public const string ModID = "YALsAwakenedAnimals";
	public static readonly FeatName FNAnimal = ModManager.RegisterFeatName(ModID, "Awakened Animal");
	public static readonly Trait TAnimal = ModManager.RegisterTrait("Awakened Animal",
		new TraitProperties("Awakened Animal", true) { IsAncestryTrait = true }
	);
	public static readonly Trait TAdjusted = ModManager.RegisterTrait("Adjusted",
		new TraitProperties("Adjusted", true, "Something about this has been changed for DD")
	);
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		//
		var heritages = AAHeritages.Init();
		AASizes.Init();
		AAAttacks.Init();
		AAFeats.Init();
		AAFlight.Init();
		AAFierceGrasp.Init();
		AASenses.Init();
		//AASensesTest.Init();
		//
		ModManager.AddFeat(
			new AncestrySelectionFeat(
				FNAnimal,
				description: string.Join("\n", [
					"Awakened animals were normal animals that underwent an experience that awakened their minds, giving them full intelligence and the ability to perceive the world through a lens of thought. There are many mysterious ways for animals to awaken, but the most well-known path is through the ritual Awaken Animal."
				]),
				traits: [Trait.Beast, TAnimal],
				hp: 8,
				speed: 4,
				[
					new EnforcedAbilityBoost(Ability.Constitution),
					new EnforcedAbilityBoost(Ability.Wisdom),
					new FreeAbilityBoost(),
				],
				heritages: heritages
			)
			.WithAbilityFlaw(Ability.Intelligence)
			.WithSpecialRules(string.Join("\n", [
				"{b}Animal Attacks{/b}. Your heritage gives you a special unarmed attack in the brawling group.",
				"{b}Awakened Mind{/b}. By remembering your instincts, you can allow yourself to be affected by spells and other effects as though you were an animal ({i}see Other Actions on your or other characters{/i})."
			]))
			.WithOnSheet(sheet => {
				if (!AATools.IsHalfAnimal(sheet)) {
					// RAW A.Animals with a versatile heritages don't gain an animal attack, but isn't that weird
					AAAttacks.Grant(sheet);
					sheet.AddSelectionOption(new SingleFeatSelectionOption($"{ModID}:Size",
						"Animal Size", 0,
						feat => feat.HasTrait(AASizes.Trait)
					));
				}
			})
			.WithPermanentQEffect(null, AAMind.Init)
		);
	}
}