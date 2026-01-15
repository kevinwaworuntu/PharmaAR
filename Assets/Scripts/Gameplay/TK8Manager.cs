using System.Collections;
using UI;
using UnityEngine;

namespace Gameplay
{
    public class TK8Manager : ARContentManager
    {
        public float floorTarget = 10;
        public float ceilTarget = 10.2f;
        public float currentWeight = 0;
        
        [SerializeField] protected AnimationClip animClipOpenKeran;
        [SerializeField] protected AnimationClip animClipCloseKeran;
        [SerializeField] protected AnimationClip animClipTetes;
        [SerializeField] protected Animator animator;
        
        [SerializeField] private Renderer titrasiRenderer;
        private Material titrasiMatInstance;
        [SerializeField] protected Color initialColor;
        [SerializeField] protected Color targetColor;
        
        private bool isPlaying = false;
        protected bool isCheckWeightToContinue;

        private void OnEnable()
        {
            base.OnEnable();
            titrasiMatInstance = titrasiRenderer.material;
            titrasiMatInstance.SetColor("_Side_Color", initialColor);
            titrasiMatInstance.SetColor("_TopColor", initialColor);
        }

        [ContextMenu("TestGantiWarna 1")]
        protected void TestGantiWarna1()
        {
            titrasiMatInstance.SetColor("_Side_Color", initialColor);
            titrasiMatInstance.SetColor("_TopColor", initialColor);
        }
        [ContextMenu("TestGantiWarna 2")]
        protected void TestGantiWarna2()
        {
            titrasiMatInstance.SetColor("_Side_Color", targetColor);
            titrasiMatInstance.SetColor("_TopColor", targetColor);
        }
        
        protected bool IsCurrentWeightComplete()
        {
            return currentWeight >= floorTarget && currentWeight <= ceilTarget;
        }
        
        protected bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > ceilTarget;
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
            titrasiMatInstance.SetColor("_Side_Color", initialColor);
            titrasiMatInstance.SetColor("_TopColor", initialColor);
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
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
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
        }

        public void SetCeilTarget(float value)
        {
            ceilTarget = value;
        }
        public void SetFloorTarget(float value)
        {
            floorTarget = value;
        }

        public void CreateButtonInteraction()
        {
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
                yield return new WaitForSeconds(animClipTetes.length);
            }
            
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(animClipOpenKeran.length);

            SetButtonEnabledState(true);
            isPlaying = false;
            if (IsCurrentWeightComplete())
            {
                titrasiMatInstance.SetColor("_Side_Color", targetColor);
                titrasiMatInstance.SetColor("_TopColor", targetColor);
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
    }
}