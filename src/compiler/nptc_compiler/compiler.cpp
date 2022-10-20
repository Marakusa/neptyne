//
// Created by Markus Kannisto on 09/03/2022.
//

#include <algorithm>
#include <utility>
#include "../../common/common.h"
#include "../../utils/file_utils.h"
#include "models/NeptyneScript.h"
#include "../nptc_lexer/lexer.h"
#include "../nptc_parser/parser.h"
#include "../nptc_asm/models/AssemblyScript.h"
#include "../nptc_compiler_errors/models/CompilerErrorInfo.h"
#include "../nptc_compiler_errors/compiler_errors.h"

class Compiler
{
private:
	int compiler_index = 0;
	AssemblyScript assemblyScript;
	NeptyneScript neptyneScript;
	vector<AssemblyScript> importedScripts;
	AssemblyFunction *currentFunction;

	int scope_memory_size_offset = 0;

	struct AssemblyVariableFinder {
	explicit AssemblyVariableFinder(string n) : name(std::move(n)) {}
	bool operator()(const AssemblyVariable &el) const { return el.name_ == "_v_" + name; }
	private:
	string name;
	};

	struct AssemblyFunctionFinder {
	explicit AssemblyFunctionFinder(string n) : name(std::move(n)) {}
	bool operator()(const AssemblyFunction &el) const { return el.name_ == "_" + name; }
	private:
	string name;
	};

	string GetParentDirectory(const string &path) {
		// If path ends with '/', remove it
		string path_ = path;
		if (path_.back() == '/') {
			path_.pop_back();
		}
		return path_.substr(0, path_.find_last_of("/\\") + 1);
	}

	bool FileExists(const string &path) {
		ifstream f(path);
		return f.good();
	}

