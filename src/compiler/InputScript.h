//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <iostream>
#include <filesystem>

using namespace std;
namespace fs = filesystem;

class InputScript {
public:

    string full_path;
    string filename;
    string extension;
    string name;
    string directory_path;
    string output_executable_path;
    string output_assembly_path;

    InputScript(const string &file) {
        full_path = fs::canonical(file);
        filename = full_path.substr(full_path.find_last_of("/\\") + 1);
        extension = filename.substr(filename.find_last_of('.'));
        name = filename.substr(0, filename.length() - extension.length());
        directory_path = full_path.substr(0, full_path.find_last_of("/\\") + 1);
        output_executable_path = directory_path + name;
        output_assembly_path = output_executable_path + ".asm";
    }
};