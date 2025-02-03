using System.Collections;
using UnityEngine;

[System.Serializable]
public class ScanBasicData 
{
    public Vector3 ForwardRayOffset = new Vector3(0, 2.5f, 0);
    [field: SerializeField] public float ForwardRayLength { get; private set; } = 0.8f;
    [field: SerializeField][field : Range(5f, 25f)] public float HeightRayLength { get; private set; } = 5f;
    [field: SerializeField] public LayerMask scanLayer { get; private set; }
}
