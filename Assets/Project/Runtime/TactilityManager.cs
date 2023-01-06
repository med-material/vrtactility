using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TactilityManager : MonoBehaviour
{
    [SerializeField] private CalibrationScriptableObject calibrationData;
    private List<PadScript.Pad> _pads;
    
    private UniformGrabbable _ug;
    private SerialController _glovePort;
    
    private bool _portWriteInProgress;
    private float[] _prevValueBatch;

    private void Awake()
    {
        // NOTE: Establish connection with the box here.
        //       This would be done by the other scene, but should be done here for debugging purposes
        //       It looks like a persistent instance of UiManager must be kept between he scenes, and it might already
        //       Be set to not destroy on load but it seems like a mess
        _pads = calibrationData.values;
    }

    private void Start()
    {
        _ug = FindObjectOfType<UniformGrabbable>();
        _portWriteInProgress = false;
        
        var serialControllerGameObject = GameObject.Find("SerialController");
        _glovePort = serialControllerGameObject.GetComponent<SerialController>();
    }

    private void Update()
    {
        if (_portWriteInProgress) return;
        
        // Update stimuli for each touching finger bone of interest
        var valueBatch = new float[5];
        for (var i = 0; i < _ug.touchingBoneIds.Count; i++)
        {
            var pressure = _ug.touchingBonePressures[i];
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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

        // If the new values are identical to the old ones and their sum is zero then we reject them
        if (valueBatch.All(value => value == 0) 
            && _prevValueBatch != null 
            && _prevValueBatch.All(value => value == 0)) return;
        _prevValueBatch = valueBatch;
        
        // Generate the command string and send it to the box
        var commandString = GenerateCommandString(valueBatch);
        WriteToPort(commandString);
    }

    private string GenerateCommandString(IReadOnlyList<float> pressureValues)
    {
        var completeString = "";
        
        const string invariablePart1 = "velec 11 *special_anodes 1 *name test *elec 1 *pads ";
        const string invariablePart2 = " *amp ";
        const string invariablePart3 = " *width ";
        var finalPart = false // pressureValues.All(value => value == 0)  // If sum is zero then we disable all electrodes
            ? " *selected 0 *sync 0\r\n" 
            : " *selected 1 *sync 0\r\n";

        var variablePart1 = "";
        var variablePart2 = "";
        var variablePart3 = "";

        for (var i = 0; i < 32; i++)
        {
            // Use remap value to determine which finger pressure value we use
            var pressureValue = i switch
            {
                < 8  => pressureValues[0],
                < 21 => pressureValues[1],
                < 26 => pressureValues[2],
                < 31 => pressureValues[3],
                _    => pressureValues[4]  // == 31
            };
            // if (i < 8) Debug.Log(_pads[i].GetAmplitude() * pressureValue);
            if (i > 1) continue;

            variablePart1 += _pads[i].GetRemap() + "=C,";
            variablePart2 += _pads[i].GetRemap() + "=" + _pads[i].GetAmplitude() /** pressureValue*/ + ",";
            variablePart3 += _pads[i].GetRemap() + "=" + _pads[i].GetPulseWidth() + ",";
        }

        // Build final string and return
        completeString = invariablePart1 
                         + variablePart1 
                         + invariablePart2 
                         + variablePart2 
                         + invariablePart3 
                         + variablePart3 
                         + finalPart;
        return completeString;
        // return "velec 11 *special_anodes 1 *name test *elec 1 *pads " +
        //        _pads[0].GetRemap() + "=C, *amp " +
        //        _pads[0].GetRemap() + "=" +
        //        _pads[0].GetAmplitude() + ", *width " +
        //        _pads[0].GetRemap() + "=" +
        //        _pads[0].GetPulseWidth() + ", *selected 1 *sync 0";
    }
    
    private void WriteToPort(string command, int timeout = 20, Action callback = null)
    {
        // NOTE: This is an attempt to port the Thread.Sleep logic from the UiManager into coroutines, since this would
        //       otherwise freeze game execution completely if done in the VR scene (from my understanding)...
        // if (_portWriteInProgress) return;
        _portWriteInProgress = true;
        
        IEnumerator WriteTimeout()
        {
            // Debug.Log(command);
            _glovePort.SendSerialMessage(command + "\r");                       // Write command to glove box
            yield return new WaitForSeconds(seconds: (float)timeout / 1_000);   // Wait for specified amount of seconds
            _portWriteInProgress = false;                                       // Start listening for new commands again
            callback?.Invoke();                                                 // Invoke the provided callback if any
        }
        StartCoroutine(WriteTimeout());
    }
}
