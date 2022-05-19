using System;

namespace Classic_Teal
{
	public class PassiveAbility_CTABinBin_PurpleTear : PassiveAbilityBase
	{
		public override string debugDesc
		{
			get
			{
				return "Defensive Dice Power +2. When inflicted with a status ailment, reduce the amount by half. (Rounded down, does not go below 1)";
			}
		}
		public static string Desc = "Defensive Dice Power +2. When inflicted with a status ailment, reduce the amount by half. (Rounded down, does not go below 1)";
		public override int OnAddKeywordBufByCard(BattleUnitBuf buf, int stack) => buf.positiveType == BufPositiveType.Negative ? -stack / 2 : 0;
		public override void BeforeRollDice(BattleDiceBehavior behavior)
		{
			if (base.IsDefenseDice(behavior.Detail))
			{
				BattleCardTotalResult battleCardResultLog = this.owner.battleCardResultLog;
				if (battleCardResultLog != null)
				{
					battleCardResultLog.SetPassiveAbility(this);
				}
				behavior.ApplyDiceStatBonus(new DiceStatBonus
				{
					power = 2
				});
			}
		}
	}
}
