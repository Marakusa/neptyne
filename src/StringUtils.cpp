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

int StringUtils::str_len(std::string str)
{
    int length = 0;
    for (int i = 0; str[i] != '\0'; i++)
    {
        length++;
    }
    return length;
}

std::string * StringUtils::str_split(std::string str, char separator)
{
    int count = 0;

    int i = 0;
    while (i <= str_len(str))
    {
        if (str[i] == separator || i == str_len(str))
            count++;

        i++;
    }

    std::string *results[count];

    int currIndex = 0;
    int startIndex = 0, endIndex = 0;
    i = 0;
    while (i <= str_len(str))
    {
        if (str[i] == separator || i == str_len(str))
        {
            endIndex = i;
            std::string subStr = "";
            subStr.append(str, startIndex, endIndex - startIndex);
            *results[currIndex] = subStr;
            currIndex += 1;
            startIndex = endIndex + 1;
        }
        i++;
    }

    return *results;
}