//
// Created by Markus Kannisto on 10/03/2022.
//

#include "string_utils.h"

string ConvertToString(char s) {
	string res;
	res += s;
	return res;
}

string ConvertToString(char *s, int size) {
	string res;
	for (int i = 0; i < size; i++) {
		res += s[i];
	}
	return res;
}

void ReplaceAll(string &s, const string &from, const string &to) {
	if (from.empty())
		return;
	size_t start_pos = 0;
	while ((start_pos = s.find(from, start_pos)) != string::npos) {
		s.replace(start_pos, from.length(), to);
		start_pos += to.length();
	}
}

void Split(std::vector<std::string> &result, const std::string &s, const std::string &del) {
	unsigned int start = 0;
	unsigned int end = s.find(del);
	
	while (end != -1) {
		result.push_back(s.substr(start, end - start));
		start = end + del.size();
		end = s.find(del, start);
	}
	
	result.push_back(s.substr(start, end - start));
}
