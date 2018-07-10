pushd
cd ./GreetingsPumper
docker build -t brightercommand/samples:pumper1.02 .
popd
pushd
cd ./GreetingsReceiverConsole
docker build -t brightercommand/samples:receiver1.02 .
popd
