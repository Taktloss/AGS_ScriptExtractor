using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGS;

namespace AGS_ScriptExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                string filename = args[0];
                Console.WriteLine("AGS Script Extractor v0.2 by Taktloss");
                AGSTools.ParseAGSFile(filename);
                Console.ReadKey();
            }
            else printHelp();
                
        }

        static void printHelp()
        {
            Console.WriteLine("Extractor needs a AGS Game File as argument.");
            Console.WriteLine(Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location) +
                    " <AGS GAME EXE>");
        }

    }

}
