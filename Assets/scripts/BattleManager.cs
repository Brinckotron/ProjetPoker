using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private bool _isPlayerTurn;
    private int _enemyHP;
    [SerializeField] private Carte[] enemyHand;
    private List<CarteData> _enemyDeck;
    private List<CarteData> _enemyDiscard;
    [SerializeField] private TMP_Text messageBox;
    private bool _isDisplayingMsg = false;

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
        Draw();
        while (_enemyDiscard.Count > 0)
        {
            _enemyDiscard.RemoveAt(0);
        }

        _isPlayerTurn = true;
    }

    public void ResetTurn()
    {
        Draw();
        _isPlayerTurn = true;
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

    public void Draw()
    {
        List<Carte> toDraw = new List<Carte>();
        List<Carte> toDiscard = new List<Carte>();
        for (int i = 0; i < 5; i++)
        {
            if (!enemyHand[i].isKept)
            {
                if (_enemyDeck.Count == 0) ReshuffleDeck();
                if(!IsPlayerTurn) toDiscard.Add(enemyHand[i]);
                toDraw.Add(enemyHand[i]);
                enemyHand[i].SetData(_enemyDeck[0]);
                _enemyDeck.RemoveAt(0);
            }
            else
            {
                enemyHand[i].Unkeep();
            }
        StartCoroutine("DrawCards", toDraw);
        StartCoroutine("DiscardCards", toDiscard);
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
        _enemyDiscard.Add(card.Data);
        card.Discard();
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
            messageBox.alpha = alpha/100;
            yield return new WaitForSeconds(0.05f);
        }

        _isDisplayingMsg = false;
    }
}