@echo off
set v=8.0.0.0
tool\EditVersion dir="%cd%" v=%v% a="Assembly.cs"