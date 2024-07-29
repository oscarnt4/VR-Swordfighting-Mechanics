using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BlockingTrigger : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] float armLength = 0.9f;
    [SerializeField] float blockSpeed = 10f;
    [SerializeField] float blockRotationSpeed = 50f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 weaponStartPosition;
    private Quaternion weaponStartRotation;
    private bool moveWeapon = false;
    private bool rotateWeapon = false;
    private float currentArmSpeed;
    private bool allowBlock = true;

    private void Awake()
    {
        currentArmSpeed = blockSpeed;
    }

    private void Start()
    {
        weaponStartPosition = weapon.transform.position;
        weaponStartRotation = weapon.transform.rotation;
    }

    /* private void Update()
     {
         if (moveWeapon) UpdateWeaponPosition(targetPosition);
         if (rotateWeapon) UpdateWeaponRotation(targetRotation);
         if (Vector3.Distance(weapon.transform.position, Vector3.zero) < 0.01f)
         {
             Debug.Log("Block speed: " + currentArmSpeed);
             currentArmSpeed = blockSpeed;
         }

     }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamaging>(out IDamaging damagingComponent) && other.tag != "Enemy" && allowBlock)
        {
            Debug.Log("Trigger entered");
            targetPosition = other.bounds.center;

            Vector3 direction = targetPosition - weaponStartPosition;
            float radianZAngle = Mathf.Atan2(direction.y, direction.x);
            float eulerZAngle = radianZAngle * Mathf.Rad2Deg;

            if (eulerZAngle > 90f) eulerZAngle -= 180f;
            else if (eulerZAngle < -90f) eulerZAngle += 180f;


            targetRotation = Quaternion.Euler(0f, 0f, eulerZAngle);

            StartCoroutine(Block(targetPosition, targetRotation));

            //moveWeapon = true;
            //rotateWeapon = true;
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IDamaging>(out IDamaging damagingComponent) && other.tag != "Enemy")
        {
            Debug.Log("trigger exited");
            targetPosition = weaponStartPosition;
            targetRotation = Vector3.zero;
            currentArmSpeed = 1f;
            moveWeapon = true;
            rotateWeapon = true;
        }
    }*/

    private IEnumerator Block(Vector3 targetPosition, Quaternion targetRotation)
    {
        allowBlock = false;
        while (!UpdateWeaponRotation(targetRotation) || !UpdateWeaponPosition(targetPosition))
        {
            yield return null;
        }
        currentArmSpeed = 3f;
        while (!UpdateWeaponRotation(weaponStartRotation) || !UpdateWeaponPosition(weaponStartPosition))
        {
            yield return null;
        }
        currentArmSpeed = blockSpeed;
        allowBlock = true;
    }

    private bool UpdateWeaponPosition(Vector3 target)
    {
        weapon.transform.position = Vector3.MoveTowards(weapon.transform.position, target, currentArmSpeed * Time.deltaTime);

        if (Vector3.Distance(weapon.transform.position, target) < 0.01f || Vector3.Distance(weaponStartPosition, weapon.transform.position) >= armLength)
        {
            weapon.transform.position = target;
            return true;
        }
        return false;
    }

    /*private bool UpdateWeaponRotation(Vector3 target)
    {
        Quaternion targetQuaternion = Quaternion.Euler(target);
        weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, targetQuaternion, blockRotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(targetQuaternion, weapon.transform.rotation) < 0.01f)
        {
            weapon.transform.rotation = targetQuaternion;
            return true;
        }
        return false;
    }*/

    private bool UpdateWeaponRotation(Quaternion target)
    {
        weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, target, blockRotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(target, weapon.transform.rotation) < 0.01f)
        {
            weapon.transform.rotation = target;
            return true;
        }
        return false;
    }
}
