using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace RoR2_AgateVillage
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(R2API.DirectorAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency("JaceDaDorito.LocationsOfPrecipitation")]
    [BepInDependency("Viliger.RegisterCommandChest")]
    [BepInDependency("com.rob.RegigigasMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.rob.Tyranitar", BepInDependency.DependencyFlags.SoftDependency)]
    public class AgateVillagePlugin : BaseUnityPlugin
    {
        public const string Author = "Viliger";
        public const string Name = nameof(AgateVillagePlugin);
        public const string Version = "1.0.3";
        public const string GUID = Author + "." + Name;

        public static ConfigEntry<bool> UseCustomMusic;

        private void Awake()
        {

#if DEBUG
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
#endif
            Log.Init(Logger);

            UseCustomMusic = Config.Bind("Custom Music", "Custom Music", true, "Does the stage use custom music.");

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            On.RoR2.MusicController.StartIntroMusic += MusicController_StartIntroMusic;
            RoR2.Language.collectLanguageRootFolders += CollectLanguageRootFolders;

            var dccsMixEnemy = Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/MixEnemy/dccsMixEnemy.asset").WaitForCompletion();

            if (RegigigasCompat.enabled)
            {
                var directorCard = RegigigasCompat.GetRegigigasSpawnCard(dccsMixEnemy);
                if (!directorCard.Equals(default(DirectorCard)))
                {
                    var directorCardHolder = new DirectorAPI.DirectorCardHolder
                    {
                        Card = directorCard,
                        MonsterCategory = DirectorAPI.MonsterCategory.Champions
                    };
                    DirectorAPI.Helpers.AddNewMonsterToStage(directorCardHolder, false, DirectorAPI.Stage.Custom, "agatevillage");
                    Log.Info("Regigigas added to agatevillage spawn pool.");
                }
            }
            if (TyranitarCompat.enabled)
            {
                var directorCard = TyranitarCompat.GetTyranitarSpawnCard(dccsMixEnemy);
                if (!directorCard.Equals(default(DirectorCard)))
                {
                    var directorCardHolder = new DirectorAPI.DirectorCardHolder
                    {
                        Card = directorCard,
                        MonsterCategory = DirectorAPI.MonsterCategory.Champions
                    };
                    DirectorAPI.Helpers.AddNewMonsterToStage(directorCardHolder, false, DirectorAPI.Stage.Custom, "agatevillage");
                    Log.Info("Tyranitar added to agatevillage spawn pool.");
                }
            }
        }

        private void MusicController_StartIntroMusic(On.RoR2.MusicController.orig_StartIntroMusic orig, RoR2.MusicController self)
        {
            orig(self);
            AkSoundEngine.PostEvent("Agate_Play_Music_System", self.gameObject);
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }

        private void CollectLanguageRootFolders(List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(base.Info.Location), "Language"));
        }
    }
}
