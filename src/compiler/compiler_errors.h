//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <map>
#include "../common_includes.h"
#include "NeptyneScript.h"
#include "CompilerErrorInfo.h"

enum CompilerErrorType {
    Unidentified,
    UnexpectedToken,
    TerminatorExpected,
};

class CompilerError {
public:
    CompilerError(string error_code, string error_message, CompilerErrorType error_type);

    string code;
    string message;
    CompilerErrorType type;
};

void compilerError(CompilerErrorType code, const CompilerErrorInfo &error_info);
