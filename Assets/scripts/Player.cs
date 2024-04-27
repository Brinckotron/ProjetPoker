using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private int _gold;
    private int _hp;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private AudioSource soundEffects;
    [SerializeField] private AudioClip coinSounds;
    private List<CarteData> _playerDeck;
    private List<CarteData> _playerDiscard;
    public List<Carte> comboCards;
    [SerializeField] private Carte[] main;
    private Vector3[] _mainPositions;
    private Quaternion[] _mainRotations;
    [SerializeField] private Transform comboCardsTransform;
    [SerializeField] private TMP_Text drawButtonText;
    private int _drawCount = 0;
    public int maxDrawCount = 1;
    public enum Combos
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

    [SerializeField] public Combos activeCombo;

    public int Gold
    {
        get => _gold;
        set => _gold = value;
    }

    private void Awake()
    {
        BuildDeck();
        ShuffleDeck();
        Deal();
        _playerDiscard = new List<CarteData>();
        _mainPositions = new Vector3[5];
        _mainRotations = new Quaternion[5];
        for (int i = 0; i < main.Length; i++)
        {
            _mainPositions[i] = main[i].transform.position;
            _mainRotations[i] = main[i].transform.rotation;
        }
    }

    public void BuildDeck()
    {
        _playerDeck = new List<CarteData>();
        for (int i = 1; i <= 13; i++)
        {
            _playerDeck.Add(new CarteData(i, Symbole.Pique));
            _playerDeck.Add(new CarteData(i, Symbole.Trefle));
            _playerDeck.Add(new CarteData(i, Symbole.Coeur));
            _playerDeck.Add(new CarteData(i, Symbole.Carreau));
        }
    }

    public void ShuffleDeck()
    {
        var temp = new List<CarteData>();
        while (_playerDeck.Count > 0)
        {
            int indexRandom = Random.Range(0, _playerDeck.Count);
            temp.Add((_playerDeck[indexRandom]));
            _playerDeck.RemoveAt(indexRandom);
        }

        _playerDeck = temp;
    }

    public void ReshuffleDeck()
    {
        for (int i = 0; i < _playerDiscard.Count; i++)
        {
            _playerDeck.Add(_playerDiscard[0]);
            _playerDiscard.RemoveAt(0);
        }

        ShuffleDeck();
    }

    public bool KeepCards()
    {
        bool canDraw;
        int keptCards = 0;
        foreach (var carte in main)
        {
            if (carte.isKept && !carte.Data.isFree)
                keptCards++;
        }

        List<Carte> selectedCards = new List<Carte>();
        foreach (var carte in main)
        {
            if (carte.isSelected && !carte.Data.isFree)
            {
                selectedCards.Add(carte);
            }
        }

        if ((selectedCards.Count + keptCards) <= 2)
        {
            foreach (var carte in main)
            {
                if (carte.isSelected) carte.Keep();
            }

            canDraw = true;
        }
        else
        {
            battleManager.DisplayMessage(false, "You cannot keep that many cards");
            canDraw = false;
        }

        return canDraw;
    }

    public void SelectCard(Carte card)
    {
        if (battleManager.IsPlayerTurn && _drawCount < maxDrawCount)
        {
            card.Select();

            int selectedCards = 0;
            for (int i = 0; i < 5; i++)
            {
                if (main[i].isSelected) selectedCards++;
            }

            if (selectedCards > 0) drawButtonText.text = "KEEP & DRAW";
            else drawButtonText.text = "DRAW";
        }
    }


    public void Draw()
    {
        if (_drawCount < maxDrawCount)
        {
            List<Carte> toDraw = new List<Carte>();
            List<Carte> toDiscard = new List<Carte>();
            for (int i = 0; i < 5; i++)
            {
                if (main[i].isSelected) main[i].Select();
                if (!main[i].isKept)
                {
                    toDiscard.Add(main[i]);
                    toDraw.Add(main[i]);
                }
                else
                {
                    main[i].Unkeep();
                }
            }

            StartCoroutine("DrawCards", toDraw);
            StartCoroutine("DiscardCards", toDiscard);
            _drawCount++;
            drawButtonText.text = "DRAW";
        }
        else
        {
            battleManager.DisplayMessage(false, "You cannot draw anymore this turn.");
        }
    }
    
    public void DrawButton()
    {
        if (battleManager.IsPlayerTurn)
        {
            if(KeepCards()) Draw();
        }
    }

    public IEnumerator DrawCards(List<Carte> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            if (_playerDeck.Count == 0) ReshuffleDeck();

            cards[i].Draw();
            cards[i].SetData(_playerDeck[0]);
            _playerDeck.RemoveAt(0);
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

    public void Deal()
    {
        StartCoroutine("DrawCards", main.ToList());
    }

    public void Discard(Carte card)
    {
        _playerDiscard.Add(card.Data);
        card.Discard();
    }

    public void GainGold(int goldAmount)
    {
        Gold += goldAmount;
        soundEffects.clip = coinSounds;
        soundEffects.Play();
    }

    public void EndTurn()
    {
        if (battleManager.IsPlayerTurn)
        {
            StartCoroutine(nameof(ResolveTurn));
            battleManager.Resolve();
        }
    }

    private IEnumerator ResolveTurn()
    {
        battleManager.IsPlayerTurn = false;
        CheckForCombos();
        List<Vector3> comboCardsPositions = new List<Vector3>();
        foreach (var carte in main)
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

        yield return new WaitForSeconds(5f);

        foreach (var comboCard in comboCards)
        {
            Discard(comboCard);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        ResetTurn();
    }

    private void CheckForCombos()
    {
        comboCards = new List<Carte>();

        //straight
        int[] tempValues = new int[5] 
            {main[0].Data.valeur, main[1].Data.valeur, main[2].Data.valeur, main[3].Data.valeur, main[4].Data.valeur};
        Array.Sort(tempValues);
        if (tempValues[0] == tempValues[1] - 1 &&
            tempValues[0] == tempValues[2] - 2 &&
            tempValues[0] == tempValues[3] - 3 &&
            tempValues[0] == tempValues[4] - 4)
        {
            foreach (var carte in main)
            {
                if (!comboCards.Contains(carte))comboCards.Add(carte);
            }
        }


        //flush
        if (main[0].Data.symbole == main[1].Data.symbole &&
            main[0].Data.symbole == main[2].Data.symbole &&
            main[0].Data.symbole == main[3].Data.symbole &&
            main[0].Data.symbole == main[4].Data.symbole)
        {
            foreach (var carte in main)
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
                if (main[i].Data.valeur == main[j].Data.valeur)
                {
                    if (!comboCards.Contains(main[i])) comboCards.Add(main[i]);
                    if (!comboCards.Contains(main[j])) comboCards.Add(main[j]);
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
                if (main[i].Data.valeur == 1)
                {
                    indexHighest = i;
                    break;
                }

                if (main[i].Data.valeur > tempValue)
                {
                    tempValue = main[i].Data.valeur;
                    indexHighest = i;
                }
            }

            comboCards.Add(main[indexHighest]);
        }
        
        //Set ActiveCombo
        switch (comboCards.Count)
        {
            case 1:
                activeCombo = Combos.HighCard;
                break;
            case 2:
                activeCombo = Combos.Pair;
                break;
            case 3:
                activeCombo = Combos.ThreeOfAKind;
                break;
            case 4:
                if (comboCards[0].Data.valeur == comboCards[1].Data.valeur &&
                    comboCards[0].Data.valeur == comboCards[2].Data.valeur &&
                    comboCards[0].Data.valeur == comboCards[3].Data.valeur)
                {
                    activeCombo = Combos.FourOfAKind;
                }
                else
                {
                    activeCombo = Combos.TwoPairs;
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
                    if (main[0].Data.symbole == main[1].Data.symbole &&
                        main[0].Data.symbole == main[2].Data.symbole &&
                        main[0].Data.symbole == main[3].Data.symbole &&
                        main[0].Data.symbole == main[4].Data.symbole)
                    {
                        if (tempValues[0] == 1 && tempValues[4] == 13)
                        {
                            activeCombo = Combos.RoyalFlush;
                        }
                        else
                        {
                            activeCombo = Combos.StraightFlush;
                        }
                    }
                    else
                    {
                        activeCombo = Combos.Straight;
                    }
                }
                else if (main[0].Data.symbole == main[1].Data.symbole &&
                         main[0].Data.symbole == main[2].Data.symbole &&
                         main[0].Data.symbole == main[3].Data.symbole &&
                         main[0].Data.symbole == main[4].Data.symbole)
                {
                    activeCombo = Combos.Flush;
                }
                else
                {
                    activeCombo = Combos.FullHouse;
                }

                break;
        }
    }

    public void ResetTurn()
    {
        _drawCount = 0;
        for (var i = 0; i < main.Length; i++)
        {
            main[i].transform.position = _mainPositions[i];
            main[i].transform.rotation = _mainRotations[i];
        }

        for (int i = 0; i < 5; i++)
        {
            if (main[i].isSelected) main[i].Select();
            if (main[i].isKept) main[i].Unkeep();
        }

        Deal();
    }
}