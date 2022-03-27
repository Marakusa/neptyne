//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#include <utility>

#include "../../../common/common.h"
#include "AssemblyStatement.h"
#include "AssemblyVariable.h"

class AssemblyFunction {
 public:
  AssemblyFunction(const string &return_type, const string &function_name, vector<AssemblyVariable> parameters =
  vector<AssemblyVariable>()) {
	  return_type_ = return_type;
	  name_ = "_";
	  name_ += function_name;
	  parameters_ = std::move(parameters);
	  scope_ = kNullToken;
	  scope_tokens_ = vector<ParserToken>();
	  has_return_statement_ = false;
  }
  AssemblyFunction(const string &return_type, const string &function_name, ParserToken scope,
                   vector<AssemblyVariable> parameters = vector<AssemblyVariable>()) {
	  return_type_ = return_type;
	  name_ = "_";
	  name_ += function_name;
	  parameters_ = std::move(parameters);
	  scope_ = std::move(scope);
	  scope_tokens_ = scope_.parameters_;
	  has_return_statement_ = false;
  }
  string return_type_;
  string name_;
  vector<AssemblyVariable> parameters_;
  vector<AssemblyStatement> statements_;
  ParserToken scope_ = kNullToken;
  vector<ParserToken> scope_tokens_;
  vector<AssemblyVariable> variables_;
  bool has_return_statement_;
  
  void Mov(const string &to, const string &from) {
	  statements_.emplace_back("mov", to, from);
  }
  void Int(const string &interrupt_number) {
	  statements_.emplace_back("int", interrupt_number);
  }
  void Call(AssemblyFunction function) {
	  statements_.emplace_back("call", function.name_);
  }
  void Push(const string &target) {
	  statements_.emplace_back("push", target);
  }
  void Pop(const string &target) {
	  statements_.emplace_back("pop", target);
  }
  void Ret() {
	  statements_.emplace_back("ret");
  }
  
  void DefineVariable(const string& to, AssemblyVariable& variable) {
	  variable.SetAssemblyRef(to);
	  Mov(to, "eax");
  }
  
  void Return() {
	  has_return_statement_ = true;
	  Pop("rbp");
	  Ret();
  }
};
