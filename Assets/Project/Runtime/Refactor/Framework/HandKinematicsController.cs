using UnityEngine;

public class HandKinematicsController : MonoBehaviour
{
    private ITouchable _touchable;
    private IHandParser _handParser;

    private PressurePoint?[] _previousPoints;
    private int _previousPointsCount;

    private void Start()
    {
        _handParser = GetComponent<IHandParser>();
        _touchable = GetComponent<ITouchable>();

        _previousPoints = _touchable.GetAllPressurePoints();
        _previousPointsCount = _touchable.GetPressurePointCount();
        
        SetIsKinematic(false);
    }

    private void Update()
    {
        var currentPoints = _touchable.GetAllPressurePoints();

        if (_previousPointsCount > 0 && _touchable.GetPressurePointCount() == 0)
        {
            SetIsKinematic(true);
            SetIsKinematic(false);
            return;
        }
        
        for (var i = 0; i < currentPoints.Length; i++)
        {
            if (!_previousPoints[i].HasValue || currentPoints[i].HasValue) continue;

            var capsule = _previousPoints[i]!.Value.Capsule;
            SetIsKinematic(in capsule, true);
            SetIsKinematic(in capsule, false);
        }

        _previousPoints = currentPoints;
        _previousPointsCount = _touchable.GetPressurePointCount();
    }

    private void SetIsKinematic(bool state)
    {
        // Loop through all Rigidbodies on all OVRBoneCapsules and update their state
        foreach (var boneCapsule in _handParser.GetBoneCapsules())
            SetIsKinematic(in boneCapsule, state);
    }
    
    private static void SetIsKinematic(in OVRBoneCapsule capsule, bool state)
    {
        capsule.CapsuleRigidbody.isKinematic = state;
    }
}
