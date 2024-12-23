using UnityEngine;

[System.Serializable]
public class MovementReferenceParameters
{
    public MovementReferenceMode movementReferenceMode = MovementReferenceMode.External;

    public Transform externalReference = null;
    CharacterActor characterActor = null;
    public enum MovementReferenceMode
    {
        World,
        External,
        Character
    }

    public Vector3 InputMovementReference { get; private set; }
    public Vector3 MovementReferenceForward { get; private set; }
    public Vector3 MovementReferenceRight { get; private set; }

    Vector3 characterInitialForward;
    Vector3 characterInitialRight;

    public void Initialize(CharacterActor characterActor)
    {
        if (characterActor == null)
        {
            Debug.Log("CharacterActor is NULL");
            return;
        }
        this.characterActor = characterActor;
        characterInitialForward = characterActor.Forward;
        characterInitialRight = characterActor.Right;
    }

    public void UpdateData(Vector2 movementInput)
    {
        UpdateMovementReferenceData();

        Vector3 inputMovementReference = CustomUtilities.Multiply(MovementReferenceRight, movementInput.x) + CustomUtilities.Multiply(MovementReferenceForward, movementInput.y);
        InputMovementReference = Vector3.ClampMagnitude(inputMovementReference, 1f);
    }

    void UpdateMovementReferenceData()
    {
        switch (movementReferenceMode)
        {
            case MovementReferenceMode.World:
                MovementReferenceForward = Vector3.forward;
                MovementReferenceRight = Vector3.right;
                break;
            case MovementReferenceMode.External:
                if (externalReference != null)
                {
                    MovementReferenceForward = Vector3.Normalize(Vector3.ProjectOnPlane(externalReference.forward, characterActor.Up));
                    MovementReferenceRight = Vector3.Normalize(Vector3.ProjectOnPlane(externalReference.right, characterActor.Up));
                }
                else
                {
                    Debug.LogWarning("û��ָ��ǰ������");
                }
                break;
            case MovementReferenceMode.Character:
                MovementReferenceForward = characterInitialForward;
                MovementReferenceRight = characterInitialRight;
                break;
        }
    }
}
