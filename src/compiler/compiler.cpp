//
// Created by Markus Kannisto on 09/03/2022.
//

#include <iostream>
#include "../utils/fileutils.h"
#include "compiler.h"
#include "InputScript.h"

using namespace std;
namespace fs = filesystem;

void compile(const InputScript& script) {
    string code = read_file(script.full_path);
    cout << code << endl;
}
