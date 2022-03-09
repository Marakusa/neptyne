//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include "../common_includes.h"
#include "TokenType.h"
#include "NeptyneScript.h"

class Token {
public:
    TokenType type;
    string value;
    int line;
    int column;
    NeptyneScript *script_file;

    Token(TokenType type, string &value, int line, int column, NeptyneScript *file) {
        this->type = type;
        this->value = value;
        this->line = line;
        this->column = column;
        this->script_file = file;
    }
};
