using Gameplay;
using UnityEngine;

public class ARContentManager : MonoBehaviour
{
    private int currentIndex;
    private TahapanInteractionController tahapanInteractionController;

    private void Start()
    {
        tahapanInteractionController = GetComponent<TahapanInteractionController>();
        if (tahapanInteractionController != null)
        {
            tahapanInteractionController.OnStartWaitingForPlayerInputToContinue += OnStartWaitingForPlayerInputToContinueHandler;
            tahapanInteractionController.OnFinishPlayingInteraction += OnFinishPlayingTahapanInteractionHandler;
            tahapanInteractionController.OnInteractionComplete += OnInteractionCompleteHandler;
        }
    }

    private void OnEnable()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.btnCompleteTahapan.gameObject.SetActive(false);
        UIManager.Instance.btnNextInteraction.gameObject.SetActive(true);
    }

    public void OnTargetFound()
    {
        if (UIManager.Instance.IsPanelInfoActive())
        {
            return;
        }
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.StartInteraction();
    }

    private void OnStartWaitingForPlayerInputToContinueHandler()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.btnCompleteTahapan.gameObject.SetActive(false);
        UIManager.Instance.btnNextInteraction.gameObject.SetActive(true);
        
        // ToDo : Play Button Next Interaction Visual Cue
        if (!tahapanInteractionController)
        {
            return;
        }
        UIManager.Instance.btnNextInteraction.onClick.RemoveAllListeners();
        UIManager.Instance.btnNextInteraction.onClick.AddListener(tahapanInteractionController.PlayerInteractToFinishInteraction);
        UIManager.Instance.btnNextInteraction.onClick.AddListener(() =>
        {
            UIManager.Instance.btnCompleteTahapan.gameObject.SetActive(false);
            UIManager.Instance.btnNextInteraction.gameObject.SetActive(false);
        });
    }
    
    private void OnFinishPlayingTahapanInteractionHandler()
    {
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.ContinueInteraction();
    }
    private void OnInteractionCompleteHandler()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.btnCompleteTahapan.gameObject.SetActive(true);
        UIManager.Instance.btnNextInteraction.gameObject.SetActive(false);

        UIManager.Instance.btnCompleteTahapan.onClick.RemoveAllListeners();
        UIManager.Instance.btnNextInteraction.onClick.AddListener(GameManager.Instance.CompleteCurrentTahap);
    }

    public void OnTargetLost()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.HideAllARPopups();
    }
}