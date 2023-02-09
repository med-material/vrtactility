using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TactilityManager : MonoBehaviour
{
    [Tooltip("Log outbound command strings to the console before they are transmitted.")] [SerializeField] 
    public bool logOutboundCommands = false;
    
    [SerializeField] private CalibrationScriptableObject calibrationData;
    private List<PadScript.Pad> _pads;
    
    private UniformGrabbable _ug;
    private SerialController _glovePort;
    
    private bool _portWriteInProgress;
    private float[] _prevValueBatch;

    private void Awake()
    {
        // Retrieve the calibration values
        _pads = calibrationData.values;
    }

    private void Start()
    {
        if (_pads.Count == 0) 
            Debug.LogWarning("No calibration data found, tactility will be disabled");
        
        _ug = FindObjectOfType<UniformGrabbable>();
        _portWriteInProgress = false;
        
        var serialControllerGameObject = GameObject.Find("SerialController");
        _glovePort = serialControllerGameObject.GetComponent<SerialController>();
    }

    private void Update()
    {
        if (_portWriteInProgress || _pads.Count == 0) return;
        
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

        // Avoid invoking the stimulator if there are no pressure values to transmit
        if (valueBatch.All(value => value == 0))
        {
            if (_prevValueBatch != null && _prevValueBatch.Any(value => value != 0))
            {
                // Disable the stimulator if the hand has just released the object
                WriteToPort("velec 11 *selected 0");
                Array.Clear(_prevValueBatch, 0, _prevValueBatch.Length);
            }
            return;
        }
        
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
        const string finalPart = " *selected 1 *sync 0\r";

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

            // Ignore pads to implicitly assign them as Anodes
            if (i is 1 or 2 or 9 or 10 or 11 or 12 or 23 or 28) 
                continue;

            var amplitudeValue = GetConstrainedValue(_pads[i].GetAmplitude() * pressureValue);

            variablePart1 += _pads[i].GetRemap() + "=C,";
            variablePart2 += _pads[i].GetRemap() + "=" + amplitudeValue + ",";
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
    }

    private static float GetConstrainedValue(float value)
    {
        // Constrain the stimulator input such that overloading is avoided
        var rounded = Mathf.Round(value * 10) / 10;  // Fix the input value to 1 decimal increments
        var clamped = Mathf.Clamp(rounded, 0.5f, 9.0f);  // Fix input to compatible range
        
        return clamped;
    }
    
    private void WriteToPort(string command, int timeout = 100, Action callback = null)
    {
        _portWriteInProgress = true;
        
        IEnumerator WriteTimeout()
        {
            Debug.Log("Outbound command: " + command);
            _glovePort.SendSerialMessage(command + "\r");                      // Write command to glove box
            yield return new WaitForSeconds(seconds: (float)timeout / 1_000);  // Wait for specified amount of seconds
            _portWriteInProgress = false;                                      // Start listening for new commands again
            callback?.Invoke();                                                // Invoke the provided callback if any
        }
        StartCoroutine(WriteTimeout());
    }
}
