//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#import "../../../common_includes.h"

class AssemblyStatement {
 public:
  explicit AssemblyStatement(const string& instruction, const string& param_1 = "", const string& param_2 = "") {
	  instruction_ = instruction;
	  param_1_ = param_1;
	  param_2_ = param_2;
  }
  
  string instruction_;
  string param_1_;
  string param_2_;
};
