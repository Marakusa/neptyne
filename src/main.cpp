#include <fstream>
#include "common/common.h"
#include "builder/builder.h"
#include "logger/logger.h"
#include "compiler/nptc_compiler_errors/models/CompilerException.h"
#include "compiler/nptc_compiler_linux/compiler_linux.cpp"
#include <regex>

fs::path selfPath;

fs::path getSelfPath() {
	return selfPath;
}

string listCommands() {
	string commands = "\tnpt help\n"
					  "\t\tPrints a list of the commands\n"
					  "\tnpt build [project folder|project file|script]\n"
					  "\t\tBuild the given project or script into an executable program\n"
					  "\tnpt project <name> [directory]\n"
					  "\t\tCreate a new project into the given directory\n"
					  "\tnpt script <path>\n"
					  "\t\tCreate a new Neptyne script\n";
	return commands;
}

string ParseToFunctionName(string s) {
	// Precompile regex
	regex numberRegex("([0-9]+)");
	regex regexCapital("([A-Z]+)");
	regex regex("([a-zA-Z0-9]+)");
	// Parse to pascal casing
	string result = "";
	bool nextIsUpper = true;
	for (int i = 0; i < s.length(); i++) {
		if (s[i] == ' ' || s[i] == '_' || s[i] == '-') {
			nextIsUpper = true;
		} else {
			if (result == "" && regex_match(string(1, s[i]), numberRegex)) {
				continue;
			}
			
			if (!regex_match(string(1, s[i]), regex)) {
				nextIsUpper = true;
			} else if (nextIsUpper) {
				result += toupper(s[i]);
				nextIsUpper = false;
			} else {
				result += s[i];
			}
		}
	}
	return result;
}

int main(int argc, char *argv[]) {
	selfPath = argv[0];
	selfPath = selfPath.remove_filename();
	
	try {
		if (argc > 1) {
			if (string(argv[1]) == "build" || string(argv[1]) == "run") {
				cout << "Neptyne v0.1.4" << endl;
				
				string f;
				if (argc <= 2) {
					f = ".";
				}
				else {
					f = string(argv[2]);
				}
				if (fs::exists(f)) {
					if (!fs::is_directory(f)) {
						LogInfo("Build started");
						Build(f, string(argv[1]) == "run");
					} else {
						if (fs::exists(f + "/Project.nptp")) {
							LogInfo("Project build started");
							Build(f + "/Project.nptp", string(argv[1]) == "run");
						} else {
							cout << "Project.nptp not found in directory: " << f << endl;
							throw "Project.nptp not found in directory";
						}
					}
				} else {
					cout << "Script not found: " << f << endl;
					throw "Invalid syntax, correct: npt build <project folder|project file|script>";
				}
			} else if (string(argv[1]) == "project") {
				if (argc > 2) {
					string name = string(argv[2]);
					string directory = name;
					if (argc > 3) {
						directory = string(argv[3]);
					}
					
					if (fs::exists(directory)) {
						if (!fs::is_directory(directory)) {
							cout << "The given path exists but is not a directory" << endl;
							throw "The given path exists but is not a directory";
						}
						
						if (fs::exists(directory + "/Project.nptp")) {
							cout << "Project.nptp already exists in the given directory" << endl;
							throw "Project.nptp already exists in the given directory";
						}
					} else {
						fs::create_directory(directory);
					}
					
					ofstream projectFile;
					projectFile.open(directory + "/Project.nptp");
					projectFile << "name:" << name << endl <<
					            "version:0.1.0" << endl <<
					            "description:This is a new project named " << name << endl <<
					            "executable:Main.npt" << endl;
					projectFile.close();
					
					ofstream mainFile;
					mainFile.open(directory + "/Main.npt");
					mainFile << "void Main: {" << endl <<
					         "\tout(\"Hello world!\");" << endl <<
					         "}" << endl;
					mainFile.close();
					
					ofstream gitIgnoreFile;
					gitIgnoreFile.open(directory + "/.gitignore");
					gitIgnoreFile << "bin/" << endl << "obj/" << endl;
					gitIgnoreFile.close();
				} else {
					throw "Invalid syntax, correct: npt project <name> [directory]";
				}
			} else if (string(argv[1]) == "script") {
				if (argc > 2) {
					string path = string(argv[2]);

					if (path.find(".") == std::string::npos || path.substr(path.find_last_of(".")) != ".npt") {
						path += ".npt";
					}
					
					if (fs::exists(path)) {
						cout << "Script " << path << " already exists" << endl;
						throw "Script already exists";
					}
					
					string name = path;

					if (path.find("/") != std::string::npos)
						name = path.substr(path.find_last_of("/\\"));
					
					name = name.substr(0, name.find_last_of("."));

					ofstream f;
					f.open(path);
					f << "void " << ParseToFunctionName(name) << ": {" << endl <<
					         "\t" << endl <<
					         "}" << endl;
					f.close();
				} else {
					throw "Invalid syntax, correct: npt script <path>";
				}
			} else if (string(argv[1]) == "dev") {
				buildELF();
			} else if (string(argv[1]) == "help") {
				cout << "Neptyne v0.1.4" << endl;
				cout << "Commands:\n" << listCommands() << endl;
			} else {
				cout << "Neptyne v0.1.4" << endl;
				cout << "Invalid command " << string(argv[1]) << ", type \"npt help\" for a list of the commands" << endl;
				throw "Invalid command";
			}
		} else {
			cout << "Neptyne v0.1.4" << endl;
			cout << "Command not given, type \"npt help\" for a list of the commands" << endl;
			throw "Command not given";
		}
	}
	catch (CompilerException &e) {
		cout << e.Get() << endl;
	}
	catch (const char *e) {
		LogError("%s", e);
	}
	
	return 0;
}
