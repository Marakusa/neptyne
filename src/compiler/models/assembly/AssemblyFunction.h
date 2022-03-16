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
	  scope_ = kNullToken;
	  scope_tokens_ = vector<ParserToken>();
  }
  AssemblyFunction(const string& return_type, const string& function_name, ParserToken scope,
      vector<AssemblyVariable> parameters = vector<AssemblyVariable>()) {
	  return_type_ = return_type;
	  name_ = "_";
	  name_ += function_name;
	  parameters_ = std::move(parameters);
	  scope_ = std::move(scope);
	  scope_tokens_ = scope_.parameters_;
  }
  string return_type_;
  string name_;
  vector<AssemblyVariable> parameters_;
  vector<AssemblyStatement> statements_;
  ParserToken scope_ = kNullToken;
  vector<ParserToken> scope_tokens_;
  vector<AssemblyVariable> variables_;
};
