//
// Created by Markus Kannisto on 09/03/2022.
//

#include <iostream>
#include "../utils/fileutils.h"
#include "compiler.h"

using namespace std;
namespace fs = filesystem;

void compile(const string& file) {
    string code = read_file(file);
}
