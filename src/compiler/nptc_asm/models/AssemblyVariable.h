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
	  asm_ref_ = "";
  }
  AssemblyVariable(const string &variable_type, const string &variable_name, vector<string> variable_keywords) {
	  type_ = variable_type;
	  name_ = "_v_";
	  name_ += variable_name;
	  keywords_ = std::move(variable_keywords);
	  asm_ref_ = "";
  }
  AssemblyVariable(const string &variable_type,
                   const string &variable_name,
                   vector<ParserToken> &initial_value_expression) {
	  type_ = variable_type;
	  name_ = "_v_";
	  name_ += variable_name;
	  initial_value_expression_ = initial_value_expression;
	  keywords_ = vector<string>();
	  asm_ref_ = "";
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
	  asm_ref_ = "";
  }
  vector<string> keywords_;
  string type_;
  string name_;
  vector<ParserToken> initial_value_expression_;
  string asm_ref_;
  void SetAssemblyRef(const string &asm_ref) {
	  asm_ref_ = asm_ref;
  }
};
