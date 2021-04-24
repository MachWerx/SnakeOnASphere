using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    private float m_Speed;
    private float m_RotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        m_Speed = 0.5f;
        m_RotationSpeed = 90f;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 pos = transform.position;
            pos += m_Speed * transform.up * Time.deltaTime;
            pos = 0.5f * pos.normalized;
            transform.position = pos;
            //transform.rotation *= Quaternion.FromToRotation(transform.forward, -pos.normalized);
            transform.rotation = Quaternion.LookRotation(-pos, transform.up);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.AngleAxis(m_RotationSpeed * Time.deltaTime, transform.forward) * transform.rotation;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.AngleAxis(-m_RotationSpeed * Time.deltaTime, transform.forward) * transform.rotation;
        }
    }
}
