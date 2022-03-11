//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <utility>

#include "NeptyneScript.h"

class CompilerErrorInfo {
 public:
  CompilerErrorInfo(NeptyneScript &file, int line, int column, string value) : file_(file) {
	  file_ = file;
	  line_ = line;
	  column_ = column;
	  value_ = std::move(value);
  }
  
  NeptyneScript &file_;
  int line_;
  int column_;
  string value_;
};


