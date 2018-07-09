pushd
cd ./GreetingsPumper
docker build -t brightercommand/samples:pumper1.0 .
popd
pushd
cd ./GreetingsReceiverConsole
docker build -t brightercommand/samples:receiver1.0 .
popd
