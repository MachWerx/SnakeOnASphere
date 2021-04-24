using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    private float m_Speed;

    // Start is called before the first frame update
    void Start()
    {
        m_Speed = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos += m_Speed * transform.up * Time.deltaTime;
        pos = 0.5f * pos.normalized;
        transform.position = pos;
        transform.rotation *= Quaternion.FromToRotation(transform.forward, -pos.normalized);
    }
}
