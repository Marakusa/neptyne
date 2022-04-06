//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <vector>
#include "../../common/common.h"
#include "models/Token.h"

vector<Token> Tokenize(NeptyneScript &code_script);
