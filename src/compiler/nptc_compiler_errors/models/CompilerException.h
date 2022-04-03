//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#import "../../../common/common.h"

struct CompilerException : public exception {
  string message;
 
 public:
  CompilerException(exception msg, const char *file, int line, int column) : std::exception(std::move(msg)) {
	  string e_prefix = "[nptc] ";
	  string e_suffix = ": (Line " + to_string(line) + ", Col " + to_string(column) + ")";
	  message = e_prefix + msg.what() + e_suffix;
  }
  
  [[nodiscard]] string Get() const { return message; }
};


