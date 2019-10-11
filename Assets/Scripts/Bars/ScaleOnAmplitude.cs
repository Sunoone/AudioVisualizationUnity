using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOnAmplitude : MonoBehaviour
{
    public AudioPeer AudioPeer;

    public float StartScale, MaxScale;
    public bool UseBuffer;
    private Material _material;
    public float Red, Green, Blue;

    void Start()
    {
        _material = GetComponent<MeshRenderer>().materials[0];
    }

    void Update()
    {
        float audioValue = (UseBuffer) ? AudioPeer.AmplitudeBuffer : AudioPeer.Amplitude;
        float newScale = StartScale + (audioValue * (MaxScale - StartScale));
        transform.localScale = new Vector3(newScale, newScale, newScale);
        Color color = new Color(Red * audioValue, Green * audioValue, Blue * audioValue);
        _material.SetColor("_EmissionColor", color);
    }
}
