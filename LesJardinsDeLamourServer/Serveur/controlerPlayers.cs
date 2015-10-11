using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Message;


class controlerPlayers
{

    public static List<Client> playersConnected;
    private static Client awaitingMatchup = null;
    public controlerPlayers()
    {
        playersConnected = new List<Client>();
    }


    public static void Queud(Client player)
    {
        if (awaitingMatchup == null)
        {
            awaitingMatchup = player;
            player.setIsLead(true);
        }
        else
        {
            AddPlayer(player.ID);
            AddPlayer(awaitingMatchup.ID);
            player.setIsLead(false);
            
            player.setOponen(awaitingMatchup.ID);
            awaitingMatchup.setOponen(player.ID);

            awaitingMatchup = null;

        }
    }





    public static bool Unqueud(Client player)
    {
        if (awaitingMatchup == player)
        {
            awaitingMatchup = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void AddPlayer(string clientID)
    {
        for (int i = 0; i < serverMain.clients.Count; i++)
        {
            Client client = serverMain.clients[i];
            if (client.ID == clientID)
            {
                playersConnected.Add(client);

            }
        }
     
    }


    public static Client GetPlayer(string clientID)
    {
        
        for (int i = 0; i < playersConnected.Count; i++)
        {
            Client client = playersConnected[i];

            if (client.ID == clientID)
            {
                return client;
            }
        }
        return null;
    }

    public static void RemovePlayer(string clientID)
    {
        
        for (int i = 0; i < playersConnected.Count; i++)
        {
            Client client = playersConnected[i];

            if (client.ID == clientID)
            {
                if (client.oponenID != "")
                {
                    GetPlayer(client.oponenID).unsetOponen();
                }
                playersConnected.RemoveAt(i);
            }

        }

    }

    public static void sendMessageToMatch(message mes, string firstClientID, string secondClientID)
    {
        sendMessageToClient(mes, firstClientID);
        sendMessageToClient(mes, secondClientID);
    }

    public static void sendMessageToClient(message mes, string clientID)
    {
        for (int j = 0; j < playersConnected.Count; j++)
        {
            Client player = playersConnected[j];
            if (player.ID == clientID)
            {
                player.sendMessage(mes);
            }
        }
    }

}

