using UnityEngine;

[RequireComponent(typeof(ITouchable))]
[RequireComponent(typeof(Renderer))]
public class PressureVisualization : MonoBehaviour
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    private ITouchable _touchable;
    private Renderer _renderer;
    
    private int _lastCount;  // The number of touching bones in the previous frame update

    private void Start()
    {
        _touchable = GetComponent<ITouchable>();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Don't process highlight if no bones are touching, but remember to update once after the list empties
        var pressurePointCount = _touchable.GetPressurePointCount();
        if (pressurePointCount + _lastCount == 0) return;
        _lastCount = pressurePointCount;

        // Find the largest pressure value
        var maxPressure = 0f;
        foreach (var point in _touchable.GetActivePressurePoints())
        {
            var pressure = point.Pressure.magnitude;
            if (pressure < maxPressure) continue;
            maxPressure = pressure;
        }
        
        // Debug.Log(maxPressure);
        
        // Update sphere material color
        _renderer.material.SetColor(Color1, new Color(maxPressure, 0f, 0f));
    }
}
