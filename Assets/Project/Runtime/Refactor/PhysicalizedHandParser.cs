using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicalizedHandParser : MonoBehaviour, IHandParser
{
    private const float MatchingThreshold = 0.001f;

    private List<OVRBone> _bones;
    private List<OVRBoneCapsule> _boneCapsules;

    private void Start()
    {
        var instance = OVRInitializer.GetOVRInstanceContainer();

        // Wait for OVR to initialize
        // while (!instance.IsPopulated()) yield return null;

        // Import OVR Bones
        _bones = instance.LeftHandBones
            .Concat(instance.RightHandBones)
            .ToList();
        
        // Import OVR Bone Capsules
        _boneCapsules = instance.LeftHandBoneCapsules
            .Concat(instance.RightHandBoneCapsules)
            .ToList();
        
        // NOTE: Initial disabling of kinematics is now done in PhysicalizedTouchable
    }

    public ref readonly List<OVRBone> GetBones()
    {
        return ref _bones;
    }

    public ref readonly List<OVRBoneCapsule> GetBoneCapsules()
    {
        return ref _boneCapsules;
    }
    
    public OVRBoneCapsule GetBoneFromCollision(in Collision collision)  // ref OVRBoneCapsule resultCapsule
    {
        // Find the OVRBone that best matches the colliding object
        var smallestDistance = float.MaxValue;
        var matchingBoneIndex = -1;
        for (var i = 0; i < GetBoneCapsules().Count; i++)
        {
            var bone = GetBoneCapsules()[i];
            var distance = Vector3.Distance(bone.CapsuleCollider.transform.position, collision.transform.position);
            if (distance > MatchingThreshold || distance > smallestDistance) continue;  // NOTE: MatchingThreshold check may be redundant

            matchingBoneIndex = i;
            smallestDistance = distance;
        }

        if (matchingBoneIndex == -1) return null;

        var boneCapsule = GetBoneCapsules()[matchingBoneIndex];
        return boneCapsule;
    }

    public OVRSkeleton.BoneId GetBoneId(in OVRBoneCapsule boneCapsule)
    {
        // Raise an error if the provided BoneIndex is for a body part and not just the hands
        if (boneCapsule.BoneIndex > 18)
            throw new IndexOutOfRangeException(
                "Error trying to process collision with unsupported OVR body part");
        
        return GetBones()[boneCapsule.BoneIndex].Id;
    }

    public int GetBoneIndex(in OVRBoneCapsule boneCapsule)
    {
        // Find out what hand the boneCapsule belongs to in order to index properly
        var isLeftHand = GetBoneCapsules().IndexOf(boneCapsule) < GetBoneCapsules().Count / 2;
        var indexOffset = isLeftHand ? 0 : GetBones().Count / 2;

        return indexOffset + boneCapsule.BoneIndex;
    }

    public bool IsIndexOnLeftHand(in int index)
    {
        // Raise an error if the provided index is out of range
        if (index < 0 || index >= GetBones().Count)
            throw new IndexOutOfRangeException(
                "Parameter cannot be greater than the number of bone capsules or less than zero");
        return index < GetBones().Count / 2;
    }

    // public bool IsInitialized()
    // {
    //     return GetBones() != null;
    // }
}
