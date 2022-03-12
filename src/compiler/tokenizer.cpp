//
// Created by Markus Kannisto on 10/03/2022.
//

#include <regex>
#include "../common_includes.h"
#include "tokenizer.h"
#include "compiler_errors.h"

int line, line_begin_index, i, tab_offset;
char current;
string code;
NeptyneScript script; // NOLINT(cert-err58-cpp)

const regex kNumberRegex{R"([0-9])"};
const regex kDoubleRegex{R"([+-]?([0-9]*[.])?[0-9]+)"};
const regex kNameRegex{R"~([a-zA-Z0-9_])~"};
const regex kWhitespaceRegex{R"~(\s)~"};

const vector<string> types{
	"bool",
	"byte",
	"char",
	"double",
	"float",
	"int",
	"long",
	"short",
	"string",
	"uint",
	"ulong",
	"ushort",
	"void"
};

const vector<string> keywords{
	"const",
	"readonly",
	"bring",
	"if",
	"else",
	"while",
	"for",
	"sizeof",
	"return"
};

vector<Token> tokens;

void TokenizeNumberLiteral();

void IncrementIndex();
void NextLine();
void CheckNextLine();
int GetColumn();

void AddToken(TokenType type);
void AddToken(TokenType type, const string &value);
CompilerErrorInfo GetErrorInfo();

TokenType GetTokenTyke(const string &value);

