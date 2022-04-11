using System;
using System.Collections.Generic;
using LOR_DiceSystem;
using UnityEngine;

namespace ClassicTealArchivist
{
	public class PassiveAbility_CTABinBin_RedMist : PassiveAbility_250322
	{
		public override string debugDesc
		{
			get
			{
				return "Choose the Speed dice with the lowest value; the Speed values of the dice change to the maximum possible value. When using a Melee Combat Page, all dice on the page gain Power against targets with slower Speed. (+1 Power per 2 points of difference, up to 3)";
			}
		}
		public static string Desc = "Choose the Speed dice with the lowest value; the Speed values of the dice change to the maximum possible value. When using a Melee Combat Page, all dice on the page gain Power against targets with slower Speed. (+1 Power per 2 points of difference, up to 3)";

		public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
		{
			int speedDiceResultValue = curCard.speedDiceResultValue;
			BattleUnitModel target = curCard.target;
			int targetSlotOrder = curCard.targetSlotOrder;
			if (targetSlotOrder >= 0 && targetSlotOrder < target.speedDiceResult.Count)
			{
				SpeedDice speedDice = target.speedDiceResult[targetSlotOrder];
				if (speedDiceResultValue > speedDice.value)
				{
					if (curCard.card.GetSpec().Ranged == CardRange.Near)
					{
						int num = Mathf.Min(3, (speedDiceResultValue - speedDice.value) / 2);
						if (num > 0)
						{
							curCard.ApplyDiceStatBonus(DiceMatch.AllDice, new DiceStatBonus
							{
								power = num
							});
						}
					}
				}
			}
		}
	}
}
