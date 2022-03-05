using System;
using System.Collections.Generic;
using System.Reflection;
using Sound;

namespace Teal_Archivist
{
    public static class LostLibraryPluginHandler {
		public static void AddToReInitList(Type passive, MethodInfo patch) {
			if (customReInit.ContainsKey(passive)) {
                customReInit[passive].Add(patch);
            } else {
                customReInit[passive] = new List<MethodInfo>(){patch};
            }
		}
        public static void AddToSummonList(LorId card, MethodInfo configureMethod) {
            if (!customSummonList.ContainsKey(card)) {
                customSummonList.Add(card, configureMethod);
                PassiveAbility_TABinBin_LostLibrary.cardList.Add(card);
            }
        }
        public static void AddToSummonList(string packageId, int id, MethodInfo summonMethod) =>
            AddToSummonList(new LorId(packageId, id), summonMethod);
        public static List<LorId> CardList => PassiveAbility_TABinBin_LostLibrary.cardList;
        public static readonly Dictionary<Type, List<MethodInfo>> customReInit = new Dictionary<Type, List<MethodInfo>>();
        public static readonly Dictionary<LorId, MethodInfo> customSummonList = new Dictionary<LorId, MethodInfo>();
	};
}