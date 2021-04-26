using UnityEngine;

public class Snake : MonoBehaviour
{
    [SerializeField] private AudioSource m_AppleChomp;
    [SerializeField] private AudioSource m_DeathSound;
    [SerializeField] private AudioSource m_BonusModeSound;


    public float distanceFromCenter { private get; set; }


    private float m_Speed;
    private float m_RotationSpeed;
    private float m_SpeedFactor;
    private float m_SnakeLength;
    private float m_SnakeLengthMax;
    private float m_SnakeRadius;
    private Vector3[] m_SplinePointsWorld;
    private int m_SplineN;
    private MeshFilter m_MeshFilter;
    private Material m_Material;

    private float m_DeathFactor;
    private const float kDeathSpeed = 0.5f;

    private Mode m_Mode;
    private const float kSpacing = .02f;
    private const int kSplineMax = 1000;
    private const int kCircumferenceMax = 12;
    private const int kExtraHeadRows = 3;

    private Color m_InitialColor;

    private TMPro.TextMeshPro m_ScoreTextField;
    private int m_Score;
    private const int kFruitScoreIncrement = 50;
    private float m_BonusScoreOverage;
    private const float kBonusScoreSpeed = 50.0f;
    private const float kSpeedFactorIncreasePerBonusSecond = 0.5f;
    private const float kSpeedFactorIncreasePerFruit = .01f;
    private const float kFruitLengthIncrement = 0.02f;

    private int score
    {
        get { return m_Score; }
        set
        {
            m_Score = value;
            m_ScoreTextField.text = m_Score.ToString();
        }
    }

    private bool m_FirstInit = true;
    enum Mode { Menu, Alive, Dying, Dead }

    private bool m_BonusMode;
    public bool bonusMode
    {
        private get { return m_BonusMode; }
        set
        {
            if (!m_BonusMode && value)
            {
                m_SpeedFactor = 1.0f;
                m_BonusModeSound.Play();
            }
            m_BonusMode = value;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    public void Init(TMPro.TextMeshPro scoreTextField)
    {
        if (m_FirstInit)
        {
            m_SplinePointsWorld = new Vector3[kSplineMax];
            m_SplineN = 2;
            m_SplinePointsWorld[0] = transform.position;
            m_SplinePointsWorld[1] = transform.position;

            m_MeshFilter = gameObject.GetComponent<MeshFilter>();
            m_Material = gameObject.GetComponent<MeshRenderer>().material;
            m_InitialColor = m_Material.color;

            m_Score = 0;

            m_FirstInit = false;
        }

        m_Mode = Mode.Menu;
        distanceFromCenter = .5f;
        m_Speed = 0.2f;
        m_RotationSpeed = 180;
        m_SpeedFactor = 1.0f;
        m_SnakeLength = m_SnakeLengthMax = 0.2f;
        m_SnakeRadius = 0.5f;
        m_Material.color = m_InitialColor;

        m_ScoreTextField = scoreTextField;
        m_BonusScoreOverage = 0;
        bonusMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Mode == Mode.Alive || m_Mode == Mode.Menu)
        {
            // turn the snake
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.rotation = Quaternion.AngleAxis(m_SpeedFactor * m_RotationSpeed * Time.deltaTime, transform.forward) * transform.rotation;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.rotation = Quaternion.AngleAxis(-m_SpeedFactor * m_RotationSpeed * Time.deltaTime, transform.forward) * transform.rotation;
            }
            float axisDelta = Input.GetAxis("Mouse X") / Camera.main.pixelWidth;
            if (Mathf.Abs(axisDelta) > 0.01f) axisDelta = 0.0f;
            transform.rotation = Quaternion.AngleAxis(-2000.0f * axisDelta, transform.forward) * transform.rotation;

            // advance the snake
            //if (Input.GetKey(KeyCode.UpArrow))
            {
                Vector3 pos = transform.position;
                pos += m_SpeedFactor * m_Speed * transform.up * Time.deltaTime;
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
                if (m_SplineN < kSplineMax && (m_SplineN - 1) * kSpacing < m_SnakeLength) m_SplineN++;
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

            Vector3 headPoint = transform.InverseTransformPoint(m_SplinePointsWorld[0]);
            for (int v = 0; v < m_SplineN + kExtraHeadRows; v++)
            {
                // calculate the coordinate system
                Vector3 forward;
                if (v - kExtraHeadRows <= 0)
                {
                    forward = transform.InverseTransformDirection(transform.up);
                } else if (v - kExtraHeadRows < m_SplineN - 1)
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

                // shape the tail
                float tailPortion = Mathf.Max(.1f / m_SnakeLength, 0.25f);
                float tailFactor = Mathf.Clamp01((1.0f - v * kSpacing / m_SnakeLength) / tailPortion);
                float radius = m_SnakeRadius * Mathf.Sin(0.5f * Mathf.PI * tailFactor);

                if ((v - kExtraHeadRows) * kSpacing > 2 * m_SnakeRadius * transform.localScale.x && v < m_SplineN + kExtraHeadRows - 3)
                {
                    float collisionFactor = Vector3.Distance(headPoint, spinePoint) / (m_SnakeRadius + radius);
                    // check for self-collision
                    if (collisionFactor < .8f && m_Mode == Mode.Alive)
                    {
                        m_Mode = Mode.Dying;
                        m_DeathFactor = 1.0f;
                        m_DeathSound.Play();
                    } else if (collisionFactor < 1.0f)
                    {
                        bool turnLeft = Vector3.Dot(headPoint - spinePoint, Vector3.right) < 0;
                        float nudgeAmount = (turnLeft ? 1.0f : -1.0f) * 0.1f * m_SpeedFactor * m_RotationSpeed * Time.deltaTime;
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
            m_MeshFilter.mesh.Clear();
            m_MeshFilter.mesh.vertices = vertices;
            m_MeshFilter.mesh.SetTriangles(tris, 0);
            m_MeshFilter.mesh.RecalculateNormals();
        } else if (m_Mode == Mode.Dying)
        {
            Color color = m_Material.GetColor("_Color");
            m_Material.SetColor("_Color", Mathf.Pow(0.2f, Time.deltaTime) * color);
            m_DeathFactor -= kDeathSpeed * Time.deltaTime;

            if (m_DeathFactor < 0)
            {
                m_Mode = Mode.Dead;
            }
        }

        if (m_Mode == Mode.Alive && bonusMode)
        {
            m_BonusScoreOverage += kBonusScoreSpeed * Time.deltaTime;
            if (m_BonusScoreOverage > 1.0f)
            {
                int overageDelta = (int)m_BonusScoreOverage;
                m_BonusScoreOverage -= overageDelta;
                score += overageDelta;
                m_SpeedFactor += kSpeedFactorIncreasePerBonusSecond * Time.deltaTime;
            }
        }
    }

    public void StartGame()
    {
        m_Mode = Mode.Alive;
        score = 0;
    }

    public bool IsDead() {
        return m_Mode == Mode.Dead;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Fruit>() != null)
        {
            Destroy(collision.gameObject);
            score += kFruitScoreIncrement;
            m_SnakeLengthMax += kFruitLengthIncrement;
            m_SpeedFactor += kSpeedFactorIncreasePerFruit;
            m_AppleChomp.Play();
        }
    }

}
