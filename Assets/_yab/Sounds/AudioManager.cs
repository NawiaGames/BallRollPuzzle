using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.AudioSystem;
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioPlayer ballSpawnAudio = null;
    [SerializeField] AudioPlayer ballHitAudio = null;
    [SerializeField] AudioPlayer ballDestroyAudio = null;
    [SerializeField] AudioPlayer victoryAudio = null;

    private void OnEnable() {
        Item.onHit += PlayBallHitSFX;
        Item.onPushedOut += PlayBallDestroyAudio;
        Level.onDone += PlayConfettiAudio;
    }
    private void OnDisable() {
        Item.onHit -= PlayBallHitSFX;        
        Item.onPushedOut -= PlayBallDestroyAudio;
        Level.onDone -= PlayConfettiAudio;
    }

    void PlayBallSpawnSFX(object sender) => ballSpawnAudio.Play();
    void PlayBallHitSFX(object sender) => ballHitAudio.Play();
    void PlayBallDestroyAudio(object sender) => ballDestroyAudio.Play();
    void PlayConfettiAudio(object sender) => victoryAudio.Play();

}
