using UnityEngine;

public struct PressurePoint
{
    public readonly Vector3 Pressure;
    public readonly Vector3 Point;
    public readonly OVRBoneCapsule Capsule;

    public PressurePoint(in Vector3 pressure, in Vector3 point, in OVRBoneCapsule capsule)
    {
        Pressure = pressure;
        Point = point;
        Capsule = capsule;
    }
}
