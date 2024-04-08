using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public GameObject combatCam;
    public GameObject lockOnCam;

    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform combatLookAt;

    public CameraStyle currentStyle = CameraStyle.Combat;

    public enum CameraStyle
    { 
        Combat,
        LockOn
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (PlayerMovement.instance.locked == true)
            currentStyle = CameraStyle.LockOn;
        else
            currentStyle = CameraStyle.Combat;

        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        if (currentStyle == CameraStyle.Combat)
        {
            if (combatCam.activeInHierarchy == false)
            {
                lockOnCam.SetActive(false);
                combatCam.SetActive(true);
            }

            /*float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }*/

            Vector3 dirToCombatLookAt = player.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
        }
        else if (currentStyle == CameraStyle.LockOn) 
        {
            if (lockOnCam.activeInHierarchy == false)
            {
                combatCam.SetActive(false);
                lockOnCam.SetActive(true);
            }

            Cinemachine.CinemachineVirtualCamera lockCam = lockOnCam.GetComponent<CinemachineVirtualCamera>();
            lockCam.m_LookAt = PlayerMovement.instance.currentLockOn;
        }
    }
}
