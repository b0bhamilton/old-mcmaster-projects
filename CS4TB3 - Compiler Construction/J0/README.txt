The job is done. Compiler sources and binaries are attached (zip archive).

The usual way to use the compiler is to unpack the zip, then go to the
'bin\Debug'
directory and copy 'J0._xe' file to a directory of your choice - together
with the 'T01.j0' file from the 'Tests' directory.
Please change the file extension for '.exe' - I had to mangle it to push it
through
the Google guard service, while emailing to myself. :-)

After that you can invoke the compiler from the command line:

J0.exe T01.j0

To run the compiled program type

T01.exe

or simply

T01

and you will see the result of the execution.

Notice that you should have .NET framework installed on your machine.
If you have Windows 7 it's preinstalled otherwise you can easily and quickly
download it from the web and install - it's free.

An alternative way to work with the compiler is to open it within Visual
Studio
(or Visual Studio Express - it's also free). If you have VS/VSE installed
on your machine
just double click on the J0.sln file.

Some remarks.
The compiler has been tested with the J0.java file and with some tests of my own.

There are two lacks in the implementation:
- 'import' construct is processed but not generated. This means
you cannot attach other programs to your one.
- Class inheritance is not supported (base classes are processed
correctly but not generated).

I guess these lacks are not that serious.