//
// Created by Markus Kannisto on 09/03/2022.
//

#include "common_includes.h"
#include "builder.h"
#include "compiler/compiler.h"
#include "compiler/NeptyneScript.h"

bool build(const string &file) {
    NeptyneScript script_file = NeptyneScript(file);
    cout << "Compile " << script_file.full_path << endl;
    cout << "Compile " << script_file.filename << endl;
    cout << "Compile " << script_file.extension << endl;
    cout << "Compile " << script_file.name << endl;
    cout << "Compile " << script_file.directory_path << endl;
    cout << "Compile " << script_file.output_executable_path << endl;
    cout << "Compile " << script_file.output_assembly_path << endl;
    compile(script_file);
    return true;
}