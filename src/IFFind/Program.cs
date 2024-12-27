/*
 MIT License

Copyright (c) Jamie Futch

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using iFFind;
using FstShared;

namespace IFFind
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Program
    {
        #region Locals
        private static readonly StringBuilder _resultsBuilder = new StringBuilder();
        private static long _count;

        private static bool _isWindowsOs;
        #endregion
        
        static void Main(string[] args)
        {
            _isWindowsOs = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var searchParams = GetParameters(args);
            FstStructs.IFFindSearchParametersToString(searchParams).PrintLine();

            
            SearchDirectoriesForFile(searchParams.SearchDirectory, searchParams.SearchText);
            $"Count: {_count}".PrintLine();
            
            if (!string.IsNullOrEmpty(searchParams.OutputFile))
            {
                SaveResultsToFile(searchParams);
            }
        }

        /// <summary>
        /// Searches directories for files that contain the search string
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <param name="searchString"></param>
        static void SearchDirectoriesForFile(string rootDirectory, string searchString)
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(FixPath(rootDirectory, _isWindowsOs)))
                {
                    SearchDirectoriesForFile(directory,searchString); // Recursively search subdirectories        
                }

                foreach (string file in Directory.GetFiles(rootDirectory))
                {
                    var tooLargeToScan = false;
                    var found = false;
                    FileInfo fileInfo = new FileInfo(file);
                    if(fileInfo.Length > 200 && fileInfo.Length < 10000)
                    {
                        var f = File.ReadAllText(file);
                        var trie = new Trie();
                        trie.Add(searchString);
                        trie.Build();
                        found = trie.Find(f).Any();
                        if (found)
                        {
                            _resultsBuilder.Append(SearchFile(file,SearchStringToArray(searchString)));
                            _count++;
                        }
                    }
                    else
                    { tooLargeToScan=true;}

                    if (tooLargeToScan || found)
                    {
                        _resultsBuilder.Append(SearchFile(file, SearchStringToArray(searchString)));
                    }
                    
                }
            }
            catch (Exception)
            {
                //Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        
        
        /// <summary>
        /// searches a file for a string
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        private static StringBuilder SearchFile(string FileName, string[] SearchString)
        {               
            StringBuilder s = new StringBuilder();
            int LineCount = 0;
            try
            {
                foreach (string line in File.ReadLines(FileName))
                {
                    var trie = new Trie();
                    trie.Add(SearchString);
                    trie.Build();
                    var found = trie.Find(line).Any();
                    if (found)
                    {
                        s.Append(LineCount);
                        s.Append("\t");
                        s.Append(FileName);
                        s.Append("\r\n");
                        s.ToString().PrintLine();
                    }
                    LineCount++;
                }
            }
            catch (Exception ex)
            {
                // ReSharper disable once UnusedVariable
                var msg = ex.Message;
            }
            return s;
        }

        private static string[] SearchStringToArray(string searchString)
        {

            return new string[] { searchString };
        }

        
        /// <summary>
        /// Fixes the path if it is missing a trailing backslash
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isWindowsOs"></param>
        /// <returns></returns>
        static string FixPath(string path, bool isWindowsOs)
        {
            var ret = path;

            if(!isWindowsOs)
            {
                if (path.EndsWith(":"))
                {
                    ret = path + "\\";
                }
            }
            else
            {
                if (path.EndsWith(":"))
                {
                    ret = path + "/";
                }
            }
            return ret;
        }

        /// <summary>
        /// parses the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static FstStructs.SearchParameters GetParameters(string[] args)
        {
            var sp = new FstStructs.SearchParameters();
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            try
            {
                sp.SearchDirectory = args[0];
                sp.SearchText = args[1];

                if (args.Length > 2)
                {
                    if (args[2].ToLower() == "-f")
                    {
                        if (args.Length > 3)
                        {
                            sp.SpecificFilename = args[3];
                        }
                    }
                    else if(args[2] == "-o")
                    {
                        if (args.Length > 3)
                        {
                            sp.OutputFile = args[3];
                        }
                    }
                    

                    if (args.Length > 4)
                    {
                        if (args[4].ToLower() == "-o")
                        {
                            if (args.Length > 5)
                            {
                                sp.OutputFile = args[5];
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                PrintUsage();
                Environment.Exit(1);

            }

            return sp;
        }

        
        /// <summary>
        /// Displays the usage information
        /// </summary>
        static void PrintUsage()
        {
            "Usage: iFFind <search directory> <search text> [-o <output file>]".PrintLine();
            "\t-f [ <search specific file only>]".PrintLine();
        }

        /// <summary>
        /// Saves the results to a file
        /// </summary>
        static void SaveResultsToFile(FstStructs.SearchParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.OutputFile))
            {
                try
                {
                    File.Delete(parameters.OutputFile);
                }
                catch (Exception)
                {
                    //Console.WriteLine($"An error occurred deleting the output file: {ex.Message}");
                }

                File.WriteAllText(parameters.OutputFile, _resultsBuilder.ToString());
            }
        }

    }
}
