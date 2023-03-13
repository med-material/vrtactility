using System.Collections;
using UnityEngine;

public class OVRInitializer : MonoBehaviour
{
    public OVRInitializeEvent onInitialized = new OVRInitializeEvent();
    
    [SerializeField] private Transform headTransform;
    [SerializeField] private OVRSkeleton leftHandSkeleton, rightHandSkeleton;

    private static OVRInstanceContainer _instanceContainer;

    protected IEnumerator Start()
    {
        // Busy-wait until both hands are initialized
        while (!leftHandSkeleton.IsInitialized && !rightHandSkeleton.IsInitialized)
            yield return null;

        // Cache populated instance container
        _instanceContainer = new OVRInstanceContainer(in headTransform, in leftHandSkeleton, in rightHandSkeleton);
        
        // Invoke initialization event
        onInitialized.Invoke(GetOVRInstanceContainer());
        Debug.Log("OVR initialization complete");
    }
    
    public static OVRInstanceContainer GetOVRInstanceContainer()
    {
        return _instanceContainer;
    }
}