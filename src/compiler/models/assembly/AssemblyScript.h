//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#include "../../../common_includes.h"
#include "AssemblyFunction.h"
#include "AssemblyStatement.h"
#include "AssemblyVariable.h"

class AssemblyScript {
 public:
  AssemblyScript() {
	  AssemblyFunction start_function = AssemblyFunction("void", "start");
	  functions_.push_back(start_function);
  }
  vector<AssemblyFunction> functions_;
  vector<AssemblyVariable> variables_;
};
