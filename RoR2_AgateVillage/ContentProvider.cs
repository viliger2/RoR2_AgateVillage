using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2_AgateVillage
{
    public class ContentProvider : IContentPackProvider
    {
        public string identifier => AgateVillagePlugin.GUID + "." + nameof(ContentProvider);

        private readonly ContentPack _contentPack = new ContentPack();

        internal const string ScenesAssetBundleFileName = "agatevillagescene";
        internal const string AssetsAssetBundleFileName = "agatevillageassets";

        internal const string MusicSoundBankFileName = "AgateVillageMusic.bnk";
        internal const string InitSoundBankFileName = "AgateVillageInit.bnk";

        public static SceneDef AgateVillageSceneDef;

        public static readonly Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbedror2/base/shaders/hgstandard", "RoR2/Base/Shaders/HGStandard.shader"},
            {"stubbedror2/base/shaders/hgsnowtopped", "RoR2/Base/Shaders/HGSnowTopped.shader"},
            {"stubbedror2/base/shaders/hgtriplanarterrainblend", "RoR2/Base/Shaders/HGTriplanarTerrainBlend.shader"},
            {"stubbedror2/base/shaders/hgintersectioncloudremap", "RoR2/Base/Shaders/HGIntersectionCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgcloudremap", "RoR2/Base/Shaders/HGCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgopaquecloudremap", "RoR2/Base/Shaders/HGOpaqueCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgdistortion", "RoR2/Base/Shaders/HGDistortion.shader" },
            {"stubbedcalm water/calmwater - dx11 - doublesided", "Calm Water/CalmWater - DX11 - DoubleSided.shader" },
            {"stubbedcalm water/calmwater - dx11", "Calm Water/CalmWater - DX11.shader" },
            {"stubbednature/speedtree", "RoR2/Base/Shaders/SpeedTreeCustom.shader"},
            {"stubbeddecalicious/decaliciousdeferreddecal", "Decalicious/DecaliciousDeferredDecal.shader" }
        };

        public static List<Material> SwappedMaterials = new List<Material>(); //debug

        private static Sprite AgateVillagePreviewSprite;

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            _contentPack.identifier = identifier;

            var musicFolderFullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), "soundbanks");
            LoadSoundBanks(musicFolderFullPath);

            var assetsFolderFullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), "assetbundles");
            AssetBundle scenesAssetBundle = null;
            yield return LoadAssetBundle(
                System.IO.Path.Combine(assetsFolderFullPath, ScenesAssetBundleFileName),
                args.progressReceiver,
                (assetBundle) => scenesAssetBundle = assetBundle);

            AssetBundle assetsAssetBundle = null;
            yield return LoadAssetBundle(
                System.IO.Path.Combine(assetsFolderFullPath, AssetsAssetBundleFileName),
                args.progressReceiver,
            (assetBundle) => assetsAssetBundle = assetBundle);

            yield return LoadAllAssetsAsync(assetsAssetBundle, args.progressReceiver, (Action<Material[]>)((assets) =>
            {
                var materials = assets;

                if (materials != null)
                {
                    foreach (Material material in materials)
                    {
                        if (!material.shader.name.StartsWith("Stubbed")) { continue; }

                        var replacementShader = Addressables.LoadAssetAsync<Shader>(ShaderLookup[material.shader.name.ToLower()]).WaitForCompletion();
                        if (replacementShader)
                        {
                            material.shader = replacementShader;
                            FixMaterials(material);

                            SwappedMaterials.Add(material);
                        }
                    }
                }
            }));

            yield return LoadAllAssetsAsync(assetsAssetBundle, args.progressReceiver, (Action<UnlockableDef[]>)((assets) =>
            {
                _contentPack.unlockableDefs.Add(assets);
            }));

            yield return LoadAllAssetsAsync(assetsAssetBundle, args.progressReceiver, (Action<Sprite[]>)((assets) =>
            {
                AgateVillagePreviewSprite = assets.First(a => a.name == "texAgateVillage");
            }));

            yield return LoadAllAssetsAsync(assetsAssetBundle, args.progressReceiver, (Action<SceneDef[]>)((assets) =>
            {
                AgateVillageSceneDef = assets.First(sd => sd.cachedName == "agatevillage");
                _contentPack.sceneDefs.Add(assets);
            }));

            var bazaarSeerMaterial = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/bazaar/matBazaarSeerWispgraveyard.mat").WaitForCompletion());
            bazaarSeerMaterial.mainTexture = AgateVillagePreviewSprite.texture; 

            AgateVillageSceneDef.previewTexture = AgateVillagePreviewSprite.texture; 
            AgateVillageSceneDef.portalMaterial = bazaarSeerMaterial;

            if (AgateVillagePlugin.UseCustomMusic.Value)
            {
                SetupMusic();
            } else
            {
                AgateVillageSceneDef.mainTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muGameplayBase_09.asset").WaitForCompletion();
                AgateVillageSceneDef.bossTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muSong16.asset").WaitForCompletion();
            }

            var normalSceneCollection = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage3.asset").WaitForCompletion();
            HG.ArrayUtils.ArrayAppend(ref normalSceneCollection._sceneEntries, new SceneCollection.SceneEntry { sceneDef = AgateVillageSceneDef, weight = 1f });
            AgateVillageSceneDef.destinationsGroup = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage4.asset").WaitForCompletion();

            var loopSceneCollection = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage3.asset").WaitForCompletion();
            HG.ArrayUtils.ArrayAppend(ref loopSceneCollection._sceneEntries, new SceneCollection.SceneEntry { sceneDef = AgateVillageSceneDef, weight = 1f });
            AgateVillageSceneDef.loopedDestinationsGroup = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage4.asset").WaitForCompletion();
        }

        private static void FixMaterials(Material material)
        {
            if (material.name == "matFire")
            {
                material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampCaptainAirstrike.png").WaitForCompletion());
            }
            else if (material.name == "matHeatGas")
            {
                material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLunarSmoke.png").WaitForCompletion());
            }
            else if (material.name == "matClouds")
            {
                material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampDefault.png").WaitForCompletion());
            } else if(material.name == "matRain")
            {
                material.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/VFX/ParticleMasks/texParticleDust1Mask.png").WaitForCompletion());
                material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampTwotoneEnvironment.jpg").WaitForCompletion());
                material.SetTexture("_Cloud1Tex", Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/texCloudDifferenceBW1.png").WaitForCompletion());
            }
        }

        private static void SetupMusic()
        {
            var mainCustomTrack = ScriptableObject.CreateInstance<SoundAPI.Music.CustomMusicTrackDef>();
            (mainCustomTrack as ScriptableObject).name = "AgateVillageMainMusic";
            mainCustomTrack.cachedName = "AgateVillageMainMusic";
            mainCustomTrack.comment = "sunnydaze - Barrow's Ceiling\r\nagatevillage";
            mainCustomTrack.CustomStates = new List<SoundAPI.Music.CustomMusicTrackDef.CustomState>();

            var cstate1 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
            cstate1.GroupId = 3508158228U; // gathered from the MOD's Init bank txt file
            cstate1.StateId = 2025383728U; // BarrowsCelling
            mainCustomTrack.CustomStates.Add(cstate1);

            var cstate2 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
            cstate2.GroupId = 792781730U; // gathered from the GAME's Init bank txt file
            cstate2.StateId = 89505537U; // gathered from the GAME's Init bank txt file
            mainCustomTrack.CustomStates.Add(cstate2);

            AgateVillageSceneDef.mainTrack = mainCustomTrack;
            var bossCustomTrack = ScriptableObject.CreateInstance<SoundAPI.Music.CustomMusicTrackDef>();
            (bossCustomTrack as ScriptableObject).name = "AgateVillageBossMusic";
            bossCustomTrack.cachedName = "AgateVillageBossMusic";
            bossCustomTrack.comment = "sunnydaze - Fight!\r\nagatevillage";
            bossCustomTrack.CustomStates = new List<SoundAPI.Music.CustomMusicTrackDef.CustomState>();

            var cstate11 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
            cstate11.GroupId = 3508158228U; // gathered from the MOD's Init bank txt file
            cstate11.StateId = 514064485U; // Fight!
            bossCustomTrack.CustomStates.Add(cstate11);

            var cstate12 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
            cstate12.GroupId = 792781730U; // gathered from the GAME's Init bank txt file
            cstate12.StateId = 580146960U; // gathered from the GAME's Init bank txt file
            bossCustomTrack.CustomStates.Add(cstate12);

            AgateVillageSceneDef.bossTrack = bossCustomTrack;
            //AgateVillageSceneDef.mainTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muGameplayBase_09.asset").WaitForCompletion();
            //AgateVillageSceneDef.bossTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muSong16.asset").WaitForCompletion();
        }

        private IEnumerator LoadAssetBundle(string assetBundleFullPath, IProgress<float> progress, Action<AssetBundle> onAssetBundleLoaded)
        {
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(assetBundleFullPath);
            while (!assetBundleCreateRequest.isDone)
            {
                progress.Report(assetBundleCreateRequest.progress);
                yield return null;
            }

            onAssetBundleLoaded(assetBundleCreateRequest.assetBundle);

            yield break;
        }

        private static IEnumerator LoadAllAssetsAsync<T>(AssetBundle assetBundle, IProgress<float> progress, Action<T[]> onAssetsLoaded) where T : UnityEngine.Object
        {
            var sceneDefsRequest = assetBundle.LoadAllAssetsAsync<T>();
            while (!sceneDefsRequest.isDone)
            {
                progress.Report(sceneDefsRequest.progress);
                yield return null;
            }

            onAssetsLoaded(sceneDefsRequest.allAssets.Cast<T>().ToArray());

            yield break;
        }

        internal static void LoadSoundBanks(string soundbanksFolderPath)
        {
            var akResult = AkSoundEngine.AddBasePath(soundbanksFolderPath);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank base path : {soundbanksFolderPath}");
            }
            else
            {
                Log.Error(
                    $"Error adding base path : {soundbanksFolderPath} " +
                    $"Error code : {akResult}");
            }

            akResult = AkSoundEngine.LoadBank(InitSoundBankFileName, out var _);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank : {InitSoundBankFileName}");
            }
            else
            {
                Log.Error(
                    $"Error loading bank : {InitSoundBankFileName} " +
                    $"Error code : {akResult}");
            }

            akResult = AkSoundEngine.LoadBank(MusicSoundBankFileName, out var _);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank : {MusicSoundBankFileName}");
            }
            else
            {
                Log.Error(
                    $"Error loading bank : {MusicSoundBankFileName} " +
                    $"Error code : {akResult}");
            }
        }
    }
}
