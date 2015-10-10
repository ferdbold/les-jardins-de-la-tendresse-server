using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using Message;





class serverMain
{

    [DllImport("user32.dll")]
    static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
    [DllImport("user32.dll")]
    static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    internal const UInt32 SC_CLOSE = 0xF060;
    internal const UInt32 MF_GRAYED = 0x00000001;


    public static bool
        keepAlive = true,
        shutdownRdy = false,
        autoClose = false;


    public static int
        serverId = 1,
        clientPort = 80;

    private static Object lastId = new int();



    public static Timer
        shutdown;

    static Listener l;
    static controlerPlayers controlerPlayers;


    public static List<Client> clients = new List<Client>();

    private static Timer timeTimer = new Timer();
    private static Timer timer = new Timer();

    public static bool gameStarted = false;


    static void Main()
    {
        // start server

        IntPtr current = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        EnableMenuItem(GetSystemMenu(current, false), SC_CLOSE, MF_GRAYED);
        controlerPlayers = new controlerPlayers();

        l = new Listener(clientPort);
        l.SocketAccepted += new Listener.SocketAcceptedHandeler(l_SocketAccepted);
        l.Start();
        //keep allive
        while (keepAlive)
        {
            doCMD(Console.ReadLine().ToLower());
        }
    }




    public static void l_SocketAccepted(System.Net.Sockets.Socket e)
    {
        Client client = new Client(e);
        clients.Add(client);
        client.Received += new Client.ClientReveivedHandeler(client_Received);
        client.Disconnected += new Client.ClientDisconectHandeler(client_Disconnected);

        output.outToScreen(clients.Count() + " Utilisateurs");
    }

    public static void client_Disconnected(Client sender)
    {
        if (!controlerPlayers.Unqueud(sender))
        {
            controlerPlayers.RemovePlayer(sender.ID);
        }

        for (int i = 0; i < clients.Count; i++)
        {
            Client client = clients[i];

            if (client.ID == sender.ID)
            {
                client.Close();

                client = null;
                clients.RemoveAt(i);

                break;
            }
        }
    }

    public static int generatePlayerId()
    {
        lock (lastId)
        {
            lastId = (int)lastId + 1;
            return (int)lastId;
        }
    }

    public static void client_Received(Client sender, byte[] data)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            Client client = clients[i];
            if (client.ID == sender.ID)
            {
                client.handleMessage(conversionTools.convertBytesToMessage(data));
                break;
            }
        }
    }

    public static void sendClientMessage(Socket cSock, message mes)
    {
        try
        {
            // convert message into a byte array, wrap the message, then send it
            byte[] messageObject = conversionTools.convertObjectToBytes(mes);
            byte[] readyToSend = conversionTools.wrapMessage(messageObject);
            cSock.Send(readyToSend);
        }
        catch { }
    }


    /*---------------------------------------------------------------------------------*
     *------------------------------   Command List    --------------------------------*
     *---------------------------------------------------------------------------------*/
    private static void doCMD(string p)
    {
        switch (p)
        {
            /*----------------------------------------------------------------------------------------------------*/
            case "svr":
                output.outToScreen("'svr -c': Shutdown server");
                output.outToScreen("'svr -x': Close server");
                output.outToScreen("'svr -c-x': Shutdown server / Server Close");
                //output.outToScreen("svr portPolicy: Set server port for policy file");
                //output.outToScreen("svr port: Set server port communications file");
                break;
            /*----------------------------------------------------------------------------------------------------*/
            case "svr -c":
                servShutdownStart(false);
                break;
            /*----------------------------------------------------------------------------------------------------*/
            case "svr -x":
                close_svr();
                break;
            /*----------------------------------------------------------------------------------------------------*/
            case "svr -c-x":
            case "svr -x-c":
                if (clients.Count() == 0)
                {
                    servShutdownStart(true);
                }
                else
                {
                    output.outToScreen(clients.Count() + " Utilisateurs");                    
                }
                break;
            /*----------------------------------------------------------------------------------------------------*/
            case "users":
                output.outToScreen(clients.Count() + " Utilisateurs");
                break;

            /*----------------------------------------------------------------------------------------------------*/
            default:
                output.outToScreen("Unknow commande.");
                break;
        }
    }
    /*---------------------------------------------------------------------------------*
     *---------------------------------   Shutdown     --------------------------------*
     *---------------------------------------------------------------------------------*/

    private static void servShutdownStart(bool close)
    {
        if (!shutdownRdy)
        {
            //controlerPlayers.sendServerMessage("Shutdown in 2 secondes");
            autoClose = close;
            output.outToScreen("Processing to Shutdown.");
            shutdown = new Timer(2000);
            shutdown.Elapsed += new ElapsedEventHandler(_shutdownEnd);
            shutdown.Enabled = true;
        }
        else
        {
            output.outToScreen("Server Already shutdowned.");
            output.outToScreen("Use 'svr -x' to close.");
        }
    }
    /*----------------------------------------------------------------------------------------------------*/
    private static void _shutdownEnd(object sender, ElapsedEventArgs e)
    {
        //controlerPlayers.sendServerMessage("Shutdowned");
        output.outToScreen("Server shutdowned.");
        shutdownRdy = true;
        shutdown.Enabled = false;
        if (autoClose)
        {
            close_svr();
        }
    }

    /*----------------------------------------------------------------------------------------------------*/
    private static void close_svr()
    {
        if (shutdownRdy)
        {
            keepAlive = false;
        }
        else
        {
            output.outToScreen("Server Not shutdowned yet.");
        }
    }
}
