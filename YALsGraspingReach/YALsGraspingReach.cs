using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Modding;

namespace YALsGraspingReach;

public class YALsGraspingReach {
	public static readonly string ModID = "YALsGraspingReach";
	public static readonly string Name = "Grasping Reach";
	public static readonly QEffectId QFID = ModManager.RegisterEnumMember<QEffectId>(ModID);
	public static readonly IllustrationName Icon = IllustrationName.Longspear;
	public static readonly IllustrationName IconMini = IllustrationName.Ranseur;

	/*// but if we were to check...
	static bool WeaponMatches(Item weapon) {
		if (!weapon.Traits.Contains(Trait.Melee)) return false;
		if (!weapon.WieldedInTwoHands) return false;
		if (weapon.Traits.Contains(Trait.Reach)) return false;
		var props = weapon.WeaponProperties;
		if (props == null || props.DamageDieSize < 6) return false;
		return true;
	}
	*/
	static void Activate(Creature target) {
		target.Space.NaturalReachModification += 1;
		target.AddQEffect(new QEffect(Name,
			"Your reach is increased by 5 feet, but your melee weapon damage dice are decreased by 1 step.",
			ExpirationCondition.Never, target, Icon
		) {
			Id = QFID,
			WhenExpires = qf => {
				qf.Owner.Space.NaturalReachModification -= 1;
			},
			OverrideItemDamageDie = (qf, weapon, mods) => {
				var props = weapon.WeaponProperties;
				if (props == null) return (Dice)1;
				if (weapon.Traits.Contains(Trait.Melee)) {
					return (Dice)DamageDiceUtils.DecreaseDamageDiceByOneStep(props.DamageDieSize);
				} else return (Dice)props.DamageDieSize;
			}
		});
	}

	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		var feat = new TrueFeat(
			ModManager.RegisterFeatName(ModID, Name), 1,
			"You can extend a tangle of vines or tendrils to support your arms and extend your reach.",
			string.Join(" ", [
				"With an Interact action, you can change between a typical grip and extended grasp",
				"that increases your reach by 5 feet but decreases your melee weapons' damage dice by 1 step."
			]),
			[Trait.Leshy]
		).WithPermanentQEffect(qfx => {
			qfx.StartOfCombat = async qf => {
				if (await qf.Owner.AskForConfirmation(Icon, "Activate Grasping Reach?", "Yeah")) {
					Activate(qf.Owner);
				}
			};
			qfx.ProvideActionIntoPossibilitySection = (qf, section) => {
				if (section.PossibilitySectionId != PossibilitySectionId.AttackManeuvers) return null;
				var action = new CombatAction(qf.Owner,
					Icon, Name,
					[Trait.Interact],
					"Change between short grip / long grip!",
					Target.Self()
				)
				.WithActionCost(1)
				.WithEffectOnEachTarget(async (action, origin, target, result) => {
					if (target.HasEffect(QFID)) {
						target.RemoveAllQEffects(qf => qf.Id == QFID);
					} else {
						Activate(target);
					}
				});
				return new ActionPossibility(action);
			};
		}).WithActionCost(1);
		ModManager.AddFeat(feat);
	}
}