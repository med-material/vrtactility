using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TactilityManager : MonoBehaviour
{
    [SerializeField] private CalibrationScriptableObject calibrationData;
    private List<PadScript.Pad> _pads;
    
    private UniformGrabbable _ug;
    
    private bool _stimOn = false;
    private bool _portWriteInProgress = false;

    private void Awake()
    {
        // NOTE: Establish connection with the box here.
        //       This would be done by the other scene, but should be done here for debugging purposes
        //       It looks like a persistent instance of UiManager must be kept between he scenes, and it might already
        //       Be set to not destroy on load but it seems like a mess
        return;
    }

    private void Start()
    {
        _pads = calibrationData.values;
        _ug = FindObjectOfType<UniformGrabbable>();
    }

    private void Update()
    {
        switch (_stimOn)
        {
            // Manage stim on / off commands and state
            case true when _ug.touchingBoneIds.Count == 0:
                _stimOn = false;
                UpdateStimState();
                break;
            case false when _ug.touchingBoneIds.Count > 0:
                _stimOn = true;
                UpdateStimState();
                break;
        }
        if (!_stimOn) return;

        // Update stimuli for each touching finger bone of interest
        var valueBatch = new float[5];
        for (var i = 0; i < _ug.touchingBoneIds.Count; i++)
        {
            var pressure = _ug.touchingBonePressures[i];
            switch (_ug.touchingBoneIds[i])
            {
                case OVRSkeleton.BoneId.Hand_Thumb3:
                    valueBatch[0] = pressure;
                    break;
                case OVRSkeleton.BoneId.Hand_Index3:
                    valueBatch[1] = pressure;
                    break;
                case OVRSkeleton.BoneId.Hand_Middle3:
                    valueBatch[2] = pressure;
                    break;
                case OVRSkeleton.BoneId.Hand_Ring3:
                    valueBatch[3] = pressure;
                    break;
                case OVRSkeleton.BoneId.Hand_Pinky3:
                    valueBatch[4] = pressure;
                    break;
            }
        }
        BatchWriteToPort(valueBatch);
    }

    private void OnDisable()
    {
        // TODO: Clean up ports and whatever other stuff there might be like in the other scene...
        return;
    }

    private void UpdateStimState()
    {
        WriteToPort(_stimOn ? "stim on" : "stim off", 150);
    }

    private void BatchWriteToPort(IReadOnlyList<float> values, int timeout = 100)
    {
        // NOTE: I don't know how to stimulate multiple different pads simultaneously.
        //       In lieu of more info, this method and subsequent "ApplyStimuli" implementation will likely need change.
        for (var i = 0; i < values.Count; i++)
        {
            ApplyStimuli(i, values[i], i == values.Count - 1 ? timeout : 10);
        }
    }

    private void ApplyStimuli(int index, float pressure, int timeout = 100)
    {
        WriteToPort("velec 11 *special_anodes 1 *name test *elec 1 *pads " + _pads[index].GetRemap() * pressure + "=C, *amp " + _pads[index].GetRemap() * pressure + "=" + _pads[index].GetAmplitude() * pressure + ", *width " + _pads[index].GetRemap() * pressure + "=" + _pads[index].GetPulseWidth() * pressure + ", *selected 1 *sync 0", timeout);
    }
    
    private void WriteToPort(string command, int timeout = 100)
    {
        // NOTE: This is an attempt to port the Thread.Sleep logic from the UiManager into coroutines, since this would
        //       otherwise freeze game execution completely if done in the VR scene (from my understanding)...
        if (_portWriteInProgress) return;
        _portWriteInProgress = true;
        
        var prevStimState = _stimOn;
        IEnumerator WriteTimeout()
        {
            ConnectDevice.glovePort.Write(command + "\r\n");

            yield return new WaitForSeconds(seconds: timeout / 1_000);
            
            if (_stimOn != prevStimState) UpdateStimState();
            _portWriteInProgress = false;
        }

        StartCoroutine(WriteTimeout());
    }
}
