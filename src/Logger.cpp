#include "Logger.h"

#define RED     "\033[31m"
#define YELLOW  "\033[33m"
#define CYAN    "\033[36m"
#define WHITE   "\033[37m"
#define RESET_COLORS   "\033[0m"

std::string parse_parameters(std::string message, const int size, const std::string args[]) {
    std::string output;

    int parameterBracket = -1;

    int message_len = message.length();

    for (int i = 0; i < message_len; i++) {
        if (parameterBracket == -1) {
            if (message[i] == '{') {
                parameterBracket = i;
            }
            else {
                output += message[i];
            }
        }
        else {
            if (message[i] == '}') {
                parameterBracket = -1;
            }
            else {
                if (message[i]>='0' && message[i]<='9') {
                    int index = (int)message[i] - '0';
                    if (index < size)
                        output += args[index];
                }
                else {
                    parameterBracket = -1;
                }
            }
        }
    }

    return output;
}

void logger::debug(const std::string message) {
    std::cout << WHITE << "[npt] DEBUG: " << message << RESET_COLORS << std::endl;
}
void logger::debug(std::string message, const int count...) {
    va_list args;
    std::string args_array[count];

    va_start(args, count);
    for (int i = 0; i < count; i++) {
        char* str = va_arg(args, char* );
        args_array[i] = str;
    }

    const std::string output = parse_parameters(std::move(message), count, args_array);
    debug(output);
}

void logger::info(const std::string message) {
    std::cout << CYAN << "[npt] INFO: " << message << RESET_COLORS << std::endl;
}
void logger::info(std::string message, const int count...) {
    va_list args;
    std::string args_array[count];

    va_start(args, count);
    for (int i = 0; i < count; i++) {
        char* str = va_arg(args, char* );
        args_array[i] = str;
    }

    const std::string output = parse_parameters(std::move(message), count, args_array);
    info(output);
}

void logger::warn(const std::string message) {
    std::cout << YELLOW << "[npt] WARN: " << message << RESET_COLORS << std::endl;
}
void logger::warn(std::string message, const int count...) {
    va_list args;
    std::string args_array[count];

    va_start(args, count);
    for (int i = 0; i < count; i++) {
        char* str = va_arg(args, char* );
        args_array[i] = str;
    }

    const std::string output = parse_parameters(std::move(message), count, args_array);
    warn(output);
}

void logger::error(const std::string message) {
    std::cout << RED << "[npt] ERROR: " << message << RESET_COLORS << std::endl;
}
void logger::error(std::string message, const int count...) {
    va_list args;
    std::string args_array[count];

    va_start(args, count);
    for (int i = 0; i < count; i++) {
        char* str = va_arg(args, char* );
        args_array[i] = str;
    }

    const std::string output = parse_parameters(std::move(message), count, args_array);
    error(output);
}
