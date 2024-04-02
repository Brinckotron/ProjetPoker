using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsCalculator : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        List<CarteData> pile = new List<CarteData>(52);
        int FlushCounter = 0;
        int PaireCounter = 0;
        for (int i = 1; i <= 13; i++)
        {
            pile.Add(new CarteData(i, Symbole.Pique));
            pile.Add(new CarteData(i, Symbole.Trefle));
            pile.Add(new CarteData(i, Symbole.Coeur));
            pile.Add(new CarteData(i, Symbole.Carreau));
        }

        List<List<CarteData>> combos = new List<List<CarteData>>();

        for (int a = 0; a < pile.Count; a++)
        {
            for (int b = a+1; b < pile.Count; b++)
            {
                for (int c = b+1; c < pile.Count; c++)
                {
                    for (int d = c+1; d < pile.Count; d++)
                    {
                        for (int e = d + 1; e < pile.Count; e++)
                        {
                            var combo = new List<CarteData>
                            {
                                pile[a], pile[b], pile[c], pile[d], pile[e]
                            };
                            combos.Add(combo);
                        }

                    }
                }
            }
        }

        
        foreach (var combo in combos)
        {
            //Flush
            if (combo[0].symbole == combo[1].symbole
                && combo[0].symbole == combo[2].symbole
                && combo[0].symbole == combo[3].symbole
                && combo[0].symbole == combo[4].symbole)
            {
                FlushCounter++;
                continue;
            }

            //Paire
            if (combo[0].valeur == combo[1].valeur
                || combo[1].valeur == combo[2].valeur
                || combo[2].valeur == combo[3].valeur
                || combo[3].valeur == combo[4].valeur)
            {
                PaireCounter++;
                continue;
            }

        }

        float pFlush = FlushCounter / (float)combos.Count;
        
        Debug.Log($"Combo count: {combos.Count} \n PFlush: {pFlush} ");
    }
}
