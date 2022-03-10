//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <utility>

#include "NeptyneScript.h"

class CompilerErrorInfo {
 public:
  CompilerErrorInfo(NeptyneScript &file, int line, int column, string value) : file_(file) {
	  file = file;
	  line = line;
	  column = column;
	  value = std::move(value);
  }
  
  NeptyneScript &file_;
  int line_;
  int column_;
  string value_;
};


