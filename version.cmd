@echo off
set v=8.2.7.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"