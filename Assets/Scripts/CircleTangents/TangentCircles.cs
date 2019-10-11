using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangentCircles : CircleTangent
{
    [Header("Setup")]
    public GameObject CirclePrefab;
    private GameObject _innerCircleGO, _outterCircleGO;
    private Vector4 _innerCircle, _outterCircle; //x, y, z, radius
    public float InnerCircleRadius, OutterCircleRadius;
    private Vector4[] _tangentCircle;
    private GameObject[] _tangentObject;
    [Range(1, 64)]
    public int CircleAmount;


    [Header("Input")]
    [Range(0, 1)]
    public float DistOutterTangent;
    [Range(0, 1)]
    public float MovementSmooth;
    [Range(0.1f, 10f)]
    public float RadiusChangeSpeed;

    private Vector2 _tsL ,_tsLSmooth;
    private float _radiusChange;

    [Header("Audio Visuals")]
    public AudioPeer AudioPeer;
    public Material MaterialBase;
    private Material[] _material;
    public Gradient Gradient;
    public float EmissionMultiplier;
    public bool EmissionBuffer;
    [Range(0,1)]
    public float EmissionThreshold;

    private float _rotateTangentObjects;
    public float RotateSpeed;
    public bool UseRotateBuffer;

    public bool ScaleYOnAudio;
    public bool ScaleBuffer;
    [Range(0,1)]
    public float ScaleThreshold;
    public float ScaleStart;
    public Vector2 ScaleMinMax;

    private void Start()
    {
        _innerCircle = new Vector4(0, 0, 0, InnerCircleRadius);
        _outterCircle = new Vector4(0, 0, 0, OutterCircleRadius);
        _tangentCircle = new Vector4[CircleAmount];
        _tangentObject = new GameObject[CircleAmount];


        _material = new Material[CircleAmount];
        for (int i = 0; i < CircleAmount; i++)
        {
            GameObject tangentInstance = (GameObject)Instantiate(CirclePrefab);
            _tangentObject[i] = tangentInstance;
            _tangentObject[i].transform.parent = this.transform;
            _material[i] = new Material(MaterialBase);
            _material[i].EnableKeyword("_EMISSION");
            _material[i].SetColor("_Color", new Color(0, 0, 0));
            if (_tangentObject[i].GetComponent<MeshRenderer>())
            {
                _tangentObject[i].GetComponent<MeshRenderer>().material = _material[i];
            }
            else
            {
                _tangentObject[i].transform.GetChild(0).GetComponent<MeshRenderer>().material = _material[i];
            }
        }
    }





    private void Update()
    {
        if (float.IsNaN(AudioPeer.AmplitudeBuffer) || float.IsNaN(AudioPeer.Amplitude))
            return;

        Radius(Input.GetAxis("TriggerL") - Input.GetAxis("TriggerR"));
        Axis(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));




        SolveRotateTangent();
        SolveCircles();
    }
    public void Radius(float change)
    {
        _radiusChange = change;
    }
    public void Axis(float horizontal, float vertical)
    {
       _tsL = new Vector2(horizontal, vertical).normalized;
        _tsLSmooth = new Vector2(
            _tsLSmooth.x * (1 - MovementSmooth) + _tsL.x * MovementSmooth,
            _tsLSmooth.y * (1 - MovementSmooth) + _tsL.y * MovementSmooth);

        _innerCircle = new Vector4(
            (_tsLSmooth.x * (_outterCircle.w - _innerCircle.w) * (1 - DistOutterTangent)) + _outterCircle.x,
            0.0f,
            (_tsLSmooth.y * (_outterCircle.w - _innerCircle.w) * (1 - DistOutterTangent)) + _outterCircle.z,
            _innerCircle.w + (_radiusChange * Time.deltaTime * RadiusChangeSpeed));
    }
    private void SolveRotateTangent()
    {
        if (UseRotateBuffer)
        {
            _rotateTangentObjects += RotateSpeed * Time.deltaTime * AudioPeer.AmplitudeBuffer;
        }
        else
        {
            _rotateTangentObjects += RotateSpeed * Time.deltaTime * AudioPeer.Amplitude;
        }
    }
    private void SolveCircles()
    {
        
        for (int i = 0; i < CircleAmount; i++)
        {
            _tangentCircle[i] = FindTangentCircle(_outterCircle, _innerCircle, (360f / CircleAmount) * i + _rotateTangentObjects);
            _tangentObject[i].transform.position = new Vector3(_tangentCircle[i].x, _tangentCircle[i].y, _tangentCircle[i].z);


            if (ScaleYOnAudio)
            {
                if (AudioPeer.AudioBandBuffer64[i] > ScaleThreshold)
                {
                    if (ScaleBuffer)
                    {
                        _tangentObject[i].transform.localScale = new Vector3(_tangentCircle[i].w, ScaleStart + Mathf.Lerp(ScaleMinMax.x, ScaleMinMax.y, AudioPeer.AudioBandBuffer64[i]), _tangentCircle[i].w) * 2;
                    }
                    else
                    {
                        _tangentObject[i].transform.localScale = new Vector3(_tangentCircle[i].w, ScaleStart + Mathf.Lerp(ScaleMinMax.x, ScaleMinMax.y, AudioPeer.AudioBand64[i]), _tangentCircle[i].w) * 2;
                    }
                }
                else
                {
                    _tangentObject[i].transform.localScale = new Vector3(_tangentCircle[i].w, ScaleStart, _tangentCircle[i].w);
                }
            }
            else
            {
                _tangentObject[i].transform.localScale = new Vector3(_tangentCircle[i].w, _tangentCircle[i].w, _tangentCircle[i].w);
            }

            

            /*Vector4 tangentCircle = FindTangentCircle(_outterCircle, _innerCircle, (360f / CircleAmount) * i);
            _tangentObject[i].transform.position = new Vector3(tangentCircle.x, tangentCircle.y, tangentCircle.z);
            _tangentObject[i].transform.localScale = new Vector3(tangentCircle.w, tangentCircle.w, tangentCircle.w) * 2;*/



            if (AudioPeer.AudioBandBuffer64[i] > EmissionThreshold)
            {
                if (EmissionBuffer)
                {
                    _material[i].SetColor("_EmissionColor", Gradient.Evaluate((1f / CircleAmount) * i) * AudioPeer.AudioBandBuffer64[i] * EmissionMultiplier);
                }
                else
                {
                    _material[i].SetColor("_EmissionColor", Gradient.Evaluate((1f / CircleAmount) * i) * AudioPeer.AudioBand64[i] * EmissionMultiplier);
                }
            }
            else
            {
                _material[i].SetColor("_EmissionColor", new Color(0, 0, 0));
            }
        }
    }
}
