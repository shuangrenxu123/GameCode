using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
/// <summary>
/// 为Rigdbody的封装
/// </summary>
public abstract class RigidbodyComponent : MonoBehaviour
{
    public abstract float Mass { get; set; }
    public abstract float LinerDrag { get; set; }
    public abstract float AngularDrag { get; set; }
    public abstract bool IsKinematic { get; set; }
    public abstract bool UseGravity { get; set; }
    public abstract bool UseInterpolation { get; set; }

    public abstract bool ContinuousCollisionDetection { get; set; }

    /// <summary>
    /// 刚体约束，即冻结xyz轴
    /// </summary>
    public abstract RigidbodyConstraints Constraints { get; set; }
    protected bool previousContinuousCollisionDetection = false;
    //是否启用CCD，即连续检测
    protected abstract bool IsUsingContinuousCollisionDetection { get; }
    //扫描
    public abstract HitInfo Sweep(Vector3 position, Vector3 direction, float distance);

    public event System.Action OnBodyTypeChange;

    protected void OnBodyTypeChangeInternal() => OnBodyTypeChange?.Invoke();

    public abstract Vector3 Position { get; set; }
    public abstract Quaternion Rotation { get; set; }
    public abstract Vector3 Velocity { get; set; }

    public abstract Vector3 AngularVelocity { get; set; }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
    /// <summary>
    /// 插值刚体位置（等效于移动位置）
    /// </summary>
    /// <param name="position"></param>
    public abstract void Interpolate(Vector3 position);
    /// <summary>
    /// 插值刚体旋转（等效于移动旋转）。
    /// </summary>
    /// <param name="rotation"></param>
    public abstract void Interpolate(Quaternion rotation);

    public void Interpolate(Vector3 position, Quaternion rotation)
    {
        Interpolate(position);
        Interpolate(rotation);
    }
    /// <summary>
    /// 根据刚体的类型自动移动刚体。如果刚体是不受物理交互的，则直接移动position。
    /// 如果刚体是动态的，则将设置速度。
    /// </summary>
    /// <param name="position"></param>
    public void Move(Vector3 position)
    {
        if (IsKinematic)
        {
            Interpolate(Position);
        }
        else
        {
            Velocity = (position - Position) / Time.deltaTime;
        }
    }

    public void Rotate(Quaternion rotation)
    {
        if (IsKinematic)
        {
            Interpolate(rotation);
        }
        else
        {
            Vector3 angularDisplacement = Mathf.Deg2Rad * (rotation * Quaternion.Inverse(Rotation)).eulerAngles;
            AngularVelocity = angularDisplacement / Time.deltaTime;
        }
    }

    public void MoveAndRotate(Vector3 position, Quaternion rotation)
    {
        Move(position);
        Rotate(rotation);
    }
    /// <summary>
    /// 获取空间中特定点的刚体速度。
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public abstract Vector3 GetPointVelocity(Vector3 point);

    public abstract void AddForceToRigidbody(Vector3 force, ForceMode forcemode = ForceMode.Force);
    /// <summary>
    /// 添加爆炸力.(估计是某个点产生的推力)
    /// </summary>
    /// <param name="explosionForce"></param>
    /// <param name="explosionPosition"></param>
    /// <param name="explosionRadius"></param>
    /// <param name="upwardsModeifier"></param>
    public abstract void AddExplosionForceToRigidbody(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModeifier = 0f);

    /// <summary>
    /// 将速度矢量添加到刚体（模拟 AddForce），根据力值计算得出。
    /// </summary>
    /// <param name="force"></param>
    /// <param name="ignoreMass"></param>
    /// <param name="useImpulse">是否是一个瞬时力</param>
    public void AddForce(Vector3 force, bool ignoreMass = false, bool useImpulse = false)
    {
        if (useImpulse)
        {
            Vector3 acceleration = force / (ignoreMass ? 1f : Mathf.Max(0.01f, Mass));
            Velocity += acceleration * Time.deltaTime;
        }
        else
        {
            Vector3 deltaVelocity = force / (ignoreMass ? 1f : MathF.Max(0.01f, Mass));
            Velocity += deltaVelocity;
        }
    }
    /// <summary>
    /// 将速度矢量添加到根据力值计算的刚体（模拟 AddExplosionForce）。
    /// </summary>
    /// <param name="explosionForce">爆炸中心的力大小</param>
    /// <param name="explosionPosition">爆炸中心</param>
    /// <param name="explosionRadius">爆炸半径</param>
    /// <param name="upwardsModeifier">向上的力</param>
    public void AddExplositonForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModeifier = 0f)
    {
        Vector3 displacementToTarget = Position - explosionPosition;
        float displacementToTargetMagnitude = displacementToTarget.magnitude;
        //如果我们不在力场中，则直接退出
        if (displacementToTargetMagnitude > explosionRadius)
        {
            return;
        }
        //-----------------------------------------------------------------未知
        explosionPosition -= Vector3.up * upwardsModeifier;
        //这里重新计算了一遍，应该不用
        displacementToTarget = Position - explosionPosition;
        //以中心的力为代表，然后根据距离递减
        float forceMagnitude = explosionForce * ((explosionRadius - displacementToTargetMagnitude) / explosionRadius);

        Vector3 force = Vector3.Normalize(displacementToTarget) * forceMagnitude;
        Vector3 velocity = force / Mathf.Max(0.01f, Mass);

        Velocity += velocity;
    }

    /// <summary>
    /// 将角速度矢量添加到刚体（模拟 AddTorque）中，该矢量是根据扭矩值计算的。
    /// </summary>
    /// <param name="torque"></param>
    /// <param name="ignoreMass"></param>
    public void AddTorque(Vector3 torque, bool ignoreMass = false)
    {
        Vector3 acceleration = torque / (ignoreMass ? 1f : Mathf.Max(0.01f, Mass));
        AngularVelocity += acceleration * Time.fixedDeltaTime;
    }
    public static RigidbodyComponent CreateInstance(GameObject gameObject)
    {
        Rigidbody rigidbody3D = gameObject.GetComponent<Rigidbody>();
        if (rigidbody3D != null)
            return gameObject.GetOrAddComponent<RigidbodyComponent3D>();


        return null;
    }
    protected virtual void Awake() { }
}

