#include <iostream>
#include <cstdarg>
#include <iostream>
#include <string>
#include <utility>
#include <vector>

class logger {
public:
    logger(){std::cout<<"constructor\n";}
    static void debug(const std::string message);
    static void debug(std::string message, const int count...);
    static void info(const std::string message);
    static void info(std::string message, const int count...);
    static void warn(const std::string message);
    static void warn(std::string message, const int count...);
    static void error(const std::string message);
    static void error(std::string message, const int count...);
};