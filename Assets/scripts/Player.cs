using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
   private int _gold;
   [SerializeField] private AudioSource soundEffects;
   [SerializeField] private AudioClip coinSounds;
   private List<CarteData> _playerDeck;
   private List<CarteData> _playerDiscard;
   [SerializeField] private Carte[] main;
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
      while (_playerDiscard.Count > 0)
      {
         _playerDiscard.RemoveAt(0);
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
      Debug.Log("Reshuf");
      while (_playerDiscard.Count > 0)
      {
         _playerDeck.Add(_playerDiscard[0]);
         _playerDiscard.RemoveAt(0);
      }
      ShuffleDeck();
   }

   public void KeepCards()
   {
      foreach (var carte in main)
      {
         if (carte.isSelected) carte.Keep();
      }
   }

   public void SelectCard(Carte card)
   {
      card.Select();
   }

   public void Draw()
   {
      for (int i = 0; i < 5; i++)
      {
         if (!main[i].isKept)
         {
            Discard(main[i]);
            if (_playerDeck.Count == 0) ReshuffleDeck();
            main[i].SetData(_playerDeck[0]);
            _playerDeck.RemoveAt(0);
         }
         else
         {
            main[i].Unkeep();
         }
      }
   }

   public void Discard(Carte card)
   {
      _playerDiscard.Add(card.Data);
   }

   public void GainGold(int goldAmount)
   {
      Gold += goldAmount;
      soundEffects.clip = coinSounds;
      soundEffects.Play();
   }
   
}