vector<Token> Tokenize(NeptyneScript &code_script) {
	line = 1;
	line_begin_index = 0;
	current = 0;
	i = 0;
	tab_offset = 0;
	
	code = code_script.code_;
	script = code_script;
	
	tokens.clear();
	
	for (i = 0; i < code.length(); i++) {
		current = code[i];
		
		// Add tab offset so error displaying would appear right
		if (current == '\t') {
			tab_offset++;
		}
		
		// Whitespace ignore
		if (regex_match(ConvertToString(current), kWhitespaceRegex)) {
			CheckNextLine();
			continue;
		}
		
		// Comments '//' and '/* ~~~ */'
		if (current == '/') {
			if (i + 1 >= code.length())
				continue;
			
			// No increment because '/' is also used as a division operator
			
			switch (code[i + 1]) {
				// Single line comment '//'
				case '/': {
					IncrementIndex();
					
					while (i + 1 < code.length()) {
						IncrementIndex();
						CheckNextLine();
						break;
					}
					break;
				}
					// Multiline comment '/* ~~~ */'
				case '*': {
					IncrementIndex();
					
					while (i + 1 < code.length()) {
						IncrementIndex();
						
						// Multiline comment end check '*/'
						if (current != '*')
							continue;
						
						IncrementIndex();
						
						// Multiline comment end '*/'
						if (current == '/')
							break;
					}
					break;
				}
				default:break;
			}
		}
		
		switch (current) {
			// Parentheses '(' and ')'
			case '(': {
				AddToken(OPEN_PARENTHESES);
				continue;
			}
			case ')': {
				AddToken(CLOSE_PARENTHESES);
				continue;
			}
				
				// Braces '{' and '}'
			case '{': {
				AddToken(OPEN_BRACES);
				continue;
			}
			case '}': {
				AddToken(CLOSE_BRACES);
				continue;
			}
				
				// Brackets '[' and ']'
			case '[': {
				AddToken(OPEN_BRACKETS);
				continue;
			}
			case ']': {
				AddToken(CLOSE_BRACKETS);
				continue;
			}
				
				// String literal
			case '"': {
				string value;
				while (i + 1 < code.length()) {
					IncrementIndex();
					
					switch (current) {
						// Character escapes
						case '\\': {
							value += ConvertToString(current);
							IncrementIndex();
							value += ConvertToString(current);
						}
							
							// End of string literal
						case '"': {
							AddToken(STRING_LITERAL, value);
							break;
						}
							
							// String character
						default: {
							value += ConvertToString(current);
						}
					}
				}
				continue;
			}
				
				// Character literal
			case '\'': {
				string value;
				
				IncrementIndex();
				
				if (current == '\\') {
					value += ConvertToString(current);
					IncrementIndex();
					value += ConvertToString(current);
				} else {
					value += ConvertToString(current);
				}
				
				IncrementIndex();
				
				if (current == '\'') {
					AddToken(CHARACTER_LITERAL, value);
					continue;
				}
				CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo());
				continue;
			}
				
				// Equals sign tokens '=' and '=='
			case '=': {
				if (i + 1 < code.length()) {
					// Comparison operator '=='
					if (code[i + 1] == '=') {
						IncrementIndex();
						AddToken(EQUALITY_OPERATOR, "==");
						continue;
					}
				}
				AddToken(ASSIGNMENT_OPERATOR);
				continue;
			}
				
				// Addition operator '+', '++' and '+='
			case '+': {
				if (i + 1 < code.length()) {
					switch (code[i + 1]) {
						// Addition assignment operator '+='
						case '=': IncrementIndex();
							AddToken(ADDITION_ASSIGNMENT_OPERATOR, "+=");
							continue;
							// Increment operator '++'
						case '+': IncrementIndex();
							AddToken(INCREMENT_OPERATOR, "++");
							continue;
						
						default: break;
					}
					
				}
				AddToken(ADDITION_OPERATOR);
				continue;
			}
				
				// Subtraction operator '-', '--' and '-='
			case '-': {
				if (i + 1 < code.length()) {
					switch (code[i + 1]) {
						// Subtraction assignment operator '+='
						case '=': IncrementIndex();
							AddToken(SUBTRACTION_ASSIGNMENT_OPERATOR, "-=");
							continue;
							// Decrement operator '++'
						case '-': IncrementIndex();
							AddToken(DECREMENT_OPERATOR, "--");
							continue;
						
						default: break;
					}
					
				}
				AddToken(SUBTRACTION_OPERATOR);
				continue;
			}
				
				// Multiplication operator '*' and '*='
			case '*': {
				if (i + 1 < code.length()) {
					// Multiplication assignment operator '*='
					if (code[i + 1] == '=') {
						IncrementIndex();
						AddToken(MULTIPLICATION_ASSIGNMENT_OPERATOR, "*=");
						continue;
					}
				}
				AddToken(MULTIPLICATION_OPERATOR);
				continue;
			}
				
				// Division operator '/' and '/='
			case '/': {
				if (i + 1 < code.length()) {
					// Division assignment operator '/='
					if (code[i + 1] == '=') {
						IncrementIndex();
						AddToken(DIVISION_ASSIGNMENT_OPERATOR, "/=");
						continue;
					}
				}
				AddToken(DIVISION_OPERATOR);
				continue;
			}
				
				// Logical comparison operators '<' and '>'
			case '<': {
				AddToken(LOGICAL_LESS_THAN_OPERATOR);
				continue;
			}
			case '>': {
				AddToken(LOGICAL_MORE_THAN_OPERATOR);
				continue;
			}
				
				// Logical AND operator '&&' and reference operator '&'
			case '&': {
				if (i + 1 < code.length()) {
					// Division assignment operator
					if (code[i + 1] == '&') {
						IncrementIndex();
						AddToken(LOGICAL_AND_OPERATOR, "&&");
						continue;
					}
				}
				AddToken(REFERENCE_OPERATOR);
				continue;
			}
				
				// Logical OR operator '||'
			case '|': {
				if (i + 1 < code.length()) {
					// Division assignment operator
					if (code[i + 1] == '|') {
						IncrementIndex();
						AddToken(LOGICAL_OR_OPERATOR, "||");
						continue;
					}
				}
				CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo());
				continue;
			}
				
				// Logical NOT operator '!' and '!='
			case '!': {
				if (i + 1 < code.length()) {
					// Logical NOT assignment operator
					if (code[i + 1] == '=') {
						IncrementIndex();
						AddToken(LOGICAL_NOT_ASSIGNMENT_OPERATOR, "!=");
						continue;
					}
				}
				AddToken(LOGICAL_NOT_OPERATOR);
				continue;
			}
				
				// Statement terminator ';'
			case ';': {
				AddToken(STATEMENT_TERMINATOR);
				continue;
			}
				
				// Colon ':'
			case ':': {
				AddToken(COLON);
				continue;
			}
				
				// Comma ','
			case ',': {
				AddToken(COMMA);
				continue;
			}
				
				// Point '.' and parameter pack '...'
			case '.': {
				if (i + 2 < code.length()) {
					// Parameter pack '...'
					if (code[i + 1] == '.' && code[i + 2] == '.') {
						IncrementIndex();
						IncrementIndex();
						AddToken(PARAMETER_PACK, "...");
						continue;
					} else if (regex_match(ConvertToString(code[i + 1]), kNumberRegex)) {
						TokenizeNumberLiteral();
						continue;
					}
				}
				AddToken(POINT);
				continue;
			}
			
			default: {
				// Number literal
				if (regex_match(ConvertToString(current), kNumberRegex)) {
					TokenizeNumberLiteral();
					continue;
				}
				
				// Other
				if (regex_match(ConvertToString(current), kNameRegex)) {
					string value;
					value += ConvertToString(current);
					
					while (i + 1 < code.length() && (regex_match(ConvertToString(code[i + 1]), kNameRegex)
						|| regex_match(ConvertToString(code[i + 1]), kNumberRegex))) {
						IncrementIndex();
						value += ConvertToString(current);
					}
					
					AddToken(GetTokenTyke(value), value);
					continue;
				}
				
				CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo());
				continue;
			}
		}
	}
	
	return tokens;
}

