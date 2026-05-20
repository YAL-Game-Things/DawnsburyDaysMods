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
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsSpiritInvoker;

public class InvokeDefense {
	public const string ID = "InvokeDefense";
	public const string Name = "Invoke Defense";
	public static readonly FeatName FeatName = YALsSpiritInvoker.RegisterFeatName(ID, Name);
	public static readonly IllustrationName Icon = IllustrationName.ShieldSpell;
	public static readonly QEffectId Effect = YALsSpiritInvoker.RegisterEnumMember<QEffectId>(ID);
	//
	public static int GetResistance(Creature self) {
		return self.Level / 2 * 10;
	}
	public static void Apply(Creature self, DamageKind damageKind) {
		foreach (var qf in self.QEffects) {
			if (qf.Id == Effect) qf.ExpiresAt = ExpirationCondition.Immediately;
		}
		self.AddQEffect(new QEffect(
			$"{Name} ({damageKind})",
			$"You gain resistance {GetResistance(self)} to {damageKind} for the duration of your spirit trance.",
			ExpirationCondition.Never, self, Icon
		) {
			Id = Effect,
			DoNotShowUpOverhead = true,
			StateCheck = delegate (QEffect qf) {
				qf.Owner.WeaknessAndResistance.AddResistance(damageKind, GetResistance(qf.Owner));
				if (!qf.Owner.HasEffect(SpiritTrance.Effect)) {
					qf.ExpiresAt = ExpirationCondition.Immediately;
				}
			}
		});
	}
	public static Feat CreateFeat() {
		return (
			new TrueFeat(FeatName, 8,
				"You manifest a defensive quality of spirits all around, such as the thick hide of an animal spirit or the sturdy bark of a nature spirit.",
				string.Join(" ", [
					"Choose bludgeoning, piercing, or slashing.",
					"You gain resistance equal to half your level to that damage type for the duration of your spirit trance.",
					"If you use Invoke Defense again, you can choose a different type of damage, but you lose the previous resistance."
				]),
				[Trait.Morph]
			))
			.WithAvailableAsArchetypeFeat(YALsSpiritInvoker.Trait)
			.WithActionCost(1)
			.WithPermanentQEffect(qfMain => {
				qfMain.ProvideActionIntoPossibilitySection = (qf, section) => {
					if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) return null;
					var submenu = new SubmenuPossibility(Icon, Name);
					var possibilitySection = new PossibilitySection(Name);
					DamageKind[] damageKinds = [DamageKind.Bludgeoning, DamageKind.Piercing, DamageKind.Slashing];
					var resistance = GetResistance(qf.Owner);
					foreach (var damageKind in damageKinds) {
						possibilitySection.AddPossibility((ActionPossibility)new CombatAction(
							qf.Owner, Icon, $"{Name} ({damageKind})",
							[Trait.Morph],
							string.Join(" ", [
								$"You gain resistance {GetResistance(qf.Owner)} to {damageKind} for the duration of your spirit trance.",
								"If you use Invoke Defense again, you can choose a different type of damage, but you lose the previous resistance."
							]),
							Target.Self().WithAdditionalRestriction(SpiritTrance.MustBeInSpiritTrance)
						).WithEffectOnSelf((self) => {
							Apply(self, damageKind);
						}));
					}
					submenu.Subsections.Add(possibilitySection);
					return submenu;
				};
			}
		);
	}
}
