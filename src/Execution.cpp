#include "Execution.h"
#include "StringUtils.h"

void execution::npt_exec(const std::string command) {
    std::string *lexemes = StringUtils::str_split(StringUtils::trim(command), ',');
    std::string m = "Lexemes: ";
    int i = 0;
    while (!lexemes[i].empty()) {
        m += lexemes[0] + '\n';
    }
    log->debug(m);
}
