using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shop : MonoBehaviour
{

    [SerializeField] private TMP_Text textBox;
    [SerializeField] private TMP_Text goldCounter;
    [SerializeField] private GameObject loadScreen;
    [SerializeField] private AudioClip coinSounds;
    [SerializeField] private AudioSource audioPrefab;
    [SerializeField] private Animator loadCardAnim;
    [SerializeField] private GameObject[] relicOwnedTexts;
    private bool[] hasBoughtRelics;

    private void Start()
    {
        hasBoughtRelics = new bool[4] { 
            GameManager.Instance.hasMagicShades, 
            GameManager.Instance.hasGoldenCards, 
            GameManager.Instance.hasHeartOfSteel, 
            GameManager.Instance.hasPrecisionScope };
        for (var i = 0; i < hasBoughtRelics.Length; i++)
        {
            if (hasBoughtRelics[i]) relicOwnedTexts[i].SetActive(true);
        }
        loadScreen.SetActive(false);
        GameManager.OnChange += DisplayGold;
        DisplayGold();
    }

    public void BuyCard(int intType)
    {
        CardType cardType = CardType.Normal;
        int price = 0;
        switch (intType)
        {
            case 1:
                cardType = CardType.Damage;
                price = 10;
                break;
            case 2:
                cardType = CardType.Free;
                price = 5;
                break;
            case 3:
                cardType = CardType.Bank;
                price = 20;
                break;
            case 4:
                cardType = CardType.Heal;
                price = 25;
                break;
        }

        if (GameManager.Instance.specialCardsDeck.Count < 52)
        {
            if (GameManager.Instance.Coins >= price)
            {
                GameManager.Instance.specialCardsDeck.Add(new CarteData(cardType));
                GameManager.Instance.Coins -= price;
                textBox.text = $"Bought {cardType} Card for {price.ToString()} coins. {GameManager.Instance.specialCardsDeck.Count.ToString()}/52";
                PlaySound(coinSounds);
            }
            else
            {
                textBox.text = "You don't have enough Coins for that.";
            }
        }
        else
        {
            textBox.text = "You have the maximum number of special Cards.";
        }
        
    }
    
    public void BuyRelic(int intRelic)
    {
        int price = 0;
        switch (intRelic)
        {
            case 0:
                price = 50;
                break;
            case 1:
                price = 100;
                break;
            case 2:
                price = 150;
                break;
            case 3:
                price = 250;
                break;
        }

        if (hasBoughtRelics[intRelic] == false)
        {
            if (GameManager.Instance.Coins >= price)
            {
                string relicName = "";
                switch (intRelic)
                {
                    case 0:
                        GameManager.Instance.hasMagicShades = true;
                        relicName = "Magic Shades";
                        break;
                    case 1:
                        GameManager.Instance.hasGoldenCards = true;
                        relicName = "Golden Cards";
                        break;
                    case 2:
                        GameManager.Instance.hasHeartOfSteel = true;
                        relicName = "Heart of Steel";
                        break;
                    case 3:
                        GameManager.Instance.hasPrecisionScope = true;
                        relicName = "Precision Scope";
                        break;
                }
                GameManager.Instance.Coins -= price;
                textBox.text = $"Bought {relicName} Card for {price.ToString()} coins.";
                hasBoughtRelics[intRelic] = true;
                relicOwnedTexts[intRelic].SetActive(true);
                PlaySound(coinSounds);
            }
            else
            {
                textBox.text = "You don't have enough Coins for that.";
            }
        }
        else
        {
            textBox.text = "You already own that relic.";
        }   
    }

    private void DisplayGold()
    {
        goldCounter.text = GameManager.Instance.Coins.ToString();
    }

    private void OnDisable()
    {
        GameManager.OnChange -= DisplayGold;
    }
    
    public void ToBattle()
    {
        GameManager.Instance.currentRound++;
        loadScreen.SetActive(true);
        loadCardAnim.Play("Loading");
        SceneManager.LoadSceneAsync("CombatScene");
    }
    
    public void PlaySound(AudioClip clip)
    {
        AudioSource audioSource = Instantiate(audioPrefab, transform);
        audioSource.volume = GameManager.Instance.gameVolume;
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(audioSource, 2f);
    }
}
