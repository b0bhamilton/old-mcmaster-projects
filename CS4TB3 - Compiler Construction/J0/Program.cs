using System;
using System.Collections.Generic;

namespace J0
{
    public class Program
    {
        static void Main(string[] args)
        {
            if ( args.Length != 1 )
            {
                System.Console.WriteLine("Usage:");
                System.Console.WriteLine();
                System.Console.WriteLine("j0.exe <FilePath>");
                System.Console.WriteLine();
                System.Console.WriteLine("The J0 compiler generates the executable .NET MSIL code.");
                System.Console.WriteLine("The code is placed to the new file with the same name");
                System.Console.WriteLine("as the source file and with the '.exe' extension.");
                return;
            }
            string fname = args[0];

            System.Console.WriteLine("J0 Command Line Compiler Version 0.0.0.1");
            System.Console.WriteLine("Copyright © R.Hamilton 2012");

            Errors errors = new Errors();
            try
            {
                Source source = new Source(fname /* @"c:\Zouev\J0\Tests\T01.j0" */,errors);
                Lexer lexer = new Lexer(source,errors);
                Parser parser = new Parser(lexer,errors);

                // 1. Parsing
                PROGRAM program = parser.parseCompilationUnit();
                if ( errors.errCount() > 0 ) { errors.report(); return; }

                // 2. Name resolution
                program.resolve(null,null,null);
                if (errors.errCount() > 0) { errors.report(); return; }

                // 3. Code generation
                Generator gen = new Generator(source,errors,program);
                gen.generateProgram();
                if ( errors.errCount() > 0 )
                {
                    errors.report();
                    System.Console.WriteLine("Code is not generated");
                }
                else
                {
                    System.Console.WriteLine("Compilation completed successfully");
                }
            }
            catch (Exception)
            {
                errors.issue(34); // internal error
                errors.report();
                System.Console.WriteLine("Compilation aborted");
            }
        }
    }
}
