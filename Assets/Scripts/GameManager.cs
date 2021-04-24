using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Snake m_Snake;
    [SerializeField] private Fruit m_FruitPrefab;

    private float m_WorldRadius = 0.5f;
    private int m_FruitN;
    private Fruit[] m_Fruits;

    // Start is called before the first frame update
    void Start()
    {
        m_WorldRadius = 0.5f;

        m_FruitN = 12;
        m_Fruits = new Fruit[m_FruitN];

        for (int i = 0; i < m_FruitN; i++)
        {
            Quaternion rotation = Random.rotationUniform;
            var fruit = GameObject.Instantiate(m_FruitPrefab);
            fruit.transform.rotation = rotation;
            fruit.transform.position = m_WorldRadius * (rotation * Vector3.up);
            m_Fruits[i] = fruit;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