	void CompilerStep(vector<ParserToken> tokens, ParserToken *parent) {
		if (compiler_index >= tokens.size()) {
			return;
		}
		ParserToken token = tokens[compiler_index];
		switch (token.type_) {
			case VALUE_TYPE: {
				// Set type
				string type = token.value_;
				
				token = Increment(tokens, EXPECTED_UNQUALIFIED_ID);
				
				// Set name
				if (token.type_ != NAME) {
					CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
					break;
				}
				string name_value = token.value_;
				ParserToken name_token = token;
				
				token = Increment(tokens, TERMINATOR_EXPECTED);
				// Check if is declaration or a function
				if (token.type_ != ASSIGNMENT_OPERATOR) {
					if (token.type_ == COLON) {
						token = Increment(tokens, EXPECTED_FUNCTION_BODY);
						if (token.type_ != SCOPE) {
							CompilerError(EXPECTED_FUNCTION_BODY, GetErrorInfo(token));
							break;
						} else {
							const ParserToken &scope = token;
							
							// Function declaration
							AssemblyFunction func = AssemblyFunction(type, name_value, scope);
							if (parent->type_ == ROOT) {
								assemblyScript.functions_.push_back(func);
							} else {
								CompilerError(FUNCTION_DEFINITION_NOT_ALLOWED, GetErrorInfo(token));
							}
							
							break;
						}
					} else if (token.type_ == EXPRESSION) {
						// TODO: Implement function parameters
						throw "Function parameters not implemented yet";
						/*token = Increment(tokens, SYMBOL_EXPECTED, ':');
						if (token.type_ != COLON) {
							CompilerError(EXPECTED_FUNCTION_BODY, GetErrorInfo(token));
							break;
						}
						else {
							// Function declaration
							assemblyScript.functions_.emplace_back(type, name_value);
							break;
						}*/
					}
					if (EndStatement(tokens)) {
						// Variable declaration
						AssemblyVariable var = AssemblyVariable(type, name_value);
						DefineVariable(var, token, parent, name_token, true);
						break;
					}
					break;
				}
				
				// Statement is a definition

				token = Increment(tokens, EXPECTED_EXPRESSION);
				
				// Variable definition expression
				vector<ParserToken> expression_tokens;
				GetExpressionTokens(tokens, expression_tokens);
				
				if (expression_tokens.empty() && token.type_ == STATEMENT_TERMINATOR) {
					CompilerError(EXPECTED_EXPRESSION, GetErrorInfo(token));
					break;
				}
				
				HandleBinaryExpressionTree(expression_tokens, type);
				if (EndStatement(tokens)) {
					// Variable definition
					AssemblyVariable var = AssemblyVariable(type, name_value, expression_tokens);
					DefineVariable(var, token, parent, name_token, false);
				}
				
				break;
			}
			case RETURN_STATEMENT: {
				if (currentFunction == nullptr) {
					CompilerError(RETURN_STATEMENT_NO_OUTSIDE, GetErrorInfo(token));
					break;
				}
				token = Increment(tokens, EXPECTED_UNQUALIFIED_ID);
				
				// Variable definition expression
				vector<ParserToken> expression_tokens;
				GetExpressionTokens(tokens, expression_tokens);
				if (currentFunction->return_type_ != "void" && expression_tokens.empty()
					&& token.type_ == STATEMENT_TERMINATOR) {
					CompilerError(EXPECTED_EXPRESSION, GetErrorInfo(token));
					break;
				}
				
				if (currentFunction->return_type_ == "void") {
					if (token.type_ == STATEMENT_TERMINATOR) {
						currentFunction->has_return_statement_ = true;
						currentFunction->Mov("eax", "0");
						currentFunction->Return();
						
						currentFunction->Empty();
						break;
					}
				}
				
				HandleBinaryExpressionTree(expression_tokens, currentFunction->return_type_);
				if (EndStatement(tokens)) {
					currentFunction->has_return_statement_ = true;
					currentFunction->Return();
				}
				
				currentFunction->Empty();

				break;
			}
			case KEYWORD: {
				if (token.value_ == "bring") {
					token = Increment(tokens, UNEXPECTED_TOKEN);

					ParserToken nameToken = token;
					string bringScript = token.value_;
					
					token = Increment(tokens, TERMINATOR_EXPECTED);

					if (token.type_ != STATEMENT_TERMINATOR) {
						CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
						break;
					}
					
					// Bring script
					
					// Set output path
					string d = GetParentDirectory(GetParentDirectory(neptyneScript.obj_directory_path_));
					// If file in doesn't exist, return an error
					if (!FileExists(d + bringScript)) {
						CompilerError(INVALID_SOURCE_FILE, GetErrorInfo(nameToken));
						break;
					}

					NeptyneScript script = NeptyneScript(d + bringScript);
					script.directory_path_ = neptyneScript.directory_path_;
					script.obj_directory_path_ = neptyneScript.obj_directory_path_;
					script.output_assembly_path_ = script.obj_directory_path_ + script.name_ + ".asm";
					script.output_obj_path_ = script.obj_directory_path_ + script.name_ + ".o";
					script.output_executable_path_ = script.directory_path_ + script.name_;
					
					Compiler compiler = Compiler();
					compiler.Compile(script, false, false);
					importedScripts.push_back(compiler.assemblyScript);
				}
				else {
					CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
				}
				break;
			}
			case NAME: {
				if (token.value_ == "out") {
					if (currentFunction == nullptr) {
						CompilerError(INVALID_SCOPE_FOR_CALL, GetErrorInfo(token));
						break;
					}
					
					token = Increment(tokens, UNEXPECTED_TOKEN);
					
					if (token.type_ != EXPRESSION) {
						CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
						break;
					}
					
					// Variable definition expression
					HandleBinaryExpressionTree(token.parameters_, "any");
					
					// Send to stdout
					currentFunction->Mov("ecx", "eax");
					currentFunction->Mov("eax", "4");
					currentFunction->Mov("ebx", "1");
					currentFunction->Int("80h");
					
					currentFunction->Empty();

					// Add newline
					currentFunction->Mov("eax", "newline");
					currentFunction->Mov("edx", "1");
					currentFunction->Mov("ecx", "eax");
					currentFunction->Mov("eax", "4");
					currentFunction->Mov("ebx", "1");
					currentFunction->Int("80h");

					currentFunction->Empty();

					token = Increment(tokens, TERMINATOR_EXPECTED);
					
					if (token.type_ != STATEMENT_TERMINATOR) {
						CompilerError(TERMINATOR_EXPECTED, GetErrorInfo(token));
						break;
					}
				}
				else {
					bool found = false;
					for (AssemblyFunction f : assemblyScript.functions_) {
						if (f.name_ == "_" + token.value_) {
							found = true;

							currentFunction->Call(f);

							token = Increment(tokens, UNEXPECTED_TOKEN);
							
							if (token.type_ != EXPRESSION) {
								CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
								break;
							}
							
							token = Increment(tokens, TERMINATOR_EXPECTED);
							
							if (token.type_ != STATEMENT_TERMINATOR) {
								CompilerError(TERMINATOR_EXPECTED, GetErrorInfo(token));
								break;
							}
							break;
						}
					}
					if (!found) {
						for (AssemblyScript s : importedScripts) {
							for (AssemblyFunction f : s.functions_) {
								if (f.name_ == "_" + token.value_) {
									found = true;

									currentFunction->Call(f);

									token = Increment(tokens, UNEXPECTED_TOKEN);
									
									if (token.type_ != EXPRESSION) {
										CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
										break;
									}
									
									token = Increment(tokens, TERMINATOR_EXPECTED);
									
									if (token.type_ != STATEMENT_TERMINATOR) {
										CompilerError(TERMINATOR_EXPECTED, GetErrorInfo(token));
										break;
									}

									// If the function already exists in extern values, don't push it
									bool exists = false;
									for (string v : assemblyScript.externValues_) {
										if (v == f.name_) {
											exists = true;
											break;
										}
									}
									if (!exists)
										assemblyScript.externValues_.push_back(f.name_);
									break;
								}
							}
						}
					}
					if (!found) {
						CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
					}
				}
				break;
			}
			default: {
				CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
				break;
			}
		}
	}

