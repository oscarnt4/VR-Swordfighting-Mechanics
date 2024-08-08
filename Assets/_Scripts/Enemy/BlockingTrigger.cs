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

    private Vector3 localTargetPosition;
    private Quaternion targetRotation;
    private Vector3 weaponStartLocalPosition;
    private Quaternion weaponStartLocalRotation;
    private bool moveWeapon = false;
    private bool rotateWeapon = false;
    private float currentArmSpeed;
    private bool allowBlock = true;
    private bool endBlock = false;
    private bool newBlock = false;

    private void Awake()
    {
        currentArmSpeed = blockSpeed;
    }

    private void Start()
    {
        weaponStartLocalPosition = weapon.transform.localPosition;
        weaponStartLocalRotation = Quaternion.identity;
        UpdateWeaponLocalPosition(weaponStartLocalPosition);
        UpdateWeaponLocalRotation(weaponStartLocalRotation);
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
            newBlock = true;
            localTargetPosition = this.transform.InverseTransformPoint(other.bounds.center);
            
            Vector3 direction = localTargetPosition - weaponStartLocalPosition;
            float radianZAngle = Mathf.Atan2(direction.y, direction.x);
            float eulerZAngle = radianZAngle * Mathf.Rad2Deg;

            if (eulerZAngle > 90f) eulerZAngle -= 180f;
            else if (eulerZAngle < -90f) eulerZAngle += 180f;


            targetRotation = Quaternion.Euler(0f, 0f, eulerZAngle);

            StartCoroutine(Block(localTargetPosition, targetRotation));

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
        float blockStartTime = Time.time;
        while (!UpdateWeaponLocalRotation(targetRotation) || !UpdateWeaponLocalPosition(targetPosition))
        {
            yield return null;
        }
        while (!endBlock && Time.time < blockStartTime + 0.3f)
        {
            yield return null;
        }
        currentArmSpeed = 3f;
        while (!UpdateWeaponLocalRotation(weaponStartLocalRotation) || !UpdateWeaponLocalPosition(weaponStartLocalPosition))
        {
            yield return null;
        }
        currentArmSpeed = blockSpeed;
        allowBlock = true;
        endBlock = false;
    }

    private bool UpdateWeaponLocalPosition(Vector3 target)
    {
        weapon.transform.localPosition = Vector3.MoveTowards(weapon.transform.localPosition, target, currentArmSpeed * Time.deltaTime);
        if (Vector3.Distance(weaponStartLocalPosition, weapon.transform.localPosition) >= armLength) return true;
        if (Vector3.Distance(weapon.transform.localPosition, target) < 0.01f)
        {
            weapon.transform.localPosition = target;
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

    private bool UpdateWeaponLocalRotation(Quaternion target)
    {
        weapon.transform.localRotation = Quaternion.Slerp(weapon.transform.localRotation, target, blockRotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(target, weapon.transform.localRotation) < 0.01f)
        {
            weapon.transform.localRotation = target;
            return true;
        }
        return false;
    }

    public void EndBlock()
    {
        endBlock = true;
    }
}
