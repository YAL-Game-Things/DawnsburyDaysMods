using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace YALsAwakenedAnimals;

public class AAAttacks {
	public static readonly Trait TAttack = ModManager.RegisterTrait("Awakened Animal Attack");
	public static readonly List<Feat> List = [];
	public static Feat Register(string name, int dieSize, DamageKind damageKind, List<Trait> traits, IllustrationName icon) {
		traits = [Trait.Finesse, .. traits];
		string traitStr;
		if (traits.Count != 1) {
			traitStr = string.Join(", ", traits) + ", and Unarmed";
		} else traitStr = traits[0] + " and Unarmed";
		var feat = new Feat(
			ModManager.RegisterFeatName($"{YALsAwakenedAnimals.ModID}:Attack:{name}", name),
			null,
			string.Join("\n", [
				$"You have a {name} unarmed attack that deals 1d{dieSize} {damageKind} damage.",
				$"It is in Brawling group and has {traitStr} traits."
			]),
			[TAttack],
			null
		).WithPermanentQEffect(null, qf => {
			qf.AdditionalUnarmedStrike = CommonItems.CreateNaturalWeapon(
				icon, name,
				$"1d{dieSize}", damageKind,
				[.. traits, Trait.Unarmed]
			);
		});
		ModManager.AddFeat(feat);
		List.Add(feat);
		return feat;
	}
	//
	public static readonly Feat Antler = Register("Antler", 6, DamageKind.Piercing, [], IllustrationName.Antler);
	public static readonly Feat Beak = Register("Beak", 6, DamageKind.Piercing, [], IllustrationName.Beak);
	public static readonly Feat Claw = Register("Claw", 4, DamageKind.Slashing, [Trait.Agile], IllustrationName.DragonClaws);
	public static readonly Feat Fangs = Register("Fangs", 6, DamageKind.Piercing, [], IllustrationName.Fang);
	// can't remove the base Fist anyway
	public static readonly Feat Horn = Register("Horn", 4, DamageKind.Piercing, [], IllustrationName.Horn);
	public static readonly Feat Jaws = Register("Jaws", 6, DamageKind.Piercing, [], IllustrationName.Jaws);
	public static readonly Feat Tail = Register("Tail", 6, DamageKind.Bludgeoning, [Trait.Trip], IllustrationName.Tail);
	public static readonly Feat Talon = Register("Talon", 4, DamageKind.Piercing, [Trait.Agile], IllustrationName.DragonClaws);
	public static readonly Feat Tongue = Register("Tongue", 6, DamageKind.Bludgeoning, [], IllustrationName.Tongue);
	public static readonly Feat Wing = Register("Wing", 4, DamageKind.Bludgeoning, [Trait.Agile], IllustrationName.Wing);
	public static List<Feat> Init() {
		return List;
	}
	public static void Grant(CalculatedCharacterSheetValues sheet) {
		sheet.AddSelectionOption(new SingleFeatSelectionOption($"{YALsAwakenedAnimals.ModID}:Attack",
			"Animal Attack", 0,
			feat => feat.HasTrait(TAttack)
		));
	}
}
