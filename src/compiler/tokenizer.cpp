//
// Created by Markus Kannisto on 10/03/2022.
//

#include <regex>
#include "../common_includes.h"
#include "tokenizer.h"

int line, column, i;
char current;
string code;

void increment_index() {
    i++;
    column++;
    current = code[i];
}

void next_line() {
    line++;
    column = 0;
}

void check_next_line() {
    if (current == '\n') {
        next_line();
    }
}

void add_token(vector<Token> &tokens, NeptyneScript &current_script, TokenType type) {
    tokens.emplace_back(OpenParentheses, get_string(current), line, column, current_script);
}
void add_token(vector<Token> &tokens, NeptyneScript &current_script, TokenType type, const string &value) {
    tokens.emplace_back(OpenParentheses, value, line, column, current_script);
}

vector<Token> tokenize(const string &input_code, NeptyneScript &script) {
    line = 1;
    column = 0;
    current = 0;
    code = input_code;
    i = -1;

    vector<Token> tokens;

    const regex numberRegex{R"([0-9])"};
    const regex floatRegex{R"([+-]?([0-9]*[.])?[0-9]+)"};
    const regex nameRegex{R"~([a-zA-Z0-9_])~"};
    const regex whitespaceRegex{R"~(\s)~"};

    for (i = -1; i < code.length(); i++) {
        increment_index();

        if (std::regex_match(get_string(current), whitespaceRegex)) {
            check_next_line();
            continue;
        }

        switch (current) {
            case '/': {
                if (i + 1 >= input_code.length())
                    continue;

                increment_index();

                switch (current) {
                    case '/': {
                        while (i + 1 < input_code.length()) {
                            increment_index();
                            check_next_line();
                            break;
                        }
                        break;
                    }
                    case '*': {
                        while (i + 1 < input_code.length()) {
                            increment_index();

                            if (current != '*')
                                continue;

                            increment_index();

                            if (current == '/')
                                break;
                        }
                        break;
                    }
                    default:
                        break;
                }
                break;
            }

            case '(': {
                add_token(tokens, script, OpenParentheses);
                break;
            }
            case ')': {
                add_token(tokens, script, CloseParentheses);
                break;
            }

            case '{': {
                add_token(tokens, script, OpenBraces);
                break;
            }
            case '}': {
                add_token(tokens, script, CloseBraces);
                break;
            }

            case '[': {
                add_token(tokens, script, OpenBrackets);
                break;
            }
            case ']': {
                add_token(tokens, script, CloseBrackets);
                break;
            }
        }
    }

    return tokens;
}
