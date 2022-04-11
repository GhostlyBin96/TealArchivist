using System;

namespace ClassicTealArchivist
{
	public class PassiveAbility_CTABinBin_BlueReverberation : PassiveAbilityBase
	{
		public override string debugDesc
		{
			get
			{
				return "Immune to Range Attacks. Draw 2 Cards at the start of the Act. Take 1-2 less damage and Stagger damage from attacks.";
			}
		}
		public static string Desc = "Immune to Range Attacks. Draw 2 Cards at the start of the Act. Take 1-2 less damage and Stagger damage from attacks.";
		public override bool isImmuneByFarAtk
		{
			get
			{
				return true;
			}
		}
		public override void OnWaveStart()
		{
			base.OnWaveStart();
			this.owner.allyCardDetail.DrawCards(2);
		}
		public override int GetDamageReduction(BattleDiceBehavior behavior)
		{
			return RandomUtil.Range(1, 2);
		}
		public override int GetBreakDamageReduction(BattleDiceBehavior behavior)
		{
			return RandomUtil.Range(1, 2);
		}
	}
}
