using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;
using GameLib.CameraSystem;
using GameLib.Splines;
using GameLib.InputSystem;
using GameLib.Utilities;

public class Level : MonoBehaviour
{
  public static System.Action<Vector3> onIntroFx;
  public static System.Action<Level>   onStart, onPlay, onFirstInteraction, onTutorialStart;
  public static System.Action<Level>   onFinished;

  [Header("Refs")]
  [SerializeField] Transform _arrowsContainer;
  [SerializeField] Transform _gridContainer;
  [SerializeField] Transform _itemsContainer;
  [SerializeField] Transform _nextItemContainer;

  public class Grid
  {
    Vector2Int _dim = Vector2Int.zero;
    Item[,]    _grid = null;

    public void init(Vector2Int dims)
    {
      _grid = new Item[dims.y, dims.x];
      _dim = dims;
    }

    public void clear()
    {
      System.Array.Clear(_grid, 0, _grid.Length);
    }
    public Vector2Int dim() => _dim;
    public bool isInside(Vector2Int v) => v.x >= -_dim.x/2  && v.x <= _dim.x/2 && v.y >= -_dim.y/2  && v.y <= _dim.y/2;

    public void set(Item item)
    {
      set(item.grid, item);
    } 
    public Item geti(Vector2Int igrid)
    {
      return geta(igrid + _dim/2);
    }
    public void set(Vector2Int igrid, Item item)
    {
      var v = igrid + _dim/2;
      _grid[v.y, v.x] = item;
    }
    public Item geta(Vector2Int grid)
    {
      if(grid.x >= 0 && grid.x < dim().x && grid.y >= 0 && grid.y < dim().y)
        return _grid[grid.y, grid.x];
      else
        return null;  
    }
    public Vector2Int clamp(Vector2Int igrid)
    {
      return new Vector2Int(Mathf.Clamp(igrid.x, -_dim.x/2, _dim.x/2), Mathf.Clamp(igrid.y, -_dim.y/2, _dim.y/2));
    }
    public Vector2Int getDest(Vector2Int vbeg, Vector2Int vdir, bool clamping)
    {
      Vector2Int vend = vbeg;
      for(int q = 0; q < dim().x; ++q)
      {
        vend += vdir;
        if(clamping)
          vend = clamp(vend);
        if(Mathf.Abs(vend.x) > dim().x/2 || Mathf.Abs(vend.y) > dim().y / 2)
        {
          vend = clamp(vend) + vdir.to_units();
          break;
        }
        else
        {
          Item block = geti(vend);
          if(block && block.grid != vbeg)
          {
            vend = block.grid - vdir;
            break;
          }
        }
      }
      return vend;
    }
    public void update(List<Item> _items)
    {
      clear();
      for(int q = 0; q < _items.Count; ++q)
      {
        if(isInside(_items[q].grid))
          set(_items[q]);
        else
        {
          _items[q].Hide();
          _items.RemoveAt(q);
          q--;
        } 
      }
    }
    // public Item seta(Vector2Int grid, Item item)
    // {
    //   _grid[grid.y, grid.x] = item;
    // }
  }
  public struct Match3
  {
    public List<Item> _matches;
    public Match3(Item i0, Item i1, Item i2)
    {
      _matches = new List<Item>(){i0, i1, i2};
    }
  }

  public enum PushType
  {
    None,
    PushOne,
    PushLine,
  };
  public enum GameType
  {
    Match3,
    Match3Move,
  }
  [Header("Gameplay Variants")]
  [SerializeField] GameType  _gameplayType = GameType.Match3;
  [SerializeField] bool      _gameplayOutside = false;
  [SerializeField] PushType  _gameplayPushType = PushType.None;

  [Header("Settings")]
  [SerializeField] Vector2Int _dim;
  [SerializeField] float _speed = 8;

  public int  LevelIdx => GameState.Progress.Level;
  public bool Succeed => true;
  public bool Finished {get; private set;}
  public GameType gameType => _gameplayType;
  public PushType pushType => _gameplayPushType;
  public bool     gameOutside => _gameplayOutside;

  bool _started = false;
  UISummary uiSummary = null;

  Grid _grid = new Grid();
  List<Arrow> _arrows = new List<Arrow>();
  List<Arrow> _arrowsSelected = new List<Arrow>();
  List<Item> _items = new List<Item>();
  List<Item> _moving = new List<Item>();
  List<Match3> _matching = new List<Match3>();

