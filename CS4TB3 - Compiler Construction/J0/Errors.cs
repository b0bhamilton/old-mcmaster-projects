using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J0
{
    public class FatalError : Exception
    {
    }

    public class Errors
    {
        private List<string> pool = new List<string>();
        private int count = 0;

        private static string[] templates =
        {
            /*  0 */ "",
            /*  1 */ "the path to the file is null",
            /*  2 */ "the path to the file is empty string",
            /*  3 */ "file '{0}' not found",
            /*  4 */ "disk or directory not found",
            /*  5 */ "illegal syntax of the file path '{0}'",
            /*  6 */ "not enough memory while reading the file",
            /*  7 */ "error while reading the file",
            /*  8 */ "cannot open or read file; compilation aborted",
            /*  9 */ "illegal character '{0}' in the source file",
            /* 10 */ "unexpected end of source file; compilation aborted",
            /* 11 */ "syntax error: '{0}' expected but not found",
            /* 12 */ "duplicating {0} declaration '{1}'",
            /* 13 */ "a type expected but not found",
            /* 14 */ "field cannot have 'void' type",
            /* 15 */ "incorrect type of parameter or local",
            /* 16 */ "syntax error in parameter list",
            /* 17 */ "syntax error in local declaration or statement",
            /* 18 */ "syntax error in expression",
            /* 19 */ "syntax error in statements",
            /* 20 */ "syntax error in while-statement",
            /* 21 */ "{0} '{1}' is not found",
            /* 22 */ "unknown statement",
            /* 23 */ "syntax error in relation",
            /* 24 */ "non-static field '{0}' cannot be accessed via class name",
            /* 25 */ "syntax error in selector",
            /* 26 */ "static field '{0}' cannot be accessed via variable name",
            /* 27 */ "private field or method '{0}' cannot be accessed from outside the class",
            /* 28 */ "static field of method '{0}' cannot be accessed from within non-static method",
            /* 29 */ "non-static field of method '{0}' cannot be accessed from within static method",
            /* 30 */ "an error in the array element construct",
            /* 31 */ "illegal call statement",
            /* 32 */ "multiple 'Main' functions in the program",
            /* 33 */ "no 'Main' function in the program",
            /* 34 */ "internal error in the compiler",
        };

        public void issue(int number, params object[] inf)
        {
            issue(null, number, inf);
        }

        public void issue(Token token, int number, params object[] inf)
        {
            string location = "";
            if ( token != null )
                location = "(" + (token.begin.number+1).ToString() + "," + (token.begin.position+1).ToString() + ") ";
            string message =  String.Format(templates[number],inf);
            pool.Add("error: " + location + message);
            count++;
        }

        public void fatal(Token token, int number, params object[] inf)
        {
            issue(token,number,inf);
            throw new FatalError();
        }

        public int errCount()
        { 
            return count; 
        }
 
        public void report()
        {
            foreach ( string msg in pool )
                System.Console.WriteLine(msg);
        }
    }
}
