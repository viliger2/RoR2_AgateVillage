using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace AgateVillage
{
    public class FishingInteractable : MonoBehaviour
    {
        private CharacterSpawnCard cscMagmaWorm;

        private CharacterSpawnCard cscElectricWorm;

        private BasicPickupDropTable dtChest1;

        private Transform spawnPoint;

        private Vector3 velocityVector = new Vector3(-1f, 65f, -35f);

        private void Awake()
        {
            var magmaHandle = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/MagmaWorm/cscMagmaWorm.asset");
            if (magmaHandle.IsValid())
            {
                magmaHandle.Completed += result =>
                {
                    if (result.IsDone && result.Result)
                    {
                        cscMagmaWorm = result.Result;
                    }
                    Addressables.Release(magmaHandle);
                };
            }

            var electricHandle = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/ElectricWorm/cscElectricWorm.asset");
            if (electricHandle.IsValid())
            {
                electricHandle.Completed += result =>
                {
                    if (result.IsDone && result.Result)
                    {
                        cscElectricWorm = result.Result;
                    }
                    Addressables.Release(electricHandle);
                };
            }

            var chestHandle = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Chest1/dtChest1.asset");
            if (chestHandle.IsValid())
            {
                chestHandle.Completed += result =>
                {
                    if (result.IsDone && result.Result)
                    {
                        dtChest1 = result.Result;
                    }
                    Addressables.Release(chestHandle);
                };
            }

            spawnPoint = base.transform;
            var childLocator = GetComponent<ChildLocator>();
            if (childLocator)
            {
                var boblePoint = childLocator.FindChild("BobberHead");
                if (boblePoint)
                {
                    spawnPoint = boblePoint;
                }
            }
        }

        public void GoneFishing(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            var result = UnityEngine.Random.Range(0, 10000);
            if (result < 10)
            {
                if (cscElectricWorm)
                {
                    DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(cscElectricWorm, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                        position = spawnPoint.position,
                        spawnOnTarget = spawnPoint
                    }, RoR2Application.rng);
                    spawnRequest.ignoreTeamMemberLimit = true;
                    spawnRequest.teamIndexOverride = TeamIndex.Monster;
                    DirectorCore.instance.TrySpawnObject(spawnRequest);
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "AGATE_VILLAGE_FISH_REALLY_BIG"
                    });
                }
            }
            else if (result < 500)
            {
                if (cscMagmaWorm)
                {
                    DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(cscMagmaWorm, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        position = spawnPoint.position,
                        spawnOnTarget = spawnPoint
                    }, RoR2Application.rng);
                    spawnRequest.ignoreTeamMemberLimit = true;
                    spawnRequest.teamIndexOverride = TeamIndex.Monster;
                    DirectorCore.instance.TrySpawnObject(spawnRequest);
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "AGATE_VILLAGE_FISH_BIG"
                    });
                }
            }
            else if (result < 3500)
            {
                if (dtChest1)
                {
                    var rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
                    var pickupIndex = dtChest1.GenerateDrop(rng);
                    PickupDropletController.CreatePickupDroplet(pickupIndex, spawnPoint.position, velocityVector);
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "AGATE_VILLAGE_FISH_ITEM"
                    });
                }
            }
            else
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "AGATE_VILLAGE_FISH_NOTHING"
                });
            }
        }
    }
}
