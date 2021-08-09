:: copy fonts
robocopy "fonts" "bin\Debug\netcoreapp3.1\fonts" /MIR
robocopy "data" "bin\Debug\netcoreapp3.1\data" /MIR
dotnet run