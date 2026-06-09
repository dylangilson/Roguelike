using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestStatus { NONE, STARTED, COMPLETED }

public class Quest {
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase questBase) {
        Base = questBase;
    }

    public IEnumerator StartQuest() {
        Status = QuestStatus.STARTED;

        yield return DialogueManager.Instance.ShowDialogue(Base.StartDialogue);
    }

    public IEnumerator CompleteQuest(Transform player) {
        Status = QuestStatus.COMPLETED;

        yield return DialogueManager.Instance.ShowDialogue(Base.CompletedDialogue);

        var inventory = Bag.GetBag();

        if (Base.RequiredItem != null) {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null) {
            inventory.AddItem(Base.RewardItem);

            string playerName = player.GetComponent<PlayerController>().PlayerName;
            yield return DialogueManager.Instance.ShowDialogueText($"{playerName} received {Base.RewardItem.ItemName}!");
        }
    }

    public bool CanBeCompleted() {
        var inventory = Bag.GetBag();
        if (Base.RequiredItem != null) {
            if (!inventory.HasItem(Base.RequiredItem)) {
                return false;
            }
        }
        return true;
    }
}
