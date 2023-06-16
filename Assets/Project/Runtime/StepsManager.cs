using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepsManager : MonoBehaviour
{
    public List<int> alv;
    List<int> l1 = new List<int> { 4, 12, 25, 30, 32 };
    List<int> l2 = new List<int> { 4, 12, 25, 30, 32,3,5,11,13,24,29 };
    List<int> l3 = new List<int> { 4, 12, 25, 30, 32, 3, 5, 11, 13, 24, 29,7,1,15,9,23,28 };
    List<int> l4 = new List<int> { 1, 2, 3, 5, 7, 9, 10, 11, 13, 15, 22, 23, 24, 26, 27, 28, 29, 31, 32 };
    public AnimationCurve stepsCurve;
    public int updateValInt(float inputVal)
    {

        var predisplayVal = stepsCurve.Evaluate(inputVal) * 4;
        return Mathf.RoundToInt(predisplayVal);
    }

    public float updateValFloat(float inputVal)
    {

        var predisplayVal = stepsCurve.Evaluate(inputVal);
        return predisplayVal;
    }
    public List<int> getLevelPads(float inputVal)
    {
        int lv = updateValInt(inputVal);
        //print("LV: " + lv);
        switch (lv)
        {
            case 1: alv = l1; break;
            case 2: alv = l2; break;
            case 3: alv = l3; break;
            case 4: alv = l4; break;
            default: alv = new List<int>(); break;
        }
        print("size: " + alv.Count);
        return alv;
    }

}
