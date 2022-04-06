//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <utility>

#include "../../../common/common.h"
#include "TokenType.h"
#include "../../nptc_compiler/models/NeptyneScript.h"

class Token {
 public:
  TokenType type_;
  string value_;
  int line_;
  int column_;
  NeptyneScript script_file_;
  
  Token(TokenType type, string value, int line, int column, NeptyneScript &file) {
	  this->type_ = type;
	  this->value_ = std::move(value);
	  this->line_ = line;
	  this->column_ = column;
	  this->script_file_ = file;
  }
};
