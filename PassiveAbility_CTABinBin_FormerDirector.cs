namespace Classic_Teal
{
    public class PassiveAbility_CTABinBin_FormerDirector : PassiveAbilityBase
	{
		public static string Desc = "At the end of each scene, all allies gain 1 Negative Emotion Coin. User’s Pages cost 1 less Light to play.";
		public override void OnRoundEnd() {
			foreach (BattleUnitModel battleUnitModel in BattleObjectManager.instance.GetAliveList(owner.faction)) {
				var createdCoin = battleUnitModel.emotionDetail.CreateEmotionCoin(EmotionCoinType.Negative, 1);
				SingletonBehavior<BattleManagerUI>.Instance.ui_battleEmotionCoinUI.OnAcquireCoin(battleUnitModel, EmotionCoinType.Negative, createdCoin);
			}
		}
		public override void OnRoundStart() {
			owner.bufListDetail.AddBuf(new BattleUnitBuf_costAllDown());
		}
		public class BattleUnitBuf_costAllDown : BattleUnitBuf
		{
			public override int GetCardCostAdder(BattleDiceCardModel card)
			{
				return -1;
			}
			public override void OnRoundEnd()
			{
				Destroy();
			}
		}
	}
}
