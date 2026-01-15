using System.Collections;
using UI;
using UnityEngine;

namespace Gameplay
{
    public class TBA6Manager : ARContentManager
    {
        private int currentWeight;
        public int currentTargetWeight;
        public int targetWeightAnhidrida = 40;
        public int targetWeightBenzena = 80;

        [SerializeField] private AnimationClip animClipAnhidrida_1ml;
        [SerializeField] private AnimationClip animClipAnhidrida_10ml;
        [SerializeField] private AnimationClip animClipBenzena_1ml;
        [SerializeField] private AnimationClip animClipBenzena_10ml;
        [SerializeField] private Animator animator;
        
        [SerializeField] private Material _fluidAnhidridaMaterial;
        [SerializeField] private Material _fluidBenzenaMaterial;
        [SerializeField] private float value1Ml = 0.0008f;
        [SerializeField] private float value10Ml = 0.008f;
        
        private bool isCheckWeightToContinue;
        
        public void CreatePenambahanLarutanAnhidridaButton()
        {
            currentTargetWeight = targetWeightAnhidrida;
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                PlayAnimation(animClipAnhidrida_1ml);
                
                // Animasi tabung bertambah after dituangin pake coroutine aja
                float fillValue = _fluidAnhidridaMaterial.GetFloat("_Fill");
                _fluidAnhidridaMaterial.SetFloat("_Fill", fillValue + value1Ml);
               
            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "10 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 10;
                PlayAnimation(animClipAnhidrida_10ml);
                
                // Animasi tabung bertambah after dituangin pake coroutine aja
                float fillValue = _fluidAnhidridaMaterial.GetFloat("_Fill");
                _fluidAnhidridaMaterial.SetFloat("_Fill", fillValue + value10Ml);
            });
           
        }
        
        public void CreatePenambahanLarutanBenzenaButton()
        {
            currentTargetWeight = targetWeightBenzena;
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                PlayAnimation(animClipBenzena_1ml);
              
                // Animasi tabung bertambah after dituangin pake coroutine aja
                float fillValue = _fluidAnhidridaMaterial.GetFloat("_Fill");
                _fluidAnhidridaMaterial.SetFloat("_Fill", fillValue + value1Ml);
            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "10 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 10;
                PlayAnimation(animClipBenzena_10ml);
            
                // Animasi tabung bertambah after dituangin pake coroutine aja
                float fillValue = _fluidAnhidridaMaterial.GetFloat("_Fill");
                _fluidAnhidridaMaterial.SetFloat("_Fill", fillValue + value1Ml);
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
            return currentWeight == currentTargetWeight;
        }
        
        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > currentTargetWeight;
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
                    currentWeight = 0; // Restart
                    //Empty shader
                }
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