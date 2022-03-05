using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using System.Linq;
using CustomMapUtility;
using UI;
using UnityEngine;
using HarmonyLib;
using Sound;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

#if DEBUG
#warning DEBUG
#endif

namespace Teal_Archivist
{
	public static class Globals {
		public const string ModId = "TealArchivist";
	}
    public class PassiveAbility_TABinBin_LostLibrary : PassiveAbilityBase {
		#if DEBUG
        	readonly public DebugInfo debugInfo = new DebugInfo();
			public class DebugInfo {
				public StageController StageController => Singleton<StageController>.Instance;
				public List<UnitBattleDataModel> UnitBattleDataList => StageController.GetCurrentStageFloorModel().GetUnitBattleDataList();
			}
		#endif
		public static string Desc = "LostLibrary";
		public Dictionary<BattleUnitModel, BattleUnitView.SkinInfo> oldList;
		public readonly List<BattleUnitModel> newList = new List<BattleUnitModel>(4);
		private int oldIndex;
		public FormationPosition oldFormationPos;
		private bool isSephirah = true;
		public StageModel Stage => Singleton<StageController>.Instance.GetStageModel();
		public SephirahType CurrentFloor => Singleton<StageController>.Instance.CurrentFloor;
		public LibraryFloorModel Floor => LibraryModel.Instance.GetFloor(CurrentFloor);
		int oldSelectEmotionLevel;
		Dictionary<Units, UnitBattleDataModel> dataList;
		public int currentIndex;
		public BattleUnitBuf_LostLibrary currentBuf;
		bool init = false;
		// bool music = true;
		// bool isKnockoutInsteadOfDeath;
		public bool imminentDeath = false;
		public override bool isImmortal => init && imminentDeath || base.isImmortal;
		
		public enum Units {
			PLUGIN = -1,
			TealArchivist = 0,
			Roland,		// Card 22
			Geburah,	// Card 23
			PurpleTear,	// Card 24
			Uberto,		// Card 25
		}
		public static readonly List<LorId> cardList = new List<LorId>{
			new LorId(Globals.ModId, 21), // Desynchronize
			new LorId(Globals.ModId, 22), // Roland
			new LorId(Globals.ModId, 23), // Geburah
			// new LorId(Globals.ModId, 24), // PurpleTear
			// new LorId(Globals.ModId, 25), // Uberto
		};

		public override void OnUnitCreated() {
			owner.personalEgoDetail.AddCard(new LorId(Globals.ModId, 20)); // LostLibrary card
		}
		public override void OnWaveStart() {
			CustomMapHandler.InitCustomMap<LostLibraryMapManager>("LostLibrary", isEgo: true);
		}
		public void InitLostLibrary() {
			if (init) {return;}

			// isKnockoutInsteadOfDeath = owner.IsKnockoutInstaedOfDeath();
			// owner.SetKnockoutInsteadOfDeath(true);
			oldList = new Dictionary<BattleUnitModel, BattleUnitView.SkinInfo>();
			foreach (var model in BattleObjectManager.instance.GetList(owner.faction)) {
				if (model == owner) {
					continue;
				}
				oldList.Add(model, model.view.GetCurrentSkinInfo());
			}
			oldIndex = owner.index;
			oldFormationPos = owner.formation;

			currentIndex = 1;
            StageModel stage = Stage;
            LibraryFloorModel floor = Floor;
			dataList = new Dictionary<Units, UnitBattleDataModel>() {
				{Units.TealArchivist, owner.UnitData},
				{Units.Roland, UnitData.CreateRolandData(stage, floor)},
				{Units.Geburah, UnitData.CreateGeburahData(stage, floor)},
			};

			foreach (var model in oldList.Keys) {
				if (model.index == 0) {
					owner.index = 0;
					isSephirah = false;
					owner.UnitData.unitData.isSephirah = true;
					owner.formation = model.formation;
				}
				BattleObjectManager.instance.UnregisterUnit(model);
			}

			owner.moveDetail.ReturnToFormationByBlink();
			CustomMapHandler.AddCustomEgoMapByAssimilation<LostLibraryMapManager>("LostLibrary", owner.faction);

			StageLibraryFloorModel currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
			oldSelectEmotionLevel = currentStageFloorModel.team.currentSelectEmotionLevel;
			currentStageFloorModel.team.currentSelectEmotionLevel = int.MaxValue;
			init = true;
		}