	void GetExpressionTokens(vector<ParserToken> tokens, vector<ParserToken> &expression_tokens) {
		ParserToken token = tokens[compiler_index];
		expression_tokens = vector<ParserToken>();
		while (compiler_index < tokens.size() && token.type_ != STATEMENT_TERMINATOR) {
			expression_tokens.push_back(token);
			token = Increment(tokens, TERMINATOR_EXPECTED);
			if (token.type_ != COMMA && token.type_ != STATEMENT_TERMINATOR) {
				CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(token));
				break;
			}
			if (token.type_ != STATEMENT_TERMINATOR)
				token = Increment(tokens, TERMINATOR_EXPECTED);
		}
	}

	string GetDeclarationType(ParserToken &token, bool throw_error_on_null) {
		if (currentFunction != nullptr) {
			// Check local variables
			auto v = find_if(currentFunction->variables_.begin(), currentFunction->variables_.end(),
							AssemblyVariableFinder(token.value_));
			if (v != currentFunction->variables_.end()) {
				return v->type_;
			}
		}
		
		// Check global variables
		auto v = find_if(assemblyScript.variables_.begin(), assemblyScript.variables_.end(),
						AssemblyVariableFinder(token.value_));
		if (v != assemblyScript.variables_.end()) {
			return v->type_;
		}
		
		// Check global functions
		auto f = find_if(assemblyScript.functions_.begin(), assemblyScript.functions_.end(),
						AssemblyFunctionFinder(token.value_));
		if (f != assemblyScript.functions_.end()) {
			return f->return_type_;
		}
		
		if (throw_error_on_null)
			CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo(token));
		return "";
	}

	string GetAssemblyNameOfVariable(ParserToken &token) {
		if (currentFunction != nullptr) {
			// Check local variables
			auto v = find_if(currentFunction->variables_.begin(), currentFunction->variables_.end(),
							AssemblyVariableFinder(token.value_));
			if (v != currentFunction->variables_.end()) {
				return v->asm_ref_;
			}
		}
		
		// Check global variables
		auto v = find_if(assemblyScript.variables_.begin(), assemblyScript.variables_.end(),
						AssemblyVariableFinder(token.value_));
		if (v != assemblyScript.variables_.end()) {
			return v->name_;
		}
		
		// Check global functions
		auto f = find_if(assemblyScript.functions_.begin(), assemblyScript.functions_.end(),
						AssemblyFunctionFinder(token.value_));
		if (f != assemblyScript.functions_.end()) {
			// TODO: Functions on binary expression trees
			return f->name_;
		}
		
		CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo(token));
		return "";
	}

	int GetTypeSize(string type) {
		if (type == "bool" || type == "byte") {
			return 1;
		} else if (type == "short" || type == "ushort") {
			return 2;
		} else if (type == "int" || type == "uint" || type == "float" || type == "char") {
			return 4;
		} else if (type == "long" || type == "ulong" || type == "double") {
			return 8;
		} else {
			throw "Type size not set for " + type;
		}
	}

	void DefineVariable(AssemblyVariable &variable, ParserToken &token, ParserToken *parent,
						ParserToken &name_token, bool declaration) {
		if (GetDeclarationType(name_token, false).empty()) {
			if (parent->type_ == ROOT) {
				assemblyScript.variables_.push_back(variable);
				// TODO: Define outside function
			} else if (parent->type_ == SCOPE) {
				currentFunction->variables_.push_back(variable);
				scope_memory_size_offset += GetTypeSize(variable.type_);
				string to = "DWORD [rbp-" + to_string(scope_memory_size_offset) + "]";
				currentFunction->DefineVariable(to, currentFunction->variables_[currentFunction->variables_.size() - 1]);
			} else {
				if (declaration) {
					CompilerError(VARIABLE_DECLARATION_NOT_ALLOWED, GetErrorInfo(name_token));
				} else {
					CompilerError(VARIABLE_DEFINITION_NOT_ALLOWED, GetErrorInfo(name_token));
				}
			}
		} else {
			CompilerError(VARIABLE_EXISTS, GetErrorInfo(name_token));
		}
	}

	bool EqualType(ParserToken &expression_token, const string &type) {
		string t;
		switch (expression_token.type_) {
			case NAME: {
				t = GetDeclarationType(expression_token, true);
				break;
			}
			case STRING_LITERAL: {
				t = "string";
				break;
			}
			case INTEGER_LITERAL: {
				t = "int";
				break;
			}
			case CHARACTER_LITERAL: {
				t = "char";
				break;
			}
			case BOOLEAN_LITERAL: {
				t = "bool";
				break;
			}
			case FLOAT_LITERAL: {
				t = "float";
				break;
			}
			case DOUBLE_LITERAL: {
				t = "double";
				break;
			}
			default:break;
		}
		if (t.empty()) return false;
		
		if (type == "bool" && t == "bool") {
			return true;
		} else if (type == "byte" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= 0 && num_val <= 255)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "int" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= -2147483648 && num_val <= 2147483647)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "uint" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= 0 && num_val <= 4294967295)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "short" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= -32768 && num_val <= 32767)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "ushort" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= 0 && num_val <= 65535)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "long" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= -2147483648 && num_val <= 2147483647)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "ulong" && t == "int") {
			if (expression_token.type_ == NAME) {
				return true;
			}
			
			long long num_val = stoll(expression_token.value_);
			if (num_val >= 0 && num_val <= 4294967295)
				return true;
			
			CompilerError(OUTSIDE_THE_RANGE, GetErrorInfo(expression_token));
			return false;
		} else if (type == "string" && t == "string") {
			return true;
		}
		
		/*TODO: These types below
		char
		double
		float
		void*/
		
		CompilerError(CANNOT_RESOLVE_SYMBOL, GetErrorInfo(expression_token));
		std::cout << "===================" << std::endl;
		std::cout << "Type " << t << " or " << type << " not supported yet" << std::endl;
		std::cout << "===================" << std::endl;
		return false;
	}

	bool EndStatement(vector<ParserToken> &tokens, bool scope = false) {
		ParserToken token = tokens[compiler_index];
		
		if (token.type_ == STATEMENT_TERMINATOR)
			return true;
		
		if (compiler_index - 1 >= 0)
			token = tokens[compiler_index - 1];
		
		if (!scope) {
			Increment(tokens, TERMINATOR_EXPECTED);
			if (token.type_ != STATEMENT_TERMINATOR) {
				CompilerError(TERMINATOR_EXPECTED, GetErrorInfo(token));
				return false;
			}
		}
		return true;
	}

	ParserToken Increment(vector<ParserToken> &tokens, CompilerErrorType fail, char  v = 0) {
		ParserToken token = tokens[compiler_index];
		compiler_index++;
		if (!CheckIncrement(tokens)) {
			CompilerErrorInfo info = GetErrorInfo(token);
			if (v != 0)
				info.value_ = v;
			CompilerError(fail, info);
		}
		return tokens[compiler_index];
	}

	bool CheckIncrement(vector<ParserToken> &tokens) {
		return compiler_index < tokens.size();
	}

	// Gets the character info of the current position of the tokenizer
	CompilerErrorInfo GetErrorInfo(ParserToken &token) {
		return {neptyneScript, token.line_, token.column_, token.value_};
	}

	void HandleBinaryExpressionTree(vector<ParserToken> &expression_tokens, const string &expression_type) {
		if (expression_tokens.size() == 1) {
			if (expression_tokens[0].type_ == NAME) {
				if (expression_type == "any" || expression_type == GetDeclarationType(expression_tokens[0], false)) {
					currentFunction->Mov("eax", GetAssemblyNameOfVariable(expression_tokens[0]));
					return;
				} else {
					CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(expression_tokens[0]));
					return;
				}
			} else if (expression_type == "any" || EqualType(expression_tokens[0], expression_type)) {
				if (expression_tokens[0].type_ == STRING_LITERAL) {
					assemblyScript.string_literals_.push_back(expression_tokens[0].value_);
					
					// Reference to the string literal
					string name = "s_" + to_string(currentConstantVariableIndex);
					assemblyScript.string_literal_names_.push_back(name);
					currentFunction->Mov("eax", name);
					currentFunction->Mov("edx", name + "_len");

					currentConstantVariableIndex++;

					return;
				}
				currentFunction->Mov("eax", expression_tokens[0].value_);
				return;
			}
			CompilerError(UNEXPECTED_TOKEN, GetErrorInfo(expression_tokens[0]));
			return;
		} else {
			// TODO: Multiple elements
			throw "Not implemented";
		}
	}

