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

public class CarteData : IComparable<CarteData>
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
    [SerializeField] private Image core;
    [SerializeField] private Transform deckTransform;
    [SerializeField] private Transform discardTransform;
    [SerializeField] private GameObject CarteVerso;
    private CarteData _carteData;
    public bool isSelected = false;
    public bool isKept = false;
    public bool isFree = false;
    public int goldValue = 0;
    private Vector3 _destinationPosition;
    private Quaternion _destinationRotation;
    private Vector3 _originPosition;
    private Quaternion _originRotation;

    public CarteData Data
    {
        get { return _carteData; }
    }

    private void Awake()
    {
        frame.enabled = false;
        core.enabled = false;
        symboleText.enabled = false;
        valeurText.enabled = false;
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

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        _originPosition = transform.position;
        _originRotation = transform.rotation;
        _destinationPosition = position;
        _destinationRotation = rotation;
        StartCoroutine("Move");
    }

    public IEnumerator Move()
    {
        for (float i = 0; i < 21; i++)
        {
            transform.position = Vector2.Lerp(_originPosition, _destinationPosition, i / 20);
            transform.rotation = Quaternion.Lerp(_originRotation, _destinationRotation, i / 20);
            yield return new WaitForSeconds(0.05f);
        }
    }


    public void Draw()
    {
        StartCoroutine(DrawAnimation());
    }

    public void Discard()
    {
        StartCoroutine(DiscardAnimation());
    }

    public IEnumerator DrawAnimation()
    {
        GameObject carteVerso = Instantiate(CarteVerso, deckTransform);

        for (float i = 0; i < 11; i++)
        {
            carteVerso.transform.position = Vector2.Lerp(deckTransform.position, transform.position, i / 10);
            carteVerso.transform.rotation = Quaternion.Lerp(deckTransform.rotation, transform.rotation, i / 10);
            yield return new WaitForSeconds(0.05f);
        }

        frame.enabled = true;
        core.enabled = true;
        symboleText.enabled = true;
        valeurText.enabled = true;
        Destroy(carteVerso);
    }

    public IEnumerator DiscardAnimation()
    {
        GameObject carteVerso = Instantiate(CarteVerso, transform);
        frame.enabled = false;
        core.enabled = false;
        symboleText.enabled = false;
        valeurText.enabled = false;
        for (float i = 0; i < 11; i++)
        {
            carteVerso.transform.position = Vector2.Lerp(transform.position, discardTransform.position, i / 10);
            carteVerso.transform.rotation = Quaternion.Lerp(transform.rotation, discardTransform.rotation, i / 10);
            yield return new WaitForSeconds(0.05f);
        }

        Destroy(carteVerso);
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