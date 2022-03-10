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

void IncrementIndex() {
	i++;
	current = code[i];
}

void NextLine() {
	line++;
	line_begin_index = i + 1;
	tab_offset = 0;
}

void CheckNextLine() {
	if (current == '\n') {
		NextLine();
	}
}

int GetColumn() {
	return i - line_begin_index + 1 + tab_offset;
}

void AddToken(vector<Token> &tokens, TokenType type) {
	tokens.emplace_back(OPEN_PARENTHESES, GetString(current), line, GetColumn(), script);
}

void AddToken(vector<Token> &tokens, TokenType type, const string &value) {
	tokens.emplace_back(OPEN_PARENTHESES, value, line, GetColumn(), script);
}

CompilerErrorInfo GetErrorInfo() {
	return CompilerErrorInfo(script, line, GetColumn(), GetString(current));
}

vector<Token> Tokenize(NeptyneScript &code_script) {
	line = 1;
	line_begin_index = 0;
	current = 0;
	code = code_script.code_;
	i = 0;
	script = code_script;
	tab_offset = 0;
	
	vector<Token> tokens;
	
	const regex kNumberRegex{R"([0-9])"};
	const regex kFloatRegex{R"([+-]?([0-9]*[.])?[0-9]+)"};
	const regex kNameRegex{R"~([a-zA-Z0-9_])~"};
	const regex kWhitespaceRegex{R"~(\s)~"};
	
	cout << i << endl;
	cout << code.length() << endl;
	cout << (i < code.length()) << endl;
	for (i = 0; i < code.length(); i++) {
		current = code[i];
		
		// Add tab offset so error displaying would appear right
		if (current == '\t') {
			tab_offset++;
		}
		
		// Whitespace ignore
		if (std::regex_match(GetString(current), kWhitespaceRegex)) {
			CheckNextLine();
			continue;
		}
		
		switch (current) {
			// Comments '//' and '/* ~~~ */'
			case '/': {
				if (i + 1 >= code.length())
					continue;
				
				IncrementIndex();
				
				switch (current) {
					// Single line comment '//'
					case '/': {
						while (i + 1 < code.length()) {
							IncrementIndex();
							CheckNextLine();
							break;
						}
						break;
					}
						// Multiline comment '/* ~~~ */'
					case '*': {
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
				break;
			}
				
				// Parentheses '(' and ')'
			case '(': {
				AddToken(tokens, OPEN_PARENTHESES);
				break;
			}
			case ')': {
				AddToken(tokens, CLOSE_PARENTHESES);
				break;
			}
				
				// Braces '{' and '}'
			case '{': {
				AddToken(tokens, OPEN_BRACES);
				break;
			}
			case '}': {
				AddToken(tokens, CLOSE_BRACES);
				break;
			}
				
				// Brackets '[' and ']'
			case '[': {
				AddToken(tokens, OPEN_BRACKETS);
				break;
			}
			case ']': {
				AddToken(tokens, CLOSE_BRACKETS);
				break;
			}
				
				// String literal
			case '"': {
				string value;
				while (i + 1 < code.length()) {
					IncrementIndex();
					
					switch (current) {
						case '\\': {
							value += GetString(current);
							IncrementIndex();
							value += GetString(current);
						}
						case '"': {
							AddToken(tokens, STRING_LITERAL, value);
							break;
						}
						default: {
							value += GetString(current);
						}
					}
				}
				break;
			}
				
				// Character literal
			case '\'': {
				string value;
				
				IncrementIndex();
				
				if (current == '\\') {
					value += GetString(current);
					IncrementIndex();
					value += GetString(current);
				} else {
					value += GetString(current);
				}
				
				IncrementIndex();
				
				if (current == '\'') {
					AddToken(tokens, CHARACTER_LITERAL, value);
					continue;
				}
				CompilerError(UNEXPECTED_TOKEN, GetErrorInfo());
				break;
			}
				
				// Equals sign tokens '=' and '=='
			case '=': {
				if (i + 1 < code.length()) {
					// Comparison operator '=='
					if (code[i + 1] == '=') {
						IncrementIndex();
						AddToken(tokens, EQUALITY_OPERATOR, "==");
						continue;
					}
				}
				AddToken(tokens, ASSIGNMENT_OPERATOR, GetString(current));
			}
			
			default:CompilerError(UNEXPECTED_TOKEN, GetErrorInfo());
				break;
		}
	}
	
	return tokens;
}
