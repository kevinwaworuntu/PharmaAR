using System.Collections;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay
{
    public class TBA5Manager : ARContentManager
    {
        [SerializeField] private BalanceObject balanceObject;
        
        private bool isCheckWeightToContinue;
        [SerializeField] private float targetWeight;
        
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip animClipBalance;
        
        public void SetupBalanceObject()
        {
            balanceObject.SetWeight(0);
          
            ContextualButtonController.Instance.GenerateContextualButton(1);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "25 mg");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                //balanceObject.IncreaseWeight(25); Triggered by anim instead
                PlayAnimation(animClipBalance);
                DecreaseSerbukFillValue(1);
            });
        }
        protected void OnDisable()
        {
            ResetSerbukFill();
        }
       
        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
            }
        }
        
        private void PlayAnimation(AnimationClip clip)
        {
            var animationConfig = GameManager.Instance.AnimationConfig;
            if (!animationConfig)
            {
                return;
            }
            if (animationConfig.GenericAnimController)
            {
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = clip;
            }
            if (!clip)
            {
                return;
            }
            if (!animator)
            {
                return;
            }
            animator.SetTrigger(animationConfig.StopAnimationParamName);
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            SetButtonEnabledState(false);
            StartCoroutine(WaitForDuration(clip.length));// This is only valid if animation speed is constant
            IEnumerator WaitForDuration(float duration)
            {
                yield return new WaitForSeconds(duration);
                SetButtonEnabledState(true);
                OnStartWaitingForPlayerInputToContinueHandler();
            }
        }
        
        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if (!balanceObject)
            {
                return;
            }
            if(isCheckWeightToContinue)
            {
                if (!IsCurrentWeightComplete())
                {
                    return;
                }
            }
            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
        }
        
        private bool IsCurrentWeightComplete()
        {
            return Mathf.FloorToInt(balanceObject.GetCurrentWeight()) == Mathf.FloorToInt(targetWeight);
        }
        
        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }

        [Header("SERBUK FILL SETTINGS")]
        [SerializeField] private Transform serbukFillObject;
        
        private float scaleModifier = 0.1225f;
        private float initialScale = 0.02f;
   
        
        private void DecreaseSerbukFillValue(float targetMl)
        {
            var targetScaleZ = serbukFillObject.transform.localScale.z + scaleModifier * targetMl;
            serbukFillObject.transform.localScale = new Vector3(targetScaleZ, targetScaleZ, targetScaleZ);
        }

        private void ResetSerbukFill()
        {
            serbukFillObject.transform.localScale = new Vector3(initialScale, initialScale, initialScale);
        }
    }
}