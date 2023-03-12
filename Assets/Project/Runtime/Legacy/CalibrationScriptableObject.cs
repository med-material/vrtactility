using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CalibrationData", menuName = "ScriptableObjects/CalibrationScriptableObject", order = 1)]
public class CalibrationScriptableObject : ScriptableObject
{
    public List<PadScript.Pad> values = new List<PadScript.Pad>();
    public string port;
}
