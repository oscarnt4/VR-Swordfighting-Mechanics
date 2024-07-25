using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingTrigger : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] float armLength = 0.9f;
    [SerializeField] float blockSpeed = 10f;
    [SerializeField] float blockRoationSpeed = 20f;

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private Vector3 weaponStartPosition;
    private bool moveWeapon = false;
    private bool rotateWeapon = false;

    private void Start()
    {
        weaponStartPosition = weapon.transform.position;
    }

    private void Update()
    {
        if (moveWeapon) UpdateWeaponPosition(targetPosition);
        if (rotateWeapon) UpdateWeaponRotation(targetRotation);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamaging>(out IDamaging damagingComponent) && other.tag != "Enemy")
        {
            targetPosition = other.bounds.center;

            Vector3 direction = targetPosition - weaponStartPosition;
            float radianZAngle = Mathf.Atan2(direction.y, direction.x);
            float eulerZAngle = radianZAngle * Mathf.Rad2Deg;

            if (eulerZAngle > 90f) eulerZAngle -= 180f;
            else if (eulerZAngle < -90f) eulerZAngle += 180f;


            targetRotation = new Vector3(0f, 0f, eulerZAngle);

            moveWeapon = true;
            rotateWeapon = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        targetPosition = weaponStartPosition;
        targetRotation = Vector3.zero;
        moveWeapon = true;
        rotateWeapon = true;
    }

    private void UpdateWeaponPosition(Vector3 target)
    {
        weapon.transform.position = Vector3.MoveTowards(weapon.transform.position, target, blockSpeed * Time.deltaTime);

        if (Vector3.Distance(weapon.transform.position, target) < 0.01f || Vector3.Distance(weaponStartPosition, weapon.transform.position) >= armLength)
        {
            weapon.transform.position = target;
            moveWeapon = false;
        }
    }

    private void UpdateWeaponRotation(Vector3 target)
    {
        Quaternion targetQuaternion = Quaternion.Euler(target);
        weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, targetQuaternion, blockRoationSpeed * Time.deltaTime);

        if(Quaternion.Angle(targetQuaternion, weapon.transform.rotation) < 0.01f)
        {
            weapon.transform.rotation = targetQuaternion;
            rotateWeapon = false;
        }
    }
}
