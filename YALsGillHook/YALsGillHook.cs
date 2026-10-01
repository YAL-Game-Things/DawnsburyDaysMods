using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;

namespace YALsGillHook;

public class YALsGillHook {
	public static readonly string Name = "Bill Hook";
	public static readonly string ModID = "YALsBillHook";
	public static readonly ModdedIllustration Icon = new ModdedIllustration("YALsBillHook.png");
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		//
		var trait = ModManager.RegisterTrait(Name, new TraitProperties(
			Name, true, string.Join(" ", [
				"This polearm has a specialized hook that can be used to hook flesh or armor.",
			]), relevantForShortBlock: false
		));
		ModManager.RegisterNewItemIntoTheShop(Name, name => {
			var item = new Item(name, Icon, Name.ToLower(), 0, 2, [
				Trait.Grapple,
				Trait.Martial,
				Trait.Polearm,
				Trait.Reach,
				Trait.TwoHand1d10,
			]).WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing));
			item.StateCheckWhenWielded = (self, weapon) => {
				self.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
					ProvideActionIntoPossibilitySection = (qf, section) => {
						if (section.Name != "Regrip") return null;
						return new ActionPossibility(new CombatAction(self, IllustrationName.DelayArrowRight, "Change grip", [
							Trait.Basic,
						], "Switch between this weapon being considered 1-handed (for grapple) and 2-handed (for maneuvers)", Target.Self().WithAdditionalRestriction(self => {
							if (!weapon.WieldedInTwoHands && self.HeldItems.Count != 1) {
								return "Other hand is occupied!";
							} else return null;
						})).WithItem(weapon).WithEffectOnEachTarget(async (action, origin, target, result) => {
							if (weapon.WieldedInTwoHands) {
								HandednessRules.MakeSingleGrip(weapon);
							} else {
								HandednessRules.MakeDoubleGrip(weapon);
								origin.Cache.Clear();
							}
						}).WithActionCost(0));
					}
				});
			};
			return item;
		});
	}
}