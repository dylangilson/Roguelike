using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create New Quest")]
public class QuestBase : ScriptableObject {
    [SerializeField] string questName;
    [SerializeField] string description;
    [SerializeField] Dialogue startDialogue;
    [SerializeField] Dialogue inProgressDialogue;
    [SerializeField] Dialogue completedDialogue;
    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;

    public string QuestName {
        get { return questName; }
    }

    public string Description {
        get { return description; }
    }

    public Dialogue StartDialogue {
        get { return startDialogue; }
    }

    public Dialogue InProgressDialogue {
        get { return inProgressDialogue?.Lines?.Count > 0 ? inProgressDialogue : startDialogue; }
    }

    public Dialogue CompletedDialogue {
        get { return completedDialogue; }
    }

    public ItemBase RequiredItem {
        get { return requiredItem; }
    }

    public ItemBase RewardItem {
        get { return rewardItem; }
    }
}
