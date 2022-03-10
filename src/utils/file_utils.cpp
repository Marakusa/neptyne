//
// Created by Markus Kannisto on 10/03/2022.
//

#include "file_utils.h"

auto ReadFile(string_view path) -> string {
    constexpr auto kReadSize = size_t(4096);
    auto stream = ifstream(path.data());
    stream.exceptions(ios_base::badbit);

    auto out = string();
    auto buf = string(kReadSize, '\0');
    while (stream.read(&buf[0], kReadSize)) {
        out.append(buf, 0, stream.gcount());
    }
    out.append(buf, 0, stream.gcount());
    return out;
}
