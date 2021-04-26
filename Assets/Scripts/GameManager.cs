using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform m_Worlds;
    [SerializeField] private World m_World;
    [SerializeField] private World m_WorldPrefab;
    [SerializeField] private Snake m_Snake;
    [SerializeField] private Fruit m_FruitPrefab;
    [SerializeField] private Transform m_FruitsBasket;
    [SerializeField] private GameObject m_LevelIndicatorPrefab;
    [SerializeField] private Transform m_LevelIndicatorsTransform;
    [SerializeField] private Transform m_StartButton;
    [SerializeField] private Color[] m_LevelColors;
    [SerializeField] private PostProcessVolume m_PostProcessVolume;
    [SerializeField] private TMPro.TextMeshPro m_Score;

    [SerializeField] private AudioSource m_GameStart;
    [SerializeField] private AudioSource m_NextLevelSound;

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
    private void Start()
    {
        Init();
    }

    void Init()
    {
        m_GameMode = GameMode.MainMenu;
        m_WorldRadius = 0.5f;
        m_CurrentLevel = -1;
        SpawnNextWorld();
        m_FruitsBasket.gameObject.SetActive(false);
        m_LevelIndicatorsTransform.gameObject.SetActive(false);
        m_Snake.Init(m_Score);

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
            bool startGame = false;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                startGame = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == m_StartButton) 
                    {
                        startGame = true;
                    }
                }

            }

            if (startGame)
            {
                m_GameMode = GameMode.Game;
                m_Snake.StartGame();

                m_StartButton.gameObject.SetActive(false);
                m_FruitsBasket.gameObject.SetActive(true);
                m_LevelIndicatorsTransform.gameObject.SetActive(true);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                m_GameStart.Play();
            }
        }
        
        else if (m_GameMode == GameMode.Game)
        {
            // advance level when all the fruits are eaten
            if (m_CurrentLevel < kLevelMax - 1 && m_FruitsBasket.childCount == 0)
            {
                m_World.Explode();
                m_World = m_WorldNext;
                m_World.BurstColor();
                SpawnNextWorld();
                m_NextLevelSound.Play();
                Destroy(m_LevelIndicators[m_CurrentLevel - 1]);
            }

            if (m_Snake.IsDead())
            {
                m_GameMode = GameMode.MainMenu;

                m_StartButton.gameObject.SetActive(true);
                m_FruitsBasket.gameObject.SetActive(false);
                m_LevelIndicatorsTransform.gameObject.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                m_Snake.Init(m_Score);

                // Destroy all worlds, fruits, and level indicators
                foreach (Transform child in m_Worlds)
                {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in m_FruitsBasket)
                {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in m_LevelIndicatorsTransform)
                {
                    Destroy(child.gameObject);
                }

                // Create initial world
                m_World = Instantiate(m_WorldPrefab, m_Worlds);
                m_World.m_PostProcessVolume = m_PostProcessVolume;
                Init();
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
                m_WorldNext = Instantiate(m_World, m_Worlds);
                m_WorldNext.transform.localScale = 2.0f * m_WorldRadius * Vector3.one;
                m_WorldNext.GetComponent<MeshRenderer>().material.SetColor("_Color", m_LevelColors[m_CurrentLevel + 1]);
            } else
            {
                m_Snake.bonusMode = true;
            }
            m_World.Retesselate();
        }
    }

    private void AddFruits()
    {
        m_FruitN = 10;
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
