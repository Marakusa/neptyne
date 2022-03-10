//
// Created by Markus Kannisto on 10/03/2022.
//

#include "string_utils.h"

string getString(char x) {
    string s(1, x);
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