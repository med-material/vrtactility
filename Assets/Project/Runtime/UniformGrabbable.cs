using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UniformGrabbable : MonoBehaviour
{
    // private OVRPlugin.Skeleton _leftHandSkeleton;   // These fields can 
    // private OVRPlugin.Skeleton _rightHandSkeleton;  // maybe be deleted
    // private List<OVRPlugin.BoneCapsule> _boneCapsules;
    [SerializeField] private OVRSkeleton _leftHandSkeleton;
    [SerializeField] private OVRSkeleton _rightHandSkeleton;
    private List<OVRBone> _bones;

    private void Start()
    {
        // OVRPlugin.GetSkeleton(OVRPlugin.SkeletonType.HandLeft, out _leftHandSkeleton);
        // OVRPlugin.GetSkeleton(OVRPlugin.SkeletonType.HandRight, out _rightHandSkeleton);
        // _boneCapsules = _leftHandSkeleton.BoneCapsules
        //     .Concat(_rightHandSkeleton.BoneCapsules)
        //     .ToList();
        _bones = _leftHandSkeleton.Bones
            .Concat(_rightHandSkeleton.Bones)
            .ToList();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Define the position of the colliding Bone Capsule
        var collisionPosition = collision.transform.position;
        
        // TODO: Find the OVRBone that best matches the colliding Bone Capsule
    }

    private void OnCollisionExit(Collision other)
    {
        // throw new NotImplementedException();
    }

    // private bool IsLeftHand(int index)
    // {
    //     return index <= _boneCapsules.Count / 2;
    // }
}
