#!/bin/bash
#Testing script, barebones and borderline insulting.

grep "[Bb]ytes*" data/man.txt > testgrep;
bin/debug/grapefruit.exe "B|bytes*" "data/man.txt" > testgrapefruit;
diff -ywB --width=150 testgrep testgrapefruit;
