using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum Symbole
{
    Coeur,
    Pique,
    Carreau,
    Trefle
}

public class CarteData: IComparable<CarteData>
{
    public int valeur;
    public Symbole symbole;

    public CarteData(int valeur, Symbole symbole)
    {
        this.valeur = valeur;
        this.symbole = symbole;
    }

    public int CompareTo(CarteData other)
    {
        return valeur.CompareTo(other.valeur);
    }
}

public class Carte : MonoBehaviour
{
    [SerializeField] private TMP_Text valeurText;
    [SerializeField] private TMP_Text symboleText;
    
    

    internal void SetData(CarteData carteData)
    {
        string valeurString = carteData.valeur.ToString();
        switch (carteData.valeur)
        {
            case 1:
                valeurString = "A";
                break;
            case 11:
                valeurString = "J";
                break;
            case 12:
                valeurString = "Q";
                break;
            case 13:
                valeurString = "K";
                break;
        }
        
        string symboleString = "";
        switch (carteData.symbole)
        {
            case Symbole.Carreau:
                symboleString = "\u2666";
                symboleText.color = Color.red;
                valeurText.color = Color.red;
                break;
            case Symbole.Coeur:
                symboleString = "\u2665";
                symboleText.color = Color.red;
                valeurText.color = Color.red;
                break;
            case Symbole.Pique:
                symboleString = "\u2660";
                symboleText.color = Color.black;
                valeurText.color = Color.black;
                break;
            case Symbole.Trefle:
                symboleString = "\u2663";
                symboleText.color = Color.black;
                valeurText.color = Color.black;
                break;
        }
        
        valeurText.text = valeurString;
        symboleText.text = symboleString;
    }
}