namespace Classic_Teal
{
    public class DiceCardAbility_CTABinBin_StealEnergy3 : DiceCardAbilityBase
	{
		public static string Desc = "[On Hit] Target loses 3 Light; Restore 3 Light";
		public override string[] Keywords
		{
			get
			{
				return new string[]
				{
				"Energy_Keyword"
				};
			}
		}
		public override void OnSucceedAttack(BattleUnitModel target)
		{
			base.owner.cardSlotDetail.RecoverPlayPointByCard(3);
			target.cardSlotDetail.LoseWhenStartRound(3);
		}
	}



}