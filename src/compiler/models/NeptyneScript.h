//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include "../../common_includes.h"

class NeptyneScript {
 public:
  string full_path_;
  string filename_;
  string extension_;
  string name_;
  string directory_path_;
  string output_executable_path_;
  string output_assembly_path_;
  
  string code_;
  vector<string> code_lines_;
  
  explicit NeptyneScript(const string &file) {
	  full_path_ = fs::canonical(file);
	  filename_ = full_path_.substr(full_path_.find_last_of("/\\") + 1);
	  extension_ = filename_.substr(filename_.find_last_of('.'));
	  name_ = filename_.substr(0, filename_.length() - extension_.length());
	  directory_path_ = full_path_.substr(0, full_path_.find_last_of("/\\") + 1);
	  output_executable_path_ = directory_path_ + name_;
	  output_assembly_path_ = output_executable_path_ + ".asm";
  }
  
  NeptyneScript() = default;
};