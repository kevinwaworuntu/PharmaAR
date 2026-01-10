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
            tahapanInteractionController.OnFinishPlayingInteraction += OnFinishPlayingTahapanInteraction;
            tahapanInteractionController.OnInteractionComplete += OnInteractionComplete;
        }
    }
    
    public void OnTargetFound()
    {
        //ToDo : CleanUp
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

    private void OnFinishPlayingTahapanInteraction()
    {
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.ContinueInteraction();
    }
    private void OnInteractionComplete()
    {
        //Debug.Break();
    }

    public void OnTargetLost()
    {
        if (UIManager.Instance == null)
        {
            return;
        }
        UIManager.Instance.HideAllARPopups();
    }
}