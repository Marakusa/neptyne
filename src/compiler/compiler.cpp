//
// Created by Markus Kannisto on 09/03/2022.
//

#include "../common_includes.h"
#include "../utils/file_utils.h"
#include "compiler.h"
#include "models/NeptyneScript.h"
#include "tokenizer.h"
#include "parser.h"

void Compile(const NeptyneScript &script) {
	// Read parser_script file
	string code = ReadFile(script.full_path_);
	NeptyneScript s = script;
	s.code_ = code;
	
	// Split parser_script to lines
	vector<string> lines;
	Split(lines, code, "\n");
	s.code_lines_ = lines;
	
	// Tokenize the parser_script
	vector<Token> tokens = Tokenize(s);
	
	// Parse parser_input_tokens into a syntax tree
	ParserToken root_token = ParserToken(ROOT, s.name_, 1, 1, s);
	ParseToSyntaxTree(root_token, tokens, s);
}
