#include <utility>

//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#import "../common_includes.h"

struct CompilerException : public exception {
    string message;

public:
    CompilerException(exception msg, const char* file_, int line_, int column_) : std::exception(std::move(msg)) {
        string e_prefix = "[nptc] ";
        string e_suffix = ": (Line " + to_string(line_) + ", Col " + to_string(column_) + ")";
        message = e_prefix + msg.what() + e_suffix;
    }

    [[nodiscard]] string get() const { return message; }
};


