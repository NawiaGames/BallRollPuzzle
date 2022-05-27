using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTimeOffset : MonoBehaviour
{
  [SerializeField] int days;
  [SerializeField] int hours;
  [SerializeField] int mins;
  [SerializeField] int secs;
  [SerializeField] string str_time;

  static DebugTimeOffset static_this = null;
  void Awake()
  {
    static_this = this;
  }
  public System.TimeSpan get_time_offset()
  {
    return new System.TimeSpan(static_this.days, static_this.hours, static_this.mins, static_this.secs);
  }
  public static System.TimeSpan GetTimeOffset()
  {
    if(static_this)
      return static_this.get_time_offset();
    else
      return new System.TimeSpan();  
  }

  void Update()
  {
    if(Time.frameCount % 60 == 0)
      str_time = CTime.get().ToString();
  }
}

static public class CTime
{
  public static System.DateTime get()
  {
    var dt = System.DateTime.Now;
  #if UNITY_EDITOR
    dt += DebugTimeOffset.GetTimeOffset();
  #endif
    return dt;
  }
}

