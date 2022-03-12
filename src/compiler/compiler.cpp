//
// Created by Markus Kannisto on 09/03/2022.
//

#include "../common_includes.h"
#include "../utils/file_utils.h"
#include "compiler.h"
#include "models/NeptyneScript.h"
#include "tokenizer.h"

void Compile(const NeptyneScript &script) {
	// Read script file
	string code = ReadFile(script.full_path_);
	NeptyneScript s = script;
	s.code_ = code;
	
	// Split script to lines
	vector<string> lines;
	Split(lines, code, "\n");
	s.code_lines_ = lines;
	
	// Tokenize the script
	vector<Token> tokens = Tokenize(s);
}
