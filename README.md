# DUMPSYM

This is a rewrite of DUMPSYM.EXE from SN Systems Software Ltd.

## Overview

Briefly, a .SYM file contains symbols, e.g. functions, typedefs, types, etc.

But there's a catch: everything is inlined, i.e. think 'value type' over 'reference type'.

The format also recycles fake names for types across all files present in the .SYM file.

All this makes exploiting the data difficult because most of it is simply redundant stuff.

## Usage

The following commands are available:

- `dump` : like the original, prints all symbols to the terminal
- `header` : generates a C header for importing declarations in IDA
- `scripts` : generates Python scripts for naming functions and variables in IDA
- `split` : splits IDA pseudo-code output using file information present in the .SYM file

## Example

### Bullfrog's Hi-Octane

PAL version of the game has a MAIN.SYM containing 151273 symbols.

When de-duped, only 24962 symbols are truly unique, i.e. ~16%.

The game has 44 files, 900 functions, 311 types, 190 typedefs.

## Notes

Not all symbol files are created equal, your mileage may vary.

Niche stuff isn't implemented yet: classes, overlays, etc.