// Tokenize the current number literal
void TokenizeNumberLiteral() {
	bool fail = false;
	
	// First character for invalid number literal
	int first_character_line = line;
	int first_character_column = GetColumn();
	
	// Double and float literals
	bool is_float = false;
	bool is_double = false;
	
	// Number literal value
	string value;
	value += ConvertToString(current);
	while (i + 1 < code.length() && (regex_match(ConvertToString(code[i + 1]), kNameRegex) ||
		regex_match(ConvertToString(code[i + 1]), kNumberRegex) ||
		regex_match(ConvertToString(code[i + 1]), kDoubleRegex))) {
		// Integer literal '0'
		if (regex_match(ConvertToString(code[i + 1]), kNumberRegex)) {
			IncrementIndex();
			value += ConvertToString(current);
			continue;
		}
			
			// Double literal '0.0'
		else if (regex_match(ConvertToString(code[i + 1]), kDoubleRegex)) {
			is_double = true;
			IncrementIndex();
			value += ConvertToString(current);
			continue;
		}
			
			// Float literal '0.0F'
		else if (code[i + 1] == 'f' || code[i + 1] == 'F') {
			is_float = true;
			IncrementIndex();
			value += ConvertToString(current);
			
			// Invalid float literal '0.0F_'
			if ((i + 1 < code.length() && !regex_match(ConvertToString(code[i + 1]),
			                                           kWhitespaceRegex))) {
				IncrementIndex();
				CompilerError(UNEXPECTED_TOKEN, GetErrorInfo());
				continue;
			}
		}
			
			// Invalid number literal
		else if (!fail) {
			fail = true;
			IncrementIndex();
			continue;
		}
	}
	
	// Number literal failed
	if (fail) {
		CompilerErrorInfo info = GetErrorInfo();
		info.line_ = first_character_line;
		info.column_ = first_character_column;
		CompilerError(INVALID_NUMBER_LITERAL, info);
		return;
	}
	
	if (is_float) {
		AddToken(FLOAT_LITERAL, value);
	} else if (is_double) {
		AddToken(DOUBLE_LITERAL, value);
	} else {
		AddToken(INTEGER_LITERAL, value);
	}
}

// Increment the tokenizer position
void IncrementIndex() {
	i++;
	current = code[i];
}

// Jump to the next line
void NextLine() {
	line++;
	line_begin_index = i + 1;
	tab_offset = 0;
}

// Check for the next line
void CheckNextLine() {
	if (current == '\n') {
		NextLine();
	}
}

// Get the current column inside the given script file
int GetColumn() {
	return i - line_begin_index + 1 + tab_offset;
}

// Add a token to the tokens list
void AddToken(TokenType type) {
	tokens.emplace_back(type, ConvertToString(current), line, GetColumn(), script);
}

// Add a token to the tokens list with the given value
void AddToken(TokenType type, const string &value) {
	tokens.emplace_back(type, value, line, GetColumn(), script);
}

// Gets the character info of the current position of the tokenizer
CompilerErrorInfo GetErrorInfo() {
	return {script, line, GetColumn(), ConvertToString(current)};
}

// Get the token type of value and return it
TokenType GetTokenTyke(const string &value) {
	if (value == "true" || value == "false") {
		return BOOLEAN_LITERAL;
	} else if (find(types.begin(), types.end(), value) != types.end()) {
		return VALUE_TYPE;
	} else if (find(keywords.begin(), keywords.end(), value) != keywords.end()) {
		return KEYWORD;
	} else if (value == "null") {
		return NULL_VALUE;
	}
	
	return NAME;
}
