//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

#include <map>
#include "../common_includes.h"
#include "models/NeptyneScript.h"
#include "models/CompilerErrorInfo.h"

enum CompilerErrorType {
  UNASSIGNED,
  UNEXPECTED_TOKEN,
  CANNOT_RESOLVE_SYMBOL,
  TERMINATOR_EXPECTED,
  SYMBOL_EXPECTED,
  INVALID_NUMBER_LITERAL,
  INVALID_BRING_STATEMENT_PLACEMENT,
  EXPECTED_UNQUALIFIED_ID,
  EXPECTED_EXPRESSION,
  EXPECTED_FUNCTION_BODY,
  OUTSIDE_THE_RANGE,
};

class CompilerError {
 public:
  CompilerError(string error_code, string error_message, CompilerErrorType error_type) {
	  code_ = std::move(error_code);
	  message_ = std::move(error_message);
	  type_ = error_type;
  };
  
  string code_;
  string message_;
  CompilerErrorType type_;
};

void CompilerError(CompilerErrorType code, const CompilerErrorInfo &error_info);
