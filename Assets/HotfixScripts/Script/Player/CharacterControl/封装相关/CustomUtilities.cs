using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Right,
    Left,
    Up,
    Down,
    Forward,
    Back
}

public static class CustomUtilities
{

    public static Vector3 Add(Vector3 vectorA, Vector3 vectorB)
    {
        vectorA.x += vectorB.x;
        vectorA.y += vectorB.y;

        return vectorA;
    }

    public static Vector3 Substract(Vector3 vectorA, Vector3 vectorB)
    {
        vectorA.x -= vectorB.x;
        vectorA.y -= vectorB.y;

        return vectorA;
    }

    public static Vector3 Multiply(Vector3 vectorValue, float floatValue)
    {
        vectorValue.x *= floatValue;
        vectorValue.y *= floatValue;
        vectorValue.z *= floatValue;

        return vectorValue;

    }

    public static Vector3 Multiply(Vector3 vectorValue, float floatValueA, float floatValueB)
    {
        vectorValue.x *= floatValueA * floatValueB;
        vectorValue.y *= floatValueA * floatValueB;
        vectorValue.z *= floatValueA * floatValueB;

        return vectorValue;

    }

    public static void AddMagnitude(ref Vector3 vector, float magnitude)
    {
        if (vector == Vector3.zero)
            return;

        float vectorMagnitude = Vector3.Magnitude(vector);
        Vector3 vectorDirection = vector / vectorMagnitude;

        vector += vectorDirection * magnitude;
    }

    public static void ChangeMagnitude(ref Vector3 vector, float magnitude)
    {
        if (vector == Vector3.zero)
            return;

        Vector3 vectorDirection = Vector3.Normalize(vector);
        vector = vectorDirection * magnitude;
    }

    public static void ChangeDirection(ref Vector3 vector, Vector3 direction)
    {
        if (vector == Vector3.zero)
            return;

        float vectorMagnitude = Vector3.Magnitude(vector);
        vector = direction * vectorMagnitude;
    }

    public static void ChangeDirectionOntoPlane(ref Vector3 vector, Vector3 planeNormal)
    {
        if (vector == Vector3.zero)
            return;

        Vector3 direction = Vector3.Normalize(Vector3.ProjectOnPlane(vector, planeNormal));
        float vectorMagnitude = Vector3.Magnitude(vector);
        vector = direction * vectorMagnitude;
    }

    public static void GetMagnitudeAndDirection(this Vector3 vector, out Vector3 direction, out float magnitude)
    {
        magnitude = Vector3.Magnitude(vector);
        direction = Vector3.Normalize(vector);
    }

    /// <summary>
    /// Projects an input vector onto the tangent of a given plane (defined by its normal).
    /// </summary>
    public static Vector3 ProjectOnTangent(Vector3 inputVector, Vector3 planeNormal, Vector3 up)
    {
        Vector3 inputVectorDirection = Vector3.Normalize(inputVector);

        if (inputVectorDirection == -up)
        {
            inputVector += planeNormal * 0.01f;
        }
        else if (inputVectorDirection == up)
        {
            return Vector3.zero;
        }

        Vector3 rotationAxis = GetPerpendicularDirection(inputVector, up);
        Vector3 tangent = GetPerpendicularDirection(planeNormal, rotationAxis);

        return Multiply(tangent, Vector3.Magnitude(inputVector));
    }

    /// <summary>
    /// Projects an input vector onto plane A and plane B orthonormal direction.
    /// </summary>
    public static Vector3 DeflectVector(Vector3 inputVector, Vector3 planeA, Vector3 planeB, bool maintainMagnitude = false)
    {
        Vector3 direction = GetPerpendicularDirection(planeA, planeB);

        if (maintainMagnitude)
            return direction * inputVector.magnitude;
        else
            return Vector3.Project(inputVector, direction);
    }

