using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevOptions : MonoBehaviour
{
  public void on_max_dbg()
  {
  #if HAS_LION_GAME_ANALYTICS_SDK
    LionStudios.Suite.Debugging.LionDebugger.Show();
  #endif
  }

  // public void on_btn_finish_level()
  // {
  //   FindObjectOfType<Game>(true).FinishLevel();
  // }
  public void on_btn_prev()
  {
    FindObjectOfType<Game>(true).PrevLevel();
  }
  public void on_btn_next()
  {
    FindObjectOfType<Game>(true).NextLevel();
  }

  public void IncreaseQualityLevel() => QualitySettings.IncreaseLevel();
  public void DecreaseQualityLevel() => QualitySettings.DecreaseLevel();
}
