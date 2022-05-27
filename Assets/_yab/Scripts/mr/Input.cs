using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace mr
{
  public static class Input
  {
    public static System.Action<Data> onInput;

    public enum State
    {
      None,
      Down,
      Hold,
      Up,
    }
    public struct Data
    {
      public State         state;
      public Vector2       vpos_down;
      public Vector2       vpos;
      public List<Vector2> vvpos_prev;
      public float         hold_time;

      public Data(State state, Vector2 vpos, Vector2 vpos_down, float hold_time, List<Vector2> vvpos_prev)
      {
        this.state = state;
        this.vpos = vpos;
        if(vvpos_prev != null)
        this.vvpos_prev = new List<Vector2>(vvpos_prev);
        else
          this.vvpos_prev = new List<Vector2>();  
        this.vpos_down = vpos_down;
        this.hold_time = hold_time;
      }
      public bool is_down()
      {
        return state == State.Down;
      }
      public bool is_hold()
      {
        return state == State.Hold;
      }
      public bool is_up()
      {
        return state == State.Up;
      }
    }
    static Data          data;
    static float         input_time = 0;
    static Vector2       vpos_dn = Vector3.zero;
    static List<Vector2> vvpos_prev = new List<Vector2>();

    private static Data process_input_dn(Vector2 vpos)
    {
      input_time = 0;
      vvpos_prev.Clear();
      vpos_dn = vpos;
      data = new Data(State.Down, vpos, vpos_dn, input_time, new List<Vector2>(vvpos_prev));
      onInput?.Invoke(data);
      
      return data;
    }
    private static Data process_input_hold(Vector2 vpos)
    {
      input_time += Time.deltaTime;
      vvpos_prev.Add(vpos);
      
      data = new Data(State.Hold, vpos, vpos_dn, input_time, new List<Vector2>(vvpos_prev));
      onInput?.Invoke(data);
      if(vvpos_prev.Count > 256)
        vvpos_prev.RemoveAt(0);

      return data;  
    }
    private static Data process_input_up(Vector2 vpos)
    {
      input_time += Time.deltaTime;
      data = new Data(State.Up, vpos, vpos_dn, input_time, new List<Vector2>(vvpos_prev));
      onInput?.Invoke(data);
      
      input_time = 0;
      vvpos_prev.Clear();
      vpos_dn = Vector2.zero;

      return data;
    }

    public static  Data process_input()
    {
      Vector2 vpos = new Vector2(0, 0);
    #if UNITY_EDITOR
      if(UnityEngine.Input.GetMouseButtonDown(0))
      {
        input_time = 0;
        vpos = UnityEngine.Input.mousePosition;
        return process_input_dn(vpos);
      }
      else if(UnityEngine.Input.GetMouseButton(0))
      {
        vpos = UnityEngine.Input.mousePosition;
        input_time += Time.deltaTime;

        return process_input_hold(vpos);
      }
      else if(UnityEngine.Input.GetMouseButtonUp(0))
      {
        vpos = UnityEngine.Input.mousePosition;
        return process_input_up(vpos);
      }
      else
        return new Data(State.None, Vector2.zero, Vector2.zero, 0, null);
    #else
      if(UnityEngine.Input.touches.Length > 0)
      {
        var touch = UnityEngine.Input.GetTouch(0);
        if(touch.phase == TouchPhase.Began)
        {
          vpos = touch.position;
          return process_input_dn(vpos);
        }
        else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
          vpos = touch.position;
          return process_input_hold(vpos);
        }
        else if(touch.phase == TouchPhase.Ended)
        {
          vpos = touch.position;
          return process_input_up(vpos);
        }
        else
          return new Data(State.None, Vector2.zero, Vector2.zero, 0, null);        
      }
      else
        return new Data(State.None, Vector2.zero, Vector2.zero, 0, null);
#endif
    }
    public static Data get_data()
    {
      return data;
    }

    public static class UI
    {
      static public List<GameObject> get_hits_list(Vector2 vscr_pos)
      {
        List<GameObject> list_hits = null;
        PointerEventData ped = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        ped.position = vscr_pos;
        var vresults = new List<RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(ped, vresults);
        if(vresults.Count > 0)
        {
          list_hits = new List<GameObject>();
          foreach(var res in vresults)
            list_hits.Add(res.gameObject);
        }
        return list_hits;
      }
      static public bool is_over_ui(Vector2 vscr_pos)
      {
        PointerEventData ped = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        ped.position = vscr_pos;
        var vresults = new List<RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(ped, vresults);
        return (vresults.Count > 0);
      }
      static public bool is_over_ui_obj(Vector2 vscr_pos, GameObject obj)
      {
        PointerEventData ped = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        ped.position = vscr_pos;
        var vresults = new List<RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(ped, vresults);
        bool is_over = false;
        foreach(var r in vresults)
        {
          if(r.gameObject == obj)
          {
            is_over = true;
            break;
          }
        }
        return is_over;
      }
    };    
  }
}