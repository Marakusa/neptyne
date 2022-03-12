//
// Created by Markus Kannisto on 12/03/2022.
//

#pragma once

#include <vector>
#include "../common_includes.h"
#include "models/Token.h"
#include "models/ParserToken.h"

void ParseToSyntaxTree(ParserToken root, vector<Token> &input_tokens, NeptyneScript &input_script);
ParserToken Walk(int &index, vector<struct Token> &tokens, NeptyneScript &script);
