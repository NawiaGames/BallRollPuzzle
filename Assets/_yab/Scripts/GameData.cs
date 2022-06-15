
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;


[CreateAssetMenu, DefaultExecutionOrder(-2)]
public class GameData : ScriptableObject
{
  private static GameData static_this = null;
  public static  GameData get(){ return static_this; }
  GameData()
  {
    static_this = this;
  }

  [Header("Prefabs")]
  [SerializeField] Item[] items;
  [SerializeField] Item[] moveItems;
  [SerializeField] Item pushItem;
  [SerializeField] Item pushLineItem;
  [SerializeField] Item bombItem;
  [SerializeField] Item staticItem;
  [SerializeField] Item colorChangeItem;
  [SerializeField] Item painterItem;

  [SerializeField] ObjectFracture fractureItem;
  //[SerializeField] Frac
  [Header("Field Elems")]
  [SerializeField] Arrow    arrowPrefab;
  [SerializeField] GridElem gridElemPrefab;
  [Header("Levels")]
  [SerializeField] List<Level> listLevels;
  [SerializeField] List<Reward> listRewards;
  [Header("Points")]
  [SerializeField] int pointBallOut = 50;
  [SerializeField] int pointBallOutEveryNext = 10;
  [SerializeField] int pointMatchStandard = 100;
  [SerializeField] int pointMatchSpec = 200;
  [SerializeField] int pointBombExplode = 100;
  [SerializeField] int pointMoveLeft = 500;
  [SerializeField] float[] percentOfPointsForStars;
  [SerializeField] string[] comboText;

  [SerializeField] Color[]    themeColors;
  public static Color[] GetThemeColors() => get().themeColors;

  [System.Serializable]
  public struct Reward
  { 
    public static readonly int PowerupsTypesCnt = 4;
    public int level;
    public int powerupBombs;
    public int powerupColors;
    public int powerupPainters;
    public int powerupArrows;

    public Reward(int lvl = -1)
    {
      level = lvl;
      powerupBombs = 0;
      powerupColors = 0;
      powerupPainters = 0;
      powerupArrows = 0;
    }
    public bool IsValid() => level >= 0;
    public int  GetPowerupCnt(GameState.Powerups.Type type)
    {
      if(type == GameState.Powerups.Type.Bomb)
        return powerupBombs;
      else if(type == GameState.Powerups.Type.Color)
        return powerupColors;
      else if(type == GameState.Powerups.Type.Painter)
        return powerupPainters;
      else if(type == GameState.Powerups.Type.Arrows)
        return powerupArrows;
      else 
        return 0;
    }
  }
  public static class Prefabs
  {
    public static Item BombPrefab => get().bombItem;
    public static Item ColorChangeItem => get().colorChangeItem;
    public static Item PainterItem => get().painterItem;
    public static Arrow CreateArrow(Transform parent){return Instantiate(get().arrowPrefab, parent);}
    public static GridElem CreateGridElem(Transform parent) { return Instantiate(get().gridElemPrefab, parent); }
    public static Item CreatePushItem(Transform parent, Item.Push push_type)
    { 
      Item item = null;
      if(push_type == Item.Push.One)
        item = Instantiate(get().pushItem, parent);
      else
        item = Instantiate(get().pushLineItem, parent);  
      item.name = get().pushItem.name;
      return item;
    }
    public static ObjectFracture CreateObjectFracture(Transform parent)
    {
      return Instantiate(get().fractureItem, parent);
    }
    public static Item CreateBombItem(Transform parent)
    {
      var item = Instantiate(get().bombItem, parent);
      item.name = get().bombItem.name;
      return item;      
    }
    public static Item CreateStaticItem(Transform parent)
    {
      var item = Instantiate(get().staticItem, parent);
      item.name = get().staticItem.name;
      return item;
    }
    public static Item CreateColorChangeItem(Transform parent)
    {
      var item = Instantiate(get().colorChangeItem, parent);
      item.name = get().colorChangeItem.name;
      return item;
    }
    public static Item CreateRandItem(Transform parent, bool moveable) 
    {
      var prefabItem = (moveable)? get().moveItems.get_random() : get().items.get_random();
      var item = Instantiate(prefabItem, parent);
      item.name = prefabItem.name;
      return item;
    }
    public static Item CreateRandItem(List<string> names, Transform parent) 
    {
      Item item = null;
      List<Item> prefabs = new List<Item>(get().items);
      prefabs.AddRange(get().moveItems);
      prefabs.RemoveAll((Item it) => !names.Contains(it.name));
      prefabs.shuffle(20);
      var prefab = prefabs.get_random();
      item = Instantiate(prefab, parent);
      item.name = prefab.name;
      item.transform.position = Vector3.zero;
      return item;
    }    
  }
  public static class Levels
  {
    static public Level GetPrefab(int idx) => get().listLevels[idx];
    static public Level CreateLevel(int idx, Transform levelsContainer)
    {
      return Instantiate(get().listLevels[idx], levelsContainer);
    }
    static public int   PrevLevel(int lvl_idx)
    {
      return (int)Mathf.Repeat(lvl_idx - 1.0f, get().listLevels.Count);
    }
    static public int   NextLevel(int lvl_idx)
    {
      return (int)Mathf.Repeat(lvl_idx + 1.0f, get().listLevels.Count); // - 1.0f);
    }
  }
  public static class Points
  {
    public static int ballOut(int num) => get().pointBallOut + get().pointBallOutEveryNext * num;
    public static int matchStandard => get().pointMatchStandard;
    public static int bombExplode => get().pointBombExplode;
    public static int moveLeft => get().pointMoveLeft;
    public static float percentForStars(int stars) => get().percentOfPointsForStars[Mathf.Clamp(stars, 0, get().percentOfPointsForStars.Length-1)];
    public static string randomComboText => get().comboText.get_random();
  }
  public static class Rewards
  {
    public static int     PowerupsCnt => Reward.PowerupsTypesCnt;
    public static Reward? GetReward(int level)
    {
      Reward? reward = null;

      int idx = get().listRewards.FindIndex((Reward rew) => rew.level == level);
      if(idx >= 0)
        reward = get().listRewards[idx];

      return reward;
    }

    public static int GetLevelForPowerup(GameState.Powerups.Type type)
    {
      int ret = -1;
      foreach(var reward in get().listRewards)
      {
        if(reward.GetPowerupCnt(type) > 0)
        {
          ret = reward.level;
          break;
        }
      }
      return ret;
    }

    public static mr.Range<int> GetRewardProgress()
    {
      var range = new mr.Range<int>();
      int lvl = GameState.Progress.Level;
      range.beg = 0;
      range.end = get().listRewards[0].level;

      for(int q = 0; q < get().listRewards.Count; ++q)
      {
        var lvl2 = get().listRewards[q].level;
        if(lvl > lvl2)
        {
          range.beg = range.end+1;
          range.end = lvl2;
        }
      }

      return range;
    }
  }
}