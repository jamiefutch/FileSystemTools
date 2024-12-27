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
using FstShared;

namespace iFind;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Program
{
    #region Locals

    // ReSharper disable once CollectionNeverQueried.Local
    // ReSharper disable once NotAccessedField.Local
    private static List<FstStructs.FileResult>? _fileResults;
    private static List<string>? _results;

    private static bool _isWindowsOs;

    #endregion

    private static void Main(string[] args)
    {
        _isWindowsOs = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        var parameters = GetParameters(args);
        FstStructs.iFindSearchParametersToString(parameters);
        FstStructs.iFindSearchParametersToString(parameters).PrintLine();

        _results = new List<string>();

        _fileResults = new List<FstStructs.FileResult>();
        SearchDirectoriesForFile(parameters);
        if (!string.IsNullOrEmpty(parameters.OutputFile)) SaveResultsToFile(parameters);
    }


    private static void SearchDirectoriesForFile(FstStructs.SearchParameters parameters)
    {
        var ss = parameters.SearchText.ToLower();
        var directoriesToSearch = new Stack<string>();
        directoriesToSearch.Push(FixPath(parameters.SearchDirectory, _isWindowsOs));
        var count = 0;

        try
        {
            while (directoriesToSearch.Count > 0)
            {
                var currentDirectory = directoriesToSearch.Pop();

                foreach (var directory in Directory.GetDirectories(currentDirectory))
                {
                    if (parameters.SearchBoth || parameters.SearchDirectories)
                        if (directory.ToLower().Contains(ss))
                        {
                            var fi = new FileInfo(directory);
                            _results?.Add(fi.FullName);
                            Console.WriteLine(fi.FullName);
                            count++;
                        }

                    directoriesToSearch.Push(directory);
                }

                foreach (var file in Directory.GetFiles(currentDirectory))
                    if (parameters.SearchBoth || parameters.SearchFiles)
                        if (file.ToLower().Contains(ss))
                        {
                            var fi = new FileInfo(file);
                            _results?.Add(fi.FullName);
                            Console.WriteLine(fi.FullName);
                            count++;
                        }
            }
        }
        catch (Exception)
        {
            //Console.WriteLine($"An error occurred: {ex.Message}");
        }

        $"Count: {count}".PrintLine();
    }
    

    /// <summary>
    ///     Fixes the path if it is missing a trailing backslash
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isWindowsOs"></param>
    /// <returns></returns>
    private static string FixPath(string path, bool isWindowsOs)
    {
        var ret = path;

        if (!isWindowsOs)
        {
            if (path.EndsWith(":")) ret = path + "\\";
        }
        else
        {
            if (path.EndsWith(":")) ret = path + "/";
        }

        return ret;
    }

    /// <summary>
    ///     parses the command line arguments
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static FstStructs.SearchParameters GetParameters(string[] args)
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
                if (args[2].ToLower() == "-o")
                    if (args.Length > 3)
                        sp.OutputFile = args[3];

                if (args[2].ToLower() == "-b") sp.SearchBoth = true;

                if (args[2].ToLower() == "-d") sp.SearchDirectories = true;

                if (args[2].ToLower() == "-f") sp.SearchFiles = true;

                if (args.Length > 4)
                    if (args[4].ToLower() == "-o")
                        if (args.Length > 5)
                            sp.OutputFile = args[5];
            }

            sp = FixParameters(sp);
        }
        catch (Exception)
        {
            PrintUsage();
            Environment.Exit(1);
        }

        return sp;
    }

    /// <summary>
    ///     Fixes the search parameters for search both parameter
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static FstStructs.SearchParameters FixParameters(FstStructs.SearchParameters parameters)
    {
        var sp = parameters;
        if (parameters is { SearchDirectories: false, SearchFiles: false }) sp.SearchBoth = true;

        return sp;
    }

    /// <summary>
    ///     Displays the usage information
    /// </summary>
    private static void PrintUsage()
    {
        "Usage: iFind <search directory> <search text> [-o <output file>]".PrintLine();
        "\t-b [ <search both filenames and directory names {default}>]".PrintLine();
        "\t-d [ <search directory names only>]".PrintLine();
        "\t-f [ <search filenames only>]".PrintLine();
    }

    /// <summary>
    ///     Saves the results to a file
    /// </summary>
    private static void SaveResultsToFile(FstStructs.SearchParameters parameters)
    {
        if (string.IsNullOrEmpty(parameters.OutputFile)) return;
        try
        {
            File.Delete(parameters.OutputFile);
        }
        catch (Exception)
        {
            //Console.WriteLine($"An error occurred deleting the output file: {ex.Message}");
        }

        using var sw = new StreamWriter(parameters.OutputFile);
        if (_results == null) return;
        foreach (var result in _results) sw.WriteLine(result);
    }
}