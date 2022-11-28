using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniformGrabbable))]
[RequireComponent(typeof(Renderer))]
public class DebugGribVisualizer : MonoBehaviour
{
    private UniformGrabbable _ug;
    private Renderer _renderer;
    private static readonly int Color1 = Shader.PropertyToID("_Color");

    private void Start()
    {
        _ug = GetComponent<UniformGrabbable>();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Calculate total pressure being applied
        var totalPressure = _ug.touchingBonePressures.Sum()/* / _ug.touchingBonePressures.Count*/;
        
        // Transpose into rgb range and apply to material
        var range = Mathf.Clamp01(totalPressure / 10f);
        _renderer.material.SetColor(Color1, new Color(range, 0f, 0f));
    }
}
