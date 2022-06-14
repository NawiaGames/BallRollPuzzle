
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
    public int maxLevelPlayed = 0;
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
    public int claimedOnLevel = -1;
    public int bombs;
    public int colors;
    public int painters;
    public int arrows;

    public int Get(Powerups.Type type)
    {
      if(type == Powerups.Type.Bomb)
        return bombs;
      else if(type == Powerups.Type.Color)
        return colors;
      else if(type == Powerups.Type.Painter)
        return painters;
      else if(type == Powerups.Type.Arrows)
        return arrows;

      return 0;
    }
    public void Add(Powerups.Type type, int amount)
    {
      if(type == Powerups.Type.Bomb)
        bombs = Mathf.Max(bombs + amount, 0);
      else if(type == Powerups.Type.Color)
        colors = Mathf.Max(colors + amount, 0);
      else if(type == Powerups.Type.Painter)
        painters = Mathf.Max(painters + amount, 0);
      else if(type == Powerups.Type.Arrows)
        arrows = Mathf.Max(arrows + amount, 0);
    }
  }
  [SerializeField] PowerUps powerups;

  public static class Progress
  {
    public static int   Level 
    {
      get => get().progress.level; 
      set
      {
        get().progress.level = value; 
        get().progress.maxLevelPlayed = Mathf.Max(get().progress.maxLevelPlayed, get().progress.level);
      }
    }
    public static int LevelPlayed => get().progress.maxLevelPlayed;
  }

  public static class Econo
  {
    public static int Cash {get => get().economy.cash; set{ get().economy.cash = value;}}
  }

  public static class Powerups
  {
    public enum Type
    {
      None = -1,
      Bomb,
      Color,
      Painter,
      Arrows,
      Cnt,
    }
    public static int  BombsCnt {get => get().powerups.bombs; set{ get().powerups.bombs = value;}}
    public static int  ColorsCnt {get => get().powerups.colors; set{ get().powerups.colors = value;}}
    public static int  PaintersCnt { get => get().powerups.painters; set { get().powerups.painters = value; } }
    public static int  ArrowsCnt { get => get().powerups.arrows; set { get().powerups.arrows = value; } }
    public static int  ClaimedOnLevel {get => get().powerups.claimedOnLevel; set{ get().powerups.claimedOnLevel = value;}}
    public static int  GetCount(Type type) => get().powerups.Get(type);
    public static void Used(Type type) => get().powerups.Add(type, -1);

    public static void AddReward(GameData.Reward? reward)
    {
      if(reward != null && reward.Value.level > ClaimedOnLevel)
      {
        BombsCnt += reward.Value.powerupBombs;
        ColorsCnt += reward.Value.powerupColors;
        PaintersCnt += reward.Value.powerupPainters;
        ArrowsCnt += reward.Value.powerupArrows;
        ClaimedOnLevel = reward.Value.level;
      }
    }
    public static bool PowerupsToShow() => ClaimedOnLevel >= 0;
  }

  [Header("Customization")]
  [SerializeField] int selectedTheme = 0;
  public int SetNextTheme() {
    selectedTheme = (int)Mathf.Repeat(++selectedTheme, GameData.GetThemeColors().Length);
    return selectedTheme;
  }
}
