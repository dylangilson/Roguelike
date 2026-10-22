using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour {
    [SerializeField] Text countText;
    [SerializeField] Text priceText;

    bool selected;
    bool cancelled;
    int currentCount;
    int maxCount;
    float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected, Action onCancel=null) {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;
        selected = false;
        cancelled = false;
        currentCount = 1;

        gameObject.SetActive(true);

        SetValues();

        yield return new WaitUntil(() => selected || cancelled);

        if (cancelled) {
            onCancel?.Invoke();
        } else {
            onCountSelected?.Invoke(currentCount);
        }

        gameObject.SetActive(false);
    }

    private void Update() {
        int prevCount = currentCount;

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            cancelled = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentCount++;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentCount--;
        }

        currentCount = Mathf.Clamp(currentCount, 1, maxCount);

        if (currentCount != prevCount) {
            SetValues();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            selected = true;
        }
    }

    private void SetValues() {
        countText.text = "x" + currentCount;
        priceText.text = "¥" + pricePerUnit * currentCount;
    }
}
