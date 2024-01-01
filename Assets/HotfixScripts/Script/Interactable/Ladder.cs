using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    bool showGizmos = true;


    [Header("Exit points")]
    [SerializeField]
    Transform topReference = null;

    [SerializeField]
    Transform bottomReference = null;

    [Header("Properties")]

    [Min(0)]
    [SerializeField]
    int climbingAnimations = 1;

    [SerializeField]
    Vector3 bottomLocalPosition = Vector3.zero;

    [SerializeField]
    Direction facingDirection = Direction.Forward;


    public int ClimbingAnimations
    {
        get
        {
            return climbingAnimations;
        }
    }

    public Transform TopReference
    {
        get
        {
            return topReference;
        }
    }

    public Transform BottomReference
    {
        get
        {
            return bottomReference;
        }
    }


    public Vector3 FacingDirectionVector
    {
        get
        {
            Vector3 facingDirectionVector = transform.forward;

            switch (facingDirection)
            {
                case Direction.Left:

                    facingDirectionVector = -transform.right;
                    break;

                case Direction.Right:

                    facingDirectionVector = transform.right;
                    break;

                case Direction.Up:

                    facingDirectionVector = transform.up;
                    break;

                case Direction.Down:

                    facingDirectionVector = -transform.up;
                    break;

                case Direction.Forward:

                    facingDirectionVector = transform.forward;
                    break;

                case Direction.Back:

                    facingDirectionVector = -transform.forward;
                    break;

            }

            return facingDirectionVector;
        }
    }



    void Awake()
    {

    }


    void OnDrawGizmos()
    {
        if (!showGizmos)
            return;


        if (bottomReference != null)
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
            Gizmos.DrawCube(bottomReference.position, Vector3.one * 0.5f);
        }

        if (topReference != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawCube(topReference.position, Vector3.one * 0.5f);
        }

        CustomUtilities.DrawArrowGizmo(transform.position, transform.position + FacingDirectionVector, Color.blue);

        Gizmos.color = Color.white;
    }
}

