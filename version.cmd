@echo off
set v=8.0.0.1
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"