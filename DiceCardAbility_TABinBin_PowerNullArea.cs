namespace Classic_Teal
{
	public class DiceCardAbility_CTABinBin_PowerNullArea : DiceCardAbilityBase
	{
		public static string Desc = "[On Hit] Target's dice Power cannot be influenced by any effects next Scene";
		public override void OnSucceedAreaAttack(BattleUnitModel target)
		{
			if (target == null)
			{
				return;
			}
			target.bufListDetail.AddKeywordBufByCard(KeywordBuf.NullifyPower, 1, base.owner);
		}
	}
}