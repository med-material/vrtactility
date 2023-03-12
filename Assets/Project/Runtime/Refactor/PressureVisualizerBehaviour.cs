using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ITouchable))]
// [RequireComponent(typeof(IGrabbable))]
[RequireComponent(typeof(Renderer))]
public class PressureVisualizerBehaviour : MonoBehaviour
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    private ITouchable _touchable;
    // private IGrabbable _grabbable;
    private Renderer _renderer;
    
    private int _lastCount;  // The number of touching bones in the previous frame update

    private void Start()
    {
        _touchable = GetComponent<ITouchable>();
        // _grabbable = GetComponent<IGrabbable>();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        // Don't process highlight if no bones are touching, but remember to update once after the list empties
        var pressurePointCount = _touchable.GetPressurePointCount();
        if (pressurePointCount + _lastCount == 0) return;
        _lastCount = pressurePointCount;

        // Calculate total pressure being applied
        var maxPressure = pressurePointCount > 0
            ? _touchable.GetPressurePoints().Aggregate(0f, (current, p) =>
            {
                if (p.HasValue) return current + p.Value.Pressure.magnitude;
                return current;
            })
            : 0f;
        
        // Update sphere material color
        _renderer.material.SetColor(Color1, new Color(maxPressure, 0f, 0f));
    }
}
