//
// Created by Markus Kannisto on 10/03/2022.
//

#include <cstring>
#include "compiler_errors.h"
#include "models/CompilerErrorInfo.h"

class CompilerError GetErrorType(CompilerErrorType type) {
	switch (type) {
		// Tokenizer errors
		case UNEXPECTED_TOKEN:return {"NPT1001", "Unexpected token '%v'", type};
		case CANNOT_RESOLVE_SYMBOL:return {"NPT1002", "Cannot resolve symbol '%v'", type};
		case TERMINATOR_EXPECTED:return {"NPT1003", "; expected", type};
		case SYMBOL_EXPECTED:return {"NPT1004", "%v expected", type};
		case INVALID_NUMBER_LITERAL:return {"NPT1004", "Invalid number literal", type};
			
			// Statement errors
		case INVALID_BRING_STATEMENT_PLACEMENT:
			return {"NPT1101", "Bring statements can only be declared outside all"
			                   " declaration bodies", type};
			
			// The rest of the compiler errors
		case EXPECTED_UNQUALIFIED_ID:return {"NPT1201", "Expected unqualified-id", type};
		case EXPECTED_EXPRESSION:return {"NPT1202", "Expected expression", type};
		case EXPECTED_FUNCTION_BODY:return {"NPT1203", "Expected function body after function declarator", type};
		case OUTSIDE_THE_RANGE:return {"NPT1204", "The value %v is outside the range of the given type", type};
		case FUNCTION_DECLARATION_NOT_ALLOWED:return {"NPT1205", "Function declaration is not allowed here", type};
		case VARIABLE_DECLARATION_NOT_ALLOWED:return {"NPT1206", "Variable declaration is not allowed here", type};
		case FUNCTION_DEFINITION_NOT_ALLOWED:return {"NPT1207", "Function definition is not allowed here", type};
		case VARIABLE_DEFINITION_NOT_ALLOWED:return {"NPT1208", "Variable definition is not allowed here", type};
		case DEFINITION_NOT_ALLOWED:return {"NPT1209", "Variable or function definition is not allowed here", type};
		case VARIABLE_EXISTS:return {"NPT1210", "Variable or function named %v already exists", type};
		
		default:return {"NPTC999", "Unidentified error", UNASSIGNED};
	}
}

void LineNumber(char result[8], int line) {
	string number = to_string(line);
	
	for (int i = 0; i < 8; i++) {
		if (i < number.length())
			result[i] = number[i];
		else
			result[i] = ' ';
	}
}

const char *LineErrorPointer(string &result, int column) {
	result = "";
	int i;
	
	for (i = 1; i < column; i++) {
		result += " ";
	}
	
	result += "^~~~~~";
}

void CompilerError(CompilerErrorType code, const CompilerErrorInfo &error_info) {
	// Get error from list
	class CompilerError e = GetErrorType(code);
	
	// Format the message
	string message = e.message_;
	ReplaceAll(message, "%v", error_info.value_);
	
	// Form an error message
	string e_prefix = "[nptc] ";
	string code_suffix = ": ";
	string e_suffix = ": (Line " + to_string(error_info.line_) + ", Col " + to_string(error_info.column_) + ")\n";
	string error_message = e_prefix + e.code_ + code_suffix + error_info.file_.full_path_ + e_suffix;
	
	// Add code preview
	char line[8];
	LineNumber(line, error_info.line_);
	error_message +=
		"\n" + ConvertToString(line, (int)strlen(line)) + "| " + error_info.file_.code_lines_[error_info.line_ - 1];
	
	// Offset error pointer
	string offset_pointer;
	LineErrorPointer(offset_pointer, error_info.column_);
	error_message += "\n        | " + offset_pointer;
	
	// Add the error message
	error_message += "\n\t\t| " + message;
	
	// Add bottom line
	error_message += "\n--------------------------------------------------";
	
	// Print error to console
	cout << error_message << endl;
}
