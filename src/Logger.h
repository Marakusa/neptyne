#include <iostream>
#include <cstdarg>
#include <iostream>
#include <string>
#include <utility>
#include <vector>

class logger {
public:
    void debug(const std::string message);
    void debug(std::string message, const int count...);
    void info(const std::string message);
    void info(std::string message, const int count...);
    void warn(const std::string message);
    void warn(std::string message, const int count...);
    void error(const std::string message);
    void error(std::string message, const int count...);
};