  Item _nextItem = null;

  void Awake()
  {
    _grid.init(_dim);
    _items = _itemsContainer.GetComponentsInChildren<Item>().ToList();
    uiSummary = FindObjectOfType<UISummary>(true);
  }
  void OnDestroy()
  {

  }
  IEnumerator Start()
  {
    yield return null;
    Init();
    _started = true;
    onStart?.Invoke(this);
  }
  
  void Init()
  {
    float scale = 1;
    Vector2Int vgrid = Vector2Int.zero;
    for(int x = 0; x < _dim.x; ++x)
    {
      vgrid.x = -_dim.x / 2 + x;
      vgrid.y = _dim.y / 2 + 1;
      var arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(0, -1);
      _arrows.Add(arrow);
      float xx = (-_dim.x + 1) * 0.5f + x;
      arrow.transform.localPosition = new Vector3(xx, 0, _dim.y/2 + 1.0f);
      arrow.transform.Rotate(new Vector3(0, 180, 0));
      arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      vgrid.y = -_dim.y / 2 - 1;
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(0, 1);
      _arrows.Add(arrow);
      arrow.transform.localPosition = new Vector3(xx, 0, -_dim.y/2 - 1.0f);
    }
    for(int y = 0; y < _dim.y; ++y)
    {
      vgrid.y = -_dim.y/2 + y ;
      vgrid.x = -_dim.x/2 - 1;
      var arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(1, 0);      
      _arrows.Add(arrow);
      float yy = (-_dim.y + 1) * 0.5f + y;
      arrow.transform.localPosition = new Vector3(-_dim.x/2 - 1.0f, 0, yy * scale);
      arrow.transform.Rotate(new Vector3(0, 90, 0));
      arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      vgrid.x = _dim.x / 2 + 1;
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(-1, 0);
      _arrows.Add(arrow);
      arrow.transform.localPosition = new Vector3(_dim.x / 2 + 1.0f, 0, -yy * scale);
      arrow.transform.Rotate(new Vector3(0, -90, 0));
    }
    
    for(int y = 0; y < _dim.y; ++y)
    {
      float yy = (-_dim.y + 1) * 0.5f + y;
      for(int x = 0; x < _dim.y; ++x)
      {
        float xx = (-_dim.x + 1) * 0.5f + x;
        var ge = GameData.Prefabs.CreateGridElem(_gridContainer);
        ge.even = ((x ^ y) & 1) == 0;
        ge.transform.localPosition = new Vector3(xx * scale, 0.05f, yy * scale);
      }
    }

    _grid.update(_items);

    _items.ForEach((item) => item.IsArrow = _gameplayType == GameType.Match3Move);

    _nextItemContainer.gameObject.SetActive(true);
    _nextItem = CreateNextItem();
    _nextItem.IsArrow = false;
    if(_gameplayPushType != PushType.None)
      _nextItemContainer.gameObject.SetActive(false);
  }
  List<Item> _pushing = new List<Item>();

