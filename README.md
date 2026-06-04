# DUMPSYM

This is a rewrite of DUMPSYM.EXE from SN Systems Software Ltd.

As a bonus, you can generate a C header and Python scripts to enrich IDA output.

## Quick overview of the .SYM file format

Briefly, a .SYM file contains symbols, e.g. functions, typedefs, types, etc.

But there's a catch: everything is inlined, i.e. think 'value type' over 'reference type'.

The format also recycles fake names for types across all files present in the .SYM file.

All this makes exploiting its data difficult because it's pretty bulky and 'mostly similar'.

### Example: Bullfrog's Hi-Octane

When reading the .SYM file, it contains 151273 symbols, literally.

Once de-duped, you get 24962 symbols, i.e. only ~16% are truly unique.

## How to use it?

The command-line application has four commands:

- `dump` : like the original, prints all the symbols to the terminal
- `header` : generates a C header to import declarations in IDA
- `scripts` : generates Python scripts to name functions and variables in IDA
- `split` : splits C source output from IDA using file info from the .SYM file
