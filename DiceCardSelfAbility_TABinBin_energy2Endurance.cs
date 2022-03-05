namespace TealArchivist
{
	public class DiceCardSelfAbility_TABinBin_energy2Endurance : DiceCardSelfAbilityBase
	{
		public static string Desc = "[On Use] Restore 2 Light; Gain 1 Endurance next scene";
		
		public override string[] Keywords
		{
			get
			{
				return new string[]
				{
				"Energy_Keyword",
				"Endurance_Keyword"
				};
			}
		}

		public override void OnUseCard()
		{
			base.owner.cardSlotDetail.RecoverPlayPointByCard(2);
			base.owner.bufListDetail.AddKeywordBufByCard(KeywordBuf.Endurance, 1, base.owner);
		}
	}
}