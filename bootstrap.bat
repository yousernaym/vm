@echo off
git submodule update --init --recursive
cd dependencies\monogame
protobuild.exe
cd..\..
cd dependencies\spatial-media
python -m venv venv
call venv\scripts\activate
python -m pip install --upgrade pip
pip install pyinstaller
pyinstaller --onefile --name minjector spatialmedia\__main__.py
call deactivate
cd..\..

