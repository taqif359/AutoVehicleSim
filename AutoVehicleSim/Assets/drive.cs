using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drive : MonoBehaviour
{
    public float speed = 50f;
    public float torque = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        float dtime = Time.deltaTime;
        MoveCar(horizontal, vertical, dtime);
    }

    private void MoveCar (float horizontal, float vertical, float dtime)
    {
        // Linear Movement
        float distance_moved = speed * vertical;
        transform.Translate(dtime * distance_moved * Vector3.forward);

        // Rotational Movement
        float rotation = horizontal * torque * 90f;
        transform.Rotate(0f, rotation * dtime, 0f);
    }
}
