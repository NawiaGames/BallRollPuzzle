using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;
using GameLib.CameraSystem;
using GameLib.Splines;
using GameLib.InputSystem;
using GameLib.Utilities;
using TMPLbl = TMPro.TextMeshPro;

public class Level : MonoBehaviour
{
  public static System.Action<Vector3> onIntroFx;
  public static System.Action<Level>   onStart, onPlay, onFirstInteraction, onTutorialStart;
  public static System.Action<Level>   onFinished;
  public static System.Action<Match3>  onItemsMatched;

  [Header("Refs")]
  [SerializeField] Transform _arrowsContainer;
  [SerializeField] Transform _gridContainer;
  [SerializeField] Transform _itemsContainer;
  [SerializeField] Transform _nextItemContainer;
  [SerializeField] Transform _trayItemContainer;
  [SerializeField] Transform _poiLT;
  [SerializeField] Transform _poiRB;
  [SerializeField] TMPLbl    _ballInfo;

  public class Grid
  {
    Vector2Int _dim = Vector2Int.zero;
    Item[,]    _grid = null;
    GridElem[,] _elems = null;
    bool[,]    _fields = null;

    public void init(Vector2Int dims)
    {
      _grid = new Item[dims.y, dims.x];
      _elems = new GridElem[dims.y, dims.x];
      _fields = new bool[dims.y, dims.x];
      for(int y = 0;y < dims.y; ++y)
      {
        for(int x = 0; x < dims.x; ++x)
          _fields[y,x] = true;
      }
      _dim = dims;
    }

    public void clear()
    {
      System.Array.Clear(_grid, 0, _grid.Length);
    }
    public Vector2Int dim() => _dim;
    bool insideDim(Vector2Int v)
    {
      return v.x >= -_dim.x / 2 && v.x <= _dim.x / 2 && v.y >= -_dim.y / 2 && v.y <= _dim.y / 2;
    }
    public bool isFieldInside(Vector2Int v)
    {
      bool inside = insideDim(v);
      if(inside)
        inside &= isField(v);

      return inside;
    }

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
      Item item = null;
      if(grid.x >= 0 && grid.x < dim().x && grid.y >= 0 && grid.y < dim().y)
        item = _grid[grid.y, grid.x];
      
