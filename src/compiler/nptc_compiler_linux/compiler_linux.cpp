#include "../../common/common.h"
#include "../../logger/logger.h"
#include <elfio/elfio.hpp>

using namespace ELFIO;

void buildELF() {
    Log("Building ELF");
    
    // Create the ELF file
    ELFIO::elfio writer;
    writer.create(ELFCLASS64, ELFDATA2LSB);
    writer.set_os_abi(ELFOSABI_LINUX);
    writer.set_type(ET_EXEC);
    writer.set_machine(EM_X86_64);
    //writer.set_version(EV_CURRENT);
    
    // Create the program header
    ELFIO::segment* pHeader = writer.segments.add();
    pHeader->set_type(PT_LOAD);
    pHeader->set_physical_address(0);
    pHeader->set_virtual_address(0);
    pHeader->set_flags(PF_R | PF_W | PF_X);
    pHeader->set_align(0x1000);
    
    // Create the section header
    ELFIO::section* sHeader = writer.sections.add(".shstrtab");
    sHeader->set_type(SHT_STRTAB);
    sHeader->set_addr_align(1);
    
    // Create the text section
    ELFIO::section* text = writer.sections.add(".text");
    text->set_type(SHT_PROGBITS);
    text->set_addr_align(0x10);
    text->set_flags(SHF_ALLOC | SHF_EXECINSTR);
    
    // Create the data section
    ELFIO::section* data = writer.sections.add(".data");
    data->set_type(SHT_PROGBITS);
    data->set_addr_align(0x10);
    data->set_flags(SHF_ALLOC | SHF_WRITE);
    
    // Create the bss section
    ELFIO::section* bss = writer.sections.add(".bss");
    bss->set_type(SHT_NOBITS);
    bss->set_addr_align(0x10);
    bss->set_flags(SHF_ALLOC | SHF_WRITE);
    
    // Create the symbol table
    /*ELFIO::section* symtab = writer.sections.add(".symtab");
    symtab->set_type(SHT_SYMTAB);
    symtab->set_addr_align(0x8);
    symtab->set_link(sHeader);
    symtab->set_info(1);
    
    // Create the string table
    ELFIO::section* strtab = writer.sections.add(".strtab");
    strtab->set_type(SHT_STRTAB);
    strtab->set_addr_align(1);
    
    // Create the relocation table
    ELFIO::section* rela = writer.sections.add(".rela.text");*/
}
