using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Copier
{
    class Program
    {
        static bool terminate = false;
        static void Main(string[] args)
        {
            //when you open it, it opens a console window and asks where you want to output to. 
            Console.WriteLine("Target save location:");
            string outputLoc = Console.ReadLine();

            //Once you've input the path, it opens a 'deposit' directory. 
            string inputLoc = "input";
            if (!Directory.Exists(inputLoc)) Directory.CreateDirectory(inputLoc);
            OpenFolder(inputLoc);

            Thread childThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //loop indefinitely, sleeping at the end of every loop
                while (!terminate)
                {
                    string fileName = Clipboard.GetText();
                    string[] files = Directory.GetFiles(inputLoc);
                    foreach(string file in files)
                    {
                        bool done = false;
                        string newPath = "";

                        while (!done)
                        {
                            newPath = GetPathInFolder(fileName, outputLoc)+Path.GetExtension(file);
                            if (File.Exists(newPath))
                            {
                                //If the target directory contains a file with that copied text, it increments and repeats this until it finds an empty spot. 
                                fileName = IncrementFileName(fileName);
                            }
                            else
                            {
                                done = true;
                                try
                                {
                                    //Files placed in the deposit directory are automatically moved to the target output directory, renaming to match the copied text. 
                                    File.Move(file, newPath);
                                    fileName = IncrementFileName(fileName);
                                }
                                catch { }
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(10);
                }
            });
            childThread.SetApartmentState(ApartmentState.STA);
            childThread.Start();

            //The user can terminate the copier by typing "end" into it.            
            do { Console.Clear(); Console.WriteLine("Input 'end' to terminate."); }
            while (Console.ReadLine().ToLower() != "end");
            terminate = true;

            System.Threading.Thread.Sleep(500);
        }
        private static string IncrementFileName(string file)
        {
            char[] numerals = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int lastNumberChar = file.Length;
            while (lastNumberChar > 0 && numerals.Contains(file[lastNumberChar - 1])) lastNumberChar--;

            string output = file;

            if (lastNumberChar != file.Length)
            {
                //so it adds 1 to the number if it ends in a number
                int numeral = Convert.ToInt32(output.Substring(lastNumberChar))+1;
                output = output.Substring(0, lastNumberChar) + numeral;
            }
            else
            {
                //otherwise adds " 1" to the end
                output += " 1";
            }

            return output;
        }
        private static string GetFileName(string file)
        {
            string[] split = file.Split(new char[] { '\\', '/' });
            if (split[split.Length-1].Length == 0)
            {
                return split[split.Length - 2];
            }
            return split[split.Length - 1];
        }
        private static string GetPathInFolder(string file, string folder)
        {
            string output = folder;
            if (!(folder[folder.Length - 1] == '\\' || folder[folder.Length-1] == '/'))
            {
                output += "\\";
            }
            return output + GetFileName(file);
        }
        private static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
        }
    }
}

