echo off

set curdir=%~dp0

cd /d %curdir%/../sprotodump

D:\LUWENHAI\lua51\lua5.1 ./sprotodump.lua -cs %curdir%/proto.c2s.sproto -o %curdir%/gen_cs/proto_c2s.cs -p C2S_
D:\LUWENHAI\lua51\lua5.1 ./sprotodump.lua -cs %curdir%/proto.s2c.sproto -o %curdir%/gen_cs/proto_s2c.cs -p S2C_


echo sproto to cs done

pause