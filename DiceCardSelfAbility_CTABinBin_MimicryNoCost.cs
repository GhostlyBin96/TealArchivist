namespace Classic_Teal
{
	public class DiceCardSelfAbility_CTABinBin_MimicryNoCost : DiceCardSelfAbilityBase
	{
		public static string Desc = "The Cost of this page cannot be changed; On hit recover HP equal to the damage dealt";
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
		public override void OnSucceedAttack(BattleDiceBehavior behavior)
		{
			int diceResultDamage = behavior.DiceResultDamage;
			if (diceResultDamage > 0)
			{
				base.owner.RecoverHP(diceResultDamage);
			}
		}
		public override bool IsFixedCost()
		{
			return true;
		}
	}
}