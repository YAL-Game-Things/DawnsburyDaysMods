using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Phases.Menus;
using Dawnsbury.Phases.Menus.CampaignViews;
using HarmonyLib;

namespace YALsRetrainAnytime;

public class YALsRetrainAnytime {
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		var harmony = new Harmony("YALsRetrainAnytime");
		harmony.PatchAll();
	}
}

[HarmonyPatch(typeof(CampaignMenuPhase), "CreateViewsWithKeepingIndex")]
public class CampaignMenuPhase_CreateViewsWithKeepingIndex {
	[HarmonyPostfix]
	public static void Postfix(CampaignMenuPhase __instance, bool keepCurrentIndex) {
		var self = __instance;
		if (!self.Views.Any(v => v is RetrainingView)) {
			self.Views.Add(new RetrainingView(self));
		}
		if (!self.Views.Any(v => v is ShopView)) {
			self.Views.RemoveAll(v => v is PartyView);
			self.Views.Add(new ShopView(self));
		}
	}
}