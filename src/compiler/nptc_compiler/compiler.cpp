//
// Created by Markus Kannisto on 09/03/2022.
//

#include <algorithm>
#include <utility>
#include "../../common_includes.h"
#include "../../utils/file_utils.h"
#include "compiler.h"
#include "models/NeptyneScript.h"
#include "../nptc_tokenizer/tokenizer.h"
#include "../nptc_parser/parser.h"
#include "../nptc_asm/models/AssemblyScript.h"
#include "../nptc_compiler_errors/models/CompilerErrorInfo.h"
#include "../nptc_compiler_errors/compiler_errors.h"

int compiler_index = 0;
AssemblyScript assemblyScript;
NeptyneScript neptyneScript;
AssemblyFunction *currentFunction;

int scope_memory_size_offset = 0;

CompilerErrorInfo GetErrorInfo(ParserToken &token);

ParserToken Increment(vector<ParserToken> &tokens, CompilerErrorType fail, char v = 0);
bool CheckIncrement(vector<ParserToken> &tokens);

void CompilerStep(vector<ParserToken> tokens, ParserToken *parent);
bool EndStatement(vector<ParserToken> &tokens, bool scope = false);

bool EqualType(vector<ParserToken> &expression_tokens, const string &type);

void DefineVariable(const AssemblyVariable &variable, ParserToken &token, ParserToken *parent,
                    ParserToken &name_token, bool declaration);

void GetExpressionTokens(vector<ParserToken> tokens, vector<ParserToken> &expression_tokens);

struct AssemblyVariableFinder {
  explicit AssemblyVariableFinder(string n) : name(std::move(n)) {}
  bool operator()(const AssemblyVariable &el) const { return el.name_ == "_v_" + name; }
 private:
  string name;
};

struct AssemblyFunctionFinder {
  explicit AssemblyFunctionFinder(string n) : name(std::move(n)) {}
  bool operator()(const AssemblyFunction &el) const { return el.name_ == "_" + name; }
 private:
  string name;
};

void Compile(const NeptyneScript &script) {
	CompilerErrorsReset();
	
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
	scope_memory_size_offset = 0;
	while (compiler_index < root_token.parameters_.size()) {
		CompilerStep(root_token.parameters_, &root_token);
		compiler_index++;
	}
	
	for (auto &function : assemblyScript.functions_) {
		if (function.name_ != "_start") {
			scope_memory_size_offset = 0;
			compiler_index = 0;
			currentFunction = &function;
			while (compiler_index < function.scope_tokens_.size()) {
				CompilerStep(function.scope_tokens_, &function.scope_);
				compiler_index++;
			}
		}
	}
	
	if (CompilerHasErrors())
		throw "Compiling failed with errors...";
	
	string result;
	assemblyScript.Form(result);
	cout << "====================" << endl << result << endl << "====================" << endl;
}

