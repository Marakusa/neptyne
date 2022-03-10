//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <vector>
#include "../common_includes.h"

std::string getString(char x);

std::string convertToString(char *a, int size);

void replaceAll(std::string &str, const std::string &from, const std::string &to);

void split(std::vector<std::string> &result, const std::string &s, const std::string &del);
