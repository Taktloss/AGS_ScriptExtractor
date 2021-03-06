﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AGS
{
    class AGSTools
    {
        /// <summary>
        /// Parse AGS file and outputs the found Scripts
        /// </summary>
        /// <param name="filename"></param>
        public static void ParseAGSFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                Console.WriteLine("Start extracting scripts from " + Path.GetFileName(filename));

                //The string we want to search in the AGS Game executable
                const string searchString = "SCOMY";

                //Set BlockSize for reading
                const int blockSize = 1024;
                long fileSize = fs.Length;
                long position = 0;

                //Read AGS EXE and search for string, should actually never reach the end 
                BinaryReader br = new BinaryReader(fs);

                //List for SCOMY Header start offsets
                List<int> SCOMY_Positions = new List<int>();

                //Read through file
                while (position < fileSize)
                {
                    //Read data with set BlockSize
                    byte[] dataBlock = br.ReadBytes(blockSize);
                    string tempDataBlock = Encoding.Default.GetString(dataBlock);

                    //If the search string is found add new File offset in List
                    if (tempDataBlock.Contains(searchString))
                    {
                        //Get Position value in the dataBlock
                        int pos = tempDataBlock.IndexOf(searchString, 0);
                        //Add new File offset current position + position in tempDataBlock
                        SCOMY_Positions.Add(pos + (int)position);
                    }
                    //Calculate new actual postiton to continue reading
                    position = position + blockSize;
                }

                //Get all Text Lines
                List<string> lines = new List<string>();
                foreach (int scomyPos in SCOMY_Positions)
                {
                    fs.Position = scomyPos + 0x08; //Dont Read the SCOMY part

                    //Read byte length between header and table
                    int dummyLength = br.ReadInt32();
                    //Read count table entrys - each entry is 4 bytes
                    int countEntrys = br.ReadInt32();
                    //Read Script Text Length - starts at __NEWSCRIPT
                    int scriptLength = br.ReadInt32();
                    //Calculate Text Postion and jump to it
                    fs.Position = fs.Position + dummyLength + (countEntrys * 4);

                    //Get the Text as bytes
                    byte[] testData = br.ReadBytes(scriptLength);
                    //Replace 0x00 with 0x0D0A0D0A = two line breaks
                    byte[] newTestData = Replace(testData, new byte[] { 0x00 }, new byte[] { 0x0D, 0x0A, 0x0D, 0x0A });
                    string sData = Encoding.ASCII.GetString(newTestData);
                    sData = Regex.Replace(sData, "__[A-Z]+.+(.ash)", "");
                    sData = Regex.Replace(sData, @"^\s+[\r\n]", "", RegexOptions.Multiline);
                    sData = Regex.Replace(sData, @"[\n\r]", "\r\n", RegexOptions.Multiline);
                    //sData = sData.Replace("__NEWSCRIPT", "//__NEWSCRIPT");

                    lines.Add(sData);
                }
                Console.WriteLine("Found " + lines.Count + " entrys.");
                //Write Text List to a trs file
                File.WriteAllLines(Path.ChangeExtension(filename, ".trs"), lines);
                Console.WriteLine("Script extracted to " + Path.ChangeExtension(filename,".trs"));
            }
        }

        /// <summary>
        /// Replace a byte sequence with another one
        /// </summary>
        /// <param name="input">Input byte array</param>
        /// <param name="pattern">byte/s to search</param>
        /// <param name="replacement">new byte array to insert</param>
        /// <returns></returns>
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
