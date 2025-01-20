FROM golang:1.23.5

WORKDIR /app

# RUN go mod tidy

COPY go.mod go.sum ./

RUN go mod download

COPY . .


RUN go build -o app ./cmd/proxy

EXPOSE 1434

CMD ["./app"]