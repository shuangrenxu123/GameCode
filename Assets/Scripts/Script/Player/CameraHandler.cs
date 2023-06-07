using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHandler : MonoBehaviour
{
    public Transform targetTransform;
    public Transform cameraTransform;
    public Transform cameraPivotTransform;
    private Transform myTransform;
    private Vector3 cameraTransformPosition;
    public LayerMask ignoreLayers;
    private LayerMask enviromentLayer;
    public float maxLockDistance = 30;

    public Player playerManager;
    public static CameraHandler singleton;
    public PlayerInputHandle InputHandle;
    /// <summary>
    /// 距离自己最近的目标
    /// </summary>
    public Transform nearestLockOnTarget;
    /// <summary>
    /// 要锁定的目标
    /// </summary>
    public Transform currentLockOnTarget;
    public Transform leftLockTarget;
    public Transform rightLockTarget;
    public float lookSpeed = 0.1f;
    public float groundFollowSpeed = 10f;//该值越小，相机跟随越快
    public float aerialFollowSpeed = 1f;
    public float pivotSpeed = 0.03f;
    public Vector3 cameraFollowSpeed = Vector3.zero;
    public List<CharacterManager> avilableTargets;
    private float targetPosition;
    private float defaultPosition;
    private float lookAngle;
    private float pivotAngle;
    public float minmumPivotAngle = -35;
    public float maxmumPivotAngle = 35;
    public float lockedPivotHeight = 2.25f;
    public float unlockedPivotHeight = 1.65f;
    public float cameraSphereRadius = 0.2f;
    public float cameraCollisionOffset = 0.2f;
    public float minimumCollisionOffset = 0.2f;
    private void Awake()
    {
        singleton = this;
        myTransform = transform;
        defaultPosition = cameraTransform.localPosition.z;
        ignoreLayers = ~(1 << 12 | 1<<13 | 1<<10 | 1 << 11);

        targetTransform = GameObject.Find("player").transform;
        playerManager = targetTransform.GetComponent<Player>();
    }
    private void Start()
    {
        enviromentLayer = LayerMask.NameToLayer("Enviroment");
        InputHandle = playerManager.inputHandle;
    }
    public void FollowTarget(float delta)
    {
        if (playerManager.isGrounded)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position,ref cameraFollowSpeed,groundFollowSpeed * Time.deltaTime);
            myTransform.position = targetPosition;
        }
        else 
        {
            Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowSpeed, aerialFollowSpeed * Time.deltaTime);
            myTransform.position = targetPosition;
        }

        HandleCameraCollisions(delta);
    }
    public void HandleCamerRotation(float delta,float mouseXInput,float mouseYInput)
    {
        if (InputHandle.LockFlag == false && currentLockOnTarget == null)
        {
            lookAngle += (mouseXInput * lookSpeed) / delta;
            pivotAngle -= (mouseYInput * pivotSpeed) / delta;
            pivotAngle = Mathf.Clamp(pivotAngle, minmumPivotAngle, maxmumPivotAngle);
            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;

            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;
            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation;
        }
        else
        {

            Vector3 dir = currentLockOnTarget.position - transform.position;
            dir.Normalize();
            dir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = targetRotation;

            dir = currentLockOnTarget.position - cameraPivotTransform.position;
            dir.Normalize();

            targetRotation = Quaternion.LookRotation(dir);
            Vector3 eulerAngle = targetRotation.eulerAngles;
            eulerAngle.y = 0;
            cameraPivotTransform.localEulerAngles= eulerAngle;
        }
    }

    private void HandleCameraCollisions(float delta)
    {
        targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
        direction.Normalize();
        if(Physics.SphereCast
            (cameraPivotTransform.position,cameraSphereRadius,direction,out hit, Mathf.Abs(targetPosition),ignoreLayers))
        {
            float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetPosition = -(dis - cameraCollisionOffset);
        }
        if(Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = -minimumCollisionOffset;
        }
        cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z,targetPosition,delta/0.2f);
        cameraTransform.localPosition = cameraTransformPosition; 
    }

    public bool HandleLockOn()
    {
        float shortTargetDistance           = Mathf.Infinity;
        float shortestDistanceOfLeftTarget  = Mathf.Infinity;
        float shortestDistanceOfRightTarget = Mathf.Infinity;


        Collider[] colliders = Physics.OverlapSphere(targetTransform.position,20);
        for (int i = 0; i < colliders.Length; i++)
        {
            CharacterManager character = colliders[i].GetComponent<CharacterManager>();
            if(character != null)
            {
                var lockTargetDirection = character.transform.position - targetTransform.position;
                float distanceFormTargetSqr = lockTargetDirection.sqrMagnitude;
                //求出我们看向敌人方向与摄像机的角度
                float viewableAngle = Vector3.Angle(lockTargetDirection, cameraTransform.forward);


                if(character.transform.root!= targetTransform.transform.root && viewableAngle >-60 
                    && viewableAngle < 60 && distanceFormTargetSqr <= maxLockDistance)
                {
                    if(Physics.Linecast(playerManager.LockOnTransform.position,character.LockOnTransform.position,out RaycastHit hit))
                    {
                        if(hit.transform.gameObject.layer != enviromentLayer)
                        {
                             avilableTargets.Add(character);
                        }
                    }
                }
            }
        }
        if (avilableTargets.Count == 0)
            return false; 
        for (int i = 0; i < avilableTargets.Count; i++)
        {
            if (avilableTargets[i].tag != "Hittable")
                continue;
            float distance = Vector3.Distance(targetTransform.position, avilableTargets[i].transform.position);
            if(distance < shortTargetDistance)
            {
                shortTargetDistance = distance;
                nearestLockOnTarget = avilableTargets[i].LockOnTransform;
            }

            if (InputHandle.LockFlag)
            {
                //下个敌人的世界坐标，以当前的选择目标为基准
                Vector3 relativeEnemyPosition = currentLockOnTarget.InverseTransformPoint(avilableTargets[i].transform.position);
                var distanceFormLeftTarget = currentLockOnTarget.transform.position.x - avilableTargets[i].transform.position.x;
                var distanceFormRightTarget = currentLockOnTarget.transform.position.x + avilableTargets[i].transform.position.x; 
                if(relativeEnemyPosition.x > 0.0 && distanceFormLeftTarget < shortestDistanceOfLeftTarget)
                {
                    shortestDistanceOfLeftTarget = distanceFormLeftTarget;
                    leftLockTarget = avilableTargets[i].LockOnTransform;
                }
                if(relativeEnemyPosition.x < 0 && distanceFormRightTarget < shortestDistanceOfRightTarget)
                {
                    shortestDistanceOfRightTarget = distanceFormRightTarget;
                    rightLockTarget = avilableTargets[i].LockOnTransform;
                }
            }
        }
        if (nearestLockOnTarget == null)
            return false;
        currentLockOnTarget = nearestLockOnTarget;
        return true;
    }

    public void ClearLockTargets()
    {
        avilableTargets.Clear();
        nearestLockOnTarget= null;
        currentLockOnTarget = null; 
    }

    public void SetCameraHeight()
    {
        Vector3 velocity = Vector3.zero;
        Vector3 newlockedPosition = new Vector3(0,lockedPivotHeight);
        Vector3 newUnlockedPosition = new Vector3(0,unlockedPivotHeight);
        if(currentLockOnTarget!= null)
        {
            cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(cameraPivotTransform.transform.localPosition,newlockedPosition,ref velocity,Time.deltaTime);
        }
        else
        {
            cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(cameraPivotTransform.transform.localPosition, newUnlockedPosition, ref velocity, Time.deltaTime);
        }
    }
}

