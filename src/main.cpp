#include "common/common.h"
#include "builder/builder.h"
#include "logger/logger.h"
#include "compiler/nptc_compiler_errors/models/CompilerException.h"

fs::path selfPath;

fs::path getSelfPath() {
	return selfPath;
}

int main(int argc, char *argv[]) {
	cout << "Neptyne v0.1.2" << endl;
	
	selfPath = argv[0];
	selfPath = selfPath.remove_filename();
	
	try {
		if (argc > 1) {
			string f = string(argv[1]);
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
				throw "Script not found";
			}
		} else {
			throw "Script not defined";
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
