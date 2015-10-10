using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Message;
using System.Timers;
using System.IO;

class Client
{
 
    public string ID
    {
        get;
        private set;
    }

    public string oponenID
    {
        get;
        private set;
    }


    public IPEndPoint EndPoint
    {
        get;
        private set;
    }


    Socket sck;

    public Client(Socket accepted)
    {
        sck = accepted;
        ID = Guid.NewGuid().ToString();

        EndPoint = (IPEndPoint)sck.RemoteEndPoint;
        sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);


        confirmConnect();
    }

    public void setOponen(string opID)
    {
        output.outToScreen("matching players");
        oponenID = opID;
        message mes = new message("startMatch");
        controlerPlayers.sendMessageToClient(mes, oponenID);
    }

    public void unsetOponen()
    {
        oponenID = "";
    }

    private void confirmConnect()
    {
        message mes = new message("confirmConnect");
        NetObject identity = new NetObject("player");
        mes.addNetObject(identity);
        sendMessage(mes);

    }


    void callback(IAsyncResult AR)
    {

        sck.ReceiveBufferSize = 512;
        try
        {
            sck.EndReceive(AR);

            while (sck.Connected)
            {
                byte[] sizeInfo = new byte[4];

                int bytesRead = 0, currentRead = 0;

                currentRead = bytesRead = sck.Receive(sizeInfo);

                while (bytesRead < sizeInfo.Length && currentRead > 0)
                {
                    currentRead = sck.Receive(sizeInfo, bytesRead, sizeInfo.Length - bytesRead, SocketFlags.None);
                    bytesRead += currentRead;
                }

                int messageSize = BitConverter.ToInt32(sizeInfo, 0);
                byte[] incMessage = new byte[messageSize];

                bytesRead = 0;
                currentRead = bytesRead = sck.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);

                while (bytesRead < messageSize && currentRead > 0)
                {
                    currentRead = sck.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);
                    bytesRead += currentRead;
                }

                try
                {
                    if (incMessage != null)
                    {

                        Received(this, incMessage);

                    }
                }
                catch { }
            }



            sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
        }
        catch (Exception ex)
        {
            output.outToScreen(ex.Message + " erreur");

            if (Disconnected != null)
            {
                Disconnected(this);
            }
        }
    }

    public void Close()
    {
        //controler_players.remove_player(ID);
        sck.Close();
        sck.Dispose();
    }




    public delegate void ClientReveivedHandeler(Client sender, byte[] data);
    public delegate void ClientDisconectHandeler(Client sender);

    public event ClientReveivedHandeler Received;
    public event ClientDisconectHandeler Disconnected;

    internal void handleMessage(message mes)
    {
        switch (mes.messageText)
        {
            /*----------------------------------------------------------------------------------------------------*/
            case "queueMatch":
                controlerPlayers.Queud(this);
                output.outToScreen("mess queud");
                break;
     
            /*----------------------------------------------------------------------------------------------------*/
            default:
                output.outToScreen("The client sent a message: " + conversionTools.convertMessageToString(mes));
                break;
        }
    }

 



    public void sendMessage(message mes)
    {
        //output.outToScreen(mes.id.ToString());
        try
        {

            MemoryStream ms = new MemoryStream();
            byte[] _buffer = conversionTools.convertMessageToBytes(mes);
            byte[] lenght = BitConverter.GetBytes(_buffer.Length);
            ms.Write(lenght, 0, lenght.Length);
            ms.Write(_buffer, 0, _buffer.Length);
            ms.Close();

            byte[] data = ms.ToArray();
            ms.Dispose();
            sck.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);

        }
        catch { }
    }



    private void SendCallBack(IAsyncResult AR)
    {
        try
        {
            if (sck.Connected)
            {
                sck.EndSend(AR);
            }
        }
        catch { }
    }



}