    public static Vector3 GetPerpendicularDirection(Vector3 vectorA, Vector3 vectorB)
    {
        return Vector3.Normalize(Vector3.Cross(vectorA, vectorB));
    }

    public static float GetTriangleValue(float center, float height, float width, float independentVariable, float minIndependentVariableLimit = Mathf.NegativeInfinity, float maxIndependentVariableLimit = Mathf.Infinity)
    {
        float minValue = center - width / 2f;
        float maxValue = center + width / 2f;

        if (independentVariable < minValue || independentVariable > maxValue)
        {
            return 0f;
        }
        else if (independentVariable < center)
        {
            return height * (independentVariable - minValue) / (center - minValue);
        }
        else
        {
            return -height * (independentVariable - center) / (maxValue - center) + height;
        }
    }


    /// <summary>
    /// Makes a value greater than or equal to zero (default value).
    /// </summary>
    public static void SetPositive<T>(ref T value) where T : System.IComparable<T>
    {
        SetMin<T>(ref value, default(T));
    }

    /// <summary>
    /// Makes a value less than or equal to zero (default value).
    /// </summary>
    public static void SetNegative<T>(ref T value) where T : System.IComparable<T>
    {
        SetMax<T>(ref value, default(T));
    }

    /// <summary>
    /// Makes a value greater than or equal to a minimum value.
    /// </summary>
    public static void SetMin<T>(ref T value, T minValue) where T : System.IComparable<T>
    {
        bool isLess = value.CompareTo(minValue) < 0;

        if (isLess)
            value = minValue;
    }

    /// <summary>
    /// Makes a value less than or equal to a maximum value.
    /// </summary>
    public static void SetMax<T>(ref T value, T maxValue) where T : System.IComparable<T>
    {
        bool isGreater = value.CompareTo(maxValue) > 0;

        if (isGreater)
            value = maxValue;
    }

    /// <summary>
    /// Limits a value range from a minimum value to a maximum value (similar to Mathf.Clamp).
    /// </summary>
    public static void SetRange<T>(ref T value, T minValue, T maxValue) where T : System.IComparable<T>
    {
        SetMin<T>(ref value, minValue);
        SetMax<T>(ref value, maxValue);
    }


    /// <summary>
    /// Returns true if the target value is between a and b ( both exclusive ). 
    /// To include the limits values set the "inclusive" parameter to true.
    /// </summary>
    public static bool isBetween(float target, float a, float b, bool inclusive = false)
    {

        if (b > a)
            return (inclusive ? target >= a : target > a) && (inclusive ? target <= b : target < b);
        else
            return (inclusive ? target >= b : target > b) && (inclusive ? target <= a : target < a);
    }

    /// <summary>
    /// Returns true if the target value is between a and b ( both exclusive ). 
    /// To include the limits values set the "inclusive" parameter to true.
    /// </summary>
    public static bool isBetween(int target, int a, int b, bool inclusive = false)
    {

        if (b > a)
            return (inclusive ? target >= a : target > a) && (inclusive ? target <= b : target < b);
        else
            return (inclusive ? target >= b : target > b) && (inclusive ? target <= a : target < a);

    }


    public static bool isCloseTo(Vector3 input, Vector3 target, float tolerance)
    {
        return Vector3.Distance(input, target) <= tolerance;

    }

    public static bool isCloseTo(float input, float target, float tolerance)
    {
        return Mathf.Abs(target - input) <= tolerance;
    }

    public static Vector3 TransformVectorUnscaled(this Transform transform, Vector3 vector)
    {
        return transform.rotation * vector;
    }

    public static Vector3 InverseTransformVectorUnscaled(this Transform transform, Vector3 vector)
    {
        return Quaternion.Inverse(transform.rotation) * vector;
    }

    public static Vector3 RotatePointAround(Vector3 point, Vector3 center, float angle, Vector3 axis)
    {

        Quaternion rotation = Quaternion.AngleAxis(angle, axis);

        Vector3 pointToCenter = center - point;

        Vector3 rotatedPointToCenter = rotation * pointToCenter;

        return center - rotatedPointToCenter;


    }



