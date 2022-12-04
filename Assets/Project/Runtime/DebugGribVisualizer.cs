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
        if (_ug.touchingBonePressures.Count == 0) return;
        
        // Calculate total pressure being applied and update sphere material color
        var maxPressure = _ug.touchingBonePressures.Max()/* / _ug.touchingBonePressures.Count*/;
        _renderer.material.SetColor(Color1, new Color(maxPressure, 0f, 0f));
    }
}
