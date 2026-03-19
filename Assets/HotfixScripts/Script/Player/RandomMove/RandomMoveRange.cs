using Character.Controller.State;
using HFSM;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;

public class RandomMoveRange : MonoBehaviour
{
    [Header("矩形范围对角点")]
    public Transform pointA;
    public Transform pointB;

    [Header("移动参数")]
    public float reachThreshold = 0.5f; // 判定"到达目标点"的距离阈值

    public float idleTime;

    public Vector3 currentTarget;
    private float minX, maxX, minZ, maxZ;

    private CharacterRandomMoveStateMachine stateMachine;
    private CharacterIdleState characterIdleState;
    private CharacterRandomMoveState characterRandomMoveState;

    private void Awake()
    {
        characterIdleState = new CharacterIdleState();
        characterRandomMoveState = new CharacterRandomMoveState();
        stateMachine = new CharacterRandomMoveStateMachine();

        characterIdleState.randomMoveRange = this;
        characterRandomMoveState.randomMoveRange = this;
        stateMachine.randomMoveRange = this;
        characterIdleState.idleDuration = 1f;
    }

    private void Start()
    {
        // 先计算一次矩形边界
        UpdateRectangleBounds();
        // 生成第一个随机目标点
        currentTarget = GetRandomPointInRectangle();

        stateMachine.Start();
    }

    private void Update()
    {
        stateMachine.Update();
    }

    // 更新矩形范围边界（只取X、Z）
    private void UpdateRectangleBounds()
    {
        if (pointA == null || pointB == null) return;

        Vector3 posA = pointA.position;
        Vector3 posB = pointB.position;
        minX = Mathf.Min(posA.x, posB.x);
        maxX = Mathf.Max(posA.x, posB.x);
        minZ = Mathf.Min(posA.z, posB.z);
        maxZ = Mathf.Max(posA.z, posB.z);
    }

    // 在矩形范围内生成一个随机点（保持当前Y高度）
    public Vector3 GetRandomPointInRectangle()
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        return new Vector3(randomX, transform.position.y, randomZ);
    }


    // 在Scene视图里预览目标点和矩形范围
    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;

        // 预览矩形范围
        UpdateRectangleBounds();
        float y = transform.position.y;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(minX, y, minZ), new Vector3(maxX, y, minZ));
        Gizmos.DrawLine(new Vector3(maxX, y, minZ), new Vector3(maxX, y, maxZ));
        Gizmos.DrawLine(new Vector3(maxX, y, maxZ), new Vector3(minX, y, maxZ));
        Gizmos.DrawLine(new Vector3(minX, y, maxZ), new Vector3(minX, y, minZ));

        // 预览当前目标点
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(currentTarget, 0.2f);
    }
}

