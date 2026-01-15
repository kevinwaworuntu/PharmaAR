using System;
using Config;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility;

namespace Gameplay
{
    public class TahapanInteractionController : MonoBehaviour
    {
        [Serializable]
        private struct TahapanInteractionMappingStruct
        {
            public TahapanInteractionData InteractionData;
            public bool IsNeedPlayerInputToContinue;
            public UnityEvent UniqueEvent;
        }

        [SerializeField] private TahapanInteractionMappingStruct[] interactionDataActionMappings;

        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;

        private int currentInteractionIndex;
        private bool isPlaying;
        private bool isStarted;
        private TahapanInteractionPlayer interactionPlayer;

        public event Action OnStartWaitingForPlayerInputToContinue;
        public event Action OnInteractionComplete;
        public event Action OnFinishPlayingInteraction;

        void Awake()
        {
            interactionPlayer = new TahapanInteractionPlayer();
            interactionPlayer.Initialize(this, animator, audioSource, GameManager.Instance.AnimationConfig);
        }

        private void OnEnable()
        {
            currentInteractionIndex = 0;
        }

        private void OnDisable()
        {
            isStarted = false;
            if (UIManager.Instance)
            {
                if (UIManager.Instance.btnPlayAnimation) UIManager.Instance.btnPlayAnimation.onClick.RemoveAllListeners();
                if (UIManager.Instance.btnStopAnimation) UIManager.Instance.btnStopAnimation.onClick.RemoveAllListeners();
            }
        }
        
        [ContextMenu("Start Interaction")]
        public void StartInteraction()
        {
            EnterInteractionState(currentInteractionIndex);
        }

        [ContextMenu("Continue Interaction")]
        public void ContinueInteraction()
        {
            EnterInteractionState(currentInteractionIndex);
        }

        public void RestartInteraction()
        {
            isPlaying = false;
            EnterInteractionState(currentInteractionIndex);
        }

        [ContextMenu("Simulating Tap")]
        public void PlayerInteractToFinishInteraction()
        {
            if (!isPlaying)
            {
                return;
            }
            if (!interactionDataActionMappings[currentInteractionIndex].IsNeedPlayerInputToContinue)
            {
                return;
            }
            ExitInteractionState();
        }

        private void EnterInteractionState(int targetIndex)
        {
            if (!CanEnterState(targetIndex))
            {
                return;
            }

            currentInteractionIndex = targetIndex;
            isPlaying = true;

            UIManager.Instance.NarationPanel.SetVisibility(HasNarationText());
            if (HasAnimationClip())
            {
                if (UIManager.Instance)
                {
                    UIManager.Instance.SetButtonAnimationVisibility(true);
                    if (UIManager.Instance.btnPlayAnimation)
                    {
                        UIManager.Instance.btnPlayAnimation.onClick.RemoveAllListeners();
                        UIManager.Instance.btnPlayAnimation.onClick.AddListener(RequestPlayAnimation);
                    }
                    if (UIManager.Instance.btnStopAnimation)
                    {
                        UIManager.Instance.btnStopAnimation.onClick.RemoveAllListeners();
                        UIManager.Instance.btnStopAnimation.onClick.AddListener(RequestStopAnimation);
                    }
                }   
            }
            
            MainInteractionState();
        }

        private void MainInteractionState()
        {
            interactionDataActionMappings[currentInteractionIndex].UniqueEvent?.Invoke();
            interactionPlayer.Play(interactionDataActionMappings[currentInteractionIndex].InteractionData, () =>
            {
                //bool isPlayingLastIndex = interactionDataActionMappings.Length - 1 == currentInteractionIndex;
                if (interactionDataActionMappings[currentInteractionIndex].IsNeedPlayerInputToContinue)// && !isPlayingLastIndex)
                {
                    OnStartWaitingForPlayerInputToContinue?.Invoke();
                    return;
                }
                ExitInteractionState();
            });
        }

        private void ExitInteractionState()
        {
            isPlaying = false;
            currentInteractionIndex++;
            OnFinishPlayingInteraction?.Invoke();
            UIManager.Instance.SetButtonAnimationVisibility(false);
            if (currentInteractionIndex >= interactionDataActionMappings.Length)
            {
                if (UIManager.Instance)
                {
                    UIManager.Instance.NarationPanel.SetVisibility(false);
                }
                OnInteractionComplete?.Invoke();
            }
        }

        private bool CanEnterState(int targetIndex)
        {
            if (targetIndex >= interactionDataActionMappings.Length)
            {
                return false;
            }

            if (targetIndex < 0)
            {
                return false;
            }
            if (!interactionDataActionMappings[targetIndex].InteractionData)
            {
                return false;
            }

            if (isPlaying)
            {
                return false;
            }
            return true;
        }

        private bool HasNarationText()
        {
            return !string.IsNullOrEmpty(interactionDataActionMappings[currentInteractionIndex].InteractionData.Title) && !string.IsNullOrEmpty(interactionDataActionMappings[currentInteractionIndex].InteractionData.Description);
        }

        private bool HasAnimationClip()
        {
            return interactionDataActionMappings[currentInteractionIndex].InteractionData.AnimationClip;
        }

        private void RequestPlayAnimation()
        {
            if (currentInteractionIndex >= interactionDataActionMappings.Length)
            {
                return;
            }
            interactionPlayer.Play(interactionDataActionMappings[currentInteractionIndex].InteractionData);
        }

        private void RequestStopAnimation()
        {
            interactionPlayer.Stop();
        }

        public bool IsPlayingLastIndex()
        {
            return interactionDataActionMappings.Length - 1 == currentInteractionIndex;
        }
    }
}