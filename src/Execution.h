#include <iostream>
#include "Logger.h"

class execution {
public:
    logger *log = nullptr;
    explicit execution(logger l) {
        log = &l;
    }

    void npt_exec(const std::string command);
};