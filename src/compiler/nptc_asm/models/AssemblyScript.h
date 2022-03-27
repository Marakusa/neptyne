//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#include "../../../common/common.h"
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
  
  void Form(string &result) {
	  result = "";
	  result += "section .data\n\n";
	  result += "section .text\n\tglobal _start\n";
	  AssemblyFunction *start_function = &functions_[0];
	  
	  // First find main function
	  for (int i = 1; i < functions_.size(); i++) {
		  AssemblyFunction *function = &functions_[i];
		  if (function->name_ == "_main") {
			  start_function->Call(*function);
			  start_function->Mov("eax", "1");
			  start_function->Mov("ebx", "0");
			  start_function->Int("80h");
		  }
	  }
	  
	  // Stringify functions
	  for (auto &i : functions_) {
		  AssemblyFunction *function = &i;
		  
		  // Function name
		  result += "\n" + function->name_ + ":\n";
		  
		  // Function init
		  if (function->name_ != "_start")
			  result += "\tpush rbp\n\tmov rbp, rsp\n";
		  
		  for (auto &statement : function->statements_) {
			  AssemblyStatement *s = &statement;
			  result += "\t" + s->instruction_ + " " + s->param_1_;
			  if (!s->param_2_.empty())
				  result += ", " + s->param_2_;
			  result += "\n";
		  }
		  
		  // Set return statement if not included in function
		  if (!function->has_return_statement_ && function->name_ != "_start")
			  result += "\tmov eax, 0\n\tpop rbp\n\tret\n";
	  }
  }
};
