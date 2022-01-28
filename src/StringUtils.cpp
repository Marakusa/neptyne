#include "StringUtils.h"

static std::string trimCharacters = " \n\r\t\f\v";

std::string StringUtils::leftTrim(const std::string &s){
    auto start = s.find_first_not_of(trimCharacters);
    return (start == std::string::npos) ? "" : s.substr(start);
}
std::string StringUtils::rightTrim(const std::string &s){
    auto end = s.find_last_not_of(trimCharacters);
    return (end == std::string::npos) ? "" : s.substr(0, end + 1);
}
std::string StringUtils::trim(const std::string &s) {
    return rightTrim(leftTrim(s));
}

int StringUtils::str_len(std::string str) {
    int length = 0;
    for (int i = 0; str[i] != '\0'; i++) {
        length++;
    }
    return length;
}

std::vector<std::string> StringUtils::str_split(std::string str, std::string separator) {
    std::vector<std::string> split{};

    size_t pos;
    while ((pos = str.find(separator)) != std::string::npos) {
        split.push_back(str.substr(0, pos));
        str.erase(0, pos + str_len(separator));
    }

    split.push_back(str);

    return split;
}