      return item;
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
    public List<Item> getNB(Vector2Int v)
    {
      List<Item> list = new List<Item>();
      Vector2Int vv = Vector2Int.zero;
      for(int y = -1; y <= 1; ++y)
      {
        vv.y = y;
        for(int x = -1; x <= 1; ++x)
        {
          vv.x = x;
          if(x != 0 || y != 0)
          {
            var item = geti(v + vv);
            if(item != null)
              list.Add(item);
          }
        }
      }
      return list;
    }
    public void update(List<Item> _items)
    {
      clear();
      for(int q = 0; q < _items.Count; ++q)
      {
        if(isFieldInside(_items[q].grid))
          set(_items[q]);
        else
        {
          _items[q].PushedOut();
          _items[q].Hide();
          _items.RemoveAt(q);
          q--;
        } 
      }
    }
    public void updateElems(List<Item> items)
    {
      for(int q = 0; q < items.Count; ++q)
      {
        if(items[q].IsRemoveElem)
        {
          var v = _dim / 2 + items[q].grid;
          _elems[v.y, v.x].gameObject.SetActive(false);
          _fields[v.y, v.x] = false;
        }
      }      
    }
    public void setElem(GridElem elem)
    {
      var v = _dim / 2 + elem.grid;
      _elems[v.y, v.x] = elem;
    }
    public GridElem getElem(Vector2Int vi)
    {
      if(insideDim(vi))
      {
        var v = _dim / 2 + vi;
        return _elems[v.y, v.x];      
      }
      else
        return null;  
    }
    public GridElem[,] elems => _elems;
    public bool isField(Vector2Int v, bool outside_ret = false)
    {
      if(!(v.x >= -_dim.x / 2 && v.x <= _dim.x / 2 && v.y >= -_dim.y / 2 && v.y <= _dim.y / 2))
        return outside_ret;
      var vv = v + _dim / 2;
      return _fields[vv.y, vv.x];
    }
    public bool isBlocked(Vector2Int grid)
    {
      return geti(grid) != null || !isField(grid);
    }
    public void selectElems(Vector2Int v, Vector2Int vdir, bool sel)
    {
      Vector2Int vbeg = v + vdir;
      for(int q = 0; q < Mathf.Max(dim().x, dim().y); ++q)
      {
        var elem = getElem(vbeg);
        if(elem)
          elem.selected = sel;
        vbeg += vdir;
      }
    }
    public void touchElems(Vector2Int v, Vector2Int vdir)
    {
      getElem(v)?.Touch();
      getElem(v + vdir)?.Touch(0.5f);
      getElem(v + new Vector2Int(-vdir.y, -vdir.x))?.Touch(0.5f);
      getElem(v + new Vector2Int(vdir.y, vdir.x))?.Touch(0.5f);
      //getElem(v + vdir + new Vector2Int(-vdir.y, -vdir.x))?.Touch(0.33f);
      //getElem(v + vdir + new Vector2Int(vdir.y, vdir.x))?.Touch(0.33f);
    }
  }
  public struct Match3
  {
    public bool _byMoving;
    public List<Item> _matches;
    public Match3(Item i0, Item i1, Item i2)
    {
      _matches = new List<Item>(){i0, i1, i2};
      for(int q = 0; q < _matches.Count; ++q)
        _matches[q].IsFrozen = true;      
      _byMoving = false;
    }
    public Match3(List<Item> list)
    {
      _matches = new List<Item>(list);
      for(int q = 0; q < _matches.Count; ++q)
        _matches[q].IsFrozen = true;
      _byMoving = false;
    }

