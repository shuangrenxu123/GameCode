using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StraightSwordControllerDemoScript : MonoBehaviour
{
    Animator animator;
    Rigidbody playerRigidBody;

    //UI VARAIBLES
    [Header("UI")]
    [SerializeField] Text currentAnimationBeingPlayed;

    //INPUT VARIABLES
    [Header("INPUTS")]
    [SerializeField] float verticalMovement;
    [SerializeField] float horizontalMovement;
    [SerializeField] float mouseX;
    [SerializeField] float mouseY;

    //PLAYER VARIABLES
    [Header("Player")]
    [SerializeField] bool isStrafing;
    [SerializeField] bool isSprinting;
    [SerializeField] bool isRunning;
    [SerializeField] bool isWalking;
    [SerializeField] bool isTwoHandingWeapon;
    [SerializeField] float rotationSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float runningSpeed;
    [SerializeField] float walkingSpeed;

    [Header("Player Debug")]
    [SerializeField] float moveAmount;
    [SerializeField] bool isPerformingAction;
    [SerializeField] bool isPerformingBackStep;
    Vector3 moveDirection;
    Vector3 planeNormal;

    //CAMERA VARIABLES
    [Header("Camera")]
    [SerializeField] float leftAndRightLookSpeed;
    [SerializeField] float upAndDownLookSpeed;
    [SerializeField] float minimumPivot;
    [SerializeField] float maximumPivot;
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject playerCameraPivot;
    [SerializeField] Camera cameraObject;
    [Header("Camera Debug")]
    [SerializeField] float leftandRightLookAngle;
    [SerializeField] float upAndDownLookAngle;
    Vector3 cameraFollowVelocity = Vector3.zero;

    //ATTACK VARIABLES
    [SerializeField] string attackLastPerformed;
    [SerializeField] bool lightAttackInput;
    [SerializeField] bool heavyAttackInput;
    [SerializeField] bool chargeAttackInput;

    [Header("Weapon")]
    public ParticleSystem weaponFX;
    public ParticleSystem weaponChargeFX;

    string oh_Light_Attack_01 = "OH_Light_Attack_01";
    string oh_Light_Attack_02 = "OH_Light_Attack_02";
    string oh_Heavy_Attack_01 = "OH_Heavy_Attack_01";
    string oh_Heavy_Attack_02 = "OH_Heavy_Attack_02";

    string th_Light_Attack_01 = "TH_Light_Attack_01";
    string th_Light_Attack_02 = "TH_Light_Attack_02";
    string th_Heavy_Attack_01 = "TH_Heavy_Attack_01";
    string th_Heavy_Attack_02 = "TH_Heavy_Attack_02";

    string oh_Charge_Attack_01 = "OH_Charge_Attack_01_Wind_Up";
    string oh_Charge_Attack_02 = "OH_Charge_Attack_02_Wind_Up";
    string th_Charge_Attack_01 = "TH_Charge_Attack_01_Wind_Up";
    string th_Charge_Attack_02 = "TH_Charge_Attack_02_Wind_Up";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerRigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInputs();
        UpdateAnimatorParameters();
        isPerformingAction = animator.GetBool("isPerformingAction");
        isPerformingBackStep = animator.GetBool("isPerformingBackStep");
    }

    private void FixedUpdate()
    {
        if (!isPerformingAction)
        {
            HandleAllPlayerLocomotion();
        }
    }

    private void LateUpdate()
    {
        HandleCameraActions();
    }

    private void UpdateAnimatorParameters()
    {
        float snappedVertical;
        float snappedHorizontal;

        #region Horizontal
        //This if chain will round the horizontal movement to -1, -0.5, 0, 0.5 or 1

        if (horizontalMovement > 0 && horizontalMovement <= 0.5f)
        {
            snappedHorizontal = 0.5f;
        }
        else if (horizontalMovement > 0.5f)
        {
            snappedHorizontal = 1;
        }
        else if (horizontalMovement < 0 && horizontalMovement >= -0.5f)
        {
            snappedHorizontal = -0.5f;
        }
        else if (horizontalMovement < -0.5f)
        {
            snappedHorizontal = -1;
        }
        else
        {
            snappedHorizontal = 0;
        }

        #endregion

        #region Vertical
        //This if chain will round the vertical movement to -1, -0.5, 0, 0.5 or 1

        if (verticalMovement > 0 && verticalMovement <= 0.5f)
        {
            snappedVertical = 0.5f;
        }
        else if (verticalMovement > 0.5f)
        {
            snappedVertical = 1;
        }
        else if (verticalMovement < 0 && verticalMovement >= -0.5f)
        {
            snappedVertical = -0.5f;
        }
        else if (verticalMovement < -0.5f)
        {
            snappedVertical = -1;
        }
        else
        {
            snappedVertical = 0;
        }

        #endregion

        if (isSprinting)
        {
            isStrafing = false;
            animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);
            animator.SetFloat("Vertical", 2, 0.2f, Time.deltaTime);
        }
        else
        {
            if (isStrafing)
            {
                if (isWalking)
                {
                    animator.SetFloat("Vertical", snappedVertical / 2, 0.2f, Time.deltaTime);
                    animator.SetFloat("Horizontal", snappedHorizontal / 2, 0.2f, Time.deltaTime);
                }
                else
                {
                    animator.SetFloat("Vertical", snappedVertical, 0.2f, Time.deltaTime);
                    animator.SetFloat("Horizontal", snappedHorizontal, 0.2f, Time.deltaTime);
                }
            }
            else
            {
                if (isWalking)
                {
                    animator.SetFloat("Vertical", moveAmount / 2, 0.2f, Time.deltaTime);
                    animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);
                }
                else
                {
                    animator.SetFloat("Vertical", moveAmount, 0.2f, Time.deltaTime);
                    animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);
                }
            }
        }

        if (moveAmount == 0)
        {
            animator.SetFloat("Vertical", 0, 0.2f, Time.deltaTime);
            animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);
        }
    }

    private void HandleInputs()
    {
        HandleLightAttackCombo();
        HandleHeavyAttackCombo();
        HandleChargeAttackCombo();
        HandleBackStepAttack();

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        HandleBackStep();
        HandleSprint();
        HandleWalkOrRun();
        HandleStrafe();
        HandleTwoHand();
        HandleLightAttack();
        HandleHeavyAttack();
        HandleChargeAttack();

        if (Input.GetKey(KeyCode.W))
        {
            verticalMovement = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            verticalMovement = -1;
        }
        else
        {
            verticalMovement = 0;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontalMovement = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            horizontalMovement = -1;
        }
        else
        {
            horizontalMovement = 0;
        }

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMovement) + Mathf.Abs(verticalMovement));
    }

    private void HandleAllPlayerLocomotion()
    {
        HandlePlayerRotation();
        HandlePlayerMovement();
    }

    private void HandleTwoHand()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isTwoHandingWeapon = !isTwoHandingWeapon;
            animator.SetBool("isTwoHandingWeapon", isTwoHandingWeapon);
        }
    }

    private void HandleLightAttack()
    {
        if (isPerformingAction)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            lightAttackInput = true;
            weaponFX.Stop();
            weaponFX.Play();
        }

        if (lightAttackInput)
        {
            lightAttackInput = false;

            if (isTwoHandingWeapon)
            {
                if (isSprinting)
                {
                    PlayActionAnimation("TH_Running_Attack_01", true);
                    return;
                }

                PlayActionAnimation("TH_Light_Attack_01", true);
                attackLastPerformed = th_Light_Attack_01;
            }
            else
            {
                if (isSprinting)
                {
                    PlayActionAnimation("OH_Running_Attack_01", true);
                    return;
                }

                PlayActionAnimation("OH_Light_Attack_01", true);
                attackLastPerformed = oh_Light_Attack_01;
            }
        }
    }

    private void HandleLightAttackCombo()
    {
        if (isPerformingAction)
        {
            if (attackLastPerformed == oh_Light_Attack_01)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PlayActionAnimation("OH_Light_Attack_02", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = oh_Light_Attack_02;
                    return;
                }
            }
            else if (attackLastPerformed == oh_Light_Attack_02)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PlayActionAnimation("OH_Light_Attack_01", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = oh_Light_Attack_01;
                }
            }
            else if (attackLastPerformed == th_Light_Attack_01)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PlayActionAnimation("TH_Light_Attack_02", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = th_Light_Attack_02;
                }
            }
            else if (attackLastPerformed == th_Light_Attack_02)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PlayActionAnimation("TH_Light_Attack_01", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = th_Light_Attack_01;
                }
            }
        }
    }

    private void HandleHeavyAttack()
    {
        if (isPerformingAction)
            return;

        if (Input.GetKeyUp(KeyCode.Q))
        {
            heavyAttackInput = true;
        }

        if (heavyAttackInput)
        {

            heavyAttackInput = false;

            if (isTwoHandingWeapon)
            {
                if (isSprinting)
                {
                    PlayActionAnimation("TH_Jumping_Attack_01", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    return;
                }

                PlayActionAnimation("TH_Heavy_Attack_01", true);
                weaponFX.Stop();
                weaponFX.Play();
                attackLastPerformed = th_Heavy_Attack_01;
            }
            else
            {
                if (isSprinting)
                {
                    PlayActionAnimation("OH_Jumping_Attack_01", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    return;
                }

                PlayActionAnimation("OH_Heavy_Attack_01", true);
                weaponFX.Stop();
                weaponFX.Play();
                attackLastPerformed = oh_Heavy_Attack_01;
            }
        }
    }

    private void HandleHeavyAttackCombo()
    {
        if (isPerformingAction)
        {
            if (attackLastPerformed == oh_Heavy_Attack_01)
            {
                if (Input.GetKeyUp(KeyCode.Q))
                {
                    PlayActionAnimation("OH_Heavy_Attack_02", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = oh_Heavy_Attack_02;
                }
            }
            else if (attackLastPerformed == oh_Heavy_Attack_02)
            {
                if (Input.GetKeyUp(KeyCode.Q))
                {
                    PlayActionAnimation("OH_Heavy_Attack_01", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = oh_Heavy_Attack_01;
                }
            }
            else if (attackLastPerformed == th_Heavy_Attack_01)
            {
                if (Input.GetKeyUp(KeyCode.Q))
                {
                    PlayActionAnimation("TH_Heavy_Attack_02", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = th_Heavy_Attack_02;
                }
            }
            else if (attackLastPerformed == th_Heavy_Attack_02)
            {
                if (Input.GetKeyUp(KeyCode.Q))
                {
                    PlayActionAnimation("TH_Heavy_Attack_01", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                    attackLastPerformed = th_Heavy_Attack_01;
                }
            }
        }
    }

    private void HandleChargeAttack()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            chargeAttackInput = true;
        }

        if (isPerformingAction)
            return;

        if (chargeAttackInput)
        {
            chargeAttackInput = false;

            if (isTwoHandingWeapon)
            {
                PlayActionAnimation("TH_Charge_Attack_01_Wind_Up", true);
                weaponChargeFX.Stop();
                weaponChargeFX.Play();
                attackLastPerformed = th_Charge_Attack_01;
            }
            else
            {
                PlayActionAnimation("OH_Charge_Attack_01_Wind_Up", true);
                weaponChargeFX.Stop();
                weaponChargeFX.Play();
                attackLastPerformed = oh_Charge_Attack_01;
            }
        }
    }

    private void HandleChargeAttackCombo()
    {
        if (isPerformingAction)
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                chargeAttackInput = true;
            }

            if (chargeAttackInput)
            {
                chargeAttackInput = false;

                if (attackLastPerformed == oh_Charge_Attack_01)
                {
                    if (Input.GetKeyUp(KeyCode.C))
                    {
                        PlayActionAnimation("OH_Charge_Attack_02_Wind_Up", true);
                        weaponChargeFX.Stop();
                        weaponChargeFX.Play();
                        attackLastPerformed = oh_Charge_Attack_02;
                    }
                }
                else if (attackLastPerformed == oh_Charge_Attack_02)
                {
                    if (Input.GetKeyUp(KeyCode.C))
                    {
                        PlayActionAnimation("OH_Charge_Attack_01_Wind_Up", true);
                        weaponChargeFX.Stop();
                        weaponChargeFX.Play();
                        attackLastPerformed = oh_Charge_Attack_01;
                    }
                }
                else if (attackLastPerformed == th_Charge_Attack_01)
                {
                    if (Input.GetKeyUp(KeyCode.C))
                    {
                        PlayActionAnimation("TH_Charge_Attack_02_Wind_Up", true);
                        weaponChargeFX.Stop();
                        weaponChargeFX.Play();
                        attackLastPerformed = th_Charge_Attack_02;
                    }
                }
                else if (attackLastPerformed == th_Charge_Attack_02)
                {
                    if (Input.GetKeyUp(KeyCode.C))
                    {
                        PlayActionAnimation("TH_Charge_Attack_01_Wind_Up", true);
                        weaponChargeFX.Stop();
                        weaponChargeFX.Play();
                        attackLastPerformed = th_Charge_Attack_01;
                    }
                }
            }
        }
    }

    private void HandleBackStep()
    {
        if (isPerformingAction)
            return;

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            attackLastPerformed = null;

            animator.SetBool("isPerformingBackStep", true);

            if (isTwoHandingWeapon)
            {
                PlayActionAnimation("TH_Backstep", true);
            }
            else
            {
                PlayActionAnimation("OH_Backstep", true);
            }
        }
    }

    private void HandleBackStepAttack()
    {
        if (isPerformingBackStep)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                animator.SetBool("isPerformingBackStep", false);

                if (isTwoHandingWeapon)
                {
                    PlayActionAnimation("TH_Backstep_Attack", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                }
                else
                {
                    PlayActionAnimation("OH_Backstep_Attack", true);
                    weaponFX.Stop();
                    weaponFX.Play();
                }
            }
        }
    }

    private void HandleStrafe()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isStrafing = true;
        }
        else
        {
            isStrafing = false;
        }
    }

    private void HandleSprint()
    {
        if (isPerformingAction)
            return;

        if (Input.GetKey(KeyCode.CapsLock) && moveAmount > 0)
        {
            isSprinting = true;
            isWalking = false;
            isRunning = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void HandleWalkOrRun()
    {
        if (isSprinting || isPerformingAction)
            return;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isWalking || !isRunning)
            {
                isWalking = false;
                isRunning = true;
            }
            else if (!isWalking || isRunning)
            {
                isWalking = true;
                isRunning = false;
            }
        }
    }

    private void HandlePlayerRotation()
    {
        if (isStrafing)
        {
            Vector3 rotationDirection = moveDirection;
            rotationDirection = cameraObject.transform.forward;
            rotationDirection.y = 0;
            rotationDirection.Normalize();
            Quaternion tr = Quaternion.LookRotation(rotationDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
        else
        {
            Vector3 targetDirection = Vector3.zero;
            targetDirection = cameraObject.transform.forward * verticalMovement;
            targetDirection = targetDirection + cameraObject.transform.right * horizontalMovement;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion turnRotation = Quaternion.LookRotation(targetDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, turnRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
    }

    private void HandlePlayerMovement()
    {
        moveDirection = cameraObject.transform.forward * verticalMovement;
        moveDirection = moveDirection + cameraObject.transform.right * horizontalMovement;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (isSprinting)
        {
            moveDirection = moveDirection * sprintSpeed;
            Vector3 projectedVelcocity = Vector3.ProjectOnPlane(moveDirection, planeNormal);
            playerRigidBody.velocity = projectedVelcocity;
            return;
        }
        else if (isRunning)
        {
            moveDirection = moveDirection * runningSpeed;
            Vector3 projectedVelcocity = Vector3.ProjectOnPlane(moveDirection, planeNormal);
            playerRigidBody.velocity = projectedVelcocity;
            return;
        }
        else if (isWalking)
        {
            moveDirection = moveDirection * walkingSpeed;
            Vector3 projectedVelcocity = Vector3.ProjectOnPlane(moveDirection, planeNormal);
            playerRigidBody.velocity = projectedVelcocity;
            return;
        }
    }

    private void HandleCameraActions()
    {
        HandleCameraFollowPlayer();
        HandleCameraRotate();
    }

    private void HandleCameraFollowPlayer()
    {
        Vector3 targetCameraPosition = Vector3.SmoothDamp(playerCamera.transform.position, transform.position, ref cameraFollowVelocity, 0.1f);
        playerCamera.transform.position = targetCameraPosition;
    }

    private void HandleCameraRotate()
    {
        Vector3 cameraRotation;
        Quaternion targetCameraRotation;

        leftandRightLookAngle += (mouseX * leftAndRightLookSpeed) * Time.deltaTime;
        upAndDownLookAngle -= (mouseY * upAndDownLookSpeed) * Time.deltaTime;
        upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);

        cameraRotation = Vector3.zero;
        cameraRotation.y = leftandRightLookAngle;
        targetCameraRotation = Quaternion.Euler(cameraRotation);
        playerCamera.transform.rotation = targetCameraRotation;

        cameraRotation = Vector3.zero;
        cameraRotation.x = upAndDownLookAngle;
        targetCameraRotation = Quaternion.Euler(cameraRotation);
        playerCameraPivot.transform.localRotation = targetCameraRotation;
    }

    private void PlayActionAnimation(string animation, bool isPerformingAction)
    {
        animator.SetBool("isPerformingAction", isPerformingAction);
        animator.CrossFade(animation, 0.2f);
    }

    private void OnAnimatorMove()
    {
        if (isPerformingAction)
        {
            playerRigidBody.drag = 0;
            Vector3 deltaPosition = animator.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition / Time.deltaTime;
            playerRigidBody.velocity = velocity;
        }
    }
}
