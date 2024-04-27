using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    private bool _isPlayerTurn;
    private int _enemyHP;
    [SerializeField] private Carte[] enemyHand;
    private List<CarteData> _enemyDeck;
    private List<CarteData> _enemyDiscard;
    [FormerlySerializedAs("comboCards")] public List<Carte> comboCards;
    [SerializeField] private TMP_Text messageBox;
    private bool _isDisplayingMsg = false;
    private Vector3[] _enemyHandPositions;
    [SerializeField] private Transform comboCardsTransform;
    [SerializeField] private Player player;

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
        BuildDeck();
        ShuffleDeck();
        _enemyDiscard = new List<CarteData>();
        Deal();
        _enemyHandPositions = new Vector3[5];
        for (int i = 0; i < enemyHand.Length; i++)
        {
            _enemyHandPositions[i] = enemyHand[i].transform.position;
        }

        StartCoroutine(nameof(InitializePlayerTurn));
    }

    public IEnumerator InitializePlayerTurn()
    {
        yield return new WaitForSeconds(1f);
        _isPlayerTurn = true;
    }

    public void ResetTurn()
    {
        for (var i = 0; i < enemyHand.Length; i++)
        {
            enemyHand[i].transform.position = _enemyHandPositions[i];
        }
        Deal();
        StartCoroutine(nameof(InitializePlayerTurn));
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

    public void CheckForCombos()
    {
        comboCards = new List<Carte>();

        //straight
        int[] tempValues = new int[5] 
            {enemyHand[0].Data.valeur, enemyHand[1].Data.valeur, enemyHand[2].Data.valeur, enemyHand[3].Data.valeur, enemyHand[4].Data.valeur};
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
                if (!comboCards.Contains(carte))comboCards.Add(carte);
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
                if (!comboCards.Contains(carte))comboCards.Add(carte);
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
        //DisplayMessage($"Player: {}");
        
        yield return new WaitForSeconds(5f);

        foreach (var comboCard in comboCards)
        {
            Discard(comboCard);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        ResetTurn();
    }

    public void DisplayMessage(string message)
    {
        if (_isDisplayingMsg) StopCoroutine("MessageFade");
        messageBox.text = message;
        StartCoroutine("MessageFade");
    }

    public IEnumerator MessageFade()
    {
        _isDisplayingMsg = true;
        for (float alpha = 100f; alpha >= 0; alpha--)
        {
            messageBox.alpha = alpha / 100;
            yield return new WaitForSeconds(0.05f);
        }

        _isDisplayingMsg = false;
    }
}