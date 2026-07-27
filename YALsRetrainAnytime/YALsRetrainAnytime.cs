using Dawnsbury.IO;
using Dawnsbury.Modding;

namespace YALsRetrainAnytime;

public class YALsRetrainAnytime {
	[DawnsburyDaysModMainMethod]
	public static void LoadMod() {
		// no longer necessary!
		PlayerProfile.Instance.RetrainAnytime = true;
	}
}
