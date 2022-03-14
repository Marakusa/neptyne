#include <utility>

//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

class AssemblyVariable {
 public:
  explicit AssemblyVariable(const string &variable_name)
	  : AssemblyVariable(variable_name, "", vector<string>()) {}
  AssemblyVariable(const string &variable_name, vector<string> variable_keywords)
	  : AssemblyVariable(variable_name, "", std::move(variable_keywords)) {}
  AssemblyVariable(const string &variable_name, const string &variable_value)
	  : AssemblyVariable(variable_name, variable_value, vector<string>()) {}
  AssemblyVariable(const string &variable_name, const string &variable_value, vector<string> variable_keywords) {
	  name_ = "_v_";
	  name_ += variable_name;
	  value_ = variable_value;
	  keywords_ = std::move(variable_keywords);
  }
  vector<string> keywords_;
  string name_;
  string value_;
};
