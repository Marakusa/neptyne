//
// Created by Markus Kannisto on 09/03/2022.
//

#include "../common_includes.h"
#include "../utils/file_utils.h"
#include "compiler.h"
#include "models/NeptyneScript.h"
#include "tokenizer.h"
#include "parser.h"
#include "models/assembly/AssemblyScript.h"
#include "models/CompilerException.h"
#include "models/CompilerErrorInfo.h"
#include "compiler_errors.h"

int compiler_index = 0;
AssemblyScript assemblyScript;
NeptyneScript neptyneScript;

CompilerErrorInfo GetErrorInfo(ParserToken &token);

ParserToken Increment(vector<ParserToken> &tokens, CompilerErrorType fail, char v = 0);
bool CheckIncrement(vector<ParserToken> &tokens);

void CompilerStep(vector<ParserToken> tokens, ParserToken parent);
bool EndStatement(vector<ParserToken> &tokens);

void Compile(const NeptyneScript &script) {
	// Read parser_script file
	string code = ReadFile(script.full_path_);
	neptyneScript = script;
	neptyneScript.code_ = code;
	
	// Split parser_script to lines
	vector<string> lines;
	Split(lines, code, "\n");
	neptyneScript.code_lines_ = lines;
	
	// Tokenize the parser_script
	vector<Token> tokens = Tokenize(neptyneScript);
	
	// Parse parser_input_tokens into a syntax tree
	ParserToken root_token = ParserToken(ROOT, neptyneScript.name_, 1, 1, neptyneScript);
	ParseToSyntaxTree(root_token, tokens, neptyneScript);
	
	// Compile code into functions and statements
	assemblyScript = AssemblyScript();
	compiler_index = 0;
	while (compiler_index < root_token.parameters_.size()) {
		CompilerStep(root_token.parameters_, root_token);
		compiler_index++;
	}
}

void CompilerStep(vector<ParserToken> tokens, ParserToken parent) {
	if (compiler_index >= tokens.size()) {
		return;
	}
	ParserToken token = tokens[compiler_index];
	switch (token.type_) {
		case VALUE_TYPE:
			// Set type
			string type = token.value_;
			
			token = Increment(tokens, EXPECTED_UNQUALIFIED_ID);
			
			// Set name
			if (token.type_ != NAME) {
				CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
				break;
			}
			string name_value = token.value_;
			
			token = Increment(tokens, TERMINATOR_EXPECTED);
			// Check if is declaration or a function
			if (token.type_ != ASSIGNMENT_OPERATOR) {
				if (token.type_ == COLON) {
					token = Increment(tokens, EXPECTED_FUNCTION_BODY);
					if (token.type_ != SCOPE) {
						CompilerError(EXPECTED_FUNCTION_BODY, GetErrorInfo(token), (int)token.value_.length());
						break;
					}
					else {
						if (EndStatement(tokens)) {
							// Function declaration
							AssemblyFunction func = AssemblyFunction(type, name_value);
							assemblyScript.functions_.push_back(func);
							break;
						}
						break;
					}
				}
				else if (token.type_ == EXPRESSION) {
					// TODO: Implement function parameters
					throw "Function parameters not implemented yet";
					/*token = Increment(tokens, SYMBOL_EXPECTED, ':');
					if (token.type_ != COLON) {
						CompilerError(EXPECTED_FUNCTION_BODY, GetErrorInfo(token));
						break;
					}
					else {
						// Function declaration
						assemblyScript.functions_.emplace_back(type, name_value);
						break;
					}*/
				}
				if (EndStatement(tokens)) {
					// Variable declaration
					AssemblyVariable var = AssemblyVariable(type, name_value);
					assemblyScript.variables_.push_back(var);
					break;
				}
				break;
			}
			
			// Statement is a definition
			
			token = Increment(tokens, EXPECTED_EXPRESSION);
			
			if (EndStatement(tokens)) {
			
			}
			
			break;
	}
}

bool EndStatement(vector<ParserToken> &tokens) {
	ParserToken token = tokens[compiler_index];
	
	if (token.type_ == STATEMENT_TERMINATOR)
		return true;
	
	if (compiler_index - 1 >= 0)
		token = tokens[compiler_index - 1];
	
	Increment(tokens, TERMINATOR_EXPECTED);
	if (token.type_ != STATEMENT_TERMINATOR) {
		CompilerError(TERMINATOR_EXPECTED, GetErrorInfo(token), (int)token.value_.length());
		return false;
	}
	return true;
}

ParserToken Increment(vector<ParserToken> &tokens, CompilerErrorType fail, char v) {
	ParserToken token = tokens[compiler_index];
	compiler_index++;
	if (!CheckIncrement(tokens)) {
		CompilerErrorInfo info = GetErrorInfo(token);
		if (v != 0)
			info.value_ = v;
		CompilerError(fail, info);
	}
	return tokens[compiler_index];
}

bool CheckIncrement(vector<ParserToken> &tokens) {
	return compiler_index < tokens.size();
}

// Gets the character info of the current position of the tokenizer
CompilerErrorInfo GetErrorInfo(ParserToken &token) {
	return {neptyneScript, token.line_, token.column_, token.value_};
}
