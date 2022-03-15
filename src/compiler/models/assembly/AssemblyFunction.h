//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#include <utility>

#include "../../../common_includes.h"
#include "AssemblyStatement.h"
#include "AssemblyVariable.h"

class AssemblyFunction {
 public:
  AssemblyFunction(const string& return_type, const string& function_name, vector<AssemblyVariable> parameters =
	  vector<AssemblyVariable>()) {
	  return_type_ = return_type;
	  name_ = "_";
	  name_ += function_name;
	  parameters_ = std::move(parameters);
  }
  string return_type_;
  string name_;
  vector<AssemblyVariable> parameters_;
  vector<AssemblyStatement> statements_;
};
