using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInitializer : MonoBehaviour
{
    public OVRSkeleton recSkeleton, rightHandSkeleton, leftHandSkeleton;
    public List<OVRBone> fingerBonesRec, fingerBonesRightH, fingerBonesLeftH;
    public List<OVRBoneCapsule> fingerCapsulesRightH, fingerCapsulesLeftH;

    public bool debugMode = false, isInitialized = false;


    // Delays the initiation of the VR hands until they are loaded properly.
    // Without this we cannot get the finger data.
    public IEnumerator DelayInitialize(Action actionToDo)
    {
        if (!debugMode)
        {
            while (!leftHandSkeleton.IsInitialized && !rightHandSkeleton.IsInitialized)
            {
                yield return null;
            }
            actionToDo.Invoke();
        }
        else if (debugMode)
        {
            while (!recSkeleton.IsInitialized)
            {
                yield return null;
            }
            actionToDo.Invoke();
        }

    }

    public void InitializeSkeleton()
    {
        if (!debugMode)
        {
            // Populate the public lists of fingerbones from both hands' OVRskeleton
            fingerBonesRightH = new List<OVRBone>(rightHandSkeleton.Bones);
            fingerBonesLeftH = new List<OVRBone>(leftHandSkeleton.Bones);
            
            fingerCapsulesLeftH = new List<OVRBoneCapsule>(leftHandSkeleton.Capsules);
            fingerCapsulesRightH = new List<OVRBoneCapsule>(rightHandSkeleton.Capsules);

            Debug.Log("Left and right hands are be initialized");
            isInitialized = true;
        }
        else
        {
            fingerBonesRec = new List<OVRBone>(recSkeleton.Bones);
            Debug.Log("Rechand is initialized");
            isInitialized = true;
        }
    }

    void Start()
    {
        StartCoroutine(DelayInitialize(InitializeSkeleton));
    }
}