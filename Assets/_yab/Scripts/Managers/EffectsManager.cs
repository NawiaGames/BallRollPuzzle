using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.Utilities;
using GameLib.UI;

public class EffectsManager : MonoBehaviour
{
  [Header("CamFX")]
    [SerializeField] ObjectShakePreset objShakePreset;
    [SerializeField] ObjectShakePreset objShakePresetLo;
    [SerializeField] ObjectShakePreset objShakePresetHi;
    [SerializeField] float offsetToCamera = .25f;
  [Header("FX Systems")]
    [SerializeField] ParticleSystem fxSparks = null;
    [SerializeField] ParticleSystem fxItemCompleted = null;
    [SerializeField] ParticleSystem fxConfettiIngame = null;
    [SerializeField] ParticleSystem fxConfettiLevel = null;
    [SerializeField] ParticleSystem fxPaintSplat = null;

    ParticleSystem fxConfetti;

    ObjectShake cameraShakeContainer;
    UIInfoLabelManager infoLblMan;

    private void Awake() 
    {
      cameraShakeContainer = Camera.main.GetComponentInParent<ObjectShake>();
      //fxConfetti = GameObject.FindGameObjectWithTag("ConfettiFX").GetComponent<ParticleSystem>();
      infoLblMan = FindObjectOfType<UIInfoLabelManager>(true);
    }
    private void OnEnable() 
    {
      Item.onHide += OnItemDestroy;
      Item.onBombExplode += OnItemBombExplo;
      Level.onFinished += OnLevelFinished;
    }
    private void OnDisable()
    {
      Item.onHide -= OnItemDestroy;
      Item.onBombExplode -= OnItemBombExplo;
      Level.onFinished -= OnLevelFinished;
    }

    Vector3 GetFxPosition(Vector3 objectPostion) => objectPostion + (objectPostion - Camera.main.transform.position).normalized * -offsetToCamera;
    void PlayFXAtPosition(ParticleSystem ps, Vector3 worldPosition, int emitCount = 0)
    {
      ps.transform.position = GetFxPosition(worldPosition);
      if (emitCount > 0)
        ps.Emit(emitCount);
      else
        ps.Play();
    }

    void OnItemBombExplo(Item sender)
    {
      var psmain = fxPaintSplat.main;
      psmain.startColor = sender.color;
      PlayFXAtPosition(fxPaintSplat, sender.transform.position, 5);      
    }
    void OnItemDestroy(Item sender)
    {
      var psmain = fxPaintSplat.main;
      psmain.startColor = sender.color;

      PlayFXAtPosition(fxPaintSplat, sender.transform.position, 5);
    }
    void OnFx00(object sender)
    {
      //PlayFXAtPosition(fxConfettiIngame, sender.transform.position);
      //PlayFXAtPosition(fxSparks, sender.transform.position);
      //PlayFXAtPosition(fxItemCompleted, sender.transform.position);
      //infoLblMan.ShowTextPopup(sender.transform.position, sender.GetSatisfactionString());
    }
    void OnFx01(object sender)
    {
      //infoLblMan.ShowTextPopup(sender.transform.position, customerWrongItemType, Color.red);
    }
    void OnFx02(object sender)
    {
      //infoLblMan.ShowTextPopup(item.vPos, GameData.Satisfy.GetSatisfyString(3), Color.green);
      //SparksFX(item.vPos);
    }

    void OnLevelFinished(Level lvl) 
    {
      if(lvl.Succeed)
      {
      //fxConfetti.Play();
      }
    fxConfettiLevel.Play();
      cameraShakeContainer.Shake(objShakePresetHi);
    }
}
