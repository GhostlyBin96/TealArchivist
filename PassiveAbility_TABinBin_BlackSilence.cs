using System;

namespace Teal_Archivist
{
	public class PassiveAbility_TABinBin_BlackSilence : PassiveAbility_10013
	{
		public override string debugDesc
		{
			get
			{
				return "At the start of the Scene, draw until 5 pages are in hand. All dice on every third Combat Page used gain +2 Power.";
			}
		}
		public static string Desc = "At the start of the Scene, draw until 5 pages are in hand. All dice on every third Combat Page used gain +2 Power.";

		public override void OnWaveStart() {}
		public override void OnRoundStart()
		{
			int count = this.owner.allyCardDetail.GetHand().Count;
			int num = 5 - count;
			if (num > 0)
			{
				this.owner.allyCardDetail.DrawCards(num);
			}
			base.OnRoundStart();
		}
	}
}
