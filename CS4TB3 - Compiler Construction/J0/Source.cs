using System;
using System.Collections.Generic;
using System.IO;

namespace J0
{
    /// <summary>
    /// Represents the source file.
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Internal representation of the source program text.
        /// </summary>
        private List<string> lines;

        // Position number within the current source line (starting form 0)
        public int linePos { get { return _linePos; } }
        private int _linePos;

        // The number of the current source line (starting from 0)
        public int lineNo { get { return _lineNo; } }
        private int _lineNo;

        /// <summary>
        /// The full path to the disk file with the source program.
        /// </summary>
        public string fileName { get { return _fileName; } }
        private string _fileName;

        /// <summary>
        /// The full path to the disk file for the target code (assembly).
        /// Will be generated from the source name after opening it.
        /// </summary>
        public string targetName { get { return _targetName; } }
        private string _targetName;

        /// <summary>
        /// Pool for the issued error messages.
        /// </summary>
        private Errors errors;

        private bool readResult;

        /// <summary>
        /// Opens the file with the source program.
        /// </summary>
        /// <param name="filePath">The path to the source file.</param>
        /// <param name="errors">Pool for diagnostics.</param>
        public Source ( string filePath, Errors errors )
        {
            this.errors = errors;
            read(filePath);
            if ( !readResult ) errors.fatal(null,8);
            _targetName = Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filePath) + ".exe";
        }

        private void read(string filePath)
        {
            StreamReader reader;

            // Prepare list of lines
            lines = new List<string>();
            // Open source file
            try
            {
                reader = new StreamReader(filePath);
            }
            catch ( ArgumentNullException ) { errors.issue(1,filePath); goto Error; }  // filePath == null
            catch ( ArgumentException )     { errors.issue(2,filePath); goto Error; }  // filePath == ""
            catch ( FileNotFoundException ) { errors.issue(3,filePath); goto Error; }  // filePath not found
            catch ( DirectoryNotFoundException ) { errors.issue(4,filePath); goto Error; } // Disk or directory not found
            catch ( IOException )           { errors.issue(5,filePath); goto Error; }  // Illegal syntax of the filePath
            _fileName = filePath;

            // Reading source lines
            try
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    lines.Add(line + "\n");
                }
                // The last "line" in the list is always null;
                // this indicates the end of source.
                lines.Add(null);
            }
            catch ( OutOfMemoryException ) { errors.issue(6,fileName); goto Error; } // Not enough memory
            catch ( IOException )          { errors.issue(7,fileName); goto Error; } // Read error

            reader.Close();
            _linePos = 0;
            _lineNo = 0;
            readResult = true;
            return;
         Error:
            readResult = false;
        }

        /// <summary>
        /// Returns the character from the current source position.
        /// </summary>
        /// <returns></returns>
        public char curr()
        {
            char ch = '\0';
            while (true)
            {
                if (lines[_lineNo] == null) { ch = '\0'; break; }  // EOS
                // Read the current character
                ch = lines[_lineNo][_linePos];
                // Check if the end of line encountered
                if (ch != '\n') break;
                // Otherwise, take the next line, and go to the next iteration
                nextLine();
            }
            return ch;
        }

        public bool tryFor(string str)
        {
            string tail = lines[_lineNo].Substring(_linePos,lines[_lineNo].Length-_linePos-1);
            if (str.Length > tail.Length) return false;
            tail = tail.Substring(0,str.Length);
            if (tail == str)
            {
                _linePos += tail.Length;
                return true;
            }
            return false;
        }

        public char next() { return lines[_lineNo][_linePos+1]; }
        public void move() { _linePos++; }
        public void move2() { _linePos += 2; }
        public void nextLine() { _lineNo++; _linePos = 0; }

        /// <summary>
        /// Scans and returns the rest of the current string.
        /// </summary>
        /// <returns></returns>
        public string scanTail()
        {
            string tail = lines[_lineNo].Substring(_linePos,lines[_lineNo].Length-_linePos-1);
            _linePos += tail.Length;
            return tail;
        }
    }
}



