using System.Collections;
using UI;
using UnityEngine;

namespace Gameplay
{
    public class TK8Manager : TBA4Manager
    {
        public float floorTarget = 10;
        public float ceilTarget = 10.2f;
      
        protected override bool IsCurrentWeightComplete()
        {
            return currentWeight >= floorTarget && currentWeight <= ceilTarget;
        }
       
    }
}