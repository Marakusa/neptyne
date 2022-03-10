//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <vector>
#include "../common_includes.h"
#include "Token.h"

vector<Token> tokenize(const string &input_code, NeptyneScript &script);
