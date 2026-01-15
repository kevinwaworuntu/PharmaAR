using System.Collections;
using UI;
using UnityEngine;

namespace Gameplay
{
    public class TK8Manager : ARContentManager
    {
        private float currentWeight;
        
        public float floorTarget = 10;
        public float ceilTarget = 10.2f;
       

        [SerializeField] private AnimationClip animClip_01ml;
        [SerializeField] private AnimationClip animClip_1ml;
        
        [SerializeField] private AnimationClip animClipFailed;
        [SerializeField] private Animator animator;
        
        // [SerializeField] private Material _fluidAnhidridaMaterial;
        // [SerializeField] private Material _fluidBenzenaMaterial;
        // [SerializeField] private float value1Ml = 0.0008f;
        // [SerializeField] private float value10Ml = 0.008f;
        
        private bool isCheckWeightToContinue;
        
        public void CreatePenambahanLarutanButton()
        {
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterTextToButton(1, "0.1 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 0.1f;
                PlayAnimation(animClip_01ml);
                
                // // Animasi tabung bertambah after dituangin pake coroutine aja
                // float fillValue = _fluidAnhidridaMaterial.GetFloat("_Fill");
                // _fluidAnhidridaMaterial.SetFloat("_Fill", fillValue + value10Ml);
            });
            ContextualButtonController.Instance.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                PlayAnimation(animClip_1ml);
                
                // Animasi tabung bertambah after dituangin pake coroutine aja
                // float fillValue = _fluidAnhidridaMaterial.GetFloat("_Fill");
                // _fluidAnhidridaMaterial.SetFloat("_Fill", fillValue + value1Ml);
               
            });
            
           
        }
        
        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
            }
        }
        
        private bool IsCurrentWeightComplete()
        {
            return currentWeight >= floorTarget && currentWeight <= ceilTarget;
        }
        
        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > ceilTarget;
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
                if (IsCurrentWeightComplete())
                {
                    OnStartWaitingForPlayerInputToContinueHandler();
                }
                if (IsCurrentWeightExceedTarget())
                {
                    ExceedTarget();
                }
            }
        }

        private void ExceedTarget()
        {
            var animationConfig = GameManager.Instance.AnimationConfig;
            if (!animationConfig)
            {
                return;
            }
            if (animationConfig.GenericAnimController)
            {
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipFailed;
            }
            if (!animClipFailed)
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
            StartCoroutine(WaitForDuration(animClipFailed.length));// This is only valid if animation speed is constant

            IEnumerator WaitForDuration(float duration)
            {
                yield return new WaitForSeconds(duration);
                currentWeight = 0;
                //Empty shader
                // Tombol Restart
            }
        }

        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if(isCheckWeightToContinue)
            {
                if (!IsCurrentWeightComplete())
                {
                    return;
                }
            }
            currentWeight = 0;
            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
        }
        
        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }
    }
}