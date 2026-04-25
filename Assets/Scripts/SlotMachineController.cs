using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SlotMachineController : MonoBehaviour
{
    [Header("UI References")]
    public Reel[] reels;
    public Button spinButton;
    public Text statusText;
    public Text scoreText;
    public GameObject winPopup;
    public Text winPopupText;
    public Text profitText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spinClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip warningClip;

    [Header("Bet Buttons")]
    public Button[] betButtons;
    public int[] betAmounts = { 10, 50, 100 };
    private int selectedBetIndex = -1;

    [Header("Assets")]
    public Sprite[] symbolSprites;
    
    [Header("Settings")]
    public int initialScore = 1000;
    
    private int currentScore;
    private bool isGameActive = false;

    [Header("Lever Sprites")]
    public Sprite leverUpSprite;
    public Sprite leverDownSprite;
    public UnityEngine.UI.Image leverImage;

    private void Start()
    {
        currentScore = initialScore;
        UpdateUI();
        
        foreach (var reel in reels)
        {
            if (reel != null) reel.Setup(symbolSprites);
        }

        if (spinButton != null) spinButton.onClick.AddListener(OnSpinClick);
        
        for (int i = 0; i < betButtons.Length; i++)
        {
            int index = i;
            betButtons[i].onClick.AddListener(() => SelectBet(index));
        }

        statusText.text = "Select BET and pull LEVER!";
        if (winPopup != null) winPopup.SetActive(false);
        
        if (leverImage != null && leverUpSprite != null)
            leverImage.sprite = leverUpSprite;

        HighlightBetButtons();
    }

    private void SelectBet(int index)
    {
        if (isGameActive) return;
        selectedBetIndex = index;
        HighlightBetButtons();
        statusText.text = $"Bet selected: ${betAmounts[selectedBetIndex]}";
    }

    private void HighlightBetButtons()
    {
        for (int i = 0; i < betButtons.Length; i++)
        {
            ColorBlock cb = betButtons[i].colors;
            cb.normalColor = (i == selectedBetIndex) ? Color.yellow : Color.white;
            cb.selectedColor = (i == selectedBetIndex) ? Color.yellow : Color.white;
            betButtons[i].colors = cb;
        }
    }

    private void OnSpinClick()
    {
        if (isGameActive) return;

        if (selectedBetIndex == -1)
        {
            statusText.text = "Select a bet first!";
            if (audioSource != null && warningClip != null)
                audioSource.PlayOneShot(warningClip);
            return;
        }

        int spinCost = betAmounts[selectedBetIndex];
        if (currentScore < spinCost)
        {
            statusText.text = "Not enough coins!";
            return;
        }

        StartCoroutine(PlayRound(spinCost));
    }

    private IEnumerator PlayRound(int cost)
    {
        isGameActive = true;
        if (winPopup != null) winPopup.SetActive(false); 
        if (profitText != null) profitText.gameObject.SetActive(false);

        // Reset all symbol animations
        foreach (var reel in reels)
        {
            if (reel != null)
            {
                reel.SetPulse(0, false);
                reel.SetPulse(1, false);
                reel.SetPulse(2, false);
            }
        }

        currentScore -= cost;
        UpdateUI();
        statusText.text = "Spinning...";

        // Play Spin Sound
        if (audioSource != null && spinClip != null)
        {
            audioSource.clip = spinClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Lever Down
        Vector3 originalScale = Vector3.one;
        Vector2 originalPos = Vector2.zero;
        RectTransform leverRT = null;

        if (leverImage != null)
        {
            leverRT = leverImage.GetComponent<RectTransform>();
            originalScale = leverRT.localScale;
            originalPos = leverRT.anchoredPosition;

            if (leverDownSprite != null)
            {
                leverImage.sprite = leverDownSprite;
                leverRT.localScale = originalScale * 0.5f;
                leverRT.anchoredPosition = originalPos + new Vector2(0, -150f);
                Canvas.ForceUpdateCanvases();
            }
        }

        // Start all reels with a slight delay
        foreach (var reel in reels)
        {
            if (reel != null) reel.StartSpin();
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.4f);

        // Lever Up
        if (leverImage != null && leverUpSprite != null)
        {
            leverImage.sprite = leverUpSprite;
            if (leverRT != null)
            {
                leverRT.localScale = originalScale;
                leverRT.anchoredPosition = originalPos;
            }
        }

        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null)
            {
                reels[i].StopSpin(symbolSprites[Random.Range(0, symbolSprites.Length)]);
                yield return new WaitForSeconds(0.8f); 
            }
        }

        bool anySpinning = true;
        while (anySpinning)
        {
            anySpinning = false;
            foreach (var reel in reels)
            {
                if (reel != null && reel.IsSpinning) anySpinning = true;
            }
            yield return null;
        }

        Sprite[][] results = new Sprite[reels.Length][];
        for (int i = 0; i < reels.Length; i++)
        {
            results[i] = reels[i].GetResults();
        }

        CheckWinLines(results);
        isGameActive = false;
    }

    private void CheckWinLines(Sprite[][] results)
    {
        if (audioSource != null) audioSource.Stop();

        bool won = false;
        int totalWin = 0;
        
        int payoutPerLine = 0;
        switch (selectedBetIndex)
        {
            case 0: payoutPerLine = 50; break;
            case 1: payoutPerLine = 200; break;
            case 2: payoutPerLine = 500; break;
            default: payoutPerLine = 50; break;
        }

        for (int line = 0; line < 3; line++)
        {
            if (results[0][line] != null && 
                results[0][line] == results[1][line] && 
                results[1][line] == results[2][line])
            {
                won = true;
                totalWin += payoutPerLine; 
                
                foreach (var reel in reels)
                {
                    if (reel != null) reel.SetPulse(line, true);
                }
            }
        }

        if (won)
        {
            if (audioSource != null && winClip != null)
                audioSource.PlayOneShot(winClip);

            currentScore += totalWin;
            statusText.text = $"BIG WIN! +{totalWin} Coins";
            
            if (winPopup != null)
            {
                winPopup.SetActive(true);
                if (winPopupText != null)
                    winPopupText.text = $"JACKPOT!\nYOU WON {totalWin} COINS";
            }

            if (profitText != null)
            {
                StartCoroutine(ShowProfitSplash(totalWin));
            }

            UpdateUI();
        }
        else
        {
            if (audioSource != null && loseClip != null)
                audioSource.PlayOneShot(loseClip);

            statusText.text = "Try again!";
        }
    }

    private IEnumerator ShowProfitSplash(int amount)
    {
        profitText.text = $"+{amount}";
        profitText.gameObject.SetActive(true);
        
        Vector3 startPos = profitText.transform.localPosition;
        float elapsed = 0;
        float duration = 2.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            profitText.transform.localPosition = startPos + Vector3.up * (elapsed * 100f);
            Color c = profitText.color;
            c.a = Mathf.Clamp01(1f - (elapsed / duration));
            profitText.color = c;
            yield return null;
        }

        profitText.gameObject.SetActive(false);
        profitText.transform.localPosition = startPos;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Coins: {currentScore}";
    }

    public void CloseWinPopup()
    {
        if (winPopup != null) winPopup.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Application.Quit();
#else
        Application.Quit();
#endif
    }
}