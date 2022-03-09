//
// Created by Markus Kannisto on 09/03/2022.
//

#include <iostream>
#include <filesystem>
#include "builder.h"
#include "compiler/compiler.h"
#include "compiler/InputScript.h"

using namespace std;
namespace fs = filesystem;

bool build(const string &file) {
    InputScript script_file = InputScript(file);
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