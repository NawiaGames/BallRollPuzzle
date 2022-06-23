using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.AudioSystem;
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioPlayer ballHitAudio = null;

    private void OnEnable() {
        Item.onHit += PlayBallHitSFX;
    }
    private void OnDisable() {
        Item.onHit -= PlayBallHitSFX;        
    }

    void PlayBallHitSFX(object sender) => ballHitAudio.Play();
}
