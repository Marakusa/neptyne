//
// Created by Markus Kannisto on 09/03/2022.
//

#include "../common/common.h"
#include "builder.h"
#include "../compiler/nptc_compiler/compiler.cpp"
#include "../utils/file_utils.h"
#include "../compiler/nptc_compiler/models/NeptyneProject.h"

bool BuildScript(const NeptyneScript& script_file, bool run) {
	Compiler compiler = Compiler();
	compiler.Compile(script_file, run);
	return true;
}

string ListParseObjectFiles(string path) {
	string result = "";
    for (const auto& entry : fs::directory_iterator(path))
	{
		string filePath = entry.path();
		if (filePath.substr(filePath.length() - 2, 2) == ".o") {
			result += "\"";
			result += filePath;
			result += "\" ";
		}
	}
	return result;
}

bool Build(const string &file, bool run) {
	if(file.substr(file.find_last_of('.') + 1) == "nptp") {
		cout << "Project " << file << endl;
		
		// Split parser_script to lines
		string project_file_content = ReadFile(file);
		vector<string> lines;
		Split(lines, project_file_content, "\n");
		
		// Create a project class object
		fs::path full_path = fs::canonical(file);
		string directory_path = full_path.string().substr(0, full_path.string().find_last_of("/\\") + 1);
		NeptyneProject project = NeptyneProject();
		for (auto & line : lines) {
			if (line.substr(0, 5) == "name:") {
				project.name_ = line.substr(5, line.size() - 5);
			} else if (line.substr(0, 8) == "version:") {
				project.version_ = line.substr(8, line.size() - 8);
			} else if (line.substr(0, 12) == "description:") {
				project.description_ = line.substr(12, line.size() - 12);
			} else if (line.substr(0, 11) == "executable:") {
				project.executable_ = NeptyneScript(directory_path + line.substr(11, line.size() - 11));
			}
		}
		
		// Set output path
		project.executable_.directory_path_ = directory_path + "bin/release/";
		project.executable_.obj_directory_path_ = directory_path + "obj/release/";
		project.executable_.output_assembly_path_ = project.executable_.obj_directory_path_ + project.executable_.name_ + ".asm";
		project.executable_.output_obj_path_ = project.executable_.obj_directory_path_ + project.executable_.name_ + ".o";
		project.executable_.output_executable_path_ = project.executable_.directory_path_ + project.name_;
		
		fs::remove_all(project.executable_.directory_path_);
		fs::remove_all(project.executable_.obj_directory_path_);
		fs::create_directories(project.executable_.directory_path_);
		fs::create_directories(project.executable_.obj_directory_path_);
		
		currentConstantVariableIndex = 0;
		BuildScript(project.executable_, run);

	#ifdef _WIN32
		cout << endl << "Build done!" << endl << "===============================" << endl << endl;
		// Start the executable
		if (run)
			system(("\"" + project.executable_.output_executable_path_ + ".exe\"").c_str());
	#endif
	#ifdef TARGET_OS_MAC
		//system("");
		cout << "Compiling in macOS is not supported yet" << endl;
	#endif
	#ifdef __linux__
		string e = "\"" + string(getSelfPath()) + "vendor/nasm/linker_linux.sh\" \"" + project.executable_.output_executable_path_ + "\" " + ListParseObjectFiles(project.executable_.obj_directory_path_);
		system(e.c_str());
		cout << endl << "Build done!" << endl << "===============================" << endl << endl;
		// Start the executable
		if (run)
			system(("\"" + project.executable_.output_executable_path_ + "\"").c_str());
	#endif
		
		return true;
	} else {
		NeptyneScript script_file = NeptyneScript(file);
		BuildScript(script_file, run);
		return true;
	}
}