		public void RemoveLostLibrary() {
			if (!init) {return;}

			foreach (var card in cardList) {
				owner.personalEgoDetail.RemoveCard(card);
			}
			#if DEBUG
			owner.personalEgoDetail.AddCard(new LorId(Globals.ModId, 20)); // LostLibrary card
			#endif

			foreach (var model in newList) {
				BattleObjectManager.instance.UnregisterUnit(model);
			}

			if (!isSephirah) {
				owner.index = oldIndex;
				owner.UnitData.unitData.isSephirah = false;
				owner.formation = oldFormationPos;
			}

			if (!imminentDeath) {
				owner.moveDetail.ReturnToFormationByBlink();
			}
			CustomMapHandler.RemoveCustomEgoMapByAssimilation("LostLibrary");
			var stageController = Singleton<StageController>.Instance;
			// int emotionTotalCoinNumberWithBonus = stageController.GetCurrentStageFloorModel().team.emotionTotalCoinNumberWithBonus;
			// int emotionTotalCoinNumberWithBonus2 = stageController.GetCurrentWaveModel().team.emotionTotalCoinNumberWithBonus;
			// stageController.GetCurrentWaveModel().team.emotionTotalBonus += emotionTotalCoinNumberWithBonus2 - emotionTotalCoinNumberWithBonus;
			// stageController.CheckMapChange();
			SingletonBehavior<BattleSceneRoot>.Instance.ChangeToSephirahMap(stageController.CurrentFloor, true);
			// SingletonBehavior<BattleSoundManager>.Instance.EndBgm();
			// music = false;

			foreach (var unit in oldList) {
				var model = unit.Key;
				BattleObjectManager.instance.RegisterUnit(model);
				model.view.ChangeSkinBySkinInfo(unit.Value);
				if (model.IsDeadReal() && !model.IsDeadSceneBlock) {
					model.view.StartDeadEffect(false);
				}
				if (LostLibraryPluginHandler.customReInit.Count != 0) {
					foreach (var passive in model.passiveDetail.PassiveList) {
						var passiveType = passive.GetType();
						if (LostLibraryPluginHandler.customReInit.TryGetValue(passiveType, out var patchList)) {
							Debug.Log($"Using LostLibraryPatch ReInits for {passiveType.Name}");
							foreach (var patch in patchList) {
								try {
									patch.Invoke(null, new object[]{model, passive});
								} catch (Exception ex) {
									Debug.LogException(ex);
								}
							}
						}
					}
				}
			}
			int num = 0;
			foreach (BattleUnitModel battleUnitModel2 in BattleObjectManager.instance.GetList()) {
				SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(battleUnitModel2.UnitData.unitData, num++, true, false);
			}
			BattleObjectManager.instance.InitUI();

			stageController.GetCurrentWaveModel().team.emotionTotalBonus = 0;

			StageLibraryFloorModel currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
			currentStageFloorModel.team.currentSelectEmotionLevel = oldSelectEmotionLevel;
			init = false;
		}

		public override void OnRoundEndTheLast()
		{
			if (init) {
				var aliveList = oldList.Keys.Where(u => !u.IsDead());
				if (imminentDeath) {
					currentBuf?.Destroy();
					if (aliveList.Count() > 0) {
						RemoveLostLibrary();
						imminentDeath = false;
						owner.SetHp(0);
						owner.DieFake();
						OnDieRedo(aliveList);
					//	foreach (var model in passiveAbility.oldList.Keys) {
					//		model.OnDieOtherUnit(model);
					//	}
					}
				} else if (currentBuf == null || currentBuf.IsDestroyed() || currentBuf.stack == 0) {
					RemoveLostLibrary();
				} else if (aliveList.Count() > 0) {
					StageLibraryFloorModel currentStageFloorModel = Singleton<StageController>.Instance.GetCurrentStageFloorModel();
					currentStageFloorModel.team.currentSelectEmotionLevel = oldSelectEmotionLevel;
				}
			}
			base.OnRoundEndTheLast();
		}
		public void OnDieRedo(IEnumerable<BattleUnitModel> aliveList, bool callEvent = true) {
			if (!callEvent)
			{
				owner.view.OnDie();
				return;
			}
			// List<BattleUnitModel> aliveList = BattleObjectManager.instance.GetAliveList(false);
			foreach (BattleUnitModel battleUnitModel in aliveList)
			{
				int count2 = battleUnitModel.emotionDetail.CreateEmotionCoin(EmotionCoinType.Negative, 3);
				battleUnitModel.OnDieOtherUnit(owner);
				battleUnitModel.emotionDetail.CheckLevelUp();
			}
			owner.view.OnDie();
		}

