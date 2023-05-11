using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TactilityManager : MonoBehaviour
{
    public SpatialStepsManager stepsManager;

    [Tooltip("Log outbound command strings to the console before they are transmitted.")] [SerializeField] 
    public bool logOutboundCommands = false;
    
    [SerializeField] private CalibrationScriptableObject calibrationData;
    private List<PadScript.Pad> _pads;
    
    private UniformGrabbable _ug;
    private SerialController _glovePort;
    
    private bool _portWriteInProgress;
    private float[] _prevValueBatch;

    public enum Modality {Amplitude, Frequency, Spatial}

    public Modality selectedModality;
    public AnimationCurve stepsCurve;
    
    [Range(0f, 1f)] public float debugVal;


    [Range(0.1f, 1.3f)] public float freqAmpMod;

    public bool UseDebugVal;
    public int updateVal(float inputVal)
    {

        var predisplayVal = stepsCurve.Evaluate(inputVal) * 4;
        return Mathf.RoundToInt(predisplayVal);
    }

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
        if (UseDebugVal) { valueBatch = new float[] { debugVal, debugVal, debugVal, debugVal, debugVal }; }
        else {
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
        var commandString = GenerateCommandString(valueBatch, selectedModality);
        WriteToPort(commandString);
    }
    private string GenerateCommandString(IReadOnlyList<float> pressureValues,Modality modality)
    {
        string command = "";
       if(modality == Modality.Amplitude) command = GenerateCommandStringAmp(pressureValues);
        if (modality == Modality.Frequency) command = GenerateCommandStringFreq(pressureValues);
        if (modality == Modality.Spatial) command = GenerateCommandStringSpatial(pressureValues);
        return command;
    }
    private string GenerateCommandStringAmp(IReadOnlyList<float> pressureValues)
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
    public List<int> alv;
    List<int> l1 = new List<int> { 1, 9, 22, 27, 32 };
    List<int> l2 = new List<int> { 1, 2, 9, 10, 22, 23, 27, 28, 32 };
    List<int> l3 = new List<int> { 1, 2, 3, 5, 9, 10, 11, 13, 22, 23, 24, 27, 28, 29, 32 };
    List<int> l4 = new List<int> { 1, 2, 3, 5, 7, 9, 10, 11, 13, 15, 22, 23, 24, 26, 27, 28, 29, 31, 32 };
    private List<int> getLevelPads()
    {
        int lv = updateVal(debugVal);
        //print("LV: " + lv);
        switch (lv)
        {
            case 1: alv = l1; break;
            case 2: alv = l2; break;
            case 3: alv = l3; break;
            case 4: alv = l4; break;
            default: alv = new List<int>(); break;
        }
        print("size: "+alv.Count);
        return alv;
    }
    private string GenerateCommandStringSpatial(IReadOnlyList<float> pressureValues) 
    {
        var completeString = "";

        const string invariablePart1 = "velec 11 *special_anodes 1 *name test *elec 1 *pads ";
        const string invariablePart2 = " *amp ";
        const string invariablePart3 = " *width ";
        const string finalPart = " *selected 1 *sync 0\r";

        var variablePart1 = "";
        var variablePart2 = "";
        var variablePart3 = "";
        int[] activeLevel = stepsManager.getLevelPads(debugVal).ToArray();
        for (var i = 0; i < activeLevel.Length; i++)
        {
            // Use remap value to determine which finger pressure value we use
            var pressureValue = i switch
            {
                < 8 => pressureValues[0],
                < 21 => pressureValues[1],
                < 26 => pressureValues[2],
                < 31 => pressureValues[3],
                _ => pressureValues[4]  // == 31
            };
            if (pressureValue <= 0) continue;
            // Ignore pads to implicitly assign them as Anodes
            if (i is 1 or 2 or 9 or 10 or 11 or 12 or 23 or 28)
                continue;

            var amplitudeValue = GetConstrainedValue(_pads[i].GetAmplitude());

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
    private string GenerateCommandStringFreq(IReadOnlyList<float> pressureValues)
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
                < 8 => pressureValues[0],
                < 21 => pressureValues[1],
                < 26 => pressureValues[2],
                < 31 => pressureValues[3],
                _ => pressureValues[4]  // == 31
            };
            if (pressureValue <= 0) continue;
            // Ignore pads to implicitly assign them as Anodes
            if (i is 1 or 2 or 9 or 10 or 11 or 12 or 23 or 28)
                continue;


            var amplitudeValue = GetConstrainedValue(_pads[i].GetAmplitude() * freqAmpMod);

            variablePart1 += _pads[i].GetRemap() + "=C,";
            variablePart2 += _pads[i].GetRemap() + "=" + amplitudeValue + ",";
            variablePart3 += _pads[i].GetRemap() + "=" + _pads[i].GetPulseWidth() + ",";
        }
        updateFreq();
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

    void updateFreq()
    {
        int freq = Mathf.RoundToInt(_pads[1].GetFrequency() * _ug.getMaxPressure());
        int freqclamp = Mathf.Clamp(freq, 10, 200);
        string command = "freq " + freqclamp;
        WriteToPort(command);
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