void CompilerStep(vector<ParserToken> tokens, ParserToken *parent) {
	if (compiler_index >= tokens.size()) {
		return;
	}
	ParserToken token = tokens[compiler_index];
	switch (token.type_) {
		case VALUE_TYPE: {
			// Set type
			string type = token.value_;
			
			token = Increment(tokens, EXPECTED_UNQUALIFIED_ID);
			
			// Set name
			if (token.type_ != NAME) {
				CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
				break;
			}
			string name_value = token.value_;
			ParserToken name_token = token;
			
			token = Increment(tokens, TERMINATOR_EXPECTED);
			// Check if is declaration or a function
			if (token.type_ != ASSIGNMENT_OPERATOR) {
				if (token.type_ == COLON) {
					token = Increment(tokens, EXPECTED_FUNCTION_BODY);
					if (token.type_ != SCOPE) {
						CompilerError(EXPECTED_FUNCTION_BODY, GetErrorInfo(token));
						break;
					} else {
						const ParserToken &scope = token;
						
						// Function declaration
						AssemblyFunction func = AssemblyFunction(type, name_value, scope);
						if (parent->type_ == ROOT) {
							assemblyScript.functions_.push_back(func);
						} else {
							CompilerError(FUNCTION_DEFINITION_NOT_ALLOWED, GetErrorInfo(token));
						}
						
						break;
					}
				} else if (token.type_ == EXPRESSION) {
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
					DefineVariable(var, token, parent, name_token, true);
					break;
				}
				break;
			}
			
			// Statement is a definition
			
			token = Increment(tokens, EXPECTED_EXPRESSION);
			
			// Variable definition expression
			vector<ParserToken> expression_tokens;
			GetExpressionTokens(tokens, expression_tokens);
			if (expression_tokens.empty() && token.type_ == STATEMENT_TERMINATOR) {
				CompilerError(EXPECTED_EXPRESSION, GetErrorInfo(token));
				break;
			}
			
			if (EqualType(expression_tokens, type)) {
				if (EndStatement(tokens)) {
					// Variable definition
					AssemblyVariable var = AssemblyVariable(type, name_value, expression_tokens);
					DefineVariable(var, token, parent, name_token, false);
				}
			}
			
			break;
		}
		case RETURN_STATEMENT: {
			if (currentFunction == nullptr) {
				CompilerError(RETURN_STATEMENT_NO_OUTSIDE, GetErrorInfo(token));
				break;
			}
			token = Increment(tokens, EXPECTED_UNQUALIFIED_ID);
			
			// Variable definition expression
			vector<ParserToken> expression_tokens;
			GetExpressionTokens(tokens, expression_tokens);
			if (currentFunction->return_type_ != "void" && expression_tokens.empty()
				&& token.type_ == STATEMENT_TERMINATOR) {
				CompilerError(EXPECTED_EXPRESSION, GetErrorInfo(token));
				break;
			}
			
			if (currentFunction->return_type_ == "void") {
				currentFunction->has_return_statement_ = true;
				currentFunction->SetReturnValue("0");
				break;
			}
			
			if (EqualType(expression_tokens, currentFunction->return_type_)) {
				if (EndStatement(tokens)) {
					currentFunction->has_return_statement_ = true;
					// TODO: Array combine
					if (expression_tokens[0].type_ == NAME) {
						// Check local variables
						auto v = find_if(currentFunction->variables_.begin(), currentFunction->variables_.end(),
						                 AssemblyVariableFinder(token.value_));
						if (v != currentFunction->variables_.end()) {
							currentFunction->SetReturnValue(v->asm_ref_);
						}
						else {
							CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
						}
					}
					else
						currentFunction->SetReturnValue(expression_tokens[0].value_);
					break;
				}
			}
			
			break;
		}
	}
}

void GetExpressionTokens(vector<ParserToken> tokens, vector<ParserToken> &expression_tokens) {
	ParserToken token = tokens[compiler_index];
	expression_tokens = vector<ParserToken>();
	while (compiler_index < tokens.size() && token.type_ != STATEMENT_TERMINATOR) {
		expression_tokens.push_back(token);
		token = Increment(tokens, TERMINATOR_EXPECTED);
		if (token.type_ != COMMA && token.type_ != STATEMENT_TERMINATOR) {
			CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
			break;
		}
		if (token.type_ != STATEMENT_TERMINATOR)
			token = Increment(tokens, TERMINATOR_EXPECTED);
	}
}

string GetDeclarationType(ParserToken &token, bool throwErrorOnNull) {
	if (currentFunction != nullptr) {
		// Check local variables
		auto v = find_if(currentFunction->variables_.begin(), currentFunction->variables_.end(),
		                 AssemblyVariableFinder(token.value_));
		if (v != currentFunction->variables_.end()) {
			return v->type_;
		}
	}
	
	// Check global variables
	auto v = find_if(assemblyScript.variables_.begin(), assemblyScript.variables_.end(),
	                 AssemblyVariableFinder(token.value_));
	if (v != assemblyScript.variables_.end()) {
		return v->type_;
	}
	
	// Check global functions
	auto f = find_if(assemblyScript.functions_.begin(), assemblyScript.functions_.end(),
	                 AssemblyFunctionFinder(token.value_));
	if (f != assemblyScript.functions_.end()) {
		return f->return_type_;
	}
	
	if (throwErrorOnNull)
		CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo(token));
	return "";
}

string GetAssemblyNameOfVariable(ParserToken &token) {
	if (currentFunction != nullptr) {
		// Check local variables
		auto v = find_if(currentFunction->variables_.begin(), currentFunction->variables_.end(),
		                 AssemblyVariableFinder(token.value_));
		if (v != currentFunction->variables_.end()) {
			return v->asm_ref_;
		}
	}
	
	// Check global variables
	auto v = find_if(assemblyScript.variables_.begin(), assemblyScript.variables_.end(),
	                 AssemblyVariableFinder(token.value_));
	if (v != assemblyScript.variables_.end()) {
		return v->name_;
	}
	
	// Check global functions
	auto f = find_if(assemblyScript.functions_.begin(), assemblyScript.functions_.end(),
	                 AssemblyFunctionFinder(token.value_));
	if (f != assemblyScript.functions_.end()) {
		// TODO: Functions on binary expression trees
		return f->name_;
	}
	
	CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo(token));
	return "";
}

