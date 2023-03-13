using System.Collections.Generic;
using UnityEngine;

public struct OVRInstanceContainer
{
    public readonly Transform HeadTransform;
    public readonly OVRSkeleton LeftHandSkeleton;
    public readonly OVRSkeleton RightHandSkeleton;
    
    public readonly IList<OVRBone> LeftHandBones;
    public readonly IList<OVRBoneCapsule> LeftHandBoneCapsules;
    public readonly IList<OVRBone> RightHandBones;
    public readonly IList<OVRBoneCapsule> RightHandBoneCapsules;

    public OVRInstanceContainer(in Transform headTransform, in OVRSkeleton leftHandSkeleton, in OVRSkeleton rightHandSkeleton)
    {
        HeadTransform = headTransform;
        LeftHandSkeleton = leftHandSkeleton;
        RightHandSkeleton = rightHandSkeleton;

        LeftHandBones = leftHandSkeleton.Bones;
        LeftHandBoneCapsules = leftHandSkeleton.Capsules;
        RightHandBones = rightHandSkeleton.Bones;
        RightHandBoneCapsules = rightHandSkeleton.Capsules;
    }

    public bool IsPopulated() => HeadTransform != null;
}