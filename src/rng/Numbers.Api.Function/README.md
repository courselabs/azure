func init Numbers.Api.Function --dotnet --docker

cd Numbers.Api.Function

func new --name rng --template "HTTP trigger" --authlevel "anonymous"

func start 

curl http://localhost:7071/api/rng

curl 'http://localhost:7071/api/rng?min=5500&max=6000'
