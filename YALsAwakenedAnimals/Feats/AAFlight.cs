using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;

namespace YALsAwakenedAnimals.Feats;

public class AAFlight {
	public static readonly FeatName TakeFlight = AATools.RegisterFeatName("Take Flight");
	public static readonly QEffectId TakeFlightCooldown = ModManager.RegisterEnumMember<QEffectId>(AATools.MakeKey("TakeFlightCooldown"));
	public static readonly FeatName StrongOfWing = AATools.RegisterFeatName("Strong of Wing");
	public static readonly FeatName FullFlight = AATools.RegisterFeatName("Full Flight");

	public static void Init() {
		var AAnimal = YALsAwakenedAnimals.TAnimal;
		var Adjusted = YALsAwakenedAnimals.TAdjusted;
		ModManager.AddFeat(new TrueFeat(
			TakeFlight, 1,
			"Though it's not fully effective, much of your ability to fly has stayed with you.",
			string.Join("\n", [
				"{b}Frequency{/b} once per round",
				"{b}Prerequisites{/b} permanent wings",
				string.Join(" ", [
					"You Fly in a straight line.",
					"If you don't normally have a fly Speed, you gain a fly Speed of 15 feet for this movement.",
					"During this movement you can pass over enemies and most terrain."
				])
			]),
			[AAnimal, Adjusted]
		).WithActionCost(1).WithOnCreature(creature => {
			creature.AddQEffect(new QEffect() {
				ProvideActionIntoPossibilitySection = (qf, section) => {
					if (section.PossibilitySectionId != PossibilitySectionId.Movement) return null;
					var self = qf.Owner;
					var distance = self.HasFeat(StrongOfWing) ? 5 : 3;
					if (self.HasEffect(QEffectId.Flying)) {
						if (distance < self.Speed) {
							distance = self.Speed;
						}
					}
					var distStr = S.HeightenedVariable(distance * 5, 15);
					var desc = string.Join("\n", [
						"{b}Frequency{/b} once per round",
						string.Join(" ", [
							"You Fly in a straight line.",
							$"You have a fly Speed of {distStr} feet for this movement.",
							"During this movement you can pass over enemies and most terrain."
						])
					]);
					return new ActionPossibility(
						new CombatAction(self,
							IllustrationName.Fly, "Take Flight",
							[Trait.Move, Trait.Basic, Trait.ProvokesAsActionBegins],
							desc,
							new TileTarget((hopper, tile) => {
								return (hopper.Occupies != null
									&& tile.IsTrulyGenuinelyFreeTo(hopper)
									&& hopper.DistanceTo(tile) <= distance
									&& hopper.Occupies.HasLineOfEffectToIgnoreLesser(tile) != CoverKind.Blocked
								);
							}, null).WithAdditionalSelfRequirement(self => {
								if (self.HasEffect(TakeFlightCooldown)) {
									return Usability.NotUsable("Already used this round!");
								} else return Usability.Usable;
							})
						)
						.WithActionId(ActionId.Leap)
						.WithActionCost(1)
						.WithEffectOnChosenTargets(async (action, creature, target) => {
							if (target.ChosenTile == null) return;
							await creature.SingleTileMove(target.ChosenTile, action);
							creature.AddQEffect(new QEffect() {
								Id = TakeFlightCooldown,
								ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
							});
						})
					);
				}
			});
		}));
		ModManager.AddFeat(
			new TrueFeat(
				StrongOfWing, 5,
				"You can fly with a greater distance.",
				"The fly Speed you gain from Take Flight increases to 25 feet.",
				[AAnimal, Adjusted]
			)
			.WithPrerequisite(TakeFlight, "Take Flight")
		);
		ModManager.AddFeat(
			new TrueFeat(
				FullFlight, 9,
				"Your flight is unbound, as natural as it was before you were awakened.",
				"Your Speed increases by 5 feet, and you have a fly Speed at all times.",
				[AAnimal, Adjusted]
			)
			.WithOnCreature(self => {
				self.AddQEffect(QEffect.Flying());
				self.AddQEffect(new QEffect {
					BonusToAllSpeeds = qf => new Bonus(1, BonusType.Untyped, "Full Flight")
				});
			})
			.WithPrerequisite(StrongOfWing, "Strong of Wing")
		);
	}
}
