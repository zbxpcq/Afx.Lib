@echo off
set v=6.5.9.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"