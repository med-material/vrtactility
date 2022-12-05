using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TactilityManager : MonoBehaviour
{
    private List<PadScript.Pad> _pads;
    private UniformGrabbable _ug;

    private void Start()
    {
        _pads = ConnectDevice.Remap2.ToList();
        _ug = FindObjectOfType<UniformGrabbable>();
        ApplyStimuli(4, 0.5f);
    }

    private void Update()
    {
        for (var i = 0; i < _ug.touchingBoneIds.Count; i++)
        {
            var pressure = _ug.touchingBonePressures[i];
            switch (_ug.touchingBoneIds[i])
            {
                case OVRSkeleton.BoneId.Hand_Thumb3:
                    // ApplyStimuli(4, pressure);
                    break;
                case OVRSkeleton.BoneId.Hand_Index3:
                    break;
                case OVRSkeleton.BoneId.Hand_Middle3:
                    break;
                case OVRSkeleton.BoneId.Hand_Ring3:
                    break;
                case OVRSkeleton.BoneId.Hand_Pinky3:
                    break;
            }
        }
    }

    private void ApplyStimuli(int index, float pressure)
    {
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + ConnectDevice.GetPadsInfo(index).GetRemap() * pressure + "=C, *amp " + ConnectDevice.GetPadsInfo(index).GetRemap() * pressure + "=" + ConnectDevice.GetPadsInfo(index).GetAmplitude() * pressure + ", *width " + ConnectDevice.GetPadsInfo(index).GetRemap() * pressure + "=" + ConnectDevice.GetPadsInfo(index).GetPulseWidth() * pressure + ", *selected 1 *sync 0\r\n");
        Thread.Sleep(100);
    }
}
