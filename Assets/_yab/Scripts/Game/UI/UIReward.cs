using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UIReward : MonoBehaviour
{
  [SerializeField] TMPLbl _lblQ;
  [SerializeField] string _strFmtQ = "+{0}";

  public void Quantity(int q)
  {
    _lblQ.text = string.Format(_strFmtQ, q);
    gameObject.SetActive(q>0);
  }
}
