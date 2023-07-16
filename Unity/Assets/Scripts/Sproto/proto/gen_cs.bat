echo off

set curdir=%~dp0

cd /d %curdir%/../sprotodump

lua ./sprotodump.lua -cs %curdir%/proto.c2s.sproto -o %curdir%/gen_cs/proto_c2s.cs

echo sproto to cs done

pause