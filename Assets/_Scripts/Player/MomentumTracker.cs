using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MomentumTracker : MonoBehaviour
{
    [SerializeField] float thresholdPointDistance = 0.05f;
    [SerializeField] float trackingWindowDuration = 0.3f;

    private List<Tuple<Vector3, DateTime>> positionsTravelled;

    public float largestDistanceTravelled { get; private set; }

    private void Start()
    {
        positionsTravelled = new List<Tuple<Vector3, DateTime>>();
        positionsTravelled.Add(new Tuple<Vector3, DateTime>(this.transform.position, DateTime.Now));
    }

    void Update()
    {
        RemoveOldPoints();
        AddNewPoint();
        //Debug.Log($"{gameObject.name}: " + largestDistanceTravelled);
    }

    void RemoveOldPoints()
    {
        if (positionsTravelled.Count == 0) return;
        bool pointsRemoved = false;
        for (int i = 0; i < positionsTravelled.Count; i++)
        {
            if (positionsTravelled[0].Item2 < DateTime.Now.AddSeconds(-trackingWindowDuration))
            {
                //Debug.Log("Point removed");
                positionsTravelled.Remove(positionsTravelled[0]);
                pointsRemoved = true;
            }
            else
            {
                break;
            }
        }
        if(pointsRemoved) CalculateLargestDistance();
    }

    void AddNewPoint()
    {
        if (positionsTravelled.Count == 0 || Vector3.Distance(positionsTravelled[positionsTravelled.Count - 1].Item1, this.transform.position) >= thresholdPointDistance) //Check distance from previous point is above threshold
        {
            //Debug.Log("Point added");
            positionsTravelled.Add(new Tuple<Vector3, DateTime>(this.transform.position, DateTime.Now));

            CalculateLargestDistance();
        }
    }

    void CalculateLargestDistance()
    {
        Vector3 currentPosition = transform.position;
        float largestDistance = 0f;

        foreach (Tuple<Vector3, DateTime> position in positionsTravelled)
        {
            float distance = Vector3.Distance(currentPosition, position.Item1);

            if (distance > largestDistance)
            {
                largestDistance = distance;
            }
        }

        largestDistanceTravelled =  largestDistance;
        //Debug.Log("Distance calculated");
    }
}
