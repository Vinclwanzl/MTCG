using MTCG;
using System;
using System.Collections;
using System.Collections.Generic;

public class Shop
{
	public Shop()
	{
    }
    public bool SellCard(User seller, Card cardToSell, int amountOfCards)
    {
        // TODO: check if player has the card and the amount of cards he wants to sell
        for (int i = 0; i < amountOfCards; i++)
        {
            if (seller.Stack.Contains(cardToSell)){
                seller.Stack.Remove(cardToSell);
                seller.CoinPurse += cardToSell.Cost;
            }
        } 
            
                // TODO identify the card and remove it out of the stack
                //_stack.Remove();
            
        return true;
            
    }
    /*
	public void BuyPackage(User buyer) 
	{ 
		// TODO: check if buyer has 5 coins
		Package package = new Package();
		string nameOfCard = "FeuFeu";

        package.chooseACardToToss(nameOfCard);
        
		// TODO: add the chosen cards to the buyer.Stack
		/*
		foreach (Card c in package.Content)
        {
            buyer.Stack.Add();
        }
		*/
}
