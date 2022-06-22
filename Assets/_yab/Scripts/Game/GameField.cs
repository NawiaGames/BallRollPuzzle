using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
  [SerializeField] GameObject water;
  [SerializeField] GameObject background;

  public bool hq {get; set;}

  void Start()
  {
    SetBkg(GameLib.Defaults.GetCurrentQualityPreset() == GameLib.Defaults.VideoQualityPresets.HQ);
  }

  public void SetBkg(bool _hq)
  {
    hq = _hq;
    water.SetActive(hq); 
    background.SetActive(!hq);
  }
}
