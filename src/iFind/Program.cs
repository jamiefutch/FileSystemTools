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

using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using iFind.Helpers;
using iFind.Structs;
//using Utility.CommandLine.Arguments;

namespace iFind
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class Program
    {
        #region Locals
        private static List<iFilesStructs.FileResult> _fileResults;
        private static List<string> _results;
        
        private static bool _isWindowsOs;
        #endregion
        
        static void Main(string[] args)
        {
            _isWindowsOs = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var parameters = GetParameters(args);
            iFilesStructs.SearchParametersToString(parameters);
            Print(iFilesStructs.SearchParametersToString(parameters));
            
            _results = new List<string>();
            
            _fileResults = new List<iFilesStructs.FileResult>();
            SearchDirectoriesForFile(parameters);
            if (!string.IsNullOrEmpty(parameters.OutputFile))
            {
                SaveResultsToFile(parameters);
            }
        }



        static void SearchDirectoriesForFile(iFilesStructs.SearchParameters parameters)
        {
            var ss = parameters.SearchText.ToLower();
            var directoriesToSearch = new Stack<string>();
            directoriesToSearch.Push(FixPath(parameters.SearchDirectory, _isWindowsOs));
            var count = 0;

            try
            {
                while (directoriesToSearch.Count > 0)
                {
                    string currentDirectory = directoriesToSearch.Pop();

                    foreach (string directory in Directory.GetDirectories(currentDirectory))
                    {
                        if (parameters.SearchBoth || parameters.SearchDirectories)
                        {
                            if (directory.ToLower().Contains(ss))
                            {
                                FileInfo fi = new FileInfo(directory);
                                _results.Add(fi.FullName);
                                Console.WriteLine(fi.FullName);
                                count++;
                            }
                        }
                        directoriesToSearch.Push(directory);
                    }

                    foreach (string file in Directory.GetFiles(currentDirectory))
                    {
                        if (parameters.SearchBoth || parameters.SearchFiles)
                        {
                            if (file.ToLower().Contains(ss))
                            {
                                FileInfo fi = new FileInfo(file);
                                _results.Add(fi.FullName);
                                Console.WriteLine(fi.FullName);
                                count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"An error occurred: {ex.Message}");
            }
            
            Print($"Count: {count}");
        }

        /// <summary>
        /// Adds a directory or file to the list of results
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="fileInfo"></param>
        static iFilesStructs.FileResult AddItemToList(DirectoryInfo? directoryInfo, FileInfo? fileInfo)
        {
            iFilesStructs.FileResult fr;
            if(directoryInfo != null)
            {
                // directory
                fr = new iFilesStructs.FileResult
                {
                    FileName = directoryInfo.Name,
                    FilePath = directoryInfo.FullName,
                    FileExtension = "",
                    FileSize = "0",
                    FileCreationDate = directoryInfo.CreationTime.ToString(CultureInfo.InvariantCulture),
                    FileLastAccessDate = directoryInfo.LastAccessTime.ToString(CultureInfo.InvariantCulture),
                    FileLastWriteDate = directoryInfo.LastWriteTime.ToString(CultureInfo.InvariantCulture)
                };
                _fileResults.Add(fr);
            }
            else if(fileInfo != null)
            {
                // file
                fr = new iFilesStructs.FileResult
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    FileExtension = fileInfo.Extension,
                    FileSize = fileInfo.Length.ToString(),
                    FileCreationDate = fileInfo.CreationTime.ToString(CultureInfo.InvariantCulture),
                    FileLastAccessDate = fileInfo.LastAccessTime.ToString(CultureInfo.InvariantCulture),
                    FileLastWriteDate = fileInfo.LastWriteTime.ToString(CultureInfo.InvariantCulture)
                };
                _fileResults.Add(fr);
            }
            else
            {
                // lazy catchall
                fr = new iFilesStructs.FileResult();
            }
            
            return fr;
        }

        /// <summary>
        /// lazy print routine
        /// </summary>
        /// <param name="text"></param>
        static void Print(string text)
        {
            Console.WriteLine(text);
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
        static iFilesStructs.SearchParameters GetParameters(string[] args)
        {
            var sp = new iFilesStructs.SearchParameters();
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            try
            {
                sp.SearchDirectory = args[0];
                sp.SearchText = args[1];

                if(args.Length > 2)
                {
                    if (args[2].ToLower() == "-o")
                    {
                        if (args.Length > 3)
                        {
                            sp.OutputFile = args[3];
                        }
                    }

                    if (args[2].ToLower() == "-b")
                    {
                        sp.SearchBoth = true;
                    }
                
                    if (args[2].ToLower() == "-d")
                    {
                        sp.SearchDirectories = true;
                    }

                    if (args[2].ToLower() == "-f")
                    {
                        sp.SearchFiles = true;
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

                sp = FixParameters(sp);
            }
            catch (Exception e)
            {
                PrintUsage();
                Environment.Exit(1);

            }

            return sp;
        }

        /// <summary>
        /// Fixes the search parameters for search both parameter
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        static iFilesStructs.SearchParameters FixParameters(iFilesStructs.SearchParameters parameters)
        {
            var sp = parameters;
            if (!parameters.SearchDirectories && !parameters.SearchFiles)
            {
                sp.SearchBoth = true;
            }
            
            return sp;
        }
        /// <summary>
        /// Displays the usage information
        /// </summary>
        static void PrintUsage()
        {
            Print("Usage: iFind <search directory> <search text> [-o <output file>]");
            Print("\t-b [ <search both filenames and directory names {default}>]");
            Print("\t-d [ <search directory names only>]");
            Print("\t-f [ <search filenames only>]");
        }

        /// <summary>
        /// Saves the results to a file
        /// </summary>
        static void SaveResultsToFile(iFilesStructs.SearchParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.OutputFile))
            {
                try
                {
                    File.Delete(parameters.OutputFile);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"An error occurred deleting the output file: {ex.Message}");
                }

                using StreamWriter sw = new StreamWriter(parameters.OutputFile);
                foreach (string result in _results)
                {
                    sw.WriteLine(result);
                }
            }
        }

    }
}
