#include <iostream>
#include <string>
#include <fstream>
#include <cstring>
#include "Logger.h"

void open_file(char *file) {
    std::ifstream i_file;
    i_file.open(file);

    if (i_file) {
    }
    else {
        logger::error("File \"{0}\" doesn't exist or isn't a file", 1, file);
    }
}

int main(int argc, char *argv[]) {
    if (argc == 1) {
        bool end = false;
        std::string input;

        std::cout << "Neptyne v0.0.1" << std::endl << "Input 'q' or press 'CTRL + C' to quit" << std::endl;

        while (!end)
        {
            std::cin >> input;

            if (input == "q")
            {
                end = true;
                break;
            }



            std::cout << input << std::endl;
        }

        std::cin.get();
    }
    else {
        if (argv[1][0] == '-') {
            if (strcmp(argv[1], "-f") == 0) {
                open_file(argv[2]);
            }
            else {
                logger::error("Incorrect command: {0}", 1, argv[1]);
            }
        }
        else {
            open_file(argv[1]);
        }
    }
}
