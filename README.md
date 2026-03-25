# File Transfer Tool

This is a console file transfer application that copies large files in chunks with verification

## Features

- Chunk-based file transfer
- MD5 hash verification per chunk with retry mechanism
- Final SHA-256 file verification
- Supports large files (1GB+)
- Simple concurrency using Parallel.ForEachAsync with batching

## How to Run

1. Run the application
2. Enter source file path
3. Enter destination directory
4. The file will be copied and verified
