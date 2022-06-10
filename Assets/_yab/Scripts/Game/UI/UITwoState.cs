using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITwoState : MonoBehaviour
{
  [SerializeField] GameObject offState;
  [SerializeField] GameObject onState;
  
  public void SetState(bool state)
  {
    onState.SetActive(state);
    offState.SetActive(!state);
  }
}
