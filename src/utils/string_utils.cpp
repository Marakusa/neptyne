//
// Created by Markus Kannisto on 10/03/2022.
//

#include "string_utils.h"

string getString(char x) {
    string s(1, x);
    return s;
}

string convertToString(char* a, int size)
{
    string s;
    for (int i = 0; i < size; i++) {
        s += a[i];
    }
    return s;
}

void replaceAll(string& str, const string& from, const string& to) {
    if (from.empty())
        return;
    size_t start_pos = 0;
    while ((start_pos = str.find(from, start_pos)) != string::npos) {
        str.replace(start_pos, from.length(), to);
        start_pos += to.length();
    }
}

void split(std::vector<std::string> &result, const std::string& s, const std::string& del) {
    unsigned int start = 0;
    unsigned int end = s.find(del);

    while (end != -1) {
        result.push_back(s.substr(start, end - start));
        start = end + del.size();
        end = s.find(del, start);
    }

    result.push_back(s.substr(start, end - start));
}
