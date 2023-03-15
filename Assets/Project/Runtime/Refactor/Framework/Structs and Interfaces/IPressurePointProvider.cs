using UnityEngine;

public interface IPressurePointProvider
{
    public PressurePoint GetPressurePoint(in Vector3 point, in OVRBoneCapsule capsule);
}
