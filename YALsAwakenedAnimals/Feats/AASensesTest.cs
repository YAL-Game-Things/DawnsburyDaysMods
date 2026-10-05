using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
namespace YALsAwakenedAnimals.Feats;

public class AASensesTest {
	static readonly Trait TSense = ModManager.RegisterTrait("Awakened Animal Sense");
	static Feat Register(string name) {
		var feat = new Feat(
			ModManager.RegisterFeatName($"YALsAwakenedAnimals:Sense:" + name, name),
			"Text", "Desc", [TSense], null
		);
		ModManager.AddFeat(feat);
		return feat;
	}
	public static readonly Feat A = Register("A");
	public static readonly Feat B = Register("B");
	public static void Init() {
		var TAnimal = YALsAwakenedAnimals.TAnimal;
		ModManager.AddFeat(
			new TrueFeat(
				ModManager.RegisterFeatName("YALsAwakenedAnimals:NaturalSenses", "Natural Senses"), 1,
				"You have retained your sharp animal senses even after awakening.",
				"Choose a sense appropriate to your kind of animal.",
				[TAnimal]
			)
			.WithOnSheet(sheet => {
				sheet.AddSelectionOption(new SingleFeatSelectionOption(
					$"{YALsAwakenedAnimals.ModID}:NaturalSenses:Sense",
					"Natural Sense", 0,
					feat => feat.HasTrait(TSense)
				));
			})
		);
	}
}
