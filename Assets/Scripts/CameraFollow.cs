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
        m_DistanceFromCenter = 1.5f;
    }

    // Late update is called once per frame after Update is called for everything else
    private void LateUpdate()
    {
        transform.position = m_Snake.position.normalized * m_DistanceFromCenter;
        transform.rotation = Quaternion.LookRotation(m_Snake.forward, m_Snake.up);
    }
}
