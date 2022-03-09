//
// Created by Markus Kannisto on 09/03/2022.
//

#include <iostream>
#include <filesystem>
#include "builder.h"

using namespace std;
namespace fs = filesystem;

bool build(const string& file) {
    string path = fs::canonical(file);
    string input_filename = path.substr(path.find_last_of("/\\") + 1);
    string input_ext = path.substr(path.find_last_of('.'));
    string output_exe_path = path.substr(0, path.length() - input_ext.length());
    string output_cpp_path = output_exe_path + ".cpp";
    cout << "Compile " << path << endl;
    return true;
}