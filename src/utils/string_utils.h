//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <vector>
#include "../common_includes.h"

std::string GetString(char x);

std::string ConvertToString(char *a, int size);

void ReplaceAll(std::string &str, const std::string &from, const std::string &to);

void Split(std::vector<std::string> &result, const std::string &s, const std::string &del);
