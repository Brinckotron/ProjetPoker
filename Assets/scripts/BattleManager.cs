using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UI.Slider;

public class BattleManager : MonoBehaviour
{
    private bool _isPlayerTurn;
    private int _enemyHP;
    private int _enemyMaxHP = 20;
    [SerializeField] private Carte[] enemyHand;
    private List<CarteData> _enemyDeck;
    private List<CarteData> _enemyDiscard;
    private Dictionary<string, int> spriteMap;
    public List<Carte> comboCards;
    [SerializeField] private Slider gameVolumeSlider;
    [SerializeField] private TMP_Text messageBox;
    [SerializeField] private TMP_Text combatBox;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text matchText;
    [SerializeField] private GameObject healthIcon;
    [SerializeField] private GameObject flippedCardPrefab;
    private GameObject[] flippedCards;
    [SerializeField] private AudioSource musicBox;
    [SerializeField] private AudioSource soundEffects;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip healSound;
    private bool _isDisplayingMsg = false;
    private bool _isDisplayingCombatMsg = false;
    private bool _isCombatWon;
    private bool _isDamageBoostOn;
    private bool _isNoThankYou;
    public bool isEnemyCardsRevealed = false;
    private Vector3[] _enemyHandPositions;
    [SerializeField] private Transform comboCardsTransform;
    [SerializeField] private Player player;
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private GameObject[] buttons;
    [SerializeField] private Image[] inventorySlots;
    [SerializeField] private Sprite[] consumableSprites;
    [SerializeField] private Sprite[] relicIcons;
    [SerializeField] private Image[] relicSlots;

    private enum Combos
    {
        HighCard,
        Pair,
        TwoPairs,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }

    private Combos _activeCombo;


    public int EnemyHP
    {
        get => _enemyHP;
        set => _enemyHP = value;
    }

    public bool IsPlayerTurn
    {
        get => _isPlayerTurn;
        set => _isPlayerTurn = value;
    }

    void Start()
    {
        gameVolumeSlider.value = GameManager.Instance.gameVolume;
        spriteMap = new Dictionary<string, int>()
        {
            { "Medkit", 0 },
            { "Mulligan", 1 },
            { "NoThankYou", 2 },
            { "DamageBoost", 3 }
        };
        musicBox.volume = GameManager.Instance.gameVolume;
        SetRelicIcons();
        isEnemyCardsRevealed = false;
        matchText.text = $"Match {GameManager.Instance.currentRound}/10";
        SetMaxHP();
        BuildDeck();
        ShuffleDeck();
        if(GameManager.Instance.currentRound >= 5) AddDamageCards();
        GainHealth(_enemyMaxHP);
        _enemyDiscard = new List<CarteData>();
        Deal();
        _enemyHandPositions = new Vector3[5];
        for (int i = 0; i < enemyHand.Length; i++)
        {
            _enemyHandPositions[i] = enemyHand[i].transform.position;
        }

        DisplayMessage(false, $"Match {GameManager.Instance.currentRound.ToString()}/10");
        StartCoroutine(nameof(InitializePlayerTurn));
    }

    public void SetRelicIcons()
    {
        List<Sprite> iconList = new List<Sprite>();
        if (GameManager.Instance.hasMagicShades) iconList.Add(relicIcons[0]);
        if (GameManager.Instance.hasGoldenCards) iconList.Add(relicIcons[1]);
        if (GameManager.Instance.hasHeartOfSteel) iconList.Add(relicIcons[2]);
        if (GameManager.Instance.hasPrecisionScope) iconList.Add(relicIcons[3]);

        for (var i = 0; i < iconList.Count; i++)
        {
            relicSlots[i].sprite = iconList[i];
            relicSlots[i].color = Color.white;
        }
    }

    private void SetMaxHP()
    {
        switch (GameManager.Instance.currentRound)
        {
            case < 10:
                _enemyMaxHP = 5 + (GameManager.Instance.currentRound * 5);
                break;
            case 10:
                _enemyMaxHP = 60;
                break;
        }
    }

    public IEnumerator InitializePlayerTurn()
    {
        yield return new WaitForSeconds(1f);
        _isPlayerTurn = true;
    }

