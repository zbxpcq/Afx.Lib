@echo off
set v=6.2.13.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"