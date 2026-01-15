using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class TBA4Manager : ARContentManager
    {
        protected int currentWeight;
        public int currentTargetWeight;
        public int targetWeightAnhidrida = 40;
        public int targetWeightBenzena = 80;

        [SerializeField] protected AnimationClip animClipAnhidrida_1ml;
        [SerializeField] protected AnimationClip animClipAnhidrida_10ml;
        [SerializeField] protected AnimationClip animClipBenzena_1ml;
        [SerializeField] protected AnimationClip animClipBenzena_10ml;
        [SerializeField] protected Animator animator;
        
        [FormerlySerializedAs("_fluidMaterial")] [SerializeField] private Material _fluidAnhidridaMaterial;
        [SerializeField] protected Material _fluidBenzenaMaterial;
        [SerializeField] protected float value1Ml = 0.0008f;
        [SerializeField] protected float value10Ml = 0.008f;
        
        protected bool isCheckWeightToContinue;
        
        protected enum LarutanState
        {
            Larutan1,
            Larutan2
        }
        protected LarutanState currentLarutanState = LarutanState.Larutan1;
        
        public void CreatePenambahanLarutanAnhidridaButton()
        {
            currentLarutanState = LarutanState.Larutan1;
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
                _fluidBenzenaMaterial.SetFloat("_Fill", fillValue + value10Ml);
            });
           
        }
        
        public void CreatePenambahanLarutanBenzenaButton()
        {
            currentLarutanState = LarutanState.Larutan2;
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

        protected void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
            }
        }
        
        protected virtual bool IsCurrentWeightComplete()
        {
            return currentWeight == currentTargetWeight;
        }
        
        protected virtual bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > currentTargetWeight;
        }

        protected void RestartCurrentInteraction()
        {
            currentWeight = 0; 
            if (currentLarutanState == LarutanState.Larutan1)
            {
                _fluidAnhidridaMaterial.SetFloat("_Fill", 0);
            }
            else if(currentLarutanState == LarutanState.Larutan2)
            {
                _fluidBenzenaMaterial.SetFloat("_Fill", 0);
            }
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
        }
        
        protected void PlayAnimation(AnimationClip clip)
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
                   RestartCurrentInteraction();
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