		public BattleUnitModel CreateUnit(Units unit, bool refreshUI = true) {
			if (currentIndex > 4) {
				Debug.LogError("More than 5 units exist");
				return null;
			}
			if (unit == Units.PLUGIN) {
				Debug.LogError("Called default CreateUnit for Plugin unit");
				return null;
			}
			var model = Singleton<StageController>.Instance.CreateLibrarianUnit(CurrentFloor, dataList[unit], currentIndex++);

			model.OnWaveStart();
			model.emotionDetail.SetEmotionLevel(owner.emotionDetail.EmotionLevel);
			model.allyCardDetail.DrawCards(model.UnitData.unitData.GetStartDraw());
			if (unit == Units.Geburah) {
				UnitData.ConfigureGeburah(model);
			}
			GameObject gameObject = Util.LoadPrefab("Battle/DiceAttackEffects/New/FX/PC/Angela/FX_PC_Angela_LibrarianCreate", model.view.transform);
			if (gameObject != null)
			{
				// gameObject.transform.position = unit.view.WorldPosition;
				gameObject.AddComponent<AutoDestruct>().time = 2f;
			}
			SingletonBehavior<SoundEffectManager>.Instance.PlayClip("Creature/Kether_Teleport", false, 1f, null);

			if (refreshUI) {
				int num = 0;
				foreach (BattleUnitModel battleUnitModel2 in BattleObjectManager.instance.GetList()) {
					SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(battleUnitModel2.UnitData.unitData, num++, true, false);
				}
				BattleObjectManager.instance.InitUI();
			}
			return model;
		}
		public BattleUnitModel CreatePluginUnit((LorId id, UnitBattleDataModel unitData) inputs, bool refreshUI = true) {
			var unitData = inputs.unitData;
			var method = LostLibraryPluginHandler.customSummonList[inputs.id];
			if (currentIndex > 4) {
				Debug.LogError("More than 5 units exist");
				return null;
			}
			var model = Singleton<StageController>.Instance.CreateLibrarianUnit(CurrentFloor, unitData, currentIndex++);

			model.OnWaveStart();
			model.emotionDetail.SetEmotionLevel(owner.emotionDetail.EmotionLevel);
			model.allyCardDetail.DrawCards(model.UnitData.unitData.GetStartDraw());
			if (method != null) {
				method.Invoke(null, new object[]{model});
			}
			GameObject gameObject = Util.LoadPrefab("Battle/DiceAttackEffects/New/FX/PC/Angela/FX_PC_Angela_LibrarianCreate", model.view.transform);
			if (gameObject != null)
			{
				// gameObject.transform.position = unit.view.WorldPosition;
				gameObject.AddComponent<AutoDestruct>().time = 2f;
			}
			SingletonBehavior<SoundEffectManager>.Instance.PlayClip("Creature/Kether_Teleport", false, 1f, null);

			if (refreshUI) {
				int num = 0;
				foreach (BattleUnitModel battleUnitModel2 in BattleObjectManager.instance.GetList()) {
					SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(battleUnitModel2.UnitData.unitData, num++, true, false);
				}
				BattleObjectManager.instance.InitUI();
			}
			return model;
		}

		private static class UnitData {
			static readonly (LorId defaultBook, LorId book) Roland = (new LorId(10), new LorId(180003));
			static readonly (LorId defaultBook, LorId book) Geburah = (new LorId(6), new LorId(180004));
			static readonly (LorId defaultBook, LorId book) PurpleTear = (new LorId(0), new LorId(0));
			static readonly (LorId defaultBook, LorId book) Uberto = (new LorId(0), new LorId(0));

			public static UnitBattleDataModel CreateData(StageModel stage, LibraryFloorModel floor, (LorId defaultBook, LorId book) tuple)
				=> CreateData(stage, floor, tuple.defaultBook, tuple.book);
			public static UnitBattleDataModel CreateData(StageModel stage, LibraryFloorModel floor, LorId defaultBook, LorId book) {
				UnitDataModel unitDataModel = new UnitDataModel(defaultBook, floor.Sephirah, true);
				unitDataModel.SetTemporaryPlayerUnitByBook(book);
				unitDataModel.isSephirah = false;
				unitDataModel.SetCustomName(Singleton<CharactersNameXmlList>.Instance.GetName(defaultBook));
				unitDataModel.forceItemChangeLock = true;
				UnitBattleDataModel unitBattleDataModel = new UnitBattleDataModel(stage, unitDataModel);
				unitBattleDataModel.Init();
				unitBattleDataModel.IsAddedBattle = false;
				return unitBattleDataModel;
			}
			public static UnitBattleDataModel CreateRolandData(StageModel stage, LibraryFloorModel floor)
				=> CreateData(stage, floor, Roland);
			public static UnitBattleDataModel CreateGeburahData(StageModel stage, LibraryFloorModel floor)
				=> CreateData(stage, floor, Geburah);
			public static UnitBattleDataModel CreatePurpleData(StageModel stage, LibraryFloorModel floor)
				=> CreateData(stage, floor, PurpleTear);
			public static UnitBattleDataModel CreateUbertoData(StageModel stage, LibraryFloorModel floor)
				=> CreateData(stage, floor, Uberto);
			
