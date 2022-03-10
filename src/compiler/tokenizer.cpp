//
// Created by Markus Kannisto on 10/03/2022.
//

#include <regex>
#include "../common_includes.h"
#include "tokenizer.h"
#include "compiler_errors.h"

int line, column, i;
char current;
string code;
NeptyneScript script; // NOLINT(cert-err58-cpp)

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

void add_token(vector<Token> &tokens, TokenType type) {
    tokens.emplace_back(OpenParentheses, getString(current), line, column, script);
}

void add_token(vector<Token> &tokens, TokenType type, const string &value) {
    tokens.emplace_back(OpenParentheses, value, line, column, script);
}

CompilerErrorInfo getErrorInfo() {
    return CompilerErrorInfo(script, line, column, getString(current));
}

vector<Token> tokenize(const string &input_code, NeptyneScript &code_script) {
    line = 1;
    column = 0;
    current = 0;
    code = input_code;
    i = -1;
    script = code_script;

    vector<Token> tokens;

    const regex numberRegex{R"([0-9])"};
    const regex floatRegex{R"([+-]?([0-9]*[.])?[0-9]+)"};
    const regex nameRegex{R"~([a-zA-Z0-9_])~"};
    const regex whitespaceRegex{R"~(\s)~"};

    for (i = -1; i < code.length(); i++) {
        increment_index();

        if (std::regex_match(getString(current), whitespaceRegex)) {
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

            // Parentheses
            case '(': {
                add_token(tokens, OpenParentheses);
                break;
            }
            case ')': {
                add_token(tokens, CloseParentheses);
                break;
            }

            // Braces
            case '{': {
                add_token(tokens, OpenBraces);
                break;
            }
            case '}': {
                add_token(tokens, CloseBraces);
                break;
            }

            // Brackets
            case '[': {
                add_token(tokens, OpenBrackets);
                break;
            }
            case ']': {
                add_token(tokens, CloseBrackets);
                break;
            }

            // String literal
            case '"': {
                string value;
                while (i + 1 < input_code.length())
                {
                    increment_index();

                    switch (current) {
                        case '\\': {
                            value += getString(current);
                            increment_index();
                            value += getString(current);
                        }
                        case '"': {
                            add_token(tokens, StringLiteral, value);
                            break;
                        }
                        default: {
                            value += getString(current);
                        }
                    }
                }
                break;
            }

            // Character literal
            case '\'': {
                string value;

                increment_index();

                if (current == '\\') {
                    value += getString(current);
                    increment_index();
                    value += getString(current);
                }
                else {
                    value += getString(current);
                }

                increment_index();

                if (current == '\'') {
                    add_token(tokens, CharacterLiteral, value);
                    continue;
                }
                compilerError(UnexpectedToken, getErrorInfo());
                break;
            }
        }
    }

    return tokens;
}
