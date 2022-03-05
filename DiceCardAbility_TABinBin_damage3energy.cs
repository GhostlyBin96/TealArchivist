namespace TealArchivist
{
    public class DiceCardAbility_TABinBin_damage3energy : DiceCardAbilityBase
	{
		public static string Desc = "[On Hit] Deal 3 damage to target; Restore 1 Light";
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
		public override void OnSucceedAttack()
		{
			BattleUnitModel target = base.card.target;
			if (target == null)
			{
				return;
			}
			target.TakeDamage(3, DamageType.Card_Ability, base.owner, KeywordBuf.None);
			base.owner.cardSlotDetail.RecoverPlayPointByCard(1);
		}
	}
}