using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Modding;

namespace YALsSpiritInvoker;

public class YALsSpiritInvoker {
	public const string ModID = "YALsSpiritInvoker";
	public static T RegisterEnumMember<T>(string technicalName) where T : Enum {
		return ModManager.RegisterEnumMember<T>(ModID + ":" + technicalName);
	}
	public static FeatName RegisterFeatName(string technicalName, string name) {
		return ModManager.RegisterFeatName(ModID + ":" + technicalName, name);
	}

	public static readonly Trait Trait = ModManager.RegisterTrait("Spirit Invoker");
	static IEnumerable<Feat> LoadAll() {
		yield return (
			ArchetypeFeats.CreateAgnosticArchetypeDedication(
				Trait,
				"You're a practicing Spirit invoker, able to enter a trance that connects you physically and mentally to surrounding spirits.",
				string.Join("\n", [
					"You become an expert in Athletics and Religion. You also gain the Diehard general feat and the Enter Spirit Trance ability.",
					"",
					string.Join(" ", [
						"You can Enter Spirit Trance as an action.",
						"The trance lasts for 10 rounds or until you fall unconscious, whichever comes first.",
						"You gain a number of temporary Hit Points equal to your level plus your Constitution modifier.",
						"While in this trance, you gain a +1 status bonus to Fortitude and Will saving throws and your melee Strikes deal 1 additional spirit damage.",
						"Other abilities may require you to be in a spirit trance.",
						"When the trace ends, you lose any remaining temporary Hit Points from Enter Spirit Trance, and you can't Enter a Spirit Trance for 1 minute."
					]),
				])
			)
			.WithPrerequisite(sheet => {
				return sheet.HasFeat(FeatName.Athletics) && sheet.HasFeat(FeatName.Religion);
			}, "You must be trained in Athletics and Religion.")
			.WithOnSheet(sheet => {
				sheet.GrantFeat(FeatName.ExpertAthletics);
				sheet.GrantFeat(FeatName.ExpertReligion);
				sheet.GrantFeat(FeatName.Diehard);
			})
			.WithPermanentQEffect(null, qfEnter => {
				qfEnter.ProvideMainAction = qf => SpiritTrance.CreatePossibility(qf.Owner);
			})
		); // dedication
		yield return InvokeOffense.CreateFeat();
		yield return InvokeDefense.CreateFeat();
	}
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		foreach (var feat in LoadAll()) {
			ModManager.AddFeat(feat);
		}
		static void BorrowModFeat(string technicalName, int level) {
			if (ModManager.TryParse<FeatName>(technicalName, out var featName)) {
				ModManager.AddFeat(ArchetypeFeats.DuplicateFeatAsArchetypeFeat(featName, Trait, level));
			}
		}
		BorrowModFeat("SlamDown", 6);
		BorrowModFeat("BarrelingCharge", 6);
		BorrowModFeat("CrushingSlam", 12);
		BorrowModFeat("OverpoweringCharge", 12);
	}
}