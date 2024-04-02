using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarteManager : MonoBehaviour
{
    [SerializeField] private Carte[] main = new Carte[5];

    private List<CarteData> deck = new List<CarteData>(52);

    private int indexCarte = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; j <= 13; j++)
            {
                deck.Add(new CarteData(j, (Symbole)i));
            }
        }

        var temp = new List<CarteData>();
        while (deck.Count > 0)
        {
            int indexRandom = Random.Range(0, deck.Count);
            temp.Add((deck[indexRandom]));
            deck.RemoveAt(indexRandom);
        }

        deck = temp;
        Piger();
    }

    public void Piger()
    {
        for (int i = 0; i < 5; i++)
        {
            main[i].SetData(deck[indexCarte++]);
        }
    }

}
