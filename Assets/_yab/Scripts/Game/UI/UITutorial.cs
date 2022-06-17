using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib.UI;

public class UITutorial : MonoBehaviour
{
  [SerializeField] InputOverlayTutorial tutorial = null;


  public static System.Action onTutorialDone;

  List<Transform> listPositions = new List<Transform>();

  Level level = null;
  
  void Awake()
  {
    Level.onStart += OnLevelCreated;
    Level.onTutorialStart += OnTutorialStart;
    Level.onItemThrow += OnItemThrow;
    Level.onFinished += OnLevelFinished;
  }
  void OnDestroy()
  {
    Level.onStart -= OnLevelCreated;
    Level.onTutorialStart -= OnTutorialStart;
    Level.onItemThrow += OnItemThrow;
    Level.onFinished -= OnLevelFinished;
  }
  void OnLevelCreated(Level lvl)
  {
    tutorial.Deactivate();
  }
  void OnTutorialStart(Level lvl)
  {
    level = lvl;
    tutorial.Activate(lvl.arrow(0).transform.position);
  }
  void OnTutorialPowerup(Level lvl, GameState.Powerups.Type type)
  {
    
  }
  void OnItemThrow(Level lvl)
  {
    if(lvl.LevelIdx == 0)
      tutorial.Deactivate();
  }
  void OnLevelFinished(Level lvl)
  {
    level = null;
  }
}
