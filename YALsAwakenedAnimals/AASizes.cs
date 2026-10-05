using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Squeezing;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace YALsAwakenedAnimals;

public class AASizes {
	public static Trait Trait = ModManager.RegisterTrait("Awakened Animal Size");

	public const string Title = "True Form";
	public const string Description = "Straighten your shoulders and show your true size";
	public const string EffectDescription = "Now that's better";
	public static readonly FeatName FeatName = AATools.RegisterFeatName(Title);
	public static readonly QEffectId EffectId = ModManager.RegisterEnumMember<QEffectId>(AATools.MakeKey("Grow"));
	public static readonly IllustrationName Icon = IllustrationName.MovementStar;

	public static Size GetCreatureSize(Creature creature) {
		var traits = creature.Traits;
		if (traits.Contains(Trait.Small)) return Size.Small;
		if (traits.Contains(Trait.Large)) return Size.Large;
		if (traits.Contains(Trait.Huge)) return Size.Huge;
		if (traits.Contains(Trait.Gargantuan)) return Size.Gargantuan;
		if (traits.Contains(Trait.Colossal5)) return Size.Colossal5;
		if (traits.Contains(Trait.Colossal6)) return Size.Colossal6;
		if (traits.Contains(Trait.Colossal7)) return Size.Colossal7;
		if (traits.Contains(Trait.Colossal8)) return Size.Colossal8;
		return Size.Medium;
	}

	public static async Task<bool> Grow(Creature creature) {
		var newSize = Size.Large;
		var form = await SizeChangeRules.EnlargeCreature(creature, creature, newSize,
			IllustrationName.None, Title, EffectDescription
		);
		if (form != null) {
			form.Traits.Remove(Trait.Polymorph);
			form.Id = EffectId;
			form.Tag = newSize;
			var wasLong = creature.Space.Long;
			creature.Space.Long = true;
			form.Dismissable = true;
			form.ProvideActionIntoPossibilitySection = (qf, section) => {
				if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) return null;
				return new ActionPossibility(new CombatAction(creature, Icon,
					"False Form",
					[Trait.Basic],
					"... and then you sometimes need to fit through a doorway",
					Target.Self()
				).WithActionCost(0).WithEffectOnSelf(async (action, creature) => {
					creature.Space.Long = wasLong;
					form.ExpiresAt = ExpirationCondition.Immediately;
				}));
			};
			return true;
		} else {
			//creature.Overhead("Nowhere to grow!", Color.Red);
			return false;
		}
	}

	public static Feat Register(string name, Trait size, int extraHP) {
		var text = $"Your size is {name} and your ancestry Hit Points are {8 + extraHP}.";
		if (size == Trait.Large) {
			text += "\nYou can switch to Medium size if the situation requires it.";
		}
		var feat = new Feat(
			ModManager.RegisterFeatName($"{YALsAwakenedAnimals.ModID}:Attack:{name}", name),
			null,
			text,
			[Trait],
			null
		).WithOnCreature(self => {
			self.MaxHP += extraHP;
			if (size == YALsAwakenedAnimals.TAdjusted) return;
			self.Traits.Add(size);
			if (size == Trait.Large) {
				self.AddQEffect(new QEffect(Title, Description) {
					Id = EffectId,
					ProvideActionIntoPossibilitySection = (qf, section) => {
						if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) return null;
						if (GetCreatureSize(qf.Owner) >= Size.Large) return null;
						return new ActionPossibility(new CombatAction(qf.Owner,
							Icon, Title,
							[YALsAwakenedAnimals.TAnimal],
							Description,
							Target.Self()
						).WithActionCost(0).WithEffectOnEachTarget(async (spell, caster, target, result) => {
							await Grow(target);
						}));
					},
					StartOfCombat = async qf => {
						if (await qf.Owner.AskForConfirmation(Icon, "Show your true size?", "Yeah")) {
							await Grow(qf.Owner);
						}
					}
				});
			}
		});
		ModManager.AddFeat(feat);
		return feat;
	}
	public static readonly Feat Small = Register("Small", Trait.Small, -2);
	public static readonly Feat Medium = Register("Medium", YALsAwakenedAnimals.TAdjusted, 0);
	public static readonly Feat Large = Register("Large", Trait.Large, 2);
	public static readonly List<Feat> List = [Small, Medium, Large];
	public static List<Feat> Init() => List;
}
