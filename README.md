# LittleGzip. A libdeflate-wrapper
Wrapper for libdeflate in C#. Implement GZIP archive create an append.

Exposes libdeflate compress and crc32 API functions.

The wapper is in safe managed code in one class. No need external dll except libdeflate_x86.dll and libdeflate_x64.dll (included v0.8). The wrapper work in 32, 64 bit or ANY (auto swith to the apropiate library).

The code is full comented and include simple example for using the wrapper.
## Compress file functions:
Open an existing GZIP file for append files or, if test.gz not exit, create a new GZIP file.
LittleGzip gzip = LittleGzip("test.gz");
```

Add full contents of a file into the GZIP storage. Optionally you can especify compression level and put a file comment.
```C#
gzip.AddFile("c:\\directory\\file1.txt", "file1.txt", 13, "This is the comment for file1")
```

Add full contents of one array into the GZIP storage. Optionally you can especify compression level and put a file comment.
```C#
byte[] buffer = File.ReadAllBytes("c:\\directory\\file1.txt");
gzip.AddFile(buffer, "file1.txt", "This is the comment for file1")
```

Close GZIP file. This function is automatic call when dispose LittleZip
```C#
gzip.Close()
```

## Use
LittleGzip can:
- Compress several files in a very little gzip.
- Compress in very little time.
- Use very little code.
- Very little learning for use.
- Put minimal overhead in GIP. 
- Create multimember GZIP. Store the original filename for complete extraction.

LittleZip can not:
- Create a large zip (> 2.147.483.647 bytes)
- Store a large file (> 2.147.483.647 bytes)
- Use little memory (need two times the compresed file)
- Decompress one GZIP file, erase files in GZIP, update files in GZIP, test files in GZIP.

## WARNING
Not well tested. Use with precaution.