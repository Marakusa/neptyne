//
// Created by Markus Kannisto on 10/03/2022.
//

#include <cstring>
#include "compiler_errors.h"
#include "CompilerErrorInfo.h"

class CompilerError GetErrorType(CompilerErrorType type) {
	switch (type) {
		// Tokenizer errors
		case UNEXPECTED_TOKEN:return {"NPT1001", "Unexpected token '%v'", type};
		case TERMINATOR_EXPECTED:return {"NPT1002", "Unexpected token '%v'", type};
		case CANNOT_RESOLVE_SYMBOL:return {"NPT1003", "Cannot resolve symbol '%v'", type};
		case INVALID_NUMBER_LITERAL:return {"NPT1004", "Invalid number literal", type};
			
			// Statement errors
		case INVALID_BRING_STATEMENT_PLACEMENT:
			return {"NPT1101", "Bring statements can only be declared outside all"
			                   " declaration bodies", type};
		
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
	string e_suffix = ": (Line " + to_string(error_info.line_) + ", Col " + to_string(error_info.column_) + ")\n\t\t";
	string error_message = e_prefix + e.code_ + code_suffix + error_info.file_.full_path_ + e_suffix + message;
	
	// Add code preview
	char line[8];
	LineNumber(line, error_info.line_);
	error_message +=
		"\n" + ConvertToString(line, (int)strlen(line)) + "| " + error_info.file_.code_lines_[error_info.line_ - 1];
	
	// Offset error pointer
	string offset_pointer;
	LineErrorPointer(offset_pointer, error_info.column_);
	error_message += "\n        | " + offset_pointer;
	
	// Add bottom line
	error_message += "\n--------------------------------------------------";
	
	// Print error to console
	cout << error_message << endl;
}
