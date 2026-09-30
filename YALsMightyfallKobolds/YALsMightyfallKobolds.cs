using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace YALsMightyfallKobolds;

public class YALsMightyfallKobolds {
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		var mightyfall = new HeritageSelectionFeat(
			ModManager.RegisterFeatName("YALsMightyfallKobold", "Mightyfall Kobold"),
			"Your proximity to a mighty kaiju has caused you to grow strong and hardy.",
			string.Join(" ", [
				"You gain 10 Hit Points from your ancestry instead of 6.",
				"Instead of the normal ability boosts and flaws, your boosts are Strength and Charisma, and your ability flaw is Intelligence."
			])
		);
		mightyfall.WithOnSheet(sheet => {
			sheet.AbilityBoostsFabric.AbilityFlaw = Ability.Intelligence;
			sheet.AbilityBoostsFabric.AncestryBoosts = [
				new EnforcedAbilityBoost(Ability.Strength),
				new EnforcedAbilityBoost(Ability.Charisma),
				new FreeAbilityBoost()
			];
		});
		mightyfall.WithOnCreature(creature => {
			creature.MaxHP += 4;
		});
		ModManager.AddFeat(mightyfall);
		//
		LoadOrder.WhenFeatsBecomeLoaded += () => {
			if (ModManager.TryParse<FeatName>("ModKobold", out var koboldFN)) {
				var koboldFeat = AllFeats.GetFeatByFeatName(koboldFN);
				if (koboldFeat != null && koboldFeat.Subfeats != null) {
					koboldFeat.Subfeats.Add(mightyfall);
				} else throw new Exception("No kobolds?");
			} else throw new Exception("No kobolds!");
		};
	}
}