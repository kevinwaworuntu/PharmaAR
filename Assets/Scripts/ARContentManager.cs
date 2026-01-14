using Gameplay;
using UnityEngine;

public class ARContentManager : MonoBehaviour
{
    protected TahapanInteractionController tahapanInteractionController;

    protected void Start()
    {
        tahapanInteractionController = GetComponent<TahapanInteractionController>();
        if (tahapanInteractionController != null)
        {
            tahapanInteractionController.OnStartWaitingForPlayerInputToContinue += OnStartWaitingForPlayerInputToContinueHandler;
            tahapanInteractionController.OnFinishPlayingInteraction += OnFinishPlayingTahapanInteractionHandler;
            tahapanInteractionController.OnInteractionComplete += OnInteractionCompleteHandler;
        }
    }

    protected void OnEnable()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.btnCompleteTahapan.gameObject.SetActive(false);
        UIManager.Instance.btnNextInteraction.gameObject.SetActive(false);
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

    protected virtual void OnStartWaitingForPlayerInputToContinueHandler()
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
    
    protected void OnFinishPlayingTahapanInteractionHandler()
    {
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.ContinueInteraction();
    }
    protected void OnInteractionCompleteHandler()
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