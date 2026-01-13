using UnityEngine;
using UI;

namespace Gameplay
{
    public class TBA4Manager : MonoBehaviour
    {
        private int currentWeight;
        private int targetWeight = 40;
        
        public void CreatePenambahanLarutanButton()
        {
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                if (IsCurrentWeightExceedTarget())
                {
                    currentWeight = 0; // Restart
                }
                if (IsCurrentWeightComplete())
                {
                    ContextualButtonController.Instance.DestroyButtons();
                    // trigger next step
                }
            });
            ContextualButtonController.Instance.RegisterTextToButton(0, "1 ml");
            
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 10;
                if (IsCurrentWeightExceedTarget())
                {
                    currentWeight = 0; // Restart
                }
                if (IsCurrentWeightComplete())
                {
                    ContextualButtonController.Instance.DestroyButtons();
                }
                // trigger next step
            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "10 ml");
        }

        private bool IsCurrentWeightComplete()
        {
            return currentWeight == targetWeight;
        }
        
        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > targetWeight;
        }
    }
}