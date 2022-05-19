using System;

namespace Classic_Teal
{
	public class PassiveAbility_CTABinBin_SpeedV : PassiveAbilityBase
	{
		public override string debugDesc
		{
			get
			{
				return "Speed Dice Slot +2. Gain an additional Speed die at Emotion Level 3. (Cannot Overlap)";
			}
		}
		public static string Desc = "Gives +2 Speed Die at the Start of the Act, Gain +1 Additional Speed Die at Emotion Level 3. (Cannot Overlap)";
		public override int SpeedDiceNumAdder()
		{
			if (this.owner == null)
			{
				return 2;
			}
			BattleUnitEmotionDetail emotionDetail = this.owner.emotionDetail;
			int? num = (emotionDetail != null) ? new int?(emotionDetail.EmotionLevel) : null;
			if ((num.GetValueOrDefault() >= 3 & num != null))
			{
				return 3;
			}
			return 2;
		}
	}
}