    public static T GetOrAddComponent<T>(this GameObject targetGameObject, bool includeChildren = false) where T : Component
    {
        T existingComponent = includeChildren ? targetGameObject.GetComponentInChildren<T>() : targetGameObject.GetComponent<T>();
        if (existingComponent != null)
        {
            return existingComponent;
        }

        T component = targetGameObject.AddComponent<T>();

        return component;
    }

    /// <summary>
    /// Gets a "target" component within a particular branch (inside the hierarchy). The branch is defined by the "branch root object", which is also defined by the chosen 
    /// "branch root component". The returned component must come from a child of the "branch root object".
    /// </summary>
    /// <param name="callerComponent"></param>
    /// <param name="includeInactive">Include inactive objects?</param>
    /// <typeparam name="T1">Branch root component type.</typeparam>
    /// <typeparam name="T2">Target component type.</typeparam>
    /// <returns>The target component.</returns>
    public static T2 GetComponentInBranch<T1, T2>(this Component callerComponent, bool includeInactive = true) where T1 : Component where T2 : Component
    {
        T1[] rootComponents = callerComponent.transform.root.GetComponentsInChildren<T1>(includeInactive);

        if (rootComponents.Length == 0)
        {
            Debug.LogWarning($"Root component: No objects found with {typeof(T1).Name} component");
            return null;
        }

        for (int i = 0; i < rootComponents.Length; i++)
        {
            T1 rootComponent = rootComponents[i];

            // Is the caller a child of this root?
            if (!callerComponent.transform.IsChildOf(rootComponent.transform) && !rootComponent.transform.IsChildOf(callerComponent.transform))
                continue;

            T2 targetComponent = rootComponent.GetComponentInChildren<T2>(includeInactive);

            if (targetComponent == null)
                continue;

            return targetComponent;

        }

        return null;

    }

    /// <summary>
    /// Gets a "target" component within a particular branch (inside the hierarchy). The branch is defined by the "branch root object", which is also defined by the chosen 
    /// "branch root component". The returned component must come from a child of the "branch root object".
    /// </summary>
    /// <param name="callerComponent"></param>
    /// <param name="includeInactive">Include inactive objects?</param>
    /// <typeparam name="T1">Target component type.</typeparam>	
    /// <returns>The target component.</returns>
    public static T1 GetComponentInBranch<T1>(this Component callerComponent, bool includeInactive = true) where T1 : Component
    {
        return callerComponent.GetComponentInBranch<T1, T1>(includeInactive);
    }



    public static bool IsNullOrEmpty(this string target)
    {
        return target == null || target.Length == 0;
    }

    public static bool IsNullOrWhiteSpace(this string target)
    {
        if (target == null)
            return true;

        for (int i = 0; i < target.Length; i++)
        {
            if (target[i] != ' ')
                return false;
        }

        return true;
    }



    public static string Between(this string targetString, string firstString, string lastString)
    {
        int start = targetString.IndexOf(firstString) + firstString.Length;
        int end = targetString.IndexOf(lastString);

        if (end - start < 0)
            return "";

        return targetString.Substring(start, end - start);
    }

    public static bool BelongsToLayerMask(int layer, int layerMask)
    {
        return (layerMask & (1 << layer)) > 0;
    }

    public static T1 GetOrAddComponent<T1>(this GameObject gameObject) where T1 : Component
    {
        if (!gameObject.TryGetComponent(out T1 component))
            component = gameObject.AddComponent<T1>();

        return component;
    }

    public static T1 GetOrAddComponent<T1, T2>(this GameObject gameObject) where T1 : Component where T2 : Component
    {
        if (!gameObject.TryGetComponent(out T1 component))
        {
            if (gameObject.TryGetComponent(out T2 requiredComponent))
                component = gameObject.AddComponent<T1>();
        }

        return component;
    }

