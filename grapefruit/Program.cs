using System;
using System.IO;
using Regex;

namespace grapefruit
{
    class Program
    {
        static void Main(string[] args)
        {
            Pattern pattern = new Pattern("");
            CheckFile(args[0],pattern);
        }

        static int CheckFile(string path, Pattern pattern,int outputMode = 0)
        {
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    string line;
                    //Iterate over file
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Print lines with matches
                        if (pattern.Check_expression(line) == 0 && outputMode == 0)
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
                return 0;
            }
            else
            {
                Console.WriteLine("Error: no file in: " + path);
                return 1;
            }
        }
    }
}
