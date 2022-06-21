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
  public void on_btn_reset_progress()
  {
    GameLib.DataSystem.DataManager.ResetProgress();
  }  
  public void on_btn_cycle_theme()
  {
    FindObjectOfType<GameLib.UI.UIColorThemeManager>().ApplyThemeColorToAllSubComponents(colorThemes[(int)Mathf.Repeat(++colorTheme, colorThemes.Length)]);
  }
  public void on_btn_wasser()
  {
    var gameField = GameObject.Find("gameField");
    if(gameField)
    {
      var wasser = gameField.transform.GetChild(0)?.gameObject ?? null;
      if(wasser)
        wasser.SetActive(!wasser.activeSelf);
    }
  }
  [SerializeField] GameLib.UI.UIColorTheme[] colorThemes = new GameLib.UI.UIColorTheme[]{};
  [SerializeField] int colorTheme = 0;

  public void IncreaseQualityLevel() => QualitySettings.IncreaseLevel();
  public void DecreaseQualityLevel() => QualitySettings.DecreaseLevel();
}
