using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using System;
using System.Collections.Generic;
using System.Text;
using Dawnsbury.Core;
using YALsAwakenedAnimals.Feats;

namespace YALsAwakenedAnimals;

public class AAHeritages {
	public static readonly FeatName Running = AATools.RegisterFeatName("Running Animal");
	public static readonly FeatName WaterDwelling = AATools.RegisterFeatName("Water-dwelling Animal");
	public static readonly FeatName Flying = AATools.RegisterFeatName("Flying Animal");
	public static readonly List<FeatName> Names = [Running, WaterDwelling, Flying];
	public static readonly List<Feat> Feats = [];
	public static List<Feat> Init() {
		if (Feats.Count != 0) return Feats;
		var adjusted = YALsAwakenedAnimals.TAdjusted;
		Feats.Add(new HeritageSelectionFeat(
			Running,
			"You are an animal meant for running at great speeds across land.",
			string.Join("\n", [
				"Typically, you run on all fours like a dog, cheetah, or an iguana, but you could also use two legs like a kangaroo, emu, or penguin.",
				"",
				"You have a land Speed of 30ft."
			])
		).WithOnCreature(creature => {
			creature.AddQEffect(new QEffect {
				BonusToAllSpeeds = qf => new Bonus(2, BonusType.Untyped, "Running Animal")
			});
		}).WithOnSheet(sheet => {
			if (AATools.IsHalfAnimal(sheet)) {
				AAAttacks.Grant(sheet);
			}
		}));

		Feats.Add(new HeritageSelectionFeat(
			WaterDwelling,
			"You are an aquatic animal who is most comfortable in the water.",
			string.Join("\n", [
				"You have a land Speed of 20ft and a matching swim Speed."
			]),
			[adjusted]
		).WithOnCreature(creature => {
			creature.AddQEffect(QEffect.Swimming());
		}));

		Feats.Add(new HeritageSelectionFeat(
			Flying,
			"You are an animal that can take flight for long or sustained bursts, such as an eagle, bat, bee, or flying squirrel.",
			string.Join("\n", [
				"The awakening process has disrupted your ability to fly as freely as you once did. What used to be an automatic process is now one that you must apply some thought to until it becomes automatic once again.",
				"",
				"You gain the Take Flight feat."
			]),
			[adjusted]
		).WithOnSheet(sheet => {
			sheet.GrantFeat(AAFlight.TakeFlight);
		}));

		return Feats;
	}
}
