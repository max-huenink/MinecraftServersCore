using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MinecraftServersCore
{
    static class Funcs
    {
        /// <summary>
        /// Uses Console.ReadLine() and checks if the entered data was null or empty
        /// Returns specified default value if empty
        /// </summary>
        /// <param name="def">The default value if input string is empty</param>
        /// <returns></returns>
        public static string NullEmpty(string def)
        {
            string value = Console.ReadLine();
            return String.IsNullOrEmpty(value) ? def : value;
        }

        /// <summary>
        /// Initializes a new instance of the StreamWriter class for the specified file by using the default encoding and buffer size.
        /// If the fize esits, it can be either overwritter or appended to.
        /// If the file does not exist, a new file is created.
        /// </summary>
        /// <param name="path">The path to write to</param>
        /// <param name="append">true to append data to the file; false to overwrite the file.
        /// If the specified file does not exist, this parameter has no effect, and the constructor creates a new file</param>
        /// <param name="textToWrite">The string to write to the file</param>
        public static void Write(string path, bool append, string textToWrite)
        {
            StreamWriter stream = new StreamWriter(path, append);
            stream.Write(textToWrite);
            stream.Close();
        }

        /// <summary>
        /// Prints a string in a prettier format from the specified IEnumerable<string>
        /// </summary>
        /// <param name="words">A collection of strings to be prettified</param>
        public static void PrettyPrint(IEnumerable<string> words)
        {
            if (words.Count() == 0)
            {
                Console.WriteLine("Empty");
                return;
            }
            int PadLength = words.Max(i => i.Length) + 10;
            int wordsPerLine = Console.WindowWidth / PadLength;
            // Prints all folders in the directory
            string printText = "";
            int wordsOnLine = 0;
            foreach (string word in words)
            {
                //Console.Write(Path.GetFileName(folder)+"\t");
                if (wordsOnLine == wordsPerLine)
                {
                    wordsOnLine = 0;
                    printText += "\n";
                }
                printText += word.PadRight(PadLength);
                wordsOnLine++;
            }
            Console.WriteLine(printText);
        }

        /// <summary>
        /// Validates integer in bounds from Console.ReadLine()
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="def">Default value</param>
        /// <returns></returns>
        public static int GetInt(int min, int max, int def)
        {
            int.TryParse(Console.ReadLine(), out int value);
            // If the read value is out of bounds, set default value
            if (value.CompareTo(min - 1) == 1 || value.CompareTo(max + 1) == -1)
            {
                return def;
            }
            return value;
        }

        /// <summary>
        /// Validates a string to a boolean with default value
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="def">The default value</param>
        /// <returns></returns>
        public static bool ValidateBool(bool def)
        {
            string input = Console.ReadLine();
            // Return default value is the input is null or empty
            if (String.IsNullOrEmpty(input))
            {
                return def;
            }
            // If the input was true/yes, return true
            // If the input was false/no, return false
            switch (input.ToLower().First<char>())
            {
                case 't':
                case 'y':
                    return true;
                case 'f':
                case 'n':
                    return false;
                default:
                    return def;
            }
        }
    }
}
