using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public interface IHandParser
{
    public ref readonly List<OVRBone> GetBones();
    public ref readonly List<OVRBoneCapsule> GetBoneCapsules();
    [CanBeNull] public OVRBoneCapsule GetBoneFromCollision(in Collision collision);
    public OVRSkeleton.BoneId GetBoneId(in OVRBoneCapsule boneCapsule);
    public int GetBoneIndex(in OVRBoneCapsule boneCapsule);
    public bool IsIndexOnLeftHand(in int index);
    // public bool IsInitialized();
}
