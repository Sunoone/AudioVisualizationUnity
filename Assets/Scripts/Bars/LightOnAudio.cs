using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOnAudio : MonoBehaviour
{
    public AudioPeer AudioPeer;

    public int Band;
    public float MinIntensity, MaxIntensity;
    public Light Light;

    void Update()
    {
        Light.intensity = (AudioPeer.AudioBandBuffer[Band] * (MaxIntensity - MinIntensity)) + MinIntensity;
    }
}
