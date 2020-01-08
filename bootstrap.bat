@echo off
git submodule update --init --recursive
cd dependencies\monogame
protobuild.exe
cd..\..

