using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackableObject : MonoBehaviour
{
    public static float stackHeight = 0.1f; // Adjust this to match the object height

    public static Vector3 GetNextStackPosition(Vector3 basePosition)
    {
        Collider[] colliders = Physics.OverlapSphere(basePosition, 2.0f);
        float maxY = basePosition.y;

        foreach (var col in colliders)
        {
            if (col.CompareTag("PlacedObject"))
            {
                float topY = col.bounds.max.y;
                if (topY > maxY)
                    maxY = topY;
            }
        }

        return new Vector3(basePosition.x, maxY + stackHeight, basePosition.z);
    }
}