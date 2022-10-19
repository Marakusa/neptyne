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
  vector<string> string_literals_;
  vector<string> string_literal_names_;
  
  void Form(string &result) {
	  result = "";
	  result += "section .data\n" + ParseStringLiterals() + "\n";
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

  string ParseStringLiterals() {
	  string result = "";

	  result += "\tnewline db 0xa\n";
	  
	  for (int i = 0; i < string_literals_.size(); i++) {
		  string name = string_literal_names_[i];
		  string value = string_literals_[i];
		  result += "\t" + name + " db \"" + EscapeString(value) + "\", 0\n";
		  result += "\t" + name + "_len equ $ - " + name + "\n";
	  }
	  return result;
  }

  string EscapeString(string s) {
	  string result = "";
	  for (int i = 0; i < s.size(); i++) {
		  char c = s[i];

		  if (c == '\\') {
			  i++;
			  if (i >= s.size())
				  break;
		  }
		  else {
			  result += c;
		  	  continue;
		  }

		  c = s[i];

		  if (c == 'n') {
			  result += "\", 0xa, \"";
		  } else if (c == 't') {
			  result += "\", 0x9, \"";
		  } else if (c == 'r') {
			  result += "\", 0xd, \"";
		  } else if (c == 'v') {
			  result += "\", 0xb, \"";
		  } else if (c == 'f') {
			  result += "\", 0xc, \"";
		  } else if (c == 'a') {
			  result += "\", 0x7, \"";
		  } else if (c == 'b') {
			  result += "\", 0x8, \"";
		  } else if (c == '0') {
			  result += "\", 0x0, \"";
		  } else {
			  result += c;
		  }
	  }
	  return result;
  }
};
