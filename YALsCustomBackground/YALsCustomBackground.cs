using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace YALsCustomBackground;

public class YALsCustomBackground {
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		ModManager.AddFeat(new BackgroundSelectionFeat(
			ModManager.RegisterFeatName("YALsCustomBackground", "Custom Background"),
			"You've lived an interesting life and the story is yours to tell.",
			"You are trained in a skill of your choice and gain a 1st-level general/skill feat of your choice.",
			[new FreeAbilityBoost(), new FreeAbilityBoost()]
		).WithOnSheet(sheet => {
			sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption(
				"YALsCustomBackground:Feat",
				$"Background Feat",
				-1,
				feat => (feat.HasTrait(Trait.General) || feat.HasTrait(Trait.Skill)) && feat.LevelIfAny <= 1
			).WithIsOptional());
			sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption(
				"YALsCustomBackground:Skill",
				"Background Skill",
				-1,
				feat => feat is SkillSelectionFeat
			).WithIsOptional());
		}));
	}
}