int get_type_size(string type) {
	if (type == "bool" || type == "byte") {
		return 1;
	}
	else if (type == "short" || type == "ushort") {
		return 2;
	}
	else if (type == "int" || type == "uint" || type == "float" || type == "char") {
		return 4;
	}
	else if (type == "long" || type == "ulong" || type == "double") {
		return 8;
	}
	else {
		throw "Type size not set for " + type;
	}
}

void DefineVariable(const AssemblyVariable &variable, ParserToken &token, ParserToken *parent,
                    ParserToken &name_token, bool declaration) {
	if (GetDeclarationType(name_token, false).empty()) {
		if (parent->type_ == ROOT) {
			assemblyScript.variables_.push_back(variable);
			// TODO: Define outside function
		} else if (parent->type_ == SCOPE) {
			currentFunction->variables_.push_back(variable);
			scope_memory_size_offset += get_type_size(variable.type_);
			string to = "DWORD [rbp-" + to_string(scope_memory_size_offset) + "]";
			currentFunction->DefineVariable(to, variable);
		} else {
			if (declaration) {
				CompilerError(VARIABLE_DECLARATION_NOT_ALLOWED, GetErrorInfo(name_token));
			} else {
				CompilerError(VARIABLE_DEFINITION_NOT_ALLOWED, GetErrorInfo(name_token));
			}
		}
	} else {
		CompilerError(VARIABLE_EXISTS, GetErrorInfo(name_token));
	}
}

bool EqualType(vector<ParserToken> &expression_tokens, const string &type) {
	int i = 0;
	
	string t;
	switch (expression_tokens[i].type_) {
		case NAME: {
			t = GetDeclarationType(expression_tokens[i], true);
			break;
		}
		case STRING_LITERAL: {
			t = "string";
			break;
		}
		case INTEGER_LITERAL: {
			t = "int";
			break;
		}
		case CHARACTER_LITERAL: {
			t = "char";
			break;
		}
		case BOOLEAN_LITERAL: {
			t = "bool";
			break;
		}
		case FLOAT_LITERAL: {
			t = "float";
			break;
		}
		case DOUBLE_LITERAL: {
			t = "double";
			break;
		}
		default:break;
	}
	if (t.empty()) return false;
	
	if (type == "bool" && t == "bool") {
		if (expression_tokens[i].type_ == NAME) {
			return true;
		}
		
		return true;
	} else if (type == "byte" && t == "int") {
		if (expression_tokens[i].type_ == NAME) {
			return true;
		}
		
		long long num_val = stoll(expression_tokens[i].value_);
		if (num_val >= 0 && num_val <= 255)
			return true;
		
		CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_tokens[i]));
		return false;
	} else if (type == "int" && t == "int") {
		if (expression_tokens[i].type_ == NAME) {
			return true;
		}
		
		long long num_val = stoll(expression_tokens[i].value_);
		if (num_val >= -2147483648 && num_val <= 2147483647)
			return true;
		
		CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_tokens[i]));
		return false;
	}
	
	/*TODO: These types below
	char
	double
	float
	long
	short
	string
	uint
	ulong
	ushort
	void*/
	
	CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo(expression_tokens[i]));
	return false;
}

bool EndStatement(vector<ParserToken> &tokens, bool scope) {
	ParserToken token = tokens[compiler_index];
	
	if (token.type_ == STATEMENT_TERMINATOR)
		return true;
	
	if (compiler_index - 1 >= 0)
		token = tokens[compiler_index - 1];
	
	if (!scope) {
		Increment(tokens, TERMINATOR_EXPECTED);
		if (token.type_ != STATEMENT_TERMINATOR) {
			CompilerError(TERMINATOR_EXPECTED, GetErrorInfo(token));
			return false;
		}
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

AssemblyVariable GetVariable(string name) {
	if (currentFunction != nullptr) {
		return currentFunction->variables_
	}
}

void HandleBinaryExpressionTree(vector<ParserToken> &expression_tokens, string expression_type) {
	if (expression_tokens.size() == 1) {
		if (expression_tokens[0].type_ == NAME) {
			if (expression_type == GetDeclarationType(expression_tokens[0])) {
				currentFunction->Mov("eax", GetAssemblyNameOfVariable(expression_tokens[0]));
				return;
			}
			else {
				CompilerError(VARIABLE_DOESNT_EXIST, )
			}
		}
		else if ()
	}
}
