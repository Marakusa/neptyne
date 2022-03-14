//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#include <utility>

#include "../../../common_includes.h"
#include "AssemblyStatement.h"

class AssemblyFunction {
 public:
  explicit AssemblyFunction(const string& function_name) {
	  name_ = "_";
	  name_ += function_name;
  }
  string name_;
  vector<AssemblyStatement> statements_;
};
