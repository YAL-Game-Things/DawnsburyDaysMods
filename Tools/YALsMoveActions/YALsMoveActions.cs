using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Tiles;
using System.Reflection;

public class YALsMoveActions {
	public static async Task<bool> StrideOrStepAdvancedAsync(
		Creature self,
		string topBarText,
		bool allowStep = false,
		Tile? strideTowards = null,
		bool allowCancel = false,
		bool allowPass = false,
		bool allowStride = true,
		int? maximumSpeed = null,
		string? passText = null,
		Func<Tile, bool>? permissibleTarget = null
	) {
		var maybeMethod = self.GetType().GetMethod("StrideOrStepAdvancedAsync",
			BindingFlags.NonPublic | BindingFlags.Instance,
			[
				typeof(string), // topBarText
				typeof(bool), // allowStep
				typeof(Tile), // strideTowards
				typeof(bool), // allowCancel
				typeof(bool), // allowPass
				typeof(bool), // allowStride
				typeof(int?), // maximumSpeed
				typeof(string), // passText
				typeof(Func<Tile,bool>), // permissibleTarget
			]
		);
		if (maybeMethod is MethodInfo method) {
			return await (Task<bool>)method.Invoke(self, [
				topBarText,
				allowStep,
				strideTowards,
				allowCancel,
				allowPass,
				allowStride,
				maximumSpeed,
				passText,
				permissibleTarget,
			])!;
		}
		// Well, the signature must have changed, guess we'll just...
		#region Original code
		bool allowPassOrCancel = allowPass || allowCancel;
		bool shouldAbortMove = allowPassOrCancel;
		if (shouldAbortMove) {
			shouldAbortMove = await CommonQuestions.ShouldAbortMove(self, IllustrationName.WarpStep);
		}
		if (shouldAbortMove) return false;
		if (!self.Alive) return true;
		if (!maximumSpeed.HasValue) {
			int value = self.Speed;
			maximumSpeed = value;
		}
		self.Actions.NextStrideIsFree = true;
		self.RegeneratePossibilities();
		var possibilities = self.Possibilities.CreateActions(usableOnly: true);
		var options = new List<Option>();
		Option? selectedOption;
		if (self.Speed > 0) {
			IList<Tile> reachableTiles = Pathfinding.Floodfill(self, self.Battle, new PathfindingDescription {
				Squares = maximumSpeed.Value,
				Style = { PermitsStep = allowStep }
			});
			if (strideTowards != null) {
				var minimumDistanceToTarget = reachableTiles.Min((Tile tl) => tl.DistanceTo(strideTowards));
				reachableTiles = reachableTiles.Where((Tile tl) => tl.DistanceTo(strideTowards) == minimumDistanceToTarget).ToList();
				var minimumDistanceToSource = reachableTiles.Min((Tile tl) => tl.DistanceTo(self));
				reachableTiles = reachableTiles.Where((Tile tl) => tl.DistanceTo(self) == minimumDistanceToSource).ToList();
			}
			if (permissibleTarget != null) {
				reachableTiles = reachableTiles.Where(permissibleTarget).ToList();
			}
			foreach (var tile in reachableTiles) {
				if (tile != self.Space.TopLeftTile) {
					if (allowStep && tile.InIteration.Steppable && possibilities.FirstOrDefault((ICombatAction pw) => pw.Action.ActionId == ActionId.Step) is CombatAction powerShift && (bool)powerShift.Target.CanBeginToUse(self)) {
						options.Add(powerShift.CreateUseOptionOn(tile).WithIllustration(powerShift.Illustration));
					} else if (allowStride && possibilities.FirstOrDefault((ICombatAction pw) => pw.Action.ActionId == ActionId.Stride) is CombatAction powerWalk && (bool)powerWalk.Target.CanBeginToUse(self)) {
						Option stride = powerWalk.CreateUseOptionOn(tile).WithIllustration(powerWalk.Illustration);
						options.Add(stride);
					}
				}
			}
			if (allowCancel) {
				options.Add(new CancelOption(midspell: true));
			}
			if (allowPass) {
				options.Add(new PassViaButtonOption(passText ?? ("Confirm no additional " + (allowStride ? "stride" : "step"))));
			}
			selectedOption = null;
			if (options.Count == 1) {
				selectedOption = options[0];
			} else if (options.Count > 0) {
				if (self.HasEffect(QEffectId.AiDoNotStrideAfterEscape)) {
					var passOption = options.FirstOrDefault((Option opt) => opt is PassViaButtonOption);
					if (passOption != null) {
						selectedOption = passOption;
						goto IL_04e0;
					}
				}
				selectedOption = (await self.Battle.SendRequest(new AdvancedRequest(self, topBarText, options) {
					IsMainTurn = false,
					IsStandardMovementRequest = true,
					TopBarIcon = IllustrationName.WarpStep,
					TopBarText = topBarText,
					DisplacedCreature = self
				})).ChosenOption;
			}
			goto IL_04e0;
		}
		goto IL_05a1;
		IL_04e0:
		if (selectedOption == null) {
			self.Actions.NextStrideIsFree = false;
			return true;
		}
		await selectedOption.Action();
		self.Battle.MovementConfirmer = null;
		if (selectedOption is CancelOption || selectedOption is PassViaButtonOption) {
			self.Actions.NextStrideIsFree = false;
			return false;
		}
		goto IL_05a1;
		IL_05a1:
		self.Actions.NextStrideIsFree = false;
		return true;
		#endregion
	}
}