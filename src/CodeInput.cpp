#include <iostream>

void code_input()
{
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