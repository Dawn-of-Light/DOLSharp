/*
 * MIGCleaner.cs
 * Xarik
 * v1.0.1
 *
*/

using System;
using System.IO; //.Directory;
using System.Threading;


class MIGCleaner
{
    public static void Main(string[] args)
    {
        Console.Write("Effacer aussi le repertoire release ? ");
        string r = Console.ReadLine();
        Console.Write("Votre choix: {0}\n\n", r);
        //Console.Write("Taper Entrer pour valider si non faite Ctrl+C\n\n");


     //   clean("debug");
        clean("DOLBase\\obj");
        clean("DOLConfig\\obj");
        clean("DOLDataBase\\obj");
        clean("DOLServer\\obj");
        clean("GameServer\\obj");
        clean("GameServerScripts\\bin");
        clean("GameServerScripts\\obj");
        clean("UnitTests\\bin");
        clean("UnitTests\\obj");

        if(r.ToLower() == "y")
            clean("release");

        Console.Write("\n\nPress any key to continue . . . ");
        Console.ReadKey(true);
    }

    public static void clean(string dir)
    {
        if (Directory.Exists(dir.ToString()))
        {
            Console.Out.WriteLine("Deleting directory: " + dir + "\\");
            Directory.Delete(dir.ToString(), true);
        }
    }
}


