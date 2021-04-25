using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class World : MonoBehaviour
{
    [SerializeField] private PostProcessVolume m_PostProcessVolume;

    private float m_Radius;
    private Mesh m_Mesh;
    private Material m_Material;
    private Vector3[] m_Vertices;
    private int[] m_Tris;
    private int[] m_TriIndices;  // map from edges to triangle index
    private Vector3[] m_Centers;
    private Vector3[] m_Vels;
    private float m_BurstFactor;
    private float m_ExplosionFactor;
    private const float kBurstSpeed = 2.0f;
    private Color m_BurstColor;

    enum Mode { Stable, Exploding };
    private Mode m_Mode;

    // Start is called before the first frame update
    void Start()
    {
        m_Mode = Mode.Stable;
        m_Material = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Mode == Mode.Exploding)
        {
            float nudge = 2f;
            for (int i = 0; i < m_Tris.Length; i += 3)
            {
                Vector3 a = m_Vertices[m_Tris[i]];
                Vector3 b = m_Vertices[m_Tris[i + 1]];
                Vector3 c = m_Vertices[m_Tris[i + 2]];

                m_Centers[i / 3] += m_Vels[i / 3] * Time.deltaTime;
                m_Vertices[i] = Vector3.Lerp(m_Vertices[i], m_Centers[i / 3], nudge * Time.deltaTime);
                m_Vertices[i + 1] = Vector3.Lerp(m_Vertices[i + 1], m_Centers[i / 3], nudge * Time.deltaTime);
                m_Vertices[i + 2] = Vector3.Lerp(m_Vertices[i + 2], m_Centers[i / 3], nudge * Time.deltaTime);
            }
            m_Mesh.vertices = m_Vertices;

            m_ExplosionFactor -= Time.deltaTime;
            if (m_ExplosionFactor < 0)
            {
                Destroy(gameObject);
            }
        }

        if (m_BurstFactor >= 0 && m_Mode == Mode.Stable)
        {
            float intensity = Mathf.Pow(2.0f, 4.0f * m_BurstFactor) - 1.0f;
            m_Material.SetColor("_EmissionColor", intensity * m_BurstColor);
            Bloom bloom = m_PostProcessVolume.profile.GetSetting<Bloom>();
            bloom.intensity.value = intensity;

            if (m_BurstFactor > 0)
            {
                Debug.Log($"Reducing bloom");
                m_BurstFactor -= kBurstSpeed * Time.deltaTime;
                if (m_BurstFactor < 0) m_BurstFactor = 0;
            } else
            {
                m_BurstFactor = -1;
            }
        }
    }

    public void Retesselate()
    {
        m_Radius = 0.5f;

        m_Mesh = gameObject.GetComponent<MeshFilter>().mesh;

        m_Vertices = m_Mesh.vertices;
        m_Tris = m_Mesh.triangles;

        int n = m_Vertices.Length;
        int nSquared = m_Vertices.Length * m_Vertices.Length;
        m_TriIndices = new int[nSquared];
        for (int i = 0; i < nSquared; i++)
        {
            m_TriIndices[i] = -1;
        }

        // jitter the verts
        for (int i = 0; i < m_Vertices.Length; i++)
        {
            Vector3 pos = m_Vertices[i];
            pos += 0.1f * new Vector3(
                Perlin.Noise(10.1f * pos),
                Perlin.Noise(10.1f * pos + Vector3.up),
                Perlin.Noise(10.1f * pos + Vector3.right));
            m_Vertices[i] = m_Radius * pos.normalized;
        }

        // Add all the edges
        for (int i = 0; i < m_Tris.Length; i += 3)
        {
            int a = m_Tris[i];
            int b = m_Tris[i + 1];
            int c = m_Tris[i + 2];

            AddEdge(a, b, i);
            AddEdge(b, c, i);
            AddEdge(c, a, i);
        }

        // swap some of them
        for (int i = 0; i < m_Tris.Length; i += 3)
        {
            int a = m_Tris[i];
            int b = m_Tris[i + 1];
            int c = m_Tris[i + 2];

            CheckEdge(a, b, c, i);
        }

        // remake tris with separate verts
        Vector3[] newVerts = new Vector3[m_Tris.Length * 3];
        m_Centers = new Vector3[m_Tris.Length];
        m_Vels = new Vector3[m_Tris.Length];
        for (int i = 0; i < m_Tris.Length; i += 3)
        {
            Vector3 a = m_Vertices[m_Tris[i]];
            Vector3 b = m_Vertices[m_Tris[i + 1]];
            Vector3 c = m_Vertices[m_Tris[i + 2]];

            newVerts[i] = a;
            newVerts[i + 1] = b;
            newVerts[i + 2] = c;
            m_Tris[i] = i;
            m_Tris[i + 1] = i + 1;
            m_Tris[i + 2] = i + 2;

            Vector3 pos = (a + b + c) / 3.0f;
            m_Centers[i / 3] = pos;
            m_Vels[i / 3] = 1f * (pos.normalized + 0.7f * new Vector3(
                Perlin.Noise(10.1f * pos),
                Perlin.Noise(10.1f * pos + Vector3.up),
                Perlin.Noise(10.1f * pos + Vector3.right)));
        }
        m_Vertices = newVerts;

        m_Mesh.vertices = m_Vertices;
        m_Mesh.SetTriangles(m_Tris, 0);
        m_Mesh.RecalculateNormals();
    }

    public void Explode()
    {
        m_Mode = Mode.Exploding;
        m_ExplosionFactor = 1.0f;
    }

    public void BurstColor()
    {
        //gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", 2.0f * Color.white); 
        m_BurstColor = m_Material.GetColor("_Color");
        m_Material.EnableKeyword("_EMISSION");
        m_BurstFactor = 1.0f;
    }

    private void AddEdge(int a, int b, int tri)
    {
        int n = m_Vertices.Length;

        m_TriIndices[a + n * b] = tri;
    }

    private void CheckEdge(int a, int b, int c, int tri)
    {
        int n = m_Vertices.Length;

        // the other triangle does exist
        int otherTri = m_TriIndices[b + a * n];
        if (otherTri == -1) return;

        // swap the edge
        int d;
        if (m_Tris[otherTri] == a)
        {
            d = m_Tris[otherTri + 1];
        } else if (m_Tris[otherTri + 1] == a)
        {
            d = m_Tris[otherTri + 2];
        } else
        {
            d = m_Tris[otherTri];
        }

        Vector3 A = m_Vertices[a];
        Vector3 B = m_Vertices[b];
        Vector3 C = m_Vertices[c];
        Vector3 D = m_Vertices[c];

        float abDist = Vector3.Distance(A, B);
        if (Vector3.Distance(C, D) > 1.1f * abDist || abDist > Vector3.Distance(B, C) && abDist > Vector3.Distance(C, A) && Random.value > 0.5f) 

        {
            //Debug.Log($"Swapping {tri} and {otherTri}");
            // swap the edge
            //Debug.Log($"({a}, {b}, {c}) Swapping {tri} ({m_Tris[tri]}, {m_Tris[tri+1]}, {m_Tris[tri+2]}) and {otherTri} ({m_Tris[otherTri]}, {m_Tris[otherTri + 1]}, {m_Tris[otherTri + 2]})");

            m_Tris[tri] = b;
            m_Tris[tri + 1] = c;
            m_Tris[tri + 2] = d;

            m_Tris[otherTri] = a;
            m_Tris[otherTri + 1] = d;
            m_Tris[otherTri + 2] = c;

            //Debug.Log($"     After Swapping {tri} ({m_Tris[tri]}, {m_Tris[tri + 1]}, {m_Tris[tri + 2]}) and {otherTri} ({m_Tris[otherTri]}, {m_Tris[otherTri + 1]}, {m_Tris[otherTri + 2]})");
            m_TriIndices[a + n * b] = -1;
            m_TriIndices[b + n * c] = tri;
            m_TriIndices[c + n * a] = otherTri;
            m_TriIndices[c + n * d] = tri;

            m_TriIndices[b + n * a] = -1;
            m_TriIndices[a + n * d] = otherTri;
            m_TriIndices[d + n * b] = tri;
            m_TriIndices[d + n * c] = otherTri;


        }
    }
}
