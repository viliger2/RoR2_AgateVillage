using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2_AgateVillage
{
    public class CalculateNetworkCost : MonoBehaviour
    {
        private void Start()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            var purchaseInteraction = GetComponentInChildren<PurchaseInteraction>();
            if (purchaseInteraction)
            {
                purchaseInteraction.Networkcost = Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost);
                purchaseInteraction.solitudeCost = purchaseInteraction.Networkcost;
            }
        }

    }
}
