using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
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
    private List<Carte> comboCards;
    [SerializeField] private Carte[] main;
    private Vector3[] _mainPositions;
    private Quaternion[] _mainRotations;
    [SerializeField] private Transform comboCardsTransform;
    private int _drawCount = 0;
    public int maxDrawCount = 1;

    public int Gold
    {
        get => _gold;
        set => _gold = value;
    }

    private void Awake()
    {
        BuildDeck();
        ShuffleDeck();
        _playerDiscard = new List<CarteData>();
        Draw();
        _drawCount = 0;
        while (_playerDiscard.Count > 0)
        {
            _playerDiscard.RemoveAt(0);
        }

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
        while (_playerDiscard.Count > 0)
        {
            _playerDeck.Add(_playerDiscard[0]);
            _playerDiscard.RemoveAt(0);
        }

        ShuffleDeck();
    }

    public void KeepCards()
    {
        int keptCards = 0;
        foreach (var carte in main)
        {
            if (carte.isKept && !carte.isFree)
                keptCards++;
        }

        List<Carte> selectedCards = new List<Carte>();
        foreach (var carte in main)
        {
            if (carte.isSelected && !carte.isFree)
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
        }
        else
        {
            battleManager.DisplayMessage("You cannot keep that many cards");
        }
    }

    public void SelectCard(Carte card)
    {
        if (battleManager.IsPlayerTurn && _drawCount < maxDrawCount) card.Select();
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
                    if (_playerDeck.Count == 0) ReshuffleDeck();
                    if(battleManager.IsPlayerTurn) toDiscard.Add(main[i]);
                    toDraw.Add(main[i]);
                    main[i].SetData(_playerDeck[0]);
                    _playerDeck.RemoveAt(0);
                }
                else
                {
                    main[i].Unkeep();
                }
            }

            StartCoroutine("DrawCards", toDraw);
            StartCoroutine("DiscardCards", toDiscard);
            _drawCount++;
        }
        else
        {
            battleManager.DisplayMessage("You cannot draw anymore this turn.");
        }
    }

    public IEnumerator DrawCards(List<Carte> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].Draw();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public IEnumerator DiscardCards(List<Carte> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].Discard();
            yield return new WaitForSeconds(0.1f);
        }
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
        StartCoroutine("ResolveTurn");
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
                carte.Discard();
                yield return new WaitForSeconds(0.1f);
            }
        }
        switch (comboCards.Count)
        {
            case 1:
                comboCardsPositions.Add(comboCardsTransform.position);
                break;
            case 2:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x-125f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x+125f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
            case 3:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x-250f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(comboCardsTransform.position);
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x+250f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
            case 4:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x-375f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x-125f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x+125f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x+375f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
            case 5:
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x-500f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x-250f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(comboCardsTransform.position);
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x+250f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                comboCardsPositions.Add(new Vector3(comboCardsTransform.position.x+500f, comboCardsTransform.position.y, comboCardsTransform.position.z));
                break;
        }
        for (var i = 0; i < comboCards.Count; i++)
        {
            comboCards[i].MoveTo(comboCardsPositions[i], comboCardsTransform.rotation);
        }
        yield return new WaitForSeconds(5f);
        
        foreach (var comboCard in comboCards)
        {
            comboCard.Discard();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        ResetTurn();
    }

    private void CheckForCombos()
    {
        comboCards = new List<Carte>();
        //pairs, triples and four of a kinds
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
    }
    public void ResetTurn()
    {
        _drawCount = 0;
        for (var i = 0; i < main.Length; i++)
        {
            main[i].transform.position = _mainPositions[i];
            main[i].transform.rotation = _mainRotations[i];
        }
        Draw();
        _drawCount = 0;
        battleManager.ResetTurn();
    }
}