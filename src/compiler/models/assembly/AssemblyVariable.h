#include <utility>

//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

class AssemblyVariable {
 public:
  AssemblyVariable(const string &variable_type, const string &variable_name) {
	  type_ = variable_type;
	  name_ = "_v_";
	  name_ += variable_name;
	  keywords_ = vector<string>();
  }
  AssemblyVariable(const string &variable_type, const string &variable_name, vector<string> variable_keywords) {
	  type_ = variable_type;
	  name_ = "_v_";
	  name_ += variable_name;
	  keywords_ = std::move(variable_keywords);
  }
  AssemblyVariable(const string &variable_type,
                   const string &variable_name,
                   vector<ParserToken> &initial_value_expression) {
	  type_ = variable_type;
	  name_ = "_v_";
	  name_ += variable_name;
	  initial_value_expression_ = initial_value_expression;
	  keywords_ = vector<string>();
  }
  AssemblyVariable(const string &variable_type,
                   const string &variable_name,
                   vector<ParserToken> &initial_value_expression,
                   vector<string> variable_keywords) {
	  type_ = variable_type;
	  name_ = "_v_";
	  name_ += variable_name;
	  initial_value_expression_ = initial_value_expression;
	  keywords_ = std::move(variable_keywords);
  }
  vector<string> keywords_;
  string type_;
  string name_;
  vector<ParserToken> initial_value_expression_;
};
