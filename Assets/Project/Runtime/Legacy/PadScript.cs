using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadScript : MonoBehaviour
{
    public class Pad {
        [Range(1, 32)]
        public int PadId;
        public int Remap;
        public float Amplitude;
        public int PulseWidth;
        public int Frequency;

        public Pad(int id, int remap, float amp, int pulse, int freq) {
            PadId = id;
            Remap = remap;
            Amplitude = amp;
            PulseWidth = pulse;
            Frequency = freq;
        }

        //Setters
        public void SetAmplitude(float amp)
        {
            Amplitude = amp;
        }

        public void SetPulseWidth(int pulse)
        {
            PulseWidth = pulse;
        }

        public void SetFrequency(int freq)
        {
            Frequency = freq;
        }

        //Getters
        public int GetPadId()
        {
            return PadId;
        }

        public float GetAmplitude() {
            return Amplitude;
        }

        public int GetPulseWidth()
        {
            return PulseWidth;
        }

        public int GetFrequency()
        {
            return Frequency;
        }

        public int GetRemap()
        {
            return Remap;
        }

    }

}
