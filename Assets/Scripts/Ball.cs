using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    const float FLOOR_POS = -10f;

    void Update()
    {
        if (transform.position.y < FLOOR_POS)
        {
            Destroy(gameObject);
        }
    }
}
