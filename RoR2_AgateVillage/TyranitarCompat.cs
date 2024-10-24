using Newtonsoft.Json.Utilities;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.AddressableAssets;

namespace RoR2_AgateVillage
{
    public class TyranitarCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.Tyranitar");
                }
                return (bool)_enabled;
            }
        }

        public static DirectorCard GetTyranitarSpawnCard(DirectorCardCategorySelection mixEnemy)
        {
            var result = mixEnemy.categories[0].cards.FirstOrDefault(card => card.spawnCard.name == "cscTyranitar");
            return result;
        }
    }
}
