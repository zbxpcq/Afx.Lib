@echo off
set v=8.2.4.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"