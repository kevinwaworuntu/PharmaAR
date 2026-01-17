using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;

namespace Gameplay
{
    public class TK8Manager : ARContentManager
    {
        [SerializeField] private float[] weights;
        [SerializeField] private float[] floorTargets;
        [SerializeField] private float[] ceilTargets;

        private float currentWeight = 0;
        private int currentIndex; // get better naming
        
        [SerializeField] protected AnimationClip animClipOpenKeran;
        [SerializeField] protected AnimationClip animClipCloseKeran;
        [SerializeField] protected AnimationClip animClipTetes;
        [SerializeField] protected Animator animator;
        
        [SerializeField] private Renderer titrasiRenderer;
        private Material titrasiMatInstance;
        [SerializeField] protected Color initialColor;
        [SerializeField] protected Color targetColor;
        
        [SerializeField] protected TextMeshProUGUI textSampelWeight;
        
        [Header("SHADER EFFECTS SETTINGS")]
        [SerializeField] private Renderer buretRenderer;
        
        private Material buretMaterial;

        private float startingBuretFilledValue = 0.1775f;
        [SerializeField] protected float value01Ml = 0.0004f;
        
        private bool isPlaying = false;
        protected bool isCheckWeightToContinue;

        private void Awake()
        {
            buretMaterial = buretRenderer.material;     
            titrasiMatInstance = titrasiRenderer.material;
        }

        private void OnEnable()
        {
            base.OnEnable();
         
            SetLarutanFilledValue(buretMaterial, startingBuretFilledValue);
         
            titrasiMatInstance.SetColor("_Side_Color", initialColor);
            titrasiMatInstance.SetColor("_TopColor", initialColor);
        }

        public void SetCurrentWeightIndex(int value)
        {
            currentIndex = value;
        }
        
        protected bool IsCurrentWeightComplete()
        {
            return currentWeight >= floorTargets[currentIndex] && currentWeight <= ceilTargets[currentIndex];
        }
        
        protected bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > ceilTargets[currentIndex];
        }
        
        protected void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
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
            textSampelWeight.gameObject.SetActive(false);
        }
        
        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }

        protected void RestartCurrentInteraction()
        {
            currentWeight = 0; 
            titrasiMatInstance.SetColor("_Side_Color", initialColor);
            titrasiMatInstance.SetColor("_TopColor", initialColor);
            
            SetLarutanFilledValue(buretMaterial, startingBuretFilledValue);
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
        }
        
        public void CreateButtonInteraction()
        {
            SetSampelWeightTextVisible(); // Temporary here
            titrasiMatInstance.SetColor("_Side_Color", initialColor);
            titrasiMatInstance.SetColor("_TopColor", initialColor);
            
            SetLarutanFilledValue(buretMaterial, startingBuretFilledValue);
            
            ContextualButtonController.Instance.GenerateContextualButton(2);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "0.1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 0.1f;
                TetesanSequenceExecutor(1);

            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "1 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 1f;
                TetesanSequenceExecutor(10);
            });
        }
        public void TetesanSequenceExecutor(int totalTetes)
        {
            if (isPlaying)
            {
                return;
            }
            StartCoroutine(TetesanSequence(totalTetes));
        }
        
        private IEnumerator TetesanSequence(int totalTetes)
        {
            isPlaying = true;
            SetButtonEnabledState(false);
            var animationConfig = GameManager.Instance.AnimationConfig; // ensure != null
            
            if (animationConfig.GenericAnimController)
            {
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipOpenKeran;
            }
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(animClipOpenKeran.length);

            for (int i = 0; i < totalTetes; i++)
            {
                if (animationConfig.GenericAnimController)
                {
                    animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipTetes;
                }
                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                SetLarutanFilledValue(buretMaterial, GetLarutanFilledValue(buretMaterial) + value01Ml);
                yield return new WaitForSeconds(animClipTetes.length);
            }
            
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(animClipOpenKeran.length);

            SetButtonEnabledState(true);
            isPlaying = false;
            if (IsCurrentWeightComplete())
            {
                StartCoroutine(DelaySetTargetColor());
                IEnumerator DelaySetTargetColor()
                {
                    yield return new WaitForSeconds(1);
                    titrasiMatInstance.SetColor("_Side_Color", targetColor);
                    titrasiMatInstance.SetColor("_TopColor", targetColor);
                }
                OnStartWaitingForPlayerInputToContinueHandler();
            }
            else
            {
                if (IsCurrentWeightExceedTarget())
                {
                    RestartCurrentInteraction();
                }
            }
        }

        public void SetSampelWeightTextVisible()
        {
            StartCoroutine(DelaySetSampelWeight());
            IEnumerator DelaySetSampelWeight() // Wait for current index updated
            {
                yield return new WaitForSeconds(2f);
                textSampelWeight.SetText($"{weights[currentIndex]} mg");
                textSampelWeight.gameObject.SetActive(true);
            }
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
    }
}