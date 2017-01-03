using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGS_ScriptExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length >= 1 )
            {
                string filename = args[0];
                Console.WriteLine("Start extracting scripts from " + filename);
                ParseAGSFile(filename);
            }
            else
            {
                Console.WriteLine("Extractor needs a argument\n" +
                    Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location) + 
                    " <AGS GAME EXE>");
            }
        }


        public static void ParseAGSFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                //The string we want to search in the AGS Game executable
                const string searchString = "SCOMY";

                const int blockSize = 1024;
                long fileSize = fs.Length;
                long position = 0;

                //Read AGS EXE and search for string, should actually never reach the end 
                BinaryReader br = new BinaryReader(fs);

                //List with SCOMY offsets
                List<int> SCOMY_Positions = new List<int>();

                //Read through file BlockSize see above
                while (position < fileSize)
                {
                    byte[] data = br.ReadBytes(blockSize);
                    string tempData = Encoding.Default.GetString(data);

                    //If the search string is found get the game info
                    if (tempData.Contains(searchString))
                    {
                        int pos = tempData.IndexOf(searchString, 0);
                        SCOMY_Positions.Add((int)position + pos);

                        //Calculate and set the position to start reading
                        pos = pos + (int)position;
                    }
                    //Calculate new postiton
                    position = position + blockSize;
                }

                List<string> texte = new List<string>();

                foreach (int scomyPos in SCOMY_Positions)
                {
                    fs.Position = scomyPos + 0x08; //Dont Read the SCOMY part

                    //Read byte length between header and table
                    int dummyLength = br.ReadInt32();
                    //Read count table entrys each entry 4bytes
                    int countEntrys = br.ReadInt32();
                    //Read Script Text Length starts at __NEWSCRIPT
                    int scriptLength = br.ReadInt32();
                    //Go to Position where Text starts
                    fs.Position = fs.Position + dummyLength + (countEntrys * 4);
                    
                    //Read the text
                    byte[] testData = br.ReadBytes(scriptLength);
                    byte[] newTestData = Replace(testData, new byte[] { 0x00 }, new byte[] { 0x0D, 0x0A, 0x0D, 0x0A });
                    
                    texte.Add(Encoding.ASCII.GetString(newTestData));
                }

                File.WriteAllLines(filename + ".txt", texte);
            }
        }

        private static byte[] Replace(byte[] input, byte[] pattern, byte[] replacement)
        {
            if (pattern.Length == 0)
            {
                return input;
            }

            List<byte> result = new List<byte>();
            int i;

            for (i = 0; i <= input.Length - pattern.Length; i++)
            {
                bool foundMatch = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (input[i + j] != pattern[j])
                    {
                        foundMatch = false;
                        break;
                    }
                }

                if (foundMatch)
                {
                    result.AddRange(replacement);
                    i += pattern.Length - 1;
                }
                else
                {
                    result.Add(input[i]);
                }
            }

            for (; i < input.Length; i++)
            {
                result.Add(input[i]);
            }

            return result.ToArray();
        }

    }
}
