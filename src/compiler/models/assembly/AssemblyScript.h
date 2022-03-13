//
// Created by Markus Kannisto on 13/03/2022.
//

#pragma once

#include "../../../common_includes.h"
#include "AssemblyFunction.h"
#include "AssemblyStatement.h"
#include "AssemblyVariable.h"

class AssemblyScript {
public:
  AssemblyScript() = default;
  vector<AssemblyFunction> functions_;
  vector<AssemblyStatement> statements_;
  vector<AssemblyVariable> variables_;
};
