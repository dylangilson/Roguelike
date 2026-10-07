using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletUI : MonoBehaviour
{
    [SerializeField] Text moneyTxt;

    private void Start() {
        Wallet.i.OnMoneychanged += SetMoneyText;
    }

    public void Show() {
        gameObject.SetActive(true);
        SetMoneyText();
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    public void SetMoneyText() {
        moneyTxt.text = "$" +  Wallet.i.Money;
    }
}
