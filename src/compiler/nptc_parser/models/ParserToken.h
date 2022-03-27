//
// Created by Markus Kannisto on 12/03/2022.
//

#pragma once

#include "../../../common/common.h"
#include "../../nptc_tokenizer/models/TokenType.h"
#include "../../nptc_compiler/models/NeptyneScript.h"

class ParserToken {
 public:
  vector<ParserToken> parameters_;
  TokenType type_;
  string value_;
  int line_;
  int column_;
  NeptyneScript script_file_;
  
  ParserToken(TokenType type, string value, int line, int column, NeptyneScript &file) {
	  this->type_ = type;
	  this->value_ = std::move(value);
	  this->line_ = line;
	  this->column_ = column;
	  this->script_file_ = file;
  }
};

static ParserToken kNullToken = *new ParserToken(NULL_VALUE, "", 0, 0, kNullScript);
