using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private World m_World;
    [SerializeField] private Snake m_Snake;
    [SerializeField] private Fruit m_FruitPrefab;
    [SerializeField] private Transform m_FruitsBasket;

    private float m_WorldRadius = 0.5f;
    private int m_FruitN;
    private Fruit[] m_Fruits;
    private World m_WorldNext;

    // Start is called before the first frame update
    void Start()
    {
        m_WorldRadius = 0.5f;
        SpawnNextWorld();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_FruitsBasket.childCount == 0)
        {
            //var worldNext = Instantiate(m_World);
            m_World.Explode();
            m_World = m_WorldNext;
            SpawnNextWorld();

        }
    }

    private void SpawnNextWorld()
    {
        AddFruits();
        m_Snake.distanceFromCenter = m_WorldRadius;

        m_WorldRadius *= 0.9f;
        m_WorldNext = Instantiate(m_World);
        m_WorldNext.transform.localScale = 2.0f * m_WorldRadius * Vector3.one;
        m_WorldNext.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());

        m_World.Retesselate();
    }

    private void AddFruits()
    {
        m_FruitN = 3;
        m_Fruits = new Fruit[m_FruitN];

        for (int i = 0; i < m_FruitN; i++)
        {
            Quaternion rotation = Random.rotationUniform;
            var fruit = GameObject.Instantiate(m_FruitPrefab, m_FruitsBasket);
            fruit.transform.rotation = rotation;
            fruit.transform.position = m_WorldRadius * (rotation * Vector3.up);
            m_Fruits[i] = fruit;
        }
    }
}
