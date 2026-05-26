docker build -t epico_backend .

docker stop epico_backend_container_1
docker rm epico_backend_container_1

docker run -d -p 5000:8080 --name epico_backend_container_1 epico_backend