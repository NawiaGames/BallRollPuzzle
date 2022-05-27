using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevOptionState : MonoBehaviour
{
  public static System.Action onActivated, onDeactivated;
  
  void Awake()
  {
    onActivated?.Invoke();
  }
  void OnEnable()
  {
    onActivated?.Invoke();
  }
  void OnDisable()
  {
    onDeactivated?.Invoke();
  }
}