    public Vector3 MidPos()
    {
      Vector3 v = Vector3.zero;
      for(int q = 0; q < _matches.Count; ++q)
        v += _matches[q].transform.position;
      if(_matches.Count > 0)
        v /= (float)_matches.Count;
      
      return v;  
    }
    public Color GetColor()
    {
      return (_matches.Count > 0)? _matches[0].color : Color.red;
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
  [SerializeField] bool       _gameplayOutside = false;
  [Header("Settings")]
  [SerializeField] Vector2Int _dim;
  [SerializeField] float      _speed = 8;
  [Header("Items")]
  [SerializeField] List<Item> _listItems;

  public int  LevelIdx => GameState.Progress.Level;
  public bool Succeed {get; private set;}
  public bool Finished {get; private set;}
  public bool gameOutside => _gameplayOutside;
  public int  movesAvail => _listItems.Count;
  public int  ColorItems => _items.Count((item) => item.IsRegular);
  public bool AnyColorItem => _items.Count((item) => item.IsRegular) > 0;

  bool _started = false;
  bool _allowInput => (movesAvail > 0 || _nextItem != null) && _pushing.Count == 0 && _moving.Count == 0 && _matching.Count == 0;
  UISummary uiSummary = null;

  Grid _grid = new Grid();
  List<Arrow> _arrows = new List<Arrow>();
  List<Arrow> _arrowsSelected = new List<Arrow>();
  List<Item> _items = new List<Item>();
  List<Item> _moving = new List<Item>();
  List<Item> _exploding = new List<Item>();
  List<Match3> _matching = new List<Match3>();

  Item _nextItem = null;
  Item _lastItemMove = null;
  bool _inputBlocked = false;

  void Awake()
  {
    _grid.init(_dim);
    _poiLT.position = new Vector3(-_grid.dim().x/2 - 2, 0, _grid.dim().y / 2 + 2);
    _poiRB.position = new Vector3(_grid.dim().x / 2 + 2, 0, -_grid.dim().y / 2 - 2);
    _items = _itemsContainer.GetComponentsInChildren<Item>().ToList();
    uiSummary = FindObjectOfType<UISummary>(true);
    
    _nextItemContainer.gameObject.SetActive(true);
    _nextItemContainer.transform.position = new Vector3(0, 0, _grid.dim().y / 2 + 3);
  }
  void OnDestroy()
  {

  }
  IEnumerator Start()
  {
    yield return null;
    Init();

    yield return new WaitForSeconds(0.125f * 0.5f);
    onStart?.Invoke(this);

    for(int q = 0; q < Mathf.Max(_grid.dim().x, _grid.dim().y); q++)
    {
      yield return new WaitForSeconds(1/60.0f);
      for(int w = -q; w <= q; ++w)
      {
        _grid.getElem(new Vector2Int(-q, w))?.Show();
        _grid.getElem(new Vector2Int(q, w))?.Show();
        _grid.getElem(new Vector2Int(w, -q))?.Show();
        _grid.getElem(new Vector2Int(w, q))?.Show();
      }
    }

    _items.Sort((Item i0, Item i1) => (int)(i0.grid.y * 100 + i0.grid.x) - (i1.grid.y * 100 + i1.grid.x));
    yield return new WaitForSeconds(0.25f);
    for(int q = 0; q < _items.Count; ++q)
    {
      yield return new WaitForSeconds(0.0625f / 4);
      _items[q].Show();
      _grid.getElem(_items[q].grid)?.Touch();
    }

    yield return new WaitForSeconds(0.25f);
    foreach(var arr in _arrows)
    {
      yield return new WaitForSeconds(1 / 60.0f);
      arr?.Show();
    }

    _started = true;
  }
  
  void Init()
  {
    float scale = 1;
    Vector2Int vgrid = Vector2Int.zero;
    List<Arrow> listT = new List<Arrow>(),listR = new List<Arrow>(), listB = new List<Arrow>(), listL = new List<Arrow>();
    for(int x = 0; x < _dim.x; ++x)
    {
      vgrid.x = -_dim.x / 2 + x;
      vgrid.y = _dim.y / 2 + 1;
      var arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(0, -1);
      //_arrows.Add(arrow);
      listT.Add(arrow);
      float xx = (-_dim.x + 1) * 0.5f + x;
      arrow.transform.localPosition = new Vector3(xx, 0, vgrid.y);
      arrow.transform.Rotate(new Vector3(0, 180, 0));
      arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      vgrid.y = -_dim.y / 2 - 1;
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(0, 1);
      //_arrows.Add(arrow);
      listB.Add(arrow);
      arrow.transform.localPosition = new Vector3(xx, 0, vgrid.y);
    }
    for(int y = 0; y < _dim.y; ++y)
    {
      vgrid.y = -_dim.y/2 + y ;
      vgrid.x = -_dim.x/2 - 1;
      var arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(1, 0);      
      //_arrows.Add(arrow);
      listL.Add(arrow);
      float yy = (-_dim.y + 1) * 0.5f + y;
      arrow.transform.localPosition = new Vector3(vgrid.x, 0, yy);
      arrow.transform.Rotate(new Vector3(0, 90, 0));
      arrow = GameData.Prefabs.CreateArrow(_arrowsContainer);
      vgrid.x = _dim.x / 2 + 1;
      arrow.grid = vgrid;
      arrow.dir = new Vector2Int(-1, 0);
      //_arrows.Add(arrow);
      listR.Add(arrow);
      arrow.transform.localPosition = new Vector3(vgrid.x, 0, yy);
      arrow.transform.Rotate(new Vector3(0, -90, 0));
    }
    _arrows.AddRange(listT);
    listR.Reverse();
    _arrows.AddRange(listR);
    listB.Reverse();
    _arrows.AddRange(listB);
    _arrows.AddRange(listL);

    int t = 0;
    for(int y = 0; y < _dim.y; ++y)
    {
      float yy = Mathf.RoundToInt((-_dim.y + 1) * 0.5f + y);
      for(int x = 0; x < _dim.x; ++x)
      {
        float xx = Mathf.RoundToInt((-_dim.x + 1) * 0.5f + x);
        var ge = GameData.Prefabs.CreateGridElem(_gridContainer);
        ge.even = ((x ^ y) & 1) == 0;
        ge.transform.localPosition = new Vector3(xx * scale, 0.05f, yy * scale);
        ge.grid = new Vector2Int((int)xx, (int)yy);
        //ge.Touch(t);
        _grid.setElem(ge);
        t--;
      }
    }
    _grid.update(_items);
    _grid.updateElems(_items);
    _items.RemoveAll((item) => 
    { 
      if(item.IsRemoveElem)
      {
        _grid.set(item.grid, null);
        return true;
      }
      else
        return false;
    });

    _nextItemContainer.gameObject.SetActive(true);
    _nextItem = CreateNextItem();
    UpdateArrows();
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

    for(int q = 0; q < _arrowsSelected.Count; ++q)
    {
      _arrowsSelected[q].IsBlocked = _grid.isBlocked(_arrowsSelected[q].grid + _arrowsSelected[q].dir); //_grid.geti(_arrowsSelected[q].grid + _arrowsSelected[q].dir) != null;
      if(_arrowsSelected[q].IsBlocked)
      {
        _arrowsSelected.RemoveAt(q);
        --q;
      }
      else
        _arrowsSelected[q].IsSelected = _arrowsSelected[q].IsSelected;
    }
    UpdateArrows();
  }
  void ClearArrows()
  {
    _arrowsSelected.Clear();
    arrowEnd = null;
    for(int q = 0; q < _arrows.Count; ++q)
      _arrows[q].IsSelected = false;

    UpdateArrows();  
  }
  void BlockArrows(bool block)
  {
    for(int q = 0; q < _arrows.Count; ++q)
    {
      _arrows[q].IsBlocked = block;
    }
    UpdateArrows();
  }
  void UpdateArrows()
  {
    for(int q = 0; q < _arrows.Count; ++q)
    {
      _grid.selectElems(_arrows[q].grid, _arrows[q].dir, false);
      if(_grid.isBlocked(_arrows[q].grid + _arrows[q].dir)) // _grid.geti(_arrows[q].grid + _arrows[q].dir) != null)
        _arrows[q].IsBlocked = true;
    }
    for(int q = 0; q < _arrowsSelected.Count; ++q)
    {
      _grid.selectElems(_arrowsSelected[q].grid, _arrowsSelected[q].dir, true);
    }
  }

  Item CreateNextItem()
  {
    Item item = null;
    if(movesAvail > 0)
    {
      var next_item = _listItems[0];
      if(next_item != null)
      {
        if(next_item.IsRandItem)
          item = GameData.Prefabs.CreateRandItem(_trayItemContainer, false);
        else if(next_item.IsRandMoveItem)
          item = GameData.Prefabs.CreateRandItem(_trayItemContainer, true);
        else if(next_item.IsRandPush)
          item = GameData.Prefabs.CreatePushItem(_trayItemContainer, (Random.value < 0.5f)? Item.Push.One : Item.Push.Line);
        else  
          item = Instantiate(_listItems[0], _trayItemContainer);
        //item.name = _listItems[0].name;
      }
      else
      {
        List<string> names = _items.Select((Item it) => it.name).Distinct().ToList();
        if(names.Count > 0)
          item = GameData.Prefabs.CreateRandItem(names, _trayItemContainer);
        else
          item = GameData.Prefabs.CreateRandItem(_trayItemContainer, false);
      }
      _listItems.RemoveAt(0);
      _ballInfo.text = "" + _listItems.Count + "x moves";
    }

    if(item)
    {
      item.transform.localPosition = Vector3.zero;
      item.name = item.name.Replace("(Clone)", "");
      item.Show();
    }

    return item;
  }

  public void OnInputBeg(TouchInputData tid)
  {
    arrowBeg = arrowEnd = null;
    if(!_allowInput || !AnyColorItem)
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
    if(!_allowInput || !AnyColorItem)
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

    if(!_allowInput || !AnyColorItem)
      return;

    for(int q = 0; q < _arrowsSelected.Count; ++q)
    {
      Item push = null;
      push = Instantiate(_nextItem, _itemsContainer);
      push.name = _nextItem.name;
      push.vturn = new Vector2Int(Mathf.RoundToInt(_arrowsSelected[q].vDir.x), Mathf.RoundToInt(_arrowsSelected[q].vDir.z));
      push.transform.localPosition = _arrowsSelected[q].transform.localPosition - new Vector3Int(_arrowsSelected[q].dir.x/2, 0, _arrowsSelected[q].dir.y/2);
      push.dir = _arrowsSelected[q].dir;
      _pushing.Add(push);
      push.Show();
    }
    if(_arrowsSelected.Count > 0 && _nextItem)
    {
      _nextItem.Deactivate();
      this.Invoke(()=>
      {
        Destroy(_nextItem.gameObject);
        _nextItem = null;
      },0.25f);
    }
    _arrowsSelected.Clear();
    _arrows.ForEach((ar) => ar.IsSelected = false);
    UpdateArrows();
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
      if(checkItems)
      {
        //_grid.getElem(_pushing[p].grid)?.Touch();
        if(_pushing[p].grid != _pushing[p].gridPrev)
          _grid.touchElems(_pushing[p].grid, _pushing[p].dir);
      }
      var vg = _pushing[p].gridNext;
      bool next_inside = _grid.isFieldInside(vg);
      var pushType = _pushing[p].push;
      var pusher = _pushing[p];
      if(next_inside || _gameplayOutside)
      {
        Item item = _grid.geti(vg);
        if(item)
        {
          if(_pushing[p].push == Item.Push.None) //_gameplayPushType == PushType.None)
          {
            item.Hit(_pushing[p]);
            _items.Add(_pushing[p]);
            _pushing[p].Stop();
            _pushing.RemoveAt(p);
            p--;
          }
          else //if(_gameplayPushType == PushType.PushOne || _gameplayPushType == PushType.PushLine)
          {
            if(!item.IsStatic && !item.IsFrozen)
            {
              toMove = _grid.geti(vg);
              toMove.dir = _pushing[p].dir;
            }
            _pushing[p].Hide();
            _pushing.RemoveAt(p);
            p--;
          }
          if(pusher.IsBomb)
            _exploding.Add(pusher);
          if(item.IsBomb)
            _exploding.Add(item);  
        }
        else
        {
          if((!_grid.isFieldInside(_pushing[p].grid) && !next_inside) || !_grid.isField(_pushing[p].grid, true))
          {
            _pushing[p].Hide();
            _pushing.RemoveAt(p);
            p--;
          }
        }
      }
      else
      {
        if(pushType == Item.Push.None) //_gameplayPushType == PushType.None)
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
        int cnt = Mathf.Max(_dim.x, _dim.y);
        for(int q = 0; q < cnt; ++q)
        {
          if(_grid.isFieldInside(v))
          {
            var item = _grid.geti(v);
            if(item != null && !item.IsStatic) // && !item.IsRemoveElem)
              pushToMove.Add(item);
            else
              break;
          }
          v += toMove.dir;
        }
        if(pushToMove.Count > 0)
        {
          bool inside = _grid.isFieldInside(pushToMove.last().grid + toMove.dir);
          if(inside || _gameplayOutside)
          {
            var gridDest = pushToMove.last().grid + toMove.dir;
            Item itemNext = _grid.geti(gridDest);
            if(pushType == Item.Push.Line)//_gameplayPushType == PushType.PushLine)
            {
              gridDest = _grid.getDest(pushToMove.last().grid, toMove.dir, !_gameplayOutside);
              if(_grid.isFieldInside(gridDest))
              {
                var vdir = gridDest - pushToMove.last().grid;
                pushToMove.ForEach((item) => item.PushBy(vdir));// toMove.dir));
              }
              else
                pushToMove.ForEach((item) => item.PushTo(gridDest));// toMove.dir));
            }
            else
            {
              if(!(itemNext && (itemNext.IsStatic || itemNext.IsRemoveElem)))
                pushToMove.ForEach((item) => item.PushBy(toMove.dir));
              else
                pushToMove.Clear();  
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
    }

    for(int q = 0; q < _moving.Count; ++q)
    {
      var dir = _moving[q].dir;
      bool moving = _moving[q].Move(Time.deltaTime * _speed, _grid.dim());
      if(!_grid.isFieldInside(_moving[q].grid))
      {
        _moving[q].Hide();
        _moving.RemoveAt(q);
        --q;
        checkItems |= true;
      }
      else if(!moving)
      {
        var nextFieldItem = _grid.geti(_moving[q].gridNext);
        _moving[q].Hit(nextFieldItem);
        if(nextFieldItem && nextFieldItem.IsBomb)
          _exploding.Add(nextFieldItem);
        if(_moving[q].IsBomb)
          _exploding.Add(_moving[q]);
        checkItems |= true;
        _moving.RemoveAt(q);
        --q;
      }
      else //moving
      {
        if(_moving[q].grid != _moving[q].gridPrev)
          _grid.touchElems(_moving[q].grid, _moving[q].dir);
      }
    }

    if(checkItems)
    {
      _grid.update(_items);
      ExplodeBombs();
      CheckMatch3();
      CheckEnd();
    }
    if(_moving.Count == 0 && _pushing.Count == 0 && _matching.Count == 0 && checkItems)
    {
      _grid.update(_items);
      this.Invoke(()=>CheckMove(),0.2f);
      //CheckBombs();
      if(_nextItem == null)
      {
        if(AnyColorItem)
        {
          _nextItem = CreateNextItem();
        }
        else
        {
          CheckEnd();
        }
      }
    }
  }
  void CheckMatch3()
  {
    Vector2Int v = Vector2Int.zero;
    Vector2Int vnx = new Vector2Int(1,0);
    Vector2Int vny = new Vector2Int(0,1);

    for(int y = 0; y < _grid.dim().y /*&& _matching.Count == 0*/; ++y)
    {
      for(int x = 0; x < _grid.dim().x; ++x)
      {
        v.x = x; v.y = y;
        var item0 = _grid.geta(v);
        if(item0 && item0.IsRegular) // || item0.IsColorChanger))
        {
          {
            List<Item> match_items = new List<Item>();
            match_items.Add(item0);
            for(int q = 0; q < _grid.dim().x; ++q)
            {
              var item1 = _grid.geta(v + vnx * (q+1));
              if(Item.EqType(item0, item1))
                match_items.Add(item1);
              else
                break;      
            }
            if(match_items.Count >= 3)
              _matching.Add(new Match3(match_items));
          }
          //if(_matching.Count == 0)
          {
            List<Item> match_items = new List<Item>();
            match_items.Add(item0);
            for(int q = 0; q < _grid.dim().y; ++q)
            {
              var item1 = _grid.geta(v + vny * (q + 1));
              if(Item.EqType(item0, item1))
                match_items.Add(item1);
              else
                break;
            }
            if(match_items.Count >= 3)
              _matching.Add(new Match3(match_items));
          }
        }
      }
    }
    if(_matching.Count > 0)
      DestroyMatch();
  }
  void ExplodeBombs()
  {
    List<Item> toRemove = new List<Item>();
    for(int q = 0; q < _exploding.Count; ++q)
    {
      var bomb = _exploding[q];
      bomb.Hide();
      bomb.Explode();
      List<Item> items =_grid.getNB(bomb.grid);
      for(int n = 0; n <items.Count; ++n)
      {
        if(!items[n].IsStatic && !items[n].IsRemoveElem)
        {
          items[n].Hide();
          toRemove.Add(items[n]);
        }
      }
    }
    toRemove.AddRange(_exploding);
    _exploding.Clear();
    _items.RemoveAll((item) => toRemove.Contains(item));
    _grid.update(_items);
  }    
  void CheckMove()
  {
    _grid.update(_items);
    for(int q = 0; q < _items.Count; ++q)
    {
      if(_items[q].IsRegular && _items[q].IsMoveable)
      {
        var dest = _grid.getDest(_items[q].grid, _items[q].vturn, !_gameplayOutside);
        if(dest != _items[q].grid)
        {
          _items[q].PushTo(dest);
          _moving.Add(_items[q]);
          _lastItemMove = _items[q];
          break;
        }
      }
    }
  }
  IEnumerator coDestroyMatch()
  {
    List<Item> toDestroy = new List<Item>();

    yield return new WaitForSeconds(0.05f);
    for(int q = 0; q < _matching.Count; ++q)
    {
      toDestroy.AddRange(_matching[q]._matches);
      onItemsMatched?.Invoke(_matching[q]);
    }
    toDestroy = toDestroy.Distinct().ToList();
    for(int q = 0; q < toDestroy.Count; ++q)
    {
      toDestroy[q].Hide();
      yield return new WaitForSeconds(0.05f);
      //toDestroy.ForEach((item) => item.Hide());
    }
    _items.RemoveAll((item) => toDestroy.Contains(item));
    _matching.Clear();
    _grid.update(_items);
    _nextItem = CreateNextItem();
  }
  void DestroyMatch()
  {
    StartCoroutine(coDestroyMatch());
    // if(_matching.Count > 0)
    // {
    //   if(_matching[0]._matches.Count > 0)
    //   {

    //   }
    // }
    // List<Item> toDestroy = new List<Item>();

    // for(int q = 0; q < _matching.Count; ++q)
    // {
    //   onItemsMatched?.Invoke(_matching[q]);
    //   toDestroy.AddRange(_matching[q]._matches);
    // }

    // toDestroy = toDestroy.Distinct().ToList();
    // toDestroy.ForEach((item) => item.Hide());
    // _items.RemoveAll((item) => toDestroy.Contains(item));
    // _matching.Clear();
    // _grid.update(_items);    
  }
  void CheckEnd()
  {
    if(!Finished)
    {
      if(!AnyColorItem || movesAvail == 0)
      {
        Finished = true;
        Succeed = !AnyColorItem;
        this.Invoke(() => onFinished?.Invoke(this), 0.5f);
        this.Invoke(() => 
        {
          Succeed = !AnyColorItem; 
          uiSummary.Show(this);
        }, 0.85f);
      }
    }
  }

  void Update()
  {
    if(_started)
      MoveItems();

    if((!_allowInput || !AnyColorItem) ^ _inputBlocked)
    {
      _inputBlocked = !_inputBlocked;
      BlockArrows(_inputBlocked);
    }
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    Vector3 vLB = new Vector3(-_dim.x * 0.5f, 0, -_dim.y * 0.5f);
    Vector3 vRT = new Vector3( _dim.x * 0.5f, 0, _dim.y * 0.5f);
    var v1 = Vector3.zero;
    v1.x = vLB.x;
    v1.z = vRT.z;
    Gizmos.DrawLine(vLB, v1);
    v1.x = vRT.x;
    v1.z = vLB.z;
    Gizmos.DrawLine(vLB, v1);
    v1.x = vRT.x;
    v1.z = vLB.z;
    Gizmos.DrawLine(vRT, v1);
    v1.x = vLB.x;
    v1.z = vRT.z;
    Gizmos.DrawLine(vRT, v1);
  }
}
