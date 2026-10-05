using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsAwakenedAnimals.Feats;

public class AAFeats {
	// 1
	public static readonly FeatName Lore = AATools.RegisterFeatName("Awakened Animal Lore");
	public static readonly FeatName ToothAndClaw = AATools.RegisterFeatName("Tooth and Claw");
	public static readonly FeatName AwakenedMagic = AATools.RegisterFeatName("Awakened Magic");
	public static readonly FeatName LandLegs = AATools.RegisterFeatName("Land Legs");
	// 5
	public static readonly FeatName LateAwakener = AATools.RegisterFeatName("Late Awakener");
	public static readonly FeatName WildStride = AATools.RegisterFeatName("Wild Stride");
	public static readonly FeatName NaturalAmbassador = AATools.RegisterFeatName("Natural Ambassador");
	public static readonly FeatName Scurry = AATools.RegisterFeatName("Scurry");
	public static readonly FeatName PackTactics = AATools.RegisterFeatName("Pack Tactics");
	// 9
	public static readonly FeatName AnimalSummoner = AATools.RegisterFeatName("Animal Summoner");
	//
	public static void Init() {
		var AAnimal = YALsAwakenedAnimals.TAnimal;
		var ModID = YALsAwakenedAnimals.ModID;
		var Adjusted = YALsAwakenedAnimals.TAdjusted;

		// Level 1
		ModManager.AddFeat(
			new TrueFeat(
				Lore, 1,
				"You have taken the time to learn about the process of awakening and the experiences of your fellow awakened animals.",
				"You gain trained proficiency in two skills related to your type of animal and/or experiences (commonly Arcana and Nature).",
				[AAnimal, Adjusted]
			).WithOnSheet(sheet => {
				sheet.AddSelectionOption(new MultipleFeatSelectionOption(
					AATools.MakeKey("AASkills"), "AA Lore skills", -1,
					feat => feat is SkillSelectionFeat, 2
				).WithIsOptional());
			})
		);
		ModManager.AddFeat(
			new TrueFeat(
				ToothAndClaw, 1,
				"You've become adept with your born weapons.",
				"Choose a second animal attack appropriate to your animal type and heritage.",
				[AAnimal]
			)
			.WithOnSheet(sheet => {
				sheet.AddSelectionOption(new SingleFeatSelectionOption($"{ModID}:ToothAndClaw:Attack",
					"Additional Attack", 1,
					feat => feat.HasTrait(AAAttacks.TAttack)
				));
			})
		);
		ModManager.AddFeat(new TrueFeat(
			AwakenedMagic, 1,
			"When you awakened, primal magic was released within you.",
			string.Join(" ", [
				"Choose one cantrip from the primal spell list.",
				"You can cast this spell as an primal innate spell at will.",
				"A cantrip is heightened to a spell rank equal to half your level rounded up.",
			]),
			[AAnimal]
		).WithOnSheet(sheet => {
			sheet.SetProficiency(Trait.Spell, Proficiency.Trained);
			sheet.InnateSpells.GetOrCreate(AAnimal, () => new InnateSpells(Trait.Primal));
			sheet.AddSelectionOption(new AddInnateSpellOption(
				"AwakenedMagicCantrip",
				"Awakened Magic cantrip", -1,
				AAnimal, 0,
				spell => spell.HasTrait(Trait.Primal)
			));
		}));
		ModManager.AddFeat(
			new TrueFeat(
				LandLegs, 1,
				"You become more comfortable with moving on land.",
				"Your Speed increases by 5 feet.",
				[AAnimal, Trait.Homebrew]
			).WithPrerequisite(sheet => {
				return sheet.HasFeat(AAHeritages.WaterDwelling) || sheet.HasFeat(AAHeritages.Flying);
			}, "You have the Wanter-dwelling Animal or Flying Animal heritage").WithOnCreature(self => {
				self.AddQEffect(new QEffect {
					BonusToAllSpeeds = qf => new Bonus(1, BonusType.Untyped, "Land Legs")
				});
			})
		);

		// Level 5
		ModManager.AddFeat(new TrueFeat(
			LateAwakener, 5,
			"Whether due to a more gradual process or your animal and sapient sides being especially in conflict, your awakening was a little slower than most, but your eyes are now fully open and your abilities have returned.",
			"You gain all the mechanical benefits of the awakened animal heritage you selected at 1st level, allowing you to take feats and gain any benefits that require a specific awakened animal heritage.",
			[AAnimal], AAHeritages.Feats
		).WithEquivalent(sheet => {
			foreach (var fn in AAHeritages.Names) {
				if (sheet.HasFeat(fn)) return true;
			}
			return false;
		}));

		ModManager.AddFeat(new TrueFeat(
			Scurry, 5,
			"Your instincts to move to safety are strong.",
			string.Join("\n", [
				"{b}Trigger{/b} You roll initiative.",
				"You Stride; you must end your movement in a location where you have cover from at least one enemy you can see, and you can't take this action if it's impossible for you to do so."
			]),
			[AAnimal]
		).WithActionCost(0).WithPermanentQEffect(qfx => {
			qfx.StartOfCombat = async qf => {
				await qf.Owner.StrideAsync(
					string.Join("\n", [
						"Choose where to Scurry.",
						"You must end your movement in a location where you have cover from at least one enemy you can see"
					]),
					allowStep: true,
					allowCancel: true
				);
			};
		}));

		var wildStrideText = string.Join("\n", [
			"{b}Prerequisites{/b} You have a free hand.",
			"Stride twice. You gain a +5-foot circumstance bonus to your Speed for these Strides, or a +10-foot circumstance bonus if you have two hands free."
		]);
		ModManager.AddFeat(new TrueFeat(
			WildStride, 5,
			"You can move quickly when using your preferred modes of travel.",
			wildStrideText,
			[AAnimal]
		).WithActionCost(2).WithPermanentQEffect(null, qfx => {
			qfx.ProvideActionIntoPossibilitySection = (qf, section) => {
				if (section.PossibilitySectionId != PossibilitySectionId.Movement) return null;
				var action = new CombatAction(qf.Owner,
					IllustrationName.FleetStep, "Wild Stride",
					[Trait.Move],
					wildStrideText,
					Target.Self().WithAdditionalRestriction(self => {
						if (!self.HasFreeHand) return "Need a free hand!";
						return null;
					})
				)
				.WithActionCost(2)
				.WithSoundEffect(Dawnsbury.Audio.SfxName.Footsteps)
				.WithEffectOnSelf(async (action, self) => {
					var bonus = new QEffect {
						BonusToAllSpeeds = qf => new Bonus(2, BonusType.Circumstance, "Wild Stride")
					};
					self.AddQEffect(bonus);
					if (await self.StrideAsync(
						"Choose where to Stride with Wild Stride. (1/2)",
						allowStep: false,
						allowCancel: true
					)) {
						if (!await self.StrideAsync(
							"Choose where to Stride with Wild Stride. (2/2)",
							allowStep: false,
							allowCancel: true
						)) {
							action.RevertRequested = true;
							action.SpentActions = 2;
						}
					} else action.RevertRequested = true;
					bonus.ExpiresAt = ExpirationCondition.Immediately;
				});
				return new ActionPossibility(action);
			};
		}));

		ModManager.AddFeat(new TrueFeat(
			NaturalAmbassador, 5,
			"You remember what it was like to speak easily with other animals, using old, familiar ways of communication to learn what they want and ask for favors in return.",
			"You can ask questions of, receive answers from, and use the Diplomacy skill with all animals, as in Speak with Animals.",
			[AAnimal]
		).WithOnCreature(self => {
			Level2Spells.GiveSpeakWithAnimals(self);
		}));

		ModManager.AddFeat(new TrueFeat(
			PackTactics, 5,
			"You are adept at working with your allies to surround an enemy.",
			"If an enemy is within reach of you and at least two of your allies, that enemy is off-guard to you.",
			[AAnimal]
		).WithPermanentQEffect(qfx => {
			qfx.StateCheck = qf => {
				// like GameLoop.RecalculateFlankingForCore
				var self = qf.Owner;
				var battle = self.Battle;
				foreach (var enemy in self.Cache.EnemiesWithinActualReach) {
					if (enemy == self || !enemy.EnemyOf(self)) continue;
					var friendCount = 0;
					foreach (var ally in battle.AllCreatures) {
						if (ally == self || ally.EnemyOf(self)) continue;
						if (!ally.Actions.CanTakeActions()) continue;
						if (!FlankingRules.SimplifiedCanAttack(ally)) continue;
						if (!ally.Cache.EnemiesWithinActualReach.Contains(enemy)) continue;
						if (++friendCount < 2) continue;
						//
						enemy.AddQEffect(new QEffect(
							"Pack Tactics",
							$"You're flat-footed to {self}.",
							ExpirationCondition.Ephemeral,
							self,
							IllustrationName.Flatfooted
						) {
							DoNotShowUpOverhead = true,
							IsFlatFootedTo = (qf, attacker, action) => {
								if (attacker == self) {
									return "Pack Tactics";
								} else return null;
							},
						});
						break;
					}
				}
			};
		}));

		// level 9
		ModManager.AddFeat(new TrueFeat(
			AnimalSummoner, 9,
			"The connection between you and primal magic deepens.",
			"You can cast summon animal as a primal innate spell once per day, heightened to half your level rounded up.",
			[AAnimal]
		).WithPermanentQEffect(qfx => {
			qfx.ProvideMainAction = qf => {
				var self = qf.Owner;
				var key = YALsAwakenedAnimals.ModID + ":AnimalSummoner";
				if (self.PersistentUsedUpResources.UsedUpActions.Contains(key)) return null;
				var action = AllSpells.CreateSpellInCombat(SpellId.SummonAnimal, self, self.MaximumSpellRank, AAnimal);
				action.WithEffectOnEachTarget(async (spell, caster, target, result) => {
					caster.PersistentUsedUpResources.UsedUpActions.Add(key);
				});
				return (Possibilities.CreateSpellPossibility(action)
					.WithPossibilitySize(PossibilitySize.Full)
					.WithPossibilityGroup("Abilities")
				);
			};
		}).WithPrerequisite(AwakenedMagic, "Awakened Magic"));
	}
}