    public static T1 GetOrAddComponent<T1>(this Component baseComponent) where T1 : Component
    {
        if (!baseComponent.TryGetComponent(out T1 component))
            component = baseComponent.gameObject.AddComponent<T1>();

        return component;
    }

    public static T1 GetOrAddComponent<T1, T2>(this Component baseComponent) where T1 : Component where T2 : Component
    {
        if (!baseComponent.TryGetComponent(out T1 component))
        {
            if (baseComponent.TryGetComponent(out T2 requiredComponent))
                component = baseComponent.gameObject.AddComponent<T1>();
        }

        return component;
    }

    public static T1 GetOrAddComponent<T1, T2>(this Component baseComponent, T2 requiredComponentType) where T1 : Component where T2 : System.Type
    {
        if (!baseComponent.TryGetComponent(out T1 component))
        {
            if (baseComponent.TryGetComponent(out T2 requiredComponent))
                component = (T1)baseComponent.gameObject.AddComponent(requiredComponentType);
        }

        return component;
    }

    /// <summary>
    /// Gets a particular value from this dictionary. If the value isn't there, it gets added.
    /// </summary>
    /// <typeparam name="T1">Key type.</typeparam>
    /// <typeparam name="T2">Value type.</typeparam>
    /// <param name="dictionary">A dictionary container.</param>
    /// <param name="key">The key parameter</param>
    /// <param name="addIfNull">Indicates if the component should be added if it doesn't exist.</param>
    /// <returns></returns>
    public static T2 GetOrRegisterValue<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, bool addIfNull = false) where T1 : Component where T2 : Component
    {
        if (key == null)
            return null;

        bool found = dictionary.TryGetValue(key, out T2 value);

        if (!found)
        {
            value = addIfNull ? key.gameObject.GetOrAddComponent<T2>() : key.GetComponent<T2>();

            if (value != null)
                dictionary.Add(key, value);
        }

        return value;
    }

    /// <summary>
    /// Gets a particular value from this dictionary. If the value isn't there, it gets added.
    /// </summary>
    /// <typeparam name="T1">Key type.</typeparam>
    /// <typeparam name="T2">Value type.</typeparam>
    /// <typeparam name="T3">Required component type.</typeparam>
    /// <param name="dictionary">A dictionary container.</param>
    /// <param name="key">The key parameter</param>
    /// <param name="addIfNull">Indicates if the component should be added if it doesn't exist and the 
    /// required component exist.</param>
    /// <returns></returns>
    public static T2 GetOrRegisterValue<T1, T2, T3>(this Dictionary<T1, T2> dictionary, T1 key, bool addIfNull = false) where T1 : Component where T2 : Component where T3 : Component
    {
        if (key == null)
            return null;

        bool found = dictionary.TryGetValue(key, out T2 value);

        if (!found)
        {
            value = addIfNull ? key.gameObject.GetOrAddComponent<T2, T3>() : key.GetComponent<T2>();

            if (value != null)
                dictionary.Add(key, value);
        }

        return value;
    }


    public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
    {
        float angle = Vector3.Angle(from, to);
        Vector3 cross = Vector3.Cross(from, to);
        cross.Normalize();

        float sign = cross == axis ? 1f : -1f;

        return sign * angle;

    }

    public static void DebugRay(Vector3 point, Vector3 direction = default(Vector3), float duration = 2f, Color color = default(Color))
    {
        Vector3 drawDirection = direction == default(Vector3) ? Vector3.up : direction;
        Color drawColor = color == default(Color) ? Color.blue : color;

        Debug.DrawRay(point, drawDirection, drawColor, duration);
    }

    public static void DrawArrowGizmo(Vector3 start, Vector3 end, Color color, float radius = 0.25f)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);

        Gizmos.DrawRay(
            end,
            Quaternion.AngleAxis(45, Vector3.forward) * Vector3.Normalize(start - end) * radius
        );

        Gizmos.DrawRay(
            end,
            Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.Normalize(start - end) * radius
        );
    }

    public static void DrawGizmoCross(Vector3 point, float radius, Color color)
    {
        Gizmos.color = color;

        Gizmos.DrawRay(
            point + Vector3.up * 0.5f * radius,
            Vector3.down * radius
        );

        Gizmos.DrawRay(
            point + Vector3.right * 0.5f * radius,
            Vector3.left * radius
        );
    }

    public static void DrawDebugCross(Vector3 point, float radius, Color color, float angleOffset = 0f)
    {

        Debug.DrawRay(
            point + Quaternion.Euler(0, 0, angleOffset) * Vector3.up * 0.5f * radius,
            Quaternion.Euler(0, 0, angleOffset) * Vector3.down * radius,
            color
        );

        Debug.DrawRay(
            point + Quaternion.Euler(0, 0, angleOffset) * Vector3.right * 0.5f * radius,
            Quaternion.Euler(0, 0, angleOffset) * Vector3.left * radius,
            color
        );
    }



    #region Animator

    /// <summary>
    /// Gets the current clip effective length, that is, the original length divided by the playback speed. The length value is always positive, regardless of the speed sign. 
    /// It returns false if the clip is not valid.
    /// </summary>
    public static bool GetCurrentClipLength(this Animator animator, ref float length)
    {
        if (animator.runtimeAnimatorController == null)
            return false;

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        if (clipInfo.Length == 0)
            return false;


        float clipLength = clipInfo[0].clip.length;
        float speed = animator.GetCurrentAnimatorStateInfo(0).speed;


        length = Mathf.Abs(clipLength / speed);

        return true;
    }

    public static bool MatchTarget(this Animator animator, Vector3 targetPosition, Quaternion targetRotation, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
    {
        if (animator.runtimeAnimatorController == null)
            return false;

        if (animator.isMatchingTarget)
            return false;

        if (animator.IsInTransition(0))
            return false;

        MatchTargetWeightMask weightMask = new MatchTargetWeightMask(Vector3.one, 1f);

        animator.MatchTarget(
            targetPosition,
            targetRotation,
            avatarTarget,
            weightMask,
            startNormalizedTime,
            targetNormalizedTime
        );


        return true;
    }

    public static bool MatchTarget(this Animator animator, Vector3 targetPosition, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
    {
        if (animator.runtimeAnimatorController == null)
            return false;

        if (animator.isMatchingTarget)
            return false;

        if (animator.IsInTransition(0))
            return false;

        MatchTargetWeightMask weightMask = new MatchTargetWeightMask(Vector3.one, 0f);

        animator.MatchTarget(
            targetPosition,
            Quaternion.identity,
            avatarTarget,
            weightMask,
            startNormalizedTime,
            targetNormalizedTime
        );

        return true;
    }

    public static bool MatchTarget(this Animator animator, Transform target, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
    {
        if (animator.runtimeAnimatorController == null)
            return false;

        if (animator.isMatchingTarget)
            return false;

        if (animator.IsInTransition(0))
            return false;

        MatchTargetWeightMask weightMask = new MatchTargetWeightMask(Vector3.one, 1f);

        animator.MatchTarget(
            target.position,
            target.rotation,
            avatarTarget,
            weightMask,
            startNormalizedTime,
            targetNormalizedTime
        );


        return true;
    }

    public static bool MatchTarget(this Animator animator, Transform target, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime, MatchTargetWeightMask weightMask)
    {
        if (animator.runtimeAnimatorController == null)
            return false;

        if (animator.isMatchingTarget)
            return false;

        if (animator.IsInTransition(0))
            return false;

        animator.MatchTarget(
            target.position,
            target.rotation,
            AvatarTarget.Root,
            weightMask,
            startNormalizedTime,
            targetNormalizedTime
        );


        return true;
    }


    #endregion

}
