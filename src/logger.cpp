//
// Created by Markus Kannisto on 09/03/2022.
//

#include <iostream>
#include "logger.h"

using namespace std;

void Log(const char *format, ...) {
    printf(format);
    cout << endl;
}