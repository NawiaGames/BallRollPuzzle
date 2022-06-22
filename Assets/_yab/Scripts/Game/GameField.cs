using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
  [SerializeField] GameObject water;
  [SerializeField] GameObject background;

  void Start()
  {
    SetBkg(GameLib.Defaults.GetCurrentQualityPreset() == GameLib.Defaults.VideoQualityPresets.HQ);
  }

  void SetBkg(bool hq)
  {
    water.SetActive(hq); 
    background.SetActive(!hq);
  }
}
