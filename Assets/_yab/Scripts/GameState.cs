
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib.DataSystem;

[CreateAssetMenu, DefaultExecutionOrder(-2)]
public class GameState : SavableScriptableObject
{
  static private GameState static_this = null;
  static public  GameState get() { return static_this;}
  public GameState()
  {
    static_this = this;
  }

  [System.Serializable]
  class StateProgress
  {
    public int level = 0;
  }
  [SerializeField] StateProgress progress;

  [System.Serializable]
  class Economy
  {
    public int cash = 0;
  }
  [SerializeField] Economy economy;

  [System.Serializable]
  class PowerUps
  {
    public int bombs;
    public int colors;
    public int painters;
    public int arrows;
  }
  [SerializeField] PowerUps powerups;

  public static class Progress
  {
    public static int   Level {get => get().progress.level; set{get().progress.level = value;}}
  }

  public static class Econo
  {
    public static int Cash {get => get().economy.cash; set{ get().economy.cash = value;}}
  }

  public static class Powerups
  {
    public static int BombsCnt {get => get().powerups.bombs; set{ get().powerups.bombs = value;}}
    public static int ColorsCnt {get => get().powerups.colors; set{ get().powerups.colors = value;}}
    public static int PaintersCnt { get => get().powerups.painters; set { get().powerups.painters = value; } }
    public static int ArrowsCnt { get => get().powerups.arrows; set { get().powerups.arrows = value; } }
    public static int GetCount(int idx)
    {
      if(idx == 0)
        return BombsCnt;
      else if(idx == 1)
        return ColorsCnt;
      else if(idx == 2)
        return PaintersCnt;
      else if(idx == 3)
        return ArrowsCnt;
      return 0; 
    }
    public static void AddReward(GameData.Reward? reward)
    {
      if(reward != null)
      {
        BombsCnt += reward.Value.powerupBombs;
        ColorsCnt += reward.Value.powerupColors;
        PaintersCnt += reward.Value.powerupPainters;
        ArrowsCnt += reward.Value.powerupArrows;
      }
    }
  }

  [Header("Customization")]
  [SerializeField] int selectedTheme = 0;
  public int SetNextTheme() {
    selectedTheme = (int)Mathf.Repeat(++selectedTheme, GameData.GetThemeColors().Length);
    return selectedTheme;
  }
}
