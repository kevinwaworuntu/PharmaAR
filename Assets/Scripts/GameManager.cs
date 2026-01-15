using Config;
using UnityEngine;
using Vuforia;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Mode Saat Ini")]
    public GameMode currentMode;

    [Header("Data Tahapan")]
    public TahapanData[] tahapanTBA;
    public TahapanData[] tahapanKompleksometri;

    public GameObject[] markerTBAMapping; // improve, not a propper way
    public GameObject[] markerTKMapping; // improve if have time, not a propper way
    
    [Header("State (runtime only)")]
    [Tooltip("Tahap yang sedang dikerjakan saat ini. -1 = belum ada.")]
    public int currentAttemptingTahapIndex = -1;

    private const string LAST_COMPLETED_TAHAP_TBA_KEY = "LastCompletedTahapTBA";
    private const string LAST_COMPLETED_TAHAP_KOMP_KEY = "LastCompletedTahapKomp";

    [Header("Config Data")] 
    [SerializeField] private AnimationConfig animationConfig;
    // ToDo : Move should not clutter GameManager
    [SerializeField] private InfoStyleConfig styleConfigDefault;
    [SerializeField] private InfoStyleConfig styleConfigTBA;
    [SerializeField] private InfoStyleConfig styleConfigTK;

    public AnimationConfig AnimationConfig => animationConfig;
    private string CurrentProgressKey
    {
        get
        {
            return currentMode == GameMode.TBA
                ? LAST_COMPLETED_TAHAP_TBA_KEY
                : LAST_COMPLETED_TAHAP_KOMP_KEY;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetARCameraActive(false);
    }
    
    public void SetARCameraActive(bool isActive)
    {
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = isActive;
            Debug.Log($"[GameManager] Vuforia AR Camera: {(isActive ? "ON" : "OFF")}");
        }
        else
        {
            Debug.LogWarning("[GameManager] VuforiaBehaviour instance not found!");
        }
    }
    
    public int GetLastCompletedTahapIndex()
    {
        return PlayerPrefs.GetInt(CurrentProgressKey, -1);
    }
    
    public void SetMode(GameMode mode)
    {
        currentMode = mode;
        currentAttemptingTahapIndex = -1;

        Debug.Log($"[GameManager] SetMode = {currentMode}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            var panelToOpen = (currentMode == GameMode.TBA)
                ? UIManager.Instance.panelTBA
                : UIManager.Instance.panelTK;

            UIManager.Instance.ShowPanelAndAddToHistory(panelToOpen);
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
    
    public void StartTahap(int tahapIndex)
    {
        int lastCompleted = GetLastCompletedTahapIndex();
        
        if (tahapIndex > lastCompleted + 1)
        {
            Debug.LogWarning($"[GameManager] Tahap {tahapIndex + 1} masih terkunci. LastCompleted = {lastCompleted}");
            return;
        }
        switch (currentMode)
        {
            case GameMode.TBA:
                if(tahapIndex < markerTBAMapping.Length) markerTBAMapping[tahapIndex].SetActive(true);
                break;
            case GameMode.Kompleksometri:
                if(tahapIndex < markerTKMapping.Length) markerTKMapping[tahapIndex].SetActive(true);
                break;
        }
        currentAttemptingTahapIndex = tahapIndex;

        // ToDo : Data cuman buat debug (?)
        TahapanData data = GetCurrentTahapanData(tahapIndex);
        string namaTahap = data != null ? data.namaTahapan : $"Tahap {tahapIndex + 1}";
        Debug.Log($"[GameManager] MULAI Tahap {tahapIndex + 1} - {namaTahap} (Mode {currentMode})");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.ShowPanelAndAddToHistory(UIManager.Instance.panelScanAR);
        }

        SetARCameraActive(true);
    }

    public void BackFrromCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            Debug.LogWarning("[GameManager] CompleteCurrentTahap dipanggil tapi tidak ada tahap aktif.");
            return;
        }
        switch (currentMode)
        {
            case GameMode.TBA:
                if(currentAttemptingTahapIndex < markerTBAMapping.Length) markerTBAMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
            case GameMode.Kompleksometri:
                if(currentAttemptingTahapIndex < markerTKMapping.Length) markerTKMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
        }
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.GoBack();
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
    public void CompleteCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            Debug.LogWarning("[GameManager] CompleteCurrentTahap dipanggil tapi tidak ada tahap aktif.");
            return;
        }
        switch (currentMode)
        {
            case GameMode.TBA:
                if(currentAttemptingTahapIndex < markerTBAMapping.Length) markerTBAMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
            case GameMode.Kompleksometri:
                if(currentAttemptingTahapIndex < markerTKMapping.Length) markerTKMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
        }
        
        PlayerPrefs.SetInt(CurrentProgressKey, currentAttemptingTahapIndex);
        PlayerPrefs.Save();

        Debug.Log($"[GameManager] SELESAI Tahap {currentAttemptingTahapIndex + 1} (Mode {currentMode}). " +
                  $"Tahap berikutnya yang akan terbuka: {currentAttemptingTahapIndex + 2}");

        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.GoBack();
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }

    public void ShowInfoPopup(GameObject infoPanel, string infoText)
    {
        if (infoPanel == null) return;

        var textComponent = infoPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent != null)
        {
            
            if (string.IsNullOrWhiteSpace(infoText))
            {
                int idx = currentAttemptingTahapIndex;
                if (currentMode == GameMode.TBA)
                    infoText = (idx >= 0 && idx < InfoTextBank.TBA.Length) ? InfoTextBank.TBA[idx] : "";
                else
                    infoText = (idx >= 0 && idx < InfoTextBank.TK.Length) ? InfoTextBank.TK[idx] : "";
            }
            textComponent.text = infoText;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            switch (currentMode)
            {
                case GameMode.TBA:
                    RetrieveTextStyle(textComponent, styleConfigTBA);
                    break;
                case GameMode.Kompleksometri:
                    RetrieveTextStyle(textComponent, styleConfigTK);
                    break;
                default:
                    RetrieveTextStyle(textComponent, styleConfigDefault);
                    break;
            }
        }

        infoPanel.SetActive(true);
        Debug.Log("[GameManager] Info popup ditampilkan (bank).");
    }
    
    // ToDo
    public void RetrieveTextStyle(TextMeshProUGUI textComponent, InfoStyleConfig config)
    {
        // ToDo default value if null
        if (config == null)
        {
            Debug.LogError("[GameManager] InfoStyleConfig config is null.");
            Debug.Break();
            return;
        }
        InfoStyleStruct style = config.GetStyle();
        
        textComponent.font = style.Font;
        textComponent.fontSize = style.FontSize;
        textComponent.alignment = style.Alignment;
        textComponent.fontStyle = style.IsBold ? FontStyles.Bold : FontStyles.Normal;
        textComponent.lineSpacing = style.LineSpacing;
        textComponent.paragraphSpacing = style.ParagraphSpacing;
        textComponent.margin = new Vector4(style.MarginLeft, style.MarginTop, style.MarginRight, style.MarginBottom);    
    }

    public void HideInfoPopup(GameObject infoPanel)
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);  
            Debug.Log("[GameManager] Info popup disembunyikan.");
        }
    }
    
    public TahapanData GetCurrentTahapanData(int index)
    {
        if (currentMode == GameMode.TBA)
        {
            if (tahapanTBA != null && index >= 0 && index < tahapanTBA.Length)
                return tahapanTBA[index];
        }
        else 
        {
            if (tahapanKompleksometri != null && index >= 0 && index < tahapanKompleksometri.Length)
                return tahapanKompleksometri[index];
        }

        return null;
    }
    
    public void OnMarkerFound(string markerName)
    {
        Debug.Log($"[GameManager] Marker ditemukan: {markerName}. (Validasi tahapan DIMATIKAN sementara untuk debug indexing.)");
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_TBA_KEY);
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_KOMP_KEY);
        PlayerPrefs.Save();

        currentAttemptingTahapIndex = -1;
        UIManager.Instance.ForceHideInfoPanel();
        SetARCameraActive(false);

        Debug.Log("[GameManager] ResetAllProgress: semua progres dihapus. Kembali ke Tahap 1 untuk tiap mode.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
}