			public static void ConfigureGeburah(BattleUnitModel battleUnitModel)
			{
				battleUnitModel.allyCardDetail.ExhaustAllCards();
				battleUnitModel.allyCardDetail.AddNewCard(607003, false);
				battleUnitModel.allyCardDetail.AddNewCard(607003, false);
				battleUnitModel.allyCardDetail.AddNewCard(607004, false);
				battleUnitModel.allyCardDetail.AddNewCard(607004, false);
				battleUnitModel.allyCardDetail.AddNewCard(607005, false);
				battleUnitModel.allyCardDetail.AddNewCard(607005, false);
				battleUnitModel.allyCardDetail.AddNewCard(607007, false);
				battleUnitModel.allyCardDetail.AddNewCardToDeck(607006, false);
				battleUnitModel.allyCardDetail.AddNewCardToDeck(607006, false);
				foreach (BattleDiceCardModel battleDiceCardModel in battleUnitModel.allyCardDetail.GetAllDeck())
				{
					LorId id = battleDiceCardModel.GetID();
					if (id == 607003 || id == 607004 || id == 607005)
					{
						battleDiceCardModel.AddCost(-1);
					}
				}
				battleUnitModel.personalEgoDetail.AddCard(607022);
			}
		}
	}
	public class DiceCardSelfAbility_LostLibrary : DiceCardSelfAbilityBase {
		PassiveAbility_TABinBin_LostLibrary passiveAbility;
		bool? keterFinalCheck;
		
		public override bool OnChooseCard(BattleUnitModel owner) {
			#if !DEBUG
			if (owner.emotionDetail.EmotionLevel < 5) {
				return false;
			}
			#endif
			if (passiveAbility == null) {
				passiveAbility = owner.passiveDetail.PassiveList.Find((PassiveAbilityBase x) => x is PassiveAbility_TABinBin_LostLibrary) as PassiveAbility_TABinBin_LostLibrary;
			}
			if (keterFinalCheck == null) {
				keterFinalCheck = Singleton<StageController>.Instance.EnemyStageManager is EnemyTeamStageManager_KeterFinal;
			}
			return passiveAbility != null && !owner.bufListDetail.HasAssimilation() && !keterFinalCheck.GetValueOrDefault() && base.OnChooseCard(owner);
		}
		public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit) {
			try {
				if (unit.bufListDetail.HasAssimilation()) {
					return;
				}
				if (passiveAbility == null) {
					passiveAbility = unit.passiveDetail.PassiveList.Find((PassiveAbilityBase x) => x is PassiveAbility_TABinBin_LostLibrary) as PassiveAbility_TABinBin_LostLibrary;
				}
				if (passiveAbility == null) {
					Debug.LogError("LostLibrary passive not found");
					return;
				}
				unit.bufListDetail.AddBuf(new BattleUnitBuf_LostLibrary(3, passiveAbility));
				foreach (var card in PassiveAbility_TABinBin_LostLibrary.cardList) {
					unit.personalEgoDetail.AddCard(card);
				}
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
			self.exhaust = true;
		}
	}
	public class BattleUnitBuf_LostLibrary : BattleUnitBuf {
		public readonly PassiveAbility_TABinBin_LostLibrary passiveAbility;
		public bool init = false;
		public readonly Queue<PassiveAbility_TABinBin_LostLibrary.Units> unitList = new Queue<PassiveAbility_TABinBin_LostLibrary.Units>(4);
		public readonly Queue<(LorId, UnitBattleDataModel)> pluginUnitList = new Queue<(LorId, UnitBattleDataModel)>(4);

		public BattleUnitBuf_LostLibrary(int stack, PassiveAbility_TABinBin_LostLibrary passiveAbility) {
			this.stack = stack;
			passiveAbility.currentBuf = this;
			this.passiveAbility = passiveAbility;
		}
		public override bool isAssimilation {
			get {
				return true;
			}
		}

