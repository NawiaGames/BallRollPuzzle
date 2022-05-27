using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class BtnStd : MonoBehaviour
{
  [SerializeField] TMPLbl lbl;
  [SerializeField] Image  ico;

  public string Text {set{if(lbl)lbl.text = value;}}
  public Sprite Icon {set{if(ico)ico.sprite = value;}}
}
