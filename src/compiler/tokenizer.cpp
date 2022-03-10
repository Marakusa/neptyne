//
// Created by Markus Kannisto on 10/03/2022.
//

#include <regex>
#include "../common_includes.h"
#include "tokenizer.h"
#include "compiler_errors.h"

int line, line_begin_index, i, tab_offset;
char current;
string code;
NeptyneScript script; // NOLINT(cert-err58-cpp)

void incrementIndex() {
    i++;
    current = code[i];
}

void nextLine() {
    line++;
    line_begin_index = i + 1;
    tab_offset = 0;
}

void checkNextLine() {
    if (current == '\n') {
        nextLine();
    }
}

int getColumn() {
    return i - line_begin_index + 1 + tab_offset;
}

void addToken(vector<Token> &tokens, TokenType type) {
    tokens.emplace_back(OpenParentheses, getString(current), line, getColumn(), script);
}

void addToken(vector<Token> &tokens, TokenType type, const string &value) {
    tokens.emplace_back(OpenParentheses, value, line, getColumn(), script);
}

CompilerErrorInfo getErrorInfo() {
    return CompilerErrorInfo(script, line, getColumn(), getString(current));
}

vector<Token> tokenize(NeptyneScript &code_script) {
    line = 1;
    line_begin_index = 0;
    current = 0;
    code = code_script.code;
    i = 0;
    script = code_script;
    tab_offset = 0;

    vector<Token> tokens;

    const regex numberRegex{R"([0-9])"};
    const regex floatRegex{R"([+-]?([0-9]*[.])?[0-9]+)"};
    const regex nameRegex{R"~([a-zA-Z0-9_])~"};
    const regex whitespaceRegex{R"~(\s)~"};

    cout << i << endl;
    cout << code.length() << endl;
    cout << (i < code.length()) << endl;
    for (i = 0; i < code.length(); i++) {
        current = code[i];

        if (current == '\t') {
            tab_offset++;
        }

        if (std::regex_match(getString(current), whitespaceRegex)) {
            checkNextLine();
            continue;
        }

        switch (current) {
            case '/': {
                if (i + 1 >= code.length())
                    continue;

                incrementIndex();

                switch (current) {
                    case '/': {
                        while (i + 1 < code.length()) {
                            incrementIndex();
                            checkNextLine();
                            break;
                        }
                        break;
                    }
                    case '*': {
                        while (i + 1 < code.length()) {
                            incrementIndex();

                            if (current != '*')
                                continue;

                            incrementIndex();

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
                addToken(tokens, OpenParentheses);
                break;
            }
            case ')': {
                addToken(tokens, CloseParentheses);
                break;
            }

            // Braces
            case '{': {
                addToken(tokens, OpenBraces);
                break;
            }
            case '}': {
                addToken(tokens, CloseBraces);
                break;
            }

            // Brackets
            case '[': {
                addToken(tokens, OpenBrackets);
                break;
            }
            case ']': {
                addToken(tokens, CloseBrackets);
                break;
            }

            // String literal
            case '"': {
                string value;
                while (i + 1 < code.length())
                {
                    incrementIndex();

                    switch (current) {
                        case '\\': {
                            value += getString(current);
                            incrementIndex();
                            value += getString(current);
                        }
                        case '"': {
                            addToken(tokens, StringLiteral, value);
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

                incrementIndex();

                if (current == '\\') {
                    value += getString(current);
                    incrementIndex();
                    value += getString(current);
                }
                else {
                    value += getString(current);
                }

                incrementIndex();

                if (current == '\'') {
                    addToken(tokens, CharacterLiteral, value);
                    continue;
                }
                compilerError(UnexpectedToken, getErrorInfo());
                break;
            }

            // Equals sign tokens
            case '=': {
                if (i + 1 < code.length())
                {
                    // Comparison operator
                    if (code[i + 1] == '=')
                    {
                        incrementIndex();
                        addToken(tokens, EqualityOperator, "==");
                        continue;
                    }
                }
                addToken(tokens, AssignmentOperator, getString(current));
            }

            default:
                compilerError(UnexpectedToken, getErrorInfo());
                break;
        }
    }

    return tokens;
}