    public void ResetTurn()
    {
        if (_enemyHP > 0 && player.hp > 0)
        {
            for (var i = 0; i < enemyHand.Length; i++)
            {
                enemyHand[i].transform.position = _enemyHandPositions[i];
            }

            isEnemyCardsRevealed = false;
            Deal();
            StartCoroutine(nameof(InitializePlayerTurn));
        }
        else
        {
            if (_enemyHP == 0) _isCombatWon = true;
            else _isCombatWon = false;
            EndCombat();
        }
    }

    public void BuildDeck()
    {
        _enemyDeck = new List<CarteData>();
        for (int i = 1; i <= 13; i++)
        {
            _enemyDeck.Add(new CarteData(i, Symbole.Pique));
            _enemyDeck.Add(new CarteData(i, Symbole.Trefle));
            _enemyDeck.Add(new CarteData(i, Symbole.Coeur));
            _enemyDeck.Add(new CarteData(i, Symbole.Carreau));
        }

        if (GameManager.Instance.currentRound < 3)
        {
            _enemyDeck.RemoveRange(0,4);
            if (GameManager.Instance.currentRound == 1) _enemyDeck.RemoveRange(_enemyDeck.Count-4,4);
        }
    }

    public void AddDamageCards()
    {
        for (var i = 0; i < GameManager.Instance.currentRound; i++)
        {
            _enemyDeck[i].cardType = CardType.Damage;
        }
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        var temp = new List<CarteData>();
        while (_enemyDeck.Count > 0)
        {
            int indexRandom = Random.Range(0, _enemyDeck.Count);
            temp.Add((_enemyDeck[indexRandom]));
            _enemyDeck.RemoveAt(indexRandom);
        }

        _enemyDeck = temp;
    }

    public void ReshuffleDeck()
    {
        while (_enemyDiscard.Count > 0)
        {
            _enemyDeck.Add(_enemyDiscard[0]);
            _enemyDiscard.RemoveAt(0);
        }

        ShuffleDeck();
    }

    public void Deal()
    {
        StartCoroutine("DrawCards", enemyHand.ToList());
    }

    public void Draw()
    {
        List<Carte> toDraw = new List<Carte>();
        List<Carte> toDiscard = new List<Carte>();
        for (int i = 0; i < 5; i++)
        {
            if (!enemyHand[i].isKept)
            {
                if (!IsPlayerTurn) toDiscard.Add(enemyHand[i]);
                toDraw.Add(enemyHand[i]);
            }
            else
            {
                enemyHand[i].Unkeep();
            }
        }

        StartCoroutine("DrawCards", toDraw);
        StartCoroutine("DiscardCards", toDiscard);
    }

    public IEnumerator DrawCards(List<Carte> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            if (_enemyDeck.Count == 0) ReshuffleDeck();
            cards[i].Draw();
            cards[i].SetData(_enemyDeck[0]);
            _enemyDeck.RemoveAt(0);
            yield return new WaitForSeconds(0.1f);
        }

