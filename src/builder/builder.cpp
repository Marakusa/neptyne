//
// Created by Markus Kannisto on 09/03/2022.
//

#include "../common/common.h"
#include "builder.h"
#include "../compiler/nptc_compiler/compiler.h"
#include "../utils/file_utils.h"
#include "../compiler/nptc_compiler/models/NeptyneProject.h"

bool BuildScript(const NeptyneScript& script_file, bool run) {
	cout << "Compile " << script_file.full_path_ << endl;
	Compile(script_file, run);
	return true;
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
		project.executable_.output_assembly_path_ = project.executable_.obj_directory_path_ + project.name_ + ".asm";
		project.executable_.output_obj_path_ = project.executable_.obj_directory_path_ + project.name_ + ".obj";
		project.executable_.output_executable_path_ = project.executable_.directory_path_ + project.name_;
		
		BuildScript(project.executable_, run);
		return true;
	} else {
		NeptyneScript script_file = NeptyneScript(file);
		BuildScript(script_file, run);
		return true;
	}
}
