//
// Created by Markus Kannisto on 12/03/2022.
//

#include "parser.h"

#include <utility>
#include "../nptc_compiler_errors/models/CompilerErrorInfo.h"
#include "../nptc_compiler_errors/compiler_errors.h"

int parser_index;
vector<Token> p_input_tokens;
NeptyneScript p_script;

int GetCurrentLine();
int GetCurrentColumn();

ParserToken Walk();
void NextToken(Token &token_p);
CompilerErrorInfo GetErrorInfo(string value = "");

void ParseToSyntaxTree(ParserToken &root, vector<Token> &input_tokens, NeptyneScript &input_script) {
	parser_index = 0;
	p_input_tokens = input_tokens;
	p_script = input_script;
	while (parser_index < input_tokens.size()) {
		root.parameters_.push_back(Walk());
		Token token = p_input_tokens[parser_index];
		NextToken(token);
	}
}

ParserToken Walk() {
	Token token = p_input_tokens[parser_index];
	
	switch (token.type_) {
		case OPEN_PARENTHESES: {
			NextToken(token);
			if (token.type_ != CLOSE_PARENTHESES) {
				ParserToken node = {EXPRESSION, "", GetCurrentLine(), GetCurrentColumn(), p_script};
				
				while (token.type_ != CLOSE_PARENTHESES) {
					node.parameters_.push_back(Walk());
					if (parser_index + 1 >= p_input_tokens.size()) {
						CompilerError(SYMBOL_EXPECTED, GetErrorInfo(")"));
						token = p_input_tokens[parser_index];
						NextToken(token);
					} else {
						NextToken(token);
						token = p_input_tokens[parser_index];
					}
				}
				
				return node;
			} else {
				ParserToken node = {EXPRESSION, "", GetCurrentLine(), GetCurrentColumn(), p_script};
				return node;
			}
		}
		case OPEN_BRACES: {
			NextToken(token);
			if (token.type_ != CLOSE_BRACES) {
				ParserToken node = {SCOPE, "", GetCurrentLine(), GetCurrentColumn(), p_script};
				
				while (token.type_ != CLOSE_BRACES) {
					node.parameters_.push_back(Walk());
					NextToken(token);
					token = p_input_tokens[parser_index];
					if (parser_index + 1 >= p_input_tokens.size() && token.type_ != CLOSE_BRACES)
						CompilerError(SYMBOL_EXPECTED, GetErrorInfo("}"));
				}
				
				return node;
			} else {
				ParserToken node = {SCOPE, "", GetCurrentLine(), GetCurrentColumn(), p_script};
				return node;
			}
		}
		case OPEN_BRACKETS: {
			NextToken(token);
			if (token.type_ != CLOSE_BRACKETS) {
				ParserToken node = {EXPRESSION, "", GetCurrentLine(), GetCurrentColumn(), p_script};
				
				while (token.type_ != CLOSE_BRACKETS) {
					node.parameters_.push_back(Walk());
					token = p_input_tokens[parser_index];
					if (parser_index + 1 >= p_input_tokens.size() && token.type_ != CLOSE_BRACKETS)
						CompilerError(SYMBOL_EXPECTED, GetErrorInfo("]"));
				}
				
				return node;
			} else {
				ParserToken node = {EXPRESSION, "", GetCurrentLine(), GetCurrentColumn(), p_script};
				return node;
			}
		}
		case KEYWORD: {
			if (token.value_ == "return") {
				ParserToken node = {RETURN_STATEMENT, token.value_, GetCurrentLine(), GetCurrentColumn(), p_script};
				return node;
			} else if (token.value_ == "sizeof") {
				ParserToken node = {OPERATOR, token.value_, GetCurrentLine(), GetCurrentColumn(), p_script};
				return node;
			} else if (token.value_ == "if"
				|| token.value_ == "else"
				|| token.value_ == "while"
				|| token.value_ == "for"
				|| token.value_ == "foreach") {
				string value = token.value_;
				NextToken(token);
				if (token.type_ != COLON) {
					ParserToken node = {KEYWORD, value, GetCurrentLine(), GetCurrentColumn(), p_script};
					
					while (token.type_ != COLON) {
						node.parameters_.push_back(Walk());
						token = p_input_tokens[parser_index];
						if (parser_index + 1 >= p_input_tokens.size())
							CompilerError(SYMBOL_EXPECTED, GetErrorInfo(":"));
					}
					
					return node;
				} else {
					ParserToken node = {KEYWORD, value, GetCurrentLine(), GetCurrentColumn(), p_script};
					return node;
				}
			} else {
				return {token.type_, token.value_, GetCurrentLine(), GetCurrentColumn(), p_script};
			}
		}
		default: {
			return {token.type_, token.value_, GetCurrentLine(), GetCurrentColumn(), p_script};
		}
	}
}

// Gets the character info of the current position of the tokenizer
CompilerErrorInfo GetErrorInfo(string value) {
	return {p_script, GetCurrentLine(), GetCurrentColumn(), std::move(value)};
}

int GetCurrentLine() {
	return p_input_tokens[parser_index].line_;
}
int GetCurrentColumn() {
	return p_input_tokens[parser_index].column_;
}

void NextToken(Token &token_p) {
	parser_index++;
	if (parser_index < p_input_tokens.size()) {
		token_p = p_input_tokens[parser_index];
	}
}
