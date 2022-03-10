//
// Created by Markus Kannisto on 10/03/2022.
//

#include <cstring>
#include "compiler_errors.h"
#include "CompilerErrorInfo.h"

CompilerError::CompilerError(string error_code, string error_message, CompilerErrorType error_type) {
    code = std::move(error_code);
    message = std::move(error_message);
    type = error_type;
}

map<CompilerErrorType, CompilerError> error_types = {
        {UnexpectedToken, CompilerError("C1000", "Unexpected token '%v'", UnexpectedToken)},
        {TerminatorExpected, CompilerError("C1001", "; expected", TerminatorExpected)},
};

CompilerError getErrorType(CompilerErrorType type) {
    switch (type) {
        case UnexpectedToken:
            return {"NPTC1001", "Unexpected token '%v'", type};
        case TerminatorExpected:
            return {"NPTC1002", "Unexpected token '%v'", type};
        default:
            return {"NPTC999", "Unidentified error", Unidentified};
    }
}

void lineNumber(char result[8], int line) {
    string number = to_string(line);

    for (int i = 0; i < 8; i++) {
        if (i < number.length())
            result[i] = number[i];
        else
            result[i] = ' ';
    }
}

const char *lineErrorPointer(string &result, int column) {
    result = "";
    int i;

    for (i = 1; i < column; i++) {
        result += " ";
    }

    result += "^~~~~~";
}

void compilerError(CompilerErrorType code, const CompilerErrorInfo& error_info) {
    // Get error from list
    CompilerError e = getErrorType(code);

    // Format the message
    string message = e.message;
    replaceAll(message, "%v", error_info.value);

    // Form an error message
    string e_prefix = "[nptc] ";
    string code_suffix = ": ";
    string e_suffix = ": (Line " + to_string(error_info.line) + ", Col " + to_string(error_info.column) + ")\n\t\t";
    string error_message = e_prefix + e.code + code_suffix + error_info.file.full_path + e_suffix + message;

    // Add code preview
    char line[8];
    lineNumber(line, error_info.line);
    error_message += "\n" + convertToString(line, (int)strlen(line)) + "| " + error_info.file.code_lines[error_info.line - 1];

    // Offset error pointer
    string offsetPointer;
    lineErrorPointer(offsetPointer, error_info.column);
    error_message += "\n        | " + offsetPointer;

    // Add bottom line
    error_message += "\n--------------------------------------------------";

    // Print error to console
    cout << error_message << endl;
}