  Arrow arrowBeg = null, arrowEnd = null;
  void SelectArrows()
  {
    _arrowsSelected.Clear();
    if(arrowBeg == null || arrowEnd == null)
      return;

    Vector3 dir = arrowBeg.vPos - arrowEnd.vPos;
    dir.x = Mathf.Abs(dir.x);
    dir.z = Mathf.Abs(dir.z);
    bool _sel = false;
    for(int q = 0; q < _arrows.Count; ++q)
    {
      var ar = _arrows[q];
      if(dir.x > dir.z)
        _sel = Mathf.Sign(ar.grid.x - arrowBeg.grid.x) != Mathf.Sign(ar.grid.x - arrowEnd.grid.x) && ar.grid.y == arrowEnd.grid.y;
      else
        _sel = Mathf.Sign(ar.grid.y - arrowBeg.grid.y) != Mathf.Sign(ar.grid.y - arrowEnd.grid.y) && ar.grid.x == arrowEnd.grid.x;
      _arrows[q].IsSelected = _sel;
      if(_sel)
        _arrowsSelected.Add(_arrows[q]);
    }
    arrowBeg.IsSelected = true;
    arrowEnd.IsSelected = true;
    _arrowsSelected.Add(arrowBeg);
    _arrowsSelected.Add(arrowEnd);
    _arrowsSelected = _arrowsSelected.Distinct().ToList();
  }
  void ClearArrows()
  {
    _arrowsSelected.Clear();
    arrowEnd = null;
    for(int q = 0; q < _arrows.Count; ++q)
      _arrows[q].IsSelected = false;
  }
  Item CreateNextItem()
  {
    List<string> names = _items.Select((Item it)=>it.name).Distinct().ToList();
    if(names.Count > 0)
      return GameData.Prefabs.CreateRandItem(names,_nextItemContainer);
    else
      return GameData.Prefabs.CreateRandItem(_nextItemContainer);
  }
  public void OnInputBeg(TouchInputData tid)
  {
    arrowBeg = arrowEnd = null;
    if(_pushing.Count > 0 || _moving.Count > 0 || _matching.Count > 0 || _items.Count == 0)
      return;

    arrowBeg = tid.GetClosestCollider(0.5f)?.GetComponent<Arrow>() ?? null;
    if(arrowBeg)
    {
      arrowEnd = arrowBeg;
      SelectArrows();
    }
  }
  public void OnInputMov(TouchInputData tid)
  {
    if(_pushing.Count > 0 || _moving.Count > 0 || _matching.Count > 0 || _items.Count == 0)
      return;

    arrowEnd = tid.GetClosestCollider(0.5f)?.GetComponent<Arrow>() ?? null;
    if(arrowEnd)
      SelectArrows();
    else
      ClearArrows();
  }
  public void OnInputEnd(TouchInputData tid)
  {
    arrowBeg = null;
    arrowEnd = null;

    if(_pushing.Count > 0 || _moving.Count > 0 || _matching.Count > 0 || _items.Count == 0)
      return;

    for(int q = 0; q < _arrowsSelected.Count; ++q)
    {
      Item push = null;//GameData.Prefabs.CreatePushItem(_itemsContainer);
      if(_gameplayPushType == PushType.None) // _gameplayType == GameType.Match3 || _gameplayType == GameType.Match3Move)
      {
        _nextItemContainer.gameObject.SetActive(true);
        push = Instantiate(_nextItem, _itemsContainer);
        push.name = _nextItem.name;
      }
      else
      {
        _nextItemContainer.gameObject.SetActive(false);
        push = GameData.Prefabs.CreatePushItem(_itemsContainer);
      }
      push.IsArrow = _gameplayType == GameType.Match3Move;
      push.vturn = new Vector2Int(Mathf.RoundToInt(_arrowsSelected[q].vDir.x), Mathf.RoundToInt(_arrowsSelected[q].vDir.z));
      push.transform.localPosition = _arrowsSelected[q].transform.localPosition - new Vector3Int(_arrowsSelected[q].dir.x/2, 0, _arrowsSelected[q].dir.y/2);
      push.dir = _arrowsSelected[q].dir;
      _pushing.Add(push);
    }
    if(_arrowsSelected.Count > 0 && _nextItem)
    {
      Destroy(_nextItem.gameObject);
      _nextItem = null;
    }
    _arrowsSelected.Clear();
    _arrows.ForEach((ar) => ar.IsSelected = false);

  }

