//
// Created by Markus Kannisto on 09/03/2022.
//

#include "common_includes.h"
#include "builder.h"
#include "compiler/compiler.h"
#include "compiler/models/NeptyneScript.h"

bool Build(const string &file) {
	NeptyneScript script_file = NeptyneScript(file);
	cout << "Compile " << script_file.full_path_ << endl;
	Compile(script_file);
	return true;
}