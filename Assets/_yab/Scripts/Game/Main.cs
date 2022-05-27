using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.DataSystem;

[DefaultExecutionOrder(-1)]
public class Main : MonoBehaviour
{
  [Header("DataRefs")]
  [SerializeField] GameState  gameState;
  [SerializeField] GameData   gameData;

  [Header("Debugparams")]
  [SerializeField] int       framerate = 60;
  
  void Awake()
  {
    Application.targetFrameRate = 60;
    DataManager.LoadAllData();
  }
  void OnApplicationPause(bool paused)
  {
    if(paused)
      DataManager.SaveAllData();
  }
  void OnApplicationQuit()
  {
    DataManager.SaveAllData();
  }

  #if UNITY_EDITOR
  void Update()
  {
    Application.targetFrameRate = framerate;
  }
  #endif
}