  void MoveItems()
  {
    bool checkItems = false;
    for(int p = 0; p < _pushing.Count; ++p)
    {
      if(!_pushing[p].IsReady)
        continue;
      Item toMove = null;
      checkItems |= _pushing[p].MoveP(Time.deltaTime * _speed);
      var vg = _pushing[p].gridNext;
      bool next_inside = _grid.isInside(vg);
      if(next_inside || _gameplayOutside)
      {
        if(_grid.geti(vg) != null)
        {
          if(_gameplayPushType == PushType.None)
          {
            _items.Add(_pushing[p]);
            _pushing[p].Stop();
            _pushing.RemoveAt(p);
            p--;
          }
          else //if(_gameplayPushType == PushType.PushOne || _gameplayPushType == PushType.PushLine)
          {
            toMove = _grid.geti(vg);
            toMove.dir = _pushing[p].dir;
            _pushing[p].Hide();
            _pushing.RemoveAt(p);
            p--;
          }
        }
        else
        {
          if(!_grid.isInside(_pushing[p].grid) && !next_inside)
          {
            _pushing[p].Hide();
            _pushing.RemoveAt(p);
            p--;
          }
        }
      }
      else
      {
        if(_gameplayPushType == PushType.None)
        {
          _items.Add(_pushing[p]);
          _pushing[p].Stop();
          _pushing.RemoveAt(p);
          p--;
        }
        else
        {
          _pushing[p].Hide();
          _pushing.RemoveAt(p);
          p--;
        }
      }
      List<Item> pushToMove = new List<Item>();
      if(toMove)
      {
        var v = toMove.grid;
        for(int q = 0; q < _dim.x; ++q)
        {
          if(_grid.isInside(v))
          {
            var item = _grid.geti(v);
            if(item != null)
              pushToMove.Add(item);
            else
              break;
          }
          v += toMove.dir;
        }
        bool inside = _grid.isInside(pushToMove.last().grid + toMove.dir);
        if(inside || _gameplayOutside)
        {
          var gridDest = pushToMove.last().grid + toMove.dir;
          if(_gameplayPushType == PushType.PushLine)
          {
            gridDest = _grid.getDest(pushToMove.last().grid, toMove.dir, !_gameplayOutside);
            if(_grid.isInside(gridDest))
            {
              var vdir = gridDest - pushToMove.last().grid;
              pushToMove.ForEach((item) => item.PushBy(vdir));// toMove.dir));
            }
            else
              pushToMove.ForEach((item) => item.PushTo(gridDest));// toMove.dir));
          }
          else
          {
            pushToMove.ForEach((item) => item.PushBy(toMove.dir));
          }
          _moving.AddRange(pushToMove);
        }
        else
        {
          pushToMove.ForEach((item) => item.Stop());
          pushToMove.Clear();
        }
      }
    }

    for(int q = 0; q < _moving.Count; ++q)
    {
      var dir = _moving[q].dir;
      if(!_moving[q].Move(Time.deltaTime * _speed, _grid.dim()))
      {
        checkItems |= true;
        _moving.RemoveAt(q);
        --q;
      }
    }

    if(checkItems)
    {
      _grid.update(_items);
      CheckMatch3();
      if(_items.Count == 0 && !Finished)
      {
        Finished = true;
        this.Invoke(()=>uiSummary.Show(this), 0.5f);
      }
    }
    if(_moving.Count == 0 && _pushing.Count == 0 && _matching.Count == 0)
    {
      _grid.update(_items);
      CheckMove();
      if(_nextItem == null)
      {
        if(_items.Count > 0)
        {
          _nextItem = CreateNextItem();
          _nextItem.IsArrow = false;
        }
      }
    }
  }
  void CheckMatch3()
  {
    Vector2Int v = Vector2Int.zero;
    Vector2Int vnx = new Vector2Int(1,0);
    Vector2Int vny = new Vector2Int(0,1);
    for(int y = 0; y < _grid.dim().y && _matching.Count == 0; ++y)
    {
      for(int x = 0; x < _grid.dim().x; ++x)
      {
        v.x = x; v.y = y;
        var item0 = _grid.geta(v);
        if(item0)
        {
          {
            var item1 = _grid.geta(v + vnx);
            var item2 = _grid.geta(v + vnx * 2);
            if(Item.EqType(item0, item1) && Item.EqType(item0, item2))
            {
              _matching.Add(new Match3(item0, item1, item2));
              break;
            }
          }
          {
            var item1 = _grid.geta(v + vny);
            var item2 = _grid.geta(v + vny * 2);
            if(Item.EqType(item0, item1) && Item.EqType(item0, item2))
            {
              _matching.Add(new Match3(item0, item1, item2));
              break;
            }
          }
        }
      }
    }
    if(_matching.Count > 0)
      DestroyMatch();
  }
  void CheckMove()
  {
    if(_gameplayType == GameType.Match3Move)
    {
      _grid.update(_items);
      for(int q = 0; q < _items.Count; ++q)
      {
        var dest = _grid.getDest(_items[q].grid, _items[q].vturn, !_gameplayOutside);
        if(dest != _items[q].grid)
        {
          _items[q].PushTo(dest);
          _moving.Add(_items[q]);
          break;
        }
      }
    }
  }
  void DestroyMatch()
  {
    if(_matching.Count > 0)
    {
      for(int q = 0; q < _matching[0]._matches.Count; ++q)
      {
        _matching[0]._matches[q].Hide();
        _items.Remove(_matching[0]._matches[q]);
      }
      _matching.RemoveAt(0);
      _grid.update(_items);
      CheckMatch3();
    }
  }

  void Update()
  {
    if(_started)
      MoveItems();
  }
}
