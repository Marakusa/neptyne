#include "Main.h"

void npt_exec(const std::string command) {
    std::vector<std::string> lexemes = StringUtils::str_split(StringUtils::trim(command), " ");
    std::string m;
    int i = 0;
    while (i < static_cast<int>(lexemes.capacity())) {
        m += lexemes[i] + ',';
        i++;
    }
    logger.debug(m);
}

void open_file(char *file) {
    std::ifstream i_file;
    i_file.open(file);

    if (i_file) {
    }
    else {
        logger.error("File \"{0}\" doesn't exist or isn't a file", 1, file);
    }
}

int main(int argc, char *argv[]) {

    if (argc == 1) {
        bool end = false;

        std::cout << "Neptyne v0.0.1" << std::endl << "Input 'q' or press 'CTRL + C' to quit" << std::endl;

        while (!end) {
            std::string input;
            getline(std::cin, input);
            std::cout << input << std::endl;

            input = StringUtils::trim(input);

            if (input == "q") {
                end = true;
            }
            else if (input != "") {
                npt_exec(input);
            }
        }
    }
    else {
        if (argv[1][0] == '-') {
            if (strcmp(argv[1], "-f") == 0) {
                open_file(argv[2]);
            }
            else {
                logger.error("Incorrect command: {0}", 1, argv[1]);
            }
        }
        else {
            open_file(argv[1]);
        }
    }
}
