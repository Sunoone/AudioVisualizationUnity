using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamCube : MonoBehaviour
{
    public AudioPeer AudioPeer;

    public int Band;
    public float StartScale, ScaleMultiplier;
    public bool UseBuffer;

    private Material _material;

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().materials[0];
    }

    void Update()
    {
        if (float.IsNaN(AudioPeer.AmplitudeBuffer) || float.IsNaN(AudioPeer.Amplitude))
            return;

        float audioValue = (UseBuffer) ? AudioPeer.AudioBandBuffer[Band] : AudioPeer.AudioBand[Band];
        // Not sure why it provides errors, even when this is in late update.


        transform.localScale = new Vector3(transform.localScale.x, StartScale + (audioValue * ScaleMultiplier), transform.localScale.z);
        Color color = new Color(audioValue, audioValue, audioValue);
        _material.SetColor("_EmissionColor", color);
    }
}
