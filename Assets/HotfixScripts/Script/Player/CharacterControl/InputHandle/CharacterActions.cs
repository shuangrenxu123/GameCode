using UnityEngine;

public struct CharacterActions
{
    public BoolAction @jump;
    public BoolAction @run;
    public BoolAction @interact;
    public BoolAction roll;
    public BoolAction @lock;
    public BoolAction @attack;
    public BoolAction @crouch;
    public Vector2Action @movement;


    public void Reset()
    {
        jump.Reset();
        run.Reset();
        interact.Reset();
        roll.Reset();
        movement.Reset();
        @lock.Reset();
        attack.Reset();
        crouch.Reset();
    }
    public void InitializeActions()
    {
        @jump = new BoolAction();
        jump.Initialize();

        run = new BoolAction();
        run.Initialize();

        interact = new BoolAction();
        interact.Initialize();

        roll = new BoolAction();
        roll.Initialize();

        @lock = new BoolAction();
        @lock.Initialize();

        attack = new BoolAction();
        attack.Initialize();

        crouch = new BoolAction();
        crouch.Initialize();

        movement = new Vector2Action();
    }
    public void Setvalue(CharacterActions characterActions)
    {
        @jump.value = characterActions.jump.value;
        @run.value = characterActions.run.value;
        @interact.value = characterActions.interact.value;
        roll.value = characterActions.roll.value;
        @movement.value = characterActions.movement.value;
        @lock.value = characterActions.@lock.value;
        attack.value = characterActions.attack.value;
        crouch.value = characterActions.crouch.value;
    }

    public void SetValues(InputHandler inputHandler)
    {
        if (inputHandler == null)
            return;

        @jump.value = inputHandler.GetBool("Jump");
        @run.value = inputHandler.GetBool("Run");
        @interact.value = inputHandler.GetBool("Interact");
        roll.value = inputHandler.GetBool("Roll");
        @lock.value = inputHandler.GetBool("Lock");
        @movement.value = inputHandler.GetVector2("Movement");
        attack.value = inputHandler.GetBool("Attack");
        crouch.value = inputHandler.GetBool("Crouch");

    }
    /// <summary>
    /// 用于记录按下时间等
    /// </summary>
    /// <param name="dt"></param>
    public void Update(float dt)
    {
        @jump.Update(dt);
        @run.Update(dt);
        @interact.Update(dt);
        roll.Update(dt);
        @lock.Update(dt);
        attack.Update(dt);
        crouch .Update(dt);
    }
}
[SerializeField]
public struct BoolAction
{
    public bool value;
    public bool Started { get; private set; }

    public bool Canceled { get; private set; }
    public float StartedElapsedTime { get; private set; }

    public float CanceledElapsedTime { get; private set; }

    public float ActiveTime { get; private set; }

    public float InactiveTime { get; private set; }

    public float LastActiveTime { get; private set; }

    public float LastInactiveTime { get; private set; }

    bool previousValue;
    bool previousStarted;
    bool previousCanceled;

    public void Initialize()
    {
        StartedElapsedTime = Mathf.Infinity;
        CanceledElapsedTime = Mathf.Infinity;

        value = false;
        previousValue = false;
        previousStarted = false;
        previousCanceled = false;
    }
    public void Reset()
    {
        Started = false;
        Canceled = false;
    }
    public void Update(float dt)
    {
        Started |= !previousValue && value;
        Canceled |= previousValue && !value;

        StartedElapsedTime += dt;
        CanceledElapsedTime += dt;

        if (Started)
        {
            StartedElapsedTime = 0f;

            if (!previousStarted)
            {
                LastActiveTime = 0f;
                LastInactiveTime = InactiveTime;
            }
        }

        if (Canceled)
        {
            CanceledElapsedTime = 0f;

            if (!previousCanceled)
            {
                LastActiveTime = ActiveTime;
                LastInactiveTime = 0f;
            }
        }


        if (value)
        {
            ActiveTime += dt;
            InactiveTime = 0f;
        }
        else
        {
            ActiveTime = 0f;
            InactiveTime += dt;
        }


        previousValue = value;
        previousStarted = Started;
        previousCanceled = Canceled;
    }

}
[SerializeField]
public struct FloatAction
{
    /// <summary>
    /// The action current value.
    /// </summary>
    public float value;

    /// <summary>
    /// Resets the action.
    /// </summary>
    public void Reset()
    {
        value = 0f;
    }
}
[SerializeField]
public struct Vector2Action
{
    /// <summary>
    /// The action current value.
    /// </summary>
    public Vector2 value;

    /// <summary>
    /// Resets the action
    /// </summary>
    public void Reset()
    {
        value = Vector2.zero;
    }


    /// <summary>
    /// Returns true if the value is not equal to zero (e.g. When pressing a D-pad)
    /// </summary>
    public bool Detected
    {
        get
        {
            return value != Vector2.zero;
        }
    }

    /// <summary>
    /// Returns true if the x component is positive.
    /// </summary>
    public bool Right
    {
        get
        {
            return value.x > 0;
        }
    }

    /// <summary>
    /// Returns true if the x component is negative.
    /// </summary>
    public bool Left
    {
        get
        {
            return value.x < 0;
        }
    }

    /// <summary>
    /// Returns true if the y component is positive.
    /// </summary>
    public bool Up
    {
        get
        {
            return value.y > 0;
        }
    }

    /// <summary>
    /// Returns true if the y component is negative.
    /// </summary>
    public bool Down
    {
        get
        {
            return value.y < 0;
        }
    }

}
