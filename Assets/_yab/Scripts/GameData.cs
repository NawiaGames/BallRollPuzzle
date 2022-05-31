
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
  [Header("Field Elems")]
  [SerializeField] Arrow    arrowPrefab;
  [SerializeField] GridElem gridElemPrefab;
  [Header("Levels")]
  [SerializeField] List<Level> listLevels;

  [SerializeField] Color[]    themeColors;
  public static Color[] GetThemeColors() => get().themeColors;

  public static class Prefabs
  {
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
}
