//
// Created by Markus Kannisto on 10/03/2022.
//

#include "compiler_errors.h"
#include "CompilerErrorInfo.h"

CompilerError::CompilerError(string error_code, string error_message, CompilerErrorType error_type) {
    code = std::move(error_code);
    message = std::move(error_message);
    type = error_type;
}

map<int, CompilerError> error_types = {
        {(int)UnexpectedToken, CompilerError("C1000", "Unexpected token '%v'", UnexpectedToken)},
        {(int)TerminatorExpected, CompilerError("C1001", "; expected", TerminatorExpected)},
};

void compilerError(CompilerErrorType code, const CompilerErrorInfo& error_info) {
    // Get error from list
    CompilerError e = error_types[(int)code];

    // Format the message
    string message = e.message;
    replaceAll(message, "%s", error_info.value);

    // Form an error message
    string e_prefix = "[nptc] ";
    string code_suffix = ": ";
    string e_suffix = ": (Line " + to_string(error_info.line) + ", Col " + to_string(error_info.column) + ")\n\t\t";
    string error_message = e_prefix + e.code + code_suffix + error_info.file.full_path + e_suffix + message;

    // Print error to console
    cout << error_message << endl;
}