public:
	Compiler(/* args */) {

	}

	void Compile(const NeptyneScript &script, bool run, bool mainScript = true) {
		cout << "    Compile:\t(" << script.name_ << ") " << script.full_path_ << endl;

		CompilerErrorsReset();
		assemblyScript = AssemblyScript(mainScript);
		
		// Read parser_script file
		string code = ReadFile(script.full_path_.string());
		neptyneScript = script;
		neptyneScript.code_ = code;
		
		// Split parser_script to lines
		vector<string> lines;
		Split(lines, code, "\n");
		neptyneScript.code_lines_ = lines;
		
		// Tokenize the parser_script
		vector<Token> tokens = Tokenize(neptyneScript);
		
		// Parse parser_input_tokens into a syntax tree
		ParserToken abstract_syntax_tree = ParserToken(ROOT, neptyneScript.name_, 1, 1, neptyneScript);
		ParseToSyntaxTree(abstract_syntax_tree, tokens, neptyneScript);
		
		// Compile code into functions and statements
		compiler_index = 0;
		scope_memory_size_offset = 0;
		while (compiler_index < abstract_syntax_tree.parameters_.size()) {
			CompilerStep(abstract_syntax_tree.parameters_, &abstract_syntax_tree);
			compiler_index++;
		}
		
		for (auto &function : assemblyScript.functions_) {
			if (function.name_ != "_start") {
				scope_memory_size_offset = 0;
				compiler_index = 0;
				currentFunction = &function;
				while (compiler_index < function.scope_tokens_.size()) {
					CompilerStep(function.scope_tokens_, &function.scope_);
					compiler_index++;
				}
			}
		}
		
		if (CompilerHasErrors())
			throw "Compiling failed with errors...";
		
		string result;
		assemblyScript.Form(result);
		//cout << "====================" << endl << result << endl << "====================" << endl;
		
		// Write asm file
		ofstream asm_file;
		asm_file.open(script.output_assembly_path_);
		asm_file << result;
		asm_file.close();
		
		// Write binaries
	#ifdef _WIN32
		string selfPath = string(getSelfPath().u8string());
		ReplaceAll(selfPath, " ", "\\ ");
		string e = "" + selfPath + "vendor\\nasm\\compile_windows.bat \"" + script.output_assembly_path_ + "\" \"" + script
				.output_obj_path_ + "\" \""
				+ script.output_executable_path_ + ".exe\" \"" + selfPath + "\"";
		system(e.c_str());
		cout << "    Compiled:\t(" << script.name_ << ") " << script.full_path_ << endl;
		//cout << endl << "Build done!" << endl << "===============================" << endl << endl;
		/*// Start the executable
		if (run)
			system(("\"" + script.output_executable_path_ + ".exe\"").c_str());*/
	#endif
	#ifdef TARGET_OS_MAC
		//system("");
		cout << "Compiling in macOS is not supported yet" << endl;
	#endif
	#ifdef __linux__
		string e = "\"" + string(getSelfPath()) + "vendor/nasm/compile_linux.sh\" \"" + script.output_assembly_path_ + "\" \"" + script
			.output_obj_path_ + "\" \""
			+ script.output_executable_path_ + "\" \"" + string(getSelfPath()) + "\"";
		system(e.c_str());
		cout << "    Compiled:\t(" << script.name_ << ") " << script.full_path_ << endl;
		//cout << endl << "Build done!" << endl << "===============================" << endl << endl;
		/*// Start the executable
		if (run)
			system(("\"" + script.output_executable_path_ + "\"").c_str());*/
	#endif
	}
};
