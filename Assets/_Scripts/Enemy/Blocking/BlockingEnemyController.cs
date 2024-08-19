using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class BlockingEnemyController : MonoBehaviour
{
    [SerializeField] float turnSpeed = 1f;

    void Update()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Quaternion lookRotation = Quaternion.LookRotation(Camera.main.transform.position - this.transform.position, Vector3.up);
        lookRotation.eulerAngles = new Vector3(0f, lookRotation.eulerAngles.y, 0f);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
    }
}
