using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiate512Cubes : MonoBehaviour
{
    /*public AudioPeer AudioPeer;

    private const int _amount = 512;

    public GameObject SampleCubePrefab;
    private GameObject[] _sampleCubes = new GameObject[_amount];

    [Range(1, 512)]
    public float Radius = 100;
    public float MaxScale;
    void Start()
    {
        float arc = 360f / 512f;
        for (int i = 0; i < _amount; i++)
        {
            GameObject InstanceSampleCube = (GameObject)Instantiate(SampleCubePrefab);
            InstanceSampleCube.transform.position = this.transform.position;
            InstanceSampleCube.transform.parent = this.transform;
            InstanceSampleCube.name = "SampleCube_" + i;

            this.transform.eulerAngles = new Vector3(0, i * arc, 0);
            InstanceSampleCube.transform.position = Vector3.forward * Radius;
            _sampleCubes[i] = InstanceSampleCube;
        }
    }

    void Update()
    {
        if (_sampleCubes == null)
            return;

        for (int i = 0; i < _amount; i++)
        {
            _sampleCubes[i].transform.localScale = new Vector3(10, (AudioPeer.Audio * MaxScale) + 2, 10);
        }
    }*/
}