        if (player.hp <= (player.maxHp/2) && GameManager.Instance.hasMagicShades) isEnemyCardsRevealed = true;
        if (!isEnemyCardsRevealed)
        {
            yield return new WaitForSeconds(0.5f - (cards.Count * 0.1f));
            flippedCards = new GameObject[cards.Count];
            for (var i = 0; i < cards.Count; i++)
            {
                flippedCards[i] = Instantiate(flippedCardPrefab, cards[i].transform);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public IEnumerator DiscardCards(List<Carte> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            Discard(cards[i]);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Discard(Carte card)
    {
        _enemyDiscard.Add(card.Data);
        card.Discard();
    }

    public void GainHealth(int healthAmount)
    {
        _enemyHP = Mathf.Clamp(_enemyHP + healthAmount, 0, _enemyMaxHP);
        hpText.text = _enemyHP.ToString();
        healthIcon.GetComponent<Animator>().Play("Heal");
    }

    public void LooseHealth(int damage)
    {
        _enemyHP = Mathf.Clamp(_enemyHP - damage, 0, _enemyMaxHP);
        hpText.text = _enemyHP.ToString();
        healthIcon.GetComponent<Animator>().Play("Hurt");
    }

    public void CheckForCombos()
    {
        comboCards = new List<Carte>();

        //straight
        int[] tempValues = new int[5]
        {
            enemyHand[0].Data.valeur, enemyHand[1].Data.valeur, enemyHand[2].Data.valeur, enemyHand[3].Data.valeur,
            enemyHand[4].Data.valeur
        };
        Array.Sort(tempValues);
        if ((tempValues[0] == tempValues[1] - 1 &&
             tempValues[0] == tempValues[2] - 2 &&
             tempValues[0] == tempValues[3] - 3 &&
             tempValues[0] == tempValues[4] - 4) ||
            (tempValues[0] == 1 &&
             tempValues[1] == 10 &&
             tempValues[2] == 11 &&
             tempValues[3] == 12 &&
             tempValues[4] == 13))
        {
            foreach (var carte in enemyHand)
            {
                if (!comboCards.Contains(carte)) comboCards.Add(carte);
            }
        }


        //flush
        if (enemyHand[0].Data.symbole == enemyHand[1].Data.symbole &&
            enemyHand[0].Data.symbole == enemyHand[2].Data.symbole &&
            enemyHand[0].Data.symbole == enemyHand[3].Data.symbole &&
            enemyHand[0].Data.symbole == enemyHand[4].Data.symbole)
        {
            foreach (var carte in enemyHand)
            {
                if (!comboCards.Contains(carte)) comboCards.Add(carte);
            }
        }

        //pairs, triples, four of a kinds, full house
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (j <= i) continue;
                if (enemyHand[i].Data.valeur == enemyHand[j].Data.valeur)
                {
                    if (!comboCards.Contains(enemyHand[i])) comboCards.Add(enemyHand[i]);
                    if (!comboCards.Contains(enemyHand[j])) comboCards.Add(enemyHand[j]);
                }
            }
        }

        //High Card
        if (comboCards.Count == 0)
        {
            int indexHighest = -1;
            int tempValue = 0;
            for (int i = 0; i < 5; i++)
            {
                if (enemyHand[i].Data.valeur == 1)
                {
                    indexHighest = i;
                    break;
                }

                if (enemyHand[i].Data.valeur > tempValue)
                {
                    tempValue = enemyHand[i].Data.valeur;
                    indexHighest = i;
                }
            }

            comboCards.Add(enemyHand[indexHighest]);
        }

        //Set ActiveCombo
        switch (comboCards.Count)
        {
            case 1:
                _activeCombo = Combos.HighCard;
                break;
            case 2:
                _activeCombo = Combos.Pair;
                break;
            case 3:
                _activeCombo = Combos.ThreeOfAKind;
                break;
            case 4:
                if (comboCards[0].Data.valeur == comboCards[1].Data.valeur &&
                    comboCards[0].Data.valeur == comboCards[2].Data.valeur &&
                    comboCards[0].Data.valeur == comboCards[3].Data.valeur)
                {
                    _activeCombo = Combos.FourOfAKind;
                }
                else
                {
                    _activeCombo = Combos.TwoPairs;
                }

                break;
            case 5:
                if ((tempValues[0] == tempValues[1] - 1 &&
                     tempValues[0] == tempValues[2] - 2 &&
                     tempValues[0] == tempValues[3] - 3 &&
                     tempValues[0] == tempValues[4] - 4) ||
                    (tempValues[0] == 1 &&
                     tempValues[1] == 10 &&
                     tempValues[2] == 11 &&
                     tempValues[3] == 12 &&
                     tempValues[4] == 13))
                {
                    if (enemyHand[0].Data.symbole == enemyHand[1].Data.symbole &&
                        enemyHand[0].Data.symbole == enemyHand[2].Data.symbole &&
                        enemyHand[0].Data.symbole == enemyHand[3].Data.symbole &&
                        enemyHand[0].Data.symbole == enemyHand[4].Data.symbole)
                    {
                        if (tempValues[0] == 1 && tempValues[4] == 13)
                        {
                            _activeCombo = Combos.RoyalFlush;
                        }
                        else
                        {
                            _activeCombo = Combos.StraightFlush;
                        }
                    }
                    else
                    {
                        _activeCombo = Combos.Straight;
                    }
                }
                else if (enemyHand[0].Data.symbole == enemyHand[1].Data.symbole &&
                         enemyHand[0].Data.symbole == enemyHand[2].Data.symbole &&
                         enemyHand[0].Data.symbole == enemyHand[3].Data.symbole &&
                         enemyHand[0].Data.symbole == enemyHand[4].Data.symbole)
                {
                    _activeCombo = Combos.Flush;
                }
                else
                {
                    _activeCombo = Combos.FullHouse;
                }

                break;
        }
    }

    public void Resolve()
    {
        StartCoroutine(nameof(ResolveTurn));
    }

    private IEnumerator ResolveTurn()
    {
        if (flippedCards != null)
        {
            foreach (var flippedCard in flippedCards)
            {
                Destroy(flippedCard);
            }
        }
        isEnemyCardsRevealed = true;
        yield return new WaitForSeconds(2f);
        if (GameManager.Instance.currentRound >= 7)
        {
            CheckForCombos();
            if (_activeCombo is Combos.HighCard or Combos.Pair or Combos.ThreeOfAKind)
            {
                List<Carte> toDraw = new List<Carte>();
                for (var i = 0; i < enemyHand.Length; i++)
                {
                    if (!comboCards.Contains(enemyHand[i]))
                    {
                        toDraw.Add(enemyHand[i]);
                        yield return new WaitForSeconds(0.1f);
                        if (GameManager.Instance.currentRound == 7 && toDraw.Count == 1) break;
                        if (GameManager.Instance.currentRound == 8 && toDraw.Count == 2) break;
                        if (GameManager.Instance.currentRound == 9 && toDraw.Count == 3) break;
                        if (GameManager.Instance.currentRound == 10 && toDraw.Count == 4) break;
                    }
                }
                StartCoroutine(DiscardCards(toDraw));
                StartCoroutine(DrawCards(toDraw));
                yield return new WaitForSeconds(1.5f);
            }
        }
        CheckForCombos();
        List<Vector3> comboCardsPositions = new List<Vector3>();
        foreach (var carte in enemyHand)
        {
            if (!comboCards.Contains(carte))
            {
                Discard(carte);
                yield return new WaitForSeconds(0.1f);
            }
        }

        switch (comboCards.Count)
        {
            case 1:
                comboCardsPositions.Add(comboCardsTransform.position);
                break;
            case 2:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x - 125f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x + 125f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
            case 3:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x - 250f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(comboCardsTransform.position);
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x + 250f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
            case 4:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x - 375f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x - 125f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x + 125f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x + 375f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
            case 5:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x - 500f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x - 250f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(comboCardsTransform.position);
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x + 250f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x + 500f,
                    comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
        }

        for (var i = 0; i < comboCards.Count; i++)
        {
            comboCards[i].MoveTo(comboCardsPositions[i], comboCardsTransform.rotation);
        }

        //resolve Round Winner
        DisplayMessage(false, $"Player: {player.activeCombo} vs Enemy: {_activeCombo}");
        string winner = "";
        int dmg = 0;
        if ((int)player.activeCombo > (int)_activeCombo)
        {
            winner = "Player";
        }
        else if ((int)player.activeCombo < (int)_activeCombo)
        {
            winner = "Enemy";
        }
        else
        {
            if (_activeCombo == Combos.HighCard || _activeCombo == Combos.Pair || _activeCombo == Combos.ThreeOfAKind ||
                _activeCombo == Combos.FourOfAKind)
            {
                if (player.comboCards[0].Data.valeur != comboCards[0].Data.valeur)
                {
                    if (player.comboCards[0].Data.valeur == 1 ||
                        (player.comboCards[0].Data.valeur > comboCards[0].Data.valeur &&
                         comboCards[0].Data.valeur != 1))
                    {
                        winner = "Player";
                    }
                    else
                    {
                        winner = "Enemy";
                    }
                }
                else
                {
                    winner = "Draw";
                }
            }
            else if (_activeCombo == Combos.TwoPairs || _activeCombo == Combos.Flush ||
                     _activeCombo == Combos.FullHouse)
            {
                int[] playerValues = new int[comboCards.Count];
                int[] enemyValues = new int[comboCards.Count];
                int playerScore = 0;
                int enemyScore = 0;

                for (var i = 0; i < comboCards.Count; i++)
                {
                    if (player.comboCards[i].Data.valeur == 1) playerValues[i] = 14;
                    else playerValues[i] = player.comboCards[i].Data.valeur;
                    playerScore += playerValues[i];

                    if (comboCards[i].Data.valeur == 1) enemyValues[i] = 14;
                    else enemyValues[i] = comboCards[i].Data.valeur;
                    enemyScore += enemyValues[i];
                }

                if (playerScore == enemyScore) winner = "Draw";
                else winner = (playerScore > enemyScore) ? "Player" : "Enemy";
            }
            else if (_activeCombo == Combos.Straight || _activeCombo == Combos.StraightFlush)
            {
                int[] playerValues = new int[5]
                {
                    player.comboCards[0].Data.valeur, player.comboCards[1].Data.valeur,
                    player.comboCards[2].Data.valeur, player.comboCards[3].Data.valeur, player.comboCards[4].Data.valeur
                };
                int[] enemyValues = new int[5]
                {
                    comboCards[0].Data.valeur, comboCards[1].Data.valeur, comboCards[2].Data.valeur,
                    comboCards[3].Data.valeur, comboCards[4].Data.valeur
                };
                Array.Sort(playerValues);
                Array.Sort(enemyValues);
                int playerScore = 0;
                int enemyScore = 0;

                if (playerValues[0] == 1 && playerValues[4] == 13) playerValues[0] = 14;
                if (enemyValues[0] == 1 && enemyValues[4] == 13) enemyValues[0] = 14;
                for (var i = 0; i < 5; i++)
                {
                    playerScore += playerValues[i];
                    enemyScore += enemyValues[i];
                }

                if (playerScore == enemyScore) winner = "Draw";
                else winner = (playerScore > enemyScore) ? "Player" : "Enemy";
            }
            else if (_activeCombo == Combos.RoyalFlush)
            {
                winner = "Draw";
            }
        }

        if (winner == "Enemy")
        {
            foreach (Carte comboCard in comboCards)
            {
                dmg += comboCard.Data.damage;
            }
        }
        else if (winner == "Player")
        {
            foreach (Carte comboCard in player.comboCards)
            {
                dmg += comboCard.Data.damage;
            }
        }

        if (winner != "Draw")
        {
            int playerDmg = dmg;
            if (GameManager.Instance.hasPrecisionScope && player.comboCards.Count >= 3) playerDmg += 2;
            if (_isDamageBoostOn) playerDmg += player.comboCards.Count;
            DisplayMessage(true, $"{winner} wins, dealing {playerDmg.ToString()} damage.");
            yield return new WaitForSeconds(1f);
            if (winner == "Player")
            {
                
                LooseHealth(playerDmg);
                PlaySound(winSound);
                _isDamageBoostOn = false;
            }
            else
            {
                player.LooseHealth(dmg);
                PlaySound(loseSound);
            }
        }
        else DisplayMessage(true, $"Round is a Draw");

        float waitTime = 2f;
        if (GameManager.Instance.currentRound >= 8) waitTime = 3f;
        yield return new WaitForSeconds(waitTime);

        foreach (var comboCard in comboCards)
        {
            Discard(comboCard);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        ResetTurn();
    }

    public void DisplayMessage(bool isCombatBox, string message)
    {
        if (!isCombatBox)
        {
            if (_isDisplayingMsg) StopCoroutine(nameof(MessageBoxFade));
            messageBox.text = message;
            StartCoroutine(nameof(MessageBoxFade));
        }
        else
        {
            if (_isDisplayingCombatMsg) StopCoroutine(nameof(CombatBoxFade));
            combatBox.text = message;
            StartCoroutine(nameof(CombatBoxFade));
        }
    }

    public IEnumerator MessageBoxFade()
    {
        _isDisplayingMsg = true;
        for (float alpha = 100f; alpha >= 0; alpha--)
        {
            messageBox.alpha = alpha / 100;
            yield return new WaitForSeconds(0.05f);
        }

        _isDisplayingMsg = false;
    }

    public IEnumerator CombatBoxFade()
    {
        _isDisplayingCombatMsg = true;
        for (float alpha = 100f; alpha >= 0; alpha--)
        {
            combatBox.alpha = alpha / 100;
            yield return new WaitForSeconds(0.05f);
        }

        _isDisplayingCombatMsg = false;
    }

    private void EndCombat()
    {
        endScreen.SetActive(true);
        if (_isCombatWon)
        {
            if (GameManager.Instance.currentRound < 10)
            {
                int goldEarned = (GameManager.Instance.currentRound * 20) + player.hp;
                endScreen.GetComponentInChildren<TMP_Text>().text =
                    $"You won the match!You have earned...\n Match #{GameManager.Instance.currentRound.ToString()} x 20\n+ Health left: {player.hp.ToString()}\n = {goldEarned} coins!";
                player.GainCoins(goldEarned);
                endScreen.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text = "Continue to Shop";
            }
            else
            {
                endScreen.GetComponentInChildren<TMP_Text>().text = "You finished the Game!";
                endScreen.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text =
                    "Return to Main Menu";
            }
        }
        else
        {
            endScreen.GetComponentInChildren<TMP_Text>().text = "You lost the match!";
            endScreen.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text = "Return to Main Menu";
        }
    }

    public void EndCombatButton()
    {
        endScreen.SetActive(false);
        if (GameManager.Instance.currentRound < 10 && _isCombatWon) SceneManager.LoadScene("Shop");
        else
        {
            GameManager.Instance.ResetGame();
            SceneManager.LoadScene("MenuScene");
        }
    }

    public void UseConsumable(int buttonIndex)
    {
        switch (GameManager.Instance.inventory[buttonIndex])
        {
            case "Medkit":
                player.GainHealth(5);
                PlaySound(healSound);
                DisplayMessage(false, "Used Medkit to heal 5 HP");
                break;
            case "Mulligan":
                player.maxDrawCount++;
                DisplayMessage(false, "Used Mulligan to draw again");
                break;
            case "NoThankYou":
                UseNoThankYou();
                DisplayMessage(false, "Used No, Thank You. Select which card to redraw");
                break;
            case "DamageBoost":
                _isDamageBoostOn = true;
                DisplayMessage(false, "Used Damage Boost. Your next winning combination will deal 1 extra damage per card");
                break;
        }
        GameManager.Instance.inventory.RemoveAt(buttonIndex);
        CloseInventory();
    }
    public void OpenInventory()
    {
        if (_isPlayerTurn)
        {
            inventoryScreen.SetActive(true);
            for (var i = 0; i < GameManager.Instance.inventory.Count; i++)
            {
                inventorySlots[i].sprite = consumableSprites[spriteMap[GameManager.Instance.inventory[i]]];
                inventorySlots[i].color = Color.white;
                inventorySlots[i].gameObject.GetComponent<Button>().enabled = true;
            }
            for (int i = GameManager.Instance.inventory.Count; i < 10; i++)
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].color = Color.clear;
                inventorySlots[i].gameObject.GetComponent<Button>().enabled = false;
            }
        }
    }

