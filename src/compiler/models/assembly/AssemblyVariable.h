#include <utility>

//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

class AssemblyVariable {
 public:
  AssemblyVariable(const string& variable_name) {
	  name_ = "_v_";
	  name_ += variable_name;
	  value_ = nullptr;
	  keywords_ = vector<string>();
  }
  AssemblyVariable(const string& variable_name, vector<string> variable_keywords) {
	  name_ = "_v_";
	  name_ += variable_name;
	  value_ = nullptr;
	  keywords_ = std::move(variable_keywords);
  }
  AssemblyVariable(const string& variable_name, const string& variable_value) {
	  name_ = "_v_";
	  name_ += variable_name;
	  value_ = variable_value;
	  keywords_ = vector<string>();
  }
  AssemblyVariable(const string& variable_name, const string& variable_value, vector<string> variable_keywords) {
	  name_ = "_v_";
	  name_ += variable_name;
	  value_ = variable_value;
	  keywords_ = std::move(variable_keywords);
  }
  vector<string> keywords_;
  string name_;
  string value_;
};
