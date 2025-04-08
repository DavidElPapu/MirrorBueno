using Mirror;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LiarsBarGameManager : MonoBehaviour
{
    public NetworkManager nm;
    //0 rey, 1 reina, 2 ace, 3 joker
    private int playersLeft, totalRounds, selectedCard;
    private int[] cardsNumbers;
    private List<int> player1Cards, player2Cards, player3Cards, player4Cards;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cardsNumbers = new int[4];
        player1Cards = new List<int>();
        player2Cards = new List<int>();
        player3Cards = new List<int>();
        player4Cards = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetRound()
    {
        player1Cards.Clear();
        player2Cards.Clear();
        player3Cards.Clear();
        player4Cards.Clear();
        selectedCard = Random.Range(0, 3);
        switch (playersLeft)
        {
            case 4:
                cardsNumbers[0] = 6;
                cardsNumbers[1] = 6;
                cardsNumbers[2] = 6;
                cardsNumbers[3] = 2;
                break;
            case 3:
                cardsNumbers[0] = 4;
                cardsNumbers[1] = 4;
                cardsNumbers[2] = 4;
                cardsNumbers[3] = 2;
                cardsNumbers[selectedCard]++;
                break;
            case 2:
                cardsNumbers[0] = 3;
                cardsNumbers[1] = 3;
                cardsNumbers[2] = 3;
                cardsNumbers[3] = 1;
                break;
            case 1:
                break;
        }
        for (int i = 0; i < 4; i++)
        {
            for (int t = 0; t < cardsNumbers[i]; t++)
            {
                switch (Random.Range(0, playersLeft))
                {
                    case 0:
                        player1Cards.Add(i);
                        break;
                    case 1:
                        player2Cards.Add(i);
                        break;
                    case 2:
                        player3Cards.Add(i);
                        break;
                    case 3:
                        player4Cards.Add(i);
                        break;
                }
            }
        }
    }
}
