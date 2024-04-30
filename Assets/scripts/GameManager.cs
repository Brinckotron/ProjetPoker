using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager is NULL");
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;
        DontDestroyOnLoad(this);
    }

    #endregion

    public float gameVolume = 0.5f;
    [SerializeField] private int coins = 20;
    public int currentRound = 0;
    public List<CarteData> specialCardsDeck;
    public bool hasMagicShades;
    public bool hasGoldenCards;
    public bool hasHeartOfSteel;
    public bool hasPrecisionScope;
    public delegate void CoinDelegate();
    public static event CoinDelegate OnChange;
    
    public int Coins
    {
        get => coins;
        set
        {
            coins = value;
            if (OnChange != null) OnChange();
        }
    }
    
    
    private void Start()
    {
        specialCardsDeck = new List<CarteData>();
    }
}
