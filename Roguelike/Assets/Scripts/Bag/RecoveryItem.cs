using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create New Recovery Item")]
public class RecoveryItem : ItemBase {
    [Header("Hitpoints")]
    [SerializeField] int amountHealed;
    [SerializeField] bool restoreMaxHitpoints;

    [Header("PowerPoints")]
    [SerializeField] int powerPointsRecovered;
    [SerializeField] bool restoreMaxPowerPoints;

    [Header("Condition")]
    [SerializeField] ConditionID statusCure;
    [SerializeField] bool cureAll;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;
}
