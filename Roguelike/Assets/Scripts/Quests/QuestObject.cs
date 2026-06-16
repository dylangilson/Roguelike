using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour {
    [SerializeField] QuestBase questToCheck;
    [SerializeField] ObjectActions onStart;
    [SerializeField] ObjectActions onComplete;

    QuestList questList;

    private void Start() {
        questList = QuestList.GetQuestList();
        questList.OnUpdated += UpdateObjectStatus;

        UpdateObjectStatus();
    }

    private void OnDestroy() {
        questList.OnUpdated -= UpdateObjectStatus;
    }

    public void UpdateObjectStatus() {
        if (onStart != ObjectActions.DO_NOTHING && questList.IsStarted(questToCheck.QuestName)) {
            foreach (Transform child in transform) {
                if (onStart == ObjectActions.ENABLE) {
                    child.gameObject.SetActive(true);
                } else if (onStart == ObjectActions.DISABLE) {
                    child.gameObject.SetActive(false);
                }
            }
        }

        if (onComplete != ObjectActions.DO_NOTHING && questList.IsCompleted(questToCheck.QuestName)) {
            foreach (Transform child in transform) {
                if (onComplete == ObjectActions.ENABLE) {
                    child.gameObject.SetActive(true);
                } else if (onComplete == ObjectActions.DISABLE) {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}

public enum ObjectActions { DO_NOTHING, ENABLE, DISABLE }