using Dawnsbury.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsAwakenedAnimals;

public class AAMind {
	public static readonly QEffectId UntagAnimal = ModManager.RegisterEnumMember<QEffectId>(AATools.MakeKey("UntagAnimal"));
	public static void Init(QEffect qfx) {
		qfx.StartOfCombat = async qf => {
			var animal = qf.Owner;
			var allies = animal.Battle.AllCreatures.Where(cr => cr.OwningFaction == animal.OwningFaction);
			foreach (var ally in allies) {
				ally.AddQEffect(new QEffect() {
					ProvideActionIntoPossibilitySection = (qf, section) => {
						if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) return null;
						var action = new CombatAction(
							qf.Owner, animal.Illustration,
							"Count as Animal",
							[YALsAwakenedAnimals.TAnimal],
							$"Makes {animal.Name} count as Animal until the end of your turn.",
							Target.Self()
						).WithActionCost(0).WithEffectOnSelf(self => {
							if (animal.HasTrait(Trait.Animal)) {
								var untag = animal.FindQEffect(UntagAnimal);
								if (untag != null) untag.ExpiresAt = ExpirationCondition.Immediately;
							} else {
								animal.Traits.Add(Trait.Animal);
								animal.AddQEffect(new QEffect("Counts as Animal", "?", IllustrationName.AnimalForm) {
									ExpiresAt = ExpirationCondition.ExpiresAtEndOfAnyTurn,
									Id = UntagAnimal,
									WhenExpires = qf => {
										animal.Traits.Remove(Trait.Animal);
									}
								});
							}
						});
						return new ActionPossibility(action, PossibilitySize.Half);
					}
				});
			}
		};
	}
}
