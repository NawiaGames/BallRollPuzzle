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
    if(lvl.tutorial == Level.Tutorial.Push)
      tutorial.Activate(lvl.arrow(0).transform.position);
    else if(lvl.tutorial == Level.Tutorial.PushOut)
    {
      var rt = lvl.Dim / 2;
      rt.x++;
      tutorial.Activate(lvl.FindArrow(rt).transform.position);
    }
  }
  void OnTutorialPowerupFirst(Level lvl, GameState.Powerups.Type type)
  {

  }
  void OnItemThrow(Level lvl)
  {
    if(lvl.tutorial == Level.Tutorial.Push || lvl.tutorial == Level.Tutorial.PushOut)
    {
      tutorial.Deactivate();
      onTutorialDone?.Invoke();
    }
  }
  void OnLevelFinished(Level lvl)
  {
    level = null;
  }
}
