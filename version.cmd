@echo off
set v=6.3.1.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"