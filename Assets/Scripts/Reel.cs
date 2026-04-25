using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Reel : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform viewport;
    public RectTransform content;
    public GameObject symbolPrefab;
    public int symbolCount = 7; 
    public float symbolHeight = 75f;
    public float spinSpeed = 2500f;
    public float stopDuration = 1.0f;

    private List<Image> symbols = new List<Image>();
    private Sprite[] availableSprites;
    private bool isSpinning = false;
    private float currentY = 0f;
    private Sprite targetSprite;
    private bool shouldStop = false;

    public bool IsSpinning => isSpinning;

    public void Setup(Sprite[] availableSymbols)
    {
        availableSprites = availableSymbols;
        
        // Clear existing
        foreach (Transform child in content)
        {
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }
        symbols.Clear();

        // Create symbols
        for (int i = 0; i < symbolCount; i++)
        {
            GameObject obj = Instantiate(symbolPrefab, content);
            obj.name = "Symbol_" + i;
            Image img = obj.GetComponent<Image>();
            img.sprite = availableSprites[Random.Range(0, availableSprites.Length)];
            img.preserveAspect = true;
            img.color = Color.white;
            img.raycastTarget = false;
            symbols.Add(img);
            
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(symbolHeight, symbolHeight);
        }
        
        currentY = 0;
        UpdatePositions();
    }

    public void StartSpin()
    {
        isSpinning = true;
        shouldStop = false;
        StartCoroutine(SpinRoutine());
    }

    public void StopSpin(Sprite resultSprite)
    {
        targetSprite = resultSprite;
        shouldStop = true;
    }

    private IEnumerator SpinRoutine()
    {
        float speed = 0;
        float targetSpeed = spinSpeed;
        
        // Accelerate
        while (isSpinning && speed < targetSpeed)
        {
            speed += Time.deltaTime * 3000f;
            currentY += speed * Time.deltaTime;
            UpdatePositions();
            yield return null;
        }

        // Spin until stop is requested
        while (!shouldStop)
        {
            currentY += targetSpeed * Time.deltaTime;
            UpdatePositions();
            yield return null;
        }

        // Decelerate and Snap to target with Overshoot Bounce
        float decelerateTime = stopDuration;
        float elapsed = 0;
        float startY = currentY;

        // Choose a symbol to be the winner. 
        symbols[0].sprite = targetSprite;
        
        float targetWrappedY = -symbolHeight; // This puts symbol 0 in the middle slot
        float totalHeight = symbolCount * symbolHeight;
        float finalY = startY + totalHeight * 2; 
        
        float currentMod = finalY % totalHeight;
        finalY = finalY - currentMod + (targetWrappedY + totalHeight) % totalHeight;

        // Overshoot value (in units)
        float overshoot = 20f;

        while (elapsed < decelerateTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / decelerateTime;
            
            // Back/Overshoot effect logic:
            // Custom curve that goes slightly past 1 and then back to 1
            // f(t) = 1 - (1-t)^3 + sin(pi * t) * (1-t) * intensity
            float overshootFactor = Mathf.Sin(Mathf.PI * t) * (1f - t) * (overshoot / totalHeight);
            float ease = 1f - Mathf.Pow(1f - t, 3); 
            
            currentY = Mathf.Lerp(startY, finalY, ease + overshootFactor);
            UpdatePositions();
            yield return null;
        }

        currentY = finalY;
        UpdatePositions();
        isSpinning = false;
        }

        public void SetPulse(int line, bool active)
        {
        // lines: 0=Top, 1=Middle, 2=Bottom
        float totalHeight = symbolCount * symbolHeight;
        float halfTotalHeight = totalHeight * 0.5f;

        float targetY = 0;
        if (line == 0) targetY = symbolHeight;
        else if (line == 2) targetY = -symbolHeight;

        for (int i = 0; i < symbolCount; i++)
        {
            float y = (currentY - i * symbolHeight) % totalHeight;
            if (y > halfTotalHeight) y -= totalHeight;
            if (y < -halfTotalHeight) y += totalHeight;

            if (Mathf.Abs(y - targetY) < 5f)
            {
                Animator anim = symbols[i].GetComponent<Animator>();
                if (anim != null) anim.SetBool("Pulse", active);
            }
        }
        }

    public Sprite[] GetResults()
    {
        Sprite[] results = new Sprite[3];
        float totalHeight = symbolCount * symbolHeight;
        float halfTotalHeight = totalHeight * 0.5f;

        for (int i = 0; i < symbolCount; i++)
        {
            float y = (currentY - i * symbolHeight) % totalHeight;
            if (y > halfTotalHeight) y -= totalHeight;
            if (y < -halfTotalHeight) y += totalHeight;

            if (Mathf.Abs(y - symbolHeight) < 5f) results[0] = symbols[i].sprite; 
            if (Mathf.Abs(y - 0) < 5f) results[1] = symbols[i].sprite; 
            if (Mathf.Abs(y + symbolHeight) < 5f) results[2] = symbols[i].sprite; 
        }
        return results;
    }

    private void UpdatePositions()
    {
        if (symbols == null || symbols.Count == 0) return;
        
        float totalHeight = symbolCount * symbolHeight;
        float halfTotalHeight = totalHeight * 0.5f;
        
        float stretch = isSpinning ? 1.2f : 1.0f; 

        for (int i = 0; i < symbolCount; i++)
        {
            if (symbols[i] == null) continue;
            
            float y = (currentY - i * symbolHeight) % totalHeight;
            
            if (y > halfTotalHeight) y -= totalHeight;
            if (y < -halfTotalHeight) y += totalHeight;
            
            symbols[i].rectTransform.anchoredPosition = new Vector2(0, y);
            symbols[i].rectTransform.localScale = new Vector3(1.0f, stretch, 1.0f);
        }
    }
}

