using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    [field:SerializeField] public ScannerSO ScanData { get; private set; }

    public Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    public HitData ScanLayer()
    {
        var hitData = new HitData();

        var forwardOrigin = transform.position + ScanData.scanBasicData.ForwardRayOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, cam.transform.forward, out hitData.forwardHit,
            ScanData.scanBasicData.ForwardRayLength, ScanData.scanBasicData.scanLayer);

        Debug.DrawRay(forwardOrigin, cam.transform.forward * ScanData.scanBasicData.ForwardRayLength, (hitData.forwardHitFound)? Color.red : Color.green);

        if(hitData.forwardHitFound )
        {
            var forwardEndPoint = forwardOrigin + cam.transform.forward * ScanData.scanBasicData.ForwardRayLength;

            var heightOrigin = forwardEndPoint + Vector3.up * ScanData.scanBasicData.HeightRayLength;
            hitData.heightFound = Physics.Raycast(heightOrigin, Vector3.down,
                out hitData.heightHit, ScanData.scanBasicData.HeightRayLength + ScanData.scanBasicData.ForwardRayLength, ScanData.scanBasicData.scanLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * (ScanData.scanBasicData.HeightRayLength + ScanData.scanBasicData.ForwardRayLength), (hitData.heightFound) ? Color.red : Color.green);
        }

        #region Check Correct Normal
        Vector3 calcNormalVector = hitData.forwardHit.normal - hitData.heightHit.normal;
        hitData.foundHitwithCorrectionNormal = calcNormalVector.magnitude > 1;
        #endregion

        return hitData;
    }

    public struct HitData
    {
        public bool forwardHitFound;
        public bool heightFound;
        public bool foundHitwithCorrectionNormal;
        public RaycastHit forwardHit;
        public RaycastHit heightHit;
    }
}
