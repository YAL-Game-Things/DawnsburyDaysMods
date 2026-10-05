using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsAwakenedAnimals;

public class AATools {
	public static string MakeKey(string name) {
		return YALsAwakenedAnimals.ModID + ":" + name;
	}
	public static FeatName RegisterFeatName(string name) {
		return ModManager.RegisterFeatName(YALsAwakenedAnimals.ModID + ":" + name, name);
	}
	public static bool IsHalfAnimal(CalculatedCharacterSheetValues sheet) {
		return sheet.Ancestry == null || !sheet.Ancestry.HasTrait(YALsAwakenedAnimals.TAnimal);
	}
}
