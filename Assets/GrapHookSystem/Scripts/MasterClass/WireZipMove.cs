using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireZipMove : MonoBehaviour
{
    public enum ZipState { Idle, WebPullAO, FlyForward, Land, Jump }

    // SO로 바꿀 것
    public Vector3 AdjustedZipPoint;
    public float Z_Boost;
    public Vector3 StartLocation;
    public Vector3 ZipToPointLastTick;
    public bool CanZip;
    public Quaternion ZipBeginRotation;  // 쿼터니언 또는 Vector3 사용할 것 분석 후 변경
    public Quaternion StartRotation;
    public Quaternion TargetRotation;
    public Vector3 LocationLastTick;
    public Vector3 ZipSomething;
    public Vector2 ZipAO;
    public ZipState zipState;
    public WireZipMove ClosestZipPoint; // 다른 클래스가 있다면 변경
    public bool FoundValidZipPoint;
    public Vector3 BestZipPoint;
    public float ZipLocationTimelineScalar;
    public AnimationCurve curveFloat;
    public Quaternion RotationYTarget;
    public bool ZipJumpWindow;
    public bool CanZipJump;
    public bool PerchBounce;

}
