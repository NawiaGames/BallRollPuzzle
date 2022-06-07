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
    [SerializeField] ParticleSystem fxHit = null;

    ParticleSystem fxConfetti;

    ObjectShake cameraShakeContainer;
    UIInfoLabelManager infoLblMan;
    Level _lvl = null;

    List<GameLib.ObjectFracture> listFractures = new List<GameLib.ObjectFracture>();

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
      Item.onPushedOut += OnItemPushedOut;
      Level.onStart += OnLevelStart;
      Level.onItemsMatched += OnItemsMatched;
      Level.onItemsHit += OnItemsHit;
      Level.onFinished += OnLevelFinished;
    }
    private void OnDisable()
    {
      Item.onHide -= OnItemDestroy;
      Item.onBombExplode -= OnItemBombExplo;
      Item.onPushedOut -= OnItemPushedOut;
      Level.onStart -= OnLevelStart;
      Level.onItemsMatched -= OnItemsMatched;
      Level.onItemsHit -= OnItemsHit;
      Level.onFinished -= OnLevelFinished;
    }

    Vector3 GetFxPosition(Vector3 objectPostion) => objectPostion + (objectPostion - Camera.main.transform.position).normalized * -offsetToCamera;
    void PlayFXAtPosition(ParticleSystem ps, Vector3 worldPosition, int emitCount = 0)
    {
      ps.transform.position = GetFxPosition(worldPosition);
      if(emitCount > 0)
        ps.Emit(emitCount);
      else
        ps.Play();
    }
    
    void OnLevelStart(Level lvl)
    {
      _lvl = lvl;
    }
    void OnItemPushedOut(Item sender)
    {
      infoLblMan.ShowTextPopup(sender.transform.position, "pushed out!");
    }
    void OnItemBombExplo(Item sender)
    {
      var psmain = fxPaintSplat.main;
      psmain.startColor = sender.color;
      PlayFXAtPosition(fxPaintSplat, sender.transform.position, 5);      
    }
    void OnItemsHit(Item itemA, Item itemB)
    {
      if(itemA && itemB)
        PlayFXAtPosition(fxHit, (itemA.transform.position + itemB.transform.position) * 0.5f, 30);
    }
    void OnItemsMatched(Level.Match3 match)
    {
      Vector3 v = match.MidPos();
      //PlayFXAtPosition(fxPaintSplat, v, 5);
      infoLblMan.ShowTextPopup(v, "match!", match.GetColor());
      cameraShakeContainer.Shake(objShakePresetLo);
    }
    void OnItemDestroy(Item sender)
    {
      var psmain = fxPaintSplat.main;
      psmain.startColor = sender.color;
      //PlayFXAtPosition(fxPaintSplat, sender.transform.position, 5);

      var fo = GameData.Prefabs.CreateObjectFracture(sender.transform.parent);
      fo.transform.position = sender.transform.position;
      fo.Fracture(sender.vdir * 2);
      if(_lvl)
        _lvl.AddFractures(fo);
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
        fxConfettiLevel.Play();
      cameraShakeContainer.Shake(objShakePresetHi);
      _lvl = null;
    }
}
