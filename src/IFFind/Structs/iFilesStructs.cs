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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace iFFind.Structs
{
    // ReSharper disable once InconsistentNaming
    internal static class iFilesStructs
    {
        public struct FileResult
        {
            public string FileName;
            public string FilePath;
            public string FileExtension;
            public string FileSize;
            public string FileCreationDate;
            public string FileLastAccessDate;
            public string FileLastWriteDate;
        }

        public struct TextResult
        {
            public string FileName;
            public string FilePath;
            public string LineNumber;
            public string LineText;
        }

        public struct SearchParameters
        {
            public string SearchText;
            public string SearchDirectory;
            public bool SearchBoth;
            public bool SearchFiles;
            public bool SearchDirectories;
            public string OutputFile;
            public string SpecificFilename;
        }

        public struct ResultsSet
        {
            public string SearchResults { get; set; }
            public bool Complete { get; set; }
        }

        public static string SearchParametersToString(SearchParameters searchParameters)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine($"Search Text: {searchParameters.SearchText}");
            s.AppendLine($"Search Directory: {searchParameters.SearchDirectory}");
            s.Append($"Filename To Search: {searchParameters.SpecificFilename}");
            //s.AppendLine($"Search Both: {searchParameters.SearchBoth}");
            //s.AppendLine($"Search Files: {searchParameters.SearchFiles}");
            //s.AppendLine($"Search Directories: {searchParameters.SearchDirectories}");
            s.AppendLine($"Output File: {searchParameters.OutputFile}");
            
            return s.ToString();
        }
    }
}
