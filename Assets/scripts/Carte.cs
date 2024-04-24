using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.SpriteAssetUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Image frame;
    private CarteData _carteData;
    public bool isSelected = false;
    public bool isKept = false;

    public CarteData Data
    {
        get { return _carteData; }
    }

    public void Select()
    {
        if (!isKept)
        {
            if (!isSelected) frame.color = Color.yellow;
            else frame.color = Color.black;
            isSelected = !isSelected;
        }
    }

    public void Keep()
    {
        isSelected = false;
        isKept = true;
        frame.color = Color.green;
    }

    public void Unkeep()
    {
        isKept = false;
        frame.color = Color.black;
    }

    internal void SetData(CarteData carteData)
    {
        _carteData = carteData;
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