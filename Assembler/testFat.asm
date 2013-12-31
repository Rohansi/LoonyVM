include 'loonyvm.inc'

struct FATBPB
    jump                rb 3    ; Jump code
    oem                 rb 8    ; OEM Name
    bytesPerSector      dw ?    ; Bytes Per Sector
    sectorsPerCluster   db ?    ; Sectors per cluster
    reservedSectors     dw ?    ; Reserved sectors
    numberOfFATs        db ?    ; Number of FATs
    directoryEntries    dw ?    ; Number of root directory entries
    totalSectors        dw ?    ; Should be 0
    media               db ?    ; Should be 0xF8
    sectorsPerFAT       dw ?    ; Should be 0
    sectorsPerTrack     dw ?    ; Sectors per track
    heads               dw ?    ; Number of heads
    hiddenSectors       dd ?    ; Number of hidden sectors
    totalSectorsLarge   dd ?    ; Set if more than 65535 sectors

    ; Extended BPB (FAT32 specific)
    sectorsPerFATLarge  dd ?    ; Number of sectors per FAT
    flags               dw ?    ; Flags
    version             dw ?    ; FAT32 Version
    clusterRoot         dd ?    ; Offset of root directory
    clusterFSInfo       dw ?    ; Offset of FS Info
    clusterBackupBS     dw ?    ; Offset to backup bootsector
    reserved            rd 12   ; Should be 0
    driverNumber        db ?    ; Should be 0x80
    reserved2           db ?    ; Reserved for WinNT
    signature           db ?    ; Should be 0x28 or 0x29
    serial              dd ?    ; Serial ID
    label               rd 11   ; Should be padded with spaces
    fsys                rd 8    ; Filesystem

    ; Non-BPB specific
    code                rd 420  ; Boot code
    bootSig             dw ?    ; Boot signature, must be 0xAA55
ends

struct FATDIRECTORYENTRY
    name                rb 11   ; 8.3 format
    attrib              db ?    ; Attributes
    reserved            db ?    ; Reserved for WinNT
    creationTimeLo      db ?    ; Creation time
    creationTimeHi      dw ?    ; Creation time
    creationDate        dw ?    ; Creation date
    accessDate          dw ?    ; Access date
    firstClusterHi      dw ?    ; First cluster
    lastModTime         dw ?    ; Last modification time
    lastModDate         dw ?    ; Last modification date
    firstClusterLo      dw ?    ; First cluster
    size                dd ?    ; File size
ends

DOS83_MAX_LEN           = 11
DIRECTORYBUFF_SIZE      = 4 * 1024
FAT_BAD                 = 0x0FFFFFF7
FAT_EOF                 = 0x0FFFFFF8
FAT_FREE                = 0x00000000

entryPoint:
    invoke FatInitialize
    invoke_va printf, msgFatInitialized

    invoke_va printf, msgSearching, folder
    invoke FatFindEntry, folder
    cmp r0, r0
    jnz .foundFolder

    invoke printString, msgFileNotFound
    jmp $

.foundFolder:
    invoke_va printf, msgFoundFile
    invoke FatReadDirectory, r0

    invoke_va printf, msgSearching, fileName
    invoke FatFindEntry, fileName
    cmp r0, r0
    jnz .foundFile

    invoke printString, msgFileNotFound
    jmp $

.foundFile:
    invoke_va printf, msgFoundFile
    invoke FatLoad, r0, 0x50000

    invoke_va printf, msgDumping
    invoke printString, 0x50000
    jmp $

folder:             db 'JUNK       ', 0
fileName:           db 'FRESH   TXT', 0

msgFatInitialized:  db 'FAT32 initialized', 10, 0
msgSearching:       db 'Searching for "%s" ... ', 0
msgFoundFile:       db 'Found!', 10, 0
msgDumping:         db 'Dumping contents: ', 10, 10, 0
msgFileNotFound:    db 'Not found!', 0

FatInitialize:
    push bp
    mov bp, sp
    push r0
    push r1

    ; read bpp
    invoke DiskReadSector, 0, bpb

    ; setup stuff
    mov r0, byte [bpb.sectorsPerCluster]
    mul r0, word [bpb.bytesPerSector]
    mov [bytesPerCluster], r0

    mov r0, byte [bpb.sectorsPerCluster]
    mul r0, word [bpb.bytesPerSector]
    div r0, sizeof.FATDIRECTORYENTRY
    mov [entriesPerCluster], r0

    mov r0, word [bpb.reservedSectors]
    mov r1, byte [bpb.numberOfFATs]
    mul r1, dword [bpb.sectorsPerFATLarge]
    add r0, r1
    sub r0, 2
    mov [clusterOffset], r0

    mov r0, DIRECTORYBUFF_SIZE
    div r0, [bytesPerCluster]
    mov [maxDirectoryClusters], r0

    mov r0, word [bpb.bytesPerSector]
    div r0, 4
    mov [fatPerSector], r0

    mov [cachedFatSector], -1

    ; load root dir
    invoke FatReadDirectoryImpl, [bpb.clusterRoot]

