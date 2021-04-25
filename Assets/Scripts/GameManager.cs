using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private World m_World;
    [SerializeField] private Snake m_Snake;
    [SerializeField] private Fruit m_FruitPrefab;
    [SerializeField] private Transform m_FruitsBasket;
    [SerializeField] private GameObject m_LevelIndicatorPrefab;
    [SerializeField] private Transform m_LevelIndicatorsTransform;
    [SerializeField] private Transform m_StartButton;
    [SerializeField] private Color[] m_LevelColors;

    private float m_WorldRadius = 0.5f;
    private int m_FruitN;
    private Fruit[] m_Fruits;
    private World m_WorldNext;
    private int m_CurrentLevel;
    private GameObject[] m_LevelIndicators;
    private const int kLevelMax = 8;
    private GameMode m_GameMode;

    enum GameMode { MainMenu, Game };


    // Start is called before the first frame update
    void Start()
    {
        m_GameMode = GameMode.MainMenu;
        m_WorldRadius = 0.5f;
        m_CurrentLevel = -1;
        SpawnNextWorld();
        m_FruitsBasket.gameObject.SetActive(false);
        m_LevelIndicatorsTransform.gameObject.SetActive(false);

        // create level indicators
        m_LevelIndicators = new GameObject[kLevelMax];
        for (int i = 0; i < kLevelMax; i++)
        {
            m_LevelIndicators[i] = Instantiate(m_LevelIndicatorPrefab, m_LevelIndicatorsTransform);
            m_LevelIndicators[i].transform.localScale = new Vector3(1.0f, 0.8f / kLevelMax, 1.0f);
            m_LevelIndicators[i].transform.localPosition = new Vector3(-1.2f, (kLevelMax - i - 0.5f) / kLevelMax - 0.5f, 1);
            m_LevelIndicators[i].GetComponent<MeshRenderer>().material.SetColor("_Color", m_LevelColors[i]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (m_GameMode == GameMode.MainMenu)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == m_StartButton) 
                    {
                        m_GameMode = GameMode.Game;

                        m_StartButton.gameObject.SetActive(false);
                        m_FruitsBasket.gameObject.SetActive(true);
                        m_LevelIndicatorsTransform.gameObject.SetActive(true);

                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }

            }
        } else if (m_GameMode == GameMode.Game)
        {
            if (m_CurrentLevel < kLevelMax - 1 && m_FruitsBasket.childCount == 0)
            {
                m_World.Explode();
                m_World = m_WorldNext;
                m_World.BurstColor();
                SpawnNextWorld();

                Destroy(m_LevelIndicators[m_CurrentLevel - 1]);
            }
        }
    }

    private void SpawnNextWorld()
    {
        if (m_CurrentLevel < kLevelMax-1)
        {
            m_CurrentLevel++;
            m_Snake.distanceFromCenter = m_WorldRadius;
            if (m_CurrentLevel < kLevelMax-1)
            {
                AddFruits();

                m_WorldRadius -= 0.05f;
                m_WorldNext = Instantiate(m_World);
                m_WorldNext.transform.localScale = 2.0f * m_WorldRadius * Vector3.one;
                m_WorldNext.GetComponent<MeshRenderer>().material.SetColor("_Color", m_LevelColors[m_CurrentLevel + 1]);
            }
            m_World.Retesselate();
        }
    }

    private void AddFruits()
    {
        m_FruitN = 1;
        m_Fruits = new Fruit[m_FruitN];

        for (int i = 0; i < m_FruitN; i++)
        {
            Quaternion rotation = Random.rotationUniform;
            var fruit = Instantiate(m_FruitPrefab, m_FruitsBasket);
            fruit.transform.rotation = rotation;
            fruit.transform.position = m_WorldRadius * (rotation * Vector3.up);
            m_Fruits[i] = fruit;
        }
    }
}
