using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
/// <summary>
/// ΪRigdbody�ķ�װ
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
    /// ����Լ����������xyz��
    /// </summary>
    public abstract RigidbodyConstraints Constraints { get; set; }
    protected bool previousContinuousCollisionDetection = false;
    //�Ƿ�����CCD�����������
    protected abstract bool IsUsingContinuousCollisionDetection { get; }
    //ɨ��
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
    /// ��ֵ����λ�ã���Ч���ƶ�λ�ã�
    /// </summary>
    /// <param name="position"></param>
    public abstract void Interpolate(Vector3 position);
    /// <summary>
    /// ��ֵ������ת����Ч���ƶ���ת����
    /// </summary>
    /// <param name="rotation"></param>
    public abstract void Interpolate(Quaternion rotation);

    public void Interpolate(Vector3 position, Quaternion rotation)
    {
        Interpolate(position);
        Interpolate(rotation);
    }
    /// <summary>
    /// ���ݸ���������Զ��ƶ����塣��������ǲ����������ģ���ֱ���ƶ�position��
    /// ��������Ƕ�̬�ģ��������ٶȡ�
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
    /// ��ȡ�ռ����ض���ĸ����ٶȡ�
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public abstract Vector3 GetPointVelocity(Vector3 point);

    public abstract void AddForceToRigidbody(Vector3 force, ForceMode forcemode = ForceMode.Force);
    /// <summary>
    /// ��ӱ�ը��.(������ĳ�������������)
    /// </summary>
    /// <param name="explosionForce"></param>
    /// <param name="explosionPosition"></param>
    /// <param name="explosionRadius"></param>
    /// <param name="upwardsModeifier"></param>
    public abstract void AddExplosionForceToRigidbody(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModeifier = 0f);

    /// <summary>
    /// ���ٶ�ʸ����ӵ����壨ģ�� AddForce����������ֵ����ó���
    /// </summary>
    /// <param name="force"></param>
    /// <param name="ignoreMass"></param>
    /// <param name="useImpulse">�Ƿ���һ��˲ʱ��</param>
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
    /// ���ٶ�ʸ����ӵ�������ֵ����ĸ��壨ģ�� AddExplosionForce����
    /// </summary>
    /// <param name="explosionForce">��ը���ĵ�����С</param>
    /// <param name="explosionPosition">��ը����</param>
    /// <param name="explosionRadius">��ը�뾶</param>
    /// <param name="upwardsModeifier">���ϵ���</param>
    public void AddExplositonForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModeifier = 0f)
    {
        Vector3 displacementToTarget = Position - explosionPosition;
        float displacementToTargetMagnitude = displacementToTarget.magnitude;
        //������ǲ��������У���ֱ���˳�
        if (displacementToTargetMagnitude > explosionRadius)
        {
            return;
        }
        //-----------------------------------------------------------------δ֪
        explosionPosition -= Vector3.up * upwardsModeifier;
        //�������¼�����һ�飬Ӧ�ò���
        displacementToTarget = Position - explosionPosition;
        //�����ĵ���Ϊ����Ȼ����ݾ���ݼ�
        float forceMagnitude = explosionForce * ((explosionRadius - displacementToTargetMagnitude) / explosionRadius);

        Vector3 force = Vector3.Normalize(displacementToTarget) * forceMagnitude;
        Vector3 velocity = force / Mathf.Max(0.01f, Mass);

        Velocity += velocity;
    }

    /// <summary>
    /// �����ٶ�ʸ����ӵ����壨ģ�� AddTorque���У���ʸ���Ǹ���Ť��ֵ����ġ�
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

