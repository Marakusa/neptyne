#include <Logger.cpp>
#include <iostream>
#include <fstream>
#include <CodeInput.cpp>

int main(int argc, char *argv[])
{
    logger::init(argc, argv);

    if (argc == 1)
    {
        code_input();
    }
    else
    {
        std::ifstream ifile;
        ifile.open(argv[1]);

        if (ifile)
        {
            logger::error("Incorrect command ({0})", argv[1]);
        }
        else if (argv[1] == "-f" || argv[1] == "f")
        {
            ifile.open(argv[2]);

            if (ifile)
            {
            }
            else
            {
                std::cout << "[nptc] ERR: File (" << argv[2] << ") doesn't exist or isn't a file";
            }
        }
        else
        {
            std::cout << "[nptc] ERR: Incorrect command (" << argv[1] << ")";
        }
    }
}