.return:
    pop r1
    pop r0
    pop bp
    ret

; void FatLoad(FATDIRECTORYENTRY* entry, byte* dest)
FatLoad:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3

    mov r1, [bp + 8]
    mov r2, [bp + 12]
    mov r3, word [r1 + FATDIRECTORYENTRY.firstClusterHi]
    shl r3, 16
    or  r3, word [r1 + FATDIRECTORYENTRY.firstClusterLo]

.read:
    invoke FatReadCluster, r3, r2
    add r2, [bytesPerCluster]

    invoke FatReadFat, r3
    mov r3, r0

    cmp r3, FAT_EOF
    jb .read

.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 8

; FATDIRECTORYENTRY* FatFindEntry(byte* name)
FatFindEntry:
    push bp
    mov bp, sp
    push r1
    push r2

    mov r1, directoryBuff
    mov r2, [entriesPerCluster]

.search:
    cmp r2, r2
    jz .notFound
    
    invoke strncmp, [bp + 8], r1, DOS83_MAX_LEN
    cmp r0, r0
    jz .found

    add r1, sizeof.FATDIRECTORYENTRY
    dec r2
    jmp .search

.notFound:
    xor r0, r0
    jmp .return

.found:
    mov r0, r1

.return:
    pop r2
    pop r1
    pop bp
    retn 4

; void FatReadDirectory(FATDIRECTORYENTRY* entry)
FatReadDirectory:
    push bp
    mov bp, sp
    push r1
    push r2

    mov r1, [bp + 8]
    mov r2, word [r1 + FATDIRECTORYENTRY.firstClusterHi]
    shl r2, 16
    or  r2, word [r1 + FATDIRECTORYENTRY.firstClusterLo]
    invoke FatReadDirectoryImpl, r2

.return:
    pop r2
    pop r1
    pop bp
    retn 4

; void FatReadDirectory(int startCluster)
FatReadDirectoryImpl:
    push bp
    mov bp, sp
    push r1
    push r2
    push r3

    mov r1, [maxDirectoryClusters]
    mov r2, directoryBuff
    mov r3, [bp + 8]

.read:
    cmp r1, r1
    jz .return
    dec r1

    invoke FatReadCluster, r3, r2
    add r2, [bytesPerCluster]

    invoke FatReadFat, r3
    mov r3, r0

    cmp r3, FAT_EOF
    jb .read

.return:
    pop r3
    pop r2
    pop r1
    pop bp
    retn 4

; int FatReadFat(int index)
FatReadFat:
    push bp
    mov bp, sp
    push r1

    mov r1, [bp + 8]
    mul r1, 4
    div r1, word [bpb.bytesPerSector]
    add r1, word [bpb.reservedSectors]

    cmp r1, [cachedFatSector]
    je .done
    invoke DiskReadSector, r1, cachedFat
    mov [cachedFatSector], r1

.done:
    mov r0, [bp + 8]
    rem r0, [fatPerSector]
    mul r0, 4
    add r0, cachedFat
    mov r0, [r0]

.return:
    pop r1
    pop bp
    retn 4

; void FatReadCluster(int cluster, void* dest)
FatReadCluster:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2

    mov r0, [bp + 8]  ; cluster
    mul r0, byte [bpb.sectorsPerCluster]
    add r0, [clusterOffset]

    mov r1, [bp + 12] ; dest
    mov r2, byte [bpb.sectorsPerCluster]

.read:
    cmp r2, r2
    jz .return

    invoke DiskReadSector, r0, r1
    inc r0
    add r1, word [bpb.bytesPerSector]

    dec r2
    jmp .read

.return:
    pop r2
    pop r1
    pop r0
    pop bp
    retn 8

; void DiskReadSector(int sector, void* dest)
DiskReadSector:
    push bp
    mov bp, sp
    push r0
    push r1
    push r2

    mov r0, 1
    mov r1, [bp + 12]
    mov r2, [bp + 8]
    int 8

.return:
    pop r2
    pop r1
    pop r0
    pop bp
    retn 8

bpb                     FATBPB
bytesPerCluster:        rd 1
entriesPerCluster:      rd 1
clusterOffset:          rd 1
fatPerSector:           rd 1
maxDirectoryClusters:   rd 1
cachedFatSector:        rd 1
cachedFat:              rd 1024
directoryBuff:          rd DIRECTORYBUFF_SIZE

include 'lib/string.asm'
include 'lib/term.asm'
include 'lib/printf.asm'