    public void CloseInventory()
    {
        inventoryScreen.SetActive(false);
    }

    public void OpenMenu()
    {
        menuScreen.SetActive(true);
    }
    public void CloseMenu()
    {
        menuScreen.SetActive(false);
    }

    public void AbandonRun()
    {
        CloseMenu();
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene("MenuScene");
    }
    public void ChangeVolume()
    {
        GameManager.Instance.gameVolume = gameVolumeSlider.value;
        musicBox.volume = GameManager.Instance.gameVolume;
    }

    private void UseNoThankYou()
    {
        foreach (var carte in player.main)
        {
            carte.Enabled(false);
        }
        foreach (var button in buttons)
        {
            button.SetActive(false);
        }
        _isNoThankYou = true;
        isEnemyCardsRevealed = true;
        if (flippedCards != null)
        {
            foreach (var flippedCard in flippedCards)
            {
                Destroy(flippedCard);
            }
        }
        foreach (var card in enemyHand)
        {
            card.gameObject.GetComponent<Button>().enabled = true;
        }
        
    }

    public void NoThankYouCard(int cardIndex)
    {
        if (_isNoThankYou)
        {
            Discard(enemyHand[cardIndex]);
            List<Carte> toDraw = new List<Carte>();
            toDraw.Add(enemyHand[cardIndex]);
            StartCoroutine(DrawCards(toDraw));
            EndNoThankYou();
        }
    }

    private void EndNoThankYou()
    {
        foreach (var carte in player.main)
        {
            carte.Enabled(true);
        }
        foreach (var button in buttons)
        {
            button.SetActive(true);
        }
        foreach (var card in enemyHand)
        {
            card.gameObject.GetComponent<Button>().enabled = false;
        }
        _isNoThankYou = false;
    }

    public void PlaySound(AudioClip clip)
    {
        AudioSource audioSource = Instantiate(soundEffects, transform);
        audioSource.volume = GameManager.Instance.gameVolume;
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(audioSource, 2f);
    }
}