using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;

namespace YALsBlisterBomb;

public class YALsBlisterBomb {
	public static readonly QEffectId EffectId = ModManager.RegisterEnumMember<QEffectId>("YALsBlisterBomb:Effect");
	public static readonly QEffectId Immunity = ModManager.RegisterEnumMember<QEffectId>("YALsBlisterBomb:Immune");
	public static Affliction BlisterPox(int dc) {
		return new Affliction(EffectId,
			"Blister Bomb", dc,
			string.Join("; ", [
				"{b}Stage 1{b} sickened 2 (1 round)",
				"Stage 2 sickened 2 and 1d6 persistent bleed damage (1 round)",
				"Stage 3 sickened 2 and 2d6 persistent bleed damage (1 round)",
				"Stage 4 sickened 3 and 2d6 persistent bleed damage (1 day)"
			]), 4, stage => null, null
		) {
			MaximumDuration = 20,
			EnterStage = async (qf, action) => {
				var stage = qf.Value;
				var victim = qf.Owner;
				if (stage > 0) {
					victim.AddQEffect(QEffect.Sickened(stage >= 4 ? 3 : 2, dc));
				}
				if (stage > 1) {
					var damage = (stage - 1) + "d6";
					victim.AddQEffect(QEffect.PersistentDamage(damage, DamageKind.Bleed));
				}
			}
		};
	}
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		ModManager.RegisterNewSpell("YALsBlisterBomb", 3, (spellID, caster, rank, inCombat, info) => {
			return Spells.CreateModern(
				IllustrationName.AlchemicalPoison,
				"Blister Bomb",
				[
					Trait.Concentrate, Trait.Disease, Trait.Manipulate,
					Trait.Arcane, Trait.Primal,
				],
				"You launch a small bomb enchanted with a fast-acting skin disease at your foes, causing their skin to break out in horrible bleeding sores.",
				("All creatures in the area of the burst must attempt a Fortitude save." + S.FourDegreesOfSuccess(
					"The creature is unaffected and immune to blister pox for 1 week.",
					"The creature is sickened 2.",
					"The creature is afflicted with blister pox at stage 1.",
					"The creature is afflicted with blister pox at stage 2."
				)),
				Target.Burst(20, 1),
				rank,
				SpellSavingThrow.Standard(Defense.Fortitude)
			)
			.WithSoundEffect(Dawnsbury.Audio.SfxName.Necromancy)
			.WithEffectOnEachTarget(async (spell, caster, target, result) => {
				var stage = result switch {
					CheckResult.Failure => 1,
					CheckResult.CriticalFailure => 2,
					CheckResult.Success => 0,
					_ => -1,
				};
				var dc = spell.SpellcastingSource?.GetSpellSaveDC() ?? 0;
				if (stage < 0 && !target.HasEffect(Immunity)) {
					target.AddQEffect(new QEffect { Id = Immunity });
				}
				if (stage >= 0) {
					target.AddQEffect(QEffect.Sickened(2, dc));
				}
				if (stage > 0 && !target.HasEffect(Immunity)) {
					await Affliction.ApplyInjuryPoison(BlisterPox(dc), caster, target, result);
				}
			});
		});
	}
}