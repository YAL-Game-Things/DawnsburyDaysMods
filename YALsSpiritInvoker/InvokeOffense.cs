using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;

namespace YALsSpiritInvoker;

public class InvokeOffense {
	public const string ID = "InvokeOffense";
	public const string Name = "Invoke Offense";
	public static readonly FeatName FeatName = YALsSpiritInvoker.RegisterFeatName(ID, Name);
	public static readonly IllustrationName Icon = IllustrationName.ShootingStar;
	public static readonly QEffectId Effect = YALsSpiritInvoker.RegisterEnumMember<QEffectId>(ID);
	//
	static DamageKind SpiritDamageKind = DamageKind.Untyped;
	//
	public static int GetDieCount(Creature self) {
		return self.Level switch {
			>= 20 => 4,
			>= 12 => 3,
			>= 5 => 2,
			_ => 1,
		};
	}
	public static void Apply(Creature self) {
		// maybe you have the Spirit Damage mod installed!
		if (SpiritDamageKind == DamageKind.Untyped) {
			SpiritDamageKind = ModManager.TryParse<DamageKind>("Spirit", out var spirit) ? spirit : DamageKind.Force;
		}
		//
		var level = self.Level;
		var weaponProps = new WeaponProperties(GetDieCount(self) + "d8", SpiritDamageKind);
		var weapon = new Item(
			IllustrationName.TempestTouch, Name,
			[Trait.Unarmed, Trait.Brawling, Trait.Agile, Trait.Finesse, Trait.Magical]
		).WithWeaponProperties(weaponProps);
		//
		self.AddQEffect(new QEffect(
			"Invoke Offense",
			"You manifest a physical attack of the spirits all around you",
			ExpirationCondition.Never, self, Icon
		) {
			Id = Effect,
			DoNotShowUpOverhead = true,
			AdditionalUnarmedStrike = weapon,
			StateCheck = delegate (QEffect qf) {
				if (!qf.Owner.HasEffect(SpiritTrance.Effect)) {
					qf.ExpiresAt = ExpirationCondition.Immediately;
				}
			}
		});
	}
	public static Feat CreateFeat() {
		return (
			new TrueFeat(FeatName, 4,
				"You manifest a physical attack of the spirits all around you, such as the claw of an animal spirit or the whipping vine of a nature spirit.",
				string.Join(" ", [
					"You gain an unarmed attack that deals 1d8 spirit damage for the duration of your spirit trance.",
					"This unarmed attack is in the brawling weapon group and has the agile, finesse, and magical traits.",
					"At 5th level, this unarmed attack gains the benefits of a striking rune.",
					"At 12th level, this unarmed attack gains the benefits of a greater striking rune.",
					"At 20th level, this unarmed attack gains the benefits of a major striking rune."
				]),
				[Trait.Morph, Trait.Spirit]
			))
			.WithAvailableAsArchetypeFeat(YALsSpiritInvoker.Trait)
			.WithActionCost(1)
			.WithPermanentQEffect(qfMain => {
				qfMain.ProvideMainAction = qf => {
					if (qf.Owner.HasEffect(Effect)) return null;
					var dieCount = GetDieCount(qf.Owner);
					return (ActionPossibility)new CombatAction(qf.Owner, Icon,
						"Invoke Offense",
						[Trait.Morph, Trait.Spirit],
						string.Join(" ", [
							"You manifest a physical attack of the spirits all around you, such as the claw of an animal spirit or the whipping vine of a nature spirit.",
							$"You gain an unarmed attack that deals {dieCount}d8 spirit damage for the duration of your spirit trance.",
							"This unarmed attack is in the brawling weapon group and has the agile, finesse, and magical traits.",
						]),
						Target.Self().WithAdditionalRestriction(SpiritTrance.MustBeInSpiritTrance)
					).WithEffectOnSelf(Apply);
				};
			}
		);
	}
}
