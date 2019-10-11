using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    private AudioSource _audioSource;

    //Microphone input
    public AudioClip AudioClip;
    public bool UseMicrophone = false;


    private const int sampleAmount = 512;
    private float[] _samplesLeft = new float[sampleAmount];
    private float[] _samplesRight = new float[sampleAmount];

    private float bandMultiplier = 10;

    private const int freqBandAmount = 8;
    private float[] _freqBand = new float[freqBandAmount];
    private float[] _bandBuffer = new float[freqBandAmount];
    private float[] _bufferDecrease = new float[freqBandAmount];
    private float[] _freqBandHighest = new float[freqBandAmount];

    //Audio64
    private const int freqBandAmount64 = 64;
    private float[] _freqBand64 = new float[freqBandAmount64];
    private float[] _bandBuffer64 = new float[freqBandAmount64];
    private float[] _bufferDecrease64 = new float[freqBandAmount64];
    private float[] _freqBandHighest64 = new float[freqBandAmount64];

    [HideInInspector]
    public float[] AudioBand, AudioBandBuffer;

    [HideInInspector]
    public float[] AudioBand64, AudioBandBuffer64;

    [Range(0f, 1f)]
    public float BufferDecrease = 0.005f;
    [Range(1, 10)]
    public float BufferDecreaseMultiplier = 1.2f;

    [HideInInspector]
    public float Amplitude, AmplitudeBuffer;
    private float _amplitudeHighest;
    public float AudioProfile;

    public enum AudioChannel
    {
        Stereo,
        Left, 
        Right
    }

    public AudioChannel Channel;

    private void Start()
    {
        AudioBand = new float[freqBandAmount];
        AudioBandBuffer = new float[freqBandAmount];
        AudioBand64 = new float[freqBandAmount64];
        AudioBandBuffer64 = new float[freqBandAmount64];

        _audioSource = GetComponent<AudioSource>();
        SetAudioProfile(AudioProfile);
    }

    private void Update()
    {
        GetSpectrumAudioSource();
        UpdateFreqBands();
        UpdateFreqBands64();
        UpdateBufferBands();
        UpdateBufferBands64();
        UpdateAudioBands();
        UpdateAudioBands64();
        GetAmplitude();
    }

    private void SetAudioProfile(float audioProfile)
    {
        for (int i = 0; i < freqBandAmount; i++)
        {
            _freqBandHighest[i] = 0;
        }
    }

    private void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    private void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0;

        for (int i = 0; i < freqBandAmount; i++)
        {
            currentAmplitude += AudioBand[i];
            currentAmplitudeBuffer += AudioBandBuffer[i];
        }

        if (currentAmplitude > _amplitudeHighest)
        {
            _amplitudeHighest = currentAmplitude;
        }

        Amplitude = currentAmplitude / _amplitudeHighest;
        AmplitudeBuffer = currentAmplitudeBuffer / _amplitudeHighest;
    }

    
    
    private void UpdateFreqBands()
    {
        /* 22050 / 512 = 43 hZ per sample
         * 
         * 20 - 60
         * 60 - 250
         * 250 - 500
         * 500 - 2000
         * 2000 - 4000
         * 4000 - 6000
         * 6000 - 20000
         * 
         * 0 - 2 = 86
         * 1 - 4 = 172 > 87 - 258
         * 2 - 8 = 344 > 259 - 602
         * 3 - 16 = 688 > 603 - 1290
         * 4 - 32 = 1376 > 1291 - 2666
         * 5 - 64 = 2752 > 2667 - 5418
         * 6 - 128 = 5504 > 5419 - 10922
         * 7 - 256 = 11008 > 10923 - 21930
         * Adds to 510
         */

        int count = 0;

        for (int i = 0; i < freqBandAmount; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;  // To add 2 to the 512, to cover the entire spectrum.
            }

            for (int j = 0; j < sampleCount; j++)
            {
                if (Channel == AudioChannel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if (Channel == AudioChannel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (Channel == AudioChannel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }

                count++;
            }

            average /= count;
            _freqBand[i] = average * bandMultiplier;
        }
    }
    private void UpdateBufferBands()
    {
        for (int i = 0; i < freqBandAmount; i++)
        {
            if (_freqBand[i] > _bandBuffer[i])
            {
                _bandBuffer[i] = _freqBand[i];
                _bufferDecrease[i] = BufferDecrease;
            }

            if (_freqBand[i] < _bandBuffer[i])
            {
                _bandBuffer[i] -= _bufferDecrease[i];
                _bufferDecrease[i] *= BufferDecreaseMultiplier;
            }
        }
    }
    private void UpdateAudioBands()
    {
        for (int i = 0; i < freqBandAmount; i++)
        {
            if (_freqBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _freqBand[i];
            }
            AudioBand[i] = (_freqBand[i] / _freqBandHighest[i]);
            AudioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
        }
    }



    private void UpdateFreqBands64()
    {
        /* 0 -15    = 1 sample  =      16
         * 16 - 31  = 2 samples =      32;
         * 32 - 39  = 4 samples =      32;
         * 40 - 47  = 6 samples =      48;
         * 48 - 55  = 16 samples =    128;
         * 56 - 63  = 32 samples =    256;
         *                            ---
         *                            512
         */
         
        int count = 0;
        int sampleCount = 1;
        int power = 0;

        for (int i = 0; i < freqBandAmount64; i++)
        {
            float average = 0;
            

            if (i == 16 || i == 32 || i == 40 || i == 48 || i ==56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power);
                if (power == 3)
                {
                    sampleCount -= 2;
                }
            }

            for (int j = 0; j < sampleCount; j++)
            {
                if (Channel == AudioChannel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if (Channel == AudioChannel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (Channel == AudioChannel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }

                count++;
            }

            average /= count;
            _freqBand64[i] = average * bandMultiplier;
        }
    }
    private void UpdateBufferBands64()
    {
        for (int i = 0; i < freqBandAmount64; i++)
        {
            if (_freqBand64[i] > _bandBuffer64[i])
            {
                _bandBuffer64[i] = _freqBand64[i];
                _bufferDecrease64[i] = BufferDecrease;
            }

            if (_freqBand64[i] < _bandBuffer64[i])
            {
                _bandBuffer64[i] -= _bufferDecrease64[i];
                _bufferDecrease64[i] *= BufferDecreaseMultiplier;
            }
        }
    }
    private void UpdateAudioBands64()
    {
        for (int i = 0; i < freqBandAmount64; i++)
        {
            if (_freqBand64[i] > _freqBandHighest64[i])
            {
                _freqBandHighest64[i] = _freqBand64[i];
            }
            AudioBand64[i] = (_freqBand64[i] / _freqBandHighest64[i]);
            AudioBandBuffer64[i] = (_bandBuffer64[i] / _freqBandHighest64[i]);
        }
    }
}
