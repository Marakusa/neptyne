#include "../../common/common.h"
#include "../../logger/logger.h"
#include "../../../vendor/elfio/elfio.hpp"

using namespace ELFIO;

void writeHeaders(elfio &writer) {
    LogInfo("Writing headers");
    writer.create( ELFCLASS32, ELFDATA2LSB );
    writer.set_os_abi( ELFOSABI_LINUX );
    writer.set_type( ET_EXEC );
    writer.set_machine( EM_386 );
}

void buildELF() {
    LogInfo("Building ELF");
    
    elfio writer;

    writeHeaders(writer);

    section* text_sec = writer.sections.add( ".text" );
    text_sec->set_type( SHT_PROGBITS );
    text_sec->set_flags( SHF_ALLOC | SHF_EXECINSTR );
    text_sec->set_addr_align( 0x10 );
    char text[] = { '\xB8', '\x04', '\x00', '\x00', '\x00', // mov eax, 4
    '\xBB', '\x01', '\x00', '\x00', '\x00', // mov ebx, 1
    '\xB9', '\x20', '\x80', '\x04', '\x08', // mov ecx, msg
    '\xBA', '\x0E', '\x00', '\x00', '\x00', // mov edx, 14
    '\xCD', '\x80', // int 0x80
    '\xB8', '\x01', '\x00', '\x00', '\x00', // mov eax, 1
    '\xCD', '\x80' }; // int 0x80
    text_sec->set_data( text, sizeof( text ) );

    segment* text_seg = writer.segments.add();
    text_seg->set_type( PT_LOAD );
    text_seg->set_virtual_address( 0x08048000 );
    text_seg->set_physical_address( 0x08048000 );
    text_seg->set_flags( PF_X | PF_R );
    text_seg->set_align( 0x1000 );
    text_seg->add_section_index( text_sec->get_index(),
    text_sec->get_addr_align() );

    section* data_sec = writer.sections.add( ".data" );
    data_sec->set_type( SHT_PROGBITS );
    data_sec->set_flags( SHF_ALLOC | SHF_WRITE );
    data_sec->set_addr_align( 0x4 );
    char data[] = { '\x48', '\x65', '\x6C', '\x6C', '\x6F', // “Hello, World!\n”
    '\x2C', '\x20', '\x57', '\x6F', '\x72',
    '\x6C', '\x64', '\x21', '\x0A' };
    data_sec->set_data( data, sizeof( data ) );

    segment* data_seg = writer.segments.add();
    data_seg->set_type( PT_LOAD );
    data_seg->set_virtual_address( 0x08048020 );
    data_seg->set_physical_address( 0x08048020 );
    data_seg->set_flags( PF_W | PF_R );
    data_seg->set_align( 0x10 );

    data_seg->add_section_index(data_sec->get_index(), data_sec->get_addr_align());

    writer.set_entry( 0x08048000 );

    // Write the ELF file
    writer.save("a.out");
    
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
