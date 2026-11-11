using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wallet : MonoBehaviour, ISavable {
    [SerializeField] float money;

    public static Wallet i { get; private set; }

    public event Action OnMoneychanged;

    public float Money => money;

    private void Awake() {
        i = this;
    }

    public void AddMoney(float amount) {
        money += amount;
        OnMoneychanged?.Invoke();
    }

    public void TakeMoney(float amount) {
        money -= amount;
        OnMoneychanged?.Invoke();
    }

    public bool HasEnoughMoney(float amount) {
        return amount <= money;
    }

    public object CaptureState() {
        return money;
    }

    public void RestoreState(object state) {
        money = (float)state;
    }
}