		public override void OnRoundStart() {
			base.OnRoundStart();
			CustomMapHandler.EnforceTheme();
		}
		public override void OnRoundEndTheLast() {
			if (stack <= 0) {
				Destroy();
				return;
			}
			if (!init) {
				passiveAbility.InitLostLibrary();
			}

			if (init && unitList.Count == 0) {goto end;}
			for (int i = unitList.Count; i > 0; i--) {
				var entry = unitList.Dequeue();
                BattleUnitModel newUnit;
                if (entry != PassiveAbility_TABinBin_LostLibrary.Units.PLUGIN) {
                    newUnit = passiveAbility.CreateUnit(entry, false);
				} else {
					newUnit = passiveAbility.CreatePluginUnit(pluginUnitList.Dequeue(), false);
				}
				if (newUnit != null) {
					passiveAbility.newList.Add(newUnit);
				}
			}
			
			int num = 0;
			foreach (BattleUnitModel battleUnitModel2 in BattleObjectManager.instance.GetList()) {
				SingletonBehavior<UICharacterRenderer>.Instance.SetCharacter(battleUnitModel2.UnitData.unitData, num++, true, false);
			}
			BattleObjectManager.instance.InitUI();

			init = true;
			end:
			stack--;
		}
		public override void OnDie()
		{
			passiveAbility.imminentDeath = true;
			_owner.Revive(1);
			_owner.formation = passiveAbility.oldFormationPos;
			base.OnDie();
		}
		public override void Destroy() {
			unitList.Clear();
			stack = 0;
			base.Destroy();
		}
	}

	public abstract class LostLibrary_Unit : DiceCardSelfAbilityBase {
		protected abstract PassiveAbility_TABinBin_LostLibrary.Units UnitEntry {get;}
		public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit) {
			var buf = unit.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) => x is BattleUnitBuf_LostLibrary)
				as BattleUnitBuf_LostLibrary;
			buf.unitList.Enqueue(UnitEntry);
			self.exhaust = true;
		}
	}
	public class DiceCardSelfAbility_LostLibrary_Desynchronize : DiceCardSelfAbilityBase {
		BattleUnitBuf_LostLibrary buf;
		public override bool OnChooseCard(BattleUnitModel owner) {
			if (buf == null) {
				buf = owner.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) =>
					x is BattleUnitBuf_LostLibrary) as BattleUnitBuf_LostLibrary;
			}
			return buf != null && buf.init && base.OnChooseCard(owner);
		}
		public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit) {
			if (buf == null) {
				buf = unit.bufListDetail.GetActivatedBufList().Find((BattleUnitBuf x) =>
					x is BattleUnitBuf_LostLibrary) as BattleUnitBuf_LostLibrary;
			}
			if (buf.init) {
				buf.Destroy();
				foreach (var card in PassiveAbility_TABinBin_LostLibrary.cardList) {
					unit.personalEgoDetail.RemoveCard(card);
				}
			} else {
				buf.Destroy();
				unit.bufListDetail.RemoveBuf(buf);
				unit.personalEgoDetail.AddCard(new LorId(Globals.ModId, 20)); // LostLibrary card
			}
			self.exhaust = true;
		}
	}

	public class DiceCardSelfAbility_LostLibrary_Roland : LostLibrary_Unit {
		protected override PassiveAbility_TABinBin_LostLibrary.Units UnitEntry {get => PassiveAbility_TABinBin_LostLibrary.Units.Roland;}
	}
	public class DiceCardSelfAbility_LostLibrary_Geburah : LostLibrary_Unit {
		protected override PassiveAbility_TABinBin_LostLibrary.Units UnitEntry {get => PassiveAbility_TABinBin_LostLibrary.Units.Geburah;}
	}
	public class DiceCardSelfAbility_LostLibrary_PurpleTear : LostLibrary_Unit {
		protected override PassiveAbility_TABinBin_LostLibrary.Units UnitEntry {get => PassiveAbility_TABinBin_LostLibrary.Units.PurpleTear;}
	}
	public class DiceCardSelfAbility_LostLibrary_Uberto : LostLibrary_Unit {
		protected override PassiveAbility_TABinBin_LostLibrary.Units UnitEntry {get => PassiveAbility_TABinBin_LostLibrary.Units.Uberto;}
	}

	public class LostLibraryMapManager : CustomMapManager {
		protected internal override string[] CustomBGMs {
			get {
				return new string[] {"BirthdayKid.ogg"};
			}
		}
	}
}