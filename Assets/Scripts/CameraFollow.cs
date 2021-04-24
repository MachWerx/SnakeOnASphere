using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform m_Snake;

    private float m_DistanceFromCenter;

    // Start is called before the first frame update
    void Start()
    {
        m_DistanceFromCenter = 3;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = m_Snake.position.normalized * m_DistanceFromCenter;
        transform.rotation = Quaternion.LookRotation(m_Snake.forward, m_Snake.up);
    }
}
