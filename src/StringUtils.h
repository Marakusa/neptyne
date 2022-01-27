#include <iostream>

class StringUtils {
public:
    static std::string leftTrim(const std::string &s);
    static std::string rightTrim(const std::string &s);
    static std::string trim(const std::string &s);
    static int str_len(std::string str);
    static std::string * str_split(std::string str, char separator);
};
