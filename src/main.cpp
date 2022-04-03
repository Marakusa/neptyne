#include <fstream>
#include "common/common.h"
#include "builder/builder.h"
#include "logger/logger.h"
#include "compiler/nptc_compiler_errors/models/CompilerException.h"

fs::path selfPath;

fs::path getSelfPath() {
	return selfPath;
}

string listCommands() {
	string commands = "npt build <project folder|project file|script>\n"
					  "\tBuild the given project or script into an executable program\n"
					  "npt project <name> <directory>\n"
					  "\tCreate a new project into the given directory\n";
	return commands;
}

int main(int argc, char *argv[]) {
	cout << "Neptyne v0.1.3" << endl;
	
	selfPath = argv[0];
	selfPath = selfPath.remove_filename();
	
	try {
		if (argc > 1) {
			if (string(argv[1]) == "build") {
				if (argc > 2) {
					string f = string(argv[2]);
					if (fs::exists(f)) {
						if (!fs::is_directory(f)) {
							Log("Build started");
							Build(f);
						} else {
							if (fs::exists(f + "/Project.nptp")) {
								Log("Project build started");
								Build(f + "/Project.nptp");
							} else {
								cout << "Project.nptp not found in directory: " << f << endl;
								throw "Project.nptp not found in directory";
							}
						}
					} else {
						cout << "Script not found: " << f << endl;
						throw "Invalid syntax, correct: npt build <project folder|project file|script>";
					}
				} else {
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
					
					cout << "Creating a project: " << directory << "/Project.nptp" << endl;
					ofstream projectFile;
					projectFile.open(directory + "/Project.nptp");
					projectFile << "name:" << name << endl <<
					            "version:0.1.0" << endl <<
					            "description:This is a new project named " << name << endl <<
					            "executable:Main.npt" << endl;
					projectFile.close();
					
					cout << "Creating: " << directory << "/Main.npt" << endl;
					ofstream mainFile;
					mainFile.open(directory + "/Main.npt");
					mainFile << "void main: {" << endl <<
					         "\tout(\"Hello world!\");" << endl <<
					         "}" << endl;
					mainFile.close();
					cout << "Success!" << endl;
				} else {
					throw "Invalid syntax, correct: npt project <name> <directory>";
				}
			} else if (string(argv[1]) == "script") {
				if (argc > 2) {
					string name = string(argv[2]);
					
					if (fs::exists(name + ".npt")) {
						cout << "Script " << name << ".npt already exists" << endl;
						throw "Script already exists";
					}
					
					cout << "Creating: " << name << ".npt" << endl;
					ofstream f;
					f.open(name + ".npt");
					f.close();
					cout << "Success!" << endl;
				} else {
					throw "Invalid syntax, correct: npt project <name> <directory>";
				}
			} else {
				cout << "Invalid command " << string(argv[1]) << "\nCommands:\n" << listCommands() << endl;
				throw "Invalid command";
			}
		} else {
			cout << "Command not given\nCommands:\n" << listCommands() << endl;
			throw "Command not given";
		}
	}
	catch (CompilerException &e) {
		cout << e.Get() << endl;
	}
	catch (const char *e) {
		Log("Error: %s", e);
	}
	
	return 0;
}
