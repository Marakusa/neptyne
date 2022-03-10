//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once


#include <utility>

#include "NeptyneScript.h"

class CompilerErrorInfo {
public:
    CompilerErrorInfo(NeptyneScript &file_, int line_, int column_, string value_) : file(file_) {
        file = file_;
        line = line_;
        column = column_;
        value = std::move(value_);
    }

    NeptyneScript &file;
    int line;
    int column;
    string value;
};


