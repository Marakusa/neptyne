//
// Created by Markus Kannisto on 12/03/2022.
//

#include "parser.h"

void ParseToSyntaxTree(ParserToken root, vector<Token> &input_tokens, NeptyneScript &input_script) {
	int index = 0;
	while (index < input_tokens.size()) {
		root.parameters_.push_back(Walk(index, input_tokens, input_script));
	}
}

ParserToken Walk(int &index, vector<struct Token> &tokens, NeptyneScript &script) {
	return {ROOT, script.name_, 1, 0, script};
}
