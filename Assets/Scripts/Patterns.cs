using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Patterns : MonoBehaviour
{
    public enum Pattern
    {
        AllSimple = 1,
        AllComplex = 2,
        IndexSimple = 3,
        IndexComplex = 4
    }
    [HideInInspector]
    public Pattern currentPattern;
    //public ActivatePattern activatePattern;
    private int[][] AllFingersSimplePads = new int[10][];
    private float[] AllFingersSimpleAmpMult = new float[10];
    private int[][] AllFingersComplexPads = new int[26][];
    private float[] AllFingersComplexAmpMult = new float[26];
    private int[][] IndexSimplePads = new int[1][];
    private float[] IndexSimpleAmpMult = new float[1];
    private int[][] IndexComplexPads = new int[10][];
    private float[] IndexComplexAmpMult = new float[10];

    //private int[] AllFIngersSimpleStimTime = new int[10];
    //private int[] AllFIngersSimpleBtwStimTime = new int[10];

    private int[][] currentPatternPads;
    private float[] currentAmpMult;

    

    // Start is called before the first frame update
    void Start()
    {
        currentPattern = Pattern.AllSimple;

        //AllFingerSimple
        AllFingersSimplePads[0] = new int[] { 1, 2, 3, 5, 6, 8};
        AllFingersSimplePads[1] = new int[] { 9, 10, 11, 13, 14, 16};
        AllFingersSimplePads[2] = new int[] { 22, 23, 24, 26};
        AllFingersSimplePads[3] = new int[] { 27, 28, 29, 31 };
        AllFingersSimplePads[4] = new int[] { 32 };
        AllFingersSimplePads[5] = new int[] { 32 };
        AllFingersSimplePads[6] = new int[] { 27, 28, 29, 31 };
        AllFingersSimplePads[7] = new int[] { 22, 23, 24, 26 };
        AllFingersSimplePads[8] = new int[] { 9, 10, 11, 13, 14, 16 };
        AllFingersSimplePads[9] = new int[] { 1, 2, 3, 5, 6, 8 };

        AllFingersSimpleAmpMult = new float[] {0.8f, 0.8f, 0.8f, 0.8f, 1f, 1f, 0.8f, 0.8f, 0.8f, 0.8f};

        //AllFIngerComplex
        AllFingersComplexPads[0] = new int[] { 1, 3, 6 };
        AllFingersComplexPads[1] = new int[] { 2, 4, 7};
        AllFingersComplexPads[2] = new int[] { 5, 8 };
        AllFingersComplexPads[3] = new int[] { 9, 11, 14 };
        AllFingersComplexPads[4] = new int[] { 10, 12, 15};
        AllFingersComplexPads[5] = new int[] { 13, 16 };
        AllFingersComplexPads[6] = new int[] { 22, 24 };
        AllFingersComplexPads[7] = new int[] { 23, 25 };
        AllFingersComplexPads[8] = new int[] { 26 };
        AllFingersComplexPads[9] = new int[] { 27, 29 };
        AllFingersComplexPads[10] = new int[] { 28, 30 };
        AllFingersComplexPads[11] = new int[] { 31 };
        AllFingersComplexPads[12] = new int[] { 32 };
        AllFingersComplexPads[13] = new int[] { 32 };
        AllFingersComplexPads[14] = new int[] { 31 };
        AllFingersComplexPads[15] = new int[] { 28, 30 };
        AllFingersComplexPads[16] = new int[] { 27, 29 };
        AllFingersComplexPads[17] = new int[] { 26 };
        AllFingersComplexPads[18] = new int[] { 23, 25 };
        AllFingersComplexPads[19] = new int[] { 22, 24 };
        AllFingersComplexPads[20] = new int[] { 13, 16 };
        AllFingersComplexPads[21] = new int[] { 10, 12, 15 };
        AllFingersComplexPads[22] = new int[] { 9, 11, 14 };
        AllFingersComplexPads[23] = new int[] { 5, 8 };
        AllFingersComplexPads[24] = new int[] { 2, 4, 7 };
        AllFingersComplexPads[25] = new int[] { 1, 3, 6 };

        AllFingersComplexAmpMult = new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        //IndexSimple
        IndexSimplePads[0] = new int[] { 9, 10, 11, 13, 17, 18, 19, 20, 21 };

        IndexSimpleAmpMult[0] = 0.8f;

        //IndexComplex
        IndexComplexPads[0] = new int[] { 9, 10 };
        IndexComplexPads[1] = new int[] { 11, 12, 13 };
        IndexComplexPads[2] = new int[] { 14, 15, 16 };
        IndexComplexPads[3] = new int[] { 17, 18, 21 };
        IndexComplexPads[4] = new int[] { 19, 20, 21 };
        IndexComplexPads[5] = new int[] { 19, 20, 21 };
        IndexComplexPads[6] = new int[] { 17, 18, 21 };
        IndexComplexPads[7] = new int[] { 14, 15, 16 };
        IndexComplexPads[8] = new int[] { 11, 12, 13 };
        IndexComplexPads[9] = new int[] { 9, 10 };

        IndexComplexAmpMult = new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };



        //AllFIngersSimpleStimTime[0] = 300;
        //AllFIngersSimpleStimTime[1] = 300;
        //AllFIngersSimpleStimTime[2] = 300;
        //AllFIngersSimpleStimTime[3] = 300;
        //AllFIngersSimpleStimTime[4] = 300;
        //AllFIngersSimpleStimTime[5] = 300;
        //AllFIngersSimpleStimTime[6] = 300;
        //AllFIngersSimpleStimTime[7] = 300;
        //AllFIngersSimpleStimTime[8] = 300;
        //AllFIngersSimpleStimTime[9] = 300;

        //AllFIngersSimpleBtwStimTime[0] = 300;
        //AllFIngersSimpleBtwStimTime[1] = 300;
        //AllFIngersSimpleBtwStimTime[2] = 300;
        //AllFIngersSimpleBtwStimTime[3] = 300;
        //AllFIngersSimpleBtwStimTime[4] = 300;
        //AllFIngersSimpleBtwStimTime[5] = 300;
        //AllFIngersSimpleBtwStimTime[6] = 300;
        //AllFIngersSimpleBtwStimTime[7] = 300;
        //AllFIngersSimpleBtwStimTime[8] = 300;
        //AllFIngersSimpleBtwStimTime[9] = 300;


    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //    activatePattern.ActivateString(AllFingersSimplePads[0]);
    }

    public string ActivateString(int index)
    {
        string CompleteLine = "";
        string InvariablePart1 = "velec 11 *special_anodes 1 *name test *elec 1 *pads ";
        string InvariablePart2 = " *amp ";
        string InvariablePart3 = " *width ";
        string InvariablePart4 = " *selected 1 *sync 0\r\n";

        string VariablePart1 = "";
        string VariablePart2 = "";
        string VariablePart3 = "";

        for (int i = 0; i < currentPatternPads[index].Length; i++)
        {
            VariablePart1 += ConnectDevice.Remap2[currentPatternPads[index][i]-1].GetRemap() + "=C,";
            VariablePart2 += ConnectDevice.Remap2[currentPatternPads[index][i]-1].GetRemap() + "=" + Mathf.Round(((ConnectDevice.Remap2[currentPatternPads[index][i]-1].GetAmplitude() + 0.001f) * currentAmpMult[index]) * 10.0f) * 0.1f + ",";
            VariablePart3 += ConnectDevice.Remap2[currentPatternPads[index][i]-1].GetRemap() + "=" + ConnectDevice.Remap2[currentPatternPads[index][i]-1].GetPulseWidth() + ",";
        }

        CompleteLine = InvariablePart1 + VariablePart1 + InvariablePart2 + VariablePart2 + InvariablePart3 + VariablePart3 + InvariablePart4;

        return CompleteLine;
    }

    public void UpdatePattern(int index)
    {
        switch (index)
        {
            case 1:
                currentPattern = Pattern.AllSimple;
                currentPatternPads = AllFingersSimplePads;
                currentAmpMult = AllFingersSimpleAmpMult;
                break;
            case 2:
                currentPattern = Pattern.AllComplex;
                currentPatternPads = AllFingersComplexPads;
                currentAmpMult = AllFingersComplexAmpMult;
                break;
            case 3:
                currentPattern = Pattern.IndexSimple;
                currentPatternPads = IndexSimplePads;
                currentAmpMult = IndexSimpleAmpMult;
                break;
            case 4:
                currentPattern = Pattern.IndexComplex;
                currentPatternPads = IndexComplexPads;
                currentAmpMult = IndexComplexAmpMult;
                break;
        }
    }
    

}
