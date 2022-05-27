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
    Level.onFinished += OnLevelFinished;
  }
  void OnDestroy()
  {
    Level.onStart -= OnLevelCreated;
    Level.onTutorialStart -= OnTutorialStart;
    Level.onFinished -= OnLevelFinished;
  }
  void OnLevelCreated(Level lvl)
  {
    tutorial.Deactivate();
  }
  void OnTutorialStart(Level lvl)
  {
    level = lvl;
  }
  void OnLevelFinished(Level lvl)
  {
    level = null;
  }
}
