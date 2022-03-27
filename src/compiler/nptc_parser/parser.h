//
// Created by Markus Kannisto on 12/03/2022.
//

#pragma once

#include <vector>
#include "../../common/common.h"
#include "../nptc_tokenizer/models/Token.h"
#include "models/ParserToken.h"

void ParseToSyntaxTree(ParserToken &root, vector<Token> &input_tokens, NeptyneScript &input_script);
