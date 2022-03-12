//
// Created by Markus Kannisto on 12/03/2022.
//

#include "parser.h"

#include <utility>
#include "models/CompilerErrorInfo.h"
#include "compiler_errors.h"

int index;
vector<struct Token> tokens;
NeptyneScript script;

int GetCurrentLine();
int GetCurrentColumn();

ParserToken Walk();
CompilerErrorInfo GetErrorInfo(string value = "");

void ParseToSyntaxTree(ParserToken root, vector<Token> &input_tokens, NeptyneScript &input_script) {
	index = 0;
	tokens = input_tokens;
	script = input_script;
	while (index < input_tokens.size()) {
		root.parameters_.push_back(Walk());
		index++;
	}
}

ParserToken Walk() {
	Token token = tokens[index];
	
	switch (token.type_) {
		case OPEN_PARENTHESES:
			index++;
			token = tokens[index];
			if (token.type_ != CLOSE_PARENTHESES) {
				ParserToken node = {EXPRESSION, "", GetCurrentLine(), GetCurrentColumn(), script};
				
				while (token.type_ != CLOSE_PARENTHESES)
				{
					node.parameters_.push_back(Walk());
					token = tokens[index];
					if (index + 1 >= tokens.size())
						CompilerError(SYMBOL_EXPECTED, GetErrorInfo(")"));
				}
				
				index++;
				return node;
			}
			else {
				ParserToken node = {EXPRESSION, "", GetCurrentLine(), GetCurrentColumn(), script};
				index++;
				return node;
			}
	}
	
	return {ROOT, script.name_, 1, 0, script};
}

// Gets the character info of the current position of the tokenizer
CompilerErrorInfo GetErrorInfo(string value) {
	return {script, GetCurrentLine(), GetCurrentColumn(), std::move(value)};
}

int GetCurrentLine() {
	return tokens[index].line_;
}
int GetCurrentColumn() {
	return tokens[index].column_;
}
