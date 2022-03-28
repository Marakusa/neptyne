//
// Created by Markus Kannisto on 28/03/2022.
//

#pragma once

#include "../../../common/common.h"
#include "NeptyneScript.h"

class NeptyneProject {
 public:
  string name_;
  string version_;
  string description_;
  NeptyneScript executable_;
  
  NeptyneProject() = default;
};
