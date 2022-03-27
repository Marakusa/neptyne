//
// Created by Markus Kannisto on 09/03/2022.
//

#include "../common/common.h"
#include "builder.h"
#include "../compiler/nptc_compiler/compiler.h"

bool BuildScript(const string &file) {
	NeptyneScript script_file = NeptyneScript(file);
	cout << "Compile " << script_file.full_path_ << endl;
	Compile(script_file);
	return true;
}

bool Build(const string &file) {
	
	BuildScript(file);
	return true;
}
