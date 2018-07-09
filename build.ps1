dotnet restore
dotnet build
pushd
cd .\GreetingsPumper
rm "out" -recurse -force
dotnet publish -c Release -o out
popd
pushd
cd .\GreetingsReceiverConsole
rm "out" -recurse -force
dotnet publish -c Release -o out
pushd

