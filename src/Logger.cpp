#include <iostream>
#include <vector>
#include <cmath>

#define RESET   "\033[0m"
#define BLACK   "\033[30m"
#define RED     "\033[31m"
#define GREEN   "\033[32m"
#define YELLOW  "\033[33m"
#define BLUE    "\033[34m"
#define MAGENTA "\033[35m"
#define CYAN    "\033[36m"
#define WHITE   "\033[37m"

namespace logger
{
    void init(int argc, char *argv[])
    {
        int LOG_LEVEL;

        int i = 0;
        while (i < argc)
        {
            if (argv[i] == "-l" && argc > argc + 1)
            {
                switch (tolower(*argv[i + 1]))
                {
                    case *"all":
                        LOG_LEVEL = 0;
                        break;
                    case *"debug":
                        LOG_LEVEL = 1;
                        break;
                    case *"info":
                        LOG_LEVEL = 2;
                        break;
                    case *"warn":
                        LOG_LEVEL = 3;
                        break;
                    case *"error":
                        LOG_LEVEL = 4;
                        break;
                    case *"off":
                        LOG_LEVEL = 5;
                        break;
                    default:
                        LOG_LEVEL = 3;
                        break;
                }
            }
            i++;
        }
    }

    void debug(char *message, const char *args...)
    {
        va_list arguments;
        std::vector<char> iargs;
        for (va_start(arguments, args); args != NULL; args = va_arg(arguments, const char *))
        {
            iargs.push_back(*args);
        }

        char *output = parse_parameters(message, iargs);

        std::cout << WHITE << "[nptc] DEBUG: " << output << std::endl;
    }

    void info(char *message, const char *args...)
    {
        va_list arguments;
        std::vector<char> iargs;
        for (va_start(arguments, args); args != NULL; args = va_arg(arguments, const char *))
        {
            iargs.push_back(*args);
        }

        char *output = parse_parameters(message, iargs);

        std::cout << CYAN << "[nptc] INFO: " << output << std::endl;
    }

    void warn(char *message, const char *args...)
    {
        va_list arguments;
        std::vector<char> iargs;
        for (va_start(arguments, args); args != NULL; args = va_arg(arguments, const char *))
        {
            iargs.push_back(*args);
        }

        char *output = parse_parameters(message, iargs);

        std::cout << YELLOW << "[nptc] WARN: " << output << std::endl;
    }

    void error(char *message, const char *args...)
    {
        va_list arguments;
        std::vector<char> iargs;
        for (va_start(arguments, args); args != NULL; args = va_arg(arguments, const char *))
        {
            iargs.push_back(*args);
        }

        char *output = parse_parameters(message, iargs);

        std::cout << RED << "[nptc] ERROR: " << output << std::endl;
    }

    char *parse_parameters(char *message, std::vector<char> iargs)
    {
        char *output = "";
        int messagelen = LoggerUtils::string_length(message);
        int parameterBracket = -1;
        char *parameter;
        for (int i = 0; i < messagelen; i++)
        {
            if (parameterBracket == -1)
            {
                if (message[i] == '{')
                {
                    for (int j = 0; j < iargs.size(); j++)
                    {
                        output += iargs[j];
                    }
                    parameterBracket = i;
                }
                else
                {
                    output += message[i];
                }
            }
            else
            {
                if (message[i] == '}')
                {
                    parameterBracket = -1;
                }
                else
                {
                    if (message[i]>='0' && message[i]<='9')
                    {
                        parameter += message[i];
                    }
                    else
                    {
                        parameterBracket = -1;
                    }
                }
            }
        }

        return output;
    }
}

namespace LoggerUtils
{
    int string_length(char* given_string)
    {
        int length = 0;
        while (*given_string != '\0') {
            length++;
            given_string++;
        }
    
        return length;
    }

    int count_digits(int number) {
        return int(log10(number) + 1);
    }
}
