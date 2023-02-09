using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UniformGrabbable))]
[RequireComponent(typeof(Renderer))]
public class DebugGribVisualizer : MonoBehaviour
{
    private UniformGrabbable _ug;
    private Renderer _renderer;
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private int _lastCount = 0;  // The number of touching bones in the previous frame update

    private void Start()
    {
        _ug = GetComponent<UniformGrabbable>();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Don't process highlight if no bones are touching, but remember to update once after the list empties
        if (_ug.touchingBonePressures.Count + _lastCount == 0) return;
        _lastCount = _ug.touchingBonePressures.Count;

        // Calculate total pressure being applied and update sphere material color
        var maxPressure = _ug.touchingBonePressures.Count > 0
            ? _ug.touchingBonePressures.Max()
            : 0f;
        _renderer.material.SetColor(Color1, new Color(maxPressure, 0f, 0f));
    }
}
