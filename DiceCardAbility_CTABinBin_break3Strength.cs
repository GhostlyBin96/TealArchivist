namespace ClassicTealArchivist
{
    public class DiceCardAbility_CTABinBin_break3Strength : DiceCardAbilityBase
	{
		public static string Desc = "[On Hit] Deal 3 Stagger damage to target; Gain 1 Strength next Scene";
		public override string[] Keywords
		{
			get
			{
				return new string[]
				{
				"Strength_Keyword"
				};
			}
		}
		public override void OnSucceedAttack()
		{
			BattleUnitModel target = base.card.target;
			if (target == null)
			{
				return;
			}
			target.TakeBreakDamage(3, DamageType.Card_Ability, base.owner, AtkResist.Normal, KeywordBuf.None);
			base.owner.bufListDetail.AddKeywordBufByCard(KeywordBuf.Strength, 1, base.owner);
		}
	}
}