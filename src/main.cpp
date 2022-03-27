#include "common/common.h"
#include "builder/builder.h"
#include "logger/logger.h"
#include "compiler/nptc_compiler_errors/models/CompilerException.h"

int main(int argc, char *argv[]) {
	cout << "Neptyne v0.1.1" << endl;
	
	try {
		if (argc > 1) {
			string f = string(argv[1]);
			if (fs::exists(f)) {
				Log("Build started");
				Build(f);
			} else {
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
