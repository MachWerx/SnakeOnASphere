using UnityEngine;

public class Snake : MonoBehaviour
{
    public float distanceFromCenter { private get; set; }


    private float m_Speed;
    private float m_RotationSpeed;
    private float m_SnakeLength;
    private float m_SnakeLengthMax;
    private float m_SnakeRadius;
    private Vector3[] m_SplinePointsWorld;
    private int m_SplineN;
    private MeshFilter m_MeshFilter;
    private Material m_Material;

    private const float kSpacing = .02f;
    private const int kSplineMax = 1000;
    private const int kCircumferenceMax = 12;
    private const int kExtraHeadRows = 3;

    // Start is called before the first frame update
    void Start()
    {
        distanceFromCenter = 0.5f;
        m_Speed = 0.2f;
        m_RotationSpeed = 180;
        float speedFactor = 2.0f;
        m_Speed *= speedFactor;
        m_RotationSpeed *= speedFactor;
        m_SnakeLength = m_SnakeLengthMax = 0.2f;
        m_SnakeRadius = 0.5f;

        m_SplinePointsWorld = new Vector3[kSplineMax];
        m_SplineN = 2;
        m_SplinePointsWorld[0] = transform.position;
        m_SplinePointsWorld[1] = transform.position;

        m_MeshFilter = gameObject.GetComponent<MeshFilter>();
        m_Material = gameObject.GetComponent<MeshRenderer>().material;
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
            pos = distanceFromCenter * pos.normalized;
            float distanceTraveled = Vector3.Distance(transform.position, pos);
            transform.position = pos;
            //transform.rotation *= Quaternion.FromToRotation(transform.forward, -pos.normalized);
            transform.rotation = Quaternion.LookRotation(-pos, transform.up);

            if (m_SnakeLength < m_SnakeLengthMax)
            {
                m_SnakeLength += distanceTraveled;
                if (m_SnakeLength > m_SnakeLengthMax)
                    m_SnakeLength = m_SnakeLengthMax;
            }
        }

        // propagate the spline
        m_SplinePointsWorld[0] = transform.position;
        float headSpacing = Vector3.Distance(m_SplinePointsWorld[0], m_SplinePointsWorld[1]);
        if (headSpacing > kSpacing)
        {
            if (m_SplineN < kSplineMax && (m_SplineN - 1)* kSpacing < m_SnakeLength) m_SplineN++;
            for (int i = m_SplineN - 1; i > 1; i--)
            {
                m_SplinePointsWorld[i] = m_SplinePointsWorld[i - 1];
            }
            m_SplinePointsWorld[1] = Vector3.Lerp(m_SplinePointsWorld[0], m_SplinePointsWorld[2], (headSpacing - kSpacing) / headSpacing);
            headSpacing -= kSpacing;
        }

        // create the mesh
        Vector3[] vertices = new Vector3[kCircumferenceMax * (m_SplineN + kExtraHeadRows)];
        int[] tris = new int[2 * 3 * kCircumferenceMax * (m_SplineN + kExtraHeadRows - 1)];
        //Vector2[] uv = new Vector2[2 * (steps + 1)];

        bool isDead = false;
        Vector3 headPoint = transform.InverseTransformPoint(m_SplinePointsWorld[0]);
        for (int v = 0; v < m_SplineN + kExtraHeadRows; v++)
        {
            // calculate the coordinate system
            Vector3 forward;
            if (v - kExtraHeadRows <= 0)
            {
                forward = transform.InverseTransformDirection(transform.up);
            }
            else if (v - kExtraHeadRows < m_SplineN - 1)
            {
                forward = transform.InverseTransformDirection(m_SplinePointsWorld[v - kExtraHeadRows - 1] - m_SplinePointsWorld[v - kExtraHeadRows + 1]);
            } else
            {
                forward = transform.InverseTransformDirection(m_SplinePointsWorld[v - kExtraHeadRows - 1] - m_SplinePointsWorld[v - kExtraHeadRows]);
            }
            forward.Normalize();
            Vector3 up = transform.InverseTransformDirection(m_SplinePointsWorld[Mathf.Max(v - kExtraHeadRows, 0)]).normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            // calculate the spine point
            Vector3 spinePoint;
            if (v - kExtraHeadRows <= 0)
            {
                spinePoint = headPoint;
            } else if (v - kExtraHeadRows < m_SplineN - 1)
            {
                spinePoint = transform.InverseTransformPoint(Vector3.Lerp(m_SplinePointsWorld[v - kExtraHeadRows + 1], m_SplinePointsWorld[v - kExtraHeadRows], headSpacing / kSpacing));
            } else
            {
                spinePoint = transform.InverseTransformPoint(m_SplinePointsWorld[v - kExtraHeadRows]);
            }

            // check for self-collision

            // shape the tail
            float tailPortion = Mathf.Max(.1f / m_SnakeLength, 0.25f);
            float tailFactor = Mathf.Clamp01((1.0f - v * kSpacing / m_SnakeLength) / tailPortion);
            float radius = m_SnakeRadius * Mathf.Sin(0.5f * Mathf.PI * tailFactor);

            if ((v - kExtraHeadRows) * kSpacing > 2 * m_SnakeRadius * transform.localScale.x && v < m_SplineN + kExtraHeadRows - 3)
            {
                float collisionFactor = Vector3.Distance(headPoint, spinePoint) / (m_SnakeRadius + radius);
                if (collisionFactor < .8f)
                {
                    isDead = true;
                } else if (collisionFactor < 1.0f)
                {
                    bool turnLeft = Vector3.Dot(headPoint - spinePoint, Vector3.right) < 0;
                    float nudgeAmount = (turnLeft ? 1.0f : -1.0f) * 0.1f * m_RotationSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.AngleAxis(nudgeAmount, transform.forward) * transform.rotation;
                }
            }

            // shape the head
            if (v < kExtraHeadRows)
            {
                float angle = 0.5f * Mathf.PI * v / kExtraHeadRows;
                spinePoint += forward * radius * Mathf.Cos(angle);
                radius *= Mathf.Sin(angle);
            }

            for (int u = 0; u < kCircumferenceMax; u++)
            {
                // calculate the vertices
                float angle = 2.0f * Mathf.PI * u / kCircumferenceMax;
                vertices[v * kCircumferenceMax + u] = spinePoint + radius * (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up);

                // set the triangles
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
        if (isDead)
        {
            m_Material.SetColor("_Color", Color.red);
            m_Speed = 0.0f;
        }

        m_MeshFilter.mesh.Clear();
        m_MeshFilter.mesh.vertices = vertices;
        m_MeshFilter.mesh.SetTriangles(tris, 0);
        m_MeshFilter.mesh.RecalculateNormals();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Fruit>() != null)
        {
            Destroy(collision.gameObject);

            m_SnakeLengthMax += 0.1f;
        }
    }

}
