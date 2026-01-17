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
        
        [Header("TARGET VOLUME")]
        public int currentTargetWeight;
        public int targetWeightLarutan1 = 40;
        public int targetWeightLarutan2 = 80;

        [Header("ANIMATION")]
        [FormerlySerializedAs("animClipAnhidrida_1ml")] [SerializeField] protected AnimationClip animClipLarutan1_1ml;
        [FormerlySerializedAs("animClipAnhidrida_10ml")] [SerializeField] protected AnimationClip animClipLarutan1_10ml;
        
        [FormerlySerializedAs("animClipBenzena_1ml")] [SerializeField] protected AnimationClip animClipLarutan2_1ml;
        [FormerlySerializedAs("animClipBenzena_10ml")] [SerializeField] protected AnimationClip animClipLarutan2_10ml;
        [SerializeField] protected Animator animator;
        
        [Header("SHADER EFFECTS SETTINGS")]
        [SerializeField] private Renderer larutan1Renderer;
        [SerializeField] private Renderer larutan2Renderer;
        
        private Material larutan1Material;
        private Material larutan2Material;
        
        [SerializeField] protected float value1Ml = 0.0008f;
        [SerializeField] protected float value10Ml = 0.008f;
        
        protected bool isCheckWeightToContinue;
        
        protected enum LarutanState
        {
            Larutan1,
            Larutan2
        }
        protected LarutanState currentLarutanState = LarutanState.Larutan1;


        protected void Awake()
        {
            larutan1Material = larutan1Renderer.material;      
            larutan2Material = larutan2Renderer.material;       
        }

        protected void OnDisable()
        {
            SetLarutanFilledValue(larutan1Material, 0);
            SetLarutanFilledValue(larutan2Material, 0);
        }

        public void CreatePenambahanLarutanAnhidridaButton()
        {
            currentLarutanState = LarutanState.Larutan1;
            currentTargetWeight = targetWeightLarutan1;
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                PlayAnimation(animClipLarutan1_1ml);
                
                // Todo : Animasi tabung bertambah after dituangin pake coroutine aja
                SetLarutanFilledValue(larutan1Material, GetLarutanFilledValue(larutan1Material) + value1Ml);
            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "10 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 10;
                PlayAnimation(animClipLarutan1_10ml);
                
                // Todo : Animasi tabung bertambah after dituangin pake coroutine aja
                SetLarutanFilledValue(larutan1Material, GetLarutanFilledValue(larutan1Material) + value10Ml);
            });
           
        }
        
        public void CreatePenambahanLarutanBenzenaButton()
        {
            currentLarutanState = LarutanState.Larutan2;
            currentTargetWeight = targetWeightLarutan2;
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                PlayAnimation(animClipLarutan2_1ml);
              
                // Todo : Animasi tabung bertambah after dituangin pake coroutine aja
                SetLarutanFilledValue(larutan2Material, GetLarutanFilledValue(larutan2Material) + value1Ml);
            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "10 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 10;
                PlayAnimation(animClipLarutan2_10ml);
            
                // Todo : Animasi tabung bertambah after dituangin pake coroutine aja
                SetLarutanFilledValue(larutan2Material, GetLarutanFilledValue(larutan2Material) + value10Ml);
            });
           
        }

        protected float GetLarutanFilledValue(Material targetMaterial)
        {
            float fillValue = 0;
            if (targetMaterial)
            {
                if(targetMaterial.HasProperty("_Fill"))
                {
                    fillValue = targetMaterial.GetFloat("_Fill");
                }
            }
            return fillValue;
        }

        protected void SetLarutanFilledValue(Material targetMaterial, float value)
        {
            if (targetMaterial)
            {
                if (targetMaterial.HasProperty("_Fill"))
                {
                    targetMaterial.SetFloat("_Fill", value);
                }
            }
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
                SetLarutanFilledValue(larutan1Material, 0);
            }
            else if(currentLarutanState == LarutanState.Larutan2)
            {
                SetLarutanFilledValue(larutan2Material, 0);
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