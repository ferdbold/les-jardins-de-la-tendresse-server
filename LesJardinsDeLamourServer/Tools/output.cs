using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class output
{
    public static void outToScreen(string text)
    {
        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + "- " + text);
    }
}
