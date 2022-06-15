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
  public static System.Action<Level>   onCreate, onStart, onPlay, onFirstInteraction, onTutorialStart, onPointsAdded, onMovesLeftChanged;
  public static System.Action<Level>   onFinished, onDestroy, onItemThrow;
  public static System.Action<Match3>  onItemsMatched;
  public static System.Action<Item, Item> onItemsHit;
  public static System.Action<GameState.Powerups.Type> onPowerupUsed;  
  public static System.Action onCombo;

  [Header("Refs")]
  [SerializeField] Transform _arrowsContainer;
  [SerializeField] Transform _gridContainer;
  [SerializeField] Transform _itemsContainer;
  [SerializeField] Transform _nextItemContainer;
  [SerializeField] Transform _trayItemContainer;
  [SerializeField] Transform _poiLT;
  [SerializeField] Transform _poiRB;

  [Header("Animations")]
  [SerializeField] float _activationInterval = 1/30f;
  [SerializeField] float _deactivationInterval = 1/30f;
  [SerializeField] float _bombExplodeDelay = 1 / 10f;

  [Header("Gameplay Variants")]
  [SerializeField] bool _gameplayOutside = false;
  [Header("Settings")]
  [SerializeField] Vector2Int _dim;
  [SerializeField] float _speed = 8;
  [SerializeField] int   _maxPoints = 10;
  [SerializeField] bool  _multiArrowSelection = false;
  [Header("Items")]
  [SerializeField] List<Item> _listItems;

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
    public bool IsInsideDim(Vector2Int v)
    {
      return v.x >= -_dim.x / 2 && v.x <= _dim.x / 2 && v.y >= -_dim.y / 2 && v.y <= _dim.y / 2;
    }
    public bool isFieldInside(Vector2Int v)
    {
      bool inside = IsInsideDim(v);
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
          if(block && block.grid != vbeg && !block.IsDirectional)
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
    public void update(List<Item> _items, Level lvl)
    {
      clear();
      for(int q = 0; q < _items.Count; ++q)
      {
        if(isFieldInside(_items[q].grid))
          set(_items[q]);
        else
        {
          _items[q].Points = GameData.Points.ballOut(lvl._pushesInMove);
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
      if(IsInsideDim(vi))
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
          elem.SetSelected(sel, vdir);
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
    //public bool ByMoving;
    public int  Points;
    public List<Item> _matches;
    public Match3(Item i0, Item i1, Item i2, int points)
    {
      _matches = new List<Item>(){i0, i1, i2};
      for(int q = 0; q < _matches.Count; ++q)
        _matches[q].IsFrozen = true;
      Points = points;
    }
    public Match3(List<Item> list)
    {
      _matches = new List<Item>(list);
      for(int q = 0; q < _matches.Count; ++q)
        _matches[q].IsFrozen = true;
      Points = GameData.Points.matchStandard * _matches.Count;
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

  enum Check
  {
    Match3,
    AutoMoves,
    End,
  }

  public int  LevelIdx => GameState.Progress.Level;
  public bool Succeed {get; private set;}
  public bool Finished {get; private set;}
  public bool gameOutside => _gameplayOutside;
  public int  movesAvail => _listItems.Count + ((_nextItem)?1:0);
  public int  ColorItems => _items.Count((item) => item.IsRegular);
  public bool AnyColorItem => _items.Count((item) => item.IsRegular) > 0;
  public int  Points {get; set;} = 0;
  public int  PointsMax => _maxPoints;
  public int  Stars {get; set;}
  public int  BallsInitialCnt {get; set;}

  bool _started = false;
  bool _allowInput => _started && movesAvail > 0 && _pushing.Count == 0 && _moving.Count == 0 && _matching.Count == 0 && _painting.Count == 0 && !_sequence;
  UISummary uiSummary = null;

  Grid _grid = new Grid();
  List<Arrow> _arrows = new List<Arrow>();
  List<Arrow> _arrowsSelected = new List<Arrow>();
  List<Item> _items = new List<Item>();
  List<Item> _moving = new List<Item>();
  List<Item> _exploding = new List<Item>();
  List<Match3> _matching = new List<Match3>();
  List<Item> _destroying = new List<Item>();
  List<Item> _painting = new List<Item>();
  List<Check> _checks = new List<Check>();
  //List<ObjectFracture> _fractures = new List<ObjectFracture>();
  List<Item> _listNextItems = new List<Item>();

  Item _nextItem = null;
  bool _inputBlocked = false;
  bool _firstInteraction = false;
  bool _checkMoves = false;
  bool _sequence = false;
  int  _matchesInMove = 0;
  int  _pushesInMove = 0;

  [SerializeField] GameState.Powerups.Type _powerupSelected = GameState.Powerups.Type.None;

  void Awake()
  {
    _grid.init(_dim);
    _poiLT.position = new Vector3(-_grid.dim().x/2 - 2, 0, _grid.dim().y / 2 + 2);
    _poiRB.position = new Vector3(_grid.dim().x / 2 + 2, 0, -_grid.dim().y / 2 - 2);
    _items = _itemsContainer.GetComponentsInChildren<Item>().ToList();
    uiSummary = FindObjectOfType<UISummary>(true);
    
    _nextItemContainer.gameObject.SetActive(false);
    _nextItemContainer.transform.position = new Vector3(0, 0, _grid.dim().y / 2 + 3);

    UIIngame.onPowerupChanged += OnPowerupChanged;
    Item.onPushedOut += OnItemPushedOut;

    BallsInitialCnt = ColorItems;

    onCreate?.Invoke(this);
  }
  void OnDestroy()
  {
    onDestroy?.Invoke(this);
    UIIngame.onPowerupChanged -= OnPowerupChanged;
    Item.onPushedOut -= OnItemPushedOut;
  }
  IEnumerator Start()
  {
    yield return null;
    Init();

    yield return new WaitForSeconds(0.125f * 0.5f);
    onStart?.Invoke(this);

    for(int q = 0; q < Mathf.Max(_grid.dim().x, _grid.dim().y); q++)
    {
      yield return new WaitForSeconds(_activationInterval);
      for(int w = -q; w <= q; ++w)
      {
        _grid.getElem(new Vector2Int(-q, w))?.Show();
        _grid.getElem(new Vector2Int(q, w))?.Show();
        _grid.getElem(new Vector2Int(w, -q))?.Show();
        _grid.getElem(new Vector2Int(w, q))?.Show();
      }
    }

    yield return new WaitForSeconds(0.25f);
    StartCoroutine(coShowBalls());
    yield return StartCoroutine(coShowArrows(true));

    _started = true;
  }
  IEnumerator coShowBalls()
  {
    _items.Sort((Item i0, Item i1) => (int)(i0.grid.y * 100 + i0.grid.x) - (i1.grid.y * 100 + i1.grid.x));
    //yield return new WaitForSeconds(0.25f);
    for(int q = 0; q < _items.Count; ++q)
    {
      yield return new WaitForSeconds(0.0625f / 4);
      _items[q].Show();
    }
  }
  // void ShowBalls()
  // {
  //   StartCoroutine(coShowBalls());
  // }
  IEnumerator coShowArrows(bool act)
  {
    //yield return new WaitForSeconds(0.25f);
    foreach(var arr in _arrows)
    {
      yield return null;//new WaitForSeconds(_activationInterval);
      if(act)
        arr?.Show();
      else
        arr?.Hide();  
    }
  }

  IEnumerator coDestroyGrid()
  {
    Vector2Int va = Vector2Int.zero;
    for(int y = 0; y < _grid.dim().y; ++y)
    {
      va.y = y - _grid.dim().y/2;
      for(int x = 0; x < _grid.dim().x; ++x)
      {
        va.x = x - _grid.dim().x/2;
        //_grid.getElem(va).Fracture();
        _grid.getElem(va).Hide();
        yield return new WaitForSeconds(1/60f);
      }
    }
  }
  void DestroyGrid()
  {
    StartCoroutine(coDestroyGrid());
  }
  void DestroyArrows()
  {
    StartCoroutine(coShowArrows(false));
  }
  IEnumerator coOutroBalls()
  {
    foreach(var item in _items)
    {
      item.Deactivate();
      yield return new WaitForSeconds(1 / 15f);
    }
  }
  void OutroBalls()
  {
    StartCoroutine(coOutroBalls());
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
    _grid.update(_items, this);
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

    //_nextItemContainer.gameObject.SetActive(true);
    _nextItem = CreateNextItem();
    UpdateArrows();
    
    FindObjectOfType<UIIngame>()?.SetLevel(this);
  }
  List<Item> _pushing = new List<Item>();

  Arrow   arrowBeg = null, arrowEnd = null;
  List<Arrow> FindArrow(Vector2Int vi)
  {
    List<Arrow> arrows = new List<Arrow>();
    bool horz = Mathf.Abs(vi.y) == _grid.dim().y/2 + 1;
    int idxPrev = -1;
    int idxNext = -1;
    Vector2Int vn = vi;
    if(horz)
    {
      vn.x = vi.x - 1;
      idxPrev = _arrows.FindIndex((arr) => arr.grid == vn);
      vn.x = vi.x + 1;
      idxNext = _arrows.FindIndex((arr) => arr.grid == vn);
    }
    else
    {
      vn.y = vi.y - 1;
      idxPrev = _arrows.FindIndex((arr) => arr.grid == vn);
      vn.y = vi.y + 1;
      idxNext = _arrows.FindIndex((arr) => arr.grid == vn);
    }
    if(idxPrev >= 0)
      arrows.Add(_arrows[idxPrev]);
    if(idxNext >= 0)
      arrows.Add(_arrows[idxNext]);

    return arrows;
  }
  void SelectArrows()
  {
    _arrowsSelected.Clear();
    if(arrowBeg == null || arrowEnd == null)
      return;

    bool horz = Mathf.Abs(arrowBeg.vPos.z) == (_grid.dim().y/2 + 1);

    Vector3 dir = arrowBeg.vPos - arrowEnd.vPos;
    dir.x = Mathf.Abs(dir.x);
    dir.z = Mathf.Abs(dir.z);
    bool _sel = false;
    for(int q = 0; q < _arrows.Count; ++q)
    {
      var ar = _arrows[q];
      if(horz)
        _sel = _sel = Mathf.Sign(ar.grid.x - arrowBeg.grid.x) != Mathf.Sign(ar.grid.x - arrowEnd.grid.x) && ar.grid.y == arrowBeg.grid.y;
      else
        _sel = _sel = Mathf.Sign(ar.grid.y - arrowBeg.grid.y) != Mathf.Sign(ar.grid.y - arrowEnd.grid.y) && ar.grid.x == arrowBeg.grid.x;
      _arrows[q].IsSelected = _sel;
      if(_arrows[q].IsSelected)
        _arrowsSelected.Add(_arrows[q]);
    }
    arrowBeg.IsSelected = true;
    _arrowsSelected.Add(arrowBeg);
    arrowEnd.IsSelected = (horz)? arrowBeg.grid.y == arrowEnd.grid.y : arrowBeg.grid.x == arrowEnd.grid.x;
    if(arrowEnd.IsSelected)
      _arrowsSelected.Add(arrowEnd);

    if(_powerupSelected == GameState.Powerups.Type.Arrows)
    {
      var arrs = FindArrow(arrowBeg.grid);
      arrs.ForEach((ar) => ar.IsSelected = true);
      _arrowsSelected.AddRange(arrs);
    }

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
    if(_listItems.Count > 0 && _nextItem == null)
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
      //onMovesLeftChanged?.Invoke(this);
    }

    if(item)
    {
      item.transform.localPosition = Vector3.zero;
      item.name = item.name.Replace("(Clone)", "");
      //item.Show();
    }

    return item;
  }
  void AddPoints(int points)
  {
    Points += points;
    if(Points >= _maxPoints * GameData.Points.percentForStars(2))
      Stars = 3;
    else if(Points >= _maxPoints * GameData.Points.percentForStars(1))
      Stars = 2;
    else if(Points >= _maxPoints * GameData.Points.percentForStars(0))
      Stars = 1;

    if(points > 0)
      onPointsAdded?.Invoke(this);
  }
  void OnItemPushedOut(Item item)
  {
    AddPoints(item.Points);
    _pushesInMove++;
  }
  void ShowBigGreets()
  {
    if(_matchesInMove > 1 || _pushesInMove > 2)
    {
      onCombo?.Invoke();
    }
  }
  public void OnInputBeg(TouchInputData tid)
  {
    arrowBeg = arrowEnd = null;
    if(!_allowInput || !AnyColorItem) // || !_firstInteraction)
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
    if(!_allowInput || !AnyColorItem) // || !_firstInteraction)
      return;

    arrowEnd = tid.GetClosestCollider(0.5f)?.GetComponent<Arrow>() ?? null;
    if(arrowEnd)
    {
      if(_multiArrowSelection)
        SelectArrows();
      else
      {
        if(arrowBeg != arrowEnd)
          ClearArrows();
        // else
        // {
        //   if(_powerupSelected == GameState.Powerups.Type.Arrows)
        //     SelectArrow(arrowBeg);
        // }
      }
    }
    else
      ClearArrows();
  }
  public void OnInputEnd(TouchInputData tid)
  {
    arrowBeg = null;
    arrowEnd = null;
    if(!_firstInteraction)
      _firstInteraction = true;

    if(!_allowInput || !AnyColorItem)
      return;

    _matchesInMove = 0;
    _pushesInMove = 0;
    Item push = null;
    for(int q = 0; q < _arrowsSelected.Count; ++q)
    {
      if(_listNextItems.Count > 0)
      {
        push = Instantiate(_listNextItems[0], _itemsContainer);
        push.name = _listNextItems[0].name;
        _listNextItems.RemoveAt(0);
      }
      else
      {
        push = Instantiate(_nextItem, _itemsContainer);
        push.name = _nextItem.name;
      }
      push.vturn = new Vector2Int(Mathf.RoundToInt(_arrowsSelected[q].vDir.x), Mathf.RoundToInt(_arrowsSelected[q].vDir.z));
      push.transform.localPosition = _arrowsSelected[q].transform.localPosition - new Vector3Int(_arrowsSelected[q].dir.x/2, 0, _arrowsSelected[q].dir.y/2);
      push.dir = _arrowsSelected[q].dir;
      _pushing.Add(push);
      push.Show();
    }
    if(_arrowsSelected.Count > 0 && _nextItem)
    {
      if(_powerupSelected != GameState.Powerups.Type.None)
      {
        onPowerupUsed?.Invoke(_powerupSelected);
        _powerupSelected = GameState.Powerups.Type.None;
      }

      onItemThrow?.Invoke(this);
      Destroy(_nextItem.gameObject);
      _nextItem = null;
    }
    _arrowsSelected.Clear();
    _arrows.ForEach((ar) => ar.IsSelected = false);
    UpdateArrows();
  }
  public void OnPowerupChanged(GameState.Powerups.Type type, bool state)
  {
    if(_listNextItems.Count > 0)
      _listNextItems.RemoveAt(0);
    if(state)
    {
      if(type == GameState.Powerups.Type.Bomb)
        _listNextItems.Add(GameData.Prefabs.BombPrefab);
      else if(type == GameState.Powerups.Type.Color)
        _listNextItems.Add(GameData.Prefabs.ColorChangeItem);
      else if(type == GameState.Powerups.Type.Painter)
        _listNextItems.Add(GameData.Prefabs.PainterItem);
      
      _powerupSelected = type;
    }
    else
    {
      if(_powerupSelected == type)
        _powerupSelected = GameState.Powerups.Type.None;
    }
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
        if(_pushing[p].grid != _pushing[p].gridPrev)
          _grid.touchElems(_pushing[p].grid, _pushing[p].dir);
      }
      var vg = _pushing[p].gridNext;
      bool next_inside = _grid.IsInsideDim(vg); //_grid.isFieldInside(vg);
      var pushType = _pushing[p].push;
      var pusher = _pushing[p];
      if(next_inside || _gameplayOutside)
      {
        Item item = _grid.geti(vg);
        if(item && !item.IsDirectional)
        {
          if(_pushing[p].push == Item.Push.None) //_gameplayPushType == PushType.None)
          {
            if(_pushing[p].IsColorChanger)
              BallsInitialCnt++;
            onItemsHit?.Invoke(item, _pushing[p]);
            item.Hit(_pushing[p]);
            if(CheckPainting(_pushing[p], item))
              _pushing[p].Hide();
            else
              _items.Add(_pushing[p]);
            _pushing[p].Stop();
            _pushing.RemoveAt(p);
            p--;
          }
          else //if(_gameplayPushType == PushType.PushOne || _gameplayPushType == PushType.PushLine)
          {
            bool isField = _grid.isField(_pushing[p].grid);
            if(!item.IsStatic && !item.IsFrozen && isField)
            {
              toMove = _grid.geti(vg);
              toMove.dir = _pushing[p].dir;
            }
            if(isField)
              onItemsHit?.Invoke(item, _pushing[p]);
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
          else
          {
            var ci = _grid.geti(_pushing[p].grid);
            if(ci && ci.IsDirectional)
            {
              if(_pushing[p].Redirected == null)
              {  
                _pushing[p].Redirected = ci.vturn;
                _pushing[p].Stop();
                _pushing[p].dir = ci.vturn;
                _pushing[p].vturn = ci.vturn;
              }
            }
            else
            {
              _pushing[p].Redirected = null;
            }
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
          if(_grid.IsInsideDim(v)) //isFieldInside(v))
          {
            var item = _grid.geti(v);
            if(item != null && !item.IsStatic && !item.IsRemoveElem && !item.IsDirectional)
              pushToMove.Add(item);
            else
              break;
          }
          // else
          //   break;
          v += toMove.dir;
        }
        if(pushToMove.Count > 0)
        {
          bool inside = _grid.isFieldInside(pushToMove.last().grid + toMove.dir);
          if(inside || _gameplayOutside)
          {
            var gridDest = pushToMove.last().grid + toMove.dir;
            Item itemNext = _grid.geti(gridDest);
            if(pushType == Item.Push.Line)
            {
              gridDest = _grid.getDest(pushToMove.last().grid, toMove.dir, !_gameplayOutside);
              if(_grid.isFieldInside(gridDest))
              {
                var vdir = gridDest - pushToMove.last().grid;
                pushToMove.ForEach((item) => item.PushBy(vdir));
              }
              else
                pushToMove.ForEach((item) => item.PushTo(gridDest));
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
      var item = _grid.geti(_moving[q].grid);
      if(!_grid.isFieldInside(_moving[q].grid))
      {
        _moving[q].Hide();
        _moving.RemoveAt(q);
        --q;
        checkItems |= true;
      }
      else if(!moving)
      {
        if(item && item.IsDirectional && _moving[q].Redirected == null)
        {
          _pushing.Add(_moving[q]);
          _moving[q].Stop();
          _moving[q].dir = item.vturn;
        }
        else
        {
          var nextFieldItem = _grid.geti(_moving[q].gridNextLast);
          if(nextFieldItem && !nextFieldItem.IsMoving)
            onItemsHit?.Invoke(_moving[q], nextFieldItem); //intentionally rem
          _moving[q].Hit(nextFieldItem);
          if(nextFieldItem && nextFieldItem.IsBomb)
            _exploding.Add(nextFieldItem);
          if(_moving[q].IsBomb)
            _exploding.Add(_moving[q]);
          if(nextFieldItem != null)
          {
            if(CheckPainting(_moving[q], nextFieldItem))
            {
              if(_moving[q].IsPainter)
                _moving[q].Hide();
              if(nextFieldItem?.IsPainter ?? false)
                nextFieldItem.Hide();
            }
          }
        }
        checkItems |= true;
        _moving.RemoveAt(q);
        --q;
      }
      else //moving
      {
        if(_moving[q].grid != _moving[q].gridPrev)
        {
          checkItems |= true;
          _grid.touchElems(_moving[q].grid, _moving[q].dir);
        }
        if(item && item.IsDirectional && _moving[q].Redirected == null)
        {
          _pushing.Add(_moving[q]);
          _moving[q].Stop();
          _moving[q].dir = item.vturn;
        }
      }
    }

    if(checkItems)
    {
      Sequence();
    }
  }
  IEnumerator coSequenece()
  {
    _grid.update(_items, this);
    _sequence = true;

    if(_painting.Count > 0)
    {
      Item hit = _painting[0];
      _painting.RemoveAt(0);
      _painting.Sort((Item i0, Item i1) => (int)(i1.grid.y * 100 + i1.grid.x) - (i0.grid.y * 100 + i0.grid.x));
      for(int q = 0; q < _painting.Count; ++q)
      { 
        if(_painting[q].id != hit.id)
        {
          _painting[q].Paint(hit);
          yield return new WaitForSeconds(1/10.0f);
        }
      }
    }
    _painting.Clear();

    bool hasMatches;
    while(hasMatches = CheckMatch3())
    {
      yield return StartCoroutine(coDestroyMatch());
      yield return null;
    };
    yield return StartCoroutine(coExplodeBombs());
    CheckMove();

    if(_moving.Count == 0 && _pushing.Count == 0 && _matching.Count == 0 && _painting.Count == 0)
    {
      ShowBigGreets();
      CheckEnd();
      if(AnyColorItem)
        _nextItem = CreateNextItem();
    }    
    _sequence = false;
  }
  void Sequence()
  {
    if(!_sequence)
    {
      _sequence = true;
      StartCoroutine(coSequenece());
    }
  }
  bool CheckMatch3()
  {
    Vector2Int v = Vector2Int.zero;
    Vector2Int vnx = new Vector2Int(1,0);
    Vector2Int vny = new Vector2Int(0,1);
    List<Match3> localMatch = new List<Match3>();

    for(int y = 0; y < _grid.dim().y; ++y)
    {
      for(int x = 0; x < _grid.dim().x; ++x)
      {
        v.x = x; v.y = y;
        var item0 = _grid.geta(v);
        if(item0 && item0.IsRegular && !item0.IsMatching && !item0.IsMoving) // || item0.IsColorChanger))
        {
          List<Item> match_items = new List<Item>();
          {
            match_items.Add(item0);
            for(int q = 0; q < _grid.dim().x; ++q)
            {
              var item1 = _grid.geta(v + vnx * (q+1));
              if(Item.EqType(item0, item1) && !item1.IsMatching && !item1.IsMoving)
              {
                match_items.Add(item1);
                x++;
              }
              else
                break;      
            }
            if(match_items.Count >= 3)
              localMatch.Add(new Match3(match_items));
          }
        }
      }
    }
    for(int x = 0; x < _grid.dim().x; ++x)
    {
      for(int y = 0; y < _grid.dim().y; ++y)
      {
        v.x = x; v.y = y;
        var item0 = _grid.geta(v);
        if(item0 && item0.IsRegular && !item0.IsMatching && !item0.IsMoving) // || item0.IsColorChanger))
        {
          List<Item> match_items = new List<Item>();
          match_items.Add(item0);
          for(int q = 0; q < _grid.dim().y; ++q)
          {
            var item1 = _grid.geta(v + vny * (q + 1));
            if(Item.EqType(item0, item1) && !item1.IsMatching && !item1.IsMoving)
            {
              match_items.Add(item1);
              y++;
            }
            else
              break;
          }
          if(match_items.Count >= 3)
            localMatch.Add(new Match3(match_items));
        }
      }
    }

    _matching.AddRange(localMatch);
    _matchesInMove += localMatch.Count;

    return localMatch.Count > 0;
  }
  bool CheckPainting(Item itemMove, Item itemHit)
  {
    Item itemPainter = null;
    Item itemProp = null;
    if(itemMove.IsPainter)
    {
      itemPainter = itemMove;
      itemProp = itemHit;
    }
    else if(itemHit.IsPainter)
    {
      itemPainter = itemHit;
      itemProp = itemMove;
    }

    bool painting = itemPainter && itemProp && itemProp.IsRegular;
    if(painting)
    {
      _painting.Add(itemProp);
      for(int q = 0; q < _items.Count; ++q)
      {
        if(_items[q].IsRegular)
          _painting.Add(_items[q]);
      }
    }

    return painting;
  }
  IEnumerator coDestroyMatch()
  {
    List<Item> toDestroy = new List<Item>();
    for(int q = 0; q < _matching.Count; ++q)
    {
      toDestroy.AddRange(_matching[q]._matches);
      AddPoints(_matching[q].Points);
      onItemsMatched?.Invoke(_matching[q]);
    }
    toDestroy = toDestroy.Distinct().ToList();
    toDestroy.ForEach((item) => item.Matched());

    yield return new WaitForSeconds(0.25f);
    for(int q = 0; q < toDestroy.Count; ++q)
    {
      yield return new WaitForSeconds(_deactivationInterval);
      toDestroy[q].Hide();
    }
    _items.RemoveAll((item) => toDestroy.Contains(item));
    _matching.Clear();
    _grid.update(_items, this);
  }
  void DestroyMatch()
  {
    StartCoroutine(coDestroyMatch());
  }
  void ExplodeBombs()
  {
    StartCoroutine(coExplodeBombs());
  }
  IEnumerator coExplodeBombs()
  {
    List<Item> toRemove = new List<Item>();
    for(int q = 0; q < _exploding.Count; ++q)
    {
      yield return new WaitForSeconds(_bombExplodeDelay);
      var bomb = _exploding[q];
      bomb.Deactivate();
      bomb.Hide(true);
      bomb.ExplodeBomb();
      List<Item> items =_grid.getNB(bomb.grid);
      yield return new WaitForSeconds(0.1f);
      items.ForEach((item)=> item.PreExplode());
      yield return new WaitForSeconds(0.1f);
      for(int n = 0; n <items.Count; ++n)
      {
        yield return new WaitForSeconds(_bombExplodeDelay);
        if(!items[n].IsStatic && !items[n].IsRemoveElem)
        {
          items[n].Points = GameData.Points.bombExplode;
          AddPoints(items[n].Points);
          items[n].Explode();
          items[n].Hide();
          toRemove.Add(items[n]);
        }
      }
    }
    toRemove.AddRange(_exploding);
    _exploding.Clear();
    _items.RemoveAll((item) => toRemove.Contains(item));
    _grid.update(_items, this);
  }

  Vector2Int[] vdirections = new Vector2Int[4]
  {
    new Vector2Int(-1, 0),
    new Vector2Int(0, 1),
    new Vector2Int(1, 0),
    new Vector2Int(0, -1),
  };
  IEnumerator coCheckMove()
  {
    List<Item> itemsToPush = new List<Item>();
    _grid.update(_items, this);
    bool _pushed = false;
    for(int d = 0; d < vdirections.Length && !_pushed; ++d)
    {
      for(int q = 0; q < _items.Count; ++q)
      {
        if(_items[q].IsRegular && _items[q].IsMoveable && !_items[q].IsMoving)
        {
          if(_items[q].vturn == vdirections[d])
          {
            var dest = _grid.getDest(_items[q].grid, _items[q].vturn, !_gameplayOutside);
            if(dest != _items[q].grid)
            {
              itemsToPush.Add(_items[q]);
              _items[q].PushTo(dest);
              _moving.Add(_items[q]);
              _pushed = true;
            }
          }
        }
      }
    }
    yield return null;
  }
  void CheckMove()
  {
    StartCoroutine(coCheckMove());
  }
  IEnumerator coEnd()
  {
    Succeed = !AnyColorItem;
    if(_nextItem != null)
      _listItems.Add(_nextItem);
    _nextItem = null;  
    while(movesAvail > 0)
    {
      _listItems.RemoveAt(0);
      onMovesLeftChanged?.Invoke(this);
      AddPoints(GameData.Points.moveLeft);
      yield return new WaitForSeconds(0.125f/4);
    }
    OutroBalls();
    if(Succeed)
    {
      DestroyGrid();
      DestroyArrows();
    }
    yield return new WaitForSeconds(1.0f);
    Succeed = !AnyColorItem;
    onFinished?.Invoke(this);
    yield return new WaitForSeconds(0.5f);
    uiSummary.Show(this);
  }
  void CheckEnd()
  {
    if(!Finished)
    {
      if((!AnyColorItem && _moving.Count == 0 && _pushing.Count == 0) || movesAvail == 0)
      {
        Finished = true;
        StartCoroutine(coEnd());
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
