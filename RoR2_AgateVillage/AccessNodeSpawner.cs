using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace AgateVillage
{
    public class AccessNodeSpawner : MonoBehaviour
    {
        public AccessCodesMissionController missionController;

        private void Awake()
        {
            if (!missionController)
            {
                return;
            }

            missionController.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC3/DLC3.asset").WaitForCompletion();

            if (!NetworkServer.active)
            {
                return;
            }

            var result = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/AccessCodesNode/Access Codes Node.prefab").WaitForCompletion(), transform.parent);
            NetworkServer.Spawn(result);

            missionController.nodes = new AccessCodesNodeData[]
            {
                new AccessCodesNodeData()
                {
                    node = result,
                    id = 0
                }
            };
        }
    }
}
