using System.IO;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using Mod;
using UI;
using UnityEngine;
using Workshop;

namespace Teal_Archivist
{
    class TealInit : ModInitializer
    {
        public override void OnInitializeMod() {
            Harmony harmony = new Harmony("LoR.uGuardian.TealArchivist");
            harmony.Patch(typeof(UISpriteDataManager).GetMethod("SetStoryIconDictionary", AccessTools.all), new HarmonyMethod(typeof(TealInit).GetMethod("AddIcon")));
            Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(x => dllList.Exists(s => x.Contains(s)));
        }
        public static void AddIcon(UISpriteDataManager __instance)
        {
            try {
                // W+H will get overwritten.
                Texture2D texture = new Texture2D(2, 2); // Initialise empty texture w/ width & height.
                Texture2D textureGlow = new Texture2D(2, 2); // SAME thing as above, but for glow.
                // Gets directory info from root mod folder; looks for BookIcon folder in Resource.
                var bookIconDir = new DirectoryInfo(ResourceDir + "/BookIcon");
                texture.LoadImage(File.ReadAllBytes(bookIconDir + "/TA.png")); // Load image into texture var; replaces width & height to new texture.
                textureGlow.LoadImage(File.ReadAllBytes(bookIconDir + "/TA.png")); // Same as above, but for glow side.
                UIIconManager.IconSet TealArchivistIcon = new UIIconManager.IconSet
                {
                    type = "TealArchivist", //Icon Type.
                    icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100), // Creates new sprite icon from texture (line 29).
                    color = Color.white, // Icon set is used for things like stagger resist; that colour modifies colour for things like HP, BP... filter? Unknown.
                    iconGlow = Sprite.Create(textureGlow, new Rect(0f, 0f, textureGlow.width, textureGlow.height), new Vector2(0.5f, 0.5f), 100),
                    colorGlow = Color.white, // Read comments 36 & 37; same things here.
                };
                // Adds TealArchivist Icon to icon list before init.
                __instance._storyicons.Add(TealArchivistIcon);
            } catch {
                Singleton<ModContentManager>.Instance.AddErrorLog("Failed to load TealArchivist icon");
            }
        }
        public void UpdateSkins() {
            var directories = Directory.GetDirectories(ResourceDir+"/CharacterSkin");
            for (int i = 0; i < directories.Length; i++)
			{
				WorkshopAppearanceInfo workshopAppearanceInfo = WorkshopAppearanceItemLoader.LoadCustomAppearance(directories[i], true);
				if (workshopAppearanceInfo != null)
				{
					string[] array = directories[i].Split(new char[]
					{
						'\\'
					});
					string bookName = array[array.Length - 1];
					workshopAppearanceInfo.path = directories[i];
					workshopAppearanceInfo.uniqueId = "TealArchivist";
					workshopAppearanceInfo.bookName = bookName;
					Debug.Log("workshop bookName : " + workshopAppearanceInfo.bookName);
                    var list = new List<WorkshopSkinData>();
					if (workshopAppearanceInfo.isClothCustom)
					{
						list.Add(new WorkshopSkinData
						{
							dic = workshopAppearanceInfo.clothCustomInfo,
							dataName = workshopAppearanceInfo.bookName,
							contentFolderIdx = workshopAppearanceInfo.uniqueId,
							id = i
						});
					}
				}
			}
        }
        readonly List<string> dllList = new List<string> {
            "0Harmony",
			"Mono.Cecil",
			"MonoMod.RuntimeDetour",
			"MonoMod.Utils",
        };
        static string ResourceDir => Assembly.GetExecutingAssembly().Location + "/../../Resource";
    }
}