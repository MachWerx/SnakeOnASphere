using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    private float m_Speed;
    private float m_RotationSpeed;
    private float m_SnakeRadius;
    private Vector3[] m_SplinePointsWorld;
    private int m_SplineN;
    private MeshFilter m_MeshFilter;

    private const float kSpacing = .02f;
    private const int kSplineMax = 50;
    private const int kCircumferenceMax = 12;

    // Start is called before the first frame update
    void Start()
    {
        m_Speed = 0.2f;
        m_RotationSpeed = 180;
        m_SnakeRadius = 0.5f;

        m_SplinePointsWorld = new Vector3[kSplineMax];
        m_SplineN = 2;
        m_SplinePointsWorld[0] = transform.position;
        m_SplinePointsWorld[1] = transform.position;

        m_MeshFilter = gameObject.GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        // turn the snake
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.AngleAxis(m_RotationSpeed * Time.deltaTime, transform.forward) * transform.rotation;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.AngleAxis(-m_RotationSpeed * Time.deltaTime, transform.forward) * transform.rotation;
        }

        // advance the snake
        //if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 pos = transform.position;
            pos += m_Speed * transform.up * Time.deltaTime;
            pos = 0.5f * pos.normalized;
            transform.position = pos;
            //transform.rotation *= Quaternion.FromToRotation(transform.forward, -pos.normalized);
            transform.rotation = Quaternion.LookRotation(-pos, transform.up);
        }

        // propagate the spline
        m_SplinePointsWorld[0] = transform.position;
        float headSpacing = Vector3.Distance(m_SplinePointsWorld[0], m_SplinePointsWorld[1]);
        if (headSpacing > kSpacing)
        {
            if (m_SplineN < kSplineMax) m_SplineN++;
            for (int i = m_SplineN - 1; i > 1; i--)
            {
                m_SplinePointsWorld[i] = m_SplinePointsWorld[i - 1];
            }
            m_SplinePointsWorld[1] = Vector3.Lerp(m_SplinePointsWorld[0], m_SplinePointsWorld[2], (headSpacing - kSpacing) / headSpacing);
        }

        // create the mesh
        //Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[kCircumferenceMax * m_SplineN];
        Vector3[] normals = new Vector3[kCircumferenceMax * m_SplineN];
        int[] tris = new int[2 * 3 * kCircumferenceMax * (m_SplineN - 1)];
        //Vector2[] uv = new Vector2[2 * (steps + 1)];

        for (int v = 0; v < m_SplineN; v++)
        {
            Vector3 forward;
            if (v == 0)
            {
                forward = transform.InverseTransformDirection(transform.up);
            }
            else if (v == m_SplineN - 1)
            {
                forward = transform.InverseTransformDirection(m_SplinePointsWorld[v - 1] - m_SplinePointsWorld[v]);
            }
            else
            {
                forward = transform.InverseTransformDirection(m_SplinePointsWorld[v - 1] - m_SplinePointsWorld[v + 1]);
            }
            forward.Normalize();
            Vector3 up = transform.InverseTransformDirection(m_SplinePointsWorld[v]).normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;
            Vector3 spinePoint = transform.InverseTransformPoint(m_SplinePointsWorld[v]);

            for (int u = 0; u < kCircumferenceMax; u++)
            {
                float angle = 2.0f * Mathf.PI * u / kCircumferenceMax;
                vertices[v * kCircumferenceMax + u] = spinePoint + m_SnakeRadius * (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up);
                normals[v * kCircumferenceMax + u] = (vertices[v * kCircumferenceMax + u] - spinePoint).normalized;

                if (v < m_SplineN - 1)
                {
                    int triIndex = 2 * 3 * (v * kCircumferenceMax + u);
                    int vertIndex = v * kCircumferenceMax + u;
                    int vertIndexNext = v * kCircumferenceMax + (u + 1) % kCircumferenceMax;
                    tris[triIndex + 0] = vertIndex;
                    tris[triIndex + 1] = vertIndexNext;
                    tris[triIndex + 2] = vertIndex + kCircumferenceMax;

                    tris[triIndex + 3] = vertIndexNext + kCircumferenceMax;
                    tris[triIndex + 4] = vertIndex + kCircumferenceMax;
                    tris[triIndex + 5] = vertIndexNext;
                }
            }
        }

        m_MeshFilter.mesh.Clear();
        m_MeshFilter.mesh.vertices = vertices;
        m_MeshFilter.mesh.normals = normals;
        m_MeshFilter.mesh.SetTriangles(tris, 0);
        //m_MeshFilter.mesh.vertices[0] = 
    }
}
