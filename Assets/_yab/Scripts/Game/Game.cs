using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.InputSystem;

public class Game : MonoBehaviour
{
  [SerializeField] Transform levelsContainer;

  public static System.Action<Level> onLevelRestart;

  Level level = null;

	void Awake()
  {
    TouchInputData.onTap += OnInputTapped;
    TouchInputData.onInputStarted += OnInputBeg;
    TouchInputData.onInputUpdated += OnInputMov;
    TouchInputData.onInputEnded += OnInputEnd;
  }
  void OnDestroy()
  {
    TouchInputData.onTap -= OnInputTapped;
    TouchInputData.onInputStarted -= OnInputBeg;
    TouchInputData.onInputUpdated -= OnInputMov;
    TouchInputData.onInputEnded -= OnInputEnd;
  }
  IEnumerator Start()
  {
    yield return new WaitForSeconds(0.125f);
    CreateLevel();
  }

  void OnInputTapped(TouchInputData tid)
  {
    //level?.OnInputTapped(tid);
  }
  void OnInputBeg(TouchInputData tid)
  {
    level?.OnInputBeg(tid);
  }
  void OnInputMov(TouchInputData tid)
  {
    level?.OnInputMov(tid);
  }
  void OnInputEnd(TouchInputData tid)
  {
    level?.OnInputEnd(tid);
  }

  public void CreateLevel()
  {
    if(level)
      Destroy(level.gameObject);

    level = GameData.Levels.CreateLevel(GameState.Progress.Level, levelsContainer);
    FindObjectOfType<UIStart>(true).Show(level);
  }
  public void RestartLevel()
  {
    CreateLevel();
    onLevelRestart?.Invoke(level);
  }
  public void PrevLevel()
  {
    GameState.Progress.Level = GameData.Levels.PrevLevel(GameState.Progress.Level);
    CreateLevel();
  }
  public void NextLevel()
  {
    GameState.Progress.Level = GameData.Levels.NextLevel(GameState.Progress.Level);
    CreateLevel();
  }

#if UNITY_EDITOR
  void Update()
  {
    if(Input.GetKeyDown(KeyCode.Z))
      PrevLevel();
    else if(Input.GetKeyDown(KeyCode.X))
      NextLevel();
    else if(Input.GetKeyDown(KeyCode.R))
      RestartLevel();
    // else if(Input.GetKeyDown(KeyCode.E))
    //   level?.DbgFinishLevel();
  }
#endif
}
